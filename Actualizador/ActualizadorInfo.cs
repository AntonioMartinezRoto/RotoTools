using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RotoTools.Actualizador
{
    public partial class ActualizadorInfo : Form
    {
        #region Private properties

        #endregion

        #region Constructors
        public ActualizadorInfo()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void ActualizadorInfo_Load(object sender, EventArgs e)
        {
            CargarTextos();
            InitializeRotoInfo();
        }
        private void btn_RefreshPVC_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Helpers.SetNombreXMLRoto(Enums.enumHardwareType.PVC, openFileDialog.SafeFileName);
                Helpers.SetFechaActualizacionRoto(Enums.enumHardwareType.PVC, DateTime.Now);
            }

            InitializeRotoInfo();
        }
        private void btn_RefreshALU_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Helpers.SetNombreXMLRoto(Enums.enumHardwareType.Aluminio, openFileDialog.SafeFileName);
                Helpers.SetFechaActualizacionRoto(Enums.enumHardwareType.Aluminio, DateTime.Now);
            }

            InitializeRotoInfo();
        }
        private void btn_RefreshPAX_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Helpers.SetNombreXMLRoto(Enums.enumHardwareType.PAX, openFileDialog.SafeFileName);
                Helpers.SetFechaActualizacionRoto(Enums.enumHardwareType.PAX, DateTime.Now);
            }

            InitializeRotoInfo();
        }
        #endregion

        #region Private methods
        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_InfoActualizacion");

            lbl_XmlPVC.Text = LocalizationManager.GetString("L_XML") + ":";
            lbl_FechaPVC.Text = LocalizationManager.GetString("L_Fecha") + ":";
            group_InfoActualizacionPVC.Text = "PVC";

            lbl_XmlALU.Text = LocalizationManager.GetString("L_XML") + ":";
            lbl_FechaALU.Text = LocalizationManager.GetString("L_Fecha") + ":";
            group_InfoActualizacionALU.Text = "ALU";

            lbl_XmlPAX.Text = LocalizationManager.GetString("L_XML") + ":";
            lbl_FechaPAX.Text = LocalizationManager.GetString("L_Fecha") + ":";
            group_InfoActualizacionPAX.Text = "PAX";
        }
        private void InitializeRotoInfo()
        {
            lbl_PVCFile.Text = Helpers.GetNombreXMLActualizacionRoto(Enums.enumHardwareType.PVC);
            lbl_PVCData.Text = Helpers.GetFechaActualizacionRoto(Enums.enumHardwareType.PVC);

            lbl_ALUFile.Text = Helpers.GetNombreXMLActualizacionRoto(Enums.enumHardwareType.Aluminio);
            lbl_ALUData.Text = Helpers.GetFechaActualizacionRoto(Enums.enumHardwareType.Aluminio);

            lbl_PAXFile.Text = Helpers.GetNombreXMLActualizacionRoto(Enums.enumHardwareType.PAX);
            lbl_PAXData.Text = Helpers.GetFechaActualizacionRoto(Enums.enumHardwareType.PAX);
        }

        #endregion




    }
}
