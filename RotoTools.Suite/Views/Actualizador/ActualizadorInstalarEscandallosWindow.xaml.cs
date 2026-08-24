using System.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using RotoEntities;
using RotoTools.Suite.Services;
using EnumTipoEscandallo = RotoTools.Enums.enumRotoTipoEscandallo;

namespace RotoTools.Suite.Views.Actualizador
{
    /// <summary>
    /// Sustituye a ActualizadorInstalarEscandallos.cs/.Designer.cs (WinForms): mismo
    /// comportamiento (6 grupos con casilla + tooltip de códigos, instalación con/sin plantilla de
    /// traducción, enlace a la instalación individualizada), reutilizando tal cual
    /// RotoTools.Helpers/RotoTools.EscandalloHelper/RotoTools.TranslateManager vía ProjectReference.
    /// </summary>
    public partial class ActualizadorInstalarEscandallosWindow : Window
    {
        public ActualizadorInstalarEscandallosWindow()
        {
            InitializeComponent();

            CargarTextos();
            SetToolTips();
        }

        #region Localización / tooltips

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_InstalarEscandallos");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_InstalarEscandallos");

            TxtGruposTitulo.Text = RotoTools.LocalizationManager.GetString("L_Grupos");
            ChkSelectAll.Content = RotoTools.LocalizationManager.GetString("L_SeleccionarTodosGrupos");
            ChkGestionGeneral.Content = RotoTools.LocalizationManager.GetString("L_GestionGeneral");
            ChkPVC.Content = RotoTools.LocalizationManager.GetString("L_ConstructivosPVC");
            ChkAlu.Content = RotoTools.LocalizationManager.GetString("L_ConstructivosALU");
            ChkManillas.Content = RotoTools.LocalizationManager.GetString("L_GestionManillas");
            ChkBombillos.Content = RotoTools.LocalizationManager.GetString("L_GestionBombillos");
            ChkCustomizations.Content = RotoTools.LocalizationManager.GetString("L_PersonalizacionClientes");

            TxtInstalacionIndividualizadaTitulo.Text = RotoTools.LocalizationManager.GetString("L_InstalacionIndividualizada");
            TxtInstalacionIndividualizadaDesc.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarEscandallos");

