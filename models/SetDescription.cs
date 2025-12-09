
namespace RotoEntities
{
    public class SetDescription
    {
        public int SetDescriptionId { get; set; }   // PK obligatoria
        public int Id { get; set; }
        public int FittingId { get; set; }
        public double MinWidth { get; set; }
        public double MaxWidth { get; set; }
        public double MinHeight { get; set; }
        public double MaxHeight { get; set; }
        public bool Horizontal { get; set; }
        public int Position { get; set; }
        public string ReferencePoint { get; set; }
        public int ChainPosition { get; set; }
        public string Movement { get; set; }
        public bool Inverted { get; set; }
        public double XPosition { get; set; }
        public int Alternative { get; set; }
        public string Location { get; set; }
        public List<Option> OptionList { get; set; }
        public Fitting Fitting { get; set; }    
    }
}
