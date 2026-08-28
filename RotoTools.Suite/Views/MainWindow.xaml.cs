using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RotoTools.Suite.Services;
using RotoTools.Suite.Views.Actualizador;
using RotoTools.Suite.Views.Cam;
using RotoTools.Suite.Views.ConectorHerraje;
using RotoTools.Suite.Views.ConfiguradorOpciones;
using RotoTools.Suite.Views.ControlCambios;
using RotoTools.Suite.Views.Exportador;
using RotoTools.Suite.Views.ManillasFKS;
using RotoTools.Suite.Views.Opciones;
using RotoTools.Suite.Views.TariffImporter;
using RotoTools.Suite.Views.Traduccion;

namespace RotoTools.Suite.Views
{
    /// <summary>
    /// Elemento del menú de navegación lateral: un módulo del menú principal original
    /// (Main.Designer.cs). "Icono"+"Color" forman la insignia circular de cada módulo (icono
    /// vectorial propio, ver Theme/RotoBrand.xaml: sin depender de ningún paquete de iconos de
    /// terceros); "CrearPagina" construye la página que se muestra al seleccionar el módulo. El
    /// mismo icono se reutiliza en la cabecera de la página del módulo (ver PlaceholderPage y
    /// ConfiguradorOpcionesPage) para que panel lateral y página central siempre coincidan.
    /// </summary>
    public class NavModuleItem
    {
        public string Titulo { get; set; } = "";
        public Geometry Icono { get; set; } = Geometry.Empty;
        public Brush Color { get; set; } = Brushes.Gray;
        public Func<UserControl> CrearPagina { get; set; } = () => new UserControl();
    }

    /// <summary>
    /// Ventana principal (shell) de RotoTools Suite: sustituye a Main.cs/Main.Designer.cs de la
    /// app WinForms original. Estructura + menú principal, con páginas "próximamente" para los
    /// módulos aún no migrados; Configurador de opciones, CAM (mecanizados 2D/3D, ver
    /// Views/Cam/CamPage.xaml), Conector de Herraje, Instalación (antes "Actualizador", ver
    /// Views/Actualizador/ActualizadorPage.xaml), Configuración Manillas FKS (ver
    /// Views/ManillasFKS/ManillasFKSPage.xaml), Cargar precios (antes "TariffImporter", ver
    /// Views/TariffImporter/TariffImporterPage.xaml), Traducción (ver
    /// Views/Traduccion/TraduccionPage.xaml), Control de cambios (ver
    /// Views/ControlCambios/ControlCambiosPage.xaml), Opciones (ver
    /// Views/Opciones/OpcionesPage.xaml) y Exportar datos (ver
    /// Views/Exportador/ExportadorPage.xaml) ya están migrados — con este último se completa la
    /// migración de todos los módulos del menú principal original. El resto (si lo hubiera) se irá
    /// añadiendo módulo a módulo en próximas entregas.
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _cargandoIdioma;
        private List<NavModuleItem> _modulos = new();

        public MainWindow()
        {
            InitializeComponent();

            CargarModulos();
            CargarSelectorIdioma();
            CargarTextos();
            CargarInfoConexion();
            CargarVersion();

            if (ListaModulos.Items.Count > 0)
                ListaModulos.SelectedIndex = 0;
        }

