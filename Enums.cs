
namespace RotoTools
{
    public static class Enums
    {
        public enum enumOpeningType
        {
            PracticableIzquierdaInt = 1,
            PracticableDerechaInt = 2,
            OscilobatienteIzquierdaInt = 3,
            OscilobatienteDerechaInt = 4,
            CorrederaDerecha = 5,
            CorrederaIzquierda = 6,
            CorrederaIzqDcha = 7,
            Abatible = 8,
            OsciloCorrederaDerecha = 9,
            OsciloCorrederaIzquierda = 10,
            ElevableIzquierda = 11,
            ElevableDerecha = 12,
            PracticableIzquierdaExt = 13,
            PracticableDerechaExt = 14,
        }
        public enum enumHardwareType
        {
            PVC = 1,
            Aluminio = 2,
            PAX = 3
        }
        public enum enumRotoTipoEscandallo
        {
            Desconocido = 0,
            PVC = 1,
            Aluminio = 2,
            GestionGeneral = 3,
            GestionManillas = 4,
            GestionBombillos = 5,
            PersonalizacionClientes = 6
        }
        public enum enumConfiguracionManillasFKS
        {
            Normalizada = 1,
            SoloFks = 2,
            NormalizadaYFks = 3
        }
        public enum enumTipoXml
        {
            origen = 0,
            nuevo = 1
        }
        public enum enumTipoDiferencia
        {
            opcionGlobal = 1,
            descripcionFitting = 2,
            cambioReferenciaOpcion = 3,
            fittingNoExistente = 4,
            manufacturerDistinto = 5,
            opcionFittingNoGenerada = 6,
            referenciaNoGeneradaFitting = 7,
            setsDiferentes = 8,
            atributosSetDiferente = 9,
            openingSetDiferente = 10,
            setDescriptionDiferente = 11,
            colorDiferente = 12,
            fittingGroupDiferente = 13,
            locationFittingDistinto = 14,
            lengthFittingDistinto = 15,
            supplierDistinto = 16,
            grupoFittings = 17,
            grupoSets = 18,
            grupoColourMaps = 19,
            colourNoExistente = 20,
            articuloNoExistenteEnColor = 21,
            grupoOpciones = 22,
            opcionGlobalNueva = 23,
            valorOpcionGlobalModificada = 24
        }
        public enum enumSeveridadDiferencia
        {
            warning = 1,
            error = 2
        }
        public enum enumOrigenXMLDiferencia
        {
            anterior = 1,
            actual = 2,
            ambos = 3
        }
    }
}
