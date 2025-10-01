
namespace RotoEntities
{
    public class ContenidoOpcion
    {
        public string Opcion { get; set; }
        public string Valor { get; set; }
        public string Texto { get; set; }
        public int Flags { get; set; }
        public bool OcultaEnLista { get; set; }
        public bool OcultaEnArbol { get; set; }
        public int Orden { get; set; }
        public int Id { get; set; }
        public int Invalid{ get; set; }

        public ContenidoOpcion(string optionName, string valor, string texto, string flags, string orden, string invalid)
        {
            Opcion = optionName;
            Valor = valor;
            Texto = texto;
            Orden = Convert.ToInt32(orden);
            Invalid = Convert.ToInt32(invalid);

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
        public ContenidoOpcion()
        {

        }
    }
}
