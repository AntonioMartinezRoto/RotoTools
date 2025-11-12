using Microsoft.Data.SqlClient;
using RotoEntities;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ManillasFKSMenu : Form
    {
        #region Private Properties
        private int ConfiguracionActual { get; set; } = 1;
        #endregion

        #region Constructor
        public ManillasFKSMenu()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        private void ManillasFKSMenu_Load(object sender, EventArgs e)
        {
            InitializeInfoConnection();
            InitializeValueConfig();
            LoadItemsHardwareSupplier();
        }
        private void btn_SaveFKS_Click(object sender, EventArgs e)
        {
            int configuracionSeleccionada = rb_Normalizada.Checked ? (int)enumConfiguracionManillasFKS.Normalizada :
                                            rb_SoloFks.Checked ? (int)enumConfiguracionManillasFKS.SoloFks :
                                            rb_NormalizadaYFks.Checked ? (int)enumConfiguracionManillasFKS.NormalizadaYFks :
                                            (int)enumConfiguracionManillasFKS.Normalizada;

            if (ConfiguracionActual == configuracionSeleccionada) return;

            if (cmb_HardwareSupplier.Text == "")
            {
                MessageBox.Show("HardwareSupplier obligatorio.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;

            InstalarConfiguracion(configuracionSeleccionada);

            Cursor = Cursors.Default;
            MessageBox.Show("Instalación realizada correctamente", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

            progress_Export.Value = 0;
            progress_Export.Visible = false;
        }
        private void cmb_HardwareSupplier_SelectedValueChanged(object sender, EventArgs e)
        {
            InitializeValueConfig();
        }
        #endregion

        #region Private Methods
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void InitializeValueConfig()
        {
            try
            {
                if (cmb_HardwareSupplier.Text == "") return;

                string queryOperationsPlaca = $"SELECT Top 1 [Name], [GeneratorReference], [X], [Side], [Id], [Location], [ReferencePoint] FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{cmb_HardwareSupplier.Text.Trim()}' ORDER BY Name, GeneratorReference";

                using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
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
                        HasOptionFksNo = Helpers.OpcionAsociadaAOperacionPrefOpen(operationId, "MANILLA_FKS", "No", cmb_HardwareSupplier.Text.Trim());
                    }
                    else if (operationX == "HP+78" || operationX == "HP-138")
                    {
                        operationXIsFks = true;
                        HasOptionFksSi = Helpers.OpcionAsociadaAOperacionPrefOpen(operationId, "MANILLA_FKS", "Si", cmb_HardwareSupplier.Text.Trim());
                    }
                }

                // Configuración normalizada: las X de las operaciones son las normalizadas y NO hay opción MANILLA_FKS asociada a la operación.
                if (!operationXIsFks && !HasOptionFksNo)
                {
                    rb_Normalizada.Checked = true;
                    ConfiguracionActual = (int)enumConfiguracionManillasFKS.Normalizada;
                }

                // Configuración FKS Solo: las X de las operaciones son las de manillas FKS y NO hay opción MANILLA_FKS asociada a la operación.
                if (operationXIsFks && !HasOptionFksSi)
                {
                    rb_SoloFks.Checked = true;
                    ConfiguracionActual = (int)enumConfiguracionManillasFKS.SoloFks;
                }

                // Configuración FKS Solo: las X de las operaciones son las de manillas FKS y NO hay opción MANILLA_FKS asociada a la operación.
                if (HasOptionFksNo || HasOptionFksSi)
                {
                    rb_NormalizadaYFks.Checked = true;
                    ConfiguracionActual = (int)enumConfiguracionManillasFKS.NormalizadaYFks;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inicializando valor inicial de configuración." + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadItemsHardwareSupplier()
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand("SELECT Valor FROM ContenidoOpciones WHERE Opcion = 'HardwareSupplier' AND Valor like '%ROTO%' ORDER BY Orden", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            cmb_HardwareSupplier.Items.Clear();

            while (reader.Read())
            {
                cmb_HardwareSupplier.Items.Add(reader[0].ToString());
            }

            if (cmb_HardwareSupplier.Items.Count == 1)
            {
                cmb_HardwareSupplier.SelectedIndex = 0;
            }
        }
        private void InstalarConfiguracion(int configuracionSeleccionada)
        {
            switch (configuracionSeleccionada)
            {
                case (int)enumConfiguracionManillasFKS.Normalizada:
                    
                    if (ConfiguracionActual != (int)enumConfiguracionManillasFKS.NormalizadaYFks)
                    {
                        ActualizarOperationsXParaNormalizada();
                    }
                    else
                    {
                        DeleteFKSConfiguracion((int)enumConfiguracionManillasFKS.SoloFks);
                    }

                    break;

                case (int)enumConfiguracionManillasFKS.SoloFks:
                    
                    if(ConfiguracionActual != (int)enumConfiguracionManillasFKS.NormalizadaYFks)
                    {
                        ActualizarOperationsXParaFks();
                    }
                    else
                    {
                        DeleteFKSConfiguracion((int)enumConfiguracionManillasFKS.Normalizada);
                    }

                    break;

                case (int)enumConfiguracionManillasFKS.NormalizadaYFks:
                    
                    CrearFKSOptions();
                    CrearFKSOperations();
                    
                    break;
            }

            ConfiguracionActual = configuracionSeleccionada;
        }
        private void ActualizarOperationsXParaFks()
        {
            try
            {
                string queryOperationsPlaca = $"SELECT [Name], [GeneratorReference], [X], [Side], [Id], [Location], [ReferencePoint] FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{cmb_HardwareSupplier.Text.Trim()}' ORDER BY Name, GeneratorReference";

                using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
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

                    Helpers.EjecutarNonQuery(updateQuery);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error actualizando distancia de las operaciones." + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void ActualizarOperationsXParaNormalizada()
        {
            try
            {
                string queryOperationsPlaca = $"SELECT [Name], [GeneratorReference], [X], [Side], [Id], [Location], [ReferencePoint] FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{cmb_HardwareSupplier.Text.Trim()}' ORDER BY Name, GeneratorReference";

                using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
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

                    Helpers.EjecutarNonQuery(updateQuery);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error actualizando distancia de las operaciones." + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CrearFKSOptions()
        {
            try
            {
                if (!Helpers.ExisteOpcionEnBD("RO_MANILLA_FKS"))
                {
                    //Creación de opción en PrefWise
                    Helpers.InsertOpcion("RO_MANILLA_FKS");

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
                        Helpers.InsertContenidoOpcion("RO_MANILLA_FKS", contenidoOpcion);
                        orden++;
                    }
                }

                //Creación de la opción para uso con PrefOpen
                if (!Helpers.ExistePrefOpenOpcionEnBD(cmb_HardwareSupplier.Text, "MANILLA_FKS", "No_FKS"))
                {
                    Helpers.InsertPrefOpenOption(cmb_HardwareSupplier.Text, "MANILLA_FKS", "No_FKS");
                }
                if (!Helpers.ExistePrefOpenOpcionEnBD(cmb_HardwareSupplier.Text, "MANILLA_FKS", "Si_FKS"))
                {
                    Helpers.InsertPrefOpenOption(cmb_HardwareSupplier.Text, "MANILLA_FKS", "Si_FKS");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creando opciones para FKS." + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void CrearFKSOperations()
        {
            try
            {
                
                string queryOperationsPlaca = $"SELECT [Name], [GeneratorReference], [X], [Side], [Id], [Location], [ReferencePoint] FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{cmb_HardwareSupplier.Text.Trim()}' ORDER BY Name, GeneratorReference";
                string queryOperationsPlacaCount = $"SELECT COUNT(*) FROM [Open].Operations WHERE Name LIKE '%Placa_%' AND Name NOT LIKE '%_17_Placa_%' AND SupplierCode = '{cmb_HardwareSupplier.Text.Trim()}'";
                string supplierCode = cmb_HardwareSupplier.Text.Trim();
                using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
                conexion.Open();

                using SqlCommand cmd = new SqlCommand(queryOperationsPlaca, conexion);
                using SqlDataReader reader = cmd.ExecuteReader();

                progress_Export.Visible = true;
                int totalFilas = Helpers.EjecutarScalarCount(queryOperationsPlacaCount);
                progress_Export.Value = 0;
                progress_Export.Maximum = totalFilas > 0 ? totalFilas : 1; // Evitar división por cero


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
                    Helpers.EjecutarNonQuery(insertOperationsOptionsQuery);

                    //Copiar Operation para FKS con distinta X
                    string insertOperationsQuery = $"INSERT INTO [Open].Operations (Name, GeneratorReference, SupplierCode, X, Location) VALUES ('{operationName}', '{operationGeneratorReference}', '{supplierCode}', '{operationXNew}', '{operationLocation}')";
                    Helpers.EjecutarNonQuery(insertOperationsQuery);

                    string operationFksId = Helpers.GetPrefOpenOperationId(operationName, operationGeneratorReference, operationXNew);

                    //Agregar OperationsOptions para que las operaciones FKS solo se ejecuten con la opcion RO_MANILLA_FKS = Si
                    //Si tenía la configuración FKS Solo activa y está activando ambas, se agrega No al valor de MANILLA_FKS
                    string valueOptionManillaFks = optionManillaFksValue == "Si_FKS" ? "No_FKS" : "Si_FKS";
                    string insertOperationsOptionsFKSQuery = $"INSERT INTO [Open].OperationsOptions (OperationId, [Option], SupplierCode, Value) VALUES ('{operationFksId}', 'MANILLA_FKS', '{supplierCode}', '{valueOptionManillaFks}')";
                    Helpers.EjecutarNonQuery(insertOperationsOptionsFKSQuery);

                    //Agregar el resto de OperationsOptions que tenga la original
                    InsertOperationsOptions(operationId, operationFksId, supplierCode);

                    // Actualizar progreso
                    progress_Export.Value++;
                    progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creando operaciones para FKS." + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InsertOperationsOptions(string operationIdOrigen, string operationIdNew, string supplierCode)
        {
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand($"SELECT [Option], [Value] FROM [Open].OperationsOptions WHERE OperationId ='{operationIdOrigen}' AND [Option] <> 'MANILLA_FKS'", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string optionName = reader[0].ToString().Trim();
                string optionValue = reader[1].ToString().Trim();

                string insertOperationsOptionsFKSQuery = $"INSERT INTO [Open].OperationsOptions (OperationId, [Option], SupplierCode, Value) VALUES ('{operationIdNew}', '{optionName}', '{supplierCode}', '{optionValue}')";
                Helpers.EjecutarNonQuery(insertOperationsOptionsFKSQuery);

            }
        }
        private void DeleteFKSConfiguracion(int configuracionEliminar)
        {
            try
            {
                progress_Export.Visible = true;
                int totalFilas = 5;
                progress_Export.Value = 0;
                progress_Export.Maximum = totalFilas;

                //Borrar registros de PrefOpen.OperationsOptions
                Helpers.DeletePrefOpenOperationsOptions("MANILLA_FKS", cmb_HardwareSupplier.Text);

                progress_Export.Value++;
                progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido

                //Borrar registros de PrefOpen.Operations
                Helpers.DeletePrefOpenOperationsPlaca(configuracionEliminar, cmb_HardwareSupplier.Text);

                progress_Export.Value++;
                progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido


                //Borrar registros de PrefOpen.Options
                Helpers.DeletePrefOpenOptions("MANILLA_FKS", cmb_HardwareSupplier.Text);

                progress_Export.Value++;
                progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido

                //Borrar ContenidoOpciones 
                Helpers.DeleteAllContenidoOpciones("RO_MANILLA_FKS");

                progress_Export.Value++;
                progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido

                //Borrar Opción
                Helpers.DeleteOpcion("RO_MANILLA_FKS");

                progress_Export.Value++;
                progress_Export.Refresh(); // Fuerza el repintado si el proceso es muy rápido
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error restaurando configuración." + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        #endregion





    }
}
