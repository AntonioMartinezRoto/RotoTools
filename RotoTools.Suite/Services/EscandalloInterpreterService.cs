using System.Globalization;
using Microsoft.Data.SqlClient;

namespace RotoTools.Suite.Services
{
    /// <summary>Tipo de cada evento que produce EscandalloInterpreter al recorrer una cadena de
    /// escandallos. El único que debe pausar el avance automático en la UI (asistente de
    /// Simulación) es PreguntarOpcion: todos los demás deben auto-consumirse/registrarse en el log
    /// sin detener el recorrido (así lo pidió el usuario: "que siga el camino" hasta llegar a un
    /// punto donde haga falta preguntar).</summary>
    public enum TipoEventoSimulacion
    {
        EntrandoEscandallo,
        SaliendoEscandallo,
        OpcionEstablecida,
        /// <summary>Una VARIABLE (ver EscandalloDsl.NodoVarCmp) recibió valor al entrar en un
        /// escandallo, vía el segundo argumento de ESCANDALLO("Codigo","Variables"); que lo llamó
        /// (ver AplicarVariablesDeLlamada) — no vía ESTABLECEOPCION, por eso es un tipo de evento
        /// aparte de OpcionEstablecida. Se registra en el Recorrido igual que cualquier otro paso,
        /// por la misma razón que pidió el usuario para las opciones: "es importante localizar desde
        /// donde se pueden estar estableciendo nuestras opciones".</summary>
        VariablePasada,
        /// <summary>Una VARIABLE recibió valor mediante una sentencia SEA Variable = Valor; (ver
        /// EscandalloDsl.NodoSea) — no vía ESCANDALLO(...) ni ESTABLECEOPCION, por eso es un tipo
        /// de evento aparte de VariablePasada/OpcionEstablecida. Se registra en el Recorrido por
        /// la misma razón que las demás asignaciones: localizar desde dónde se establece cada
        /// valor.</summary>
        VariableAsignada,
        PreguntarOpcion,
        NoReconocido,
        ErrorParseo,
        EscandalloNoEncontrado,
        CicloDetectado,
        Fin
    }

    /// <summary>Un paso del recorrido. Los campos que no aplican a un Tipo concreto quedan a null
    /// (p.ej. Opcion/Valor solo se rellenan en OpcionEstablecida/PreguntarOpcion/VariablePasada).</summary>
    public sealed class EventoSimulacion
    {
        public TipoEventoSimulacion Tipo { get; set; }
        public string Escandallo { get; set; } = "";
        public string? Opcion { get; set; }
        public string? Valor { get; set; }
        public string? Mensaje { get; set; }
        public IReadOnlyList<string>? PilaLlamadas { get; set; }

        /// <summary>Solo relevante en PreguntarOpcion: true si lo que hay que responder es una
        /// VARIABLE (EscandalloDsl.NodoVarCmp, p.ej. "A" en una condición "A=1200") en vez de una
        /// opción real del catálogo o un flag interno del DSL. La UI (SimulacionPage.MostrarPregunta)
        /// lo usa para pedir directamente un valor NUMÉRICO en vez de ir a buscar una lista de
        /// valores posibles en la tabla Opciones (que no tiene ninguna fila para el nombre de una
        /// variable).</summary>
        public bool EsVariable { get; set; }
    }

    /// <summary>Un asiento del historial de una opción: qué escandallo le puso qué valor y por qué
    /// vía (Origen). Se conserva TODO el historial, no solo el último valor (Q3 del usuario:
    /// "historial completo", porque detectar un valor pisado por un escandallo posterior es
    /// justamente el tipo de bug que esta funcionalidad busca localizar).</summary>
    public sealed class HistorialOpcion
    {
        public string Escandallo { get; set; } = "";
        public string Valor { get; set; } = "";
        public string Origen { get; set; } = "";
    }

    /// <summary>Estado acumulado de una opción durante la simulación: su valor actual (el último
    /// asignado) y el historial completo de quién se lo fue asignando.</summary>
    public sealed class EstadoOpcion
    {
        public string? Valor { get; set; }
        public List<HistorialOpcion> Historial { get; } = new();
    }

