using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using RotoEntities;

namespace RotoTools.Suite.Views.Cam
{
    /// <summary>
    /// Fila de la lista de condiciones (listBox_Condiciones del original): envuelve una
    /// MechanizedConditions real, o representa el marcador "Sin Condiciones" (Condicion == null)
    /// cuando la lista de condiciones usadas está vacía — mismo criterio que el original, incluida
    /// la particularidad de que en ese caso el filtro se aplica por Conditions == "" (ver comentario
    /// en CamConfigurarGeometriaWindow.FiltrarPorCondicion).
    /// </summary>
    public class CondicionListItem
    {
        public string Etiqueta { get; set; } = "";
        public MechanizedConditions? Condicion { get; set; }
    }

    /// <summary>
    /// Sustituye a CamConfigurarGeometria.cs (WinForms): visor de la geometría (formas) de una
    /// operación, con selector Interior/Exterior y filtro por condición. En la app WinForms
    /// original, el botón "Guardar" (y "Añadir primitiva"/"Exportar condiciones") estaban ocultos
    /// (Visible = false) en el Designer, así que en producción este diálogo era, de hecho, de solo
    /// lectura/consulta — nunca llegaba a devolver DialogResult.OK. Se replica ese comportamiento
    /// tal cual: no hay acción de guardado, solo consulta con los mismos dos filtros (Interior/
    /// Exterior y Condición) que sí eran interactivos en el original.
    /// </summary>
    public partial class CamConfigurarGeometriaWindow : Window
    {
        private readonly List<OperationsShapes> _allShapes;
        private readonly List<OperationsShapes> _allExteriorShapes;
        private readonly List<MechanizedConditions> _allConditionsList;

        private readonly ObservableCollection<OperationsShapes> _shapesVisibles = new();
        private readonly ObservableCollection<CondicionListItem> _condicionesVisibles = new();

        /// <summary>Igual que el original: nunca se rellena en la práctica, porque el botón
        /// Guardar estaba oculto en producción y aquí no se ha añadido ninguno nuevo (se ha
        /// migrado tal cual el comportamiento de solo consulta).</summary>
        public List<OperationsShapes>? ResultOperationsShapesList { get; private set; }

        public CamConfigurarGeometriaWindow(string operationName, List<OperationsShapes>? existingShapes, List<OperationsShapes>? existingExteriorShapes)
        {
            InitializeComponent();

            Title = operationName;
            TxtTitulo.Text = operationName;

            _allShapes = existingShapes?.ToList() ?? new List<OperationsShapes>();
            _allExteriorShapes = existingExteriorShapes?.ToList() ?? new List<OperationsShapes>();
            _allConditionsList = RotoTools.Helpers.CargarMechanizedConditionsEmbebidos();

            GridGeometria.ItemsSource = _shapesVisibles;
            ListaCondiciones.ItemsSource = _condicionesVisibles;

            RbInterior.IsChecked = true;
            CargarListaActual();
        }

        private void RbInterior_Checked(object sender, RoutedEventArgs e) => CargarListaActual();

        private void RbExterior_Checked(object sender, RoutedEventArgs e) => CargarListaActual();

        /// <summary>Igual que rb_Interior_CheckedChanged/rb_Exterior_CheckedChanged del original:
        /// recarga la lista de condiciones usadas por el conjunto activo (interior o exterior) y
        /// selecciona la primera.</summary>
        private void CargarListaActual()
        {
            var listaActiva = RbInterior.IsChecked == true ? _allShapes : _allExteriorShapes;
            CargarCondiciones(listaActiva);
            if (_condicionesVisibles.Count > 0) ListaCondiciones.SelectedIndex = 0;
            else MostrarShapes(listaActiva);
        }

        /// <summary>Idéntico a CamConfigurarGeometria.CargarCondiciones: solo las condiciones
        /// realmente usadas (Conditions no vacío) por la lista activa, ordenadas por Name. Si no
        /// hay ninguna, se muestra el marcador "Sin Condiciones" (con Condicion = null), igual que
        /// el original.</summary>
        private void CargarCondiciones(List<OperationsShapes> existingShapes)
        {
            var usedConditionIds = existingShapes
                .Where(s => !string.IsNullOrEmpty(s.Conditions))
                .Select(s => s.Conditions)
                .Distinct()
                .ToHashSet();

            var condicionesList = _allConditionsList
                .Where(c => usedConditionIds.Contains(c.RowId))
                .OrderBy(c => c.Name)
                .ToList();

            _condicionesVisibles.Clear();

            if (condicionesList.Count == 0)
            {
                _condicionesVisibles.Add(new CondicionListItem { Etiqueta = "Sin Condiciones", Condicion = null });
            }
            else
            {
                foreach (var c in condicionesList)
                    _condicionesVisibles.Add(new CondicionListItem { Etiqueta = c.Name, Condicion = c });
            }
        }

        /// <summary>Idéntico a listBox_Condiciones_SelectedIndexChanged + FiltrarPorCondicion: si
        /// el elemento seleccionado no tiene condición asociada ("Sin Condiciones"), se filtra por
        /// Conditions == "" (particularidad ya presente en el original: no es "mostrar todo", ver
        /// comentario en la clase); si tiene condición, se filtra por su RowId.</summary>
        private void ListaCondiciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaCondiciones.SelectedItem is not CondicionListItem item) return;

            var listaActiva = RbInterior.IsChecked == true ? _allShapes : _allExteriorShapes;
            string conditionId = item.Condicion?.RowId ?? "";

            var filtradas = listaActiva.Where(o => (o.Conditions ?? "") == conditionId).ToList();
            MostrarShapes(filtradas);
        }

        private void MostrarShapes(List<OperationsShapes> shapes)
        {
            _shapesVisibles.Clear();
            foreach (var s in shapes) _shapesVisibles.Add(s);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();
    }
}
