using System.Xml.Serialization;

namespace RotoConectorHerraje
{
    public class ConnectorNode
    {
        [XmlAttribute("Script")]
        public string Script { get; set; }

        [XmlAttribute("Fitting_Code")]
        public string FittingCode { get; set; }

        [XmlElement("References")]
        public List<ConnectorReferences> ReferenceList { get; set; }

        [XmlElement("Opening")]
        public List<ConnectorOpening> OpeningList { get; set; }

        [XmlElement("Levels")]
        public ConnectorLevelsWrapper Levels { get; set; }

        [XmlElement("Included_Options")]
        public ConnectorOptionsContainer IncludedOptions { get; set; }
    }
}
