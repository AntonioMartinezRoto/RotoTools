using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Microsoft.Win32;
using RotoEntities;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.ControlCambios
{
    /// <summary>
    /// Sustituye a ControlCambiosMenu.cs/.Designer.cs (WinForms). Ver el comentario grande en
    /// ControlCambiosPage.xaml y en CrearMenuOculto (más abajo) para la explicación de por qué
    /// toda la generación del PDF y el motor de comparación de XML se reutilizan por reflexión en
    /// vez de transcribirse a mano: son casi 3000 líneas de lógica autocontenida (no dependen de
    /// ningún control de WinForms, solo de List&lt;DiferenciaXml&gt;/XmlData/RotoEntities), y ya
    /// existe precedente exacto para este mismo criterio en ConectorHerrajePage.xaml.cs
    /// (ObtenerSetsParaConector/GetSetsToConnector).
    /// </summary>
    public partial class ControlCambiosPage : UserControl
    {
        #region Estado

        private XmlData? _xmlOrigen = new();
        private XmlData? _xmlNuevo = new();
        private bool _xmlOrigenCargado;
        private bool _xmlNuevoCargado;
        private XmlNamespaceManager? _nsmgr;

        // Mismos valores por defecto que InitializeCompareFittings/InitializeCompareSets +
        // las propiedades públicas compareColours/compareOptions/compareFittingGroups/
        // compareMecanizados (todas = true por defecto) del ControlCambiosMenu original.
        private bool _compareColours = true;
        private ComparaFittingsProperties _compareFittings = new ComparaFittingsProperties
        {
            compararFittings = true,
            compararFittingsManufacturer = true,
            compararFittingsLength = true,
            compararFittingsLocation = true,
            compararFittingsDescription = true,
            compararFittingsArticles = true
        };
        private ComparaSetsProperties _compareSets = new ComparaSetsProperties
        {
            compararSets = true,
            compararCantidadSetDescriptions = true
        };
        private bool _compareOptions = true;
        private bool _compareFittingGroups = true;
        private bool _compareMecanizados = true;

        #endregion

        public ControlCambiosPage()
        {
            InitializeComponent();

            CargarTextos();
            SetMode();
        }

        #region Localización

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_ControlCambios");
            TxtSubtitulo.Text = Loc("L_Suite_ControlCambiosSubtitulo");

            TxtFicherosTitulo.Text = Loc("L_Suite_FicherosXmlTitulo");
            LblXml1.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarXMLAnterior");
            LblXml2.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarXMLNuevo");

            TxtCardCompareTitulo.Text = RotoTools.LocalizationManager.GetString("L_GenerarInforme");
            TxtCardCompareDesc.Text = Loc("L_Suite_GenerarInformeDesc");
            TxtCardConfigTitulo.Text = RotoTools.LocalizationManager.GetString("L_ConfiguracionInforme");
            TxtCardConfigDesc.Text = Loc("L_Suite_ConfiguracionInformeDesc");

            // Mismo texto que la tarjeta "avanzada" (L_GenerarInforme): el original también
            // reutiliza la misma clave para lbl_ControlCambios y lbl_ControlCambiosSimple (ver
            // CargarTextos en ControlCambiosMenu.cs) — no se corrige aquí, se mantiene tal cual.
            TxtCardSimpleTitulo.Text = RotoTools.LocalizationManager.GetString("L_GenerarInforme");
            TxtCardSimpleDesc.Text = Loc("L_Suite_GenerarInformeSimpleDesc");
        }

        /// <summary>Igual que SetMode() en el original: alterna entre el informe "avanzado" (con
        /// botón de configuración) y el informe "simple" (un único botón) según
        /// Properties.Settings.Default["ControlCambiosAvanzado"] — aquí, App.CurrentSettings.
        /// ControlCambiosAvanzado (ver Services/AppSettingsService.cs). Por defecto es false
        /// (igual que el valor por defecto de esa Setting en RotoTools/Properties/Settings.settings),
        /// así que de momento se muestra siempre el modo simple: el módulo "Opciones" (donde en la
        /// app original se cambia este ajuste, OptionsMenu.chk_ControlCambiosAvanzado) todavía no
        /// está migrado a la Suite.</summary>
        private void SetMode()
        {
            bool avanzado = App.CurrentSettings.ControlCambiosAvanzado;
            PanelAvanzado.Visibility = avanzado ? Visibility.Visible : Visibility.Collapsed;
            PanelSimple.Visibility = avanzado ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Events

        private void BtnSelectXml1_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml", Title = "Selecciona XML" };

            if (openFileDialog.ShowDialog() == true)
            {
                EnableButtons(false);
                string rutaXml = openFileDialog.FileName;
                _xmlOrigen = LoadXml(rutaXml, LblXml1);
                LblXml1.Text = rutaXml;
                EnableButtons(true);
                // Igual que el original: xmlOrigenCargado se pone a true SIEMPRE tras llamar a
                // LoadXml, aunque haya fallado y devuelto null (LoadXml solo hace try/catch
                // devolviendo null). No se corrige aquí, se mantiene tal cual.
                _xmlOrigenCargado = true;
                FillListasConfiguracion();
            }
        }

        private void BtnSelectXml2_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml", Title = "Selecciona XML" };

            if (openFileDialog.ShowDialog() == true)
            {
                EnableButtons(false);
                string rutaXml = openFileDialog.FileName;
                _xmlNuevo = LoadXml(rutaXml, LblXml2);
                LblXml2.Text = rutaXml;
                EnableButtons(true);
                _xmlNuevoCargado = true;
                FillListasConfiguracion();
            }
        }

        private void BtnCompare_Click(object sender, RoutedEventArgs e)
        {
            if (_xmlOrigenCargado && _xmlNuevoCargado)
            {
                List<DiferenciaXml> diferenciaslist = CompararXmlDataReflexion();

                if (diferenciaslist.Count > 0)
                {
                    var saveFileDialog = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = "Roto.pdf" };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string outputPath = saveFileDialog.FileName;
                        GenerarPdfReflexion(outputPath, diferenciaslist, avanzado: true);
                        MessageBox.Show("Comparación finalizada. Se generó el PDF.");
                    }
                }
                else
                {
                    MessageBox.Show("No se encontraron diferencias.");
                }
            }
        }

        private void BtnGenerarInformeSimple_Click(object sender, RoutedEventArgs e)
        {
            if (_xmlOrigenCargado && _xmlNuevoCargado)
            {
                List<DiferenciaXml> diferenciaslist = CompararXmlDataSimpleReflexion();

                if (diferenciaslist.Count > 0)
                {
                    var saveFileDialog = new SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf", FileName = "Roto.pdf" };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string outputPath = saveFileDialog.FileName;
                        GenerarPdfReflexion(outputPath, diferenciaslist, avanzado: false);
                        MessageBox.Show("Comparación finalizada. Se generó el PDF.");
                    }
                }
                else
                {
                    MessageBox.Show("No se encontraron diferencias.");
                }
            }
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ControlCambiosConfiguracionWindow(
                _compareOptions, _compareFittingGroups, _compareFittings, _compareSets, _compareColours, _compareMecanizados)
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                _compareColours = ventana.CompararColores;
                _compareFittingGroups = ventana.CompararFittingGroups;
                _compareFittings = ventana.CompararFittings;
                _compareOptions = ventana.CompararOpciones;
                _compareSets = ventana.CompararSets;
                _compareMecanizados = ventana.CompararMecanizados;
            }
        }

        #endregion

        #region Private Methods

        private XmlData? LoadXml(string xmlPath, TextBlock lblEstado)
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
                    lblEstado.Text = RotoTools.LocalizationManager.GetString("L_Cargando") + $"... {type} {value.TrimEnd()}";
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
                xmlData.FittingsVersion = loader.LoadFittingsVersion(doc);
                xmlData.OptionsVersion = loader.LoadOptionsVersion(doc);
                xmlData.ColoursVersion = loader.LoadColoursVersion(doc);
                xmlData.FittingGroupVersion = loader.LoadFittingGroupVersion(doc);

                return xmlData;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Igual que EnableButtons en el original: solo alterna btn_SelectXml1,
        /// btn_SelectXml2 y btn_Compare — btn_GenerarInformeSimple (modo simple) y btn_Config no se
        /// tocan, igual que en ControlCambiosMenu.cs. No se corrige aquí, se mantiene tal cual.</summary>
        private void EnableButtons(bool enable)
        {
            BtnSelectXml1.IsEnabled = enable;
            BtnSelectXml2.IsEnabled = enable;
            BtnCompare.IsEnabled = enable;
        }

        /// <summary>Igual que SetListasSetsComparaSets/SetListasFittingsComparaFittings/
        /// FillListasConfiguracion en el original: rellena las listas de "comunes"/"solo en XML 1"/
        /// "solo en XML 2" que usa la ventana de filtro (ControlCambiosFiltroItemsWindow). Es lógica
        /// corta y no depende de ningún control de WinForms, así que se ha portado directamente
        /// (igual que AplicarOpcionesLWC en ConectorHerrajePage.xaml.cs) en vez de por reflexión.</summary>
        private void FillListasConfiguracion()
        {
            if (_xmlOrigenCargado && _xmlNuevoCargado && _xmlOrigen != null && _xmlNuevo != null)
            {
                var setsXml1 = _xmlOrigen.SetList.Select(s => s.Code.Trim()).ToHashSet();
                var setsXml2 = _xmlNuevo.SetList.Select(s => s.Code.Trim()).ToHashSet();

                _compareSets.compararSetsComunesList = setsXml1.Intersect(setsXml2).ToList();
                _compareSets.compararSetsSoloXml1List = setsXml1.Except(setsXml2).ToList();
                _compareSets.compararSetsSoloXml2List = setsXml2.Except(setsXml1).ToList();

                var fittingGroupsXml1 = _xmlOrigen.FittingGroupList.Select(s => s.Class.Trim()).ToHashSet();
                var fittingGroupsXml2 = _xmlNuevo.FittingGroupList.Select(s => s.Class.Trim()).ToHashSet();

                _compareFittings.compararFittingsComunesList = fittingGroupsXml1.Intersect(fittingGroupsXml2).ToList();
            }
        }

        #endregion

        #region Reutilización de RotoTools.ControlCambiosMenu por reflexión

        /// <summary>
        /// Crea una instancia OCULTA (nunca visible; nunca se llama Show/ShowDialog sobre ella) de
        /// RotoTools.ControlCambiosMenu y le inyecta los mismos datos privados que usaría el menú
        /// clásico: xmlOrigen/xmlNuevo (campos privados, vía reflexión) y lbl_Xml1/lbl_Xml2 (Labels
        /// de WinForms que GeneratePdf/GeneratePdfSimple leen para escribir la cabecera del PDF —
        /// InsertHeader/InsertHeaderSimple hacen "lbl_Xml1.Text"/"lbl_Xml2.Text" tal cual, ver
        /// ControlCambiosMenu.cs línea ~1899). compareColours/compareSets/compareFittings/
        /// compareOptions/compareFittingGroups/compareMecanizados SÍ son propiedades públicas en el
        /// original, así que esas se asignan directamente, sin reflexión.
        /// </summary>
        private RotoTools.ControlCambiosMenu CrearMenuOculto()
        {
            var menuOculto = new RotoTools.ControlCambiosMenu();
            var tipo = menuOculto.GetType();

            tipo.GetField("xmlOrigen", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(menuOculto, _xmlOrigen);
            tipo.GetField("xmlNuevo", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(menuOculto, _xmlNuevo);

            var lblXml1 = (System.Windows.Forms.Label)tipo
                .GetField("lbl_Xml1", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(menuOculto)!;
            lblXml1.Text = LblXml1.Text;

            var lblXml2 = (System.Windows.Forms.Label)tipo
                .GetField("lbl_Xml2", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(menuOculto)!;
            lblXml2.Text = LblXml2.Text;

            menuOculto.compareColours = _compareColours;
            menuOculto.compareSets = _compareSets;
            menuOculto.compareFittings = _compareFittings;
            menuOculto.compareOptions = _compareOptions;
            menuOculto.compareFittingGroups = _compareFittingGroups;
            menuOculto.compareMecanizados = _compareMecanizados;

            return menuOculto;
        }

        /// <summary>CompareXmlData SÍ es un método público en el original (a diferencia de
        /// GeneratePdf/GeneratePdfSimple, que son privados), así que se invoca directamente, sin
        /// reflexión.</summary>
        private List<DiferenciaXml> CompararXmlDataReflexion()
        {
            using var menuOculto = CrearMenuOculto();
            return menuOculto.CompareXmlData(_xmlOrigen!, _xmlNuevo!);
        }

        /// <summary>CompareXmlDataSimple también es público, pero (a diferencia de CompareXmlData)
        /// no recibe xmlOrigen/xmlNuevo como parámetros: los lee de sus propios campos privados
        /// (this.xmlOrigen/this.xmlNuevo), que CrearMenuOculto ya le ha inyectado por reflexión.</summary>
        private List<DiferenciaXml> CompararXmlDataSimpleReflexion()
        {
            using var menuOculto = CrearMenuOculto();
            return menuOculto.CompareXmlDataSimple();
        }

        /// <summary>GeneratePdf/GeneratePdfSimple SÍ son métodos privados en el original, así que
        /// aquí sí hace falta invocarlos por reflexión (ver comentario de CrearMenuOculto).</summary>
        private void GenerarPdfReflexion(string outputPath, List<DiferenciaXml> diferenciasList, bool avanzado)
        {
            using var menuOculto = CrearMenuOculto();
            string nombreMetodo = avanzado ? "GeneratePdf" : "GeneratePdfSimple";
            var metodo = menuOculto.GetType().GetMethod(nombreMetodo, BindingFlags.NonPublic | BindingFlags.Instance)!;
            metodo.Invoke(menuOculto, new object[] { outputPath, diferenciasList });
        }

        #endregion

        /// <summary>Equivalente WPF de Application.DoEvents() (mismo helper que
        /// ConectorHerrajePage/ManillasFKSPage/TariffImporterPage/TraduccionPage).</summary>
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
