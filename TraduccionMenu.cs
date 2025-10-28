using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using RotoEntities;
using System.Xml;

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
            ISheet hojaFittingGroups = workbook.CreateSheet("FittingGroups v" +xmlOrigen.FittingGroupVersion);

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
                FillOptionsSheet(colOptions, option, option.Name, filaOption);

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
        #endregion






    }
}