    /// <summary>
    /// Intérprete "pausable" del DSL de escandallos (ver EscandalloDsl.cs para la gramática/AST):
    /// recorre una cadena de escandallos empezando por uno inicial (típicamente el asociado a la
    /// hoja de un dibujo, o "RO_Gestion Herraje" directamente) y, cada vez que una condición SI[...]
    /// necesita el valor de una opción que todavía no se conoce, produce un evento PreguntarOpcion y
    /// espera: el consumidor (la UI del asistente de Simulación) debe entonces llamar a
    /// ResponderOpcion(...) con la respuesta del usuario ANTES de pedir el siguiente evento
    /// (MoveNext()/próxima vuelta del foreach). Este es el mismo patrón "generador pausable" que el
    /// prototipo Python validado (interp.py, yield + generator.send()), adaptado a C#: como
    /// IEnumerable&lt;T&gt;/yield return de C# no admite "enviar" un valor de vuelta al punto donde
    /// se pausó (a diferencia de los generadores de Python), la respuesta viaja por el propio
    /// diccionario de Estado (que ResponderOpcion actualiza) en vez de por el valor de retorno del
    /// yield: cuando la enumeración se reanuda, el código simplemente vuelve a leer Estado, que ya
    /// tiene el valor nuevo. Es exactamente el mismo mecanismo que usaba la versión Python por
    /// debajo (allí también se escribía en self.estado justo después del yield), así que el
    /// comportamiento -incluido el orden de las preguntas y el cortocircuito de Y/O- es idéntico.
    ///
    /// A diferencia del resto de Services de este proyecto (que son "static class" sin estado, ver
    /// DibujoConstructivosService/DibujoOpcionesRotoService), esta clase es intencionalmente una
    /// instancia con estado propio (Estado, pila de llamadas, caché de AST): cada simulación del
    /// asistente necesita su propio recorrido aislado, y varias simulaciones (o una reiniciada) no
    /// deben compartir ni pisarse el estado.
    ///
    /// Funcionalidad de solo lectura: no escribe nada en BBDD ni en el XML de ningún dibujo, solo
    /// lee el texto Programa de la tabla Escandallos (ver EscandalloProgramaLoader) y, más adelante
    /// (tarea #10), los valores reales de las opciones de la hoja elegida.
    /// </summary>
    public sealed class EscandalloInterpreter
    {
        public const string OrigenEstablecido = "establecido";
        public const string OrigenElegidoUsuario = "elegido_usuario";

        /// <summary>Origen reservado para los valores precargados desde el dibujo real (Q1 del
        /// usuario: "mixto, con opción a cambiarlo") vía PrecargarValorConocido. No lo usa este
        /// intérprete por sí solo: lo rellenará la tarea #10 antes de arrancar Ejecutar(...).</summary>
        public const string OrigenValorRealDibujo = "valor_real_dibujo";

        /// <summary>Origen de una VARIABLE (EscandalloDsl.NodoVarCmp) cuyo valor llegó al entrar en
        /// un escandallo vía el segundo argumento de la llamada ESCANDALLO("Codigo","Variables");
        /// que lo invocó (ver AplicarVariablesDeLlamada) — a diferencia de OrigenElegidoUsuario, que
        /// es cuando el propio usuario la escribe a mano en el Paso 4 porque nadie se la pasó.</summary>
        public const string OrigenVariablePasada = "variable_pasada";

        /// <summary>Origen de una OPCIÓN establecida por ESTABLECEOPCIONNUMERICA("Nombre",[Valor]);
        /// (ver NodoSetOptNum/EjecutarStmt) — a diferencia de OrigenEstablecido (ESTABLECEOPCION con
        /// un literal de texto entre comillas), aquí el valor asignado es el resultado NUMÉRICO de
        /// evaluar una expresión (un literal, una variable, o una combinación con +, -, *, /), así
        /// que interesa distinguirlo en el "Origen" del mapa conceptual (ver
        /// SimulacionPage.TraducirOrigen).</summary>
        public const string OrigenEstablecidoNumerico = "establecido_numerico";

        /// <summary>Origen de una VARIABLE asignada por una sentencia SEA Variable = Valor; (ver
        /// EscandalloDsl.NodoSea/EjecutarStmt).</summary>
        public const string OrigenAsignadoPorSea = "asignado_sea";

        private readonly Func<string, string?> _cargarPrograma;
        private readonly Dictionary<string, AstCacheEntry> _cacheAst = new();
        private readonly List<string> _pilaLlamadas = new();

        /// <summary>Estado por opción (valor actual + historial completo), expuesto para que la UI
        /// pueda construir el "mapa conceptual" final directamente desde aquí al terminar el
        /// recorrido (evento Fin).</summary>
        public Dictionary<string, EstadoOpcion> Estado { get; } = new();

        private sealed class AstCacheEntry
        {
            public bool Encontrado;
            public List<NodoStmt>? Stmts;
            public string? ErrorParseo;
        }

        /// <summary>Constructor por defecto: carga los Programa reales desde la tabla Escandallos
        /// (ver EscandalloProgramaLoader).</summary>
        public EscandalloInterpreter() : this(EscandalloProgramaLoader.CargarProgramaPorCodigo) { }

