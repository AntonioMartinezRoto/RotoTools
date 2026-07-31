using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using RotoEntities;

namespace RotoTools.Suite.Views.ConectorHerraje
{
    /// <summary>
    /// Sustituye a ConectorHerrajeGenerador.cs/.Designer.cs (WinForms): genera el conector de
    /// herraje (XML descargable o registro en BBDD) a partir de la lista de Sets que
    /// ConectorHerrajeMenu calcula para el XML/tipo de herraje cargado. Reutiliza tal cual la
    /// lógica de negocio del proyecto original (RotoTools.Helpers, RotoTools.LocalizationManager,
    /// RotoEntities.*) vía ProjectReference.
    /// </summary>
    public partial class ConectorHerrajeGeneradorWindow : Window
    {
        private readonly XmlData _xmlOrigen;
        private readonly List<Set> _setsWorkingList;
        private readonly ObservableCollection<SetGridRowVm> _filas = new();
        private ICollectionView? _vistaFilas;

        private bool _initializing;
        private bool _sincronizandoSelectAll;
        private bool _necesarioInsertarOpcionTipoCorredera;
        private bool _necesarioInsertarOpcionTipoLWC;

        public ConectorHerrajeGeneradorWindow(XmlData xmlOrigen, List<Set> setList, string supplierName)
        {
            InitializeComponent();

            _xmlOrigen = xmlOrigen;
            _setsWorkingList = setList ?? new List<Set>();

            CargarTextos();
            TxtConectorName.Text = supplierName ?? "";

            GridSets.ItemsSource = _filas;
            _vistaFilas = CollectionViewSource.GetDefaultView(_filas);
            _vistaFilas.Filter = FiltrarFila;

            CargarFilas();
            CheckOpcionTipoCorredera();
            CheckOpcionTipoVentanaLwc();
            ActualizarInfoConexion();

            // Igual que CheckAllChecks (ConectorHerrajeGenerador.cs): todos los tipos de apertura
            // marcados por defecto, sin disparar todavía el filtrado (_initializing).
            _initializing = true;
            ChkVentanas.IsChecked = true;
            ChkBalconeras.IsChecked = true;
            ChkPuertas.IsChecked = true;
            ChkCorrederas.IsChecked = true;
            ChkElevables.IsChecked = true;
            ChkParalelas.IsChecked = true;
            ChkAbatibles.IsChecked = true;
            ChkPlegables.IsChecked = true;
            _initializing = false;

            AplicarFiltros();

            // Igual que el Load original: al fijar chk_SelectAll.Checked = true se dispara su
            // propio evento y se seleccionan todas las filas actualmente visibles (con los
            // filtros de arriba ya aplicados).
            ChkSeleccionarTodas.IsChecked = true;
        }

        #region Localización / cabecera

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_GenerarConector");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_GenerarConector");

            TxtGuardarGrupoTitulo.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
            LblCodigoConector.Text = RotoTools.LocalizationManager.GetString("L_Codigo");
            ChkPredefinido.Content = RotoTools.LocalizationManager.GetString("L_PonerPredefinido");
            TxtBtnGuardarXml.Text = RotoTools.LocalizationManager.GetString("L_GuardarEnXML");
            TxtBtnGuardarBD.Text = RotoTools.LocalizationManager.GetString("L_GuardarEnBD");