            TxtBtnInstalarEscandallos.Text = RotoTools.LocalizationManager.GetString("L_Instalar");
        }

        /// <summary>Idéntico a SetToolTips/GenerarTooltip del original: cada casilla muestra, como
        /// tooltip, la lista de códigos de escandallo que instalaría.</summary>
        private void SetToolTips()
        {
            var tiposSeleccionados = new List<EnumTipoEscandallo>
            {
                EnumTipoEscandallo.PVC, EnumTipoEscandallo.Aluminio, EnumTipoEscandallo.GestionGeneral,
                EnumTipoEscandallo.GestionManillas, EnumTipoEscandallo.GestionBombillos, EnumTipoEscandallo.PersonalizacionClientes
            };

            List<Escandallo> escandallosList = RotoTools.Helpers.CargarEscandallosEmbebidos(tiposSeleccionados);

            ChkPVC.ToolTip = GenerarTooltip(escandallosList, EnumTipoEscandallo.PVC);
            ChkAlu.ToolTip = GenerarTooltip(escandallosList, EnumTipoEscandallo.Aluminio);
            ChkGestionGeneral.ToolTip = GenerarTooltip(escandallosList, EnumTipoEscandallo.GestionGeneral);
            ChkManillas.ToolTip = GenerarTooltip(escandallosList, EnumTipoEscandallo.GestionManillas);
            ChkBombillos.ToolTip = GenerarTooltip(escandallosList, EnumTipoEscandallo.GestionBombillos);
            ChkCustomizations.ToolTip = GenerarTooltip(escandallosList, EnumTipoEscandallo.PersonalizacionClientes);
        }

        private static string GenerarTooltip(List<Escandallo> escandallos, EnumTipoEscandallo tipo)
        {
            var nombres = escandallos.Where(e => e.RotoTipo == tipo).Select(e => e.Codigo);
            return string.Join("\n", nombres);
        }

        private void EnableControls(bool enable)
        {
            BtnInstalarEscandallos.IsEnabled = enable;
            ChkGestionGeneral.IsEnabled = enable;
            ChkPVC.IsEnabled = enable;
            ChkAlu.IsEnabled = enable;
            ChkManillas.IsEnabled = enable;
            ChkBombillos.IsEnabled = enable;
            ChkCustomizations.IsEnabled = enable;
        }

        #endregion

        #region Events

        private void ChkSelectAll_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool marcado = ChkSelectAll.IsChecked == true;
            ChkGestionGeneral.IsChecked = marcado;
            ChkPVC.IsChecked = marcado;
            ChkAlu.IsChecked = marcado;
            ChkManillas.IsChecked = marcado;
            ChkBombillos.IsChecked = marcado;
            ChkCustomizations.IsChecked = marcado;
        }

        private void BtnFiltrarEscandallos_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ActualizadorInstalarManualEscandallosWindow { Owner = this };
            ventana.ShowDialog();
        }

        private void BtnInstalarEscandallos_Click(object sender, RoutedEventArgs e)
        {
            bool algunoMarcado = ChkAlu.IsChecked == true || ChkPVC.IsChecked == true || ChkGestionGeneral.IsChecked == true ||
                                  ChkManillas.IsChecked == true || ChkBombillos.IsChecked == true || ChkCustomizations.IsChecked == true;
            if (!algunoMarcado)
                return;

            if (MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ConfirmarInstalar"), "",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var tiposSeleccionados = new List<EnumTipoEscandallo>();
            if (ChkPVC.IsChecked == true) tiposSeleccionados.Add(EnumTipoEscandallo.PVC);
            if (ChkAlu.IsChecked == true) tiposSeleccionados.Add(EnumTipoEscandallo.Aluminio);
            if (ChkGestionGeneral.IsChecked == true) tiposSeleccionados.Add(EnumTipoEscandallo.GestionGeneral);
            if (ChkManillas.IsChecked == true) tiposSeleccionados.Add(EnumTipoEscandallo.GestionManillas);
            if (ChkBombillos.IsChecked == true) tiposSeleccionados.Add(EnumTipoEscandallo.GestionBombillos);
            if (ChkCustomizations.IsChecked == true) tiposSeleccionados.Add(EnumTipoEscandallo.PersonalizacionClientes);

            RotoTools.Helpers.InstalarOpcionConfiguraciónStandard();
            InstallEscandallos(tiposSeleccionados);
        }

        #endregion

        #region Instalación (InstallEscandallos, idéntico al original)

        private void InstallEscandallos(List<EnumTipoEscandallo> tipoEscandallosSeleccionados)
        {
            try
            {
                RotoTools.TranslateManager.AplicarTraduccion = false;

                if (RotoTools.TranslateManager.PermitirTraduccionesEnConectorEscandallos)
                {
                    if (MessageBox.Show(RotoTools.LocalizationManager.GetString("L_AplicarPlantillaTraduccion"), "",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var openFileDialog = new OpenFileDialog { Filter = "XLS Files (*.xls)|*.xlsx" };
                        if (openFileDialog.ShowDialog() == true)
                        {
                            EnableControls(false);
                            RotoTools.TranslateManager.AplicarTraduccion = true;
                            RotoTools.TranslateManager.TraduccionesActuales = RotoTools.Helpers.CargarTraducciones(openFileDialog.FileName);
                            EnableControls(true);
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                EnableControls(false);
                string messageEscandallos = RotoTools.LocalizationManager.GetString("L_EscandallosInstalados") + Environment.NewLine + Environment.NewLine;

                using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                {
                    conn.Open();

                    List<Escandallo> escandallosList = RotoTools.Helpers.CargarEscandallosEmbebidos(tipoEscandallosSeleccionados);

                    foreach (var escandallo in escandallosList)
                    {
                        RotoTools.EscandalloHelper.AplicarTraduccion(escandallo);

                        if (RotoTools.Helpers.ExisteEscandalloEnBD(escandallo.Codigo))
                        {
                            const string queryInstall = @"UPDATE Escandallos SET Programa=@Programa WHERE Codigo=@Codigo";
                            using var cmd = new SqlCommand(queryInstall, conn);
                            cmd.Parameters.AddWithValue("@Codigo", escandallo.Codigo);
                            cmd.Parameters.AddWithValue("@Programa", (object?)escandallo.Programa ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            const string insert = @"INSERT INTO Escandallos
                                        (Codigo, [Type], Descripcion, Nivel1, Nivel2, Nivel3, Nivel4, Nivel5, Variables, Programa, Texto, Familia)
                                        VALUES (@Codigo, @Type, @Descripcion, @Nivel1, @Nivel2, @Nivel3, @Nivel4, @Nivel5, @Variables, @Programa, @Texto, @Familia)";

                            using var cmd = new SqlCommand(insert, conn);
                            cmd.Parameters.AddWithValue("@Codigo", escandallo.Codigo);
                            cmd.Parameters.AddWithValue("@Type", escandallo.Type);
                            cmd.Parameters.AddWithValue("@Descripcion", (object?)escandallo.Descripcion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel1", (object?)escandallo.Nivel1 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel2", (object?)escandallo.Nivel2 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel3", (object?)escandallo.Nivel3 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel4", (object?)escandallo.Nivel4 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Nivel5", (object?)escandallo.Nivel5 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Variables", (object?)escandallo.Variables ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Programa", (object?)escandallo.Programa ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Texto", (object?)escandallo.Texto ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Familia", (object?)escandallo.Familia ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }

                        messageEscandallos += escandallo.Codigo + Environment.NewLine;
                    }
                }

                EnableControls(true);
                MessageBox.Show(messageEscandallos, "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error(36): " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
