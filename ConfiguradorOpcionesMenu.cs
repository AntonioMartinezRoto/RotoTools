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
        }
        private void btn_ConfigOpciones_Click(object sender, EventArgs e)
        {
            ConfiguradorOpciones configuradorOpcionesForm = new ConfiguradorOpciones();
            configuradorOpcionesForm.ShowDialog();
        }
        private void btn_Restore_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML configuración";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string rutaXml = openFileDialog.FileName;
                Helpers.RestoreOpcionesDesdeXml(rutaXml);
            }
        }

        #endregion

        #region Private methods
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }        

        #endregion

    }
}
