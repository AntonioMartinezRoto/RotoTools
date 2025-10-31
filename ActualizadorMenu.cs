﻿using Microsoft.Data.SqlClient;
using RotoEntities;
using System.Text.Json;

namespace RotoTools
{
    public partial class ActualizadorMenu : Form
    {
        #region Const
        const string referenciaValorPorDefecto = "RO_260272";
        #endregion

        #region Private Properties

        private List<Proveedor> proveedoresList = new List<Proveedor>();
        private List<GrupoPresupuestado> gruposPresupuestadoList = new List<GrupoPresupuestado>();
        private List<GrupoProduccion> gruposProduccionList = new List<GrupoProduccion>();

        #endregion

        #region Private Const

        private const string queryUpdateNivel1OpcionesMaterialesBase = "UPDATE MaterialesBase SET Nivel1='ROTO' WHERE Nivel1 = 'ROTO NX'; UPDATE Opciones SET Nivel1 = 'ROTO' WHERE Nivel1 = 'ROTO NX';";
        private const string queryUpdateDescripciones = @"
                            UPDATE MB
                            SET MB.DESCRIPCION = F.DESCRIPTION
                            FROM MaterialesBase MB
                            INNER JOIN [OPEN].Fittings F
                                ON SUBSTRING(MB.ReferenciaBase, 4, LEN(MB.ReferenciaBase) - 3) = F.Reference
                            WHERE MB.ReferenciaBase LIKE 'RO\_%' ESCAPE '\'";
        private string[] materialesFicticios = { "RO_PROGRAM%", "RO_MEC%" };

        #endregion

        #region Constructor

        public ActualizadorMenu()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        private void ActualizadorMenu_Load(object sender, EventArgs e)
        {
            InitializeInfoConnection();
            CargarProveedores();
            CargarGruposPresupuestado();
            CargarGruposProduccion();
            AsignarValoresPorDefecto();
            CargarTextos();
        }
        private void btn_EjecutarScripts_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                EnableControls(false);
                ResultQuerys resultQuerys = EjecutarScripts();

                string mensaje = "Scripts ejecutados correctamente." + Environment.NewLine + Environment.NewLine +
                                 resultQuerys.ResultQueryUpdateGruposYProveedor.ToString() + " registros actualizados (Grupos presupuestado, producción y proveedor): " + Environment.NewLine + Environment.NewLine +
                                 resultQuerys.ResultQueryUpdateNivel1MaterialesBaseYOpciones.ToString() + " registros actualizados (Nivel 1 Materiales Base y Opciones): " + Environment.NewLine + Environment.NewLine +
                                 resultQuerys.ResultQueryUpdatePropFicticios.ToString() + " registros actualizados (Materiales Base ficticios): " + Environment.NewLine + Environment.NewLine +
                                 resultQuerys.ResultQueryUpdateDescripcionesMateriales.ToString() + " registros actualizados (Descripciones Materiales Base): " + Environment.NewLine;

