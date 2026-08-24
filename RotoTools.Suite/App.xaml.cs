using System.Windows;
using RotoTools.Suite.Services;
using RotoTools.Suite.Views;

namespace RotoTools.Suite
{
    /// <summary>
    /// Punto de arranque de RotoTools Suite. Equivale a Program.cs + Main.cs de la app WinForms
    /// original: carga el idioma guardado y abre la ventana principal siempre maximizada.
    /// </summary>
    public partial class App : Application
    {
        public static AppSettings CurrentSettings { get; private set; } = new AppSettings();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CurrentSettings = AppSettingsService.Load();

            // Reutiliza el LocalizationManager ya existente en RotoTools.csproj (mismo mecanismo
            // de traducción que usa toda la app WinForms): todos los recursos .resx embebidos
            // están disponibles automáticamente a través de la referencia de proyecto, sin
            // duplicar ni un solo texto.
            RotoTools.LocalizationManager.SetLanguage(CurrentSettings.Language);

            var ventanaPrincipal = new MainWindow
            {
                WindowState = WindowState.Maximized
            };
            ventanaPrincipal.Show();
        }
    }
}
