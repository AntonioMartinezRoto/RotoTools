using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using RotoEntities;

namespace RotoTools.Suite.Views.Exportador
{
    /// <summary>
    /// Sustituye a ExportacionOrgadata.cs/.Designer.cs (WinForms). Ver el comentario grande en el
    /// XAML para el resumen general (por qué se omiten los 3 pares combo+etiqueta muertos del
    /// original, y por qué no hay ningún atajo de teclado que reproducir).
    /// </summary>
    public partial class ExportacionOrgadataWindow : Window
    {
        private readonly XmlData _exportDataXml;

        public ExportacionOrgadataWindow(XmlData exportDataXml)
        {
            InitializeComponent();

            _exportDataXml = exportDataXml;

            CargarTextos();
            LoadSets("");
        }

        #region Localización

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_ExportarOrgadata");
            TxtTitulo.Text = Title;

            ChkAll.Content = RotoTools.LocalizationManager.GetString("L_SeleccionarTodos");
            LblBusqueda.Text = RotoTools.LocalizationManager.GetString("L_Buscar");

            TxtBtnExport.Text = RotoTools.Suite.Services.SuiteLocalization.GetString("L_Suite_Exportar");
        }

        #endregion

        /// <summary>Igual que LoadSets en el original: mismo comportamiento (y misma ausencia de
        /// lista maestra de selección) ya documentado en
        /// ExportacionWinPerfilWindow.xaml.cs/LoadSets — filtrar el buscador reconstruye la lista
        /// entera marcando todo según el estado actual de chk_All, perdiendo cualquier selección
        /// manual anterior. No se corrige aquí.</summary>
        private void LoadSets(string filter)
        {
            IEnumerable<Set> query = _exportDataXml.SetList;

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(s => s.Code != null && s.Code.ToLower().Contains(filter.ToLower()));
            }

            bool marcarTodos = ChkAll.IsChecked == true;

            var lista = query.OrderBy(s => s.Code)
                .Select(s => new SetSeleccionable(s, marcarTodos))
                .ToList();

            ListaSets.ItemsSource = lista;
        }

        #region Events

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSets(TxtFilter.Text);
        }

        private void ChkAll_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ListaSets.ItemsSource is List<SetSeleccionable> visibles)
            {
                bool marcado = ChkAll.IsChecked == true;
                foreach (var item in visibles)
                    item.Seleccionado = marcado;
            }
        }

        /// <summary>Igual que btn_ExportSets_Click en el original: doble MessageBox si algo falla
        /// dentro de GenerarExportacion ("Error (17)" interno + "Error (18)" aquí) — mismo
        /// comportamiento que ExportacionWinPerfil ("Error (16)"+"Error (15)"), no se corrige.</summary>
        private void BtnExportSets_Click(object sender, RoutedEventArgs e)
        {
            bool haySeleccion = ListaSets.ItemsSource is List<SetSeleccionable> sets && sets.Any(s => s.Seleccionado);
            if (!haySeleccion) return;

            var saveFileDialog = new SaveFileDialog { Filter = "Archivo Excel (*.xlsx)|*.xlsx", Title = "Save as", FileName = "Export.xlsx" };
            if (saveFileDialog.ShowDialog() != true) return;

            string excelPath = saveFileDialog.FileName;

            ProgressExport.Visibility = Visibility.Visible;
            Mouse.OverrideCursor = Cursors.Wait;
            bool resultadoExportacion;
            try
            {
                resultadoExportacion = GenerarExportacionReflexion(excelPath);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                ProgressExport.Visibility = Visibility.Collapsed;
            }

            if (resultadoExportacion)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ExportacionCompletada"), "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Error (18)", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Reutilización de RotoTools.ExportacionOrgadata por reflexión

        /// <summary>
        /// Crea una instancia OCULTA (nunca visible) de RotoTools.ExportacionOrgadata. A diferencia
        /// de ExportacionWinPerfil, aquí ExportDataXml es una propiedad PRIVADA (con setter también
        /// privado, ver "private XmlData ExportDataXml { get; set; }" en el original), así que hace
        /// falta reflexión también para inyectarla, no solo para chkList_Sets.
        /// </summary>
        private RotoTools.ExportacionOrgadata CrearFormularioOculto()
        {
            var formOculto = new RotoTools.ExportacionOrgadata();
            var tipo = formOculto.GetType();

            tipo.GetProperty("ExportDataXml", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(formOculto, _exportDataXml);

            var chkListSets = (System.Windows.Forms.CheckedListBox)tipo
                .GetField("chkList_Sets", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(formOculto)!;

            if (ListaSets.ItemsSource is List<SetSeleccionable> setsVisibles)
            {
                foreach (var item in setsVisibles.Where(i => i.Seleccionado))
                    chkListSets.Items.Add(item.Set, true);
            }

            return formOculto;
        }

        /// <summary>GenerarExportacion SÍ es un método privado en el original, así que hace falta
        /// invocarlo por reflexión (ver comentario de CrearFormularioOculto).</summary>
        private bool GenerarExportacionReflexion(string excelPath)
        {
            using var formOculto = CrearFormularioOculto();
            var metodo = formOculto.GetType().GetMethod("GenerarExportacion", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (bool)metodo.Invoke(formOculto, new object[] { excelPath })!;
        }

        #endregion
    }
}
