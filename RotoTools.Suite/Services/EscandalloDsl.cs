using System.Globalization;
using System.Text.RegularExpressions;

namespace RotoTools.Suite.Services
{
    /// <summary>
    /// Tokenizer, AST y parser (descenso recursivo) del pequeño lenguaje en el que están escritos
    /// los campos "Programa" de la tabla Escandallos (SI/ENTONCES/FINSI anidable, OPCION(...) y su
    /// forma negada OPCION(...)=0, booleanos O/Y con paréntesis "(" ")" de agrupación (ver
    /// ParserDsl.ParseTerm), comparación de una VARIABLE (p.ej. "A=1200", "L&lt;500"... ver
    /// ParserDsl.ParseVarCmp/NodoVarCmp) con los operadores =, &lt;, &gt;, &lt;=, &gt;=,
    /// ESTABLECEOPCION(...);, ESTABLECEOPCIONNUMERICA(...,[Variable]); (ver NodoSetOptNum,
    /// asigna el valor NUMÉRICO actual de una variable en vez de un literal de texto),
    /// ESCANDALLO(...); para encadenar -incluido su segundo argumento
    /// "Variables", ver EscandalloInterpreter.AplicarVariablesDeLlamada-, comentarios de línea con
    /// "%"). Basado en un prototipo Python (dsl.py) validado contra 31 escandallos reales de
    /// producción, pero AMPLIADO desde entonces con construcciones reales que ese prototipo no
    /// cubría -confirmadas por el usuario, no por precedente de código: el RotoTools original nunca
    /// ejecuta este DSL, solo lo guarda como texto opaco, así que no hay ningún engine de referencia
    /// en este repositorio-: (1) paréntesis de agrupación en condiciones, (2) comparación directa
    /// de variables pasadas de un escandallo a otro vía ESCANDALLO("Codigo","Variables");, con dos
    /// formas confirmadas para ese segundo argumento -"A=L1;L=L2;" (nombre=nombre, para dar valor a
    /// una variable a partir de otra ya conocida, típicamente las cotas reales del modelo) y "A" a
    /// secas (reenviar el valor ya conocido de una variable sin renombrarla), ambas separadas por
    /// ";" si hay varias-, (3) ESTABLECEOPCIONNUMERICA("Nombre",[Variable]); (ejemplo real
    /// confirmado: ESTABLECEOPCIONNUMERICA("AlturaManeta",[AM]);), y (4) tres sentencias más, sin
    /// ejemplo de código real pero con su significado explicado por el usuario -ver
    /// NodoPreguntaValor/NodoOpciones/NodoSea-: PREGUNTAVALOR("Variable"); (pausa a preguntar esa
    /// variable si no se conoce, igual que ya hace una comparación de variable desconocida),
    /// OPCIONES("..."); (instrucción de depuración de Preference sin efecto en la simulación: se
    /// reconoce pero se ignora), y SEA Variable = [Valor]; (asigna a una variable el resultado de
    /// evaluar una expresión aritmética -ver NodoExpr/ParserDsl.ParseExprAdd-, ejemplos reales
    /// confirmados: "SEA VARIABLE1 = [5];", "SEA VARIABLE2 = [VARIABLE1];" y "SEA AM = [A/2];" — el
    /// valor va siempre entre corchetes, igual que el segundo argumento de
    /// ESTABLECEOPCIONNUMERICA, y admite +, -, *, / con la precedencia aritmética habitual y
    /// paréntesis de agrupación).
    ///
    /// A nivel de SENTENCIA (SI/ESTABLECEOPCION/ESCANDALLO), cualquier texto que no encaje en la
    /// gramática produce un nodo NodoUnrecognized en vez de abortar el parseo completo (ver
    /// ParserDsl.ParseStmt), tal y como pidió el usuario ("cualquier cosa que el intérprete no
    /// reconozca debe quedar señalada con claridad, no fallar en silencio ni abortar toda la
    /// simulación"). A nivel de CONDICIÓN (dentro de un SI [...]), en cambio, un error de sintaxis sí
    /// aborta el parseo del escandallo completo (ErrorParseoDsl, ver EscandalloInterpreter.ObtenerAst):
    /// es esa la razón por la que dar un mensaje de error claro ahí importa tanto -sin él, un
    /// escandallo entero se queda sin ejecutar y solo se ve un aviso genérico en el Recorrido-.
    /// </summary>
    #region Tokenizer

    internal enum TipoToken
    {
        STRING, NUM, SI, ENTONCES, FINSI, OPCION, ESTABLECEOPCION, ESCANDALLO, O, Y,
        LBRACK, RBRACK, LPAREN, RPAREN, COMMA, SEMI, EQ,
        // Operadores relacionales adicionales, y el identificador de variable (p.ej. "A", "L1"),
        // para la comparación de variables (ver NodoVarCmp/ParserDsl.ParseVarCmp) — construcción
        // confirmada por el usuario, sin precedente en el prototipo Python original.
        LE, GE, LT, GT, IDENT,
        // ESTABLECEOPCIONNUMERICA("Nombre",[Variable]); (ver NodoSetOptNum/ParserDsl.ParseSetOptNum):
        // asigna a una opción el valor NUMÉRICO ACTUAL de una variable -distinto de ESTABLECEOPCION,
        // que siempre asigna un literal de texto entre comillas-. Confirmada por el usuario con el
        // ejemplo real ESTABLECEOPCIONNUMERICA("AlturaManeta",[AM]);.
        ESTABLECEOPCIONNUMERICA,
        // Las tres siguientes, confirmadas por el usuario (ver NodoPreguntaValor/NodoOpciones/NodoSea
        // más abajo): PREGUNTAVALOR("Variable"); (pausa a preguntar esa variable si no se conoce
        // -mismo mecanismo que ya usa NodoVarCmp-), OPCIONES("..."); (instrucción de depuración de
        // Preference sin efecto en nuestra simulación: se ignora), SEA Variable = [Valor]; (asigna
        // un literal numérico u otra variable ya conocida a una variable; el valor va SIEMPRE entre
        // corchetes, igual que el segundo argumento de ESTABLECEOPCIONNUMERICA).
        PREGUNTAVALOR, OPCIONES, SEA,
        // Operadores aritméticos dentro de un valor entre corchetes (ver NodoExpr/ParserDsl.ParseExprAdd
        // /ParseExprMul/ParseExprAtom): confirmados por el usuario a raíz de "SEA AM = [A/2];" -el
        // valor entre corchetes de SEA (y, por consistencia, el de ESTABLECEOPCIONNUMERICA) no es
        // siempre un simple NUM o IDENT suelto, puede ser una expresión aritmética completa. Se
        // reutilizan LPAREN/RPAREN (ya existentes para agrupar condiciones) para agrupar dentro de
        // estas expresiones también.
        PLUS, MINUS, STAR, SLASH
    }

