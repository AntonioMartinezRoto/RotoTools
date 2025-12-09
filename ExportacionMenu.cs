using MathNet.Numerics;
using NPOI.SS.Formula.Functions;
using RotoEntities;
using System.Xml;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ExportacionMenu : Form
    {
        #region PRIVATE PROPERTIES

        private XmlNamespaceManager nsmgr;
        private XmlData xmlFile = new XmlData();
        private bool xmlLoadedFile = false;

        #endregion

        #region CONSTRUCTOR
        public ExportacionMenu()
        {
            InitializeComponent();
        }

        #endregion

        #region EVENTS
        private void ExportacionMenu_Load(object sender, EventArgs e)
        {
            CargarTextos();
        }
        private void btn_Xml_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                EnableButtons(false);
                string xmlPath = openFileDialog.FileName;
                xmlFile = LoadXml(xmlPath);
                lbl_info.Text = xmlPath;
                EnableButtons(true);
                xmlLoadedFile = true;
            }
        }
        private void btn_ExportWinPerfil_Click(object sender, EventArgs e)
        {
            if (xmlLoadedFile)
            {
                ExportacionWinPerfil winPerfilExportForm = new ExportacionWinPerfil(xmlFile);
                winPerfilExportForm.ShowDialog();
            }
        }
        private void btn_ExportOrgadata_Click(object sender, EventArgs e)
        {
            if (xmlLoadedFile)
            {
                ExportacionOrgadata orgadataExportForm = new ExportacionOrgadata(xmlFile);
                orgadataExportForm.ShowDialog();
            }
        }
        private void btn_ExportOpera_Click(object sender, EventArgs e)
        {
            if (xmlLoadedFile)
            {
                ExportacionOpera operaForm = new ExportacionOpera(xmlFile);
                operaForm.ShowDialog();
            }
        }
        private void btn_Sql_Click(object sender, EventArgs e)
        {
            string connectionString = @"Server=RFSPNB13\SQLEXPRESS;Database=RotoBD;Integrated Security=True;MultipleActiveResultSets = True;Encrypt=False;TrustServerCertificate=True;";

            using (var db = new MyDbContext(connectionString))
            {
                db.Database.EnsureCreated(); // CREA la BD y las tablas si no existen
            }

            MessageBox.Show("Base de datos creada correctamente");

            //InsertDataInDB();

            //MessageBox.Show("Datos insertados correctamente");
        }

        #endregion

        #region METHODS

        private XmlData LoadXml(string xmlPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);

                nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

                XmlLoader loader = new XmlLoader(nsmgr);
                // Vinculamos el evento para actualizar la Label del formulario
                loader.OnLoadingInfo += (type, value) =>
                {
                    lbl_info.Visible = true;
                    lbl_info.Text = LocalizationManager.GetString("L_Cargando") + $"... {type} {value.TrimEnd()}";
                    Application.DoEvents(); // refresca la UI
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
        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_ExportarDatos");
            lbl_info.Text = LocalizationManager.GetString("L_SeleccionarXML");
            lbl_ExportWinPerfil.Text = LocalizationManager.GetString("L_ExportarWinPerfil");
            lbl_ExportOrgadata.Text = LocalizationManager.GetString("L_ExportarOrgadata");
            lbl_ExportOpera.Text = LocalizationManager.GetString("L_ExportarOpera");
        }
        private void EnableButtons(bool enable)
        {
            btn_ExportWinPerfil.Enabled = enable;
            btn_ExportOrgadata.Enabled = enable;
            btn_ExportOpera.Enabled = enable;
            btn_Sql.Enabled = enable;
        }
        private void InsertDataInDB()
        {
                
                //db.Sets.AddRange(xmlFile.SetList);
                //db.Colours.AddRange(xmlFile.ColourList);
                //db.Options.AddRange(xmlFile.OptionList);
            string connectionString = @"Server=RFSPNB13\SQLEXPRESS;Database=RotoBD;Integrated Security=True;MultipleActiveResultSets = True;Encrypt=False;TrustServerCertificate=True;";

            using (var db = new MyDbContext(connectionString))
            {
                db.FittingGroups.AddRange(xmlFile.FittingGroupList);
                db.Fittings.AddRange(xmlFile.FittingList);
                db.SaveChanges();
            }
        }

        #endregion



    }
}
