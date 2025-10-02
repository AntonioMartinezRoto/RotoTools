using Microsoft.Data.SqlClient;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ConectorHerrajeGenerador : Form
    {
        #region PRIVATE PROPERTIES

        private List<Set> setsWorkingList = new List<Set>();
        private string supplierName = "";
        private List<SetGridRow> _allData;
        private DataTable _dataTable;
        private BindingSource _bindingSource;
        XmlData xmlOrigen = new XmlData();

        #endregion

        #region CONSTRUCTORS
        public ConectorHerrajeGenerador()
        {
            InitializeComponent();
        }
        public ConectorHerrajeGenerador(XmlData xmlOrigen, List<Set> setList, string supplierName)
        {
            InitializeComponent();
            this.setsWorkingList = setList;
            this.supplierName = supplierName;
            this.xmlOrigen = xmlOrigen;
        }
        #endregion

        #region EVENTS
        private void ConectorHerrajeGenerador_Load(object sender, EventArgs e)
        {
            List<Set> listaSets = setsWorkingList; // tu método para cargar Sets
            txt_ConectorName.Text = supplierName;

            CrearGrid();
            EstiloCabeceras();

            _allData = ConvertSetsToGrid(listaSets);
            CargarDatos(_allData);

            statusStrip1.BackColor = Color.Transparent;
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void txt_Filtro_TextChanged(object sender, EventArgs e)
        {
            string filtro = txt_Filtro.Text.Trim().Replace("'", "''"); // evitar errores por comillas
            if (string.IsNullOrEmpty(filtro))
            {
                _bindingSource.RemoveFilter();
            }
            else
            {
                _bindingSource.Filter = $"Codigo LIKE '%{filtro}%'";
            }
        }
        private void btn_GenerarConector_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Archivo XML (*.xml)|*.xml";
                saveFileDialog.Title = "Guardar archivo XML";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    XmlDocument conectorHerrajeGenerado = GenerateConnectorXml(xmlOrigen.Supplier);

                    conectorHerrajeGenerado.Save(saveFileDialog.FileName);
                    MessageBox.Show("Conector generadado correctamente.");
                }
            }
        }
        private void btn_InsertConector_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = @"INSERT INTO ConectorHerrajes (DataVerId, Codigo, XML) VALUES (dbo.GetCurrentDVID(), @Codigo, @Xml);";
                //EnableButtons(false);
                if (ExisteConectorEnBD(txt_ConectorName.Text))
                {
                    if (MessageBox.Show("Existe un conector con el nombre seleccionado ¿Quieres sobreescribirlo?", "Conector existente", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        sql = @"UPDATE ConectorHerrajes SET XML = @Xml Where Codigo = @Codigo;";
                    }
                    else
                    {
                        return;
                    }

                }
                XmlDocument xmlConector = GenerateConnectorXml(txt_ConectorName.Text);

                if (chk_Predefinido.Checked && !String.IsNullOrEmpty(txt_ConectorName.Text))
                {
                    sql += @"UPDATE VARIABLESGLOBALES SET VALOR = '" + txt_ConectorName.Text + "' WHERE NOMBRE = 'Conector Herraje';";
                }
                using (SqlConnection connection = new SqlConnection(Helpers.GetConnectionString()))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Codigo", txt_ConectorName.Text);
                    command.Parameters.AddWithValue("@Xml", xmlConector.OuterXml);
                    command.ExecuteNonQuery();
                }
                MessageBox.Show("Conector insertado correctamente.");

                //EnableButtons(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error insertando conector: " + ex.Message);
                //EnableButtons(true);
            }
        }

        #endregion

        #region PRIVATE METHODS
        private List<SetGridRow> ConvertSetsToGrid(List<Set> sets)
        {
            var result = new List<SetGridRow>();

            foreach (var set in sets)
            {
                Image img = Properties.Resources.Escandallo;

                if (set.Opening != null)
                {
                    switch (set.Opening.openingType)
                    {
                        case (int)enumOpeningType.PracticableIzquierdaInt:
                            img = Properties.Resources.Opening1;
                            break;
                        case (int)enumOpeningType.PracticableDerechaInt:
                            img = Properties.Resources.Opening2;
                            break;
                        case (int)enumOpeningType.OscilobatienteIzquierdaInt:
                            img = Properties.Resources.Opening3;
                            break;
                        case (int)enumOpeningType.OscilobatienteDerechaInt:
                            img = Properties.Resources.Opening4;
                            break;
                        case (int)enumOpeningType.CorrederaDerecha:
                            img = Properties.Resources.Opening5;
                            break;
                        case (int)enumOpeningType.CorrederaIzquierda:
                            img = Properties.Resources.Opening6;
                            break;
                        case (int)enumOpeningType.CorrederaIzqDcha:
                            img = Properties.Resources.Opening7;
                            break;
                        case (int)enumOpeningType.Abatible:
                            img = Properties.Resources.Opening8;
                            break;
                        case (int)enumOpeningType.OsciloCorrederaDerecha:
                            img = Properties.Resources.Opening9;
                            break;
                        case (int)enumOpeningType.OsciloCorrederaIzquierda:
                            img = Properties.Resources.Opening10;
                            break;
                        case (int)enumOpeningType.ElevableIzquierda:
                            img = Properties.Resources.Opening11;
                            break;
                        case (int)enumOpeningType.ElevableDerecha:
                            img = Properties.Resources.Opening12;
                            break;
                        case (int)enumOpeningType.PracticableIzquierdaExt:
                            img = Properties.Resources.Opening13;
                            break;
                        case (int)enumOpeningType.PracticableDerechaExt:
                            img = Properties.Resources.Opening14;
                            break;
                        default:
                            img = Properties.Resources.Escandallo;
                            break;
                    }
                }

                // 2. Texto de opciones
                string options = "";
                if (set.OptionConectorList?.Any() == true)
                {
                    options = string.Join(Environment.NewLine,
                                            set.OptionConectorList.Select(o => @$"{o.Name}\{o.Value}"));
                }

                // 3. Crear fila
                result.Add(new SetGridRow
                {
                    Apertura = img,
                    Opciones = options,
                    Codigo = set.Code,
                    Escandallo = set.Script
                });
            }

            return result;
        }
        private XmlDocument GenerateConnectorXml(string conectorCode)
        {
            XmlDocument doc = new XmlDocument();

            // Crear nodo raíz <Connector>
            XmlElement connectorNode = doc.CreateElement("Connector");
            connectorNode.SetAttribute("Connector_code", conectorCode);
            connectorNode.SetAttribute("Message", "true");
            doc.AppendChild(connectorNode);

            foreach (Set set in setsWorkingList)
            {
                if (set.OpeningFlagConectorList == null) continue;

                XmlElement nodeElement = doc.CreateElement("Node");

                if (!String.IsNullOrEmpty(set.Script))
                {
                    nodeElement.SetAttribute("Script", set.Script);
                    XmlElement openingElementScript = doc.CreateElement("Opening");
                    nodeElement.AppendChild(openingElementScript);
                    connectorNode.AppendChild(nodeElement);

                    continue;
                }

                nodeElement.SetAttribute("Fitting_Code", set.Code);

                // Openings
                XmlElement openingElement = doc.CreateElement("Opening");

                foreach (Option openingFlag in set.OpeningFlagConectorList)
                {
                    XmlElement flag = doc.CreateElement("Opening_Flag");
                    flag.SetAttribute("Value", openingFlag.Value);
                    openingElement.AppendChild(flag);
                }

                nodeElement.AppendChild(openingElement);

                // Opciones
                XmlElement includedOptions = doc.CreateElement("Included_Options");
                XmlElement optionsNode = doc.CreateElement("Options");

                foreach (Option option in set.OptionConectorList)
                {
                    XmlElement optionElement = doc.CreateElement("Option");
                    optionElement.SetAttribute("Name", option.Name);
                    optionElement.SetAttribute("Value", option.Value);
                    optionsNode.AppendChild(optionElement);
                }

                includedOptions.AppendChild(optionsNode);
                nodeElement.AppendChild(includedOptions);

                connectorNode.AppendChild(nodeElement);
            }

            return doc;
        }
        private bool ExisteConectorEnBD(string conectorName)
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM ConectorHerrajes WHERE Codigo = '" + conectorName + "'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }

        private void CrearGrid()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.RowTemplate.Height = 50;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Definir DataTable con columnas
            _dataTable = new DataTable();
            _dataTable.Columns.Add("Escandallo", typeof(string));
            _dataTable.Columns.Add("Apertura", typeof(Image));
            _dataTable.Columns.Add("Opciones", typeof(string));
            _dataTable.Columns.Add("Codigo", typeof(string));

            // Crear BindingSource y asignarle la tabla
            _bindingSource = new BindingSource();
            _bindingSource.DataSource = _dataTable;

            // Conectar la grilla al BindingSource (no directamente al DataTable)
            dataGridView1.DataSource = _bindingSource;

            // Configuración de columnas (como ya lo tenías)
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Escandallo",
                DataPropertyName = "Escandallo",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });

            dataGridView1.Columns.Add(new DataGridViewImageColumn
            {
                HeaderText = "Apertura",
                DataPropertyName = "Apertura",
                ReadOnly = true,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Opciones",
                DataPropertyName = "Opciones",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Codigo",
                DataPropertyName = "Codigo",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });
        }
        private void CargarDatos(List<SetGridRow> listaSets)
        {
            _dataTable.Rows.Clear();

            foreach (var set in listaSets)
            {
                _dataTable.Rows.Add(set.Escandallo, set.Apertura, set.Opciones, set.Codigo);
            }
        }
        private void EstiloCabeceras()
        {
            dataGridView1.EnableHeadersVisualStyles = false; // muy importante, evita que Windows sobrescriba el estilo
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Opcional: bordes y selección
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.GridColor = Color.LightGray;
        }

        #endregion

    }
    public class SetGridRow
    {
        public string Escandallo { get; set; }
        public Image? Apertura { get; set; }
        public string Opciones { get; set; }
        public string Codigo { get; set; }
    }
}
