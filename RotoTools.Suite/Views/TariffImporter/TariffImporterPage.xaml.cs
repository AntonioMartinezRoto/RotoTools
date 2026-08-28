using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using RotoEntities;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.TariffImporter
{
    /// <summary>
    /// Sustituye a TariffsImporterMenu.cs/.Designer.cs (WinForms): mismo comportamiento y misma
    /// lógica de negocio, reutilizada tal cual vía ProjectReference. Las consultas SQL y la lectura
    /// del Excel (NPOI) que en el original vivían directamente en el code-behind del formulario se
    /// han portado aquí letra por letra, en vez de intentar moverlas a RotoTools.csproj, que no se
    /// debe tocar bajo ningún concepto.
    /// </summary>
    public partial class TariffImporterPage : UserControl
    {
        #region Estado

        private List<Tariff> _tariffList = new();
        private bool _fileSelected;
        private string _filePath = string.Empty;

        #endregion

        public TariffImporterPage()
        {
            InitializeComponent();

            CargarTextos();
            CargarDatos();
        }

        #region Localización / carga inicial

        private static string Loc(string key) => SuiteLocalization.GetString(key);

        private void CargarTextos()
        {
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_TariffImporter");
            TxtSubtitulo.Text = Loc("L_Suite_TariffImporterSubtitulo");

            TxtArchivoTitulo.Text = RotoTools.LocalizationManager.GetString("L_Archivo");
            TxtBtnLoadTariff.Text = RotoTools.LocalizationManager.GetString("L_SeleccionarArchivo");
            LblFichero.Text = Loc("L_Suite_NingunArchivoSeleccionado");

            LblTarifa.Text = RotoTools.LocalizationManager.GetString("L_Tarifa");

            TxtBtnImportTariff.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
        }

        /// <summary>Igual que TariffsImporterMenu_Load, salvo lbl_Conexion: el servidor/base de
        /// datos ya se muestra siempre en la cabecera de MainWindow, así que repetirlo aquí sería
        /// información duplicada (mismo criterio ya aplicado en ActualizadorPage/ManillasFKSPage).</summary>
        private void CargarDatos()
        {
            CargarTarifas();
            SeleccionarTarifaRoto();
        }

        #endregion

        #region Events

        /// <summary>Público (no solo se llama desde el propio botón): es también el acceso directo
        /// "Seleccionar fichero de precios" de la portada Inicio (ver
        /// BtnAccesoTariffImporter_Click en DashboardPage.xaml.cs y MainWindow.IrAModulo), que
        /// navega a este módulo y ejecuta esta misma acción tal cual, sin duplicar su lógica.</summary>
        public void BtnLoadTariff_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Excel (*.xlsx)|*.xlsx" };

            if (openFileDialog.ShowDialog() == true)
            {
                LblFichero.Text = openFileDialog.FileName;
                _fileSelected = true;
                _filePath = openFileDialog.FileName;
            }
        }

        private void BtnImportTariff_Click(object sender, RoutedEventArgs e)
        {
            if (CmbTarifas.SelectedValue == null)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_TarifaObligatoria"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_fileSelected)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_FicheroPreciosObligatorio"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ImportarTarifaDesdeExcel();
        }

        private void BtnAddTariff_Click(object sender, RoutedEventArgs e)
        {
            var tariffsImporterAddTariffWindow = new TariffImporterAddTariffWindow { Owner = Window.GetWindow(this) };
            tariffsImporterAddTariffWindow.ShowDialog();

            CargarTarifas();
        }

        #endregion

        #region Private Methods

        private void CargarTarifas()
        {
            _tariffList.Clear();

            using (var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
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

            CmbTarifas.ItemsSource = null;
            CmbTarifas.ItemsSource = _tariffList;
        }

        private void ImportarTarifaDesdeExcel()
        {
            IWorkbook workbook;

            using (var fs = new System.IO.FileStream(_filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                workbook = new XSSFWorkbook(fs);
            }

            var sheet = workbook.GetSheet("Tarifa");

            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            if (sheet == null)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ExcelSinTarifa"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int lastRow = sheet.LastRowNum;

            if (lastRow < 1)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_TarifaSinDatos"), "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int totalRegistros = lastRow; // fila 0 = cabecera

            var confirm = MessageBox.Show(
                $"Se van a importar {totalRegistros} registros.\n¿Desea continuar?",
                "",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            System.Guid tariffRowId = System.Guid.Parse(CmbTarifas.SelectedValue.ToString());
            ProcesarFilasTarifa(sheet, lastRow, tariffRowId, evaluator);
        }

        private void ProcesarFilasTarifa(ISheet sheet, int lastRow, System.Guid tariffRowId, IFormulaEvaluator evaluator)
        {
            ProgressInstall.Visibility = Visibility.Visible;
            int totalFilas = lastRow;
            ProgressInstall.Value = 0;
            ProgressInstall.Maximum = totalFilas > 0 ? totalFilas : 1; // Evitar división por cero

            using var conn = new SqlConnection(RotoTools.Helpers.GetConnectionString());
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

                    ProgressInstall.Value++;
                    DoEvents(); // Fuerza el repintado si el proceso es muy rápido
                }

                tran.Commit();

                MessageBox.Show(
                    RotoTools.LocalizationManager.GetString("L_PreciosCargados"),
                    "",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ProgressInstall.Value = 0;
                ProgressInstall.Visibility = Visibility.Collapsed;
            }
            catch (System.Exception ex)
            {
                tran.Rollback();
                MessageBox.Show("Error(40):\n" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                ProgressInstall.Value = 0;
                ProgressInstall.Visibility = Visibility.Collapsed;
            }
        }

        private void InsertarOActualizarTarifa(string referencia, decimal amount, System.Guid tariffRowId, SqlConnection conn, SqlTransaction tran)
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
            cmd.Parameters.Add("@amount", SqlDbType.Float).Value = System.Convert.ToDouble(amount);
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
                        return System.Convert.ToDecimal(cell.NumericCellValue);

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
                            return System.Convert.ToDecimal(evaluated.NumberValue);

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
                    t.Name.Contains("roto", System.StringComparison.OrdinalIgnoreCase) &&
                    t.Name.Contains("neto", System.StringComparison.OrdinalIgnoreCase));

            if (tarifaRoto != null)
            {
                CmbTarifas.SelectedValue = tarifaRoto.RowId;
            }
        }

        /// <summary>Equivalente WPF de Application.DoEvents() (mismo helper que
        /// ConectorHerrajePage/ManillasFKSPage): bombea el bucle de mensajes para que la
        /// ProgressBar se repinte durante el bucle síncrono, igual que hacía
        /// progress_Install.Refresh() en la app WinForms original.</summary>
        private static void DoEvents()
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new System.Windows.Threading.DispatcherOperationCallback(f =>
                {
                    ((System.Windows.Threading.DispatcherFrame)f!).Continue = false;
                    return null;
                }), frame);
            System.Windows.Threading.Dispatcher.PushFrame(frame);
        }

        #endregion
    }
}
