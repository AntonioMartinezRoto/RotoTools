
namespace RotoEntities
{
    public class OperationsShapes
    {
        public string OperationName { get; set; }
        public string BasicShape { get; set; }
        public Int16 External { get; set; }
        public string XDistance { get; set; }
        public string YDistance {  get; set; }
        public string ZDistance { get; set; }
        public string Mill { get; set; }
        public double Depth { get; set; }
        public string XmlParameters { get; set; }
        public double Dimension { get; set; }
        public double Rotation { get; set; }
        public string Conditions { get; set; }
        public int Order { get; set; }

        public OperationsShapes()
        {

        }
        public OperationsShapes(string operationName, string basicShape)
        {
            OperationName = operationName;
            BasicShape = basicShape;
            External = 0;
        }
        public OperationsShapes(string operationName, string basicShape, string x, string y, string z)
        {
            OperationName = operationName;
            BasicShape = basicShape;
            XDistance = x;
            YDistance = y;
            ZDistance = z;
            External = 0;
        }
    }
}
