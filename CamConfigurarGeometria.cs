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
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class CamConfigurarGeometria : Form
    {
        #region Private Properties
        private string OperationName { get; set; }
        private BindingList<OperationsShapes> _bindingList { get; set; }
        private List<OperationsShapes> _allShapes { get; set; }
        public List<OperationsShapes> ResultOperationsShapesList { get; private set; }
        private List<MechanizedConditions> CondicionesList { get; set; }
        #endregion

        #region Constructors
        public CamConfigurarGeometria()
        {
            InitializeComponent();
        }
        public CamConfigurarGeometria(string operationName, List<OperationsShapes> existingShapes)
        {
            InitializeComponent();

            _allShapes = existingShapes != null
                ? existingShapes
                    .Select(o => o)   // MISMA instancia, distinta colección
                    .ToList()
                : new List<OperationsShapes>();

            _bindingList = new BindingList<OperationsShapes>(_allShapes.ToList());

            this.OperationName = operationName;
            CrearGridGeometria();
            dataGridView1.DataSource = _bindingList;
            CargarCondiciones(existingShapes);
        }
        #endregion

        #region Events
        private void CamConfigurarGeometria_Load(object sender, EventArgs e)
        {
            this.Text = this.OperationName;
            LoadPrimitivas();
            FillConditionsList();
            listBox_Condiciones.SelectedIndex = 0;
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
            ResultOperationsShapesList = _allShapes;
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
        private void btn_ExportConditions_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string carpeta = dialog.SelectedPath;


                        Cursor.Current = Cursors.WaitCursor;

                        List<MechanizedConditions> mechanizedConditionsList = new List<MechanizedConditions>();

                        using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                        {
                            conn.Open();
                            string query = @"SELECT * FROM MechanizedConditions WHERE ROWID IN (SELECT Conditions FROM OperationsShapes WHERE Conditions IS NOT NULL AND OperationName LIKE 'RO\_%' ESCAPE '\')";
                            using (var cmd = new SqlCommand(query, conn))
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var mechanizedCondition = new MechanizedConditions
                                    {
                                        RowId = reader["RowId"].ToString().Trim(),
                                        Name = reader["Name"].ToString().Trim(),
                                        XmlConditions = reader["XmlConditions"].ToString().Trim(),
                                        XmlOptions = reader["XmlOptions"].ToString().Trim()
                                    };

                                    mechanizedConditionsList.Add(mechanizedCondition);
                                }
                            }
                        }

                        var options = new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        };

                        foreach (var mechanizedCondition in mechanizedConditionsList)
                        {
                            string fileName = $"{mechanizedCondition.Name}.json";
                            string path = Path.Combine(carpeta, fileName);
                            File.WriteAllText(path, JsonSerializer.Serialize(mechanizedCondition, options));
                        }

                        MessageBox.Show(mechanizedConditionsList.Count.ToString() + " " + LocalizationManager.GetString("L_Condiciones") + ": " + Environment.NewLine + carpeta, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error(3)" + Environment.NewLine + Environment.NewLine +
                                 ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void listBox_Condiciones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Condiciones.SelectedItem == null)
                return;

            var condicion = listBox_Condiciones.SelectedItem as MechanizedConditions;
            if (condicion == null)
            {
                FiltrarPorCondicion(String.Empty);
                return;
            }
            else
            {
                FiltrarPorCondicion(condicion.RowId);
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
                Width = 200
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "X",
                DataPropertyName = nameof(OperationsShapes.XDistance),
                ReadOnly = true,
                Width = 50
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Y",
                DataPropertyName = nameof(OperationsShapes.YDistance),
                ReadOnly = true,
                Width = 50
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Z",
                DataPropertyName = nameof(OperationsShapes.ZDistance),
                ReadOnly = true,
                Width = 50
            });

            //var colEliminar = new DataGridViewImageColumn
            //{
            //    Name = "Eliminar",
            //    HeaderText = "",
            //    Image = Properties.Resources.delete2, // tu icono
            //    ImageLayout = DataGridViewImageCellLayout.Zoom,
            //    Width = 30
            //};

            //dataGridView1.Columns.Add(colEliminar);
        }
        private void LoadPrimitivas()
        {
            List<string> primitivasList = Helpers.LoadPrimitivesBD();
            foreach (string operation in primitivasList)
            {
                cmb_Primitivas.Items.Add(operation.Trim());
            }
        }
        private void FillConditionsList()
        {
            listBox_Condiciones.Items.Clear();

            if (!CondicionesList.Any())
            {
                listBox_Condiciones.Items.Add("Sin Condiciones");
            }

            foreach (MechanizedConditions mechanizedCondition in CondicionesList.OrderBy(c => c.Name))
            {
                listBox_Condiciones.Items.Add(mechanizedCondition);
            }

            listBox_Condiciones.DisplayMember = "Name";
        }
        private void CargarCondiciones(List<OperationsShapes> existingShapes)
        {
            // 1. Cargar todas las condiciones disponibles
            var allConditionsList = Helpers.CargarMechanizedConditionsEmbebidos();

            // 2. Obtener los RowId usados por los shapes
            var usedConditionIds = existingShapes
                .Where(s => !String.IsNullOrEmpty(s.Conditions))
                .Select(s => s.Conditions)
                .Distinct()
                .ToHashSet();

            // 3. Filtrar solo las condiciones realmente usadas
            this.CondicionesList = allConditionsList
                .Where(c => usedConditionIds.Contains(c.RowId))
                .OrderBy(c => c.Name)
                .ToList();
        }
        private void FiltrarPorCondicion(string conditionId)
        {
            _bindingList.RaiseListChangedEvents = false;
            _bindingList.Clear();

            IEnumerable<OperationsShapes> filtradas;

            if (conditionId == null)
            {
                filtradas = _allShapes;
            }
            else
            {
                filtradas = _allShapes
                    .Where(o => o.Conditions == conditionId);
            }

            foreach (var shape in filtradas)
            {
                _bindingList.Add(shape);
            }

            _bindingList.RaiseListChangedEvents = true;
            _bindingList.ResetBindings();
        }
        #endregion
    }
}
