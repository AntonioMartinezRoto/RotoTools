using Microsoft.Data.SqlClient;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NSAX.Helpers;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RotoTools
{
    public partial class TariffsImporterMenu : Form
    {
        #region Private properties

        private List<Tariff> _tariffList = new List<Tariff>();
        private bool _fileSelected = false;
        private string _filePath = string.Empty;

        #endregion

        #region Constructors
        public TariffsImporterMenu()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        private void btn_LoadTariff_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                lbl_Fichero.Text = openFileDialog.FileName;
                _fileSelected = true;
                _filePath = openFileDialog.FileName;
            }
        }
        private void TariffsImporterMenu_Load(object sender, EventArgs e)
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
            CargarTarifas();
            CargarTextos();
            SeleccionarTarifaRoto();
        }
        private void btn_ImportTariff_Click(object sender, EventArgs e)
        {
            if (cmb_Tarifas.SelectedValue == null)
            {
                MessageBox.Show(LocalizationManager.GetString("L_TarifaObligatoria"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!_fileSelected)
            {
                MessageBox.Show(LocalizationManager.GetString("L_FicheroPreciosObligatorio"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ImportarTarifaDesdeExcel();
        }
        private void btn_AddTariff_Click(object sender, EventArgs e)
        {
            TariffsImporterAddTariff tariffsImporterAddTariffForm = new TariffsImporterAddTariff();
            tariffsImporterAddTariffForm.ShowDialog();

            CargarTarifas();
        }
        #endregion

        #region Private methods
        private void CargarTextos()
        {
            this.Text = LocalizationManager.GetString("L_TariffImporter");
            lbl_Fichero.Text = LocalizationManager.GetString("L_SeleccionarArchivo");
            lbl_Tarifa.Text = LocalizationManager.GetString("L_Tarifa");
            btn_ImportTariff.Text = LocalizationManager.GetString("L_Guardar");
            groupBox1.Text = LocalizationManager.GetString("L_Archivo");
        }
        private void CargarTarifas()
        {
            _tariffList.Clear();

            using (var conn = new SqlConnection(Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand("Select RowId, Name From Tariff Where Type=0 ORDER BY Name", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        _tariffList.Add(new Tariff
                        {
                            RowId = reader["RowId"].ToString(),
                            Name = reader["Name"].ToString()
                        });
                    }
                }
            }

            cmb_Tarifas.DataSource = null;
            cmb_Tarifas.DataSource = _tariffList;
            cmb_Tarifas.DisplayMember = "Name";
            cmb_Tarifas.ValueMember = "RowId";
        }
        private void ImportarTarifaDesdeExcel()
        {
            IWorkbook workbook;

            using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(fs);
            }

            var sheet = workbook.GetSheet("Tarifa");

            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            if (sheet == null)
            {
                MessageBox.Show(LocalizationManager.GetString("L_ExcelSinTarifa"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int lastRow = sheet.LastRowNum;

            if (lastRow < 1)
            {
                MessageBox.Show(LocalizationManager.GetString("L_TarifaSinDatos"), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int totalRegistros = lastRow; // fila 0 = cabecera

            var confirm = MessageBox.Show(
                $"Se van a importar {totalRegistros} registros.\n¿Desea continuar?",
                "",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            Guid tariffRowId = Guid.Parse(cmb_Tarifas.SelectedValue.ToString());
            ProcesarFilasTarifa(sheet, lastRow, tariffRowId, evaluator);
        }
        private void ProcesarFilasTarifa(ISheet sheet, int lastRow, Guid tariffRowId, IFormulaEvaluator evaluator)
        {
            progress_Install.Visible = true;
            int totalFilas = lastRow;
            progress_Install.Value = 0;
            progress_Install.Maximum = totalFilas > 0 ? totalFilas : 1; // Evitar división por cero

            using var conn = new SqlConnection(Helpers.GetConnectionString());
            conn.Open();

            using var tran = conn.BeginTransaction();

            int procesados = 0;

            try
            {
                for (int rowIndex = 1; rowIndex <= lastRow; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null)
                        continue;

                    string referencia = ObtenerTextoCelda(row.GetCell(0));
                    decimal? precio = ObtenerDecimalCelda(row.GetCell(1), evaluator);

                    if (string.IsNullOrWhiteSpace(referencia) || precio == null)
                        continue;

                    InsertarOActualizarTarifa(
                        referencia,
                        precio.Value,
                        tariffRowId,
                        conn,
                        tran);

                    procesados++;

                    progress_Install.Value++;
                    progress_Install.Refresh(); // Fuerza el repintado si el proceso es muy rápido
                }

                tran.Commit();

                MessageBox.Show(
                    LocalizationManager.GetString("L_PreciosCargados"),
                    "",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                progress_Install.Value = 0;
                progress_Install.Visible = false;
            }
            catch (Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Error(40):\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                progress_Install.Value = 0;
                progress_Install.Visible = false;
            }
        }
        private void InsertarOActualizarTarifa(string referencia, decimal amount, Guid tariffRowId, SqlConnection conn, SqlTransaction tran)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;

            cmd.CommandText = @"
                    DECLARE @existMaterial smallint
                    DECLARE @updateInsert smallint

                    SET @existMaterial = (
                        SELECT COUNT(*) 
                        FROM Materiales 
                        WHERE Referencia = @reference
                    )

                    IF (@existMaterial = 1)
                    BEGIN
                        SET @updateInsert = (
                            SELECT COUNT(*)
                            FROM TariffsContent
                            WHERE TariffRowId = @guidTariff
                              AND Reference = @reference
                        )

                        IF (@updateInsert = 1)
                        BEGIN
                            UPDATE TariffsContent
                            SET Value = @amount
                            WHERE Reference = @reference
                              AND TariffRowId = @guidTariff
                        END
                        ELSE
                        BEGIN
                            INSERT INTO TariffsContent
                                (TariffRowId, Reference, Value, Type)
                            VALUES
                                (@guidTariff, @reference, @amount, 3)
                        END
                    END";

            cmd.Parameters.Add("@reference", SqlDbType.NChar, 25).Value = referencia.Trim();
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = Convert.ToDouble(amount);
            cmd.Parameters.Add("@guidTariff", SqlDbType.UniqueIdentifier).Value = tariffRowId;

            cmd.ExecuteNonQuery();
        }
        private string ObtenerTextoCelda(ICell cell)
        {
            if (cell == null)
                return string.Empty;

            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue.Trim(),
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Formula => cell.ToString(),
                _ => string.Empty
            };
        }
        private decimal? ObtenerDecimalCelda(ICell cell, IFormulaEvaluator evaluator)
        {
            if (cell == null)
                return null;

            try
            {
                switch (cell.CellType)
                {
                    case CellType.Numeric:
                        return Convert.ToDecimal(cell.NumericCellValue);

                    case CellType.String:
                        if (decimal.TryParse(
                                cell.StringCellValue.Replace(",", "."),
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out var valueString))
                        {
                            return valueString;
                        }
                        break;

                    case CellType.Formula:
                        var evaluated = evaluator.Evaluate(cell);
                        if (evaluated == null)
                            return null;

                        if (evaluated.CellType == CellType.Numeric)
                            return Convert.ToDecimal(evaluated.NumberValue);

                        if (evaluated.CellType == CellType.String &&
                            decimal.TryParse(
                                evaluated.StringValue.Replace(",", "."),
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture,
                                out var valueFormula))
                        {
                            return valueFormula;
                        }
                        break;
                }
            }
            catch
            {
                // opcional: log
            }

            return null;
        }
        private void SeleccionarTarifaRoto()
        {
            if (_tariffList == null || !_tariffList.Any())
                return;

            var tarifaRoto = _tariffList
                .FirstOrDefault(t =>
                    t.Name != null &&
                    t.Name.Contains("roto", StringComparison.OrdinalIgnoreCase) &&
                    t.Name.Contains("neto", StringComparison.OrdinalIgnoreCase));

            if (tarifaRoto != null)
            {
                cmb_Tarifas.SelectedValue = tarifaRoto.RowId;
            }
        }

        #endregion


    }
}
