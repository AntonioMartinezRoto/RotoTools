using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RotoTools
{
    public partial class ExportacionOpciones : Form
    {
        #region Private properties



        #endregion

        #region Public properties
        public bool ShowSetDescriptionId { get; set; }
        public bool ShowSetDescriptionPosition { get; set; }
        public bool ShowFittingId { get; set; }
        public bool ShowFittingLength { get; set; }
        public bool FormatoTabla { get; set; }
        public bool ShowSetId { get; set; }

        #endregion

        #region Constructors
        public ExportacionOpciones()
        {
            InitializeComponent();
        }
        public ExportacionOpciones(bool showSetDescriptionId, bool showSetDescriptionPosition, bool showFittingId, bool showFittingLength, bool formatoTabla, bool showSetId)
        {
            InitializeComponent();
            this.ShowSetDescriptionId = showSetDescriptionId;
            this.ShowSetDescriptionPosition = showSetDescriptionPosition;
            this.ShowFittingId = showFittingId;
            this.ShowFittingLength = showFittingLength;
            this.FormatoTabla = formatoTabla;
            this.ShowSetId = showSetId;
        }

        #endregion

        #region Events
        private void ExportacionOpciones_Load(object sender, EventArgs e)
        {
            SetChecks();
        }
        private void btn_SaveOptions_Click(object sender, EventArgs e)
        {
            SetValues();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        #endregion

        #region Private methods

        private void SetChecks()
        {
            chk_SetId.Checked = ShowSetId;
            chk_FittingId.Checked = ShowFittingId;
            chk_FittingLength.Checked = ShowFittingLength;
            chk_SDId.Checked = ShowSetDescriptionId;
            chk_Position.Checked = ShowSetDescriptionPosition;
            chk_FormatoTabla.Checked = FormatoTabla;
        }

        private void SetValues()
        {
            FormatoTabla = chk_FormatoTabla.Checked;
            ShowSetDescriptionId = chk_SDId.Checked;
            ShowSetDescriptionPosition = chk_Position.Checked;
            ShowFittingId = chk_FittingId.Checked;
            ShowFittingLength = chk_FittingLength.Checked;
            ShowSetId = chk_SetId.Checked;
        }
        #endregion
    }
}
