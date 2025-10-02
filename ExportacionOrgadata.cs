using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
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
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ExportacionOrgadata : Form
    {
        #region PRIVATE PROPERTIES
        private XmlData ExportDataXml { get; set; }
        
        private List<string> virtualItemsAdded = new List<string>();

        #endregion

        #region CONSTRUCTORS
        public ExportacionOrgadata()
        {
            InitializeComponent();
        }
        public ExportacionOrgadata(XmlData xmlData)
        {
            InitializeComponent();
            this.ExportDataXml = xmlData;
        }
        #endregion

        #region EVENTS

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
                    MessageBox.Show("Exportación a Orgadata completada.");
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
        private void OrgadataExportForm_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            chkList_Sets.Items.Clear();
            LoadColours();
            LoadProfiles();
            LoadSistemas();
            LoadSets("");
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
                List<RotoEntities.Value> valueList = ExportDataXml.OptionList.Where(o => o.Name == "1SISTEMA").FirstOrDefault().ValuesList.OrderBy(v => v.Valor).ToList();
                valueList.Insert(0, new RotoEntities.Value { Valor = "" });

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
            List<RotoEntities.Value> valueList = ExportDataXml.OptionList.Where(o => o.Name == "1PERFIL").FirstOrDefault().ValuesList.OrderBy(v => v.Valor).ToList();
            valueList.Insert(0, new RotoEntities.Value { Valor = "" });

            cmb_Perfil.DataSource = valueList;
            cmb_Perfil.DisplayMember = "Valor";
        }
        private bool GenerarExportacion(string excelPath)
        {
            try
            {
                EnableControls(false);
                XSSFWorkbook workbook = new XSSFWorkbook();
                ISheet hojaKitList = workbook.CreateSheet("Kit List");
                ISheet hojaMachinings = workbook.CreateSheet("Machinings");
                ISheet hojaKitDescriptions = workbook.CreateSheet("Kit Descriptions");
                ISheet hojaItemList = workbook.CreateSheet("Item List");

                List<string> virtualKitDescriptionInSetAddedList = new List<string>();

                CreateHeaderKitList(hojaKitList);
                CreateHeaderMachinings(hojaMachinings);
                CreateHeaderKitDescriptions(hojaKitDescriptions);
                CreateHeaderItemList(hojaItemList);

                int totalFilas = 0;
                foreach (Set set in chkList_Sets.CheckedItems.OfType<Set>())
                {
                    totalFilas += set.SetDescriptionList?.Count ?? 0;
                }

                totalFilas += ExportDataXml.FittingList.Count;

                progress_Export.Value = 0;
                progress_Export.Maximum = totalFilas > 0 ? totalFilas : 1; // Evitar división por cero

                int filaActualKitList = 1;
                int filaActualMachining = 1;
                int filaActualKitDescriptions = 1;
                int filaActualItemList = 1;

                //Rellenar Kit List, Machinings y Kit Descriptions
                foreach (var itemListChecked in chkList_Sets.CheckedItems)
                {
                    Set set = itemListChecked as Set;
                    if (set != null && set.SetDescriptionList != null)
                    {
                        virtualKitDescriptionInSetAddedList.Clear();

                        IRow fila = hojaKitList.CreateRow(filaActualKitList++);
                        int col = 0;
                        FillKitListSheet(col, set, fila);                       

                        foreach (SetDescription setDescription in set.SetDescriptionList)
                        {
                            if (EsFittingFicticio(setDescription.Fitting))
                            {
                                foreach (Article article in setDescription.Fitting.ArticleList)
                                {
                                    if (EsFittingDefinidoConColor(setDescription.Fitting))
                                    {
                                        foreach (Colour color in ExportDataXml.ColourList)
                                        {
                                            foreach (Article articleColor in color.ArticleList.Where(a => a.Ref == setDescription.Fitting?.Ref))
                                            {
                                                if (!virtualKitDescriptionInSetAddedList.Contains(articleColor.Final))
                                                {
                                                    IRow filaKitDescriptions = hojaKitDescriptions.CreateRow(filaActualKitDescriptions++);
                                                    int colKitDescriptions = 0;
                                                    FillKitDescriptions(set, setDescription, filaKitDescriptions, colKitDescriptions, articleColor.Final, setDescription.Fitting.Description + " -> " + color.Name, color.Name, "Yes");

                                                    virtualKitDescriptionInSetAddedList.Add(articleColor.Final);

                                                    if (setDescription.Fitting.OperationList.Any())
                                                    {
                                                        foreach (Operation operation in setDescription.Fitting.OperationList)
                                                        {
                                                            IRow filaMachining = hojaMachinings.CreateRow(filaActualMachining++);
                                                            int colMachining = 0;
                                                            FillMachiningSheet(set, setDescription, filaMachining, colMachining, articleColor.Final, operation.Name, operation.XPosition, operation.Name.ToUpper() == "SCREW_NO_KP" ? "SCREW" : "MACHINING");
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!virtualKitDescriptionInSetAddedList.Contains(article.Ref))
                                        {
                                            IRow filaKitDescriptions = hojaKitDescriptions.CreateRow(filaActualKitDescriptions++);
                                            int colKitDescriptions = 0;
                                            FillKitDescriptions(set, setDescription, filaKitDescriptions, colKitDescriptions, article.Ref, ExportDataXml.FittingList.FirstOrDefault(f => f.Ref == article.Ref.ToString())?.Description, "", "");

                                            virtualKitDescriptionInSetAddedList.Add(article.Ref);

                                            if (setDescription.Fitting.OperationList.Any())
                                            {
                                                foreach (Operation operation in setDescription.Fitting.OperationList)
                                                {
                                                    IRow filaMachining = hojaMachinings.CreateRow(filaActualMachining++);
                                                    int colMachining = 0;
                                                    FillMachiningSheet(set, setDescription, filaMachining, colMachining, article.Ref, operation.Name, operation.XPosition, operation.Name.ToUpper() == "SCREW_NO_KP" ? "SCREW" : "MACHINING");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (EsFittingDefinidoConColor(setDescription.Fitting))
                                {
                                    foreach  (Colour color in ExportDataXml.ColourList)
                                    {
                                        foreach (Article article in color.ArticleList.Where(a => a.Ref == setDescription.Fitting?.Ref))
                                        {
                                            if (!virtualKitDescriptionInSetAddedList.Contains(article.Final))
                                            {
                                                IRow filaKitDescriptions = hojaKitDescriptions.CreateRow(filaActualKitDescriptions++);
                                                int colKitDescriptions = 0;
                                                FillKitDescriptions(set, setDescription, filaKitDescriptions, colKitDescriptions, article.Final, setDescription.Fitting?.Description + " -> " + color.Name, color.Name, "Yes");

                                                virtualKitDescriptionInSetAddedList.Add(article.Final);

                                                if (setDescription.Fitting.OperationList.Any())
                                                {
                                                    foreach (Operation operation in setDescription.Fitting.OperationList)
                                                    {
                                                        IRow filaMachining = hojaMachinings.CreateRow(filaActualMachining++);
                                                        int colMachining = 0;
                                                        FillMachiningSheet(set, setDescription, filaMachining, colMachining, article.Final, operation.Name, operation.XPosition, operation.Name.ToUpper() == "SCREW_NO_KP" ? "SCREW" : "MACHINING");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {


                                    if (!virtualKitDescriptionInSetAddedList.Contains(setDescription.Fitting.Ref))
                                    {
                                        IRow filaKitDescriptions = hojaKitDescriptions.CreateRow(filaActualKitDescriptions++);
                                        int colKitDescriptions = 0;
                                        FillKitDescriptions(set, setDescription, filaKitDescriptions, colKitDescriptions, setDescription.Fitting.Ref, setDescription.Fitting.Description, "", "");

                                        virtualKitDescriptionInSetAddedList.Add(setDescription.Fitting.Ref);

                                        if (setDescription.Fitting.OperationList.Any())
                                        {
                                            foreach (Operation operation in setDescription.Fitting.OperationList)
                                            {
                                                IRow filaMachining = hojaMachinings.CreateRow(filaActualMachining++);
                                                int colMachining = 0;
                                                FillMachiningSheet(set, setDescription, filaMachining, colMachining, setDescription.Fitting.Ref, operation.Name, operation.XPosition, operation.Name.ToUpper() == "SCREW_NO_KP" ? "SCREW" : "MACHINING");
                                            }
                                        }
                                    }
                                }
                            }

                            // Actualizar progreso
                            progress_Export.Value++;
                            progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido
                        }
                    }
                }

                virtualItemsAdded.Clear();
                //Rellenar Hoja ItemList
                if (ExportDataXml.FittingList != null && ExportDataXml.FittingList.Any())
                {
                    foreach (Fitting fitting in ExportDataXml.FittingList)
                    {
                        if (EsFittingFicticio(fitting))
                        {
                            foreach (Article article in fitting.ArticleList)
                            {
                                if (EsFittingDefinidoConColor(fitting))
                                {
                                    foreach (Colour color in ExportDataXml.ColourList)
                                    {
                                        foreach (Article articleColor in color.ArticleList.Where(a => a.Ref == fitting.Ref))
                                        {
                                            if (!virtualItemsAdded.Contains(articleColor.Final))
                                            {
                                                IRow row = hojaItemList.CreateRow(filaActualItemList++);
                                                int colItem = 0;
                                                FillRowItemList(colItem, row, articleColor.Final, ExportDataXml.FittingList.Where(f => f.Ref == article.Ref).FirstOrDefault().Description, ExportDataXml.FittingList.Where(f => f.Ref == article.Ref).FirstOrDefault().FittingGroupId, "");
                                                virtualItemsAdded.Add(articleColor.Final);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!virtualItemsAdded.Contains(article.Ref))
                                    {
                                        IRow row = hojaItemList.CreateRow(filaActualItemList++);
                                        int colItem = 0;
                                        FillRowItemList(colItem, row, article.Ref, ExportDataXml.FittingList.Where(f => f.Ref == article.Ref).FirstOrDefault().Description, ExportDataXml.FittingList.Where(f => f.Ref == article.Ref).FirstOrDefault().FittingGroupId, "");
                                        virtualItemsAdded.Add(article.Ref);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!virtualItemsAdded.Contains(fitting.Ref))
                            {
                                IRow row = hojaItemList.CreateRow(filaActualItemList++);
                                int colItem = 0;
                                FillRowItemList(colItem, row, fitting.Ref, fitting.Description, fitting.FittingGroupId, "");
                                virtualItemsAdded.Add(fitting.Ref);
                            }

                        }

                        // Actualizar progreso
                        progress_Export.Value++;
                        progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido
                    }
                }

                //Ajustar ancho de columnas en hoja Kit List
                SetColumnsWidthKitList(hojaKitList);

                //Ajustar ancho de columnas en hoja Machinings
                SetColumnsWidthMachinings(hojaMachinings);

                //Ajustar ancho de columnas en hoja KitDescriptions
                SetColumnsWidthKitDescriptions(hojaKitDescriptions);

                //Ajustar ancho de columnas en hoja KitDescriptions
                SetColumnsWidthItemList(hojaItemList);

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
                progress_Export.Value = 0;
                return false;
            }
        }
        private void FillMachiningSheet(Set set, SetDescription setDescription, IRow filaMachining, int colMachining, string reference, string? description, string machiningPosition, string itemType)
        {
            filaMachining.CreateCell(colMachining++).SetCellValue(itemType);
            filaMachining.CreateCell(colMachining++).SetCellValue(set.Id);
            filaMachining.CreateCell(colMachining++).SetCellValue(set.Code);
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");

            filaMachining.CreateCell(colMachining++).SetCellValue(setDescription.MinWidth);
            filaMachining.CreateCell(colMachining++).SetCellValue(setDescription.MaxWidth);
            filaMachining.CreateCell(colMachining++).SetCellValue(setDescription.MinHeight);
            filaMachining.CreateCell(colMachining++).SetCellValue(setDescription.MaxHeight);
            filaMachining.CreateCell(colMachining++).SetCellValue("");

            filaMachining.CreateCell(colMachining++).SetCellValue(reference);
            filaMachining.CreateCell(colMachining++).SetCellValue(description);
            filaMachining.CreateCell(colMachining++).SetCellValue(GetProfilePosition(setDescription.Position));
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue(machiningPosition);

            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
        }
        private void FillKitDescriptions(Set set, SetDescription setDescription, IRow filaMachining, int colMachining, string reference, string? description, string colorName, string visible)
        {
            filaMachining.CreateCell(colMachining++).SetCellValue("Item");
            filaMachining.CreateCell(colMachining++).SetCellValue(set.Id);
            filaMachining.CreateCell(colMachining++).SetCellValue(set.Code);
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue(setDescription.MinWidth);
            filaMachining.CreateCell(colMachining++).SetCellValue(setDescription.MaxWidth);
            filaMachining.CreateCell(colMachining++).SetCellValue(setDescription.MinHeight);
            filaMachining.CreateCell(colMachining++).SetCellValue(setDescription.MaxHeight);
            filaMachining.CreateCell(colMachining++).SetCellValue(1);
            filaMachining.CreateCell(colMachining++).SetCellValue(reference);
            filaMachining.CreateCell(colMachining++).SetCellValue(description);
            filaMachining.CreateCell(colMachining++).SetCellValue(visible);
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue(colorName);
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
            filaMachining.CreateCell(colMachining++).SetCellValue("");
        }
        private void FillKitListSheet(int col, Set set, IRow fila)
        {
            fila.CreateCell(col++).SetCellValue(set.Id);
            fila.CreateCell(col++).SetCellValue(set.Code);
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue(set.MinWidth);
            fila.CreateCell(col++).SetCellValue(set.MaxWidth);
            fila.CreateCell(col++).SetCellValue(set.MinHeight);
            fila.CreateCell(col++).SetCellValue(set.MaxHeight);
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("KS");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("Sash Rebate");
            fila.CreateCell(col++).SetCellValue(GetHardwareType(set.Opening));
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("Rectangle");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue(GetHandlePosition(set));
        }
        private void FillRowItemList(int col, IRow fila, string itemReference, string itemDescription, int? discountGroup, string color)
        {
            fila.CreateCell(col++).SetCellValue(itemReference);
            fila.CreateCell(col++).SetCellValue(itemDescription);
            fila.CreateCell(col++).SetCellValue(discountGroup.Value);
            fila.CreateCell(col++).SetCellValue(1);
            fila.CreateCell(col++).SetCellValue(0);

            fila.CreateCell(col++).SetCellValue(0);
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("EUR");
            fila.CreateCell(col++).SetCellValue("");
            fila.CreateCell(col++).SetCellValue("");
            
        }
        private string GetHardwareType(Opening opening)
        {
            string hardwareType = String.Empty;
                switch (opening.openingType)
                {
                    case (int)enumOpeningType.PracticableIzquierdaInt:
                        hardwareType = "D";
                    break;
                    case (int)enumOpeningType.PracticableDerechaInt:
                        hardwareType = "D";
                    break;
                    case (int)enumOpeningType.OscilobatienteIzquierdaInt:
                        hardwareType = "DK";
                    break;
                    case (int)enumOpeningType.OscilobatienteDerechaInt:
                        hardwareType = "DK";
                    break;
                    case (int)enumOpeningType.CorrederaDerecha:
                        hardwareType = "ST A RECHTS";
                    break;
                    case (int)enumOpeningType.CorrederaIzquierda:
                        hardwareType = "ST A LINKS";
                    break;
                    case (int)enumOpeningType.CorrederaIzqDcha:
                        hardwareType = "ST A LINKS - ST A RECHTS";
                    break;
                    case (int)enumOpeningType.Abatible:
                        hardwareType = "KLAA";
                    break;
                    case (int)enumOpeningType.OsciloCorrederaDerecha:
                        hardwareType = "SK";
                    break;
                    case (int)enumOpeningType.OsciloCorrederaIzquierda:
                        hardwareType = "SK";
                    break;
                    case (int)enumOpeningType.ElevableIzquierda:
                        hardwareType = "HST A LINKS";
                    break;
                    case (int)enumOpeningType.ElevableDerecha:
                        hardwareType = "HST A RECHTS";
                    break;
                    case (int)enumOpeningType.PracticableIzquierdaExt:
                        hardwareType = "DAB";
                    break;
                    case (int)enumOpeningType.PracticableDerechaExt:
                        hardwareType = "DAB";
                    break;
                    default:
                    hardwareType = "DK";
                        break;
                }
            return hardwareType;            
        }
        private string GetHandlePosition(Set set)
        {
            string handlePosition = String.Empty;
            switch (set.Opening.openingType)
            {
                case (int)enumOpeningType.PracticableIzquierdaInt:
                case (int)enumOpeningType.PracticableDerechaInt:
                case (int)enumOpeningType.OscilobatienteIzquierdaInt:
                case (int)enumOpeningType.OscilobatienteDerechaInt:
                case (int)enumOpeningType.CorrederaDerecha:
                case (int)enumOpeningType.CorrederaIzquierda:
                case (int)enumOpeningType.CorrederaIzqDcha:
                case (int)enumOpeningType.OsciloCorrederaDerecha:
                case (int)enumOpeningType.OsciloCorrederaIzquierda:
                case (int)enumOpeningType.ElevableIzquierda:
                case (int)enumOpeningType.ElevableDerecha:
                case (int)enumOpeningType.PracticableIzquierdaExt:
                case (int)enumOpeningType.PracticableDerechaExt:
                    if (EsAperturaPasiva(set))
                    {
                        handlePosition = "None";
                    }
                    else
                    {
                        handlePosition = "Side";
                    }
                    break;
                case (int)enumOpeningType.Abatible:
                    handlePosition = "Top";
                    break;
                default:
                    handlePosition = "Side";
                    break;
            }
            return handlePosition;
        }
        private string GetOpeningDirection(Set set)
        {
            string openingDirection = String.Empty;
            switch (set.Opening.openingType)
            {
                case (int)enumOpeningType.PracticableIzquierdaInt:
                case (int)enumOpeningType.OscilobatienteIzquierdaInt:
                case (int)enumOpeningType.CorrederaIzquierda:
                case (int)enumOpeningType.OsciloCorrederaIzquierda:
                case (int)enumOpeningType.ElevableIzquierda:
                case (int)enumOpeningType.PracticableIzquierdaExt:
                    openingDirection = "LEFT";
                    break;
                case (int)enumOpeningType.PracticableDerechaInt:
                case (int)enumOpeningType.OscilobatienteDerechaInt:
                case (int)enumOpeningType.CorrederaDerecha:
                case (int)enumOpeningType.OsciloCorrederaDerecha:
                case (int)enumOpeningType.ElevableDerecha:
                case (int)enumOpeningType.PracticableDerechaExt:
                    openingDirection = "RIGHT";
                    break;
                case (int)enumOpeningType.CorrederaIzqDcha:
                case (int)enumOpeningType.Abatible:
                    openingDirection = "";
                    break;
                default:
                    openingDirection = "";
                    break;
            }
            return openingDirection;
        }
        private string GetProfilePosition(int position)
        {
            string profilePosition = String.Empty;

            switch (position)
            {
                case 1:
                    profilePosition = "Handle";
                    break;
                case 2:
                    profilePosition = "Top";
                    break;
                case 3:
                    profilePosition = "Hinge";
                    break;
                case 4:
                    profilePosition = "Bottom";
                    break;
                default:
                    profilePosition = "Handle";
                    break;
            }

            return profilePosition;
        }
        private bool EsAperturaPasiva(Set set)
        {
            if (set.Code.ToUpper().StartsWith("(1V)2P") 
                || set.Code.ToUpper().StartsWith("(2V)2P") 
                || set.Code.ToUpper().StartsWith("(1)2P")
                || set.Code.ToUpper().StartsWith("(2)2P")
                ||(set.Code.StartsWith("(2V") && set.Code.ToUpper().Contains("ALV") && set.Code.ToUpper().Contains("2P"))
                )
            {
                return true;
            }

            return false;
        }
        private bool EsFittingFicticio(Fitting fitting)
        {
            if (fitting != null && fitting.Ref.ToString().StartsWith("PROGRAM"))
            {
                return true;
            }

            return false;
        }
        private bool EsFittingDefinidoConColor(Fitting fitting)
        {
            if (ExportDataXml.ColourList != null && ExportDataXml.ColourList.Any(c => c.ArticleList.Any(a => a.Ref == fitting.Ref)))
            {
                return true;
            }

            return false;
        }
        private void CreateHeaderKitList(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Kit Name");
            filaCabecera.CreateCell(col++).SetCellValue("Kit Description");
            filaCabecera.CreateCell(col++).SetCellValue("");
            filaCabecera.CreateCell(col++).SetCellValue("SRW min");
            filaCabecera.CreateCell(col++).SetCellValue("SRW max");

            filaCabecera.CreateCell(col++).SetCellValue("SRH min");
            filaCabecera.CreateCell(col++).SetCellValue("SRH max");
            filaCabecera.CreateCell(col++).SetCellValue("Profile Supplier");
            filaCabecera.CreateCell(col++).SetCellValue("Profile System");
            filaCabecera.CreateCell(col++).SetCellValue("Hardware groove type");

            filaCabecera.CreateCell(col++).SetCellValue("Burglar Resistance");
            filaCabecera.CreateCell(col++).SetCellValue("Dimension Reference");
            filaCabecera.CreateCell(col++).SetCellValue("Hardware type");
            filaCabecera.CreateCell(col++).SetCellValue("Corresponding Side-Hung Hardware");
            filaCabecera.CreateCell(col++).SetCellValue("Corresponding turn-tilt hardware");

            filaCabecera.CreateCell(col++).SetCellValue("Corresponding slide-tilt hardware");
            filaCabecera.CreateCell(col++).SetCellValue("Corresponding tilt-before-turn hardware");
            filaCabecera.CreateCell(col++).SetCellValue("Shape");
            filaCabecera.CreateCell(col++).SetCellValue("Info1");
            filaCabecera.CreateCell(col++).SetCellValue("Info2");

            filaCabecera.CreateCell(col++).SetCellValue("Only for Stile");
            filaCabecera.CreateCell(col++).SetCellValue("Easy Access");
            filaCabecera.CreateCell(col++).SetCellValue("Hinge side type");
            filaCabecera.CreateCell(col++).SetCellValue("Handle position");

        }
        private void CreateHeaderMachinings(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Type");
            filaCabecera.CreateCell(col++).SetCellValue("Kit Name");
            filaCabecera.CreateCell(col++).SetCellValue("Kit Description");
            filaCabecera.CreateCell(col++).SetCellValue("");
            filaCabecera.CreateCell(col++).SetCellValue("Opening direction");
            
            filaCabecera.CreateCell(col++).SetCellValue("SRW from");
            filaCabecera.CreateCell(col++).SetCellValue("SRW to");
            filaCabecera.CreateCell(col++).SetCellValue("SRH from");
            filaCabecera.CreateCell(col++).SetCellValue("SRH to");
            filaCabecera.CreateCell(col++).SetCellValue("");

            filaCabecera.CreateCell(col++).SetCellValue("Item no");
            filaCabecera.CreateCell(col++).SetCellValue("Description");
            filaCabecera.CreateCell(col++).SetCellValue("Profile position");
            filaCabecera.CreateCell(col++).SetCellValue("");
            filaCabecera.CreateCell(col++).SetCellValue("Machining position");
            
            filaCabecera.CreateCell(col++).SetCellValue("min. weight");
            filaCabecera.CreateCell(col++).SetCellValue("max. weight");            
            filaCabecera.CreateCell(col++).SetCellValue("Profile Supplier");
            filaCabecera.CreateCell(col++).SetCellValue("Profile System");
            filaCabecera.CreateCell(col++).SetCellValue("Selection Groups 1-8");
        }
        private void CreateHeaderKitDescriptions(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Type");
            filaCabecera.CreateCell(col++).SetCellValue("Kit Name");
            filaCabecera.CreateCell(col++).SetCellValue("Kit Description");
            filaCabecera.CreateCell(col++).SetCellValue("");
            filaCabecera.CreateCell(col++).SetCellValue("Opening direction");

            filaCabecera.CreateCell(col++).SetCellValue("SRW from");
            filaCabecera.CreateCell(col++).SetCellValue("SRW to");
            filaCabecera.CreateCell(col++).SetCellValue("SRH from");
            filaCabecera.CreateCell(col++).SetCellValue("SRH to");
            filaCabecera.CreateCell(col++).SetCellValue("Number");

            filaCabecera.CreateCell(col++).SetCellValue("Item Number");
            filaCabecera.CreateCell(col++).SetCellValue("Description");
            filaCabecera.CreateCell(col++).SetCellValue("Visible");
            filaCabecera.CreateCell(col++).SetCellValue("");
            filaCabecera.CreateCell(col++).SetCellValue("Hardware colour");

            filaCabecera.CreateCell(col++).SetCellValue("Handle position");
            filaCabecera.CreateCell(col++).SetCellValue("");
            filaCabecera.CreateCell(col++).SetCellValue("Location definition");
            filaCabecera.CreateCell(col++).SetCellValue("Component install location");
            filaCabecera.CreateCell(col++).SetCellValue("Order of installation");

            filaCabecera.CreateCell(col++).SetCellValue("Relative dimensions");
            filaCabecera.CreateCell(col++).SetCellValue("min. weight");
            filaCabecera.CreateCell(col++).SetCellValue("max. weight");
            filaCabecera.CreateCell(col++).SetCellValue("Profile Supplier");
            filaCabecera.CreateCell(col++).SetCellValue("Profile System");

            filaCabecera.CreateCell(col++).SetCellValue("Selection Groups 1-8");
        }
        private void CreateHeaderItemList(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Item number");
            filaCabecera.CreateCell(col++).SetCellValue("Description");
            filaCabecera.CreateCell(col++).SetCellValue("Discount group");
            filaCabecera.CreateCell(col++).SetCellValue("Packaging unit");
            filaCabecera.CreateCell(col++).SetCellValue("Price per packaging unit");

            filaCabecera.CreateCell(col++).SetCellValue("Price each item");
            filaCabecera.CreateCell(col++).SetCellValue("Crosscut part");
            filaCabecera.CreateCell(col++).SetCellValue("Currency");
            filaCabecera.CreateCell(col++).SetCellValue("Visible");
            filaCabecera.CreateCell(col++).SetCellValue("Hardware colour");

        }
        private void SetColumnsWidthKitList(ISheet hoja)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hoja.SetColumnWidth(col++, 15 * 256); // Kit Name
            hoja.SetColumnWidth(col++, 60 * 256); // Kit Description
            hoja.SetColumnWidth(col++, 5 * 256); // Empty
            hoja.SetColumnWidth(col++, 10 * 256); // MinWidth                                                  
            hoja.SetColumnWidth(col++, 10 * 256); // MaxWidth

            hoja.SetColumnWidth(col++, 10 * 256); // MinHeight 
            hoja.SetColumnWidth(col++, 10 * 256); // MaxHeight 
            hoja.SetColumnWidth(col++, 15 * 256); // Profile Supplier
            hoja.SetColumnWidth(col++, 15 * 256); // Profile System
            hoja.SetColumnWidth(col++, 20 * 256); // Hardware groove type

            hoja.SetColumnWidth(col++, 15 * 256); // Burglar Resistance
            hoja.SetColumnWidth(col++, 15 * 256); // Dimension Reference
            hoja.SetColumnWidth(col++, 15 * 256); // Hardware type
            hoja.SetColumnWidth(col++, 30 * 256); // Corresponding Side-Hung Hardware
            hoja.SetColumnWidth(col++, 30 * 256); // Corresponding turn-tilt hardware

            hoja.SetColumnWidth(col++, 30 * 256); // Corresponding slide-tilt hardware
            hoja.SetColumnWidth(col++, 30 * 256); // Corresponding tilt-before-turn hardware
            hoja.SetColumnWidth(col++, 15 * 256); // Shape
            hoja.SetColumnWidth(col++, 5 * 256); // Info1
            hoja.SetColumnWidth(col++, 5 * 256); // Info2

            hoja.SetColumnWidth(col++, 10 * 256); // Only for Stile
            hoja.SetColumnWidth(col++, 10 * 256); // Easy Access
            hoja.SetColumnWidth(col++, 10 * 256); // Hinge side type
            hoja.SetColumnWidth(col++, 15 * 256); // Handle position
        }
        private void SetColumnsWidthMachinings(ISheet hoja)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hoja.SetColumnWidth(col++, 15 * 256);   // Type
            hoja.SetColumnWidth(col++, 15 * 256);   // Kit Name
            hoja.SetColumnWidth(col++, 60 * 256);   // Kit Description
            hoja.SetColumnWidth(col++, 5 * 256);    // Empty                                                   
            hoja.SetColumnWidth(col++, 10 * 256);   // Opening direction 

            hoja.SetColumnWidth(col++, 10 * 256);   // SRW from  
            hoja.SetColumnWidth(col++, 10 * 256);   // SRW to  
            hoja.SetColumnWidth(col++, 10 * 256);   // SRH from 
            hoja.SetColumnWidth(col++, 10 * 256);   // SRH to
            hoja.SetColumnWidth(col++, 15 * 256);   // Item no

            hoja.SetColumnWidth(col++, 60 * 256);   // Description
            hoja.SetColumnWidth(col++, 15 * 256);   // Profile position
            hoja.SetColumnWidth(col++, 15 * 256);   // Machining position
            hoja.SetColumnWidth(col++, 15 * 256);   // min. weight
            hoja.SetColumnWidth(col++, 15 * 256);   // max. weight

            hoja.SetColumnWidth(col++, 15 * 256);   // Profile Supplier
            hoja.SetColumnWidth(col++, 15 * 256);   // Profile System
            hoja.SetColumnWidth(col++, 15 * 256);   // Selection Groups 1-8

        }
        private void SetColumnsWidthKitDescriptions(ISheet hoja)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hoja.SetColumnWidth(col++, 15 * 256);   // Type
            hoja.SetColumnWidth(col++, 15 * 256);   // Kit Name
            hoja.SetColumnWidth(col++, 60 * 256);   // Kit Description
            hoja.SetColumnWidth(col++, 5 * 256);    // Empty                                                   
            hoja.SetColumnWidth(col++, 10 * 256);   // Opening direction 

            hoja.SetColumnWidth(col++, 10 * 256);   // SRW from  
            hoja.SetColumnWidth(col++, 10 * 256);   // SRW to  
            hoja.SetColumnWidth(col++, 10 * 256);   // SRH from 
            hoja.SetColumnWidth(col++, 10 * 256);   // SRH to
            hoja.SetColumnWidth(col++, 10 * 256);   // Number

            hoja.SetColumnWidth(col++, 15 * 256);   // Item Number
            hoja.SetColumnWidth(col++, 60 * 256);   // Description
            hoja.SetColumnWidth(col++, 10 * 256);   // Visible
            hoja.SetColumnWidth(col++, 5 * 256);    // Empty
            hoja.SetColumnWidth(col++, 15 * 256);   // Hardware colour

            hoja.SetColumnWidth(col++, 10 * 256);   // Hardware position
            hoja.SetColumnWidth(col++, 5 * 256);    // Empty
            hoja.SetColumnWidth(col++, 10 * 256);   // Location definition
            hoja.SetColumnWidth(col++, 10 * 256);   // Component install location
            hoja.SetColumnWidth(col++, 10 * 256);   // Order of installation

            hoja.SetColumnWidth(col++, 10 * 256);   // Relative dimensions
            hoja.SetColumnWidth(col++, 10 * 256);   // min. weight
            hoja.SetColumnWidth(col++, 10 * 256);   // max. weight
            hoja.SetColumnWidth(col++, 10 * 256);   // Profile Supplier
            hoja.SetColumnWidth(col++, 10 * 256);   // Profile System

            hoja.SetColumnWidth(col++, 10 * 256);   // Selection Groups 1 - 8
        }
        private void SetColumnsWidthItemList(ISheet hoja)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hoja.SetColumnWidth(col++, 15 * 256);   // Item number
            hoja.SetColumnWidth(col++, 60 * 256);   // Description
            hoja.SetColumnWidth(col++, 15 * 256);   // Discount group
            hoja.SetColumnWidth(col++, 15 * 256);   // Packaging unit                                           
            hoja.SetColumnWidth(col++, 15 * 256);   // Price per packaging unit

            hoja.SetColumnWidth(col++, 15 * 256);   // SRW from  
            hoja.SetColumnWidth(col++, 15 * 256);   // SRW to  
            hoja.SetColumnWidth(col++, 15 * 256);   // SRH from 
            hoja.SetColumnWidth(col++, 15 * 256);   // SRH to
            hoja.SetColumnWidth(col++, 15 * 256);   // Number
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

        #endregion


    }
}
