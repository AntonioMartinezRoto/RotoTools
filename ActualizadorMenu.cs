using Microsoft.Data.SqlClient;

namespace RotoTools
{
    public partial class ActualizadorMenu : Form
    {
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
        }
        private void btn_EjecutarScripts_Click(object sender, EventArgs e)
        {
            try
            {
                EjecutarScripts();
                MessageBox.Show("Scripts ejecutados correctamente", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error ejecutando los scripts:" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void cmb_Proveedor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_Proveedor.SelectedItem is Proveedor prov)
            {
                txt_Proveedor.Text = prov.CodigoProveedor;
            }
        }
        private void cmb_IdPresupuestado_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_IdPresupuestado.SelectedItem is GrupoPresupuestado grupoPresupuestado)
            {
                txt_Presupuestado.Text = grupoPresupuestado.Id;
            }
        }
        private void cmb_IdProduccion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_IdProduccion.SelectedItem is GrupoProduccion grupoProduccion)
            {
                txt_Produccion.Text = grupoProduccion.Id;
            }
        }
        private void btn_EjecutarCarpeta_Click(object sender, EventArgs e)
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

                    int totalEjecutados = 0;
                    foreach (string fichero in ficheros)
                    {
                        string script = File.ReadAllText(fichero);

                        if (!string.IsNullOrWhiteSpace(script))
                        {
                            int filas = Helpers.EjecutarNonQuery(script);
                            totalEjecutados++;
                        }
                    }

                    MessageBox.Show($"Se ejecutaron {totalEjecutados} script(s).");
                }
            }
        }
        private void btn_OcultaOpciones_Click(object sender, EventArgs e)
        {
            AgregarValorOcultoOpcionesRoto();
        }

        #endregion

        #region Private methods

        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void EjecutarScripts()
        {
            //Actualizar grupos de Presupuestado y Produccion y Proveedor Nivel1 = ROTO NX
            Helpers.UpdateGruposYProveedor(txt_Presupuestado.Text, txt_Produccion.Text, txt_Proveedor.Text);

            //Actualizar Nivel1 Opciones y MaterialesBase de ROTO NX a ROTO
            Helpers.EjecutarNonQuery(queryUpdateNivel1OpcionesMaterialesBase);

            //Actualizar propiedades MaterialesBase ficticios
            Helpers.UpdateMaterialesBaseFicticiosPropiedades(materialesFicticios);

            //Actualizar descripciones MaterialesBase desde los fittings
            Helpers.EjecutarNonQuery(queryUpdateDescripciones);
        }
        private void CargarProveedores()
        {
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
            using (var cmd = new SqlCommand("SELECT CodigoProveedor, IdGrupoPresupuestado, IdGrupoProduccion FROM MATERIALESBASE WHERE REFERENCIABASE = 'RO_260272'", conn))
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

            using SqlCommand cmd = new SqlCommand("SELECT Valor FROM ContenidoOpciones WHERE Opcion = N'" +  optionName +"' AND DataVerId = N'" + dataVerId + "' AND Valor= N'Oculto'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {                
                return true;
            }
            return false;
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
