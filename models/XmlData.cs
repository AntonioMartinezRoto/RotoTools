
namespace RotoEntities
{
    public class XmlData
    {
        public string Supplier { get; set; }
        public int HardwareType { get; set; }
        public List<Set> SetList { get; set; }
        public List<FittingGroup> FittingGroupList { get; set; }
        public List<Fitting> FittingList { get; set; }
        public List<Colour> ColourList { get; set; }
        public List<Option> OptionList { get; set; }
        public string FittingsVersion { get; set; }
        public string OptionsVersion { get; set; }
        public string ColoursVersion { get; set; }
        public string FittingGroupVersion { get; set; }
    }
}
