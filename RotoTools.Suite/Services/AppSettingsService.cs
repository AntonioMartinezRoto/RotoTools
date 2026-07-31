using System.IO;
using System.Text.Json;

namespace RotoTools.Suite.Services
{
    /// <summary>
    /// Preferencias de la app persistidas en un fichero JSON junto al ejecutable (NO en el
    /// registro de Windows ni en %AppData%), para que la app siga siendo 100% portable.
    /// </summary>
    public class AppSettings
    {
        public string Language { get; set; } = "es";
    }

    public static class AppSettingsService
    {
        private static readonly string SettingsPath =
            Path.Combine(AppContext.BaseDirectory, "rototools.suite.settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                        return settings;
                }
            }
            catch
            {
                // Fichero corrupto o inaccesible: se continúa con las preferencias por defecto.
            }

            return new AppSettings();
        }

        public static void Save(AppSettings settings)
        {
            try
            {
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, opciones);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Si no se puede escribir, la app sigue funcionando con las preferencias en
                // memoria de esta sesión.
            }
        }
    }
}
