using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using RotoTools;

namespace RotoTools.Suite.Views.Cam
{
    /// <summary>
    /// Sustituye a Cam3DCatalogoAdmin.cs/.Designer.cs (WinForms): administración exclusiva del
    /// catálogo embebido de plantillas de mecanizado 3D (Resources\Mecanizados3D
    /// \CatalogoOperaciones3D.json), usado por Cam3DWindow.BtnInstalarOperaciones_Click. Reutiliza
    /// tal cual RotoTools.Operacion3DTemplate, RotoTools.OperacionFaltanteRow y
    /// RotoTools.Cam3DHelpers del proyecto original (son públicos y viven en el mismo ensamblado
    /// que se referencia vía ProjectReference), sin necesidad de redefinir ningún modelo aquí.
    /// Misma lógica que el original: copia de trabajo en memoria, filas nuevas resaltadas hasta
    /// guardar, Guardar serializa agrupado por Rol (igual formato que el recurso embebido) y
    /// localiza el fichero fuente subiendo hasta 8 niveles de directorio desde el ejecutable.
    /// </summary>
    public partial class Cam3DCatalogoOperacionesWindow : Window
    {
        private readonly List<OperationInstalarGridItem> _operacionesSeleccionadas;
        private List<Operacion3DTemplate> _catalogoTrabajo = new();
        private readonly List<OperacionFaltanteRow> _operacionesFaltantes = new();
        private readonly HashSet<Operacion3DTemplate> _filasNuevasSinGuardar = new();
        private string? _rutaArchivoCatalogo;
        private string _tituloFaltantesBase = "";

        public Cam3DCatalogoOperacionesWindow(List<OperationInstalarGridItem> operacionesSeleccionadas)
        {
            InitializeComponent();
            _operacionesSeleccionadas = operacionesSeleccionadas ?? new List<OperationInstalarGridItem>();

            CargarTextos();
            _tituloFaltantesBase = LblFaltantes.Text;
            _rutaArchivoCatalogo = ResolverRutaArchivo("CatalogoOperaciones3D.json");

            _catalogoTrabajo = Cam3DHelpers.CargarCatalogoOperaciones3D().Select(ClonarPlantilla).ToList();

            ConfigurarFiltros();
            RefrescarGridCatalogo();
            CargarOperacionesFaltantes();
        }

        #region Localización / cabecera

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_CatalogoOperaciones3D");
            TxtTitulo.Text = Title;
            TxtSubtitulo.Text = "Añada al catálogo las combinaciones Operación/Rol que todavía no tienen una plantilla definida.";
            LblCatalogoActual.Text = RotoTools.LocalizationManager.GetString("L_CatalogoActual");
            LblFaltantes.Text = RotoTools.LocalizationManager.GetString("L_OperacionesSinDefinicionEnCatalogo");
            LblFiltroExterior.Text = RotoTools.LocalizationManager.GetString("L_Lado") + ":";
            LblFiltroRol.Text = RotoTools.LocalizationManager.GetString("L_Rol") + ":";
            LblBuscarCatalogo.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
            LblBuscarFaltantes.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
            TxtBtnVolver.Text = RotoTools.LocalizationManager.GetString("L_Volver");
            TxtBtnAnadirFilaFaltante.Text = "Añadir fila";
        }

        private void ConfigurarFiltros()
        {
            CmbFiltroExterior.Items.Clear();
            CmbFiltroExterior.Items.Add(RotoTools.LocalizationManager.GetString("L_Todas"));
            CmbFiltroExterior.Items.Add(RotoTools.LocalizationManager.GetString("L_Interior"));
            CmbFiltroExterior.Items.Add(RotoTools.LocalizationManager.GetString("L_Exterior"));
            CmbFiltroExterior.SelectedIndex = 0;

            CmbFiltroRol.Items.Clear();
            CmbFiltroRol.Items.Add(RotoTools.LocalizationManager.GetString("L_Todas"));
            foreach (string rol in Cam3DHelpers.RolesMecanizado3D) CmbFiltroRol.Items.Add(rol);
            CmbFiltroRol.SelectedIndex = 0;

            var opcionesRol = new List<string> { "" };
            opcionesRol.AddRange(Cam3DHelpers.RolesMecanizado3D);
            ColCatRol.ItemsSource = opcionesRol;
            ColFalRol.ItemsSource = opcionesRol;
        }

