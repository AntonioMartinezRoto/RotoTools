using System.Globalization;
using System.Resources;

namespace RotoTools
{
    public static class LocalizationManager
    {
        private static ResourceManager _resourceManager =
            new ResourceManager("RotoTools.Resources.Strings", typeof(LocalizationManager).Assembly);

        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;

        public static void SetLanguage(string cultureCode)
        {
            CurrentCulture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentUICulture = CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CurrentCulture;
        }

        public static string GetString(string key)
        {
            return _resourceManager.GetString(key, CurrentCulture) ?? $"[{key}]";
        }
    }
}
