using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.ConfiguradorOpciones
{
    /// <summary>
    /// Nueva (no existía en el original): añade la carpeta "ROTO" y una lista de opciones al XML
    /// embebido de uno o varios dibujos (tabla Dibujos, columna Buffer, comprimido). Toda la
    /// lógica de BBDD/XML vive en DibujoOpcionesRotoService; esta ventana solo se encarga de la
    /// selección de fichero de opciones, modo (modelo general / por elemento) y de qué dibujos
    /// aplicar, reutilizando el patrón árbol+grid+seleccionados de Cam3DWindow.
    /// </summary>
    public partial class ConfiguradorOpcionesAnadirRotoWindow : Window
    {
        private List<DibujoRow> _todosDibujos = new();
        private readonly ObservableCollection<DibujoRow> _dibujosVisibles = new();
        private readonly ObservableCollection<DibujoTreeNode> _nodosRaiz = new();
        private readonly ObservableCollection<DibujoRow> _seleccionados = new();

        private List<XElement> _opcionesCargadas = new();
        private string? _nombreFicheroOpciones;
        private OpcionCarpetaTreeNode? _carpetaSeleccionada;

        public ConfiguradorOpcionesAnadirRotoWindow()
        {
            InitializeComponent();

            GridDibujos.ItemsSource = _dibujosVisibles;
            TreeDibujos.ItemsSource = _nodosRaiz;
            GridSeleccionados.ItemsSource = _seleccionados;

            CargarTextos();
            CargarDibujos();
        }

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        #region Localización / cabecera

        private void CargarTextos()
        {
            Title = Loc("L_Suite_AnadirOpcionesRoto");
            TxtTitulo.Text = Title;
            TxtSubtitulo.Text = Loc("L_Suite_AnadirOpcionesRotoSubtitulo");

            TxtBtnCargarOpciones.Text = Loc("L_Suite_CargarXml");
            ActualizarLblOpcionesCargadas();

            RbModoModelo.Content = Loc("L_Suite_ModoModeloGeneral");
            RbModoElemento.Content = Loc("L_Suite_ModoPorElemento");

            LblCarpetaDestino.Text = Loc("L_Suite_CarpetaDestino");
            TxtBtnElegirCarpeta.Text = Loc("L_Suite_ElegirCarpeta");
            ActualizarLblCarpetaDestino();

            TxtCarpetas.Text = Loc("L_Suite_Carpetas");
            LblTodosDibujos.Text = Loc("L_Suite_TodosDibujosHint");
            LblSeleccionados.Text = Loc("L_Suite_DibujosSeleccionados");
            TxtBtnLimpiarSeleccionados.Text = RotoTools.LocalizationManager.GetString("L_Limpiar");

            ColDibujoCodigo.Header = Loc("L_Suite_Codigo");
            ColDibujoDescripcion.Header = RotoTools.LocalizationManager.GetString("L_Descripcion");
            ColDibujoSistema.Header = Loc("L_Suite_Sistema");
            string nivel = RotoTools.LocalizationManager.GetString("L_Nivel");
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

        private void ActualizarLblOpcionesCargadas()
        {
            LblOpcionesCargadas.Text = _opcionesCargadas.Count == 0
                ? Loc("L_Suite_NingunaOpcionCargada")
                : string.Format(Loc("L_Suite_OpcionesCargadasDesde"), _opcionesCargadas.Count, _nombreFicheroOpciones);
        }

        private void ActualizarLblCarpetaDestino()
        {
            LblCarpetaDestinoElegida.Text = _carpetaSeleccionada == null
                ? Loc("L_Suite_NingunaCarpetaSeleccionada")
                : string.Join("\\", _carpetaSeleccionada.Ruta);
        }

        #endregion

        #region Selección de la carpeta destino (árbol de Opciones, Nivel1..5)

        /// <summary>
        /// Abre el árbol de carpetas de la tabla OPCIONES (ver
        /// DibujoOpcionesRotoService.GetArbolCarpetasOpciones) en un diálogo modal: ya no se
        /// escribe la ruta a mano, se elige haciendo clic, para evitar errores de tecleo.
        /// </summary>
        private void BtnElegirCarpeta_Click(object sender, RoutedEventArgs e)
        {
            List<OpcionCarpetaTreeNode> raiz;
            try
            {
                raiz = DibujoOpcionesRotoService.GetArbolCarpetasOpciones();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc("L_Suite_ErrorCargandoCarpetasOpciones") + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dialogo = new SeleccionarCarpetaOpcionesWindow(raiz, _carpetaSeleccionada) { Owner = this };
            if (dialogo.ShowDialog() == true && dialogo.CarpetaSeleccionada != null)
            {
                _carpetaSeleccionada = dialogo.CarpetaSeleccionada;
                ActualizarLblCarpetaDestino();
            }
        }

        #endregion

        #region Carga de dibujos (árbol + grid, igual que Cam3DWindow.CargarMaterialesBase/CargarTreeViewMateriales)

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
            _nodosRaiz.Clear();

            foreach (var fila in _todosDibujos)
            {
                if (string.IsNullOrWhiteSpace(fila.Codigo)) continue;

                string[] niveles = { fila.Nivel1, fila.Nivel2, fila.Nivel3, fila.Nivel4, fila.Nivel5 };
                IList<DibujoTreeNode> nodosActuales = _nodosRaiz;
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
                else _nodosRaiz.Add(hoja);
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
                SeleccionarNodoEnTreeView(fila.Codigo);
        }

        /// <summary>Selecciona (y expande hasta) el nodo hoja cuyo Código coincide, igual que
        /// Cam3DWindow.SeleccionarNodoEnTreeView + BuscarNodoPorTag, adaptado al patrón MVVM de
        /// IsExpanded/IsSelected bindables (ver DibujoTreeNode en DibujoOpcionesRotoService.cs).
        /// Además hace scroll para dejar el nodo visible (ver DesplazarTreeViewHastaNodo).</summary>
        private void SeleccionarNodoEnTreeView(string codigo)
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

            if (BuscarYExpandir(_nodosRaiz))
            {
                ruta.Reverse(); // de raíz a hoja
                DesplazarTreeViewHastaNodo(ruta);
            }
        }

        /// <summary>A diferencia de TreeView.SelectedItem, marcar IsSelected=true en el
        /// contenedor (TreeViewItem) vía binding NO hace scroll automático para dejarlo visible.
        /// Además, los contenedores de los nodos que se acaban de expandir (IsExpanded=true de
        /// arriba) todavía no existen en el momento en que se asigna: hay que esperar
        /// (DispatcherPriority.ContextIdle) a que el árbol visual los genere y bajar nivel a
        /// nivel con ItemContainerGenerator.ContainerFromItem (que solo busca en los hijos
        /// directos, no de forma recursiva) hasta llegar al TreeViewItem de la hoja.</summary>
        private void DesplazarTreeViewHastaNodo(List<DibujoTreeNode> ruta)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ItemsControl contenedorActual = TreeDibujos;
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

        private void TreeDibujos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ObtenerTreeViewItemDesdeOrigen(e.OriginalSource as DependencyObject);
            if (item?.DataContext is not DibujoTreeNode nodo) return;

            if (nodo.EsHoja) AgregarDibujoASeleccionados(nodo.Codigo!);
            else AgregarDibujosDeNodoASeleccionados(nodo);
        }

        private static TreeViewItem? ObtenerTreeViewItemDesdeOrigen(DependencyObject? source)
        {
            while (source != null && source is not TreeViewItem)
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
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

        #endregion

        #region Seleccionados

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

        #region Carga del fichero de opciones

        private void BtnCargarOpciones_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };
            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                var opciones = DibujoOpcionesRotoService.CargarOpcionesDesdeXml(openFileDialog.FileName);
                if (opciones.Count == 0)
                {
                    MessageBox.Show(Loc("L_Suite_NoSeEncontraronOpcionesEnFichero"), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _opcionesCargadas = opciones;
                _nombreFicheroOpciones = System.IO.Path.GetFileName(openFileDialog.FileName);
                ActualizarLblOpcionesCargadas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc("L_Suite_ErrorCargandoOpciones") + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Aplicar

        /// <summary>
        /// Pide confirmación (esto escribe en BBDD y no se puede deshacer, igual criterio que
        /// ConectorHerrajeEliminarWindow.BtnEliminar_Click), aplica dibujo a dibujo con
        /// DibujoOpcionesRotoService.AplicarOpcionesRoto y muestra un resumen final agregando
        /// éxitos/fallos, opciones añadidas/ya existentes y elementos hoja modificados.
        /// </summary>
        private void BtnAplicar_Click(object sender, RoutedEventArgs e)
        {
            if (_opcionesCargadas.Count == 0)
            {
                MessageBox.Show(Loc("L_Suite_CargaPrimeroOpciones"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_seleccionados.Count == 0)
            {
                MessageBox.Show(Loc("L_Suite_SeleccionaAlMenosUnDibujo"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // No se asume que la carpeta se llame siempre "ROTO": se construye a partir de la
            // carpeta elegida por el usuario en el árbol (ver ConstruirNivelCarpeta y
            // BtnElegirCarpeta_Click). Se valida aquí, una sola vez para todo el lote, antes de
            // pedir confirmación y antes de tocar ningún dibujo.
            if (_carpetaSeleccionada == null)
            {
                MessageBox.Show(Loc("L_Suite_SeleccionaCarpetaDestino"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string nivelCarpeta;
            try
            {
                nivelCarpeta = DibujoOpcionesRotoService.ConstruirNivelCarpeta(_carpetaSeleccionada);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool porElemento = RbModoElemento.IsChecked == true;
            string carpetaTexto = string.Join("\\", _carpetaSeleccionada.Ruta);
            string mensajeConfirmacion = string.Format(Loc("L_Suite_ConfirmarAplicarOpcionesRoto"), _seleccionados.Count, carpetaTexto);
            if (MessageBox.Show(mensajeConfirmacion, Loc("L_Suite_ConfirmarAplicar"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var resultados = new List<ResultadoAplicarOpciones>();
            var lista = _seleccionados.ToList();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                BtnAplicar.IsEnabled = false;
                BtnVolver.IsEnabled = false;
                MostrarProgreso(0, lista.Count);

                for (int i = 0; i < lista.Count; i++)
                {
                    var resultado = DibujoOpcionesRotoService.AplicarOpcionesRoto(lista[i].Codigo, _opcionesCargadas, porElemento, nivelCarpeta);
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

        /// <summary>Aplicar puede tardar (cada dibujo es una lectura + escritura en BBDD): se
        /// muestra PanelProgreso con una ProgressBar determinada (Maximum = total de dibujos
        /// seleccionados) y una etiqueta "X de Y", actualizada dibujo a dibujo. DoEvents() (ya
        /// usado antes solo para repintar el cursor de espera) es lo que hace que estos cambios se
        /// vean en pantalla mientras el bucle sigue ejecutándose.</summary>
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

        private void MostrarResumen(List<ResultadoAplicarOpciones> resultados)
        {
            int exitosos = resultados.Count(r => r.Exito);
            int totalAnadidas = resultados.Where(r => r.Exito).Sum(r => r.OpcionesAnadidas);
            int totalYaExistian = resultados.Where(r => r.Exito).Sum(r => r.OpcionesYaExistian);
            int totalElementos = resultados.Where(r => r.Exito).Sum(r => r.ElementosModificados);

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(Loc("L_Suite_ResumenAplicarOpcionesRoto"),
                exitosos, resultados.Count, totalAnadidas, totalYaExistian, totalElementos));

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

        /// <summary>Igual que en CamPage/Cam3DWindow: bombea el bucle de mensajes para que el
        /// cursor de espera se repinte mientras se procesan varios dibujos seguidos.</summary>
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
