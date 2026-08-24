using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using Microsoft.Win32;
using RotoEntities;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.Cam
{
    /// <summary>
    /// Sustituye a CamMenu.cs/CamMenu.Designer.cs (WinForms): carga de XML de herrajes, selección
    /// de Sets, listado/instalación de operaciones (mecanizados 2D) y acceso a instalación 3D,
    /// configuración de geometría e información de operación. Reutiliza tal cual la lógica de
    /// negocio del proyecto original (RotoTools.Helpers, RotoTools.LocalizationManager, XmlLoader,
    /// RotoEntities.*) vía ProjectReference.
    ///
    /// A diferencia de CamMenu (ventana de tamaño fijo 1415x888), esta página ocupa todo el ancho
    /// disponible de la ventana principal: los Sets, la grid de operaciones a instalar y la grid
    /// de detalle se reparten en un layout de rejilla con separadores arrastrables (GridSplitter),
    /// tal y como se pidió explícitamente para los módulos con grids/listas grandes.
    ///
    /// Alcance de esta entrega: se migra el flujo completo de operaciones 2D (cargar XML, marcar
    /// Sets, cargar/filtrar/instalar operaciones, ver info, configurar geometría) y el acceso a la
    /// instalación 3D (Cam3DWindow). "Exportar mecanizados" y "Normalizar operaciones" (funciones
    /// de administración basadas en Excel) y los dos catálogos JSON de administración 3D quedan
    /// para una entrega posterior; sus botones muestran un aviso en vez de ejecutar la acción.
    /// </summary>
    public partial class CamPage : UserControl
    {
        // ------------------------------------------------------------------
        // Estado (equivalente a los campos privados de CamMenu.cs)
        // ------------------------------------------------------------------
        private XmlData? _xmlOrigen;
        private bool _xmlCargado;
        private System.Xml.XmlNamespaceManager? _nsmgr;

        private List<Operation> _operationsXmlList = new();
        private List<OperationInstalarGridItem> _allOperations = new();
        private List<OperationGridRow> _allData = new();
        private Dictionary<string, bool> _cacheExisteBD = new();
        private List<OperationsShapes> _operationsShapesListEmbebidos = new();

        private readonly List<SetListItem> _todosLosSets = new();
        private readonly ObservableCollection<SetListItem> _setsVisibles = new();
        private readonly ObservableCollection<OperationInstalarGridItem> _operacionesVisibles = new();

        private bool _syncingSelection;
        private bool _actualizandoCheckMaestro;

        public CamPage()
        {
            InitializeComponent();

            ListaSets.ItemsSource = _setsVisibles;
            GridInstalarOperaciones.ItemsSource = _operacionesVisibles;

            CargarTextos();
            CleanInfo();
        }

        #region Localización / cabecera

        private void CargarTextos()
        {
            TxtTitulo.Text = "CAM";
            TxtSubtitulo.Text = SuiteLocalization.GetString("L_Suite_CamSubtitulo");
            LblXml.Text = "";
            TxtFiltroSets.ToolTip = RotoTools.LocalizationManager.GetString("L_Buscar");
            TxtFiltroOperaciones.ToolTip = RotoTools.LocalizationManager.GetString("L_Buscar");
            RbTodas.Content = RotoTools.LocalizationManager.GetString("L_Todas");
            RbNoExisten.Content = RotoTools.LocalizationManager.GetString("L_NoExiste");
            RbTodas.IsChecked = true;

            TxtBtnCargarXml.Text = "Cargar XML";
            TxtBtnCargarOperaciones.Text = RotoTools.LocalizationManager.GetString("L_CargarOperaciones");
            TxtBtnLimpiar.Text = RotoTools.LocalizationManager.GetString("L_LimpiarInfo");
            TxtBtnInstalarMacros.Text = RotoTools.LocalizationManager.GetString("L_InstalarMacros");
            TxtBtnExportar.Text = RotoTools.LocalizationManager.GetString("L_ExportarMecanizados");
            TxtBtnNormalizar.Text = RotoTools.LocalizationManager.GetString("L_NormalizarOperaciones");
            TxtBtnInstalar.Text = RotoTools.LocalizationManager.GetString("L_InstalarOperaciones");
            TxtBtnInstalar3D.Text = "3D";
        }

        // Nota: ya no se muestra aquí abajo el servidor/base de datos de conexión (InitializeInfoConnection
        // se ha retirado): esa información ya aparece siempre arriba, junto al selector de idioma
        // (ver MainWindow), así que repetirla en cada página era redundante.

        #endregion

        #region Carga de XML (LoadXml / btn_LoadXml_Click)

        private void BtnLoadXml_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };

            if (openFileDialog.ShowDialog() == true)
            {
                CleanInfo();
                string rutaXml = openFileDialog.FileName;
                EnableControls(false);

                _xmlOrigen = LoadXml(rutaXml);
                LblXml.Text = rutaXml;
                LoadSets("");

                EnableControls(true);
            }
        }

        /// <summary>Idéntico a CamMenu.LoadXml: XmlLoader (no XmlSerializer) sobre el namespace
        /// "hw" del esquema de herrajes. Si algo falla, devuelve null silenciosamente, igual que
        /// el original (no había MessageBox de error aquí).</summary>
        private XmlData? LoadXml(string xmlPath)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(xmlPath);

                _nsmgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
                _nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

                var loader = new RotoTools.XmlLoader(_nsmgr);
                loader.OnLoadingInfo += (tipo, valor) =>
                {
                    LblXml.Text = RotoTools.LocalizationManager.GetString("L_Cargando") + $"... {tipo} {valor?.TrimEnd()}";
                    DoEvents();
                };

                var xmlData = new XmlData
                {
                    Supplier = loader.LoadSupplier(doc)
                };
                xmlData.HardwareType = loader.LoadHardwareType(xmlData.Supplier);
                xmlData.FittingGroupList = loader.LoadFittingGroups(doc);
                xmlData.ColourList = loader.LoadColourMaps(doc);
                xmlData.OptionList = loader.LoadDocOptions(doc);
                xmlData.FittingList = loader.LoadFittings(doc);
                xmlData.SetList = loader.LoadSets(doc, xmlData.FittingList);

                _xmlCargado = true;
                return xmlData;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Sets (LoadSets / filtro / seleccionar todos)

        /// <summary>Igual que CamMenu.LoadSets: cada Set nuevo hereda el estado actual del check
        /// "seleccionar todos", no un flag propio del XML.</summary>
        private void LoadSets(string filtro)
        {
            _todosLosSets.Clear();

            if (_xmlCargado && _xmlOrigen?.SetList is { Count: > 0 })
            {
                bool marcarTodos = ChkSetsTodos.IsChecked == true;

                IEnumerable<Set> sets = string.IsNullOrEmpty(filtro)
                    ? _xmlOrigen.SetList
                    : _xmlOrigen.SetList.Where(s => (s.Code ?? "").ToLower().Contains(filtro.ToLower()));

                foreach (var set in sets.OrderBy(s => s.Code))
                {
                    _todosLosSets.Add(new SetListItem(set, marcarTodos));
                }
            }

            RefrescarSetsVisibles();
        }

        private void RefrescarSetsVisibles()
        {
            _setsVisibles.Clear();
            foreach (var item in _todosLosSets) _setsVisibles.Add(item);
        }

        private void TxtFiltroSets_TextChanged(object sender, TextChangedEventArgs e)
        {
            _actualizandoCheckMaestro = true;
            ChkSetsTodos.IsChecked = false;
            _actualizandoCheckMaestro = false;
            LoadSets(TxtFiltroSets.Text);
        }

        private void ChkSetsTodos_Changed(object sender, RoutedEventArgs e)
        {
            if (_actualizandoCheckMaestro) return;
            bool marcar = ChkSetsTodos.IsChecked == true;
            foreach (var item in _setsVisibles) item.Checked = marcar;
        }

        #endregion

        #region Operaciones desde el XML (CargarListaOperacionesFromXml / ObtenerOperaciones / GetGeometriaOperacionList)

        /// <summary>Idéntico a CamMenu.CargarListaOperacionesFromXml: recorre los Sets marcados,
        /// descarta "SCREW" y nombres en blanco, y agrupa por nombre de operación en un
        /// OperationGridRow "principal" con sus apariciones por Fitting en OperationsList.</summary>
        private void CargarListaOperacionesFromXml()
        {
            var gridRowDict = new Dictionary<string, OperationGridRow>();

            foreach (var setItem in _todosLosSets.Where(s => s.Checked))
            {
                var set = setItem.SetRef;
                foreach (var setDescription in set.SetDescriptionList ?? new List<SetDescription>())
                {
                    foreach (var operation in ObtenerOperaciones(setDescription))
                    {
                        if (string.IsNullOrWhiteSpace(operation.Name)) continue;
                        if (operation.Name.Contains("SCREW", StringComparison.OrdinalIgnoreCase)) continue;

                        if (!gridRowDict.TryGetValue(operation.Name, out var mainRow))
                        {
                            mainRow = new OperationGridRow(
                                operation.Name,
                                setDescription.Fitting?.Id.ToString(),
                                setDescription.Fitting?.Ref,
                                setDescription.Fitting?.Description,
                                operation.XPosition,
                                operation.Location,
                                set.Code,
                                setDescription.XPosition.ToString());

                            mainRow.OperationsList.Add(mainRow);
                            gridRowDict.Add(operation.Name, mainRow);
                        }
                        else
                        {
                            bool existe = mainRow.OperationsList.Any(x => x.FittingID == setDescription.Fitting?.Id.ToString());
                            if (!existe)
                            {
                                mainRow.OperationsList.Add(new OperationGridRow(
                                    operation.Name,
                                    setDescription.Fitting?.Id.ToString(),
                                    setDescription.Fitting?.Ref,
                                    setDescription.Fitting?.Description,
                                    operation.XPosition,
                                    operation.Location,
                                    set.Code,
                                    setDescription.XPosition.ToString()));
                            }
                        }
                    }
                }
            }

            _allData = gridRowDict.Values.OrderBy(o => o.Operation).ToList();
            _operationsXmlList = _allData.Select(o => new Operation { Name = o.Operation }).ToList();
        }

        /// <summary>Idéntico a CamMenu.ObtenerOperaciones: recorrido de 3 niveles (Fitting →
        /// Article → Article) sobre el objeto en memoria, sin acceso a BBDD.</summary>
        private static IEnumerable<Operation> ObtenerOperaciones(SetDescription setDescription)
        {
            if (setDescription.Fitting?.OperationList != null)
                foreach (var op in setDescription.Fitting.OperationList)
                    yield return op;

            if (setDescription.Fitting?.ArticleList != null)
            {
                foreach (var article in setDescription.Fitting.ArticleList)
                {
                    if (article.Fitting?.OperationList != null)
                        foreach (var op in article.Fitting.OperationList)
                            yield return op;

                    if (article.Fitting?.ArticleList != null)
                    {
                        foreach (var articleProgram in article.Fitting.ArticleList)
                        {
                            if (articleProgram.Fitting?.OperationList != null)
                                foreach (var opProgram in articleProgram.Fitting.OperationList)
                                    yield return opProgram;
                        }
                    }
                }
            }
        }

        /// <summary>Idéntico a CamMenu.GetGeometriaOperacionList: filtra en memoria sobre las
        /// formas embebidas ya cargadas (JSON de recursos, vía Helpers.CargarOperationsShapesRotoEmbebidos).</summary>
        private List<OperationsShapes> GetGeometriaOperacionList(string operationName, short exterior)
            => _operationsShapesListEmbebidos
                .Where(o => o.OperationName == "RO_" + operationName && o.External == exterior)
                .OrderBy(o => o.BasicShape)
                .ToList();

        #endregion

        #region Grid de instalación de operaciones (CargarGridInstalarOperaciones / AplicarFiltros)

        private void CargarGridInstalarOperaciones()
        {
            _allOperations = _operationsXmlList
                .OrderBy(o => o.Name)
                .Select(o => new OperationInstalarGridItem
                {
                    Selected = false,
                    OperationName = o.Name,
                    OperationShapeList = GetGeometriaOperacionList(o.Name, 0),
                    OperationShapeExtList = GetGeometriaOperacionList(o.Name, 1)
                })
                .ToList();

            AplicarFiltros();
        }

        /// <summary>Idéntico a CamMenu.AplicarFiltros: siempre se filtra desde _allOperations (no
        /// se encadena sobre un filtrado anterior), texto + radio "Todas"/"No existen".</summary>
        private void AplicarFiltros()
        {
            IEnumerable<OperationInstalarGridItem> query = _allOperations;

            string filtroTexto = (TxtFiltroOperaciones.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(filtroTexto))
                query = query.Where(o => o.OperationName.Contains(filtroTexto, StringComparison.OrdinalIgnoreCase));

            if (RbNoExisten.IsChecked == true)
                query = query.Where(o => !ExisteOperacionEnBDCacheado("RO_" + o.OperationName));

            var lista = query.ToList();
            _operacionesVisibles.Clear();
            foreach (var item in lista) _operacionesVisibles.Add(item);

            GrpOperaciones.Text = RotoTools.LocalizationManager.GetString("L_Operaciones") + $" ({_operacionesVisibles.Count})";
        }

        private void TxtFiltroOperaciones_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChkOperacionesTodas.IsChecked = false;
            AplicarFiltros();
        }

        private void RbFiltro_Checked(object sender, RoutedEventArgs e) => AplicarFiltros();

        private void ChkOperacionesTodas_Changed(object sender, RoutedEventArgs e)
        {
            bool marcar = ChkOperacionesTodas.IsChecked == true;
            foreach (var item in _operacionesVisibles) item.Selected = marcar;
        }

        /// <summary>Envoltorio con caché por nombre, igual que CamMenu.ExisteOperacionEnBD
        /// (instancia), que delega en RotoTools.Helpers.ExisteOperacionEnBD (BBDD).</summary>
        private bool ExisteOperacionEnBDCacheado(string operationName)
        {
            if (!_cacheExisteBD.TryGetValue(operationName, out bool existe))
            {
                existe = RotoTools.Helpers.ExisteOperacionEnBD(operationName);
                _cacheExisteBD[operationName] = existe;
            }
            return existe;
        }

        #endregion

        #region Grid de detalle + sincronización de selección

        private void CargarDatosGridDetalle()
        {
            GridDetalleOperaciones.ItemsSource = null;
            GridDetalleOperaciones.ItemsSource = _allData;
        }

        /// <summary>Igual que CamMenu: seleccionar una fila en la grid de instalación selecciona
        /// (y hace scroll hasta) la fila correspondiente en la grid de detalle. Sincronización en
        /// un solo sentido, igual que el original.</summary>
        private void GridInstalarOperaciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_syncingSelection) return;
            if (GridInstalarOperaciones.SelectedItem is not OperationInstalarGridItem item) return;

            try
            {
                _syncingSelection = true;
                var fila = _allData.FirstOrDefault(f => string.Equals(f.Operation, item.OperationName, StringComparison.OrdinalIgnoreCase));
                if (fila != null)
                {
                    GridDetalleOperaciones.SelectedItem = fila;
                    GridDetalleOperaciones.ScrollIntoView(fila);
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        #endregion

        #region Limpieza / carga completa (CleanInfo / LoadOperations)

        private void CleanInfo()
        {
            TxtFiltroSets.Text = "";
            TxtFiltroOperaciones.Text = "";
            RbTodas.IsChecked = true;
            _todosLosSets.Clear();
            _setsVisibles.Clear();
            ChkSetsTodos.IsChecked = false;
            ChkOperacionesTodas.IsChecked = false;
            _cacheExisteBD = new Dictionary<string, bool>();

            _allOperations = new List<OperationInstalarGridItem>();
            _operacionesVisibles.Clear();
            _allData = new List<OperationGridRow>();
            CargarDatosGridDetalle();

            GrpOperaciones.Text = RotoTools.LocalizationManager.GetString("L_Operaciones") + " (0)";
        }

        /// <summary>Idéntico a CamMenu.LoadOperations: recarga formas embebidas, reconstruye la
        /// lista de operaciones desde los Sets marcados, y re-aplica los filtros.</summary>
        private void LoadOperations()
        {
            _operationsShapesListEmbebidos = RotoTools.Helpers.CargarOperationsShapesRotoEmbebidos();
            CargarListaOperacionesFromXml();
            CargarGridInstalarOperaciones();
            CargarDatosGridDetalle();
        }

        private void BtnCargarOperaciones_Click(object sender, RoutedEventArgs e)
        {
            if (_xmlCargado) LoadOperations();
        }

        private void BtnClearOperations_Click(object sender, RoutedEventArgs e) => CleanInfo();

        #endregion

        #region Instalación de operaciones 2D (InstallConditions / btn_InstallOperation_Click)

        /// <summary>Idéntico a CamMenu.InstallConditions: resuelve (instalando si hace falta) la
        /// condición embebida referenciada por operationShape.Conditions y devuelve su RowId real
        /// en BBDD.</summary>
        private string InstallConditions(string conditionId)
        {
            var allConditionsList = RotoTools.Helpers.CargarMechanizedConditionsEmbebidos();
            var mechanizedCondition = allConditionsList.FirstOrDefault(c => c.RowId == conditionId);

            if (mechanizedCondition == null) return "";

            if (!RotoTools.Helpers.ExisteCondicionEnBD(mechanizedCondition.XmlConditions, Convert.ToBoolean(mechanizedCondition.NecesitaObjetoDeUsuario)))
            {
                if (mechanizedCondition.NecesitaObjetoDeUsuario == "true" && !string.IsNullOrEmpty(mechanizedCondition.XmlObject))
                {
                    if (!RotoTools.Helpers.ExisteObjetoUsuarioEnBD(mechanizedCondition.ObjetoDeUsuario))
                    {
                        RotoTools.Helpers.InstallMechanizedObject(mechanizedCondition.ObjetoDeUsuario, mechanizedCondition.XmlObject);
                    }

                    string rowIdMechanizedObject = RotoTools.Helpers.GetMechanizedObjectRowId(mechanizedCondition.ObjetoDeUsuario);
                    mechanizedCondition.XmlConditions = mechanizedCondition.XmlConditions.Replace("RowIdObjetoDeUsuario", rowIdMechanizedObject);
                }

                RotoTools.Helpers.InstallMechanizedCondition(mechanizedCondition);
                return RotoTools.Helpers.GetMechanizedConditionRowId(mechanizedCondition.Name);
            }

            return Convert.ToBoolean(mechanizedCondition.NecesitaObjetoDeUsuario)
                ? RotoTools.Helpers.GetMechanizedConditionRowIdByXmlConditionsConObjetoUsuario(mechanizedCondition.XmlConditions)
                : RotoTools.Helpers.GetMechanizedConditionRowIdByXmlConditions(mechanizedCondition.XmlConditions);
        }

        /// <summary>Idéntico a CamMenu.btn_InstallOperation_Click: instala en BBDD las operaciones
        /// marcadas (de la lista actualmente visible/filtrada, igual que el original que usa
        /// _bindingSource.List) que aún no existen, junto con sus formas/geometría y condiciones.
        /// Sin transacción compartida, igual que el original (cada Helpers.InstallXxx abre su
        /// propia conexión).</summary>
        private void BtnInstallOperation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                EnableControls(false);

                var itemsAProcesar = _operacionesVisibles.ToList();
                ProgressInstall.Visibility = Visibility.Visible;
                ProgressInstall.Value = 0;
                ProgressInstall.Maximum = itemsAProcesar.Count > 0 ? itemsAProcesar.Count : 1;

                var mechanizedOperationsEmbebidos = RotoTools.Helpers.CargarMechanizedOperationsRotoEmbebidos();
                var macrosEmbeddedMechanizedOperations = RotoTools.Helpers.CargarMacrosMechanizedOperationsEmbebidos();
                var macroOperationsShapesEmbeddedList = RotoTools.Helpers.CargarMacrosOperationsShapesEmbebidos();

                foreach (var item in itemsAProcesar)
                {
                    if (item.Selected && !RotoTools.Helpers.ExisteOperacionEnBD("RO_" + item.OperationName))
                    {
                        var mechanizedOperationsList = mechanizedOperationsEmbebidos
                            .Where(op => op.OperationName == "RO_" + item.OperationName)
                            .ToList();

                        if (mechanizedOperationsList.Count > 0)
                        {
                            foreach (var operation in mechanizedOperationsList)
                            {
                                operation.InitializeLevel2(operation.OperationName);
                                operation.InitializeLevel3(operation.OperationName, operation.Level2);
                                RotoTools.Helpers.InstallMechanizedOperation(operation);
                            }
                        }
                        else
                        {
                            var mechanizedOperation = new RotoEntities.MechanizedOperation("RO_" + item.OperationName);
                            RotoTools.Helpers.InstallMechanizedOperation(mechanizedOperation);
                        }

                        var allOperationsShapes = new List<OperationsShapes>();
                        allOperationsShapes.AddRange(item.OperationShapeList);
                        allOperationsShapes.AddRange(item.OperationShapeExtList);

                        foreach (var operationShape in allOperationsShapes)
                        {
                            if (!string.IsNullOrEmpty(operationShape.Conditions))
                                operationShape.Conditions = InstallConditions(operationShape.Conditions);

                            if (!RotoTools.Helpers.ExisteOperacionEnBD(operationShape.BasicShape))
                            {
                                var embeddedOperation = macrosEmbeddedMechanizedOperations
                                    .FirstOrDefault(op => op.OperationName == operationShape.BasicShape);
                                if (embeddedOperation != null)
                                    RotoTools.Helpers.InstallMechanizedOperation(embeddedOperation);

                                var macroOperationsShapesList = macroOperationsShapesEmbeddedList
                                    .Where(o => o.OperationName == operationShape.BasicShape).ToList();
                                foreach (var operation in macroOperationsShapesList)
                                    RotoTools.Helpers.InstallOperationShape(operation);
                            }

                            RotoTools.Helpers.InstallOperationShape(operationShape);
                        }
                    }

                    ProgressInstall.Value++;
                    DoEvents();
                }

                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_OperacionesInstaladas"), "",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ProgressInstall.Value = 0;
                ProgressInstall.Visibility = Visibility.Collapsed;
                _cacheExisteBD = new Dictionary<string, bool>();
                ChkOperacionesTodas.IsChecked = false;
                LoadOperations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error(34)" + Environment.NewLine + Environment.NewLine + ex.Message, "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                EnableControls(true);
            }
        }

        private void BtnInstalarMacros_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                EnableControls(false);

                RotoTools.Helpers.InstallMacrosMechanizedOperations();
                RotoTools.Helpers.InstallMacrosOperationsShapes();

                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_MacrosInstaladas"), "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error(33)" + Environment.NewLine + Environment.NewLine + ex.Message, "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                EnableControls(true);
            }
        }

        #endregion

        #region Instalación 3D (btn_Mecanizados3D_Click)

        private void BtnInstalar3D_Click(object sender, RoutedEventArgs e)
        {
            var seleccionadas = _operacionesVisibles.Where(o => o.Selected).ToList();
            if (seleccionadas.Count == 0)
            {
                MessageBox.Show("Seleccione primero una o varias operaciones para instalar en 3D.", "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ventana3D = new Cam3DWindow(seleccionadas) { Owner = Window.GetWindow(this) };
            ventana3D.ShowDialog();

            // Igual que el original: Cam3D puede haber instalado definiciones 2D que faltaban
            // (Cam3DHelpers.AsegurarDefinicion2DInstalada), así que se refresca todo.
            _cacheExisteBD = new Dictionary<string, bool>();
            ChkOperacionesTodas.IsChecked = false;
            LoadOperations();
        }

        #endregion

        #region Info de operación / configurar geometría (columnas de icono de las grids)

        private void BtnInfoOperacion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is OperationGridRow fila)
            {
                var ventana = new CamInfoOperacionWindow(fila.Operation, fila.OperationsList.OrderBy(o => o.Article).ToList())
                {
                    Owner = Window.GetWindow(this)
                };
                ventana.ShowDialog();
            }
        }

        private void BtnConfigurarGeometria_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is OperationInstalarGridItem item)
            {
                var ventana = new CamConfigurarGeometriaWindow("RO_" + item.OperationName, item.OperationShapeList, item.OperationShapeExtList)
                {
                    Owner = Window.GetWindow(this)
                };

                if (ventana.ShowDialog() == true)
                {
                    item.OperationShapeList = ventana.ResultOperationsShapesList;
                }
            }
        }

        #endregion

        #region Funciones aplazadas para una próxima entrega (Excel/administración)

        private void MostrarAvisoPendiente()
        {
            MessageBox.Show(
                "Esta función todavía no se ha migrado a RotoTools Suite (se incorporará en una próxima entrega). " +
                "Por ahora, utilice el RotoTools clásico para esta acción.",
                "Próximamente", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportarMecanizados_Click(object sender, RoutedEventArgs e) => MostrarAvisoPendiente();

        private void BtnNormalizarOperaciones_Click(object sender, RoutedEventArgs e) => MostrarAvisoPendiente();

        #endregion

        #region Utilidades

        private void EnableControls(bool enabled)
        {
            BtnCargarOperaciones.IsEnabled = enabled;
            BtnClearOperations.IsEnabled = enabled;
            BtnInstalarMacros.IsEnabled = enabled;
            BtnExportarMecanizados.IsEnabled = enabled;
            BtnInstallOperation.IsEnabled = enabled;
            TxtFiltroSets.IsEnabled = enabled;
            TxtFiltroOperaciones.IsEnabled = enabled;
            RbTodas.IsEnabled = enabled;
            RbNoExisten.IsEnabled = enabled;
            ChkSetsTodos.IsEnabled = enabled;
            ChkOperacionesTodas.IsEnabled = enabled;
            GridDetalleOperaciones.IsEnabled = enabled;
            ListaSets.IsEnabled = enabled;
        }

        /// <summary>Equivalente WPF de Application.DoEvents(): bombea el bucle de mensajes para
        /// que la barra de progreso y la etiqueta de "cargando..." se repinten durante bucles
        /// síncronos largos, igual que hacía la app WinForms original.</summary>
        private static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(f =>
                {
                    ((DispatcherFrame)f!).Continue = false;
                    return null;
                }), frame);
            Dispatcher.PushFrame(frame);
        }

        #endregion
    }
}
