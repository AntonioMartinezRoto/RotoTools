using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using RotoEntities;
using RotoTools.Suite.Services;
using Value = RotoEntities.Value;

namespace RotoTools.Suite.Views.Traduccion
{
    /// <summary>
    /// Sustituye a TraduccionMenu.cs/.Designer.cs (WinForms): mismo comportamiento y misma lógica
    /// de negocio, reutilizada tal cual vía ProjectReference (RotoTools.Helpers,
    /// RotoTools.XmlLoader, RotoTools.LocalizationManager). La generación del Excel (NPOI) y la
    /// aplicación de la traducción sobre el XML (System.Xml.Linq) se han portado aquí letra por
    /// letra, en vez de intentar moverlas a RotoTools.csproj, que no se debe tocar bajo ningún
    /// concepto.
    /// </summary>
    public partial class TraduccionPage : UserControl
    {
        #region Estado

        private XmlData? _xmlOrigen = new();
        private bool _xmlCargado;
        private XmlNamespaceManager? _nsmgr;

        #endregion

        public TraduccionPage()
        {
            InitializeComponent();

            CargarTextos();
        }

        #region Localización

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_Traduccion");
            TxtSubtitulo.Text = Loc("L_Suite_TraduccionSubtitulo");

            TxtBtnCargarXml.Text = Loc("L_Suite_CargarXml");
            LblXml.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarXML");

            TxtCard1Titulo.Text = RotoTools.LocalizationManager.GetString("L_TraducirXML");
            TxtCard1Desc.Text = Loc("L_Suite_TraducirXmlDesc");
            TxtCard2Titulo.Text = RotoTools.LocalizationManager.GetString("L_GenerarPlantilla");
            TxtCard2Desc.Text = Loc("L_Suite_GenerarPlantillaDesc");
        }

        #endregion

        #region Events

        private void BtnLoadXml_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml", Title = "Selecciona XML" };

