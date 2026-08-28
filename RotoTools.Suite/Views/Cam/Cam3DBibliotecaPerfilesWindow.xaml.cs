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
    /// Sustituye a Cam3DPerfilesCatalogoAdmin.cs/.Designer.cs (WinForms): administración exclusiva
    /// de la biblioteca embebida de perfiles (Resources\Mecanizados3D\BibliotecaPerfiles3D.json),
    /// usada por Cam3DWindow.AgregarPerfilAResultado para autocompletar Rol/canal de herraje de
    /// perfiles ya conocidos. Reutiliza tal cual RotoTools.PerfilLibreriaEntry y
    /// RotoTools.Cam3DHelpers del proyecto original. Mismo patrón que
    /// Cam3DCatalogoOperacionesWindow: copia de trabajo en memoria, filas nuevas resaltadas hasta
    /// guardar, Guardar serializa una lista plana ordenada por ReferenciaBase y localiza el
    /// fichero fuente subiendo hasta 8 niveles de directorio desde el ejecutable.
    /// </summary>
    public partial class Cam3DBibliotecaPerfilesWindow : Window
    {
        private List<PerfilLibreriaEntry> _bibliotecaTrabajo = new();
        private readonly HashSet<PerfilLibreriaEntry> _filasNuevasSinGuardar = new();
        private string? _rutaArchivoBiblioteca;

        public Cam3DBibliotecaPerfilesWindow()
        {
            InitializeComponent();

            CargarTextos();
            _rutaArchivoBiblioteca = Cam3DCatalogoOperacionesWindow.ResolverRutaArchivo("BibliotecaPerfiles3D.json");
            _bibliotecaTrabajo = Cam3DHelpers.CargarListaBibliotecaPerfiles3D().Select(ClonarEntrada).ToList();

            ConfigurarComboRol();
            ConfigurarComboFiltroRol();
            RefrescarGridBiblioteca();
        }

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_BibliotecaPerfiles");
            TxtTitulo.Text = Title;
            LblBuscarBiblioteca.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
            LblFiltroRol.Text = RotoTools.LocalizationManager.GetString("L_Rol") + ":";
            TxtBtnNuevoPerfil.Text = RotoTools.LocalizationManager.GetString("L_NuevoPerfil");
            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
            TxtBtnVolver.Text = RotoTools.LocalizationManager.GetString("L_Volver");
            ColReferenciaBase.Header = RotoTools.LocalizationManager.GetString("L_ReferenciaBase");
            ColRol.Header = RotoTools.LocalizationManager.GetString("L_Rol");
            ColPosicionCanalHerraje.Header = RotoTools.LocalizationManager.GetString("L_PosicionCanalHerraje");
            Resources["TooltipDuplicarPerfil"] = RotoTools.LocalizationManager.GetString("L_TooltipDuplicarPerfil");
            Resources["TooltipEliminarPerfil"] = RotoTools.LocalizationManager.GetString("L_TooltipEliminarPerfil");
        }

        /// <summary>Igual criterio que ConfigurarComboRol, pero para el combo de FILTRADO de la
        /// grid (no de edición de celda): "Todas" + los 9 roles canónicos + cualquier Rol
        /// "heredado" ya presente en la biblioteca, para poder filtrar también por esos valores
        /// antiguos.</summary>
        private void ConfigurarComboFiltroRol()
        {
            var opciones = new List<string> { RotoTools.LocalizationManager.GetString("L_Todas") };
            opciones.AddRange(Cam3DHelpers.RolesMecanizado3D);

            var extra = _bibliotecaTrabajo
                .Select(p => p.Role)
                .Where(r => !string.IsNullOrWhiteSpace(r) && !Cam3DHelpers.RolesMecanizado3D.Contains(r, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r, StringComparer.OrdinalIgnoreCase);

            opciones.AddRange(extra);
            CmbFiltroRol.ItemsSource = opciones;
            CmbFiltroRol.SelectedIndex = 0;
        }

        private void CmbFiltroRol_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefrescarGridBiblioteca();

        /// <summary>Igual que ConfigurarGridBiblioteca (WinForms): blanco + los 9 roles canónicos +
        /// cualquier valor de Rol "heredado" ya presente en la biblioteca que no esté en esa lista
        /// (p.ej. "Elevadora", "Lift Sash", "Sash"), para que esas filas antiguas sigan siendo
        /// seleccionables/visibles sin forzar al admin a normalizarlas primero.</summary>
        private void ConfigurarComboRol()
        {
            var opciones = new List<string> { "" };
            opciones.AddRange(Cam3DHelpers.RolesMecanizado3D);

            var extra = _bibliotecaTrabajo
                .Select(p => p.Role)
                .Where(r => !string.IsNullOrWhiteSpace(r) && !Cam3DHelpers.RolesMecanizado3D.Contains(r, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r, StringComparer.OrdinalIgnoreCase);

            opciones.AddRange(extra);
            ColRol.ItemsSource = opciones;
        }

        private static PerfilLibreriaEntry ClonarEntrada(PerfilLibreriaEntry original) => new()
        {
            ReferenciaBase = original.ReferenciaBase,
            Role = original.Role,
            PosicionCanalHerraje = original.PosicionCanalHerraje
        };

        #region Grid: filtro/orden/refresco + resaltado de filas nuevas

        private void TxtBuscarBiblioteca_TextChanged(object sender, TextChangedEventArgs e) => RefrescarGridBiblioteca();

        private void RefrescarGridBiblioteca()
        {
            var seleccionActual = GridBiblioteca.SelectedItem as PerfilLibreriaEntry;
            string texto = (TxtBuscarBiblioteca.Text ?? "").Trim();

            IEnumerable<PerfilLibreriaEntry> query = _bibliotecaTrabajo;
            if (!string.IsNullOrEmpty(texto))
                query = query.Where(p => !string.IsNullOrEmpty(p.ReferenciaBase) &&
                                          p.ReferenciaBase.Contains(texto, StringComparison.OrdinalIgnoreCase));

            if (CmbFiltroRol.SelectedIndex > 0 && CmbFiltroRol.SelectedItem is string rolFiltro)
                query = query.Where(p => string.Equals(p.Role, rolFiltro, StringComparison.OrdinalIgnoreCase));

            var lista = query.OrderBy(p => p.ReferenciaBase, StringComparer.OrdinalIgnoreCase).ToList();
            GridBiblioteca.ItemsSource = lista;

            if (seleccionActual != null)
            {
                var match = lista.FirstOrDefault(p => ReferenceEquals(p, seleccionActual));
                if (match != null) GridBiblioteca.SelectedItem = match;
            }
        }

        private void SeleccionarEntrada(PerfilLibreriaEntry entrada)
        {
            if (GridBiblioteca.ItemsSource is List<PerfilLibreriaEntry> lista && lista.Any(p => ReferenceEquals(p, entrada)))
                GridBiblioteca.SelectedItem = entrada;
        }

        /// <summary>Igual que Cam3DPerfilesCatalogoAdmin_CellFormatting: resalta en ámbar las
        /// filas añadidas/duplicadas esta sesión y aún sin guardar.</summary>
        private void GridBiblioteca_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is PerfilLibreriaEntry entrada && _filasNuevasSinGuardar.Contains(entrada))
                e.Row.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 244, 204));
            else
                e.Row.ClearValue(DataGridRow.BackgroundProperty);
        }

        #endregion

        #region Nuevo / Duplicar / Eliminar

        private void BtnNuevoPerfil_Click(object sender, RoutedEventArgs e)
        {
            var nueva = new PerfilLibreriaEntry { ReferenciaBase = "", Role = "", PosicionCanalHerraje = 0 };
            _bibliotecaTrabajo.Add(nueva);
            _filasNuevasSinGuardar.Add(nueva);
            RefrescarGridBiblioteca();
            SeleccionarEntrada(nueva);
        }

        private void BtnDuplicarPerfil_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not PerfilLibreriaEntry entrada) return;

            var duplicado = ClonarEntrada(entrada);
            duplicado.ReferenciaBase = ""; // en blanco a propósito: evita dos filas con la misma clave antes de asignarla
            _bibliotecaTrabajo.Add(duplicado);
            _filasNuevasSinGuardar.Add(duplicado);
            RefrescarGridBiblioteca();
            SeleccionarEntrada(duplicado);
        }

        private void BtnEliminarPerfil_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not PerfilLibreriaEntry entrada) return;

            string nombre = string.IsNullOrWhiteSpace(entrada.ReferenciaBase) ? "(sin referencia)" : entrada.ReferenciaBase;
            var resultado = MessageBox.Show($"¿Eliminar de la biblioteca el perfil {nombre}?", "",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultado != MessageBoxResult.Yes) return;

            _bibliotecaTrabajo.Remove(entrada);
            _filasNuevasSinGuardar.Remove(entrada);
            RefrescarGridBiblioteca();
        }

        #endregion

        #region Guardar / Volver

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var filasSinReferencia = _bibliotecaTrabajo.Where(p => string.IsNullOrWhiteSpace(p.ReferenciaBase)).ToList();
            if (filasSinReferencia.Count > 0)
            {
                var respuesta = MessageBox.Show(
                    $"Hay {filasSinReferencia.Count} fila(s) sin 'Referencia base'." + Environment.NewLine +
                    "Esas filas NO se guardarán en el fichero hasta que se les asigne una referencia." + Environment.NewLine + Environment.NewLine +
                    "¿Continuar guardando el resto igualmente?",
                    "", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (respuesta != MessageBoxResult.Yes) return;
            }

            var referenciasDuplicadas = _bibliotecaTrabajo
                .Where(p => !string.IsNullOrWhiteSpace(p.ReferenciaBase))
                .GroupBy(p => p.ReferenciaBase.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (referenciasDuplicadas.Count > 0)
            {
                var respuesta = MessageBox.Show(
                    $"Hay referencias repetidas en la biblioteca: {string.Join(", ", referenciasDuplicadas)}." + Environment.NewLine +
                    "Al cargarse, solo se tendrá en cuenta la última de cada una." + Environment.NewLine + Environment.NewLine +
                    "¿Continuar guardando igualmente?",
                    "", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (respuesta != MessageBoxResult.Yes) return;
            }

            string? ruta = _rutaArchivoBiblioteca;
            if (string.IsNullOrEmpty(ruta))
            {
                var dialogo = new SaveFileDialog
                {
                    Title = "Guardar BibliotecaPerfiles3D.json",
                    Filter = "Fichero JSON (*.json)|*.json",
                    FileName = "BibliotecaPerfiles3D.json"
                };
                if (dialogo.ShowDialog() != true) return;
                ruta = dialogo.FileName;
            }

            try
            {
                var aGuardar = _bibliotecaTrabajo
                    .Where(p => !string.IsNullOrWhiteSpace(p.ReferenciaBase))
                    .OrderBy(p => p.ReferenciaBase, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(aGuardar, opciones);
                File.WriteAllText(ruta, json, new UTF8Encoding(false));

                _rutaArchivoBiblioteca = ruta;
                Cam3DHelpers.ActualizarCacheBibliotecaPerfiles(aGuardar.Select(ClonarEntrada).ToList());

                MessageBox.Show(
                    "Biblioteca de perfiles guardada correctamente en:" + Environment.NewLine + ruta + Environment.NewLine + Environment.NewLine +
                    "Recuerde subir este cambio al repositorio (git) para que quede disponible en la próxima compilación. " +
                    "Mientras tanto, esta sesión ya utiliza la biblioteca actualizada.",
                    "", MessageBoxButton.OK, MessageBoxImage.Information);

                _filasNuevasSinGuardar.Clear();
                RefrescarGridBiblioteca();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la biblioteca de perfiles:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Close();

        #endregion
    }
}
