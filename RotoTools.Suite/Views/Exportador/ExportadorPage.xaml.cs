using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Microsoft.Win32;
using RotoEntities;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.Exportador
{
    /// <summary>
    /// Sustituye a ExportacionMenu.cs/.Designer.cs (WinForms). Ver el comentario grande en el XAML
    /// para el resumen general (por qué "Exportar a Opera" no se migra).
    /// </summary>
    public partial class ExportadorPage : UserControl
    {
        #region Estado

        private XmlData? _xmlFile = new();
        private bool _xmlLoadedFile;
        private XmlNamespaceManager? _nsmgr;

        #endregion

        public ExportadorPage()
        {
            InitializeComponent();

            CargarTextos();
        }

        #region Localización

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_ExportarDatos");
            TxtSubtitulo.Text = Loc("L_Suite_ExportadorSubtitulo");

            TxtFicheroTitulo.Text = Loc("L_Suite_ExportadorFicheroXmlTitulo");
            LblInfo.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarXML");

            TxtCardWinPerfilTitulo.Text = RotoTools.LocalizationManager.GetString("L_ExportarWinPerfil");
            TxtCardWinPerfilDesc.Text = Loc("L_Suite_ExportarWinPerfilDesc");

            TxtCardOrgadataTitulo.Text = RotoTools.LocalizationManager.GetString("L_ExportarOrgadata");
            TxtCardOrgadataDesc.Text = Loc("L_Suite_ExportarOrgadataDesc");
        }

        #endregion

        #region Events

        private void BtnSelectXml_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml", Title = "Selecciona XML" };

            if (openFileDialog.ShowDialog() == true)
            {
                EnableButtons(false);
                string xmlPath = openFileDialog.FileName;
                _xmlFile = LoadXml(xmlPath);
                LblInfo.Text = xmlPath;
                EnableButtons(true);
                // Igual que el original: xmlLoadedFile se pone a true SIEMPRE tras llamar a
                // LoadXml, aunque haya fallado y devuelto null (LoadXml solo hace try/catch
                // devolviendo null). No se corrige aquí (mismo criterio ya aplicado en
                // ControlCambiosPage.BtnSelectXml1_Click).
                _xmlLoadedFile = true;
            }
        }

        private void BtnExportWinPerfil_Click(object sender, RoutedEventArgs e)
        {
            if (!_xmlLoadedFile) return;

            var ventana = new ExportacionWinPerfilWindow(_xmlFile!) { Owner = Window.GetWindow(this) };
            ventana.ShowDialog();
        }

        private void BtnExportOrgadata_Click(object sender, RoutedEventArgs e)
        {
            if (!_xmlLoadedFile) return;

            var ventana = new ExportacionOrgadataWindow(_xmlFile!) { Owner = Window.GetWindow(this) };
            ventana.ShowDialog();
        }

        #endregion

        #region Private Methods

        private XmlData? LoadXml(string xmlPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);

                _nsmgr = new XmlNamespaceManager(doc.NameTable);
                _nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

                RotoTools.XmlLoader loader = new RotoTools.XmlLoader(_nsmgr);
                loader.OnLoadingInfo += (type, value) =>
                {
                    LblInfo.Text = RotoTools.LocalizationManager.GetString("L_Cargando") + $"... {type} {value.TrimEnd()}";
                    DoEvents();
                };

                XmlData xmlData = new XmlData();
                xmlData.Supplier = loader.LoadSupplier(doc);
                xmlData.HardwareType = loader.LoadHardwareType(xmlData.Supplier);
                xmlData.FittingGroupList = loader.LoadFittingGroups(doc);
                xmlData.ColourList = loader.LoadColourMaps(doc);
                xmlData.OptionList = loader.LoadDocOptions(doc);
                xmlData.FittingList = loader.LoadFittings(doc);
                xmlData.SetList = loader.LoadSets(doc, xmlData.FittingList);

                return xmlData;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Igual que EnableButtons en el original: solo alterna btn_ExportWinPerfil y
        /// btn_ExportOrgadata (btn_ExportOpera también se alternaba en el original, pero como esa
        /// tarjeta no se migra aquí, no hay nada que tocar para ella).</summary>
        private void EnableButtons(bool enable)
        {
            BtnExportWinPerfil.IsEnabled = enable;
            BtnExportOrgadata.IsEnabled = enable;
        }

        #endregion

        /// <summary>Equivalente WPF de Application.DoEvents() (mismo helper que
        /// ControlCambiosPage/ConectorHerrajePage/ManillasFKSPage/TariffImporterPage/TraduccionPage).</summary>
        private static void DoEvents()
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new System.Windows.Threading.DispatcherOperationCallback(f =>
                {
                    ((System.Windows.Threading.DispatcherFrame)f!).Continue = false;
                    return null;
                }), frame);
            System.Windows.Threading.Dispatcher.PushFrame(frame);
        }
    }
}