            if (openFileDialog.ShowDialog() == true)
            {
                EnableButtons(false);
                string rutaXml = openFileDialog.FileName;
                _xmlOrigen = LoadXml(rutaXml);
                LblXml.Text = rutaXml;
                EnableButtons(true);
            }
        }

        private void BtnGenerarPlantillaExcel_Click(object sender, RoutedEventArgs e)
        {
            if (!_xmlCargado) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                Title = "Save as",
                FileName = "Translations.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string excelPath = saveFileDialog.FileName;
                try
                {
                    EnableButtons(false);
                    GenerateTemplate(excelPath);
                    MessageBox.Show(RotoTools.LocalizationManager.GetString("L_PlantillaGenerada"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                    EnableButtons(true);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error (14)" + System.Environment.NewLine +
                        ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnTraducir_Click(object sender, RoutedEventArgs e)
        {
            if (!_xmlCargado) return;

            var openFileDialog = new OpenFileDialog { Filter = "XLS Files (*.xls)|*.xlsx", Title = "Selecciona traducción" };

            if (openFileDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                EnableButtons(false);

                TranslateXML(openFileDialog.FileName);

                EnableButtons(true);
                Mouse.OverrideCursor = null;
            }
        }

        #endregion

        #region Private Methods

        private void TranslateXML(string translationFileName)
        {
            try
            {
                Traducciones translations = RotoTools.Helpers.CargarTraducciones(translationFileName);

                XDocument doc = XDocument.Load(LblXml.Text);
                XNamespace hw = "http://www.preference.com/XMLSchemas/2006/Hardware";

                foreach (var fg in doc.Descendants(hw + "FittingGroup"))
                {
                    var attr = fg.Attribute("class");
                    if (attr != null && translations.FittingGroups.TryGetValue(attr.Value.Trim(), out string nuevo))
                        attr.Value = nuevo;
                }

                foreach (var fg in doc.Descendants(hw + "Set"))
                {
                    var attr = fg.Attribute("code");
                    if (attr != null && translations.Sets.TryGetValue(attr.Value.Trim(), out string nuevo))
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
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivo XML (*.xml)|*.xml",
                    Title = "Save as",
                    FileName = "Roto.xml"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    doc.Save(saveFileDialog.FileName);
                    MessageBox.Show(RotoTools.LocalizationManager.GetString("L_XMLTraducidoCorrectamente"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error (13)" + System.Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>Igual que TraduccionMenu.EnableButtons: solo alterna btn_LoadXml y
        /// btn_GenerarPlantillaExcel, no btn_Traducir (así es el original, no se corrige aquí).</summary>
        private void EnableButtons(bool enable)
        {
            BtnLoadXml.IsEnabled = enable;
            BtnGenerarPlantillaExcel.IsEnabled = enable;
        }

        private XmlData? LoadXml(string xmlPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);

                _nsmgr = new XmlNamespaceManager(doc.NameTable);
                _nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

                RotoTools.XmlLoader loader = new RotoTools.XmlLoader(_nsmgr);
                // Vinculamos el evento para actualizar la etiqueta de estado de la página
                loader.OnLoadingInfo += (type, value) =>
                {
                    LblXml.Text = RotoTools.LocalizationManager.GetString("L_Cargando") + $"... {type} {value.TrimEnd()}";
                    DoEvents();
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

                _xmlCargado = true;
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
            ISheet hojaFittings = workbook.CreateSheet("Fittings v" + _xmlOrigen.FittingsVersion);
            ISheet hojaOptions = workbook.CreateSheet("Options v" + _xmlOrigen.OptionsVersion);
            ISheet hojaColours = workbook.CreateSheet("Colours v" + _xmlOrigen.ColoursVersion);
            ISheet hojaFittingGroups = workbook.CreateSheet("FittingGroups v" + _xmlOrigen.FittingGroupVersion);
            ISheet hojaSets = workbook.CreateSheet("Sets");

            #endregion

            #region Headers

            CreateHeaderFittings(hojaFittings);
            CreateHeaderOptions(hojaOptions);
            CreateHeaderColours(hojaColours);
            CreateHeaderFittingGroups(hojaFittingGroups);
            CreateHeaderSets(hojaSets);

            #endregion

            #region Fittings

            int filaActualFittings = 1;
            foreach (Fitting fitting in _xmlOrigen.FittingList)
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
            foreach (Option option in _xmlOrigen.OptionList)
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
            foreach (Colour colour in _xmlOrigen.ColourList)
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
            foreach (FittingGroup fittingGroup in _xmlOrigen.FittingGroupList)
            {
                IRow fila = hojaFittingGroups.CreateRow(filaActualFittingGroup++);
                int colFittingGroup = 0;
                FillFittingGroupSheet(colFittingGroup, fittingGroup, fila);
            }

            //Ajustar ancho de columnas en hoja Kit List
            SetColumnsWidthFittingGroup(hojaFittingGroups);

            #endregion

            #region Sets

            int filaActualSets = 1;
            foreach (Set set in _xmlOrigen.SetList)
            {
                IRow fila = hojaSets.CreateRow(filaActualSets++);
                int colSet = 0;
                FillSetSheet(colSet, set, fila);
            }

            //Ajustar ancho de columnas en hoja Kit List
            SetColumnsWidthSet(hojaSets);

            #endregion

            // Guardar el archivo Excel
            using (System.IO.FileStream fs = new System.IO.FileStream(excelPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
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

        private void CreateHeaderSets(ISheet hoja)
        {
            // Crear encabezados en la primera fila
            IRow filaCabecera = hoja.CreateRow(0);

            int col = 0;

            filaCabecera.CreateCell(col++).SetCellValue("Codigo");
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

        private void FillSetSheet(int col, Set set, IRow fila)
        {
            fila.CreateCell(col++).SetCellValue(set.Code);
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

        private void SetColumnsWidthSet(ISheet hojaSets)
        {
            // El valor es en 1/256 de unidad de carácter
            int col = 0;

            hojaSets.SetColumnWidth(col++, 60 * 256);    // Codigo
            hojaSets.SetColumnWidth(col++, 30 * 256);    // Traducción
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
                        if (string.IsNullOrEmpty(traduccion))
                        {
                            opt.SetAttributeValue("Value", value);
                        }
                        else
                        {
                            opt.SetAttributeValue("Value", traduccion);
                        }
                    }
                    // Traducir el nombre de la opción si existe traducción
                    if (traducciones.OptionNames.TryGetValue(name, out string traduccionName))
                    {
                        if (string.IsNullOrEmpty(traduccionName))
                        {
                            opt.SetAttributeValue("Name", name);
                        }
                        else
                        {
                            opt.SetAttributeValue("Name", traduccionName);
                        }
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
                            if (string.IsNullOrEmpty(traduccion))
                            {
                                val.SetAttributeValue("Value", valText);
                            }
                            else
                            {
                                val.SetAttributeValue("Value", traduccion);
                            }
                        }
                    }

                    // Traducir el nombre de la opción si existe traducción
                    if (traducciones.OptionNames.TryGetValue(name, out string traduccionName))
                    {
                        if (string.IsNullOrEmpty(traduccionName))
                        {
                            opt.SetAttributeValue("Name", name);
                        }
                        else
                        {
                            opt.SetAttributeValue("Name", traduccionName);
                        }
                    }
                }
            }
        }

        /// <summary>Equivalente WPF de Application.DoEvents() (mismo helper que
        /// ConectorHerrajePage/ManillasFKSPage/TariffImporterPage): bombea el bucle de mensajes
        /// para que la etiqueta de estado se repinte durante la carga síncrona del XML, igual que
        /// hacía Application.DoEvents() en la app WinForms original.</summary>
        private static void DoEvents()
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new System.Windows.Threading.DispatcherOperationCallback(f =>
                {
                    ((System.Windows.Threading.DispatcherFrame)f!).Continue = false;
                    return null;
                }), frame);
            System.Windows.Threading.Dispatcher.PushFrame(frame);
        }

        #endregion
    }
}
