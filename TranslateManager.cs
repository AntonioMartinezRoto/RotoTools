using RotoEntities;

namespace RotoTools
{
    public static class TranslateManager
    {
        public static bool PermitirTraduccionesEnConectorEscandallos { get; set; } = false;
        public static bool AplicarTraduccion { get; set; } = false;
        public static Traducciones TraduccionesActuales { get; set; }
    }
}