            TxtBuscarGrupoTitulo.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
            ChkVentanas.Content = RotoTools.LocalizationManager.GetString("L_Ventanas");
            ChkBalconeras.Content = RotoTools.LocalizationManager.GetString("L_Balconeras");
            ChkPuertas.Content = RotoTools.LocalizationManager.GetString("L_Puertas");
            ChkCorrederas.Content = RotoTools.LocalizationManager.GetString("L_Correderas");
            ChkElevables.Content = RotoTools.LocalizationManager.GetString("L_Elevables");
            ChkParalelas.Content = RotoTools.LocalizationManager.GetString("L_Paralelas");
            ChkAbatibles.Content = RotoTools.LocalizationManager.GetString("L_Abatibles");
            ChkPlegables.Content = RotoTools.LocalizationManager.GetString("L_Plegables");
        }

        /// <summary>Igual que ConectorHerrajeGenerador_Load: el servidor/base de datos y el
        /// "conector activo" (variable global "Conector Herraje").</summary>
        private void ActualizarInfoConexion()
        {
            string servidor = RotoTools.Helpers.GetServer();
            string baseDatos = RotoTools.Helpers.GetDataBase();
            string conectorActivo = RotoTools.Helpers.GetConectorActivo() ?? "";
            LblConectorActivo.Text = $@"{servidor}\{baseDatos}    ·    " +
                RotoTools.Suite.Services.SuiteLocalization.GetString("L_Suite_ConectorActivo") + ": " + conectorActivo;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Close();

        #endregion

        #region Filas / filtros (equivalente a ConvertSetsToGrid / AplicarFiltros)

        private void CargarFilas()
        {
            _filas.Clear();
            foreach (var set in _setsWorkingList)
            {
                var fila = new SetGridRowVm(set);
                fila.PropertyChanged += Fila_PropertyChanged;
                _filas.Add(fila);
            }
        }

        private void TxtFiltro_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => AplicarFiltros();

        private void ChkTipoApertura_Click(object sender, RoutedEventArgs e) => AplicarFiltros();

        /// <summary>Igual que AplicarFiltros (ConectorHerrajeGenerador.cs): filtro de texto sobre
        /// el Código (AND) combinado con los tipos de apertura marcados (OR entre ellos; si no
        /// hay ninguno marcado, no se filtra por tipo, igual que el original).</summary>
        private void AplicarFiltros()
        {
            if (_initializing) return;
            _vistaFilas?.Refresh();
            ActualizarContadorTotal();
        }

        private bool FiltrarFila(object obj)
        {
            if (obj is not SetGridRowVm fila) return false;

            string texto = (TxtFiltro.Text ?? "").Trim();
            if (!string.IsNullOrEmpty(texto) &&
                (fila.Codigo ?? "").IndexOf(texto, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            var tiposSeleccionados = new List<int>();
            if (ChkVentanas.IsChecked == true) tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.Ventana);
            if (ChkPuertas.IsChecked == true) tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.Puerta);
            if (ChkBalconeras.IsChecked == true)
            {
                tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.Balconera);
                tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.PuertaSecundaria);
            }
            if (ChkElevables.IsChecked == true) tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.Elevable);
            if (ChkCorrederas.IsChecked == true) tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.Corredera);
            if (ChkParalelas.IsChecked == true) tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.Osciloparalela);
            if (ChkAbatibles.IsChecked == true) tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.Abatible);
            if (ChkPlegables.IsChecked == true) tiposSeleccionados.Add((int)RotoTools.Enums.enumWindowType.Plegable);

            if (tiposSeleccionados.Count > 0 && !tiposSeleccionados.Contains(fila.WindowType))
                return false;

            return true;
        }

        private void ActualizarContadorTotal()
        {
            int total = 0;
            if (_vistaFilas != null)
                foreach (var _ in _vistaFilas) total++;

            LblTotal.Text = total + " " + RotoTools.LocalizationManager.GetString("L_Lineas");
        }

        #endregion

        #region Selección (equivalente a chk_SelectAll_CheckedChanged / dataGridView1_CellValueChanged)

        private void ChkSeleccionarTodas_Checked(object sender, RoutedEventArgs e)
        {
            if (_sincronizandoSelectAll) return;
            MarcarFilasVisibles(true);
        }

        private void ChkSeleccionarTodas_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_sincronizandoSelectAll) return;
            MarcarFilasVisibles(false);
        }

        private void MarcarFilasVisibles(bool marcar)
        {
            if (_vistaFilas == null) return;
            foreach (var obj in _vistaFilas)
            {
                if (obj is SetGridRowVm fila) fila.Selected = marcar;
            }
        }

        /// <summary>Igual que dataGridView1_CellValueChanged: cuando cambia la selección de una
        /// fila, se recalcula si "Seleccionar todas" debe quedar marcada (todas las filas
        /// visibles seleccionadas) sin volver a disparar su propio Checked/Unchecked.</summary>
        private void Fila_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SetGridRowVm.Selected)) return;

            bool hayFilas = false;
            bool todasMarcadas = true;
            if (_vistaFilas != null)
            {
                foreach (var obj in _vistaFilas)
                {
                    if (obj is not SetGridRowVm fila) continue;
                    hayFilas = true;
                    if (!fila.Selected) { todasMarcadas = false; break; }
                }
            }

            _sincronizandoSelectAll = true;
            ChkSeleccionarTodas.IsChecked = hayFilas && todasMarcadas;
            _sincronizandoSelectAll = false;
        }

        #endregion

        #region Guardar (equivalente a btn_GenerarConector_Click / btn_InsertConector_Click)

        private void BtnGuardarXml_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Archivo XML (*.xml)|*.xml",
                Title = "Guardar archivo XML"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var conectorHerrajeGenerado = GenerateConnectorXml(_xmlOrigen.Supplier);
                conectorHerrajeGenerado.Save(saveFileDialog.FileName);
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ConectorGenerado"));
            }
        }

        private void BtnGuardarBD_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sql = @"INSERT INTO ConectorHerrajes (DataVerId, Codigo, XML) VALUES (dbo.GetCurrentDVID(), @Codigo, @Xml);";

                if (ExisteConectorEnBD(TxtConectorName.Text))
                {
                    if (MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ExisteConector"), "",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        sql = @"UPDATE ConectorHerrajes SET XML = @Xml Where Codigo = @Codigo;";
                    }
                    else
                    {
                        return;
                    }
                }

                var xmlConector = GenerateConnectorXml(TxtConectorName.Text);

                if (ChkPredefinido.IsChecked == true && !string.IsNullOrEmpty(TxtConectorName.Text))
                {
                    sql += @"UPDATE VARIABLESGLOBALES SET VALOR = '" + TxtConectorName.Text + "' WHERE NOMBRE = 'Conector Herraje';";
                }

                using (var connection = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Codigo", TxtConectorName.Text);
                    command.Parameters.AddWithValue("@Xml", xmlConector.OuterXml);
                    command.ExecuteNonQuery();
                }

                // Si están los sets de corredera e Inowa deben distinguirse con la opción
                // RO_TIPO_CORREDERA y hay que instalarla.
                if (_necesarioInsertarOpcionTipoCorredera)
                    RotoTools.Helpers.InstalarOpcionTipoCorredera();

                // Si están los sets de LOW COST hay que distinguir las tablas STD de LWC con la
                // opción RO_TIPO_VENTANA_STD y hay que instalarla.
                if (_necesarioInsertarOpcionTipoLWC)
                    RotoTools.Helpers.InstalarOpcionTipoLWC();

                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ConectorInsertado"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (7): " + ex.Message);
            }
        }

        private bool ExisteConectorEnBD(string conectorName)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand("SELECT Count(*) FROM ConectorHerrajes WHERE Codigo = '" + conectorName + "'", conexion);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                return Convert.ToInt32(reader[0]?.ToString()) > 0;

            return false;
        }

        /// <summary>Igual que GenerateConnectorXml: construye el XML del conector a partir de los
        /// Sets marcados (Selected) ACTUALMENTE VISIBLES (con los filtros de arriba aplicados) —
        /// igual que el original, que iteraba sobre el BindingSource ya filtrado, así que un Set
        /// marcado pero oculto por el filtro en este momento no se incluye.</summary>
        private XmlDocument GenerateConnectorXml(string conectorCode)
        {
            var doc = new XmlDocument();

            var connectorNode = doc.CreateElement("Connector");
            connectorNode.SetAttribute("Connector_code", conectorCode);
            connectorNode.SetAttribute("Message", "true");
            doc.AppendChild(connectorNode);

            var codigosSeleccionados = ObtenerCodigosSeleccionados();

            var setsSeleccionados = _setsWorkingList
                .Where(s => codigosSeleccionados.Contains(s.Code) || codigosSeleccionados.Contains(s.Script))
                .ToList();

            foreach (var set in setsSeleccionados)
            {
                if (!set.IsTitle && set.OpeningFlagConectorList == null) continue;

                var nodeElement = doc.CreateElement("Node");

                if (!string.IsNullOrEmpty(set.Script))
                {
                    nodeElement.SetAttribute("Script", set.Script);
                    var openingElementScript = doc.CreateElement("Opening");
                    nodeElement.AppendChild(openingElementScript);
                    connectorNode.AppendChild(nodeElement);
                    continue;
                }

                nodeElement.SetAttribute("Fitting_Code", set.Code);

                var openingElement = doc.CreateElement("Opening");
                foreach (var openingFlag in set.OpeningFlagConectorList)
                {
                    var flag = doc.CreateElement("Opening_Flag");
                    flag.SetAttribute("Value", openingFlag.Value);
                    openingElement.AppendChild(flag);
                }
                nodeElement.AppendChild(openingElement);

                var includedOptions = doc.CreateElement("Included_Options");
                var optionsNode = doc.CreateElement("Options");
                foreach (var option in set.OptionConectorList)
                {
                    var optionElement = doc.CreateElement("Option");
                    optionElement.SetAttribute("Name", option.Name);
                    optionElement.SetAttribute("Value", option.Value);
                    optionsNode.AppendChild(optionElement);
                }
                includedOptions.AppendChild(optionsNode);
                nodeElement.AppendChild(includedOptions);

                connectorNode.AppendChild(nodeElement);
            }

            return doc;
        }

        private List<string> ObtenerCodigosSeleccionados()
        {
            var codigos = new List<string>();
            if (_vistaFilas == null) return codigos;

            foreach (var obj in _vistaFilas)
            {
                if (obj is not SetGridRowVm fila || !fila.Selected) continue;

                if (!string.IsNullOrEmpty(fila.Codigo)) codigos.Add(fila.Codigo);
                else if (!string.IsNullOrEmpty(fila.Escandallo)) codigos.Add(fila.Escandallo);
            }
            return codigos;
        }

        #endregion

        #region Opciones especiales (equivalente a CheckOpcionTipoCorredera / CheckOpcionTipoVentanaLwc)

        private void CheckOpcionTipoCorredera()
        {
            bool existenSetsCorredera = _setsWorkingList.Where(x => !string.IsNullOrEmpty(x.Code)).Any(s => s.Code.ToUpper().Contains("CORREDERA"));
            bool existenSetsInowa = _setsWorkingList.Where(x => !string.IsNullOrEmpty(x.Code)).Any(s => s.Code.ToUpper().Contains("INOWA"));

            if (existenSetsCorredera && existenSetsInowa)
                _necesarioInsertarOpcionTipoCorredera = true;
        }

        private void CheckOpcionTipoVentanaLwc()
        {
            bool existenSetsLWC = _setsWorkingList.Where(x => !string.IsNullOrEmpty(x.Code)).Any(s => s.Code.ToUpper().Contains("LWC"));

            if (existenSetsLWC)
                _necesarioInsertarOpcionTipoLWC = true;
        }

        #endregion
    }

    /// <summary>Equivalente a SetGridRow (ConectorHerrajeGenerador.cs): una fila de la grid de
    /// Sets. "Apertura" era una imagen (Properties.Resources.OpeningN) en el original; aquí se
    /// muestra como una etiqueta de texto (ver ObtenerEtiquetaApertura) porque esos recursos
    /// gráficos no están disponibles en este entorno de migración.</summary>
    public class SetGridRowVm : INotifyPropertyChanged
    {
        private bool _selected;

        public SetGridRowVm(Set set)
        {
            Escandallo = set.Script ?? "";
            Codigo = set.Code ?? "";
            WindowType = set.WindowType;
            Apertura = ObtenerEtiquetaApertura(set);
            Opciones = set.OptionConectorList != null && set.OptionConectorList.Count > 0
                ? string.Join(Environment.NewLine, set.OptionConectorList.Select(o => $@"{o.Name}\{o.Value}"))
                : "";
        }

        public string Escandallo { get; }
        public string Apertura { get; }
        public string Opciones { get; }
        public string Codigo { get; }
        public int WindowType { get; }

        public bool Selected
        {
            get => _selected;
            set { if (_selected != value) { _selected = value; OnPropertyChanged(); } }
        }

        private static string ObtenerEtiquetaApertura(Set set)
        {
            if (set.Opening == null) return "";

            return set.Opening.openingType switch
            {
                (int)RotoTools.Enums.enumOpeningType.PracticableIzquierdaInt => "Practicable izda. int.",
                (int)RotoTools.Enums.enumOpeningType.PracticableDerechaInt => "Practicable dcha. int.",
                (int)RotoTools.Enums.enumOpeningType.OscilobatienteIzquierdaInt => "Oscilobatiente izda. int.",
                (int)RotoTools.Enums.enumOpeningType.OscilobatienteDerechaInt => "Oscilobatiente dcha. int.",
                (int)RotoTools.Enums.enumOpeningType.CorrederaDerecha => "Corredera dcha.",
                (int)RotoTools.Enums.enumOpeningType.CorrederaIzquierda => "Corredera izda.",
                (int)RotoTools.Enums.enumOpeningType.CorrederaIzqDcha => "Corredera izda./dcha.",
                (int)RotoTools.Enums.enumOpeningType.Abatible => "Abatible",
                (int)RotoTools.Enums.enumOpeningType.OsciloCorrederaDerecha => "Oscilocorredera dcha.",
                (int)RotoTools.Enums.enumOpeningType.OsciloCorrederaIzquierda => "Oscilocorredera izda.",
                (int)RotoTools.Enums.enumOpeningType.ElevableIzquierda => "Elevable izda.",
                (int)RotoTools.Enums.enumOpeningType.ElevableDerecha => "Elevable dcha.",
                (int)RotoTools.Enums.enumOpeningType.PracticableIzquierdaExt => "Practicable izda. ext.",
                (int)RotoTools.Enums.enumOpeningType.PracticableDerechaExt => "Practicable dcha. ext.",
                _ => ""
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
