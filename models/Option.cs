
namespace RotoEntities
{
    public class Option
    {
        public int Id { get; set; }   // PK obligatoria
        public string Name { get; set; }
        public string Value { get; set; }
        public List<Value> ValuesList { get; set; }
        public Option() { }
        public Option(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
