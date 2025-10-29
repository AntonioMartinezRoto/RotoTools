using ClosedXML.Excel;
using iTextSharp.xmp.impl.xpath;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using RotoEntities;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Value = RotoEntities.Value;

namespace RotoTools
{
    public partial class TraduccionMenu : Form
    {
        #region Private properties

        private XmlData xmlOrigen = new();
        private bool xmlCargado = false;
        private XmlNamespaceManager nsmgr;

        #endregion

        #region Constructors

        public TraduccionMenu()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        private void btn_LoadXml_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                EnableButtons(false);
                string rutaXml = openFileDialog.FileName;
                xmlOrigen = LoadXml(rutaXml);
                lbl_Xml.Text = rutaXml;
                EnableButtons(true);
            }
        }
        private void btn_GenerarPlantillaExcel_Click(object sender, EventArgs e)
        {
            if (!xmlCargado) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo Excel (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Save as";
            saveFileDialog.FileName = "Translations.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string excelPath = saveFileDialog.FileName;
                try
                {
                    EnableButtons(false);
                    GenerateTemplate(excelPath);
                    MessageBox.Show("Plantilla generada correctamente.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    EnableButtons(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ha ocurrido un problema y no se ha podido generar la plantilla." + Environment.NewLine +
                        ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void btn_Traducir_Click(object sender, EventArgs e)
        {
            if (!xmlCargado) return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XLS Files (*.xls)|*.xlsx";
            openFileDialog.Title = "Selecciona traducción";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Cursor.Current = Cursors.WaitCursor;
                EnableButtons(false);

                TranslateXML(openFileDialog.FileName);

                EnableButtons(true);
                Cursor.Current = Cursors.Default;
            }            
        }

        private void TranslateXML(string translationFileName)
        {
            try
            {
                Traducciones translations = Helpers.CargarTraducciones(translationFileName);

                XDocument doc = XDocument.Load(lbl_Xml.Text);
                XNamespace hw = "http://www.preference.com/XMLSchemas/2006/Hardware";

                foreach (var fg in doc.Descendants(hw + "FittingGroup"))
                {
                    var attr = fg.Attribute("class");
                    if (attr != null && translations.FittingGroups.TryGetValue(attr.Value.Trim(), out string nuevo))
                        attr.Value = nuevo;
                }

                foreach (var f in doc.Descendants(hw + "Fitting"))
                {
                    var refAttr = f.Attribute("ref")?.Value.Trim();
                    var descAttr = f.Attribute("Description");

                    if (refAttr != null && descAttr != null &&
                        translations.Fittings.TryGetValue(refAttr, out string nuevaDesc))
                    {
                        descAttr.Value = nuevaDesc;
                    }
                }

                foreach (var c in doc.Descendants(hw + "Colour"))
                {
                    var attr = c.Attribute("name");
                    if (attr != null && translations.Colours.TryGetValue(attr.Value.Trim(), out string nuevo))
                        attr.Value = nuevo;
                }

                AplicarTraduccionesOptions(doc, translations, hw);

                //Guardar el XML traducido
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Archivo XML (*.xml)|*.xml";
                saveFileDialog.Title = "Save as";
                saveFileDialog.FileName = "Roto.xml";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    doc.Save(saveFileDialog.FileName);
                    MessageBox.Show("Archivo traducido correctamente.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error traduciendo el archivo." + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
        #endregion

        #region Private methods
        private void EnableButtons(bool enable)
        {
            btn_LoadXml.Enabled = enable;
            btn_GenerarPlantillaExcel.Enabled = enable;
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
                    lbl_Xml.Text = $"Cargando... {type} {value.TrimEnd()}";
                    Application.DoEvents();
                };

                XmlData xmlData = new XmlData();
                xmlData.Supplier = loader.LoadSupplier(doc);
                xmlData.HardwareType = loader.LoadHardwareType(xmlData.Supplier);
                xmlData.FittingGroupList = loader.LoadFittingGroups(doc);
                xmlData.ColourList = loader.LoadColourMaps(doc);
                xmlData.OptionList = loader.LoadDocOptions(doc);
                xmlData.FittingList = loader.LoadFittings(doc);
                xmlData.SetList = loader.LoadSets(doc, xmlData.FittingList);
                xmlData.FittingsVersion = loader.LoadFittingsVersion(doc);
                xmlData.OptionsVersion = loader.LoadOptionsVersion(doc);
                xmlData.ColoursVersion = loader.LoadColoursVersion(doc);
                xmlData.FittingGroupVersion = loader.LoadFittingGroupVersion(doc);

                xmlCargado = true;
                return xmlData;
            }
            catch
            {
                return null;
            }
        }
        private void GenerateTemplate(string excelPath)
        {
            #region Sheets definitions

            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet hojaFittings = workbook.CreateSheet("Fittings v" + xmlOrigen.FittingsVersion);
            ISheet hojaOptions = workbook.CreateSheet("Options v" + xmlOrigen.OptionsVersion);
            ISheet hojaColours = workbook.CreateSheet("Colours v" + xmlOrigen.ColoursVersion);
            ISheet hojaFittingGroups = workbook.CreateSheet("FittingGroups v" + xmlOrigen.FittingGroupVersion);

            #endregion

            #region Headers

            CreateHeaderFittings(hojaFittings);
            CreateHeaderOptions(hojaOptions);
            CreateHeaderColours(hojaColours);
            CreateHeaderFittingGroups(hojaFittingGroups);

            #endregion

            #region Fittings

            int filaActualFittings = 1;
            foreach (Fitting fitting in xmlOrigen.FittingList)
            {
                IRow fila = hojaFittings.CreateRow(filaActualFittings++);
                int colFittings = 0;
                FillFittingsSheet(colFittings, fitting, fila);
            }

            //Ajustar ancho de columnas en hoja Kit List
            SetColumnsWidthFittings(hojaFittings);

            #endregion

            #region Options

            int filaActualOptions = 1;
            foreach (Option option in xmlOrigen.OptionList)
            {
                IRow filaOption = hojaOptions.CreateRow(filaActualOptions++);
                int colOptions = 0;
                FillOptionsSheet(colOptions, option, "", filaOption);

                foreach (Value optionValue in option.ValuesList)
                {
                    IRow filaOptionValue = hojaOptions.CreateRow(filaActualOptions++);
                    int colOptionsValue = 0;
                    FillOptionsSheet(colOptionsValue, option, optionValue.Valor, filaOptionValue);
                }
            }

            //Ajustar ancho de columnas en hoja Kit List
            SetColumnsWidthOptions(hojaOptions);

            #endregion

            #region Colours

            int filaActualColours = 1;
            foreach (Colour colour in xmlOrigen.ColourList)
            {
                IRow fila = hojaColours.CreateRow(filaActualColours++);
                int colColours = 0;
                FillColoursSheet(colColours, colour, fila);
            }

            //Ajustar ancho de columnas en hoja Kit List
            SetColumnsWidthColours(hojaColours);

            #endregion

            #region FittingsGroup

            int filaActualFittingGroup = 1;
            foreach (FittingGroup fittingGroup in xmlOrigen.FittingGroupList)
            {
                IRow fila = hojaFittingGroups.CreateRow(filaActualFittingGroup++);
                int colFittingGroup = 0;
                FillFittingGroupSheet(colFittingGroup, fittingGroup, fila);
            }

            //Ajustar ancho de columnas en hoja Kit List
            SetColumnsWidthFittingGroup(hojaFittingGroups);

            #endregion

            // Guardar el archivo Excel
            using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        private void CreateHeaderFittings(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Referencia");
            filaCabecera.CreateCell(col++).SetCellValue("Descripción");
            filaCabecera.CreateCell(col++).SetCellValue("Traducción");
        }
        private void CreateHeaderOptions(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Opción");
            filaCabecera.CreateCell(col++).SetCellValue("Valor");
            filaCabecera.CreateCell(col++).SetCellValue("Traducción");
        }
        private void CreateHeaderColours(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Color");
            filaCabecera.CreateCell(col++).SetCellValue("Traducción");
        }
        private void CreateHeaderFittingGroups(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Clase");
            filaCabecera.CreateCell(col++).SetCellValue("Traducción");
        }
        private void FillFittingsSheet(int col, Fitting fitting, IRow fila)
        {
            fila.CreateCell(col++).SetCellValue(fitting.Ref);
            fila.CreateCell(col++).SetCellValue(fitting.Description);
            fila.CreateCell(col++).SetCellValue("");
        }
        private void FillOptionsSheet(int col, Option option, string optionValue, IRow fila)
        {
            fila.CreateCell(col++).SetCellValue(option.Name);
            fila.CreateCell(col++).SetCellValue(optionValue);
            fila.CreateCell(col++).SetCellValue("");
        }
        private void FillColoursSheet(int col, Colour colour, IRow fila)
        {
            fila.CreateCell(col++).SetCellValue(colour.Name);
            fila.CreateCell(col++).SetCellValue("");
        }
        private void FillFittingGroupSheet(int col, FittingGroup fittingGroup, IRow fila)
        {
            fila.CreateCell(col++).SetCellValue(fittingGroup.Class);
            fila.CreateCell(col++).SetCellValue("");
        }
        private void SetColumnsWidthFittings(ISheet hojaFittings)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hojaFittings.SetColumnWidth(col++, 20 * 256);   // Referencia
            hojaFittings.SetColumnWidth(col++, 65 * 256);   // Descripción
            hojaFittings.SetColumnWidth(col++, 65 * 256);    // Traducción
        }
        private void SetColumnsWidthOptions(ISheet hojaOptions)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hojaOptions.SetColumnWidth(col++, 30 * 256);   // Referencia
            hojaOptions.SetColumnWidth(col++, 30 * 256);   // Descripción
            hojaOptions.SetColumnWidth(col++, 30 * 256);    // Traducción
        }
        private void SetColumnsWidthColours(ISheet hojaColours)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hojaColours.SetColumnWidth(col++, 30 * 256);    // Color
            hojaColours.SetColumnWidth(col++, 30 * 256);    // Traducción
        }
        private void SetColumnsWidthFittingGroup(ISheet hojaFittingGroup)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hojaFittingGroup.SetColumnWidth(col++, 30 * 256);    // Color
            hojaFittingGroup.SetColumnWidth(col++, 30 * 256);    // Traducción
        }        
        private void AplicarTraduccionesOptions(XDocument doc, Traducciones traducciones, XNamespace hw)
        {
            // Buscar TODAS las opciones, sin importar dónde estén
            var allOptions = doc.Descendants(hw + "Option");

            foreach (var opt in allOptions)
            {
                string name = opt.Attribute("Name")?.Value?.Trim();
                string value = opt.Attribute("Value")?.Value?.Trim();

                // Si tiene un valor (formato <Option Name="..." Value="..."/>)
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (traducciones.OptionValues.TryGetValue((name, value), out string traduccion))
                    {
                        opt.SetAttributeValue("Value", traduccion);
                    }
                    // Traducir el nombre de la opción si existe traducción
                    if (traducciones.OptionNames.TryGetValue(name, out string traduccionName))
                    {
                        opt.SetAttributeValue("Name", traduccionName);
                    }
                }
                // Si no tiene Value y hay nodos <hw:Value> hijos
                else
                {
                    foreach (var val in opt.Elements(hw + "Value"))
                    {
                        string valText = val.Attribute("Value")?.Value?.Trim();
                        if (traducciones.OptionValues.TryGetValue((name, valText), out string traduccion))
                        {
                            val.SetAttributeValue("Value", traduccion);
                        }
                    }

                    // Traducir el nombre de la opción si existe traducción
                    if (traducciones.OptionNames.TryGetValue(name, out string traduccionName))
                    {
                        opt.SetAttributeValue("Name", traduccionName);
                    }
                }
            }
        }

    }
    #endregion


}

