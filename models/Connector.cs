using System.Xml.Serialization;

namespace RotoEntities
{
    [XmlRoot("Connector")]
    public class Connector
    {
        [XmlAttribute("Connector_code")]
        public string ConnectorCode { get; set; }

        [XmlAttribute("Message")]
        public string Message { get; set; }

        [XmlElement("Node")]
        public List<ConnectorNode> Nodes { get; set; }
    }
}
