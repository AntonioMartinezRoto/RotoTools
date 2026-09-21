using Microsoft.Data.SqlClient;

namespace RotoTools.Suite.Services
{
    /// <summary>
    /// Nueva (no existía en el original ni en ningún otro módulo de la Suite): lecturas de solo
    /// lectura sobre la tabla ContenidoPAFBlob para el asistente de "Simulación" — Paso 1, modo
    /// "Presupuesto" (petición del usuario: poder elegir un modelo guardado en un presupuesto, no
    /// solo uno ya guardado como Dibujo en BBDD, "El usuario podría elegir un numero de presupuesto
    /// de un desplegable... Al cargar el numero se carga en otro desplegable las versiones
    /// disponibles... y luego otro desplegable que cargue los numeros de linea"). Mismo criterio de
    /// solo lectura que SimulacionDatosService/DibujoImagenService: esta clase NUNCA escribe nada
    /// en BBDD — la Simulación completa (también en modo Presupuesto) es puramente exploratoria.
    ///
    /// ContenidoPAFBlob (según la descripción del propio usuario, no confirmada todavía contra el
    /// esquema real de la base de datos): Numero/Version/Orden identifican un presupuesto/versión/
    /// línea concretos (los tres desplegables en cascada del Paso 1), Buffer contiene el mismo XML
    /// comprimido con las funciones del esquema [zlib] que Dibujos.Buffer (ver
    /// DibujoOpcionesRotoService.LeerXmlDescomprimido, cuyo patrón EXACTO se reutiliza aquí en
    /// LeerXmlDescomprimido más abajo — misma función fija [zlib].[UnzipBLOB], mismo reintento vía
    /// EjecutarConReintentoZlib, solo cambian la tabla y el WHERE), y Metafile contiene la vista
    /// previa vectorial igual que Dibujos.Metafile (ver
    /// DibujoImagenService.CargarVistaPreviaPresupuesto).
    ///
    /// IMPORTANTE (pendiente de confirmar con el primer uso real contra la base de datos): se
    /// tratan Numero/Version/Orden como texto de principio a fin — cada ComboBox del Paso 1 muestra
    /// cadenas y el parámetro SQL se manda vía AddWithValue como texto, dejando que SQL Server haga
    /// la conversión implícita si en realidad son columnas numéricas — porque el usuario no dio el
    /// tipo SQL exacto de estas tres columnas ("Tabla: ContenidoPAFBlob; Campo: Numero", sin más
    /// detalle). Si esto no fuera correcto (p.ej. una conversión implícita ambigua, o un tipo que
    /// no admite CAST a NVARCHAR sin más), el primer intento real lo dirá con un error de SQL claro
    /// — mismo criterio iterativo ya aplicado con éxito varias veces en el resto de este proyecto:
    /// mejor entregar algo funcional ya y corregir con el error real que bloquear la funcionalidad
    /// entera por una suposición que no se puede confirmar desde aquí (sin acceso a la base de
    /// datos real).
    /// </summary>
    public static class PresupuestoDatosService
    {
        /// <summary>Números de presupuesto disponibles (primer desplegable del Paso 1, modo
        /// Presupuesto). CAST a NVARCHAR: mismo motivo que el resto de esta clase (tipo real de la
        /// columna no confirmado). ORDER BY sobre el propio texto ya convertido — si Numero fuera en
        /// realidad numérico, un ORDER BY alfabético de texto podría no coincidir con el orden
        /// numérico esperado (p.ej. "10" antes que "2"); puramente cosmético, no afecta a qué
        /// opciones aparecen.</summary>
        public static List<string> ObtenerNumerosPresupuesto()
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand(
                "SELECT DISTINCT CAST(Numero AS NVARCHAR(50)) FROM ContenidoPAFBlob WHERE Numero IS NOT NULL ORDER BY 1", conexion);
            return LeerListaTexto(cmd);
        }

        /// <summary>Versiones disponibles para un Número ya elegido (segundo desplegable).</summary>
        public static List<string> ObtenerVersionesPresupuesto(string numero)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand(
                "SELECT DISTINCT CAST(Version AS NVARCHAR(50)) FROM ContenidoPAFBlob WHERE Numero=@numero AND Version IS NOT NULL ORDER BY 1", conexion);
            cmd.Parameters.AddWithValue("@numero", numero);
            return LeerListaTexto(cmd);
        }

        /// <summary>Números de línea (Orden) disponibles para un Número+Versión ya elegidos (tercer
        /// desplegable). Al elegir uno de estos, el Paso 1 ya puede previsualizar el Metafile y,
        /// al continuar, leer las hojas del Buffer (ver DibujoImagenService.
        /// CargarVistaPreviaPresupuesto / SimulacionDatosService.ObtenerHojasDesdePresupuesto).</summary>
        public static List<string> ObtenerOrdenesPresupuesto(string numero, string version)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand(
                "SELECT DISTINCT CAST(Orden AS NVARCHAR(50)) FROM ContenidoPAFBlob WHERE Numero=@numero AND Version=@version AND Orden IS NOT NULL ORDER BY 1", conexion);
            cmd.Parameters.AddWithValue("@numero", numero);
            cmd.Parameters.AddWithValue("@version", version);
            return LeerListaTexto(cmd);
        }

        private static List<string> LeerListaTexto(SqlCommand cmd)
        {
            var resultado = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0)) resultado.Add(reader.GetString(0));
            }
            return resultado;
        }

        /// <summary>Mismo patrón EXACTO que DibujoOpcionesRotoService.LeerXmlDescomprimido (mismo
        /// CAST explícito a NVARCHAR(MAX), misma función fija [zlib].[UnzipBLOB] — no hace falta
        /// resolverla dinámicamente como ResolverFuncionComprimir, eso es solo para el sentido de
        /// escritura y aquí no se escribe nada —, mismo reintento vía EjecutarConReintentoZlib,
        /// reutilizado tal cual por ser "internal" y vivir en el mismo ensamblado/namespace), pero
        /// contra ContenidoPAFBlob.Buffer filtrado por Numero+Version+Orden en vez de
        /// Dibujos.Buffer filtrado por Codigo. Usado por
        /// SimulacionDatosService.ObtenerHojasDesdePresupuesto (internal: solo para consumo desde
        /// otro Service de este mismo ensamblado, igual que su equivalente de Dibujos).</summary>
        internal static string LeerXmlDescomprimido(SqlConnection conexion, string numero, string version, string orden)
        {
            using var cmd = new SqlCommand(
                "SELECT CAST([zlib].[UnzipBLOB](Buffer) AS NVARCHAR(MAX)) FROM ContenidoPAFBlob WHERE Numero=@numero AND Version=@version AND Orden=@orden",
                conexion);
            cmd.Parameters.AddWithValue("@numero", numero);
            cmd.Parameters.AddWithValue("@version", version);
            cmd.Parameters.AddWithValue("@orden", orden);

            object? resultado = DibujoOpcionesRotoService.EjecutarConReintentoZlib(() => cmd.ExecuteScalar());
            if (resultado == null || resultado == DBNull.Value)
                throw new InvalidOperationException(
                    $"No se ha encontrado el presupuesto Número '{numero}', Versión '{version}', Orden '{orden}', o su Buffer está vacío.");

            return resultado is byte[] bytes ? DibujoOpcionesRotoService.DecodificarTexto(bytes) : resultado.ToString() ?? "";
        }
    }
}
