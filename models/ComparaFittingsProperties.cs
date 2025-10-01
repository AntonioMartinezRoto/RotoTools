

namespace RotoEntities
{
    public class ComparaFittingsProperties
    {
        public bool compararFittings { get; set; }
        public bool compararFittingsFiltrados { get; set; }
        public List<String> compararFittingsFiltradosList { get; set; } = new List<String>();
        public List<String> compararFittingsComunesList { get; set; } = new List<String>();
        public bool compararFittingsLocation { get; set; }
        public bool compararFittingsManufacturer { get; set; }
        public bool compararFittingsLength { get; set; }
        public bool compararFittingsDescription { get; set; }
        public bool compararFittingsArticles { get; set; }
    }
}
