using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Microsoft.Win32;
using RotoEntities;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using static RotoTools.Enums;

namespace RotoTools
{
    public static class Helpers
    {
        #region public methods

        public static string GetServer()
        {
            string baseKey = @"HKEY_CURRENT_USER\SOFTWARE\Preference\OLEDB";
            return Registry.GetValue(baseKey, "Server", null) as string;
        }
        public static string GetDataBase()
        {
            string baseKey = @"HKEY_CURRENT_USER\SOFTWARE\Preference\OLEDB";
            return Registry.GetValue(baseKey, "Database", null) as string;
        }
        public static string GetConnectionString()
        {
            try
            {
                string baseKey = @"HKEY_CURRENT_USER\SOFTWARE\Preference\OLEDB";

                string database = Registry.GetValue(baseKey, "Database", null) as string;
                string server = Registry.GetValue(baseKey, "Server", null) as string;
                string user = Registry.GetValue(baseKey, "User", null) as string;
                string mru0 = Registry.GetValue(baseKey, "MRU0", null) as string;
                object trustedObj = Registry.GetValue(baseKey, "TrustedConnection", null);

                if (string.IsNullOrEmpty(database) || string.IsNullOrEmpty(server) || string.IsNullOrEmpty(mru0))
                    throw new Exception("Faltan valores en el registro para construir la cadena de conexión.");

                // Extraer contraseña del valor MRU0
                string password = ExtraerValorRegistro(mru0, "PWD=");
                bool trustedConnection = (trustedObj is int tc && tc == 1);

                if (trustedConnection)
                {
                    return $"Data Source={server};Initial Catalog={database};Integrated Security=True;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True";
                }
                else
                {
                    if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                        throw new Exception("Usuario o contraseña no definidos correctamente.");

                    return $"Data Source={server};Initial Catalog={database};User ID={user};Password={password};MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True";
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static bool IsVersionPrefSuiteCompatible()
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Top 1 Version FROM PrefDBManagerHistory ORDER BY ExecutionDate desc", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string[] versionChar = reader[0].ToString().Split('.');
                if (versionChar != null && versionChar.Length > 0)
                {
                    if (Convert.ToInt32(versionChar[0]) >= 20)
                        return true;
                }
            }
            return false;
        }
        public static int CalcularFlags(ContenidoOpcion c)
        {
            if (c.OcultaEnLista && c.OcultaEnArbol) return 3;
            if (c.OcultaEnLista && !c.OcultaEnArbol) return 1;
            if (!c.OcultaEnLista && c.OcultaEnArbol) return 2;
            return 0;
        }
        public static void InsertOpcion(string opcionName)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"INSERT INTO Opciones(Nombre, Nivel1, GenerateVariable, Flags) VALUES(@nombre, 'ROTO', 0, 0)", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void DeleteOpcion(string optionName)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("DELETE Opciones WHERE Nombre=@nombre", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", optionName);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void InsertPrefOpenOption(string supplierCode, string optionName, string optionValue)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"INSERT INTO [Open].Options (SupplierCode, [Option], [Value]) VALUES(@suppliercode, @optionname, @optionvalue)", conn))
            {
                cmd.Parameters.AddWithValue("@suppliercode", supplierCode);
                cmd.Parameters.AddWithValue("@optionname", optionName);
                cmd.Parameters.AddWithValue("@optionvalue", optionValue);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static List<ContenidoOpcion> GetContenidoOpciones(string opcionName)
        {
            var lista = new List<ContenidoOpcion>();
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT Valor, Texto, Flags, Orden, Invalid, DesAuto FROM ContenidoOpciones WHERE Opcion=@nombre", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        lista.Add(new ContenidoOpcion
                        {
                            Valor = rdr[0].ToString(),
                            Texto = rdr[1].ToString(),
                            Flags = String.IsNullOrEmpty(rdr[2].ToString()) ? 0 : rdr.GetInt32(2),
                            Orden = rdr.GetInt16(3),
                            Invalid = rdr.GetInt16(4),
                            DesAuto = rdr[5].ToString()
                        });
                    }
                }
            }
            return lista;
        }
        public static void InsertContenidoOpcion(string opcionName, ContenidoOpcion cont)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("INSERT INTO ContenidoOpciones (Opcion, Valor, Texto, Flags, Orden, DesAuto, Invalid) VALUES (@nombre, @valor, @texto, @flags, @orden, @desauto, @invalid)", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                cmd.Parameters.AddWithValue("@valor", cont.Valor);
                cmd.Parameters.AddWithValue("@texto", cont.Texto);
                cmd.Parameters.AddWithValue("@flags", cont.Flags);
                cmd.Parameters.AddWithValue("@orden", cont.Orden);
                cmd.Parameters.AddWithValue("@invalid", cont.Invalid);
                cmd.Parameters.AddWithValue("@desauto", cont.DesAuto);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void UpdateContenidoOpcion(string opcionName, ContenidoOpcion cont)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("UPDATE ContenidoOpciones SET Texto=@texto, Flags=@flags, Orden=@orden, Invalid=@invalid, DesAuto=@desauto WHERE Opcion=@nombre AND Valor=@valor", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                cmd.Parameters.AddWithValue("@valor", cont.Valor);
                cmd.Parameters.AddWithValue("@texto", cont.Texto);
                cmd.Parameters.AddWithValue("@flags", cont.Flags);
                cmd.Parameters.AddWithValue("@orden", cont.Orden);
                cmd.Parameters.AddWithValue("@invalid", cont.Invalid);
                cmd.Parameters.AddWithValue("@desauto", cont.DesAuto);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void DeleteContenidoOpcion(string opcionName, string valor)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("DELETE FROM ContenidoOpciones WHERE Opcion=@nombre AND Valor=@valor", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                cmd.Parameters.AddWithValue("@valor", valor);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void DeleteAllContenidoOpciones(string opcionName)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("DELETE FROM ContenidoOpciones WHERE Opcion=@nombre", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static int UpdateGruposYProveedor(string IdGrupoPresupuestado, string IdGrupoProduccion, string IdProveedor)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("UPDATE MaterialesBase SET IdGrupoPresupuestado=@grupopresupuestado, IdGrupoProduccion=@grupoproduccion, CodigoProveedor=@codigoproveedor WHERE Nivel1 = 'ROTO NX' OR Nivel1 = 'ROTO NX ALU' OR Nivel1 = 'ROTO NX PAX'", conn))
            {
                cmd.Parameters.AddWithValue("@grupopresupuestado", IdGrupoPresupuestado);
                cmd.Parameters.AddWithValue("@grupoproduccion", IdGrupoProduccion);
                cmd.Parameters.AddWithValue("@codigoproveedor", IdProveedor);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
        public static int UpdateMaterialesBaseFicticiosPropiedades(string[] articulos)
        {
            int rowsAfected = 0;
            using (var conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();

                foreach (var articulo in articulos)
                {
                    string sql = @"
                                UPDATE MaterialesBase 
                                SET NoIncluirEnHojaDeTrabajo=1,
                                    NoOptimizar=1,
                                    NoIncluirEnMaterialNeeds=1,
                                    DoNotShowInMonitors=1,
                                    DontIncludeInMaterialReport=1
                                WHERE ReferenciaBase LIKE @articulo";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@articulo", articulo);
                        rowsAfected += cmd.ExecuteNonQuery();
                    }
                }
            }
            return rowsAfected;
        }
        public static void RestoreOpcionesDesdeXml(string rutaXml)
        {
            try
            {
                if (!System.IO.File.Exists(rutaXml))
                {
                    MessageBox.Show(LocalizationManager.GetString("L_FicheroConfigNoEncontrado"), "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                XDocument doc = XDocument.Load(rutaXml);

                var opcionesXml = doc.Descendants("Opcion")
                    .Select(opElem => new Opcion
                    {
                        Name = (string)opElem.Attribute("nombre"),
                        Nivel1 = (string)opElem.Attribute("nivel1"),
                        Nivel2 = (string)opElem.Attribute("nivel2"),
                        Nivel3 = (string)opElem.Attribute("nivel3"),
                        Nivel4 = (string)opElem.Attribute("nivel4"),
                        Nivel5 = (string)opElem.Attribute("nivel5"),
                        Flags = (int?)opElem.Attribute("flags") ?? 0,
                        ContenidoOpcionesList = opElem.Elements("ContenidoOpcion")
                            .Select(c => new ContenidoOpcion
                            {
                                Valor = (string)c.Attribute("valor"),
                                Texto = (string)c.Attribute("texto"),
                                Flags = (int?)c.Attribute("flags") ?? 0,
                                Orden = (int?)c.Attribute("orden") ?? 0,
                                Invalid = (int?)c.Attribute("invalid") ?? 0,
                                DesAuto = (string)c.Attribute("desauto")
                            }).ToList()
                    }).ToList();

                foreach (var opcionXml in opcionesXml)
                {
                    var contenidosDb = Helpers.GetContenidoOpciones(opcionXml.Name);

                    // ¿Falta alguno del XML en BD?
                    bool faltaAlguno = opcionXml.ContenidoOpcionesList
                        .Any(cXml => !contenidosDb.Any(cDb => cDb.Valor.ToUpper().Trim() == cXml.Valor.ToUpper().Trim()));

                    if (faltaAlguno || contenidosDb.Count != opcionXml.ContenidoOpcionesList.Count)
                    {
                        // 1) Copia exacta del XML
                        Helpers.DeleteAllContenidoOpciones(opcionXml.Name);

                        foreach (var contXml in opcionXml.ContenidoOpcionesList.OrderBy(co => co.Orden))
                        {
                            Helpers.InsertContenidoOpcion(opcionXml.Name, contXml);
                        }
                    }
                    else
                    {
                        // 2) Todos están → solo actualizamos Texto y Flags
                        foreach (var contXml in opcionXml.ContenidoOpcionesList)
                        {
                            var contDb = contenidosDb.First(c => c.Valor.ToUpper().Trim() == contXml.Valor.ToUpper().Trim());

                            if (contDb.Texto.ToUpper().Trim() != contXml.Texto.ToUpper().Trim() || contDb.Flags != contXml.Flags)
                            {
                                Helpers.UpdateContenidoOpcion(opcionXml.Name, contXml);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (19)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static List<Escandallo> CargarEscandallosEmbebidos(List<enumRotoTipoEscandallo> tiposSeleccionados)
        {
            List<Escandallo> escandallosList = new();
            var assembly = Assembly.GetExecutingAssembly();

            string resourcePrefix = "RotoTools.Resources.Escandallos."; // Ajusta según tu namespace

            var escandallosEmbebidos = assembly.GetManifestResourceNames()
                                              .Where(r => r.StartsWith(resourcePrefix) && r.EndsWith(".json"))
                                              .ToList();

            foreach (string recurso in escandallosEmbebidos)
            {
                using var stream = assembly.GetManifestResourceStream(recurso);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                var escandallo = JsonSerializer.Deserialize<Escandallo>(json);
                if (escandallo != null)
                {
                    // Asegura que tiene su tipo asignado (por si no lo trae en el JSON)
                    InicializarEscandalloRotoTipo(escandallo);

                    // Filtra según los checks seleccionados
                    if (tiposSeleccionados.Contains(escandallo.RotoTipo))
                        escandallosList.Add(escandallo);

                }
            }

            return escandallosList;
        }
        public static void InicializarEscandalloRotoTipo(Escandallo escandallo)
        {
            string codigo = escandallo.Codigo?.ToUpper() ?? "";

            if (codigo.Contains("PVC"))
                escandallo.RotoTipo = enumRotoTipoEscandallo.PVC;
            else if (codigo.Contains("ALU"))
                escandallo.RotoTipo = enumRotoTipoEscandallo.Aluminio;
            else if (codigo.Contains("MANILLAS") || codigo.Contains("MANILLA"))
                escandallo.RotoTipo = enumRotoTipoEscandallo.GestionManillas;
            else if (codigo.Contains("BOMBILLOS"))
                escandallo.RotoTipo = enumRotoTipoEscandallo.GestionBombillos;
            else if (codigo.StartsWith("RO_HERRAJE", StringComparison.OrdinalIgnoreCase) || codigo.StartsWith("RO_Herr.", StringComparison.OrdinalIgnoreCase))
                escandallo.RotoTipo = enumRotoTipoEscandallo.PersonalizacionClientes;
            else
                escandallo.RotoTipo = enumRotoTipoEscandallo.GestionGeneral;
        }
        public static bool ExisteEscandalloEnBD(string escandalloCodigo)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM Escandallos WHERE Codigo = '" + escandalloCodigo + "'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static void InstalarOpcionConfiguraciónStandard()
        {
            if (!ExisteOpcionEnBD("01 Configuracion Estandar"))
            {
                CrearOpcionConfiguracionStandard();
                CrearContenidoOpcionesConfiguracionStandard();
            }
        }
        public static void InstalarOpcionTipoCorredera()
        {
            if (!ExisteOpcionEnBD("RO_TIPO_CORREDERA"))
            {
                CrearOpcionTipoCorredera();
                CrearContenidoOpcionesTipoCorredera();
            }
        }
        public static bool ExisteOpcionEnBD(string optionName)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM Opciones WHERE Nombre = '{optionName}'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static bool ExistePrefOpenOpcionEnBD(string supplierCode, string optionName, string optionValue)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM [Open].Options WHERE SupplierCode = '{supplierCode}' AND [Option] = '{optionName}' AND Value = '{optionValue}'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static void CrearContenidoOpcionesConfiguracionStandard()
        {
            try
            {
                List<string> contenidoOpcionesConfiguracionStandard =
                [
                    "Si",
                    "No"
                ];
                int orden = 0;
                foreach (string contenidoOpcionValor in contenidoOpcionesConfiguracionStandard)
                {
                    ContenidoOpcion contenidoOpcion = new ContenidoOpcion("01 Configuracion Estandar", contenidoOpcionValor, "", "0", orden.ToString(), "0", "");
                    InsertContenidoOpcion("01 Configuracion Estandar", contenidoOpcion);
                    orden++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (20)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void CrearContenidoOpcionesTipoCorredera()
        {
            try
            {
                List<string> contenidoOpcionesConfiguracionStandard =
                [
                    "ISlide",
                    "Inowa"
                ];
                int orden = 0;
                foreach (string contenidoOpcionValor in contenidoOpcionesConfiguracionStandard)
                {
                    ContenidoOpcion contenidoOpcion = new ContenidoOpcion("RO_TIPO_CORREDERA", contenidoOpcionValor, "", "0", orden.ToString(), "0", "");
                    InsertContenidoOpcion("RO_TIPO_CORREDERA", contenidoOpcion);
                    orden++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (22)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void CrearOpcionConfiguracionStandard()
        {
            try
            {
                InsertOpcion("01 Configuracion Estandar");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (21)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void CrearOpcionTipoCorredera()
        {
            try
            {
                InsertOpcion("RO_TIPO_CORREDERA");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (23)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static string GetPrefOpenOperationId(string operationName, string generatorReference, string operationX)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"SELECT Id FROM [Open].Operations WHERE Name = @name AND GeneratorReference = @generatorreference AND X = @operationX AND Id NOT IN (SELECT OperationId FROM [Open].OperationsOptions)", conn))
            {
                cmd.Parameters.AddWithValue("@name", operationName);
                cmd.Parameters.AddWithValue("@generatorreference", generatorReference);
                cmd.Parameters.AddWithValue("@operationX", operationX);

                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        return rdr[0].ToString();
                    }
                }
            }
            return String.Empty;
        }
        public static bool OpcionAsociadaAOperacionPrefOpen(string operationId, string optionName, string optionValue, string supplierCode)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM [Open].OperationsOptions WHERE OperationId = '{operationId}' AND SupplierCode = '{supplierCode}' AND [Option] = '{optionName}' AND Value = '{optionValue}'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static void DeletePrefOpenOperationsOptions(string optionName, string supplierCode)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("DELETE [Open].OperationsOptions WHERE [Option]=@nombre AND SupplierCode=@suppliercode", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", optionName);
                cmd.Parameters.AddWithValue("@suppliercode", supplierCode);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void DeletePrefOpenOperationsPlaca(int configuracionEliminar, string supplierCode)
        {
            string operationXCondition = "";
            switch (configuracionEliminar)
            {
                case (int)enumConfiguracionManillasFKS.Normalizada:
                    operationXCondition = "(X = 'HP+70' OR X = 'HP-130')";
                    break;
                case (int)enumConfiguracionManillasFKS.SoloFks:
                    operationXCondition = "(X = 'HP+78' OR X = 'HP-138')";
                    break;
            }
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("DELETE [Open].Operations WHERE SupplierCode=@suppliercode AND Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND " + operationXCondition, conn))
            {
                cmd.Parameters.AddWithValue("@suppliercode", supplierCode);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void DeletePrefOpenOptions(string optionName, string supplierCode)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("DELETE [Open].Options WHERE [Option]=@nombre AND SupplierCode=@suppliercode", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", optionName);
                cmd.Parameters.AddWithValue("@suppliercode", supplierCode);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static Traducciones CargarTraducciones(string excelPath)
        {
            var traducciones = new Traducciones();

            using (var wb = new XLWorkbook(excelPath))
            {
                foreach (var ws in wb.Worksheets)
                {
                    string nombreHoja = ws.Name.Trim().ToLower();

                    if (nombreHoja.StartsWith("fittings"))
                    {
                        foreach (var row in ws.RowsUsed().Skip(1))
                        {
                            string refId = row.Cell(1).GetString().Trim();
                            string trad = row.Cell(3).GetString().Trim();
                            if (!string.IsNullOrEmpty(refId) && !string.IsNullOrEmpty(trad))
                                traducciones.Fittings[refId] = trad;
                        }
                    }
                    else if (nombreHoja.StartsWith("fittinggroups"))
                    {
                        foreach (var row in ws.RowsUsed().Skip(1))
                        {
                            string desc = row.Cell(1).GetString().Trim();
                            string trad = row.Cell(2).GetString().Trim();
                            if (!string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(trad))
                                traducciones.FittingGroups[desc] = trad;
                        }
                    }
                    else if (nombreHoja.StartsWith("colours"))
                    {
                        foreach (var row in ws.RowsUsed().Skip(1))
                        {
                            string desc = row.Cell(1).GetString().Trim();
                            string trad = row.Cell(2).GetString().Trim();
                            if (!string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(trad))
                                traducciones.Colours[desc] = trad;
                        }
                    }
                    else if (nombreHoja.StartsWith("options"))
                    {
                        foreach (var row in ws.RowsUsed().Skip(1))
                        {
                            string name = row.Cell(1).GetString().Trim();
                            string value = row.Cell(2).GetString().Trim();
                            string trad = row.Cell(3).GetString().Trim();

                            if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(value))
                                traducciones.OptionNames[name] = trad;
                            else if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                                traducciones.OptionValues[(name, value)] = trad;
                        }
                    }
                }
            }

            return traducciones;
        }
        public static int EjecutarNonQuery(string sql)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }
        public static int EjecutarScalarCount(string sql)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public static int TryParseInt(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }
        public static double TryParseDouble(string value)
        {
            return double.TryParse(value, out double result) ? result : 0;
        }
        public static bool TryParseBool(string value)
        {
            return bool.TryParse(value, out bool result) ? result : false;
        }
        public static T DeserializarXML<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xml);
            return (T)serializer.Deserialize(reader);
        }
        public static string SerializarXml<T>(T objeto)
        {
            using (var stringWriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stringWriter, objeto);
                return stringWriter.ToString();
            }
        }
        #endregion

        #region private methos

        private static string ExtraerValorRegistro(string fuente, string clave)
        {
            if (string.IsNullOrEmpty(fuente) || string.IsNullOrEmpty(clave))
                return null;

            int start = fuente.IndexOf(clave, StringComparison.OrdinalIgnoreCase);
            if (start == -1) return null;

            start += clave.Length;
            int end = fuente.IndexOf(";", start);
            if (end == -1) end = fuente.Length;

            return fuente.Substring(start, end - start).Trim();

        }

        #endregion
    }
    public static class OpcionHelper
    {
        public static Option Crear(string name, string value)
        {
            // Si no hay traducción activa, devolver tal cual
            if (!TranslateManager.AplicarTraduccion || TranslateManager.TraduccionesActuales == null)
                return new Option(name, value);

            // Traducir nombre y valor si existen en el diccionario
            string nombreTraducido = TranslateManager.TraduccionesActuales.TraducirOptionName(name);
            string valorTraducido = TranslateManager.TraduccionesActuales.TraducirOptionValue(name, value);

            return new Option("RO_" + nombreTraducido, valorTraducido);
        }
    }
    public static class EscandalloHelper
    {
        public static void AplicarTraduccion(Escandallo escandallo)
        {
            if (!TranslateManager.AplicarTraduccion || TranslateManager.TraduccionesActuales == null)
                return;

            var trad = TranslateManager.TraduccionesActuales;

            // Si hay JSONs embebidos (Variables o Programa) con opciones dentro, traducirlos también
            if (!string.IsNullOrEmpty(escandallo.Programa))
            {
                escandallo.Programa = TraducirOpcionesDentroJson(escandallo.Programa, trad);
            }
        }
        private static string TraducirOpcionesDentroJson(string programa, Traducciones trad)
        {
            if (string.IsNullOrWhiteSpace(programa))
                return programa;

            try
            {
                // Traducimos directamente como script o texto
                return TraducirTextoPrograma(programa, trad);
               
            }
            catch (JsonException)
            {
                // Si por algún motivo no se puede parsear, devolvemos el texto sin modificar
                return programa;
            }
        }
        private static string TraducirTextoPrograma(string programa, Traducciones trad)
        {
            if (string.IsNullOrWhiteSpace(programa))
                return programa;

            // Detecta expresiones del tipo: OPCION("Nombre", "Valor")
            var regex = new Regex(@"OPCION\(\""(?<nombre>[^\""]+)\"",\""(?<valor>[^\""]+)\""\)", RegexOptions.Compiled);

            return regex.Replace(programa, match =>
            {
                string nombreOriginal = match.Groups["nombre"].Value;
                string valorOriginal = match.Groups["valor"].Value;

                //Eliminar prefijo "RO_" para buscar traducción
                string nombreSinPrefijo = nombreOriginal.StartsWith("RO_", StringComparison.OrdinalIgnoreCase)
                    ? nombreOriginal.Substring(3)
                    : nombreOriginal;

                //Traducir nombre y valor
                string nombreTrad = trad.TraducirOptionName(nombreSinPrefijo);
                string valorTrad = trad.TraducirOptionValue(nombreSinPrefijo, valorOriginal);

                //Restaurar el prefijo "RO_" si estaba originalmente
                if (nombreOriginal.StartsWith("RO_", StringComparison.OrdinalIgnoreCase) &&
                    !nombreTrad.StartsWith("RO_", StringComparison.OrdinalIgnoreCase))
                {
                    nombreTrad = "RO_" + nombreTrad;
                }

                return $"OPCION(\"{nombreTrad}\",\"{valorTrad}\")";
            });
        }
    }
}
