using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Data.SqlClient;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.ConectorHerraje
{
    /// <summary>
    /// Nueva (no existía en el original): el proyecto original nunca tuvo forma de eliminar un
    /// conector de herraje ya guardado en BBDD. Ni siquiera btn_Delete_Click/btn_DeleteAll_Click
    /// de ConectorHerrajeCombinar.cs tocan la tabla ConectorHerrajes: solo mueven elementos entre
    /// las dos listas en memoria de ese diálogo (listBox_AllConectores/listBox_Combinar) antes de
    /// guardar la combinación, nunca borran nada de la base de datos.
    ///
    /// Este diálogo, abierto desde la nueva tarjeta "Eliminar conectores" de ConectorHerrajePage,
    /// permite seleccionar uno o varios conectores guardados (tabla ConectorHerrajes) y
    /// eliminarlos definitivamente. El conector actualmente activo (VariablesGlobales, fila
    /// "Conector Herraje", ver RotoTools.Helpers.GetConectorActivo) nunca se puede seleccionar:
    /// eliminarlo dejaría esa variable apuntando a un conector inexistente y rompería cualquier
    /// operación que dependa de ella (por ejemplo, la propia ConectorHerrajePage la muestra en
    /// LblConectorActivo). Su casilla se deshabilita en la grid (ver PuedeEliminarse más abajo).
    /// </summary>
    public partial class ConectorHerrajeEliminarWindow : Window
    {
        private bool _sincronizandoSelectAll;
        private readonly ObservableCollection<ConectorEliminarRowVm> _filas = new();

        public ConectorHerrajeEliminarWindow()
        {
            InitializeComponent();

            CargarTextos();
            CargarConectores();
        }

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            Title = Loc("L_Suite_EliminarConectores");
            TxtTitulo.Text = Title;
            ChkSeleccionarTodas.Content = Loc("L_Suite_SeleccionarTodas");
            TxtBtnEliminar.Text = Loc("L_Suite_Eliminar");
        }

        /// <summary>Mismo origen (tabla ConectorHerrajes, columna Codigo) que CargarConectores en
        /// ConectorHerrajeActivoWindow. Cada fila se suscribe a su propio PropertyChanged para
        /// mantener sincronizada "Seleccionar todas" al marcar/desmarcar filas sueltas, igual que
        /// GridSets/ChkSeleccionarTodas en ConectorHerrajeGeneradorWindow. Se vuelve a llamar tras
        /// eliminar (ver BtnEliminar_Click) para que la lista quede al día sin cerrar el diálogo.</summary>
        private void CargarConectores()
        {
            string conectorActivo = RotoTools.Helpers.GetConectorActivo() ?? "";

            foreach (var fila in _filas)
                fila.PropertyChanged -= Fila_PropertyChanged;
            _filas.Clear();

            using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT Codigo FROM ConectorHerrajes ORDER BY Codigo", conexion))
            {
                conexion.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string codigo = reader[0].ToString() ?? "";
                    var fila = new ConectorEliminarRowVm(codigo, codigo == conectorActivo);
                    fila.PropertyChanged += Fila_PropertyChanged;
                    _filas.Add(fila);
                }
            }

            GridConectores.ItemsSource = null;
            GridConectores.ItemsSource = _filas;

            _sincronizandoSelectAll = true;
            ChkSeleccionarTodas.IsChecked = false;
            _sincronizandoSelectAll = false;

            LblTotal.Text = string.Format(Loc("L_Suite_TotalConectoresGuardados"), _filas.Count);
        }

        #region Selección ("Seleccionar todas", igual que ConectorHerrajeGeneradorWindow)

        private void ChkSeleccionarTodas_Checked(object sender, RoutedEventArgs e)
        {
            if (_sincronizandoSelectAll) return;
            MarcarFilasSeleccionables(true);
        }

        private void ChkSeleccionarTodas_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_sincronizandoSelectAll) return;
            MarcarFilasSeleccionables(false);
        }

        /// <summary>El conector activo (PuedeEliminarse = false) nunca se marca, ni siquiera con
        /// "Seleccionar todas": su casilla está deshabilitada precisamente para impedirlo.</summary>
        private void MarcarFilasSeleccionables(bool marcar)
        {
            foreach (var fila in _filas)
            {
                if (fila.PuedeEliminarse)
                    fila.Selected = marcar;
            }
        }

        /// <summary>Igual que Fila_PropertyChanged en ConectorHerrajeGeneradorWindow: recalcula si
        /// "Seleccionar todas" debe quedar marcada (todas las filas seleccionables marcadas) sin
        /// volver a disparar su propio Checked/Unchecked.</summary>
        private void Fila_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ConectorEliminarRowVm.Selected)) return;

            bool haySeleccionables = false;
            bool todasMarcadas = true;
            foreach (var fila in _filas)
            {
                if (!fila.PuedeEliminarse) continue;
                haySeleccionables = true;
                if (!fila.Selected) { todasMarcadas = false; break; }
            }

            _sincronizandoSelectAll = true;
            ChkSeleccionarTodas.IsChecked = haySeleccionables && todasMarcadas;
            _sincronizandoSelectAll = false;
        }

        #endregion

        /// <summary>Elimina de la tabla ConectorHerrajes, en un único DELETE parametrizado, todos
        /// los conectores marcados (el conector activo nunca puede estarlo, ver PuedeEliminarse).
        /// Pide confirmación primero porque, a diferencia de "Eliminar líneas no usadas" (Revisión
        /// de sets) o "Limpiar" (Instalación), esta acción no se puede deshacer desde ningún otro
        /// sitio de la aplicación: el conector desaparece por completo de la base de datos.</summary>
        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionados = new List<string>();
            foreach (var fila in _filas)
            {
                if (fila.Selected)
                    seleccionados.Add(fila.Codigo);
            }

            if (seleccionados.Count == 0)
            {
                MessageBox.Show(Loc("L_Suite_SeleccionaAlMenosUnConector"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string mensajeConfirmacion = string.Format(Loc("L_Suite_ConfirmarEliminarConectores"), seleccionados.Count);
            if (MessageBox.Show(mensajeConfirmacion, Loc("L_Suite_ConfirmarEliminacion"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                {
                    conexion.Open();

                    var nombresParametros = new List<string>();
                    using var cmd = new SqlCommand { Connection = conexion };
                    for (int i = 0; i < seleccionados.Count; i++)
                    {
                        string nombreParametro = "@c" + i;
                        nombresParametros.Add(nombreParametro);
                        cmd.Parameters.AddWithValue(nombreParametro, seleccionados[i]);
                    }
                    cmd.CommandText = "DELETE FROM ConectorHerrajes WHERE Codigo IN (" + string.Join(",", nombresParametros) + ")";

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(string.Format(Loc("L_Suite_ConectoresEliminados"), seleccionados.Count), Loc("L_Suite_OperacionCompletada"),
                    MessageBoxButton.OK, MessageBoxImage.Information);

                CargarConectores();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error (44):" + System.Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>Fila de la grid de conectores a eliminar: código + si es el conector activo actual
    /// (EsActivo) + si el usuario la ha marcado (Selected). Implementa INotifyPropertyChanged para
    /// poder sincronizar automáticamente "Seleccionar todas" al marcar/desmarcar filas sueltas,
    /// igual que SetGridRowVm en ConectorHerrajeGeneradorWindow.</summary>
    public class ConectorEliminarRowVm : INotifyPropertyChanged
    {
        private bool _selected;

        public ConectorEliminarRowVm(string codigo, bool esActivo)
        {
            Codigo = codigo;
            EsActivo = esActivo;
        }

        public string Codigo { get; }
        public bool EsActivo { get; }

        /// <summary>El conector activo nunca puede seleccionarse para eliminar (ver comentario de
        /// clase en ConectorHerrajeEliminarWindow).</summary>
        public bool PuedeEliminarse => !EsActivo;

        public string EtiquetaActivo => EsActivo ? SuiteLocalization.GetString("L_Suite_Activo") : "";

        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected == value) return;
                _selected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
