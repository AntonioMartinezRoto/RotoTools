
namespace RotoEntities
{
    public class Article
    {
        public int Id { get; set; }   // PK obligatoria
        public string Ref { get; set; }
        public string Final { get; set; }
        public double XPosition { get; set; }
        public string ReferencePoint { get; set; }
        public string Side { get; set; }
        public string Location { get; set; }
        public List<Option> OptionList { get; set; }
    }
}