        #endregion

        #region Clonado (para no mutar la caché compartida hasta Guardar)

        private static Operacion3DTemplate ClonarPlantilla(Operacion3DTemplate original) => new()
        {
            OperationName = original.OperationName,
            Role = original.Role,
            Outer = original.Outer,
            XFormula = original.XFormula,
            YFormula = original.YFormula,
            ZFormula = original.ZFormula,
            Plane = original.Plane,
            Depth = original.Depth,
            Master = original.Master,
            XmlParameters = original.XmlParameters,
            Layers = original.Layers,
            MirrorHorizontalForMachining = original.MirrorHorizontalForMachining,
            MirrorVerticalForMachining = original.MirrorVerticalForMachining,
            RotationForMachining = original.RotationForMachining,
            Face = original.Face,
            Disabled = original.Disabled,
            IsBidirectional = original.IsBidirectional
        };

        #endregion

        #region Grid de catálogo: filtro/orden/refresco

        private void TxtBuscarCatalogo_TextChanged(object sender, TextChangedEventArgs e) => RefrescarGridCatalogo();
        private void FiltroCatalogo_Changed(object sender, SelectionChangedEventArgs e) => RefrescarGridCatalogo();

        private void RefrescarGridCatalogo()
        {
            var seleccionActual = GridCatalogo.SelectedItem as Operacion3DTemplate;
            string texto = (TxtBuscarCatalogo.Text ?? "").Trim();

            IEnumerable<Operacion3DTemplate> query = _catalogoTrabajo;

            if (!string.IsNullOrEmpty(texto))
                query = query.Where(p => (p.OperationName ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase));

            if (CmbFiltroExterior.SelectedIndex == 1) query = query.Where(p => p.Outer == 0);
            else if (CmbFiltroExterior.SelectedIndex == 2) query = query.Where(p => p.Outer == 1);

            if (CmbFiltroRol.SelectedIndex > 0)
            {
                string rol = (string)CmbFiltroRol.SelectedItem;
                query = query.Where(p => string.Equals(p.Role, rol, StringComparison.OrdinalIgnoreCase));
            }

            var lista = query
                .OrderBy(p => p.OperationName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Role, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Outer)
                .ToList();

            GridCatalogo.ItemsSource = lista;

            if (seleccionActual != null)
            {
                var match = lista.FirstOrDefault(p => ReferenceEquals(p, seleccionActual));
                if (match != null) GridCatalogo.SelectedItem = match;
            }

        }

