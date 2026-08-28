using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Microsoft.Win32;
using RotoEntities;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.ConectorHerraje
{
    /// <summary>
    /// Sustituye a ConectorHerrajeMenu.cs/.Designer.cs (WinForms): carga de XML de herrajes y
    /// selección de una de las 3 acciones del módulo Conector de Herraje. Reutiliza tal cual la
    /// lógica de negocio del proyecto original (RotoTools.Helpers, RotoTools.LocalizationManager,
    /// RotoTools.XmlLoader, RotoEntities.XmlData) vía ProjectReference.
    ///
    /// Los 3 submódulos ya están migrados: Generar conector de herraje -> ConectorHerrajeGeneradorWindow,
    /// Combinar conectores de herraje -> ConectorHerrajeCombinarWindow, Revisión de sets ->
    /// ConectorHerrajeRevisionSetsWindow (ver BtnGeneraConector_Click/BtnCombinarConectores_Click/
    /// BtnSetsNoUtilizados_Click más abajo).
    /// </summary>
    public partial class ConectorHerrajePage : UserControl
    {
        // ------------------------------------------------------------------
        // Estado (equivalente a los campos privados de ConectorHerrajeMenu.cs)
        // ------------------------------------------------------------------
        private XmlData? _xmlOrigen;
        private bool _xmlCargado;
        private XmlNamespaceManager? _nsmgr;

        public ConectorHerrajePage()
        {
            InitializeComponent();

            CargarTextos();
            CargarDatos();
        }

        #region Localización / cabecera

        private void CargarTextos()
        {
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_ConectorHerraje");
            TxtSubtitulo.Text = SuiteLocalization.GetString("L_Suite_ConectorHerrajeSubtitulo");

            TxtBtnCargarXml.Text = SuiteLocalization.GetString("L_Suite_CargarXml");
            LblXml.Text = "";

            TxtCard1Titulo.Text = RotoTools.LocalizationManager.GetString("L_GenerarConector");
            TxtCard1Desc.Text = SuiteLocalization.GetString("L_Suite_GenerarConectorDesc");
            ChkConfigAE.Content = RotoTools.LocalizationManager.GetString("L_BalconerasAEconAI");
            TxtCard2Titulo.Text = RotoTools.LocalizationManager.GetString("L_CombinarConectores");
            TxtCard2Desc.Text = SuiteLocalization.GetString("L_Suite_CombinarConectoresDesc");
            TxtCard3Titulo.Text = RotoTools.LocalizationManager.GetString("L_RevisionSets");
            TxtCard3Desc.Text = SuiteLocalization.GetString("L_Suite_RevisionSetsDesc");

            TxtCard4Titulo.Text = SuiteLocalization.GetString("L_Suite_CambiarConectorActivo");
            TxtCard4Desc.Text = SuiteLocalization.GetString("L_Suite_CambiarConectorActivoDesc");
        }

        /// <summary>Igual que ConectorHerrajeMenu.CargarDatos: comprueba la conexión/conector
        /// activo y si la versión de base de datos es compatible con la suite (v2020+); si no lo
        /// es, deshabilita las 4 acciones y muestra el aviso, igual que ShowVersionNoCompatible.
        /// Público (no solo se llama desde el constructor): MainWindow.BtnActualizarConexion_Click
        /// también lo invoca para volver a comprobar la compatibilidad si la cadena de conexión
        /// se ha cambiado fuera de la Suite (por ejemplo con el propio RotoTools) mientras este
        /// módulo ya estaba abierto, sin necesidad de navegar a otro módulo y volver.</summary>
        public void CargarDatos()
        {
            ActualizarInfoConexion();

            if (RotoTools.Helpers.IsVersionPrefSuiteCompatible())
            {
                EnableButtons(true);
                BorderAvisoVersion.Visibility = Visibility.Collapsed;
            }
            else
            {
                EnableButtons(false);
                BorderAvisoVersion.Visibility = Visibility.Visible;
                TxtAvisoVersion.Text = SuiteLocalization.GetString("L_Suite_BaseDatosNoCompatible");
            }
        }

        /// <summary>Igual que ConectorHerrajeMenu.InitializeInfoConnection: el servidor/base de
        /// datos ya se muestra siempre arriba (ver MainWindow), así que aquí solo se repite el
        /// "conector activo" (Helpers.GetConectorActivo -> variable global "Conector Herraje"),
        /// que es información propia de este módulo y no aparece en ningún otro sitio.</summary>
        private void ActualizarInfoConexion()
        {
            string conectorActivo = RotoTools.Helpers.GetConectorActivo() ?? "";
            LblConectorActivo.Text = SuiteLocalization.GetString("L_Suite_ConectorActivo") + ": " + conectorActivo;
        }

        private void EnableButtons(bool habilitado)
        {
            BtnLoadXml.IsEnabled = habilitado;
            BtnGeneraConector.IsEnabled = habilitado;
            BtnCombinarConectores.IsEnabled = habilitado;
            BtnSetsNoUtilizados.IsEnabled = habilitado;
            BtnCambiarConectorActivo.IsEnabled = habilitado;
        }

        #endregion

        #region Carga de XML (LoadXml / btn_LoadXml_Click)

        private void BtnLoadXml_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };

            if (openFileDialog.ShowDialog() == true)
            {
                string rutaXml = openFileDialog.FileName;
                EnableButtons(false);

                _xmlOrigen = LoadXml(rutaXml);
                LblXml.Text = rutaXml;

                EnableButtons(true);
            }
        }

        /// <summary>Idéntico a ConectorHerrajeMenu.LoadXml: XmlLoader (no XmlSerializer) sobre el
        /// namespace "hw" del esquema de herrajes. Si algo falla, devuelve null silenciosamente,
        /// igual que el original (no había MessageBox de error aquí).</summary>
        private XmlData? LoadXml(string xmlPath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(xmlPath);

                _nsmgr = new XmlNamespaceManager(doc.NameTable);
                _nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

                var loader = new RotoTools.XmlLoader(_nsmgr);
                loader.OnLoadingInfo += (tipo, valor) =>
                {
                    LblXml.Text = RotoTools.LocalizationManager.GetString("L_Cargando") + $"... {tipo} {valor?.TrimEnd()}";
                    DoEvents();
                };

                var xmlData = new XmlData
                {
                    Supplier = loader.LoadSupplier(doc)
                };
                xmlData.HardwareType = loader.LoadHardwareType(xmlData.Supplier);
                xmlData.FittingGroupList = loader.LoadFittingGroups(doc);
                xmlData.ColourList = loader.LoadColourMaps(doc);
                xmlData.OptionList = loader.LoadDocOptions(doc);
                xmlData.FittingList = loader.LoadFittings(doc);
                xmlData.SetList = loader.LoadSets(doc, xmlData.FittingList);

                _xmlCargado = true;
                return xmlData;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Acciones (tarjetas)

        /// <summary>El checkbox vive dentro de la tarjeta 1 (que es en sí misma un Button); sin
        /// marcar el evento como controlado, el clic se propagaría también al Click de la
        /// tarjeta (ButtonBase.Click es un evento enrutado que burbujea) y abriría "Generar
        /// Conector" cada vez que se marca/desmarca la casilla.</summary>
        private void ChkConfigAE_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>Igual que btn_GeneraConector_Click (ConectorHerrajeMenu.cs): sin XML
        /// cargado, no hace nada. Calcula los Sets del conector (ver ObtenerSetsParaConector),
        /// aplica la traducción opcional de escandallos y la gestión de Sets "Low Cost", y abre
        /// ConectorHerrajeGeneradorWindow (ya migrada) con ese resultado.</summary>
        private void BtnGeneraConector_Click(object sender, RoutedEventArgs e)
        {
            if (!_xmlCargado || _xmlOrigen == null) return;

            if (RotoTools.TranslateManager.PermitirTraduccionesEnConectorEscandallos)
            {
                if (MessageBox.Show(RotoTools.LocalizationManager.GetString("L_AplicarPlantillaTraduccion"), "",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var openFileDialog = new OpenFileDialog { Filter = "XLS Files (*.xls)|*.xlsx" };
                    if (openFileDialog.ShowDialog() == true)
                    {
                        RotoTools.TranslateManager.AplicarTraduccion = true;
                        RotoTools.TranslateManager.TraduccionesActuales = RotoTools.Helpers.CargarTraducciones(openFileDialog.FileName);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    RotoTools.TranslateManager.AplicarTraduccion = false;
                }
            }

            List<Set> setsConector = ObtenerSetsParaConector(_xmlOrigen, ChkConfigAE.IsChecked == true);

            // Gestión Sets Ventana para Low Cost (igual que SetLWCOptions en ConectorHerrajeMenu).
            if (_xmlOrigen.SetList.Any(s => (s.Code ?? "").ToUpper().Contains("LWC")))
                AplicarOpcionesLWC(setsConector);

            if (RotoTools.TranslateManager.AplicarTraduccion)
            {
                foreach (var set in setsConector)
                    set.Code = RotoTools.SetHelpers.TranslateSet(set.Code);
            }

            new ConectorHerrajeGeneradorWindow(_xmlOrigen, setsConector, _xmlOrigen.Supplier)
            {
                Owner = Window.GetWindow(this)
            }.ShowDialog();

            ActualizarInfoConexion();
        }

        /// <summary>GetSetsToConnector() —y las casi 9000 líneas de reglas de negocio de
        /// PVC/Aluminio/PAX que invoca internamente— es un método PRIVADO de instancia de
        /// ConectorHerrajeMenu, no una clase de ayuda pública reutilizable. Reescribir esa lógica
        /// aquí sería enorme y muy arriesgado de transcribir a mano. En vez de eso, se reutiliza
        /// el código EXACTO del original sin tocar RotoTools.csproj: se crea una instancia oculta
        /// (nunca visible; nunca se llama Show/ShowDialog sobre ella) de ConectorHerrajeMenu, se
        /// le inyectan por reflexión los mismos datos que usaría el menú clásico (el XML cargado,
        /// el tipo de herraje y el estado del checkbox "Balconeras AE con Sets de AI"), y se
        /// invoca su método privado GetSetsToConnector() tal cual.</summary>
        private static List<Set> ObtenerSetsParaConector(XmlData xmlOrigen, bool balconerasAEconSetsDeAI)
        {
            var tipo = typeof(RotoTools.ConectorHerrajeMenu);

            using var menuOculto = new RotoTools.ConectorHerrajeMenu();

            tipo.GetField("xmlOrigen", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(menuOculto, xmlOrigen);

            tipo.GetProperty("HardwareType", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(menuOculto, xmlOrigen.HardwareType);

            var chkConfigAE = (System.Windows.Forms.CheckBox)tipo
                .GetField("chk_ConfigAE", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(menuOculto)!;
            chkConfigAE.Checked = balconerasAEconSetsDeAI;

            var metodo = tipo.GetMethod("GetSetsToConnector", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (List<Set>)metodo.Invoke(menuOculto, null)!;
        }

        /// <summary>Igual que SetLWCOptions (ConectorHerrajeMenu.cs): distingue las tablas STD de
        /// LWC añadiendo la opción TIPO_VENTANA_STD a los Sets de tipo Ventana. A diferencia de
        /// GetSetsToConnector, este método es corto y no depende de ningún control de UI, así que
        /// se ha portado directamente en vez de invocarlo por reflexión.</summary>
        private static void AplicarOpcionesLWC(List<Set> setsConector)
        {
            foreach (var set in setsConector.Where(s =>
                         s.WindowType == (int)RotoTools.Enums.enumWindowType.Ventana && !string.IsNullOrEmpty(s.Code)))
            {
                if (set.Code.ToUpper().Contains("LWC"))
                {
                    if (!set.OptionConectorList.Any(o => o.Name == "RO_TIPO_VENTANA_STD"))
                        set.OptionConectorList.Add(RotoTools.OpcionHelper.Crear("TIPO_VENTANA_STD", "LWC"));
                }
                else
                {
                    if (!set.OptionConectorList.Any(o => o.Name == "RO_TIPO_VENTANA_STD"))
                        set.OptionConectorList.Add(RotoTools.OpcionHelper.Crear("TIPO_VENTANA_STD", "STD"));
                }
            }
        }

        /// <summary>Igual que btn_CombinarConectores_Click: el original no exige tener un XML
        /// cargado para combinar conectores ya generados anteriormente.</summary>
        private void BtnCombinarConectores_Click(object sender, RoutedEventArgs e)
        {
            new ConectorHerrajeCombinarWindow { Owner = Window.GetWindow(this) }.ShowDialog();
            ActualizarInfoConexion();
        }

        /// <summary>Igual que btn_SetsNoUtilizados_Click: sin XML cargado, no hace nada.</summary>
        private void BtnSetsNoUtilizados_Click(object sender, RoutedEventArgs e)
        {
            if (!_xmlCargado || _xmlOrigen == null) return;

            new ConectorHerrajeRevisionSetsWindow(_xmlOrigen) { Owner = Window.GetWindow(this) }.ShowDialog();
        }

        /// <summary>Nueva (no existía en el original): igual que btn_CombinarConectores_Click, no
        /// exige tener un XML cargado, ya que solo cambia qué conector ya guardado en BBDD queda
        /// como activo (RotoTools.Helpers.GetConectorActivo/VariablesGlobales, ver
        /// ConectorHerrajeActivoWindow). ActualizarInfoConexion() refresca aquí LblConectorActivo
        /// para que la página muestre el nuevo activo sin tener que reabrir el módulo.</summary>
        private void BtnCambiarConectorActivo_Click(object sender, RoutedEventArgs e)
        {
            new ConectorHerrajeActivoWindow { Owner = Window.GetWindow(this) }.ShowDialog();
            ActualizarInfoConexion();
        }

        #endregion

        /// <summary>Equivalente WPF de Application.DoEvents(): bombea el bucle de mensajes para
        /// que la etiqueta de "cargando..." se repinte durante la carga síncrona del XML, igual
        /// que hacía la app WinForms original.</summary>
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