        /// <summary>
        /// Muestra junto a "RotoTools Suite", en la cabecera, la versión de ensamblado del propio
        /// RotoTools.Suite.csproj (AssemblyVersion/FileVersion, ambas puestas a la misma tipología
        /// X.X.X), con el formato "vX.X.X" pedido. Se lee de Assembly.GetName().Version en vez de
        /// escribir el número a mano aquí: AssemblyVersion se completa siempre a 4 partes
        /// (Major.Minor.Build.Revision, la última a 0 si no se especifica), así que solo se toman
        /// las 3 primeras para no mostrar ese ".0" final que no forma parte de la tipología usada
        /// en el .csproj.
        /// </summary>
        private void CargarVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            RunVersion.Text = version == null ? "" : $"v{version.Major}.{version.Minor}.{version.Build}";
        }

        private void CargarTextos()
        {
            LblIdioma.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarIdioma");
            TxtBtnSalir.Text = SuiteLocalization.GetString("L_Suite_Salir");
        }

        /// <summary>Botón nuevo (no existía como tal en el menú principal original, donde se
        /// cerraba la aplicación con los controles estándar de la ventana): cierra toda la
        /// aplicación, no solo la ventana principal, igual que el aspa de la ventana.</summary>
        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CargarInfoConexion()
        {
            try
            {
                string servidor = RotoTools.Helpers.GetServer();
                string baseDatos = RotoTools.Helpers.GetDataBase();
                TxtConexion.Text = string.IsNullOrWhiteSpace(servidor) ? "" : $@"{servidor}\{baseDatos}";
            }
            catch
            {
                TxtConexion.Text = "";
            }
        }

        /// <summary>Migrado de Main.cs (btn_Refresh_Click -> CargarDatos -> InitializeInfoConnection):
        /// vuelve a leer y mostrar el servidor/base de datos actuales, por si han cambiado desde
        /// que se abrió la aplicación. Añadido (no existía en el original): si el módulo Conector
        /// de Herraje es el que está abierto ahora mismo, también se repite su propia comprobación
        /// de compatibilidad de versión de BBDD (ConectorHerrajePage.CargarDatos), ya que esa
        /// comprobación solo se ejecutaba antes en el constructor de la página. Sin esto, si se
        /// cambiaba la cadena de conexión fuera de la Suite estando ya en ese módulo, el aviso de
        /// "base de datos no compatible" se quedaba bloqueado hasta cambiar de módulo y volver.</summary>
        private void BtnActualizarConexion_Click(object sender, RoutedEventArgs e)
        {
            CargarInfoConexion();

            if (ContentHost.Content is ConectorHerrajePage paginaConectorHerraje)
                paginaConectorHerraje.CargarDatos();
        }

        /// <summary>
        /// Mismo inventario de 10 módulos que Main.Designer.cs original, más "Inicio" (nuevo,
        /// pantalla de bienvenida que no existía en la app WinForms). Configurador de opciones,
        /// CAM, Conector de Herraje e Instalación ya abren su página real migrada; el resto sigue
        /// abriendo una página "próximamente" hasta que se migre en próximas entregas.
        /// </summary>
        private void CargarModulos()
        {
            var rojoRoto = (Brush)FindResource("RotoRedBrush");

            var iconoInicio = (Geometry)FindResource("IconHome");
            var iconoCam = (Geometry)FindResource("IconGearHex");
            var iconoConfigOpciones = (Geometry)FindResource("IconSliders");
            var iconoActualizador = (Geometry)FindResource("IconDownload");
            var iconoExportar = (Geometry)FindResource("IconUpload");
            var iconoConector = (Geometry)FindResource("IconLink");
            var iconoControlCambios = (Geometry)FindResource("IconClipboard");
            var iconoTraduccion = (Geometry)FindResource("IconChatBubble");
            var iconoManillas = (Geometry)FindResource("IconHandle");
            var iconoTarifas = (Geometry)FindResource("IconTag");
            var iconoAjustes = (Geometry)FindResource("IconDots");

            _modulos = new List<NavModuleItem>
            {
                new() { Titulo = "Inicio", Icono = iconoInicio, Color = rojoRoto,
                        CrearPagina = () => new DashboardPage() },

                new() { Titulo = "CAM · Mecanizados", Icono = iconoCam, Color = new SolidColorBrush(Color.FromRgb(0x2E,0x7D,0x32)),
                        CrearPagina = () => new CamPage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ConfiguradorOpciones"), Icono = iconoConfigOpciones, Color = new SolidColorBrush(Color.FromRgb(0x6A,0x5A,0xCD)),
                        CrearPagina = () => new ConfiguradorOpcionesPage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_Actualizador"), Icono = iconoActualizador, Color = new SolidColorBrush(Color.FromRgb(0x19,0x76,0xD2)),
                        CrearPagina = () => new ActualizadorPage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ExportarDatos"), Icono = iconoExportar, Color = new SolidColorBrush(Color.FromRgb(0xF5,0x7C,0x00)),
                        CrearPagina = () => new ExportadorPage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ConectorHerraje"), Icono = iconoConector, Color = new SolidColorBrush(Color.FromRgb(0x00,0x83,0x8F)),
                        CrearPagina = () => new ConectorHerrajePage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ControlCambios"), Icono = iconoControlCambios, Color = new SolidColorBrush(Color.FromRgb(0x8E,0x24,0xAA)),
                        CrearPagina = () => new ControlCambiosPage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_Traduccion"), Icono = iconoTraduccion, Color = new SolidColorBrush(Color.FromRgb(0xC2,0x18,0x5B)),
                        CrearPagina = () => new TraduccionPage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ConfManillasFKS"), Icono = iconoManillas, Color = new SolidColorBrush(Color.FromRgb(0x5D,0x40,0x37)),
                        CrearPagina = () => new ManillasFKSPage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_TariffImporter"), Icono = iconoTarifas, Color = new SolidColorBrush(Color.FromRgb(0x00,0x97,0xA7)),
                        CrearPagina = () => new TariffImporterPage() },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_Opciones"), Icono = iconoAjustes, Color = new SolidColorBrush(Color.FromRgb(0x45,0x5A,0x64)),
                        CrearPagina = () => new OpcionesPage() },
            };

            ListaModulos.ItemsSource = _modulos;
        }

        private void ListaModulos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaModulos.SelectedItem is NavModuleItem modulo)
                ContentHost.Content = modulo.CrearPagina();
        }

        /// <summary>
        /// Mismos 3 idiomas que ofrece hoy OptionsMenu.cs en la app WinForms original (es/en/pt):
        /// alemán e italiano existen como culturas de recursos, pero sus .resx todavía no tienen
        /// traducción real, así que de momento se dejan fuera del selector, igual que en la app
        /// original.
        /// </summary>
        private void CargarSelectorIdioma()
        {
            _cargandoIdioma = true;

            CmbIdioma.Items.Clear();
            CmbIdioma.Items.Add(new IdiomaItem("Español", "es"));
            CmbIdioma.Items.Add(new IdiomaItem("English", "en"));
            CmbIdioma.Items.Add(new IdiomaItem("Português", "pt"));

            CmbIdioma.DisplayMemberPath = "Nombre";

            string actual = RotoTools.LocalizationManager.CurrentCulture.TwoLetterISOLanguageName;

            foreach (var item in CmbIdioma.Items)
            {
                if (item is IdiomaItem idioma && idioma.Codigo == actual)
                {
                    CmbIdioma.SelectedItem = idioma;
                    break;
                }
            }

            if (CmbIdioma.SelectedItem == null && CmbIdioma.Items.Count > 0)
                CmbIdioma.SelectedIndex = 0;

            _cargandoIdioma = false;
        }

        private void CmbIdioma_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_cargandoIdioma)
                return;

            if (CmbIdioma.SelectedItem is not IdiomaItem idioma)
                return;

            RotoTools.LocalizationManager.SetLanguage(idioma.Codigo);

            App.CurrentSettings.Language = idioma.Codigo;
            AppSettingsService.Save(App.CurrentSettings);

            CargarTextos();

            // Reconstruye el menú (los títulos de los módulos vienen de LocalizationManager) y
            // vuelve a mostrar la página actualmente seleccionada, ya en el nuevo idioma.
            int indiceSeleccionado = ListaModulos.SelectedIndex;
            CargarModulos();
            ListaModulos.SelectedIndex = indiceSeleccionado >= 0 ? indiceSeleccionado : 0;
        }

        private class IdiomaItem
        {
            public string Nombre { get; }
            public string Codigo { get; }

            public IdiomaItem(string nombre, string codigo)
            {
                Nombre = nombre;
                Codigo = codigo;
            }

            public override string ToString() => Nombre;
        }
    }
}
