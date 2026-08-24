using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using RotoEntities;

namespace RotoTools.Suite.Views.ConectorHerraje
{
    /// <summary>
    /// Sustituye a ConectorHerrajeRevisionSets.cs/.Designer.cs (WinForms): para el XML de herrajes
    /// ya cargado (ver ConectorHerrajePage), permite elegir un conector guardado en BBDD y revisar
    /// qué Sets están/no están incluidos en él, y qué códigos del conector no tienen ningún Set
    /// correspondiente en el XML. Reutiliza tal cual RotoTools.Helpers (conexión,
    /// (de)serialización XML) y RotoEntities.Connector/ConnectorNode/Option vía ProjectReference.
    /// Export a Excel con NPOI, igual que el original.
    /// </summary>
    public partial class ConectorHerrajeRevisionSetsWindow : Window
    {
        private readonly XmlData _xmlData;
        private Connector? _connectorHerraje;
        private List<string> _codesInConector = new();

        // Listas persistidas usadas por la exportación a Excel (independientes de los filtros de
        // texto de cada pestaña, igual que setsIncluidosList/setsNoIncluidosList/
        // codigosNoIncluidosEnXml en el original). Se reinician en cada recálculo (ver comentario
        // de la mejora deliberada al inicio del XAML).
        private List<Set> _setsIncluidosList = new();
        private List<Set> _setsNoIncluidosList = new();
        private List<string> _codigosNoIncluidosEnXml = new();

        private DispatcherTimer? _timerCopiado;

        public ConectorHerrajeRevisionSetsWindow(XmlData xmlData)
        {
            InitializeComponent();

            _xmlData = xmlData;

            CargarTextos();
            InitializeInfoConnection();
            LoadItemsConectorHerraje();
            LoadItemsHardwareSupplier();

            // Sin conector seleccionado todavía: estado inicial vacío (más claro que el "label1"
            // literal que mostraba el Designer original hasta la primera carga).
            LblTotalSetsIncluidos.Text = Loc("L_Suite_SeleccionaConectorIncluidos");
            LblTotalSetsNoIncluidos.Text = Loc("L_Suite_SeleccionaConectorNoIncluidos");
            LblTotalCodigosNoXml.Text = Loc("L_Suite_SeleccionaConectorCodigos");
        }

        /// <summary>Atajo a RotoTools.Suite.Services.SuiteLocalization.GetString: todos los textos
        /// propios de esta ventana (no existentes en el proyecto original, ver el comentario al
        /// inicio del XAML sobre por qué el original no usaba LocalizationManager aquí) se
        /// resuelven a través de él, igual idioma que el resto de la Suite.</summary>
        private static string Loc(string key) => RotoTools.Suite.Services.SuiteLocalization.GetString(key);

        #region Cabecera / conexión

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_RevisionSets");
            TxtTitulo.Text = Title;

            LblConector.Text = Loc("L_Suite_Conector");

            RbTabIncluidos.Content = Loc("L_Suite_RevisionSets_TabIncluidos");
            RbTabNoIncluidos.Content = Loc("L_Suite_RevisionSets_TabNoIncluidos");
            RbTabCodigos.Content = Loc("L_Suite_RevisionSets_TabCodigos");

            string buscar = RotoTools.LocalizationManager.GetString("L_Buscar");
            LblBuscarIncluidos.Text = buscar;
            LblBuscarNoIncluidos.Text = buscar;

            string exportar = Loc("L_Suite_ExportarExcel");
            TxtBtnExportIncluidos.Text = exportar;
            TxtBtnExportNoIncluidos.Text = exportar;
            TxtBtnExportCodigos.Text = exportar;

            TxtBtnEliminarLineas.Text = Loc("L_Suite_EliminarLineasNoUsadas");
            TxtBtnVolver.Text = RotoTools.LocalizationManager.GetString("L_Volver");
        }

