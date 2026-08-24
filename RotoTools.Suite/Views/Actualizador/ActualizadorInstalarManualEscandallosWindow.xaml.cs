using System.ComponentModel;
using System.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using RotoEntities;
using RotoTools.Suite.Services;
using EnumTipoEscandallo = RotoTools.Enums.enumRotoTipoEscandallo;

namespace RotoTools.Suite.Views.Actualizador
{
    /// <summary>Envoltorio de un Escandallo con su casilla de selección: sustituye al
    /// CheckedListBox.CheckedItems del original (ver comentario en el XAML).</summary>
    public class EscandalloSeleccionable : INotifyPropertyChanged
    {
        public Escandallo Escandallo { get; }
        public string Codigo => Escandallo.Codigo;

        private bool _seleccionado;
        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (_seleccionado == value) return;
                _seleccionado = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Seleccionado)));
            }
        }

        public EscandalloSeleccionable(Escandallo escandallo) => Escandallo = escandallo;

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Sustituye a ActualizadorInstalarManualEscandallos.cs/.Designer.cs (WinForms): mismo
    /// comportamiento (filtro + seleccionar todos + instalar los escandallos marcados),
    /// reutilizando tal cual RotoTools.Helpers/RotoTools.EscandalloHelper/RotoTools.TranslateManager.
    /// </summary>
    public partial class ActualizadorInstalarManualEscandallosWindow : Window
    {
        private List<EscandalloSeleccionable> _escandalloList = new();

        public ActualizadorInstalarManualEscandallosWindow()
        {
            InitializeComponent();

            LoadAllEscandallos();
            LoadEscandallosInList("");
            CargarTextos();
        }

        #region Localización / carga

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_InstalacionIndividualizada");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_InstalacionIndividualizada");

            ChkSelectAll.Content = RotoTools.LocalizationManager.GetString("L_SeleccionarTodos");
            LblBuscar.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
            TxtBtnInstalarEscandallos.Text = RotoTools.LocalizationManager.GetString("L_Instalar");
        }

        private void LoadAllEscandallos()
        {
            var tiposSeleccionados = new List<EnumTipoEscandallo>
            {
                EnumTipoEscandallo.PVC, EnumTipoEscandallo.Aluminio, EnumTipoEscandallo.GestionGeneral,
                EnumTipoEscandallo.GestionManillas, EnumTipoEscandallo.GestionBombillos, EnumTipoEscandallo.PersonalizacionClientes
            };

            _escandalloList = RotoTools.Helpers.CargarEscandallosEmbebidos(tiposSeleccionados)
                .Select(e => new EscandalloSeleccionable(e))
                .ToList();
        }

        private void LoadEscandallosInList(string filter)
        {
            var mostrar = string.IsNullOrEmpty(filter)
                ? _escandalloList
                : _escandalloList.Where(e => e.Codigo.ToUpper().Contains(filter.ToUpper())).ToList();

            ListaEscandallos.ItemsSource = mostrar;
        }

        private void EnableControls(bool enabled)
        {
            BtnInstalarEscandallos.IsEnabled = enabled;
            ListaEscandallos.IsEnabled = enabled;
            TxtFiltro.IsEnabled = enabled;
            ChkSelectAll.IsEnabled = enabled;
        }

        #endregion

        #region Events

        private void TxtFiltro_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ChkSelectAll.IsChecked = false;
            LoadEscandallosInList(TxtFiltro.Text);
        }

        private void ChkSelectAll_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool marcado = ChkSelectAll.IsChecked == true;
            foreach (var item in _escandalloList)
                item.Seleccionado = marcado;
        }

        private void BtnInstalarEscandallos_Click(object sender, RoutedEventArgs e)
        {
            if (!_escandalloList.Any(i => i.Seleccionado))
                return;

            if (MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ConfirmarInstalar"), "",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            RotoTools.Helpers.InstalarOpcionConfiguraciónStandard();
            InstallEscandallos();
        }

        #endregion

        #region Instalación (InstallEscandallos, idéntico al original)

        private void InstallEscandallos()
        {
            try
            {
                RotoTools.TranslateManager.AplicarTraduccion = false;

                if (RotoTools.TranslateManager.PermitirTraduccionesEnConectorEscandallos)
                {
                    if (MessageBox.Show(RotoTools.LocalizationManager.GetString("L_AplicarPlantillaTraduccion"), "",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var openFileDialog = new OpenFileDialog { Filter = "XLS Files (*.xls)|*.xlsx" };
                        if (openFileDialog.ShowDialog() == true)
                        {
                            EnableControls(false);
                            RotoTools.TranslateManager.AplicarTraduccion = true;
                            RotoTools.TranslateManager.TraduccionesActuales = RotoTools.Helpers.CargarTraducciones(openFileDialog.FileName);
                            EnableControls(true);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                EnableControls(false);
                string messageEscandallos = RotoTools.LocalizationManager.GetString("L_EscandallosInstalados") + Environment.NewLine + Environment.NewLine;

                using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                {
                    conn.Open();

                    foreach (var item in _escandalloList.Where(i => i.Seleccionado))
                    {
                        var escandallo = item.Escandallo;
                        RotoTools.EscandalloHelper.AplicarTraduccion(escandallo);

                        if (RotoTools.Helpers.ExisteEscandalloEnBD(escandallo.Codigo))
                        {
                            const string queryInstall = @"UPDATE Escandallos SET Programa=@Programa WHERE Codigo=@Codigo";
                            using var cmd = new SqlCommand(queryInstall, conn);
                            cmd.Parameters.AddWithValue("@Codigo", escandallo.Codigo);
                            cmd.Parameters.AddWithValue("@Programa", (object?)escandallo.Programa ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            const string insert = @"INSERT INTO Escandallos
                                        (Codigo, [Type], Descripcion, Nivel1, Nivel2, Nivel3, Nivel4, Nivel5, Variables, Programa, Texto, Familia)
                                        VALUES (@Codigo, @Type, @Descripcion, @Nivel1, @Nivel2, @Nivel3, @Nivel4, @Nivel5, @Variables, @Programa, @Texto, @Familia)";

                            using var cmd = new SqlCommand(insert, conn);
                            cmd.Parameters.AddWithValue("@Codigo", escandallo.Codigo);
                            cmd.Parameters.AddWithValue("@Type", escandallo.Type);
                            cmd.Parameters.AddWithValue("@Descripcion", (object?)escandallo.Descripcion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel1", (object?)escandallo.Nivel1 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel2", (object?)escandallo.Nivel2 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel3", (object?)escandallo.Nivel3 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel4", (object?)escandallo.Nivel4 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel5", (object?)escandallo.Nivel5 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variables", (object?)escandallo.Variables ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Programa", (object?)escandallo.Programa ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Texto", (object?)escandallo.Texto ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Familia", (object?)escandallo.Familia ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        messageEscandallos += escandallo.Codigo + Environment.NewLine;
                    }
                }

                EnableControls(true);
                MessageBox.Show(messageEscandallos, "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (6): " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
