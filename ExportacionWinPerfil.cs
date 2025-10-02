using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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

namespace RotoTools
{
    public partial class ExportacionWinPerfil : Form
    {
        #region PRIVATE PROPERTIES
        public XmlData ExportDataXml { get; set; }
        public bool showSetDescriptionId { get; set; }
        public bool showSetDescriptionPosition { get; set; }
        public bool showFittingId { get; set; }
        public bool showFittingLength { get; set; }
        public bool formatoTabla { get; set; }

        #endregion

        #region CONSTRUCTORS
        public ExportacionWinPerfil()
        {
            InitializeComponent();
        }
        public ExportacionWinPerfil(XmlData exportDataXml)
        {
            InitializeComponent();
            ExportDataXml = exportDataXml;
        }

        #endregion

        #region EVENTS
        private void WinPerfilExportForm_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            chkList_Sets.Items.Clear();
            LoadColours();
            LoadProfiles();
            LoadSistemas();
            LoadSets("");
        }
        private void btn_ExportSets_Click(object sender, EventArgs e)
        {
            if (chkList_Sets.CheckedItems.Count == 0) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo Excel (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Save as";
            saveFileDialog.FileName = "Export.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelPath = saveFileDialog.FileName;

                bool resultadoExportacion = GenerarExportacion(excelPath);
                if (resultadoExportacion)
                {
                    MessageBox.Show("Exportación a WinPerfil completada.");
                }
                else
                {
                    MessageBox.Show("Ha habido algún problema con la exportación.");
                }
            }
        }
        private void txt_filter_TextChanged(object sender, EventArgs e)
        {
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
        #endregion

        #region PRIVATE METHODS
        private void LoadSistemas()
        {
            if (ExportDataXml.OptionList != null)
            {
                List<Value> valueList = ExportDataXml.OptionList.Where(o => o.Name == "1SISTEMA").FirstOrDefault().ValuesList.OrderBy(v => v.Valor).ToList();
                valueList.Insert(0, new Value { Valor = "" });

                cmb_Sistema.DataSource = valueList;
                cmb_Sistema.DisplayMember = "Valor";
            }

        }
        private void LoadSets(string filter)
        {
            List<Set> setsFilter = new List<Set>();

            if (ExportDataXml != null && ExportDataXml.SetList.Count > 0)
            {
                if (string.IsNullOrEmpty(filter))
                {
                    foreach (Set set in ExportDataXml.SetList.OrderBy(s => s.Code))
                    {
                        chkList_Sets.Items.Add(set, chk_All.Checked);
                    }
                }
                else
                {
                    foreach (Set set in ExportDataXml.SetList.Where(sl => sl.Code.ToLower().Contains(filter.ToLower())).OrderBy(s => s.Code))
                    {
                        chkList_Sets.Items.Add(set, chk_All.Checked);
                    }
                }

                chkList_Sets.DisplayMember = "Code"; // Muestra el código del Set
            }
        }
        private void LoadColours()
        {
            List<Colour> colourList = ExportDataXml.ColourList.OrderBy(c => c.Name).ToList();
            colourList.Insert(0, new Colour { Name = "" });

            cmb_Color.DataSource = colourList;
            cmb_Color.DisplayMember = "Name";
        }
        private void LoadProfiles()
        {
            List<Value> valueList = ExportDataXml.OptionList.Where(o => o.Name == "1PERFIL").FirstOrDefault().ValuesList.OrderBy(v => v.Valor).ToList();
            valueList.Insert(0, new Value { Valor = "" });

            cmb_Perfil.DataSource = valueList;
            cmb_Perfil.DisplayMember = "Valor";
        }
        private bool GenerarExportacion(string excelPath)
        {
            try
            {
                EnableControls(false);
                XSSFWorkbook workbook = new XSSFWorkbook();
                ISheet hoja = workbook.CreateSheet("Roto");

                CreateHeader(hoja);

                int totalFilas = 0;
                foreach (Set set in chkList_Sets.CheckedItems.OfType<Set>())
                {
                    totalFilas += set.SetDescriptionList?.Count ?? 0;
                }

                progress_Export.Value = 0;
                progress_Export.Maximum = totalFilas > 0 ? totalFilas : 1; // Evitar división por cero

                int filaActual = 1;
                foreach (var itemListChecked in chkList_Sets.CheckedItems)
                {
                    Set set = itemListChecked as Set;
                    if (set != null && set.SetDescriptionList != null)
                    {
                        foreach (SetDescription setDescription in set.SetDescriptionList)
                        {
                            if (EsFittingFicticio(setDescription.Fitting))
                            {
                                foreach (Article article in setDescription.Fitting.ArticleList)
                                {
                                    string condiciones = GetOptionsStringArticle(setDescription, article);
                                    if (GenerateRowCheckingConditions(condiciones))
                                    {
                                        IRow fila = hoja.CreateRow(filaActual++);

                                        int col = 0;
                                        if (showSetDescriptionId)
                                            fila.CreateCell(col++).SetCellValue(setDescription.Id);

                                        fila.CreateCell(col++).SetCellValue(set.Code);
                                        fila.CreateCell(col++).SetCellValue(setDescription.MinHeight);
                                        fila.CreateCell(col++).SetCellValue(setDescription.MaxHeight);
                                        fila.CreateCell(col++).SetCellValue(setDescription.MinWidth);
                                        fila.CreateCell(col++).SetCellValue(setDescription.MaxWidth);

                                        if (showFittingId)
                                            fila.CreateCell(col++).SetCellValue(setDescription.FittingId);

                                        fila.CreateCell(col++).SetCellValue(ExportDataXml.FittingList.FirstOrDefault(f => f.Ref == article.Ref.ToString())?.Description);
                                        fila.CreateCell(col++).SetCellValue(GetFinalReferenceColor(article.Ref));

                                        if (showSetDescriptionPosition)
                                            fila.CreateCell(col++).SetCellValue(setDescription.Position);

                                        if (showFittingLength)
                                            fila.CreateCell(col++).SetCellValue(ExportDataXml.FittingList.FirstOrDefault(f => f.Ref == article.Ref.ToString()).Lenght);

                                        fila.CreateCell(col++).SetCellValue(GetColour(article.Ref));
                                        fila.CreateCell(col++).SetCellValue(1);
                                        fila.CreateCell(col++).SetCellValue(condiciones);
                                    }
                                }
                            }
                            else
                            {
                                string condicionesSetDescription = GetOptionsString(setDescription);
                                if (GenerateRowCheckingConditions(condicionesSetDescription))
                                {
                                    IRow fila = hoja.CreateRow(filaActual++);
                                    int col = 0;
                                    if (showSetDescriptionId)
                                        fila.CreateCell(col++).SetCellValue(setDescription.Id);

                                    fila.CreateCell(col++).SetCellValue(set.Code);
                                    fila.CreateCell(col++).SetCellValue(setDescription.MinHeight);
                                    fila.CreateCell(col++).SetCellValue(setDescription.MaxHeight);
                                    fila.CreateCell(col++).SetCellValue(setDescription.MinWidth);
                                    fila.CreateCell(col++).SetCellValue(setDescription.MaxWidth);

                                    if (showFittingId)
                                        fila.CreateCell(col++).SetCellValue(setDescription.FittingId);

                                    fila.CreateCell(col++).SetCellValue(setDescription.Fitting?.Description);
                                    fila.CreateCell(col++).SetCellValue(GetFinalReferenceColor(setDescription.Fitting?.Ref));

                                    if (showSetDescriptionPosition)
                                        fila.CreateCell(col++).SetCellValue(setDescription.Position);

                                    if (showFittingLength)
                                        fila.CreateCell(col++).SetCellValue(setDescription.Fitting.Lenght);

                                    fila.CreateCell(col++).SetCellValue(GetColour(setDescription.Fitting?.Ref));
                                    fila.CreateCell(col++).SetCellValue(1);
                                    fila.CreateCell(col++).SetCellValue(condicionesSetDescription);
                                }
                            }
                            // Actualizar progreso
                            progress_Export.Value++;
                            progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido
                        }
                    }
                }

                //Ajustar ancho de columnas
                SetColumnsWidth(hoja);

                if (formatoTabla)
                {
                    int columnasTotales = 10;
                    if (showSetDescriptionId) columnasTotales++;
                    if (showSetDescriptionPosition) columnasTotales++;
                    if (showFittingId) columnasTotales++;
                    if (showFittingLength) columnasTotales++;

                    DarFomartoTabla(hoja, filaActual - 1, columnasTotales);
                }

                // Guardar el archivo Excel
                using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs);
                }

                EnableControls(true);
                progress_Export.Value = 0;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al procesar el XML: " + ex.Message);
                EnableControls(true);
                return false;
            }
        }
        private void EnableControls(bool enable)
        {
            chkList_Sets.Enabled = enable;
            chk_All.Enabled = enable;
            btn_ExportSets.Enabled = enable;
            cmb_Perfil.Enabled = enable;
            cmb_Sistema.Enabled = enable;
            cmb_Color.Enabled = enable;
            txt_filter.Enabled = enable;
        }
        private void CreateHeader(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            if (showSetDescriptionId)
                filaCabecera.CreateCell(col++).SetCellValue("SetDescription Id");

            filaCabecera.CreateCell(col++).SetCellValue("Tabla");
            filaCabecera.CreateCell(col++).SetCellValue("Alto desde");
            filaCabecera.CreateCell(col++).SetCellValue("Alto hasta");
            filaCabecera.CreateCell(col++).SetCellValue("Ancho desde");
            filaCabecera.CreateCell(col++).SetCellValue("Ancho hasta");

            if (showFittingId)
                filaCabecera.CreateCell(col++).SetCellValue("Fitting Id");

            filaCabecera.CreateCell(col++).SetCellValue("Descripción");
            filaCabecera.CreateCell(col++).SetCellValue("Referencia");

            if (showSetDescriptionPosition)
                filaCabecera.CreateCell(col++).SetCellValue("SetDescription Position");

            if (showFittingLength)
                filaCabecera.CreateCell(col++).SetCellValue("Longitud");

            filaCabecera.CreateCell(col++).SetCellValue("Color");
            filaCabecera.CreateCell(col++).SetCellValue("Unidades");
            filaCabecera.CreateCell(col++).SetCellValue("Condicion");
        }
        private bool EsFittingFicticio(Fitting fitting)
        {
            if (fitting != null && fitting.Ref.ToString().StartsWith("PROGRAM"))
            {
                return true;
            }

            return false;
        }
        private string GetOptionsStringArticle(SetDescription setDescription, Article article)
        {
            try
            {
                string optionString = GetOptionsString(setDescription);

                if (!string.IsNullOrEmpty(optionString))
                {
                    optionString += " y ";
                }

                if (article.OptionList != null || article.OptionList.Count > 0)
                {
                    foreach (Option option in article.OptionList)
                    {
                        optionString += "(";
                        optionString += option.Name;
                        optionString += " = ";
                        optionString += option.Value;
                        optionString += ")";

                        optionString += " y ";
                    }

                    optionString = optionString.Substring(0, optionString.Length - 3);
                }
                return optionString;
            }
            catch
            {
                return string.Empty;
            }
        }
        private string GetOptionsString(SetDescription setDescription)
        {
            try
            {
                string optionString = "";
                if (setDescription.OptionList != null || setDescription.OptionList.Count > 0)
                {
                    foreach (Option option in setDescription.OptionList)
                    {
                        optionString += "(";
                        optionString += option.Name;
                        optionString += " = ";
                        optionString += option.Value;
                        optionString += ")";

                        optionString += " y ";
                    }

                    optionString = optionString.Substring(0, optionString.Length - 3);
                }
                return optionString;
            }
            catch
            {
                return string.Empty;
            }
        }
        private bool GenerateRowCheckingConditions(string condiciones)
        {
            bool generateRowPerfil = true;
            bool generateRowSistema = true;
            //Si no hay filtro por perfil se genera siempre la linea
            if (string.IsNullOrEmpty(cmb_Perfil.Text) && string.IsNullOrEmpty(cmb_Sistema.Text))
            {
                return true;
            }
            else
            {
                if (!string.IsNullOrEmpty(cmb_Perfil.Text))
                {
                    //XML PVC
                    if (condiciones.Contains("1PERFIL = "))
                    {
                        //Si tiene la opción, y el combo está filtrando, el valor de la opción debe ser igual al combo
                        if (condiciones.Contains("1PERFIL = " + cmb_Perfil.Text))
                        {
                            generateRowPerfil = true;
                        }
                        else
                        {
                            generateRowPerfil = false;
                        }
                    }
                    else
                    {
                        generateRowPerfil = true;
                    }
                }

                if (!string.IsNullOrEmpty(cmb_Sistema.Text))
                {
                    //XML PVC
                    if (condiciones.Contains("1SISTEMA = "))
                    {
                        //Si tiene la opción, y el combo está filtrando, el valor de la opción debe ser igual al combo
                        if (condiciones.Contains("1SISTEMA = " + cmb_Sistema.Text))
                        {
                            generateRowSistema = true;
                        }
                        else
                        {
                            generateRowSistema = false;
                        }
                    }
                    else
                    {
                        generateRowSistema = true;
                    }
                }

                if (generateRowPerfil && generateRowSistema)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private string GetFinalReferenceColor(string articleRef)
        {
            if (string.IsNullOrEmpty(cmb_Color.Text))
            {
                return articleRef;
            }
            else
            {
                Colour color = ExportDataXml.ColourList.Where(c => c.Name == cmb_Color.Text).FirstOrDefault();
                if (color != null)
                {
                    Article article = color.ArticleList.Where(a => a.Ref == articleRef).FirstOrDefault();
                    if (article != null)
                    {
                        return article.Final;
                    }
                }
                return articleRef;
            }

        }
        private string GetColour(string articleRef)
        {
            if (string.IsNullOrEmpty(cmb_Color.Text))
            {
                return "";
            }
            else
            {
                Colour color = ExportDataXml.ColourList.Where(c => c.Name == cmb_Color.Text).FirstOrDefault();
                if (color != null)
                {
                    Article article = color.ArticleList.Where(a => a.Ref == articleRef).FirstOrDefault();
                    if (article != null)
                    {
                        return color.Name;
                    }
                }
                return "";
            }
        }
        private void SetColumnsWidth(ISheet hoja)
        {
            ////Ajustar tamaño de las columnas automaticamente (relentiza proceso mucho)
            //for (int i = 0; i < 10; i++)
            //{
            //    hoja.AutoSizeColumn(i);
            //}

            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            if (showSetDescriptionId)
                hoja.SetColumnWidth(col++, 20 * 256); // Id

            hoja.SetColumnWidth(col++, 45 * 256); // Código set
            hoja.SetColumnWidth(col++, 15 * 256); // MinHeight
            hoja.SetColumnWidth(col++, 15 * 256); // MaxHeight
            hoja.SetColumnWidth(col++, 15 * 256); // MinWidth
            hoja.SetColumnWidth(col++, 15 * 256); // MaxWidth

            if (showFittingId)
                hoja.SetColumnWidth(col++, 15 * 256); // FittingId

            hoja.SetColumnWidth(col++, 45 * 256); // Descripción fitting
            hoja.SetColumnWidth(col++, 15 * 256); // Referencia final

            if (showSetDescriptionPosition)
                hoja.SetColumnWidth(col++, 25 * 256); // Posición

            if (showFittingLength)
                hoja.SetColumnWidth(col++, 15 * 256); // Longitud pieza

            hoja.SetColumnWidth(col++, 25 * 256); // Color
            hoja.SetColumnWidth(col++, 15 * 256); // Cantidad
            hoja.SetColumnWidth(col++, 140 * 256); // Condiciones
        }
        private void DarFomartoTabla(ISheet hoja, int totalFilas, int totalColumnas)
        {
            var xssfSheet = hoja as XSSFSheet;
            if (xssfSheet == null) return;

            XSSFTable table = xssfSheet.CreateTable();
            table.Name = "RotoTabla";
            table.DisplayName = "RotoTabla";

            // Rango de la tabla
            var startCell = new NPOI.SS.Util.CellReference(0, 0);
            var endCell = new NPOI.SS.Util.CellReference(totalFilas, totalColumnas - 1);
            string areaRef = $"{startCell.FormatAsString()}:{endCell.FormatAsString()}";

            CT_Table ctTable = table.GetCTTable();
            ctTable.@ref = areaRef;
            ctTable.displayName = "RotoTabla";
            ctTable.name = "RotoTabla";
            ctTable.id = (uint)1;
            ctTable.headerRowCount = 1;

            // Habilita autofiltro para que aparezca el botón de ordenar/filtrar en la cabecera
            ctTable.autoFilter = new NPOI.OpenXmlFormats.Spreadsheet.CT_AutoFilter
            {
                @ref = areaRef
            };

            // Columnas
            ctTable.tableColumns = new NPOI.OpenXmlFormats.Spreadsheet.CT_TableColumns
            {
                count = (uint)totalColumnas,
                tableColumn = new List<NPOI.OpenXmlFormats.Spreadsheet.CT_TableColumn>()
            };

            IRow headerRow = hoja.GetRow(0);
            for (int i = 0; i < totalColumnas; i++)
            {
                string colName = headerRow?.GetCell(i)?.ToString() ?? $"Col{i + 1}";
                ctTable.tableColumns.tableColumn.Add(new NPOI.OpenXmlFormats.Spreadsheet.CT_TableColumn
                {
                    id = (uint)(i + 1),
                    name = colName
                });
            }

            // Estilo
            ctTable.tableStyleInfo = new NPOI.OpenXmlFormats.Spreadsheet.CT_TableStyleInfo
            {
                name = "TableStyleMedium2",
                showColumnStripes = false,
                showRowStripes = true,
                showFirstColumn = false,
                showLastColumn = false
            };
        }
        #endregion

    }
}
