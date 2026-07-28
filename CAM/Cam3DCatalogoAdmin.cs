using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace RotoTools
{
    /// <summary>
    /// Pantalla exclusiva para el administrador del código: permite ver el catálogo embebido
    /// de operaciones 3D (CatalogoOperaciones3D.json), detectar qué combinaciones
    /// Operación/Rol de las operaciones seleccionadas para instalar en 3D no tienen todavía
    /// una plantilla en el catálogo, rellenar sus datos (fórmulas, plano, profundidad, etc.) y
    /// añadirlas al catálogo.
    /// El botón "Guardar" escribe el fichero fuente Resources\Mecanizados3D\CatalogoOperaciones3D.json
    /// en disco (agrupado por Role, igual que el fichero original), para que el administrador lo
    /// suba al repositorio y quede disponible para todos los usuarios en la siguiente compilación.
    /// No es una funcionalidad pensada para el usuario final.
    /// </summary>
    public partial class Cam3DCatalogoAdmin : Form
    {
        #region Public properties
        private List<OperationInstalarGridITem> _operacionesSeleccionadas;
        private List<Operacion3DTemplate> _catalogoTrabajo;
        private BindingSource _bindingCatalogo;
        private List<OperacionFaltanteRow> _operacionesFaltantes;
        private BindingSource _bindingFaltantes;
        private string _rutaArchivoCatalogo;

        // Iconos dibujados por código (sin fichero de recurso) para los botones "Copiar de
        // catálogo" y "Agregar al catálogo" de la grid de operaciones sin definición.
        private readonly Bitmap _iconoCopiar = CrearIconoCopiar();
        private readonly Bitmap _iconoAgregarAlCatalogo = CrearIconoAgregarAlCatalogo();
        #endregion

        #region Constructors
        public Cam3DCatalogoAdmin()
        {
            InitializeComponent();
        }

        public Cam3DCatalogoAdmin(List<OperationInstalarGridITem> operacionesSeleccionadas) : this()
        {
            _operacionesSeleccionadas = operacionesSeleccionadas ?? new List<OperationInstalarGridITem>();
        }
        #endregion

        #region Events
        private void Cam3DCatalogoAdmin_Load(object sender, EventArgs e)
        {
            // La ruta del fichero no se muestra en pantalla; se resuelve igualmente aquí para no
            // tener que pedirla al pulsar 'Guardar' salvo que no se localice automáticamente.
            _rutaArchivoCatalogo = ResolverRutaArchivoCatalogo();

            // Copia de trabajo en memoria: el catálogo real embebido no se toca hasta que se pulsa
            // 'Guardar', que es cuando se escribe en el fichero fuente.
            _catalogoTrabajo = Cam3DHelpers.CargarCatalogoOperaciones3D()
                .Select(ClonarPlantilla)
                .ToList();

            ConfigurarFiltrosCatalogo();
            ConfigurarGridCatalogo();
            CargarGridCatalogo();

            ConfigurarGridFaltantes();
            CargarOperacionesFaltantes();
        }

        /// <summary>
        /// Botón por fila "Copiar de catálogo": rellena Rol/Outer/Fórmulas/Plano/Profundidad de esta
        /// fila con los datos de la fila actualmente seleccionada en la grid del catálogo, para
        /// agilizar el rellenado cuando existe una operación similar (mismo patrón de fórmulas) de
        /// la que partir. El nombre de la operación de esta fila no se toca.
        /// </summary>
        private void dataGridViewFaltantes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            string nombreColumna = dataGridViewFaltantes.Columns[e.ColumnIndex].Name;

            if (nombreColumna == "CopiarDeCatalogo")
            {
                CopiarDatosDesdeCatalogoSeleccionado(e.RowIndex);
            }
            else if (nombreColumna == "AgregarAlCatalogo")
            {
                AgregarFilaFaltanteAlCatalogo(e.RowIndex);
            }
        }

        private void CopiarDatosDesdeCatalogoSeleccionado(int indiceFilaFaltante)
        {
            if (dataGridViewFaltantes.Rows[indiceFilaFaltante].DataBoundItem is not OperacionFaltanteRow fila)
                return;

            if (dataGridViewCatalogo.CurrentRow?.DataBoundItem is not Operacion3DTemplate plantilla)
            {
                MessageBox.Show("Seleccione primero, en la grid del catálogo, la fila de la que quiere copiar los datos.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            fila.Role = plantilla.Role;
            fila.Outer = plantilla.Outer;
            fila.YFormula = plantilla.YFormula;
            fila.ZFormula = plantilla.ZFormula;
            fila.Plane = plantilla.Plane;
            fila.Depth = plantilla.Depth;

            _bindingFaltantes.ResetBindings(false);
        }

        /// <summary>
        /// Botón por fila "Agregar al catálogo": añade esta fila concreta a la grid del catálogo
        /// (copia de trabajo en memoria) y la quita de la lista de "sin definición". El fichero
        /// fuente no se toca hasta que se pulsa 'Guardar'.
        /// </summary>
        private void AgregarFilaFaltanteAlCatalogo(int indiceFilaFaltante)
        {
            dataGridViewFaltantes.EndEdit();
            _bindingFaltantes.EndEdit();

            if (dataGridViewFaltantes.Rows[indiceFilaFaltante].DataBoundItem is not OperacionFaltanteRow fila)
                return;

            if (string.IsNullOrWhiteSpace(fila.OperationName) || string.IsNullOrWhiteSpace(fila.Role))
            {
                MessageBox.Show("Indique al menos 'Operación' y 'Rol' en esta fila para poder añadirla al catálogo.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool yaExiste = _catalogoTrabajo.Any(p =>
                string.Equals(p.OperationName, fila.OperationName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(p.Role, fila.Role, StringComparison.OrdinalIgnoreCase) &&
                p.Outer == fila.Outer);

            if (yaExiste)
            {
                MessageBox.Show($"Ya existe en el catálogo: {fila.OperationName} / {fila.Role} (Outer={fila.Outer}).",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Campos no editables en esta pantalla: se dejan con el valor por defecto que
            // usa prácticamente la totalidad del catálogo actual (XFormula = "0", Master = 0,
            // sin XmlParameters/Layers, sin espejados/rotación, Face = 0, Disabled = 0,
            // IsBidirectional = 0).
            _catalogoTrabajo.Add(new Operacion3DTemplate
            {
                OperationName = fila.OperationName.Trim(),
                Role = fila.Role.Trim(),
                Outer = fila.Outer,
                XFormula = "0",
                YFormula = fila.YFormula ?? "",
                ZFormula = fila.ZFormula ?? "",
                Plane = fila.Plane,
                Depth = fila.Depth,
                Master = 0,
                XmlParameters = "",
                Layers = null,
                MirrorHorizontalForMachining = 0,
                MirrorVerticalForMachining = 0,
                RotationForMachining = 0,
                Face = 0,
                Disabled = 0,
                IsBidirectional = 0
            });

            _operacionesFaltantes.Remove(fila);

            RefrescarGridCatalogo();
            AplicarFiltroFaltantes();

            MessageBox.Show(
                $"Se ha añadido {fila.OperationName} / {fila.Role} a la grid del catálogo." + Environment.NewLine +
                "Pulse 'Guardar' para escribirla en el fichero CatalogoOperaciones3D.json.",
                "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_Guardar_Click(object sender, EventArgs e)
        {
            dataGridViewCatalogo.EndEdit();
            _bindingCatalogo.EndEdit();

            string ruta = _rutaArchivoCatalogo;

            if (string.IsNullOrEmpty(ruta))
            {
                using SaveFileDialog dialogo = new SaveFileDialog
                {
                    Title = "Guardar CatalogoOperaciones3D.json",
                    Filter = "Fichero JSON (*.json)|*.json",
                    FileName = "CatalogoOperaciones3D.json"
                };

                if (dialogo.ShowDialog(this) != DialogResult.OK)
                    return;

                ruta = dialogo.FileName;
            }

            try
            {
                string json = SerializarCatalogoPorRole(_catalogoTrabajo);

                File.WriteAllText(ruta, json, new UTF8Encoding(false));

                _rutaArchivoCatalogo = ruta;

                // La sesión en curso ya usa el catálogo actualizado (sin necesidad de reiniciar),
                // aunque el recurso embebido en el ensamblado no se actualice hasta la próxima
                // compilación, una vez subido el cambio al repositorio.
                Cam3DHelpers.ActualizarCacheCatalogo(_catalogoTrabajo.Select(ClonarPlantilla).ToList());

                MessageBox.Show(
                    "Catálogo guardado correctamente en:" + Environment.NewLine + ruta + Environment.NewLine + Environment.NewLine +
                    "Recuerde subir este cambio al repositorio (git) para que quede disponible en la próxima compilación." + Environment.NewLine +
                    "Mientras tanto, esta sesión ya utiliza el catálogo actualizado.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el catálogo:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Volver_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txt_BuscarCatalogo_TextChanged(object sender, EventArgs e)
        {
            RefrescarGridCatalogo();
        }

        private void txt_BuscarFaltantes_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltroFaltantes();
        }

        private void cmb_FiltroExterior_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefrescarGridCatalogo();
        }

        private void cmb_FiltroRol_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefrescarGridCatalogo();
        }

        /// <summary>
        /// Dibuja, encima del botón normal de la celda, el icono correspondiente a "Copiar de
        /// catálogo" o "Agregar al catálogo" (en vez del texto), para las dos columnas de botones de
        /// la grid de operaciones sin definición.
        /// </summary>
        private void dataGridViewFaltantes_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string nombreColumna = dataGridViewFaltantes.Columns[e.ColumnIndex].Name;

            Bitmap icono = nombreColumna switch
            {
                "CopiarDeCatalogo" => _iconoCopiar,
                "AgregarAlCatalogo" => _iconoAgregarAlCatalogo,
                _ => null
            };

            if (icono == null)
                return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            int x = e.CellBounds.X + (e.CellBounds.Width - icono.Width) / 2;
            int y = e.CellBounds.Y + (e.CellBounds.Height - icono.Height) / 2;

            e.Graphics.DrawImage(icono, x, y, icono.Width, icono.Height);

            e.Handled = true;
        }

        /// <summary>
        /// Dibuja, en la columna "Plano" (tanto de la grid del catálogo como de la de "sin
        /// definición"), una flecha junto al número que indica hacia dónde apunta el plano:
        /// 0°→derecha, 90°→arriba, 180°→izquierda, 270°→abajo (360° equivale a 0°).
        /// </summary>
        private void DataGridViewPlano_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridView grid = (DataGridView)sender;

            if (grid.Columns[e.ColumnIndex].Name != "Plane" || e.Value == null)
                return;

            if (!int.TryParse(e.Value.ToString(), out int plano))
                return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            string flecha = ObtenerFlechaPlano(plano);
            string texto = string.IsNullOrEmpty(flecha) ? plano.ToString() : $"{plano} {flecha}";

            Rectangle area = new Rectangle(e.CellBounds.X + 2, e.CellBounds.Y, e.CellBounds.Width - 4, e.CellBounds.Height);

            TextRenderer.DrawText(e.Graphics, texto, e.CellStyle.Font, area, e.CellStyle.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            e.Handled = true;
        }

        private static string ObtenerFlechaPlano(int plano)
        {
            int normalizado = ((plano % 360) + 360) % 360;

            switch (normalizado)
            {
                case 0: return "→";
                case 90: return "↑";
                case 180: return "←";
                case 270: return "↓";
                default: return "";
            }
        }

        private static Bitmap CrearIconoCopiar()
        {
            var bmp = new Bitmap(18, 18, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using Pen pen = new Pen(Color.FromArgb(90, 90, 90), 1.3f);

                // Rectángulo trasero
                g.DrawRectangle(pen, 2, 5, 10, 11);

                // Rectángulo delantero (relleno + borde), superpuesto: icono típico de "copiar"
                using SolidBrush fondo = new SolidBrush(Color.White);
                g.FillRectangle(fondo, 6, 2, 10, 11);
                g.DrawRectangle(pen, 6, 2, 10, 11);
            }

            return bmp;
        }

        private static Bitmap CrearIconoAgregarAlCatalogo()
        {
            var bmp = new Bitmap(18, 18, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using Pen pen = new Pen(Color.FromArgb(46, 125, 50), 2.2f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };

                // Flecha hacia arriba (mismo motivo que el botón "↑ Agregar al catálogo" anterior)
                g.DrawLine(pen, 9, 16, 9, 3);
                g.DrawLine(pen, 4, 8, 9, 3);
                g.DrawLine(pen, 14, 8, 9, 3);
            }

            return bmp;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Rellena los combos de filtro por Exterior (Interior/Exterior/Todas) y por Rol (uno de
        /// los 9 roles de mecanizado 3D, o Todas) que aparecen encima de la grid del catálogo, con
        /// "Todas" seleccionado por defecto en ambos.
        /// </summary>
        private void ConfigurarFiltrosCatalogo()
        {
            cmb_FiltroExterior.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_FiltroExterior.Items.Clear();
            cmb_FiltroExterior.Items.AddRange(new object[] { "Todas", "Interior", "Exterior" });
            cmb_FiltroExterior.SelectedIndex = 0;
            cmb_FiltroExterior.SelectedIndexChanged += cmb_FiltroExterior_SelectedIndexChanged;

            cmb_FiltroRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb_FiltroRol.Items.Clear();
            cmb_FiltroRol.Items.Add("Todas");
            cmb_FiltroRol.Items.AddRange(Cam3DHelpers.RolesMecanizado3D);
            cmb_FiltroRol.SelectedIndex = 0;
            cmb_FiltroRol.SelectedIndexChanged += cmb_FiltroRol_SelectedIndexChanged;
        }

        private void ConfigurarGridCatalogo()
        {
            // Mismas columnas (y mismo orden) que la grid de "operaciones sin definición en el
            // catálogo", para que sea fácil comparar una con otra. Aquí siempre en solo lectura.
            dataGridViewCatalogo.AutoGenerateColumns = false;
            dataGridViewCatalogo.Columns.Clear();

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OperationName",
                HeaderText = "Operación",
                DataPropertyName = "OperationName",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //Width = 260
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Role",
                HeaderText = "Rol",
                DataPropertyName = "Role",
                ReadOnly = true,
                Width = 150
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Outer",
                HeaderText = "Exterior",
                DataPropertyName = "Outer",
                ReadOnly = true,
                Width = 70
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "YFormula",
                HeaderText = "Y",
                DataPropertyName = "YFormula",
                ReadOnly = true,
                Width = 280
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ZFormula",
                HeaderText = "Z",
                DataPropertyName = "ZFormula",
                ReadOnly = true,
                Width = 280
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Plane",
                HeaderText = "Plano",
                DataPropertyName = "Plane",
                ReadOnly = true,
                Width = 90
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Depth",
                HeaderText = "Profundidad",
                DataPropertyName = "Depth",
                ReadOnly = true,
                Width = 100
            });

            _bindingCatalogo = new BindingSource();
            dataGridViewCatalogo.DataSource = _bindingCatalogo;
            dataGridViewCatalogo.CellPainting += DataGridViewPlano_CellPainting;
        }

        private void CargarGridCatalogo()
        {
            RefrescarGridCatalogo();
        }

        /// <summary>
        /// Vuelve a enlazar la grid del catálogo con una copia de _catalogoTrabajo (filtrada por el
        /// texto del buscador y por los combos de Exterior/Rol, si los hay) ordenada por
        /// OperationName, para que se pueda localizar fácilmente una operación concreta.
        /// </summary>
        private void RefrescarGridCatalogo()
        {
            string texto = txt_BuscarCatalogo.Text.Trim();

            IEnumerable<Operacion3DTemplate> query = _catalogoTrabajo;

            if (!string.IsNullOrWhiteSpace(texto))
            {
                query = query.Where(p => !string.IsNullOrWhiteSpace(p.OperationName) &&
                    p.OperationName.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            string filtroExterior = cmb_FiltroExterior.SelectedItem as string;

            if (string.Equals(filtroExterior, "Interior", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Outer == 0);
            }
            else if (string.Equals(filtroExterior, "Exterior", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => p.Outer == 1);
            }

            string filtroRol = cmb_FiltroRol.SelectedItem as string;

            if (!string.IsNullOrWhiteSpace(filtroRol) && !string.Equals(filtroRol, "Todas", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => string.Equals(p.Role, filtroRol, StringComparison.OrdinalIgnoreCase));
            }

            _bindingCatalogo.DataSource = query
                .OrderBy(p => p.OperationName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Role, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Outer)
                .ToList();
        }

        private void ConfigurarGridFaltantes()
        {
            dataGridViewFaltantes.AutoGenerateColumns = false;
            dataGridViewFaltantes.Columns.Clear();

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OperationName",
                HeaderText = "Operación (RO_...)",
                DataPropertyName = "OperationName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //Width = 260
            });

            var columnaRole = new DataGridViewComboBoxColumn
            {
                Name = "Role",
                HeaderText = "Rol",
                DataPropertyName = "Role",
                Width = 150,
                FlatStyle = FlatStyle.Flat,
                DropDownWidth = 150
            };

            List<string> opcionesRol = new List<string> { "" };
            opcionesRol.AddRange(Cam3DHelpers.RolesMecanizado3D);
            columnaRole.DataSource = opcionesRol;
            dataGridViewFaltantes.Columns.Add(columnaRole);

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Outer",
                HeaderText = "Outer",
                DataPropertyName = "Outer",
                Width = 70
            });

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "YFormula",
                HeaderText = "Fórmula Y",
                DataPropertyName = "YFormula",
                Width = 280
            });

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ZFormula",
                HeaderText = "Fórmula Z",
                DataPropertyName = "ZFormula",
                Width = 280
            });

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Plane",
                HeaderText = "Plano",
                DataPropertyName = "Plane",
                Width = 90
            });

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Depth",
                HeaderText = "Profundidad",
                DataPropertyName = "Depth",
                Width = 100
            });

            dataGridViewFaltantes.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "CopiarDeCatalogo",
                HeaderText = "",
                Text = "",
                UseColumnTextForButtonValue = true,
                Width = 50
            });

            dataGridViewFaltantes.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "AgregarAlCatalogo",
                HeaderText = "",
                Text = "",
                UseColumnTextForButtonValue = true,
                Width = 50
            });

            _bindingFaltantes = new BindingSource();
            dataGridViewFaltantes.DataSource = _bindingFaltantes;
            dataGridViewFaltantes.CellContentClick += dataGridViewFaltantes_CellContentClick;
            dataGridViewFaltantes.CellPainting += dataGridViewFaltantes_CellPainting;
            dataGridViewFaltantes.CellPainting += DataGridViewPlano_CellPainting;
        }

        /// <summary>
        /// Calcula qué operaciones, de las seleccionadas por el usuario para instalar en 3D, no
        /// tienen todavía NINGUNA definición en el catálogo (para ningún rol). Se muestra una única
        /// fila por operación (no una por cada rol): si ya existe una definición para algún rol, no
        /// se considera "sin definición", porque puede que el resto de roles sencillamente no la
        /// necesiten. El administrador elige el rol concreto al rellenar la fila.
        ///
        /// Si la operación tiene OperationShapeExtList (es decir, aplica también por el lado
        /// Exterior/outer), se muestran DOS filas para esa operación: una con Outer = 0 (Interior)
        /// y otra con Outer = 1 (Exterior), ya que puede necesitar una definición distinta para
        /// cada lado.
        /// </summary>
        private void CargarOperacionesFaltantes()
        {
            _operacionesFaltantes = new List<OperacionFaltanteRow>();

            if (_operacionesSeleccionadas != null && _operacionesSeleccionadas.Any())
            {
                foreach (OperationInstalarGridITem op in _operacionesSeleccionadas)
                {
                    string nombreCompleto = "RO_" + op.OperationName;

                    bool existeParaAlgunRol = _catalogoTrabajo.Any(p =>
                        string.Equals(p.OperationName, nombreCompleto, StringComparison.OrdinalIgnoreCase));

                    if (existeParaAlgunRol)
                        continue;

                    bool esExterior = op.OperationShapeExtList != null && op.OperationShapeExtList.Any();

                    _operacionesFaltantes.Add(new OperacionFaltanteRow
                    {
                        OperationName = nombreCompleto,
                        Role = "",
                        Outer = 0,
                        YFormula = "",
                        ZFormula = "",
                        Plane = 0,
                        Depth = 0
                    });

                    if (esExterior)
                    {
                        _operacionesFaltantes.Add(new OperacionFaltanteRow
                        {
                            OperationName = nombreCompleto,
                            Role = "",
                            Outer = 1,
                            YFormula = "",
                            ZFormula = "",
                            Plane = 0,
                            Depth = 0
                        });
                    }
                }
            }

            _operacionesFaltantes.Sort((a, b) => string.Compare(a.OperationName, b.OperationName, StringComparison.OrdinalIgnoreCase));
            AplicarFiltroFaltantes();
        }

        /// <summary>
        /// Vuelve a enlazar la grid de "operaciones sin definición" con _operacionesFaltantes (o con
        /// una vista filtrada por el texto del buscador). Mientras haya un filtro activo se
        /// deshabilita "añadir fila nueva" en la grid, para no añadir filas nuevas a una vista
        /// parcial y perderlas al limpiar el filtro.
        /// </summary>
        private void AplicarFiltroFaltantes()
        {
            string texto = txt_BuscarFaltantes.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                dataGridViewFaltantes.AllowUserToAddRows = true;
                _bindingFaltantes.DataSource = _operacionesFaltantes;
            }
            else
            {
                dataGridViewFaltantes.AllowUserToAddRows = false;
                _bindingFaltantes.DataSource = _operacionesFaltantes
                    .Where(f => !string.IsNullOrWhiteSpace(f.OperationName) &&
                        f.OperationName.Contains(texto, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        /// <summary>
        /// Busca el fichero fuente Resources\Mecanizados3D\CatalogoOperaciones3D.json subiendo
        /// desde la carpeta de ejecución (p.ej. bin\Debug\net8.0-windows\) hacia la raíz del
        /// proyecto, tal y como se ejecuta habitualmente en Visual Studio durante el desarrollo.
        /// Si no se encuentra (por ejemplo, ejecutando una copia instalada/publicada), se pedirá
        /// la ruta manualmente al pulsar 'Guardar'.
        /// </summary>
        private static string ResolverRutaArchivoCatalogo()
        {
            string rutaRelativa = Path.Combine("Resources", "Mecanizados3D", "CatalogoOperaciones3D.json");

            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            for (int i = 0; i < 8 && dir != null; i++)
            {
                string candidato = Path.Combine(dir.FullName, rutaRelativa);
                if (File.Exists(candidato))
                    return candidato;

                dir = dir.Parent;
            }

            return null;
        }

        /// <summary>
        /// Serializa el catálogo agrupado por Role (mismo formato "plegable por regiones" que el
        /// fichero original), ordenando cada grupo por OperationName y Outer para facilitar su
        /// localización manual.
        /// </summary>
        private string SerializarCatalogoPorRole(List<Operacion3DTemplate> catalogo)
        {
            var agrupado = new Dictionary<string, List<Operacion3DTemplate>>();

            foreach (string role in Cam3DHelpers.RolesMecanizado3D)
            {
                agrupado[role] = catalogo
                    .Where(p => string.Equals(p.Role, role, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.OperationName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(p => p.Outer)
                    .ToList();
            }

            // Por si apareciera algún Role no contemplado en RolesMecanizado3D, para no perder
            // ninguna fila al guardar (no debería ocurrir en el uso normal de esta pantalla).
            HashSet<string> rolesConocidos = new HashSet<string>(Cam3DHelpers.RolesMecanizado3D, StringComparer.OrdinalIgnoreCase);
            List<string> rolesExtra = catalogo
                .Select(p => p.Role)
                .Where(r => !string.IsNullOrWhiteSpace(r) && !rolesConocidos.Contains(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (string roleExtra in rolesExtra)
            {
                agrupado[roleExtra] = catalogo
                    .Where(p => string.Equals(p.Role, roleExtra, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.OperationName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(p => p.Outer)
                    .ToList();
            }

            JsonSerializerOptions opciones = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(agrupado, opciones);
        }

        private static Operacion3DTemplate ClonarPlantilla(Operacion3DTemplate original)
        {
            return new Operacion3DTemplate
            {
                OperationName = original.OperationName,
                Role = original.Role,
                Outer = original.Outer,
                XFormula = original.XFormula,
                YFormula = original.YFormula,
                ZFormula = original.ZFormula,
                Plane = original.Plane,
                Depth = original.Depth,
                Master = original.Master,
                XmlParameters = original.XmlParameters,
                Layers = original.Layers,
                MirrorHorizontalForMachining = original.MirrorHorizontalForMachining,
                MirrorVerticalForMachining = original.MirrorVerticalForMachining,
                RotationForMachining = original.RotationForMachining,
                Face = original.Face,
                Disabled = original.Disabled,
                IsBidirectional = original.IsBidirectional
            };
        }
        #endregion
    }

    /// <summary>
    /// Fila editable en la grid de "operaciones sin definición en el catálogo" de
    /// Cam3DCatalogoAdmin. Solo expone los campos que el administrador necesita rellenar a mano;
    /// el resto de campos de Operacion3DTemplate se completan con los valores por defecto que ya
    /// usa la práctica totalidad del catálogo actual (ver Cam3DCatalogoAdmin.AgregarFilaFaltanteAlCatalogo).
    /// </summary>
    public class OperacionFaltanteRow
    {
        public string OperationName { get; set; }
        public string Role { get; set; }
        public int Outer { get; set; }
        public string YFormula { get; set; }
        public string ZFormula { get; set; }
        public int Plane { get; set; }
        public int Depth { get; set; }
    }
}
