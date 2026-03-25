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
        private bool necesarioInsertarOpcionTipoCorredera = false;

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
            CargarTextos();

            CheckOpcionTipoCorredera();

            statusStrip1.BackColor = Color.Transparent;
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
            lbl_ConectorActivo.Text = Helpers.GetConectorActivo();
        }
        private void txt_Filtro_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
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
                    MessageBox.Show(LocalizationManager.GetString("L_ConectorGenerado"));
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
                    if (MessageBox.Show(LocalizationManager.GetString("L_ExisteConector"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

                //Si están los sets de corredera e Inowa deben distinguirse con la opción RO_TIPO_CORREDERA y hay que instalarla
                if (necesarioInsertarOpcionTipoCorredera)
                {
                    Helpers.InstalarOpcionTipoCorredera();
                }

                MessageBox.Show(LocalizationManager.GetString("L_ConectorInsertado"));

                //EnableButtons(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (7): " + ex.Message);
                //EnableButtons(true);
            }
        }
        private void chk_Ventanas_CheckedChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }
        private void chk_Puertas_CheckedChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }
        private void chk_Balconeras_CheckedChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }
        private void chk_Correderas_CheckedChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }
        private void chk_Elevables_CheckedChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }
        private void chk_Paralelas_CheckedChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }
        private void chk_Abatibles_CheckedChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }
        private void chk_Plegables_CheckedChanged(object sender, EventArgs e)
        {
            AplicarFiltros();
        }

        #endregion

        #region PRIVATE METHODS
        private void AplicarFiltros()
        {
            var filtros = new List<string>();

            // Filtro por texto
            string texto = txt_Filtro.Text.Trim().Replace("'", "''");

            if (!string.IsNullOrEmpty(texto))
            {
                filtros.Add($"{LocalizationManager.GetString("L_Codigo")} LIKE '%{texto}%'");
            }

            // Filtro por WindowType (checkboxes)
            var windowTypesSeleccionados = new List<int>();

            if (chk_Ventanas.Checked)
                windowTypesSeleccionados.Add((int)enumWindowType.Ventana);
            if (chk_Puertas.Checked)
                windowTypesSeleccionados.Add((int)enumWindowType.Puerta);
            if (chk_Balconeras.Checked)
                windowTypesSeleccionados.Add((int)enumWindowType.Balconera);
            if (chk_Elevables.Checked)
                windowTypesSeleccionados.Add((int)enumWindowType.Elevable);
            if (chk_Correderas.Checked)
                windowTypesSeleccionados.Add((int)enumWindowType.Corredera);
            if (chk_Paralelas.Checked)
                windowTypesSeleccionados.Add((int)enumWindowType.Osciloparalela);
            if (chk_Abatibles.Checked)
                windowTypesSeleccionados.Add((int)enumWindowType.Abatible);
            if (chk_Plegables.Checked)
                windowTypesSeleccionados.Add((int)enumWindowType.Plegable);

            if (windowTypesSeleccionados.Any())
            {
                string filtroWindowType = string.Join(" OR ",
                    windowTypesSeleccionados.Select(w => $"WindowType = {w}")
                );

                filtros.Add($"({filtroWindowType})");
            }

            //Aplicar filtro final
            _bindingSource.Filter = filtros.Any()
                ? string.Join(" AND ", filtros)
                : string.Empty;
        }
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
                    Escandallo = set.Script,
                    WindowType = set.WindowType,
                    Selected = false
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
                if (!set.IsTitle && set.OpeningFlagConectorList == null) continue;

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
            _dataTable.Columns.Add("", typeof(bool));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Escandallo"), typeof(string));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Apertura"), typeof(Image));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Opciones"), typeof(string));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Codigo"), typeof(string));
            _dataTable.Columns.Add("WindowType", typeof(int));

            // Crear BindingSource y asignarle la tabla
            _bindingSource = new BindingSource();
            _bindingSource.DataSource = _dataTable;

            // Conectar la grilla al BindingSource (no directamente al DataTable)
            dataGridView1.DataSource = _bindingSource;

            dataGridView1.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Selected",
                HeaderText = "",
                Width = 30
            });

            // Configuración de columnas (como ya lo tenías)
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Escandallo"),
                DataPropertyName = LocalizationManager.GetString("L_Escandallo"),
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });

            dataGridView1.Columns.Add(new DataGridViewImageColumn
            {
                HeaderText = LocalizationManager.GetString("L_Apertura"),
                DataPropertyName = LocalizationManager.GetString("L_Apertura"),
                ReadOnly = true,
                //ImageLayout = DataGridViewImageCellLayout.Zoom,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Opciones"),
                DataPropertyName = LocalizationManager.GetString("L_Opciones"),
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Codigo"),
                DataPropertyName = LocalizationManager.GetString("L_Codigo"),
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "WindowType",
                DataPropertyName = "WindowType",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { WrapMode = DataGridViewTriState.True },
                Visible = false
            });
        }
        private void CargarDatos(List<SetGridRow> listaSets)
        {
            _dataTable.Rows.Clear();

            foreach (var set in listaSets)
            {
                _dataTable.Rows.Add(set.Selected, set.Escandallo, set.Apertura, set.Opciones, set.Codigo, set.WindowType);
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
        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_GenerarConector");
            lbl_SaveXML.Text = LocalizationManager.GetString("L_GuardarEnXML");
            lbl_SaveBD.Text = LocalizationManager.GetString("L_GuardarEnBD");
            chk_Predefinido.Text = LocalizationManager.GetString("L_PonerPredefinido");
            lbl_Filtro.Text = LocalizationManager.GetString("L_Buscar");
            btn_GenerarConector.Text = LocalizationManager.GetString("L_XML");
            btn_InsertConector.Text = LocalizationManager.GetString("L_BBDD");
            groupSaveConector.Text = LocalizationManager.GetString("L_Guardar");
            group_Buscar.Text = LocalizationManager.GetString("L_Buscar");

            chk_Ventanas.Text = LocalizationManager.GetString("L_Ventanas");
            chk_Balconeras.Text = LocalizationManager.GetString("L_Balconeras");
            chk_Puertas.Text = LocalizationManager.GetString("L_Puertas");
            chk_Correderas.Text = LocalizationManager.GetString("L_Correderas");
            chk_Elevables.Text = LocalizationManager.GetString("L_Elevables");
            chk_Paralelas.Text = LocalizationManager.GetString("L_Paralelas");
            chk_Plegables.Text = LocalizationManager.GetString("L_Plegables");
            chk_Abatibles.Text = LocalizationManager.GetString("L_Abatibles");
        }
        private void CheckOpcionTipoCorredera()
        {
            bool existenSetsCorredera = this.setsWorkingList.Where(x => !String.IsNullOrEmpty(x.Code)).Any(s => s.Code.ToUpper().Contains("CORREDERA"));
            bool existenSetsInowa = this.setsWorkingList.Where(x => !String.IsNullOrEmpty(x.Code)).Any(s => s.Code.ToUpper().Contains("INOWA"));

            if (existenSetsCorredera && existenSetsInowa)
            {
                necesarioInsertarOpcionTipoCorredera = true;
            }
        }
        #endregion


    }
    public class SetGridRow
    {
        public string Escandallo { get; set; }
        public Image? Apertura { get; set; }
        public string Opciones { get; set; }
        public string Codigo { get; set; }
        public bool Selected { get; set; }
        public int WindowType { get; set; }
    }
}
