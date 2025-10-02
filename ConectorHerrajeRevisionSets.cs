using RotoEntities;
using System.Data;

namespace RotoTools
{
    public partial class ConectorHerrajeRevisionSets : Form
    {
        public XmlData xmlData = new XmlData();
        public Connector connector = new Connector();
        List<string> codesInConector = new List<string>();

        public ConectorHerrajeRevisionSets()
        {
            InitializeComponent();
        }

        public ConectorHerrajeRevisionSets(XmlData xmlData, Connector connectorHerraje)
        {
            InitializeComponent();
            this.xmlData = xmlData;
            this.connector = connectorHerraje;

            // Obtener los códigos Fitting_Code del XML:
            var fittingCodesFromXml = connector.Nodes
                .Where(n => !string.IsNullOrWhiteSpace(n.FittingCode))
                .Select(n => n.FittingCode)
                .Distinct()
                .ToList();
            var fittingCodesFromXmlNulos = connector.Nodes
                .Where(n => string.IsNullOrWhiteSpace(n.FittingCode))
                .Select(n => n.Script)
                .Distinct()
                .ToList();
            codesInConector.Clear();
            codesInConector = fittingCodesFromXml;

            //CargarSetCodesNoUsados();
            CargarSetsEnListViews(xmlData.SetList, fittingCodesFromXml);
        }

        private void CargarSetsEnListViews(List<Set> sets, List<string> fittingCodesFromXml)
        {
            list_SetsUsadosEnConectorH.Items.Clear();
            list_SetsNoUsadosEnConector.Items.Clear();
            list_CodigosNoXml.Items.Clear();

            list_SetsUsadosEnConectorH.View = View.Details;
            list_SetsUsadosEnConectorH.Columns.Add("Set", 515);

            list_SetsNoUsadosEnConector.View = View.Details;
            list_SetsNoUsadosEnConector.Columns.Add("Set", 515);

            list_CodigosNoXml.View = View.Details;
            list_CodigosNoXml.Columns.Add("Set", 515);

            int totalNoIncluidas = 0;
            int totalIncluidas = 0;
            int totalCodigoNoXml = 0;

            foreach (var set in sets.OrderBy(s => s.Code))
            {
                if (string.IsNullOrWhiteSpace(set.Code)) continue;

                var item = new ListViewItem(set.Code);

                if (fittingCodesFromXml.Contains(set.Code))
                {
                    list_SetsUsadosEnConectorH.Items.Add(item); // Está en el XML
                    totalIncluidas++;
                }
                else
                {
                    list_SetsNoUsadosEnConector.Items.Add(item); // No está en el XML
                    totalNoIncluidas++;
                }
            }

            foreach (var codigoHerraje in fittingCodesFromXml)
            {
                if (string.IsNullOrWhiteSpace(codigoHerraje)) continue;
                var item = new ListViewItem(codigoHerraje);

                if (!xmlData.SetList.Any(s => s.Code.Trim().ToLower() == codigoHerraje.Trim().ToLower()))
                {
                    list_CodigosNoXml.Items.Add(item);
                    totalCodigoNoXml++;
                }

            }

            lbl_TotalSetsNoIncluidosConector.Text = "Hay " + totalNoIncluidas.ToString() + " de " + xmlData.SetList.Count().ToString() + " Sets NO incluidos en el conector";
            lbl_TotalSetsIncluidos.Text = "Hay " + totalIncluidas.ToString() + " de " + xmlData.SetList.Count().ToString() + " Sets que están incluidos en el conector";
            lbl_TotalCodigosNoXml.Text = "Hay " + totalCodigoNoXml.ToString() + " Códigos de Herraje que no están en el XML";
        }

        private void list_SetsUsadosEnConectorH_ItemActivate(object sender, EventArgs e)
        {
            CopiarAlPortapapeles(sender, e);
        }

        private void list_SetsNoUsadosEnConector_ItemActivate(object sender, EventArgs e)
        {
            CopiarAlPortapapeles(sender, e);
        }

