using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Data.SqlClient;
using RotoEntities;
using System.Data;

namespace RotoTools
{
    public partial class ConectorHerrajeCombinar : Form
    {
        #region Constructor

        public ConectorHerrajeCombinar()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        private void CombinarConectores_Load(object sender, EventArgs e)
        {
            
            CargarTextos();
            CargarListaConectores();
            LoadItemsConectorHerraje();
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
            lbl_ConectorActivo.Text = Helpers.GetConectorActivo();
        }
        private void btn_Add_Click(object sender, EventArgs e)
        {
            if (listBox_AllConectores.SelectedItem != null)
            {
                var item = listBox_AllConectores.SelectedItem;
                listBox_Combinar.Items.Add(item);
                listBox_AllConectores.Items.Remove(item);
            }
        }
        private void btn_Add_All_Click(object sender, EventArgs e)
        {
            foreach (var item in listBox_AllConectores.Items)
            {
                listBox_Combinar.Items.Add(item);
            }
            listBox_AllConectores.Items.Clear();
        }
        private void btn_Delete_Click(object sender, EventArgs e)
        {
            if (listBox_Combinar.SelectedItem != null)
            {
                var item = listBox_Combinar.SelectedItem;
                listBox_AllConectores.Items.Add(item);
                listBox_Combinar.Items.Remove(item);
            }
        }
        private void btn_DeleteAll_Click(object sender, EventArgs e)
        {
            foreach (var item in listBox_Combinar.Items)
            {
                listBox_AllConectores.Items.Add(item);
            }
            listBox_Combinar.Items.Clear();
        }
        private void btn_up_Click(object sender, EventArgs e)
        {
            if (listBox_Combinar.SelectedItem == null) return;

            int index = listBox_Combinar.SelectedIndex;
            if (index > 0) // no subir si ya está en la primera posición
            {
                var item = listBox_Combinar.SelectedItem;

                listBox_Combinar.Items.RemoveAt(index);
                listBox_Combinar.Items.Insert(index - 1, item);

                listBox_Combinar.SelectedIndex = index - 1; // mantener selección
            }
        }
        private void btn_down_Click(object sender, EventArgs e)
        {
            if (listBox_Combinar.SelectedItem == null) return;

            int index = listBox_Combinar.SelectedIndex;
            if (index < listBox_Combinar.Items.Count - 1) // no bajar si ya está en la última posición
            {
                var item = listBox_Combinar.SelectedItem;

                listBox_Combinar.Items.RemoveAt(index);
                listBox_Combinar.Items.Insert(index + 1, item);

                listBox_Combinar.SelectedIndex = index + 1; // mantener selección
            }
        }
        private void bnt_Guardar_Click(object sender, EventArgs e)
        {
            if (rb_NuevoConector.Checked)
            {
                CrearNuevoConector();
            }
            else
            {
                InsertarEnConectorExistente();
            }

        }
        private void rb_NuevoConector_CheckedChanged(object sender, EventArgs e)
        {
            CambiarModo();
        }
        private void rb_InsertarEnConector_CheckedChanged(object sender, EventArgs e)
        {
            CambiarModo();
        }
        private void cmb_Conectores_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaConectores();
        }
        #endregion

        #region Private methods

