using Microsoft.Data.SqlClient;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RotoTools
{
    public partial class TariffsImporterAddTariff : Form
    {
        #region Private Properties

        public List<Moneda> _monedasList = new List<Moneda>();

        #endregion

        #region Constructors
        public TariffsImporterAddTariff()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        private void TariffsImporterAddTariff_Load(object sender, EventArgs e)
        {
            this.AcceptButton = btn_Aceptar;
            CargarMonedas();
            CargarTextos();
        }
        private void btn_Aceptar_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txt_TariffName.Text))
            {
                MessageBox.Show(LocalizationManager.GetString("L_NombreTarifaObligatorio"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Helpers.ExisteTariffEnBD(txt_TariffName.Text, 0))
            {
                MessageBox.Show(LocalizationManager.GetString("L_TarifaExistente"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Helpers.ExisteTariffEnBD(txt_TariffName.Text, 0))
            {
                MessageBox.Show(LocalizationManager.GetString("L_DivisaObligatoria"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CrearTarifa();
        }
        #endregion

        #region Private Methods
        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_CrearTarifa");
            lbl_Tarifa.Text = LocalizationManager.GetString("L_Tarifa");
            lbl_Monedas.Text = LocalizationManager.GetString("L_MonedaAsociada");
            btn_Aceptar.Text = LocalizationManager.GetString("L_Guardar");
        }
        private void CargarMonedas()
        {
            _monedasList.Clear();

            using (var conn = new SqlConnection(Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT Nombre, ISO4217, Simbolo, Relacion, Decimales FROM Monedas ORDER BY Nombre", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _monedasList.Add(new Moneda
                        {
                            Nombre = reader["Nombre"].ToString().Trim(),
                            ISO4217 = reader["ISO4217"].ToString().Trim(),
                            Simbolo = reader["Simbolo"].ToString().Trim(),
                            Relacion = Convert.ToDouble(reader["Relacion"]),
                            Decimales = Convert.ToInt32(reader["Decimales"])
                        });
                    }
                }
            }

            cmb_Monedas.DataSource = null;
            cmb_Monedas.DataSource = _monedasList;
            cmb_Monedas.DisplayMember = "Nombre";
            cmb_Monedas.ValueMember = "Nombre";

            SeleccionarMonedaPorDefecto();
        }
        private void SeleccionarMonedaPorDefecto()
        {
            if (_monedasList == null || !_monedasList.Any())
                return;

            string monedaDefecto = Helpers.GetDivisaPorDefecto();
            cmb_Monedas.SelectedValue = monedaDefecto;
        }
        private void CrearTarifa()
        {
            try
            {
                Helpers.InsertTariff(txt_TariffName.Text, cmb_Monedas.SelectedValue?.ToString());
                Helpers.UpdateTariffOrder(txt_TariffName.Text);

                Helpers.InsertTariffAlLargo(txt_TariffName.Text, cmb_Monedas.SelectedValue?.ToString());
                Helpers.UpdateTariffOrderAlLargo(txt_TariffName.Text);

                MessageBox.Show(LocalizationManager.GetString("L_TarifaCreada"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error(41):\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

    }
}
