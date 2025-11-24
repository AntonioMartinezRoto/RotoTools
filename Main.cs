using Microsoft.Win32;
using System.Reflection;

namespace RotoTools
{
    public partial class Main : Form
    {
        #region Properties


        #endregion

        #region Constructors

        public Main()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        private void Main_Load(object sender, EventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = $"RotoTools v{version.Major}.{version.Minor}";
            CargarTextos();
            CargarDatos();
        }
        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }
        private void btn_ConfigOpciones_Click(object sender, EventArgs e)
        {
            ConfiguradorOpcionesMenu configuradorOpcionesMenuForm = new ConfiguradorOpcionesMenu();
            configuradorOpcionesMenuForm.ShowDialog();
        }
        private void btn_Actualizador_Click(object sender, EventArgs e)
        {
            ActualizadorMenu actualizadorMenuForm = new ActualizadorMenu();
            actualizadorMenuForm.ShowDialog();
        }
        private void btn_Conector_Click(object sender, EventArgs e)
        {
            ConectorHerrajeMenu conectorHerrajeMenuForm = new ConectorHerrajeMenu();
            conectorHerrajeMenuForm.ShowDialog();
        }

        private void btn_Export_Click(object sender, EventArgs e)
        {
            ExportacionMenu exportacionMenuForm = new ExportacionMenu();
            exportacionMenuForm.ShowDialog();
        }
        private void btn_ControlCambios_Click(object sender, EventArgs e)
        {
            ControlCambiosMenu controlCambiosMenuForm = new ControlCambiosMenu();
            controlCambiosMenuForm.ShowDialog();
        }
        private void btn_Traduccion_Click(object sender, EventArgs e)
        {
            TraduccionMenu traduccionMenuForm = new TraduccionMenu();
            traduccionMenuForm.ShowDialog();
        }
        private void btn_Config_Click(object sender, EventArgs e)
        {
            OptionsMenu optionsMenuForm = new OptionsMenu();
            optionsMenuForm.ShowDialog();
            CargarTextos();
        }
        private void btn_ManillasFKS_Click(object sender, EventArgs e)
        {
            ManillasFKSMenu manillasFKSMenuForm = new ManillasFKSMenu();
            manillasFKSMenuForm.ShowDialog();
        }
        #endregion

        #region Private methods

        private void CargarDatos()
        {
            InitializeInfoConnection();
        }
        private void CargarTextos()
        {
            lbl_Conector.Text = LocalizationManager.GetString("L_ConectorHerraje");
            lbl_ConfigOpciones.Text = LocalizationManager.GetString("L_ConfiguradorOpciones");
            lbl_Export.Text = LocalizationManager.GetString("L_ExportarDatos");
            lbl_ControlCambios.Text = LocalizationManager.GetString("L_ControlCambios");
            lbl_Actualizacion.Text = LocalizationManager.GetString("L_Actualizador");
            lbl_Traduccion.Text = LocalizationManager.GetString("L_Traduccion");
            lbl_ConfigFKS.Text = LocalizationManager.GetString("L_ConfManillasFKS");
            this.Text = LocalizationManager.GetString("L_Menu");
        }
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        #endregion
    }
}
