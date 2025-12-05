using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections;
using System.ComponentModel.Design;
using System.Resources;
using System.Xml.Linq;

namespace RotoTools
{
    public partial class OptionsMenu : Form
    {
        #region Constructor
        public OptionsMenu()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void OptionsMenu_Load(object sender, EventArgs e)
        {
            CargarTextos();
            var idiomas = new List<LanguageItem>
                {
                    new LanguageItem { Text = "Español", Value = "es" },
                    new LanguageItem { Text = "English", Value = "en" },
                    //new LanguageItem { Text = "Português", Value = "pt" },
                    //new LanguageItem { Text = "Italiano", Value = "it" }
                };

            cmb_Idioma.DataSource = idiomas;
            cmb_Idioma.DisplayMember = "Text";
            cmb_Idioma.ValueMember = "Value";

            // Selecciona el idioma actual
            cmb_Idioma.SelectedValue = LocalizationManager.CurrentCulture.TwoLetterISOLanguageName;
            chk_PermitirTraduccion.Checked = TranslateManager.PermitirTraduccionesEnConectorEscandallos;
        }
        private void btn_SaveOptions_Click(object sender, EventArgs e)
        {
            if (cmb_Idioma.SelectedValue != null)
            {
                string selectedCulture = cmb_Idioma.SelectedValue.ToString();

                LocalizationManager.SetLanguage(selectedCulture);

                Properties.Settings.Default["Language"] = selectedCulture;
                Properties.Settings.Default.Save();

                Close();
            }
            TranslateManager.PermitirTraduccionesEnConectorEscandallos = chk_PermitirTraduccion.Checked;
        }
        private void btn_ExportarResources_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo Excel (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Save as";
            saveFileDialog.FileName = "Export.xlsx";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                GenerarExportacionRecursos(saveFileDialog.FileName);
            }
        }
        private void btn_ImportarResources_Click(object sender, EventArgs e)
        {
            string resourcesFolder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources");
            resourcesFolder = Path.GetFullPath(resourcesFolder);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivo Excel (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ImportarRecursos(openFileDialog.FileName, resourcesFolder, "Strings");
            }            
        }

        #endregion