    internal sealed class Token
    {
        public TipoToken Tipo { get; }
        public string Valor { get; }
        public int Pos { get; }
        public Token(TipoToken tipo, string valor, int pos) { Tipo = tipo; Valor = valor; Pos = pos; }
        public override string ToString() => $"{Tipo}:{Valor}@{Pos}";
    }

    internal static class Tokenizer
    {
        // Mismo orden que TOKEN_SPEC en dsl.py: las palabras clave (\b...\b) antes que los símbolos
        // sueltos. No hay solapamiento real entre patrones en este alfabeto, pero se conserva el
        // orden validado por si acaso.
        private static readonly (TipoToken Tipo, string Patron)[] EspecTokens =
        {
            (TipoToken.STRING, "\"[^\"]*\""),
            // (\.\d+)? opcional: admite decimales con punto ("20.6"), no solo enteros. Antes era
            // solo @"\d+", así que "20.6" se tokenizaba como NUM("20") + el '.' se descartaba en
            // silencio (carácter no reconocido, ver Tokenizar) + NUM("6") sueltos -dos tokens en vez
            // de uno-, lo que producía el error real reportado por el usuario en
            // "SEA DIST1 = [20.6];": "se esperaba el corchete de cierre, pero se ha encontrado un
            // número (\"6\")", porque ParseSea solo consumía el primer NUM("20") y luego topaba con
            // el NUM("6") sobrante en vez del corchete de cierre esperado.
            (TipoToken.NUM, @"\d+(\.\d+)?"),
            (TipoToken.SI, @"\bSI\b"),
            (TipoToken.ENTONCES, @"\bENTONCES\b"),
            (TipoToken.FINSI, @"\bFINSI\b"),
            (TipoToken.OPCION, @"\bOPCION\b"),
            // Mismo motivo que ESTABLECEOPCIONNUMERICA más abajo: "OPCIONES" no cumple \bOPCION\b
            // (sin límite de palabra entre la "N" de OPCION y la "E" de "ES"), así que el orden
            // entre esta línea y la anterior tampoco importa.
            (TipoToken.OPCIONES, @"\bOPCIONES\b"),
            (TipoToken.ESTABLECEOPCION, @"\bESTABLECEOPCION\b"),
            // El orden entre esta y la anterior no importa (a diferencia de IDENT/LE-GE más abajo):
            // "ESTABLECEOPCIONNUMERICA" NO cumple \bESTABLECEOPCION\b (no hay límite de palabra
            // entre la "N" final de "OPCION" y la "N" inicial de "NUMERICA", las dos son \w), así
            // que esa rama simplemente no matchea ahí y la alternancia sigue probando hasta llegar
            // a esta. Se coloca aquí solo por agrupar las dos palabras clave relacionadas.
            (TipoToken.ESTABLECEOPCIONNUMERICA, @"\bESTABLECEOPCIONNUMERICA\b"),
            (TipoToken.ESCANDALLO, @"\bESCANDALLO\b"),
            (TipoToken.PREGUNTAVALOR, @"\bPREGUNTAVALOR\b"),
            (TipoToken.SEA, @"\bSEA\b"),
            (TipoToken.O, @"\bO\b"),
            (TipoToken.Y, @"\bY\b"),
            // IDENT (identificador de variable, p.ej. "A", "L1", "ANCHO") va DESPUÉS de todas las
            // palabras clave de arriba a propósito: en la alternancia de MasterRegex, .NET prueba
            // cada rama EN ORDEN y se queda con la primera que encaje en esa posición (no es
            // "coincidencia más larga" como POSIX) — si IDENT fuera antes, "SI"/"O"/"Y"/etc. se
            // tokenizarían como IDENT en vez de como la palabra clave correspondiente. Los patrones
            // \b...\b de las palabras clave ya evitan el problema inverso (un identificador que
            // EMPIECE por esas letras, p.ej. "SISTEMA", no matchea \bSI\b porque no hay límite de
            // palabra tras "SI" ahí), así que este orden basta.
            (TipoToken.IDENT, @"[A-Za-z][A-Za-z0-9]*"),
            (TipoToken.LBRACK, @"\["),
            (TipoToken.RBRACK, @"\]"),
            (TipoToken.LPAREN, @"\("),
            (TipoToken.RPAREN, @"\)"),
            (TipoToken.COMMA, ","),
            (TipoToken.SEMI, ";"),
            // LE/GE ("<=" / ">=") van ANTES que LT/GT ("<" / ">") por el mismo motivo que IDENT va
            // al final de las palabras clave: si LT fuera antes, "<=" se partiría en LT + EQ en vez
            // de reconocerse como un único token LE.
            (TipoToken.LE, "<="),
            (TipoToken.GE, ">="),
            (TipoToken.LT, "<"),
            (TipoToken.GT, ">"),
            (TipoToken.EQ, "="),
            // Operadores aritméticos (ver NodoExpr más abajo): solo se usan dentro de un valor entre
            // corchetes ("[...]"), pero se tokenizan siempre igual, sea cual sea el contexto -es el
            // parser, no el tokenizador, quien decide dónde son válidos-.
            (TipoToken.PLUS, @"\+"),
            (TipoToken.MINUS, "-"),
            (TipoToken.STAR, @"\*"),
            (TipoToken.SLASH, "/"),
        };

        private const string NombreGrupoWs = "WS";
        private const string PatronEspacios = @"[ \t\r\n]+";

        // \G ancla cada intento de match a la posición exacta pasada a Match(texto, pos): es el
        // equivalente .NET de re.match(texto, pos) en Python (que SIEMPRE ancla al inicio, a
        // diferencia de re.search). Sin \G, Regex.Match busca la siguiente coincidencia en
        // cualquier punto a partir de pos, no necesariamente en pos.
        private static readonly Regex MasterRegex = ConstruirRegex();

        private static Regex ConstruirRegex()
        {
            var grupos = EspecTokens.Select(e => $"(?<{e.Tipo}>{e.Patron})").ToList();
            grupos.Add($"(?<{NombreGrupoWs}>{PatronEspacios})");
            return new Regex(@"\G(?:" + string.Join("|", grupos) + ")", RegexOptions.Compiled);
        }

