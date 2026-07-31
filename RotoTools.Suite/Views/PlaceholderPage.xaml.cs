using System.Windows.Controls;
using System.Windows.Media;

namespace RotoTools.Suite.Views
{
    /// <summary>
    /// Página genérica "próximamente" para los módulos del menú principal original que todavía no
    /// se han migrado a la suite en esta primera fase (ver Main.Designer.cs para el inventario
    /// completo de 10 módulos).
    /// </summary>
    public partial class PlaceholderPage : UserControl
    {
        public PlaceholderPage(string titulo, string descripcion, Geometry icono, Brush color)
        {
            InitializeComponent();

            TxtTitulo.Text = titulo;
            TxtDescripcion.Text = descripcion;
            IconoModulo.Data = icono;
            Badge.Background = color;
        }
    }
}
