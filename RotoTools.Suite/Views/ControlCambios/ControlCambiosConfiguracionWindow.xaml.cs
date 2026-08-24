using System.Windows;
using RotoEntities;

namespace RotoTools.Suite.Views.ControlCambios
{
    /// <summary>
    /// Sustituye a ControlCambiosConfiguracion.cs/.Designer.cs (WinForms): mismo comportamiento
    /// (6 interruptores generales + sub-apartados de Fittings/Sets con habilitado en cascada),
    /// portado directamente (a diferencia de ControlCambiosPage, esta lógica es corta y solo
    /// depende de sus propios controles, no del motor de comparación/PDF de ~3000 líneas).
    /// </summary>
    public partial class ControlCambiosConfiguracionWindow : Window
    {
        public bool CompararOpciones { get; private set; }
        public bool CompararFittingGroups { get; private set; }
        public ComparaFittingsProperties CompararFittings { get; private set; }
        public ComparaSetsProperties CompararSets { get; private set; }
        public bool CompararColores { get; private set; }
        public bool CompararMecanizados { get; private set; }

        public ControlCambiosConfiguracionWindow(bool compararOpciones, bool compararFittingGroups,
            ComparaFittingsProperties compararFittings, ComparaSetsProperties compararSets,
            bool compararColores, bool compararMecanizados)
        {
            InitializeComponent();

            CompararOpciones = compararOpciones;
            CompararFittingGroups = compararFittingGroups;
            CompararColores = compararColores;
            CompararMecanizados = compararMecanizados;
            CompararFittings = compararFittings;
            CompararSets = compararSets;

            CargarTextos();

            ChkComparaOpciones.IsChecked = compararOpciones;
            ChkCompararColores.IsChecked = compararColores;
            ChkCompararFittingGroups.IsChecked = compararFittingGroups;
            ChkComparaMecanizados.IsChecked = compararMecanizados;

            ChkCompararFittings.IsChecked = compararFittings.compararFittings;
            ChkFittingsFiltrados.IsChecked = compararFittings.compararFittingsFiltrados;
            ChkFittingsDescripcion.IsChecked = compararFittings.compararFittingsDescription;
            ChkFittingsLength.IsChecked = compararFittings.compararFittingsLength;
            ChkFittingsManufacturer.IsChecked = compararFittings.compararFittingsManufacturer;
            ChkFittingsLocation.IsChecked = compararFittings.compararFittingsLocation;
            ChkFittingsArticles.IsChecked = compararFittings.compararFittingsArticles;

            ChkCompararSets.IsChecked = compararSets.compararSets;
            ChkSetsFiltrados.IsChecked = compararSets.compararSetsFiltrados;
            ChkSetsNumero.IsChecked = compararSets.compararCantidadSetDescriptions;

            GestionChecksFittings();
            GestionChecksSets();
        }

        #region Localización

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_ConfiguracionInforme");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_ConfiguracionInforme");
            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");

            ChkCompararFittingGroups.Content = RotoTools.LocalizationManager.GetString("L_CompararFittingGroups");
            ChkCompararColores.Content = RotoTools.LocalizationManager.GetString("L_CompararColores");
            ChkComparaOpciones.Content = RotoTools.LocalizationManager.GetString("L_CompararOpciones");
            ChkComparaMecanizados.Content = RotoTools.LocalizationManager.GetString("L_CompararMecanizados");

            ChkCompararSets.Content = RotoTools.LocalizationManager.GetString("L_CompararSets");
            ChkSetsNumero.Content = RotoTools.LocalizationManager.GetString("L_CompararNumeroSets");
            ChkSetsFiltrados.Content = RotoTools.LocalizationManager.GetString("L_FiltrarSets");

