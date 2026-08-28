using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using RotoEntities;
using RotoTools.Suite.Services;
using EnumHardware = RotoTools.Enums.enumHardwareType;

namespace RotoTools.Suite.Views.Actualizador
{
    /// <summary>
    /// Sustituye a ActualizadorMenu.cs/.Designer.cs (WinForms): mismo comportamiento y misma
    /// lógica de negocio, reutilizada tal cual vía ProjectReference. Las consultas SQL que en el
    /// original vivían directamente en el code-behind del formulario (no en RotoTools.Helpers, que
    /// solo expone lo reutilizable desde varios sitios) se han portado aquí letra por letra, en vez
    /// de intentar moverlas a RotoTools.csproj, que no se debe tocar bajo ningún concepto.
    /// Las clases auxiliares Proveedor/GrupoPresupuestado/GrupoProduccion sí son públicas en el
    /// proyecto original (declaradas en ActualizadorMenu.cs, fuera de la clase del formulario), así
    /// que se reutilizan tal cual (RotoTools.Proveedor, etc.) en vez de duplicarlas aquí.
    /// </summary>
    public partial class ActualizadorPage : UserControl
    {
        #region Const (idénticas a ActualizadorMenu.cs)

        private const string referenciaValorPorDefecto = "RO_260272";

        private const string queryUpdateNivel1OpcionesMaterialesBase =
            "UPDATE MaterialesBase SET Nivel1='ROTO' WHERE Nivel1 = 'ROTO NX' OR Nivel1 = 'ROTO NX ALU' OR Nivel1 = 'ROTO NX PAX'; " +
            "UPDATE Opciones SET Nivel1 = 'ROTO' WHERE Nivel1 = 'ROTO NX' OR Nivel1 = 'ROTO NX ALU' OR Nivel1 = 'ROTO NX PAX' ;";

        private const string queryUpdateDescripciones = @"
                            UPDATE MB
                            SET MB.DESCRIPCION = F.DESCRIPTION
                            FROM MaterialesBase MB
                            INNER JOIN [OPEN].Fittings F
                                ON SUBSTRING(MB.ReferenciaBase, 4, LEN(MB.ReferenciaBase) - 3) = F.Reference
                            WHERE MB.ReferenciaBase LIKE 'RO\_%' ESCAPE '\'";

        private readonly string[] materialesFicticios = { "RO_PROGRAM%", "RO_MEC%" };

        private const string queryUpdateSustituirPor =
            "UPDATE MATERIALESBASE SET SustituirPor = LEFT(ReferenciaBase, CHARINDEX('-', [ReferenciaBase]) - 1) WHERE ReferenciaBase LIKE 'RO_%-%'";

        #endregion

        #region Estado

        private List<RotoTools.Proveedor> _proveedoresList = new();
        private List<RotoTools.GrupoPresupuestado> _gruposPresupuestadoList = new();
        private List<RotoTools.GrupoProduccion> _gruposProduccionList = new();

        #endregion

        public ActualizadorPage()
        {
            InitializeComponent();

            CargarTextos();
            CargarDatos();
        }

        #region Localización / carga inicial

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_Actualizador");
            TxtSubtitulo.Text = Loc("L_Suite_ActualizadorSubtitulo");

            TxtCard1Titulo.Text = RotoTools.LocalizationManager.GetString("L_InstalarEscandallos");
            TxtCard1Desc.Text = Loc("L_Suite_InstalarEscandallosDesc");
            TxtCard2Titulo.Text = RotoTools.LocalizationManager.GetString("L_VerEscandallos");
            TxtCard2Desc.Text = Loc("L_Suite_VerEscandallosDesc");
            TxtCard3Titulo.Text = RotoTools.LocalizationManager.GetString("L_ExportarEscandallos");
            TxtCard3Desc.Text = Loc("L_Suite_ExportarEscandallosDesc");

            TxtGruposTitulo.Text = RotoTools.LocalizationManager.GetString("L_Grupos");
            LblPresupuestado.Text = RotoTools.LocalizationManager.GetString("L_Presupuestado");
            LblProduccion.Text = RotoTools.LocalizationManager.GetString("L_Produccion");

            TxtProveedorTitulo.Text = RotoTools.LocalizationManager.GetString("L_Proveedor");
            LblProveedor.Text = RotoTools.LocalizationManager.GetString("L_Nombre");
            BtnAddProveedor.ToolTip = Loc("L_Suite_AnadirProveedorTooltip");

