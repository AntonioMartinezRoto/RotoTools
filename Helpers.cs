using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using RotoEntities;
using System.Text;
using System.Xml.Serialization;

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
        public static List<ContenidoOpcion> GetContenidoOpciones(string opcionName)
        {
            var lista = new List<ContenidoOpcion>();
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT Valor, Texto, Flags, Orden, Invalid FROM ContenidoOpciones WHERE Opcion=@nombre", conn))
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
                            Invalid = rdr.GetInt16(4)
                        });
                    }
                }
            }
            return lista;
        }
        public static void InsertContenidoOpcion(string opcionName, ContenidoOpcion cont)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("INSERT INTO ContenidoOpciones (Opcion, Valor, Texto, Flags, Orden, Invalid) VALUES (@nombre, @valor, @texto, @flags, @orden, @invalid)", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                cmd.Parameters.AddWithValue("@valor", cont.Valor);
                cmd.Parameters.AddWithValue("@texto", cont.Texto);
                cmd.Parameters.AddWithValue("@flags", cont.Flags);
                cmd.Parameters.AddWithValue("@orden", cont.Orden);
                cmd.Parameters.AddWithValue("@invalid", cont.Invalid);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void UpdateContenidoOpcion(string opcionName, ContenidoOpcion cont)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("UPDATE ContenidoOpciones SET Texto=@texto, Flags=@flags, Orden=@orden, Invalid=@invalid WHERE Opcion=@nombre AND Valor=@valor", conn))
            {
                cmd.Parameters.AddWithValue("@nombre", opcionName);
                cmd.Parameters.AddWithValue("@valor", cont.Valor);
                cmd.Parameters.AddWithValue("@texto", cont.Texto);
                cmd.Parameters.AddWithValue("@flags", cont.Flags);
                cmd.Parameters.AddWithValue("@orden", cont.Orden);
                cmd.Parameters.AddWithValue("@invalid", cont.Invalid);
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
        public static void UpdateGruposYProveedor(string IdGrupoPresupuestado, string IdGrupoProduccion, string IdProveedor)
        {
            using (var conn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("UPDATE MaterialesBase SET IdGrupoPresupuestado=@grupopresupuestado, IdGrupoProduccion=@grupoproduccion, CodigoProveedor=@codigoproveedor WHERE Nivel1='ROTO NX'", conn))
            {
                cmd.Parameters.AddWithValue("@grupopresupuestado", IdGrupoPresupuestado);
                cmd.Parameters.AddWithValue("@grupoproduccion", IdGrupoProduccion);
                cmd.Parameters.AddWithValue("@codigoproveedor", IdProveedor);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void UpdateMaterialesBaseFicticiosPropiedades(string[] articulos)
        {
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
                        cmd.ExecuteNonQuery();
                    }
                }
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
}
