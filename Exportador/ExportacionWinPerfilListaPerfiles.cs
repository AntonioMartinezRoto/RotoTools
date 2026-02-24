using DocumentFormat.OpenXml.InkML;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RotoTools.Exportador
{
    public partial class ExportacionWinPerfilListaPerfiles : Form
    {
        #region Private properties
        private List<Value> _perfilesList = new List<Value>();

        #endregion

        #region Public properties

        public List<Value> PerfilesListSelected = new List<Value>();

        #endregion

        #region Constructors
        public ExportacionWinPerfilListaPerfiles()
        {
            InitializeComponent();
        }
        public ExportacionWinPerfilListaPerfiles(List<Value> perfilesList, List<Value> perfilesListSelected)
        {
            InitializeComponent();
            _perfilesList = perfilesList;
            PerfilesListSelected = perfilesListSelected;
        }

        #endregion

        #region Events
        private void ExportacionWinPerfilListaPerfiles_Load(object sender, EventArgs e)
        {
            chkList_Perfiles.Items.Clear();
            LoadItems("");
            CargarTextos();
        }
        private void btn_Save_Click(object sender, EventArgs e)
        {
            FillPerfilesFiltradosList();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        #endregion

        #region Private methods
        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_Filtro");
            btn_Save.Text = LocalizationManager.GetString("L_Guardar");
            lbl_Buscar.Text = LocalizationManager.GetString("L_Buscar");
        }
        private void LoadItems(string filter)
        {
            chkList_Perfiles.Items.Clear();

            IEnumerable<Value> query = _perfilesList;

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(p =>
                    p.Valor != null &&
                    p.Valor.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            foreach (var perfil in query.OrderBy(p => p.Valor))
            {
                int index = chkList_Perfiles.Items.Add(perfil);
                chkList_Perfiles.SetItemChecked(index, PerfilesListSelected.Contains(perfil));
            }
        }
        private void FillPerfilesFiltradosList()
        {
            PerfilesListSelected.Clear();
            foreach (Value perfil in chkList_Perfiles.CheckedItems)
            {
                PerfilesListSelected.Add(perfil);
            }
        }

        #endregion



        private void chkList_Perfiles_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var perfil = (Value)chkList_Perfiles.Items[e.Index];

            if (e.NewValue == CheckState.Checked)
            {
                if (!PerfilesListSelected.Contains(perfil))
                    PerfilesListSelected.Add(perfil);
            }
            else
            {
                PerfilesListSelected.Remove(perfil);
            }
        }

        private void txt_filter_TextChanged(object sender, EventArgs e)
        {
            chkList_Perfiles.Items.Clear();
            LoadItems(txt_filter.Text);
        }
    }
}
