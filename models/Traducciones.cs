

using DocumentFormat.OpenXml.Wordprocessing;

namespace RotoEntities
{
    public class Traducciones
    {
        public Dictionary<string, string> FittingGroups { get; } = new();
        public Dictionary<string, string> Fittings { get; } = new();
        public Dictionary<string, string> Colours { get; } = new();
        public Dictionary<string, string> OptionNames { get; } = new();
        public Dictionary<(string OptionName, string Value), string> OptionValues { get; } = new();

        public string TraducirOptionName(string optionName)
        {
            if (OptionNames.TryGetValue(optionName, out string traduccionName))
            {
                if (String.IsNullOrEmpty(traduccionName))
                {
                    return optionName;
                }
                else
                {
                    return traduccionName;
                }
            }
            return optionName;
        }
        public string TraducirOptionValue(string optionName, string optionValue)
        {
            if (OptionValues.TryGetValue((optionName, optionValue), out string traduccionValue))
            {
                if (String.IsNullOrEmpty(traduccionValue))
                {
                    return optionValue;
                }
                else
                {
                    return traduccionValue;
                }
                return traduccionValue;
            }
            return optionValue;
        }
    }
}
