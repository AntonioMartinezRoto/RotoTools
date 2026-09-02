using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace RotoTools.Suite.Services
{
    /// <summary>
    /// Fila de la tabla Escandallos para el árbol+grid de selección (Codigo, Descripcion,
    /// Nivel1..5), mismas columnas que pidió el usuario. No incluye el resto de columnas de la
    /// tabla (Type, Variables, Programa, Texto, Familia, XMLTable, ProductionType,
    /// PrefShopStatus: ver ActualizadorPage.xaml.cs/RotoEntities.Escandallo) porque esta pantalla
    /// solo necesita identificar y localizar el escandallo, no editarlo.
    /// </summary>
    public class EscandalloRow
    {
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Nivel1 { get; set; } = "";
        public string Nivel2 { get; set; } = "";
        public string Nivel3 { get; set; } = "";
        public string Nivel4 { get; set; } = "";
        public string Nivel5 { get; set; } = "";
    }

    /// <summary>
    /// Nodo del árbol de carpetas de Escandallos (Nivel1..5), idéntico criterio y misma forma que
    /// DibujoTreeNode (ver DibujoOpcionesRotoService.cs): Codigo == null → carpeta,
    /// Codigo != null → hoja/escandallo.
    /// </summary>
    public class EscandalloTreeNode : System.ComponentModel.INotifyPropertyChanged
    {
        public string Texto { get; set; } = "";
        public string? Codigo { get; set; }
        public bool EsHoja => Codigo != null;
        public List<EscandalloTreeNode> Hijos { get; } = new();

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

    /// <summary>Un escandallo elegido por el usuario para asociar, con las variables que ha
    /// escrito para él (mismo texto de variables para todos los escandallos de una misma
    /// aplicación, ver ActualizadorAsociarConstructivosWindow).</summary>
    public class EscandalloSeleccionado
    {
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }

    /// <summary>Resultado de asociar los constructivos elegidos a un único Dibujo, para el resumen final.</summary>
    public class ResultadoAplicarConstructivo
    {
        public string Codigo { get; set; } = "";
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = "";
        public int EscandallosAnadidos { get; set; }
        public int EscandallosYaExistian { get; set; }
        public int ElementosModificados { get; set; }
    }

    /// <summary>
    /// Nueva (no existía en el original ni en ningún otro módulo de la Suite): inserta uno o
    /// varios Escandallos (tabla Escandallos) en el nodo psr:ConstructiveScript de cada elemento
    /// "hoja" (mismo criterio que DibujoOpcionesRotoService.ObtenerElementosHoja: psr:Hole
    /// terminal -sin psr:Holes anidado- con psr:Opening, que es lo único que distingue una hoja
    /// real de otros huecos/paños intermedios) del XML de un Dibujo guardado en BBDD (tabla
    /// Dibujos, columna Buffer, comprimido). A diferencia de las opciones ROTO, aquí SIEMPRE se
    /// aplica por elemento (a cada hoja), nunca al modelo general: así lo pidió el usuario, porque
    /// un constructivo es intrínsecamente una propiedad de cada hoja, no del modelo.
    ///
    /// Formato de psr:ConstructiveScript (confirmado con un XML real de ejemplo, no documentado en
    /// ningún sitio del proyecto): texto plano con 5 secciones fijas, en este orden, cada una
    /// empezando por una línea "% NombreSección": Materiales, Escandallos, Mano de Obra, Tablas,
    /// Secciones. Cada línea termina con el separador "&amp;#D;&amp;#A;" (así, LITERAL: no son
    /// referencias de carácter XML reales -eso sería &amp;amp;#D;&amp;amp;#A; en el XML crudo-,
    /// sino la propia convención interna de Preference para CR+LF dentro de este campo de texto,
    /// que ya ha pasado por una capa de escapado XML antes de llegar aquí; ver
    /// EscribirTextoConstructiveScript). Dentro de la sección Escandallos, cada escandallo
    /// instalado es una línea con el formato "ESCANDALLO(&amp;quot;Codigo&amp;quot;,&amp;quot;Variables&amp;quot;);"
    /// (de nuevo, "&amp;quot;" literal, no una comilla real: es la convención de Preference para
    /// comillas dentro de este campo). Reutiliza tal cual (ver DibujoOpcionesRotoService,
    /// internal) el mismo namespace psr, el mismo criterio de elemento "hoja", y toda la
    /// infraestructura de compresión/descompresión + verificación round-trip antes de escribir en
    /// BBDD, para no duplicar ni divergir de esa lógica ya validada.
    /// </summary>
    public static class DibujoConstructivosService
    {
        #region Listado de Escandallos (árbol + grid)

        public static List<EscandalloRow> GetEscandallos()
        {
            var lista = new List<EscandalloRow>();

            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            using var cmd = new SqlCommand(
                "SELECT Codigo, Descripcion, Nivel1, Nivel2, Nivel3, Nivel4, Nivel5 " +
                "FROM Escandallos ORDER BY Nivel1, Nivel2, Nivel3, Nivel4, Nivel5, Codigo", conexion);

            conexion.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new EscandalloRow
                {
                    Codigo = reader["Codigo"]?.ToString()?.Trim() ?? "",
                    Descripcion = reader["Descripcion"]?.ToString()?.Trim() ?? "",
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

        #region Texto de psr:ConstructiveScript (formato confirmado con ejemplo real)

        /// <summary>Separador de línea LITERAL usado por Preference dentro de este campo de texto
        /// (no una referencia de carácter XML real: ver el comentario de la clase).</summary>
        private const string SepLinea = "&#D;&#A;";

        /// <summary>Comilla LITERAL usada por Preference dentro de este campo de texto (idem).</summary>
        private const string ComillaLiteral = "&quot;";

        /// <summary>Las 5 secciones fijas de psr:ConstructiveScript, en este orden exacto
        /// (confirmado con el ejemplo real aportado).</summary>
        private static readonly string[] SeccionesConstructiveScript =
            { "Materiales", "Escandallos", "Mano de Obra", "Tablas", "Secciones" };

        /// <summary>Esqueleto de un psr:ConstructiveScript nuevo (las 5 secciones vacías), para el
        /// caso -no visto en el ejemplo real, pero posible- de que una hoja no tenga todavía este
        /// nodo.</summary>
        private static string ConstruirEsqueletoConstructiveScript()
            => string.Concat(SeccionesConstructiveScript.Select(s => "% " + s + SepLinea));

        /// <summary>Evita que un Codigo/Variables escrito por el usuario con una comilla o un
        /// salto de línea real rompa el formato de línea único de esta sección: los sustituye por
        /// la misma convención literal que usa Preference. En la práctica no debería hacer falta
        /// (el Codigo viene del árbol, no de texto libre, y las Variables son del tipo
        /// "L=L1;A=L2;"), pero es una salvaguarda barata.</summary>
        private static string EscaparTextoConstructivo(string valor)
            => valor.Replace("\"", ComillaLiteral)
                    .Replace("\r\n", SepLinea)
                    .Replace("\n", SepLinea);

        /// <summary>
        /// Inserta (si no existe ya, comparando por Código) una línea
        /// "ESCANDALLO(&quot;Codigo&quot;,&quot;Variables&quot;);" al final de la sección
        /// "Escandallos" del texto de psr:ConstructiveScript indicado, sin tocar el resto de
        /// secciones ni su contenido. Verificado contra un XML real (ver comentario de la clase):
        /// reconstruye byte a byte el mismo texto cuando no hay nada que insertar.
        /// </summary>
        private static (string TextoResultado, bool Anadido) InsertarEscandalloEnSeccion(string textoActual, string codigo, string variables)
        {
            string codigoEscapado = EscaparTextoConstructivo(codigo);
            string variablesEscapadas = EscaparTextoConstructivo(variables);

            var lineas = textoActual.Split(new[] { SepLinea }, StringSplitOptions.None).ToList();

            int idxEscandallos = lineas.FindIndex(l => l.Trim() == "% Escandallos");
            if (idxEscandallos < 0)
                throw new InvalidOperationException(
                    "El psr:ConstructiveScript de este elemento no tiene la sección \"% Escandallos\" esperada: " +
                    "no tiene el formato que se conoce para este campo, revisa el XML de este dibujo a mano.");

            int idxSiguienteSeccion = lineas.FindIndex(idxEscandallos + 1, l => l.StartsWith("% ", StringComparison.Ordinal));
            if (idxSiguienteSeccion < 0)
                idxSiguienteSeccion = lineas.Count - 1; // por si "Escandallos" fuera la última sección (no esperado, pero defensivo)

            // ¿Ya existe un ESCANDALLO con este mismo Código en la sección? (comparación por
            // Código, no por línea completa: no se duplica aunque las Variables sean distintas,
            // mismo criterio que AplicarOpcionesYNivelRotoEnContenedor con los nombres de Opción.)
            var patronCodigo = new Regex(
                "^ESCANDALLO\\(" + Regex.Escape(ComillaLiteral) + "(?<codigo>.*?)" + Regex.Escape(ComillaLiteral) + ",",
                RegexOptions.Compiled);

            for (int i = idxEscandallos + 1; i < idxSiguienteSeccion; i++)
            {
                var m = patronCodigo.Match(lineas[i]);
                if (m.Success && string.Equals(m.Groups["codigo"].Value, codigoEscapado, StringComparison.Ordinal))
                    return (textoActual, false);
            }

            string nuevaLinea = "ESCANDALLO(" + ComillaLiteral + codigoEscapado + ComillaLiteral + "," +
                                 ComillaLiteral + variablesEscapadas + ComillaLiteral + ");";
            lineas.Insert(idxSiguienteSeccion, nuevaLinea);

            return (string.Join(SepLinea, lineas), true);
        }

        #endregion

        #region Aplicar constructivos a un Dibujo

        /// <summary>
        /// Asocia, a cada elemento "hoja" del Dibujo indicado, los escandallos de
        /// "escandallosSeleccionados" (mismas Variables para todos, ver
        /// ActualizadorAsociarConstructivosWindow) en su psr:ConstructiveScript. Siempre por
        /// elemento (nunca al modelo general): un constructivo es una propiedad de cada hoja.
        /// </summary>
        public static ResultadoAplicarConstructivo AplicarConstructivosRoto(string codigoDibujo, List<(string Codigo, string Variables)> escandallosSeleccionados)
        {
            var resultado = new ResultadoAplicarConstructivo { Codigo = codigoDibujo };

            try
            {
                using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
                conexion.Open();

                string xmlOriginal = DibujoOpcionesRotoService.LeerXmlDescomprimido(conexion, codigoDibujo);
                XDocument doc = XDocument.Parse(xmlOriginal);
                XElement raiz = doc.Root ?? throw new InvalidOperationException(
                    "El XML del dibujo está vacío o no es válido.");
                XNamespace psr = raiz.Name.Namespace;

                int totalAnadidos = 0, totalYaExistian = 0, elementos = 0;

                foreach (var elementoHoja in DibujoOpcionesRotoService.ObtenerElementosHoja(raiz, psr))
                {
                    XElement? script = elementoHoja.Element(psr + "ConstructiveScript");
                    if (script == null)
                    {
                        script = new XElement(psr + "ConstructiveScript", ConstruirEsqueletoConstructiveScript());
                        elementoHoja.AddFirst(script);
                    }

                    string textoActual = script.Value;
                    foreach (var (codigoEscandallo, variables) in escandallosSeleccionados)
                    {
                        var (textoResultado, anadido) = InsertarEscandalloEnSeccion(textoActual, codigoEscandallo, variables);
                        textoActual = textoResultado;
                        if (anadido) totalAnadidos++; else totalYaExistian++;
                    }

                    script.Value = textoActual;
                    elementos++;
                }

                resultado.EscandallosAnadidos = totalAnadidos;
                resultado.EscandallosYaExistian = totalYaExistian;
                resultado.ElementosModificados = elementos;

                if (elementos == 0)
                {
                    resultado.Exito = false;
                    resultado.Mensaje = "No se ha encontrado ningún elemento hoja en este dibujo.";
                    return resultado;
                }

                string xmlFinal = doc.ToString(SaveOptions.DisableFormatting);

                string funcionComprimir = DibujoOpcionesRotoService.ResolverFuncionComprimir(conexion);
                byte[] bytesComprimidos = DibujoOpcionesRotoService.ComprimirBytes(conexion, xmlFinal, funcionComprimir);

                // Misma verificación de seguridad que DibujoOpcionesRotoService.AplicarOpcionesRoto:
                // antes de escribir en BBDD, comprobamos que lo que acabamos de comprimir se
                // descomprime exactamente igual.
                string verificacion = DibujoOpcionesRotoService.DescomprimirBytes(conexion, bytesComprimidos);
                if (verificacion != xmlFinal)
                {
                    throw new InvalidOperationException(
                        $"La función de compresión detectada ([zlib].[{funcionComprimir}]) no reproduce el mismo XML al " +
                        "descomprimir el resultado. Puede que no sea la función correcta: revisa el esquema [zlib] en la " +
                        "base de datos. No se ha modificado el dibujo.");
                }

                DibujoOpcionesRotoService.GuardarBufferComprimido(conexion, codigoDibujo, bytesComprimidos);
                resultado.Exito = true;
            }
            catch (Exception ex)
            {
                resultado.Exito = false;
                resultado.Mensaje = ex.Message;
            }

            return resultado;
        }

        #endregion
    }
}
