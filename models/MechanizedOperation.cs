
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
        public string Phase { get; set; }
        public string XmlParameters { get; set; }
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
        public MechanizedOperation(string operationName, string description, short external, short isPrimitive, string level1, string level2, string level3, string level4, string level5, string phase, string xmlParameters, int rGB, bool disable)
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
            Phase = phase;
            XmlParameters = xmlParameters;
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
            InitializeLevel3(operationName, Level2);
        }
        public void InitializeLevel2(string operationName)
        {
            string op = operationName.ToUpper();

            if (op.Contains("BOMBILLO"))
            {
                Level2 = "BOMBILLOS";
            }
            else if (op.Contains("ALV_"))
            {
                Level2 = "ALVERSA";
            }
            else if (op.Contains("PERNIO") || op.Contains("BISAGRA") || op.Contains("PB10") || op.Contains("150R") || op.Contains("150P") || 
                     op.Contains("SOPORTE_COMPAS") || op.Contains("SOPORTE_NX_TORNILLO") || op.Contains("222") || op.Contains("218") || op.Contains("318") || 
                     op.Contains("322") || op.Contains("PS23") || op.Contains("PS27") || op.Contains("SOPORTE"))
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
        public void InitializeLevel3(string operationName, string level2)
        {
            string op = operationName.ToUpper();

            switch (level2)
            {
                case "BOMBILLOS":
                case "CERRADURAS":
                case "PLACAS":
                case "MANILLAS":
                case "CREMONAS":
                case "BOCALLAVES":
                    if (op.Contains("15"))
                    {
                        Level3 = "Aguja 15";
                        break;
                    }
                    if (op.Contains("17"))
                    {
                        Level3 = "Aguja 17";
                        break;
                    }
                    if (op.Contains("25"))
                    {
                        Level3 = "Aguja 25";
                        break;
                    }
                    if (op.Contains("28"))
                    {
                        Level3 = "Aguja 28";
                        break;
                    }
                    if (op.Contains("30"))
                    {
                        Level3 = "Aguja 30";
                        break;
                    }
                    if (op.Contains("35"))
                    {
                        Level3 = "Aguja 35";
                        break;
                    }
                    if (op.Contains("37"))
                    {
                        Level3 = "Aguja 37";
                        break;
                    }
                    if (op.Contains("40"))
                    {
                        Level3 = "Aguja 40";
                        break;
                    }
                    if (op.Contains("45"))
                    {
                        Level3 = "Aguja 45";
                        break;
                    }
                    if (op.Contains("50"))
                    {
                        Level3 = "Aguja 50";
                        break;
                    }
                    if (op.Contains("55"))
                    {
                        Level3 = "Aguja 55";
                        break;
                    }
                    if (op.Contains("7"))
                    {
                        Level3 = "Aguja 7";
                        break;
                    }
                    if (op.Contains("8"))
                    {
                        Level3 = "Aguja 8"; 
                        break;
                    }
                    break;
                case "BISAGRAS":
                    if (op.Contains("150"))
                    {
                        Level3 = "150";
                        break;
                    }
                    if (op.Contains("222") || op.Contains("218") || op.Contains("318") || op.Contains("322"))
                    {
                        Level3 = "SOLID B";
                        break;
                    }
                    if (op.Contains("PB10"))
                    {
                        Level3 = "PB10";
                        break;
                    }
                    if (op.Contains("PS23"))
                    {
                        Level3 = "PS23";
                        break;
                    }
                    if (op.Contains("PS27"))
                    {
                        Level3 = "PS27";
                        break;
                    }
                    break;
            }
        }
    }
}
