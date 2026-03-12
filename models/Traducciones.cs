

using DocumentFormat.OpenXml.Wordprocessing;

namespace RotoEntities
{
    public class Traducciones
    {
        public Dictionary<string, string> FittingGroups { get; } = new();
        public Dictionary<string, string> Sets { get; } = new();
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
            }
            return optionValue;
        }
        public string TraducirSetCode(string setCode)
        {
            if (Sets.TryGetValue(setCode, out string traduccionCode))
            {
                if (String.IsNullOrEmpty(traduccionCode))
                {
                    return setCode;
                }
                else
                {
                    return traduccionCode;
                }
            }
            return setCode;
        }
    }
}
