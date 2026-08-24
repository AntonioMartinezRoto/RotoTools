using System.Collections.Generic;
using System.ComponentModel;
using RotoEntities;

namespace RotoTools.Suite.Views.Exportador
{
    /// <summary>
    /// Envoltorio de un Set con su casilla de selección: sustituye a
    /// CheckedListBox.Items.Add(set, checked)/CheckedItems (campo privado chkList_Sets) tanto en
    /// ExportacionWinPerfil.cs como en ExportacionOrgadata.cs — mismo patrón que ItemSeleccionable
    /// en ControlCambiosFiltroItemsWindow.xaml.cs, pero envolviendo un RotoEntities.Set completo (no
    /// solo su nombre) porque GenerarExportacion en ambos originales necesita los objetos Set
    /// reales para inyectarlos por reflexión en chkList_Sets.Items.Add(set, true) (ver
    /// ExportacionWinPerfilWindow.xaml.cs/ExportacionOrgadataWindow.xaml.cs, método
    /// InyectarSetsSeleccionados).
    /// </summary>
    public class SetSeleccionable : INotifyPropertyChanged
    {
        public Set Set { get; }
        public string Codigo => Set.Code;

        private bool _seleccionado;
        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (_seleccionado == value) return;
                _seleccionado = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Seleccionado)));
            }
        }

        public SetSeleccionable(Set set, bool seleccionado)
        {
            Set = set;
            _seleccionado = seleccionado;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    /// <summary>
    /// Envoltorio de un Value (perfil) con su casilla de selección: sustituye al
    /// CheckedListBox.CheckedItems de ExportacionWinPerfilListaPerfiles.cs (ver
    /// ExportacionPerfilesFiltroWindow.xaml.cs, reutilizada 2 veces por
    /// ExportacionWinPerfilWindow — filtro de Perfil y de Perfil ALU).
    /// </summary>
    public class PerfilSeleccionable : INotifyPropertyChanged
    {
        public Value Perfil { get; }
        public string Valor => Perfil.Valor;

        private bool _seleccionado;
        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                if (_seleccionado == value) return;
                _seleccionado = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Seleccionado)));
            }
        }

        public PerfilSeleccionable(Value perfil, bool seleccionado)
        {
            Perfil = perfil;
            _seleccionado = seleccionado;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
