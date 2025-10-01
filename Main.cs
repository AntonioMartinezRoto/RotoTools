using Microsoft.Win32;
using System.Reflection;

namespace RotoTools
{
    public partial class Main : Form
    {
        #region Properties

        private string ConnectionString { get; set; }

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
        #endregion

        #region Private methods

        private void CargarDatos()
        {
            ConnectionString = Helpers.GetConnectionString();
            InitializeInfoConnection();
            if (Helpers.IsVersionPrefSuiteCompatible(ConnectionString))
            {
                //EnableButtons(true);
                //LoadItemsConectorHerraje();
            }
            else
            {
                //EnableButtons(false);
                //ShowVersionNoCompatible();
            }
        }
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        #endregion


    }
}
