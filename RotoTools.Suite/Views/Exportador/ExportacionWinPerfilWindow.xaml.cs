using System;
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
    /// Sustituye a ExportacionWinPerfil.cs/.Designer.cs (WinForms). Ver el comentario grande en el
    /// XAML para el resumen general; aquí, el detalle de qué campos/controles privados del original
    /// hace falta inyectar por reflexión antes de invocar su GenerarExportacion.
    /// </summary>
    public partial class ExportacionWinPerfilWindow : Window
    {
        #region Estado

        private readonly XmlData _exportDataXml;

        // Mismos campos privados que el original (_perfilesListSelected/_perfilesAluListSelected/
        // _filtroListaPerfilesActivo/_filtroListaPerfilesAluActivo), con los mismos valores por
        // defecto (listas vacías, banderas a false).
        private List<Value> _perfilesListSelected = new();
        private List<Value> _perfilesAluListSelected = new();
        private bool _filtroListaPerfilesActivo;
        private bool _filtroListaPerfilesAluActivo;

        // Mismas propiedades públicas que el original (showSetDescriptionId/
        // showSetDescriptionPosition/showFittingId/showFittingLength/formatoTabla/showSetId), todas
        // false por defecto igual que en ExportacionWinPerfil.cs. Solo se pueden cambiar desde el
        // diálogo de opciones (F11, ver Window_KeyDown).
        private bool _showSetDescriptionId;
        private bool _showSetDescriptionPosition;
        private bool _showFittingId;
        private bool _showFittingLength;
        private bool _formatoTabla;
        private bool _showSetId;

        #endregion

        public ExportacionWinPerfilWindow(XmlData exportDataXml)
        {
            InitializeComponent();

            _exportDataXml = exportDataXml;

            CargarTextos();
            LoadColours();
            LoadProfiles();
            LoadProfilesAlu();
            LoadSistemas();
            LoadSets("");
        }

        #region Localización

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_ExportarWinPerfil");
            TxtTitulo.Text = Title;

            LblProfile.Text = RotoTools.LocalizationManager.GetString("L_Perfil");
            LblSystem.Text = RotoTools.LocalizationManager.GetString("L_Sistema");
            LblColour.Text = RotoTools.LocalizationManager.GetString("L_Color");
            ChkAll.Content = RotoTools.LocalizationManager.GetString("L_SeleccionarTodos");
            LblBusqueda.Text = RotoTools.LocalizationManager.GetString("L_Buscar");

            TxtBtnExport.Text = RotoTools.Suite.Services.SuiteLocalization.GetString("L_Suite_Exportar");
            LblOpciones.Text = RotoTools.Suite.Services.SuiteLocalization.GetString("L_Suite_OpcionesF11Hint");
        }

        #endregion

        #region Carga de combos / lista de Sets

        private void LoadSistemas()
        {
            if (_exportDataXml.OptionList == null) return;

            List<Value> valueList = _exportDataXml.OptionList.Where(o => o.Name == "1SISTEMA").FirstOrDefault()!.ValuesList.OrderBy(v => v.Valor).ToList();
            valueList.Insert(0, new Value { Valor = "" });
            CmbSistema.ItemsSource = valueList;
        }

        private void LoadColours()
        {
            List<Colour> colourList = _exportDataXml.ColourList.OrderBy(c => c.Name).ToList();
            colourList.Insert(0, new Colour { Name = "" });
            CmbColor.ItemsSource = colourList;
        }

        private void LoadProfiles()
        {
            List<Value> valueList = _exportDataXml.OptionList.Where(o => o.Name == "1PERFIL").FirstOrDefault()!.ValuesList.OrderBy(v => v.Valor).ToList();
            valueList.Insert(0, new Value { Valor = "" });
            CmbPerfil.ItemsSource = valueList;
        }

        private void LoadProfilesAlu()
        {
            List<Value> valueList = _exportDataXml.OptionList.Where(o => o.Name == "1PERFIL_ALU").FirstOrDefault()!.ValuesList.OrderBy(v => v.Valor).ToList();
            valueList.Insert(0, new Value { Valor = "" });
            CmbPerfilAlu.ItemsSource = valueList;
        }

        /// <summary>Igual que LoadSets en el original: OJO, a diferencia de los diálogos de filtro
        /// (ExportacionPerfilesFiltroWindow/ControlCambiosFiltroItemsWindow), aquí NO existe ninguna
        /// lista maestra que recuerde qué Sets estaban marcados antes de filtrar — cada llamada
        /// reconstruye chkList_Sets desde cero y marca TODOS los elementos nuevos según el estado
        /// actual de chk_All (chkList_Sets.Items.Add(set, chk_All.Checked) en el original). Esto
        /// significa que si el usuario marca Sets sueltos a mano y luego escribe en el buscador, la
        /// selección manual se pierde por completo (se sustituye por el estado de "Seleccionar
        /// todos"). Es un comportamiento real del original, no se corrige aquí.</summary>
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

        #endregion

        #region Events

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadSets(TxtFilter.Text);
        }

        /// <summary>Igual que chk_All_CheckedChanged en el original: solo afecta a los Sets
        /// actualmente visibles/cargados (el subconjunto filtrado en este momento), no a todo
        /// ExportDataXml.SetList.</summary>
        private void ChkAll_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ListaSets.ItemsSource is List<SetSeleccionable> visibles)
            {
                bool marcado = ChkAll.IsChecked == true;
                foreach (var item in visibles)
                    item.Seleccionado = marcado;
            }
        }

        /// <summary>Igual que ExportacionWinPerfil_KeyDown en el original: F11 es el único
        /// disparador del diálogo de opciones de columnas, no hay ningún botón visible para
        /// abrirlo (ver comentario en ExportacionOpcionesWindow.xaml).</summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F11) return;

            var dlg = new ExportacionOpcionesWindow(
                _showSetDescriptionId, _showSetDescriptionPosition, _showFittingId,
                _showFittingLength, _formatoTabla, _showSetId)
            { Owner = this };

            if (dlg.ShowDialog() == true)
            {
                _showSetDescriptionId = dlg.ShowSetDescriptionId;
                _showSetDescriptionPosition = dlg.ShowSetDescriptionPosition;
                _showFittingId = dlg.ShowFittingId;
                _showFittingLength = dlg.ShowFittingLength;
                _formatoTabla = dlg.FormatoTabla;
                _showSetId = dlg.ShowSetId;
            }
        }

        private void BtnFiltrarPerfil_Click(object sender, RoutedEventArgs e)
        {
            List<Value> profileList = _exportDataXml.OptionList.FirstOrDefault(o => o.Name == "1PERFIL")!.ValuesList.OrderBy(v => v.Valor).ToList();
            var dlg = new ExportacionPerfilesFiltroWindow(profileList, _perfilesListSelected) { Owner = this };

            if (dlg.ShowDialog() == true)
                _perfilesListSelected = dlg.PerfilesListSelected;

            if (_perfilesListSelected.Any())
            {
                CmbPerfil.SelectedIndex = -1;
                CmbPerfil.IsEnabled = false;
                _filtroListaPerfilesActivo = true;
            }
            else
            {
                CmbPerfil.IsEnabled = true;
                _filtroListaPerfilesActivo = false;
            }
        }

        /// <summary>Igual que btn_FiltrarPerfilAlu_Click en el original: el diálogo se sigue
        /// pre-cargando (a propósito, es un bug del original, no se corrige aquí) con
        /// _perfilesListSelected — la lista de Perfil, NO la de Perfil ALU — en vez de con
        /// _perfilesAluListSelected. Solo el resultado se guarda correctamente en
        /// _perfilesAluListSelected. Como ExportacionPerfilesFiltroWindow recibe esa lista por
        /// referencia y la muta en vivo mientras el usuario marca/desmarca casillas (igual que
        /// chkList_Perfiles_ItemCheck en el original), usar "Filtrar Perfil ALU" también dejará
        /// _perfilesListSelected con contenido de la sesión de Perfil ALU — el mismo efecto
        /// colateral que tiene el original al pasar la lista por referencia.</summary>
        private void BtnFiltrarPerfilAlu_Click(object sender, RoutedEventArgs e)
        {
            List<Value> profileList = _exportDataXml.OptionList.FirstOrDefault(o => o.Name == "1PERFIL_ALU")!.ValuesList.OrderBy(v => v.Valor).ToList();
            var dlg = new ExportacionPerfilesFiltroWindow(profileList, _perfilesListSelected) { Owner = this };

            if (dlg.ShowDialog() == true)
                _perfilesAluListSelected = dlg.PerfilesListSelected;

            if (_perfilesAluListSelected.Any())
            {
                CmbPerfilAlu.SelectedIndex = -1;
                CmbPerfilAlu.IsEnabled = false;
                _filtroListaPerfilesAluActivo = true;
            }
            else
            {
                CmbPerfilAlu.IsEnabled = true;
                _filtroListaPerfilesAluActivo = false;
            }
        }

        /// <summary>Igual que btn_ExportSets_Click en el original: si no hay ningún Set marcado, no
        /// hace nada (ni siquiera muestra un aviso). Ojo con el doble MessageBox si algo falla: el
        /// original ya muestra su propio "Error (16)" dentro de GenerarExportacion (catch interno)
        /// y ADEMÁS este método muestra "Error (15)" al ver que el resultado es false — dos avisos
        /// para un mismo fallo. Es un comportamiento real del original (ver el mismo patrón, con
        /// "Error (17)"+"Error (18)", en ExportacionOrgadata), no se corrige aquí.</summary>
        private void BtnExportSets_Click(object sender, RoutedEventArgs e)
        {
            bool haySeleccion = ListaSets.ItemsSource is List<SetSeleccionable> sets && sets.Any(s => s.Seleccionado);
            if (!haySeleccion) return;

            var saveFileDialog = new SaveFileDialog { Filter = "Archivo Excel (*.xlsx)|*.xlsx", Title = "Save as", FileName = "Export.xlsx" };
            if (saveFileDialog.ShowDialog() != true) return;

            string excelPath = saveFileDialog.FileName;

            // La barra de progreso del original (progress_Export) vive dentro del formulario
            // oculto y nunca se muestra: no hay forma sencilla de leer su avance real desde aquí sin
            // sondeo adicional, así que esta barra solo indica "operación en curso" (indeterminada),
            // igual de honesto que no mostrar nada, pero con mejor feedback visual para exportaciones
            // grandes.
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
                MessageBox.Show("Error (15)", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Reutilización de RotoTools.ExportacionWinPerfil por reflexión

        /// <summary>
        /// Crea una instancia OCULTA (nunca visible; nunca se llama Show/ShowDialog sobre ella) de
        /// RotoTools.ExportacionWinPerfil y le inyecta los mismos datos que usaría el formulario
        /// clásico antes de generar la exportación: ExportDataXml y los 6 show*/formatoTabla SÍ son
        /// propiedades públicas en el original, así que se asignan directamente; _perfilesListSelected/
        /// _perfilesAluListSelected/_filtroListaPerfilesActivo/_filtroListaPerfilesAluActivo SÍ son
        /// campos privados, así que hace falta reflexión; cmb_Perfil/cmb_Sistema/cmb_PerfilAlu/
        /// cmb_Color son controles privados de WinForms cuyo .Text lee GenerateRowCheckingConditions/
        /// GetFinalReferenceColor/GetColour tal cual — basta con inyectar el mismo texto elegido en
        /// los combos WPF equivalentes, sin necesidad de poblar su DataSource/Items; chkList_Sets es
        /// un CheckedListBox privado que GenerarExportacion recorre con .CheckedItems.OfType&lt;Set&gt;(),
        /// así que se rellena con los mismos Set marcados en ListaSets (WPF), todos "checked".
        /// </summary>
        private RotoTools.ExportacionWinPerfil CrearFormularioOculto()
        {
            var formOculto = new RotoTools.ExportacionWinPerfil();
            var tipo = formOculto.GetType();

            formOculto.ExportDataXml = _exportDataXml;
            formOculto.showSetDescriptionId = _showSetDescriptionId;
            formOculto.showSetDescriptionPosition = _showSetDescriptionPosition;
            formOculto.showFittingId = _showFittingId;
            formOculto.showFittingLength = _showFittingLength;
            formOculto.formatoTabla = _formatoTabla;
            formOculto.showSetId = _showSetId;

            tipo.GetField("_perfilesListSelected", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(formOculto, _perfilesListSelected);
            tipo.GetField("_perfilesAluListSelected", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(formOculto, _perfilesAluListSelected);
            tipo.GetField("_filtroListaPerfilesActivo", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(formOculto, _filtroListaPerfilesActivo);
            tipo.GetField("_filtroListaPerfilesAluActivo", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(formOculto, _filtroListaPerfilesAluActivo);

            SetControlText(tipo, formOculto, "cmb_Perfil", (CmbPerfil.SelectedItem as Value)?.Valor ?? "");
            SetControlText(tipo, formOculto, "cmb_Sistema", (CmbSistema.SelectedItem as Value)?.Valor ?? "");
            SetControlText(tipo, formOculto, "cmb_PerfilAlu", (CmbPerfilAlu.SelectedItem as Value)?.Valor ?? "");
            SetControlText(tipo, formOculto, "cmb_Color", (CmbColor.SelectedItem as Colour)?.Name ?? "");

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

        private static void SetControlText(Type tipo, object instancia, string nombreCampo, string texto)
        {
            var control = (System.Windows.Forms.Control)tipo
                .GetField(nombreCampo, BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(instancia)!;
            control.Text = texto;
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
