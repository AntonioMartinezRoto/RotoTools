
namespace RotoEntities
{
    public class Set
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Movement { get; set; }
        public string Associated { get; set; }
        public string MinWidth { get; set; }
        public string MaxWidth { get; set; }
        public string MinHeight { get; set; }
        public string MaxHeight { get; set; }
        public Opening Opening { get; set; }
        public List<SetDescription> SetDescriptionList { get; set; }

    }
}
