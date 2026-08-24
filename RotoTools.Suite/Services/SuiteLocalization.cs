using System.Resources;

namespace RotoTools.Suite.Services
{
    /// <summary>
    /// Textos propios de RotoTools.Suite que no existen en el proyecto original (p.ej. los
    /// subtítulos descriptivos de las páginas de la Suite): viven en su propio recurso
    /// (Resources/SuiteStrings*.resx, embebido en este mismo proyecto) en vez de añadir claves al
    /// Resources/Strings*.resx de RotoTools.csproj, para no tocar el proyecto original en
    /// absoluto. Sigue siempre el mismo idioma que RotoTools.LocalizationManager, así que cambia
    /// junto con el selector de idioma de la cabecera.
    /// </summary>
    public static class SuiteLocalization
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager("RotoTools.Suite.Resources.SuiteStrings", typeof(SuiteLocalization).Assembly);

        public static string GetString(string key)
            => _resourceManager.GetString(key, RotoTools.LocalizationManager.CurrentCulture) ?? $"[{key}]";
    }
}
