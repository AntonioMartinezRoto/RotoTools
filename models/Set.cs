
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
        public string Script { get; set; }
        public List<Option> OptionConectorList { get; set; }
        public List<Option> OpeningFlagConectorList { get; set; }
        public bool IsTitle { get; set; }

        public Set(string script)
        {
            Script = script;
            IsTitle = true;
        }
        public Set()
        {

        }
        public Set(Set set)
        {
            Id = set.Id;
            Code = set.Code;
            Movement = set.Movement;
            Associated = set.Associated;
            MinWidth = set.MinWidth;
            MaxWidth = set.MaxWidth;
            MinHeight = set.MinHeight;
            MaxHeight = set.MaxHeight;
            Opening = set.Opening;
            OptionConectorList = set.OptionConectorList != null
                ? new List<Option>(set.OptionConectorList.Select(o => new Option(o.Name, o.Value)))
                : new List<Option>();

            OpeningFlagConectorList = set.OpeningFlagConectorList != null
                ? new List<Option>(set.OpeningFlagConectorList.Select(o => new Option(o.Name, o.Value)))
                : new List<Option>();
        }

    }
}
