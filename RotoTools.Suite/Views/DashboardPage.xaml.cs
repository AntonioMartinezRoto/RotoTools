using System.Windows;
using System.Windows.Controls;
using RotoTools.Suite.Services;
using RotoTools.Suite.Views.Actualizador;
using RotoTools.Suite.Views.Cam;
using RotoTools.Suite.Views.ConectorHerraje;
using RotoTools.Suite.Views.ConfiguradorOpciones;
using RotoTools.Suite.Views.TariffImporter;

namespace RotoTools.Suite.Views
{
    /// <summary>
    /// Página de inicio de la suite (nueva: la app WinForms original no tenía una pantalla de
    /// bienvenida propiamente dicha, era directamente el menú). Rediseñada a petición del usuario
    /// para parecerse a la portada de RotoGestionClientes: logo + "Bienvenido a RotoTools" y un
    /// dashboard con accesos directos REALMENTE clicables a la acción más habitual de 5 módulos
    /// (antes había una cuadrícula con los 11 módulos que solo se veía, sin reaccionar al clic, lo
    /// que confundía, y una frase de introducción que también se quitó a petición del usuario).
    /// Cada acceso directo reutiliza tal cual el método público del Click del propio botón de
    /// destino (ver comentario de cada uno más abajo), así que no hay ninguna lógica de negocio
    /// duplicada aquí: esta página solo sabe navegar y disparar el evento correspondiente.
    /// </summary>
    public partial class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            InitializeComponent();
            CargarTextos();
        }

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtBienvenida.Text = Loc("L_Suite_Bienvenido");
            TxtSubtitulo.Text = "Roto Frank FTT GmbH";
            TxtAccesosTitulo.Text = Loc("L_Suite_AccesosDirectosTitulo");

            TxtAccesoCamTitulo.Text = Loc("L_Suite_AccesoCargarXml");
            TxtAccesoCamModulo.Text = "CAM · Mecanizados";

            TxtAccesoConfigOpcionesTitulo.Text = RotoTools.LocalizationManager.GetString("L_RestaurarOpciones");
            TxtAccesoConfigOpcionesModulo.Text = RotoTools.LocalizationManager.GetString("L_ConfiguradorOpciones");

            TxtAccesoActualizadorTitulo.Text = RotoTools.LocalizationManager.GetString("L_InstalarEscandallos");
            TxtAccesoActualizadorModulo.Text = RotoTools.LocalizationManager.GetString("L_Actualizador");

            TxtAccesoConectorHerrajeTitulo.Text = Loc("L_Suite_CambiarConectorActivo");
            TxtAccesoConectorHerrajeModulo.Text = RotoTools.LocalizationManager.GetString("L_ConectorHerraje");

            TxtAccesoTariffImporterTitulo.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarArchivo");
            TxtAccesoTariffImporterModulo.Text = RotoTools.LocalizationManager.GetString("L_TariffImporter");
        }

        /// <summary>Navega al módulo TPagina (ver MainWindow.IrAModulo) y, si la ventana actual es
        /// de verdad una MainWindow (siempre lo es en ejecución normal: DashboardPage solo vive
        /// dentro de ContentHost), ejecuta la acción del acceso directo sobre la página recién
        /// creada.</summary>
        private void IrAModulo<TPagina>(System.Action<TPagina>? accion) where TPagina : UserControl
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.IrAModulo(accion);
        }

        /// <summary>Acceso directo a "Cargar XML" del módulo CAM: reutiliza tal cual
        /// CamPage.BtnLoadXml_Click (ahora público).</summary>
        private void BtnAccesoCam_Click(object sender, RoutedEventArgs e) =>
            IrAModulo<CamPage>(pagina => pagina.BtnLoadXml_Click(pagina, new RoutedEventArgs()));

        /// <summary>Acceso directo a "Restaurar opciones" del Configurador de opciones: reutiliza
        /// tal cual ConfiguradorOpcionesPage.BtnRestore_Click (ahora público).</summary>
        private void BtnAccesoConfigOpciones_Click(object sender, RoutedEventArgs e) =>
            IrAModulo<ConfiguradorOpcionesPage>(pagina => pagina.BtnRestore_Click(pagina, new RoutedEventArgs()));

        /// <summary>Acceso directo a "Instalar Escandallos" del módulo Instalación: reutiliza tal
        /// cual ActualizadorPage.BtnInstalarEscandallos_Click (ahora público).</summary>
        private void BtnAccesoActualizador_Click(object sender, RoutedEventArgs e) =>
            IrAModulo<ActualizadorPage>(pagina => pagina.BtnInstalarEscandallos_Click(pagina, new RoutedEventArgs()));

        /// <summary>Acceso directo a "Establecer conector activo" del módulo Conector de herraje:
        /// reutiliza tal cual ConectorHerrajePage.BtnCambiarConectorActivo_Click (ahora público).
        /// A diferencia de los otros 4 accesos, este módulo puede estar bloqueado (base de datos
        /// con versión anterior a 2020, ver CargarDatos/BorderAvisoVersion en ConectorHerrajePage):
        /// si BtnCambiarConectorActivo está deshabilitado por ese motivo, el acceso directo se
        /// limita a navegar al módulo -- que ya muestra su propio aviso -- en vez de forzar la
        /// apertura del diálogo.</summary>
        private void BtnAccesoConectorHerraje_Click(object sender, RoutedEventArgs e) =>
            IrAModulo<ConectorHerrajePage>(pagina =>
            {
                if (pagina.ModuloDesbloqueado)
                    pagina.BtnCambiarConectorActivo_Click(pagina, new RoutedEventArgs());
            });

        /// <summary>Acceso directo a "Seleccionar fichero de precios" del módulo Cargar precios:
        /// reutiliza tal cual TariffImporterPage.BtnLoadTariff_Click (ahora público).</summary>
        private void BtnAccesoTariffImporter_Click(object sender, RoutedEventArgs e) =>
            IrAModulo<TariffImporterPage>(pagina => pagina.BtnLoadTariff_Click(pagina, new RoutedEventArgs()));
    }
}