            ChkCompararFittings.Content = RotoTools.LocalizationManager.GetString("L_CompararFittings");
            ChkFittingsFiltrados.Content = RotoTools.LocalizationManager.GetString("L_FiltrarFittings");
            ChkFittingsDescripcion.Content = RotoTools.LocalizationManager.GetString("L_CompararDescripciones");
            ChkFittingsManufacturer.Content = RotoTools.LocalizationManager.GetString("L_CompararManufacturer");
            ChkFittingsLength.Content = RotoTools.LocalizationManager.GetString("L_CompararLength");
            ChkFittingsLocation.Content = RotoTools.LocalizationManager.GetString("L_CompararLocation");
            ChkFittingsArticles.Content = RotoTools.LocalizationManager.GetString("L_CompararArticulosOpciones");
        }

        #endregion

        #region Events

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            CompararOpciones = ChkComparaOpciones.IsChecked == true;
            CompararFittingGroups = ChkCompararFittingGroups.IsChecked == true;
            CompararColores = ChkCompararColores.IsChecked == true;
            CompararMecanizados = ChkComparaMecanizados.IsChecked == true;

            CompararFittings.compararFittings = ChkCompararFittings.IsChecked == true;
            CompararFittings.compararFittingsFiltrados = ChkFittingsFiltrados.IsChecked == true;
            CompararFittings.compararFittingsLength = ChkFittingsLength.IsChecked == true;
            CompararFittings.compararFittingsLocation = ChkFittingsLocation.IsChecked == true;
            CompararFittings.compararFittingsDescription = ChkFittingsDescripcion.IsChecked == true;
            CompararFittings.compararFittingsManufacturer = ChkFittingsManufacturer.IsChecked == true;
            CompararFittings.compararFittingsArticles = ChkFittingsArticles.IsChecked == true;

            if (!CompararFittings.compararFittingsFiltrados)
                CompararFittings.compararFittingsFiltradosList = new List<string>();

            CompararSets.compararSets = ChkCompararSets.IsChecked == true;
            CompararSets.compararSetsFiltrados = ChkSetsFiltrados.IsChecked == true;
            CompararSets.compararCantidadSetDescriptions = ChkSetsNumero.IsChecked == true;

            if (!CompararSets.compararSetsFiltrados)
                CompararSets.compararSetsFiltradosList = new List<string>();

            DialogResult = true;
            Close();
        }

        private void ChkCompararFittings_CheckedChanged(object sender, RoutedEventArgs e) => GestionChecksFittings();

        private void ChkCompararSets_CheckedChanged(object sender, RoutedEventArgs e) => GestionChecksSets();

        private void GestionChecksFittings()
        {
            bool marcado = ChkCompararFittings.IsChecked == true;

            ChkFittingsDescripcion.IsEnabled = marcado;
            ChkFittingsLength.IsEnabled = marcado;
            ChkFittingsManufacturer.IsEnabled = marcado;
            ChkFittingsLocation.IsEnabled = marcado;
            ChkFittingsArticles.IsEnabled = marcado;
            ChkFittingsFiltrados.IsEnabled = marcado;
            BtnFittingsFiltrados.IsEnabled = ChkFittingsFiltrados.IsChecked == true;

            if (!marcado)
            {
                ChkFittingsFiltrados.IsChecked = false;
                ChkFittingsDescripcion.IsChecked = false;
                ChkFittingsLength.IsChecked = false;
                ChkFittingsManufacturer.IsChecked = false;
                ChkFittingsLocation.IsChecked = false;
                ChkFittingsArticles.IsChecked = false;
            }
        }

        private void GestionChecksSets()
        {
            bool marcado = ChkCompararSets.IsChecked == true;

            ChkSetsFiltrados.IsEnabled = marcado;
            BtnSetsFiltrados.IsEnabled = ChkSetsFiltrados.IsChecked == true;
            ChkSetsNumero.IsEnabled = marcado;

            if (!marcado)
            {
                ChkSetsFiltrados.IsChecked = false;
                ChkSetsNumero.IsChecked = false;
            }
        }

        private void ChkSetsFiltrados_CheckedChanged(object sender, RoutedEventArgs e)
        {
            BtnSetsFiltrados.IsEnabled = ChkSetsFiltrados.IsChecked == true;
        }

        private void ChkFittingsFiltrados_CheckedChanged(object sender, RoutedEventArgs e)
        {
            BtnFittingsFiltrados.IsEnabled = ChkFittingsFiltrados.IsChecked == true;
        }

        private void BtnSetsFiltrados_Click(object sender, RoutedEventArgs e)
        {
            List<string> listaFiltroCopia = new List<string>(CompararSets.compararSetsFiltradosList);
            var filtroSetsWindow = new ControlCambiosFiltroItemsWindow(
                CompararSets.compararSetsComunesList, listaFiltroCopia,
                CompararSets.compararSetsSoloXml1List, CompararSets.compararSetsSoloXml2List)
            {
                Owner = this
            };

            if (filtroSetsWindow.ShowDialog() == true)
                CompararSets.compararSetsFiltradosList = filtroSetsWindow.ItemsComunesFiltradosList;
        }

        private void BtnFittingsFiltrados_Click(object sender, RoutedEventArgs e)
        {
            List<string> listaFiltroCopia = new List<string>(CompararFittings.compararFittingsFiltradosList);
            var filtroFittingsWindow = new ControlCambiosFiltroItemsWindow(
                CompararFittings.compararFittingsComunesList, listaFiltroCopia)
            {
                Owner = this
            };

            if (filtroFittingsWindow.ShowDialog() == true)
                CompararFittings.compararFittingsFiltradosList = filtroFittingsWindow.ItemsComunesFiltradosList;
        }

        #endregion
    }
}
