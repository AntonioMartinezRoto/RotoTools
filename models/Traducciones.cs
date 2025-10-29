

namespace RotoEntities
{
    public class Traducciones
    {
        public Dictionary<string, string> FittingGroups { get; } = new();
        public Dictionary<string, string> Fittings { get; } = new();
        public Dictionary<string, string> Colours { get; } = new();
        public Dictionary<string, string> OptionNames { get; } = new();
        public Dictionary<(string OptionName, string Value), string> OptionValues { get; } = new();

    }
}
