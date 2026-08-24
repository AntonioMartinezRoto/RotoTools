using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RotoTools.Suite.Views.Cam
{
    /// <summary>
    /// Fila de "todos los perfiles" / origen para el árbol de materiales, equivalente a
    /// MaterialBaseTreeRow en Cam3D.cs (WinForms). Carga precomputada por una sola consulta SQL
    /// (ver Cam3DWindow.CargarMaterialesBase), igual que el original.
    /// </summary>
    public class MaterialBaseTreeRow
    {
        public Guid RowId { get; set; }
        public string ReferenciaBase { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Nivel1 { get; set; } = "";
        public string Nivel2 { get; set; } = "";
        public string Nivel3 { get; set; } = "";
        public string Nivel4 { get; set; } = "";
        public string Nivel5 { get; set; } = "";
        public string Role { get; set; } = "";
        public double AnchoInterior { get; set; }
        public double AnchoExterior { get; set; }
        public double CuerpoInterior { get; set; }
        public double CuerpoExterior { get; set; }
        public double Altura { get; set; }
        public double? DescuentoCanalHerraje { get; set; }
    }

    /// <summary>
    /// Fila de la lista/tabla de "perfiles a instalar", equivalente a PerfilAInstalarRow en
    /// Cam3D.cs. RolMecanizado, DescuentoCanalHerraje y PosicionCanalHerraje son editables desde
    /// la grid (columnas RolMecanizado/DescuentoCanalHerraje/PosicionCanalHerraje del original).
    /// </summary>
    public class PerfilAInstalarRow : INotifyPropertyChanged
    {
        public Guid ProfileId { get; set; }
        public string ReferenciaBase { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Role { get; set; } = "";
        public double AnchoInterior { get; set; }
        public double AnchoExterior { get; set; }
        public double CuerpoInterior { get; set; }
        public double CuerpoExterior { get; set; }
        public double Altura { get; set; }

        private string _rolMecanizado = "";
        public string RolMecanizado
        {
            get => _rolMecanizado;
            set { if (_rolMecanizado != value) { _rolMecanizado = value; OnPropertyChanged(); } }
        }

        private double? _descuentoCanalHerraje;
        public double? DescuentoCanalHerraje
        {
            get => _descuentoCanalHerraje;
            set { if (_descuentoCanalHerraje != value) { _descuentoCanalHerraje = value; OnPropertyChanged(); } }
        }

        private double? _posicionCanalHerraje;
        public double? PosicionCanalHerraje
        {
            get => _posicionCanalHerraje;
            set { if (_posicionCanalHerraje != value) { _posicionCanalHerraje = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Nodo del árbol de materiales, equivalente a los TreeNode de treeViewMateriales en Cam3D.cs:
    /// Tag == null → nodo de nivel/carpeta; ReferenciaBase != null → hoja/perfil (mismo criterio
    /// que el original). IsExpanded/IsSelected son bindables (vía ItemContainerStyle del
    /// TreeView) para poder expandir/seleccionar nodos desde código sin depender del
    /// ItemContainerGenerator, ya que TreeView.SelectedItem es de solo lectura en WPF.
    /// </summary>
    public class MaterialTreeNode : INotifyPropertyChanged
    {
        public string Texto { get; set; } = "";
        public string? ReferenciaBase { get; set; }
        public bool EsHoja => ReferenciaBase != null;
        public List<MaterialTreeNode> Hijos { get; } = new();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { if (_isExpanded != value) { _isExpanded = value; OnPropertyChanged(); } }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