        #region Private Methods
        private void CargarTextos()
        {
            lbl_Idioma.Text = LocalizationManager.GetString("L_SeleccionarIdioma");
            this.Text = LocalizationManager.GetString("L_Opciones");
            chk_PermitirTraduccion.Text = LocalizationManager.GetString("L_PermitirTraduccion");
            btn_SaveOptions.Text = LocalizationManager.GetString("L_Guardar");
        }
        private void GenerarExportacionRecursos(string fileName)
        {
            string folder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources");
            folder = Path.GetFullPath(folder);

            var entries = LoadAllResourceValues(folder, "Strings");

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Resources");

            int rowIndex = 0;

            // Crear fila del encabezado
            IRow header = sheet.CreateRow(rowIndex++);
            header.CreateCell(0).SetCellValue("Key");
            header.CreateCell(1).SetCellValue("es");
            header.CreateCell(2).SetCellValue("en");
            header.CreateCell(3).SetCellValue("de");
            header.CreateCell(4).SetCellValue("it");
            header.CreateCell(5).SetCellValue("pt");

            // Escribir cada entrada
            foreach (var entry in entries)
            {
                IRow row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(entry.Key);

                row.CreateCell(1).SetCellValue(entry.ValuesByLanguage.ContainsKey("es") ? entry.ValuesByLanguage["es"] : "");
                row.CreateCell(2).SetCellValue(entry.ValuesByLanguage.ContainsKey("en") ? entry.ValuesByLanguage["en"] : "");
                row.CreateCell(3).SetCellValue(entry.ValuesByLanguage.ContainsKey("de") ? entry.ValuesByLanguage["de"] : "");
                row.CreateCell(4).SetCellValue(entry.ValuesByLanguage.ContainsKey("it") ? entry.ValuesByLanguage["it"] : "");
                row.CreateCell(5).SetCellValue(entry.ValuesByLanguage.ContainsKey("pt") ? entry.ValuesByLanguage["pt"] : "");
            }

            // Ajustar columnas al contenido
            for (int i = 0; i < 6; i++)
                sheet.AutoSizeColumn(i);

            // Guardar archivo
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fs);
            }
        }
        public static List<ResourceEntry> LoadAllResourceValues(string resourcesFolderPath, string baseFileName)
        {
            // Ejemplo baseFileName = "Strings"
            // Archivos buscados: Strings.resx, Strings.es.resx, Strings.de.resx, Strings.en.resx, etc.

            var result = new Dictionary<string, ResourceEntry>();

            var resxFiles = Directory.GetFiles(resourcesFolderPath, $"{baseFileName}*.resx");

            foreach (var file in resxFiles)
            {
                // Idioma por convención de nombre
                string language = GetLanguageFromFileName(file, baseFileName);

                using var reader = new ResXResourceReader(file);
                reader.UseResXDataNodes = true;

                foreach (DictionaryEntry entry in reader)
                {
                    string key = entry.Key.ToString();
                    var node = (ResXDataNode)entry.Value;
                    string value = node.GetValue((ITypeResolutionService)null)?.ToString() ?? "";

                    if (!result.ContainsKey(key))
                    {
                        result[key] = new ResourceEntry
                        {
                            Key = key
                        };
                    }

                    result[key].ValuesByLanguage[language] = value;
                }
            }

            return result.Values.ToList();
        }
        private static string GetLanguageFromFileName(string filePath, string baseFileName)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Strings.resx → idioma = "es" (predeterminado)
            if (fileName == baseFileName)
                return "es";

            // Strings.de.resx → idioma = "de"
            var parts = fileName.Split('.');
            return parts.Length > 1 ? parts[1] : "unknown";
        }
        private void ImportarRecursos(string fileName, string resourcesFolder, string baseName)
        {
            // Abrir Excel
            XSSFWorkbook wb;
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(fs);
            }

            var sheet = wb.GetSheetAt(0);

            // Diccionario: idioma → diccionario de pares clave/valor
            Dictionary<string, Dictionary<string, string>> langs =
                new Dictionary<string, Dictionary<string, string>>
                {
                    { "es", new Dictionary<string,string>() },
                    { "en", new Dictionary<string,string>() },
                    { "de", new Dictionary<string,string>() },
                    { "it", new Dictionary<string,string>() },
                    { "pt", new Dictionary<string,string>() }
                };

            // Procesar filas del Excel
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                string key = row.GetCell(0)?.ToString()?.Trim();
                if (string.IsNullOrEmpty(key)) continue;

                langs["es"][key] = row.GetCell(1)?.ToString() ?? "";
                langs["en"][key] = row.GetCell(2)?.ToString() ?? "";
                langs["de"][key] = row.GetCell(3)?.ToString() ?? "";
                langs["it"][key] = row.GetCell(4)?.ToString() ?? "";
                langs["pt"][key] = row.GetCell(5)?.ToString() ?? "";
            }

            // Guardar cada idioma en su .resx
            SaveResx(Path.Combine(resourcesFolder, $"{baseName}.resx"), langs["es"]);
            SaveResx(Path.Combine(resourcesFolder, $"{baseName}.en.resx"), langs["en"]);
            SaveResx(Path.Combine(resourcesFolder, $"{baseName}.de.resx"), langs["de"]);
            SaveResx(Path.Combine(resourcesFolder, $"{baseName}.it.resx"), langs["it"]);
            SaveResx(Path.Combine(resourcesFolder, $"{baseName}.pt.resx"), langs["pt"]);
        }
        private void SaveResx(string resxPath, Dictionary<string, string> values)
        {
            // Crear XML base
            XElement root = new XElement("root",
                new XElement("resheader",
                    new XAttribute("name", "resmimetype"),
                    new XElement("value", "text/microsoft-resx")
                ),
                new XElement("resheader",
                    new XAttribute("name", "version"),
                    new XElement("value", "2.0")
                ),
                new XElement("resheader",
                    new XAttribute("name", "reader"),
                    new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms")
                ),
                new XElement("resheader",
                    new XAttribute("name", "writer"),
                    new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms")
                )
            );

            // Añadir cada clave del idioma
            foreach (var kv in values)
            {
                root.Add(
                    new XElement("data",
                        new XAttribute("name", kv.Key),
                        new XAttribute(XNamespace.Xml + "space", "preserve"),
                        new XElement("value", kv.Value ?? "")
                    )
                );
            }

            // Guardar fichero
            root.Save(resxPath);
        }

        #endregion







    }
    public class LanguageItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
    public class ResourceEntry
    {
        public string Key { get; set; }
        public Dictionary<string, string> ValuesByLanguage { get; set; } = new();
    }
}
