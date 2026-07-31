using System.Globalization;
using System.Windows.Data;

namespace RotoTools.Suite.Views.Cam
{
    /// <summary>
    /// Convierte un double? a texto de celda editable y viceversa, aceptando vacío como null.
    /// Necesario porque las columnas "Descuento canal de herraje" / "Posición canal de herraje"
    /// (Cam3D original: DataGridViewTextBoxColumn sobre un double? de libre edición) deben poder
    /// quedar en blanco, y el binding por defecto de WPF no siempre resuelve bien ese caso con
    /// tipos Nullable&lt;double&gt;.
    /// </summary>
    public class NullableDoubleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is double d ? d.ToString(CultureInfo.InvariantCulture) : "";

        public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            string texto = (value as string ?? "").Trim();
            if (string.IsNullOrEmpty(texto)) return null;
            return double.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out double resultado)
                ? resultado
                : Binding.DoNothing;
        }
    }

    /// <summary>
    /// Igual que Cam3DCatalogoAdmin.DataGridViewPlano_CellPainting (WinForms): a un valor entero
    /// de plano (grados, normalizado módulo 360) le añade una flecha de dirección como sufijo de
    /// texto ("0 →", "90 ↑", "180 ←", "270 ↓"; sin flecha para cualquier otro ángulo). ConvertBack
    /// solo toma la parte numérica inicial (antes del primer espacio), ignorando la flecha, para
    /// que la columna siga siendo editable como si fuera un entero normal.
    /// </summary>
    public class PlaneArrowConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not int plano) return value?.ToString() ?? "";

            int normalizado = ((plano % 360) + 360) % 360;
            string flecha = normalizado switch
            {
                0 => "→",
                90 => "↑",
                180 => "←",
                270 => "↓",
                _ => ""
            };

            return string.IsNullOrEmpty(flecha) ? plano.ToString(CultureInfo.InvariantCulture) : $"{plano} {flecha}";
        }

        public object? ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            string texto = (value as string ?? "").Trim();
            int espacio = texto.IndexOf(' ');
            if (espacio >= 0) texto = texto[..espacio];

            return int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out int resultado)
                ? resultado
                : Binding.DoNothing;
        }
    }
}
