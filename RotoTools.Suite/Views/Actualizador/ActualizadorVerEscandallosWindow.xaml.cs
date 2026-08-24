using System.Windows;
using System.Windows.Controls;
using RotoEntities;
using RotoTools.Suite.Services;
using EnumTipoEscandallo = RotoTools.Enums.enumRotoTipoEscandallo;

namespace RotoTools.Suite.Views.Actualizador
{
    /// <summary>
    /// Sustituye a ActualizadorEscandallos.cs/.Designer.cs (WinForms): mismo comportamiento
    /// (filtro por código + contenido/Programa del escandallo seleccionado), reutilizando tal cual
    /// RotoTools.Helpers.CargarEscandallosEmbebidos vía ProjectReference.
    /// </summary>
    public partial class ActualizadorVerEscandallosWindow : Window
    {
        private List<Escandallo> _escandallosList = new();

        public ActualizadorVerEscandallosWindow()
        {
            InitializeComponent();

            CargarEscandallos();
            FillEscandallosList(_escandallosList);
            CargarTextos();
        }

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_Escandallos");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_Escandallos");
            LblFiltrar.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
            TxtSeleccionaUno.Text = SuiteLocalization.GetString("L_Suite_VerEscandallosSeleccionaUno");
        }

        private void CargarEscandallos()
        {
            var tiposSeleccionados = new List<EnumTipoEscandallo>
            {
                EnumTipoEscandallo.PVC, EnumTipoEscandallo.Aluminio, EnumTipoEscandallo.GestionGeneral,
                EnumTipoEscandallo.GestionManillas, EnumTipoEscandallo.GestionBombillos, EnumTipoEscandallo.PersonalizacionClientes
            };

            _escandallosList = RotoTools.Helpers.CargarEscandallosEmbebidos(tiposSeleccionados);
        }

        private void FillEscandallosList(IEnumerable<Escandallo> escandallosList)
        {
            ListaEscandallos.ItemsSource = escandallosList.OrderBy(c => c.Codigo).ToList();
        }

        private void TxtFiltro_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = TxtFiltro.Text.Trim().ToUpper();
            var escandallosFiltrados = _escandallosList.Where(o => o.Codigo.ToUpper().Contains(filtro)).ToList();
            FillEscandallosList(escandallosFiltrados);
        }

        private void ListaEscandallos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaEscandallos.SelectedItem is Escandallo esc)
            {
                // Igual que el original: si el Programa vino con los saltos de línea escapados,
                // se "desescapan" para que se vean como saltos de línea de verdad.
                string contenido = (esc.Programa ?? string.Empty)
                    .Replace("\\r\\n", "\r\n")
                    .Replace("\\n", "\r\n");

                TxtContenidoEscandallo.Text = contenido;
                TxtContenidoEscandallo.Visibility = Visibility.Visible;
                TxtSeleccionaUno.Visibility = Visibility.Collapsed;

                TxtContenidoEscandallo.SelectionStart = 0;
                TxtContenidoEscandallo.ScrollToHome();
            }
            else
            {
                TxtContenidoEscandallo.Text = "";
                TxtContenidoEscandallo.Visibility = Visibility.Collapsed;
                TxtSeleccionaUno.Visibility = Visibility.Visible;
            }
        }
    }
}
