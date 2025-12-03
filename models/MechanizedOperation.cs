
namespace RotoEntities
{
    public class MechanizedOperation
    {
        public string OperationName { get; set; }
        public string Description { get; set; }
        public Int16 External { get; set; }
        public Int16 IsPrimitive { get; set; }
        public string Level1 {  get; set; }
        public string Level2 { get; set; }
        public string Level3 { get; set; }
        public string Level4 { get; set; }
        public string Level5 { get; set; }
        public int RGB { get; set; }
        public bool Disable { get; set; } = false;
        public MechanizedOperation()
        {
        }
        public MechanizedOperation(string operationName, string description, short external, short isPrimitive, string level1, string level2, string level3, string level4, string level5, int rGB, bool disable)
        {
            OperationName = operationName;
            Description = description;
            External = external;
            IsPrimitive = isPrimitive;
            Level1 = level1;
            Level2 = level2;
            Level3 = level3;
            Level4 = level4;
            Level5 = level5;
            RGB = rGB;
            Disable = disable;
        }
        public MechanizedOperation(string operationName) 
        {
            OperationName = operationName;
            Description = "";
            External = 0;
            IsPrimitive = 0;
            Level1 = "ROTO";
            RGB = 255;
            Disable = false;
        }
    }
}
