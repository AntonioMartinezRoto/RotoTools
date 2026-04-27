using RotoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RotoTools.models
{    public static class SetExtensions
    {
        public static bool EsPasivaVentanaPracticable(this Set s)
        {
            if (s?.Code == null) return false;

            string code = s.Code.ToUpper();

            // Centralizamos los prefijos permitidos
            string[] prefijosValidos = { "(1V)2P", "(1E)2P", "(1)2P" };

            bool cumplePrefijo = prefijosValidos.Any(p => code.StartsWith(p));
            bool esValido = cumplePrefijo &&
                            !code.Contains("-2P") &&
                            !code.Contains("ALV") &&
                            !code.Contains("BALC");

            return esValido;
        }
    }
}
