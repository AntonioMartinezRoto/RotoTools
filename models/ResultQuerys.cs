
namespace RotoEntities
{
    public class ResultQuerys
    {
        public int ResultQueryUpdateGruposYProveedor { get; set; }
        public int ResultQueryUpdateNivel1MaterialesBaseYOpciones { get; set; }
        public int ResultQueryUpdatePropFicticios { get; set; }
        public int ResultQueryUpdateDescripcionesMateriales { get; set; }
        public int ResultQueryGeneral { get; set; }

        public ResultQuerys()
        {

        }
        public ResultQuerys(int resultQueryUpdateGruposYProveedor, int resultQueryUpdateNivel1MaterialesBaseYOpciones, int resultQueryUpdatePropFicticios, int resultQueryUpdateDescripcionesMateriales)
        {
            this.ResultQueryUpdateGruposYProveedor = resultQueryUpdateGruposYProveedor;
            this.ResultQueryUpdatePropFicticios = resultQueryUpdatePropFicticios;
            this.ResultQueryUpdateDescripcionesMateriales = resultQueryUpdateDescripcionesMateriales;
            this.ResultQueryUpdateNivel1MaterialesBaseYOpciones = resultQueryUpdateNivel1MaterialesBaseYOpciones;
        }
        public ResultQuerys(int resultQueryGeneral)
        {
            this.ResultQueryGeneral = resultQueryGeneral;
        }
    }

}
