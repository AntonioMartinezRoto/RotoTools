using Azure;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Microsoft.Win32;
using RotoEntities;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using static RotoTools.Enums;

namespace RotoTools
{
    public static class Helpers
    {
        #region public methods

        #region Utilidades BBDD
        public static string? GetServer()
        {
            string baseKey = @"HKEY_CURRENT_USER\SOFTWARE\Preference\OLEDB";
            return Registry.GetValue(baseKey, "Server", null) as string;
        }
        public static string? GetDataBase()
        {
            string baseKey = @"HKEY_CURRENT_USER\SOFTWARE\Preference\OLEDB";
            return Registry.GetValue(baseKey, "Database", null) as string;
        }
        public static string? GetConnectionString()
        {
            try
            {
                string baseKey = @"HKEY_CURRENT_USER\SOFTWARE\Preference\OLEDB";

                string? database = Registry.GetValue(baseKey, "Database", null) as string;
                string? server = Registry.GetValue(baseKey, "Server", null) as string;
                string? user = Registry.GetValue(baseKey, "User", null) as string;
                string? mru0 = Registry.GetValue(baseKey, "MRU0", null) as string;
                object? trustedObj = Registry.GetValue(baseKey, "TrustedConnection", null);

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
            catch
            {
                return null;
            }
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

        #endregion

        #region Utilidades Opciones
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
            using (var cmd = new SqlCommand("UPDATE ContenidoOpciones SET Texto=@texto, Flags=@flags, Invalid=@invalid, DesAuto=@desauto WHERE Opcion=@nombre AND Valor=@valor", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                cmd.Parameters.AddWithValue("@valor", cont.Valor);
                cmd.Parameters.AddWithValue("@texto", cont.Texto);
                cmd.Parameters.AddWithValue("@flags", cont.Flags);
                //cmd.Parameters.AddWithValue("@orden", cont.Orden);
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
                    List<ContenidoOpcion> contenidoOpcionDbList = Helpers.GetContenidoOpciones(opcionXml.Name);

                    // Normalizamos comparaciones
                    Func<string, string> norm = v => v?.Trim().ToUpper();

                    // 1) Detectar los que faltan en BD
                    var contenidosFaltantes = opcionXml.ContenidoOpcionesList
                        .Where(cXml => !contenidoOpcionDbList
                            .Any(cDb => norm(cDb.Valor) == norm(cXml.Valor)))
                        .ToList();

                    // 2) Actualizar los existentes (Texto / Flags)
                    foreach (var contXml in opcionXml.ContenidoOpcionesList)
                    {
                        var contDb = contenidoOpcionDbList.FirstOrDefault(cDb => norm(cDb.Valor) == norm(contXml.Valor));

                        if (contDb != null)
                        {
                            if (norm(contDb.Texto.Trim()) != norm(contXml.Texto) ||
                                contDb.Flags != contXml.Flags)
                            {
                                Helpers.UpdateContenidoOpcion(opcionXml.Name, contXml);
                            }
                        }
                    }

                    // 3) Insertar solo los que faltan
                    if (contenidosFaltantes.Any())
                    {
                        int maxOrdenActual = contenidoOpcionDbList.Any()
                            ? contenidoOpcionDbList.Max(c => c.Orden)
                            : 0;

                        int orden = maxOrdenActual + 1;

                        foreach (var contXml in contenidosFaltantes)
                        {
                            contXml.Orden = orden++;
                            Helpers.InsertContenidoOpcion(opcionXml.Name, contXml);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (19)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
        public static void InstalarOpcionTipoLWC()
        {
            if (!ExisteOpcionEnBD("RO_TIPO_VENTANA_STD"))
            {
                CrearOpcionTipoLWC();
                CrearContenidoOpcionesTipoLWC();
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
        public static void CrearContenidoOpcionesTipoLWC()
        {
            try
            {
                List<string> contenidoOpcionesTipoLwc =
                [
                    "STD",
                    "LWC"
                ];
                int orden = 0;
                foreach (string contenidoOpcionValor in contenidoOpcionesTipoLwc)
                {
                    ContenidoOpcion contenidoOpcion = new ContenidoOpcion("RO_TIPO_VENTANA_STD", contenidoOpcionValor, "", "0", orden.ToString(), "0", "");
                    InsertContenidoOpcion("RO_TIPO_VENTANA_STD", contenidoOpcion);
                    orden++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (26)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        public static void CrearOpcionTipoLWC()
        {
            try
            {
                InsertOpcion("RO_TIPO_VENTANA_STD");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (28)" + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Utilidades Operaciones
        public static List<MechanizedOperation> CargarMacrosMechanizedOperationsEmbebidos()
        {
            List<MechanizedOperation> macrosMechanizedOperationsList = new();
            var assembly = Assembly.GetExecutingAssembly();

            string resourcePrefix = "RotoTools.Resources.Operaciones.Macros.MechanizedOperations.";

            var macrosMechanizedOperationsEmbebidos = assembly.GetManifestResourceNames()
                                              .Where(r => r.StartsWith(resourcePrefix) && r.EndsWith(".json"))
                                              .ToList();

            foreach (string recurso in macrosMechanizedOperationsEmbebidos)
            {
                using var stream = assembly.GetManifestResourceStream(recurso);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                var mechanizedOperation = JsonSerializer.Deserialize<MechanizedOperation>(json);
                if (mechanizedOperation != null)
                {
                    macrosMechanizedOperationsList.Add(mechanizedOperation);
                }
            }

            return macrosMechanizedOperationsList;
        }
        public static List<OperationsShapes> CargarMacrosOperationsShapesEmbebidos()
        {
            List<OperationsShapes> macrosOperationsShapesList = new();
            var assembly = Assembly.GetExecutingAssembly();

            string resourcePrefix = "RotoTools.Resources.Operaciones.Macros.OperationsShapes.";

            var macrosOperationsShapesEmbebidos = assembly.GetManifestResourceNames()
                                              .Where(r => r.StartsWith(resourcePrefix) && r.EndsWith(".json"))
                                              .ToList();

            foreach (string recurso in macrosOperationsShapesEmbebidos)
            {
                using var stream = assembly.GetManifestResourceStream(recurso);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                var operationsShapes = JsonSerializer.Deserialize<OperationsShapes>(json);
                if (operationsShapes != null)
                {
                    macrosOperationsShapesList.Add(operationsShapes);
                }
            }

            return macrosOperationsShapesList;
        }
        public static List<MechanizedConditions> CargarMechanizedConditionsEmbebidos()
        {
            List<MechanizedConditions> mechanizedConditionsList = new();
            var assembly = Assembly.GetExecutingAssembly();

            string resourcePrefix = "RotoTools.Resources.Operaciones.MechanizedConditions.";

            var mechanizedConditionsEmbebidos = assembly.GetManifestResourceNames()
                                              .Where(r => r.StartsWith(resourcePrefix) && r.EndsWith(".json"))
                                              .ToList();

            foreach (string recurso in mechanizedConditionsEmbebidos)
            {
                using var stream = assembly.GetManifestResourceStream(recurso);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                var mechanizedConditions = JsonSerializer.Deserialize<MechanizedConditions>(json);
                if (mechanizedConditions != null)
                {
                    mechanizedConditionsList.Add(mechanizedConditions);
                }
            }

            return mechanizedConditionsList;
        }
        public static List<OperationsShapes> CargarOperationsShapesRotoEmbebidos()
        {
            List<OperationsShapes> operationsShapesList = new();
            var assembly = Assembly.GetExecutingAssembly();

            string resourcePrefix = "RotoTools.Resources.Operaciones.Roto.OperationsShapesRoto.";

            var operationsShapesEmbebidos = assembly.GetManifestResourceNames()
                                              .Where(r => r.StartsWith(resourcePrefix) && r.EndsWith(".json"))
                                              .ToList();

            foreach (string recurso in operationsShapesEmbebidos)
            {
                using var stream = assembly.GetManifestResourceStream(recurso);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                var operationShape = JsonSerializer.Deserialize<OperationsShapes>(json);
                if (operationShape != null)
                {
                    operationsShapesList.Add(operationShape);
                }
            }

            return operationsShapesList;
        }
        public static List<MechanizedOperation> CargarMechanizedOperationsRotoEmbebidos()
        {
            List<MechanizedOperation> mechanizedOperationsList = new();
            var assembly = Assembly.GetExecutingAssembly();

            string resourcePrefix = "RotoTools.Resources.Operaciones.Roto.MechanizedOperationsRoto.";

            var mechanizedOperationsEmbebidos = assembly.GetManifestResourceNames()
                                              .Where(r => r.StartsWith(resourcePrefix) && r.EndsWith(".json"))
                                              .ToList();

            foreach (string recurso in mechanizedOperationsEmbebidos)
            {
                using var stream = assembly.GetManifestResourceStream(recurso);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                var mechanizedOperation = JsonSerializer.Deserialize<MechanizedOperation>(json);
                if (mechanizedOperation != null)
                {
                    mechanizedOperationsList.Add(mechanizedOperation);
                }
            }

            return mechanizedOperationsList;
        }
        public static List<string> LoadPrimitivesBD()
        {
            var list = new List<string>();
            string sqlQueryPrimitivas = "SELECT OperationName FROM MechanizedOperations WHERE IsPrimitive = 1 ORDER BY OperationName";
            using (var conn = new SqlConnection(Helpers.GetConnectionString()))
            {
                conn.Open();
                var cmd = new SqlCommand(sqlQueryPrimitivas, conn);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        list.Add(dr.GetString(0));
                }
            }

            return list;
        }
        public static bool ExisteOperacionEnBD(string operationName)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM MechanizedOperations WHERE OperationName = '{operationName}'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static bool ExisteOperacionEnBD(string operationName, int exterior)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM MechanizedOperations WHERE OperationName = '{operationName}' AND [External] = {exterior}", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static bool ExisteOperationShapeEnBD(string operationName)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM OperationsShapes WHERE OperationName = '{operationName}'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static bool ExisteCondicionEnBD(string mechanizedConditionXmlConditions, bool necesitaObjetoDeUsuario)
        {
            bool existeCondicionEnBD = ExisteCondicion(mechanizedConditionXmlConditions);
            if (existeCondicionEnBD)
                return true;

            if (necesitaObjetoDeUsuario)
            {
                bool existeParteDeCondicion = false;
                var partes = mechanizedConditionXmlConditions?.Split(new[] { "OBJECTID=" }, StringSplitOptions.None);
                if (partes != null && partes.Length == 2)
                {
                    existeParteDeCondicion = ExisteContenidoCondicion(partes[0]);
                    if (existeParteDeCondicion) 
                        return true;
                }
            }

            return false;
        }
        public static bool ExisteCondicion(string mechanizedConditionXmlConditions)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM MechanizedConditions WHERE XmlConditions like '{mechanizedConditionXmlConditions}'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static bool ExisteContenidoCondicion(string mechanizedConditionXmlConditions)
        {
            // La consulta busca cualquier coincidencia que contenga el fragmento de XML proporcionado
            const string sql = "SELECT COUNT(1) FROM MechanizedConditions WHERE XmlConditions LIKE @XmlConditions";

            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    // Limpiamos espacios en blanco y rodeamos con comodines %
                    // El uso de SqlDbType.NVarChar es más seguro para contenido XML
                    cmd.Parameters.Add("@XmlConditions", SqlDbType.NVarChar).Value = $"%{mechanizedConditionXmlConditions.Trim()}%";

                    conn.Open();

                    // ExecuteScalar devuelve el resultado de COUNT(1)
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                // Opcional: Loguear el error según tu sistema de logs
                // Console.WriteLine($"Error al consultar condiciones: {ex.Message}");
                return false;
            }
        }
        public static bool ExisteObjetoUsuarioEnBD(string name)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM MechanizedObjects WHERE Name like '%{name}%'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        public static void InstallMacrosMechanizedOperations()
        {
            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    //Cargar la lista de macros embebidas
                    List<MechanizedOperation> macrosMechanizedOperationsList = new List<MechanizedOperation>();
                    macrosMechanizedOperationsList = CargarMacrosMechanizedOperationsEmbebidos();


                    // Insertar los del proyecto
                    foreach (var macroMechanizedOperation in macrosMechanizedOperationsList)
                    {
                        if (!ExisteOperacionEnBD(macroMechanizedOperation.OperationName))
                        {
                            InstallMechanizedOperation(macroMechanizedOperation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(34)" + Environment.NewLine + Environment.NewLine +
                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void InstallMacrosOperationsShapes()
        {
            try
            {
                List<MechanizedOperation> macrosMechanizedOperationsList = new List<MechanizedOperation>();
                List<OperationsShapes> macrosOperationsShapesList = new List<OperationsShapes>();
                macrosMechanizedOperationsList = CargarMacrosMechanizedOperationsEmbebidos();
                macrosOperationsShapesList = CargarMacrosOperationsShapesEmbebidos();


                // Insertar los del proyecto
                foreach (var macroMechanizedOperation in macrosMechanizedOperationsList)
                {
                    foreach (OperationsShapes operationShapes in macrosOperationsShapesList.Where(os => os.OperationName == macroMechanizedOperation.OperationName))
                    {
                        InstallOperationShape(operationShapes);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(35)" + Environment.NewLine + Environment.NewLine +
                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void InstallMechanizedOperation(MechanizedOperation mechanizedOperation)
        {

            string insert = @"INSERT INTO MechanizedOperations 
                                        (OperationName, [Description], [External], RGB, Level1, Level2, Level3, Level4, Level5, IsPrimitive, XMLParameters, Disable)
                                        VALUES (@OperationName, @Description, @External, @Rgb, @Level1, @Level2, @Level3, @Level4, @Level5, @IsPrimitive, @XMLParameters, @Disable)";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand(insert, conn))
                {
                    cmd.Parameters.AddWithValue("@OperationName", mechanizedOperation.OperationName.Trim());
                    cmd.Parameters.AddWithValue("@Description", (object?)mechanizedOperation.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@External", (object)mechanizedOperation.External);
                    cmd.Parameters.AddWithValue("@Rgb", (object)mechanizedOperation.RGB ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Level1", (object)mechanizedOperation.Level1 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Level2", (object)mechanizedOperation.Level2 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Level3", (object)mechanizedOperation.Level3 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Level4", (object)mechanizedOperation.Level4 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Level5", (object)mechanizedOperation.Level5 ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsPrimitive", (object)mechanizedOperation.IsPrimitive ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@XMLParameters", (object)mechanizedOperation.XmlParameters ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Disable", (object)mechanizedOperation.Disable ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void InstallOperationShape(OperationsShapes operationShape)
        {
            string insert = @"INSERT INTO OperationsShapes 
                                    (OperationName, BasicShape, [External], XDistance, YDistance, ZDistance, Mill, Depth, XMLParameters, Dimension, Rotation, Conditions, [Order])
                                    VALUES (@OperationName, @BasicShape, @External, @XDistance, @YDistance, @ZDistance, @Mill, @Depth, @XMLParameters, @Dimension, @Rotation, @Conditions, @Order)";

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand(insert, conn))
                {
                    cmd.Parameters.AddWithValue("@OperationName", operationShape.OperationName.Trim());
                    cmd.Parameters.AddWithValue("@BasicShape", (object?)operationShape.BasicShape ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@External", (object)operationShape.External);
                    cmd.Parameters.AddWithValue("@XDistance", (object)operationShape.XDistance ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@YDistance", (object)operationShape.YDistance ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZDistance", (object)operationShape.ZDistance ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Mill", (object)operationShape.Mill ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Depth", (object)operationShape.Depth ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@XMLParameters", (object)operationShape.XmlParameters ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Dimension", (object)operationShape.Dimension ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Rotation", (object)operationShape.Rotation ?? DBNull.Value);
                    cmd.Parameters.Add("@Conditions", SqlDbType.UniqueIdentifier).Value = string.IsNullOrWhiteSpace(operationShape.Conditions) ? (object)DBNull.Value : Guid.Parse(operationShape.Conditions);
                    cmd.Parameters.AddWithValue("@Order", (object)operationShape.Order ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void InstallMechanizedCondition(MechanizedConditions mechanizedCondition)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"INSERT INTO MechanizedConditions (Name, XmlConditions) VALUES (@conditionName, @xmlConditions)", conn))
            {
                cmd.Parameters.AddWithValue("@conditionName", mechanizedCondition.Name);
                cmd.Parameters.AddWithValue("@xmlConditions", mechanizedCondition.XmlConditions);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void InstallMechanizedObject(string name, string xmlObject)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"INSERT INTO MechanizedObjects ([Name], [XmlObject]) VALUES (@name, @xmlObject)", conn))
            {
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@xmlObject", xmlObject);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static string? GetMechanizedConditionRowId(string conditionName)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"SELECT RowId FROM MechanizedConditions WHERE Name = @name", conn))
            {
                cmd.Parameters.AddWithValue("@name", conditionName);

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
        public static string? GetMechanizedObjectRowId(string objectName)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"SELECT RowId FROM MechanizedObjects WHERE Name = @name", conn))
            {
                cmd.Parameters.AddWithValue("@name", objectName);

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
        public static string? GetMechanizedConditionRowIdByXmlConditions(string xmlConditions)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"SELECT RowId FROM MechanizedConditions WHERE XmlConditions like @xmlConditions", conn))
            {
                cmd.Parameters.AddWithValue("@xmlConditions", xmlConditions);

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
        public static string? GetMechanizedConditionRowIdByXmlConditionsConObjetoUsuario(string mechanizedConditionXmlConditions)
        {
            var parteCondicion = mechanizedConditionXmlConditions?.Split(new[] { "OBJECTID=" }, StringSplitOptions.None);
            if (parteCondicion == null || parteCondicion.Length != 2)
            {
                return String.Empty;
            }

            // La consulta busca cualquier coincidencia que contenga el fragmento de XML proporcionado
            const string sql = "SELECT RowId FROM MechanizedConditions WHERE XmlConditions LIKE @XmlConditions";

            try
            {
                using (var conn = new SqlConnection(GetConnectionString()))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    // Limpiamos espacios en blanco y rodeamos con comodines %
                    // El uso de SqlDbType.NVarChar es más seguro para contenido XML
                    cmd.Parameters.Add("@XmlConditions", SqlDbType.NVarChar).Value = $"%{parteCondicion[0].Trim()}%";

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
            catch (Exception ex)
            {
                // Opcional: Loguear el error según tu sistema de logs
                // Console.WriteLine($"Error al consultar condiciones: {ex.Message}");
                return String.Empty;
            }
        }
        public static Dictionary<string, string> ObtenerMapaEquivalenciasOperacionesTest()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Cerr. Super-infer pestillo recto", "Cerr_Sup_Inf_Pestillo" },
            };

        }
        public static Dictionary<string, string> ObtenerMapaEquivalenciasOperaciones()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "15_Cajeado cremona", "15_Caja_Cremona" },
                { "15_Taladro triple manilla", "15_Taladro_X3" },
                { "150R_Mecanizado cuerpo hoja", "150R_Hoja" },
                { "150R_Mecanizado cuerpo marco", "150R_Marco" },
                { "17_Bombillo exterior", "17_Bombillo_Ext" },
                { "17_Bombillo inter-exter", "17_Bombillo_Int_Ext" },
                { "17_Bombillo interior", "17_Bombillo_Int" },
                { "17_Cajeado cerradura", "17_Caja_Cerradura" },
                { "17_Cajeado cremona", "17_Caja_Cremona" },
                { "17_Mecanizado tirador exterior", "17_Taladro_Tirad_Ext" },
                { "17_Mecanizado tirador inter-exter", "17_Taladro_Tirad_Int_Ext" },
                { "17_Mecanizado tirador interior", "17_Taladro_Tirad_Int" },
                { "17_Mecanizado uñero con bombillo interior", "17_Caja_Uñero_B_Int" },
                { "17_Mecanizado uñero interior", "17_Caja_Uñero_Int" },
                { "17_Taladro triple manilla exterior", "17_Taladro_X3_Ext" },
                { "17_Taladro triple manilla inter-exter", "17_Taladro_X3_Int_Ext" },
                { "17_Taladro triple manilla inter-exterr", "17_Taladro_X3_Int_Ext" },
                { "17_Taladro triple manilla interior", "17_Taladro_X3_Int" },
                { "230177_Taladrado 3/100", "Soporte_NT_Taladro_3" },
                { "230177_Taladrado 3/100 Tornillo", "Soporte_NT_Tornillo_3" },
                { "230178_Taladrado 6/100", "Soporte_NT_Taladro_6" },
                { "230178_Taladrado 6/100 Tornillo", "Soporte_NT_Tornillo_6" },
                { "230343_Taladro 3/100", "Bisagra_Ang_Taladro_3" },
                { "245707_Taladrado 6/100", "Soporte_NT_Taladro_6" },
                { "25_Bocallave exterior", "25_Bocallave_Ext" },
                { "25_Bocallave inter-exter", "25_Bocallave_Int_Ext" },
                { "25_Bocallave interior", "25_Bocallave_Int" },
                { "25_Bombillo exterior", "25_Bombillo_Ext" },
                { "25_Bombillo inter-exter", "25_Bombillo_Int_Ext" },
                { "25_Bombillo interior", "25_Bombillo_Int" },
                { "25_Cajeado central cerradura", "25_Caja_Cerradura_Ctrl" },
                { "25_Cajeado central cerradura 1P", "25_Caja_Cerradura1P_Ctrl" },
                { "25_Cajeado cremona", "25_Caja_Cremona" },
                { "25_Cajeado inferior cremona", "25_Caja_Cremona_Inf" },
                { "25_Cajeado superior cremona", "25_Caja_Cremona_Sup" },
                { "25_Manilla exterior", "25_Manilla_Ext" },
                { "25_Manilla inter-exter", "25_Manilla_Int_Ext" },
                { "25_Manilla interior", "25_Manilla_Int" },
                { "25_Placa Manilla exterior", "25_Placa_Ext" },
                { "25_Placa Manilla inter-exter", "25_Placa_Int_Ext" },
                { "25_Placa Manilla interior", "25_Placa_Int" },
                { "25_Taladro triple manilla", "25_Taladro_X3" },
                { "25_Taladro triple manilla exterior", "25_Taladro_X3_Ext" },
                { "25_Taladro triple manilla inter-exter", "25_Taladro_X3_Int_Ext" },
                { "25_Taladro triple manilla interior", "25_Taladro_X3_Int" },
                { "258590_Taladrado 3/100", "Pernio_NT_Taladro_3" },
                { "258590_Taladrado 3/100 Tornillo", "Pernio_NT_Tornillo_3" },
                { "258592_Taladrado 6/100", "Pernio_NT_Taladro_6" },
                { "258592_Taladrado 6/100 Tornillo", "Pernio_NT_Tornillo_6" },
                { "263858_Taladro 6/130", "Bisagra_Ang_Taladro_6" },
                { "264015_Bisagra canal", "Bisagra_Canal" },
                { "264019_Bisagra canal", "Bisagra_Canal" },
                { "28_Bombillo exterior", "28_Bombillo_Ext" },
                { "28_Bombillo inter-exter", "28_Bombillo_Int_Ext" },
                { "28_Bombillo interior", "28_Bombillo_Int" },
                { "28_Cajeado central cerradura", "28_Caja_Cerradura_Ctrl" },
                { "28_Manilla exterior", "28_Manilla_Ext" },
                { "28_Manilla inter-exter", "28_Manilla_Int_Ext" },
                { "28_Manilla interior", "28_Manilla_Int" },
                { "28_Placa Manilla exterior", "28_Placa_Ext" },
                { "28_Placa Manilla inter-exter", "28_Placa_Int_Ext" },
                { "28_Placa Manilla interior", "28_Placa_Int" },
                { "2D_Hoja", "2D_Hoja" },
                { "2D_Marco", "2D_Marco" },
                { "30_Bocallave exterior", "30_Bocallave_Ext" },
                { "30_Bocallave inter-exter", "30_Bocallave_Int_Ext" },
                { "30_Bocallave interior", "30_Bocallave_Int" },
                { "30_Bombillo exterior", "30_Bombillo_Ext" },
                { "30_Bombillo inter-exter", "30_Bombillo_Int_Ext" },
                { "30_Bombillo interior", "30_Bombillo_Int" },
                { "30_Cajeado central cerradura", "30_Caja_Cerradura_Ctrl" },
                { "30_Cajeado central cerradura 1P", "30_Caja_Cerradura1P_Ctrl" },
                { "30_Cajeado cremona", "30_Caja_Cremona" },
                { "30_Cajeado inferior cerradura", "30_Caja_Cerradura_Inf" },
                { "30_Cajeado inferior cremona", "30_Caja_Cremona_Inf" },
                { "30_Cajeado superior cerradura", "30_Caja_Cerradura_Sup" },
                { "30_Cajeado superior cremona", "30_Caja_Cremona_Sup" },
                { "30_Manilla exterior", "30_Manilla_Ext" },
                { "30_Manilla inter-exter", "30_Manilla_Int_Ext" },
                { "30_Manilla interior", "30_Manilla_Int" },
                { "30_Placa Manilla exterior", "30_Placa_Ext" },
                { "30_Placa Manilla inter-exter", "30_Placa_Int_Ext" },
                { "30_Placa Manilla interior", "30_Placa_Int" },
                { "30_Taladro triple manilla", "30_Taladro_X3" },
                { "30_Taladro triple manilla exterior", "30_Taladro_X3_Ext" },
                { "30_Taladro triple manilla inter-exter", "30_Taladro_X3_Int_Ext" },
                { "30_Taladro triple manilla interior", "30_Taladro_X3_Int" },
                { "31_Mecanizado cuerpo hoja", "PS23_Hoja" },
                { "31_Mecanizado cuerpo marco", "PS23_Marco" },
                { "35_Bocallave exterior", "35_Bocallave_Ext" },
                { "35_Bocallave inter-exter", "35_Bocallave_Int_Ext" },
                { "35_Bocallave interior", "35_Bocallave_Int" },
                { "35_Bombillo exterior", "35_Bombillo_Ext" },
                { "35_Bombillo inter-exter", "35_Bombillo_Int_Ext" },
                { "35_Bombillo interior", "35_Bombillo_Int" },
                { "35_Cajeado central cerradura", "35_Caja_Cerradura_Ctrl" },
                { "35_Cajeado central cerradura 1P", "35_Caja_Cerradura1P_Ctrl" },
                { "35_Cajeado cremona", "35_Caja_Cremona" },
                { "35_Cajeado inferior cerradura", "35_Caja_Cerradura_Inf" },
                { "35_Cajeado inferior cremona", "35_Caja_Cremona_Inf" },
                { "35_Cajeado intermedio infer cerradura", "35_Caja_Cerradura_M_Inf" },
                { "35_Cajeado intermedio super cerradura", "35_Caja_Cerradura_M_Sup" },
                { "35_Cajeado motor", "Eneo_Caja_Motor" },
                { "35_Cajeado superior cerradura", "35_Caja_Cerradura_Sup" },
                { "35_Cajeado superior cremona", "35_Caja_Cremona_Sup" },
                { "35_Manilla exterior", "35_Manilla_Ext" },
                { "35_Manilla inter-exter", "35_Manilla_Int_Ext" },
                { "35_Manilla interior", "35_Manilla_Int" },
                { "35_Placa Manilla exterior", "35_Placa_Ext" },
                { "35_Placa Manilla inter-exter", "35_Placa_Int_Ext" },
                { "35_Placa Manilla interior", "35_Placa_Int" },
                { "35_Taladro triple manilla", "35_Taladro_X3" },
                { "35_Taladro triple manilla exterior", "35_Taladro_X3_Ext" },
                { "35_Taladro triple manilla inter-exter", "35_Taladro_X3_Int_Ext" },
                { "35_Taladro triple manilla interior", "35_Taladro_X3_Int" },
                { "350571_Iman", "Eneo_Iman" },
                { "37.5_Bombillo exterior", "37.5_Bombillo_Ext" },
                { "37.5_Bombillo inter-exter", "37.5_Bombillo_Int_Ext" },
                { "37.5_Bombillo interior", "37.5_Bombillo_Int" },
                { "37.5_Cajeado cremona", "37.5_Caja_Cremona" },
                { "37.5_Mecanizado uñero exterior", "37.5_Caja_Uñero_Ext" },
                { "37.5_Taladro triple manilla exterior", "37.5_Taladro_X3_Ext" },
                { "37.5_Taladro triple manilla inter-exter", "37.5_Taladro_X3_Int_Ext" },
                { "37.5_Taladro triple manilla interior", "37.5_Taladro_X3_Int" },
                { "39_Mecanizado cuerpo hoja", "150P_Hoja" },
                { "39_Mecanizado cuerpo marco", "150P_Marco" },
                { "40_Bocallave exterior", "40_Bocallave_Ext" },
                { "40_Bocallave inter-exter", "40_Bocallave_Int_Ext" },
                { "40_Bocallave interior", "40_Bocallave_Int" },
                { "40_Bombillo exterior", "40_Bombillo_Ext" },
                { "40_Bombillo inter-exter", "40_Bombillo_Int_Ext" },
                { "40_Bombillo interior", "40_Bombillo_Int" },
                { "40_Cajeado central cerradura", "40_Caja_Cerradura_Ctrl" },
                { "40_Cajeado central cerradura 1P", "40_Caja_Cerradura1P_Ctrl" },
                { "40_Cajeado cremona", "40_Caja_Cremona" },
                { "40_Cajeado inferior cerradura", "40_Caja_Cerradura_Inf" },
                { "40_Cajeado inferior cremona", "40_Caja_Cremona_Inf" },
                { "40_Cajeado intermedio infer cerradura", "40_Caja_Cerradura_M_Inf" },
                { "40_Cajeado intermedio super cerradura", "40_Caja_Cerradura_M_Sup" },
                { "40_Cajeado motor", "Eneo_Caja_Motor" },
                { "40_Cajeado superior cerradura", "40_Caja_Cerradura_Sup" },
                { "40_Cajeado superior cremona", "40_Caja_Cremona_Sup" },
                { "40_Manilla exterior", "40_Manilla_Ext" },
                { "40_Manilla inter-exter", "40_Manilla_Int_Ext" },
                { "40_Manilla interior", "40_Manilla_Int" },
                { "40_Placa Manilla exterior", "40_Placa_Ext" },
                { "40_Placa Manilla inter-exter", "40_Placa_Int_Ext" },
                { "40_Placa Manilla interior", "40_Placa_Int" },
                { "40_Taladro triple manilla exterior", "40_Taladro_X3_Ext" },
                { "40_Taladro triple manilla inter-exter", "40_Taladro_X3_Int_Ext" },
                { "40_Taladro triple manilla interior", "40_Taladro_X3_Int" },
                { "45_Bocallave exterior", "45_Bocallave_Ext" },
                { "45_Bocallave inter-exter", "45_Bocallave_Int_Ext" },
                { "45_Bocallave interior", "45_Bocallave_Int" },
                { "45_Bombillo exterior", "45_Bombillo_Ext" },
                { "45_Bombillo inter-exter", "45_Bombillo_Int_Ext" },
                { "45_Bombillo interior", "45_Bombillo_Int" },
                { "45_Cajeado central cerradura", "45_Caja_Cerradura_Ctrl" },
                { "45_Cajeado central cerradura 1P", "45_Caja_Cerradura1P_Ctrl" },
                { "45_Cajeado cremona", "45_Caja_Cremona" },
                { "45_Cajeado inferior cerradura", "45_Caja_Cerradura_Inf" },
                { "45_Cajeado inferior cremona", "45_Caja_Cremona_Inf" },
                { "45_Cajeado pico loro inferior", "45_Caja_Cerradura_PicoL_Inf" },
                { "45_Cajeado pico loro superior", "45_Caja_Cerradura_PicoL_Sup" },
                { "45_Cajeado superior cerradura", "45_Caja_Cerradura_Sup" },
                { "45_Cajeado superior cremona", "45_Caja_Cremona_Sup" },
                { "45_INV_Cajeado cremona", "45_Caja_Cremona_Inv" },
                { "45_INV_Taladro triple manilla", "45_Taladro_X3_Inv" },
                { "45_Manilla exterior", "45_Manilla_Ext" },
                { "45_Manilla inter-exter", "45_Manilla_Int_Ext" },
                { "45_Manilla interior", "45_Manilla_Int" },
                { "45_Placa Manilla exterior", "45_Placa_Ext" },
                { "45_Placa Manilla inter-exter", "45_Placa_Int_Ext" },
                { "45_Placa Manilla interior", "45_Placa_Int" },
                { "45_Taladro triple manilla exterior", "45_Taladro_X3_Ext" },
                { "45_Taladro triple manilla inter-exter", "45_Taladro_X3_Int_Ext" },
                { "45_Taladro triple manilla interior", "45_Taladro_X3_Int" },
                { "50_Bocallave exterior", "50_Bocallave_Ext" },
                { "50_Bocallave inter-exter", "50_Bocallave_Int_Ext" },
                { "50_Bocallave interior", "50_Bocallave_Int" },
                { "50_Bombillo exterior", "50_Bombillo_Ext" },
                { "50_Bombillo inter-exter", "50_Bombillo_Int_Ext" },
                { "50_Bombillo interior", "50_Bombillo_Int" },
                { "50_Cajeado central cerradura", "50_Caja_Cerradura_Ctrl" },
                { "50_Cajeado inferior cerradura", "50_Caja_Cerradura_Inf" },
                { "50_Cajeado inferior cremona", "50_Caja_Cremona_Inf" },
                { "50_Cajeado superior cerradura", "50_Caja_Cerradura_Sup" },
                { "50_Cajeado superior cremona", "50_Caja_Cremona_Sup" },
                { "50_INV_Cajeado cremona", "50_Caja_Cremona_Inv" },
                { "50_INV_Taladro triple manilla", "50_Taladro_X3_Inv" },
                { "50_Manilla exterior", "50_Manilla_Ext" },
                { "50_Manilla inter-exter", "50_Manilla_Int_Ext" },
                { "50_Manilla interior", "50_Manilla_Int" },
                { "50_Placa Manilla exterior", "50_Placa_Ext" },
                { "50_Placa Manilla inter-exter", "50_Placa_Int_Ext" },
                { "50_Placa Manilla interior", "50_Placa_Int" },
                { "50_Taladro triple manilla exterior", "50_Taladro_X3_Ext" },
                { "50_Taladro triple manilla inter-exter", "50_Taladro_X3_Int_Ext" },
                { "50_Taladro triple manilla interior", "50_Taladro_X3_Int" },
                { "55_Bombillo exterior", "55_Bombillo_Ext" },
                { "55_Bombillo inter-exter", "55_Bombillo_Int_Ext" },
                { "55_Bombillo interior", "55_Bombillo_Int" },
                { "55_Cajeado central cerradura", "55_Caja_Cerradura_Ctrl" },
                { "55_Cajeado inferior cerradura", "55_Caja_Cerradura_Inf" },
                { "55_Cajeado superior cerradura", "55_Caja_Cerradura_Sup" },
                { "55_Manilla exterior", "55_Manilla_Ext" },
                { "55_Manilla inter-exter", "55_Manilla_Int_Ext" },
                { "55_Manilla interior", "55_Manilla_Int" },
                { "55_Placa Manilla exterior", "55_Placa_Ext" },
                { "55_Placa Manilla inter-exter", "55_Placa_Int_Ext" },
                { "55_Placa Manilla interior", "55_Placa_Int" },
                { "-6_Taladro triple manilla", "6N_Taladro_X3" },
                { "7_Cajeado cremona", "7_Caja_Cremona" },
                { "7_Mecanizado uñero interior", "7_Caja_Uñero_Int" },
                { "7_Taladro triple manilla interior", "7_Taladro_X3_Int" },
                { "8_Cajeado cremona", "8_Caja_Cremona" },
                { "8_Taladro triple manilla", "8_Taladro_X3" },
                { "ALV_CarroCorto", "ALV_Carro_Corto" },
                { "ALV_CarroLargo", "ALV_Carro_Largo" },
                { "ALV_RefuerzoCarro", "ALV_Refuerzo_Carro" },
                { "ATB120_EXT_Mecanizado Hoja", "ATB120_Hoja_Ext" },
                { "ATB120_EXT_Mecanizado Marco", "ATB120_Marco_Ext" },
                { "ATB120_Mecanizado Hoja", "ATB120_Hoja" },
                { "ATB120_Mecanizado Marco", "ATB120_Marco" },
                { "ATB80_EXT_Mecanizado Hoja", "ATB80_Hoja_Ext" },
                { "ATB80_EXT_Mecanizado Marco", "ATB80_Marco_Ext" },
                { "ATB80_Mecanizado Hoja", "ATB80_Hoja" },
                { "ATB80_Mecanizado Marco", "ATB80_Marco" },
                { "Bisagra Pleg_Hoja EXT_20", "Pleg_BIS_Hoja_20_Ext" },
                { "Bisagra Pleg_Hoja EXT_30", "Pleg_BIS_Hoja_30_Ext" },
                { "Bisagra Pleg_Hoja EXT_40", "Pleg_BIS_Hoja_40_Ext" },
                { "Bisagra Pleg_Hoja EXT_44", "Pleg_BIS_Hoja_44_Ext" },
                { "Bisagra Pleg_Hoja EXT_M", "Pleg_BIS_Hoja_M_Ext" },
                { "Bisagra Pleg_Hoja_20", "Pleg_BIS_Hoja_20" },
                { "Bisagra Pleg_Hoja_30", "Pleg_BIS_Hoja_30" },
                { "Bisagra Pleg_Hoja_40", "Pleg_BIS_Hoja_40" },
                { "Bisagra Pleg_Hoja_44", "Pleg_BIS_Hoja_44" },
                { "Bisagra Pleg_Marco", "Pleg_BIS_Marco" },
                { "Bisagra_K_Hoja_6", "Bisagra_Canal" },
                { "Cajeado central cerradura", "Pletina_Caja_Ctrl" },
                { "Cajeado hoja cierre bisagra", "Cierre_Bisagra_Hoja" },
                { "Cajeado inferior cerradura", "Pletina_Caja_Inf" },
                { "Cajeado marco cierre bisagra", "Cierre_Bisagra_Marco" },
                { "Cajeado palanca cerradura", "Caja_Palanca" },
                { "Cajeado superior cerradura", "Pletina_Caja_Sup" },
                { "Cajeado_Pasacables", "Eneo_Pasacables" },
                { "Cerr. Super-infer combinado", "Cerr_Sup_Inf_Combinado" },
                { "Cerr. Super-infer pestillo recto", "Cerr_Sup_Inf_Pestillo" },
                { "Cerr. Super-infer pico loro", "Cerr_Sup_Inf_PicoL" },
                { "Cerradero aireacion", "ALV_Aireacion" },
                { "Cerradero basculacion", "Mrc_Cerr_Basc" },
                { "Cerradero basculacion derecha", "Mrc_Cerr_Basc_DR" },
                { "Cerradero basculacion izquierda", "Mrc_Cerr_Basc_IZ" },
                { "Cerradero batiente", "Mrc_Cerr_Inv" },
                { "Cerradero central Derecha", "Cerr_Ctrl_DR" },
                { "Cerradero central electrico Derecha", "Cerr_Elect_DR" },
                { "Cerradero central electrico Izquierda", "Cerr_Elect_IZ" },
                { "Cerradero central electrico Izquierdo", "Cerr_Elect_IZ" },
                { "Cerradero central inversora Derecha", "Cerr_Ctrl_Inv_DR" },
                { "Cerradero central inversora Izquierda", "Cerr_Ctrl_Inv_IZ" },
                { "Cerradero central Izquierda", "Cerr_Ctrl_IZ" },
                { "Cerradero corredera", "Mrc_Cerr_Corredera" },
                { "Cerradero electrico inversora Derecha", "Cerr_Elect_Inv_DR" },
                { "Cerradero electrico inversora Izquierda", "Cerr_Elect_Inv_IZ" },
                { "Cerradero inversora automatico", "Cerr_Inv_Automatico" },
                { "Cerradero inversora combi", "Cerr_Inv_Combinado" },
                { "Cerradero inversora pestillo recto", "Cerr_Inv_Pestillo" },
                { "Cerradero inversora pico loro", "Cerr_Inv_PicoL" },
                { "Cerradero lateral", "Mrc_Cerr_Simple" },
                { "Cerradero pasador", "Mrc_Cerr_Pasador" },
                { "Cerradero pasador puerta", "Cerr_Pasador_Puerta" },
                { "Cerradero seguridad", "Mrc_Cerr_Seguridad" },
                { "Cerradero super-infer automatico", "Cerr_Sup_Inf_Automatico" },
                { "Cerradero super-infer combinado", "Cerr_Sup_Inf_Combinado" },
                { "Cerradero ventilacion", "Mrc_Cerr_Ventilacion" },
                { "Cierre oculto", "Mrc_Cierre_Oculto_Marco" },
                { "Cierre oculto hoja", "Mrc_Cierre_Oculto_Hoja" },
                { "Click hoja", "Mrc_Click_Hoja" },
                { "Click marco", "Mrc_Click_Marco" },
                { "control de acceso", "Eneo_Caja_Control" },
                { "EXT 31_Mecanizado cuerpo hoja", "PS23_Hoja_Ext" },
                { "EXT 31_Mecanizado cuerpo marco", "PS23_Marco_Ext" },
                { "EXT 39_Mecanizado cuerpo hoja", "150P_Hoja_Ext" },
                { "EXT 39_Mecanizado cuerpo marco", "150P_Marco_Ext" },
                { "EXT_150R_Mecanizado cuerpo hoja", "150R_Hoja_Ext" },
                { "EXT_150R_Mecanizado cuerpo marco", "150R_Marco_Ext" },
                { "EXT_Pb10_Mecanizado cuerpo hoja", "Pb10_Hoja_Ext" },
                { "EXT_Pb10_Mecanizado cuerpo marco", "Pb10_Marco_Ext" },
                { "EXT_SolidC_Mec_hoja", "SolidC_Hoja_Ext" },
                { "EXT_SolidC_Mec_Marco", "SolidC_Marco_Ext" },
                { "Exter Mecanizado cuerpo hoja", "PS27_Hoja_Ext" },
                { "Exter Mecanizado cuerpo marco", "PS27_Marco_Ext" },
                { "FM batiente", "Mrc_FM_Inv" },
                { "FM marco", "Mrc_FM_Marco" },
                { "FM_Inferior", "ALV_FM_Inf" },
                { "Iman", "Eneo_Iman" },
                { "Mecanizado carro inferior", "Pleg_Carro_Inf" },
                { "Mecanizado carro inferior solera", "Pleg_Carro_Solera_Inf" },
                { "Mecanizado carro supeior", "Pleg_Carro_Sup" },
                { "Mecanizado carro superior", "Pleg_Carro_Sup" },
                { "Mecanizado cuerpo hoja", "PS27_Hoja" },
                { "Mecanizado cuerpo marco", "PS27_Marco" },
                { "Pb10_Mecanizado cuerpo hoja", "Pb10_Hoja" },
                { "Pb10_Mecanizado cuerpo marco", "Pb10_Marco" },
                { "Pernio_NX_Taladro_6", "Pernio_NX_Taladro_6" },
                { "Portero electrico", "Portero_Electrico" },
                { "Retenedor_Marco", "Mrc_Retenedor_Marco" },
                { "SCR100_EXT_Taladrado 3/100", "SR100_Taladro_3_Ext" },
                { "SCR100_Taladrado 3/100", "SR100_Taladro_3" },
                { "SolidC_Mec_hoja", "SolidC_Hoja" },
                { "SolidC_Mec_Marco", "SolidC_Marco" },
                { "Soporte_Compas_Taladro_6", "Soporte_NX_Taladro_6" },
                { "Soporte_Compas_Tornillo_6", "Soporte_NX_Tornillo_6" },
                { "Soporte_Confort", "ALV_Soporte_Confort" },
                { "Tornillo Cerradero puerta", "Cerr_Tornillo" },
                { "Vent reducida", "Mrc_Ventilacion_Reduc" },
                { "Soporte_Compas_Taladro_3", "Soporte_NX_Taladro_3" },
                { "Soporte_Compas_Taladro_3_EXT", "Soporte_NX_Taladro_3_Ext" },
                { "Cerradero central Fasteo", "Cerr_Ctrl_Fasteo" },
                { "263858_SCREW_NO_KP", "SCREW_NO_KP_H" },
            };

        }
        #endregion

        #region Utilidades PrefOpen
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
        #endregion

        #region Utilidades Escandallos
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
        public static string GetContenidoEscandalloRO_C_OCULTAR(List<Option> optionsList)
        {
            string contenidoEscandallo = "";

            foreach (Option opcion in optionsList.OrderBy(o => o.Name))
            {
                contenidoEscandallo += $"ESTABLECEOPCION(\"RO_{opcion.Name}\",\"Oculto\");\r\n";
            }

            return contenidoEscandallo;
        }

        #endregion

        #region Utilidades Traducción
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
                    else if (nombreHoja.StartsWith("sets"))
                    {
                        foreach (var row in ws.RowsUsed().Skip(1))
                        {
                            string desc = row.Cell(1).GetString().Trim();
                            string trad = row.Cell(2).GetString().Trim();
                            if (!string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(trad))
                                traducciones.Sets[desc] = trad;
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

        #endregion

        #region Utilidades Tarifas
        public static void InsertTariff(string tariffName, string divisa)
        {
            string queryInsert = "INSERT INTO Tariff(AliasInEntity,Type,Name,IsCost,TariffOrder,Currency,CustomerCode,CustomerEntityId,CustomerTypeId," +
                                                    "ProjectCode,SalesDocumentNumber,SalesDocumentVersion,PaintFactor,TariffPaintUnitsType,TariffAForReseller,TariffBForReseller)" +
                                "SELECT NULL,0,@tariffName,0,-1,@divisa,0,NULL,0,0,0,0,1.000000,6,NULL,NULL";

            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(queryInsert, conn))
            {
                cmd.Parameters.AddWithValue("@tariffName", tariffName);
                cmd.Parameters.AddWithValue("@divisa", divisa);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void InsertTariffAlLargo(string tariffName, string divisa)
        {
            string queryInsert = "INSERT INTO Tariff(AliasInEntity,Type,Name,IsCost,TariffOrder,Currency,CustomerCode,CustomerEntityId,CustomerTypeId," +
                                                    "ProjectCode,SalesDocumentNumber,SalesDocumentVersion,PaintFactor,TariffPaintUnitsType,TariffAForReseller,TariffBForReseller)" +
                                "SELECT NULL,4,@tariffName,0,-1,@divisa,0,NULL,0,0,0,0,1.000000,6,NULL,NULL";

            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(queryInsert, conn))
            {
                cmd.Parameters.AddWithValue("@tariffName", tariffName);
                cmd.Parameters.AddWithValue("@divisa", divisa);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static int UpdateTariffOrder(string tariffName)
        {
            string queryUpdate = "UPDATE Tariff SET TariffOrder=(SELECT ISNULL(MAX(TariffOrder)+1, 1) FROM Tariff) WHERE Type = 0 AND Name=@tariffName";

            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(queryUpdate, conn))
            {
                cmd.Parameters.AddWithValue("@tariffName", tariffName);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
        public static int UpdateTariffOrderAlLargo(string tariffName)
        {
            string queryUpdate = "UPDATE Tariff SET TariffOrder=(SELECT ISNULL(MAX(TariffOrder)+1, 1) FROM Tariff) WHERE Type = 4 AND Name=@tariffName";

            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(queryUpdate, conn))
            {
                cmd.Parameters.AddWithValue("@tariffName", tariffName);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
        public static bool ExisteTariffEnBD(string tariffName, int type)
        {
            using SqlConnection conexion = new SqlConnection(GetConnectionString());
            conexion.Open();
            using SqlCommand cmd = new SqlCommand($"SELECT Count(*) FROM Tariff WHERE Name = '{tariffName}' AND Type = {type}", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;
            }
            return false;
        }
        public static string? GetDivisaPorDefecto()
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"SELECT RTRIM(Valor) FROM VariablesGlobales WHERE Empresa=1 AND Nombre=N'DivisaDefecto'", conn))
            {
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

        #endregion

        #region  Utilidades Varias
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
        public static string? GetConectorActivo()
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"SELECT Valor FROM VariablesGlobales WHERE Nombre = 'Conector Herraje'", conn))
            {
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
        public static string? GetNombreXMLActualizacionRoto(enumHardwareType hardwareType)
        {
            string variableGlobalName = "RotoXmlNombrePVC";
            switch (hardwareType)
            {
                case enumHardwareType.PVC:
                    variableGlobalName = "RotoXmlNombrePVC";
                    break;
                case enumHardwareType.Aluminio:
                    variableGlobalName = "RotoXmlNombreALU";
                    break;
                case enumHardwareType.PAX:
                    variableGlobalName = "RotoXmlNombrePAX";
                    break;
            }

            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"SELECT RTRIM(Valor) FROM VariablesGlobales WHERE Nombre=N'{variableGlobalName}'", conn))
            {
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
        public static string GetFechaActualizacionRoto(enumHardwareType hardwareType)
        {
            string variableGlobalName = "RotoFechaActualizacionPVC";
            switch (hardwareType)
            {
                case enumHardwareType.PVC:
                    variableGlobalName = "RotoFechaActualizacionPVC";
                    break;
                case enumHardwareType.Aluminio:
                    variableGlobalName = "RotoFechaActualizacionALU";
                    break;
                case enumHardwareType.PAX:
                    variableGlobalName = "RotoFechaActualizacionPAX";
                    break;
            }

            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand($"SELECT RTRIM(Valor) FROM VariablesGlobales WHERE Nombre=N'{variableGlobalName}'", conn))
            {
                conn.Open();
                var resultado = cmd.ExecuteScalar(); // ExecuteScalar es más eficiente para un solo valor

                if (resultado != null && resultado != DBNull.Value)
                {
                    if (DateTime.TryParse(resultado.ToString(), out DateTime fecha))
                    {
                        return fecha.ToString("dd/MM/yyyy HH:mm");
                    }
                }
            }
            return string.Empty;
        }
        public static void SetFechaActualizacionRoto(enumHardwareType hardwareType, DateTime fecha)
        {
            string variableGlobalName = "RotoFechaActualizacionPVC";
            switch (hardwareType)
            {
                case enumHardwareType.PVC:
                    variableGlobalName = "RotoFechaActualizacionPVC";
                    break;
                case enumHardwareType.Aluminio:
                    variableGlobalName = "RotoFechaActualizacionALU";
                    break;
                case enumHardwareType.PAX:
                    variableGlobalName = "RotoFechaActualizacionPAX";
                    break;
            }

            // Convertimos la fecha al formato que espera tu columna 'Valor' (usualmente string)
            string fechaString = fecha.ToString("yyyy-MM-dd HH:mm");

            string sql = @"
                        IF EXISTS (SELECT 1 FROM VariablesGlobales WHERE Nombre = N'" + variableGlobalName + "')" +
                        "BEGIN " +
                            "UPDATE VariablesGlobales " +
                            "SET Valor = @valor " +
                            "WHERE Nombre = N'" + variableGlobalName + "' " +
                        "END " +
                        "ELSE " +
                        "BEGIN " +
                            "INSERT INTO VariablesGlobales (Empresa, Nombre, Valor) " +
                            "VALUES (1, N'" + variableGlobalName + "', @valor) " +
                        "END;";

            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            {
                // Usamos parámetros para evitar SQL Injection y errores de formato
                cmd.Parameters.AddWithValue("@valor", fechaString);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void SetNombreXMLRoto(enumHardwareType hardwareType, string fileName)
        {
            string variableGlobalName = "RotoXmlNombrePVC";
            switch (hardwareType)
            {
                case enumHardwareType.PVC:
                    variableGlobalName = "RotoXmlNombrePVC";
                    break;
                case enumHardwareType.Aluminio:
                    variableGlobalName = "RotoXmlNombreALU";
                    break;
                case enumHardwareType.PAX:
                    variableGlobalName = "RotoXmlNombrePAX";
                    break;
            }

            string sql = @"
                        IF EXISTS (SELECT 1 FROM VariablesGlobales WHERE Nombre = N'" + variableGlobalName + "')" +
                        "BEGIN " +
                            "UPDATE VariablesGlobales " +
                            "SET Valor = @valor " +
                            "WHERE Nombre = N'" + variableGlobalName + "' " +
                        "END " +
                        "ELSE " +
                        "BEGIN " +
                            "INSERT INTO VariablesGlobales (Empresa, Nombre, Valor) " +
                            "VALUES (1, N'" + variableGlobalName + "', @valor) " +
                        "END;";

            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(sql, conn))
            {
                // Usamos parámetros para evitar SQL Injection y errores de formato
                cmd.Parameters.AddWithValue("@valor", fileName);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Utilidades Generales
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

        #endregion

        #region private methos

        private static string? ExtraerValorRegistro(string fuente, string clave)
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
                return new Option("RO_" + name, value);

            // Traducir nombre y valor si existen en el diccionario
            string nombreTraducido = TranslateManager.TraduccionesActuales.TraducirOptionName(name);
            string valorTraducido = TranslateManager.TraduccionesActuales.TraducirOptionValue(name, value);


            return new Option("RO_" + nombreTraducido, valorTraducido);
        }
    }
    public static class SetHelpers
    {
        public static string TranslateSet(string setCode)
        {
            // Si no hay traducción activa, devolver tal cual
            if (!TranslateManager.AplicarTraduccion || TranslateManager.TraduccionesActuales == null)
                return setCode;

            // Traducir codigo si existen en el diccionario
            string setCodeTraducido = TranslateManager.TraduccionesActuales.TraducirSetCode(setCode);


            return setCodeTraducido;
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
