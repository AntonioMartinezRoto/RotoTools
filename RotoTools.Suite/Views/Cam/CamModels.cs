using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RotoEntities;

namespace RotoTools.Suite.Views.Cam
{
    /// <summary>
    /// Clases auxiliares de la página CAM, equivalentes a las clases locales de CamMenu.cs
    /// (WinForms): OperationGridRow, OperationInstalarGridITem (aquí sin la errata del nombre
    /// original) y el envoltorio de Set con checkbox para la lista de Sets (CheckedListBox
    /// original). INotifyPropertyChanged en las que llevan checkbox: es lo que permite que el
    /// CheckBox de la plantilla de fila reaccione con un único clic (ver nota en
    /// ConfiguradorOpcionesEditorWindow / RotoBrand sobre DataGridTemplateColumn).
    /// </summary>
    public class OperationGridRow
    {
        public string Operation { get; set; } = "";
        public string? FittingID { get; set; }
        public string? Article { get; set; }
        public string? Descripcion { get; set; }
        public string? X { get; set; }
        public string? Location { get; set; }
        public string? Set { get; set; }
        public string? SetDescriptionXPosition { get; set; }

        public List<OperationGridRow> OperationsList { get; set; } = new();

        public OperationGridRow() { }

        public OperationGridRow(string operation, string? fittingId, string? article, string? descripcion,
            string? x, string? location, string? set, string? setDescriptionXPosition)
        {
            Operation = operation;
            FittingID = fittingId;
            Article = article;
            Descripcion = descripcion;
            X = x;
            Location = location;
            Set = set;
            SetDescriptionXPosition = setDescriptionXPosition;
            OperationsList = new List<OperationGridRow>();
        }
    }

    public class OperationInstalarGridItem : INotifyPropertyChanged
    {
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set { if (_selected != value) { _selected = value; OnPropertyChanged(); } }
        }

        public string OperationName { get; set; } = "";
        public List<OperationsShapes> OperationShapeList { get; set; } = new();
        public List<OperationsShapes> OperationShapeExtList { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Envuelve un Set del XML con un estado "marcado" propio: sustituye a las entradas de
    /// chkList_Sets (CheckedListBox de WinForms), que en WPF se modelan como una ListBox con un
    /// CheckBox por fila en la plantilla de datos.
    /// </summary>
    public class SetListItem : INotifyPropertyChanged
    {
        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set { if (_checked != value) { _checked = value; OnPropertyChanged(); } }
        }

        public Set SetRef { get; }
        public string Code => SetRef.Code ?? "";

        public SetListItem(Set set, bool isChecked)
        {
            SetRef = set;
            _checked = isChecked;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Resultado a mostrar tras instalar operaciones 2D, similar al resumen que ya se hace en
    /// Cam3D para la instalación 3D (mensaje final único con recuento).
    /// </summary>
    public class ResultadoInstalacion2D
    {
        public int OperacionesInstaladas { get; set; }
        public int OperacionesYaExistian { get; set; }
        public List<string> Errores { get; } = new();
    }
}
