
using RotoEntities;

namespace RotoTools
{
    public partial class ControlCambiosConfiguracion : Form
    {
        public bool compararOpciones { get; set; }
        public bool compararFittingGroups { get; set; }
        public ComparaFittingsProperties compararFittings { get; set; }
        public ComparaSetsProperties compararSets { get; set; }
        public bool compararColores { get; set; }
        public enum enumTipoItem
        {
            set = 1,
            fittingGroup = 2
        }

        public ControlCambiosConfiguracion()
        {
            InitializeComponent();
        }
        public ControlCambiosConfiguracion(bool compararOpciones, bool compararFittingGroups, ComparaFittingsProperties compararFittings,
            ComparaSetsProperties compararSets, bool compararColores)
        {
            InitializeComponent();
            this.compararOpciones = compararOpciones;
            this.compararFittingGroups = compararFittingGroups;
            this.compararColores = compararColores;

            this.compararFittings = compararFittings;
            this.compararSets = compararSets;

            chk_ComparaOpciones.Checked = compararOpciones;
            chk_CompararColores.Checked = compararColores;
            chk_CompararFittingGroups.Checked = compararFittingGroups;

            chk_CompararFittings.Checked = compararFittings.compararFittings;
            chk_FittingsFiltrados.Checked = compararFittings.compararFittingsFiltrados;
            chk_FittingsDescripcion.Checked = compararFittings.compararFittingsDescription;
            chk_FittingsLength.Checked = compararFittings.compararFittingsLength;
            chk_FittingsManufacturer.Checked = compararFittings.compararFittingsManufacturer;
            chk_FittingsLocation.Checked = compararFittings.compararFittingsLocation;
            chk_FittingsArticles.Checked = compararFittings.compararFittingsArticles;

            chk_CompararSets.Checked = compararSets.compararSets;
            chk_SetsFiltrados.Checked = compararSets.compararSetsFiltrados;
            chk_SetsNumero.Checked = compararSets.compararCantidadSetDescriptions;

            GestionChecksFittings();
            GestionChecksSets();
        }

        private void btn_Guardar_Click(object sender, EventArgs e)
        {
            compararOpciones = chk_ComparaOpciones.Checked;
            compararFittingGroups = chk_CompararFittingGroups.Checked;
            compararColores = chk_CompararColores.Checked;

            compararFittings.compararFittings = chk_CompararFittings.Checked;
            compararFittings.compararFittingsFiltrados = chk_FittingsFiltrados.Checked;
            compararFittings.compararFittingsLength = chk_FittingsLength.Checked;
            compararFittings.compararFittingsLocation = chk_FittingsLocation.Checked;
            compararFittings.compararFittingsDescription = chk_FittingsDescripcion.Checked;
            compararFittings.compararFittingsManufacturer = chk_FittingsManufacturer.Checked;
            compararFittings.compararFittingsArticles = chk_FittingsArticles.Checked;

            if (!compararFittings.compararFittingsFiltrados)
            {
                compararFittings.compararFittingsFiltradosList = new List<string>();
            }

            compararSets.compararSets = chk_CompararSets.Checked;
            compararSets.compararSetsFiltrados = chk_SetsFiltrados.Checked;
            compararSets.compararCantidadSetDescriptions = chk_SetsNumero.Checked;

            if (!compararSets.compararSetsFiltrados)
            {
                compararSets.compararSetsFiltradosList = new List<string>();
            }


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void chk_CompararFittings_CheckedChanged(object sender, EventArgs e)
        {
            GestionChecksFittings();
        }

        private void chk_CompararSets_CheckedChanged(object sender, EventArgs e)
        {
            GestionChecksSets();
        }

        private void GestionChecksFittings()
        {
            chk_FittingsDescripcion.Enabled = chk_CompararFittings.Checked;
            chk_FittingsLength.Enabled = chk_CompararFittings.Checked;
            chk_FittingsManufacturer.Enabled = chk_CompararFittings.Checked;
            chk_FittingsLocation.Enabled = chk_CompararFittings.Checked;
            chk_FittingsArticles.Enabled = chk_CompararFittings.Checked;
            chk_FittingsFiltrados.Enabled = chk_CompararFittings.Checked;
            btn_FittingsFiltrados.Enabled = chk_FittingsFiltrados.Checked;

            if (!chk_CompararFittings.Checked)
            {
                chk_FittingsFiltrados.Checked = false;
                chk_FittingsDescripcion.Checked = false;
                chk_FittingsLength.Checked = false;
                chk_FittingsManufacturer.Checked = false;
                chk_FittingsLocation.Checked = false;
                chk_FittingsArticles.Checked = false;
            }
        }

        private void GestionChecksSets()
        {
            chk_SetsFiltrados.Enabled = chk_CompararSets.Checked;
            btn_SetsFiltrados.Enabled = chk_SetsFiltrados.Checked;
            chk_SetsNumero.Enabled = chk_CompararSets.Checked;

            if (!chk_CompararSets.Checked)
            {
                chk_SetsFiltrados.Checked = false;
                chk_SetsNumero.Checked = false;
            }
        }

        private void btn_SetsFiltrados_Click(object sender, EventArgs e)
        {
            List<string> listaFiltroCopia = new List<string>(compararSets.compararSetsFiltradosList);
            ControlCambiosFiltroItems filtroSetsForm = new ControlCambiosFiltroItems(compararSets.compararSetsComunesList,
                                                        listaFiltroCopia,
                                                        compararSets.compararSetsSoloXml1List,
                                                        compararSets.compararSetsSoloXml2List);

            if (filtroSetsForm.ShowDialog() == DialogResult.OK)
            {
                compararSets.compararSetsFiltradosList = filtroSetsForm.itemsComunesFiltradosList;
            }
        }

        private void chk_SetsFiltrados_CheckedChanged(object sender, EventArgs e)
        {
            btn_SetsFiltrados.Enabled = chk_SetsFiltrados.Checked;
        }

        private void chk_FittingsFiltrados_CheckedChanged(object sender, EventArgs e)
        {
            btn_FittingsFiltrados.Enabled = chk_FittingsFiltrados.Checked;
        }

        private void btn_FittingsFiltrados_Click(object sender, EventArgs e)
        {
            List<string> listaFiltroCopia = new List<string>(compararFittings.compararFittingsFiltradosList);
            ControlCambiosFiltroItems filtroSetsForm = new ControlCambiosFiltroItems(compararFittings.compararFittingsComunesList,
                                                        listaFiltroCopia);

            if (filtroSetsForm.ShowDialog() == DialogResult.OK)
            {
                compararFittings.compararFittingsFiltradosList = filtroSetsForm.itemsComunesFiltradosList;
            }
        }
    }
}
