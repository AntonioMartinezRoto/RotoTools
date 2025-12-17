using Microsoft.Data.SqlClient;
using NPOI.HSSF.Record.Chart;
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
    public partial class CamConfigurarGeometria : Form
    {
        #region Private Properties
        private string OperationName { get; set; }
        private BindingList<OperationsShapes> _bindingList;
        public List<OperationsShapes> ResultOperationsShapesList { get; private set; }
        #endregion

        #region Constructors
        public CamConfigurarGeometria()
        {
            InitializeComponent();
        }
        public CamConfigurarGeometria(string operationName, List<OperationsShapes> existingShapes)
        {
            InitializeComponent();

            this._bindingList = new BindingList<OperationsShapes>(
                existingShapes != null
                    ? existingShapes.OrderBy(x => x.OperationName).Select(o => new OperationsShapes(operationName, o.BasicShape, o.XDistance, o.YDistance, o.ZDistance)).ToList()
                    : new List<OperationsShapes>()
            );
            this.OperationName = operationName;
            CrearGridGeometria();
            dataGridView1.DataSource = _bindingList;
        }
        #endregion

        #region Events
        private void CamConfigurarGeometria_Load(object sender, EventArgs e)
        {
            this.Text = this.OperationName;
            LoadPrimitivas();
        }
        private void btn_AddPrimitiva_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(cmb_Primitivas.Text))
            {
                _bindingList.Add(new OperationsShapes(this.OperationName, cmb_Primitivas.Text));
            }
        }
        private void btn_Save_Click(object sender, EventArgs e)
        {
            ResultOperationsShapesList = _bindingList.ToList();
            DialogResult = DialogResult.OK;
            Close();
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Eliminar")
            {
                _bindingList.RemoveAt(e.RowIndex);
            }
        }
        #endregion

        #region Private methods
        private void CrearGridGeometria()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowTemplate.Height = 10;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Operacion"),
                DataPropertyName = nameof(OperationsShapes.BasicShape),
                ReadOnly = true,
                Width = 230
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "X",
                DataPropertyName = nameof(OperationsShapes.XDistance),
                Width = 50
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Y",
                DataPropertyName = nameof(OperationsShapes.YDistance),
                Width = 50
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Z",
                DataPropertyName = nameof(OperationsShapes.ZDistance),
                Width = 50
            });

            var colEliminar = new DataGridViewImageColumn
            {
                Name = "Eliminar",
                HeaderText = "",
                Image = Properties.Resources.delete2, // tu icono
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Width = 30
            };

            dataGridView1.Columns.Add(colEliminar);
        }
        private void LoadPrimitivas()
        {
            List<string> primitivasList = Helpers.LoadPrimitivesBD();
            foreach (string operation in primitivasList)
            {
                cmb_Primitivas.Items.Add(operation.Trim());
            }
        }
        #endregion

        #region

        #endregion






    }
}