        /// <summary>Constructor con carga de Programa inyectada: útil para pruebas contra texto en
        /// memoria (igual que el prototipo Python, que recibía "cargar_programa" como función) sin
        /// tocar la base de datos.</summary>
        public EscandalloInterpreter(Func<string, string?> cargarPrograma)
        {
            _cargarPrograma = cargarPrograma;
        }

        /// <summary>Fija de antemano el valor conocido de una opción con procedencia "valor real del
        /// dibujo", para que la simulación no pregunte por ella y en su lugar muestre que ese valor
        /// viene del dibujo elegido (editable por el usuario llamando a ResponderOpcion más tarde si
        /// quiere explorar una hipótesis distinta: la última entrada de Historial siempre gana).
        /// Debe llamarse ANTES de Ejecutar(...).</summary>
        public void PrecargarValorConocido(string opcion, string valor)
        {
            // .Trim(): los valores reales que vienen de BBDD (p.ej. columnas CHAR(n), que SQL
            // Server rellena con espacios hasta el ancho fijo) pueden traer espacios finales que no
            // están en los literales que compara el Programa del escandallo (esos se escriben a
            // mano). Sin este Trim, ValorConocido(...) == cmp.Valor en EvaluarCondicionPura no
            // coincide nunca aunque "parezca" el mismo valor -exactamente el bug reportado con
            // HardwareSupplier, confirmado por el usuario-.
            valor = valor.Trim();
            var entry = ObtenerOCrearEstado(opcion);
            entry.Valor = valor;
            entry.Historial.Add(new HistorialOpcion { Escandallo = "(dibujo)", Valor = valor, Origen = OrigenValorRealDibujo });
        }

        /// <summary>Debe llamarse tras recibir un evento PreguntarOpcion, ANTES de pedir el
        /// siguiente evento (antes de la próxima vuelta del foreach/MoveNext()): fija la respuesta
        /// del usuario para esa opción con procedencia "elegido_usuario". Si no se llama, la opción
        /// sigue sin valor conocido y la condición que la usaba se evalúa como "no coincide" (ver
        /// EvaluarCondicionPura) en vez de bloquear la simulación.</summary>
        public void ResponderOpcion(string opcion, string valor, string escandallo)
        {
            // Mismo motivo que en PrecargarValorConocido: el valor elegido en la lista de "valores
            // posibles" del asistente viene de RotoTools.Helpers.GetContenidoOpciones (lectura
            // directa de BBDD), así que puede traer el mismo relleno de espacios finales.
            valor = valor.Trim();
            var entry = ObtenerOCrearEstado(opcion);
            entry.Valor = valor;
            entry.Historial.Add(new HistorialOpcion { Escandallo = escandallo, Valor = valor, Origen = OrigenElegidoUsuario });
        }

        private EstadoOpcion ObtenerOCrearEstado(string opcion)
        {
            if (!Estado.TryGetValue(opcion, out var entry))
            {
                entry = new EstadoOpcion();
                Estado[opcion] = entry;
            }
            return entry;
        }

        private string? ValorConocido(string opcion) => Estado.TryGetValue(opcion, out var entry) ? entry.Valor : null;

        /// <summary>Evalúa una expresión aritmética entre corchetes (ver EscandalloDsl.NodoExpr,
        /// usada por SEA y por el segundo argumento de ESTABLECEOPCIONNUMERICA), leyendo variables
        /// desde Estado igual que ValorConocido. Devuelve null -en vez de lanzar o de propagar
        /// Infinity/NaN- tanto si alguna variable de la expresión todavía no tiene valor conocido en
        /// este punto del recorrido, como si hay una división entre cero: mismo criterio de "no
        /// bloquear ni dar error" que el resto del intérprete (ver comentario de NodoExpr), la
        /// asignación que dependa de este resultado simplemente se queda sin hacer todavía, no es un
        /// error de simulación.</summary>
        private double? EvaluarExpr(NodoExpr expr)
        {
            switch (expr)
            {
                case NodoExprNum num:
                    return num.Valor;
                case NodoExprVar var_:
                {
                    string? valorTexto = ValorConocido(var_.Variable);
                    if (valorTexto == null) return null;
                    return double.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out double valorNum)
                        ? valorNum
                        : null;
                }
                case NodoExprBin bin:
                {
                    double? izq = EvaluarExpr(bin.Izq);
                    double? der = EvaluarExpr(bin.Der);
                    if (izq == null || der == null) return null;
                    return bin.Operador switch
                    {
                        OperadorArit.Suma => izq.Value + der.Value,
                        OperadorArit.Resta => izq.Value - der.Value,
                        OperadorArit.Mult => izq.Value * der.Value,
                        // División entre cero: se trata como "no resuelto todavía" (null), no como
                        // Infinity/NaN ni como excepción -mismo criterio que una variable desconocida.
                        OperadorArit.Div => der.Value == 0 ? null : izq.Value / der.Value,
                        _ => null
                    };
                }
                default:
                    return null;
            }
        }

