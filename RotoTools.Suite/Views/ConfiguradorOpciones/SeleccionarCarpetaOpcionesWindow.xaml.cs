using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.ConfiguradorOpciones
{
    /// <summary>
    /// Nueva (no existía en el original): diálogo modal con el árbol de carpetas de la tabla
    /// OPCIONES (ver DibujoOpcionesRotoService.GetArbolCarpetasOpciones/OpcionCarpetaTreeNode),
    /// para que ConfiguradorOpcionesAnadirRotoWindow deje elegir la carpeta destino haciendo clic
    /// en vez de escribiéndola a mano. Un solo clic selecciona el nodo, doble clic equivale a
    /// seleccionar + Aceptar (mismo criterio "doble clic = acción directa" que el resto de árboles
    /// de la Suite, p. ej. TreeDibujos en esta misma carpeta o Cam3DWindow).
    /// </summary>
    public partial class SeleccionarCarpetaOpcionesWindow : Window
    {
        /// <summary>Carpeta elegida por el usuario, o null si canceló sin elegir ninguna.</summary>
        public OpcionCarpetaTreeNode? CarpetaSeleccionada { get; private set; }

        public SeleccionarCarpetaOpcionesWindow(List<OpcionCarpetaTreeNode> raiz, OpcionCarpetaTreeNode? carpetaActual)
        {
            InitializeComponent();

            CargarTextos();

            TreeCarpetas.ItemsSource = raiz;

            if (carpetaActual != null)
                SeleccionarYExpandirRuta(raiz, carpetaActual.Ruta);
        }

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            Title = Loc("L_Suite_SeleccionarCarpetaOpciones");
            TxtTitulo.Text = Title;
            TxtBtnAceptar.Text = Loc("L_Suite_Aceptar");
            TxtBtnCancelar.Text = Loc("L_Suite_Cancelar");
        }

        /// <summary>Preselecciona (y expande hasta) la carpeta actualmente elegida, si la hay,
        /// buscándola por su Ruta (Nivel1..N) en vez de por referencia de objeto, porque el árbol
        /// se recarga entero (nueva consulta a BBDD) cada vez que se abre este diálogo.</summary>
        private void SeleccionarYExpandirRuta(List<OpcionCarpetaTreeNode> raiz, string[] ruta)
        {
            if (ruta.Length == 0) return;

            var nodosActuales = raiz;
            OpcionCarpetaTreeNode? nodo = null;
            foreach (var nombre in ruta)
            {
                nodo = nodosActuales.FirstOrDefault(n => string.Equals(n.Nombre, nombre, StringComparison.Ordinal));
                if (nodo == null) return;
                nodo.IsExpanded = true;
                nodosActuales = nodo.Hijos;
            }

            if (nodo != null) nodo.IsSelected = true;
        }

        private void TreeCarpetas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ObtenerTreeViewItemDesdeOrigen(e.OriginalSource as DependencyObject);
            if (item?.DataContext is not OpcionCarpetaTreeNode nodo) return;

            CarpetaSeleccionada = nodo;
            DialogResult = true;
        }

        private static TreeViewItem? ObtenerTreeViewItemDesdeOrigen(DependencyObject? source)
        {
            while (source != null && source is not TreeViewItem)
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (TreeCarpetas.SelectedItem is not OpcionCarpetaTreeNode nodo)
            {
                MessageBox.Show(Loc("L_Suite_SeleccionaCarpetaDestino"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CarpetaSeleccionada = nodo;
            DialogResult = true;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
