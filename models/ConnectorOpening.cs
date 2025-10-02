using System.Xml.Serialization;

namespace RotoEntities
{
    public class ConnectorOpeningFlag
    {
        [XmlAttribute("Value")]
        public string Value { get; set; }
    }

    public class ConnectorOpening
    {
        [XmlElement("Opening_Flag")]
        public List<ConnectorOpeningFlag> Flags { get; set; }
    }
}
