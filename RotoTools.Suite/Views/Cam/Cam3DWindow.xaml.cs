using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using RotoTools;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.Cam
{
    /// <summary>
    /// Sustituye a Cam3D.cs/Cam3D.Designer.cs (WinForms): árbol de materiales + grid de todos los
    /// perfiles + lista de perfiles a instalar en 3D, con rol de mecanizado / descuento y
    /// posición de canal de herraje editables, e instalación en BBDD de ProfileOperations.
    /// Reutiliza tal cual RotoTools.Cam3DHelpers y RotoTools.Helpers del proyecto original.
    ///
    /// Como este módulo tiene un TreeView + 2 grids grandes, la ventana se abre maximizada/con un
    /// tamaño amplio y todo el layout se reparte en rejilla con separadores arrastrables, en vez
    /// del diálogo de tamaño fijo 1850x967 del original.
    /// </summary>
    public partial class Cam3DWindow : Window
    {
        private readonly List<OperationInstalarGridItem> _operacionesSeleccionadas;

        private List<MaterialBaseTreeRow> _materialesBase = new();
        private readonly ObservableCollection<MaterialBaseTreeRow> _materialesVisibles = new();
        private readonly ObservableCollection<MaterialTreeNode> _nodosRaiz = new();
        private readonly ObservableCollection<PerfilAInstalarRow> _perfilesAInstalar = new();
        private ICollectionView? _vistaResultado;

        public Cam3DWindow(List<OperationInstalarGridItem> operacionesSeleccionadas)
        {
            InitializeComponent();

            _operacionesSeleccionadas = operacionesSeleccionadas ?? new List<OperationInstalarGridItem>();

            GridMateriales.ItemsSource = _materialesVisibles;
            TreeMateriales.ItemsSource = _nodosRaiz;
            GridResultado.ItemsSource = _perfilesAInstalar;

            var opcionesRol = new List<string> { "" };
            opcionesRol.AddRange(RotoTools.Cam3DHelpers.RolesMecanizado3D);
            ColRolMecanizado.ItemsSource = opcionesRol;

            // Filtro por Rol mecanizado de la lista "Perfiles a instalar": se aplica sobre la
            // misma ObservableCollection ya asignada arriba (CollectionViewSource.GetDefaultView
            // devuelve la vista que WPF ya usa internamente para GridResultado), así que añadir,
            // quitar o editar filas sigue funcionando igual, solo cambia qué se muestra.
            CmbFiltroRolResultado.Items.Clear();
            CmbFiltroRolResultado.Items.Add(RotoTools.LocalizationManager.GetString("L_Todas"));
            foreach (string rol in RotoTools.Cam3DHelpers.RolesMecanizado3D) CmbFiltroRolResultado.Items.Add(rol);
            CmbFiltroRolResultado.SelectedIndex = 0;

            _vistaResultado = CollectionViewSource.GetDefaultView(_perfilesAInstalar);
            _vistaResultado.Filter = FiltrarPerfilAInstalarPorRol;

            CargarTextos();
            InitializeInfoConnection();
            MostrarOperacionesSeleccionadas();
            CargarMaterialesBase();
            CargarTreeViewMateriales();
            CargarGridMateriales();
        }

        #region Localización / cabecera

        private void CargarTextos()
        {
            Title = "Cam3D";
            TxtBtnVolver.Text = RotoTools.LocalizationManager.GetString("L_Volver");
            TxtBtnCatalogoOperaciones.Text = RotoTools.LocalizationManager.GetString("L_Operaciones");
            TxtBtnCatalogoPerfiles.Text = RotoTools.LocalizationManager.GetString("L_Perfiles");
            TxtBtnInstalar.Text = RotoTools.LocalizationManager.GetString("L_Instalar");
            TxtBtnLimpiarResultado.Text = RotoTools.LocalizationManager.GetString("L_Limpiar");
            LblTodosPerfiles.Text = "Todos los perfiles (doble clic para añadir a la lista)";
            LblResultado.Text = RotoTools.LocalizationManager.GetString("L_PerfilesAInstalar");
            LblFiltroRolResultado.Text = RotoTools.LocalizationManager.GetString("L_Rol") + ":";

            TxtMateriales.Text = SuiteLocalization.GetString("L_Suite_Materiales");

            ColMatReferencia.Header = RotoTools.LocalizationManager.GetString("L_Referencia");
            ColMatDescripcion.Header = RotoTools.LocalizationManager.GetString("L_Descripcion");
            ColMatRol.Header = RotoTools.LocalizationManager.GetString("L_Rol");
            string nivel = RotoTools.LocalizationManager.GetString("L_Nivel");
            ColMatNivel1.Header = nivel + " 1";
            ColMatNivel2.Header = nivel + " 2";
            ColMatNivel3.Header = nivel + " 3";
            ColMatNivel4.Header = nivel + " 4";
            ColMatNivel5.Header = nivel + " 5";

            ColResReferencia.Header = RotoTools.LocalizationManager.GetString("L_Referencia");
            ColResDescripcion.Header = RotoTools.LocalizationManager.GetString("L_Descripcion");
            ColResRol.Header = RotoTools.LocalizationManager.GetString("L_Rol");
            ColResAnchoInt.Header = SuiteLocalization.GetString("L_Suite_AnchoInt");
            ColResAnchoExt.Header = SuiteLocalization.GetString("L_Suite_AnchoExt");
            ColResCuerpoInt.Header = SuiteLocalization.GetString("L_Suite_CuerpoInt");
            ColResCuerpoExt.Header = SuiteLocalization.GetString("L_Suite_CuerpoExt");
            ColResAltura.Header = RotoTools.LocalizationManager.GetString("L_Altura");
            ColRolMecanizado.Header = RotoTools.LocalizationManager.GetString("L_RolMecanizado");
            ColResDescuentoCanalHerraje.Header = RotoTools.LocalizationManager.GetString("L_DescuentoCanalHerraje");
            ColResPosicionCanalHerraje.Header = RotoTools.LocalizationManager.GetString("L_PosicionCanalHerraje");
        }

        private bool FiltrarPerfilAInstalarPorRol(object obj)
        {
            if (CmbFiltroRolResultado.SelectedIndex <= 0) return true;
            if (obj is not PerfilAInstalarRow fila) return false;

            string rol = (string)CmbFiltroRolResultado.SelectedItem;
            return string.Equals(fila.RolMecanizado, rol, StringComparison.OrdinalIgnoreCase);
        }

        private void CmbFiltroRolResultado_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => _vistaResultado?.Refresh();

        private void InitializeInfoConnection()
        {
            try
            {
                TxtConexion.Text = RotoTools.Helpers.GetServer() + @"\" + RotoTools.Helpers.GetDataBase();
            }
            catch
            {
                TxtConexion.Text = "";
            }
        }

        /// <summary>Idéntico a Cam3D.MostrarOperacionesSeleccionadas: informativo, "RO_" + nombre
        /// por cada operación que se instalará en 3D sobre los perfiles que se añadan abajo.</summary>
        private void MostrarOperacionesSeleccionadas()
        {
            ListaOperacionesInfo.Items.Clear();

            if (_operacionesSeleccionadas.Count == 0)
            {
                ListaOperacionesInfo.Items.Add(RotoTools.LocalizationManager.GetString("L_NoHayOperacionesSeleccionadas"));
            }
            else
            {
                foreach (var op in _operacionesSeleccionadas)
                    ListaOperacionesInfo.Items.Add("RO_" + op.OperationName);
            }

            GrpOperacionesInfo.Text = $"{RotoTools.LocalizationManager.GetString("L_OperacionesAInstalarEn3D")} ({_operacionesSeleccionadas.Count})";
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Close();

        #endregion

        #region Carga de materiales base (árbol + grid de "todos los perfiles")

        /// <summary>Idéntico a Cam3D.CargarMaterialesBase: una única consulta con LEFT JOIN a
        /// Distances (descuento de canal de herraje del rol "esclavo de ala") que alimenta tanto
        /// el árbol como la grid de todos los perfiles. Sin esto no hay ninguna otra consulta SQL
        /// para poblar ninguno de los dos.</summary>
        private void CargarMaterialesBase()
        {
            var lista = new List<MaterialBaseTreeRow>();

            try
            {
                using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
                conexion.Open();

                using var cmd = new SqlCommand(@"
SELECT
    mb.RowId,
    mb.ReferenciaBase,
    mb.Descripcion,
    mb.Nivel1,
    mb.Nivel2,
    mb.Nivel3,
    mb.Nivel4,
    mb.Nivel5,
    mb.Role,
    p.AnchoInterior,
    p.AnchoExterior,
    p.CuerpoInterior,
    p.CuerpoExterior,
    p.Altura,
    d.PDistance AS DescuentoCanalHerraje
FROM MaterialesBase mb
INNER JOIN Perfiles p
    ON p.ReferenciaBase = mb.ReferenciaBase
LEFT JOIN Distances d
    ON d.MasterId = mb.RowId AND d.SlaveId = @slaveId
WHERE mb.[Role]='frame' OR mb.[Role]='sash' OR mb.[Role]='mullion' OR mb.[Role]='sash stop'
ORDER BY
    mb.Nivel1,
    mb.Nivel2,
    mb.Nivel3,
    mb.Nivel4,
    mb.Nivel5,
    mb.ReferenciaBase", conexion);

                cmd.Parameters.Add("@slaveId", System.Data.SqlDbType.UniqueIdentifier).Value =
                    Guid.Parse(RotoTools.Cam3DHelpers.RowIdDescuentoTipoEsclavoAla);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new MaterialBaseTreeRow
                    {
                        RowId = reader.GetGuid(0),
                        ReferenciaBase = reader[1]?.ToString()?.Trim() ?? "",
                        Descripcion = reader[2]?.ToString()?.Trim() ?? "",
                        Nivel1 = reader[3]?.ToString()?.Trim() ?? "",
                        Nivel2 = reader[4]?.ToString()?.Trim() ?? "",
                        Nivel3 = reader[5]?.ToString()?.Trim() ?? "",
                        Nivel4 = reader[6]?.ToString()?.Trim() ?? "",
                        Nivel5 = reader[7]?.ToString()?.Trim() ?? "",
                        Role = reader[8]?.ToString()?.Trim() ?? "",
                        AnchoInterior = RotoTools.Cam3DHelpers.ConvertirADouble(reader[9]),
                        AnchoExterior = RotoTools.Cam3DHelpers.ConvertirADouble(reader[10]),
                        CuerpoInterior = RotoTools.Cam3DHelpers.ConvertirADouble(reader[11]),
                        CuerpoExterior = RotoTools.Cam3DHelpers.ConvertirADouble(reader[12]),
                        Altura = RotoTools.Cam3DHelpers.ConvertirADouble(reader[13]),
                        DescuentoCanalHerraje = reader.IsDBNull(14) ? (double?)null : RotoTools.Cam3DHelpers.ConvertirADouble(reader[14])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los materiales base:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _materialesBase = lista;
        }

        /// <summary>Idéntico a Cam3D.CargarTreeViewMateriales: descompone Nivel1..5 en una
        /// jerarquía de carpetas ("(Sin definir)" si un nivel está en blanco) y añade la
        /// referencia como hoja al final de la rama. Árbol totalmente colapsado al cargar, igual
        /// que el original (sin ExpandAll).</summary>
        private void CargarTreeViewMateriales()
        {
            _nodosRaiz.Clear();

            foreach (var fila in _materialesBase)
            {
                if (string.IsNullOrWhiteSpace(fila.ReferenciaBase)) continue;

                string[] niveles = { fila.Nivel1, fila.Nivel2, fila.Nivel3, fila.Nivel4, fila.Nivel5 };
                IList<MaterialTreeNode> nodosActuales = _nodosRaiz;
                MaterialTreeNode? ultimoNodo = null;

                foreach (var nivelRaw in niveles)
                {
                    if (string.IsNullOrWhiteSpace(nivelRaw)) break;

                    string nivel = nivelRaw.Trim();
                    var existente = nodosActuales.FirstOrDefault(n => !n.EsHoja && string.Equals(n.Texto, nivel, StringComparison.OrdinalIgnoreCase));
                    if (existente == null)
                    {
                        existente = new MaterialTreeNode { Texto = nivel };
                        nodosActuales.Add(existente);
                    }

                    ultimoNodo = existente;
                    // Los hijos reales del nodo (List<MaterialTreeNode>) pasan a ser la colección
                    // "actual" de la siguiente vuelta, así que Add() de aquí en adelante sí
                    // modifica el árbol de verdad (no una copia).
                    nodosActuales = existente.Hijos;
                }

                var hoja = new MaterialTreeNode { Texto = fila.ReferenciaBase, ReferenciaBase = fila.ReferenciaBase };
                if (ultimoNodo != null) ultimoNodo.Hijos.Add(hoja);
                else _nodosRaiz.Add(hoja);
            }
        }

        private void CargarGridMateriales()
        {
            _materialesVisibles.Clear();
            foreach (var fila in _materialesBase) _materialesVisibles.Add(fila);
        }

        /// <summary>Idéntico a Cam3D.txt_Buscar_TextChanged: filtra solo por subcadena de
        /// ReferenciaBase (no descripción/niveles/rol) y no afecta al árbol.</summary>
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = (TxtBuscar.Text ?? "").Trim();
            _materialesVisibles.Clear();

            IEnumerable<MaterialBaseTreeRow> query = string.IsNullOrEmpty(texto)
                ? _materialesBase
                : _materialesBase.Where(x => !string.IsNullOrEmpty(x.ReferenciaBase) &&
                                              x.ReferenciaBase.Contains(texto, StringComparison.OrdinalIgnoreCase));

            foreach (var fila in query) _materialesVisibles.Add(fila);
        }

        private void GridMateriales_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridMateriales.SelectedItem is MaterialBaseTreeRow fila)
                AgregarPerfilAResultado(fila.ReferenciaBase);
        }

        private void GridMateriales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridMateriales.SelectedItem is MaterialBaseTreeRow fila)
                SeleccionarNodoEnTreeView(fila.ReferenciaBase);
        }

        /// <summary>Selecciona (y expande hasta) el nodo hoja cuya ReferenciaBase coincide,
        /// igual que Cam3D.SeleccionarNodoEnTreeView + BuscarNodoPorTag, adaptado al patrón
        /// MVVM de IsExpanded/IsSelected bindables (ver Cam3DModels.MaterialTreeNode). Además
        /// hace scroll para dejar el nodo visible (ver DesplazarTreeViewHastaNodo).</summary>
        private void SeleccionarNodoEnTreeView(string referenciaBase)
        {
            var ruta = new List<MaterialTreeNode>();

            bool BuscarYExpandir(IEnumerable<MaterialTreeNode> nodos)
            {
                foreach (var nodo in nodos)
                {
                    if (nodo.EsHoja && string.Equals(nodo.ReferenciaBase, referenciaBase, StringComparison.OrdinalIgnoreCase))
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
        private void DesplazarTreeViewHastaNodo(List<MaterialTreeNode> ruta)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ItemsControl contenedorActual = TreeMateriales;
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

        private void TreeMateriales_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ObtenerTreeViewItemDesdeOrigen(e.OriginalSource as DependencyObject);
            if (item?.DataContext is not MaterialTreeNode nodo) return;

            if (nodo.EsHoja) AgregarPerfilAResultado(nodo.ReferenciaBase!);
            else AgregarPerfilesDeNodoAResultado(nodo);
        }

        private static TreeViewItem? ObtenerTreeViewItemDesdeOrigen(DependencyObject? source)
        {
            while (source != null && source is not TreeViewItem)
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
        }

        /// <summary>Idéntico a Cam3D.AgregarPerfilesDeNodoAResultado + RecolectarReferenciasHoja:
        /// añade todas las hojas bajo una carpeta, en el orden del árbol.</summary>
        private void AgregarPerfilesDeNodoAResultado(MaterialTreeNode nodoCarpeta)
        {
            var referencias = new List<string>();
            void Recolectar(MaterialTreeNode nodo)
            {
                if (nodo.EsHoja) { referencias.Add(nodo.ReferenciaBase!); return; }
                foreach (var hijo in nodo.Hijos) Recolectar(hijo);
            }
            Recolectar(nodoCarpeta);

            foreach (var referencia in referencias) AgregarPerfilAResultado(referencia);
        }

        #endregion

        #region Lista de perfiles a instalar (AgregarPerfilAResultado / Quitar / Limpiar)

        /// <summary>Idéntico a Cam3D.AgregarPerfilAResultado: sin duplicados, sin consulta SQL
        /// (todo viene precargado de _materialesBase), rol/descuento/posición por defecto
        /// tomados de la biblioteca de perfiles embebida o, si no hay match, del rol de
        /// MaterialesBase (Cam3DHelpers.RolPorDefecto).</summary>
        private void AgregarPerfilAResultado(string? referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia)) return;
            referencia = referencia.Trim();

            if (_perfilesAInstalar.Any(p => string.Equals(p.ReferenciaBase, referencia, StringComparison.OrdinalIgnoreCase)))
                return;

            var filaOrigen = _materialesBase.FirstOrDefault(m => string.Equals(m.ReferenciaBase, referencia, StringComparison.OrdinalIgnoreCase));
            if (filaOrigen == null) return;

            string rolMecanizado = "";
            double? canal = null;

            try
            {
                var biblioteca = RotoTools.Cam3DHelpers.CargarBibliotecaPerfiles3D();
                if (biblioteca != null && biblioteca.TryGetValue(referencia, out var entradaLibreria))
                {
                    rolMecanizado = RotoTools.Cam3DHelpers.NormalizarRolBiblioteca(entradaLibreria.Role);
                    canal = entradaLibreria.PosicionCanalHerraje;
                }
            }
            catch
            {
                // Igual que el original: si la biblioteca embebida no resuelve la referencia,
                // simplemente se cae al rol por defecto calculado más abajo.
            }

            if (string.IsNullOrEmpty(rolMecanizado))
                rolMecanizado = RotoTools.Cam3DHelpers.RolPorDefecto(filaOrigen.Role);

            _perfilesAInstalar.Add(new PerfilAInstalarRow
            {
                ProfileId = filaOrigen.RowId,
                ReferenciaBase = filaOrigen.ReferenciaBase,
                Descripcion = filaOrigen.Descripcion,
                Role = filaOrigen.Role,
                RolMecanizado = rolMecanizado,
                AnchoInterior = filaOrigen.AnchoInterior,
                AnchoExterior = filaOrigen.AnchoExterior,
                CuerpoInterior = filaOrigen.CuerpoInterior,
                CuerpoExterior = filaOrigen.CuerpoExterior,
                Altura = filaOrigen.Altura,
                DescuentoCanalHerraje = filaOrigen.DescuentoCanalHerraje,
                PosicionCanalHerraje = canal
            });
        }

        private void BtnQuitarPerfil_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is PerfilAInstalarRow fila)
                _perfilesAInstalar.Remove(fila);
        }

        private void BtnLimpiarResultado_Click(object sender, RoutedEventArgs e)
        {
            if (_perfilesAInstalar.Count == 0) return;

            var respuesta = MessageBox.Show(
                $"¿Quitar los {_perfilesAInstalar.Count} perfil(es) de la lista de perfiles a instalar?",
                "", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (respuesta == MessageBoxResult.Yes) _perfilesAInstalar.Clear();
        }

        #endregion

        #region Instalación 3D (btn_InstalarOperaciones_Click)

        /// <summary>Idéntico a Cam3D.btn_InstalarOperaciones_Click: valida, resuelve plantillas
        /// del catálogo 3D por Operación+Rol, y las instala en una única transacción (con
        /// rollback si algo falla), igual que el original.</summary>
        private void BtnInstalarOperaciones_Click(object sender, RoutedEventArgs e)
        {
            if (_operacionesSeleccionadas.Count == 0)
            {
                MessageBox.Show(
                    "No hay operaciones seleccionadas para instalar. Cierre esta ventana, seleccione operaciones en la grid del CAM y vuelva a pulsar 'Instalar 3D'.",
                    "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var perfilesAInstalar = _perfilesAInstalar.ToList();
            if (perfilesAInstalar.Count == 0)
            {
                MessageBox.Show(
                    "Añada al menos un perfil a la lista de instalación, haciendo doble clic sobre él en el árbol o en la grid de perfiles.",
                    "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sinRol = perfilesAInstalar.Where(p => string.IsNullOrWhiteSpace(p.RolMecanizado)).ToList();
            if (sinRol.Count > 0)
            {
                MessageBox.Show("Indique el 'Rol mecanizado' para todos los perfiles de la lista:" + Environment.NewLine +
                    string.Join(", ", sinRol.Select(p => p.ReferenciaBase)), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sinDescuento = perfilesAInstalar.Where(p =>
                RotoTools.Cam3DHelpers.RolesConCanalHerraje.Contains(p.RolMecanizado) && p.DescuentoCanalHerraje == null).ToList();
            if (sinDescuento.Count > 0)
            {
                MessageBox.Show("Indique el 'Descuento canal de herraje' para los siguientes perfiles (rol de hoja):" + Environment.NewLine +
                    string.Join(", ", sinDescuento.Select(p => p.ReferenciaBase)), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sinPosicion = perfilesAInstalar.Where(p =>
                RotoTools.Cam3DHelpers.RolesConCanalHerraje.Contains(p.RolMecanizado) && p.PosicionCanalHerraje == null).ToList();
            if (sinPosicion.Count > 0)
            {
                MessageBox.Show("Indique la 'Posición canal de herraje' para los siguientes perfiles (rol de hoja):" + Environment.NewLine +
                    string.Join(", ", sinPosicion.Select(p => p.ReferenciaBase)), "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int perfilesProcesados = 0, operacionesInstaladas = 0, operacionesOmitidas = 0;
            var combinacionesSinDefinicion = new List<string>();
            var operacionesSinDefinicionEnCatalogo = new List<string>();

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                EnableControls(false);
                ProgressInstalar3D.Visibility = Visibility.Visible;
                ProgressInstalar3D.Value = 0;
                ProgressInstalar3D.Maximum = perfilesAInstalar.Count > 0 ? perfilesAInstalar.Count : 1;

                var catalogo = RotoTools.Cam3DHelpers.CargarCatalogoOperaciones3D();

                var operacionesConDefinicion = _operacionesSeleccionadas
                    .Select(op => "RO_" + op.OperationName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(n => n, _ => false, StringComparer.OrdinalIgnoreCase);

                // Igual que el original: asegura que la definición 2D de cada operación exista
                // antes de instalar el mecanizado 3D sobre ella.
                var mechanizedOperationsEmbebidos = RotoTools.Helpers.CargarMechanizedOperationsRotoEmbebidos();
                var macrosEmbeddedMechanizedOperations = RotoTools.Helpers.CargarMacrosMechanizedOperationsEmbebidos();
                var macroOperationsShapesEmbeddedList = RotoTools.Helpers.CargarMacrosOperationsShapesEmbebidos();
                foreach (var op in _operacionesSeleccionadas)
                {
                    RotoTools.Cam3DHelpers.AsegurarDefinicion2DInstalada("RO_" + op.OperationName, op.OperationShapeList,
                        op.OperationShapeExtList, mechanizedOperationsEmbebidos, macrosEmbeddedMechanizedOperations,
                        macroOperationsShapesEmbeddedList);
                }

                using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                {
                    conexion.Open();
                    using var tx = conexion.BeginTransaction();
                    try
                    {
                        foreach (var perfil in perfilesAInstalar)
                        {
                            if (perfil.ProfileId == Guid.Empty)
                            {
                                combinacionesSinDefinicion.Add($"{perfil.ReferenciaBase}: no se han encontrado datos constructivos.");
                                ProgressInstalar3D.Value++;
                                DoEvents();
                                continue;
                            }

                            perfilesProcesados++;

                            var variables = new Dictionary<string, double>
                            {
                                ["AnchoInterior"] = perfil.AnchoInterior,
                                ["AnchoExterior"] = perfil.AnchoExterior,
                                ["CuerpoInterior"] = perfil.CuerpoInterior,
                                ["CuerpoExterior"] = perfil.CuerpoExterior,
                                ["Altura"] = perfil.Altura
                            };

                            if (RotoTools.Cam3DHelpers.RolesConCanalHerraje.Contains(perfil.RolMecanizado))
                            {
                                variables["Ala"] = perfil.DescuentoCanalHerraje ?? 0;
                                variables["PosicionCanalHerraje"] = perfil.PosicionCanalHerraje ?? 0;
                            }

                            foreach (var op in _operacionesSeleccionadas)
                            {
                                string nombreCompleto = "RO_" + op.OperationName;
                                var plantillas = catalogo.Where(c =>
                                    string.Equals(c.OperationName, nombreCompleto, StringComparison.OrdinalIgnoreCase) &&
                                    string.Equals(c.Role, perfil.RolMecanizado, StringComparison.OrdinalIgnoreCase)).ToList();

                                if (plantillas.Count == 0) continue;

                                operacionesConDefinicion[nombreCompleto] = true;

                                foreach (var plantilla in plantillas)
                                {
                                    if (RotoTools.Cam3DHelpers.ExisteProfileOperation(conexion, tx, perfil.ProfileId, plantilla.OperationName, plantilla.Outer))
                                    {
                                        operacionesOmitidas++;
                                        continue;
                                    }

                                    RotoTools.Cam3DHelpers.InstalarProfileOperation(conexion, tx, perfil.ProfileId, perfil.ReferenciaBase, plantilla, variables);
                                    operacionesInstaladas++;
                                }
                            }

                            ProgressInstalar3D.Value++;
                            DoEvents();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }

                foreach (var kv in operacionesConDefinicion.Where(k => !k.Value))
                    operacionesSinDefinicionEnCatalogo.Add(kv.Key);

                _perfilesAInstalar.Clear();

                var resumen = new System.Text.StringBuilder();
                resumen.AppendLine($"Perfiles procesados: {perfilesProcesados}");
                resumen.AppendLine($"Operaciones instaladas: {operacionesInstaladas}");
                resumen.AppendLine($"Operaciones omitidas (ya existían): {operacionesOmitidas}");

                if (combinacionesSinDefinicion.Count > 0)
                {
                    resumen.AppendLine();
                    resumen.AppendLine("Perfiles sin datos constructivos:");
                    resumen.AppendLine(string.Join(Environment.NewLine, combinacionesSinDefinicion.Take(20)));
                    if (combinacionesSinDefinicion.Count > 20)
                        resumen.AppendLine($"... y {combinacionesSinDefinicion.Count - 20} más.");
                }

                if (operacionesSinDefinicionEnCatalogo.Count > 0)
                {
                    resumen.AppendLine();
                    resumen.AppendLine("No se ha encontrado definición en el catálogo 3D (para ningún rol de los perfiles de la lista) para:");
                    resumen.AppendLine(string.Join(Environment.NewLine, operacionesSinDefinicionEnCatalogo.Take(20)));
                    if (operacionesSinDefinicionEnCatalogo.Count > 20)
                        resumen.AppendLine($"... y {operacionesSinDefinicionEnCatalogo.Count - 20} más.");
                }

                bool hayAvisos = combinacionesSinDefinicion.Count > 0 || operacionesSinDefinicionEnCatalogo.Count > 0;
                MessageBox.Show(resumen.ToString(), "Instalación de mecanizados 3D",
                    MessageBoxButton.OK, hayAvisos ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al instalar los mecanizados 3D:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                EnableControls(true);
                ProgressInstalar3D.Value = 0;
                ProgressInstalar3D.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Catálogos de administración (aplazados a una próxima entrega)

        private void BtnCatalogoOperaciones_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new Cam3DCatalogoOperacionesWindow(_operacionesSeleccionadas) { Owner = this };
            ventana.ShowDialog();
        }

        private void BtnCatalogoPerfiles_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new Cam3DBibliotecaPerfilesWindow { Owner = this };
            ventana.ShowDialog();
        }

        #endregion

        #region Utilidades

        private void EnableControls(bool enabled)
        {
            BtnInstalarOperaciones.IsEnabled = enabled;
            GridMateriales.IsEnabled = enabled;
            GridResultado.IsEnabled = enabled;
            TreeMateriales.IsEnabled = enabled;
            TxtBuscar.IsEnabled = enabled;
        }

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
