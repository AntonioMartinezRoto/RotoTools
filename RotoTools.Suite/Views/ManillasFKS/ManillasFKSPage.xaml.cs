using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using RotoEntities;
using RotoTools.Suite.Services;
using EnumConfigManillasFKS = RotoTools.Enums.enumConfiguracionManillasFKS;

namespace RotoTools.Suite.Views.ManillasFKS
{
    /// <summary>
    /// Sustituye a ManillasFKSMenu.cs/.Designer.cs (WinForms): mismo comportamiento y misma
    /// lógica de negocio (reutilizada tal cual vía RotoTools.Helpers/RotoTools.LocalizationManager
    /// por ProjectReference). Las consultas SQL que en el original vivían directamente en el
    /// code-behind del formulario se han portado aquí letra por letra, en vez de intentar moverlas
    /// a RotoTools.csproj, que no se debe tocar bajo ningún concepto.
    /// </summary>
    public partial class ManillasFKSPage : UserControl
    {
        #region Estado

        private int _configuracionActual = (int)EnumConfigManillasFKS.Normalizada;

        private string HardwareSupplierSeleccionado => CmbHardwareSupplier.SelectedItem as string ?? "";

        #endregion

        public ManillasFKSPage()
        {
            InitializeComponent();

            CargarTextos();
            CargarDatos();
        }

        #region Localización / carga inicial

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_ConfManillasFKS");
            TxtSubtitulo.Text = Loc("L_Suite_ManillasFksSubtitulo");

            TxtSeleccionarConfiguracion.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarConfiguracion");
            TxtRbNormalizada.Text = RotoTools.LocalizationManager.GetString("L_ConfiguracionNormalizada");
            TxtRbSoloFks.Text = RotoTools.LocalizationManager.GetString("L_ConfiguracionFKS");
            TxtRbNormalizadaYFks.Text = RotoTools.LocalizationManager.GetString("L_ConfiguracionNormalizadaFKS");

            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
        }

        /// <summary>Igual que ManillasFKSMenu_Load, salvo InitializeInfoConnection: el
        /// servidor/base de datos ya se muestra siempre en la cabecera de MainWindow, así que
        /// repetirlo aquí sería información duplicada (mismo criterio ya aplicado en
        /// ActualizadorPage/ConectorHerrajePage). InitializeValueConfig no hace falta llamarlo
        /// aquí de forma explícita: LoadItemsHardwareSupplier ya dispara
        /// CmbHardwareSupplier_SelectionChanged (que a su vez llama a InitializeValueConfig) en
        /// cuanto hay un único proveedor y se selecciona automáticamente; si hay varios, el
        /// original tampoco evaluaba la configuración actual hasta que el usuario elegía uno.</summary>
        private void CargarDatos()
        {
            LoadItemsHardwareSupplier();
        }

        #endregion

        #region Events

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            int configuracionSeleccionada = RbNormalizada.IsChecked == true ? (int)EnumConfigManillasFKS.Normalizada :
                                            RbSoloFks.IsChecked == true ? (int)EnumConfigManillasFKS.SoloFks :
                                            RbNormalizadaYFks.IsChecked == true ? (int)EnumConfigManillasFKS.NormalizadaYFks :
                                            (int)EnumConfigManillasFKS.Normalizada;

            if (_configuracionActual == configuracionSeleccionada) return;

            if (HardwareSupplierSeleccionado == "")
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_HardwareSupplierObligatorio"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                InstalarConfiguracion(configuracionSeleccionada);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            MessageBox.Show(RotoTools.LocalizationManager.GetString("L_InstalacionCompletada"), "", MessageBoxButton.OK, MessageBoxImage.Information);

