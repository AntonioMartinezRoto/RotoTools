using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace RotoTools.Suite.Services
{
    /// <summary>
    /// Fila de la tabla Dibujos (nueva: el proyecto original nunca ha necesitado listar/buscar
    /// dibujos, solo abrirlos desde Preference). "Sistema" se llama así en C# (en vez de "System",
    /// el nombre real de la columna) para no chocar con el namespace System.
    /// </summary>
    public class DibujoRow
    {
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Sistema { get; set; } = "";
        public string Nivel1 { get; set; } = "";
        public string Nivel2 { get; set; } = "";
        public string Nivel3 { get; set; } = "";
        public string Nivel4 { get; set; } = "";
        public string Nivel5 { get; set; } = "";
    }

    /// <summary>
    /// Nodo del árbol de carpetas de Dibujos (Nivel1..5), mismo criterio que MaterialTreeNode en
    /// Cam3DModels.cs: Codigo == null → carpeta, Codigo != null → hoja/dibujo.
    /// </summary>
    public class DibujoTreeNode : System.ComponentModel.INotifyPropertyChanged
    {
        public string Texto { get; set; } = "";
        public string? Codigo { get; set; }
        public bool EsHoja => Codigo != null;
        public List<DibujoTreeNode> Hijos { get; } = new();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { if (_isExpanded != value) { _isExpanded = value; OnPropertyChanged(); } }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Nodo del árbol de carpetas de la tabla OPCIONES (NIVEL1..NIVEL5): a diferencia de
    /// DibujoTreeNode, aquí TODOS los nodos son "carpeta" (no hay distinción hoja/carpeta como con
    /// los dibujos), porque en Preference cualquier nivel del árbol de opciones puede ser el
    /// destino elegido, tenga o no subcarpetas debajo. Cada nodo guarda en Ruta los cajones
    /// (Nivel1..Nivel_n) desde la raíz hasta él mismo, que es justo lo que necesita
    /// ConstruirNivelCarpeta para completar el resto de cajones vacíos hasta el ancho fijo del XML.
    /// </summary>
    public class OpcionCarpetaTreeNode : System.ComponentModel.INotifyPropertyChanged
    {
        public string Nombre { get; set; } = "";
        public string[] Ruta { get; set; } = Array.Empty<string>();
        public List<OpcionCarpetaTreeNode> Hijos { get; } = new();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { if (_isExpanded != value) { _isExpanded = value; OnPropertyChanged(); } }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    /// <summary>Resultado de aplicar las opciones ROTO a un único Dibujo, para el resumen final.</summary>
    public class ResultadoAplicarOpciones
    {
        public string Codigo { get; set; } = "";
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = "";
        public int OpcionesAnadidas { get; set; }
        public int OpcionesYaExistian { get; set; }
        public int ElementosModificados { get; set; }
        public bool NivelRotoAnadido { get; set; }
    }

    /// <summary>
    /// Nueva (no existía en el original ni en ningún otro módulo de la Suite): añade una carpeta
    /// elegida por el usuario (psr:Model\psr:Options\psr:Levels\psr:Level, ver
    /// ConstruirNivelCarpeta) y una lista de opciones (psr:Model\psr:Options\psr:List\psr:Option)
    /// al XML de un Dibujo guardado en BBDD (tabla Dibujos, columna Buffer, comprimido). No se
    /// asume que la carpeta se llame siempre "ROTO": los usuarios de Preference pueden mover esas
    /// opciones a otro sitio, así que la carpeta es un parámetro (nivelCarpeta) de
    /// AplicarOpcionesRoto, no una constante; se elige en un árbol construido con los datos reales
    /// de la tabla OPCIONES (ver GetArbolCarpetasOpciones/OpcionCarpetaTreeNode) en vez de
    /// escribirse a mano, para evitar errores de ruta. El modo "por elemento" hace lo mismo pero dentro de
    /// cada elemento "hoja" (psr:Hole sin psr:Holes anidado: paños/hojas terminales, no
    /// subdivisiones intermedias) en vez de en el nodo raíz psr:Model.
    ///
    /// Reutiliza RotoTools.Helpers.GetConnectionString() (de solo lectura) para la cadena de
    /// conexión, pero toda la lógica de esta funcionalidad es nueva y vive aquí, no en el proyecto
    /// original: nunca se toca RotoTools.csproj.
    ///
    /// Descompresión: se usa tal cual la función SQL indicada, [zlib].[UnzipBLOB](Buffer). Como no
    /// hay ninguna referencia previa en el código a la función inversa (comprimir), su nombre se
    /// descubre en tiempo de ejecución consultando sys.objects del esquema "zlib" (ver
    /// ResolverFuncionComprimir) en vez de asumir un nombre fijo que podría no existir. Antes de
    /// escribir nada en BBDD, se verifica que comprimir+descomprimir con esa función reproduce
    /// exactamente el XML modificado (ver VerificarRoundTrip): si la función detectada no es la
    /// correcta, la operación falla con un mensaje claro en vez de guardar un Buffer corrupto.
    /// </summary>
    public static class DibujoOpcionesRotoService
    {
        private static readonly XNamespace PsrNs = "http://www.preference.com/XMLSchemas/2006/Serialization";
        private static readonly XNamespace XsiNs = "http://www.w3.org/2001/XMLSchema-instance";

        private static string? _nombreFuncionComprimirCache;

        #region Listado de Dibujos (árbol + grid)

        public static List<DibujoRow> GetDibujos()
        {
            var lista = new List<DibujoRow>();

            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            using var cmd = new SqlCommand(
                "SELECT Codigo, Descripcion, System, Nivel1, Nivel2, Nivel3, Nivel4, Nivel5 " +
                "FROM Dibujos ORDER BY Nivel1, Nivel2, Nivel3, Nivel4, Nivel5, Codigo", conexion);

            conexion.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new DibujoRow
                {
                    Codigo = reader["Codigo"]?.ToString()?.Trim() ?? "",
                    Descripcion = reader["Descripcion"]?.ToString()?.Trim() ?? "",
                    Sistema = reader["System"]?.ToString()?.Trim() ?? "",
                    Nivel1 = reader["Nivel1"]?.ToString()?.Trim() ?? "",
                    Nivel2 = reader["Nivel2"]?.ToString()?.Trim() ?? "",
                    Nivel3 = reader["Nivel3"]?.ToString()?.Trim() ?? "",
                    Nivel4 = reader["Nivel4"]?.ToString()?.Trim() ?? "",
                    Nivel5 = reader["Nivel5"]?.ToString()?.Trim() ?? ""
                });
            }

            return lista;
        }

        #endregion

        #region Árbol de carpetas de Opciones (Nivel1..5)

        /// <summary>
        /// Construye el árbol de carpetas (Nivel1..Nivel5) a partir de TODAS las filas de la
        /// tabla OPCIONES, sin filtrar por nombre: a diferencia de ConfiguradorOpcionesEditorWindow
        /// (que solo necesita listar las Opciones con prefijo "RO_" para editarlas), aquí el
        /// objetivo es dejar elegir cualquier carpeta existente en el árbol de opciones de
        /// Preference, porque el usuario puede haber movido las opciones de ROTO (o cualquier
        /// otra) a una carpeta cuyo nombre no tenga por qué empezar por "RO_" -el nombre de la
        /// Opción y la carpeta en la que vive son cosas independientes-, así que restringir por
        /// "RO_" dejaría fuera carpetas válidas.
        /// </summary>
        public static List<OpcionCarpetaTreeNode> GetArbolCarpetasOpciones()
        {
            var filas = new List<string[]>();

            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            using var cmd = new SqlCommand(@"
SELECT DISTINCT NIVEL1, NIVEL2, NIVEL3, NIVEL4, NIVEL5
FROM OPCIONES
ORDER BY NIVEL1, NIVEL2, NIVEL3, NIVEL4, NIVEL5", conexion);

            conexion.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                filas.Add(new[]
                {
                    reader["NIVEL1"]?.ToString()?.Trim() ?? "",
                    reader["NIVEL2"]?.ToString()?.Trim() ?? "",
                    reader["NIVEL3"]?.ToString()?.Trim() ?? "",
                    reader["NIVEL4"]?.ToString()?.Trim() ?? "",
                    reader["NIVEL5"]?.ToString()?.Trim() ?? ""
                });
            }

            var raiz = new List<OpcionCarpetaTreeNode>();
            foreach (var fila in filas)
            {
                var hijosActuales = raiz;
                var rutaActual = new List<string>();

                foreach (var nivelRaw in fila)
                {
                    if (string.IsNullOrEmpty(nivelRaw)) break; // cajón vacío: fin de esta rama

                    rutaActual.Add(nivelRaw);

                    var nodo = hijosActuales.FirstOrDefault(n => string.Equals(n.Nombre, nivelRaw, StringComparison.Ordinal));
                    if (nodo == null)
                    {
                        nodo = new OpcionCarpetaTreeNode { Nombre = nivelRaw, Ruta = rutaActual.ToArray() };
                        hijosActuales.Add(nodo);
                    }

                    hijosActuales = nodo.Hijos;
                }
            }

            return raiz;
        }

        #endregion

        #region Carga del XML de opciones a añadir

        /// <summary>
        /// Carga el fichero EXACTAMENTE igual que ConectorHerrajePage.LoadXml / CamPage /
        /// TraduccionPage (mismo XML general de herrajes: XmlDocument + XmlNamespaceManager con
        /// el namespace "hw" del esquema, y RotoTools.XmlLoader para leerlo — no XmlSerializer),
        /// y de ese XML solo se usa el nodo hw:Options (RotoTools.XmlLoader.LoadDocOptions,
        /// que hace justamente doc.SelectNodes("//hw:Options/hw:Option", nsmgr)), que es donde
        /// está definida la lista de opciones. Cada hw:Option se convierte en un psr:Option con
        /// RotoTools.OpcionHelper.Crear (igual que en el resto del proyecto: nombre con el
        /// prefijo "RO_", más la traducción activa si TranslateManager.AplicarTraduccion está
        /// puesta), tomando el primer hw:Value del Option como valor — en este XML general cada
        /// opción trae un único valor.
        /// </summary>
        public static List<XElement> CargarOpcionesDesdeXml(string rutaXml)
        {
            var doc = new XmlDocument();
            doc.Load(rutaXml);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

            var loader = new RotoTools.XmlLoader(nsmgr);
            List<RotoEntities.Option>? opcionesHw = loader.LoadDocOptions(doc);
            if (opcionesHw == null) return new List<XElement>();

            var resultado = new List<XElement>();
            foreach (var opcionHw in opcionesHw)
            {
                if (string.IsNullOrWhiteSpace(opcionHw.Name)) continue;

                string? valor = opcionHw.ValuesList?.FirstOrDefault()?.Valor;
                if (valor == null) continue;

                RotoEntities.Option opcionRo = RotoTools.OpcionHelper.Crear(opcionHw.Name, valor);

                resultado.Add(new XElement(PsrNs + "Option",
                    new XAttribute(XsiNs + "type", "psr:typeOption"),
                    new XAttribute("name", opcionRo.Name),
                    new XAttribute("value", opcionRo.Value)));
            }

            return resultado;
        }

        #endregion

        #region Aplicar opciones a un Dibujo

        /// <summary>
        /// Ancho fijo (en "cajones" de carpeta) del valor de psr:Level en este XML: 6 cajones
        /// separados por exactamente 5 "\", estén o no todos ocupados (confirmado contando las
        /// barras en varios ejemplos aportados: "ROTO\\\\\", "01 COSTES ADICIONALES\\\\\",
        /// "SERIES PVC\COLORES BASE/JUNTAS\\\\\", siempre 5 en total). No es una convención de
        /// RotoTools: la impone el formato del propio XML de Preference.
        /// </summary>
        private const int NumeroCajonesCarpeta = 6;

        /// <summary>
        /// Construye el valor de psr:Level a partir de la carpeta elegida por el usuario en el
        /// árbol (ver OpcionCarpetaTreeNode/GetArbolCarpetasOpciones): ya no se acepta una ruta
        /// escrita a mano (se prestaba a errores de tecleo, p.ej. barras de más/de menos), así que
        /// aquí solo queda completar con cajones vacíos la Ruta del nodo elegido hasta los
        /// NumeroCajonesCarpeta cajones que exige el formato fijo del XML.
        /// </summary>
        public static string ConstruirNivelCarpeta(OpcionCarpetaTreeNode? carpeta)
        {
            if (carpeta == null || carpeta.Ruta.Length == 0)
                throw new ArgumentException("Selecciona en el árbol la carpeta donde añadir las opciones.");

            var todos = carpeta.Ruta
                .Concat(Enumerable.Repeat(string.Empty, NumeroCajonesCarpeta - carpeta.Ruta.Length));
            return string.Join("\\", todos);
        }

        public static ResultadoAplicarOpciones AplicarOpcionesRoto(string codigoDibujo, List<XElement> opcionesFuente, bool porElemento, string nivelCarpeta)
        {
            var resultado = new ResultadoAplicarOpciones { Codigo = codigoDibujo };

            try
            {
                using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
                conexion.Open();

                string xmlOriginal = LeerXmlDescomprimido(conexion, codigoDibujo);
                XDocument doc = XDocument.Parse(xmlOriginal);
                XElement raiz = doc.Root ?? throw new InvalidOperationException(
                    "El XML del dibujo está vacío o no es válido.");
                XNamespace psr = raiz.Name.Namespace;

                if (!porElemento)
                {
                    var (anadidas, yaExistian, nivelAnadido) = AplicarOpcionesYNivelRotoEnContenedor(raiz, psr, opcionesFuente, nivelCarpeta);
                    resultado.OpcionesAnadidas = anadidas;
                    resultado.OpcionesYaExistian = yaExistian;
                    resultado.NivelRotoAnadido = nivelAnadido;
                    resultado.ElementosModificados = 1;
                }
                else
                {
                    int totalAnadidas = 0, totalYaExistian = 0, elementos = 0;
                    bool algunNivelAnadido = false;

                    foreach (var elementoHoja in ObtenerElementosHoja(raiz, psr))
                    {
                        var (anadidas, yaExistian, nivelAnadido) = AplicarOpcionesYNivelRotoEnContenedor(elementoHoja, psr, opcionesFuente, nivelCarpeta);
                        totalAnadidas += anadidas;
                        totalYaExistian += yaExistian;
                        if (nivelAnadido) algunNivelAnadido = true;
                        elementos++;
                    }

                    resultado.OpcionesAnadidas = totalAnadidas;
                    resultado.OpcionesYaExistian = totalYaExistian;
                    resultado.ElementosModificados = elementos;
                    resultado.NivelRotoAnadido = algunNivelAnadido;

                    if (elementos == 0)
                    {
                        resultado.Exito = false;
                        resultado.Mensaje = "No se ha encontrado ningún elemento hoja en este dibujo.";
                        return resultado;
                    }
                }

                string xmlFinal = doc.ToString(SaveOptions.DisableFormatting);

                string funcionComprimir = ResolverFuncionComprimir(conexion);
                byte[] bytesComprimidos = ComprimirBytes(conexion, xmlFinal, funcionComprimir);

                // Verificación de seguridad: antes de escribir en BBDD, comprobamos que lo que
                // acabamos de comprimir se descomprime exactamente igual. Si la función detectada
                // en el esquema zlib no es la correcta, esto falla aquí (sin tocar el Buffer) en
                // vez de guardar datos corruptos.
                string verificacion = DescomprimirBytes(conexion, bytesComprimidos);
                if (verificacion != xmlFinal)
                {
                    throw new InvalidOperationException(
                        $"La función de compresión detectada ([zlib].[{funcionComprimir}]) no reproduce el mismo XML al " +
                        "descomprimir el resultado. Puede que no sea la función correcta: revisa el esquema [zlib] en la " +
                        "base de datos y ajusta ResolverFuncionComprimir si hace falta. No se ha modificado el dibujo.");
                }

                GuardarBufferComprimido(conexion, codigoDibujo, bytesComprimidos);
                resultado.Exito = true;
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Mensaje = ex.Message;
            }

            return resultado;
        }

        /// <summary>
        /// Asegura que "contenedor" (psr:Model o psr:Element) tiene un psr:Options con psr:List
        /// (opciones nuevas fusionadas y reordenadas alfabéticamente igual que en los ejemplos
        /// aportados, comparación ordinal por "name", sin tocar las ya existentes salvo su
        /// posición) y psr:Levels con el nivel "nivelCarpeta" (ya construido con
        /// ConstruirNivelCarpeta; simplemente añadido al final, igual que en los ejemplos: los
        /// niveles no van ordenados alfabéticamente).
        /// </summary>
        private static (int anadidas, int yaExistian, bool nivelAnadido) AplicarOpcionesYNivelRotoEnContenedor(
            XElement contenedor, XNamespace psr, List<XElement> opcionesFuente, string nivelCarpeta)
        {
            XElement? options = contenedor.Element(psr + "Options");
            if (options == null)
            {
                options = new XElement(psr + "Options", new XAttribute(XsiNs + "type", "psr:typeOptions"));
                contenedor.Add(options);
            }

            XElement? list = options.Element(psr + "List");
            if (list == null)
            {
                list = new XElement(psr + "List");
                options.AddFirst(list);
            }

            var existentes = list.Elements(psr + "Option").ToList();
            var nombresExistentes = existentes
                .Select(o => (string?)o.Attribute("name") ?? "")
                .ToHashSet(StringComparer.Ordinal);

            int anadidas = 0, yaExistian = 0;
            var nuevas = new List<XElement>();
            foreach (var opcionFuente in opcionesFuente)
            {
                string nombre = (string?)opcionFuente.Attribute("name") ?? "";
                if (nombresExistentes.Contains(nombre)) { yaExistian++; continue; }
                nuevas.Add(new XElement(opcionFuente));
                nombresExistentes.Add(nombre);
                anadidas++;
            }

            if (nuevas.Count > 0)
            {
                var todas = existentes.Concat(nuevas)
                    .OrderBy(o => (string?)o.Attribute("name") ?? "", StringComparer.Ordinal)
                    .ToList();
                list.ReplaceNodes(todas);
            }

            XElement? levels = options.Element(psr + "Levels");
            if (levels == null)
            {
                levels = new XElement(psr + "Levels");
                options.Add(levels);
            }

            bool yaTieneNivel = levels.Elements(psr + "Level")
                .Any(l => string.Equals((string)l, nivelCarpeta, StringComparison.Ordinal));

            bool nivelAnadido = false;
            if (!yaTieneNivel)
            {
                levels.Add(new XElement(psr + "Level", nivelCarpeta));
                nivelAnadido = true;
            }

            return (anadidas, yaExistian, nivelAnadido);
        }

        /// <summary>Id de hoja real: "H" + número (H8, H9, H10, H11...), igual en todos los
        /// ejemplos vistos (practicables de 2/4 hojas y una corredera de 3), tanto si el modelo
        /// tiene un tipo de perfil PVC/Alu/PAX como si es corredera.</summary>
        private static readonly Regex IdHojaRegex = new(@"^H\d+$", RegexOptions.Compiled);

        /// <summary>
        /// Elementos "hoja": el psr:Hole "de contenido" (el que tiene un psr:Element hijo directo;
        /// el otro nivel de psr:Hole, xsi:type="psr:typeBinaryHole", es solo un envoltorio del
        /// árbol binario y no tiene psr:Element propio) que NO tiene un psr:Holes hijo, es decir,
        /// que no se subdivide más (paño/hoja terminal, con o sin psr:Glass) Y cuyo
        /// psr:Element/@id tiene el formato "H"+número.
        ///
        /// Este último filtro por id se añadió tras comparar ejemplos editados a mano de varios
        /// modelos: en todos ellos, junto a las hojas reales (H8/H9 en un 2 hojas; H10/H11/H14/H15
        /// en un 4 hojas; H7/H9/H10/H12 en una corredera de 3 hojas) aparecía siempre un
        /// psr:Hole/psr:Element adicional que cumple el criterio estructural (Element sin Holes)
        /// pero NO es una hoja -típicamente con id "RL0", ya con una única opción propia previa no
        /// relacionada con ROTO- y que en ninguno de esos ejemplos recibió ni las opciones ni el
        /// nivel ROTO. El XML lo genera un software externo (no RotoTools/PrefSuite) sobre el que
        /// no hay control ni documentación de su convención de ids, así que este filtro es una
        /// inferencia empírica a partir de los ejemplos disponibles, no una regla documentada del
        /// esquema: si aparece un modelo cuyas hojas reales no sigan el patrón "H"+número, esta
        /// función las dejaría fuera y habría que revisar el criterio.
        /// </summary>
        private static IEnumerable<XElement> ObtenerElementosHoja(XElement raiz, XNamespace psr)
        {
            foreach (var hole in raiz.Descendants(psr + "Hole"))
            {
                var elemento = hole.Element(psr + "Element");
                if (elemento == null) continue;
                if (hole.Element(psr + "Holes") != null) continue;

                string id = elemento.Attribute("id")?.Value ?? "";
                if (!IdHojaRegex.IsMatch(id)) continue;

                yield return elemento;
            }
        }

        #endregion

        #region Compresión / descompresión (BBDD)

        /// <summary>
        /// Reintento único para cualquier llamada a una función del esquema [zlib] (defensa
        /// adicional para hipos puntuales de carga del ensamblado CLR; ver también el CAST
        /// explícito en cada consulta más abajo, que es lo que realmente evita el error visto en
        /// producción).
        /// </summary>
        private static T EjecutarConReintentoZlib<T>(Func<T> accion)
        {
            try
            {
                return accion();
            }
            catch
            {
                System.Threading.Thread.Sleep(800);
                return accion();
            }
        }

        /// <summary>
        /// El CAST explícito a NVARCHAR(MAX) es imprescindible: confirmado en producción que
        /// "SELECT [zlib].[UnzipBLOB](Buffer) FROM Dibujos WHERE Codigo=..." SIN el CAST lanza
        /// System.IO.FileLoadException ("prefzipnet"...) tanto desde aquí como ejecutado a mano
        /// en SSMS con el mismo login, mientras que con el CAST funciona siempre. SQL Server
        /// deja de intentar devolver el tipo CLR "en crudo" y lo convierte a NVARCHAR dentro de
        /// la propia llamada, evitando el fallo.
        /// </summary>
        private static string LeerXmlDescomprimido(SqlConnection conexion, string codigoDibujo)
        {
            using var cmd = new SqlCommand(
                "SELECT CAST([zlib].[UnzipBLOB](Buffer) AS NVARCHAR(MAX)) FROM Dibujos WHERE Codigo=@codigo", conexion);
            cmd.Parameters.AddWithValue("@codigo", codigoDibujo);

            object? resultado = EjecutarConReintentoZlib(() => cmd.ExecuteScalar());
            if (resultado == null || resultado == DBNull.Value)
                throw new InvalidOperationException($"No se ha encontrado el dibujo '{codigoDibujo}' o su Buffer está vacío.");

            return resultado is byte[] bytes ? DecodificarTexto(bytes) : resultado.ToString() ?? "";
        }

        /// <summary>
        /// Descubre en tiempo de ejecución el nombre de la función de compresión del esquema
        /// [zlib]. Confirmado en producción que la pareja correcta para este XML es
        /// zlib.UnzipXML / zlib.ZipXml (no zlib.UnzipBLOB / zlib.ZipBLOB, aunque ambos pares
        /// pueden coexistir en el esquema): se prioriza un candidato llamado exactamente
        /// "ZipXml" si existe. Como fallback (por si en otro entorno no existiera ese nombre
        /// exacto) se usa un heurístico que excluye cualquier función que EMPIECE por "Unzip"
        /// -antes solo se excluía el nombre exacto "UnzipBLOB", lo que dejaba pasar "UnzipXML"
        /// como candidato "de compresión" por contener la subcadena "Zip" y quedar primero en
        /// orden alfabético; ese fue precisamente el bug visto en producción (se compilaba con
        /// UnzipXML en vez de ZipXml). Se cachea en memoria del proceso una vez resuelta.
        /// </summary>
        private static string ResolverFuncionComprimir(SqlConnection conexion)
        {
            if (_nombreFuncionComprimirCache != null) return _nombreFuncionComprimirCache;

            var candidatos = new List<string>();
            using (var cmd = new SqlCommand(@"
SELECT o.name
FROM sys.objects o
JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE s.name = 'zlib' AND o.type IN ('FN','TF','IF','FS','FT')
ORDER BY o.name", conexion))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) candidatos.Add(reader.GetString(0));
            }

            string? elegido = candidatos.FirstOrDefault(n => n.Equals("ZipXml", StringComparison.OrdinalIgnoreCase))
                ?? candidatos.FirstOrDefault(n =>
                    !n.StartsWith("Unzip", StringComparison.OrdinalIgnoreCase) &&
                    (n.Contains("Zip", StringComparison.OrdinalIgnoreCase) || n.Contains("Compress", StringComparison.OrdinalIgnoreCase)))
                ?? candidatos.FirstOrDefault(n => !n.StartsWith("Unzip", StringComparison.OrdinalIgnoreCase));

            if (elegido == null)
            {
                string encontrados = candidatos.Count > 0 ? string.Join(", ", candidatos) : "(ninguna)";
                throw new InvalidOperationException(
                    "No se ha encontrado ninguna función de compresión en el esquema [zlib] de la base de datos " +
                    $"(funciones encontradas: {encontrados}).");
            }

            _nombreFuncionComprimirCache = elegido;
            return elegido;
        }

        /// <summary>
        /// Mismo criterio que LeerXmlDescomprimido: CAST explícito a VARBINARY(MAX) del
        /// resultado (necesitamos los bytes comprimidos, no texto) en vez de devolver "en crudo"
        /// lo que da la función CLR. El parámetro de entrada se manda como NVARCHAR(MAX) (texto),
        /// no como VARBINARY: dado que la función se llama "ZipXml" (no "ZipBLOB"), lo más
        /// probable es que espere directamente el XML como texto, igual que su pareja de lectura
        /// zlib.UnzipXML devuelve texto. Si esto no fuera así (firma real distinta), la
        /// verificación round-trip de más abajo (comprimir + descomprimir + comparar contra el
        /// XML final) lo detectaría con un error claro antes de escribir nada en BBDD.
        /// </summary>
        private static byte[] ComprimirBytes(SqlConnection conexion, string xmlTexto, string funcionComprimir)
        {
            using var cmd = new SqlCommand($"SELECT CAST([zlib].[{funcionComprimir}](@xml) AS VARBINARY(MAX))", conexion);
            cmd.Parameters.Add("@xml", System.Data.SqlDbType.NVarChar, -1).Value = xmlTexto;

            object? resultado = EjecutarConReintentoZlib(() => cmd.ExecuteScalar());
            if (resultado is byte[] bytes) return bytes;

            throw new InvalidOperationException(
                $"[zlib].[{funcionComprimir}] no ha devuelto datos binarios (varbinary) como se esperaba.");
        }

        /// <summary>Mismo CAST explícito que LeerXmlDescomprimido, por el mismo motivo.</summary>
        private static string DescomprimirBytes(SqlConnection conexion, byte[] bytesComprimidos)
        {
            using var cmd = new SqlCommand("SELECT CAST([zlib].[UnzipBLOB](@buffer) AS NVARCHAR(MAX))", conexion);
            cmd.Parameters.Add("@buffer", System.Data.SqlDbType.VarBinary, -1).Value = bytesComprimidos;

            object? resultado = EjecutarConReintentoZlib(() => cmd.ExecuteScalar());
            if (resultado == null || resultado == DBNull.Value) return "";
            return resultado is byte[] bytes ? DecodificarTexto(bytes) : resultado.ToString() ?? "";
        }

        private static void GuardarBufferComprimido(SqlConnection conexion, string codigoDibujo, byte[] bytesComprimidos)
        {
            using var cmd = new SqlCommand("UPDATE Dibujos SET Buffer=@buffer WHERE Codigo=@codigo", conexion);
            cmd.Parameters.Add("@buffer", System.Data.SqlDbType.VarBinary, -1).Value = bytesComprimidos;
            cmd.Parameters.AddWithValue("@codigo", codigoDibujo);
            cmd.ExecuteNonQuery();
        }

        private static string DecodificarTexto(byte[] bytes)
        {
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return new UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3);
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);

            return new UTF8Encoding(false).GetString(bytes);
        }

        #endregion
    }
}
