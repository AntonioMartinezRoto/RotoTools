using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views
{
    /// <summary>
    /// Página de inicio de la suite (nueva: la app WinForms original no tenía una pantalla de
    /// bienvenida propiamente dicha, era directamente el menú). Ahora que los 11 módulos del
    /// menú principal ya están migrados, sirve como portada de resumen: nombre + marca Roto,
    /// una frase breve, y una cuadrícula no interactiva con los módulos disponibles (ver
    /// CargarModulos). Ya no explica ninguna "fase de migración" porque no queda ninguna
    /// pendiente.
    /// </summary>
    public partial class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            InitializeComponent();
            CargarTextos();
            CargarModulos();
        }

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtBienvenida.Text = "RotoTools Suite";
            TxtSubtitulo.Text = "Roto Frank FTT GmbH";
            TxtDescripcion.Text = Loc("L_Suite_DashboardTagline");
            TxtModulosTitulo.Text = Loc("L_Suite_DashboardModulosTitulo");
        }

        /// <summary>
        /// Mismo inventario de iconos/colores por módulo que MainWindow.xaml.cs (CargarModulos),
        /// para que la cuadrícula de esta portada coincida visualmente con la barra lateral, sin
        /// "Inicio" (ya estamos en él) ni CrearPagina (aquí no se navega, ver comentario del
        /// XAML). NavModuleItem se reutiliza tal cual porque DashboardPage vive en el mismo
        /// espacio de nombres (RotoTools.Suite.Views) que la clase donde se define.
        /// </summary>
        private void CargarModulos()
        {
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

            var modulos = new List<NavModuleItem>
            {
                new() { Titulo = "CAM · Mecanizados", Icono = iconoCam, Color = new SolidColorBrush(Color.FromRgb(0x2E,0x7D,0x32)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ConfiguradorOpciones"), Icono = iconoConfigOpciones, Color = new SolidColorBrush(Color.FromRgb(0x6A,0x5A,0xCD)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_Actualizador"), Icono = iconoActualizador, Color = new SolidColorBrush(Color.FromRgb(0x19,0x76,0xD2)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ExportarDatos"), Icono = iconoExportar, Color = new SolidColorBrush(Color.FromRgb(0xF5,0x7C,0x00)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ConectorHerraje"), Icono = iconoConector, Color = new SolidColorBrush(Color.FromRgb(0x00,0x83,0x8F)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ControlCambios"), Icono = iconoControlCambios, Color = new SolidColorBrush(Color.FromRgb(0x8E,0x24,0xAA)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_Traduccion"), Icono = iconoTraduccion, Color = new SolidColorBrush(Color.FromRgb(0xC2,0x18,0x5B)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_ConfManillasFKS"), Icono = iconoManillas, Color = new SolidColorBrush(Color.FromRgb(0x5D,0x40,0x37)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_TariffImporter"), Icono = iconoTarifas, Color = new SolidColorBrush(Color.FromRgb(0x00,0x97,0xA7)) },

                new() { Titulo = RotoTools.LocalizationManager.GetString("L_Opciones"), Icono = iconoAjustes, Color = new SolidColorBrush(Color.FromRgb(0x45,0x5A,0x64)) },
            };

            ListaModulosInicio.ItemsSource = modulos;
        }
    }
}
