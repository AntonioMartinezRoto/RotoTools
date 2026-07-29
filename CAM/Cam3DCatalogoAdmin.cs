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

        // Texto original (sin el contador) de lbl_Faltantes, capturado en el Load antes de
        // empezar a añadirle " (N)" con la cantidad de operaciones sin definición.
        private string _tituloFaltantesBase;

        // Filas de _catalogoTrabajo añadidas en esta sesión que todavía no se han guardado en el
        // fichero: bien porque vienen de la grid de "sin definición" ("Agregar al catálogo" /
        // "Copiar todos los roles"), bien porque se han creado con el botón "Duplicar" de esta
        // misma grid. Se usa comparación por referencia (son las mismas instancias que las de
        // _catalogoTrabajo), no por valor. Se vacía al guardar correctamente.
        private readonly HashSet<Operacion3DTemplate> _filasNuevasSinGuardar = new HashSet<Operacion3DTemplate>();

        // Iconos dibujados por código (sin fichero de recurso) para los botones "Copiar de
        // catálogo", "Agregar al catálogo" y "Copiar todos los roles" de la grid de operaciones
        // sin definición, y "Duplicar" de la grid del catálogo.
        private readonly Bitmap _iconoCopiar = CrearIconoCopiar();
        private readonly Bitmap _iconoAgregarAlCatalogo = CrearIconoAgregarAlCatalogo();
        private readonly Bitmap _iconoCopiarTodosRoles = CrearIconoCopiarTodosRoles();
        private readonly Bitmap _iconoDuplicar = CrearIconoDuplicar();
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
            _tituloFaltantesBase = lbl_Faltantes.Text;

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
        /// "Copiar todos los roles": si la operación seleccionada en la grid del catálogo tiene
        /// varias filas (una por cada Rol), copia los datos de TODAS esas filas directamente a la
        /// grid del catálogo, usando el nombre de esta operación sin definición.
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
            else if (nombreColumna == "CopiarTodosLosRoles")
            {
                CopiarTodosLosRolesDesdeCatalogoSeleccionado(e.RowIndex);
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
            Operacion3DTemplate nueva = new Operacion3DTemplate
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
            };

            _catalogoTrabajo.Add(nueva);
            _filasNuevasSinGuardar.Add(nueva);

            _operacionesFaltantes.Remove(fila);

            RefrescarGridCatalogo();
            AplicarFiltroFaltantes();

            MessageBox.Show(
                $"Se ha añadido {fila.OperationName} / {fila.Role} a la grid del catálogo." + Environment.NewLine +
                "Pulse 'Guardar' para escribirla en el fichero CatalogoOperaciones3D.json.",
                "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Botón por fila "Copiar todos los roles": toma la operación actualmente seleccionada en
        /// la grid del catálogo y, si esa operación tiene varias filas (una por cada Rol distinto),
        /// añade directamente a la grid del catálogo una copia de cada una de esas filas, usando el
        /// nombre de la operación sin definición en vez del nombre de la operación de origen. Así se
        /// puede definir de golpe una operación nueva para todos los roles que ya tiene otra
        /// operación similar, sin tener que copiar/rellenar/añadir rol a rol.
        /// </summary>
        private void CopiarTodosLosRolesDesdeCatalogoSeleccionado(int indiceFilaFaltante)
        {
            dataGridViewFaltantes.EndEdit();
            _bindingFaltantes.EndEdit();

            if (dataGridViewFaltantes.Rows[indiceFilaFaltante].DataBoundItem is not OperacionFaltanteRow fila)
                return;

            if (string.IsNullOrWhiteSpace(fila.OperationName))
            {
                MessageBox.Show("Indique al menos 'Operación' en esta fila para poder añadirla al catálogo.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridViewCatalogo.CurrentRow?.DataBoundItem is not Operacion3DTemplate plantillaSeleccionada)
            {
                MessageBox.Show("Seleccione primero, en la grid del catálogo, una operación de la que quiera copiar todos sus roles.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<Operacion3DTemplate> variantesPorRol = _catalogoTrabajo
                .Where(p => string.Equals(p.OperationName, plantillaSeleccionada.OperationName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (variantesPorRol.Count <= 1)
            {
                MessageBox.Show(
                    $"La operación seleccionada ({plantillaSeleccionada.OperationName}) solo tiene un rol definido en el catálogo." + Environment.NewLine +
                    "Use el botón 'Copiar de catálogo' para copiar sus datos a esta fila.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int anadidas = 0;
            int yaExistian = 0;

            foreach (Operacion3DTemplate variante in variantesPorRol)
            {
                bool yaExiste = _catalogoTrabajo.Any(p =>
                    string.Equals(p.OperationName, fila.OperationName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.Role, variante.Role, StringComparison.OrdinalIgnoreCase) &&
                    p.Outer == variante.Outer);

                if (yaExiste)
                {
                    yaExistian++;
                    continue;
                }

                Operacion3DTemplate nueva = new Operacion3DTemplate
                {
                    OperationName = fila.OperationName.Trim(),
                    Role = variante.Role,
                    Outer = variante.Outer,
                    XFormula = variante.XFormula,
                    YFormula = variante.YFormula,
                    ZFormula = variante.ZFormula,
                    Plane = variante.Plane,
                    Depth = variante.Depth,
                    Master = variante.Master,
                    XmlParameters = variante.XmlParameters,
                    Layers = variante.Layers,
                    MirrorHorizontalForMachining = variante.MirrorHorizontalForMachining,
                    MirrorVerticalForMachining = variante.MirrorVerticalForMachining,
                    RotationForMachining = variante.RotationForMachining,
                    Face = variante.Face,
                    Disabled = variante.Disabled,
                    IsBidirectional = variante.IsBidirectional
                };

                _catalogoTrabajo.Add(nueva);
                _filasNuevasSinGuardar.Add(nueva);

                anadidas++;
            }

            string mensaje;

            if (anadidas > 0)
            {
                // Igual que CargarOperacionesFaltantes() no distingue Outer al comprobar si una
                // operación ya tiene definición, se quitan aquí las dos filas (Outer 0 y 1, si las
                // hubiera) de "sin definición" para esta operación.
                _operacionesFaltantes.RemoveAll(f =>
                    string.Equals(f.OperationName, fila.OperationName, StringComparison.OrdinalIgnoreCase));

                RefrescarGridCatalogo();
                AplicarFiltroFaltantes();

                mensaje = $"Se han añadido {anadidas} fila(s) a la grid del catálogo para {fila.OperationName}, copiando los roles de {plantillaSeleccionada.OperationName}.";
            }
            else
            {
                mensaje = $"Todas las combinaciones de rol de {plantillaSeleccionada.OperationName} ya existían para {fila.OperationName}; no se ha añadido ninguna fila nueva.";
            }

            if (yaExistian > 0)
                mensaje += Environment.NewLine + $"({yaExistian} ya existían y no se han duplicado.)";

            if (anadidas > 0)
                mensaje += Environment.NewLine + "Pulse 'Guardar' para escribirlas en el fichero CatalogoOperaciones3D.json.";

            MessageBox.Show(mensaje, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btn_Guardar_Click(object sender, EventArgs e)
        {
            dataGridViewCatalogo.EndEdit();
            _bindingCatalogo.EndEdit();

            // Las filas duplicadas con el botón "Duplicar" se dejan a propósito con el Rol vacío
            // (para no guardar sin querer una fila idéntica a la original); si se guarda sin
            // rellenarlo, SerializarCatalogoPorRole las descarta en silencio, así que se avisa aquí
            // antes de continuar.
            List<Operacion3DTemplate> filasSinRol = _catalogoTrabajo
                .Where(p => string.IsNullOrWhiteSpace(p.Role))
                .ToList();

            if (filasSinRol.Count > 0)
            {
                string nombres = string.Join(", ", filasSinRol
                    .Select(p => p.OperationName)
                    .Distinct(StringComparer.OrdinalIgnoreCase));

                DialogResult respuestaRolVacio = MessageBox.Show(
                    $"Hay {filasSinRol.Count} fila(s) en el catálogo sin Rol asignado ({nombres})." + Environment.NewLine +
                    "Esas filas NO se guardarán en el fichero hasta que se les asigne un Rol." + Environment.NewLine + Environment.NewLine +
                    "¿Continuar guardando el resto igualmente?",
                    "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (respuestaRolVacio != DialogResult.Yes)
                    return;
            }

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

                // Ya está guardado: ninguna fila del catálogo sigue "sin guardar", así que se
                // quita el resaltado de todas.
                _filasNuevasSinGuardar.Clear();

                // Refrescar ambas grids tras guardar: la del catálogo (por si se han editado datos
                // directamente en sus celdas) y la de "sin definición" (recalculada desde cero, por
                // si alguna operación ya tiene ahora definición gracias a esos cambios).
                RefrescarGridCatalogo();
                CargarOperacionesFaltantes();
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
                "CopiarTodosLosRoles" => _iconoCopiarTodosRoles,
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
        /// Texto emergente (tooltip) para los tres botones de icono de la grid de "sin
        /// definición", ya que sin texto visible su acción no es evidente a simple vista.
        /// </summary>
        private void dataGridViewFaltantes_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string nombreColumna = dataGridViewFaltantes.Columns[e.ColumnIndex].Name;

            e.ToolTipText = nombreColumna switch
            {
                "CopiarDeCatalogo" => "Copiar de catálogo: rellena esta fila con los datos de la fila seleccionada en la grid del catálogo.",
                "AgregarAlCatalogo" => "Agregar al catálogo: añade esta fila a la grid del catálogo.",
                "CopiarTodosLosRoles" => "Copiar todos los roles: si la operación seleccionada en la grid del catálogo tiene varios roles, añade una fila al catálogo por cada uno de ellos, con este nombre de operación.",
                _ => e.ToolTipText
            };
        }

        /// <summary>
        /// Dibuja, encima del botón normal de la celda, el icono de "Duplicar" (en vez del texto)
        /// en su columna de la grid del catálogo.
        /// </summary>
        private void dataGridViewCatalogo_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dataGridViewCatalogo.Columns[e.ColumnIndex].Name != "Duplicar")
                return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            int x = e.CellBounds.X + (e.CellBounds.Width - _iconoDuplicar.Width) / 2;
            int y = e.CellBounds.Y + (e.CellBounds.Height - _iconoDuplicar.Height) / 2;

            e.Graphics.DrawImage(_iconoDuplicar, x, y, _iconoDuplicar.Width, _iconoDuplicar.Height);

            e.Handled = true;
        }

        /// <summary>
        /// Texto emergente (tooltip) para el botón de icono "Duplicar" de la grid del catálogo.
        /// </summary>
        private void dataGridViewCatalogo_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dataGridViewCatalogo.Columns[e.ColumnIndex].Name == "Duplicar")
            {
                e.ToolTipText = "Duplicar: añade una copia de esta fila al catálogo con el Rol vacío, para asignarle uno distinto sin sobrescribir esta operación.";
            }
        }

        /// <summary>
        /// Resalta con un fondo distinto las filas del catálogo añadidas en esta sesión que
        /// todavía no se han guardado en el fichero (ver _filasNuevasSinGuardar), para
        /// distinguirlas claramente de las que ya estaban en el catálogo.
        /// </summary>
        private void dataGridViewCatalogo_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dataGridViewCatalogo.Rows[e.RowIndex].DataBoundItem is not Operacion3DTemplate plantilla)
                return;

            if (!_filasNuevasSinGuardar.Contains(plantilla))
                return;

            e.CellStyle.BackColor = Color.FromArgb(255, 244, 204);
            e.CellStyle.SelectionBackColor = Color.FromArgb(255, 224, 130);
        }

        private void dataGridViewCatalogo_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dataGridViewCatalogo.Columns[e.ColumnIndex].Name == "Duplicar")
            {
                DuplicarFilaCatalogo(e.RowIndex);
            }
        }

        /// <summary>
        /// Botón por fila "Duplicar": añade una copia de esta fila al catálogo (copia de trabajo
        /// en memoria) con el Rol vacío, para poder definir la misma operación para un rol
        /// distinto sin rellenar de cero fórmulas/plano/profundidad. Se deja el Rol vacío a
        /// propósito, para no guardar sin querer dos filas idénticas (Operación + Rol + Exterior)
        /// antes de que el administrador elija el nuevo rol.
        /// </summary>
        private void DuplicarFilaCatalogo(int indiceFila)
        {
            dataGridViewCatalogo.EndEdit();
            _bindingCatalogo.EndEdit();

            if (dataGridViewCatalogo.Rows[indiceFila].DataBoundItem is not Operacion3DTemplate origen)
                return;

            Operacion3DTemplate duplicado = ClonarPlantilla(origen);
            duplicado.Role = "";

            _catalogoTrabajo.Add(duplicado);
            _filasNuevasSinGuardar.Add(duplicado);

            RefrescarGridCatalogo();
            SeleccionarOperacionEnGridCatalogo(duplicado);
        }

        /// <summary>
        /// Selecciona, en la grid del catálogo, la fila que corresponde exactamente a esa
        /// instancia de Operacion3DTemplate (comparando por referencia, no por valor), si sigue
        /// visible con los filtros/búsqueda actuales.
        /// </summary>
        private void SeleccionarOperacionEnGridCatalogo(Operacion3DTemplate plantilla)
        {
            if (_bindingCatalogo.DataSource is not List<Operacion3DTemplate> lista)
                return;

            int indice = -1;

            for (int i = 0; i < lista.Count; i++)
            {
                if (ReferenceEquals(lista[i], plantilla))
                {
                    indice = i;
                    break;
                }
            }

            if (indice >= 0)
                _bindingCatalogo.Position = indice;
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

        /// <summary>
        /// Icono para "Copiar todos los roles": tres rectángulos apilados en diagonal, para
        /// distinguirlo visualmente del icono de "Copiar de catálogo" (una sola operación/rol) e
        /// indicar que copia varias filas (una por cada rol) de golpe.
        /// </summary>
        private static Bitmap CrearIconoCopiarTodosRoles()
        {
            var bmp = new Bitmap(18, 18, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using Pen pen = new Pen(Color.FromArgb(30, 90, 160), 1.2f);
                using SolidBrush fondo = new SolidBrush(Color.White);

                g.FillRectangle(fondo, 1, 9, 8, 8);
                g.DrawRectangle(pen, 1, 9, 8, 8);

                g.FillRectangle(fondo, 5, 5, 8, 8);
                g.DrawRectangle(pen, 5, 5, 8, 8);

                g.FillRectangle(fondo, 9, 1, 8, 8);
                g.DrawRectangle(pen, 9, 1, 8, 8);
            }

            return bmp;
        }

        /// <summary>
        /// Icono para "Duplicar" (grid del catálogo): el mismo motivo de dos rectángulos
        /// superpuestos que "Copiar de catálogo", con una insignia "+" para indicar que crea una
        /// fila NUEVA (duplicado), en vez de solo rellenar los campos de una fila existente.
        /// </summary>
        private static Bitmap CrearIconoDuplicar()
        {
            var bmp = new Bitmap(18, 18, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using Pen pen = new Pen(Color.FromArgb(90, 90, 90), 1.3f);
                using SolidBrush fondo = new SolidBrush(Color.White);

                g.DrawRectangle(pen, 1, 4, 9, 10);
                g.FillRectangle(fondo, 5, 1, 9, 10);
                g.DrawRectangle(pen, 5, 1, 9, 10);

                using SolidBrush fondoInsignia = new SolidBrush(Color.FromArgb(46, 125, 50));
                g.FillEllipse(fondoInsignia, 8, 8, 9, 9);

                using Pen penInsignia = new Pen(Color.White, 1.4f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                g.DrawLine(penInsignia, 12.5f, 10.5f, 12.5f, 13.5f);
                g.DrawLine(penInsignia, 11f, 12f, 14f, 12f);
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
            // catálogo", para que sea fácil comparar una con otra. Editable, para poder corregir
            // operaciones ya existentes sin tener que borrarlas y volver a añadirlas: los cambios
            // se aplican directamente sobre _catalogoTrabajo (misma instancia que la fila
            // enlazada) y se escriben en disco al pulsar "Guardar".
            dataGridViewCatalogo.AutoGenerateColumns = false;
            dataGridViewCatalogo.Columns.Clear();

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OperationName",
                HeaderText = "Operación",
                DataPropertyName = "OperationName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                //Width = 260
            });

            var columnaRoleCatalogo = new DataGridViewComboBoxColumn
            {
                Name = "Role",
                HeaderText = "Rol",
                DataPropertyName = "Role",
                Width = 150,
                FlatStyle = FlatStyle.Flat,
                DropDownWidth = 150
            };

            List<string> opcionesRolCatalogo = new List<string> { "" };
            opcionesRolCatalogo.AddRange(Cam3DHelpers.RolesMecanizado3D);
            columnaRoleCatalogo.DataSource = opcionesRolCatalogo;
            dataGridViewCatalogo.Columns.Add(columnaRoleCatalogo);

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Outer",
                HeaderText = "Exterior",
                DataPropertyName = "Outer",
                Width = 70
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "YFormula",
                HeaderText = "Y",
                DataPropertyName = "YFormula",
                Width = 280
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ZFormula",
                HeaderText = "Z",
                DataPropertyName = "ZFormula",
                Width = 280
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Plane",
                HeaderText = "Plano",
                DataPropertyName = "Plane",
                Width = 90
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Depth",
                HeaderText = "Profundidad",
                DataPropertyName = "Depth",
                Width = 100
            });

            dataGridViewCatalogo.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Duplicar",
                HeaderText = "",
                Text = "",
                UseColumnTextForButtonValue = true,
                Width = 50
            });

            _bindingCatalogo = new BindingSource();
            dataGridViewCatalogo.DataSource = _bindingCatalogo;
            dataGridViewCatalogo.CellPainting += DataGridViewPlano_CellPainting;
            dataGridViewCatalogo.CellPainting += dataGridViewCatalogo_CellPainting;
            dataGridViewCatalogo.CellFormatting += dataGridViewCatalogo_CellFormatting;
            dataGridViewCatalogo.CellContentClick += dataGridViewCatalogo_CellContentClick;
            dataGridViewCatalogo.CellToolTipTextNeeded += dataGridViewCatalogo_CellToolTipTextNeeded;
        }

        private void CargarGridCatalogo()
        {
            RefrescarGridCatalogo();
        }

        /// <summary>
        /// Vuelve a enlazar la grid del catálogo con una copia de _catalogoTrabajo (filtrada por el
        /// texto del buscador y por los combos de Exterior/Rol, si los hay) ordenada por
        /// OperationName, para que se pueda localizar fácilmente una operación concreta.
        /// Conserva la fila seleccionada (por Operación/Rol/Exterior) si sigue existiendo tras el
        /// refresco: si no se hiciera, cada vez que se añade algo al catálogo (p.ej. con "Agregar
        /// al catálogo" o "Copiar todos los roles") se perdería la selección y habría que volver a
        /// elegir la misma fila de origen a mano antes de poder repetir la acción.
        /// </summary>
        private void RefrescarGridCatalogo()
        {
            // Cierra cualquier edición pendiente antes de volver a enlazar: si no se hace, tras
            // añadir filas desde la grid de "sin definición" (p.ej. con "Copiar todos los roles")
            // la grid del catálogo puede quedar dibujada con datos de una celda que ya no
            // corresponde a la fila real hasta que se fuerza un refresco completo (como el que hace
            // 'Guardar').
            dataGridViewCatalogo.EndEdit();

            Operacion3DTemplate seleccionActual = dataGridViewCatalogo.CurrentRow?.DataBoundItem as Operacion3DTemplate;

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

            List<Operacion3DTemplate> lista = query
                .OrderBy(p => p.OperationName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Role, StringComparer.OrdinalIgnoreCase)
                .ThenBy(p => p.Outer)
                .ToList();

            _bindingCatalogo.DataSource = lista;

            if (seleccionActual != null)
            {
                int indice = lista.FindIndex(p =>
                    string.Equals(p.OperationName, seleccionActual.OperationName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.Role, seleccionActual.Role, StringComparison.OrdinalIgnoreCase) &&
                    p.Outer == seleccionActual.Outer);

                if (indice >= 0)
                    _bindingCatalogo.Position = indice;
            }

            // Fuerza un repintado completo: al añadir filas desde otra grid (botones de la grid de
            // "sin definición"), el cambio de tamaño/orden de la lista enlazada puede dejar alguna
            // celda con el contenido dibujado de la fila anterior hasta que algo repinta la grid
            // entera (como ya ocurría al pulsar 'Guardar').
            dataGridViewCatalogo.Refresh();
        }

        private void ConfigurarGridFaltantes()
        {
            dataGridViewFaltantes.AutoGenerateColumns = false;
            dataGridViewFaltantes.Columns.Clear();

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OperationName",
                HeaderText = "Operación",
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
                HeaderText = "Exterior",
                DataPropertyName = "Outer",
                Width = 70
            });

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "YFormula",
                HeaderText = "Y",
                DataPropertyName = "YFormula",
                Width = 280
            });

            dataGridViewFaltantes.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ZFormula",
                HeaderText = "Z",
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

            dataGridViewFaltantes.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "CopiarTodosLosRoles",
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
            dataGridViewFaltantes.CellToolTipTextNeeded += dataGridViewFaltantes_CellToolTipTextNeeded;
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
        /// parcial y perderlas al limpiar el filtro. También actualiza, junto al título de la
        /// sección, el número de operaciones (no de filas: una operación exterior puede tener dos
        /// filas, una por Outer) que siguen sin definición en el catálogo.
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

            int cantidadOperaciones = _operacionesFaltantes
                .Select(f => f.OperationName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            lbl_Faltantes.Text = $"{_tituloFaltantesBase} ({cantidadOperaciones})";
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
