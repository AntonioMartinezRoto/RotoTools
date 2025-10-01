
namespace RotoEntities
{
    public class Opcion
    {
        public string Name { get; set; }
        public string Descripcion { get; set; }
        public string Nivel1 { get; set; }
        public string Nivel2{ get; set; }
        public string Nivel3 { get; set; }
        public string Nivel4 { get; set; }
        public string Nivel5 { get; set; }
        public int Flags { get; set; } = 0;
        public bool OcultaEnLista { get; set; }
        public bool OcultaEnArbol { get; set; }
        public List<ContenidoOpcion> ContenidoOpcionesList { get; set; } = new List<ContenidoOpcion>();

        public Opcion(string name, string descripcion, string nivel1, string nivel2, string nivel3, string nivel4, string nivel5, string flags)
        {
            Name = name;
            Descripcion = descripcion;
            Nivel1 = nivel1;
            Nivel2 = nivel2;
            Nivel3 = nivel3;
            Nivel4 = nivel4;
            Nivel5 = nivel5;
            if (String.IsNullOrEmpty(flags))
            {
                Flags = 0;
            }
            else
            {
                Flags = Convert.ToInt16(flags);
            }

            switch (Flags)
            {
                case 0:
                    OcultaEnLista = false;
                    OcultaEnArbol = false;
                    break;
                case 1:
                    OcultaEnLista = true;
                    OcultaEnArbol = false;
                    break;
                case 2:
                    OcultaEnLista = false;
                    OcultaEnArbol = true;
                    break;
                case 3:
                    OcultaEnLista = true;
                    OcultaEnArbol = true;
                    break;
            }
        }
        public Opcion()
        {

        }
    }
}
