using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Data.SqlClient;
using RotoEntities;

namespace RotoTools.Suite.Views.TariffImporter
{
    /// <summary>
    /// Sustituye a TariffsImporterAddTariff.cs/.Designer.cs (WinForms): mismo comportamiento y
    /// misma lógica de negocio, reutilizada tal cual vía ProjectReference. Se mantiene tal cual la
    /// doble comprobación de "Helpers.ExisteTariffEnBD(txt_TariffName.Text, 0)" del original (la
    /// segunda, pensada para validar que hay una moneda seleccionada, repite por error la misma
    /// condición que la primera en vez de comprobar cmb_Monedas/CmbMonedas): no se corrige aquí,
    /// solo se traslada la lógica de negocio tal cual, sin modificarla.
    /// </summary>
    public partial class TariffImporterAddTariffWindow : Window
    {
        private List<Moneda> _monedasList = new();

        public TariffImporterAddTariffWindow()
        {
            InitializeComponent();

            CargarTextos();
            CargarMonedas();
        }

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_CrearTarifa");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_CrearTarifa");
            LblTarifa.Text = RotoTools.LocalizationManager.GetString("L_Tarifa");
            LblMonedas.Text = RotoTools.LocalizationManager.GetString("L_MonedaAsociada");
            TxtBtnAceptar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
        }

        private void CargarMonedas()
        {
            _monedasList.Clear();

            using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
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
                            Relacion = System.Convert.ToDouble(reader["Relacion"]),
                            Decimales = System.Convert.ToInt32(reader["Decimales"])
                        });
                    }
                }
            }

            CmbMonedas.ItemsSource = null;
            CmbMonedas.ItemsSource = _monedasList;

            SeleccionarMonedaPorDefecto();
        }

        private void SeleccionarMonedaPorDefecto()
        {
            if (_monedasList == null || !_monedasList.Any())
                return;

            string? monedaDefecto = RotoTools.Helpers.GetDivisaPorDefecto();
            CmbMonedas.SelectedValue = monedaDefecto;
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtTariffName.Text))
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_NombreTarifaObligatorio"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (RotoTools.Helpers.ExisteTariffEnBD(TxtTariffName.Text, 0))
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_TarifaExistente"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (RotoTools.Helpers.ExisteTariffEnBD(TxtTariffName.Text, 0))
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_DivisaObligatoria"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CrearTarifa();
        }

        private void CrearTarifa()
        {
            try
            {
                RotoTools.Helpers.InsertTariff(TxtTariffName.Text, CmbMonedas.SelectedValue?.ToString());
                RotoTools.Helpers.UpdateTariffOrder(TxtTariffName.Text);

                RotoTools.Helpers.InsertTariffAlLargo(TxtTariffName.Text, CmbMonedas.SelectedValue?.ToString());
                RotoTools.Helpers.UpdateTariffOrderAlLargo(TxtTariffName.Text);

                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_TarifaCreada"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error(41):\n" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
