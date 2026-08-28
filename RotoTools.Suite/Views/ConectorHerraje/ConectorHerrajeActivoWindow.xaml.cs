using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Data.SqlClient;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.ConectorHerraje
{
    /// <summary>
    /// Nueva (no existía en el original): permite elegir directamente cualquiera de los
    /// conectores ya guardados en la tabla ConectorHerrajes como el conector activo (fila
    /// "Conector Herraje" en VariablesGlobales, la misma que lee/escribe
    /// RotoTools.Helpers.GetConectorActivo y que ConectorHerrajeCombinarWindow/
    /// ConectorHerrajeGeneradorWindow actualizan de rebote al marcar "Poner como predefinido").
    /// Ver el comentario grande en el XAML.
    /// </summary>
    public partial class ConectorHerrajeActivoWindow : Window
    {
        public ConectorHerrajeActivoWindow()
        {
            InitializeComponent();

            CargarTextos();
            CargarConectores();
        }

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            Title = Loc("L_Suite_CambiarConectorActivo");
            TxtTitulo.Text = Title;

            LblConector.Text = RotoTools.LocalizationManager.GetString("L_Nombre");
            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
        }

        /// <summary>Mismo origen (tabla ConectorHerrajes, columna Codigo) que
        /// CargarConectoresExistentesCombo en ConectorHerrajeCombinarWindow, pero ordenado
        /// alfabéticamente (allí no lo estaba) porque aquí es la única lista que se muestra, sin
        /// ningún filtro adicional que ya la organice.</summary>
        private void CargarConectores()
        {
            var conectores = new List<string>();
            using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT Codigo FROM ConectorHerrajes ORDER BY Codigo", conexion))
            {
                conexion.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    conectores.Add(reader[0].ToString());
            }

            CmbConectores.ItemsSource = conectores;

            string conectorActivo = RotoTools.Helpers.GetConectorActivo() ?? "";
            LblConectorActivoActual.Text = Loc("L_Suite_ConectorActivo") + ": " +
                (string.IsNullOrEmpty(conectorActivo) ? "-" : conectorActivo);

            // Igual criterio que SeleccionarMonedaPorDefecto en TariffImporterAddTariffWindow: si
            // el conector activo actual está en la lista, se preselecciona, para que el usuario
            // vea de entrada de dónde parte en vez de un combo vacío.
            if (conectores.Contains(conectorActivo))
                CmbConectores.SelectedItem = conectorActivo;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbConectores.SelectedItem is not string conectorSeleccionado || string.IsNullOrEmpty(conectorSeleccionado))
            {
                MessageBox.Show(Loc("L_Suite_SeleccionaConectorObligatorio"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                using (var cmd = new SqlCommand("UPDATE VariablesGlobales SET Valor = @valor WHERE Nombre = 'Conector Herraje'", conexion))
                {
                    cmd.Parameters.AddWithValue("@valor", conectorSeleccionado);
                    conexion.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(Loc("L_Suite_OperacionCompletada"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error (42):" + System.Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
