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

namespace RotoTools
{
    public partial class MostrarInfo : Form
    {
        private XmlData _xmlOrigen = new();
        public MostrarInfo()
        {
            InitializeComponent();
        }
        public MostrarInfo(XmlData xmlData)
        {
            InitializeComponent();
            _xmlOrigen = xmlData;
        }

        private void MostrarInfo_Load(object sender, EventArgs e)
        {
            if (_xmlOrigen != null) 
            {
                txt_ContenidoInfo.Clear();
                txt_ContenidoInfo.AppendText(Helpers.GetContenidoEscandalloRO_C_OCULTAR(_xmlOrigen.OptionList));
            }

        }
    }
}
