using Microsoft.Data.SqlClient;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ActualizadorInstalarEscandallos : Form
    {
        #region Private properties
        private bool SeleccionIndividualizadaActiva { get; set; } = false;
        #endregion

        #region Constructors
        public ActualizadorInstalarEscandallos()
        {
            InitializeComponent();
        }
        #endregion

        #region Private methods
        private void InstallEscandallos(List<enumRotoTipoEscandallo> tipoEscandallosSeleccionados)
        {
            try
            {
                TranslateManager.AplicarTraduccion = false;

                if (!LocalizationManager.CurrentCulture.Equals(new CultureInfo("es")))
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
                string messageEscandallos = "Escandallos instalados: " + Environment.NewLine + Environment.NewLine;
                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();

                    //Cargar la lista de escandallos seleccionados
                    List<Escandallo> escandallosList = new List<Escandallo>();
                    escandallosList = Helpers.CargarEscandallosEmbebidos(tipoEscandallosSeleccionados);


                    // Insertar los del proyecto
                    foreach (var escandallo in escandallosList)
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

                EnableControls(true);
                MessageBox.Show(messageEscandallos, "Instalación completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error instalando escandallos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void EnableControls(bool enable)
        {
            btn_InstalarEscandallos.Enabled = enable;
            chk_GestionGeneral.Enabled = enable;
            chk_PVC.Enabled = enable;
            chk_Alu.Enabled = enable;
            chk_Manillas.Enabled = enable;
            chk_Bombillos.Enabled = enable;
            chk_Customizations.Enabled = enable;
        }
        
        private void SetToolTips()
        {
            List<enumRotoTipoEscandallo> tiposSeleccionados = new();

            tiposSeleccionados.Add(enumRotoTipoEscandallo.PVC);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.Aluminio);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionGeneral);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionManillas);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionBombillos);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.PersonalizacionClientes);

            List<Escandallo> escandallosList = Helpers.CargarEscandallosEmbebidos(tiposSeleccionados);

            toolTipEscandallos.SetToolTip(chk_PVC, GenerarTooltip(escandallosList, enumRotoTipoEscandallo.PVC));
            toolTipEscandallos.SetToolTip(chk_Alu, GenerarTooltip(escandallosList, enumRotoTipoEscandallo.Aluminio));
            toolTipEscandallos.SetToolTip(chk_GestionGeneral, GenerarTooltip(escandallosList, enumRotoTipoEscandallo.GestionGeneral));
            toolTipEscandallos.SetToolTip(chk_Manillas, GenerarTooltip(escandallosList, enumRotoTipoEscandallo.GestionManillas));
            toolTipEscandallos.SetToolTip(chk_Bombillos, GenerarTooltip(escandallosList, enumRotoTipoEscandallo.GestionBombillos));
            toolTipEscandallos.SetToolTip(chk_Customizations, GenerarTooltip(escandallosList, enumRotoTipoEscandallo.PersonalizacionClientes));
        }
        private string GenerarTooltip(List<Escandallo> escandallos, enumRotoTipoEscandallo tipo)
        {
            var nombres = escandallos
                .Where(e => e.RotoTipo == tipo)
                .Select(e => e.Codigo);

            return string.Join("\n", nombres);
        }
        #endregion

        #region Events
        private void btn_InstalarEscandallos_Click(object sender, EventArgs e)
        {
            if (chk_Alu.Checked || chk_PVC.Checked || chk_GestionGeneral.Checked || chk_Manillas.Checked || chk_Bombillos.Checked || chk_Customizations.Checked)
            {
                if (MessageBox.Show("Al instalar se perderán los datos actuales de los escandallos seleccionados. ¿Desea continuar?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    List<enumRotoTipoEscandallo> tiposSeleccionados = new();

                    if (chk_PVC.Checked) tiposSeleccionados.Add(enumRotoTipoEscandallo.PVC);
                    if (chk_Alu.Checked) tiposSeleccionados.Add(enumRotoTipoEscandallo.Aluminio);
                    if (chk_GestionGeneral.Checked) tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionGeneral);
                    if (chk_Manillas.Checked) tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionManillas);
                    if (chk_Bombillos.Checked) tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionBombillos);
                    if (chk_Customizations.Checked) tiposSeleccionados.Add(enumRotoTipoEscandallo.PersonalizacionClientes);

                    InstallEscandallos(tiposSeleccionados);
                }
                else
                {
                    return;
                }
            }
        }
        private void ActualizadorInstalarEscandallos_Load(object sender, EventArgs e)
        {
            SetToolTips();
        }
        private void chk_SelectAll_CheckedChanged(object sender, EventArgs e)
        {
            chk_GestionGeneral.Checked = chk_SelectAll.Checked;
            chk_PVC.Checked = chk_SelectAll.Checked;
            chk_Alu.Checked = chk_SelectAll.Checked;
            chk_Manillas.Checked = chk_SelectAll.Checked;
            chk_Bombillos.Checked = chk_SelectAll.Checked;
            chk_Customizations.Checked = chk_SelectAll.Checked;
        }
        private void btn_FiltrarEscandallos_Click(object sender, EventArgs e)
        {
            ActualizadorInstalarManualEscandallos actualizadorInstalarManualEscandallosForm = new ActualizadorInstalarManualEscandallos();
            actualizadorInstalarManualEscandallosForm.ShowDialog();
        }
        #endregion

    }
}
