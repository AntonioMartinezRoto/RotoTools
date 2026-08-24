using System.Windows;

namespace RotoTools.Suite.Views.Cam
{
    /// <summary>
    /// Sustituye a CamInfoOperacion.cs (WinForms): diálogo de solo lectura con todas las
    /// apariciones (por herraje) de una operación, idéntico en contenido al original (columnas
    /// FittingID/Article/Descripcion/X/SetDescriptionXPosition/Location — el resto de campos de
    /// OperationGridRow no se mostraban en el original tampoco).
    /// </summary>
    public partial class CamInfoOperacionWindow : Window
    {
        public CamInfoOperacionWindow(string operationName, List<OperationGridRow> operationsList)
        {
            InitializeComponent();

            TxtTitulo.Text = operationName;
            Title = operationName;
            GridDetalle.ItemsSource = operationsList;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();
    }
}
