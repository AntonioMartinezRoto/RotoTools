
namespace RotoEntities
{
    public class DiferenciaXml
    {
        public int Tipo { get; set; }
        public string Descripcion { get; set; }
        public int Severidad { get; set; }
        public bool Visible { get; set; }

        public DiferenciaXml(int tipo, string descripcion, int severidad, bool visible) 
        {
            Descripcion = descripcion;
            Tipo = tipo;
            Severidad = severidad;
            Visible = visible;
        }
    }
}
