
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
            InitializeLevel2(operationName);
        }

        private void InitializeLevel2(string operationName)
        {
            string op = operationName.ToUpper();

            if (op.Contains("BOMBILLO"))
            {
                Level2 = "BOMBILLOS";
            }
            else if (op.Contains("PERNIO") || op.Contains("BISAGRA") || op.Contains("PB10") || op.Contains("150R") || op.Contains("SOPORTE_COMPAS"))
            {
                Level2 = "BISAGRAS";
            }
            else if (op.Contains("BOCALLAVE"))
            {
                Level2 = "BOCALLAVES";
            }
            else if (op.Contains("TRIPLE") || op.Contains("CREMONA") || op.Contains("X3") || op.Contains("TALADRO") || op.Contains("UÑERO") || op.Contains("PALANCA"))
            {
                Level2 = "CREMONAS";
            }
            else if (op.Contains("MANILLA"))
            {
                Level2 = "MANILLAS";
            }
            else if (op.Contains("PLACA"))
            {
                Level2 = "PLACAS";
            }
            else if (op.Contains("CERRADURA") || op.Contains("MOTOR") || op.Contains("ENEO") || op.Contains("PLETINA"))
            {
                Level2 = "CERRADURAS";
            }
            else if (op.Contains("CERR_") || op.Contains("MRC_"))
            {
                Level2 = "CERRADEROS";
            }
            else if (op.Contains("TANDEM"))
            {
                Level2 = "RUEDAS";
            }
            else if (op.Contains("ALV_"))
            {
                Level2 = "ALVERSA";
            }
            else if (op.Contains("ELEV_"))
            {
                Level2 = "PATIO LIFT";
            }
            else if (op.Contains("PLEG_"))
            {
                Level2 = "PLEGABLES";
            }
            else if (op.Contains("INOWA"))
            {
                Level2 = "INOWA";
            }
            else
            {
                Level2 = "OTRAS";
            }
        }
    }
}
