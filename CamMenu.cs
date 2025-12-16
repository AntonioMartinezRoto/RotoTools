using Microsoft.Data.SqlClient;
using RotoEntities;
using System.Data;
using System.Text.Json;
using System.Xml;

namespace RotoTools
{
    public partial class CamMenu : Form
    {
        #region Private Properties

        private XmlData xmlOrigen = new();
        private bool xmlCargado = false;
        private XmlNamespaceManager nsmgr;
        private List<Operation> operationsXmlList = new List<Operation>();
        private List<OperationInstalarGridITem> _allOperations = new List<OperationInstalarGridITem>();
        private DataTable _dataTable;
        private BindingSource _bindingSource = new BindingSource();
        private BindingSource _bindingDetalleOperaciones;
        private List<OperationGridRow> _allData;
        private Dictionary<string, bool> _cacheExisteBD = new();

        #endregion

        #region Constructors
        public CamMenu()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        private void CamMenu_Load(object sender, EventArgs e)
        {
            CargarTextos();
            rb_All.Checked = true;
            CrearGridDetalleOperaciones();
            CrearGridInstalarOperaciones();
            DarEstiloCabecerasDetalleOperaciones();

            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();

        }
        private void btn_LoadXml_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                CleanInfo();
                string rutaXml = openFileDialog.FileName;
                EnableButtons(false);
                xmlOrigen = LoadXml(rutaXml);
                lbl_Xml.Text = rutaXml;
                LoadSets("");
                EnableButtons(true);
            }
        }
        private void btn_InstalarMacros_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                EnableControls(false);
                Helpers.InstallMacrosMechanizedOperations();
                Helpers.InstallMacrosOperationsShapes();
                Cursor = Cursors.Default;
                EnableControls(true);

                MessageBox.Show(LocalizationManager.GetString("L_MacrosInstaladas"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(33)" + Environment.NewLine + Environment.NewLine +
                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor = Cursors.Default;
                EnableControls(true);
            }

        }
        private void btn_ClearOperations_Click(object sender, EventArgs e)
        {
            CleanInfo();
        }
        private void btn_ExportMacros_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string savePath = dialog.SelectedPath;

                        Cursor.Current = Cursors.WaitCursor;
                        EnableControls(false);

                        int exportedMacrosCount = ExportMacrosMechanizedOperations(Path.Combine(savePath, "MechanizedOperations"));
                        ExportMacrosOperationsShapes(Path.Combine(savePath, "OperationsShapes"));
                        MessageBox.Show(exportedMacrosCount.ToString() + " " + LocalizationManager.GetString("L_Operaciones") + ": " + Environment.NewLine + savePath, "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        EnableControls(true);
                        Cursor.Current = Cursors.Default;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(30)" + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls(true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void btn_CargarOperations_Click(object sender, EventArgs e)
        {
            if (this.xmlCargado)
            {
                CargarListaOperacionesFromXml();
                CargarGridInstalarOperaciones();
                CargarDatosGridDetalle(this._allData);
            }
        }
        private void txt_filter_TextChanged(object sender, EventArgs e)
        {
            chk_All.Checked = false;
            chkList_Sets.Items.Clear();
            LoadSets(txt_filter.Text);
        }
        private void chk_All_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < chkList_Sets.Items.Count; i++)
            {
                chkList_Sets.SetItemChecked(i, chk_All.Checked);
            }
        }
        private void chk_AllOperations_CheckedChanged(object sender, EventArgs e)
        {
            bool seleccionar = chk_AllOperations.Checked;

            foreach (OperationInstalarGridITem item in this._bindingSource.List)
            {
                item.Selected = seleccionar;
            }

            dataGridView2.Refresh();
        }
        private void rb_All_CheckedChanged(object sender, EventArgs e)
        {
            chk_AllOperations.Checked = false;
            AplicarFiltros();
        }
        private void rb_NoExists_CheckedChanged(object sender, EventArgs e)
        {
            chk_AllOperations.Checked = false;
            AplicarFiltros();
        }
        private void txt_FilterOperations_TextChanged(object sender, EventArgs e)
        {
            chk_AllOperations.Checked = false;
            AplicarFiltros();
        }
        private void btn_InstallOperation_Click(object sender, EventArgs e)
        {
            foreach (OperationInstalarGridITem item in this._bindingSource.List)
            {
                if (item.Selected && !ExisteEnBD("RO_" + item.OperationName))
                {
                    MechanizedOperation mechanizedOperation = new MechanizedOperation("RO_" + item.OperationName);
                    Helpers.InstallMechanizedOperation(mechanizedOperation);

                    foreach (OperationsShapes operationShape in item.OperationShapeList)
                    {
                        Helpers.InstallOperationShape(operationShape);
                    }
                }
            }

            MessageBox.Show(LocalizationManager.GetString("L_OperacionesInstaladas"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _cacheExisteBD = new();
            AplicarFiltros();
        }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0)
                return;

            if (dataGridView2.Columns[e.ColumnIndex].Name == "ConfigureShapes")
            {
                var item = (OperationInstalarGridITem)dataGridView2.Rows[e.RowIndex].DataBoundItem;

                using var frm = new CamConfigurarGeometria("RO_" + item.OperationName, item.OperationShapeList);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    item.OperationShapeList = frm.ResultOperationsShapesList;
                }
            }
        }
        #endregion

        #region Private methods
        private void CargarTextos()
        {
            lbl_Xml.Text = LocalizationManager.GetString("L_SeleccionarXML");

            chk_All.Text = LocalizationManager.GetString("L_SeleccionarTodos");
            chk_AllOperations.Text = LocalizationManager.GetString("L_SeleccionarTodos");
            lbl_Busqueda.Text = LocalizationManager.GetString("L_Buscar");
            lbl_BusquedaOp.Text = LocalizationManager.GetString("L_Buscar");
            group_Operaciones.Text = LocalizationManager.GetString("L_Operaciones");

            rb_All.Text = LocalizationManager.GetString("L_Todas");
            rb_NoExists.Text = LocalizationManager.GetString("L_NoExiste");
        }
        private void CargarListaOperacionesFromXml()
        {
            List<Operation> operationList = new List<Operation>();
            List<OperationGridRow> operationGridRowList = new List<OperationGridRow>();

            foreach (var itemListChecked in chkList_Sets.CheckedItems)
            {
                Set set = itemListChecked as Set;
                foreach (SetDescription setDescription in set.SetDescriptionList)
                {
                    foreach (Operation operation in setDescription.Fitting?.OperationList)
                    {
                        if (!operationList.Any(o => o.Name == operation.Name) && !operation.Name.ToUpper().Contains("SCREW"))
                        {
                            operationList.Add(operation);
                            OperationGridRow operationGridRow = new OperationGridRow(operation.Name,
                                setDescription.Fitting?.Id.ToString(),
                                setDescription.Fitting?.Ref.ToString(),
                                setDescription.Fitting?.Description,
                                operation.XPosition,
                                operation.Location);

                            operationGridRowList.Add(operationGridRow);
                        }
                    }
                }
            }

            this.operationsXmlList = operationList;
            this._allData = operationGridRowList.OrderBy(o => o.Operation).ToList();
        }
        private void CargarGridInstalarOperaciones()
        {
            this._allOperations = this.operationsXmlList.OrderBy(op => op.Name).Select(o => new OperationInstalarGridITem
            {
                Selected = false,
                OperationName = o.Name,
                OperationShapeList = new List<OperationsShapes>()
            }).ToList();

            _bindingSource.DataSource = _allOperations;
            dataGridView2.DataSource = _bindingSource;
        }
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
                    lbl_Xml.Visible = true;
                    lbl_Xml.Text = LocalizationManager.GetString("L_Cargando") + $"... {type} {value.TrimEnd()}";
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

                xmlCargado = true;
                return xmlData;
            }
            catch
            {
                return null;
            }

        }
        private void AplicarFiltros()
        {
            IEnumerable<OperationInstalarGridITem> query = _allOperations;

            // 1️⃣ Filtro por texto
            string filtroTexto = txt_FilterOperations.Text.Trim();

            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                query = query.Where(o =>
                    o.OperationName.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase));
            }

            // 2️⃣ Filtro por RadioButton
            if (rb_NoExists.Checked)
            {
                query = query.Where(o => !ExisteEnBD("RO_" + o.OperationName));
            }
            // rb_Todas.Checked => no se filtra nada

            // 3️⃣ Aplicar resultado
            _bindingSource.DataSource = query.ToList();
        }
        private int ExportMacrosMechanizedOperations(string savePath)
        {
            try
            {
                List<MechanizedOperation> mechanizedOperationsList = new List<MechanizedOperation>();

                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT * FROM MechanizedOperations WHERE OperationName LIKE 'RO_M%' AND IsPrimitive = 1 ORDER BY OperationName";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MechanizedOperation mechanizedOperation = new MechanizedOperation(reader["OperationName"].ToString().Trim(),
                                reader["Description"].ToString(),
                                Convert.ToInt16(reader["External"]),
                                Convert.ToInt16(reader["IsPrimitive"]),
                                reader["Level1"].ToString(),
                                reader["Level2"].ToString(),
                                reader["Level3"].ToString(),
                                reader["Level4"].ToString(),
                                reader["Level5"].ToString(),
                                Convert.ToInt32(reader["RGB"]),
                                Convert.ToBoolean(reader["Disable"]));

                            mechanizedOperationsList.Add(mechanizedOperation);
                        }
                    }
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                if (!System.IO.Directory.Exists(savePath))
                {
                    System.IO.Directory.CreateDirectory(savePath);
                }

                foreach (var mechanizedOperation in mechanizedOperationsList)
                {
                    string fileName = $"{mechanizedOperation.OperationName.Trim()}.json";
                    string path = Path.Combine(savePath, fileName);
                    File.WriteAllText(path, JsonSerializer.Serialize(mechanizedOperation, options));
                }

                return mechanizedOperationsList.Count();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(31)" + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }
        private void ExportMacrosOperationsShapes(string savePath)
        {
            try
            {
                List<OperationsShapes> operationsShapesList = new List<OperationsShapes>();

                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT OperationsShapes.* FROM OperationsShapes
                                    INNER JOIN MechanizedOperations ON MechanizedOperations.OperationName = OperationsShapes.OperationName
                                    WHERE OperationsShapes.OperationName like 'RO_M%' AND MechanizedOperations.IsPrimitive = 1 
                                    ORDER BY OperationsShapes.OperationName";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var operationShape = new OperationsShapes
                            {
                                OperationName = reader["OperationName"].ToString().Trim(),
                                BasicShape = reader["BasicShape"].ToString().Trim(),
                                External = Convert.ToInt16(reader["External"]),
                                XDistance = reader["XDistance"].ToString(),
                                YDistance = reader["YDistance"].ToString(),
                                ZDistance = reader["ZDistance"].ToString(),
                                Mill = reader["Mill"].ToString(),
                                Depth = Convert.ToDouble(reader["Depth"]),
                                XmlParameters = reader["XmlParameters"].ToString(),
                                Dimension = Convert.ToDouble(reader["Dimension"]),
                                Rotation = Convert.ToDouble(reader["Rotation"]),
                                Conditions = reader["Conditions"].ToString(),
                                Order = Convert.ToInt32(reader["Order"])
                            };

                            operationsShapesList.Add(operationShape);
                        }
                    }
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                if (!System.IO.Directory.Exists(savePath))
                {
                    System.IO.Directory.CreateDirectory(savePath);
                }

                int numberOperationShape = 1;
                string operationName = string.Empty;
                foreach (var operationShape in operationsShapesList.OrderBy(os => os.OperationName))
                {
                    if (operationShape.OperationName != operationName)
                    {
                        numberOperationShape = 1;
                    }
                    else
                    {
                        numberOperationShape++;
                    }

                    string fileName = $"{operationShape.OperationName.Trim()}_{numberOperationShape.ToString()}.json";
                    string path = Path.Combine(savePath, fileName);
                    File.WriteAllText(path, JsonSerializer.Serialize(operationShape, options));

                    operationName = operationShape.OperationName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(32)" + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void EnableControls(bool enable)
        {
            chkList_Operaciones.Enabled = enable;
            btn_CargarOperations.Enabled = enable;
            btn_ClearOperations.Enabled = enable;
            btn_ExportMacros.Enabled = enable;
            btn_InstalarMacros.Enabled = enable;
        }
        private void LoadSets(string filter)
        {
            List<Set> setsFilter = new List<Set>();

            if (xmlCargado && xmlOrigen != null && xmlOrigen.SetList.Count > 0)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    foreach (Set set in xmlOrigen.SetList.OrderBy(s => s.Code))
                    {
                        chkList_Sets.Items.Add(set, chk_All.Checked);
                    }
                }
                else
                {
                    foreach (Set set in xmlOrigen.SetList.Where(sl => sl.Code.ToLower().Contains(filter.ToLower())).OrderBy(s => s.Code))
                    {
                        chkList_Sets.Items.Add(set, chk_All.Checked);
                    }
                }

                chkList_Sets.DisplayMember = "Code"; // Muestra el código del Set
            }
        }
        private void CrearGridDetalleOperaciones()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.RowTemplate.Height = 10;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Definir DataTable con columnas
            _dataTable = new DataTable();
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Operacion"), typeof(string));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_FittingId"), typeof(string));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Articulo"), typeof(string));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Descripcion"), typeof(string));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Posicion"), typeof(string));
            _dataTable.Columns.Add(LocalizationManager.GetString("L_Location"), typeof(string));

            // Crear BindingSource y asignarle la tabla
            _bindingDetalleOperaciones = new BindingSource();
            _bindingDetalleOperaciones.DataSource = _dataTable;

            // Conectar la grilla al BindingSource (no directamente al DataTable)
            dataGridView1.DataSource = _bindingDetalleOperaciones;

            // Configuración de columnas (como ya lo tenías)
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Operacion"),
                DataPropertyName = LocalizationManager.GetString("L_Operacion"),
                ReadOnly = true,
                Width = 240
                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                //DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_FittingId"),
                DataPropertyName = LocalizationManager.GetString("L_FittingId"),
                ReadOnly = true,
                Width = 80
                //ImageLayout = DataGridViewImageCellLayout.Zoom,
                //AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Articulo"),
                DataPropertyName = LocalizationManager.GetString("L_Articulo"),
                ReadOnly = true,
                Width = 80
                //AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                //DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Descripcion"),
                DataPropertyName = LocalizationManager.GetString("L_Descripcion"),
                ReadOnly = true,
                Width = 330
                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                //DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Posicion"),
                DataPropertyName = LocalizationManager.GetString("L_Posicion"),
                ReadOnly = true,
                Width = 80
                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                //DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });


            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Location"),
                DataPropertyName = LocalizationManager.GetString("L_Location"),
                ReadOnly = true,
                Width = 80
                //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                //DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
            });
        }
        private void CrearGridInstalarOperaciones()
        {
            dataGridView2.AutoGenerateColumns = false;
            dataGridView2.AllowUserToAddRows = false;

            dataGridView2.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Selected",
                HeaderText = "",
                Width = 30
            });

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "OperationName",
                HeaderText = LocalizationManager.GetString("L_Operacion"),
                ReadOnly = true,
                Width = 400
            });

            var colGeometria = new DataGridViewImageColumn
            {
                Name = "ConfigureShapes",
                HeaderText = "",
                Image = Properties.Resources.Geometria, // tu icono
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Width = 30
            };
            dataGridView2.Columns.Add(colGeometria);

            dataGridView2.ReadOnly = false;
            dataGridView2.Enabled = true;

        }
        private void DarEstiloCabecerasDetalleOperaciones()
        {
            dataGridView1.EnableHeadersVisualStyles = false; // muy importante, evita que Windows sobrescriba el estilo
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Opcional: bordes y selección
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.GridColor = Color.LightGray;
        }
        private void CargarDatosGridDetalle(List<OperationGridRow> operationGridRowList)
        {
            _dataTable.Rows.Clear();

            foreach (var operationGridRow in operationGridRowList)
            {
                _dataTable.Rows.Add(operationGridRow.Operation, operationGridRow.FittingID, operationGridRow.Article, operationGridRow.Descripcion, operationGridRow.X, operationGridRow.Location);
            }
        }
        private void CleanInfo()
        {
            txt_filter.Text = "";
            txt_FilterOperations.Text = "";
            rb_All.Checked = true;
            chkList_Sets.Items.Clear();
            chk_All.Checked = false;
            if (_dataTable != null)
            {
                _dataTable.Rows.Clear();
            }
            chk_AllOperations.Checked = false;
            txt_FilterOperations.Clear();

            _bindingSource.DataSource = new List<OperationInstalarGridITem>();
        }
        private bool ExisteEnBD(string operationName)
        {
            if (!_cacheExisteBD.TryGetValue(operationName, out bool existe))
            {
                existe = Helpers.ExisteOperacionEnBD(operationName);
                _cacheExisteBD[operationName] = existe;
            }
            return existe;
        }
        private void EnableButtons(bool enable)
        {
            btn_LoadXml.Enabled = enable;
            btn_CargarOperations.Enabled = enable;
            btn_ClearOperations.Enabled = enable;
            btn_InstalarMacros.Enabled = enable;
            btn_ExportMacros.Enabled = enable;
            btn_InstallOperation.Enabled = enable;
            txt_filter.Enabled = enable;
            txt_FilterOperations.Enabled = enable;
            rb_All.Enabled = enable;
            rb_NoExists.Enabled = enable;
            chk_All.Enabled = enable;
            chk_AllOperations.Enabled = enable;
            dataGridView1.Enabled = enable;
            //dataGridView2.Enabled = enable;
            chkList_Sets.Enabled = enable;
            chkList_Operaciones.Enabled = enable;
        }
        #endregion

        private void dataGridView2_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            if (dataGridView2.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
            {
                dataGridView2.EndEdit();

                var item = (OperationInstalarGridITem)
                    dataGridView2.Rows[e.RowIndex].DataBoundItem;

                //MessageBox.Show($"Selected = {item.Selected}");
            }
        }
    }
    public class OperationGridRow
    {
        public string Operation { get; set; }
        public string FittingID { get; set; }
        public string Article { get; set; }
        public string Descripcion { get; set; }
        public string X { get; set; }
        public string Location { get; set; }

        public OperationGridRow() { }
        public OperationGridRow(string operation, string fittingId, string article, string descripcion, string x, string location)
        {
            Operation = operation;
            FittingID = fittingId;
            Article = article;
            Descripcion = descripcion;
            X = x;
            Location = location;
        }
    }
    public class OperationInstalarGridITem
    {
        public bool Selected { get; set; }
        public string OperationName { get; set; }
        public List<OperationsShapes> OperationShapeList { get; set; }
    }
}