        private void InitializeInfoConnection()
        {
            string servidor = RotoTools.Helpers.GetServer();
            string baseDatos = RotoTools.Helpers.GetDataBase();
            string conectorActivo = RotoTools.Helpers.GetConectorActivo() ?? "";
            LblConectorActivo.Text = $@"{servidor}\{baseDatos}    ·    " + Loc("L_Suite_ConectorActivo") + ": " + conectorActivo;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Close();

        #endregion

        #region Pestañas

        private void Tab_Changed(object sender, RoutedEventArgs e)
        {
            // Los 3 paneles pueden no existir todavía la primerísima vez que se marca el
            // RadioButton por defecto (IsChecked="True" en XAML, disparado durante
            // InitializeComponent antes de que termine de construirse el árbol visual).
            if (PanelIncluidos == null || PanelNoIncluidos == null || PanelCodigos == null) return;

            PanelIncluidos.Visibility = RbTabIncluidos.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelNoIncluidos.Visibility = RbTabNoIncluidos.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelCodigos.Visibility = RbTabCodigos.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Combos (Conector / Hardware Supplier)

        private void LoadItemsConectorHerraje()
        {
            var items = new List<string>();
            using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            {
                conexion.Open();
                using var cmd = new SqlCommand("SELECT Codigo, XML FROM ConectorHerrajes", conexion);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) items.Add(reader[0].ToString());
            }

            CmbConectores.ItemsSource = items;
        }

        private void LoadItemsHardwareSupplier()
        {
            var items = new List<string> { "" };
            using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            {
                conexion.Open();
                using var cmd = new SqlCommand(
                    "SELECT Valor FROM ContenidoOpciones WHERE Opcion = 'HardwareSupplier' ORDER BY Orden", conexion);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) items.Add(reader[0].ToString());
            }

            CmbHardwareSupplier.ItemsSource = items;
            CmbHardwareSupplier.SelectedIndex = 0;
        }

        private void CmbConectores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string conectorName = (CmbConectores.SelectedItem as string)?.Trim() ?? "";
            if (conectorName.Length == 0) return;

            LoadConectorDataFromDB(conectorName);

