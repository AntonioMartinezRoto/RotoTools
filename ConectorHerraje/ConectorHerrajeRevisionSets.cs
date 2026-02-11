using Microsoft.Data.SqlClient;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using RotoEntities;
using System.Data;
using System.Xml;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ConectorHerrajeRevisionSets : Form
    {
        #region Private properties

        private XmlData xmlData = new XmlData();
        private Connector connectorHerraje = new();
        private List<string> codesInConector = new List<string>();

        private List<Set> setsIncluidosList = new List<Set>();
        private List<Set> setsNoIncluidosList = new List<Set>();
        private List<string> codigosNoIncluidosEnXml = new List<string>();
        #endregion

        #region Constructors
        public ConectorHerrajeRevisionSets()
        {
            InitializeComponent();
        }
        public ConectorHerrajeRevisionSets(XmlData xmlData)
        {
            InitializeComponent();
            this.xmlData = xmlData;
        }
        public ConectorHerrajeRevisionSets(XmlData xmlData, Connector connectorHerraje)
        {
            InitializeComponent();
            this.xmlData = xmlData;
            this.connectorHerraje = connectorHerraje;
        }
        #endregion

        #region Events
        private void ConectorHerrajeRevisionSets_Load(object sender, EventArgs e)
        {
            InitializeInfoConnection();
            LoadItemsConectorHerraje();
            LoadItemsHardwareSupplier();
            FormatGrids();
        }
        private void list_SetsNoUsadosEnConector_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CopiarAlPortapapeles(sender, e);
        }
        private void list_SetsUsadosEnConectorH_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CopiarAlPortapapeles(sender, e);
        }
        private void list_CodigosNoXml_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CopiarAlPortapapeles(sender, e);
        }
        private void txt_FiltroIncluidos_TextChanged(object sender, EventArgs e)
        {
            list_SetsUsadosEnConectorH.Items.Clear();
            list_SetsUsadosEnConectorH.View = View.Details;

            if (string.IsNullOrEmpty(txt_FiltroIncluidos.Text))
            {
                foreach (var set in xmlData.SetList.OrderBy(s => s.Code))
                {
                    if (string.IsNullOrWhiteSpace(set.Code)) continue;

                    // Creamos el item principal con el ID
                    var item = new ListViewItem(set.Id.ToString());

                    // Añadimos el código como subítem
                    item.SubItems.Add(set.Code);

                    if (codesInConector.Contains(set.Code))
                    {
                        list_SetsUsadosEnConectorH.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (Set set in xmlData.SetList.Where(sl => sl.Code.ToLower().Contains(txt_FiltroIncluidos.Text.ToLower())).OrderBy(s => s.Code))
                {
                    if (string.IsNullOrWhiteSpace(set.Code)) continue;

                    // Creamos el item principal con el ID
                    var item = new ListViewItem(set.Id.ToString());

                    // Añadimos el código como subítem
                    item.SubItems.Add(set.Code);

                    if (codesInConector.Contains(set.Code))
                    {
                        list_SetsUsadosEnConectorH.Items.Add(item);
                    }
                }
            }
        }
        private void txt_FiltroNoIncluidos_TextChanged(object sender, EventArgs e)
        {
            list_SetsNoUsadosEnConector.Items.Clear();
            list_SetsNoUsadosEnConector.View = View.Details;

            if (string.IsNullOrEmpty(txt_FiltroNoIncluidos.Text))
            {
                foreach (Set set in setsNoIncluidosList)
                {
                    if (string.IsNullOrWhiteSpace(set.Code)) continue;

                    // Creamos el item principal con el ID
                    var item = new ListViewItem(set.Id.ToString());

                    // Añadimos el código como subítem
                    item.SubItems.Add(set.Code);
                    list_SetsNoUsadosEnConector.Items.Add(item);
                }
            }
            else
            {
                foreach (Set set in setsNoIncluidosList.Where(s => s.Code.ToLower().Contains(txt_FiltroNoIncluidos.Text.ToLower())).OrderBy(c => c.Code))
                {
                    if (string.IsNullOrWhiteSpace(set.Code)) continue;

                    // Creamos el item principal con el ID
                    var item = new ListViewItem(set.Id.ToString());

                    // Añadimos el código como subítem
                    item.SubItems.Add(set.Code);
                    list_SetsNoUsadosEnConector.Items.Add(item);
                }
            }
        }
        private void txt_FiltroCodigoNoXml_TextChanged(object sender, EventArgs e)
        {
            //list_CodigosNoXml.Items.Clear();
            //list_CodigosNoXml.View = View.Details;

            //if (string.IsNullOrEmpty(txt_FiltroCodigoNoXml.Text))
            //{
            //    foreach (var codigoHerraje in codesInConector.OrderBy(s => s))
            //    {
            //        if (string.IsNullOrWhiteSpace(codigoHerraje)) continue;
            //        var item = new ListViewItem(codigoHerraje);

            //        if (!xmlData.SetList.Any(s => s.Code.Trim().ToLower() == codigoHerraje.Trim().ToLower()))
            //        {
            //            list_CodigosNoXml.Items.Add(item);
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (var codigoHerraje in codesInConector.Where(c => c.Trim().ToLower().Contains(txt_FiltroCodigoNoXml.Text.Trim().ToLower())).OrderBy(s => s))
            //    {
            //        if (string.IsNullOrWhiteSpace(codigoHerraje)) continue;
            //        var item = new ListViewItem(codigoHerraje);

            //        if (!xmlData.SetList.Any(s => s.Code.Trim().ToLower() codigoHerraje.Trim().ToLower()))
            //        {
            //            list_CodigosNoXml.Items.Add(item);
            //        }
            //    }
            //}
        }
        private void btn_ExportExcelIncluidos_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo Excel (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Save as";
            saveFileDialog.FileName = "SetsEnConector.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelPath = saveFileDialog.FileName;

                bool resultadoExportacion = ExportarExcel(excelPath, setsIncluidosList, "Sets incluidos en el conector");
                if (resultadoExportacion)
                {
                    MessageBox.Show("Guardado correctamente.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ha ocurrido un problema y no se ha podido exportar a Excel.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void btn_ExportExcelNoIncluidos_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo Excel (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Save as";
            saveFileDialog.FileName = "SetsNOEnConector.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelPath = saveFileDialog.FileName;

                bool resultadoExportacion = ExportarExcel(excelPath, setsNoIncluidosList, "Sets NO incluidos en el conector");
                if (resultadoExportacion)
                {
                    MessageBox.Show("Guardado correctamente.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ha ocurrido un problema y no se ha podido exportar a Excel.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void btn_ExportExcelCodigos_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo Excel (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Save as";
            saveFileDialog.FileName = "CodigosEnConector.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelPath = saveFileDialog.FileName;

                bool resultadoExportacion = ExportarExcel(excelPath, codigosNoIncluidosEnXml, "Códigos NO incluidos en el XML");
                if (resultadoExportacion)
                {
                    MessageBox.Show("Guardado correctamente.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ha ocurrido un problema y no se ha podido exportar a Excel.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void btn_EliminarLineasConector_Click(object sender, EventArgs e)
        {
            if (connectorHerraje == null)
            {
                MessageBox.Show("No hay ningún conector cargado.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (list_CodigosNoXml.Items.Count == 0)
            {
                MessageBox.Show("No hay líneas no usadas para eliminar.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                "Se eliminarán las líneas del conector que no están en el XML.\n¿Deseas continuar?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // Eliminar los nodos del objeto en memoria
                int antes = connectorHerraje.Nodes.Count;

                connectorHerraje.Nodes = connectorHerraje.Nodes
                    .Where(n => !codigosNoIncluidosEnXml.Contains(n.FittingCode, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                int eliminados = antes - connectorHerraje.Nodes.Count;

                // Serializar el objeto actualizado
                string xmlActualizado = Helpers.SerializarXml(connectorHerraje);

                // Guardar en base de datos
                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"UPDATE ConectorHerrajes SET XML = @Xml Where Codigo = @Codigo;";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Xml", xmlActualizado);
                        cmd.Parameters.AddWithValue("@Codigo", connectorHerraje.ConnectorCode);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    $"Eliminadas {eliminados} líneas no usadas.\nConector actualizado correctamente.",
                    "Operación completada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Actualizar la vista
                FillData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error eliminando las líneas no usadas:\n\n" + ex.Message,
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void cmb_Conectores_SelectedValueChanged(object sender, EventArgs e)
        {
            LoadConectorDataFromDB(cmb_Conectores.Text.TrimEnd());

            if (connectorHerraje != null)
                FillData();
        }
        private void cmb_HardwareSupplier_SelectedValueChanged(object sender, EventArgs e)
        {
            FillData();
        }
        #endregion

        #region Private methods
        private void InitializeInfoConnection()
        {
            statusStrip1.BackColor = Color.Transparent;
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void FormatGrids()
        {
            list_SetsUsadosEnConectorH.View = View.Details;
            list_SetsUsadosEnConectorH.Columns.Add("Id");
            list_SetsUsadosEnConectorH.Columns.Add("Set");

            list_SetsNoUsadosEnConector.View = View.Details;
            list_SetsNoUsadosEnConector.Columns.Add("Id");
            list_SetsNoUsadosEnConector.Columns.Add("Set");

            list_CodigosNoXml.View = View.Details;
            list_CodigosNoXml.Columns.Add("Set", 515);

            list_SetsUsadosEnConectorH.FullRowSelect = true;

            list_SetsNoUsadosEnConector.FullRowSelect = true;
        }
        private void CargarSetsEnListViews(List<Set> sets, List<string> fittingCodesFromConector)
        {
            list_SetsUsadosEnConectorH.Items.Clear();
            list_SetsNoUsadosEnConector.Items.Clear();
            list_CodigosNoXml.Items.Clear();

            int totalNoIncluidas = 0;
            int totalIncluidas = 0;
            int totalCodigoNoXml = 0;

            foreach (var set in sets.OrderBy(s => s.Code))
            {
                if (string.IsNullOrWhiteSpace(set.Code)) continue;

                // Creamos el item principal con el ID
                var item = new ListViewItem(set.Id.ToString());

                // Añadimos el código como subítem
                item.SubItems.Add(set.Code);

                if (fittingCodesFromConector.Contains(set.Code))
                {
                    list_SetsUsadosEnConectorH.Items.Add(item); // Está en el XML
                    totalIncluidas++;
                    setsIncluidosList.Add(set);
                }
                else
                {
                    list_SetsNoUsadosEnConector.Items.Add(item); // No está en el XML
                    totalNoIncluidas++;
                    setsNoIncluidosList.Add(set);
                }
            }

            foreach (var codigoHerraje in fittingCodesFromConector)
            {
                if (string.IsNullOrWhiteSpace(codigoHerraje)) continue;
                var item = new ListViewItem(codigoHerraje);

                if (!xmlData.SetList.Any(s => s.Code.Trim().ToLower() == codigoHerraje.Trim().ToLower()))
                {
                    list_CodigosNoXml.Items.Add(item);
                    totalCodigoNoXml++;
                    codigosNoIncluidosEnXml.Add(codigoHerraje);
                }

            }

            // Ajuste automático del ancho de columnas según el contenido
            list_SetsUsadosEnConectorH.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            list_SetsNoUsadosEnConector.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            lbl_TotalSetsNoIncluidosConector.Text = "Hay " + totalNoIncluidas.ToString() + " de " + xmlData.SetList.Count().ToString() + " Sets NO incluidos en el conector";
            lbl_TotalSetsIncluidos.Text = "Hay " + totalIncluidas.ToString() + " de " + xmlData.SetList.Count().ToString() + " Sets que están incluidos en el conector";
            lbl_TotalCodigosNoXml.Text = "Hay " + totalCodigoNoXml.ToString() + " Códigos de Herraje que no están en el XML";
        }
        private void CopiarAlPortapapeles(object sender, MouseEventArgs e)
        {
            if (sender is not ListView listView)
                return;

            var info = listView.HitTest(e.X, e.Y);
            if (info.Item == null || info.SubItem == null)
                return;

            string texto = info.SubItem.Text;

            if (!string.IsNullOrEmpty(texto))
            {
                Clipboard.SetText(texto);
                ToolTip tt = new ToolTip();
                tt.Show($"Copiado: {texto}", listView, e.X + 10, e.Y + 10, 1200);
            }
        }
        private bool ExportarExcel(string excelPath, List<Set> setsExportar, string tituloHoja)
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet hoja = workbook.CreateSheet(tituloHoja);
            CreateHeader(hoja);

            int filaActual = 1;
            foreach (Set set in setsExportar)
            {
                IRow fila = hoja.CreateRow(filaActual++);

                int col = 0;

                fila.CreateCell(col++).SetCellValue(set.Id);
                fila.CreateCell(col++).SetCellValue(set.Code);
            }

            SetColumnsWidth(hoja);
            // Guardar el archivo Excel
            using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
            }
            return true;
        }
        private bool ExportarExcel(string excelPath, List<string> codigosExportar, string tituloHoja)
        {
            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet hoja = workbook.CreateSheet(tituloHoja);
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int colHeader = 0;
            filaCabecera.CreateCell(colHeader++).SetCellValue("Código");

            int filaActual = 1;
            foreach (string codigo in codigosExportar)
            {
                IRow fila = hoja.CreateRow(filaActual++);

                int colRow = 0;

                fila.CreateCell(colRow++).SetCellValue(codigo);
            }

            int colWidth = 0;
            hoja.SetColumnWidth(colWidth++, 60 * 256);  // Codigo

            // Guardar el archivo Excel
            using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
            }
            return true;
        }
        private void CreateHeader(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Id");
            filaCabecera.CreateCell(col++).SetCellValue("Código");
        }
        private void SetColumnsWidth(ISheet hoja)
        {
            int col = 0;

            hoja.SetColumnWidth(col++, 10 * 256);   // Id
            hoja.SetColumnWidth(col++, 60 * 256);  // Codigo
        }
        private void LoadItemsHardwareSupplier()
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Valor FROM ContenidoOpciones WHERE Opcion = 'HardwareSupplier' ORDER BY Orden", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            cmb_HardwareSupplier.Items.Clear();

            cmb_HardwareSupplier.Items.Add("");

            while (reader.Read())
            {
                cmb_HardwareSupplier.Items.Add(reader[0].ToString());
            }
        }
        private void LoadItemsConectorHerraje()
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Codigo, XML FROM ConectorHerrajes", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            cmb_Conectores.Items.Clear();

            while (reader.Read())
            {
                cmb_Conectores.Items.Add(reader[0].ToString());
            }
        }
        private void LoadConectorDataFromDB(string conectorName)
        {
            try
            {
                string xmlString = null;

                using (var conexion = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conexion.Open();

                    var query = "SELECT XML FROM ConectorHerrajes WHERE Codigo = @codigo"; // Reemplaza 'Id' y agrega parámetro si necesitas.
                    using var cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@codigo", conectorName); // Ajusta según tu clave primaria o criterio

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        xmlString = result.ToString();
                    }
                    else
                    {
                        connectorHerraje = null;
                        MessageBox.Show("El conector seleccionado no contiene información en la base de datos", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                if (!string.IsNullOrWhiteSpace(xmlString))
                {
                    connectorHerraje = Helpers.DeserializarXML<Connector>(xmlString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el conector: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void FillData()
        {
            if (connectorHerraje?.Nodes == null || !connectorHerraje.Nodes.Any())
                return;

            // Valor seleccionado en el combo (puede ser vacío o nulo si no se filtra)
            string selectedSupplier = cmb_HardwareSupplier.SelectedItem?.ToString()?.Trim();
            bool aplicarFiltroProveedor = !string.IsNullOrEmpty(selectedSupplier);

            // Filtramos los nodos según si tienen FittingCode válido y cumplen el filtro
            var fittingCodesFromConector = connectorHerraje.Nodes
                .Where(n => !string.IsNullOrWhiteSpace(n.FittingCode))
                .Where(n =>
                {
                    if (!aplicarFiltroProveedor)
                        return true; // sin filtro

                    // Buscar si este nodo tiene la opción HardwareSupplier con el valor del combo
                    var options = n.IncludedOptions?.Options?.OptionList;
                    if (options == null)
                        return false;

                    return options.Any(o =>
                        o.Name.Equals("HardwareSupplier", StringComparison.OrdinalIgnoreCase) &&
                        o.Value.Equals(selectedSupplier, StringComparison.OrdinalIgnoreCase));
                })
                .Select(n => n.FittingCode)
                .Distinct()
                .ToList();


            //// Obtener los códigos Fitting_Code del XML:
            //var fittingCodesFromConector = connectorHerraje.Nodes
            //    .Where(n => !string.IsNullOrWhiteSpace(n.FittingCode))
            //    .Select(n => n.FittingCode)
            //    .Distinct()
            //    .ToList();

            codesInConector.Clear();
            codesInConector = fittingCodesFromConector;

            CargarSetsEnListViews(xmlData.SetList, fittingCodesFromConector);
        }
        
        #endregion




    }
}