        /// <summary>Recorre la cadena de escandallos empezando por codigoInicial, produciendo un
        /// evento por cada paso (ver TipoEventoSimulacion). Debe consumirse manualmente
        /// (GetEnumerator/MoveNext, no un foreach que agote todo de golpe) cuando se espera recibir
        /// PreguntarOpcion, para poder llamar a ResponderOpcion(...) antes de reanudar.</summary>
        public IEnumerable<EventoSimulacion> Ejecutar(string codigoInicial)
        {
            foreach (var ev in EjecutarEscandallo(codigoInicial))
                yield return ev;
            yield return new EventoSimulacion { Tipo = TipoEventoSimulacion.Fin };
        }

        private AstCacheEntry ObtenerAst(string codigo)
        {
            if (_cacheAst.TryGetValue(codigo, out var cached)) return cached;

            AstCacheEntry entry;
            string? texto = _cargarPrograma(codigo);
            if (texto == null)
            {
                entry = new AstCacheEntry { Encontrado = false };
            }
            else
            {
                try
                {
                    entry = new AstCacheEntry { Encontrado = true, Stmts = DslParser.ParsePrograma(texto) };
                }
                catch (ErrorParseoDsl ex)
                {
                    entry = new AstCacheEntry { Encontrado = true, ErrorParseo = ex.Message };
                }
            }
            _cacheAst[codigo] = entry;
            return entry;
        }

        private IEnumerable<EventoSimulacion> EjecutarEscandallo(string codigo)
        {
            if (_pilaLlamadas.Contains(codigo))
            {
                yield return new EventoSimulacion
                {
                    Tipo = TipoEventoSimulacion.CicloDetectado,
                    Escandallo = codigo,
                    PilaLlamadas = new List<string>(_pilaLlamadas)
                };
                yield break;
            }

            var ast = ObtenerAst(codigo);
            if (!ast.Encontrado)
            {
                yield return new EventoSimulacion { Tipo = TipoEventoSimulacion.EscandalloNoEncontrado, Escandallo = codigo };
                yield break;
            }
            if (ast.ErrorParseo != null)
            {
                yield return new EventoSimulacion { Tipo = TipoEventoSimulacion.ErrorParseo, Escandallo = codigo, Mensaje = ast.ErrorParseo };
                yield break;
            }

            _pilaLlamadas.Add(codigo);
            yield return new EventoSimulacion { Tipo = TipoEventoSimulacion.EntrandoEscandallo, Escandallo = codigo };
            foreach (var ev in EjecutarBloque(ast.Stmts!, codigo))
                yield return ev;
            yield return new EventoSimulacion { Tipo = TipoEventoSimulacion.SaliendoEscandallo, Escandallo = codigo };
            _pilaLlamadas.RemoveAt(_pilaLlamadas.Count - 1);
        }

        private IEnumerable<EventoSimulacion> EjecutarBloque(List<NodoStmt> stmts, string esc)
        {
            foreach (var stmt in stmts)
                foreach (var ev in EjecutarStmt(stmt, esc))
                    yield return ev;
        }