        /// <summary>Quita todo lo que va desde un '%' hasta el fin de línea (comentario). Los finales
        /// de línea \r\n se normalizan a \n antes de trocear: no cambia el resultado del tokenizado
        /// porque \r ya es tratado como espacio en blanco (PatronEspacios) y se descarta igual.</summary>
        public static string QuitarComentarios(string texto)
        {
            var lineas = texto.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lineas.Length; i++)
            {
                int idx = lineas[i].IndexOf('%');
                if (idx >= 0) lineas[i] = lineas[i].Substring(0, idx);
            }
            return string.Join("\n", lineas);
        }

        public static List<Token> Tokenizar(string texto)
        {
            var tokens = new List<Token>();
            int pos = 0;
            int n = texto.Length;
            while (pos < n)
            {
                Match m = MasterRegex.Match(texto, pos);
                if (!m.Success)
                {
                    // Carácter no reconocido: se salta (defensivo, igual que el prototipo Python) y se sigue.
                    pos++;
                    continue;
                }
                if (!m.Groups[NombreGrupoWs].Success)
                {
                    foreach (var (tipo, _) in EspecTokens)
                    {
                        if (m.Groups[tipo.ToString()].Success)
                        {
                            tokens.Add(new Token(tipo, m.Value, pos));
                            break;
                        }
                    }
                }
                pos = m.Index + m.Length;
            }
            return tokens;
        }
    }

    #endregion

    #region AST

    internal abstract class NodoCond { }

    internal sealed class NodoCmp : NodoCond
    {
        public string Opcion { get; }
        public string Valor { get; }
        /// <summary>true si la condición era OPCION("X","Y")=0 (negada).</summary>
        public bool Negado { get; }
        public NodoCmp(string opcion, string valor, bool negado) { Opcion = opcion; Valor = valor; Negado = negado; }
    }

    /// <summary>Operador de una comparación de VARIABLE (ver NodoVarCmp): los cinco confirmados por
    /// el usuario ("Igualdad '=' " y "Mayor/menor '&lt;' '&gt;' '&lt;=' '&gt;='"). NO incluye
    /// distinto "&lt;&gt;": el usuario no lo marcó como necesario.</summary>
    internal enum OperadorCmp { Igual, Menor, Mayor, MenorIgual, MayorIgual }

    /// <summary>Comparación de una VARIABLE (p.ej. "A=1200", "L&lt;500") con un número, distinta de
    /// NodoCmp (que compara una OPCION del catálogo). Las variables se pasan de un escandallo a otro
    /// vía el segundo argumento de ESCANDALLO("Codigo","Variables"); (ver
    /// EscandalloInterpreter.AplicarVariablesDeLlamada) y, si su valor no se conoce por ese camino,
    /// se le pregunta directamente al usuario -igual que con las opciones, ver
    /// EscandalloInterpreter.AsegurarValoresConocidos-. Construcción real confirmada por el usuario
    /// tras el aviso "esperaba OPCION, encontrado EQ" en "Constructivo Hoja_2": el valor que allí
    /// faltaba por reconocer era justo una variable "A" pasada por ESCANDALLO("Constructivo
    /// Hoja_2","A"); desde "Constructivo Hoja", que a su vez la recibe como ESCANDALLO("Constructivo
    /// Hoja","A=L1;L=L2;") -L1/L2 son las cotas reales de alto/ancho del modelo-.</summary>
    internal sealed class NodoVarCmp : NodoCond
    {
        public string Variable { get; }
        public OperadorCmp Operador { get; }
        public double Valor { get; }
        public NodoVarCmp(string variable, OperadorCmp operador, double valor) { Variable = variable; Operador = operador; Valor = valor; }
    }

    internal sealed class NodoAnd : NodoCond
    {
        public NodoCond Izq { get; }
        public NodoCond Der { get; }
        public NodoAnd(NodoCond izq, NodoCond der) { Izq = izq; Der = der; }
    }

    internal sealed class NodoOr : NodoCond
    {
        public NodoCond Izq { get; }
        public NodoCond Der { get; }
        public NodoOr(NodoCond izq, NodoCond der) { Izq = izq; Der = der; }
    }

    /// <summary>Operador aritmético dentro de un valor entre corchetes (ver NodoExprBin). Los cuatro
    /// básicos, con la precedencia habitual: Mult/Div ligan más fuerte que Suma/Resta (ver
    /// ParserDsl.ParseExprAdd/ParseExprMul), y se puede agrupar con paréntesis (ver
    /// ParserDsl.ParseExprAtom).</summary>
    internal enum OperadorArit { Suma, Resta, Mult, Div }

    /// <summary>Expresión aritmética dentro de un valor entre corchetes -"[Valor]" en SEA Variable =
    /// [Valor]; y en el segundo argumento de ESTABLECEOPCIONNUMERICA("Nombre",[Valor]);-. Antes
    /// "Valor" solo admitía un NUM o un IDENT sueltos (guardados como texto crudo); confirmado por el
    /// usuario, a raíz de "SEA AM = [A/2];" ("la instruccion quiere asignar el valor de A dividido
    /// entre 2 a la variable AM"), que puede ser una expresión aritmética completa con +, -, *, /, y
    /// paréntesis de agrupación. Se evalúa en EscandalloInterpreter.EvaluarExpr: un NodoExprVar cuya
    /// variable todavía no tiene valor conocido hace que TODA la expresión quede sin resolver -mismo
    /// criterio de "no bloquear ni dar error" ya usado en el resto del intérprete, ver
    /// EscandalloInterpreter.ValorConocido-, igual que una división entre cero.</summary>
    internal abstract class NodoExpr { }

    /// <summary>Un número literal dentro de una expresión (p.ej. el "2" de "[A/2]", o el "5" de
    /// "[5]").</summary>
    internal sealed class NodoExprNum : NodoExpr
    {
        public double Valor { get; }
        public NodoExprNum(double valor) { Valor = valor; }
    }

    /// <summary>El nombre de una variable dentro de una expresión (p.ej. la "A" de "[A/2]"): su valor
    /// se resuelve en ejecución vía EscandalloInterpreter.ValorConocido, igual que en el resto del
    /// intérprete.</summary>
    internal sealed class NodoExprVar : NodoExpr
    {
        public string Variable { get; }
        public NodoExprVar(string variable) { Variable = variable; }
    }

    /// <summary>Una operación binaria entre dos subexpresiones (p.ej. "A/2" es
    /// NodoExprBin(NodoExprVar("A"), Div, NodoExprNum(2))).</summary>
    internal sealed class NodoExprBin : NodoExpr
    {
        public NodoExpr Izq { get; }
        public OperadorArit Operador { get; }
        public NodoExpr Der { get; }
        public NodoExprBin(NodoExpr izq, OperadorArit operador, NodoExpr der) { Izq = izq; Operador = operador; Der = der; }
    }

    internal abstract class NodoStmt { }

    internal sealed class NodoIf : NodoStmt
    {
        public NodoCond Cond { get; }
        public List<NodoStmt> Cuerpo { get; }
        /// <summary>Texto reconstruido de la condición (p.ej. "OPCION(\"A\",\"B\") Y OPCION(\"C\",\"D\")"),
        /// solo para mostrar en la UI/mapa conceptual; no participa en la evaluación.</summary>
        public string CondTexto { get; }
        public NodoIf(NodoCond cond, List<NodoStmt> cuerpo, string condTexto) { Cond = cond; Cuerpo = cuerpo; CondTexto = condTexto; }
    }

    internal sealed class NodoSetOpt : NodoStmt
    {
        public string Opcion { get; }
        public string Valor { get; }
        public NodoSetOpt(string opcion, string valor) { Opcion = opcion; Valor = valor; }
    }

    /// <summary>ESTABLECEOPCIONNUMERICA("Nombre",[Valor]); (confirmada por el usuario con el ejemplo
    /// real ESTABLECEOPCIONNUMERICA("AlturaManeta",[AM]);): a diferencia de NodoSetOpt -que asigna
    /// siempre un literal de texto entre comillas-, aquí el valor a asignar es el resultado NUMÉRICO
    /// de evaluar una expresión (ver NodoExpr: un literal, una variable, o una combinación con +, -,
    /// *, / y paréntesis -esto último extendido por consistencia con SEA/NodoSea, mismo convenio de
    /// corchetes, aunque el usuario solo confirmó una expresión con operador para SEA-), evaluada en
    /// el momento de ejecutar esta sentencia. Si esa expresión no se puede evaluar todavía porque
    /// alguna variable que usa no tiene valor conocido en ese punto del recorrido, no hay nada que
    /// asignar -confirmado por el usuario: "puede que no se establezca"-, así que la opción se queda
    /// sin establecer (no es un error).</summary>
    internal sealed class NodoSetOptNum : NodoStmt
    {
        public string Opcion { get; }
        public NodoExpr Valor { get; }
        public NodoSetOptNum(string opcion, NodoExpr valor) { Opcion = opcion; Valor = valor; }
    }

    internal sealed class NodoCallEsc : NodoStmt
    {
        public string Codigo { get; }
        public string Variables { get; }
        public NodoCallEsc(string codigo, string variables) { Codigo = codigo; Variables = variables; }
    }

    /// <summary>PREGUNTAVALOR("Variable"); (confirmada por el usuario: "es para preguntarle al
    /// usuario en modo depuración dentro de PREFERENCE qué valor tiene [la variable]. En nuestro
    /// caso ya lo estás haciendo"): en Preference real es una instrucción de depuración interactiva;
    /// en nuestra simulación equivale exactamente a lo que ya hace AsegurarValoresConocidos para una
    /// comparación de variable (NodoVarCmp) sobre una variable todavía desconocida -pausar y
    /// preguntar (ver EscandalloInterpreter.EjecutarStmt)-, solo que aquí se dispara de forma
    /// EXPLÍCITA en este punto del Programa en vez de reactivamente cuando hace falta para evaluar
    /// una condición. Si la variable YA se conoce en este punto, no hace nada (no hay nada nuevo que
    /// preguntar).</summary>
    internal sealed class NodoPreguntaValor : NodoStmt
    {
        public string Variable { get; }
        public NodoPreguntaValor(string variable) { Variable = variable; }
    }

    /// <summary>OPCIONES("..."); (confirmada por el usuario: "puedes obviarla"): se reconoce a nivel
    /// de gramática -para que no quede marcada como NodoUnrecognized en el Recorrido, que sería
    /// confuso ya que SÍ es una instrucción real y válida de Preference- pero no tiene ningún efecto
    /// en la simulación (ver EscandalloInterpreter.EjecutarStmt: case NodoOpciones, sin cuerpo). Se
    /// guarda igualmente el argumento bruto por si algún día hiciera falta para depurar, aunque hoy
    /// no se usa para nada.</summary>
    internal sealed class NodoOpciones : NodoStmt
    {
        public string Argumento { get; }
        public NodoOpciones(string argumento) { Argumento = argumento; }
    }

    /// <summary>SEA Variable = [Valor]; (confirmada por el usuario: "es para asignar un valor a una
    /// variable... ya sea mediante asignación numérica o asignación del contenido de una variable",
    /// con dos ejemplos reales: "SEA VARIABLE1 = [5];" y "SEA VARIABLE2 = [VARIABLE1];" — el valor
    /// va SIEMPRE entre corchetes, igual que el segundo argumento de ESTABLECEOPCIONNUMERICA; la
    /// primera versión de este parser no exigía los corchetes y por eso rechazaba
    /// "SEA LLAGA = [5];" como error). Dentro del corchete, "Valor" es una expresión aritmética
    /// completa (ver NodoExpr) -confirmado por el usuario con un tercer ejemplo real,
    /// "SEA AM = [A/2];" ("la instruccion quiere asignar el valor de A dividido entre 2 a la
    /// variable AM"), tras que la versión anterior de este parser -que solo admitía un NUM o un
    /// IDENT sueltos entre corchetes- rechazara esa división como error-: puede ser un literal
    /// numérico (p.ej. "[5]"), el nombre de otra variable ya conocida (p.ej. "[VARIABLE1]"), o
    /// cualquier combinación de ambos con +, -, *, / y paréntesis de agrupación (p.ej. "[A/2]",
    /// "[(A+B)/2]"). Se evalúa en ejecución vía EscandalloInterpreter.EvaluarExpr; si esa evaluación
    /// no se puede completar todavía porque alguna variable que usa la expresión no tiene valor
    /// conocido en ese punto del recorrido, no hay nada que asignar -mismo criterio que
    /// ESTABLECEOPCIONNUMERICA/AplicarVariablesDeLlamada: no es un error, la variable de la
    /// izquierda simplemente se queda sin valor-.</summary>
    internal sealed class NodoSea : NodoStmt
    {
        public string Variable { get; }
        public NodoExpr Valor { get; }
        public NodoSea(string variable, NodoExpr valor) { Variable = variable; Valor = valor; }
    }

    /// <summary>Cualquier fragmento que el parser no reconoce a nivel de sentencia (p.ej. un
    /// eventual MENSAJE(...); visto comentado en RO_Gestion Marcos, o cualquier otra construcción no
    /// contemplada en el alcance v1). Se conserva el texto bruto para mostrarlo como aviso en vez de
    /// abortar el parseo de todo el escandallo.</summary>
    internal sealed class NodoUnrecognized : NodoStmt
    {
        public string Texto { get; }
        public NodoUnrecognized(string texto) { Texto = texto; }
    }

    #endregion

    #region Parser

    internal sealed class ErrorParseoDsl : Exception
    {
        public ErrorParseoDsl(string mensaje) : base(mensaje) { }
    }

    internal sealed class ParserDsl
    {
        private readonly List<Token> _toks;
        private readonly string _texto;
        private int _i;

        /// <summary>"texto" es el mismo texto (ya sin comentarios) que se tokenizó para producir
        /// "tokens": Token.Pos son índices sobre ESTE texto, así que hace falta guardarlo aparte
        /// para poder traducir esas posiciones a línea/columna/fragmento cuando hay que construir un
        /// mensaje de error (ver ConstruirMensajeError/DescribirPosicion) — petición del usuario:
        /// "pueden los mensajes ser más claros y explicativos para que el usuario pueda corregirlo",
        /// a raíz de mensajes como "esperaba OPCION, encontrado LPAREN, símbolo paréntesis de
        /// apertura, en la posición 290" que exigían contar caracteres a mano para siquiera saber en
        /// qué línea estaba el problema. (Ese caso concreto -paréntesis de agrupación- ya no da
        /// error, ver ParseTerm: el usuario confirmó que es sintaxis válida y se añadió soporte.)</summary>
        public ParserDsl(List<Token> tokens, string texto) { _toks = tokens; _texto = texto; _i = 0; }

        private Token? Peek() => _i < _toks.Count ? _toks[_i] : null;
        private bool At(TipoToken tipo) => Peek()?.Tipo == tipo;
        private Token Advance() => _toks[_i++];

        private Token Expect(TipoToken tipo)
        {
            if (!At(tipo))
                throw new ErrorParseoDsl(ConstruirMensajeError(tipo));
            return Advance();
        }

        /// <summary>Nombre en español, sin jerga de tokenizer, de cada tipo de token: para que el
        /// mensaje de error diga "se esperaba la palabra OPCION" en vez de "esperaba OPCION" a secas
        /// (que obligaba a conocer la gramática interna para entender qué significaba "OPCION" ahí).</summary>
        private static string NombreLegible(TipoToken tipo) => tipo switch
        {
            TipoToken.STRING => "un texto entre comillas (p.ej. \"Sí\")",
            TipoToken.NUM => "un número",
            TipoToken.SI => "la palabra SI",
            TipoToken.ENTONCES => "la palabra ENTONCES",
            TipoToken.FINSI => "la palabra FINSI",
            TipoToken.OPCION => "la palabra OPCION",
            TipoToken.ESTABLECEOPCION => "la palabra ESTABLECEOPCION",
            TipoToken.ESTABLECEOPCIONNUMERICA => "la palabra ESTABLECEOPCIONNUMERICA",
            TipoToken.ESCANDALLO => "la palabra ESCANDALLO",
            TipoToken.PREGUNTAVALOR => "la palabra PREGUNTAVALOR",
            TipoToken.OPCIONES => "la palabra OPCIONES",
            TipoToken.SEA => "la palabra SEA",
            TipoToken.O => "la palabra O",
            TipoToken.Y => "la palabra Y",
            TipoToken.LBRACK => "el corchete de apertura '['",
            TipoToken.RBRACK => "el corchete de cierre ']'",
            TipoToken.LPAREN => "el paréntesis de apertura '('",
            TipoToken.RPAREN => "el paréntesis de cierre ')'",
            TipoToken.COMMA => "una coma ','",
            TipoToken.SEMI => "un punto y coma ';'",
            TipoToken.EQ => "el símbolo igual '='",
            TipoToken.LT => "el símbolo menor que '<'",
            TipoToken.GT => "el símbolo mayor que '>'",
            TipoToken.LE => "el símbolo menor o igual '<='",
            TipoToken.GE => "el símbolo mayor o igual '>='",
            TipoToken.IDENT => "el nombre de una variable (p.ej. \"A\")",
            TipoToken.PLUS => "el símbolo de suma '+'",
            TipoToken.MINUS => "el símbolo de resta '-'",
            TipoToken.STAR => "el símbolo de multiplicación '*'",
            TipoToken.SLASH => "el símbolo de división '/'",
            _ => tipo.ToString()
        };

        /// <summary>Construye el mensaje de error que antes era solo "esperaba X, encontrado
        /// Y:valor@posición" -técnicamente correcto pero ilegible para corregir el Programa sin
        /// abrir el código de este intérprete-: ahora incluye línea/columna (en vez de un offset de
        /// caracteres crudo) y un fragmento del propio Programa con el punto exacto señalado, para
        /// que el usuario pueda localizarlo directamente en el campo "Programa" del escandallo en
        /// Preference. Para el caso más habitual -se esperaba OPCION y no aparece- añade además una
        /// pista concreta con los TRES términos válidos que puede empezar aquí (OPCION(...), un
        /// paréntesis de agrupación, o una comparación de variable como "A=1200"): con las tres
        /// formas ya soportadas (ver ParseTerm/ParseCmp/ParseVarCmp), si esto se sigue disparando lo
        /// más probable es un error de tecleo real en el Programa, no una construcción que falte por
        /// reconocer.</summary>
        private string ConstruirMensajeError(TipoToken esperado)
        {
            Token? encontrado = Peek();
            string queEsperaba = NombreLegible(esperado);

            if (encontrado == null)
            {
                var (lineaFin, columnaFin, _) = DescribirPosicion(_texto.Length);
                return $"El Programa termina de forma inesperada (línea {lineaFin}, columna {columnaFin}): en ese punto se " +
                       $"esperaba {queEsperaba}, pero el texto ya se había acabado. Revisa que no falte cerrar algo cerca del " +
                       "final (un paréntesis '(' ')', unas comillas, un ';', un FINSI...).";
            }

            string queEncontro = $"{NombreLegible(encontrado.Tipo)} (\"{encontrado.Valor}\")";
            var (linea, columna, fragmento) = DescribirPosicion(encontrado.Pos);

            string mensaje = $"En la línea {linea}, columna {columna}: se esperaba {queEsperaba}, pero se ha encontrado " +
                              $"{queEncontro}.{Environment.NewLine}Fragmento del Programa en ese punto: {fragmento}";

            if (esperado == TipoToken.OPCION)
            {
                mensaje += $"{Environment.NewLine}Posible causa: aquí debía empezar un término de condición: la palabra OPCION " +
                           "(p.ej. OPCION(\"X\",\"Y\")), un paréntesis de apertura para agrupar, o el nombre de una variable " +
                           "seguido de un operador de comparación (p.ej. \"A=1200\", \"L<500\"). Revisa que no sea un simple " +
                           "error de tecleo (una coma, comilla o paréntesis de más o de menos justo antes de este punto).";
            }

            return mensaje;
        }

        /// <summary>Mensaje de error para ParseVarCmp cuando, tras el nombre de una variable, no
        /// sigue ninguno de los cinco operadores de comparación válidos (=, &lt;, &gt;, &lt;=, &gt;=).
        /// Construido a mano (no vía ConstruirMensajeError/Expect) porque aquí no hay un único
        /// TipoToken "esperado": son cinco alternativas posibles a la vez.</summary>
        private string ConstruirMensajeErrorOperadorVariable(string variable)
        {
            Token? encontrado = Peek();
            if (encontrado == null)
            {
                var (lineaFin, columnaFin, _) = DescribirPosicion(_texto.Length);
                return $"El Programa termina de forma inesperada (línea {lineaFin}, columna {columnaFin}): tras la variable " +
                       $"\"{variable}\" se esperaba un operador de comparación ('=', '<', '>', '<=' o '>='), pero el texto ya " +
                       "se había acabado.";
            }

            string queEncontro = $"{NombreLegible(encontrado.Tipo)} (\"{encontrado.Valor}\")";
            var (linea, columna, fragmento) = DescribirPosicion(encontrado.Pos);
            return $"En la línea {linea}, columna {columna}: tras la variable \"{variable}\" se esperaba un operador de " +
                   $"comparación ('=', '<', '>', '<=' o '>='), pero se ha encontrado {queEncontro}." +
                   $"{Environment.NewLine}Fragmento del Programa en ese punto: {fragmento}";
        }

        /// <summary>Traduce un índice de carácter sobre _texto a (línea, columna) 1-based, más un
        /// fragmento corto de esa misma línea (hasta 40 caracteres a cada lado, sin cruzar saltos de
        /// línea) con el punto exacto marcado como " ▶aquí▶ ", para no obligar al usuario a contar
        /// caracteres a mano sobre el campo "Programa" del escandallo.</summary>
        private (int Linea, int Columna, string Fragmento) DescribirPosicion(int pos)
        {
            pos = Math.Max(0, Math.Min(pos, _texto.Length));

            int linea = 1, inicioLinea = 0;
            for (int i = 0; i < pos; i++)
            {
                if (_texto[i] == '\n') { linea++; inicioLinea = i + 1; }
            }
            int columna = pos - inicioLinea + 1;

            int finLinea = _texto.IndexOf('\n', pos);
            if (finLinea < 0) finLinea = _texto.Length;

            const int Contexto = 40;
            int desde = Math.Max(inicioLinea, pos - Contexto);
            int hasta = Math.Min(finLinea, pos + Contexto);

            string antes = _texto.Substring(desde, pos - desde);
            string resto = _texto.Substring(pos, hasta - pos);

            string fragmento = (desde > inicioLinea ? "…" : "") + antes + " ▶aquí▶ " + resto + (hasta < finLinea ? "…" : "");
            return (linea, columna, fragmento.Trim());
        }

        public List<NodoStmt> ParseProgram(bool hastaFinsi = false)
        {
            var stmts = new List<NodoStmt>();
            while (true)
            {
                var t = Peek();
                if (t == null)
                {
                    if (hastaFinsi) throw new ErrorParseoDsl(ConstruirMensajeError(TipoToken.FINSI));
                    break;
                }
                if (hastaFinsi && t.Tipo == TipoToken.FINSI) break;
                var stmt = ParseStmt();
                if (stmt != null) stmts.Add(stmt);
            }
            return stmts;
        }

        // Tokens que claramente empiezan una sentencia nueva: sirven de límite defensivo cuando se
        // recupera texto no reconocido (ver ParseStmt), igual que en el prototipo Python.
        private static readonly HashSet<TipoToken> InicioStmtClaro = new()
        {
            TipoToken.SI, TipoToken.ESTABLECEOPCION, TipoToken.ESTABLECEOPCIONNUMERICA, TipoToken.ESCANDALLO,
            TipoToken.PREGUNTAVALOR, TipoToken.OPCIONES, TipoToken.SEA, TipoToken.FINSI
        };

        private NodoStmt? ParseStmt()
        {
            var t = Peek();
            if (t == null) return null;
            if (t.Tipo == TipoToken.SI) return ParseIf();
            if (t.Tipo == TipoToken.ESTABLECEOPCION) return ParseSetOpt();
            if (t.Tipo == TipoToken.ESTABLECEOPCIONNUMERICA) return ParseSetOptNum();
            if (t.Tipo == TipoToken.ESCANDALLO) return ParseCallEsc();
            if (t.Tipo == TipoToken.PREGUNTAVALOR) return ParsePreguntaValor();
            if (t.Tipo == TipoToken.OPCIONES) return ParseOpciones();
            if (t.Tipo == TipoToken.SEA) return ParseSea();

            // Token no reconocido a nivel de sentencia: se consume hasta el próximo ';' o hasta un
            // token que empiece claramente una sentencia nueva (lo que venga antes), y se devuelve
            // un NodoUnrecognized con el texto bruto consumido, en vez de abortar el parseo entero.
            var partes = new List<string>();
            while (true)
            {
                var t2 = Peek();
                if (t2 == null || InicioStmtClaro.Contains(t2.Tipo)) break;
                partes.Add(Advance().Valor);
                if (partes.Count > 0 && partes[^1] == ";") break;
            }
            if (partes.Count == 0)
            {
                // No se pudo avanzar (el primer token ya era uno "claro"): evita bucle infinito.
                Advance();
                return null;
            }
            return new NodoUnrecognized(string.Join(" ", partes));
        }

        private NodoIf ParseIf()
        {
            Expect(TipoToken.SI);
            Expect(TipoToken.LBRACK);
            var (cond, condTexto) = ParseOr();
            Expect(TipoToken.RBRACK);
            Expect(TipoToken.ENTONCES);
            var cuerpo = ParseProgram(hastaFinsi: true);
            Expect(TipoToken.FINSI);
            return new NodoIf(cond, cuerpo, condTexto);
        }

        private (NodoCond, string) ParseOr()
        {
            var (izq, txtIzq) = ParseAnd();
            var textos = new List<string> { txtIzq };
            NodoCond nodo = izq;
            while (At(TipoToken.O))
            {
                Advance();
                var (der, txtDer) = ParseAnd();
                nodo = new NodoOr(nodo, der);
                textos.Add(txtDer);
            }
            return (nodo, string.Join(" O ", textos));
        }

        private (NodoCond, string) ParseAnd()
        {
            var (izq, txtIzq) = ParseTerm();
            var textos = new List<string> { txtIzq };
            NodoCond nodo = izq;
            while (At(TipoToken.Y))
            {
                Advance();
                var (der, txtDer) = ParseTerm();
                nodo = new NodoAnd(nodo, der);
                textos.Add(txtDer);
            }
            return (nodo, string.Join(" Y ", textos));
        }

        /// <summary>Un "término" dentro de una condición: o bien una comparación suelta (por ahora
        /// solo OPCION(...), ver ParseCmp), o bien una subexpresión COMPLETA entre paréntesis "(" ")"
        /// -que puede contener a su vez cualquier combinación de Y/O, incluidos más paréntesis
        /// anidados, con la precedencia habitual (Y liga más fuerte que O dentro de cada nivel de
        /// paréntesis, igual que fuera)-. Antes esta función (entonces llamada ParseCmp, sin
        /// distinguir "término" de "comparación") exigía SIEMPRE empezar por OPCION, así que
        /// cualquier condición real que agrupara con paréntesis fallaba con "esperaba OPCION,
        /// encontrado LPAREN". Corregido: confirmado por el usuario que agrupar con paréntesis SÍ es
        /// sintaxis válida y habitual en los escandallos de Preference ("el uso de parentesis para
        /// agrupar condiciones, es admitido y válido").</summary>
        private (NodoCond, string) ParseTerm()
        {
            if (At(TipoToken.LPAREN))
            {
                Advance();
                var (interior, txtInterior) = ParseOr();
                Expect(TipoToken.RPAREN);
                return (interior, $"({txtInterior})");
            }
            return ParseCmp();
        }

        /// <summary>Una "comparación" (el nivel más interno de ParseTerm): OPCION("X","Y") con su
        /// forma negada opcional -como antes-, O BIEN (nuevo, ver NodoVarCmp) una comparación de
        /// variable "IDENT operador NUM" (p.ej. "A=1200", "L&lt;500"). Se distingue mirando el
        /// primer token: OPCION empieza siempre con la palabra clave OPCION, una comparación de
        /// variable empieza con un IDENT.</summary>
        private (NodoCond, string) ParseCmp()
        {
            if (At(TipoToken.IDENT))
                return ParseVarCmp();

            Expect(TipoToken.OPCION);
            Expect(TipoToken.LPAREN);
            string nombre = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.COMMA);
            string valor = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.RPAREN);
            bool negado = false;
            if (At(TipoToken.EQ))
            {
                Advance();
                var numTok = Expect(TipoToken.NUM);
                negado = numTok.Valor == "0";
            }
            string txt = $"OPCION(\"{nombre}\",\"{valor}\")" + (negado ? "=0" : "");
            return (new NodoCmp(nombre, valor, negado), txt);
        }

        /// <summary>"IDENT operador NUM", p.ej. "A=1200" o "L&lt;=500" (ver NodoVarCmp). El operador
        /// es obligatorio -a diferencia del "=0"/"=1" opcional de OPCION(...)-: aquí SIEMPRE hay un
        /// número a la derecha, porque una variable suelta sin comparar no significa nada en esta
        /// gramática.</summary>
        private (NodoCond, string) ParseVarCmp()
        {
            string variable = Expect(TipoToken.IDENT).Valor;

            OperadorCmp operador;
            string simbolo;
            if (At(TipoToken.LE)) { Advance(); operador = OperadorCmp.MenorIgual; simbolo = "<="; }
            else if (At(TipoToken.GE)) { Advance(); operador = OperadorCmp.MayorIgual; simbolo = ">="; }
            else if (At(TipoToken.EQ)) { Advance(); operador = OperadorCmp.Igual; simbolo = "="; }
            else if (At(TipoToken.LT)) { Advance(); operador = OperadorCmp.Menor; simbolo = "<"; }
            else if (At(TipoToken.GT)) { Advance(); operador = OperadorCmp.Mayor; simbolo = ">"; }
            else
                throw new ErrorParseoDsl(ConstruirMensajeErrorOperadorVariable(variable));

            var numTok = Expect(TipoToken.NUM);
            double valor = double.Parse(numTok.Valor, CultureInfo.InvariantCulture);
            string txt = $"{variable}{simbolo}{numTok.Valor}";
            return (new NodoVarCmp(variable, operador, valor), txt);
        }

        private NodoSetOpt ParseSetOpt()
        {
            Expect(TipoToken.ESTABLECEOPCION);
            Expect(TipoToken.LPAREN);
            string nombre = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.COMMA);
            string valor = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.RPAREN);
            Expect(TipoToken.SEMI);
            return new NodoSetOpt(nombre, valor);
        }

        /// <summary>ESTABLECEOPCIONNUMERICA("Nombre",[Valor]); (ver NodoSetOptNum): a diferencia de
        /// ParseSetOpt, el segundo argumento no es un literal de texto entre comillas, sino una
        /// expresión aritmética entre corchetes (ver ParseExprCorchete) -extendido desde un simple
        /// NUM/IDENT suelto por la misma razón que ParseSea, ver el comentario de esa función-.</summary>
        private NodoSetOptNum ParseSetOptNum()
        {
            Expect(TipoToken.ESTABLECEOPCIONNUMERICA);
            Expect(TipoToken.LPAREN);
            string nombre = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.COMMA);
            NodoExpr valor = ParseExprCorchete();
            Expect(TipoToken.RPAREN);
            Expect(TipoToken.SEMI);
            return new NodoSetOptNum(nombre, valor);
        }

        private NodoCallEsc ParseCallEsc()
        {
            Expect(TipoToken.ESCANDALLO);
            Expect(TipoToken.LPAREN);
            string codigo = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.COMMA);
            string variables = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.RPAREN);
            Expect(TipoToken.SEMI);
            return new NodoCallEsc(codigo, variables);
        }

        /// <summary>PREGUNTAVALOR("Variable"); (ver NodoPreguntaValor): mismo argumento entre
        /// comillas que ESTABLECEOPCION/OPCIONES, pero aquí es el NOMBRE de una variable, no una
        /// opción -por eso QuitarComillas y ya está, sin corchetes: el propio texto entrecomillado ES
        /// el nombre-.</summary>
        private NodoPreguntaValor ParsePreguntaValor()
        {
            Expect(TipoToken.PREGUNTAVALOR);
            Expect(TipoToken.LPAREN);
            string variable = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.RPAREN);
            Expect(TipoToken.SEMI);
            return new NodoPreguntaValor(variable);
        }

        /// <summary>OPCIONES("..."); (ver NodoOpciones): se parsea igual que ESTABLECEOPCION (un
        /// único argumento entre comillas) pero no tiene ningún efecto en la simulación.</summary>
        private NodoOpciones ParseOpciones()
        {
            Expect(TipoToken.OPCIONES);
            Expect(TipoToken.LPAREN);
            string argumento = QuitarComillas(Expect(TipoToken.STRING).Valor);
            Expect(TipoToken.RPAREN);
            Expect(TipoToken.SEMI);
            return new NodoOpciones(argumento);
        }

        /// <summary>SEA Variable = [Valor]; (ver NodoSea) — el valor a la derecha del '=' va SIEMPRE
        /// entre corchetes (confirmado por el usuario con dos ejemplos reales, "SEA VARIABLE1 = [5];"
        /// y "SEA VARIABLE2 = [VARIABLE1];", tras que la primera versión de este parser -sin
        /// corchetes- rechazara "SEA LLAGA = [5];" como error). Mismo corchete que ya usa
        /// ESTABLECEOPCIONNUMERICA para su segundo argumento: en este DSL, un valor entre "[" "]" es
        /// siempre una referencia numérica, nunca un literal de texto entre comillas. Dentro del
        /// corchete, "Valor" es ahora una expresión aritmética completa (ver ParseExprCorchete), no
        /// solo un NUM o un IDENT sueltos -extendido tras un tercer ejemplo real del usuario,
        /// "SEA AM = [A/2];" ("la instruccion quiere asignar el valor de A dividido entre 2 a la
        /// variable AM"), que la versión anterior de esta función rechazaba con el mismo tipo de
        /// error que ya había dado el caso de los decimales ("se esperaba el corchete de cierre...
        /// pero se ha encontrado un número") porque solo sabía consumir un único token dentro del
        /// corchete.</summary>
        private NodoSea ParseSea()
        {
            Expect(TipoToken.SEA);
            string variable = Expect(TipoToken.IDENT).Valor;
            Expect(TipoToken.EQ);
            NodoExpr valor = ParseExprCorchete();
            Expect(TipoToken.SEMI);
            return new NodoSea(variable, valor);
        }

        /// <summary>Un valor entre corchetes -"[Expresión]"-, usado tanto por SEA como por el segundo
        /// argumento de ESTABLECEOPCIONNUMERICA (ver ParseSea/ParseSetOptNum): delega en ParseExprAdd
        /// para la expresión propiamente dicha.</summary>
        private NodoExpr ParseExprCorchete()
        {
            Expect(TipoToken.LBRACK);
            NodoExpr expr = ParseExprAdd();
            Expect(TipoToken.RBRACK);
            return expr;
        }

        /// <summary>Nivel de precedencia más débil dentro de una expresión: suma y resta, evaluadas
        /// de izquierda a derecha (p.ej. "A-B+C" es "(A-B)+C"). Delega en ParseExprMul para cada
        /// operando, de forma que "*" y "/" liguen más fuerte -p.ej. "A+B/2" es "A+(B/2)", no
        /// "(A+B)/2"-, la precedencia aritmética habitual.</summary>
        private NodoExpr ParseExprAdd()
        {
            NodoExpr izq = ParseExprMul();
            while (At(TipoToken.PLUS) || At(TipoToken.MINUS))
            {
                OperadorArit operador = Advance().Tipo == TipoToken.PLUS ? OperadorArit.Suma : OperadorArit.Resta;
                NodoExpr der = ParseExprMul();
                izq = new NodoExprBin(izq, operador, der);
            }
            return izq;
        }

        /// <summary>Multiplicación y división, evaluadas de izquierda a derecha (p.ej. "A/2*B" es
        /// "(A/2)*B"). Delega en ParseExprAtom para cada operando -un literal, una variable, o una
        /// subexpresión completa entre paréntesis-.</summary>
        private NodoExpr ParseExprMul()
        {
            NodoExpr izq = ParseExprAtom();
            while (At(TipoToken.STAR) || At(TipoToken.SLASH))
            {
                OperadorArit operador = Advance().Tipo == TipoToken.STAR ? OperadorArit.Mult : OperadorArit.Div;
                NodoExpr der = ParseExprAtom();
                izq = new NodoExprBin(izq, operador, der);
            }
            return izq;
        }

        /// <summary>El nivel más interno de una expresión: un número (NUM), el nombre de una
        /// variable (IDENT), un signo menos delante de otro átomo -para admitir un negativo directo,
        /// p.ej. "[-5]", aunque el usuario no lo ha confirmado con un ejemplo real, se admite por
        /// coherencia con la resta binaria ya soportada-, o una subexpresión COMPLETA entre
        /// paréntesis "(" ")" -reutilizando LPAREN/RPAREN, ya existentes para agrupar condiciones-.</summary>
        private NodoExpr ParseExprAtom()
        {
            if (At(TipoToken.LPAREN))
            {
                Advance();
                NodoExpr interior = ParseExprAdd();
                Expect(TipoToken.RPAREN);
                return interior;
            }
            if (At(TipoToken.MINUS))
            {
                Advance();
                NodoExpr interior = ParseExprAtom();
                return new NodoExprBin(new NodoExprNum(0), OperadorArit.Resta, interior);
            }
            if (At(TipoToken.NUM))
                return new NodoExprNum(double.Parse(Advance().Valor, CultureInfo.InvariantCulture));
            return new NodoExprVar(Expect(TipoToken.IDENT).Valor);
        }

        private static string QuitarComillas(string s) => s.Substring(1, s.Length - 2);
    }

    /// <summary>Punto de entrada del parser: quita comentarios, tokeniza y parsea un texto de
    /// Programa completo. Lanza ErrorParseoDsl si el texto no encaja en la gramática.</summary>
    internal static class DslParser
    {
        public static List<NodoStmt> ParsePrograma(string texto)
        {
            string limpio = Tokenizer.QuitarComentarios(texto);
            var toks = Tokenizer.Tokenizar(limpio);
            // "limpio" (no el "texto" original) porque Token.Pos son índices sobre el texto YA sin
            // comentarios -es el que de verdad se tokenizó-, y ParserDsl lo necesita para poder
            // traducir esas posiciones a línea/columna/fragmento en sus mensajes de error.
            var p = new ParserDsl(toks, limpio);
            return p.ParseProgram(hastaFinsi: false);
        }
    }

    #endregion
}