        private void CargarListaConectores()
        {
            List<string> conectoresList = new List<string>();
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Codigo, XML FROM ConectorHerrajes", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            conectoresList.Clear();

            while (reader.Read())
            {
                conectoresList.Add(reader[0].ToString());
            }

            listBox_AllConectores.Items.Clear();

            var conectoresListFiltrados = new List<string>();

            if (cmb_Conectores.SelectedItem != null)
            {
                conectoresListFiltrados = conectoresList.Where(c => c != cmb_Conectores.SelectedItem.ToString()).ToList();
            }
            else
            {
                conectoresListFiltrados = conectoresList;
            }

            foreach (string conector in conectoresListFiltrados.OrderBy(c => c))
            {
                listBox_AllConectores.Items.Add(conector);
            }
        }
        private Connector CargarConectorDesdeBaseDeDatos(string conectorName)
        {
            try
            {
                string xmlString = null;

                using (var conexion = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conexion.Open();

                    var query = "SELECT XML FROM ConectorHerrajes WHERE Codigo = @codigo";
                    using var cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@codigo", conectorName);

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        xmlString = result.ToString();
                    }
                }

                if (!string.IsNullOrWhiteSpace(xmlString))
                {
                    return Helpers.DeserializarXML<Connector>(xmlString);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (40): " + Environment.NewLine + ex.Message);
                return null;
            }
        }
        private void GuardarConectorEnBD(string conectorName, string xml)
        {
            try
            {
                string sql = @"INSERT INTO ConectorHerrajes (DataVerId, Codigo, XML) VALUES (dbo.GetCurrentDVID(), @Codigo, @Xml);";

                if (ExisteConectorEnBD(conectorName))
                {
                    if (MessageBox.Show(LocalizationManager.GetString("L_ExisteConector"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        sql = @"UPDATE ConectorHerrajes SET XML = @Xml Where Codigo = @Codigo;";
                    }
                    else
                    {
                        return;
                    }
                }

                if (chk_Predefinido.Checked && !String.IsNullOrEmpty(conectorName))
                {
                    sql += @"UPDATE VARIABLESGLOBALES SET VALOR = '" + conectorName + "' WHERE NOMBRE = 'Conector Herraje';";
                }

                using (SqlConnection connection = new SqlConnection(Helpers.GetConnectionString()))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Codigo", conectorName);
                    command.Parameters.AddWithValue("@Xml", xml);
                    command.ExecuteNonQuery();
                }
                MessageBox.Show(LocalizationManager.GetString("L_ConectorInsertado")); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (41): " +Environment.NewLine + ex.Message);
            }
        }
        private void ActualizarConectorExistente(string conectorName, string xml)
        {
            try
            {
                string sql = @"UPDATE ConectorHerrajes SET XML = @Xml Where Codigo = @Codigo;";
                
                using (SqlConnection connection = new SqlConnection(Helpers.GetConnectionString()))
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Codigo", conectorName);
                    command.Parameters.AddWithValue("@Xml", xml);
                    command.ExecuteNonQuery();
                }
                MessageBox.Show(LocalizationManager.GetString("L_ConectorActualizado")); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (42): " + Environment.NewLine + ex.Message);
            }
        }
        private bool ExisteConectorEnBD(string conectorName)
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM ConectorHerrajes WHERE Codigo = '" + conectorName + "'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return Convert.ToInt32(reader[0].ToString()) > 0;

            }
            return false;
        }
        private void CambiarModo()
        {
            groupCrearNuevo.Enabled = rb_NuevoConector.Checked;
            groupExistente.Enabled = rb_InsertarEnConector.Checked;

            if (rb_NuevoConector.Checked)
            {
                cmb_Conectores.SelectedIndex = -1;
            }
        }
        private void LoadItemsConectorHerraje()
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Codigo, XML FROM ConectorHerrajes", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            cmb_Conectores.Items.Clear();

            while (reader.Read())
            {
                cmb_Conectores.Items.Add(reader[0].ToString());
            }
        }
        private void InsertarEnConectorExistente()
        {
            if (cmb_Conectores.SelectedIndex == -1)
            {
                MessageBox.Show(LocalizationManager.GetString("L_SeleccioneConector"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Connector conectorFinal = CargarConectorDesdeBaseDeDatos(cmb_Conectores.SelectedItem.ToString());
            if (conectorFinal != null)
            {
                foreach (string conector in listBox_Combinar.Items)
                {
                    Connector conectorFusion = CargarConectorDesdeBaseDeDatos(conector);
                    conectorFinal.Nodes.AddRange(conectorFusion.Nodes);
                }

                string xml = Helpers.SerializarXml(conectorFinal);
                ActualizarConectorExistente(cmb_Conectores.SelectedItem.ToString(), xml);
            }
        }
        private void CrearNuevoConector()
        {
            if (String.IsNullOrEmpty(txt_ConectorName.Text))
            {
                MessageBox.Show(LocalizationManager.GetString("L_AsignarNombreConector"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (listBox_Combinar.Items.Count > 0)
            {
                Connector conectorFinal = new Connector();
                conectorFinal.Message = "true";
                conectorFinal.Nodes = new List<ConnectorNode>();

                foreach (string conector in listBox_Combinar.Items)
                {
                    Connector conectorFusion = CargarConectorDesdeBaseDeDatos(conector);
                    conectorFinal.Nodes.AddRange(conectorFusion.Nodes);
                }

                string xml = Helpers.SerializarXml(conectorFinal);
                GuardarConectorEnBD(txt_ConectorName.Text,xml);
            }
        }
        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_CombinarConectores");
            lbl_NombreConector.Text = LocalizationManager.GetString("L_Nombre");
            chk_Predefinido.Text = LocalizationManager.GetString("L_PonerPredefinido");
            lbl_SeleccionarConector.Text = LocalizationManager.GetString("L_Nombre");
            lbl_ConectoresBD.Text = LocalizationManager.GetString("L_ConectoresBBDD");
            lbl_ConectoresSeleccionados.Text = LocalizationManager.GetString("L_ConectoresCombinar");
            btn_Guardar.Text = LocalizationManager.GetString("L_Guardar");
            rb_NuevoConector.Text = LocalizationManager.GetString("L_CrearConector");
            rb_InsertarEnConector.Text = LocalizationManager.GetString("L_InsertarEnConector");
        }
        #endregion
    }
}
