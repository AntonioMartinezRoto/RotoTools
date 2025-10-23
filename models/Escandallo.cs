
using static RotoTools.Enums;

namespace RotoEntities
{
    public class Escandallo
    {
        public Guid DataVerId { get; set; }
        public Guid RowId { get; set; }
        public Guid LCRowId { get; set; }
        public string Codigo { get; set; }
        public short Type { get; set; }
        public string Descripcion { get; set; }
        public string Nivel1 { get; set; }
        public string Nivel2 { get; set; }
        public string Nivel3 { get; set; }
        public string Nivel4 { get; set; }
        public string Nivel5 { get; set; }
        public string Variables { get; set; }
        public string Programa { get; set; }
        public byte[] Image { get; set; }
        public string Texto { get; set; }
        public string Familia { get; set; }
        public string XMLTable { get; set; }
        public Guid ProductionType { get; set; }
        public short PrefShopStatus { get; set; }

        public enumRotoTipoEscandallo RotoTipo { get; set; }
    }
}
