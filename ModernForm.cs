namespace RotoTools
{
    /// <summary>
    /// Clase base que sustituye a <see cref="Form"/> en todos los formularios de
    /// RotoTools para darles un aspecto moderno y consistente.
    ///
    /// No añade ni quita ningún comportamiento: simplemente, una vez el
    /// formulario ha terminado de cargar (después de que se ejecute su propio
    /// controlador de Load, si lo tiene, y por tanto después de que haya
    /// cargado sus datos), aplica <see cref="ModernTheme.Apply"/> sobre sí
    /// mismo para repintar colores, tipografía y estilos de los controles ya
    /// creados por el diseñador.
    /// </summary>
    public class ModernForm : Form
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ModernTheme.Apply(this);
        }
    }
}
