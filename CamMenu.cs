using Microsoft.Data.SqlClient;
using RotoEntities;
using System.Collections.Generic;
using System.ComponentModel;
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
        private List<OperationsShapes> _operationsShapesListEmbebidos = new List<OperationsShapes>();
        private bool _syncingSelection = false;

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
                EnableControls(false);
                xmlOrigen = LoadXml(rutaXml);
                lbl_Xml.Text = rutaXml;
                LoadSets("");
                EnableControls(true);
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

                        progress_Install.Visible = true;
                        int totalFilas = 5;
                        progress_Install.Value = 0;
                        progress_Install.Maximum = totalFilas ;

                        progress_Install.Value++;
                        progress_Install.Refresh();

                        ExportMacrosMechanizedOperations(Path.Combine(savePath, @"Macros\MechanizedOperations"));

                        progress_Install.Value++;
                        progress_Install.Refresh();

                        ExportMacrosOperationsShapes(Path.Combine(savePath, @"Macros\OperationsShapes"));

                        progress_Install.Value++;
                        progress_Install.Refresh();

                        ExportMecanizadosRoto(savePath);

                        progress_Install.Value++;
                        progress_Install.Refresh();

                        ExportMechanizedConditions(Path.Combine(savePath, "MechanizedConditions"));

                        progress_Install.Value++;
                        progress_Install.Refresh();

                        MessageBox.Show(savePath, "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        progress_Install.Value = 0;
                        progress_Install.Visible = false;

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
                progress_Install.Value = 0;
                progress_Install.Visible = false;
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
                LoadOperations();
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
            try
            {
                Cursor = Cursors.WaitCursor;
                EnableControls(false);

                progress_Install.Visible = true;
                int totalFilas = this._bindingSource.List.Count;
                progress_Install.Value = 0;
                progress_Install.Maximum = totalFilas > 0 ? totalFilas : 1; // Evitar división por cero

                List<MechanizedOperation> mechanizedOperationsEmbebdded = Helpers.CargarMechanizedOperationsRotoEmbebidos();
                List<MechanizedOperation> macrosEmbeddeMechanizeddOperations = Helpers.CargarMacrosMechanizedOperationsEmbebidos();
                List<OperationsShapes> macroOperationsShapesEmbeddedList = Helpers.CargarMacrosOperationsShapesEmbebidos();

                foreach (OperationInstalarGridITem item in this._bindingSource.List)
                {
                    if (item.Selected && !Helpers.ExisteOperacionEnBD("RO_" + item.OperationName))
                    {
                        List<MechanizedOperation> mechanizedOperationsList = mechanizedOperationsEmbebdded
                            .Where(op => op.OperationName == "RO_" + item.OperationName)
                            .ToList();

                        if (mechanizedOperationsList.Any())
                        {
                            foreach (MechanizedOperation operation in mechanizedOperationsList)
                            {
                                operation.InitializeLevel2(operation.OperationName);
                                operation.InitializeLevel3(operation.OperationName, operation.Level2);
                                Helpers.InstallMechanizedOperation(operation);
                            }
                        }
                        else
                        {
                            //Si no existe en los embebidos, se crea una operación básica
                            MechanizedOperation mechanizedOperation = new MechanizedOperation("RO_" + item.OperationName);
                            Helpers.InstallMechanizedOperation(mechanizedOperation);
                        }


                        List<OperationsShapes> allOperationsShapes = new List<OperationsShapes>();
                        allOperationsShapes.AddRange(item.OperationShapeList);
                        allOperationsShapes.AddRange(item.OperationShapeExtList);

                        //Instalar geometrías de la operación
                        foreach (OperationsShapes operationShape in allOperationsShapes)
                        {
                            //Se comprueba si tiene condiciones para instalarlas y obtener el RowId
                            if (!String.IsNullOrEmpty(operationShape.Conditions))
                            {
                                //Gestión de las condiciones
                                operationShape.Conditions = InstallConditions(operationShape.Conditions);
                            }

                            if (!Helpers.ExisteOperacionEnBD(operationShape.BasicShape))
                            {

                                MechanizedOperation? embeddedOperation = macrosEmbeddeMechanizeddOperations
                                    .FirstOrDefault(op => op.OperationName == operationShape.BasicShape);

                                if (embeddedOperation != null)
                                {
                                    Helpers.InstallMechanizedOperation(embeddedOperation!);
                                }

                                List<OperationsShapes> macroOperationsShapesList = macroOperationsShapesEmbeddedList.Where(o => o.OperationName == operationShape.BasicShape).ToList();
                                foreach (OperationsShapes operation in macroOperationsShapesList)
                                {
                                    Helpers.InstallOperationShape(operation);
                                }

                            }

                            Helpers.InstallOperationShape(operationShape);
                        }
                    }
                    // Actualizar progreso
                    progress_Install.Value++;
                    progress_Install.Refresh(); // Fuerza el repintado si el proceso es muy rápido
                }
                Cursor = Cursors.Default;
                EnableControls(true);
                MessageBox.Show(LocalizationManager.GetString("L_OperacionesInstaladas"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                progress_Install.Value = 0;
                progress_Install.Visible = false;
                _cacheExisteBD = new();
                chk_AllOperations.Checked = false;
                LoadOperations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(34)" + Environment.NewLine + Environment.NewLine +
                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                EnableControls(true);
            }
        }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0)
                return;

            if (dataGridView2.Columns[e.ColumnIndex].Name == "ConfigureShapes")
            {
                var item = (OperationInstalarGridITem)dataGridView2.Rows[e.RowIndex].DataBoundItem;

                using var frm = new CamConfigurarGeometria("RO_" + item.OperationName, item.OperationShapeList, item.OperationShapeExtList);
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    item.OperationShapeList = frm.ResultOperationsShapesList;
                }
            }
        }
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
        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (_syncingSelection) return;
            if (dataGridView2.CurrentRow == null) return;

            string operationName =
                dataGridView2.CurrentRow.Cells["OperationName"].Value?.ToString();

            if (string.IsNullOrWhiteSpace(operationName))
                return;

            try
            {
                _syncingSelection = true;
                SeleccionarFilaEnGrid1PorOperacion(operationName);
            }
            finally
            {
                _syncingSelection = false;
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Name != "Info")
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Info")
            {
                var item = (OperationGridRow)dataGridView1.Rows[e.RowIndex].DataBoundItem;

                using var frm = new CamInfoOperacion(item.Operation, item.OperationsList.OrderBy(o => o.Article).ToList());
                frm.ShowDialog();
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
            var gridRowDict = new Dictionary<string, OperationGridRow>();

            foreach (Set set in chkList_Sets.CheckedItems)
            {
                foreach (var setDescription in set.SetDescriptionList)
                {
                    foreach (var operation in ObtenerOperaciones(setDescription))
                    {
                        if (string.IsNullOrWhiteSpace(operation.Name))
                            continue;

                        if (operation.Name.Contains("SCREW", StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!gridRowDict.TryGetValue(operation.Name, out var mainRow))
                        {
                            mainRow = new OperationGridRow(
                                operation.Name,
                                setDescription.Fitting?.Id.ToString(),
                                setDescription.Fitting?.Ref,
                                setDescription.Fitting?.Description,
                                operation.XPosition,
                                operation.Location,
                                set.Code,
                                setDescription.XPosition.ToString()
                            );

                            //
                            mainRow.OperationsList.Add(mainRow);

                            gridRowDict.Add(operation.Name, mainRow);
                        }
                        else
                        {
                            bool exists = mainRow.OperationsList.Any(x => x.FittingID == setDescription.Fitting?.Id.ToString());

                            if (!exists)
                            {
                                mainRow.OperationsList.Add(new OperationGridRow(
                                    operation.Name,
                                    setDescription.Fitting?.Id.ToString(),
                                    setDescription.Fitting?.Ref,
                                    setDescription.Fitting?.Description,
                                    operation.XPosition,
                                    operation.Location,
                                    set.Code,
                                    setDescription.XPosition.ToString()
                                ));
                            }
                        }
                    }
                }
            }

            _allData = gridRowDict.Values
                .OrderBy(o => o.Operation)
                .ToList();

            operationsXmlList = _allData
                .Select(o => new Operation { Name = o.Operation })
                .ToList();
        }
        private void CargarGridInstalarOperaciones()
        {
            this._allOperations = this.operationsXmlList.OrderBy(op => op.Name).Select(o => new OperationInstalarGridITem
            {
                Selected = false,
                OperationName = o.Name,
                OperationShapeList = GetGeometriaOperacionList(o.Name, 0),
                OperationShapeExtList = GetGeometriaOperacionList(o.Name, 1)
            }).ToList();

            _bindingSource.DataSource = _allOperations;
            dataGridView2.DataSource = _bindingSource;
        }
        private IEnumerable<Operation> ObtenerOperaciones(SetDescription setDescription)
        {
            // Operaciones directas del fitting
            if (setDescription.Fitting?.OperationList != null)
            {
                foreach (var op in setDescription.Fitting.OperationList)
                    yield return op;
            }

            // Operaciones de los artículos del fitting
            if (setDescription.Fitting?.ArticleList != null)
            {
                foreach (var article in setDescription.Fitting.ArticleList)
                {
                    if (article.Fitting?.OperationList != null)
                    {
                        foreach (var op in article.Fitting.OperationList)
                            yield return op;
                    }
                }
            }
        }
        private List<OperationsShapes> GetGeometriaOperacionList(string operationName, short exterior)
        {
            return _operationsShapesListEmbebidos
                .Where(os => os.OperationName == "RO_" + operationName && os.External == exterior)
                .OrderBy(o => o.BasicShape)
                .ToList();
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

            // Filtro por texto
            string filtroTexto = txt_FilterOperations.Text.Trim();

            if (!string.IsNullOrWhiteSpace(filtroTexto))
            {
                query = query.Where(o =>
                    o.OperationName.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase));
            }

            // Filtro por RadioButton
            if (rb_NoExists.Checked)
            {
                query = query.Where(o => !ExisteOperacionEnBD("RO_" + o.OperationName));
            }
            // rb_Todas.Checked => no se filtra nada

            // Aplicar resultado
            _bindingSource.DataSource = query.ToList();
            group_Operaciones.Text = LocalizationManager.GetString("L_Operaciones") + $" ({_bindingSource.Count})";
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
                                reader["Description"].ToString().Trim(),
                                Convert.ToInt16(reader["External"]),
                                Convert.ToInt16(reader["IsPrimitive"]),
                                reader["Level1"].ToString().Trim(),
                                reader["Level2"].ToString().Trim(),
                                reader["Level3"].ToString().Trim(),
                                reader["Level4"].ToString().Trim(),
                                reader["Level5"].ToString().Trim(),
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
                                Mill = reader["Mill"].ToString().Trim(),
                                Depth = Convert.ToDouble(reader["Depth"]),
                                XmlParameters = reader["XmlParameters"].ToString().Trim(),
                                Dimension = Convert.ToDouble(reader["Dimension"]),
                                Rotation = Convert.ToDouble(reader["Rotation"]),
                                Conditions = reader["Conditions"].ToString().Trim(),
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
                MessageBox.Show($"Error(36)" + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ExportMecanizadosRoto(string savePath)
        {
            ExportMecanizadosRotoMechanizedOperations(Path.Combine(savePath, "MechanizedOperationsRoto"));
            ExportMecanizadosRotoOperationsShapes(Path.Combine(savePath, "OperationsShapesRoto"));
        }
        private void ExportMecanizadosRotoOperationsShapes(string savePath)
        {
            try
            {
                List<OperationsShapes> operationsShapesList = new List<OperationsShapes>();

                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT OperationsShapes.* FROM OperationsShapes
                                    INNER JOIN MechanizedOperations ON MechanizedOperations.OperationName = OperationsShapes.OperationName 
                                                AND MechanizedOperations.[External] = OperationsShapes.[External]
                                    WHERE OperationsShapes.OperationName like 'RO_%' AND MechanizedOperations.IsPrimitive = 0
                                    ORDER BY OperationsShapes.OperationName, OperationsShapes.[External]";
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
                                XDistance = reader["XDistance"].ToString().Trim(),
                                YDistance = reader["YDistance"].ToString().Trim(),
                                ZDistance = reader["ZDistance"].ToString().Trim(),
                                Mill = reader["Mill"].ToString().Trim(),
                                Depth = Convert.ToDouble(reader["Depth"]),
                                XmlParameters = reader["XmlParameters"].ToString().Trim(),
                                Dimension = Convert.ToDouble(reader["Dimension"]),
                                Rotation = Convert.ToDouble(reader["Rotation"]),
                                Conditions = reader["Conditions"].ToString().Trim(),
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

                int numberOperationShape = 0;
                string operationName = null;
                short? operationExternal = null;

                foreach (var operationShape in operationsShapesList.OrderBy(os => os.OperationName).ThenBy(os => os.External))
                {
                    if (operationShape.OperationName != operationName || operationShape.External != operationExternal)
                    {
                        numberOperationShape = 1;
                    }
                    else
                    {
                        numberOperationShape++;
                    }

                    string fileName = $"{operationShape.OperationName.Trim()}-{operationShape.External}_{numberOperationShape}.json";

                    string path = Path.Combine(savePath, fileName);
                    File.WriteAllText(path, JsonSerializer.Serialize(operationShape, options));

                    operationName = operationShape.OperationName;
                    operationExternal = operationShape.External;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(36)" + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ExportMecanizadosRotoMechanizedOperations(string savePath)
        {
            try
            {
                List<MechanizedOperation> mechanizedOperationsRotoList = new List<MechanizedOperation>();

                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT * FROM MechanizedOperations WHERE OperationName LIKE 'RO_%' AND IsPrimitive = 0 ORDER BY OperationName";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MechanizedOperation mechanizedOperation = new MechanizedOperation(reader["OperationName"].ToString().Trim(),
                                reader["Description"].ToString().Trim(),
                                Convert.ToInt16(reader["External"]),
                                Convert.ToInt16(reader["IsPrimitive"]),
                                reader["Level1"].ToString().Trim(),
                                reader["Level2"].ToString().Trim(),
                                reader["Level3"].ToString().Trim(),
                                reader["Level4"].ToString().Trim(),
                                reader["Level5"].ToString().Trim(),
                                reader["Phase"].ToString().Trim(),
                                reader["XmlParameters"].ToString().Trim(),
                                Convert.ToInt32(reader["RGB"]),
                                Convert.ToBoolean(reader["Disable"]));

                            mechanizedOperationsRotoList.Add(mechanizedOperation);
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

                foreach (var mechanizedOperation in mechanizedOperationsRotoList)
                {
                    string fileName = $"{mechanizedOperation.OperationName.Trim()}-{mechanizedOperation.External.ToString()}.json";
                    string path = Path.Combine(savePath, fileName);
                    File.WriteAllText(path, JsonSerializer.Serialize(mechanizedOperation, options));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(32)" + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ExportMechanizedConditions(string savePath)
        {
            try
            {
                List<MechanizedConditions> mechanizedConditionsList = new List<MechanizedConditions>();

                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT * FROM MechanizedConditions WHERE ROWID IN (SELECT Conditions FROM OperationsShapes WHERE Conditions IS NOT NULL AND OperationName LIKE 'RO\_%' ESCAPE '\')";
                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var mechanizedCondition = new MechanizedConditions
                            {
                                RowId = reader["RowId"].ToString().Trim(),
                                Name = reader["Name"].ToString().Trim(),
                                XmlConditions = reader["XmlConditions"].ToString().Trim(),
                                XmlOptions = reader["XmlOptions"].ToString().Trim()
                            };

                            mechanizedConditionsList.Add(mechanizedCondition);
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

                foreach (var mechanizedCondition in mechanizedConditionsList)
                {
                    string fileName = $"{mechanizedCondition.Name}.json";
                    string path = Path.Combine(savePath, fileName);
                    File.WriteAllText(path, JsonSerializer.Serialize(mechanizedCondition, options));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(3)" + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void EnableControls(bool enable)
        {
            btn_CargarOperations.Enabled = enable;
            btn_ClearOperations.Enabled = enable;
            btn_ExportMacros.Enabled = enable;
            btn_InstalarMacros.Enabled = enable;
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
            chkList_Sets.Enabled = enable;
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

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Operacion"),
                DataPropertyName = nameof(OperationGridRow.Operation),
                Name = "OperationName",
                ReadOnly = true,
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_FittingId"),
                DataPropertyName = nameof(OperationGridRow.FittingID),
                ReadOnly = true,
                Width = 80
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Articulo"),
                DataPropertyName = nameof(OperationGridRow.Article),
                ReadOnly = true,
                Width = 80
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Descripcion"),
                DataPropertyName = nameof(OperationGridRow.Descripcion),
                ReadOnly = true,
                Width = 330
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Location"),
                DataPropertyName = nameof(OperationGridRow.Location),
                ReadOnly = true,
                Width = 80
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Posicion"),
                DataPropertyName = nameof(OperationGridRow.X),
                ReadOnly = true,
                Width = 80
            });

            var colInfo = new DataGridViewImageColumn
            {
                Name = "Info",
                HeaderText = "",
                Image = Properties.Resources.info, // tu icono
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Width = 30
            };
            dataGridView1.Columns.Add(colInfo);
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
                Width = 400,
                Name = "OperationName"
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
        private void CargarDatosGridDetalle()
        {
            _bindingDetalleOperaciones = new BindingSource();
            _bindingDetalleOperaciones.DataSource = _allData;

            dataGridView1.DataSource = _bindingDetalleOperaciones;
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
            _cacheExisteBD = new();

            _bindingSource.DataSource = new List<OperationInstalarGridITem>();
        }
        private bool ExisteOperacionEnBD(string operationName)
        {
            if (!_cacheExisteBD.TryGetValue(operationName, out bool existe))
            {
                existe = Helpers.ExisteOperacionEnBD(operationName);
                _cacheExisteBD[operationName] = existe;
            }
            return existe;
        }
        private string InstallConditions(string conditionId)
        {
            //Obtener todas las condiciones embebidas
            List<MechanizedConditions> allConditionsList = Helpers.CargarMechanizedConditionsEmbebidos();

            //Buscar la condición por su RowId
            MechanizedConditions? mechanizedCondition = allConditionsList.FirstOrDefault(c => c.RowId == conditionId);

            if (mechanizedCondition != null)
            {
                //Comprobar si existe en base de datos una condicion con el mismo contenido en XmlConditions (El RowId puede variar)
                if (!Helpers.ExisteCondicionEnBD(mechanizedCondition.XmlConditions))
                {
                    //Insertar la condición en la base de datos
                    Helpers.InstallMechanizedCondition(mechanizedCondition);

                    //Obtener el rowId de la nueva condición creada
                    return Helpers.GetMechanizedConditionRowId(mechanizedCondition.Name);
                }
                else
                {
                    //Obtener el rowId de la condición existente
                    return Helpers.GetMechanizedConditionRowIdByXmlConditions(mechanizedCondition.XmlConditions);
                }
            }
            else
            {
                return "";
            }
        }
        private void LoadOperations()
        {
            _operationsShapesListEmbebidos = Helpers.CargarOperationsShapesRotoEmbebidos();
            CargarListaOperacionesFromXml();
            CargarGridInstalarOperaciones();
            CargarDatosGridDetalle();
            AplicarFiltros();
        }
        private void SeleccionarFilaEnGrid1PorOperacion(string operationName)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                var value = row.Cells["OperationName"].Value?.ToString();

                if (string.Equals(value, operationName, StringComparison.OrdinalIgnoreCase))
                {
                    dataGridView1.ClearSelection();
                    row.Selected = true;
                    dataGridView1.CurrentCell = row.Cells[0];

                    // Opcional: llevar la fila a la vista
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        #endregion


    }
    public class OperationGridRow
    {
        public string Operation { get; set; }
        public string FittingID { get; set; }
        public string Article { get; set; }
        public string Descripcion { get; set; }
        public string X { get; set; }
        public string Location { get; set; }
        public string Set { get; set; }
        public string SetDescriptionXPosition { get; set; }

        public List<OperationGridRow> OperationsList { get; set; }

        public OperationGridRow() { }
        public OperationGridRow(string operation, string fittingId, string article, string descripcion, string x, string location, string set, string setDescriptionXPosition)
        {
            Operation = operation;
            FittingID = fittingId;
            Article = article;
            Descripcion = descripcion;
            X = x;
            Location = location;
            OperationsList = new List<OperationGridRow>();
            Set = set;
            SetDescriptionXPosition = setDescriptionXPosition;
        }
    }
    public class OperationInstalarGridITem
    {
        public bool Selected { get; set; }
        public string OperationName { get; set; }
        public List<OperationsShapes> OperationShapeList { get; set; }
        public List<OperationsShapes> OperationShapeExtList { get; set; }
    }
}
