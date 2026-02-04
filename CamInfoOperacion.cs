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
    public partial class CamInfoOperacion : Form
    {
        #region Private properties
        private DataTable _dataTable;
        private BindingSource _bindingDetalleOperaciones;
        private string _operationName;
        private List<OperationGridRow> _operationList = new List<OperationGridRow>();

        #endregion

        #region Constructors 
        public CamInfoOperacion()
        {
            InitializeComponent();
        }
        public CamInfoOperacion(string operationName, List<OperationGridRow> operationsList)
        {
            InitializeComponent();
            _operationName = operationName;
            _operationList = operationsList;
        }

        #endregion

        #region Events
        private void CamInfoOperacion_Load(object sender, EventArgs e)
        {
            this.Text = _operationName;
            CrearGridDetalleOperaciones();
            DarEstiloCabecerasDetalleOperaciones();
            CargarOperacionesEnGrid();
        }

        #endregion

        #region Private methods
        private void CrearGridDetalleOperaciones()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.RowTemplate.Height = 10;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_FittingId"),
                DataPropertyName = nameof(OperationGridRow.FittingID),
                ReadOnly = true,
                Width = 80
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Articulo"),
                DataPropertyName = nameof(OperationGridRow.Article),
                ReadOnly = true,
                Width = 100
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Descripcion"),
                DataPropertyName = nameof(OperationGridRow.Descripcion),
                ReadOnly = true,
                Width = 400
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Posicion"),
                DataPropertyName = nameof(OperationGridRow.X),
                ReadOnly = true,
                Width = 80
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "SD " + LocalizationManager.GetString("L_Posicion"),
                DataPropertyName = nameof(OperationGridRow.SetDescriptionXPosition),
                ReadOnly = true,
                Width = 130
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Location"),
                DataPropertyName = nameof(OperationGridRow.Location),
                ReadOnly = true,
                Width = 80
            });
        }
        private void DarEstiloCabecerasDetalleOperaciones()
        {
            dataGridView1.EnableHeadersVisualStyles = false; // muy importante, evita que Windows sobrescriba el estilo
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Opcional: bordes y selección
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.GridColor = Color.LightGray;
        }
        private void CargarOperacionesEnGrid()
        {
            _bindingDetalleOperaciones = new BindingSource();
            _bindingDetalleOperaciones.DataSource = _operationList;

            dataGridView1.DataSource = _bindingDetalleOperaciones;
        }
        #endregion




    }
}
