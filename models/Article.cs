
namespace RotoEntities
{
    public class Article
    {
        public string Ref { get; set; }
        public string Final { get; set; }
        public double XPosition { get; set; }
        public string ReferencePoint { get; set; }
        public string Side { get; set; }
        public string Location { get; set; }
        public List<Option> OptionList { get; set; }
        public Fitting Fitting { get; set; }
    }
}