                MessageBox.Show(mensaje,
                                "Ejecución completada",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                EnableControls(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error ejecutando los scripts: " + ex.Message,
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                EnableControls(true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void cmb_Proveedor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_Proveedor.SelectedItem is Proveedor prov)
            {
                txt_Proveedor.Text = prov.CodigoProveedor;
            }
            else
            {
                txt_Proveedor.Text = "";
            }
        }
        private void cmb_IdPresupuestado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_IdPresupuestado.SelectedItem is GrupoPresupuestado grupoPresupuestado)
            {
                txt_Presupuestado.Text = grupoPresupuestado.Id;
            }
            else
            {
                txt_Presupuestado.Text = "";
            }
        }
        private void cmb_IdProduccion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_IdProduccion.SelectedItem is GrupoProduccion grupoProduccion)
            {
                txt_Produccion.Text = grupoProduccion.Id;
            }
            else
            {
                txt_Produccion.Text = "";
            }
        }
        private void btn_EjecutarCarpeta_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Selecciona la carpeta con los scripts SQL";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string carpeta = dialog.SelectedPath;
                        string[] ficheros = Directory.GetFiles(carpeta, "*.sql");

                        if (ficheros.Length == 0)
                        {
                            MessageBox.Show("No se encontraron scripts .sql en la carpeta seleccionada.");
                            return;
                        }

                        Cursor.Current = Cursors.WaitCursor;
                        EnableControls(false);

                        int totalEjecutados = 0;
                        int rowsAfected = 0;
                        string message = "";

                        foreach (string fichero in ficheros)
                        {
                            string script = File.ReadAllText(fichero);

                            if (!string.IsNullOrWhiteSpace(script))
                            {
                                rowsAfected += Helpers.EjecutarNonQuery(script);
                                totalEjecutados++;
                                message += rowsAfected.ToString() + " registros afectados para el script " + fichero + Environment.NewLine + Environment.NewLine;
                            }
                        }
                        MessageBox.Show(message, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        EnableControls(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error ejecutando scripts(s): " + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls(true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

        }
        private void btn_OcultaOpciones_Click(object sender, EventArgs e)
        {
            AgregarValorOcultoOpcionesRoto();
        }
        private void btn_ExportarEscandallos_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Selecciona la carpeta donde guardar los escandallos";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string carpeta = dialog.SelectedPath;


                        Cursor.Current = Cursors.WaitCursor;
                        EnableControls(false);

                        List<Escandallo> escandallos = new List<Escandallo>();

                        using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                        {
                            conn.Open();
                            string query = @"SELECT * FROM Escandallos WHERE CODIGO LIKE 'RO\_%' ESCAPE '\'";
                            using (var cmd = new SqlCommand(query, conn))
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var escandallo = new Escandallo
                                    {
                                        Codigo = reader["Codigo"].ToString().Trim(),
                                        Type = Convert.ToInt16(reader["Type"]),
                                        Descripcion = reader["Descripcion"] as string,
                                        Nivel1 = reader["Nivel1"] as string,
                                        Nivel2 = reader["Nivel2"] as string,
                                        Nivel3 = reader["Nivel3"] as string,
                                        Nivel4 = reader["Nivel4"] as string,
                                        Nivel5 = reader["Nivel5"] as string,
                                        Variables = reader["Variables"] as string,
                                        Programa = reader["Programa"] as string,
                                        Texto = reader["Texto"] as string,
                                        Familia = reader["Familia"] as string,
                                        XMLTable = reader["XMLTable"] as string,
                                        ProductionType = reader.GetGuid(reader.GetOrdinal("ProductionType")),
                                        PrefShopStatus = Convert.ToInt16(reader["PrefShopStatus"])
                                    };

                                    Helpers.InicializarEscandalloRotoTipo(escandallo);

                                    escandallos.Add(escandallo);
                                }
                            }
                        }

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        };

                        foreach (var escandallo in escandallos)
                        {
                            string fileName = $"{escandallo.Codigo.Trim()}.json";
                            string path = Path.Combine(carpeta, fileName);
                            File.WriteAllText(path, JsonSerializer.Serialize(escandallo, options));
                        }

                        MessageBox.Show($"Exportados {escandallos.Count} escandallos a: " + Environment.NewLine + carpeta, "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        EnableControls(true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error guardando los escandallos: " + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                EnableControls(true);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void btn_InstalarEscandallos_Click(object sender, EventArgs e)
        {
            ActualizadorInstalarEscandallos actualizadorInstalarEscandallosForm = new ActualizadorInstalarEscandallos();
            actualizadorInstalarEscandallosForm.ShowDialog();
        }
        private void btn_ShowScripts_Click(object sender, EventArgs e)
        {
            ActualizadorEscandallos actualizadorEscandallosForm = new ActualizadorEscandallos();
            actualizadorEscandallosForm.ShowDialog();
        }
        private void btn_AddProveedor_Click(object sender, EventArgs e)
        {
            try
            {
                if (ExisteProveedorRotoEnBD())
                {
                    if (MessageBox.Show("Ya existe un proveedor de Roto. ¿Desea agregar igualmente el proveedor Roto Frank SA?",
                                        "Proveedor existente",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }

                AgregarProveedorRotoFrankSA();
                MessageBox.Show("Proveedor Roto Frank SA agregado correctamente.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarProveedores();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error insertando el proveedor Roto Frank SA" +Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Private methods
        private void EnableControls(bool enable)
        {
            btn_EjecutarCarpeta.Enabled = enable;
            btn_EjecutarScripts.Enabled = enable;
            btn_OcultaOpciones.Enabled = enable;
            btn_InstalarEscandallos.Enabled = enable;
            btn_ExportarEscandallos.Enabled = enable;
            cmb_IdPresupuestado.Enabled = enable;
            cmb_IdProduccion.Enabled = enable;
            cmb_Proveedor.Enabled = enable;
        }
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private bool ExisteProveedorRotoEnBD()
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM Proveedores WHERE Nombre LIKE '%ROTO%" + "'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        private void AgregarProveedorRotoFrankSA()
        {
            string insertProveedorRoto = @"INSERT INTO Proveedores (CodigoProveedor, Nombre) VALUES (" + GetNuevoCodigoProveedor() + ", 'Roto Frank SA')";
            Helpers.EjecutarNonQuery(insertProveedorRoto);
        }
        public int GetNuevoCodigoProveedor()
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(CodigoProveedor),0) FROM Proveedores", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) + 1;

            }
            return 1;
        }
        private ResultQuerys EjecutarScripts()
        {
            //Actualizar grupos de Presupuestado y Produccion y Proveedor Nivel1 = ROTO NX
            int rowsAfected = Helpers.UpdateGruposYProveedor(txt_Presupuestado.Text, txt_Produccion.Text, txt_Proveedor.Text);

            //Actualizar Nivel1 Opciones y MaterialesBase de ROTO NX a ROTO
            int rowsAfected2 = Helpers.EjecutarNonQuery(queryUpdateNivel1OpcionesMaterialesBase);

            //Actualizar propiedades MaterialesBase ficticios
            int rowsAfected3 = Helpers.UpdateMaterialesBaseFicticiosPropiedades(materialesFicticios);

            //Actualizar descripciones MaterialesBase desde los fittings
            int rowsAfected4 = Helpers.EjecutarNonQuery(queryUpdateDescripciones);

            ResultQuerys resultQuerys = new ResultQuerys(rowsAfected, rowsAfected2, rowsAfected3, rowsAfected4);
            return resultQuerys;

        }
        private void CargarProveedores()
        {
            proveedoresList.Clear();
            using (var conn = new SqlConnection(Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT CodigoProveedor, Nombre FROM Proveedores ORDER BY Nombre", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        proveedoresList.Add(new Proveedor
                        {
                            CodigoProveedor = reader["CodigoProveedor"].ToString(),
                            Nombre = reader["Nombre"].ToString()
                        });
                    }
                }
            }
            cmb_Proveedor.DataSource = null;
            cmb_Proveedor.DataSource = proveedoresList;
            cmb_Proveedor.DisplayMember = "Nombre";
            cmb_Proveedor.ValueMember = "CodigoProveedor";
        }
        private void CargarGruposPresupuestado()
        {
            using (var conn = new SqlConnection(Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT GroupId, GroupName FROM Groups WHERE GroupType = 2 ORDER BY GroupName", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gruposPresupuestadoList.Add(new GrupoPresupuestado
                        {
                            Id = reader["GroupId"].ToString(),
                            Name = reader["GroupName"].ToString()
                        });
                    }
                }
            }

            cmb_IdPresupuestado.DataSource = gruposPresupuestadoList;
            cmb_IdPresupuestado.DisplayMember = "Name";
            cmb_IdPresupuestado.ValueMember = "Id";
        }
        private void CargarGruposProduccion()
        {
            using (var conn = new SqlConnection(Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT GroupId, GroupName FROM Groups WHERE GroupType = 3 ORDER BY GroupName", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gruposProduccionList.Add(new GrupoProduccion
                        {
                            Id = reader["GroupId"].ToString(),
                            Name = reader["GroupName"].ToString()
                        });
                    }
                }
            }

            cmb_IdProduccion.DataSource = gruposProduccionList;
            cmb_IdProduccion.DisplayMember = "Name";
            cmb_IdProduccion.ValueMember = "Id";
        }
        private void AsignarValoresPorDefecto()
        {
            using (var conn = new SqlConnection(Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT CodigoProveedor, IdGrupoPresupuestado, IdGrupoProduccion FROM MATERIALESBASE WHERE REFERENCIABASE = '" + referenciaValorPorDefecto + "'", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string codigoProveedor = reader["CodigoProveedor"].ToString();
                        string idGrupoPresupuestado = reader["IdGrupoPresupuestado"].ToString();
                        string idGrupoProduccion = reader["IdGrupoProduccion"].ToString();

                        cmb_Proveedor.SelectedValue = codigoProveedor;
                        cmb_IdPresupuestado.SelectedValue = idGrupoPresupuestado;
                        cmb_IdProduccion.SelectedValue = idGrupoProduccion;
                    }
                }
            }
        }
        private void AgregarValorOcultoOpcionesRoto()
        {
            try
            {
                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                using (var cmd = new SqlCommand("SELECT Nombre, DataVerId FROM Opciones WHERE left(Nombre,3) = N'RO_'", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InsertContenidoOpcionOculto(reader["Nombre"].ToString(), reader.GetGuid(1).ToString());
                        }
                    }
                    MessageBox.Show("Valor Oculto añadido correctamente.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error insertando valor oculto a las opciones: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void InsertContenidoOpcionOculto(string? optionName, string dataVerId)
        {
            if (!ExistContenidoOpcionOculto(optionName, dataVerId))
            {
                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                using (var cmd = new SqlCommand("INSERT INTO ContenidoOpciones ([Opcion], [Orden], [Valor], [Texto], [Flags]) " +
                    "                           VALUES (@nombre, @orden, @valor, @texto, @flags)", conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", optionName);
                    cmd.Parameters.AddWithValue("@orden", GetLastContenidoOpcionOrden(optionName, dataVerId));
                    cmd.Parameters.AddWithValue("@valor", "Oculto");
                    cmd.Parameters.AddWithValue("@texto", "");
                    cmd.Parameters.AddWithValue("@flags", 3);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

        }
        private int GetLastContenidoOpcionOrden(string? optionName, string dataVerId)
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT MAX(Orden) FROM ContenidoOpciones WHERE Opcion = N'" + optionName + "' AND DataVerId = N'" + dataVerId + "'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return reader.GetInt16(0) + 1;
            }
            return 1;
        }
        private bool ExistContenidoOpcionOculto(string? optionName, string dataVerId)
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Valor FROM ContenidoOpciones WHERE Opcion = N'" + optionName + "' AND DataVerId = N'" + dataVerId + "' AND Valor= N'Oculto'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return true;
            }
            return false;
        }
        private void CargarTextos()
        {
            btn_EjecutarScripts.Text = LocalizationManager.GetString("L_EjecutarSQL");
            btn_EjecutarCarpeta.Text = LocalizationManager.GetString("L_EjecutarCarpeta");
            btn_InstalarEscandallos.Text = LocalizationManager.GetString("L_InstalarEscandallos");
            btn_ShowScripts.Text = LocalizationManager.GetString("L_VerEscandallos");
            btn_ExportarEscandallos.Text = LocalizationManager.GetString("L_ExportarEscandallos");
            btn_OcultaOpciones.Text = LocalizationManager.GetString("L_OcultaOpciones");
            groupBox_Grupos.Text = LocalizationManager.GetString("L_Grupos");
            groupBox_Proveedor.Text = LocalizationManager.GetString("L_Proveedor");
            lbl_IdPresupuestado.Text = LocalizationManager.GetString("L_Presupuestado");
            lbl_IdProduccion.Text = LocalizationManager.GetString("L_Produccion");
            lbl_Proveedor.Text = LocalizationManager.GetString("L_Nombre");
            this.Text = LocalizationManager.GetString("L_Actualizador");
        }

        #endregion


    }

    public class Proveedor
    {
        public string CodigoProveedor { get; set; }
        public string Nombre { get; set; }
    }
    public class GrupoPresupuestado
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class GrupoProduccion
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
