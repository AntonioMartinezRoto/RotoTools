

namespace RotoEntities
{
    public class Moneda
    {
        public string Nombre { get; set; }
        public string? ISO4217 { get; set; }
        public string Simbolo { get; set; }
        public double Relacion { get; set; }
        public int Decimales { get; set; }
    }
}