            TxtBtnEjecutarScripts.Text = RotoTools.LocalizationManager.GetString("L_EjecutarSQL");
            TxtBtnEjecutarCarpeta.Text = RotoTools.LocalizationManager.GetString("L_EjecutarCarpeta");
            TxtBtnOcultaOpciones.Text = RotoTools.LocalizationManager.GetString("L_OcultaOpciones");

            TxtInfoActualizacionTitulo.Text = RotoTools.LocalizationManager.GetString("L_InfoActualizacion");
            BtnRefreshPVC.ToolTip = Loc("L_Suite_InfoActualizacionTooltip");
            BtnRefreshALU.ToolTip = Loc("L_Suite_InfoActualizacionTooltip");
            BtnRefreshPAX.ToolTip = Loc("L_Suite_InfoActualizacionTooltip");
            BtnClearPVC.ToolTip = Loc("L_Suite_LimpiarActualizacionTooltip");
            BtnClearALU.ToolTip = Loc("L_Suite_LimpiarActualizacionTooltip");
            BtnClearPAX.ToolTip = Loc("L_Suite_LimpiarActualizacionTooltip");
            LblXmlPVC.Text = RotoTools.LocalizationManager.GetString("L_XML") + ":";
            LblFechaPVC.Text = RotoTools.LocalizationManager.GetString("L_Fecha") + ":";
            LblXmlALU.Text = RotoTools.LocalizationManager.GetString("L_XML") + ":";
            LblFechaALU.Text = RotoTools.LocalizationManager.GetString("L_Fecha") + ":";
            LblXmlPAX.Text = RotoTools.LocalizationManager.GetString("L_XML") + ":";
            LblFechaPAX.Text = RotoTools.LocalizationManager.GetString("L_Fecha") + ":";
        }

        /// <summary>Igual que ActualizadorMenu_Load, salvo InitializeInfoConnection: el
        /// servidor/base de datos ya se muestra siempre en la cabecera de MainWindow, así que
        /// repetirlo aquí sería información duplicada (no hacía falta ninguna comprobación de
        /// compatibilidad de versión en el módulo original, a diferencia de Conector de Herraje).
        /// InitializeRotoInfo (antes en ActualizadorInfoWindow, ver más abajo) también se carga
        /// aquí, ya que ahora la información PVC/Aluminio/PAX vive directamente en esta página.</summary>
        private void CargarDatos()
        {
            CargarProveedores();
            CargarGruposPresupuestado();
            CargarGruposProduccion();
            AsignarValoresPorDefecto();
            InitializeRotoInfo();
        }

        private void EnableControls(bool enable)
        {
            BtnEjecutarCarpeta.IsEnabled = enable;
            BtnEjecutarScripts.IsEnabled = enable;
            BtnOcultaOpciones.IsEnabled = enable;
            BtnInstalarEscandallos.IsEnabled = enable;
            BtnExportarEscandallos.IsEnabled = enable;
            CmbPresupuestado.IsEnabled = enable;
            CmbProduccion.IsEnabled = enable;
            CmbProveedor.IsEnabled = enable;
        }

        #endregion

        #region Grupos / Proveedor (CargarProveedores / CargarGruposPresupuestado / CargarGruposProduccion / AsignarValoresPorDefecto)

        private void CargarProveedores()
        {
            _proveedoresList.Clear();
            using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT CodigoProveedor, Nombre FROM Proveedores ORDER BY Nombre", conn))
            {
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _proveedoresList.Add(new RotoTools.Proveedor
                    {
                        CodigoProveedor = reader["CodigoProveedor"].ToString(),
                        Nombre = reader["Nombre"].ToString()
                    });
                }
            }

