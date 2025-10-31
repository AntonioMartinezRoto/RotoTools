using RotoEntities;
using System.Data;
using System.Xml.Linq;

namespace RotoTools
{
    public partial class ConfiguradorOpcionesMenu : Form
    {
        #region Constructor
        public ConfiguradorOpcionesMenu()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        private void ConfiguradorOpciones_Load(object sender, EventArgs e)
        {
            InitializeInfoConnection();
            //CargarTextos();
        }
        private void btn_ConfigOpciones_Click(object sender, EventArgs e)
        {
            ConfiguradorOpciones configuradorOpcionesForm = new ConfiguradorOpciones();
            configuradorOpcionesForm.ShowDialog();
        }
        private void btn_Restore_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "XML Files (*.xml)|*.xml";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string rutaXml = openFileDialog.FileName;
                    Helpers.RestoreOpcionesDesdeXml(rutaXml);
                    MessageBox.Show(LocalizationManager.GetString("L_ConfiguracionRestaurada"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationManager.GetString("L_ErrorRestaurandoConfiguracion") + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Private methods
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void CargarTextos()
        {
            lbl_ConfigOpciones.Text = LocalizationManager.GetString("L_ConfigurarGuardarOpciones");
            this.Text = LocalizationManager.GetString("L_MenuConfigurarOpciones");
            lbl_RestoreOptions.Text = LocalizationManager.GetString("L_RestaurarOpciones");
        }

        #endregion

    }
}