            ProgressExport.Value = 0;
            ProgressExport.Visibility = Visibility.Collapsed;
        }

        private void CmbHardwareSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitializeValueConfig();
        }

        #endregion

        #region Private Methods

        private void InitializeValueConfig()
        {
            try
            {
                if (HardwareSupplierSeleccionado == "") return;

                string queryOperationsPlaca = $"SELECT Top 1 [Name], [GeneratorReference], [X], [Side], [Id], [Location], [ReferencePoint] FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{HardwareSupplierSeleccionado.Trim()}' ORDER BY Name, GeneratorReference";

                using SqlConnection conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
                conexion.Open();

                using SqlCommand cmd = new SqlCommand(queryOperationsPlaca, conexion);
                using SqlDataReader reader = cmd.ExecuteReader();

                bool HasOptionFksNo = false;
                bool HasOptionFksSi = false;
                bool operationXIsFks = false;

                while (reader.Read())
                {
                    string operationCurrentName = reader[0].ToString().Trim();
                    string operationCurrentGeneratorReference = reader[1].ToString().Trim();
                    string operationX = reader[2].ToString().Trim();
                    string operationId = reader[4].ToString().Trim();

                    if (operationX == "HP+70" || operationX == "HP-130")
                    {
                        operationXIsFks = false;
                        HasOptionFksNo = RotoTools.Helpers.OpcionAsociadaAOperacionPrefOpen(operationId, "MANILLA_FKS", "No_FKS", HardwareSupplierSeleccionado.Trim());
                    }
                    else if (operationX == "HP+78" || operationX == "HP-138")
                    {
                        operationXIsFks = true;
                        HasOptionFksSi = RotoTools.Helpers.OpcionAsociadaAOperacionPrefOpen(operationId, "MANILLA_FKS", "Si_FKS", HardwareSupplierSeleccionado.Trim());
                    }
                }

                // Configuración normalizada: las X de las operaciones son las normalizadas y NO hay opción MANILLA_FKS asociada a la operación.
                if (!operationXIsFks && !HasOptionFksNo)
                {
                    RbNormalizada.IsChecked = true;
                    _configuracionActual = (int)EnumConfigManillasFKS.Normalizada;
                }

                // Configuración FKS Solo: las X de las operaciones son las de manillas FKS y NO hay opción MANILLA_FKS asociada a la operación.
                if (operationXIsFks && !HasOptionFksSi)
                {
                    RbSoloFks.IsChecked = true;
                    _configuracionActual = (int)EnumConfigManillasFKS.SoloFks;
                }

                // Configuración FKS Solo: las X de las operaciones son las de manillas FKS y NO hay opción MANILLA_FKS asociada a la operación.
                if (HasOptionFksNo || HasOptionFksSi)
                {
                    RbNormalizadaYFks.IsChecked = true;
                    _configuracionActual = (int)EnumConfigManillasFKS.NormalizadaYFks;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (8)" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadItemsHardwareSupplier()
        {
            using SqlConnection conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Valor FROM ContenidoOpciones WHERE Opcion = 'HardwareSupplier' AND Valor like '%ROTO%' ORDER BY Orden", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            var items = new List<string>();

            while (reader.Read())
            {
                items.Add(reader[0].ToString());
            }

            CmbHardwareSupplier.ItemsSource = items;

            if (items.Count == 1)
            {
                CmbHardwareSupplier.SelectedIndex = 0;
            }
        }

        private void InstalarConfiguracion(int configuracionSeleccionada)
        {
            switch (configuracionSeleccionada)
            {
                case (int)EnumConfigManillasFKS.Normalizada:

                    if (_configuracionActual != (int)EnumConfigManillasFKS.NormalizadaYFks)
                    {
                        ActualizarOperationsXParaNormalizada();
                    }
                    else
                    {
                        DeleteFKSConfiguracion((int)EnumConfigManillasFKS.SoloFks);
                    }

                    break;

                case (int)EnumConfigManillasFKS.SoloFks:

                    if (_configuracionActual != (int)EnumConfigManillasFKS.NormalizadaYFks)
                    {
                        ActualizarOperationsXParaFks();
                    }
                    else
                    {
                        DeleteFKSConfiguracion((int)EnumConfigManillasFKS.Normalizada);
                    }

                    break;

                case (int)EnumConfigManillasFKS.NormalizadaYFks:

                    CrearFKSOptions();
                    CrearFKSOperations();

                    break;
            }

            _configuracionActual = configuracionSeleccionada;
        }

        private void ActualizarOperationsXParaFks()
        {
            try
            {
                string queryOperationsPlaca = $"SELECT [Name], [GeneratorReference], [X], [Side], [Id], [Location], [ReferencePoint] FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{HardwareSupplierSeleccionado.Trim()}' ORDER BY Name, GeneratorReference";

                using SqlConnection conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
                conexion.Open();

                using SqlCommand cmd = new SqlCommand(queryOperationsPlaca, conexion);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string operationX = reader[2].ToString();
                    string operationId = reader[4].ToString();
                    string updateQuery = $"UPDATE [Open].Operations SET X='@x' WHERE Id='{operationId}'";

                    if (operationX == "HP+70")
                    {
                        updateQuery = updateQuery.Replace("@x", "HP+78");
                    }
                    else if (operationX == "HP-130")
                    {
                        updateQuery = updateQuery.Replace("@x", "HP-138");
                    }

                    RotoTools.Helpers.EjecutarNonQuery(updateQuery);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (9)" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ActualizarOperationsXParaNormalizada()
        {
            try
            {
                string queryOperationsPlaca = $"SELECT [Name], [GeneratorReference], [X], [Side], [Id], [Location], [ReferencePoint] FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{HardwareSupplierSeleccionado.Trim()}' ORDER BY Name, GeneratorReference";

                using SqlConnection conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
                conexion.Open();

                using SqlCommand cmd = new SqlCommand(queryOperationsPlaca, conexion);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string operationX = reader[2].ToString();
                    string operationId = reader[4].ToString();
                    string updateQuery = $"UPDATE [Open].Operations SET X='@x' WHERE Id='{operationId}'";

                    if (operationX == "HP+78")
                    {
                        updateQuery = updateQuery.Replace("@x", "HP+70");
                    }
                    else if (operationX == "HP-138")
                    {
                        updateQuery = updateQuery.Replace("@x", "HP-130");
                    }

                    RotoTools.Helpers.EjecutarNonQuery(updateQuery);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (10)" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CrearFKSOptions()
        {
            try
            {
                if (!RotoTools.Helpers.ExisteOpcionEnBD("RO_MANILLA_FKS"))
                {
                    //Creación de opción en PrefWise
                    RotoTools.Helpers.InsertOpcion("RO_MANILLA_FKS");

                    //Creación de los valores de ContenidoOpcion de PrefWise
                    List<string> contenidoOpcionesConfiguracionStandard =
                       [
                           "No_FKS",
                           "Si_FKS"
                       ];
                    int orden = 0;
                    foreach (string contenidoOpcionValor in contenidoOpcionesConfiguracionStandard)
                    {
                        ContenidoOpcion contenidoOpcion = new ContenidoOpcion("RO_MANILLA_FKS", contenidoOpcionValor, "", "0", orden.ToString(), "0", "");
                        RotoTools.Helpers.InsertContenidoOpcion("RO_MANILLA_FKS", contenidoOpcion);
                        orden++;
                    }
                }

                //Creación de la opción para uso con PrefOpen
                if (!RotoTools.Helpers.ExistePrefOpenOpcionEnBD(HardwareSupplierSeleccionado, "MANILLA_FKS", "No_FKS"))
                {
                    RotoTools.Helpers.InsertPrefOpenOption(HardwareSupplierSeleccionado, "MANILLA_FKS", "No_FKS");
                }
                if (!RotoTools.Helpers.ExistePrefOpenOpcionEnBD(HardwareSupplierSeleccionado, "MANILLA_FKS", "Si_FKS"))
                {
                    RotoTools.Helpers.InsertPrefOpenOption(HardwareSupplierSeleccionado, "MANILLA_FKS", "Si_FKS");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (11)" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CrearFKSOperations()
        {
            try
            {

                string queryOperationsPlaca = $"SELECT [Name], [GeneratorReference], [X], [Side], [Id], [Location], [ReferencePoint] FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{HardwareSupplierSeleccionado.Trim()}' ORDER BY Name, GeneratorReference";
                string queryOperationsPlacaCount = $"SELECT COUNT(*) FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{HardwareSupplierSeleccionado.Trim()}'";
                string supplierCode = HardwareSupplierSeleccionado.Trim();
                using SqlConnection conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
                conexion.Open();

                using SqlCommand cmd = new SqlCommand(queryOperationsPlaca, conexion);
                using SqlDataReader reader = cmd.ExecuteReader();

                ProgressExport.Visibility = Visibility.Visible;
                int totalFilas = RotoTools.Helpers.EjecutarScalarCount(queryOperationsPlacaCount);
                ProgressExport.Value = 0;
                ProgressExport.Maximum = totalFilas > 0 ? totalFilas : 1; // Evitar división por cero


                while (reader.Read())
                {
                    string operationName = reader[0].ToString().Trim();
                    string operationGeneratorReference = reader[1].ToString().Trim();
                    string operationX = reader[2].ToString().Trim();
                    string operationSide = reader[3].ToString().Trim();
                    string operationId = reader[4].ToString().Trim();
                    string operationLocation = reader[5].ToString().Trim();
                    string operationReferencePoint = reader[6].ToString().Trim();

                    string operationXNew = "";
                    string optionManillaFksValue = "No_FKS";

                    if (operationX == "HP+70")
                    {
                        operationXNew = "HP+78";
                        optionManillaFksValue = "No_FKS";
                    }
                    else if (operationX == "HP+78")
                    {
                        operationXNew = "HP+70";
                        optionManillaFksValue = "Si_FKS";
                    }
                    else if (operationX == "HP-130")
                    {
                        operationXNew = "HP-138";
                        optionManillaFksValue = "No_FKS";
                    }
                    else if (operationX == "HP-138")
                    {
                        operationXNew = "HP-130";
                        optionManillaFksValue = "Si_FKS";
                    }

                    //Agregar OperationsOptions para que las operaciones normalizadas solo se ejecuten con la opcion MANILLA_FKS = No
                    //Si tenía la configuración FKS Solo activa y está activando ambas, se agrega Sí al valor de MANILLA_FKS
                    string insertOperationsOptionsQuery = $"INSERT INTO [Open].OperationsOptions (OperationId, [Option], SupplierCode, Value) VALUES ('{operationId}', 'MANILLA_FKS', '{supplierCode}', '{optionManillaFksValue}')";
                    RotoTools.Helpers.EjecutarNonQuery(insertOperationsOptionsQuery);

                    //Copiar Operation para FKS con distinta X
                    string insertOperationsQuery = $"INSERT INTO [Open].Operations (Name, GeneratorReference, SupplierCode, X, Location) VALUES ('{operationName}', '{operationGeneratorReference}', '{supplierCode}', '{operationXNew}', '{operationLocation}')";
                    RotoTools.Helpers.EjecutarNonQuery(insertOperationsQuery);

                    string operationFksId = RotoTools.Helpers.GetPrefOpenOperationId(operationName, operationGeneratorReference, operationXNew);

                    //Agregar OperationsOptions para que las operaciones FKS solo se ejecuten con la opcion RO_MANILLA_FKS = Si
                    //Si tenía la configuración FKS Solo activa y está activando ambas, se agrega No al valor de MANILLA_FKS
                    string valueOptionManillaFks = optionManillaFksValue == "Si_FKS" ? "No_FKS" : "Si_FKS";
                    string insertOperationsOptionsFKSQuery = $"INSERT INTO [Open].OperationsOptions (OperationId, [Option], SupplierCode, Value) VALUES ('{operationFksId}', 'MANILLA_FKS', '{supplierCode}', '{valueOptionManillaFks}')";
                    RotoTools.Helpers.EjecutarNonQuery(insertOperationsOptionsFKSQuery);

                    //Agregar el resto de OperationsOptions que tenga la original
                    InsertOperationsOptions(operationId, operationFksId, supplierCode);

                    // Actualizar progreso
                    ProgressExport.Value++;
                    DoEvents(); // Fuerza el repintado si el proceso es muy rápido

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (12)" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertOperationsOptions(string operationIdOrigen, string operationIdNew, string supplierCode)
        {
            using SqlConnection conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT [Option], [Value] FROM [Open].OperationsOptions WHERE OperationId ='{operationIdOrigen}' AND [Option] <> 'MANILLA_FKS'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string optionName = reader[0].ToString().Trim();
                string optionValue = reader[1].ToString().Trim();

                string insertOperationsOptionsFKSQuery = $"INSERT INTO [Open].OperationsOptions (OperationId, [Option], SupplierCode, Value) VALUES ('{operationIdNew}', '{optionName}', '{supplierCode}', '{optionValue}')";
                RotoTools.Helpers.EjecutarNonQuery(insertOperationsOptionsFKSQuery);

            }
        }

        private void DeleteFKSConfiguracion(int configuracionEliminar)
        {
            try
            {
                ProgressExport.Visibility = Visibility.Visible;
                int totalFilas = 5;
                ProgressExport.Value = 0;
                ProgressExport.Maximum = totalFilas;

                //Borrar registros de PrefOpen.OperationsOptions
                RotoTools.Helpers.DeletePrefOpenOperationsOptions("MANILLA_FKS", HardwareSupplierSeleccionado);

                ProgressExport.Value++;
                DoEvents(); // Fuerza el repintado si el proceso es muy rápido

                //Borrar registros de PrefOpen.Operations
                RotoTools.Helpers.DeletePrefOpenOperationsPlaca(configuracionEliminar, HardwareSupplierSeleccionado);

                ProgressExport.Value++;
                DoEvents(); // Fuerza el repintado si el proceso es muy rápido


                //Borrar registros de PrefOpen.Options
                RotoTools.Helpers.DeletePrefOpenOptions("MANILLA_FKS", HardwareSupplierSeleccionado);

                ProgressExport.Value++;
                DoEvents(); // Fuerza el repintado si el proceso es muy rápido

                //Borrar ContenidoOpciones
                RotoTools.Helpers.DeleteAllContenidoOpciones("RO_MANILLA_FKS");

                ProgressExport.Value++;
                DoEvents(); // Fuerza el repintado si el proceso es muy rápido

                //Borrar Opción
                RotoTools.Helpers.DeleteOpcion("RO_MANILLA_FKS");

                ProgressExport.Value++;
                DoEvents(); // Fuerza el repintado si el proceso es muy rápido
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (12)" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>Equivalente WPF de Application.DoEvents() (mismo helper que
        /// ConectorHerrajePage): bombea el bucle de mensajes para que la ProgressBar se repinte
        /// durante el bucle síncrono, igual que hacía progress_Export.Refresh() en la app WinForms
        /// original.</summary>
        private static void DoEvents()
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new System.Windows.Threading.DispatcherOperationCallback(f =>
                {
                    ((System.Windows.Threading.DispatcherFrame)f!).Continue = false;
                    return null;
                }), frame);
            System.Windows.Threading.Dispatcher.PushFrame(frame);
        }

        #endregion
    }
}
