using System.Data;
using Microsoft.Data.SqlClient;
using RotoTools;

namespace RotoTools.Suite.Services
{
    /// <summary>
    /// Envoltorio de RotoTools.Cam3DHelpers.InstalarProfileOperation que añade la opción
    /// "Dejar margen en posición de inicio" (checkbox + TextBox de Cam3DWindow, junto al título
    /// de la grid "Perfiles a instalar"; el TextBox admite de 0 a 10 mm, 1 mm por defecto): si el
    /// checkbox está marcado, ajusta el valor calculado en ese margen (margenPosicionInicio, en
    /// mm) antes de insertar el registro en ProfileOperations, según el plano de la operación
    /// (plantilla.Plane):
    ///   - Plano 0 o 360 -> resta el margen a ZDistance.
    ///   - Plano 90 -> resta el margen a YDistance.
    ///   - Plano 180 -> suma el margen a ZDistance.
    ///   - Plano 270 -> suma el margen a YDistance.
    /// Duplica deliberadamente el INSERT de Cam3DHelpers.InstalarProfileOperation (reutilizando
    /// EvaluarFormula, que sí es público) en vez de modificar el proyecto RotoTools original,
    /// con el mismo criterio ya usado por RotoTools.Suite.Services.SuiteLocalization: no tocar
    /// RotoTools.csproj en absoluto para esta funcionalidad nueva de la Suite.
    /// </summary>
    public static class Cam3DInstalacionService
    {
        /// <param name="margenPosicionInicio">Margen en mm a aplicar (checkbox marcado con un
        /// valor ya validado por Cam3DWindow, entre 0 exclusive y 10 inclusive), o null si el
        /// checkbox "Dejar margen en posición de inicio" no está marcado (sin ajuste).</param>
        public static void InstalarProfileOperation(SqlConnection conn, SqlTransaction tx, Guid profileId, string referenciaBase,
            Operacion3DTemplate plantilla, Dictionary<string, double> variables, double? margenPosicionInicio)
        {
            double x = Cam3DHelpers.EvaluarFormula(plantilla.XFormula, variables);
            double y = Cam3DHelpers.EvaluarFormula(plantilla.YFormula, variables);
            double z = Cam3DHelpers.EvaluarFormula(plantilla.ZFormula, variables);

            if (margenPosicionInicio.HasValue)
            {
                double margen = margenPosicionInicio.Value;
                if (plantilla.Plane == 0 || plantilla.Plane == 360)
                {
                    z -= margen;
                }
                else if (plantilla.Plane == 90)
                {
                    y -= margen;
                }
                else if (plantilla.Plane == 180)
                {
                    z += margen;
                }
                else if (plantilla.Plane == 270)
                {
                    y += margen;
                }
            }

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
    }
}
