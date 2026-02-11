using RotoEntities;
using System.Data;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ActualizadorEscandallos : Form
    {
        #region Private properties
        private List<Escandallo> EscandallosList = new List<Escandallo>();
        #endregion

        #region Constructors
        public ActualizadorEscandallos()
        {
            InitializeComponent();
            CargarEscandallos();
        }


        public ActualizadorEscandallos(List<Escandallo> escandallosList)
        {
            InitializeComponent();
            this.EscandallosList = escandallosList;
        }
        #endregion

        #region Events
        private void ActualizadorEscandallos_Load(object sender, EventArgs e)
        {
            FillEscandallosList(EscandallosList);
            CargarTextos();
        }
        private void txt_Filter_TextChanged(object sender, EventArgs e)
        {
            string filtro = txt_Filter.Text.Trim().ToUpper();

            var escandallosFiltrados = EscandallosList
                .Where(o => o.Codigo.ToUpper().Contains(filtro))
                .ToList();

            FillEscandallosList (escandallosFiltrados);
        }
        private void listBox_Escandallos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Escandallos.SelectedItem is Escandallo esc)
            {
                txt_ContenidoEscandallo.Text = esc.Programa ?? string.Empty;

                // Opcional: asegurarte de que los saltos de línea se vean correctamente
                txt_ContenidoEscandallo.Text = txt_ContenidoEscandallo.Text
                    .Replace("\\r\\n", "\r\n")   // si vienen escapados
                    .Replace("\\n", "\r\n");

                txt_ContenidoEscandallo.SelectionStart = 0;
                txt_ContenidoEscandallo.ScrollToCaret();
            }
        }
        #endregion

        #region Private methods
        private void FillEscandallosList(IEnumerable<Escandallo> escandallosList)
        {
            listBox_Escandallos.Items.Clear();
            
            foreach (Escandallo escandallo in escandallosList.OrderBy(c => c.Codigo))
            {
                listBox_Escandallos.Items.Add(escandallo);
            }

            listBox_Escandallos.DisplayMember = "Codigo";
        }
        private void CargarEscandallos()
        {
            List<enumRotoTipoEscandallo> tiposSeleccionados = new();

            tiposSeleccionados.Add(enumRotoTipoEscandallo.PVC);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.Aluminio);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionGeneral);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionManillas);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.GestionBombillos);
            tiposSeleccionados.Add(enumRotoTipoEscandallo.PersonalizacionClientes);

            this.EscandallosList = Helpers.CargarEscandallosEmbebidos(tiposSeleccionados);

        }
        private void CargarTextos()
        {
            lbl_Filtrar.Text = LocalizationManager.GetString("L_Buscar");
            this.Text = LocalizationManager.GetString("L_Escandallos");
        }

        #endregion

    }
}
