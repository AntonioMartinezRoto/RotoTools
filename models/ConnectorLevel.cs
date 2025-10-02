
using System.Xml.Serialization;

namespace RotoEntities
{
    public class ConnectorLevel
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
    }

    public class ConnectorLevelsWrapper
    {
        [XmlElement("Level")]
        public List<ConnectorLevel> LevelList { get; set; }
    }
}