        private void CopiarAlPortapapeles(object sender, EventArgs e)
        {
            var listView = sender as System.Windows.Forms.ListView;
            if (listView?.SelectedItems.Count > 0)
            {
                string code = listView.SelectedItems[0].Text;
                Clipboard.SetText(code);
            }
        }

        private void txt_FiltroIncluidos_TextChanged(object sender, EventArgs e)
        {
            list_SetsUsadosEnConectorH.Items.Clear();

            list_SetsUsadosEnConectorH.View = View.Details;

            if (string.IsNullOrEmpty(txt_FiltroIncluidos.Text))
            {
                foreach (var set in xmlData.SetList.OrderBy(s => s.Code))
                {
                    if (string.IsNullOrWhiteSpace(set.Code)) continue;

                    var item = new ListViewItem(set.Code);

                    if (codesInConector.Contains(set.Code))
                    {
                        list_SetsUsadosEnConectorH.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (Set set in xmlData.SetList.Where(sl => sl.Code.ToLower().Contains(txt_FiltroIncluidos.Text.ToLower())).OrderBy(s => s.Code))
                {
                    if (string.IsNullOrWhiteSpace(set.Code)) continue;

                    var item = new ListViewItem(set.Code);

                    if (codesInConector.Contains(set.Code))
                    {
                        list_SetsUsadosEnConectorH.Items.Add(item);
                    }
                }
            }
        }

        private void txt_FiltroNoIncluidos_TextChanged(object sender, EventArgs e)
        {
            list_SetsNoUsadosEnConector.Items.Clear();
            list_SetsNoUsadosEnConector.View = View.Details;

            if (string.IsNullOrEmpty(txt_FiltroIncluidos.Text))
            {
                foreach (var set in xmlData.SetList.OrderBy(s => s.Code))
                {
                    if (string.IsNullOrWhiteSpace(set.Code)) continue;

                    var item = new ListViewItem(set.Code);

                    if (codesInConector.Contains(set.Code))
                    {
                        list_SetsNoUsadosEnConector.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (Set set in xmlData.SetList.Where(sl => sl.Code.ToLower().Contains(txt_FiltroNoIncluidos.Text.ToLower())).OrderBy(s => s.Code))
                {
                    if (string.IsNullOrWhiteSpace(set.Code)) continue;

                    var item = new ListViewItem(set.Code);

                    if (codesInConector.Contains(set.Code))
                    {
                        list_SetsNoUsadosEnConector.Items.Add(item);
                    }
                }
            }
        }

        private void txt_FiltroCodigoNoXml_TextChanged(object sender, EventArgs e)
        {
            //list_CodigosNoXml.Items.Clear();
            //list_CodigosNoXml.View = View.Details;

            //if (string.IsNullOrEmpty(txt_FiltroCodigoNoXml.Text))
            //{
            //    foreach (var codigoHerraje in codesInConector.OrderBy(s => s))
            //    {
            //        if (string.IsNullOrWhiteSpace(codigoHerraje)) continue;
            //        var item = new ListViewItem(codigoHerraje);

            //        if (!xmlData.SetList.Any(s => s.Code.Trim().ToLower() == codigoHerraje.Trim().ToLower()))
            //        {
            //            list_CodigosNoXml.Items.Add(item);
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (var codigoHerraje in codesInConector.Where(c => c.Trim().ToLower().Contains(txt_FiltroCodigoNoXml.Text.Trim().ToLower())).OrderBy(s => s))
            //    {
            //        if (string.IsNullOrWhiteSpace(codigoHerraje)) continue;
            //        var item = new ListViewItem(codigoHerraje);

            //        if (!xmlData.SetList.Any(s => s.Code.Trim().ToLower() codigoHerraje.Trim().ToLower()))
            //        {
            //            list_CodigosNoXml.Items.Add(item);
            //        }
            //    }
            //}
        }
    }
}
