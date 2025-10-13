
namespace RotoEntities
{
    public class DiferenciaXml
    {
        public int Tipo { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int Severidad { get; set; }
        public bool Visible { get; set; }
        public int OrigenDiferencia { get; set; }
        public string DetalleDiferenciaDescription { get; set; }
        public string DetalleDiferenciaAtributos { get; set; }
        public string DetalleDiferenciaArticulo { get; set; }

        public DiferenciaXml(int tipo, string descripcion, int severidad, bool visible)
        {
            Descripcion = descripcion;
            Tipo = tipo;
            Severidad = severidad;
            Visible = visible;
        }
        public DiferenciaXml(int tipo, string descripcion, int origenDiferencia, int severidad, bool visible)
        {
            Descripcion = descripcion;
            Tipo = tipo;
            Severidad = severidad;
            Visible = visible;
        }
        public DiferenciaXml(int tipo, string titulo, string descripcion, int severidad, bool visible, int origenDiferencia, string detalleDiferenciaAtributos, string detalleDiferenciaArticulo)
        {
            Titulo = titulo;
            Descripcion = descripcion;
            Tipo = tipo;
            Severidad = severidad;
            Visible = visible;
            OrigenDiferencia = origenDiferencia;
            DetalleDiferenciaAtributos = detalleDiferenciaAtributos;
            DetalleDiferenciaArticulo = detalleDiferenciaArticulo;
        }
        public DiferenciaXml(int tipo, string titulo, string descripcion, int severidad, bool visible, int origenDiferencia,
                             string detalleDiferenciaDescription, string detalleDiferenciaAtributos, string detalleDiferenciaArticulo)
        {
            Titulo = titulo;
            Descripcion = descripcion;
            Tipo = tipo;
            Severidad = severidad;
            Visible = visible;
            OrigenDiferencia = origenDiferencia;
            DetalleDiferenciaDescription = detalleDiferenciaDescription;
            DetalleDiferenciaAtributos = detalleDiferenciaAtributos;
            DetalleDiferenciaArticulo = detalleDiferenciaArticulo;
        }

    }
}
