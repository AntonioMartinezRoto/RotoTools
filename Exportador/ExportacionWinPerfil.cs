using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using RotoEntities;
using RotoTools.Exportador;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ExportacionWinPerfil : Form
    {
        #region PRIVATE PROPERTIES

        private List<Value> _perfilesListSelected = new List<Value>();
        private bool _filtroListaPerfilesActivo = false;

        #endregion

        #region Public properties
        public XmlData ExportDataXml { get; set; }
        public bool showSetDescriptionId { get; set; }
        public bool showSetDescriptionPosition { get; set; }
        public bool showFittingId { get; set; }
        public bool showFittingLength { get; set; }
        public bool formatoTabla { get; set; }
        public bool showSetId { get; set; }
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
            CargarTextos();
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
                    MessageBox.Show(LocalizationManager.GetString("L_ExportacionCompletada"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Error (15)", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private void ExportacionWinPerfil_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ExportacionOpciones exportacionOpcionesForm = new ExportacionOpciones(showSetDescriptionId, showSetDescriptionPosition, showFittingId, showFittingLength, formatoTabla, showSetId);
                if (exportacionOpcionesForm.ShowDialog() == DialogResult.OK)
                {
                    //Actualizar opciones de visualización según lo seleccionado en el formulario de opciones
                    showSetDescriptionId = exportacionOpcionesForm.ShowSetDescriptionId;
                    showSetDescriptionPosition = exportacionOpcionesForm.ShowSetDescriptionPosition;
                    showFittingId = exportacionOpcionesForm.ShowFittingId;
                    showFittingLength = exportacionOpcionesForm.ShowFittingLength;
                    formatoTabla = exportacionOpcionesForm.FormatoTabla;
                    showSetId = exportacionOpcionesForm.ShowSetId;
                }
            }
        }
        private void btn_FiltrarPerfil_Click(object sender, EventArgs e)
        {
            List<Value> profileList = ExportDataXml.OptionList.FirstOrDefault(o => o.Name == "1PERFIL").ValuesList.OrderBy(v => v.Valor).ToList();
            ExportacionWinPerfilListaPerfiles exportacionWinPerfilListaPerfilesForm = new ExportacionWinPerfilListaPerfiles(profileList, _perfilesListSelected);

            if (exportacionWinPerfilListaPerfilesForm.ShowDialog() == DialogResult.OK)
            {
                this._perfilesListSelected = exportacionWinPerfilListaPerfilesForm.PerfilesListSelected;
            }

            if (this._perfilesListSelected.Any())
            {
                cmb_Perfil.SelectedIndex = -1;
                cmb_Perfil.Enabled = false;
                _filtroListaPerfilesActivo = true;
            }
            else
            {
                cmb_Perfil.Enabled = true;
                _filtroListaPerfilesActivo = false;
            }
        }
        #endregion

        #region PRIVATE METHODS
        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_ExportarWinPerfil");
            lbl_Profile.Text = LocalizationManager.GetString("L_Perfil");
            lbl_System.Text = LocalizationManager.GetString("L_Sistema");
            lbl_Colour.Text = LocalizationManager.GetString("L_Color");
            chk_All.Text = LocalizationManager.GetString("L_SeleccionarTodos");
            lbl_Busqueda.Text = LocalizationManager.GetString("L_Buscar");
        }
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
                SXSSFWorkbook workbook = new SXSSFWorkbook(500);


                ISheet hojaTodo = workbook.CreateSheet("Todo");
                ISheet hojaVentanas = workbook.CreateSheet("Ventanas");
                ISheet hojaBalconeras = workbook.CreateSheet("Balconeras");
                ISheet hojaPuertas = workbook.CreateSheet("Puertas");
                ISheet hojaCorrederas = workbook.CreateSheet("Correderas");
                ISheet hojaElevables = workbook.CreateSheet("Elevables");
                ISheet hojaOsciloparalelas = workbook.CreateSheet("Osciloparalelas");
                ISheet hojaAbatibles = workbook.CreateSheet("Abatibles");
                ISheet hojaPlegables = workbook.CreateSheet("Plegables");


                CreateHeader(hojaTodo);
                CreateHeader(hojaVentanas);
                CreateHeader(hojaBalconeras);
                CreateHeader(hojaPuertas);
                CreateHeader(hojaCorrederas);
                CreateHeader(hojaElevables);
                CreateHeader(hojaOsciloparalelas);
                CreateHeader(hojaAbatibles);
                CreateHeader(hojaPlegables);

                int totalFilas = 0;
                string setsOtros = String.Empty;
                foreach (Set set in chkList_Sets.CheckedItems.OfType<Set>())
                {
                    totalFilas += set.SetDescriptionList?.Count ?? 0;
                    if (set.WindowType == (int)enumWindowType.Otro)
                    {
                        setsOtros += set.Code + Environment.NewLine;
                    }
                }

                progress_Export.Value = 0;
                progress_Export.Maximum = totalFilas > 0 ? totalFilas : 1;

                int filaActualTodo = 1;
                int filaActualVentanas = 1;
                int filaActualBalconeras = 1;
                int filaActualPuertas = 1;
                int filaActualCorrederas = 1;
                int filaActualElevables = 1;
                int filaActualOsciloparalelas = 1;
                int filaActualAbatibles = 1;
                int filaActualPlegables = 1;

                if (!String.IsNullOrEmpty(setsOtros))
                {
                    MessageBox.Show(setsOtros, "Los siguientes sets están marcados como 'Otro' y se incluirán en la hoja 'Todo'", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                var fittingDict = ExportDataXml.FittingList
                                    .GroupBy(f => f.Ref)
                                    .ToDictionary(g => g.Key, g => g.First());

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
                                        //Insertar en hoja general
                                        InsertRowFittingFicticio(hojaTodo, filaActualTodo++, set, setDescription, article, condiciones, fittingDict);

                                        switch (set.WindowType)
                                        {
                                            case (int)enumWindowType.Ventana:
                                                InsertRowFittingFicticio(hojaVentanas, filaActualVentanas++, set, setDescription, article, condiciones, fittingDict);
                                                break;
                                            case (int)enumWindowType.Balconera:
                                            case (int)enumWindowType.PuertaSecundaria:
                                                InsertRowFittingFicticio(hojaBalconeras, filaActualBalconeras++, set, setDescription, article, condiciones, fittingDict);
                                                break;
                                            case (int)enumWindowType.Puerta:
                                                InsertRowFittingFicticio(hojaPuertas, filaActualPuertas++, set, setDescription, article, condiciones, fittingDict);
                                                break;
                                            case (int)enumWindowType.Corredera:
                                                InsertRowFittingFicticio(hojaCorrederas, filaActualCorrederas++, set, setDescription, article, condiciones, fittingDict);
                                                break;
                                            case (int)enumWindowType.Elevable:
                                                InsertRowFittingFicticio(hojaElevables, filaActualElevables++, set, setDescription, article, condiciones, fittingDict);
                                                break;
                                            case (int)enumWindowType.Osciloparalela:
                                                InsertRowFittingFicticio(hojaOsciloparalelas, filaActualOsciloparalelas++, set, setDescription, article, condiciones, fittingDict);
                                                break;
                                            case (int)enumWindowType.Abatible:
                                                InsertRowFittingFicticio(hojaAbatibles, filaActualAbatibles++, set, setDescription, article, condiciones, fittingDict);
                                                break;
                                            case (int)enumWindowType.Plegable:
                                                InsertRowFittingFicticio(hojaPlegables, filaActualPlegables++, set, setDescription, article, condiciones, fittingDict);
                                                break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string condicionesSetDescription = GetOptionsString(setDescription);
                                if (GenerateRowCheckingConditions(condicionesSetDescription))
                                {
                                    //Insertar en hoja general
                                    InsertRowFitting(hojaTodo, filaActualTodo++, set, setDescription, condicionesSetDescription);

                                    switch (set.WindowType)
                                    {
                                        case (int)enumWindowType.Ventana:
                                            InsertRowFitting(hojaVentanas, filaActualVentanas++, set, setDescription, condicionesSetDescription);
                                            break;
                                        case (int)enumWindowType.Balconera:
                                        case (int)enumWindowType.PuertaSecundaria:
                                            InsertRowFitting(hojaBalconeras, filaActualBalconeras++, set, setDescription, condicionesSetDescription);
                                            break;
                                        case (int)enumWindowType.Puerta:
                                            InsertRowFitting(hojaPuertas, filaActualPuertas++, set, setDescription, condicionesSetDescription);
                                            break;
                                        case (int)enumWindowType.Corredera:
                                            InsertRowFitting(hojaCorrederas, filaActualCorrederas++, set, setDescription, condicionesSetDescription);
                                            break;
                                        case (int)enumWindowType.Elevable:
                                            InsertRowFitting(hojaElevables, filaActualElevables++, set, setDescription, condicionesSetDescription);
                                            break;
                                        case (int)enumWindowType.Osciloparalela:
                                            InsertRowFitting(hojaOsciloparalelas, filaActualOsciloparalelas++, set, setDescription, condicionesSetDescription);
                                            break;
                                        case (int)enumWindowType.Abatible:
                                            InsertRowFitting(hojaAbatibles, filaActualAbatibles++, set, setDescription, condicionesSetDescription);
                                            break;
                                        case (int)enumWindowType.Plegable:
                                            InsertRowFitting(hojaPlegables, filaActualPlegables++, set, setDescription, condicionesSetDescription);
                                            break;
                                    }
                                }
                            }
                            // Actualizar progreso
                            progress_Export.Value++;

                            if (progress_Export.Value % 100 == 0)
                                Application.DoEvents();
                        }
                    }
                }

                //Ajustar ancho de columnas
                SetColumnsWidth(hojaTodo);
                SetColumnsWidth(hojaVentanas);
                SetColumnsWidth(hojaBalconeras);
                SetColumnsWidth(hojaPuertas);
                SetColumnsWidth(hojaCorrederas);
                SetColumnsWidth(hojaElevables);
                SetColumnsWidth(hojaOsciloparalelas);
                SetColumnsWidth(hojaAbatibles);
                SetColumnsWidth(hojaPlegables);


                // Guardar el archivo Excel
                using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fs);
                }

                if (workbook is SXSSFWorkbook sx)
                {
                    sx.Dispose(); // Limpia archivos temporales
                }



                if (formatoTabla)
                {
                    int columnasTotales = 10;
                    if (showSetId) columnasTotales++;
                    if (showSetDescriptionId) columnasTotales++;
                    if (showSetDescriptionPosition) columnasTotales++;
                    if (showFittingId) columnasTotales++;
                    if (showFittingLength) columnasTotales++;

                    var configTablas = new Dictionary<string, (int, int)>
                    {
                        { "Todo", (filaActualTodo - 1, columnasTotales) },
                        { "Ventanas", (filaActualVentanas - 1, columnasTotales) },
                        { "Balconeras", (filaActualBalconeras - 1, columnasTotales) },
                        { "Puertas", (filaActualPuertas - 1, columnasTotales) },
                        { "Correderas", (filaActualCorrederas - 1, columnasTotales) },
                        { "Elevables", (filaActualElevables - 1, columnasTotales) },
                        { "Osciloparalelas", (filaActualOsciloparalelas - 1, columnasTotales) },
                        { "Abatibles", (filaActualAbatibles - 1, columnasTotales) },
                        { "Plegables", (filaActualPlegables - 1, columnasTotales) }
                    };

                    ReabrirYAplicarFormatoTabla(excelPath, configTablas);
                }


                EnableControls(true);
                progress_Export.Value = 0;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (16)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls(true);
                return false;
            }
        }
        private void InsertRowFitting(ISheet hoja, int filaActual, Set set, SetDescription setDescription, string condicionesSetDescription)
        {
            IRow fila = hoja.CreateRow(filaActual);
            int col = 0;

            if (showSetId)
                fila.CreateCell(col++).SetCellValue(set.Id);

            fila.CreateCell(col++).SetCellValue(set.Code);

            if (showSetDescriptionId)
                fila.CreateCell(col++).SetCellValue(setDescription.Id);

            fila.CreateCell(col++).SetCellValue(setDescription.MinHeight);
            fila.CreateCell(col++).SetCellValue(setDescription.MaxHeight);
            fila.CreateCell(col++).SetCellValue(setDescription.MinWidth);
            fila.CreateCell(col++).SetCellValue(setDescription.MaxWidth);

            if (showFittingId)
                fila.CreateCell(col++).SetCellValue(setDescription.FittingId);

            var fitting = setDescription.Fitting;

            fila.CreateCell(col++).SetCellValue(fitting?.Description ?? string.Empty);
            fila.CreateCell(col++).SetCellValue(GetFinalReferenceColor(fitting?.Ref));

            if (showSetDescriptionPosition)
                fila.CreateCell(col++).SetCellValue(setDescription.Position);

            if (showFittingLength)
                fila.CreateCell(col++).SetCellValue(fitting?.Lenght ?? 0);

            fila.CreateCell(col++).SetCellValue(GetColour(fitting?.Ref));
            fila.CreateCell(col++).SetCellValue(1);
            fila.CreateCell(col++).SetCellValue(condicionesSetDescription ?? string.Empty);
        }
        private void InsertRowFittingFicticio(ISheet hoja, int filaActual, Set set, SetDescription setDescription, Article article, string condiciones, Dictionary<string, Fitting> fittingDict)
        {
            IRow fila = hoja.CreateRow(filaActual);
            int col = 0;

            if (showSetId)
                fila.CreateCell(col++).SetCellValue(set.Id);

            fila.CreateCell(col++).SetCellValue(set.Code);

            if (showSetDescriptionId)
                fila.CreateCell(col++).SetCellValue(setDescription.Id);

            fila.CreateCell(col++).SetCellValue(setDescription.MinHeight);
            fila.CreateCell(col++).SetCellValue(setDescription.MaxHeight);
            fila.CreateCell(col++).SetCellValue(setDescription.MinWidth);
            fila.CreateCell(col++).SetCellValue(setDescription.MaxWidth);

            if (showFittingId)
                fila.CreateCell(col++).SetCellValue(setDescription.FittingId);

            fittingDict.TryGetValue(article.Ref.ToString(), out var fitting);

            fila.CreateCell(col++).SetCellValue(fitting?.Description ?? string.Empty);
            fila.CreateCell(col++).SetCellValue(GetFinalReferenceColor(article.Ref));

            if (showSetDescriptionPosition)
                fila.CreateCell(col++).SetCellValue(setDescription.Position);

            if (showFittingLength)
                fila.CreateCell(col++).SetCellValue(fitting?.Lenght ?? 0);

            fila.CreateCell(col++).SetCellValue(GetColour(article.Ref));
            fila.CreateCell(col++).SetCellValue(1);
            fila.CreateCell(col++).SetCellValue(condiciones ?? string.Empty);
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

            if (showSetId)
                filaCabecera.CreateCell(col++).SetCellValue("Set Id");

            filaCabecera.CreateCell(col++).SetCellValue("Tabla");

            if (showSetDescriptionId)
                filaCabecera.CreateCell(col++).SetCellValue("SetDescription Id");

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
            var baseOptions = GetOptionsString(setDescription);

            if (article?.OptionList == null || article.OptionList.Count == 0)
                return baseOptions;

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(baseOptions))
            {
                sb.Append(baseOptions).Append(" y ");
            }

            foreach (Option option in article.OptionList)
            {
                sb.Append("(")
                  .Append(option.Name)
                  .Append(" = ")
                  .Append(option.Value)
                  .Append(") y ");
            }

            sb.Length -= 3; // quitar último " y "
            return sb.ToString();
        }
        private string GetOptionsString(SetDescription setDescription)
        {
            if (setDescription?.OptionList == null || setDescription.OptionList.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (Option option in setDescription.OptionList)
            {
                sb.Append("(")
                  .Append(option.Name)
                  .Append(" = ")
                  .Append(option.Value)
                  .Append(") y ");
            }

            sb.Length -= 3; // quitar último " y "
            return sb.ToString();
        }
        private bool GenerateRowCheckingConditions(string condiciones)
        {
            bool generateRowPerfil = true;
            bool generateRowSistema = true;

            //Si no hay filtro por perfil se genera siempre la linea
            if (string.IsNullOrEmpty(cmb_Perfil.Text) && string.IsNullOrEmpty(cmb_Sistema.Text) && !_filtroListaPerfilesActivo)
            {
                return true;
            }
            else
            {
                if (condiciones.Contains("1PERFIL = "))
                {
                    var perfilesSeleccionados = new List<string>();

                    if (_perfilesListSelected != null && _perfilesListSelected.Any())
                        perfilesSeleccionados = _perfilesListSelected
                                                .Select(p => p.Valor)
                                                .ToList();
                    else if (!string.IsNullOrWhiteSpace(cmb_Perfil.Text))
                        perfilesSeleccionados.Add(cmb_Perfil.Text);

                    generateRowPerfil = perfilesSeleccionados.Any(p => condiciones.Contains("1PERFIL = " + p));

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

            if (showSetId)
                hoja.SetColumnWidth(col++, 20 * 256); // Set Id

            hoja.SetColumnWidth(col++, 45 * 256); // Código set

            if (showSetDescriptionId)
                hoja.SetColumnWidth(col++, 20 * 256); // SetDescriptionId

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
        private void DarFormatoTabla(XSSFSheet hoja, int totalFilas, int totalColumnas)
        {
            if (totalFilas < 1) return;

            // Crear tabla
            XSSFTable table = hoja.CreateTable();

            string tableName = $"Tabla_{hoja.SheetName.Replace(" ", "")}";

            table.Name = tableName;
            table.DisplayName = tableName;

            // Definir rango
            var startCell = new NPOI.SS.Util.CellReference(0, 0);
            var endCell = new NPOI.SS.Util.CellReference(totalFilas, totalColumnas - 1);
            string areaRef = $"{startCell.FormatAsString()}:{endCell.FormatAsString()}";

            CT_Table ctTable = table.GetCTTable();
            ctTable.@ref = areaRef;
            ctTable.id = (uint)(hoja.Workbook.GetSheetIndex(hoja) + 1);
            ctTable.headerRowCount = 1;

            // Autofiltro
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
                string colName = headerRow?.GetCell(i)?.ToString();

                if (string.IsNullOrWhiteSpace(colName))
                    colName = $"Col{i + 1}";

                ctTable.tableColumns.tableColumn.Add(
                    new NPOI.OpenXmlFormats.Spreadsheet.CT_TableColumn
                    {
                        id = (uint)(i + 1),
                        name = colName
                    });
            }

            // Estilo visual
            ctTable.tableStyleInfo = new NPOI.OpenXmlFormats.Spreadsheet.CT_TableStyleInfo
            {
                name = "TableStyleMedium2",
                showColumnStripes = false,
                showRowStripes = true,
                showFirstColumn = false,
                showLastColumn = false
            };
        }
        public void ReabrirYAplicarFormatoTabla(string rutaFichero, Dictionary<string, (int totalFilas, int totalColumnas)> configuracionTablas)
        {
            // Abrir fichero generado previamente
            using (FileStream fs = new FileStream(rutaFichero, FileMode.Open, FileAccess.Read))
            {
                XSSFWorkbook workbook = new XSSFWorkbook(fs);

                foreach (var kvp in configuracionTablas)
                {
                    string nombreHoja = kvp.Key;
                    int totalFilas = kvp.Value.totalFilas;
                    int totalColumnas = kvp.Value.totalColumnas;

                    XSSFSheet hoja = workbook.GetSheet(nombreHoja) as XSSFSheet;

                    if (hoja == null)
                        continue;

                    if (totalFilas <= 0 || totalColumnas <= 0)
                        continue;

                    DarFormatoTabla(hoja, totalFilas, totalColumnas);
                }

                // Guardar cambios
                using (FileStream fsFinal = new FileStream(rutaFichero, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fsFinal);
                }
            }
        }
        #endregion

    }
}