        /// <summary>Igual que Cam3DCatalogoAdmin_CellFormatting: colorea en ámbar las filas cuyo
        /// Operacion3DTemplate está en _filasNuevasSinGuardar (añadidas esta sesión, sin guardar
        /// todavía). Como no hay INotifyPropertyChanged en Operacion3DTemplate (viene tal cual del
        /// proyecto original), se hace vía el evento LoadingRow (wireado una única vez en el XAML)
        /// en vez de un DataTrigger de binding: cada vez que se reasigna ItemsSource, WPF regenera
        /// todas las filas y este evento se dispara de nuevo para cada una.</summary>
        private void GridCatalogo_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is Operacion3DTemplate plantilla && _filasNuevasSinGuardar.Contains(plantilla))
                e.Row.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 244, 204));
            else
                e.Row.ClearValue(DataGridRow.BackgroundProperty);
        }

        #endregion

        #region Acciones fila catálogo: Duplicar / Eliminar

        private void BtnDuplicarCatalogo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not Operacion3DTemplate plantilla) return;

            var duplicado = ClonarPlantilla(plantilla);
            duplicado.Role = ""; // en blanco a propósito: evita guardar sin querer un duplicado idéntico
            _catalogoTrabajo.Add(duplicado);
            _filasNuevasSinGuardar.Add(duplicado);

            RefrescarGridCatalogo();
            GridCatalogo.SelectedItem = duplicado;
        }

        private void BtnEliminarCatalogo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not Operacion3DTemplate plantilla) return;

            var resultado = MessageBox.Show(
                $"¿Eliminar del catálogo la operación {plantilla.OperationName} / {plantilla.Role} (Exterior={plantilla.Outer})?",
                "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultado != MessageBoxResult.Yes) return;

            _catalogoTrabajo.Remove(plantilla);
            _filasNuevasSinGuardar.Remove(plantilla);
            RefrescarGridCatalogo();
        }

        #endregion

        #region Grid de faltantes: cálculo / filtro

        /// <summary>Igual que CargarOperacionesFaltantes: para cada operación 2D seleccionada,
        /// si "RO_" + OperationName no existe TODAVÍA para ningún Rol en el catálogo, se añade
        /// como fila pendiente (una para interior, y otra para exterior si la operación también
        /// aplica al lado exterior).</summary>
        private void CargarOperacionesFaltantes()
        {
            _operacionesFaltantes.Clear();

            foreach (var op in _operacionesSeleccionadas)
            {
                string nombreCompleto = "RO_" + op.OperationName;

                bool yaDefinida = _catalogoTrabajo.Any(p => string.Equals(p.OperationName, nombreCompleto, StringComparison.OrdinalIgnoreCase));
                if (yaDefinida) continue;

                _operacionesFaltantes.Add(new OperacionFaltanteRow
                {
                    OperationName = nombreCompleto,
                    Role = "",
                    Outer = 0,
                    YFormula = "",
                    ZFormula = "",
                    Plane = 0,
                    Depth = 0
                });

                if (op.OperationShapeExtList != null && op.OperationShapeExtList.Count > 0)
                {
                    _operacionesFaltantes.Add(new OperacionFaltanteRow
                    {
                        OperationName = nombreCompleto,
                        Role = "",
                        Outer = 1,
                        YFormula = "",
                        ZFormula = "",
                        Plane = 0,
                        Depth = 0
                    });
                }
            }

            _operacionesFaltantes.Sort((a, b) => string.Compare(a.OperationName, b.OperationName, StringComparison.OrdinalIgnoreCase));
            AplicarFiltroFaltantes();
        }

        private void TxtBuscarFaltantes_TextChanged(object sender, TextChangedEventArgs e) => AplicarFiltroFaltantes();

        private void AplicarFiltroFaltantes()
        {
            string texto = (TxtBuscarFaltantes.Text ?? "").Trim();

            IEnumerable<OperacionFaltanteRow> query = _operacionesFaltantes;
            if (!string.IsNullOrEmpty(texto))
                query = query.Where(r => (r.OperationName ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase));

            GridFaltantes.ItemsSource = query.ToList();

            int cantidadOperaciones = _operacionesFaltantes.Select(r => r.OperationName).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            LblFaltantes.Text = $"{_tituloFaltantesBase} ({cantidadOperaciones})";
        }

        private void BtnAnadirFilaFaltante_Click(object sender, RoutedEventArgs e)
        {
            var nueva = new OperacionFaltanteRow { OperationName = "", Role = "", Outer = 0, YFormula = "", ZFormula = "", Plane = 0, Depth = 0 };
            _operacionesFaltantes.Add(nueva);
            AplicarFiltroFaltantes();
        }

        #endregion

        #region Acciones fila faltantes: Copiar de catálogo / Agregar al catálogo / Copiar todos los roles

        private void BtnCopiarDeCatalogo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OperacionFaltanteRow fila) return;

            if (GridCatalogo.SelectedItem is not Operacion3DTemplate seleccionada)
            {
                MessageBox.Show("Seleccione primero, en la grid del catálogo, la fila de la que quiere copiar los datos.", "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            fila.Role = seleccionada.Role;
            fila.Outer = seleccionada.Outer;
            fila.YFormula = seleccionada.YFormula;
            fila.ZFormula = seleccionada.ZFormula;
            fila.Plane = seleccionada.Plane;
            fila.Depth = seleccionada.Depth;

            AplicarFiltroFaltantes();
        }

        private void BtnAgregarAlCatalogo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OperacionFaltanteRow fila) return;

            if (string.IsNullOrWhiteSpace(fila.OperationName) || string.IsNullOrWhiteSpace(fila.Role))
            {
                MessageBox.Show("Indique al menos 'Operación' y 'Rol' en esta fila para poder añadirla al catálogo.", "",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool existe = _catalogoTrabajo.Any(p =>
                string.Equals(p.OperationName, fila.OperationName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.Role, fila.Role, StringComparison.OrdinalIgnoreCase) &&
                p.Outer == fila.Outer);

            if (existe)
            {
                MessageBox.Show($"Ya existe en el catálogo: {fila.OperationName} / {fila.Role} (Outer={fila.Outer}).", "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var nueva = new Operacion3DTemplate
            {
                OperationName = fila.OperationName.Trim(),
                Role = fila.Role.Trim(),
                Outer = fila.Outer,
                XFormula = "0",
                YFormula = fila.YFormula ?? "",
                ZFormula = fila.ZFormula ?? "",
                Plane = fila.Plane,
                Depth = fila.Depth,
                Master = 0,
                XmlParameters = "",
                Layers = null,
                MirrorHorizontalForMachining = 0,
                MirrorVerticalForMachining = 0,
                RotationForMachining = 0,
                Face = 0,
                Disabled = 0,
                IsBidirectional = 0
            };

            _catalogoTrabajo.Add(nueva);
            _filasNuevasSinGuardar.Add(nueva);
            _operacionesFaltantes.Remove(fila);

            RefrescarGridCatalogo();
            AplicarFiltroFaltantes();

            MessageBox.Show($"Añadido al catálogo (sin guardar todavía): {nueva.OperationName} / {nueva.Role}. Pulse Guardar para persistirlo.",
                "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCopiarTodosRoles_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not OperacionFaltanteRow fila) return;

            if (string.IsNullOrWhiteSpace(fila.OperationName))
            {
                MessageBox.Show("Indique la 'Operación' en esta fila antes de copiar todos los roles.", "",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GridCatalogo.SelectedItem is not Operacion3DTemplate plantillaSeleccionada)
            {
                MessageBox.Show("Seleccione primero, en la grid del catálogo, una de las filas de la operación de la que quiere copiar todos los roles.", "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var variantesPorRol = _catalogoTrabajo
                .Where(p => string.Equals(p.OperationName, plantillaSeleccionada.OperationName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (variantesPorRol.Count <= 1)
            {
                MessageBox.Show("Esta operación del catálogo solo tiene un rol definido: use 'Copiar de catálogo' en su lugar.", "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string nombreDestino = fila.OperationName.Trim();
            int anadidas = 0, yaExistian = 0;

            foreach (var variante in variantesPorRol)
            {
                bool existe = _catalogoTrabajo.Any(p =>
                    string.Equals(p.OperationName, nombreDestino, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.Role, variante.Role, StringComparison.OrdinalIgnoreCase) &&
                    p.Outer == variante.Outer);

                if (existe) { yaExistian++; continue; }

                var nueva = ClonarPlantilla(variante);
                nueva.OperationName = nombreDestino;
                _catalogoTrabajo.Add(nueva);
                _filasNuevasSinGuardar.Add(nueva);
                anadidas++;
            }

            if (anadidas > 0)
            {
                _operacionesFaltantes.RemoveAll(r => string.Equals(r.OperationName, nombreDestino, StringComparison.OrdinalIgnoreCase));
                RefrescarGridCatalogo();
                AplicarFiltroFaltantes();
            }

            string mensaje = anadidas > 0
                ? $"Añadidas {anadidas} combinaciones de rol para {nombreDestino} (a partir de {plantillaSeleccionada.OperationName})."
                : "Todas las combinaciones ya existían en el catálogo; no se ha añadido nada.";
            if (yaExistian > 0) mensaje += $" Se omitieron {yaExistian} que ya existían.";
            if (anadidas > 0) mensaje += " Pulse Guardar para persistirlo.";

            MessageBox.Show(mensaje, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Guardar / Volver

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var filasSinRol = _catalogoTrabajo.Where(p => string.IsNullOrWhiteSpace(p.Role)).ToList();
            if (filasSinRol.Count > 0)
            {
                string nombres = string.Join(", ", filasSinRol.Select(p => p.OperationName).Distinct(StringComparer.OrdinalIgnoreCase));
                var respuesta = MessageBox.Show(
                    $"Hay {filasSinRol.Count} fila(s) en el catálogo sin Rol asignado ({nombres})." + Environment.NewLine +
                    "Esas filas NO se guardarán en el fichero hasta que se les asigne un Rol." + Environment.NewLine + Environment.NewLine +
                    "¿Continuar guardando el resto igualmente?",
                    "", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (respuesta != MessageBoxResult.Yes) return;
            }

            string? ruta = _rutaArchivoCatalogo;
            if (string.IsNullOrEmpty(ruta))
            {
                var dialogo = new SaveFileDialog
                {
                    Title = "Guardar CatalogoOperaciones3D.json",
                    Filter = "Fichero JSON (*.json)|*.json",
                    FileName = "CatalogoOperaciones3D.json"
                };
                if (dialogo.ShowDialog() != true) return;
                ruta = dialogo.FileName;
            }

            try
            {
                string json = SerializarCatalogoPorRole(_catalogoTrabajo);
                File.WriteAllText(ruta, json, new UTF8Encoding(false));

                _rutaArchivoCatalogo = ruta;
                Cam3DHelpers.ActualizarCacheCatalogo(_catalogoTrabajo.Select(ClonarPlantilla).ToList());

                MessageBox.Show(
                    "Catálogo guardado correctamente en:" + Environment.NewLine + ruta + Environment.NewLine + Environment.NewLine +
                    "Recuerde subir este cambio al repositorio (git) para que quede disponible en la próxima compilación. " +
                    "Mientras tanto, esta sesión ya utiliza el catálogo actualizado.",
                    "", MessageBoxButton.OK, MessageBoxImage.Information);

                _filasNuevasSinGuardar.Clear();
                RefrescarGridCatalogo();
                CargarOperacionesFaltantes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el catálogo:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>Igual que Cam3DCatalogoAdmin.SerializarCatalogoPorRole: agrupa por Rol (los 9
        /// roles canónicos, en ese orden, más cualquier rol no canónico presente, ordenado
        /// alfabéticamente), cada grupo ordenado por Operación y luego por Exterior. Las filas con
        /// Rol en blanco se excluyen (no se guardan hasta que se les asigne un Rol).</summary>
        private static string SerializarCatalogoPorRole(List<Operacion3DTemplate> catalogo)
        {
            var agrupado = new Dictionary<string, List<Operacion3DTemplate>>();

            foreach (string rol in Cam3DHelpers.RolesMecanizado3D)
            {
                agrupado[rol] = catalogo
                    .Where(p => string.Equals(p.Role, rol, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.OperationName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(p => p.Outer)
                    .ToList();
            }

            var rolesExtra = catalogo
                .Select(p => p.Role)
                .Where(r => !string.IsNullOrWhiteSpace(r) && !Cam3DHelpers.RolesMecanizado3D.Contains(r, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r, StringComparer.OrdinalIgnoreCase);

            foreach (string rol in rolesExtra)
            {
                agrupado[rol] = catalogo
                    .Where(p => string.Equals(p.Role, rol, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.OperationName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(p => p.Outer)
                    .ToList();
            }

            var opciones = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(agrupado, opciones);
        }

        /// <summary>Igual que ResolverRutaArchivoCatalogo/ResolverRutaArchivoBiblioteca (WinForms):
        /// sube hasta 8 niveles de directorio desde la carpeta del ejecutable buscando
        /// Resources\Mecanizados3D\{nombreArchivo}. Como RotoTools.Suite vive en una subcarpeta del
        /// mismo repositorio que el proyecto original, este mismo recorrido encuentra el fichero
        /// fuente compartido (el mismo que usa/edita también el RotoTools clásico).</summary>
        internal static string? ResolverRutaArchivo(string nombreArchivo)
        {
            string rutaRelativa = Path.Combine("Resources", "Mecanizados3D", nombreArchivo);
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            for (int i = 0; i < 8 && dir != null; i++)
            {
                string candidato = Path.Combine(dir.FullName, rutaRelativa);
                if (File.Exists(candidato)) return candidato;
                dir = dir.Parent;
            }

            return null;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Close();

        #endregion
    }
}