            if (_connectorHerraje != null) FillData();
        }

        private void CmbHardwareSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e) => FillData();

        private void LoadConectorDataFromDB(string conectorName)
        {
            try
            {
                string? xmlString = null;

                using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                {
                    conexion.Open();
                    using var cmd = new SqlCommand("SELECT XML FROM ConectorHerrajes WHERE Codigo = @codigo", conexion);
                    cmd.Parameters.AddWithValue("@codigo", conectorName);

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        xmlString = result.ToString();
                    }
                    else
                    {
                        _connectorHerraje = null;
                        MessageBox.Show(Loc("L_Suite_ConectorSinInfoBD"), "",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                if (!string.IsNullOrWhiteSpace(xmlString))
                    _connectorHerraje = RotoTools.Helpers.DeserializarXML<Connector>(xmlString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc("L_Suite_ErrorCargarConector") + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Cálculo de Sets incluidos / no incluidos / códigos sin XML

        private void FillData()
        {
            if (_connectorHerraje?.Nodes == null || _connectorHerraje.Nodes.Count == 0)
                return;

            string? selectedSupplier = CmbHardwareSupplier.SelectedItem?.ToString()?.Trim();
            bool aplicarFiltroProveedor = !string.IsNullOrEmpty(selectedSupplier);

            var fittingCodesFromConector = _connectorHerraje.Nodes
                .Where(n => !string.IsNullOrWhiteSpace(n.FittingCode))
                .Where(n =>
                {
                    if (!aplicarFiltroProveedor) return true;

                    var options = n.IncludedOptions?.Options?.OptionList;
                    if (options == null) return false;

                    return options.Any(o =>
                        o.Name.Equals("HardwareSupplier", StringComparison.OrdinalIgnoreCase) &&
                        o.Value.Equals(selectedSupplier, StringComparison.OrdinalIgnoreCase));
                })
                .Select(n => n.FittingCode)
                .Distinct()
                .ToList();

            _codesInConector = fittingCodesFromConector;

            ProcesarSetsDelConector(_xmlData.SetList, fittingCodesFromConector);
        }

        /// <summary>Equivalente a CargarSetsEnListViews (original): separa xmlData.SetList entre
        /// incluidos/no incluidos según fittingCodesFromConector, y calcula los códigos del
        /// conector sin Set correspondiente en el XML. A diferencia del original, reinicia las 3
        /// listas persistidas al empezar (ver comentario de la mejora deliberada al inicio del
        /// XAML) para que un recálculo no acumule datos del cálculo anterior.</summary>
        private void ProcesarSetsDelConector(List<Set> sets, List<string> fittingCodesFromConector)
        {
            _setsIncluidosList = new List<Set>();
            _setsNoIncluidosList = new List<Set>();
            _codigosNoIncluidosEnXml = new List<string>();

            foreach (var set in sets.OrderBy(s => s.Code, StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(set.Code)) continue;

                if (fittingCodesFromConector.Contains(set.Code))
                    _setsIncluidosList.Add(set);
                else
                    _setsNoIncluidosList.Add(set);
            }

            foreach (var codigoHerraje in fittingCodesFromConector)
            {
                if (string.IsNullOrWhiteSpace(codigoHerraje)) continue;

                if (!_xmlData.SetList.Any(s => string.Equals(s.Code?.Trim(), codigoHerraje.Trim(), StringComparison.OrdinalIgnoreCase)))
                    _codigosNoIncluidosEnXml.Add(codigoHerraje);
            }

            LblTotalSetsIncluidos.Text =
                string.Format(Loc("L_Suite_TotalSetsIncluidos"), _setsIncluidosList.Count, _xmlData.SetList.Count);
            LblTotalSetsNoIncluidos.Text =
                string.Format(Loc("L_Suite_TotalSetsNoIncluidos"), _setsNoIncluidosList.Count, _xmlData.SetList.Count);
            LblTotalCodigosNoXml.Text =
                string.Format(Loc("L_Suite_TotalCodigosNoXml"), _codigosNoIncluidosEnXml.Count);

            ActualizarListaIncluidos();
            ActualizarListaNoIncluidos();
            ActualizarListaCodigos();
        }

        #endregion

        #region Filtros de texto (txt_FiltroIncluidos / txt_FiltroNoIncluidos)

        private void TxtFiltroIncluidos_TextChanged(object sender, TextChangedEventArgs e) => ActualizarListaIncluidos();

        private void TxtFiltroNoIncluidos_TextChanged(object sender, TextChangedEventArgs e) => ActualizarListaNoIncluidos();

        /// <summary>Igual que txt_FiltroIncluidos_TextChanged (original): filtra siempre sobre el
        /// XML completo (xmlData.SetList), no sobre _setsIncluidosList, comprobando pertenencia a
        /// _codesInConector para cada Set.</summary>
        private void ActualizarListaIncluidos()
        {
            string filtro = TxtFiltroIncluidos.Text ?? "";

            var items = _xmlData.SetList
                .Where(s => !string.IsNullOrWhiteSpace(s.Code))
                .Where(s => s.Code.ToLower().Contains(filtro.ToLower()))
                .Where(s => _codesInConector.Contains(s.Code))
                .OrderBy(s => s.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            GridSetsIncluidos.ItemsSource = items;
        }

        /// <summary>Igual que txt_FiltroNoIncluidos_TextChanged (original): filtra sobre la lista
        /// ya calculada _setsNoIncluidosList, no sobre el XML completo.</summary>
        private void ActualizarListaNoIncluidos()
        {
            string filtro = TxtFiltroNoIncluidos.Text ?? "";

            var items = _setsNoIncluidosList
                .Where(s => !string.IsNullOrWhiteSpace(s.Code))
                .Where(s => s.Code.ToLower().Contains(filtro.ToLower()))
                .OrderBy(s => s.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();

            GridSetsNoIncluidos.ItemsSource = items;
        }

        /// <summary>El original tenía un cuadro de filtro para esta pestaña (txt_FiltroCodigoNoXml)
        /// pero su TextChanged estaba completamente comentado (no filtraba nada) y el propio
        /// control era invisible (Visible=false en el Designer); aquí se omite el cuadro y se
        /// muestra siempre la lista completa.</summary>
        private void ActualizarListaCodigos()
        {
            ListCodigosNoXml.ItemsSource = _codigosNoIncluidosEnXml
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        #endregion

        #region Copiar al portapapeles (doble clic)

        /// <summary>Igual que CopiarAlPortapapeles (original, sobre ListView): copia al
        /// portapapeles el texto de la celda concreta bajo el cursor en el doble clic. El
        /// original mostraba un ToolTip flotante de WinForms junto al cursor; aquí se sustituye
        /// por un mensaje transitorio en la barra inferior (LblCopiado), que desaparece solo tras
        /// ~1.2s con un DispatcherTimer, ya que WPF no tiene un equivalente directo de
        /// ToolTip.Show(texto, control, x, y, duración).</summary>
        private void GridSets_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var celda = BuscarAncestro<DataGridCell>(e.OriginalSource as DependencyObject);
            if (celda?.Content is TextBlock textBlock)
                CopiarTexto(textBlock.Text);
        }

        private void ListCodigosNoXml_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListCodigosNoXml.SelectedItem is string texto)
                CopiarTexto(texto);
        }

        private static T? BuscarAncestro<T>(DependencyObject? origen) where T : DependencyObject
        {
            while (origen != null && origen is not T)
                origen = VisualTreeHelper.GetParent(origen);
            return origen as T;
        }

        private void CopiarTexto(string? texto)
        {
            if (string.IsNullOrEmpty(texto)) return;

            Clipboard.SetText(texto);

            LblCopiado.Text = Loc("L_Suite_Copiado") + texto;
            LblCopiado.Visibility = Visibility.Visible;

            _timerCopiado?.Stop();
            _timerCopiado = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1200) };
            _timerCopiado.Tick += (_, _) =>
            {
                LblCopiado.Visibility = Visibility.Collapsed;
                _timerCopiado?.Stop();
            };
            _timerCopiado.Start();
        }

        #endregion

        #region Exportar a Excel (NPOI)

        private void BtnExportIncluidos_Click(object sender, RoutedEventArgs e) =>
            ExportarConDialogo("SetsEnConector.xlsx", _setsIncluidosList, "Sets incluidos en el conector");

        private void BtnExportNoIncluidos_Click(object sender, RoutedEventArgs e) =>
            ExportarConDialogo("SetsNOEnConector.xlsx", _setsNoIncluidosList, "Sets NO incluidos en el conector");

        private void BtnExportCodigos_Click(object sender, RoutedEventArgs e) =>
            ExportarConDialogo("CodigosEnConector.xlsx", _codigosNoIncluidosEnXml, "Códigos NO incluidos en el XML");

        private void ExportarConDialogo(string nombreSugerido, List<Set> sets, string tituloHoja)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                Title = "Save as",
                FileName = nombreSugerido,
            };

            if (saveFileDialog.ShowDialog() != true) return;

            bool ok = ExportarExcel(saveFileDialog.FileName, sets, tituloHoja);
            MostrarResultadoExportacion(ok);
        }

        private void ExportarConDialogo(string nombreSugerido, List<string> codigos, string tituloHoja)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                Title = "Save as",
                FileName = nombreSugerido,
            };

            if (saveFileDialog.ShowDialog() != true) return;

            bool ok = ExportarExcel(saveFileDialog.FileName, codigos, tituloHoja);
            MostrarResultadoExportacion(ok);
        }

        private static void MostrarResultadoExportacion(bool ok)
        {
            MessageBox.Show(
                ok ? RotoTools.LocalizationManager.GetString("L_GuardadoCorrectamente") : Loc("L_Suite_ErrorExportarExcel"),
                "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool ExportarExcel(string excelPath, List<Set> setsExportar, string tituloHoja)
        {
            try
            {
                var workbook = new XSSFWorkbook();
                ISheet hoja = workbook.CreateSheet(tituloHoja);
                CreateHeader(hoja);

                int filaActual = 1;
                foreach (Set set in setsExportar)
                {
                    IRow fila = hoja.CreateRow(filaActual++);
                    int col = 0;
                    fila.CreateCell(col++).SetCellValue(set.Id);
                    fila.CreateCell(col++).SetCellValue(set.Code);
                }

                SetColumnsWidth(hoja);

                using var fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write);
                workbook.Write(fs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ExportarExcel(string excelPath, List<string> codigosExportar, string tituloHoja)
        {
            try
            {
                var workbook = new XSSFWorkbook();
                ISheet hoja = workbook.CreateSheet(tituloHoja);

                IRow filaCabecera = hoja.CreateRow(0);
                filaCabecera.CreateCell(0).SetCellValue("Código");

                int filaActual = 1;
                foreach (string codigo in codigosExportar)
                {
                    IRow fila = hoja.CreateRow(filaActual++);
                    fila.CreateCell(0).SetCellValue(codigo);
                }

                hoja.SetColumnWidth(0, 60 * 256);

                using var fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write);
                workbook.Write(fs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CreateHeader(ISheet hoja)
        {
            IRow filaCabecera = hoja.CreateRow(0);
            int col = 0;
            filaCabecera.CreateCell(col++).SetCellValue("Id");
            filaCabecera.CreateCell(col++).SetCellValue("Código");
        }

        private static void SetColumnsWidth(ISheet hoja)
        {
            int col = 0;
            hoja.SetColumnWidth(col++, 10 * 256);
            hoja.SetColumnWidth(col++, 60 * 256);
        }

        #endregion

        #region Eliminar líneas no usadas del conector

        /// <summary>Igual que btn_EliminarLineasConector_Click (original): quita del conector
        /// cargado en memoria los nodos cuyo FittingCode no corresponde a ningún Set del XML,
        /// serializa el resultado y actualiza la fila en BBDD (localizada por
        /// connectorHerraje.ConnectorCode, que solo se rellena al deserializar el XML, no por el
        /// texto del combo).</summary>
        private void BtnEliminarLineas_Click(object sender, RoutedEventArgs e)
        {
            if (_connectorHerraje == null)
            {
                MessageBox.Show(Loc("L_Suite_NoHayConectorCargado"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_codigosNoIncluidosEnXml.Count == 0)
            {
                MessageBox.Show(Loc("L_Suite_NoHayLineasParaEliminar"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirm = MessageBox.Show(
                Loc("L_Suite_ConfirmarEliminarLineas"),
                Loc("L_Suite_ConfirmarEliminacion"), MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                int antes = _connectorHerraje.Nodes.Count;

                _connectorHerraje.Nodes = _connectorHerraje.Nodes
                    .Where(n => !_codigosNoIncluidosEnXml.Contains(n.FittingCode, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                int eliminados = antes - _connectorHerraje.Nodes.Count;

                string xmlActualizado = RotoTools.Helpers.SerializarXml(_connectorHerraje);

                using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                {
                    conn.Open();
                    using var cmd = new SqlCommand("UPDATE ConectorHerrajes SET XML = @Xml Where Codigo = @Codigo;", conn);
                    cmd.Parameters.AddWithValue("@Xml", xmlActualizado);
                    cmd.Parameters.AddWithValue("@Codigo", _connectorHerraje.ConnectorCode);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(
                    string.Format(Loc("L_Suite_LineasEliminadas"), eliminados),
                    Loc("L_Suite_OperacionCompletada"), MessageBoxButton.OK, MessageBoxImage.Information);

                FillData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc("L_Suite_ErrorEliminarLineas") + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion
    }
}
