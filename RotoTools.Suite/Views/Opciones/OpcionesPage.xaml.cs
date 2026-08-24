using System.Windows;
using System.Windows.Controls;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.Opciones
{
    /// <summary>
    /// Sustituye a OptionsMenu.cs/.Designer.cs (WinForms). Ver el comentario grande en
    /// OpcionesPage.xaml para el detalle de qué se migra y qué se deja fuera a propósito
    /// (selector de idioma, ya cubierto por la barra lateral de MainWindow; exportar/importar
    /// recursos, oculto e inalcanzable ya en el original).
    /// </summary>
    public partial class OpcionesPage : UserControl
    {
        public OpcionesPage()
        {
            InitializeComponent();

            CargarTextos();

            ChkPermitirTraduccion.IsChecked = RotoTools.TranslateManager.PermitirTraduccionesEnConectorEscandallos;
            ChkControlCambiosAvanzado.IsChecked = App.CurrentSettings.ControlCambiosAvanzado;
        }

        #region Localización

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_Opciones");
            TxtSubtitulo.Text = Loc("L_Suite_OpcionesSubtitulo");
            TxtNotaIdioma.Text = Loc("L_Suite_NotaSelectorIdioma");

            TxtTraduccionesTitulo.Text = RotoTools.LocalizationManager.GetString("L_Traduccion");
            ChkPermitirTraduccion.Content = RotoTools.LocalizationManager.GetString("L_PermitirTraduccion");

            TxtControlCambiosTitulo.Text = RotoTools.LocalizationManager.GetString("L_ControlCambios");
            ChkControlCambiosAvanzado.Content = RotoTools.LocalizationManager.GetString("L_ControlCambiosAvanzado");

            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
        }

        #endregion

        #region Events

        /// <summary>
        /// Igual que btn_SaveOptions_Click en el original, sin el idioma (ver comentario del XAML)
        /// y sin el bug que tenía allí: en el original, Properties.Settings.Default["ControlCambios
        /// Avanzado"] se asignaba pero solo se llamaba a Properties.Settings.Default.Save() dentro
        /// del bloque del idioma (que se ejecuta siempre antes, ya que cmb_Idioma.SelectedValue
        /// nunca es null) — así que ese ajuste en la práctica solo persistía entre sesiones si,
        /// en esa misma visita a Opciones, el idioma también se llegaba a guardar. Aquí no aplica:
        /// AppSettingsService.Save no tiene ese problema de guardado parcial, así que
        /// ControlCambiosAvanzado se persiste siempre que se pulsa Guardar, tal y como se espera
        /// (y tal y como hace falta para poder probar el modo avanzado, que es el motivo de esta
        /// migración).
        /// </summary>
        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Sesión únicamente, igual que en el original: TranslateManager.
            // PermitirTraduccionesEnConectorEscandallos no se persiste en ningún fichero de
            // ajustes ni en el original ni aquí, solo vive en memoria mientras la app está abierta.
            RotoTools.TranslateManager.PermitirTraduccionesEnConectorEscandallos = ChkPermitirTraduccion.IsChecked == true;

            App.CurrentSettings.ControlCambiosAvanzado = ChkControlCambiosAvanzado.IsChecked == true;
            AppSettingsService.Save(App.CurrentSettings);

            MessageBox.Show(Loc("L_Suite_OperacionCompletada"), "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
