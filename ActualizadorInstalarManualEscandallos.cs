

using Microsoft.Data.SqlClient;
using RotoEntities;
using System.Globalization;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ActualizadorInstalarManualEscandallos : Form
    {
        #region Private properties

        private List<Escandallo> EscandalloList = new List<Escandallo>();

        #endregion

        #region Constructor

        public ActualizadorInstalarManualEscandallos()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void ActualizadorInstalarManualEscandallos_Load(object sender, EventArgs e)
        {
            LoadAllEscandallos();
            LoadEscandallosInList("");
            CargarTextos();
        }
        private void txt_filter_TextChanged(object sender, EventArgs e)
        {
            chk_SelectAll.Checked = false;
            LoadEscandallosInList(txt_filter.Text);
        }
        private void chk_SelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < chkList_Escandallos.Items.Count; i++)
            {
                chkList_Escandallos.SetItemChecked(i, chk_SelectAll.Checked);

            }
        }
        private void btn_InstalarEscandallos_Click(object sender, EventArgs e)
        {
            if (chkList_Escandallos.CheckedItems.Count == 0) return;

            if (MessageBox.Show(LocalizationManager.GetString("L_ConfirmarInstalar"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Helpers.InstalarOpcionConfiguraciónStandard();
                InstallEscandallos();
            }
            else
            {
                return;
            }
        }
        #endregion

        #region Private methods
        private void LoadAllEscandallos()
        {
            List<enumRotoTipoEscandallo> tiposSeleccionados = new();

            tiposSeleccionados.Add(enumRotoTipoEscandallo.PVC);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.Aluminio);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionGeneral);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionManillas);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionBombillos);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.PersonalizacionClientes);

            this.EscandalloList = Helpers.CargarEscandallosEmbebidos(tiposSeleccionados);
        }
        private void LoadEscandallosInList(String filter)
        {
            chkList_Escandallos.Items.Clear();

            List<Escandallo> escandallosListMostrar = new List<Escandallo>();

            if (String.IsNullOrEmpty(filter))
            {
                escandallosListMostrar.AddRange(this.EscandalloList);
            }
            else
            {
                escandallosListMostrar = this.EscandalloList.Where(e => e.Codigo.ToUpper().Contains(filter.ToUpper())).ToList();
            }

            foreach (Escandallo escandallo in escandallosListMostrar)
            {
                chkList_Escandallos.Items.Add(escandallo);
            }
            chkList_Escandallos.DisplayMember = "Codigo"; // Muestra el código del Set
        }
        private void EnableControls(bool enabled)
        {
            btn_InstalarEscandallos.Enabled = enabled;
            chkList_Escandallos.Enabled = enabled;
            txt_filter.Enabled = enabled;
            chk_SelectAll.Enabled = enabled;
        }
        private void InstallEscandallos()
        {
            try
            {
                TranslateManager.AplicarTraduccion = false;

                if (TranslateManager.PermitirTraduccionesEnConectorEscandallos)
                {
                    if (MessageBox.Show(LocalizationManager.GetString("L_AplicarPlantillaTraduccion"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "XLS Files (*.xls)|*.xlsx";

                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            EnableControls(false);

                            TranslateManager.AplicarTraduccion = true;
                            TranslateManager.TraduccionesActuales = Helpers.CargarTraducciones(openFileDialog.FileName);

                            EnableControls(true);
                            Cursor.Current = Cursors.Default;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                Cursor = Cursors.WaitCursor;
                EnableControls(false);
                string messageEscandallos = LocalizationManager.GetString("L_EscandallosInstalados") + Environment.NewLine + Environment.NewLine;
                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();

                    foreach (var itemListChecked in chkList_Escandallos.CheckedItems)
                    {
                        Escandallo escandallo = itemListChecked as Escandallo;
                        if (escandallo != null)
                        {
                            EscandalloHelper.AplicarTraduccion(escandallo);

                            string queryInstall = "";
                            if (Helpers.ExisteEscandalloEnBD(escandallo.Codigo))
                            {
                                queryInstall = @"UPDATE Escandallos SET Programa=@Programa WHERE Codigo=@Codigo";
                                using (var cmd = new SqlCommand(queryInstall, conn))
                                {
                                    cmd.Parameters.AddWithValue("@Codigo", escandallo.Codigo);
                                    cmd.Parameters.AddWithValue("@Programa", (object)escandallo.Programa ?? DBNull.Value);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                string insert = @"INSERT INTO Escandallos 
                                        (Codigo, [Type], Descripcion, Nivel1, Nivel2, Nivel3, Nivel4, Nivel5, Variables, Programa, Texto, Familia)
                                        VALUES (@Codigo, @Type, @Descripcion, @Nivel1, @Nivel2, @Nivel3, @Nivel4, @Nivel5, @Variables, @Programa, @Texto, @Familia)";

                                using (var cmd = new SqlCommand(insert, conn))
                                {
                                    cmd.Parameters.AddWithValue("@Codigo", escandallo.Codigo);
                                    cmd.Parameters.AddWithValue("@Type", escandallo.Type);
                                    cmd.Parameters.AddWithValue("@Descripcion", (object)escandallo.Descripcion ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Nivel1", (object)escandallo.Nivel1 ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Nivel2", (object)escandallo.Nivel2 ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Nivel3", (object)escandallo.Nivel3 ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Nivel4", (object)escandallo.Nivel4 ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Nivel5", (object)escandallo.Nivel5 ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Variables", (object)escandallo.Variables ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Programa", (object)escandallo.Programa ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Texto", (object)escandallo.Texto ?? DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Familia", (object)escandallo.Familia ?? DBNull.Value);

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            messageEscandallos += escandallo.Codigo + Environment.NewLine;
                        }
                        
                    }
                }

                EnableControls(true);
                MessageBox.Show(messageEscandallos, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (6): " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void CargarTextos()
        {
            chk_SelectAll.Text = LocalizationManager.GetString("L_SeleccionarTodos");
            lbl_Buscar.Text = LocalizationManager.GetString("L_Buscar");
            btn_InstalarEscandallos.Text = LocalizationManager.GetString("L_Instalar");
            this.Text = LocalizationManager.GetString("L_InstalacionIndividualizada");
        }
        #endregion

    }
}
