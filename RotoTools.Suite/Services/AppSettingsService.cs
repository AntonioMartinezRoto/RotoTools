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

        /// <summary>
        /// Equivale a Properties.Settings.Default["ControlCambiosAvanzado"] de la app WinForms
        /// original (ver RotoTools/Properties/Settings.settings, valor por defecto False):
        /// alterna entre el informe "avanzado" (con diálogo de configuración, ver
        /// Views/ControlCambios/ControlCambiosConfiguracionWindow.xaml) y el informe "simple"
        /// (un único botón, sin configuración) de Control de cambios. En la app original se
        /// cambiaba desde OptionsMenu (chk_ControlCambiosAvanzado); ese módulo ("Opciones") todavía
        /// no está migrado a la Suite (sigue siendo un PlaceholderPage en MainWindow), así que de
        /// momento esta preferencia solo se puede cambiar editando a mano
        /// rototools.suite.settings.json — se deja aquí ya preparada para cuando se migre ese
        /// módulo, igual que Language ya lo estaba antes de que existiera el selector de idioma.
        /// </summary>
        public bool ControlCambiosAvanzado { get; set; } = false;
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
