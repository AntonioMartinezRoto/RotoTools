using System.Windows.Controls;

namespace RotoTools.Suite.Views
{
    /// <summary>
    /// Página de inicio de la suite (nueva: la app WinForms original no tenía una pantalla de
    /// bienvenida propiamente dicha, era directamente el menú). Explica qué es RotoTools Suite y
    /// en qué fase de la migración está, mientras dura la transición desde la app clásica.
    /// </summary>
    public partial class DashboardPage : UserControl
    {
        public DashboardPage()
        {
            InitializeComponent();
            CargarTextos();
        }

        private void CargarTextos()
        {
            TxtBienvenida.Text = "RotoTools Suite";
            TxtSubtitulo.Text = "Roto Frank FTT GmbH";
            TxtDescripcion.Text =
                "Nueva shell moderna y portable de RotoTools. Esta primera versión sienta la " +
                "estructura general (menú principal, navegación, idiomas, marca Roto): los " +
                "módulos se irán migrando uno a uno, empezando por CAM, manteniendo siempre la " +
                "versión clásica disponible para comparar mientras dure la transición.";
            TxtAvisoFase.Text =
                "Fase actual: estructura y menú principal. Todavía ningún módulo tiene su " +
                "funcionalidad completa migrada aquí; siguen usándose desde la versión clásica " +
                "de RotoTools hasta que se migren.";
        }
    }
}
