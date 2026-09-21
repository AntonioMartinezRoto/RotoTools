using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace RotoTools.Suite.Services
{
    /// <summary>Una hoja (elemento terminal con apertura, ver DibujoOpcionesRotoService.ObtenerElementosHoja)
    /// dentro del XML de un Dibujo, tal y como la necesita el asistente de Simulación: qué
    /// escandallo(s) tiene asociados en su psr:ConstructiveScript y qué valores reales tiene ya
    /// guardados en sus opciones (psr:Options/psr:List/psr:Option), para poder precargarlos en el
    /// intérprete (Q1 del usuario: "mixto, con opción a cambiarlo").</summary>
    public sealed class HojaSimulacion
    {
        /// <summary>Posición (1-based) en el orden en que ObtenerElementosHoja las va encontrando.
        /// No hay ningún identificador fiable en el XML (ver Etiqueta/IdHole): esta posición es lo
        /// único que distingue de forma estable una hoja de otra dentro del mismo dibujo.</summary>
        public int Indice { get; set; }

        /// <summary>Atributo "id" del psr:Hole que contiene esta hoja, si lo tiene (patrón visto en
        /// la práctica: "H"+número). Puramente informativo/decorativo para la Etiqueta: NO se usa
        /// para identificar ni filtrar hojas (ver el comentario de
        /// DibujoOpcionesRotoService.ObtenerElementosHoja sobre por qué ese patrón dio falsos
        /// positivos cuando se usó como criterio de filtrado).</summary>
        public string? IdHole { get; set; }

        /// <summary>Etiqueta lista para mostrar en la UI, p.ej. "Hoja 2 (id: H3)" o simplemente
        /// "Hoja 2" si el psr:Hole no tenía atributo "id".</summary>
        public string Etiqueta { get; set; } = "";

        /// <summary>Códigos de los escandallos asociados a esta hoja (sección "% Escandallos" de su
        /// psr:ConstructiveScript, ver ObtenerEscandallosAsociados). Habitualmente uno solo (el
        /// "escandallo constructivo" que el Responsable Técnico asoció y que a su vez llama a
        /// RO_Gestion Herraje), pero se listan todos los que haya. Vacío si la hoja no tiene ningún
        /// escandallo asociado todavía.</summary>
        public List<string> EscandallosAsociados { get; } = new();

        /// <summary>Valores reales ya guardados para esta hoja (Name → Value): la UNIÓN de los
        /// valores "por modelo" del Dibujo entero (psr:Options/psr:List en la raíz del XML, p.ej.
        /// HardwareSupplier, que siempre vive ahí) y los "por elemento" de esta hoja en concreto
        /// (mismo camino pero dentro de su propio psr:Hole), con estos últimos ganando si un mismo
        /// nombre de opción tuviera valor en los dos sitios (ver ObtenerHojas). Es el "origen de
        /// valores conocidos" mixto que pidió el usuario: EscandalloInterpreter.PrecargarValorConocido
        /// debe rellenarse con estos pares antes de arrancar la simulación de esta hoja.</summary>
        public Dictionary<string, string> ValoresReales { get; } = new();

        /// <summary>Nombres de ValoresReales que NO son un valor ya guardado en Preference sino
        /// DEDUCIDOS del atributo "value" (numérico, suma de flags) de psr:Opening de esta hoja
        /// (ver DecodificarTipoApertura/DetectarDatosDeOpening en SimulacionDatosService): a día de
        /// hoy puede contener cualquiera de "Activa", "Puerta", "Exterior", "Elevable",
        /// "CotaVariable", "Oscilobatiente", "Practicable" y "Corredera" -la tabla de flags para
        /// "value" y la lista de opciones a autorresponder con ella las dio el usuario directamente-. Se guarda aparte
        /// (en vez de mezclarse sin más con el resto de ValoresReales, que sí son valores REALES
        /// leídos de psr:Options) para que la UI pueda distinguir "deducido de Opening.value" de
        /// "valor real guardado" en la columna "Origen" del Paso 3.</summary>
        public HashSet<string> ValoresDetectadosHeuristica { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Valor crudo (texto) del atributo "value" de psr:Opening de esta hoja, tal cual
        /// viene en el XML. Null si la hoja no tiene psr:Opening, o no tiene ese atributo, o no es
        /// numérico.</summary>
        public string? TipoAperturaXml { get; set; }

        /// <summary>Descripción legible del tipo de apertura decodificado a partir de Opening.value
        /// (ver DecodificarTipoApertura), p.ej. "Practicable Izquierda + Oscilobatiente" o "Puerta".
        /// Null si no se pudo decodificar nada reconocible (o no hay psr:Opening). Ya NO se muestra
        /// como texto en el Paso 2 (ver AperturaIconoClave), pero se conserva como ToolTip del
        /// icono y por si hiciera falta en otro sitio.</summary>
        public string? DescripcionApertura { get; set; }

        /// <summary>Petición del usuario: en el Paso 2, sustituir la línea de texto de la apertura
        /// por el DIBUJO correspondiente -"usa los dibujos de la grid de generar conector de
        /// herraje, la columna apertura"-. Es la clave (x:Key) de una de las geometrías
        /// IconApertura* de Theme/RotoBrand.xaml -las MISMAS 8 que usa la columna "Apertura" de
        /// ConectorHerrajeGeneradorWindow (ver SetGridRowVm.ObtenerIconoApertura ahí)-, resuelta
        /// aquí por ObtenerIconoApertura a partir de los bits de BitsApertura. Null si la
        /// combinación de bits no se puede traducir con confianza a ninguna de esas 8 geometrías
        /// (mismos casos que DescribirTipoApertura deja sin una categoría clara: Puerta sola,
        /// Pivotante, Hoja fija, Fijo, o un valor sin interpretar) -mejor no mostrar ningún icono
        /// que mostrar uno potencialmente equivocado-.
        ///
        /// IMPORTANTE: esto NO reutiliza el cálculo de ConectorHerrajeGeneradorWindow (ver el
        /// comentario de ObtenerIconoApertura más abajo para el porqué: el dato de origen es
        /// distinto, un XML de catálogo de herraje totalmente aparte). Solo se reutilizan las
        /// GEOMETRÍAS (los dibujos en sí, para que el usuario vea el mismo lenguaje visual en los
        /// dos sitios), no el código que decide cuál mostrar.</summary>
        public string? AperturaIconoClave { get; set; }

        /// <summary>Volteo horizontal del icono (ScaleTransform.ScaleX en el XAML): mismo convenio
        /// EXACTO que SetGridRowVm.ObtenerIconoApertura en ConectorHerrajeGeneradorWindow, que NO es
        /// uniforme entre familias de icono -ver el comentario de ObtenerIconoApertura más abajo-:
        /// 1 = sin voltear, -1 = volteado horizontalmente.</summary>
        public double AperturaIconoFlip { get; set; } = 1;

        /// <summary>Para el Visibility del icono en el XAML (ver PlantillaHoja en
        /// SimulacionPage.xaml): equivalente a TieneLineaApertura pero para AperturaIconoClave en
        /// vez de DescripcionApertura.</summary>
        public bool TieneIconoApertura => !string.IsNullOrEmpty(AperturaIconoClave);

        /// <summary>Petición del usuario: en Paso 2 "solo debe aparecer si es Activa, la apertura
        /// que tiene y si es cota variable o no", CADA información en su propia línea para poder
        /// leerlo bien -de ahí que sean tres propiedades de texto independientes en vez de una sola
        /// combinada, cada una con su TextBlock propio en PlantillaHoja (SimulacionPage.xaml)-.
        /// "" si esa información concreta no está disponible (p.ej. la hoja no tiene psr:Opening, o
        /// esa opción no llegó a auto-responderse ni tiene valor real guardado).</summary>
        public string LineaActivaDetectada => ValoresReales.TryGetValue("Activa", out var activa) ? $"Activa: {activa}" : "";

        /// <summary>Ídem, para si tiene un Visibility propio que ocultar cuando no hay nada que mostrar.</summary>
        public bool TieneLineaActiva => LineaActivaDetectada.Length > 0;

        /// <summary>Ídem LineaActivaDetectada, para el tipo de apertura (DescripcionApertura).</summary>
        public string LineaAperturaDetectada => string.IsNullOrEmpty(DescripcionApertura) ? "" : $"Apertura: {DescripcionApertura}";

        public bool TieneLineaApertura => LineaAperturaDetectada.Length > 0;

        /// <summary>Ídem LineaActivaDetectada, para CotaVariable.</summary>
        public string LineaCotaVariableDetectada => ValoresReales.TryGetValue("CotaVariable", out var cotaVariable) ? $"Cota variable: {cotaVariable}" : "";

        public bool TieneLineaCotaVariable => LineaCotaVariableDetectada.Length > 0;

        /// <summary>Texto resumen (línea 1 en Paso 2, ver PlantillaHoja en SimulacionPage.xaml) para
        /// el selector de hojas: qué escandallo Constructivo llama esta hoja. Petición del usuario:
        /// ya NO se muestra en Paso 2 cuántos valores de opciones reales hay guardados -antes existía
        /// una DescripcionValoresReales para eso, ver histórico de este fichero-, así que esta es la
        /// única línea de "resumen" que queda además de las 3 de Activa/Apertura/CotaVariable.</summary>
        public string DescripcionEscandallos => EscandallosAsociados.Count == 0
            ? "Sin escandallo asociado todavía"
            : "Escandallo(s): " + string.Join(", ", EscandallosAsociados);
    }

    /// <summary>
    /// Nueva (no existía en el original ni en ningún otro módulo de la Suite): lecturas de solo
    /// lectura sobre el XML de un Dibujo que necesita el asistente de "Simulación" (ver tareas #10
    /// y #11 del plan de esa funcionalidad) — enumerar sus hojas, qué escandallo(s) tiene asociado
    /// cada una y qué valores reales de opciones tiene ya guardados. A diferencia de
    /// DibujoOpcionesRotoService/DibujoConstructivosService, esta clase NUNCA escribe nada en BBDD:
    /// la Simulación es puramente exploratoria/de diagnóstico ("localizar posibles errores"), así
    /// que no hay ninguna operación de guardado aquí, a propósito.
    ///
    /// Reutiliza (internal, mismo namespace) la infraestructura ya validada de
    /// DibujoOpcionesRotoService para leer y descomprimir el XML (LeerXmlDescomprimido) y para
    /// identificar los elementos "hoja" (ObtenerElementosHoja), y el mismo formato de
    /// psr:ConstructiveScript documentado en DibujoConstructivosService (separador de línea literal
    /// "&amp;#D;&amp;#A;", comilla literal "&amp;quot;", 5 secciones fijas empezando por "% NombreSección")
    /// para parsear -en vez de escribir- la sección "% Escandallos".
    /// </summary>
    public static class SimulacionDatosService
    {
        /// <summary>Mismo separador de línea LITERAL que usa Preference dentro de
        /// psr:ConstructiveScript (ver comentario de clase de DibujoConstructivosService).</summary>
        private const string SepLinea = "&#D;&#A;";

        /// <summary>Reconoce una línea "ESCANDALLO(&amp;quot;Codigo&amp;quot;,&amp;quot;Variables&amp;quot;);"
        /// de la sección "% Escandallos", capturando Código y Variables. Mismo formato -literal, no
        /// comillas/XML reales- que escribe DibujoConstructivosService.InsertarEscandalloEnSeccion.</summary>
        private static readonly Regex PatronLineaEscandallo = new(
            "^ESCANDALLO\\(&quot;(?<codigo>.*?)&quot;,&quot;(?<variables>.*?)&quot;\\);$",
            RegexOptions.Compiled);

        /// <summary>
        /// Enumera todas las hojas del dibujo indicado, con sus escandallos asociados y sus
        /// valores reales de opciones ya cargados (ver HojaSimulacion). Orden: el mismo en que
        /// DibujoOpcionesRotoService.ObtenerElementosHoja las va recorriendo (recorrido en
        /// profundidad del XML), que es el mismo orden que usan Añadir/Quitar Opciones y Asociar
        /// Constructivos para el modo "por elemento".
        /// </summary>
        public static List<HojaSimulacion> ObtenerHojas(string codigoDibujo)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            string xml = DibujoOpcionesRotoService.LeerXmlDescomprimido(conexion, codigoDibujo);
            return ObtenerHojasDesdeXml(xml);
        }

        /// <summary>
        /// Mismo resultado que ObtenerHojas, pero para el Paso 1 en modo "Presupuesto" (petición
        /// del usuario: "al continuar en el paso 2, será comun, cargará el Campo Buffer de la misma
        /// tabla [ContenidoPAFBlob] y el proceso será el mismo"): lee el Buffer de ContenidoPAFBlob
        /// filtrado por Numero+Version+Orden (PresupuestoDatosService.LeerXmlDescomprimido, mismo
        /// patrón de descompresión que la lectura de Dibujos) y delega el análisis del XML en
        /// ObtenerHojasDesdeXml — el MISMO parser que usa el modo Base de datos, sin ninguna
        /// diferencia una vez se tiene el XML en memoria: el formato psr: es idéntico en los dos
        /// orígenes (confirmado por el propio usuario: "el proceso será el mismo").
        /// </summary>
        public static List<HojaSimulacion> ObtenerHojasDesdePresupuesto(string numero, string version, string orden)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            string xml = PresupuestoDatosService.LeerXmlDescomprimido(conexion, numero, version, orden);
            return ObtenerHojasDesdeXml(xml);
        }

        /// <summary>Cuerpo compartido de ObtenerHojas/ObtenerHojasDesdePresupuesto: todo el análisis
        /// del XML psr: una vez ya se tiene como texto descomprimido, con independencia de si salió
        /// de Dibujos.Buffer o de ContenidoPAFBlob.Buffer.
        ///
        /// Dos diferencias reales entre esos dos orígenes, reportadas por el usuario tras probar el
        /// modo Presupuesto contra la base de datos real (el propio error "Data at the root level
        /// is invalid. Line 1, position 1" y su aviso de que la raíz cambia):
        ///
        /// (1) BOM (marca de orden de bytes, U+FEFF) como carácter LITERAL al principio del texto
        /// ya descomprimido: es justo la causa típica de ese mensaje de XDocument.Parse ("Data at
        /// the root level is invalid. Line 1, position 1") cuando el texto en sí SÍ es XML válido a
        /// partir de esa marca. DecodificarTexto (DibujoOpcionesRotoService) ya sabe quitarla
        /// cuando el resultado de SQL llega como byte[], pero NO si SqlClient lo devuelve ya como
        /// string (rama "resultado.ToString()" de LeerXmlDescomprimido/PresupuestoDatosService.
        /// LeerXmlDescomprimido) — con un Buffer generado por una vía de exportación distinta a la
        /// de un Dibujo guardado directamente (como parece ser el caso de un Presupuesto, a juzgar
        /// por la diferencia de raíz de (2)) no se puede descartar que ese BOM sobreviva ahí. Se
        /// quita aquí, de forma defensiva y sin coste para el caso normal (Dibujos, que nunca lo ha
        /// tenido): no cambia nada si no hay BOM que quitar.
        ///
        /// (2) La raíz del XML de un Presupuesto es psr:PrefItemModel, con el psr:Model real como
        /// HIJO directo -a diferencia del XML de un Dibujo de BBDD, donde la raíz YA es
        /// directamente psr:Model- (aviso textual del usuario). Importa porque ObtenerValoresReales
        /// más abajo lee las opciones "por modelo" con .Element() (hijo DIRECTO, no
        /// .Descendants()) sobre lo que aquí se llame "raiz": si se dejara "raiz" apuntando a
        /// psr:PrefItemModel en vez de al psr:Model real, esas opciones de modelo (p.ej.
        /// HardwareSupplier) no se encontrarían nunca. Se detecta por el nombre LOCAL del elemento
        /// (LocalName, que en Linq-to-XML ignora el prefijo/namespace) y, si la raíz del documento
        /// no es ya "Model", se busca el primer descendiente psr:Model y se usa como raíz real
        /// para todo el análisis siguiente -degradándose a la raíz del documento tal cual si no
        /// hubiera ningún "Model" debajo, en vez de reventar, por si algún día apareciera un
        /// tercer formato distinto de estos dos-.
        /// </summary>
        private static List<HojaSimulacion> ObtenerHojasDesdeXml(string xml)
        {
            xml = xml.TrimStart((char)0xFEFF, (char)0);

            XDocument doc = XDocument.Parse(xml);
            XElement raizDocumento = doc.Root ?? throw new InvalidOperationException(
                "El XML del dibujo está vacío o no es válido.");

            XNamespace psr = raizDocumento.Name.Namespace;
            XElement raiz = raizDocumento.Name.LocalName == "Model"
                ? raizDocumento
                : raizDocumento.Descendants(psr + "Model").FirstOrDefault() ?? raizDocumento;

            // Opciones "por modelo": psr:Model\psr:Options\psr:List\psr:Option, en la RAÍZ del XML
            // (no anidadas dentro de ningún psr:Hole/hoja) — mismo sitio donde
            // DibujoOpcionesRotoService.AplicarOpcionesRoto escribe cuando el usuario elige
            // porElemento=false, y donde SIEMPRE escribe HardwareSupplier con independencia de ese
            // modo ("la opción HardwareSupplier... siempre va al MODELO, nunca por elemento", ver
            // comentario de AplicarOpcionesRoto). Se lee una sola vez para todo el dibujo: aplica
            // igual a todas sus hojas.
            var valoresModelo = ObtenerValoresReales(raiz, psr).ToList();

            var resultado = new List<HojaSimulacion>();
            int indice = 0;
            // ObtenerElementosHoja es "internal" en DibujoOpcionesRotoService: accesible aquí sin
            // envoltorio porque este fichero vive en el mismo ensamblado y el mismo namespace
            // RotoTools.Suite.Services (ver comentario de esa clase: "internal (no solo private):
            // DibujoConstructivosService reutiliza..." — este servicio hace exactamente lo mismo).
            foreach (var elementoHoja in DibujoOpcionesRotoService.ObtenerElementosHoja(raiz, psr))
            {
                indice++;
                string? idHole = ((string?)elementoHoja.Parent?.Attribute("id"))?.Trim();

                var hoja = new HojaSimulacion
                {
                    Indice = indice,
                    IdHole = string.IsNullOrWhiteSpace(idHole) ? null : idHole,
                };
                hoja.Etiqueta = hoja.IdHole == null ? $"Hoja {indice}" : $"Hoja {indice} (id: {hoja.IdHole})";

                hoja.EscandallosAsociados.AddRange(ObtenerEscandallosAsociados(elementoHoja, psr));

                // Se cargan primero los valores "por modelo" y LUEGO los "por elemento" de esta hoja
                // concreta, para que estos últimos ganen si el mismo nombre de opción tuviera valor
                // en los dos sitios (no debería pasar en la práctica -un Dibujo se gestiona con un
                // modo u otro, no mezclado- pero así queda cubierto igualmente sin necesidad de
                // detectar explícitamente qué modo se usó: cada Dibujo solo tendrá datos reales en
                // uno de los dos sitios, y aquí se cogen los que efectivamente haya).
                foreach (var (nombre, valor) in valoresModelo)
                    hoja.ValoresReales[nombre] = valor;
                foreach (var (nombre, valor) in ObtenerValoresReales(elementoHoja, psr))
                    hoja.ValoresReales[nombre] = valor;

                // Petición del usuario: ayudar a precargar "Activa" (y mostrar el tipo de apertura,
                // informativo) a partir del nodo psr:Opening de esta hoja. Se hace DESPUÉS de cargar
                // los valores reales de arriba, precisamente para poder comprobar si "Activa" ya
                // tiene un valor real guardado y no pisarlo con la heurística (ver
                // DetectarDatosDeOpening).
                DetectarDatosDeOpening(hoja, elementoHoja, psr);

                resultado.Add(hoja);
            }

            return resultado;
        }

        /// <summary>Códigos de los ESCANDALLO("Codigo","Variables"); de la sección "% Escandallos"
        /// del psr:ConstructiveScript de esta hoja (vacío si no tiene ese nodo, o no tiene esa
        /// sección, o la sección está vacía). Solo lectura: no modifica nada.</summary>
        private static IEnumerable<string> ObtenerEscandallosAsociados(XElement elementoHoja, XNamespace psr)
        {
            XElement? script = elementoHoja.Element(psr + "ConstructiveScript");
            if (script == null) yield break;

            var lineas = script.Value.Split(new[] { SepLinea }, StringSplitOptions.None);

            int idxEscandallos = Array.FindIndex(lineas, l => l.Trim() == "% Escandallos");
            if (idxEscandallos < 0) yield break;

            int idxSiguienteSeccion = Array.FindIndex(lineas, idxEscandallos + 1, l => l.StartsWith("% ", StringComparison.Ordinal));
            if (idxSiguienteSeccion < 0) idxSiguienteSeccion = lineas.Length;

            for (int i = idxEscandallos + 1; i < idxSiguienteSeccion; i++)
            {
                var m = PatronLineaEscandallo.Match(lineas[i].Trim());
                if (m.Success) yield return m.Groups["codigo"].Value;
            }
        }

        /// <summary>
        /// Flags (bits) que puede sumar el atributo "value" de psr:Opening, tal y como los dio el
        /// usuario directamente (nombres originales "ta..."/"ta..." en inglés y español que usa el
        /// software externo que genera este XML -no RotoTools/PrefSuite-, ver el comentario de
        /// DibujoOpcionesRotoService.ObtenerElementosHoja sobre ese mismo nodo; aquí solo se copian
        /// los valores numéricos, con un nombre único por bit). "value" es la SUMA de los flags que
        /// aplican a esa apertura -por eso son potencias de 2 (o combinaciones ya sumadas, como
        /// Practicable=Izquierda|Derecha o Corredera=Arriba|Abajo|Izquierda|Derecha)-, así que se
        /// decodifica con AND a nivel de bit contra cada flag individual, no por igualdad.
        /// </summary>
        private static class BitsApertura
        {
            public const int Fijo = 0;                    // taFijo / taFixed
            public const int PracticableIzquierda = 1;     // taPracticableIzquierda / taTurnLeft
            public const int PracticableDerecha = 2;       // taPracticableDerecha / taTurnRight
            public const int Practicable = 3;               // taPracticable / taTurn (Izquierda|Derecha)
            public const int HojaFija = 4;                  // taHojaFija / taFixedSash
            public const int Puerta = 8;                    // taPuerta / taDoor
            public const int OscilobatienteInferior = 16;   // taOscilobatienteInferior / taLowerTilt
            public const int OscilobatienteSuperior = 32;   // taOscilobatienteSuperior / taUpperTilt
            public const int Oscilobatiente = 48;            // taOscilobatiente / taTilt (Inferior|Superior)
            public const int Pivotante = 64;                 // taPivotante / taPivoting
            public const int Arriba = 256;                   // taArriba / taUp
            public const int Abajo = 512;                    // taAbajo / taDown
            public const int Izquierda = 1024;               // taIzquierda / taLeft
            public const int Derecha = 2048;                 // taDerecha / taRight
            public const int Corredera = 3840;               // taCorredera / taSliding (Arriba|Abajo|Izquierda|Derecha)
            public const int Exterior = 4096;                // taExterior / taOuter
            public const int Elevable = 8192;                // taElevable / taLift
            public const int Activa = 16384;                 // taActiva / taActive
            public const int CotaVariable = 32768;           // taCotaVariable / taVariableHandlePosition
            public const int Doble = 65536;                  // taDoble / taDouble
        }

        /// <summary>
        /// Decodifica psr:Opening de esta hoja (confirmado por el usuario: "value" es la SUMA de
        /// los flags de BitsApertura) para: (a) precargar automáticamente las pseudo-opciones que
        /// el usuario pidió que se autorrespondan con estos datos -Activa, Puerta, Exterior,
        /// Elevable, CotaVariable (bit taCotaVariable=32768), Oscilobatiente, Practicable y
        /// Corredera- en vez de preguntarlas de forma interactiva, y (b) construir
        /// HojaSimulacion.DescripcionApertura para Paso 2.
        ///
        /// psr:Opening es HERMANO de psr:Element (el propio "elementoHoja"), los dos hijos directos
        /// del mismo psr:Hole -no está anidado dentro de elementoHoja-, así que hay que subir al
        /// padre para encontrarlo (mismo nodo que ya exige DibujoOpcionesRotoService.ObtenerElementosHoja
        /// para considerar "hoja" a un elemento: "if (hole.Element(psr + Opening) == null) continue;"),
        /// así que aquí SIEMPRE existe para cualquier elementoHoja que llegue a este punto.
        ///
        /// Nunca pisa un valor que ya viniera de psr:Options (ver AsignarSiNoHayValorReal): un valor
        /// REAL guardado en Preference siempre gana a lo deducido aquí. Si "value" no está presente
        /// o no es numérico, no se detecta ni se autorresponde nada -degradación segura- y las
        /// opciones siguen preguntándose de forma interactiva como hasta ahora.
        /// </summary>
        private static void DetectarDatosDeOpening(HojaSimulacion hoja, XElement elementoHoja, XNamespace psr)
        {
            XElement? opening = elementoHoja.Parent?.Element(psr + "Opening");
            if (opening == null) return;

            string? textoValor = (string?)opening.Attribute("value");
            if (string.IsNullOrWhiteSpace(textoValor)) return;
            if (!int.TryParse(textoValor, NumberStyles.Any, CultureInfo.InvariantCulture, out int valor)) return;

            hoja.TipoAperturaXml = textoValor;
            hoja.DescripcionApertura = DescribirTipoApertura(valor);
            (hoja.AperturaIconoClave, hoja.AperturaIconoFlip) = ObtenerIconoApertura(valor);

            AsignarSiNoHayValorReal(hoja, "Activa", (valor & BitsApertura.Activa) != 0 ? "Sí" : "No");
            AsignarSiNoHayValorReal(hoja, "Puerta", (valor & BitsApertura.Puerta) != 0 ? "Sí" : "No");
            AsignarSiNoHayValorReal(hoja, "Exterior", (valor & BitsApertura.Exterior) != 0 ? "Sí" : "No");
            AsignarSiNoHayValorReal(hoja, "Elevable", (valor & BitsApertura.Elevable) != 0 ? "Sí" : "No");
            AsignarSiNoHayValorReal(hoja, "CotaVariable", (valor & BitsApertura.CotaVariable) != 0 ? "Sí" : "No");

            // Oscilobatiente: la lista fija de valores de esta pseudo-opción solo admite "Inferior"
            // o "Ninguna" (confirmado antes por el usuario), sin distinguir Inferior/Superior/ambos
            // -> cualquiera de esos dos bits presente se traduce como "Inferior".
            AsignarSiNoHayValorReal(hoja, "Oscilobatiente",
                (valor & BitsApertura.Oscilobatiente) != 0 ? "Inferior" : "Ninguna");

            // Practicable: lista fija "Izquierda"/"Derecha"/"Ninguna" (sin combinado "ambas"). Si
            // algún día apareciera un "value" con los dos bits a la vez, se prioriza Izquierda -caso
            // no visto hasta ahora, y de todas formas queda marcado como "detectado" en Paso 3 (ver
            // ValoresDetectadosHeuristica) para poder revisarlo con el dibujo real.
            bool practicableIzq = (valor & BitsApertura.PracticableIzquierda) != 0;
            bool practicableDer = (valor & BitsApertura.PracticableDerecha) != 0;
            AsignarSiNoHayValorReal(hoja, "Practicable",
                practicableIzq ? "Izquierda" : practicableDer ? "Derecha" : "Ninguna");

            // Corredera: lista fija "Izquierda"/"Derecha"/"IzquierdaDerecha"/"Ninguna" -solo
            // dirección horizontal-. Los bits Arriba/Abajo (verticales) no tienen hueco en esa
            // lista: si el único movimiento detectado es vertical (sin Izquierda/Derecha) no se
            // puede resolver de forma fiable con los valores permitidos, así que se deja SIN
            // autorresponder -sigue preguntándose- en vez de forzar un valor que podría ser
            // incorrecto.
            bool correderaIzq = (valor & BitsApertura.Izquierda) != 0;
            bool correderaDer = (valor & BitsApertura.Derecha) != 0;
            if (correderaIzq && correderaDer)
                AsignarSiNoHayValorReal(hoja, "Corredera", "IzquierdaDerecha");
            else if (correderaIzq)
                AsignarSiNoHayValorReal(hoja, "Corredera", "Izquierda");
            else if (correderaDer)
                AsignarSiNoHayValorReal(hoja, "Corredera", "Derecha");
            else if ((valor & BitsApertura.Corredera) == 0)
                AsignarSiNoHayValorReal(hoja, "Corredera", "Ninguna");
            // else: solo bits verticales (Arriba/Abajo) sin Izquierda/Derecha -> no se autorresponde.
        }

        /// <summary>Guarda "valorDetectado" en ValoresReales[opcion] y lo marca en
        /// ValoresDetectadosHeuristica, PERO solo si "opcion" no tuviera ya un valor real guardado
        /// (leído de psr:Options): ese siempre debe ganar a lo deducido de Opening.value.</summary>
        private static void AsignarSiNoHayValorReal(HojaSimulacion hoja, string opcion, string valorDetectado)
        {
            if (hoja.ValoresReales.ContainsKey(opcion)) return;
            hoja.ValoresReales[opcion] = valorDetectado;
            hoja.ValoresDetectadosHeuristica.Add(opcion);
        }

        /// <summary>Texto legible del tipo de apertura para Paso 2 (petición del usuario: "que solo
        /// ponga por ahora si es Activa y la apertura"), a partir de los mismos bits de "value" que
        /// decodifica DetectarDatosDeOpening -pero aquí solo para MOSTRAR, no para autorresponder,
        /// así que no hace falta evitar ambigüedades (p.ej. si hay Practicable Izquierda Y Derecha a
        /// la vez, aquí sí se listan las dos).</summary>
        private static string DescribirTipoApertura(int valor)
        {
            var partes = new List<string>();

            if ((valor & BitsApertura.Puerta) != 0) partes.Add("Puerta");

            bool practicableIzq = (valor & BitsApertura.PracticableIzquierda) != 0;
            bool practicableDer = (valor & BitsApertura.PracticableDerecha) != 0;
            if (practicableIzq && practicableDer) partes.Add("Practicable (Izquierda y Derecha)");
            else if (practicableIzq) partes.Add("Practicable Izquierda");
            else if (practicableDer) partes.Add("Practicable Derecha");

            if ((valor & BitsApertura.Oscilobatiente) != 0) partes.Add("Oscilobatiente");
            if ((valor & BitsApertura.Pivotante) != 0) partes.Add("Pivotante");

            bool correderaIzq = (valor & BitsApertura.Izquierda) != 0;
            bool correderaDer = (valor & BitsApertura.Derecha) != 0;
            bool correderaVert = (valor & (BitsApertura.Arriba | BitsApertura.Abajo)) != 0;
            if (correderaIzq || correderaDer || correderaVert)
            {
                string direccion = correderaIzq && correderaDer ? "Izquierda y Derecha"
                    : correderaIzq ? "Izquierda"
                    : correderaDer ? "Derecha"
                    : "vertical";
                partes.Add($"Corredera {direccion}");
            }

            if (partes.Count == 0 && (valor & BitsApertura.HojaFija) != 0) partes.Add("Hoja fija");
            if (partes.Count == 0) partes.Add(valor == BitsApertura.Fijo ? "Fijo" : $"valor {valor} sin interpretar");

            return string.Join(" + ", partes);
        }

        /// <summary>
        /// Traduce los mismos bits de BitsApertura que ya decodifica DescribirTipoApertura a la
        /// clave (x:Key) de una de las 8 geometrías IconApertura* de Theme/RotoBrand.xaml -petición
        /// del usuario: "usa los dibujos de la grid de generar conector de herraje, la columna
        /// apertura"-, más si hay que voltearla horizontalmente.
        ///
        /// Esas mismas 8 geometrías ya las usa ConectorHerrajeGeneradorWindow.SetGridRowVm.
        /// ObtenerIconoApertura para su columna "Apertura", pero a partir de un dato de origen
        /// TOTALMENTE DISTINTO: ese cálculo parte de un "Set.Opening" con campos turn/tilt/left/
        /// right/sliding/lift/outer/bottom sueltos, leídos de un XML de CATÁLOGO DE HERRAJE aparte
        /// (namespace hw:, ver XmlLoader.GetSetOpening/ConectorHerrajeMenu.GetOpeningOptions en el
        /// proyecto original) -un documento que la Simulación nunca abre-, mientras que aquí solo
        /// se tiene el bitmask "value" de psr:Opening (namespace psr:, el XML del propio Dibujo, ver
        /// BitsApertura). No hay ningún dato compartido entre los dos para reutilizar literalmente
        /// el mismo cálculo -reutilizar el código de ConectorHerraje tal cual exigiría primero
        /// sintetizar un Opening al estilo hw: a partir de este bitmask, lo cual sería en sí mismo
        /// una traducción nueva, no una reutilización real-, así que esta función es una traducción
        /// NUEVA de estos bits concretos a la MISMA familia de iconos, con el mismo criterio de
        /// prioridad ya validado por el usuario para DescribirTipoApertura (Izquierda gana si algún
        /// día aparecen los dos bits de una misma familia a la vez).
        ///
        /// Devuelve (null, 1) -sin icono- en cualquier combinación que DescribirTipoApertura tampoco
        /// distingue con una categoría propia (Puerta sola, Pivotante, Hoja fija, Fijo, o un valor
        /// sin interpretar): mejor no mostrar icono que mostrar uno potencialmente equivocado.
        ///
        /// El volteo (flip) copia el convenio EXACTO de SetGridRowVm.ObtenerIconoApertura, que NO es
        /// uniforme entre familias -se verificó línea a línea contra ese código, no se dedujo a
        /// ojo-: en Practicable/Oscilobatiente, Izquierda es la orientación SIN voltear (flip=1) y
        /// Derecha la volteada (flip=-1); en Corredera/OsciloCorredera/Elevable es AL REVÉS,
        /// Izquierda es la volteada (flip=-1) y Derecha la que no se toca (flip=1). Abatible y
        /// CorrederaIzqDcha no tienen variante Izq/Der en el enum original (un único icono), así que
        /// aquí tampoco se voltean (flip=1 siempre).
        /// </summary>
        private static (string? clave, double flip) ObtenerIconoApertura(int valor)
        {
            bool practicableIzq = (valor & BitsApertura.PracticableIzquierda) != 0;
            bool practicableDer = (valor & BitsApertura.PracticableDerecha) != 0;
            bool oscilobatiente = (valor & BitsApertura.Oscilobatiente) != 0;
            bool correderaIzq = (valor & BitsApertura.Izquierda) != 0;
            bool correderaDer = (valor & BitsApertura.Derecha) != 0;
            bool exterior = (valor & BitsApertura.Exterior) != 0;
            bool elevable = (valor & BitsApertura.Elevable) != 0;

            if (practicableIzq || practicableDer)
            {
                // Prioridad Izquierda si algún día aparecen los dos bits a la vez (mismo criterio
                // que ya aplica DetectarDatosDeOpening para autorresponder la pseudo-opción
                // "Practicable").
                double flip = practicableIzq ? 1 : -1;

                // Con Turn (Practicable) + Tilt (Oscilobatiente) a la vez: es un Oscilobatiente
                // (tilt-and-turn), no un Practicable a secas -mismo criterio que el enum original,
                // donde OscilobatienteIzquierdaInt/DerechaInt exigen turn+tilt+left/right-. El bit
                // Exterior no tiene variante Oscilobatiente en el enum original, así que aquí se
                // ignora en ese caso (degradación razonable: se muestra el Oscilobatiente normal).
                if (oscilobatiente) return ("IconAperturaOscilobatiente", flip);
                return (exterior ? "IconAperturaPracticableExterior" : "IconAperturaPracticable", flip);
            }

            // Tilt a secas, sin Turn ni Corredera: Abatible (bascula desde abajo, sin giro). Con
            // Corredera combinado es OsciloCorredera, tratado más abajo, no Abatible.
            if (oscilobatiente && !correderaIzq && !correderaDer)
                return ("IconAperturaAbatible", 1);

            if (correderaIzq || correderaDer)
            {
                if (correderaIzq && correderaDer) return ("IconAperturaCorrederaDoble", 1);

                // Convenio de flip OPUESTO al de Practicable/Oscilobatiente (ver el comentario de
                // esta función): aquí Izquierda SÍ se voltea.
                double flip = correderaIzq ? -1 : 1;
                if (oscilobatiente) return ("IconAperturaOsciloCorredera", flip);
                if (elevable) return ("IconAperturaElevable", flip);
                return ("IconAperturaCorrederaSimple", flip);
            }

            return (null, 1);
        }

        /// <summary>Pares Name/Value de psr:Options/psr:List/psr:Option de "contenedor": los valores
        /// REALES que ya tiene guardados en Preference, ahora mismo. Vacío si "contenedor" no tiene
        /// psr:Options todavía. Genérico a propósito -no solo "de esta hoja"-: ObtenerHojas lo llama
        /// tanto con cada elementoHoja (opciones "por elemento") como con la raíz del XML del propio
        /// Dibujo (opciones "por modelo": mismo psr:Options/psr:List pero fuera de cualquier
        /// psr:Hole, donde AplicarOpcionesRoto escribe cuando porElemento=false y donde SIEMPRE
        /// escribe HardwareSupplier) — misma forma de XML en los dos sitios, ver
        /// DibujoOpcionesRotoService.AplicarOpcionesYNivelRotoEnContenedor.</summary>
        private static IEnumerable<(string Nombre, string Valor)> ObtenerValoresReales(XElement contenedor, XNamespace psr)
        {
            XElement? list = contenedor.Element(psr + "Options")?.Element(psr + "List");
            if (list == null) yield break;

            foreach (var opcion in list.Elements(psr + "Option"))
            {
                string? nombre = (string?)opcion.Attribute("name");
                string? valor = (string?)opcion.Attribute("value");
                if (!string.IsNullOrEmpty(nombre) && valor != null)
                    yield return (nombre, valor);
            }
        }
    }
}
