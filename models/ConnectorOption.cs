using System.Xml.Serialization;

namespace RotoEntities
{
    public class ConnectorOption
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Value")]
        public string Value { get; set; }
    }

    public class ConnectorOptions
    {
        [XmlElement("Option")]
        public List<ConnectorOption> OptionList { get; set; }
    }

    public class ConnectorOptionsWrapper
    {
        [XmlElement("Option")]
        public List<ConnectorOption> OptionList { get; set; }
    }

    public class ConnectorOptionsContainer
    {
        [XmlElement("Options")]
        public ConnectorOptionsWrapper Options { get; set; }
    }
}
