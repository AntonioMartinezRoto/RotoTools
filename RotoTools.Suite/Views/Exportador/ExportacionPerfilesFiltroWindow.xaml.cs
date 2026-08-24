using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using RotoEntities;

namespace RotoTools.Suite.Views.Exportador
{
    /// <summary>
    /// Sustituye a ExportacionWinPerfilListaPerfiles.cs/.Designer.cs (WinForms): ver el comentario
    /// grande en el XAML. Reutilizada 2 veces por ExportacionWinPerfilWindow — una instancia por
    /// cada filtro (Perfil / Perfil ALU), igual que en el original (dos llamadas a
    /// "new ExportacionWinPerfilListaPerfiles(...)" con distinta lista de perfiles/seleccionados).
    /// </summary>
    public partial class ExportacionPerfilesFiltroWindow : Window
    {
        private readonly List<Value> _perfilesList;
        private List<Value> _perfilesListSelected;

        /// <summary>Resultado a leer por el llamador cuando ShowDialog() devuelve true (igual que el
        /// campo público PerfilesListSelected + DialogResult.OK en el original).</summary>
        public List<Value> PerfilesListSelected { get; private set; } = new();

        public ExportacionPerfilesFiltroWindow(List<Value> perfilesList, List<Value> perfilesListSelected)
        {
            InitializeComponent();

            _perfilesList = perfilesList;
            _perfilesListSelected = perfilesListSelected;

            CargarTextos();
            LoadItems("");
        }

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_Filtro");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_Filtro");
            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
            LblBuscar.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
        }

        /// <summary>Igual que LoadItems en el original: repuebla la lista visible a partir de
        /// _perfilesList, filtrando por texto sobre Value.Valor. Cada fila se envuelve en un
        /// PerfilSeleccionable NUEVO cada vez, con el estado marcado calculado a partir de
        /// _perfilesListSelected.Contains(perfil) — igual que
        /// chkList_Perfiles.SetItemChecked(index, PerfilesListSelected.Contains(perfil)) en el
        /// original (comparación por referencia, ya que Value no sobrescribe Equals; válido aquí
        /// porque _perfilesList y _perfilesListSelected comparten siempre las mismas instancias de
        /// Value, igual que en el original).</summary>
        private void LoadItems(string filter)
        {
            IEnumerable<Value> query = _perfilesList;

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(p =>
                    p.Valor != null &&
                    p.Valor.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var lista = query.OrderBy(p => p.Valor)
                .Select(p => new PerfilSeleccionable(p, _perfilesListSelected.Contains(p)))
                .ToList();

            foreach (var item in lista)
                item.PropertyChanged += Item_PropertyChanged;

            ListaPerfiles.ItemsSource = lista;
        }

        /// <summary>Igual que chkList_Perfiles_ItemCheck en el original: mantiene
        /// _perfilesListSelected sincronizada con lo que el usuario va marcando/desmarcando, solo
        /// para los elementos actualmente visibles.</summary>
        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not PerfilSeleccionable item) return;

            if (item.Seleccionado)
            {
                if (!_perfilesListSelected.Contains(item.Perfil))
                    _perfilesListSelected.Add(item.Perfil);
            }
            else
            {
                _perfilesListSelected.Remove(item.Perfil);
            }
        }

        private void TxtFiltro_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LoadItems(TxtFiltro.Text);
        }

        /// <summary>Igual que FillPerfilesFiltradosList en el original: reconstruye
        /// _perfilesListSelected SOLO a partir de lo que está actualmente marcado y visible en la
        /// lista en este momento (no de todo lo marcado alguna vez) — mismo comportamiento que puede
        /// sorprender (no se corrige aquí, ver el comentario detallado ya dejado en
        /// ControlCambiosFiltroItemsWindow.BtnGuardarFiltro_Click para la explicación completa).</summary>
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (ListaPerfiles.ItemsSource is List<PerfilSeleccionable> visibles)
            {
                _perfilesListSelected = visibles.Where(i => i.Seleccionado).Select(i => i.Perfil).ToList();
            }

            PerfilesListSelected = _perfilesListSelected;

            DialogResult = true;
            Close();
        }
    }
}
