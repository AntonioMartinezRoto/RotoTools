using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.Simulacion
{
    /// <summary>Fila editable de "valores reales" (Paso 3): el valor que ya tiene guardado la hoja
    /// para esta opción, que el usuario puede cambiar antes de arrancar la simulación (Q1 del
    /// usuario: "mixto, con opción a cambiarlo"). Vive aquí (no en Services) porque es un DTO de
    /// presentación puro, sin ninguna lógica de negocio ni acceso a BBDD.</summary>
    public sealed class FilaValorReal
    {
        public string Nombre { get; set; } = "";
        public string Valor { get; set; } = "";

        /// <summary>De dónde sale este valor: "Valor real guardado" (psr:Options, tal cual está en
        /// Preference) o "Deducido de Opening.value (tabla de tipos de apertura)" para las opciones
        /// que se autorrespondieron a partir de psr:Opening -Activa, Puerta, Exterior, Elevable,
        /// Oscilobatiente, Practicable, Corredera- (ver HojaSimulacion.ValoresDetectadosHeuristica y
        /// SimulacionDatosService.DetectarDatosDeOpening). Puramente informativo: el usuario puede
        /// editar "Valor" igual en los dos casos.</summary>
        public string Origen { get; set; } = "";
    }

    /// <summary>Fila de "valor posible" al responder una pregunta del asistente (Paso 4), envolviendo
    /// un RotoEntities.ContenidoOpcion con lo justo para mostrarlo. Se muestran TAMBIÉN los valores
    /// ocultos en Preference (Flags != 0), marcados como tales: un valor necesario pero oculto por
    /// error es justo el tipo de problema que esta herramienta de diagnóstico debe poder localizar,
    /// no algo que deba ocultarse aquí también.</summary>
    public sealed class FilaValorPosible
    {
        public string Valor { get; set; } = "";
        public string Texto { get; set; } = "";
        public bool Oculto { get; set; }
        public Visibility VisibilidadEtiquetaOculto => Oculto ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Fila del mapa conceptual final (Paso 5): valor final de una opción + su historial
    /// COMPLETO ya formateado como texto multilínea (Q3 del usuario: "historial completo").</summary>
    public sealed class FilaMapaOpcion
    {
        public string Nombre { get; set; } = "";
        public string ValorFinal { get; set; } = "";
        public string HistorialTexto { get; set; } = "";

        /// <summary>true si el valor final coincide con un valor marcado "Oculto" en Preference
        /// (OcultaEnLista/OcultaEnArbol de ContenidoOpciones) para esta opción real de catálogo —
        /// ver EsValorOculto. Las opciones internas del DSL (que no empiezan por "RO", ver
        /// MostrarPregunta) nunca lo son: no existen en ese catálogo. Por defecto estas filas se
        /// ocultan en el mapa; el check "Mostrar también las ocultas" las desbloquea (petición del
        /// usuario).</summary>
        public bool Oculto { get; set; }

        /// <summary>Origen del último valor aplicado (mismo texto que ya lleva el Historial),
        /// mostrado también como columna propia para verlo de un vistazo sin leer el historial
        /// entero.</summary>
        public string OrigenFinal { get; set; } = "";

        /// <summary>true si esta opción fue tocada más de una vez (Historial.Count > 1): un valor
        /// "pisado" por un escandallo posterior es justo el tipo de caso que esta herramienta busca
        /// localizar (ver comentario de HistorialOpcion), así que se resalta en la grid.</summary>
        public bool Sobrescrita { get; set; }
    }

    /// <summary>Fila del desplegable "Orden" (Paso 1, modo Presupuesto). Petición del usuario: la
    /// columna ContenidoPAFBlob.Orden es de base CERO en BBDD, pero "para el usuario no tiene
    /// sentido que aparezca Orden 0" -en el propio presupuesto, la línea 1 es la primera, no la
    /// línea 0-. ValorReal guarda el valor de BBDD TAL CUAL (lo que de verdad hay que mandar en
    /// las consultas: PresupuestoDatosService.ObtenerOrdenesPresupuesto/LeerXmlDescomprimido,
    /// DibujoImagenService.CargarVistaPreviaPresupuesto, SimulacionDatosService.
    /// ObtenerHojasDesdePresupuesto siguen recibiendo siempre este valor, nunca la Etiqueta);
    /// Etiqueta es solo la cadena que ve el usuario en el desplegable (ValorReal + 1, o ValorReal
    /// tal cual si por lo que sea no fuera numérico -no debería darse, pero así no se rompe el
    /// desplegable por una suposición incorrecta-). El "+1" es puramente de PRESENTACIÓN: no
    /// cambia ni un dato ni una consulta, solo cómo se rotula esta lista.</summary>
    public sealed class FilaOrdenPresupuesto
    {
        public string ValorReal { get; set; } = "";
        public string Etiqueta { get; set; } = "";

        /// <summary>Por si algún control interno de WPF mostrara el elemento vía ToString() en vez
        /// de DisplayMemberPath (p.ej. accesibilidad/automatización): mismo texto que Etiqueta.</summary>
        public override string ToString() => Etiqueta;
    }

    /// <summary>Resuelve HojaSimulacion.AperturaIconoClave (una clave x:Key de texto, p.ej.
    /// "IconAperturaPracticable") a la Geometry real de Theme/RotoBrand.xaml, para el icono de
    /// apertura de cada hoja en el Paso 2 (ver PlantillaHoja en el XAML). Vive aquí (no en
    /// SimulacionDatosService) a propósito: ese Service no tiene ninguna dependencia de WPF -ver su
    /// comentario de clase-, así que la traducción "clave de texto → objeto Geometry real" se hace
    /// en la capa de UI, igual que SetGridRowVm.ObtenerIconoApertura en
    /// ConectorHerrajeGeneradorWindow resuelve su propia clave con Application.Current.
    /// TryFindResource. Null en cualquier caso no resoluble (clave nula, o no encontrada en el
    /// ResourceDictionary) en vez de lanzar: un Path con Data=null simplemente no dibuja nada,
    /// degradación seguía igual que en el resto de esta funcionalidad de solo lectura.</summary>
    internal sealed class ClaveIconoAperturaConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string clave || string.IsNullOrEmpty(clave)) return null;
            return Application.Current.TryFindResource(clave) as Geometry;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    /// <summary>
    /// Nueva (no existía en el original ni en ningún otro módulo de la Suite): asistente de
    /// "Depuración de Escandallos" (antes "Simulación", ver comentario del XAML para el porqué
    /// completo del renombrado). Toda la lógica de recorrido del DSL vive en
    /// EscandalloInterpreter/EscandalloDsl; toda la lectura de hojas/escandallos
    /// asociados/valores reales vive en SimulacionDatosService (Services): esta página solo dirige
    /// el asistente paso a paso (MostrarPaso) y traduce los eventos del intérprete a la UI.
    ///
    /// Solo lectura/diagnóstico: no escribe nada en BBDD ni en el XML de ningún dibujo.
    ///
    /// Localización ES/EN/PT completa (petición del usuario): CargarTextos() fija todos los textos
    /// estáticos vía SuiteLocalization (nuevas claves L_Suite_* propias de este módulo) o, para los
    /// pocos conceptos que ya existían en el original, vía RotoTools.LocalizationManager. Todo
    /// mensaje/log dinámico usa Loc()/string.Format(Loc(...), ...) en vez de texto "a pelo".
    /// </summary>
    public partial class SimulacionPage : UserControl
    {
        /// <summary>Alias de brevedad, mismo patrón que el resto de la Suite ya migrada (ver p.ej.
        /// ConfiguradorOpcionesAnadirRotoWindow.Loc).</summary>
        private static string Loc(string key) => SuiteLocalization.GetString(key);
        /// <summary>De dónde viene el modelo elegido en el Paso 1 (petición del usuario: "que el
        /// usuario pueda elegir cargar un modelo de la base de datos, como hasta ahora, o bien un
        /// modelo guardado en un presupuesto"). Determina qué servicio consultar tanto para la
        /// vista previa del Paso 2 (CargarVistaPreviaDibujo) como para las hojas del Paso 2
        /// (IniciarPaso2ConHojas ya recibe la lista ya calculada, pero SeleccionarDibujo/
        /// BtnPaso1PresupuestoContinuar_Click deciden CUÁL de los dos orígenes consultar según
        /// esto). El resto del asistente (Pasos 2-5) es exactamente el mismo código con
        /// independencia de este valor -petición del propio usuario: "el proceso será el mismo"-.</summary>
        private enum OrigenModeloSimulacion { BaseDatos, Presupuesto }

        private List<DibujoRow> _todosDibujos = new();
        private readonly ObservableCollection<DibujoRow> _dibujosVisibles = new();
        private readonly ObservableCollection<DibujoTreeNode> _nodosRaizDibujos = new();

        private OrigenModeloSimulacion _origenModelo = OrigenModeloSimulacion.BaseDatos;
        private string? _dibujoSeleccionado;

        // Presupuesto (ContenidoPAFBlob) elegido en el Paso 1, modo "Presupuesto": los tres valores
        // de las cascada Número→Versión→Orden, según los va eligiendo el usuario (ver
        // CmbPresupuesto*_SelectionChanged). Se guardan como texto -mismo criterio que
        // PresupuestoDatosService, ver su comentario de clase sobre el tipo real no confirmado de
        // estas tres columnas-.
        private string? _presupuestoNumero;
        private string? _presupuestoVersion;
        private string? _presupuestoOrden;

        private List<HojaSimulacion> _hojas = new();
        private HojaSimulacion? _hojaSeleccionada;

        private List<FilaValorReal> _valoresReales = new();

        private EscandalloInterpreter? _interprete;
        private IEnumerator<EventoSimulacion>? _enumerador;
        private EventoSimulacion? _preguntaActual;
        private readonly ObservableCollection<string> _log = new();

        private List<FilaMapaOpcion> _mapaCompleto = new();

        public SimulacionPage()
        {
            InitializeComponent();

            CargarTextos();

            GridDibujos.ItemsSource = _dibujosVisibles;
            TreeDibujos.ItemsSource = _nodosRaizDibujos;
            ListLog.ItemsSource = _log;

            CargarDibujos();
            MostrarPaso(1);
        }

        /// <summary>Fija todos los textos estáticos de la página (título/subtítulo, breadcrumbs de
        /// paso, botones, cabeceras de columna, checks, tooltips...) vía SuiteLocalization. Se llama
        /// una sola vez desde el constructor, después de InitializeComponent -mismo patrón que el
        /// resto de la Suite ya migrada, ver p.ej. ConfiguradorOpcionesPage.CargarTextos-. Los
        /// textos DINÁMICOS (mensajes de error con datos insertados, líneas del Recorrido...) no se
        /// fijan aquí: usan Loc()/string.Format(Loc(...), ...) en el punto donde se generan.</summary>
        private void CargarTextos()
        {
            TxtTituloPagina.Text = Loc("L_Suite_ModuloDepuracionEscandallos");
            TxtSubtituloPagina.Text = Loc("L_Suite_DepuracionEscandallosSubtitulo");

            RbOrigenBaseDatos.Content = Loc("L_Suite_OrigenBaseDatos");
            RbOrigenPresupuesto.Content = Loc("L_Suite_OrigenPresupuesto");

            TxtCarpetasPaso1.Text = Loc("L_Suite_Carpetas");
            TxtElegirModeloDobleClic.Text = Loc("L_Suite_ElegirModeloDobleClic");
            ColDibujoCodigo.Header = Loc("L_Suite_Codigo");
            ColDibujoDescripcion.Header = RotoTools.LocalizationManager.GetString("L_Descripcion");
            ColDibujoSistema.Header = Loc("L_Suite_Sistema");
            ColDibujoNivel1.Header = Loc("L_Suite_Nivel1");
            ColDibujoNivel2.Header = Loc("L_Suite_Nivel2");
            ColDibujoNivel3.Header = Loc("L_Suite_Nivel3");
            ColDibujoNivel4.Header = Loc("L_Suite_Nivel4");
            ColDibujoNivel5.Header = Loc("L_Suite_Nivel5");

            TxtElegirPresupuesto.Text = Loc("L_Suite_ElegirPresupuesto");
            TxtEtiquetaNumero.Text = Loc("L_Suite_Numero");
            TxtEtiquetaVersion.Text = Loc("L_Suite_Version");
            TxtEtiquetaOrdenLinea.Text = Loc("L_Suite_OrdenLinea");
            BtnPaso1PresupuestoContinuar.Content = Loc("L_Suite_ContinuarFlecha");
            TxtVistaPreviaPresupuestoAviso.Text = Loc("L_Suite_ElegeNumVersOrdenVistaPrevia");
            BtnZoomPresupuestoAjustar.Content = Loc("L_Suite_Ajustar");

            TxtVistaPreviaAviso.Text = Loc("L_Suite_CargandoVistaPreviaDibujo");
            BtnZoomAjustar.Content = Loc("L_Suite_Ajustar");
            BtnPaso2Atras.Content = Loc("L_Suite_AtrasFlecha");
            BtnPaso2Continuar.Content = Loc("L_Suite_ContinuarFlecha");

            TxtTituloEscandalloInicial.Text = Loc("L_Suite_EscandalloDePartida");
            TxtValoresRealesDescripcion.Text = Loc("L_Suite_ValoresRealesDescripcion");
            ChkMostrarOtrasOpcionesReales.Content = Loc("L_Suite_MostrarOtrasOpcionesNoRoto");
            ColValorRealOpcion.Header = Loc("L_Suite_Opcion");
            ColValorRealValor.Header = Loc("L_Suite_ValorReal");
            ColValorRealOrigen.Header = Loc("L_Suite_Origen");
            BtnPaso3Atras.Content = Loc("L_Suite_AtrasFlecha");
            BtnIniciarSimulacion.Content = Loc("L_Suite_IniciarSimulacionFlecha");

            TxtTituloRecorrido.Text = Loc("L_Suite_Recorrido");
            TxtTituloPregunta.Text = Loc("L_Suite_Pregunta");
            BtnResponder.Content = Loc("L_Suite_ResponderConValorElegido");
            TxtOtroValor.Text = Loc("L_Suite_OtroValorDosPuntos");
            BtnResponderLibre.Content = Loc("L_Suite_Responder");
            BtnReiniciarPaso4.Content = Loc("L_Suite_ReiniciarSimulacion");
            BtnVerMapaConceptual.Content = Loc("L_Suite_VerMapaConceptualFinal");

            TxtTituloMapaConceptual.Text = Loc("L_Suite_MapaConceptualFinal");
            ChkMostrarOcultos.Content = Loc("L_Suite_MostrarOpcionesValorOculto");
            ColMapaOpcion.Header = Loc("L_Suite_Opcion");
            ColMapaValorFinal.Header = Loc("L_Suite_ValorFinal");
            ColMapaOrigenFinal.Header = Loc("L_Suite_OrigenDelValorFinal");
            ColMapaHistorial.Header = Loc("L_Suite_HistorialCompletoOrden");
            BtnNuevaSimulacion.Content = Loc("L_Suite_NuevaSimulacion");
            BtnPaso5Atras.Content = Loc("L_Suite_VolverAlRecorridoPaso4");

            Resources["TxtSufijoOculto"] = " (" + Loc("L_Suite_Oculto") + ")";
            Resources["TooltipValorPisado"] = Loc("L_Suite_TooltipValorPisado");
        }

        #region Navegación entre pasos

        private void MostrarPaso(int paso)
        {
            PanelPaso1.Visibility = paso == 1 ? Visibility.Visible : Visibility.Collapsed;
            PanelPaso2.Visibility = paso == 2 ? Visibility.Visible : Visibility.Collapsed;
            PanelPaso3.Visibility = paso == 3 ? Visibility.Visible : Visibility.Collapsed;
            PanelPaso4.Visibility = paso == 4 ? Visibility.Visible : Visibility.Collapsed;
            PanelPaso5.Visibility = paso == 5 ? Visibility.Visible : Visibility.Collapsed;

            TxtPaso.Text = paso switch
            {
                1 => Loc("L_Suite_Paso1De5ElegirModelo"),
                2 => Loc("L_Suite_Paso2De5ElegirHoja"),
                3 => Loc("L_Suite_Paso3De5ValoresYEscandallo"),
                4 => Loc("L_Suite_Paso4De5Asistente"),
                5 => Loc("L_Suite_Paso5De5MapaConceptual"),
                _ => ""
            };
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            _origenModelo = OrigenModeloSimulacion.BaseDatos;
            _dibujoSeleccionado = null;
            _presupuestoNumero = null;
            _presupuestoVersion = null;
            _presupuestoOrden = null;
            _hojas = new List<HojaSimulacion>();
            _hojaSeleccionada = null;
            _valoresReales = new List<FilaValorReal>();
            _interprete = null;
            _enumerador = null;
            _preguntaActual = null;
            _mapaCompleto = new List<FilaMapaOpcion>();
            _log.Clear();

            TxtErrorPaso1.Visibility = Visibility.Collapsed;
            TxtBuscarDibujos.Text = "";

            // Vuelve también el Paso 1 al modo "Base de datos" y limpia la cascada de Presupuesto,
            // para no arrastrar la elección de un presupuesto anterior a una simulación nueva -
            // mismo criterio que ya se aplica a los checks "mostrar todo"/"mostrar ocultos" de
            // otros pasos (no arrastrar el estado de una vuelta anterior del asistente).
            RbOrigenBaseDatos.IsChecked = true;
            CmbPresupuestoNumero.ItemsSource = null;
            CmbPresupuestoVersion.ItemsSource = null;
            CmbPresupuestoVersion.IsEnabled = false;
            CmbPresupuestoOrden.ItemsSource = null;
            CmbPresupuestoOrden.IsEnabled = false;
            TxtErrorPaso1Presupuesto.Visibility = Visibility.Collapsed;
            BtnPaso1PresupuestoContinuar.IsEnabled = false;
            LimpiarVistaPreviaPresupuesto();

            MostrarPaso(1);
        }

        #endregion

        #region Paso 1: elegir modelo (Dibujo) — mismo patrón de árbol+grid+buscador que
        // ActualizadorAsociarConstructivosWindow/ConfiguradorOpcionesAnadirRotoWindow

        private void CargarDibujos()
        {
            try
            {
                _todosDibujos = DibujoOpcionesRotoService.GetDibujos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Loc("L_Suite_NoSeHanPodidoCargarDibujos") + Environment.NewLine + Environment.NewLine + ex.Message,
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
                SeleccionarDibujo(fila.Codigo);
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
            if (nodo.EsHoja) SeleccionarDibujo(nodo.Codigo);
        }

        private static TreeViewItem? ObtenerTreeViewItemDesdeOrigen(DependencyObject? source)
        {
            while (source != null && source is not TreeViewItem)
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
        }

        /// <summary>Mismo helper que ActualizadorAsociarConstructivosWindow (ver su comentario): hay
        /// que esperar a que el árbol visual genere los contenedores de los nodos recién expandidos
        /// antes de poder hacerles BringIntoView.</summary>
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

        /// <summary>Lee las hojas del dibujo elegido (SimulacionDatosService.ObtenerHojas) y decide
        /// si hace falta el Paso 2 (más de una hoja) o se puede pasar directamente al Paso 3 (una
        /// sola hoja, o ninguna: en ese caso se avisa en el propio Paso 1 y no se avanza).</summary>
        private void SeleccionarDibujo(string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return;

            TxtErrorPaso1.Visibility = Visibility.Collapsed;
            List<HojaSimulacion> hojas;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                hojas = SimulacionDatosService.ObtenerHojas(codigo);
            }
            catch (Exception ex)
            {
                TxtErrorPaso1.Text = string.Format(Loc("L_Suite_NoSeHaPodidoLeerDibujo"), ex.Message);
                TxtErrorPaso1.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            if (hojas.Count == 0)
            {
                TxtErrorPaso1.Text = Loc("L_Suite_NoSeHaEncontradoHojaDibujo");
                TxtErrorPaso1.Visibility = Visibility.Visible;
                return;
            }

            _origenModelo = OrigenModeloSimulacion.BaseDatos;
            _dibujoSeleccionado = codigo;
            _hojas = hojas;
            IniciarPaso2ConHojas(hojas);
        }

        #endregion

        #region Paso 1: elegir modelo (Presupuesto) — nuevo modo alternativo al de Dibujo de arriba

        /// <summary>Alterna entre los dos sub-paneles del Paso 1 (ver RbOrigenBaseDatos/
        /// RbOrigenPresupuesto en el XAML). Con guardas de null porque el RadioButton marcado por
        /// defecto (IsChecked="True" en el XAML) dispara su propio evento Checked durante
        /// InitializeComponent, ANTES de que los paneles referenciados aquí (declarados después en
        /// el mismo árbol) existan todavía -mismo problema y misma solución ya usada en este mismo
        /// fichero para ChkMostrarOtrasOpcionesReales/AplicarFiltroValoresReales-. Al entrar por
        /// primera vez en modo Presupuesto, carga los números disponibles (una sola vez: si
        /// CmbPresupuestoNumero ya tiene ItemsSource no se vuelve a consultar la BBDD solo por
        /// alternar de un lado a otro).</summary>
        private void RbOrigenModelo_Changed(object sender, RoutedEventArgs e)
        {
            if (PanelPaso1BaseDatos == null || PanelPaso1Presupuesto == null) return;

            bool presupuesto = RbOrigenPresupuesto.IsChecked == true;
            PanelPaso1BaseDatos.Visibility = presupuesto ? Visibility.Collapsed : Visibility.Visible;
            PanelPaso1Presupuesto.Visibility = presupuesto ? Visibility.Visible : Visibility.Collapsed;

            if (presupuesto && CmbPresupuestoNumero.ItemsSource == null)
                CargarNumerosPresupuesto();
        }

        private void CargarNumerosPresupuesto()
        {
            TxtErrorPaso1Presupuesto.Visibility = Visibility.Collapsed;
            List<string> numeros;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                numeros = PresupuestoDatosService.ObtenerNumerosPresupuesto();
            }
            catch (Exception ex)
            {
                TxtErrorPaso1Presupuesto.Text = string.Format(Loc("L_Suite_NoSeHanPodidoCargarNumerosPresupuesto"), ex.Message);
                TxtErrorPaso1Presupuesto.Visibility = Visibility.Visible;
                numeros = new List<string>();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            CmbPresupuestoNumero.ItemsSource = numeros;
        }

        /// <summary>Cascada Número→Versión: al elegir un Número se limpian Versión/Orden/vista
        /// previa/Continuar (todos dependían del Número anterior) y se cargan las versiones
        /// disponibles para el nuevo.</summary>
        private void CmbPresupuestoNumero_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _presupuestoNumero = CmbPresupuestoNumero.SelectedItem as string;
            _presupuestoVersion = null;
            _presupuestoOrden = null;
            CmbPresupuestoVersion.ItemsSource = null;
            CmbPresupuestoVersion.IsEnabled = false;
            CmbPresupuestoOrden.ItemsSource = null;
            CmbPresupuestoOrden.IsEnabled = false;
            BtnPaso1PresupuestoContinuar.IsEnabled = false;
            LimpiarVistaPreviaPresupuesto();
            TxtErrorPaso1Presupuesto.Visibility = Visibility.Collapsed;

            if (_presupuestoNumero == null) return;

            List<string> versiones;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                versiones = PresupuestoDatosService.ObtenerVersionesPresupuesto(_presupuestoNumero);
            }
            catch (Exception ex)
            {
                TxtErrorPaso1Presupuesto.Text = string.Format(Loc("L_Suite_NoSeHanPodidoCargarVersiones"), ex.Message);
                TxtErrorPaso1Presupuesto.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            CmbPresupuestoVersion.ItemsSource = versiones;
            CmbPresupuestoVersion.IsEnabled = versiones.Count > 0;
        }

        /// <summary>Cascada Versión→Orden: mismo criterio que CmbPresupuestoNumero_SelectionChanged.</summary>
        private void CmbPresupuestoVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _presupuestoVersion = CmbPresupuestoVersion.SelectedItem as string;
            _presupuestoOrden = null;
            CmbPresupuestoOrden.ItemsSource = null;
            CmbPresupuestoOrden.IsEnabled = false;
            BtnPaso1PresupuestoContinuar.IsEnabled = false;
            LimpiarVistaPreviaPresupuesto();
            TxtErrorPaso1Presupuesto.Visibility = Visibility.Collapsed;

            if (_presupuestoNumero == null || _presupuestoVersion == null) return;

            List<string> ordenes;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ordenes = PresupuestoDatosService.ObtenerOrdenesPresupuesto(_presupuestoNumero, _presupuestoVersion);
            }
            catch (Exception ex)
            {
                TxtErrorPaso1Presupuesto.Text = string.Format(Loc("L_Suite_NoSeHanPodidoCargarOrdenes"), ex.Message);
                TxtErrorPaso1Presupuesto.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            // Etiqueta = ValorReal + 1 (ver comentario de clase de FilaOrdenPresupuesto): el usuario
            // ve "Orden 1, 2, 3..." aunque BBDD guarde "0, 1, 2...". Si Orden no fuera numérico (no
            // debería darse, pero así no se rompe el desplegable), se muestra tal cual sin sumar.
            CmbPresupuestoOrden.ItemsSource = ordenes
                .Select(o => new FilaOrdenPresupuesto
                {
                    ValorReal = o,
                    Etiqueta = int.TryParse(o, NumberStyles.Integer, CultureInfo.InvariantCulture, out int baseCero)
                        ? (baseCero + 1).ToString(CultureInfo.InvariantCulture)
                        : o
                })
                .ToList();
            CmbPresupuestoOrden.IsEnabled = ordenes.Count > 0;
        }

        /// <summary>Último desplegable de la cascada: al elegir Orden ya hay Número+Versión+Orden
        /// completos, así que se dispara la vista previa (petición del usuario: "Al elegir el orden
        /// se previsualizará el dibujo del modelo") y se habilita "Continuar" -con independencia de
        /// si la vista previa consigue generarse o no, igual que en el Paso 2 el botón "Continuar"
        /// depende solo de haber elegido una hoja, no de si su vista previa cargó bien-.</summary>
        private void CmbPresupuestoOrden_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ValorReal (base cero, el de BBDD), NUNCA la Etiqueta que ve el usuario (base uno): ver
            // comentario de clase de FilaOrdenPresupuesto.
            _presupuestoOrden = (CmbPresupuestoOrden.SelectedItem as FilaOrdenPresupuesto)?.ValorReal;

            if (_presupuestoOrden == null)
            {
                BtnPaso1PresupuestoContinuar.IsEnabled = false;
                LimpiarVistaPreviaPresupuesto();
                return;
            }

            BtnPaso1PresupuestoContinuar.IsEnabled = true;
            CargarVistaPreviaPresupuestoSeleccionado();
        }

        private void BtnPaso1PresupuestoContinuar_Click(object sender, RoutedEventArgs e)
        {
            if (_presupuestoNumero == null || _presupuestoVersion == null || _presupuestoOrden == null) return;

            TxtErrorPaso1Presupuesto.Visibility = Visibility.Collapsed;
            List<HojaSimulacion> hojas;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                hojas = SimulacionDatosService.ObtenerHojasDesdePresupuesto(_presupuestoNumero, _presupuestoVersion, _presupuestoOrden);
            }
            catch (Exception ex)
            {
                TxtErrorPaso1Presupuesto.Text = string.Format(Loc("L_Suite_NoSeHaPodidoLeerPresupuesto"), ex.Message);
                TxtErrorPaso1Presupuesto.Visibility = Visibility.Visible;
                return;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            if (hojas.Count == 0)
            {
                TxtErrorPaso1Presupuesto.Text = Loc("L_Suite_NoSeHaEncontradoHojaPresupuesto");
                TxtErrorPaso1Presupuesto.Visibility = Visibility.Visible;
                return;
            }

            _origenModelo = OrigenModeloSimulacion.Presupuesto;
            // El origen ya no es un Codigo de Dibujos: se deja a null por claridad (CargarVistaPreviaDibujo
            // despacha según _origenModelo, no según que este campo sea o no null, pero así no queda
            // un Código de una elección anterior en modo Base de datos "colgando" sin sentido).
            _dibujoSeleccionado = null;
            _hojas = hojas;
            IniciarPaso2ConHojas(hojas);
        }

        /// <summary>Cuerpo común del paso "ya tengo la lista de hojas, hay que mostrar el Paso 2"
        /// (petición del usuario sobre el modo Presupuesto: "al continuar en el paso 2, será comun
        /// ... y el proceso será el mismo"), compartido por SeleccionarDibujo (modo Base de datos) y
        /// BtnPaso1PresupuestoContinuar_Click (modo Presupuesto): ambos ya han dejado _hojas y
        /// _origenModelo preparados antes de llamar aquí.</summary>
        private void IniciarPaso2ConHojas(List<HojaSimulacion> hojas)
        {
            // El Paso 2 aparece SIEMPRE ahora (antes se saltaba con una sola hoja): con una única
            // hoja se preselecciona sola (ListHojas_SelectionChanged habilita "Continuar" igual que
            // si la hubiera elegido el usuario a mano), pero el usuario quiere verla y, sobre todo,
            // ver la vista previa del dibujo aquí también en ese caso.
            TxtTituloPaso2.Text = hojas.Count == 1
                ? Loc("L_Suite_EsteModeloTiene1Hoja")
                : string.Format(Loc("L_Suite_EsteModeloTieneNHojas"), hojas.Count);
            ListHojas.ItemsSource = _hojas;
            BtnPaso2Continuar.IsEnabled = false;
            CargarVistaPreviaDibujo();
            MostrarPaso(2);

            if (hojas.Count == 1)
                ListHojas.SelectedItem = hojas[0];
        }

        #endregion

        #region Paso 2: elegir hoja (solo si hay más de una)

        private void ListHojas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnPaso2Continuar.IsEnabled = ListHojas.SelectedItem is HojaSimulacion;
        }

        private void BtnPaso2Continuar_Click(object sender, RoutedEventArgs e)
        {
            if (ListHojas.SelectedItem is HojaSimulacion hoja)
                IniciarPaso3(hoja);
        }

        private void BtnPaso2Atras_Click(object sender, RoutedEventArgs e) => MostrarPaso(1);

        /// <summary>Token incrementado cada vez que arranca una carga de vista previa nueva en el
        /// Paso 2 (ver CargarVistaPreviaDibujo): la callback en el hilo de UI compara su copia
        /// capturada contra el valor ACTUAL de este campo para descartarse si mientras tanto se
        /// eligió otro dibujo/presupuesto -mismo propósito que antes cumplía comparar
        /// "_dibujoSeleccionado != codigoDibujo", pero un simple contador sirve igual para los dos
        /// orígenes (Base de datos y Presupuesto) sin tener que comparar varios campos a la vez.</summary>
        private int _tokenVistaPreviaDibujo;

        /// <summary>Carga la vista previa visual del dibujo del modelo (DibujoImagenService,
        /// petición del usuario: "en la parte derecha... quiero que se muestre el dibujo del
        /// modelo") para mostrarla junto al listado de hojas. Es la misma imagen para todas las
        /// hojas de este dibujo -no depende de cuál se elija-, así que se pide una sola vez aquí, no
        /// dentro de ListHojas_SelectionChanged. Se hace en un hilo aparte (implica una consulta a
        /// BBDD + descomprimir + a veces rasterizar un metaarchivo) para no bloquear la UI mientras
        /// el usuario ya puede estar mirando la lista de la izquierda.
        ///
        /// Despacha según _origenModelo (petición del usuario sobre el modo Presupuesto: "al
        /// continuar en el paso 2, será comun... el proceso será el mismo"): Base de datos sigue
        /// pidiendo DibujoImagenService.CargarVistaPrevia(_dibujoSeleccionado) como siempre,
        /// Presupuesto pide CargarVistaPreviaPresupuesto con el Número/Versión/Orden ya elegidos en
        /// el Paso 1.</summary>
        private void CargarVistaPreviaDibujo()
        {
            int token = ++_tokenVistaPreviaDibujo;
            var origen = _origenModelo;
            string? dibujo = _dibujoSeleccionado;
            string? numero = _presupuestoNumero, version = _presupuestoVersion, orden = _presupuestoOrden;

            ImgVistaPreviaDibujo.Source = null;
            ImgVistaPreviaDibujo.Visibility = Visibility.Collapsed;
            TxtVistaPreviaAviso.Text = Loc("L_Suite_CargandoVistaPreviaDibujo");
            TxtVistaPreviaAviso.Visibility = Visibility.Visible;
            TxtVistaPreviaFuente.Visibility = Visibility.Collapsed;

            Task.Run(() =>
            {
                ImagenDibujo resultado;
                try
                {
                    resultado = origen == OrigenModeloSimulacion.Presupuesto
                        ? DibujoImagenService.CargarVistaPreviaPresupuesto(numero!, version!, orden!)
                        : DibujoImagenService.CargarVistaPrevia(dibujo!);
                }
                catch (Exception ex)
                {
                    resultado = new ImagenDibujo { Mensaje = string.Format(Loc("L_Suite_NoSePudoGenerarVistaPreviaError"), ex.Message) };
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Si mientras tanto se eligió otro dibujo/presupuesto, se descarta: evita que
                    // una vista previa "vieja" llegue tarde y se muestre encima de la nueva.
                    if (token != _tokenVistaPreviaDibujo) return;

                    if (resultado.Imagen != null)
                    {
                        // Width/Height a su tamaño NATIVO en píxeles: el zoom lo pone solo
                        // TransformZoomDibujo (ScaleTransform) encima, ver comentario del XAML sobre
                        // por qué Stretch=Uniform no sirve aquí (ScrollViewer da espacio "infinito").
                        ImgVistaPreviaDibujo.Source = resultado.Imagen;
                        ImgVistaPreviaDibujo.Width = resultado.Imagen.PixelWidth;
                        ImgVistaPreviaDibujo.Height = resultado.Imagen.PixelHeight;
                        ImgVistaPreviaDibujo.Visibility = Visibility.Visible;
                        TxtVistaPreviaAviso.Visibility = Visibility.Collapsed;
                        TxtVistaPreviaFuente.Text = string.Format(Loc("L_Suite_FuenteImagen"), resultado.Fuente, resultado.Imagen.PixelWidth, resultado.Imagen.PixelHeight);
                        TxtVistaPreviaFuente.Visibility = Visibility.Visible;
                        AjustarZoomDibujoAVentana();
                    }
                    else
                    {
                        TxtVistaPreviaAviso.Text = resultado.Mensaje ?? Loc("L_Suite_NoSePudoGenerarVistaPreviaDibujo");
                        TxtVistaPreviaAviso.Visibility = Visibility.Visible;
                        TxtVistaPreviaFuente.Visibility = Visibility.Collapsed;
                    }
                }));
            });
        }

        #region Zoom de la vista previa

        private double _zoomDibujo = 1.0;
        private const double ZoomMinimo = 0.1;
        private const double ZoomMaximo = 8.0;
        private const double ZoomPaso = 1.25;

        /// <summary>Calcula y aplica el zoom más grande que permita ver el dibujo entero dentro del
        /// hueco visible del ScrollViewer (botón "Ajustar", y también el zoom inicial al cargar una
        /// imagen nueva). Si algo no cuadra (imagen o panel sin tamaño todavía) se deja en 100%.</summary>
        private void AjustarZoomDibujoAVentana()
        {
            if (ImgVistaPreviaDibujo.Source is not BitmapSource bmp || bmp.PixelWidth <= 0 || bmp.PixelHeight <= 0)
            {
                AplicarZoomDibujo(1.0);
                return;
            }

            double anchoDisponible = ScrollVistaPreviaDibujo.ViewportWidth > 0
                ? ScrollVistaPreviaDibujo.ViewportWidth : ScrollVistaPreviaDibujo.ActualWidth - 28;
            double altoDisponible = ScrollVistaPreviaDibujo.ViewportHeight > 0
                ? ScrollVistaPreviaDibujo.ViewportHeight : ScrollVistaPreviaDibujo.ActualHeight - 28;

            if (anchoDisponible <= 0 || altoDisponible <= 0)
            {
                AplicarZoomDibujo(1.0);
                return;
            }

            double escala = Math.Min(anchoDisponible / bmp.PixelWidth, altoDisponible / bmp.PixelHeight);
            AplicarZoomDibujo(escala);
        }

        private void AplicarZoomDibujo(double zoom)
        {
            _zoomDibujo = Math.Max(ZoomMinimo, Math.Min(ZoomMaximo, zoom));
            TransformZoomDibujo.ScaleX = _zoomDibujo;
            TransformZoomDibujo.ScaleY = _zoomDibujo;
            TxtZoomPorcentaje.Text = $"{_zoomDibujo * 100:0}%";
        }

        private void BtnZoomMas_Click(object sender, RoutedEventArgs e) => AplicarZoomDibujo(_zoomDibujo * ZoomPaso);

        private void BtnZoomMenos_Click(object sender, RoutedEventArgs e) => AplicarZoomDibujo(_zoomDibujo / ZoomPaso);

        private void BtnZoomAjustar_Click(object sender, RoutedEventArgs e) => AjustarZoomDibujoAVentana();

        /// <summary>Rueda del ratón sobre la vista previa = zoom (sin necesidad de Ctrl: este panel
        /// no tiene más contenido que la imagen, así que no hay ambigüedad con un scroll normal); el
        /// propio desplazamiento cuando la imagen no cabe entera lo dan las barras del ScrollViewer
        /// o arrastrando. Sin efecto mientras no haya imagen cargada todavía.</summary>
        private void ScrollVistaPreviaDibujo_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ImgVistaPreviaDibujo.Visibility != Visibility.Visible) return;
            e.Handled = true;
            AplicarZoomDibujo(e.Delta > 0 ? _zoomDibujo * ZoomPaso : _zoomDibujo / ZoomPaso);
        }

        #endregion

        #endregion

        #region Paso 1 (modo Presupuesto): vista previa PROPIA + su propio zoom, independiente del Paso 2

        /// <summary>Mismo propósito que _tokenVistaPreviaDibujo (Paso 2), pero para este panel
        /// aparte: se incrementa en cada carga nueva y también al limpiar (LimpiarVistaPreviaPresupuesto),
        /// para descartar cualquier respuesta "vieja" que llegara tarde de un Orden ya abandonado.</summary>
        private int _tokenVistaPreviaPresupuesto;

        private double _zoomPresupuesto = 1.0;

        /// <summary>Deja el panel de vista previa del Paso 1/Presupuesto en su estado "vacío" (sin
        /// Número/Versión/Orden completos todavía, o cambiando de uno ya elegido a otro distinto):
        /// invalida cualquier carga en curso (ver CargarVistaPreviaPresupuestoSeleccionado) y
        /// restaura el aviso inicial.</summary>
        private void LimpiarVistaPreviaPresupuesto()
        {
            _tokenVistaPreviaPresupuesto++;
            ImgVistaPreviaPresupuesto.Source = null;
            ImgVistaPreviaPresupuesto.Visibility = Visibility.Collapsed;
            TxtVistaPreviaPresupuestoAviso.Text = Loc("L_Suite_ElegeNumVersOrdenVistaPrevia");
            TxtVistaPreviaPresupuestoAviso.Visibility = Visibility.Visible;
            TxtVistaPreviaPresupuestoFuente.Visibility = Visibility.Collapsed;
        }

        /// <summary>Carga la vista previa del panel PROPIO del Paso 1/Presupuesto (petición del
        /// usuario, tras la pregunta aclaratoria sobre dónde debía aparecer: "Panel de
        /// previsualización propio dentro del Paso 1") a partir de Número+Versión+Orden ya
        /// elegidos. Mismo patrón EXACTO que CargarVistaPreviaDibujo del Paso 2 (hilo aparte +
        /// token para descartar respuestas obsoletas), pero contra
        /// DibujoImagenService.CargarVistaPreviaPresupuesto y los elementos "...Presupuesto" del
        /// XAML en vez de los del Paso 2.</summary>
        private void CargarVistaPreviaPresupuestoSeleccionado()
        {
            int token = ++_tokenVistaPreviaPresupuesto;
            string numero = _presupuestoNumero!, version = _presupuestoVersion!, orden = _presupuestoOrden!;

            ImgVistaPreviaPresupuesto.Source = null;
            ImgVistaPreviaPresupuesto.Visibility = Visibility.Collapsed;
            TxtVistaPreviaPresupuestoAviso.Text = Loc("L_Suite_CargandoVistaPreviaDibujo");
            TxtVistaPreviaPresupuestoAviso.Visibility = Visibility.Visible;
            TxtVistaPreviaPresupuestoFuente.Visibility = Visibility.Collapsed;

            Task.Run(() =>
            {
                ImagenDibujo resultado;
                try
                {
                    resultado = DibujoImagenService.CargarVistaPreviaPresupuesto(numero, version, orden);
                }
                catch (Exception ex)
                {
                    resultado = new ImagenDibujo { Mensaje = string.Format(Loc("L_Suite_NoSePudoGenerarVistaPreviaError"), ex.Message) };
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (token != _tokenVistaPreviaPresupuesto) return;

                    if (resultado.Imagen != null)
                    {
                        ImgVistaPreviaPresupuesto.Source = resultado.Imagen;
                        ImgVistaPreviaPresupuesto.Width = resultado.Imagen.PixelWidth;
                        ImgVistaPreviaPresupuesto.Height = resultado.Imagen.PixelHeight;
                        ImgVistaPreviaPresupuesto.Visibility = Visibility.Visible;
                        TxtVistaPreviaPresupuestoAviso.Visibility = Visibility.Collapsed;
                        TxtVistaPreviaPresupuestoFuente.Text = string.Format(Loc("L_Suite_FuenteImagen"), resultado.Fuente, resultado.Imagen.PixelWidth, resultado.Imagen.PixelHeight);
                        TxtVistaPreviaPresupuestoFuente.Visibility = Visibility.Visible;
                        AjustarZoomPresupuestoAVentana();
                    }
                    else
                    {
                        TxtVistaPreviaPresupuestoAviso.Text = resultado.Mensaje ?? Loc("L_Suite_NoSePudoGenerarVistaPreviaPresupuesto");
                        TxtVistaPreviaPresupuestoAviso.Visibility = Visibility.Visible;
                        TxtVistaPreviaPresupuestoFuente.Visibility = Visibility.Collapsed;
                    }
                }));
            });
        }

        /// <summary>Mismo cálculo EXACTO que AjustarZoomDibujoAVentana (Paso 2), contra los
        /// elementos del panel de Presupuesto.</summary>
        private void AjustarZoomPresupuestoAVentana()
        {
            if (ImgVistaPreviaPresupuesto.Source is not BitmapSource bmp || bmp.PixelWidth <= 0 || bmp.PixelHeight <= 0)
            {
                AplicarZoomPresupuesto(1.0);
                return;
            }

            double anchoDisponible = ScrollVistaPreviaPresupuesto.ViewportWidth > 0
                ? ScrollVistaPreviaPresupuesto.ViewportWidth : ScrollVistaPreviaPresupuesto.ActualWidth - 28;
            double altoDisponible = ScrollVistaPreviaPresupuesto.ViewportHeight > 0
                ? ScrollVistaPreviaPresupuesto.ViewportHeight : ScrollVistaPreviaPresupuesto.ActualHeight - 28;

            if (anchoDisponible <= 0 || altoDisponible <= 0)
            {
                AplicarZoomPresupuesto(1.0);
                return;
            }

            double escala = Math.Min(anchoDisponible / bmp.PixelWidth, altoDisponible / bmp.PixelHeight);
            AplicarZoomPresupuesto(escala);
        }

        private void AplicarZoomPresupuesto(double zoom)
        {
            _zoomPresupuesto = Math.Max(ZoomMinimo, Math.Min(ZoomMaximo, zoom));
            TransformZoomPresupuesto.ScaleX = _zoomPresupuesto;
            TransformZoomPresupuesto.ScaleY = _zoomPresupuesto;
            TxtZoomPresupuestoPorcentaje.Text = $"{_zoomPresupuesto * 100:0}%";
        }

        private void BtnZoomPresupuestoMas_Click(object sender, RoutedEventArgs e) => AplicarZoomPresupuesto(_zoomPresupuesto * ZoomPaso);

        private void BtnZoomPresupuestoMenos_Click(object sender, RoutedEventArgs e) => AplicarZoomPresupuesto(_zoomPresupuesto / ZoomPaso);

        private void BtnZoomPresupuestoAjustar_Click(object sender, RoutedEventArgs e) => AjustarZoomPresupuestoAVentana();

        private void ScrollVistaPreviaPresupuesto_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ImgVistaPreviaPresupuesto.Visibility != Visibility.Visible) return;
            e.Handled = true;
            AplicarZoomPresupuesto(e.Delta > 0 ? _zoomPresupuesto * ZoomPaso : _zoomPresupuesto / ZoomPaso);
        }

        #endregion

        #region Paso 3: valores reales (editables) + escandallo de partida

        private void IniciarPaso3(HojaSimulacion hoja)
        {
            _hojaSeleccionada = hoja;

            TxtEscandalloInicial.Text = hoja.EscandallosAsociados.FirstOrDefault() ?? "RO_Gestion Herraje";

            var otros = hoja.EscandallosAsociados.Skip(1).ToList();
            TxtOtrosEscandallos.Text = hoja.EscandallosAsociados.Count == 0
                ? Loc("L_Suite_HojaSinEscandalloPropuesta")
                : otros.Count > 0
                    ? string.Format(Loc("L_Suite_HojaTambienTieneAsociados"), string.Join(", ", otros))
                    : "";

            _valoresReales = hoja.ValoresReales
                .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => new FilaValorReal
                {
                    Nombre = kv.Key,
                    Valor = kv.Value,
                    Origen = hoja.ValoresDetectadosHeuristica.Contains(kv.Key)
                        ? Loc("L_Suite_OrigenDeducidoOpening")
                        : Loc("L_Suite_OrigenValorRealGuardado")
                })
                .ToList();

            // Reinicia el check a "solo ROTO" cada vez que se entra aquí con una hoja nueva (mismo
            // criterio que ChkMostrarOcultos en el Paso 5: no arrastrar el "mostrar todo" de una
            // hoja anterior a la siguiente).
            if (ChkMostrarOtrasOpcionesReales != null) ChkMostrarOtrasOpcionesReales.IsChecked = false;
            AplicarFiltroValoresReales();

            MostrarPaso(3);
        }

        /// <summary>"Mostrar también el resto de opciones (no ROTO)" (ver ChkMostrarOtrasOpcionesReales
        /// en el XAML): petición del usuario, mismo criterio que el mapa conceptual del Paso 5 -por
        /// defecto solo se listan las opciones reales de catálogo ROTO (Nombre empieza por "RO_")-,
        /// pero aquí configurable con un check en vez de fijo, porque en el Paso 3 SÍ puede interesar
        /// ver/editar de un vistazo los flags internos del DSL (Puerta, Practicable...) o
        /// HardwareSupplier antes de arrancar la simulación. Filtra SOLO la vista de GridValoresReales:
        /// _valoresReales (la lista completa) no se toca, así que BtnIniciarSimulacion_Click sigue
        /// precargando TODOS los valores reales guardados, estén o no visibles aquí en este momento.</summary>
        private void AplicarFiltroValoresReales()
        {
            if (GridValoresReales == null) return;

            bool mostrarTodo = ChkMostrarOtrasOpcionesReales?.IsChecked == true;

            IEnumerable<FilaValorReal> filas = _valoresReales;
            if (!mostrarTodo) filas = filas.Where(f => f.Nombre.StartsWith("RO_", StringComparison.OrdinalIgnoreCase));

            GridValoresReales.ItemsSource = filas.ToList();
        }

        private void ChkMostrarOtrasOpcionesReales_Changed(object sender, RoutedEventArgs e) => AplicarFiltroValoresReales();

        // El Paso 2 aparece siempre ahora (ver SeleccionarDibujo), así que "Atrás" desde el Paso 3
        // vuelve siempre a él.
        private void BtnPaso3Atras_Click(object sender, RoutedEventArgs e) => MostrarPaso(2);

        #endregion

        #region Paso 4: asistente pregunta-respuesta

        private void BtnIniciarSimulacion_Click(object sender, RoutedEventArgs e)
        {
            // Fuerza a confirmar cualquier edición de "Valor real" en curso antes de leerlas (mismo
            // criterio que BtnAplicar_Click en ActualizadorAsociarConstructivosWindow).
            GridValoresReales.CommitEdit(DataGridEditingUnit.Cell, true);
            GridValoresReales.CommitEdit(DataGridEditingUnit.Row, true);

            string codigoInicial = (TxtEscandalloInicial.Text ?? "").Trim();
            if (string.IsNullOrEmpty(codigoInicial))
            {
                MessageBox.Show(Loc("L_Suite_EscribeCodigoEscandalloPartida"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _interprete = new EscandalloInterpreter();
            foreach (var fila in _valoresReales)
            {
                if (!string.IsNullOrWhiteSpace(fila.Valor))
                    _interprete.PrecargarValorConocido(fila.Nombre, fila.Valor);
            }

            _log.Clear();
            _preguntaActual = null;
            _enumerador = _interprete.Ejecutar(codigoInicial).GetEnumerator();

            // Por si se arranca una simulación nueva sin pasar por "Reiniciar simulación" (p.ej.
            // Atrás a Paso 3, cambiar algo, y volver a pulsar "Iniciar"): sin este reset, el botón
            // podría quedar habilitado desde el final de la simulación ANTERIOR mientras la nueva
            // todavía tiene preguntas pendientes.
            BtnVerMapaConceptual.IsEnabled = false;

            MostrarPaso(4);
            AvanzarSimulacion();
        }

        /// <summary>Consume eventos del intérprete uno a uno (registrándolos en el log) hasta que
        /// hace falta preguntar algo al usuario (PreguntarOpcion, la única pausa real) o hasta que
        /// termina (Fin, que ahora deja ver el recorrido en vez de saltar directo al mapa conceptual,
        /// ver MostrarSimulacionTerminada). Todos los demás tipos de evento se auto-consumen sin
        /// detener el recorrido, tal y como pidió el usuario.</summary>
        private void AvanzarSimulacion()
        {
            if (_enumerador == null) return;

            while (_enumerador.MoveNext())
            {
                var ev = _enumerador.Current;
                RegistrarLog(ev);

                if (ev.Tipo == TipoEventoSimulacion.PreguntarOpcion)
                {
                    MostrarPregunta(ev);
                    return;
                }
                if (ev.Tipo == TipoEventoSimulacion.Fin)
                {
                    MostrarSimulacionTerminada();
                    return;
                }
            }
        }

        /// <summary>Petición del usuario: al terminar todas las preguntas, YA NO se pasa
        /// automáticamente al Paso 5 (mapa conceptual) -antes sí, llamando aquí mismo a
        /// MostrarMapaConceptual()-. Motivo textual del usuario: "necesito realizar comprobaciones
        /// en algun escandallo porque por ejemplo el mapa conceptual de la opcion CotaVariable no
        /// está completa ya que se asigna fuera de los escandallos de ROTO y es importante localizar
        /// desde donde se pueden estar estableciendo nuestras opciones fuera de nuestros
        /// escandallos" — el "Recorrido" de la izquierda (ListLog) es justo donde se ve ESO: la
        /// secuencia completa de entra/sale de cada escandallo y cada ESTABLECEOPCION, algo que el
        /// mapa conceptual del Paso 5 no puede mostrar por sí solo porque solo lista el valor final
        /// de cada opción, no el recorrido paso a paso. Se queda en Paso 4 (el propio Recorrido no se
        /// toca ni se limpia) con la "Pregunta" reemplazada por un aviso de fin, y con el nuevo botón
        /// "Ver mapa conceptual final" habilitado para cuando el usuario ya haya terminado de
        /// revisar.</summary>
        private void MostrarSimulacionTerminada()
        {
            _preguntaActual = null;

            TxtPreguntaOpcion.Text = Loc("L_Suite_SimulacionTerminada");
            TxtPreguntaEscandallo.Text = Loc("L_Suite_RevisaRecorridoAviso");
            TxtValorLibre.Text = "";
            ListValoresPosibles.ItemsSource = null;
            ListValoresPosibles.Visibility = Visibility.Collapsed;
            PanelRespuestaPregunta.Visibility = Visibility.Collapsed;

            BtnVerMapaConceptual.IsEnabled = true;
        }

        private void BtnVerMapaConceptual_Click(object sender, RoutedEventArgs e) => MostrarMapaConceptual();

        /// <summary>"← Volver al recorrido (Paso 4)", en el Paso 5 (ver MostrarMapaConceptual): a
        /// diferencia de "Nueva simulación" (BtnReiniciar_Click), este botón NO toca nada del estado
        /// -_interprete, _enumerador, _log y _preguntaActual se quedan tal cual-, así que basta con
        /// volver a mostrar el panel del Paso 4 para verlo exactamente como se dejó (el "Recorrido"
        /// completo a la izquierda, y a la derecha el aviso "Simulación terminada" con "Ver mapa
        /// conceptual final" ya habilitado, listo para volver al Paso 5 tantas veces como haga
        /// falta). Petición del usuario: poder ir y venir entre el mapa conceptual y el recorrido
        /// libremente, no solo una vez antes de llegar al mapa.</summary>
        private void BtnPaso5Atras_Click(object sender, RoutedEventArgs e) => MostrarPaso(4);

        private void RegistrarLog(EventoSimulacion ev)
        {
            string linea = ev.Tipo switch
            {
                TipoEventoSimulacion.EntrandoEscandallo => string.Format(Loc("L_Suite_LogEntraEn"), ev.Escandallo),
                TipoEventoSimulacion.SaliendoEscandallo => string.Format(Loc("L_Suite_LogSaleDe"), ev.Escandallo),
                TipoEventoSimulacion.OpcionEstablecida => string.Format(Loc("L_Suite_LogOpcionEstablecida"), ev.Opcion, ev.Valor, ev.Escandallo),
                // ESCANDALLO("Código","Variables"): la variable se "pasa" al escandallo llamado ANTES
                // de entrar en él (ver EscandalloInterpreter.AplicarVariablesDeLlamada) — ev.Escandallo
                // aquí es el escandallo LLAMADOR (el que hace la llamada), no el llamado, así que la
                // línea se redacta en ese sentido para no confundirla con OpcionEstablecida.
                TipoEventoSimulacion.VariablePasada => string.Format(Loc("L_Suite_LogVariablePasada"), ev.Escandallo, ev.Opcion, ev.Valor),
                // SEA Variable = Valor; (ver EscandalloDsl.NodoSea): a diferencia de VariablePasada
                // (que llega vía ESCANDALLO(...)), aquí la propia línea del Programa asigna el valor
                // directamente, así que se redacta como una asignación.
                TipoEventoSimulacion.VariableAsignada => string.Format(Loc("L_Suite_LogVariableAsignada"), ev.Escandallo, ev.Opcion, ev.Valor),
                TipoEventoSimulacion.PreguntarOpcion => ev.EsVariable
                    ? string.Format(Loc("L_Suite_LogNecesitaValorNumericoVariable"), ev.Escandallo, ev.Opcion)
                    : string.Format(Loc("L_Suite_LogNecesitaValorOpcion"), ev.Escandallo, ev.Opcion),
                TipoEventoSimulacion.NoReconocido => string.Format(Loc("L_Suite_LogFragmentoNoReconocido"), ev.Escandallo, ev.Mensaje),
                TipoEventoSimulacion.ErrorParseo => string.Format(Loc("L_Suite_LogErrorInterpretarPrograma"), ev.Escandallo, ev.Mensaje),
                TipoEventoSimulacion.EscandalloNoEncontrado => string.Format(Loc("L_Suite_LogEscandalloNoEncontrado"), ev.Escandallo),
                TipoEventoSimulacion.CicloDetectado => string.Format(Loc("L_Suite_LogCicloDetectado"), ev.Escandallo, string.Join(" → ", ev.PilaLlamadas ?? new List<string>())),
                TipoEventoSimulacion.Fin => Loc("L_Suite_SimulacionTerminada"),
                _ => ev.Tipo.ToString()
            };

            _log.Add(linea);
            if (_log.Count > 0) ListLog.ScrollIntoView(_log[_log.Count - 1]);
        }

        /// <summary>Valores posibles fijos para los flags internos del DSL que el intérprete SÍ deja
        /// preguntar de momento (EscandalloInterpreter.OpcionesInternasPreguntables: "por el momento
        /// debe seguir preguntando por las opciones que te especifiqué los valores concretos"). No
        /// existen como fila en la tabla Opciones (esa tabla solo tiene código "RO_%"), así que
        /// RotoTools.Helpers.GetContenidoOpciones no tiene nada que devolver para ellos: se usa esta
        /// lista, dada por el usuario y verificada además contra los literales que de verdad
        /// comparan los escandallos (p.ej. OPCION("Puerta","Sí") con tilde).</summary>
        private static readonly Dictionary<string, string[]> ValoresFijosOpcionesInternas = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Puerta"] = new[] { "Sí", "No" },
            ["Practicable"] = new[] { "Izquierda", "Derecha", "Ninguna" },
            ["Oscilobatiente"] = new[] { "Inferior", "Ninguna" },
            ["Corredera"] = new[] { "Izquierda", "Derecha", "IzquierdaDerecha", "Ninguna" },
            ["Elevable"] = new[] { "Sí", "No" },
            ["CotaVariable"] = new[] { "Sí", "No" },
            ["Activa"] = new[] { "Sí", "No" },
            ["Asociada"] = new[] { "Ninguna", "Practicable", "Oscilobatiente" },
            ["Exterior"] = new[] { "Sí", "No" },
        };

        /// <summary>Traduce, SOLO para presentación (FilaValorPosible.Texto), cada uno de los
        /// literales fijos de ValoresFijosOpcionesInternas. El VALOR que de verdad se envía a
        /// EscandalloInterpreter.ResponderOpcion (FilaValorPosible.Valor, ver MostrarPregunta) se
        /// deja SIEMPRE tal cual, en español -son literales del propio DSL que los escandallos reales
        /// comparan textualmente, p.ej. OPCION("Puerta","Sí")-, así que traducir aquí el texto
        /// mostrado nunca cambia el valor con el que de verdad se responde la pregunta. Cualquier
        /// literal no listado (no debería darse: son justo los 9 valores fijos de arriba) se
        /// devuelve tal cual, sin traducir.</summary>
        private static string TraducirValorFijo(string valor) => valor switch
        {
            "Sí" => Loc("L_Suite_ValorSi"),
            "No" => Loc("L_Suite_ValorNo"),
            "Izquierda" => Loc("L_Suite_ValorIzquierda"),
            "Derecha" => Loc("L_Suite_ValorDerecha"),
            "Ninguna" => Loc("L_Suite_ValorNinguna"),
            "IzquierdaDerecha" => Loc("L_Suite_ValorIzquierdaDerecha"),
            "Inferior" => Loc("L_Suite_ValorInferior"),
            "Practicable" => Loc("L_Suite_ValorPracticable"),
            "Oscilobatiente" => Loc("L_Suite_ValorOscilobatiente"),
            _ => valor
        };

        /// <summary>El intérprete (EscandalloInterpreter.EsOpcionPreguntable) ya filtra de raíz qué
        /// opciones llegan a pausar la simulación con un PreguntarOpcion: opciones reales del
        /// catálogo ROTO ("RO_..."), la excepción "HardwareSupplier", y los flags internos listados
        /// en ValoresFijosOpcionesInternas. Aquí solo queda decidir DE DÓNDE sacar la lista de
        /// valores posibles a mostrar: BBDD para las dos primeras, la lista fija para la tercera.
        ///
        /// Caso aparte, ev.EsVariable: una comparación de variable (A=1200, L&lt;500...) cuyo valor
        /// no se ha podido resolver desde la cadena de llamadas ESCANDALLO("Código","Variables") -ni
        /// alias tipo "A=L1", ni literal directo, ver EscandalloInterpreter.AplicarVariablesDeLlamada.
        /// Aquí NO hay catálogo de valores posibles que consultar (no es una opción, es una cota
        /// numérica), así que se salta por completo la búsqueda en BBDD/lista fija y se deja la lista
        /// de "valores posibles" vacía: el usuario responde siempre por "Otro valor", validado como
        /// numérico en BtnResponderLibre_Click.</summary>
        private void MostrarPregunta(EventoSimulacion ev)
        {
            // Por si esta pregunta llega tras un MostrarSimulacionTerminada anterior en la misma
            // sesión de Paso 4 (no debería pasar en un recorrido normal -Fin es siempre el último
            // evento-, pero así queda cubierto igual): se restauran, ambos colapsados allí.
            ListValoresPosibles.Visibility = Visibility.Visible;
            PanelRespuestaPregunta.Visibility = Visibility.Visible;

            _preguntaActual = ev;
            string opcion = ev.Opcion ?? "";
            TxtValorLibre.Text = "";

            List<FilaValorPosible> posibles;
            if (ev.EsVariable)
            {
                TxtPreguntaOpcion.Text = string.Format(Loc("L_Suite_VariableEntreComillas"), opcion);
                TxtPreguntaEscandallo.Text = string.Format(Loc("L_Suite_NecesitaValorNumericoVariable"), ev.Escandallo, opcion);
                posibles = new List<FilaValorPosible>();
            }
            else
            {
                TxtPreguntaOpcion.Text = opcion;
                TxtPreguntaEscandallo.Text = string.Format(Loc("L_Suite_PreguntadoPor"), ev.Escandallo);

                if (ValoresFijosOpcionesInternas.TryGetValue(opcion, out var valoresFijos))
                {
                    // Valor: literal del DSL sin traducir (ver TraducirValorFijo). Texto: SOLO el
                    // mostrado se traduce.
                    posibles = valoresFijos
                        .Select(v => new FilaValorPosible { Valor = v, Texto = TraducirValorFijo(v), Oculto = false })
                        .ToList();
                }
                else
                {
                    try
                    {
                        posibles = RotoTools.Helpers.GetContenidoOpciones(opcion)
                            .OrderBy(c => c.Orden)
                            .Select(c => new FilaValorPosible
                            {
                                Valor = c.Valor ?? "",
                                Texto = c.Texto ?? "",
                                Oculto = c.OcultaEnLista || c.OcultaEnArbol
                            })
                            .ToList();
                    }
                    catch
                    {
                        // Si la propia consulta de posibles valores fallara, se deja la lista vacía: el
                        // usuario siempre puede responder con "Otro valor" (BtnResponderLibre_Click).
                        posibles = new List<FilaValorPosible>();
                    }
                }
            }

            ListValoresPosibles.ItemsSource = posibles;
        }

        private void BtnResponder_Click(object sender, RoutedEventArgs e)
        {
            if (_preguntaActual == null) return;

            if (ListValoresPosibles.SelectedItem is not FilaValorPosible elegido)
            {
                MessageBox.Show(Loc("L_Suite_EligeValorListaOtroValor"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Responder(elegido.Valor);
        }

        private void BtnResponderLibre_Click(object sender, RoutedEventArgs e)
        {
            if (_preguntaActual == null) return;

            string valor = (TxtValorLibre.Text ?? "").Trim();
            if (string.IsNullOrEmpty(valor))
            {
                MessageBox.Show(Loc("L_Suite_EscribeUnValor"), "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Una pregunta de variable (A=1200, L<500...) exige un valor numérico: es lo único que
            // EvaluarCondicionPura sabe interpretar para un NodoVarCmp (double.TryParse, ver su
            // comentario). Se valida aquí, en la UI, para dar un aviso claro en el momento en vez de
            // dejar que la condición simplemente evalúe "no coincide" silenciosamente más adelante.
            if (_preguntaActual.EsVariable && !double.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show(string.Format(Loc("L_Suite_ValorNoValidoNumerico"), valor, _preguntaActual.Opcion),
                    "", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Responder(valor);
        }

        private void Responder(string valor)
        {
            var pregunta = _preguntaActual;
            if (pregunta?.Opcion == null || _interprete == null) return;

            // Escribe la respuesta en Estado ANTES de reanudar el enumerador: ver el comentario de
            // clase de EscandalloInterpreter sobre por qué ResponderOpcion debe llamarse siempre
            // antes del siguiente MoveNext(), no después.
            _interprete.ResponderOpcion(pregunta.Opcion, valor, pregunta.Escandallo);

            // Deja constancia en el Recorrido de qué valor se ha respondido realmente (antes esto
            // no quedaba registrado en ningún sitio, lo que hacía imposible confirmar a posteriori
            // qué se había contestado en cada pregunta al revisar un caso como el reportado con
            // RO_C_TIPOLOGIAS/HardwareSupplier).
            _log.Add(string.Format(Loc("L_Suite_LogRespondidoEnAsistente"), pregunta.Opcion, valor));
            if (_log.Count > 0) ListLog.ScrollIntoView(_log[_log.Count - 1]);

            _preguntaActual = null;
            AvanzarSimulacion();
        }

        #endregion

        #region Paso 5: mapa conceptual final

        private void MostrarMapaConceptual()
        {
            _mapaCompleto = _interprete!.Estado
                .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .Select(kv => new FilaMapaOpcion
                {
                    Nombre = kv.Key,
                    ValorFinal = kv.Value.Valor ?? Loc("L_Suite_SinValor"),
                    HistorialTexto = string.Join(Environment.NewLine, kv.Value.Historial.Select((h, i) =>
                        string.Format(Loc("L_Suite_HistorialLinea"), i + 1, h.Escandallo, h.Valor, TraducirOrigen(h.Origen)))),
                    OrigenFinal = kv.Value.Historial.Count > 0 ? TraducirOrigen(kv.Value.Historial[^1].Origen) : "",
                    Sobrescrita = kv.Value.Historial.Count > 1,
                    Oculto = EsValorOculto(kv.Value.Valor)
                })
                .ToList();

            // Las de OpcionesInternasEnMapa (CotaVariable, AlturaManeta, AlturaManetaPorDefecto)
            // interesa verlas SIEMPRE en el mapa, incluso cuando NINGÚN escandallo real llegó a
            // establecerlas durante este recorrido -petición explícita del usuario: "puede que no
            // se establezca (debe mostrarse que no se ha establecido)"-, a diferencia de cualquier
            // otra opción (RO_...), que solo aparece si de verdad llegó a tener algún valor -no
            // tendría sentido listar cada opción posible del catálogo entero "por si acaso"-. Se
            // añaden aquí, aparte, como filas sintéticas sin historial -no como entradas "vacías" en
            // Estado- para no ensuciar el propio motor de simulación con una cuestión que es
            // puramente de presentación del mapa.
            foreach (string nombre in OpcionesInternasEnMapa)
            {
                if (_interprete.Estado.ContainsKey(nombre)) continue; // ya tiene su fila real, con historial, más arriba.

                _mapaCompleto.Add(new FilaMapaOpcion
                {
                    Nombre = nombre,
                    ValorFinal = Loc("L_Suite_NoEstablecidoEnRecorrido"),
                    HistorialTexto = "",
                    OrigenFinal = "",
                    Sobrescrita = false,
                    Oculto = false
                });
            }
            _mapaCompleto = _mapaCompleto.OrderBy(f => f.Nombre, StringComparer.OrdinalIgnoreCase).ToList();

            TxtBuscarMapa.Text = "";
            if (ChkMostrarOcultos != null) ChkMostrarOcultos.IsChecked = false;
            AplicarFiltroMapa();
            MostrarPaso(5);
        }

        private static string TraducirOrigen(string origen) => origen switch
        {
            EscandalloInterpreter.OrigenEstablecido => Loc("L_Suite_OrigenEstablecidoPorEstablece"),
            EscandalloInterpreter.OrigenElegidoUsuario => Loc("L_Suite_OrigenElegidoEnAsistente"),
            EscandalloInterpreter.OrigenValorRealDibujo => Loc("L_Suite_OrigenValorRealDibujo"),
            EscandalloInterpreter.OrigenVariablePasada => Loc("L_Suite_OrigenVariablePasadaPor"),
            EscandalloInterpreter.OrigenEstablecidoNumerico => Loc("L_Suite_OrigenEstablecidoNumerico"),
            EscandalloInterpreter.OrigenAsignadoPorSea => Loc("L_Suite_OrigenAsignadoPorSea"),
            _ => origen
        };

        /// <summary>true si el valor final de esta opción es LITERALMENTE el valor "Oculto": el
        /// sentinel que algunos escandallos asignan vía ESTABLECEOPCION("RO_...","Oculto") para
        /// señalar que esa opción no aplica / debe tratarse como oculta -el mismo valor que compara
        /// el propio DSL, p.ej. "OPCION('RO_1SISTEMA','Oculto')=1" (aclaración del usuario: eso
        /// significa que RO_1SISTEMA SÍ tiene seleccionado el valor Oculto). NO tiene nada que ver
        /// con OcultaEnLista/OcultaEnArbol de ContenidoOpciones -eso es otra cosa: qué valores
        /// posibles se esconden en la lista/árbol de un desplegable, no si el valor asignado ES ese
        /// sentinel-: la primera versión de este método consultaba esas flags por error, por lo que
        /// el filtro nunca ocultaba nada (ninguna opción real terminaba marcada así). Comparación
        /// simple, sin BBDD.</summary>
        private static bool EsValorOculto(string? valor) =>
            string.Equals((valor ?? "").Trim(), "Oculto", StringComparison.Ordinal);

        private void TxtBuscarMapa_TextChanged(object sender, TextChangedEventArgs e) => AplicarFiltroMapa();

        private void ChkMostrarOcultos_Changed(object sender, RoutedEventArgs e) => AplicarFiltroMapa();

        /// <summary>Flags internos del DSL que, aunque no son opciones reales de catálogo (no
        /// empiezan por "RO_"), SÍ interesa ver en el mapa conceptual porque pueden quedar
        /// establecidos por un ESTABLECEOPCION dentro de un escandallo -a diferencia de
        /// Puerta/Practicable/Oscilobatiente/etc., que son solo datos de ENTRADA para la
        /// simulación (se preguntan, pero ningún escandallo real los "establece" como resultado) y
        /// por eso se quedan fuera del mapa-. Empezó solo con "CotaVariable" (petición del usuario:
        /// "añade tambien el mapa conceptual para la opcion CotaVariable, ya que ha podido
        /// establecerse por Escandallo y quiero saber su historial"); ahora también
        /// "AlturaManetaPorDefecto" y "AlturaManeta" (misma petición, mismo motivo, para estas dos
        /// opciones).</summary>
        private static readonly HashSet<string> OpcionesInternasEnMapa = new(StringComparer.OrdinalIgnoreCase)
        {
            "CotaVariable", "AlturaManetaPorDefecto", "AlturaManeta",
        };

        /// <summary>Aplica sobre _mapaCompleto, en este orden: (1) filtro FIJO -no configurable- a
        /// solo opciones reales de catálogo (Nombre empieza por "RO_": mismo criterio que
        /// EsOpcionPreguntable/EsValorOculto), dejando fuera del mapa a HardwareSupplier y a los
        /// flags internos del DSL (Puerta, Practicable...) — petición explícita del usuario: "que
        /// aparezcan solo las opciones que empiecen por RO_" — MÁS las excepciones explícitas de
        /// OpcionesInternasEnMapa (ver su comentario); (2) el de "mostrar ocultas" (ChkMostrarOcultos:
        /// por defecto se ocultan las que tienen valor final marcado Oculto, el check las
        /// desbloquea); (3) el de texto libre (TxtBuscarMapa).</summary>
        private void AplicarFiltroMapa()
        {
            if (GridMapa == null) return;

            string texto = (TxtBuscarMapa.Text ?? "").Trim();
            bool mostrarOcultos = ChkMostrarOcultos?.IsChecked == true;

            IEnumerable<FilaMapaOpcion> filas = _mapaCompleto
                .Where(f => f.Nombre.StartsWith("RO_", StringComparison.OrdinalIgnoreCase)
                         || OpcionesInternasEnMapa.Contains(f.Nombre));
            if (!mostrarOcultos) filas = filas.Where(f => !f.Oculto);
            if (!string.IsNullOrEmpty(texto)) filas = filas.Where(f => f.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase));

            GridMapa.ItemsSource = filas.ToList();
        }

        #endregion
    }
}
