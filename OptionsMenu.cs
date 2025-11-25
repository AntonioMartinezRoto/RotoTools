using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections;
using System.ComponentModel.Design;
using System.Resources;

namespace RotoTools
{
    public partial class OptionsMenu : Form
    {
        public OptionsMenu()
        {
            InitializeComponent();
        }

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
        private void CargarTextos()
        {
            lbl_Idioma.Text = LocalizationManager.GetString("L_SeleccionarIdioma");
            this.Text = LocalizationManager.GetString("L_Opciones");
            chk_PermitirTraduccion.Text = LocalizationManager.GetString("L_PermitirTraduccion");
            btn_SaveOptions.Text = LocalizationManager.GetString("L_Guardar");
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

        private void btn_ImportarResources_Click(object sender, EventArgs e)
        {

        }
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
