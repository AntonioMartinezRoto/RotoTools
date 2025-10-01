

namespace RotoEntities
{
    public class ComparaSetsProperties
    {
        public bool compararSets { get; set; }
        public bool compararOpenings { get; set; }
        public bool compararSetsFiltrados { get; set; }
        public List<String> compararSetsFiltradosList { get; set; } = new List<String>();
        public List<String> compararSetsComunesList { get; set; } = new List<String>();
        public List<String> compararSetsSoloXml1List { get; set; } = new List<String>();
        public List<String> compararSetsSoloXml2List { get; set; } = new List<String>();
        public bool compararCantidadSetDescriptions { get; set; }
    }
}