            CmbProveedor.ItemsSource = null;
            CmbProveedor.ItemsSource = _proveedoresList;
        }

        private void CargarGruposPresupuestado()
        {
            _gruposPresupuestadoList.Clear();
            using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT GroupId, GroupName FROM Groups WHERE GroupType = 2 ORDER BY GroupName", conn))
            {
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _gruposPresupuestadoList.Add(new RotoTools.GrupoPresupuestado
                    {
                        Id = reader["GroupId"].ToString(),
                        Name = reader["GroupName"].ToString()
                    });
                }
            }

            CmbPresupuestado.ItemsSource = _gruposPresupuestadoList;
        }

        private void CargarGruposProduccion()
        {
            _gruposProduccionList.Clear();
            using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT GroupId, GroupName FROM Groups WHERE GroupType = 3 ORDER BY GroupName", conn))
            {
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _gruposProduccionList.Add(new RotoTools.GrupoProduccion
                    {
                        Id = reader["GroupId"].ToString(),
                        Name = reader["GroupName"].ToString()
                    });
                }
            }

            CmbProduccion.ItemsSource = _gruposProduccionList;
        }

        private void AsignarValoresPorDefecto()
        {
            using var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            using var cmd = new SqlCommand(
                "SELECT CodigoProveedor, IdGrupoPresupuestado, IdGrupoProduccion FROM MATERIALESBASE WHERE REFERENCIABASE = '" + referenciaValorPorDefecto + "'", conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CmbProveedor.SelectedValue = reader["CodigoProveedor"].ToString();
                CmbPresupuestado.SelectedValue = reader["IdGrupoPresupuestado"].ToString();
                CmbProduccion.SelectedValue = reader["IdGrupoProduccion"].ToString();
            }
        }

        private void CmbProveedor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtProveedor.Text = CmbProveedor.SelectedItem is RotoTools.Proveedor prov ? prov.CodigoProveedor : "";
        }

        private void CmbPresupuestado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtPresupuestado.Text = CmbPresupuestado.SelectedItem is RotoTools.GrupoPresupuestado g ? g.Id : "";
        }

        private void CmbProduccion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtProduccion.Text = CmbProduccion.SelectedItem is RotoTools.GrupoProduccion g ? g.Id : "";
        }

        private void BtnAddProveedor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ExisteProveedorRotoEnBD())
                {
                    if (MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ExisteProveedor"), "",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                        return;
                }

                AgregarProveedorRotoFrankSA();
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ProveedorAgregado"), "", MessageBoxButton.OK, MessageBoxImage.Information);

                CargarProveedores();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (4)" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ExisteProveedorRotoEnBD()
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand("SELECT Count(*) FROM Proveedores WHERE Nombre LIKE '%ROTO%'", conexion);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                return Convert.ToInt32(reader[0].ToString()) > 0;
            return false;
        }

        private void AgregarProveedorRotoFrankSA()
        {
            string insertProveedorRoto = "INSERT INTO Proveedores (CodigoProveedor, Nombre) VALUES (" + GetNuevoCodigoProveedor() + ", 'Roto Frank SA')";
            RotoTools.Helpers.EjecutarNonQuery(insertProveedorRoto);
        }

        private int GetNuevoCodigoProveedor()
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand("SELECT ISNULL(MAX(CodigoProveedor),0) FROM Proveedores", conexion);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                return Convert.ToInt32(reader[0].ToString()) + 1;
            return 1;
        }

        #endregion

        #region Ejecutar Scripts (btn_EjecutarScripts / EjecutarScripts)

        private void BtnEjecutarScripts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnableControls(false);
                ResultQuerys resultQuerys = EjecutarScripts();

                string mensaje =
                    RotoTools.LocalizationManager.GetString("L_ScriptsEjecutados") + Environment.NewLine + Environment.NewLine +
                    RotoTools.LocalizationManager.GetString("L_GroupsSupplier") + ": " + resultQuerys.ResultQueryUpdateGruposYProveedor + " " + RotoTools.LocalizationManager.GetString("L_RegistrosActualizados") + Environment.NewLine + Environment.NewLine +
                    RotoTools.LocalizationManager.GetString("L_Level1MBOpciones") + ": " + resultQuerys.ResultQueryUpdateNivel1MaterialesBaseYOpciones + " " + RotoTools.LocalizationManager.GetString("L_RegistrosActualizados") + Environment.NewLine + Environment.NewLine +
                    RotoTools.LocalizationManager.GetString("L_MBFicticios") + ": " + resultQuerys.ResultQueryUpdatePropFicticios + " " + RotoTools.LocalizationManager.GetString("L_RegistrosActualizados") + Environment.NewLine + Environment.NewLine +
                    RotoTools.LocalizationManager.GetString("L_DescripcionesMB") + ": " + resultQuerys.ResultQueryUpdateDescripcionesMateriales + " " + RotoTools.LocalizationManager.GetString("L_RegistrosActualizados") + Environment.NewLine + Environment.NewLine +
                    RotoTools.LocalizationManager.GetString("L_SustituirPor") + ": " + resultQuerys.ResultQuerySustituirPor + " " + RotoTools.LocalizationManager.GetString("L_RegistrosActualizados") + Environment.NewLine;

                MessageBox.Show(mensaje, "", MessageBoxButton.OK, MessageBoxImage.Information);
                EnableControls(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (1): " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                EnableControls(true);
            }
        }

        private ResultQuerys EjecutarScripts()
        {
            // Actualizar grupos de Presupuestado y Producción y Proveedor Nivel1 = ROTO NX
            int rowsAfected = RotoTools.Helpers.UpdateGruposYProveedor(TxtPresupuestado.Text, TxtProduccion.Text, TxtProveedor.Text);

            // Actualizar Nivel1 Opciones y MaterialesBase de ROTO NX a ROTO
            int rowsAfected2 = RotoTools.Helpers.EjecutarNonQuery(queryUpdateNivel1OpcionesMaterialesBase);

            // Actualizar propiedades MaterialesBase ficticios
            int rowsAfected3 = RotoTools.Helpers.UpdateMaterialesBaseFicticiosPropiedades(materialesFicticios);

            // Actualizar descripciones MaterialesBase desde los fittings
            int rowsAfected4 = RotoTools.Helpers.EjecutarNonQuery(queryUpdateDescripciones);

            // Actualizar Sustituir por referencias ficticias
            int rowsAfected5 = RotoTools.Helpers.EjecutarNonQuery(queryUpdateSustituirPor);

            return new ResultQuerys(rowsAfected, rowsAfected2, rowsAfected3, rowsAfected4, rowsAfected5);
        }

        #endregion

        #region Elegir Scripts (btn_EjecutarCarpeta)

        private void BtnEjecutarCarpeta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Microsoft.Win32.OpenFolderDialog (.NET 8 WPF) en vez del FolderBrowserDialog de
                // WinForms del original, para seguir el mismo criterio que el resto de diálogos de
                // fichero/carpeta ya migrados en la suite (ver ConectorHerrajePage.xaml.cs).
                var dialog = new OpenFolderDialog();
                if (dialog.ShowDialog() != true)
                    return;

                string carpeta = dialog.FolderName;
                string[] ficheros = Directory.GetFiles(carpeta, "*.sql");

                if (ficheros.Length == 0)
                {
                    MessageBox.Show(RotoTools.LocalizationManager.GetString("L_NoScriptsSql"));
                    return;
                }

                EnableControls(false);

                string message = "";
                foreach (string fichero in ficheros)
                {
                    string script = File.ReadAllText(fichero);
                    if (!string.IsNullOrWhiteSpace(script))
                    {
                        int rowsAfected = RotoTools.Helpers.EjecutarNonQuery(script);
                        message += rowsAfected + " " + RotoTools.LocalizationManager.GetString("L_RegistrosActualizadosScript") + " " + fichero + Environment.NewLine + Environment.NewLine;
                    }
                }

                MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Information);
                EnableControls(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (2): " + Environment.NewLine + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                EnableControls(true);
            }
        }

        #endregion

        #region Ocultar Opciones (btn_OcultaOpciones / AgregarValorOcultoOpcionesRoto)

        private void BtnOcultaOpciones_Click(object sender, RoutedEventArgs e)
        {
            AgregarValorOcultoOpcionesRoto();
        }

        private void AgregarValorOcultoOpcionesRoto()
        {
            try
            {
                using var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString());
                using var cmd = new SqlCommand("SELECT Nombre, DataVerId FROM Opciones WHERE left(Nombre,3) = N'RO_'", conn);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        InsertContenidoOpcionOculto(reader["Nombre"].ToString(), reader.GetGuid(1).ToString());
                }

                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_OcultoAgregado"), "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (5): " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InsertContenidoOpcionOculto(string? optionName, string dataVerId)
        {
            if (ExistContenidoOpcionOculto(optionName, dataVerId))
                return;

            using var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            using var cmd = new SqlCommand(
                "INSERT INTO ContenidoOpciones ([Opcion], [Orden], [Valor], [Texto], [Flags]) " +
                "                           VALUES (@nombre, @orden, @valor, @texto, @flags)", conn);
            cmd.Parameters.AddWithValue("@nombre", optionName);
            cmd.Parameters.AddWithValue("@orden", GetLastContenidoOpcionOrden(optionName, dataVerId));
            cmd.Parameters.AddWithValue("@valor", "Oculto");
            cmd.Parameters.AddWithValue("@texto", "");
            cmd.Parameters.AddWithValue("@flags", 3);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        private int GetLastContenidoOpcionOrden(string? optionName, string dataVerId)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand("SELECT MAX(Orden) FROM ContenidoOpciones WHERE Opcion = N'" + optionName + "' AND DataVerId = N'" + dataVerId + "'", conexion);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                return reader.GetInt16(0) + 1;
            return 1;
        }

        private bool ExistContenidoOpcionOculto(string? optionName, string dataVerId)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand("SELECT Valor FROM ContenidoOpciones WHERE Opcion = N'" + optionName + "' AND DataVerId = N'" + dataVerId + "' AND Valor= N'Oculto'", conexion);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                return true;
            return false;
        }

        #endregion

        #region Exportar Escandallos (btn_ExportarEscandallos)

        private void BtnExportarEscandallos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFolderDialog();
                if (dialog.ShowDialog() != true)
                    return;

                string carpeta = dialog.FolderName;
                EnableControls(false);

                var escandallos = new List<Escandallo>();
                using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT * FROM Escandallos WHERE CODIGO LIKE 'RO\_%' ESCAPE '\'";
                    using var cmd = new SqlCommand(query, conn);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var escandallo = new Escandallo
                        {
                            Codigo = reader["Codigo"].ToString().Trim(),
                            Type = Convert.ToInt16(reader["Type"]),
                            Descripcion = reader["Descripcion"] as string,
                            Nivel1 = reader["Nivel1"] as string,
                            Nivel2 = reader["Nivel2"] as string,
                            Nivel3 = reader["Nivel3"] as string,
                            Nivel4 = reader["Nivel4"] as string,
                            Nivel5 = reader["Nivel5"] as string,
                            Variables = reader["Variables"] as string,
                            Programa = reader["Programa"] as string,
                            Texto = reader["Texto"] as string,
                            Familia = reader["Familia"] as string,
                            XMLTable = reader["XMLTable"] as string,
                            ProductionType = reader.GetGuid(reader.GetOrdinal("ProductionType")),
                            PrefShopStatus = Convert.ToInt16(reader["PrefShopStatus"])
                        };

                        RotoTools.Helpers.InicializarEscandalloRotoTipo(escandallo);
                        escandallos.Add(escandallo);
                    }
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                foreach (var escandallo in escandallos)
                {
                    string fileName = $"{escandallo.Codigo.Trim()}.json";
                    string path = Path.Combine(carpeta, fileName);
                    File.WriteAllText(path, JsonSerializer.Serialize(escandallo, options));
                }

                MessageBox.Show(escandallos.Count + " " + RotoTools.LocalizationManager.GetString("L_Escandallos") + ": " + Environment.NewLine + carpeta, "", MessageBoxButton.OK, MessageBoxImage.Information);

                EnableControls(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error(3)" + Environment.NewLine + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                EnableControls(true);
            }
        }

        #endregion

        #region Ventanas hijas (Instalar / Ver Escandallos)

        /// <summary>Público (no solo se llama desde el propio botón): es también el acceso directo
        /// "Instalar Escandallos" de la portada Inicio (ver BtnAccesoActualizador_Click en
        /// DashboardPage.xaml.cs y MainWindow.IrAModulo), que navega a este módulo y ejecuta esta
        /// misma acción tal cual, sin duplicar su lógica.</summary>
        public void BtnInstalarEscandallos_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ActualizadorInstalarEscandallosWindow { Owner = Window.GetWindow(this) };
            ventana.ShowDialog();
        }

        private void BtnVerEscandallos_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ActualizadorVerEscandallosWindow { Owner = Window.GetWindow(this) };
            ventana.ShowDialog();
        }

        #endregion

        #region Información de la última actualización (antes ActualizadorInfoWindow)

        /// <summary>Igual que InitializeRotoInfo del antiguo ActualizadorInfoWindow: relee y
        /// muestra el XML/fecha actuales de los 3 tipos de herraje (se llama también tras cada
        /// Refresh, porque el original refresca los 3 aunque solo se haya cambiado uno).</summary>
        private void InitializeRotoInfo()
        {
            LblPVCFile.Text = RotoTools.Helpers.GetNombreXMLActualizacionRoto(EnumHardware.PVC);
            LblPVCData.Text = RotoTools.Helpers.GetFechaActualizacionRoto(EnumHardware.PVC);

            LblALUFile.Text = RotoTools.Helpers.GetNombreXMLActualizacionRoto(EnumHardware.Aluminio);
            LblALUData.Text = RotoTools.Helpers.GetFechaActualizacionRoto(EnumHardware.Aluminio);

            LblPAXFile.Text = RotoTools.Helpers.GetNombreXMLActualizacionRoto(EnumHardware.PAX);
            LblPAXData.Text = RotoTools.Helpers.GetFechaActualizacionRoto(EnumHardware.PAX);
        }

        private void SeleccionarYActualizarXml(EnumHardware tipo)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = Loc("L_Suite_SeleccionaXml")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                RotoTools.Helpers.SetNombreXMLRoto(tipo, openFileDialog.SafeFileName);
                RotoTools.Helpers.SetFechaActualizacionRoto(tipo, DateTime.Now);
            }

            InitializeRotoInfo();
        }

        private void BtnRefreshPVC_Click(object sender, RoutedEventArgs e) => SeleccionarYActualizarXml(EnumHardware.PVC);

        private void BtnRefreshALU_Click(object sender, RoutedEventArgs e) => SeleccionarYActualizarXml(EnumHardware.Aluminio);

        private void BtnRefreshPAX_Click(object sender, RoutedEventArgs e) => SeleccionarYActualizarXml(EnumHardware.PAX);

        /// <summary>
        /// Nuevo (no existía en el original): borra de VariablesGlobales el registro de la última
        /// actualización de un herraje, es decir las 2 filas que
        /// GetNombreXMLActualizacionRoto/GetFechaActualizacionRoto leen y
        /// SetNombreXMLRoto/SetFechaActualizacionRoto escriben (RotoXmlNombrePVC/ALU/PAX +
        /// RotoFechaActualizacionPVC/ALU/PAX, ver Helpers.cs). RotoTools.Helpers no expone ningún
        /// método para borrar esas filas (solo leer/upsert), y no se puede añadir uno allí sin
        /// modificar RotoTools.csproj, así que el DELETE vive aquí, con el mismo mapeo
        /// tipo→nombre de variable que usan esos 4 métodos del original (letra por letra) y
        /// reutilizando RotoTools.Helpers.EjecutarNonQuery para ejecutarlo, igual que ya hace esta
        /// misma página en AgregarProveedorRotoFrankSA. Los nombres de variable son constantes
        /// fijas del propio código (no entrada del usuario), así que interpolarlos directamente en
        /// el SQL es seguro, igual criterio que Helpers.SetNombreXMLRoto/SetFechaActualizacionRoto.
        /// </summary>
        private void LimpiarActualizacionRoto(EnumHardware tipo)
        {
            if (MessageBox.Show(Loc("L_Suite_ConfirmarLimpiarActualizacion"), Loc("L_Suite_ConfirmarEliminacion"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            string variableXmlName;
            string variableFechaName;
            switch (tipo)
            {
                case EnumHardware.Aluminio:
                    variableXmlName = "RotoXmlNombreALU";
                    variableFechaName = "RotoFechaActualizacionALU";
                    break;
                case EnumHardware.PAX:
                    variableXmlName = "RotoXmlNombrePAX";
                    variableFechaName = "RotoFechaActualizacionPAX";
                    break;
                case EnumHardware.PVC:
                default:
                    variableXmlName = "RotoXmlNombrePVC";
                    variableFechaName = "RotoFechaActualizacionPVC";
                    break;
            }

            string sql = "DELETE FROM VariablesGlobales WHERE Nombre = N'" + variableXmlName + "' OR Nombre = N'" + variableFechaName + "'";
            RotoTools.Helpers.EjecutarNonQuery(sql);

            InitializeRotoInfo();
        }

        private void BtnClearPVC_Click(object sender, RoutedEventArgs e) => LimpiarActualizacionRoto(EnumHardware.PVC);

        private void BtnClearALU_Click(object sender, RoutedEventArgs e) => LimpiarActualizacionRoto(EnumHardware.Aluminio);

        private void BtnClearPAX_Click(object sender, RoutedEventArgs e) => LimpiarActualizacionRoto(EnumHardware.PAX);

        #endregion
    }
}
