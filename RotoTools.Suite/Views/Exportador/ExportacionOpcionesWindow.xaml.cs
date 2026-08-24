using System.Windows;

namespace RotoTools.Suite.Views.Exportador
{
    /// <summary>
    /// Sustituye a ExportacionOpciones.cs/.Designer.cs (WinForms): ver el comentario grande en el
    /// XAML sobre por qué todos los textos de esta ventana son literales fijos en español (el
    /// original nunca los pasaba por LocalizationManager).
    /// </summary>
    public partial class ExportacionOpcionesWindow : Window
    {
        public bool ShowSetDescriptionId { get; private set; }
        public bool ShowSetDescriptionPosition { get; private set; }
        public bool ShowFittingId { get; private set; }
        public bool ShowFittingLength { get; private set; }
        public bool FormatoTabla { get; private set; }
        public bool ShowSetId { get; private set; }

        public ExportacionOpcionesWindow(bool showSetDescriptionId, bool showSetDescriptionPosition,
            bool showFittingId, bool showFittingLength, bool formatoTabla, bool showSetId)
        {
            InitializeComponent();

            ShowSetDescriptionId = showSetDescriptionId;
            ShowSetDescriptionPosition = showSetDescriptionPosition;
            ShowFittingId = showFittingId;
            ShowFittingLength = showFittingLength;
            FormatoTabla = formatoTabla;
            ShowSetId = showSetId;

            ChkSDId.IsChecked = showSetDescriptionId;
            ChkPosition.IsChecked = showSetDescriptionPosition;
            ChkFittingId.IsChecked = showFittingId;
            ChkFittingLength.IsChecked = showFittingLength;
            ChkFormatoTabla.IsChecked = formatoTabla;
            ChkSetId.IsChecked = showSetId;
        }

        private void BtnSaveOptions_Click(object sender, RoutedEventArgs e)
        {
            ShowSetDescriptionId = ChkSDId.IsChecked == true;
            ShowSetDescriptionPosition = ChkPosition.IsChecked == true;
            ShowFittingId = ChkFittingId.IsChecked == true;
            ShowFittingLength = ChkFittingLength.IsChecked == true;
            FormatoTabla = ChkFormatoTabla.IsChecked == true;
            ShowSetId = ChkSetId.IsChecked == true;

            DialogResult = true;
            Close();
        }
    }
}