        private IEnumerable<EventoSimulacion> EjecutarStmt(NodoStmt stmt, string esc)
        {
            switch (stmt)
            {
                case NodoIf ifStmt:
                {
                    foreach (var ev in AsegurarValoresConocidos(ifStmt.Cond, esc))
                        yield return ev;
                    if (EvaluarCondicionPura(ifStmt.Cond))
                    {
                        foreach (var ev in EjecutarBloque(ifStmt.Cuerpo, esc))
                            yield return ev;
                    }
                    break;
                }
                case NodoSetOpt setOpt:
                {
                    // Trim también aquí por consistencia (ver PrecargarValorConocido/ResponderOpcion):
                    // en la práctica el literal de ESTABLECEOPCION ya viene limpio del tokenizador,
                    // pero así todo valor que entra en Estado pasa por el mismo criterio.
                    string valorEstablecido = (setOpt.Valor ?? "").Trim();
                    var entry = ObtenerOCrearEstado(setOpt.Opcion);
                    entry.Valor = valorEstablecido;
                    entry.Historial.Add(new HistorialOpcion { Escandallo = esc, Valor = valorEstablecido, Origen = OrigenEstablecido });
                    yield return new EventoSimulacion
                    {
                        Tipo = TipoEventoSimulacion.OpcionEstablecida,
                        Escandallo = esc,
                        Opcion = setOpt.Opcion,
                        Valor = valorEstablecido
                    };
                    break;
                }
                case NodoSetOptNum setOptNum:
                {
                    // ESTABLECEOPCIONNUMERICA("Nombre",[Valor]);: a diferencia de NodoSetOpt, el
                    // valor a asignar no viene en el propio Programa como literal, sino que hay que
                    // EVALUAR AHORA una expresión aritmética (ver EvaluarExpr; un simple número o
                    // variable son el caso más común, pero puede llevar +, -, *, /, igual que SEA).
                    // Si esa expresión no se puede evaluar todavía -alguna variable que usa no tiene
                    // valor conocido en este punto del recorrido-, no hay nada que asignar: la opción
                    // se queda sin establecer, sin generar ningún evento ni error -confirmado por el
                    // usuario ("puede que no se establezca"), mismo criterio que un NodoVarCmp sobre
                    // una variable desconocida se evalúa como "no coincide" en vez de bloquear la
                    // simulación-.
                    double? valorNum = EvaluarExpr(setOptNum.Valor);
                    if (valorNum != null)
                    {
                        string valorTexto = valorNum.Value.ToString(CultureInfo.InvariantCulture);
                        var entryNum = ObtenerOCrearEstado(setOptNum.Opcion);
                        entryNum.Valor = valorTexto;
                        entryNum.Historial.Add(new HistorialOpcion { Escandallo = esc, Valor = valorTexto, Origen = OrigenEstablecidoNumerico });
                        yield return new EventoSimulacion
                        {
                            Tipo = TipoEventoSimulacion.OpcionEstablecida,
                            Escandallo = esc,
                            Opcion = setOptNum.Opcion,
                            Valor = valorTexto
                        };
                    }
                    break;
                }
                case NodoCallEsc callEsc:
                    // ANTES de entrar en el escandallo llamado: procesa su segundo argumento
                    // "Variables" (ver AplicarVariablesDeLlamada), para que las variables que
                    // necesite ya estén en Estado cuando AsegurarValoresConocidos las busque dentro
                    // de él, en vez de preguntarlas de más solo porque todavía no habían llegado.
                    foreach (var (nombreVar, valorVar) in AplicarVariablesDeLlamada(callEsc.Variables, esc))
                    {
                        yield return new EventoSimulacion
                        {
                            Tipo = TipoEventoSimulacion.VariablePasada,
                            Escandallo = esc,
                            Opcion = nombreVar,
                            Valor = valorVar
                        };
                    }
                    foreach (var ev in EjecutarEscandallo(callEsc.Codigo))
                        yield return ev;
                    break;
                case NodoPreguntaValor preguntaValor:
                    // PREGUNTAVALOR("Variable"); (confirmado por el usuario: en Preference es una
                    // instrucción de depuración interactiva para que el usuario vea el valor de
                    // "Variable"; "en nuestro caso ya lo estás haciendo"). Aquí equivale exactamente
                    // a lo que ya hace AsegurarValoresConocidos para una comparación de variable
                    // desconocida (NodoVarCmp) -pausar y preguntar-, solo que disparado
                    // EXPLÍCITAMENTE en este punto del Programa en vez de reactivamente al evaluar
                    // una condición. Si la variable YA se conoce aquí, no hay nada nuevo que
                    // preguntar.
                    if (ValorConocido(preguntaValor.Variable) == null)
                    {
                        yield return new EventoSimulacion
                        {
                            Tipo = TipoEventoSimulacion.PreguntarOpcion,
                            Escandallo = esc,
                            Opcion = preguntaValor.Variable,
                            EsVariable = true
                        };
                    }
                    break;
                case NodoOpciones:
                    // OPCIONES("..."); confirmado por el usuario: "puedes obviarla". Se reconoce a
                    // nivel de gramática -para no ensuciar el Recorrido con un aviso de "no
                    // reconocido" sobre algo que SÍ es una instrucción válida de Preference- pero no
                    // tiene ningún efecto aquí: ni evento, ni cambio de Estado.
                    break;
                case NodoSea sea:
                {
                    // SEA Variable = [Valor]; (ver NodoSea): "Valor" es una expresión aritmética
                    // completa (ver EvaluarExpr) -un literal numérico o el nombre de otra variable
                    // ya conocida son el caso más común (p.ej. "SEA A = [5];", "SEA A = [L];"), pero
                    // también puede combinar +, -, *, / y paréntesis (p.ej. "SEA AM = [A/2];",
                    // confirmado por el usuario). Si esa expresión no se puede evaluar todavía -p.ej.
                    // "SEA A = [L];" con L todavía desconocida en este punto-, no hay nada que
                    // asignar todavía: no es un error, igual que ESTABLECEOPCIONNUMERICA.
                    double? valorNum = EvaluarExpr(sea.Valor);

                    if (valorNum != null)
                    {
                        string valorResuelto = valorNum.Value.ToString(CultureInfo.InvariantCulture);
                        var entrySea = ObtenerOCrearEstado(sea.Variable);
                        entrySea.Valor = valorResuelto;
                        entrySea.Historial.Add(new HistorialOpcion { Escandallo = esc, Valor = valorResuelto, Origen = OrigenAsignadoPorSea });
                        yield return new EventoSimulacion
                        {
                            Tipo = TipoEventoSimulacion.VariableAsignada,
                            Escandallo = esc,
                            Opcion = sea.Variable,
                            Valor = valorResuelto
                        };
                    }
                    break;
                }
                case NodoUnrecognized unrec:
                    yield return new EventoSimulacion { Tipo = TipoEventoSimulacion.NoReconocido, Escandallo = esc, Mensaje = unrec.Texto };
                    break;
            }
        }

