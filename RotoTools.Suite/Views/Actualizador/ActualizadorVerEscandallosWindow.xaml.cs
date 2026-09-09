using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RotoEntities;
using RotoTools.Suite.Services;
using EnumTipoEscandallo = RotoTools.Enums.enumRotoTipoEscandallo;

namespace RotoTools.Suite.Views.Actualizador
{
    /// <summary>
    /// Sustituye a ActualizadorEscandallos.cs/.Designer.cs (WinForms): mismo comportamiento
    /// (filtro por código + contenido/Programa del escandallo seleccionado), reutilizando tal cual
    /// RotoTools.Helpers.CargarEscandallosEmbebidos vía ProjectReference.
    ///
    /// Añadido (no existía en el original): buscador de texto dentro del contenido del escandallo
    /// seleccionado, estilo Ctrl+F de un editor de texto -ver comentario de clase del XAML para el
    /// porqué y el flujo completo-. La parte menos evidente es CÓMO se resaltan varias coincidencias
    /// a la vez sobre un TextBox normal: un TextBox de WPF solo sabe pintar UNA selección (con
    /// Select/SelectionStart+SelectionLength), no colores de fondo distintos por rango de texto
    /// arbitrario -eso sí lo permite un RichTextBox, pero cambiar a RichTextBox habría significado
    /// perder gratis el TextWrapping="NoWrap" + scroll horizontal que ya tenía esta pantalla (un
    /// FlowDocument no tiene un "no ajustar línea" directo)-. La solución adoptada: se sigue usando
    /// el TextBox normal tal cual, y se dibuja un Rectangle semitransparente por cada coincidencia
    /// en un Canvas SUPERPUESTO (mismo hueco de Grid, hermano del TextBox, no hijo), usando
    /// TextBox.GetRectFromCharacterIndex para saber dónde cae cada carácter y
    /// TransformToVisual/TransformBounds para pasar esas coordenadas -relativas al TextBox- a las
    /// del Canvas. Ver ObtenerRectanguloDeRango/DibujarResaltados más abajo.
    /// </summary>
    public partial class ActualizadorVerEscandallosWindow : Window
    {
        private List<Escandallo> _escandallosList = new();

        /// <summary>Posición (índice de carácter en TxtContenidoEscandallo.Text) y longitud de cada
        /// coincidencia de la búsqueda actual, en el orden en que aparecen en el texto.</summary>
        private readonly List<(int Inicio, int Longitud)> _coincidenciasBusqueda = new();

        /// <summary>Índice dentro de _coincidenciasBusqueda de la coincidencia "actual" (la que se
        /// resalta en naranja y hacia la que se ha desplazado el contenido); -1 si no hay ninguna
        /// (búsqueda vacía o sin resultados).</summary>
        private int _indiceCoincidenciaActual = -1;

        /// <summary>Amarillo semitransparente para TODAS las coincidencias.</summary>
        private static readonly SolidColorBrush PincelResaltadoTodos = new(Color.FromArgb(0x66, 0xFF, 0xEB, 0x3B));

        /// <summary>Naranja, más opaco, solo para la coincidencia ACTUAL: así se localiza de un
        /// vistazo cuál de todas las resaltadas en amarillo es la que se acaba de mostrar.</summary>
        private static readonly SolidColorBrush PincelResaltadoActual = new(Color.FromArgb(0x99, 0xFF, 0x6D, 0x00));

