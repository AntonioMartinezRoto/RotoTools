using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.Actualizador
{
    /// <summary>
    /// Nueva (no existía en el original): asocia uno o varios Escandallos al nodo
    /// psr:ConstructiveScript de cada elemento hoja del XML embebido de uno o varios dibujos (tabla
    /// Dibujos, columna Buffer, comprimido). Toda la lógica de BBDD/XML vive en
    /// DibujoConstructivosService; esta ventana solo se encarga de elegir qué Escandallos, con qué
    /// Variables, y a qué Dibujos, reutilizando dos veces el mismo patrón árbol+grid+buscador+
    /// seleccionados de ConfiguradorOpcionesAnadirRotoWindow (una vez para Escandallos, otra para
    /// Dibujos).
    /// </summary>
    public partial class ActualizadorAsociarConstructivosWindow : Window
    {
        private List<EscandalloRow> _todosEscandallos = new();
        private readonly ObservableCollection<EscandalloRow> _escandallosVisibles = new();
        private readonly ObservableCollection<EscandalloTreeNode> _nodosRaizEscandallos = new();
        private readonly ObservableCollection<EscandalloSeleccionado> _escandallosSeleccionados = new();

        private List<DibujoRow> _todosDibujos = new();
        private readonly ObservableCollection<DibujoRow> _dibujosVisibles = new();
        private readonly ObservableCollection<DibujoTreeNode> _nodosRaizDibujos = new();
        private readonly ObservableCollection<DibujoRow> _seleccionados = new();

        public ActualizadorAsociarConstructivosWindow()
        {
            InitializeComponent();

            GridEscandallos.ItemsSource = _escandallosVisibles;
            TreeEscandallos.ItemsSource = _nodosRaizEscandallos;
            GridEscSeleccionados.ItemsSource = _escandallosSeleccionados;

            GridDibujos.ItemsSource = _dibujosVisibles;
            TreeDibujos.ItemsSource = _nodosRaizDibujos;
            GridSeleccionados.ItemsSource = _seleccionados;

            CargarTextos();
            CargarEscandallos();
            CargarDibujos();
        }

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        #region Localización / cabecera

        private void CargarTextos()
        {
            Title = Loc("L_Suite_AsociarConstructivos");
            TxtTitulo.Text = Title;
            TxtSubtitulo.Text = Loc("L_Suite_AsociarConstructivosSubtitulo");

            TxtCarpetasEscandallos.Text = Loc("L_Suite_Carpetas");
            LblTodosEscandallos.Text = Loc("L_Suite_TodosEscandallosHint");
            LblEscSeleccionados.Text = Loc("L_Suite_EscandallosSeleccionados");
            TxtBtnLimpiarEscSeleccionados.Text = RotoTools.LocalizationManager.GetString("L_Limpiar");
            LblVariables.Text = Loc("L_Suite_Variables");

            ColEscCodigo.Header = Loc("L_Suite_Codigo");
            ColEscDescripcion.Header = RotoTools.LocalizationManager.GetString("L_Descripcion");
            string nivel = RotoTools.LocalizationManager.GetString("L_Nivel");
            ColEscNivel1.Header = nivel + " 1";
            ColEscNivel2.Header = nivel + " 2";
            ColEscNivel3.Header = nivel + " 3";
            ColEscNivel4.Header = nivel + " 4";
            ColEscNivel5.Header = nivel + " 5";
            ColEscSelCodigo.Header = Loc("L_Suite_Codigo");
            ColEscSelDescripcion.Header = RotoTools.LocalizationManager.GetString("L_Descripcion");

            TxtCarpetasDibujos.Text = Loc("L_Suite_Carpetas");
            LblTodosDibujos.Text = Loc("L_Suite_TodosDibujosHint");
            LblSeleccionados.Text = Loc("L_Suite_DibujosSeleccionados");
            TxtBtnLimpiarSeleccionados.Text = RotoTools.LocalizationManager.GetString("L_Limpiar");

            ColDibujoCodigo.Header = Loc("L_Suite_Codigo");
            ColDibujoDescripcion.Header = RotoTools.LocalizationManager.GetString("L_Descripcion");
            ColDibujoSistema.Header = Loc("L_Suite_Sistema");
            ColDibujoNivel1.Header = nivel + " 1";
            ColDibujoNivel2.Header = nivel + " 2";
            ColDibujoNivel3.Header = nivel + " 3";
            ColDibujoNivel4.Header = nivel + " 4";
            ColDibujoNivel5.Header = nivel + " 5";
            ColSelCodigo.Header = Loc("L_Suite_Codigo");
            ColSelDescripcion.Header = RotoTools.LocalizationManager.GetString("L_Descripcion");

            Resources["TooltipQuitarSeleccionado"] = RotoTools.LocalizationManager.GetString("L_Quitar");

            TxtBtnAplicar.Text = Loc("L_Suite_Aplicar");
            TxtBtnVolver.Text = RotoTools.LocalizationManager.GetString("L_Volver");
        }

        #endregion

        #region Carga de Escandallos (árbol + grid, mismo patrón que Dibujos más abajo)

        private void CargarEscandallos()
        {
            try
            {
                _todosEscandallos = DibujoConstructivosService.GetEscandallos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc("L_Suite_ErrorCargandoEscandallos") + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
                _todosEscandallos = new List<EscandalloRow>();
            }

            CargarTreeViewEscandallos();
            CargarGridEscandallos();
        }

        private void CargarTreeViewEscandallos()
        {
            _nodosRaizEscandallos.Clear();

            foreach (var fila in _todosEscandallos)
            {
                if (string.IsNullOrWhiteSpace(fila.Codigo)) continue;

                string[] niveles = { fila.Nivel1, fila.Nivel2, fila.Nivel3, fila.Nivel4, fila.Nivel5 };
                IList<EscandalloTreeNode> nodosActuales = _nodosRaizEscandallos;
                EscandalloTreeNode? ultimoNodo = null;

                foreach (var nivelRaw in niveles)
                {
                    if (string.IsNullOrWhiteSpace(nivelRaw)) break;

                    string nivel = nivelRaw.Trim();
                    var existente = nodosActuales.FirstOrDefault(n => !n.EsHoja && string.Equals(n.Texto, nivel, StringComparison.OrdinalIgnoreCase));
                    if (existente == null)
                    {
                        existente = new EscandalloTreeNode { Texto = nivel };
                        nodosActuales.Add(existente);
                    }

                    ultimoNodo = existente;
                    nodosActuales = existente.Hijos;
                }

                var hoja = new EscandalloTreeNode { Texto = $"{fila.Codigo} - {fila.Descripcion}", Codigo = fila.Codigo };
                if (ultimoNodo != null) ultimoNodo.Hijos.Add(hoja);
                else _nodosRaizEscandallos.Add(hoja);
            }
        }

        private void CargarGridEscandallos()
        {
            _escandallosVisibles.Clear();
            foreach (var fila in _todosEscandallos) _escandallosVisibles.Add(fila);
        }

        private void TxtBuscarEscandallos_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = (TxtBuscarEscandallos.Text ?? "").Trim();
            _escandallosVisibles.Clear();

            IEnumerable<EscandalloRow> query = string.IsNullOrEmpty(texto)
                ? _todosEscandallos
                : _todosEscandallos.Where(d =>
                    (d.Codigo?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Descripcion?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false));

            foreach (var fila in query) _escandallosVisibles.Add(fila);
        }

        private void GridEscandallos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridEscandallos.SelectedItem is EscandalloRow fila)
                AgregarEscandalloASeleccionados(fila.Codigo, fila.Descripcion);
        }

        private void GridEscandallos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridEscandallos.SelectedItem is EscandalloRow fila)
                SeleccionarNodoEnTreeViewEscandallos(fila.Codigo);
        }

        private void SeleccionarNodoEnTreeViewEscandallos(string codigo)
        {
            var ruta = new List<EscandalloTreeNode>();

            bool BuscarYExpandir(IEnumerable<EscandalloTreeNode> nodos)
            {
                foreach (var nodo in nodos)
                {
                    if (nodo.EsHoja && string.Equals(nodo.Codigo, codigo, StringComparison.OrdinalIgnoreCase))
                    {
                        nodo.IsSelected = true;
                        ruta.Add(nodo);
                        return true;
                    }

                    if (BuscarYExpandir(nodo.Hijos))
                    {
                        nodo.IsExpanded = true;
                        ruta.Add(nodo);
                        return true;
                    }
                }
                return false;
            }

            if (BuscarYExpandir(_nodosRaizEscandallos))
            {
                ruta.Reverse();
                DesplazarTreeViewHastaNodo(TreeEscandallos, ruta.Cast<object>().ToList());
            }
        }

        private void TreeEscandallos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ObtenerTreeViewItemDesdeOrigen(e.OriginalSource as DependencyObject);
            if (item?.DataContext is not EscandalloTreeNode nodo) return;

            if (nodo.EsHoja)
            {
                var fila = _todosEscandallos.FirstOrDefault(d => string.Equals(d.Codigo, nodo.Codigo, StringComparison.OrdinalIgnoreCase));
                AgregarEscandalloASeleccionados(nodo.Codigo!, fila?.Descripcion ?? "");
            }
            else
            {
                AgregarEscandallosDeNodoASeleccionados(nodo);
            }
        }

        private void AgregarEscandallosDeNodoASeleccionados(EscandalloTreeNode nodoCarpeta)
        {
            var codigos = new List<string>();
            void Recolectar(EscandalloTreeNode nodo)
            {
                if (nodo.EsHoja) { codigos.Add(nodo.Codigo!); return; }
                foreach (var hijo in nodo.Hijos) Recolectar(hijo);
            }
            Recolectar(nodoCarpeta);

            foreach (var codigo in codigos)
            {
                var fila = _todosEscandallos.FirstOrDefault(d => string.Equals(d.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
                AgregarEscandalloASeleccionados(codigo, fila?.Descripcion ?? "");
            }
        }

        private void AgregarEscandalloASeleccionados(string? codigo, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return;

            if (_escandallosSeleccionados.Any(d => string.Equals(d.Codigo, codigo, StringComparison.OrdinalIgnoreCase)))
                return;

            _escandallosSeleccionados.Add(new EscandalloSeleccionado { Codigo = codigo, Descripcion = descripcion });
        }

        private void BtnQuitarEscSeleccionado_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is EscandalloSeleccionado fila)
                _escandallosSeleccionados.Remove(fila);
        }

        private void BtnLimpiarEscSeleccionados_Click(object sender, RoutedEventArgs e) => _escandallosSeleccionados.Clear();

        #endregion

        #region Carga de Dibujos (árbol + grid, igual que ConfiguradorOpcionesAnadirRotoWindow)

        private void CargarDibujos()
        {
            try
            {
                _todosDibujos = DibujoOpcionesRotoService.GetDibujos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc("L_Suite_ErrorCargandoDibujos") + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
                _todosDibujos = new List<DibujoRow>();
            }

            CargarTreeViewDibujos();
            CargarGridDibujos();
        }

        private void CargarTreeViewDibujos()
        {
            _nodosRaizDibujos.Clear();

            foreach (var fila in _todosDibujos)
            {
                if (string.IsNullOrWhiteSpace(fila.Codigo)) continue;

                string[] niveles = { fila.Nivel1, fila.Nivel2, fila.Nivel3, fila.Nivel4, fila.Nivel5 };
                IList<DibujoTreeNode> nodosActuales = _nodosRaizDibujos;
                DibujoTreeNode? ultimoNodo = null;

                foreach (var nivelRaw in niveles)
                {
                    if (string.IsNullOrWhiteSpace(nivelRaw)) break;

                    string nivel = nivelRaw.Trim();
                    var existente = nodosActuales.FirstOrDefault(n => !n.EsHoja && string.Equals(n.Texto, nivel, StringComparison.OrdinalIgnoreCase));
                    if (existente == null)
                    {
                        existente = new DibujoTreeNode { Texto = nivel };
                        nodosActuales.Add(existente);
                    }

                    ultimoNodo = existente;
                    nodosActuales = existente.Hijos;
                }

                var hoja = new DibujoTreeNode { Texto = $"{fila.Codigo} - {fila.Descripcion}", Codigo = fila.Codigo };
                if (ultimoNodo != null) ultimoNodo.Hijos.Add(hoja);
                else _nodosRaizDibujos.Add(hoja);
            }
        }

        private void CargarGridDibujos()
        {
            _dibujosVisibles.Clear();
            foreach (var fila in _todosDibujos) _dibujosVisibles.Add(fila);
        }

        private void TxtBuscarDibujos_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = (TxtBuscarDibujos.Text ?? "").Trim();
            _dibujosVisibles.Clear();

            IEnumerable<DibujoRow> query = string.IsNullOrEmpty(texto)
                ? _todosDibujos
                : _todosDibujos.Where(d =>
                    (d.Codigo?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Descripcion?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false));

            foreach (var fila in query) _dibujosVisibles.Add(fila);
        }

        private void GridDibujos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridDibujos.SelectedItem is DibujoRow fila)
                AgregarDibujoASeleccionados(fila.Codigo);
        }

        private void GridDibujos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridDibujos.SelectedItem is DibujoRow fila)
                SeleccionarNodoEnTreeViewDibujos(fila.Codigo);
        }

        private void SeleccionarNodoEnTreeViewDibujos(string codigo)
        {
            var ruta = new List<DibujoTreeNode>();

            bool BuscarYExpandir(IEnumerable<DibujoTreeNode> nodos)
            {
                foreach (var nodo in nodos)
                {
                    if (nodo.EsHoja && string.Equals(nodo.Codigo, codigo, StringComparison.OrdinalIgnoreCase))
                    {
                        nodo.IsSelected = true;
                        ruta.Add(nodo);
                        return true;
                    }

                    if (BuscarYExpandir(nodo.Hijos))
                    {
                        nodo.IsExpanded = true;
                        ruta.Add(nodo);
                        return true;
                    }
                }
                return false;
            }

            if (BuscarYExpandir(_nodosRaizDibujos))
            {
                ruta.Reverse();
                DesplazarTreeViewHastaNodo(TreeDibujos, ruta.Cast<object>().ToList());
            }
        }

        private void TreeDibujos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ObtenerTreeViewItemDesdeOrigen(e.OriginalSource as DependencyObject);
            if (item?.DataContext is not DibujoTreeNode nodo) return;

            if (nodo.EsHoja) AgregarDibujoASeleccionados(nodo.Codigo!);
            else AgregarDibujosDeNodoASeleccionados(nodo);
        }

        private void AgregarDibujosDeNodoASeleccionados(DibujoTreeNode nodoCarpeta)
        {
            var codigos = new List<string>();
            void Recolectar(DibujoTreeNode nodo)
            {
                if (nodo.EsHoja) { codigos.Add(nodo.Codigo!); return; }
                foreach (var hijo in nodo.Hijos) Recolectar(hijo);
            }
            Recolectar(nodoCarpeta);

            foreach (var codigo in codigos) AgregarDibujoASeleccionados(codigo);
        }

        private void AgregarDibujoASeleccionados(string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return;

            if (_seleccionados.Any(d => string.Equals(d.Codigo, codigo, StringComparison.OrdinalIgnoreCase)))
                return;

            var fila = _todosDibujos.FirstOrDefault(d => string.Equals(d.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
            if (fila != null) _seleccionados.Add(fila);
        }

        private void BtnQuitarSeleccionado_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is DibujoRow fila)
                _seleccionados.Remove(fila);
        }

        private void BtnLimpiarSeleccionados_Click(object sender, RoutedEventArgs e) => _seleccionados.Clear();

        #endregion

        #region Utilidades comunes de árbol (Cam3DWindow-style: BringIntoView tras expandir)

        private static TreeViewItem? ObtenerTreeViewItemDesdeOrigen(DependencyObject? source)
        {
            while (source != null && source is not TreeViewItem)
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
        }

        /// <summary>A diferencia de TreeView.SelectedItem, marcar IsSelected=true en el contenedor
        /// (TreeViewItem) vía binding NO hace scroll automático para dejarlo visible. Además, los
        /// contenedores de los nodos que se acaban de expandir todavía no existen en el momento en
        /// que se asigna: hay que esperar (DispatcherPriority.ContextIdle) a que el árbol visual los
        /// genere y bajar nivel a nivel con ItemContainerGenerator.ContainerFromItem (que solo busca
        /// en los hijos directos, no de forma recursiva) hasta llegar al TreeViewItem de la hoja.
        /// Compartida por los dos árboles (Escandallos y Dibujos): recibe el TreeView y la ruta de
        /// nodos (de raíz a hoja) como object, ya que DibujoTreeNode y EscandalloTreeNode son tipos
        /// distintos sin interfaz común.</summary>
        private void DesplazarTreeViewHastaNodo(ItemsControl arbol, List<object> ruta)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ItemsControl contenedorActual = arbol;
                TreeViewItem? item = null;

                foreach (var nodo in ruta)
                {
                    contenedorActual.UpdateLayout();
                    item = contenedorActual.ItemContainerGenerator.ContainerFromItem(nodo) as TreeViewItem;
                    if (item == null) return;
                    contenedorActual = item;
                }

                item?.BringIntoView();
            }), DispatcherPriority.ContextIdle);
        }

        #endregion

        #region Aplicar

        /// <summary>
        /// Pide confirmación (esto escribe en BBDD y no se puede deshacer), aplica dibujo a dibujo
        /// con DibujoConstructivosService.AplicarConstructivosRoto (siempre por elemento hoja, nunca
        /// al modelo general) y muestra un resumen final agregando éxitos/fallos, escandallos
        /// añadidos/ya existentes y elementos hoja modificados.
        /// </summary>
        private void BtnAplicar_Click(object sender, RoutedEventArgs e)
        {
            if (_escandallosSeleccionados.Count == 0)
            {
                MessageBox.Show(Loc("L_Suite_SeleccionaAlMenosUnEscandallo"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_seleccionados.Count == 0)
            {
                MessageBox.Show(Loc("L_Suite_SeleccionaAlMenosUnDibujo"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string variables = TxtVariables.Text ?? "";
            if (string.IsNullOrWhiteSpace(variables))
            {
                MessageBox.Show(Loc("L_Suite_EscribeVariables"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var escandallos = _escandallosSeleccionados.Select(esc => (esc.Codigo, variables)).ToList();

            string mensajeConfirmacion = string.Format(Loc("L_Suite_ConfirmarAsociarConstructivos"), _escandallosSeleccionados.Count, _seleccionados.Count);
            if (MessageBox.Show(mensajeConfirmacion, Loc("L_Suite_ConfirmarAplicar"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var resultados = new List<ResultadoAplicarConstructivo>();
            var lista = _seleccionados.ToList();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                BtnAplicar.IsEnabled = false;
                BtnVolver.IsEnabled = false;
                MostrarProgreso(0, lista.Count);

                for (int i = 0; i < lista.Count; i++)
                {
                    var resultado = DibujoConstructivosService.AplicarConstructivosRoto(lista[i].Codigo, escandallos);
                    resultados.Add(resultado);
                    MostrarProgreso(i + 1, lista.Count);
                    DoEvents();
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
                BtnAplicar.IsEnabled = true;
                BtnVolver.IsEnabled = true;
                OcultarProgreso();
            }

            MostrarResumen(resultados);
        }

        private void MostrarProgreso(int hechos, int total)
        {
            PanelProgreso.Visibility = Visibility.Visible;
            BarraProgreso.Maximum = total;
            BarraProgreso.Value = hechos;
            LblProgreso.Text = string.Format(Loc("L_Suite_AplicandoOpcionesProgreso"), hechos, total);
        }

        private void OcultarProgreso()
        {
            PanelProgreso.Visibility = Visibility.Collapsed;
        }

        private void MostrarResumen(List<ResultadoAplicarConstructivo> resultados)
        {
            int exitosos = resultados.Count(r => r.Exito);
            int totalAnadidos = resultados.Where(r => r.Exito).Sum(r => r.EscandallosAnadidos);
            int totalYaExistian = resultados.Where(r => r.Exito).Sum(r => r.EscandallosYaExistian);
            int totalElementos = resultados.Where(r => r.Exito).Sum(r => r.ElementosModificados);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(Loc("L_Suite_ResumenAsociarConstructivos"),
                exitosos, resultados.Count, totalAnadidos, totalYaExistian, totalElementos));

            var fallidos = resultados.Where(r => !r.Exito).ToList();
            if (fallidos.Count > 0)
            {
                sb.AppendLine();
                foreach (var fallo in fallidos)
                    sb.AppendLine(string.Format(Loc("L_Suite_ErrorEnDibujo"), fallo.Codigo, fallo.Mensaje));
            }

            MessageBox.Show(sb.ToString(), Loc("L_Suite_OperacionCompletada"),
                MessageBoxButton.OK, fallidos.Count > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }

        #endregion

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Close();

        /// <summary>Igual que en ConfiguradorOpcionesAnadirRotoWindow/CamPage: bombea el bucle de
        /// mensajes para que el cursor de espera y la barra de progreso se repinten mientras se
        /// procesan varios dibujos seguidos.</summary>
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
    }
}
