
namespace RotoEntities
{
    public class Value
    {
        public string Valor { get; set; }
        public override string ToString() => Valor;

        public bool Equals(Value other)
        {
            if (other is null) return false;
            return string.Equals(Valor, other.Valor, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) => Equals(obj as Value);

        public override int GetHashCode() => Valor?.GetHashCode() ?? 0;
    }
}