        public ActualizadorVerEscandallosWindow()
        {
            InitializeComponent();

            CargarEscandallos();
            FillEscandallosList(_escandallosList);
            CargarTextos();

            // ScrollChangedEvent es un evento enrutado que burbujea: aunque el ScrollViewer real
            // vive DENTRO de la plantilla del TextBox (PART_ContentHost, ver RotoTextBox/estilo por
            // defecto), engancharlo aquí con AddHandler funciona igual, porque el evento sube por
            // el árbol visual pasando por el propio TextBox. Hace falta para recolocar los
            // rectángulos de resaltado cuando el usuario desplaza el contenido a mano (con las
            // barras de scroll o la rueda del ratón), no solo cuando navega con "Siguiente/Anterior".
            TxtContenidoEscandallo.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(TxtContenidoEscandallo_ScrollChanged));
            TxtContenidoEscandallo.SizeChanged += (_, _) => DibujarResaltados();
        }

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_Escandallos");
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_Escandallos");
            LblFiltrar.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
            TxtSeleccionaUno.Text = SuiteLocalization.GetString("L_Suite_VerEscandallosSeleccionaUno");
        }

        private void CargarEscandallos()
        {
            var tiposSeleccionados = new List<EnumTipoEscandallo>
            {
                EnumTipoEscandallo.PVC, EnumTipoEscandallo.Aluminio, EnumTipoEscandallo.GestionGeneral,
                EnumTipoEscandallo.GestionManillas, EnumTipoEscandallo.GestionBombillos, EnumTipoEscandallo.PersonalizacionClientes
            };

            _escandallosList = RotoTools.Helpers.CargarEscandallosEmbebidos(tiposSeleccionados);
        }

        private void FillEscandallosList(IEnumerable<Escandallo> escandallosList)
        {
            ListaEscandallos.ItemsSource = escandallosList.OrderBy(c => c.Codigo).ToList();
        }

        private void TxtFiltro_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = TxtFiltro.Text.Trim().ToUpper();
            var escandallosFiltrados = _escandallosList.Where(o => o.Codigo.ToUpper().Contains(filtro)).ToList();
            FillEscandallosList(escandallosFiltrados);
        }

        private void ListaEscandallos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaEscandallos.SelectedItem is Escandallo esc)
            {
                // Igual que el original: si el Programa vino con los saltos de línea escapados,
                // se "desescapan" para que se vean como saltos de línea de verdad.
                string contenido = (esc.Programa ?? string.Empty)
                    .Replace("\\r\\n", "\r\n")
                    .Replace("\\n", "\r\n");

                TxtContenidoEscandallo.Text = contenido;
                TxtContenidoEscandallo.Visibility = Visibility.Visible;
                TxtSeleccionaUno.Visibility = Visibility.Collapsed;

                TxtContenidoEscandallo.SelectionStart = 0;
                TxtContenidoEscandallo.ScrollToHome();
            }
            else
            {
                TxtContenidoEscandallo.Text = "";
                TxtContenidoEscandallo.Visibility = Visibility.Collapsed;
                TxtSeleccionaUno.Visibility = Visibility.Visible;
            }

            // Si la barra de búsqueda ya estaba abierta con un término escrito, se repite la
            // búsqueda sobre el contenido del escandallo recién elegido en vez de cerrarla: es lo
            // que se espera de un buscador (Chrome, VS Code...) al cambiar de "documento" sin
            // haberlo pedido explícitamente. Si no hay ningún término, EjecutarBusqueda simplemente
            // deja _coincidenciasBusqueda vacía y no dibuja nada.
            if (PanelBusqueda.Visibility == Visibility.Visible)
                EjecutarBusqueda();
        }

        #region Buscador de texto dentro del contenido (Ctrl+F)

        /// <summary>Ctrl+F en cualquier punto de la ventana abre la barra de búsqueda; con la barra
        /// ya abierta, F3/Mayús+F3 navegan sin tener que volver a hacer clic en el cuadro de texto;
        /// Esc la cierra. Enganchado como PreviewKeyDown del Window (no del TextBox del contenido)
        /// para que funcione tenga el foco quien lo tenga -filtro de código, lista de escandallos o
        /// el propio contenido-, igual que el Ctrl+F de cualquier aplicación de verdad.</summary>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                AbrirBarraBusqueda();
                e.Handled = true;
            }
            else if (e.Key == Key.F3 && PanelBusqueda.Visibility == Visibility.Visible)
            {
                bool haciaAtras = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                IrACoincidencia(_indiceCoincidenciaActual + (haciaAtras ? -1 : 1));
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && PanelBusqueda.Visibility == Visibility.Visible)
            {
                CerrarBarraBusqueda();
                e.Handled = true;
            }
        }

        private void AbrirBarraBusqueda()
        {
            PanelBusqueda.Visibility = Visibility.Visible;
            TxtBuscarContenido.Focus();
            TxtBuscarContenido.SelectAll();

            // Si ya había un término escrito de una búsqueda anterior en esta misma sesión de la
            // ventana (p.ej. se cerró con Esc y se vuelve a abrir con Ctrl+F), se recalculan
            // coincidencias y resaltado por si el contenido cambió mientras tanto.
            if (!string.IsNullOrEmpty(TxtBuscarContenido.Text))
                EjecutarBusqueda();
        }

        private void CerrarBarraBusqueda()
        {
            PanelBusqueda.Visibility = Visibility.Collapsed;
            _coincidenciasBusqueda.Clear();
            _indiceCoincidenciaActual = -1;
            CanvasResaltadoBusqueda.Children.Clear();

            // Quita también la selección azul que hubiera dejado la última coincidencia visitada
            // (Select(0,0): sin longitud, no selecciona nada, solo mueve el caret al principio).
            TxtContenidoEscandallo.Select(0, 0);
            Keyboard.Focus(TxtContenidoEscandallo);
        }

        private void TxtBuscarContenido_TextChanged(object sender, TextChangedEventArgs e) => EjecutarBusqueda();

        private void TxtBuscarContenido_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool haciaAtras = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                IrACoincidencia(_indiceCoincidenciaActual + (haciaAtras ? -1 : 1));
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                CerrarBarraBusqueda();
                e.Handled = true;
            }
        }

        private void BtnBusquedaSiguiente_Click(object sender, RoutedEventArgs e) => IrACoincidencia(_indiceCoincidenciaActual + 1);

        private void BtnBusquedaAnterior_Click(object sender, RoutedEventArgs e) => IrACoincidencia(_indiceCoincidenciaActual - 1);

        private void BtnCerrarBusqueda_Click(object sender, RoutedEventArgs e) => CerrarBarraBusqueda();

        private void TxtContenidoEscandallo_ScrollChanged(object sender, ScrollChangedEventArgs e) => DibujarResaltados();

        /// <summary>Recalcula _coincidenciasBusqueda desde cero contra el contenido ACTUAL del
        /// TextBox (búsqueda simple, sin distinguir mayúsculas/minúsculas -el criterio habitual de
        /// un Ctrl+F, y el que pidió el usuario-, sin solapar coincidencias entre sí). Se llama en
        /// cada tecla que se escribe en el cuadro de búsqueda y también al cambiar de escandallo con
        /// la barra ya abierta.</summary>
        private void EjecutarBusqueda()
        {
            _coincidenciasBusqueda.Clear();
            _indiceCoincidenciaActual = -1;

            string termino = TxtBuscarContenido.Text;
            string contenido = TxtContenidoEscandallo.Text;

            if (!string.IsNullOrEmpty(termino) && !string.IsNullOrEmpty(contenido))
            {
                int pos = 0;
                while (true)
                {
                    int encontrado = contenido.IndexOf(termino, pos, StringComparison.OrdinalIgnoreCase);
                    if (encontrado < 0) break;

                    _coincidenciasBusqueda.Add((encontrado, termino.Length));
                    pos = encontrado + termino.Length; // termino.Length > 0 aquí siempre: avanza de verdad, sin riesgo de bucle infinito.
                }
            }

            ActualizarContadorBusqueda();
            DibujarResaltados();

            if (_coincidenciasBusqueda.Count > 0)
                IrACoincidencia(0);
        }

        private void ActualizarContadorBusqueda()
        {
            if (string.IsNullOrEmpty(TxtBuscarContenido.Text))
                TxtContadorBusqueda.Text = "";
            else if (_coincidenciasBusqueda.Count == 0)
                TxtContadorBusqueda.Text = "Sin resultados";
            else
                TxtContadorBusqueda.Text = $"{_indiceCoincidenciaActual + 1} de {_coincidenciasBusqueda.Count}";
        }

        /// <summary>Se desplaza a la coincidencia "indice" (circular: por debajo de 0 va a la
        /// última, por encima de la última vuelve a la primera -mismo criterio que la mayoría de
        /// buscadores de editor-), la deja seleccionada en el TextBox y actualiza contador +
        /// resaltado. No hace nada si no hay ninguna coincidencia.</summary>
        private void IrACoincidencia(int indice)
        {
            if (_coincidenciasBusqueda.Count == 0) return;

            if (indice < 0) indice = _coincidenciasBusqueda.Count - 1;
            if (indice >= _coincidenciasBusqueda.Count) indice = 0;

            _indiceCoincidenciaActual = indice;
            var (inicio, longitud) = _coincidenciasBusqueda[indice];

            // Enfocar el TextBox y seleccionar el rango es lo que hace que WPF desplace su
            // ScrollViewer interno para mantener visible el caret/selección -no hay ningún método
            // público "ScrollToCharacterIndex", así que se aprovecha este efecto colateral ya
            // conocido en vez de calcular el scroll a mano-. ScrollToLine de refuerzo justo debajo:
            // garantiza el desplazamiento VERTICAL siempre, incluso si por cualquier motivo el
            // truco de foco+selección no llegara a desplazar (defensivo).
            TxtContenidoEscandallo.Focus();
            TxtContenidoEscandallo.Select(inicio, longitud);
            int lineaIndice = TxtContenidoEscandallo.GetLineIndexFromCharacterIndex(inicio);
            if (lineaIndice >= 0) TxtContenidoEscandallo.ScrollToLine(lineaIndice);

            ActualizarContadorBusqueda();
            DibujarResaltados();

            // El foco vuelve al cuadro de búsqueda: así se puede seguir escribiendo o pulsando
            // Enter/F3 sin tener que volver a hacer clic en él.
            TxtBuscarContenido.Focus();
        }

        /// <summary>Vuelve a pintar TODOS los rectángulos de resaltado desde cero (más simple y ya
        /// suficientemente rápido para el volumen de coincidencias real de un escandallo, en vez de
        /// llevar la cuenta de qué rectángulo mover) — se llama tras cada búsqueda, cada navegación,
        /// y cada vez que cambia lo que hay visible (scroll o redimensionado de la ventana, ver
        /// TxtContenidoEscandallo_ScrollChanged/SizeChanged en el constructor).</summary>
        private void DibujarResaltados()
        {
            CanvasResaltadoBusqueda.Children.Clear();
            if (_coincidenciasBusqueda.Count == 0) return;

            for (int i = 0; i < _coincidenciasBusqueda.Count; i++)
            {
                var (inicio, longitud) = _coincidenciasBusqueda[i];
                Rect? rect = ObtenerRectanguloDeRango(inicio, longitud);
                if (rect == null) continue; // fuera de la parte visible ahora mismo (con scroll): no hay nada que dibujar todavía.

                var rectangulo = new Rectangle
                {
                    Width = Math.Max(rect.Value.Width, 2),
                    Height = rect.Value.Height,
                    Fill = i == _indiceCoincidenciaActual ? PincelResaltadoActual : PincelResaltadoTodos,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(rectangulo, rect.Value.Left);
                Canvas.SetTop(rectangulo, rect.Value.Top);
                CanvasResaltadoBusqueda.Children.Add(rectangulo);
            }
        }

        /// <summary>Rectángulo (en coordenadas del Canvas de resaltado) que ocupa el tramo de texto
        /// que empieza en "inicio" y tiene "longitud" caracteres, o null si ese tramo cae fuera de
        /// lo que se ve ahora mismo del TextBox (con contenido desplazado por scroll) — en ese caso no hace falta ni
        /// crear el Rectangle, ClipToBounds del Canvas lo recortaría igual, así que se descarta
        /// antes para no acumular cientos de formas invisibles con una búsqueda de un término muy
        /// corto y repetido (p.ej. una sola letra) en un Programa largo.
        ///
        /// Asume coincidencia de una sola línea (el caso normal: TextWrapping="NoWrap" y ningún
        /// término de búsqueda real trae un salto de línea dentro): si alguna vez lo trajera, se
        /// dibuja igualmente un único rectángulo que abarca del principio al final -una
        /// aproximación visualmente imperfecta para ese caso raro, pero sin romper nada-.</summary>
        private Rect? ObtenerRectanguloDeRango(int inicio, int longitud)
        {
            if (TxtContenidoEscandallo.ActualWidth <= 0 || TxtContenidoEscandallo.ActualHeight <= 0)
                return null; // todavía sin layout (p.ej. ventana recién abierta): nada que calcular aún.

            try
            {
                Rect rectInicio = TxtContenidoEscandallo.GetRectFromCharacterIndex(inicio);
                Rect rectFin = TxtContenidoEscandallo.GetRectFromCharacterIndex(inicio + longitud);

                double top = Math.Min(rectInicio.Top, rectFin.Top);
                double bottom = Math.Max(rectInicio.Bottom, rectFin.Bottom);
                var rectTextBox = new Rect(rectInicio.Left, top, Math.Max(rectFin.Left - rectInicio.Left, 1), Math.Max(bottom - top, 1));

                var visibleTextBox = new Rect(0, 0, TxtContenidoEscandallo.ActualWidth, TxtContenidoEscandallo.ActualHeight);
                if (!visibleTextBox.IntersectsWith(rectTextBox))
                    return null;

                // GetRectFromCharacterIndex ya da coordenadas relativas al propio TextBox
                // (incluyendo el desplazamiento actual del scroll): se transforman a las del Canvas
                // superpuesto -que NO es hijo del TextBox, ver comentario del XAML- con
                // TransformToVisual, así el resultado es correcto sea cual sea el
                // Padding/BorderThickness de cada uno de los dos.
                GeneralTransform transformacion = TxtContenidoEscandallo.TransformToVisual(CanvasResaltadoBusqueda);
                return transformacion.TransformBounds(rectTextBox);
            }
            catch (Exception)
            {
                // Defensivo: GetRectFromCharacterIndex podría comportarse mal si el índice quedara
                // fuera de rango tras un cambio de contenido a medio recalcular (no debería llegar a
                // pasar, pero mejor no reventar el resaltado completo por una sola coincidencia).
                return null;
            }
        }

        #endregion
    }
}