        /// <summary>
        /// Procesa el segundo argumento de un ESCANDALLO("Codigo","Variables"); ANTES de entrar en
        /// el escandallo llamado, aplicando cada asignación a Estado y devolviendo (para que
        /// EjecutarStmt registre un evento VariablePasada por cada una) los pares nombre/valor que
        /// de verdad se han podido resolver aquí.
        ///
        /// Confirmado por el usuario, dos formas reales, separadas por ";" si hay varias:
        /// - "nombre=otroNombre" (p.ej. "A=L1;L=L2;", la forma real con la que "Constructivo Hoja"
        ///   recibe sus variables: "L1 y L2 tienen las cotas de alto y ancho del modelo"): si
        ///   "otroNombre" (L1) YA tiene un valor conocido en Estado, se copia a "nombre" (A) con
        ///   origen OrigenVariablePasada. Si "otroNombre" TODAVÍA no se conoce -el caso más
        ///   habitual, ya que L1/L2 no los establece este DSL en ningún sitio: son cotas que
        ///   Preference conoce por su cuenta y de las que aquí no hay ninguna otra fuente-, no se
        ///   hace nada: cuando el escandallo llamado de verdad necesite "A", se le preguntará
        ///   directamente al usuario (ver AsegurarValoresConocidos/NodoVarCmp), que puede consultar
        ///   la cota real en el propio dibujo.
        /// - "nombre" a secas, sin "=" (p.ej. "A" en ESCANDALLO("Constructivo Hoja_2","A");, la
        ///   forma con la que "Constructivo Hoja" reenvía su propia variable A -ya resuelta o no- a
        ///   "Constructivo Hoja_2"): no hace falta ninguna acción. Estado es un único almacén GLOBAL
        ///   para toda la simulación (no hay un scope de variables distinto por cada llamada): como
        ///   "A" ya es la misma entrada de Estado en cualquier punto del recorrido, "reenviarla" no
        ///   implica copiar nada. Esta simplificación es razonable porque una simulación entera
        ///   recorre la cadena de UNA sola hoja: el mismo nombre de variable (A, L...) representa la
        ///   misma cota real a lo largo de todo ese recorrido, así que no hace falta aislar valores
        ///   entre llamadas como si fueran parámetros de función independientes.
        ///
        /// También soporta, por si apareciera en algún escandallo real, "nombre=NUMERO" (p.ej.
        /// "A=1200"): fija ese valor directamente, con el mismo origen.
        /// </summary>
        private IEnumerable<(string Nombre, string Valor)> AplicarVariablesDeLlamada(string variables, string escandalloLlamador)
        {
            if (string.IsNullOrWhiteSpace(variables)) yield break;

            foreach (var segmentoCrudo in variables.Split(';'))
            {
                string segmento = segmentoCrudo.Trim();
                if (segmento.Length == 0) continue;

                int idxIgual = segmento.IndexOf('=');
                if (idxIgual < 0) continue; // "nombre" a secas: nada que hacer, ver comentario de arriba.

                string nombre = segmento.Substring(0, idxIgual).Trim();
                string derecha = segmento.Substring(idxIgual + 1).Trim();
                if (nombre.Length == 0 || derecha.Length == 0) continue;

                string? valorAAsignar = double.TryParse(derecha, NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                    ? derecha // "nombre=NUMERO": literal directo.
                    : ValorConocido(derecha); // "nombre=otroNombre": copia el valor YA conocido de otroNombre, si lo hay.

                if (valorAAsignar == null) continue; // "otroNombre" (p.ej. L1) todavía no se conoce: no hay nada que copiar todavía.

                var entry = ObtenerOCrearEstado(nombre);
                entry.Valor = valorAAsignar;
                entry.Historial.Add(new HistorialOpcion { Escandallo = escandalloLlamador, Valor = valorAAsignar, Origen = OrigenVariablePasada });
                yield return (nombre, valorAAsignar);
            }
        }

        /// <summary>Flags internos del propio DSL de escandallos (no existen en la tabla Opciones,
        /// ver EsOpcionPreguntable) para los que el usuario SÍ quiere que se siga preguntando -con
        /// una lista fija de valores posibles, ver SimulacionPage.MostrarPregunta- mientras no haya
        /// lógica que los resuelva solo ("por el momento debe seguir preguntando por las opciones
        /// que te especifiqué los valores concretos"). Cualquier otro flag interno que no esté en
        /// esta lista sigue sin preguntarse nunca (ver EsOpcionPreguntable).</summary>
        internal static readonly IReadOnlyCollection<string> OpcionesInternasPreguntables = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "Puerta", "Practicable", "Oscilobatiente", "Corredera", "Elevable",
            "CotaVariable", "Activa", "Asociada", "Exterior",
        };

