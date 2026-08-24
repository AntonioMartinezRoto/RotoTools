using System.ComponentModel;
using System.Windows;

namespace RotoTools.Suite.Views.ControlCambios
{
    /// <summary>Envoltorio de un elemento (set o fitting group) con su casilla de selección:
    /// sustituye al CheckedListBox.CheckedItems del original (ver comentario en el XAML).</summary>
    public class ItemSeleccionable : INotifyPropertyChanged
    {
        public string Nombre { get; }

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

        public ItemSeleccionable(string nombre, bool seleccionado)
        {
            Nombre = nombre;
            _seleccionado = seleccionado;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Sustituye a ControlCambiosFiltroItems.cs/.Designer.cs (WinForms): mismo comportamiento
    /// (filtro + seleccionar todos + solo seleccionados + guardar), portado directamente.
    /// </summary>
    public partial class ControlCambiosFiltroItemsWindow : Window
    {
        private enum TipoItem { Set = 1, FittingGroup = 2 }

        private List<string> _allItemsComunesList = new();
        private List<string> _itemsComunesFiltradosList = new();
        private int _itemComunesCount;
        private bool _estoyFiltrando;
        private TipoItem _itemTypeLoaded = TipoItem.Set;

        /// <summary>Resultado a leer por el llamador cuando ShowDialog() devuelve true (igual que
        /// itemsComunesFiltradosList/DialogResult.OK en el original).</summary>
        public List<string> ItemsComunesFiltradosList { get; private set; } = new();

        public ControlCambiosFiltroItemsWindow(List<string> itemsComunesList, List<string> itemsFiltradosList,
            List<string> itemsSoloEnXml1List, List<string> itemsSoloEnXml2List)
        {
            InitializeComponent();

            _allItemsComunesList = itemsComunesList;
            _itemsComunesFiltradosList = itemsFiltradosList;
            _itemComunesCount = itemsComunesList.Count;
            _itemTypeLoaded = TipoItem.Set;
            // itemSoloEnXml1Count/itemSoloEnXml2Count no se usan en ningún texto de la UI en el
            // original (solo se guardan como propiedades públicas, nunca leídas) — no se portan.

            Inicializar();
        }

        public ControlCambiosFiltroItemsWindow(List<string> itemsComunesList, List<string> itemsFiltradosList)
        {
            InitializeComponent();

            _allItemsComunesList = itemsComunesList;
            _itemsComunesFiltradosList = itemsFiltradosList;
            _itemComunesCount = itemsComunesList.Count;
            _itemTypeLoaded = TipoItem.FittingGroup;

            Inicializar();
        }

        private void Inicializar()
        {
            CargarTextos();
            LoadItems("", false);
            InitializeCounters();
        }

        #region Localización

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_Filtro");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_Filtro");
            TxtBtnGuardarFiltro.Text = RotoTools.LocalizationManager.GetString("L_Guardar");

            ChkSelectAll.Content = RotoTools.LocalizationManager.GetString("L_SeleccionarTodos");
            ChkSoloFiltrados.Content = RotoTools.LocalizationManager.GetString("L_SoloSeleccionados");
            LblBuscar.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
        }

        private void InitializeCounters()
        {
            string clave = _itemTypeLoaded == TipoItem.Set ? "L_SetsComunes" : "L_FittingsComunes";
            LblNumeroComunes.Text = RotoTools.LocalizationManager.GetString(clave) + ": " + _itemComunesCount;
        }

        #endregion

        #region Carga / filtro

        /// <summary>Igual que LoadItems en el original: repuebla la lista visible a partir de
        /// allItemsComunesList (o de itemsComunesFiltradosList si "Solo seleccionados" está
        /// marcado), filtrando por texto. Cada fila se envuelve en un ItemSeleccionable NUEVO cada
        /// vez (igual que chkList_Sets.Items.Add(set, itemsComunesFiltradosList.Contains(set)) en
        /// el original: el estado marcado/desmarcado se recalcula desde itemsComunesFiltradosList
        /// cada vez que se repuebla la lista, no se conserva por identidad de objeto).</summary>
        private void LoadItems(string filter, bool soloSeleccionadosChecked)
        {
            IEnumerable<string> origen = soloSeleccionadosChecked ? _itemsComunesFiltradosList : _allItemsComunesList;

            IEnumerable<string> filtrados = string.IsNullOrEmpty(filter)
                ? origen
                : origen.Where(s => s.ToLower().Contains(filter.ToLower()));

            var lista = filtrados.OrderBy(s => s)
                .Select(s => new ItemSeleccionable(s, _itemsComunesFiltradosList.Contains(s)))
                .ToList();

            foreach (var item in lista)
                item.PropertyChanged += Item_PropertyChanged;

            ListaItems.ItemsSource = lista;
        }

        /// <summary>Igual que chkList_Sets_ItemCheck en el original: mantiene
        /// itemsComunesFiltradosList sincronizada con lo que el usuario va marcando/desmarcando,
        /// SOLO para los elementos actualmente visibles (los que están fuera del filtro activo no
        /// se tocan aquí).</summary>
        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not ItemSeleccionable item) return;

            if (item.Seleccionado)
            {
                if (!_itemsComunesFiltradosList.Contains(item.Nombre))
                    _itemsComunesFiltradosList.Add(item.Nombre);
            }
            else
            {
                _itemsComunesFiltradosList.Remove(item.Nombre);
            }
        }

        #endregion

        #region Events

        private void TxtFiltro_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _estoyFiltrando = true;
            ChkSelectAll.IsChecked = false;

            LoadItems(TxtFiltro.Text, ChkSoloFiltrados.IsChecked == true);

            _estoyFiltrando = false;
        }

        private void ChkSoloFiltrados_CheckedChanged(object sender, RoutedEventArgs e)
        {
            LoadItems(TxtFiltro.Text, ChkSoloFiltrados.IsChecked == true);
        }

        private void ChkSelectAll_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (_estoyFiltrando) return;

            if (ListaItems.ItemsSource is List<ItemSeleccionable> visibles)
            {
                bool marcado = ChkSelectAll.IsChecked == true;
                foreach (var item in visibles)
                    item.Seleccionado = marcado;
            }
        }

        /// <summary>Igual que FillFiltradosList en el original: SOLO tiene en cuenta lo que está
        /// actualmente visible en la lista (según el filtro/"Solo seleccionados" activos en este
        /// momento), no todo lo marcado alguna vez. Esto reproduce tal cual un comportamiento del
        /// original que puede sorprender (no se corrige aquí): si filtras, marcas/desmarcas algo
        /// fuera del filtro no es posible, pero SI habías marcado elementos antes de filtrar y le
        /// das a Guardar mientras el filtro sigue activo, itemsComunesFiltradosList se reconstruye
        /// solo con lo visible en ese momento, perdiendo la selección de los elementos que quedaron
        /// fuera del filtro.</summary>
        private void BtnGuardarFiltro_Click(object sender, RoutedEventArgs e)
        {
            if (ListaItems.ItemsSource is List<ItemSeleccionable> visibles)
            {
                _itemsComunesFiltradosList = visibles.Where(i => i.Seleccionado).Select(i => i.Nombre).ToList();
            }

            ItemsComunesFiltradosList = _itemsComunesFiltradosList;

            DialogResult = true;
            Close();
        }

        #endregion
    }
}
