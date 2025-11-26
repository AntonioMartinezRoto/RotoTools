using System.Data;

namespace RotoTools
{
    public partial class ControlCambiosFiltroItems : Form
    {
        public List<String> allItemsComunesList { get; set; } = new List<String>();
        public List<String> itemsComunesFiltradosList { get; set; } = new List<String>();
        public List<String> itemsComunesFiltradosForCheckList { get; set; } = new List<String>();
        public int itemComunesCount { get; set; } = 0;
        public int itemSoloEnXml1Count { get; set; } = 0;
        public int itemSoloEnXml2Count { get; set; } = 0;
        private bool estoyFiltrando { get; set; } = false;
        private int itemTypeLoaded { get; set; } = 1;
        public enum enumTipoItem
        {
            set = 1,
            fittingGroup = 2
        }

        public ControlCambiosFiltroItems()
        {
            InitializeComponent();
        }
        public ControlCambiosFiltroItems(List<String> itemsComunesList, List<String> itemsFiltradosList, List<String> itemsSoloEnXml1List, List<String> itemsSoloEnXml2List)
        {
            InitializeComponent();
            allItemsComunesList = itemsComunesList;
            itemsComunesFiltradosList = itemsFiltradosList;
            itemsComunesFiltradosForCheckList = itemsFiltradosList;
            itemComunesCount = itemsComunesList.Count;
            itemSoloEnXml1Count = itemsSoloEnXml1List.Count;
            itemSoloEnXml2Count = itemsSoloEnXml2List.Count;

            itemTypeLoaded = (int)enumTipoItem.set;
        }

        public ControlCambiosFiltroItems(List<String> itemsComunesList, List<String> itemsFiltradosList)
        {
            InitializeComponent();
            allItemsComunesList = itemsComunesList;
            itemsComunesFiltradosList = itemsFiltradosList;
            itemsComunesFiltradosForCheckList = itemsFiltradosList;
            itemComunesCount = itemsComunesList.Count;

            itemTypeLoaded = (int)enumTipoItem.fittingGroup;
        }

        private void FiltroSets_Load(object sender, EventArgs e)
        {
            chkList_Sets.Items.Clear();
            LoadItems("");
            InitializeCounters();
            CargarTextos();
        }

        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_Filtro");
            btn_GuardarFiltro.Text = LocalizationManager.GetString("L_Guardar");

            chk_SelectAll.Text = LocalizationManager.GetString("L_SeleccionarTodos");
            chk_SoloFiltrados.Text = LocalizationManager.GetString("L_SoloSeleccionados");
            lbl_Buscar.Text = LocalizationManager.GetString("L_Buscar");

        }

        private void LoadItems(String filter, bool soloSeleccionadosChecked = false)
        {
            if (allItemsComunesList.Any())
            {
                if (string.IsNullOrEmpty(filter))
                {
                    if (soloSeleccionadosChecked)
                    {
                        foreach (string set in itemsComunesFiltradosList.Where(sl => sl.ToLower().Contains(filter.ToLower())).OrderBy(s => s))
                        {
                            chkList_Sets.Items.Add(set, itemsComunesFiltradosList.Contains(set));
                        }

                    }
                    else
                    {
                        foreach (string set in allItemsComunesList.OrderBy(s => s))
                        {
                            chkList_Sets.Items.Add(set, itemsComunesFiltradosList.Contains(set));
                        }
                    }
                }
                else
                {
                    if (soloSeleccionadosChecked)
                    {
                        foreach (string set in itemsComunesFiltradosList.Where(sl => sl.ToLower().Contains(filter.ToLower())).OrderBy(s => s))
                        {
                            chkList_Sets.Items.Add(set, itemsComunesFiltradosList.Contains(set));
                        }

                    }
                    else
                    {
                        foreach (string set in allItemsComunesList.Where(sl => sl.ToLower().Contains(filter.ToLower())).OrderBy(s => s))
                        {
                            chkList_Sets.Items.Add(set, itemsComunesFiltradosList.Contains(set));
                        }
                    }
                }
            }
        }
        private void InitializeCounters()
        {
            switch (itemTypeLoaded)
            {
                case (int)enumTipoItem.set:
                    lbl_NumeroComunes.Text = LocalizationManager.GetString("L_SetsComunes")  + ": " + itemComunesCount.ToString();
                    break;
                case (int)enumTipoItem.fittingGroup:
                    lbl_NumeroComunes.Text = LocalizationManager.GetString("L_FittingsComunes")  + ": " + itemComunesCount.ToString();
                    break;

            }
        }

        private void btn_GuardarFiltro_Click(object sender, EventArgs e)
        {
            FillFiltradosList();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FillFiltradosList()
        {
            itemsComunesFiltradosList.Clear();
            foreach (String set in chkList_Sets.CheckedItems)
            {
                itemsComunesFiltradosList.Add(set);
            }
        }
        private void chk_SoloFiltrados_CheckedChanged(object sender, EventArgs e)
        {
            chkList_Sets.Items.Clear();
            if (chk_SoloFiltrados.Checked)
            {
                if (String.IsNullOrEmpty(txt_filter.Text))
                {
                    foreach (String set in itemsComunesFiltradosList.OrderBy(s => s))
                    {
                        chkList_Sets.Items.Add(set, true);
                    }
                }
                else
                {
                    foreach (String set in itemsComunesFiltradosList.Where(z => z.ToLower().Contains(txt_filter.Text.ToLower())).OrderBy(s => s))
                    {
                        chkList_Sets.Items.Add(set, true);
                    }
                }
            }
            else
            {
                if (String.IsNullOrEmpty(txt_filter.Text))
                {
                    foreach (String set in allItemsComunesList.OrderBy(s => s))
                    {
                        chkList_Sets.Items.Add(set, itemsComunesFiltradosList.Contains(set));
                    }
                }
                else
                {
                    foreach (String set in allItemsComunesList.Where(z => z.ToLower().Contains(txt_filter.Text.ToLower())).OrderBy(s => s))
                    {
                        chkList_Sets.Items.Add(set, itemsComunesFiltradosList.Contains(set));
                    }
                }

            }

        }

        private void txt_filter_TextChanged(object sender, EventArgs e)
        {
            estoyFiltrando = true;
            chk_SelectAll.Checked = false;

            chkList_Sets.Items.Clear();
            LoadItems(txt_filter.Text, chk_SoloFiltrados.Checked);
            estoyFiltrando = false;
        }

        private void chkList_Sets_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            String set = chkList_Sets.Items[e.Index].ToString();
            if (e.NewValue == CheckState.Checked)
            {
                // Se va a marcar -> añadir a la lista si no existe
                if (!itemsComunesFiltradosList.Contains(set))
                    itemsComunesFiltradosList.Add(set);
            }
            else if (e.NewValue == CheckState.Unchecked)
            {
                // Se va a desmarcar -> eliminar si existe
                if (itemsComunesFiltradosList.Contains(set))
                    itemsComunesFiltradosList.Remove(set);
            }
        }

        private void chk_SelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (estoyFiltrando) return;

            for (int i = 0; i < chkList_Sets.Items.Count; i++)
            {
                chkList_Sets.SetItemChecked(i, chk_SelectAll.Checked);

            }
        }

    }
}