        /// <summary>true si tiene sentido PARAR y preguntarle al usuario por esta opción: opciones
        /// reales del catálogo ROTO (Nombre empieza por "RO_", igual que el filtro real de la tabla
        /// Opciones: "NOMBRE LIKE 'RO\_%'" en ConfiguradorOpciones.cs), la excepción confirmada por
        /// el usuario "HardwareSupplier" (se gestiona aparte del catálogo Opciones/ContenidoOpciones
        /// pero sigue siendo una opción real y preguntable, con sus posibilidades reales), y los
        /// flags internos listados en OpcionesInternasPreguntables (Puerta, Practicable...: el
        /// usuario quiere seguir viéndolos preguntados con su lista fija de valores por ahora).
        /// Cualquier otro flag interno del DSL que no esté en ninguno de esos tres grupos NUNCA debe
        /// pausar la simulación: si no se conoce todavía, la condición que lo usa simplemente se
        /// evalúa como "no coincide" (ValorConocido sigue siendo null) sin generar ningún evento, y
        /// el recorrido continúa solo.</summary>
        private static bool EsOpcionPreguntable(string opcion) =>
            opcion.StartsWith("RO_", StringComparison.OrdinalIgnoreCase)
            || string.Equals(opcion, "HardwareSupplier", StringComparison.OrdinalIgnoreCase)
            || OpcionesInternasPreguntables.Contains(opcion);

        /// <summary>Recorre el árbol de la condición y produce un evento PreguntarOpcion por cada
        /// opción PREGUNTABLE (ver EsOpcionPreguntable) cuyo valor no se conoce todavía, respetando
        /// el cortocircuito de Y/O: si el operando izquierdo de un Y ya es falso (o el de un O ya es
        /// verdadero) con lo que se sabe hasta ahora, NO hace falta preguntar por el operando
        /// derecho -mismo orden de preguntas que el prototipo Python validado, con el añadido de
        /// EsOpcionPreguntable encima-. Tras agotar este IEnumerable, toda opción preguntable que la
        /// condición necesita ya tiene valor conocido (salvo que el consumidor no haya llamado a
        /// ResponderOpcion tras algún PreguntarOpcion), así que EvaluarCondicionPura(cond) ya puede
        /// calcular el resultado sin generar más eventos.</summary>
        private IEnumerable<EventoSimulacion> AsegurarValoresConocidos(NodoCond cond, string esc)
        {
            switch (cond)
            {
                case NodoCmp cmp:
                    if (ValorConocido(cmp.Opcion) == null && EsOpcionPreguntable(cmp.Opcion))
                        yield return new EventoSimulacion { Tipo = TipoEventoSimulacion.PreguntarOpcion, Escandallo = esc, Opcion = cmp.Opcion };
                    break;
                case NodoVarCmp varCmp:
                    // A diferencia de NodoCmp, aquí no hace falta ningún filtro tipo
                    // EsOpcionPreguntable: toda comparación de variable que aparece en un Programa
                    // real es, por definición, una variable que ese escandallo necesita conocer (no
                    // hay "variables internas no preguntables" como sí hay pseudo-opciones del DSL),
                    // así que si no se conoce todavía, se pregunta siempre.
                    if (ValorConocido(varCmp.Variable) == null)
                        yield return new EventoSimulacion { Tipo = TipoEventoSimulacion.PreguntarOpcion, Escandallo = esc, Opcion = varCmp.Variable, EsVariable = true };
                    break;
                case NodoAnd and_:
                    foreach (var ev in AsegurarValoresConocidos(and_.Izq, esc)) yield return ev;
                    if (!EvaluarCondicionPura(and_.Izq)) yield break; // cortocircuito: el resto no hace falta
                    foreach (var ev in AsegurarValoresConocidos(and_.Der, esc)) yield return ev;
                    break;
                case NodoOr or_:
                    foreach (var ev in AsegurarValoresConocidos(or_.Izq, esc)) yield return ev;
                    if (EvaluarCondicionPura(or_.Izq)) yield break; // cortocircuito
                    foreach (var ev in AsegurarValoresConocidos(or_.Der, esc)) yield return ev;
                    break;
            }
        }

