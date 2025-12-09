
namespace RotoEntities
{
    public class Operation
    {
        public int Id { get; set; }   // PK obligatoria
        public string Name { get; set; }
        public string XPosition { get; set; }
        public string ReferencePoint { get; set; }
        public string Location {  get; set; }
    }
}
