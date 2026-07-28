using Microsoft.Data.SqlClient;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RotoTools
{
    /// <summary>
    /// Plantilla de mecanizado 3D: para una Operación + Rol de perfil concretos, define las
    /// fórmulas (en función de los datos constructivos del perfil) que hay que evaluar para
    /// obtener los valores reales a insertar en ProfileOperations.
    /// Se carga desde el catálogo embebido Resources/Mecanizados3D/CatalogoOperaciones3D.json,
    /// generado a partir de "consultas mecanizados 3D_PARTE_1/2.sql" y de
    /// "informacion tablas centros 3D.xlsx", que se usaban manualmente hasta ahora.
    /// </summary>
    public class Operacion3DTemplate
    {
        public string OperationName { get; set; }
        public string Role { get; set; }
        public int Outer { get; set; }
        public string XFormula { get; set; }
        public string YFormula { get; set; }
        public string ZFormula { get; set; }
        public int Plane { get; set; }
        public int Depth { get; set; }
        public int Master { get; set; }
        public string XmlParameters { get; set; }
        public string Layers { get; set; }
        public int MirrorHorizontalForMachining { get; set; }
        public int MirrorVerticalForMachining { get; set; }
        public int RotationForMachining { get; set; }
        public int Face { get; set; }
        public int Disabled { get; set; }
        public int IsBidirectional { get; set; }
    }

    /// <summary>
    /// Datos constructivos de un perfil (Perfiles + MaterialesBase), usados como variables
    /// para evaluar las fórmulas del catálogo de mecanizados 3D.
    /// </summary>
    public class DatosConstructivosPerfil
    {
        public string ReferenciaBase { get; set; }
        public Guid ProfileId { get; set; } // MaterialesBase.RowId
        public double AnchoInterior { get; set; }
        public double AnchoExterior { get; set; }
        public double CuerpoInterior { get; set; }
        public double CuerpoExterior { get; set; }
        public double Altura { get; set; }
    }

    /// <summary>
    /// Resumen del resultado de una instalación de operaciones 3D, para informar al usuario.
    /// </summary>
    public class ResultadoInstalacion3D
    {
        public int PerfilesProcesados { get; set; }
        public int OperacionesInstaladas { get; set; }
        public int OperacionesOmitidasPorExistente { get; set; }

        // Perfiles para los que no se han encontrado datos constructivos (Perfiles/MaterialesBase),
        // es decir, un problema de datos del perfil, no del catálogo de operaciones 3D.
        public List<string> CombinacionesSinDefinicion { get; } = new List<string>();

        // Operaciones seleccionadas por el usuario que no han encontrado ninguna plantilla en el
        // catálogo para NINGÚN rol de los perfiles de la lista (si solo falta para algún rol
        // concreto, no se informa: puede que esa operación no la necesite).
        public List<string> OperacionesSinDefinicionEnCatalogo { get; } = new List<string>();
    }

    /// <summary>
    /// Entrada de la biblioteca de perfiles: para una ReferenciaBase ya conocida (de las perfilerías
    /// AluEuropa OM, Cortizo, Deceuninck y Kommerling), el Rol de mecanizado y la altura del canal de
    /// herraje que se usaron anteriormente, para no tener que volver a pedirlos al usuario.
    /// Se carga desde el catálogo embebido Resources/Mecanizados3D/BibliotecaPerfiles3D.json, generado a
    /// partir de las hojas "AluEuropa OM", "Cortizo", "DECEUNINCK" y "Kommerling" de
    /// "informacion tablas centros 3D.xlsx".
    /// </summary>
    public class PerfilLibreriaEntry
    {
        public string ReferenciaBase { get; set; }
        public string Role { get; set; }
        public double PosicionCanalHerraje { get; set; }
    }

    public static class Cam3DHelpers
    {
        // ------------------------------------------------------------------
        // Roles de mecanizado 3D (columna "Role" del catálogo)
        // ------------------------------------------------------------------

        public static readonly string[] RolesMecanizado3D =
        {
            "Frame", "Outer Frame", "Mullion",
            "Outer Sash", "Window Sash", "Balcony Sash", "Door Sash", "Slide Sash",
            "Sash Stop"
        };

        /// <summary>
        /// Roles de tipo hoja que necesitan la altura del canal de herraje
        /// (introducida a mano) y el descuento de "Ala" (obtenido automáticamente
        /// de la tabla Distances).
        /// </summary>
        public static readonly HashSet<string> RolesConCanalHerraje = new(StringComparer.OrdinalIgnoreCase)
        {
            "Outer Sash", "Window Sash", "Balcony Sash", "Door Sash", "Slide Sash", "Sash Stop"
        };

        // RowId fijo, en la tabla Distances, del descuento de tipo "esclavo" que determina el
        // Ala de la hoja (= "Descuento canal de herraje" en la grid de perfiles a instalar).
        // Mismo valor que se usaba en "consultas mecanizados 3D_PARTE_1.sql". Público para que
        // Cam3D.cs pueda incluirlo directamente en la consulta de carga de perfiles (LEFT JOIN
        // Distances), y así traer el descuento de todos los perfiles en una sola consulta.
        public const string RowIdDescuentoTipoEsclavoAla = "322B30B2-6F40-4FA3-BCDD-80420F6D363F";

        /// <summary>
        /// Propone un Role de mecanizado 3D a partir del Role (más genérico) de MaterialesBase.
        /// Para 'sash' no hay forma de saber automáticamente si es Outer/Window/Balcony/Door/Slide,
        /// así que se deja en blanco para que lo indique el usuario.
        /// </summary>
        public static string RolPorDefecto(string roleMaterialesBase)
        {
            if (string.IsNullOrWhiteSpace(roleMaterialesBase)) return "";

            switch (roleMaterialesBase.Trim().ToLowerInvariant())
            {
                case "frame": return "Frame";
                case "mullion": return "Mullion";
                case "sash stop": return "Sash Stop";
                default: return "";
            }
        }

        // ------------------------------------------------------------------
        // Catálogo embebido
        // ------------------------------------------------------------------

        private static List<Operacion3DTemplate> _catalogoCache;

        // El JSON embebido está organizado como un diccionario "Role" -> lista de operaciones
        // (en vez de una única lista plana) para que en un editor de código las secciones de
        // cada Role se puedan plegar/colapsar como si fueran regiones, facilitando localizar
        // y modificar operaciones concretas. Aquí se aplana de nuevo a una List<Operacion3DTemplate>
        // para que el resto del código (que consume una lista plana) no tenga que cambiar.
        public static List<Operacion3DTemplate> CargarCatalogoOperaciones3D()
        {
            if (_catalogoCache != null) return _catalogoCache;

            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "RotoTools.Resources.Mecanizados3D.CatalogoOperaciones3D.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                _catalogoCache = new List<Operacion3DTemplate>();
                return _catalogoCache;
            }

            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();

            var porRole = JsonSerializer.Deserialize<Dictionary<string, List<Operacion3DTemplate>>>(json)
                ?? new Dictionary<string, List<Operacion3DTemplate>>();

            _catalogoCache = porRole.SelectMany(kvp => kvp.Value).ToList();
            return _catalogoCache;
        }

        /// <summary>
        /// Reemplaza la caché en memoria del catálogo (usado por la pantalla de administración
        /// "Catálogo 3D", tras guardar el fichero fuente en disco), para que la sesión en curso
        /// use inmediatamente las plantillas nuevas/editadas sin reiniciar la aplicación. El
        /// recurso embebido en el ensamblado en ejecución NO se modifica: solo estará actualizado
        /// en la próxima compilación, una vez el fichero fuente se suba al repositorio.
        /// </summary>
        public static void ActualizarCacheCatalogo(List<Operacion3DTemplate> catalogoActualizado)
        {
            _catalogoCache = catalogoActualizado ?? new List<Operacion3DTemplate>();
        }

        // ------------------------------------------------------------------
        // Biblioteca de perfiles (ReferenciaBase -> Rol de mecanizado + canal de herraje ya conocidos)
        // ------------------------------------------------------------------

        private static Dictionary<string, PerfilLibreriaEntry> _bibliotecaPerfilesCache;

        /// <summary>
        /// Diccionario (clave = ReferenciaBase recortada, sin distinguir mayúsculas) con los datos ya
        /// conocidos de cada perfil, para autocompletar el Rol de mecanizado y la altura del canal de
        /// herraje al añadirlo a la lista de instalación.
        /// </summary>
        public static Dictionary<string, PerfilLibreriaEntry> CargarBibliotecaPerfiles3D()
        {
            if (_bibliotecaPerfilesCache != null) return _bibliotecaPerfilesCache;

            var resultado = new Dictionary<string, PerfilLibreriaEntry>(StringComparer.OrdinalIgnoreCase);

            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "RotoTools.Resources.Mecanizados3D.BibliotecaPerfiles3D.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                List<PerfilLibreriaEntry> lista = JsonSerializer.Deserialize<List<PerfilLibreriaEntry>>(json) ?? new List<PerfilLibreriaEntry>();

                foreach (PerfilLibreriaEntry entrada in lista)
                {
                    string clave = entrada.ReferenciaBase?.Trim();
                    if (string.IsNullOrEmpty(clave)) continue;

                    resultado[clave] = entrada;
                }
            }

            _bibliotecaPerfilesCache = resultado;
            return _bibliotecaPerfilesCache;
        }

        /// <summary>
        /// Normaliza el Rol tal y como viene en la biblioteca de perfiles a uno de los roles del
        /// catálogo de operaciones 3D (RolesMecanizado3D), sin distinguir mayúsculas/minúsculas.
        /// "Sash" es un rol genérico (no indica si es Outer/Window/Balcony/Door/Slide Sash), así que
        /// en ese caso se devuelve vacío para que lo indique el usuario. Cualquier otro rol de la
        /// biblioteca que no esté en el catálogo (p.ej. "Elevadora", "Lift Sash") se devuelve tal cual.
        /// </summary>
        public static string NormalizarRolBiblioteca(string rolBiblioteca)
        {
            if (string.IsNullOrWhiteSpace(rolBiblioteca)) return "";

            string limpio = rolBiblioteca.Trim();

            if (string.Equals(limpio, "Sash", StringComparison.OrdinalIgnoreCase))
                return "";

            string coincidencia = RolesMecanizado3D
                .FirstOrDefault(r => string.Equals(r, limpio, StringComparison.OrdinalIgnoreCase));

            return coincidencia ?? limpio;
        }

        // ------------------------------------------------------------------
        // Evaluador de fórmulas (p.ej. "@CuerpoInterior-18.5", "@Altura-9-3.5", "0")
        // ------------------------------------------------------------------

        private static readonly Regex TokenPattern = new(@"([+-]?)\s*(@[A-Za-z]+|\d+(?:\.\d+)?)", RegexOptions.Compiled);

        public static double EvaluarFormula(string formula, Dictionary<string, double> variables)
        {
            if (string.IsNullOrWhiteSpace(formula)) return 0;

            // Alguna fórmula del origen tiene una comilla suelta delante (p.ej. "'@Ala+17"),
            // se elimina antes de tokenizar.
            string limpio = formula.Trim().TrimStart('\'').Trim();

            double total = 0;
            bool huboToken = false;

            foreach (Match m in TokenPattern.Matches(limpio))
            {
                huboToken = true;
                string signo = m.Groups[1].Value;
                string termino = m.Groups[2].Value;
                double valor;

                if (termino.StartsWith("@"))
                {
                    string nombreVariable = termino.Substring(1);
                    if (!variables.TryGetValue(nombreVariable, out valor))
                        throw new InvalidOperationException(
                            $"La fórmula '{formula}' usa la variable '@{nombreVariable}', que no está disponible para este perfil.");
                }
                else
                {
                    valor = double.Parse(termino, CultureInfo.InvariantCulture);
                }

                total += (signo == "-") ? -valor : valor;
            }

            if (!huboToken)
                throw new InvalidOperationException($"No se ha podido interpretar la fórmula '{formula}'.");

            return total;
        }

        // ------------------------------------------------------------------
        // Acceso a datos
        // ------------------------------------------------------------------

        /// <summary>
        /// Obtiene los datos constructivos (Perfiles + MaterialesBase.RowId) de un conjunto de
        /// perfiles, identificados por su ReferenciaBase. Equivale a la primera consulta de
        /// "consultas mecanizados 3D_PARTE_1.sql", pero sin pasar por una tabla temporal.
        /// Nota: Cam3D.cs ya no llama a este método en el flujo interactivo (doble clic / instalar):
        /// esos datos se cargan todos de una vez en Cam3D.CargarMaterialesBase() para evitar una
        /// consulta a la base de datos por cada perfil. Se mantiene como utilidad reutilizable.
        /// </summary>
        public static Dictionary<string, DatosConstructivosPerfil> ObtenerDatosConstructivos(List<string> referenciasBase)
        {
            var resultado = new Dictionary<string, DatosConstructivosPerfil>(StringComparer.OrdinalIgnoreCase);
            if (referenciasBase == null || referenciasBase.Count == 0) return resultado;

            using var conn = new SqlConnection(Helpers.GetConnectionString());
            conn.Open();

            var paramNames = referenciasBase.Select((r, i) => "@ref" + i).ToList();
            string query = $@"
                SELECT p.ReferenciaBase, mb.RowId, p.AnchoInterior, p.AnchoExterior,
                       p.CuerpoInterior, p.CuerpoExterior, p.Altura
                FROM Perfiles p
                INNER JOIN MaterialesBase mb ON mb.ReferenciaBase = p.ReferenciaBase
                WHERE p.ReferenciaBase IN ({string.Join(",", paramNames)})";

            using var cmd = new SqlCommand(query, conn);
            for (int i = 0; i < referenciasBase.Count; i++)
                cmd.Parameters.AddWithValue(paramNames[i], referenciasBase[i]);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string referenciaBase = reader["ReferenciaBase"].ToString().Trim();
                resultado[referenciaBase] = new DatosConstructivosPerfil
                {
                    ReferenciaBase = referenciaBase,
                    ProfileId = reader["RowId"] == DBNull.Value ? Guid.Empty : (Guid)reader["RowId"],
                    AnchoInterior = ConvertirADouble(reader["AnchoInterior"]),
                    AnchoExterior = ConvertirADouble(reader["AnchoExterior"]),
                    CuerpoInterior = ConvertirADouble(reader["CuerpoInterior"]),
                    CuerpoExterior = ConvertirADouble(reader["CuerpoExterior"]),
                    Altura = ConvertirADouble(reader["Altura"]),
                };
            }

            return resultado;
        }

        /// <summary>
        /// Obtiene el valor de "Ala" (descuento) para un conjunto de perfiles (identificados por
        /// el RowId de MaterialesBase), a partir de la tabla Distances - igual que hacía
        /// "consultas mecanizados 3D_PARTE_1.sql" para los perfiles de tipo hoja.
        /// Nota: Cam3D.cs ya no llama a este método (ver ObtenerDatosConstructivos); el descuento se
        /// trae para todos los perfiles de golpe con un LEFT JOIN en Cam3D.CargarMaterialesBase().
        /// Se mantiene como utilidad reutilizable.
        /// </summary>
        public static Dictionary<Guid, double> ObtenerAla(List<Guid> profileIds)
        {
            var resultado = new Dictionary<Guid, double>();
            if (profileIds == null || profileIds.Count == 0) return resultado;

            using var conn = new SqlConnection(Helpers.GetConnectionString());
            conn.Open();

            var paramNames = profileIds.Select((_, i) => "@pid" + i).ToList();
            string query = $@"
                SELECT MasterId, PDistance
                FROM Distances
                WHERE SlaveId = @slaveId AND MasterId IN ({string.Join(",", paramNames)})";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@slaveId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(RowIdDescuentoTipoEsclavoAla);
            for (int i = 0; i < profileIds.Count; i++)
                cmd.Parameters.Add(paramNames[i], SqlDbType.UniqueIdentifier).Value = profileIds[i];

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Guid masterId = (Guid)reader["MasterId"];
                resultado[masterId] = ConvertirADouble(reader["PDistance"]);
            }

            return resultado;
        }

        /// <summary>
        /// Comprueba si ya existe un ProfileOperations para ese perfil+operación. Debe ejecutarse
        /// sobre la MISMA conexión/transacción que está haciendo las inserciones (ver
        /// InstalarProfileOperation): si se abriera una conexión nueva, sus lecturas quedarían
        /// bloqueadas por los INSERT aún no confirmados de la transacción en curso sobre la misma
        /// tabla, y acabarían agotando el tiempo de espera ("Se agotó el tiempo de espera...").
        /// </summary>
        public static bool ExisteProfileOperation(SqlConnection conn, SqlTransaction tx, Guid profileId, string operationName, int outer)
        {
            using var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM ProfileOperations WHERE ProfileId = @profileId AND OperationName = @operationName AND [Outer] = @outer",
                conn, tx);
            cmd.Parameters.Add("@profileId", SqlDbType.UniqueIdentifier).Value = profileId;
            cmd.Parameters.AddWithValue("@operationName", operationName);
            cmd.Parameters.AddWithValue("@outer", outer);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Inserta un registro en ProfileOperations, evaluando las fórmulas de la plantilla con
        /// los datos constructivos del perfil. Equivale a uno de los INSERT INTO ProfileOperations
        /// de "consultas mecanizados 3D_PARTE_2.sql".
        /// </summary>
        public static void InstalarProfileOperation(SqlConnection conn, SqlTransaction tx, Guid profileId, string referenciaBase,
            Operacion3DTemplate plantilla, Dictionary<string, double> variables)
        {
            double x = EvaluarFormula(plantilla.XFormula, variables);
            double y = EvaluarFormula(plantilla.YFormula, variables);
            double z = EvaluarFormula(plantilla.ZFormula, variables);

            const string insert = @"
                INSERT INTO ProfileOperations
                    ([MakerId],[RowId],[ProfileId],[BaseReference],[OperationName],[Outer],[XDistance],[YDistance],[ZDistance],
                     [Plane],[Depth],[Master],[XMLParameters],[Layers],[MirrorHorizontalForMachining],[MirrorVerticalForMachining],
                     [RotationForMachining],[Face],[Disabled],[IsBidirectional])
                VALUES
                    (dbo.Getmakerid(), NEWID(), @ProfileId, @BaseReference, @OperationName, @Outer, @XDistance, @YDistance, @ZDistance,
                     @Plane, @Depth, @Master, @XmlParameters, @Layers, @MirrorH, @MirrorV, @RotationForMachining, @Face, @Disabled, @IsBidirectional)";

            using var cmd = new SqlCommand(insert, conn, tx);
            cmd.Parameters.Add("@ProfileId", SqlDbType.UniqueIdentifier).Value = profileId;
            cmd.Parameters.AddWithValue("@BaseReference", referenciaBase);
            cmd.Parameters.AddWithValue("@OperationName", plantilla.OperationName);
            cmd.Parameters.AddWithValue("@Outer", plantilla.Outer);
            cmd.Parameters.AddWithValue("@XDistance", x);
            cmd.Parameters.AddWithValue("@YDistance", y);
            cmd.Parameters.AddWithValue("@ZDistance", z);
            cmd.Parameters.AddWithValue("@Plane", plantilla.Plane);
            cmd.Parameters.AddWithValue("@Depth", plantilla.Depth);
            cmd.Parameters.AddWithValue("@Master", plantilla.Master);
            cmd.Parameters.AddWithValue("@XmlParameters", (object)plantilla.XmlParameters ?? "");
            cmd.Parameters.AddWithValue("@Layers", (object)plantilla.Layers ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MirrorH", plantilla.MirrorHorizontalForMachining);
            cmd.Parameters.AddWithValue("@MirrorV", plantilla.MirrorVerticalForMachining);
            cmd.Parameters.AddWithValue("@RotationForMachining", plantilla.RotationForMachining);
            cmd.Parameters.AddWithValue("@Face", plantilla.Face);
            cmd.Parameters.AddWithValue("@Disabled", plantilla.Disabled);
            cmd.Parameters.AddWithValue("@IsBidirectional", plantilla.IsBidirectional);

            cmd.ExecuteNonQuery();
        }

        public static double ConvertirADouble(object valorBD)
        {
            if (valorBD == null || valorBD == DBNull.Value) return 0;
            return Convert.ToDouble(valorBD, CultureInfo.InvariantCulture);
        }

        // ------------------------------------------------------------------
        // Instalación de la definición 2D (MechanizedOperation + OperationsShapes) si falta,
        // reutilizando el mismo banco de recursos que usa la instalación 2D del CAM.
        //
        // Duplicado deliberadamente a partir de CamMenu.btn_InstallOperation_Click /
        // CamMenu.InstallConditions (en vez de refactorizarlos) para no modificar ese flujo
        // 2D ya probado.
        // ------------------------------------------------------------------

        public static void AsegurarDefinicion2DInstalada(
            string operationFullName,
            List<OperationsShapes> operationShapeList,
            List<OperationsShapes> operationShapeExtList,
            List<MechanizedOperation> mechanizedOperationsEmbebidos,
            List<MechanizedOperation> macrosEmbeddedMechanizedOperations,
            List<OperationsShapes> macroOperationsShapesEmbeddedList)
        {
            if (Helpers.ExisteOperacionEnBD(operationFullName))
                return;

            List<MechanizedOperation> mechanizedOperationsList = mechanizedOperationsEmbebidos
                .Where(op => op.OperationName == operationFullName)
                .ToList();

            if (mechanizedOperationsList.Any())
            {
                foreach (MechanizedOperation operation in mechanizedOperationsList)
                {
                    operation.InitializeLevel2(operation.OperationName);
                    operation.InitializeLevel3(operation.OperationName, operation.Level2);
                    Helpers.InstallMechanizedOperation(operation);
                }
            }
            else
            {
                MechanizedOperation mechanizedOperation = new MechanizedOperation(operationFullName);
                Helpers.InstallMechanizedOperation(mechanizedOperation);
            }

            List<OperationsShapes> allOperationsShapes = new List<OperationsShapes>();
            if (operationShapeList != null) allOperationsShapes.AddRange(operationShapeList);
            if (operationShapeExtList != null) allOperationsShapes.AddRange(operationShapeExtList);

            foreach (OperationsShapes operationShape in allOperationsShapes)
            {
                if (!string.IsNullOrEmpty(operationShape.Conditions))
                {
                    operationShape.Conditions = InstallConditions(operationShape.Conditions);
                }

                if (!Helpers.ExisteOperacionEnBD(operationShape.BasicShape))
                {
                    MechanizedOperation? embeddedOperation = macrosEmbeddedMechanizedOperations
                        .FirstOrDefault(op => op.OperationName == operationShape.BasicShape);

                    if (embeddedOperation != null)
                    {
                        Helpers.InstallMechanizedOperation(embeddedOperation!);
                    }

                    List<OperationsShapes> macroOperationsShapesList = macroOperationsShapesEmbeddedList
                        .Where(o => o.OperationName == operationShape.BasicShape).ToList();
                    foreach (OperationsShapes operation in macroOperationsShapesList)
                    {
                        Helpers.InstallOperationShape(operation);
                    }
                }

                Helpers.InstallOperationShape(operationShape);
            }
        }

        private static string InstallConditions(string conditionId)
        {
            List<MechanizedConditions> allConditionsList = Helpers.CargarMechanizedConditionsEmbebidos();

            MechanizedConditions? mechanizedCondition = allConditionsList.FirstOrDefault(c => c.RowId == conditionId);

            if (mechanizedCondition != null)
            {
                if (!Helpers.ExisteCondicionEnBD(mechanizedCondition.XmlConditions, Convert.ToBoolean(mechanizedCondition.NecesitaObjetoDeUsuario)))
                {
                    string rowIdMechanizedObject = "";

                    if (mechanizedCondition.NecesitaObjetoDeUsuario == "true" && !string.IsNullOrEmpty(mechanizedCondition.XmlObject))
                    {
                        if (!Helpers.ExisteObjetoUsuarioEnBD(mechanizedCondition.ObjetoDeUsuario))
                        {
                            Helpers.InstallMechanizedObject(mechanizedCondition.ObjetoDeUsuario, mechanizedCondition.XmlObject);
                        }

                        rowIdMechanizedObject = Helpers.GetMechanizedObjectRowId(mechanizedCondition.ObjetoDeUsuario);
                        mechanizedCondition.XmlConditions = mechanizedCondition.XmlConditions.Replace("RowIdObjetoDeUsuario", rowIdMechanizedObject);
                    }

                    Helpers.InstallMechanizedCondition(mechanizedCondition);

                    return Helpers.GetMechanizedConditionRowId(mechanizedCondition.Name);
                }
                else
                {
                    if (Convert.ToBoolean(mechanizedCondition.NecesitaObjetoDeUsuario))
                    {
                        return Helpers.GetMechanizedConditionRowIdByXmlConditionsConObjetoUsuario(mechanizedCondition.XmlConditions);
                    }
                    else
                    {
                        return Helpers.GetMechanizedConditionRowIdByXmlConditions(mechanizedCondition.XmlConditions);
                    }
                }
            }
            else
            {
                return "";
            }
        }
    }
}