        /// <summary>Evaluación pura (sin efectos, sin generar eventos) de una condición leyendo
        /// solo lo que ya hay en Estado. Debe llamarse siempre DESPUÉS de agotar
        /// AsegurarValoresConocidos(cond, esc) para ese mismo nodo, de modo que toda opción que la
        /// condición realmente necesita (según el cortocircuito) ya tenga valor.</summary>
        private bool EvaluarCondicionPura(NodoCond cond)
        {
            switch (cond)
            {
                case NodoCmp cmp:
                    // .Trim() en ambos lados: el que compara aquí es el único punto que garantiza
                    // la coincidencia pase lo que pase por donde haya entrado el valor en Estado
                    // (ver comentarios de PrecargarValorConocido/ResponderOpcion sobre columnas
                    // CHAR(n) rellenadas con espacios). cmp.Valor es el literal del propio Programa
                    // del escandallo: en teoría siempre limpio, pero se recorta también por
                    // simetría y para no depender de esa suposición.
                    bool coincide = string.Equals(
                        ValorConocido(cmp.Opcion)?.Trim(),
                        cmp.Valor?.Trim(),
                        StringComparison.Ordinal);
                    return cmp.Negado ? !coincide : coincide;
                case NodoVarCmp varCmp:
                {
                    string? conocido = ValorConocido(varCmp.Variable);
                    // Sin valor conocido (el usuario no llegó a responder, p.ej. porque canceló la
                    // simulación a medias) o con un valor no numérico (defensivo: no debería poder
                    // pasar, ver la validación en SimulacionPage.BtnResponderLibre_Click, pero mejor
                    // no reventar si algún día algo lo deja escapar): se trata como "no coincide",
                    // el mismo criterio que ya usa NodoCmp para una opción sin valor.
                    if (conocido == null || !double.TryParse(conocido, NumberStyles.Any, CultureInfo.InvariantCulture, out double valorActual))
                        return false;

                    const double Epsilon = 0.0001; // tolerancia para "=" entre doubles (cotas en mm, normalmente enteras).
                    return varCmp.Operador switch
                    {
                        OperadorCmp.Igual => Math.Abs(valorActual - varCmp.Valor) < Epsilon,
                        OperadorCmp.Menor => valorActual < varCmp.Valor,
                        OperadorCmp.Mayor => valorActual > varCmp.Valor,
                        OperadorCmp.MenorIgual => valorActual <= varCmp.Valor,
                        OperadorCmp.MayorIgual => valorActual >= varCmp.Valor,
                        _ => false
                    };
                }
                case NodoAnd and_:
                    return EvaluarCondicionPura(and_.Izq) && EvaluarCondicionPura(and_.Der);
                case NodoOr or_:
                    return EvaluarCondicionPura(or_.Izq) || EvaluarCondicionPura(or_.Der);
                default:
                    throw new InvalidOperationException("nodo de condición desconocido");
            }
        }
    }

    /// <summary>Carga de textos Programa desde la tabla Escandallos por Código, para el
    /// EscandalloInterpreter "de verdad" (el que usa la Simulación contra la BBDD real). Separado en
    /// su propia clase estática -en vez de meterlo dentro de EscandalloInterpreter- para poder seguir
    /// inyectando una función de carga distinta en pruebas, igual que hacía el prototipo Python.</summary>
    public static class EscandalloProgramaLoader
    {
        /// <summary>Devuelve el texto Programa del Escandallo con este Código, o null si no existe
        /// (mismo contrato que "cargar_programa" en el prototipo Python: None = el recorrido debe
        /// producir un evento EscandalloNoEncontrado, no lanzar una excepción).</summary>
        public static string? CargarProgramaPorCodigo(string codigo)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            using var cmd = new SqlCommand("SELECT Programa FROM Escandallos WHERE Codigo = @codigo", conexion);
            cmd.Parameters.AddWithValue("@codigo", codigo);
            conexion.Open();
            var resultado = cmd.ExecuteScalar();
            return (resultado == null || resultado == DBNull.Value) ? null : resultado.ToString();
        }
    }
}
