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
    /// Pantalla exclusiva para el administrador del código: permite ver la biblioteca embebida de
    /// perfiles ya conocidos (BibliotecaPerfiles3D.json), añadir perfiles nuevos, modificar los
    /// existentes, eliminarlos o duplicarlos para crear uno parecido.
    /// El botón "Guardar" escribe el fichero fuente Resources\Mecanizados3D\BibliotecaPerfiles3D.json
    /// en disco, para que el administrador lo suba al repositorio y quede disponible para todos los
    /// usuarios en la siguiente compilación.
    /// No es una funcionalidad pensada para el usuario final. Mismo patrón (grid editable, filas
    /// resaltadas mientras no se guarden, botones de icono "Duplicar"/"Eliminar") que
    /// Cam3DCatalogoAdmin, la pantalla equivalente para el catálogo de operaciones 3D.
    /// </summary>
    public partial class Cam3DPerfilesCatalogoAdmin : Form
    {
        #region Public properties
        private List<PerfilLibreriaEntry> _bibliotecaTrabajo;
        private BindingSource _bindingBiblioteca;
        private string _rutaArchivoBiblioteca;

        // Filas de _bibliotecaTrabajo añadidas en esta sesión que todavía no se han guardado en el
        // fichero: bien porque se han creado con el botón "Nuevo perfil", bien porque se han creado
        // con el botón "Duplicar". Se usa comparación por referencia (son las mismas instancias que
        // las de _bibliotecaTrabajo), no por valor. Se vacía al guardar correctamente.
        private readonly HashSet<PerfilLibreriaEntry> _filasNuevasSinGuardar = new HashSet<PerfilLibreriaEntry>();

        // Iconos dibujados por código (sin fichero de recurso) para los botones "Duplicar" y
        // "Eliminar" de la grid, mismo estilo que los de Cam3DCatalogoAdmin.
        private readonly Bitmap _iconoDuplicar = CrearIconoDuplicar();
        private readonly Bitmap _iconoEliminar = CrearIconoEliminar();
        #endregion

        #region Constructors
        public Cam3DPerfilesCatalogoAdmin()
        {
            InitializeComponent();
        }
        #endregion

        #region Events
        private void Cam3DPerfilesCatalogoAdmin_Load(object sender, EventArgs e)
        {
            // La ruta del fichero no se muestra en pantalla; se resuelve igualmente aquí para no
            // tener que pedirla al pulsar 'Guardar' salvo que no se localice automáticamente.
            _rutaArchivoBiblioteca = ResolverRutaArchivoBiblioteca();

            // Copia de trabajo en memoria: la biblioteca real embebida no se toca hasta que se
            // pulsa 'Guardar', que es cuando se escribe en el fichero fuente.
            _bibliotecaTrabajo = Cam3DHelpers.CargarListaBibliotecaPerfiles3D()
                .Select(ClonarEntrada)
                .ToList();

            ConfigurarGridBiblioteca();
            CargarGridBiblioteca();
        }

        private void txt_BuscarBiblioteca_TextChanged(object sender, EventArgs e)
        {
            RefrescarGridBiblioteca();
        }

        /// <summary>
        /// Botón "Nuevo perfil": añade una fila en blanco a la biblioteca (copia de trabajo en
        /// memoria) y la selecciona, para rellenarla directamente en la grid.
        /// </summary>
        private void btn_NuevoPerfil_Click(object sender, EventArgs e)
        {
            PerfilLibreriaEntry nueva = new PerfilLibreriaEntry
            {
                ReferenciaBase = "",
                Role = "",
                PosicionCanalHerraje = 0
            };

            _bibliotecaTrabajo.Add(nueva);
            _filasNuevasSinGuardar.Add(nueva);

            RefrescarGridBiblioteca();
            SeleccionarEntradaEnGridBiblioteca(nueva);
        }

        /// <summary>
        /// Dibuja, encima del botón normal de la celda, el icono de "Duplicar" o "Eliminar" (en vez
        /// del texto) en sus columnas.
        /// </summary>
        private void dataGridViewBiblioteca_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string nombreColumna = dataGridViewBiblioteca.Columns[e.ColumnIndex].Name;

            Bitmap icono = nombreColumna switch
            {
                "Duplicar" => _iconoDuplicar,
                "Eliminar" => _iconoEliminar,
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
        /// Texto emergente (tooltip) para los botones de icono "Duplicar" y "Eliminar", ya que sin
        /// texto visible su acción no es evidente a simple vista.
        /// </summary>
        private void dataGridViewBiblioteca_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string nombreColumna = dataGridViewBiblioteca.Columns[e.ColumnIndex].Name;

            e.ToolTipText = nombreColumna switch
            {
                "Duplicar" => "Duplicar: añade una copia de esta fila con la Referencia base vacía, para asignarle una distinta sin sobrescribir esta.",
                "Eliminar" => "Eliminar: quita este perfil de la biblioteca (de la copia de trabajo en memoria; no se borra del fichero hasta que se pulse 'Guardar').",
                _ => e.ToolTipText
            };
        }

        /// <summary>
        /// Resalta con un fondo distinto las filas añadidas en esta sesión que todavía no se han
        /// guardado en el fichero (ver _filasNuevasSinGuardar), para distinguirlas claramente de
        /// las que ya estaban en la biblioteca.
        /// </summary>
        private void dataGridViewBiblioteca_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dataGridViewBiblioteca.Rows[e.RowIndex].DataBoundItem is not PerfilLibreriaEntry entrada)
                return;

            if (!_filasNuevasSinGuardar.Contains(entrada))
                return;

            e.CellStyle.BackColor = Color.FromArgb(255, 244, 204);
            e.CellStyle.SelectionBackColor = Color.FromArgb(255, 224, 130);
        }

        private void dataGridViewBiblioteca_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            string nombreColumna = dataGridViewBiblioteca.Columns[e.ColumnIndex].Name;

            if (nombreColumna == "Duplicar")
            {
                DuplicarFilaBiblioteca(e.RowIndex);
            }
            else if (nombreColumna == "Eliminar")
            {
                EliminarFilaBiblioteca(e.RowIndex);
            }
        }

        /// <summary>
        /// Botón por fila "Duplicar": añade una copia de esta fila a la biblioteca (copia de
        /// trabajo en memoria) con la Referencia base vacía, para poder crear un perfil parecido
        /// sin rellenar de cero el Rol y el canal de herraje. Se deja la Referencia base vacía a
        /// propósito, para no guardar sin querer dos filas con la misma referencia (que es la
        /// clave con la que se busca cada perfil) antes de que el administrador indique la nueva.
        /// </summary>
        private void DuplicarFilaBiblioteca(int indiceFila)
        {
            dataGridViewBiblioteca.EndEdit();
            _bindingBiblioteca.EndEdit();

            if (dataGridViewBiblioteca.Rows[indiceFila].DataBoundItem is not PerfilLibreriaEntry origen)
                return;

            PerfilLibreriaEntry duplicado = ClonarEntrada(origen);
            duplicado.ReferenciaBase = "";

            _bibliotecaTrabajo.Add(duplicado);
            _filasNuevasSinGuardar.Add(duplicado);

            RefrescarGridBiblioteca();
            SeleccionarEntradaEnGridBiblioteca(duplicado);
        }

        /// <summary>
        /// Botón por fila "Eliminar": quita esta fila de la biblioteca (copia de trabajo en
        /// memoria), previa confirmación. El fichero fuente no se toca hasta que se pulsa 'Guardar'.
        /// </summary>
        private void EliminarFilaBiblioteca(int indiceFila)
        {
            dataGridViewBiblioteca.EndEdit();
            _bindingBiblioteca.EndEdit();

            if (dataGridViewBiblioteca.Rows[indiceFila].DataBoundItem is not PerfilLibreriaEntry entrada)
                return;

            string nombre = string.IsNullOrWhiteSpace(entrada.ReferenciaBase) ? "(sin referencia)" : entrada.ReferenciaBase;

            DialogResult respuesta = MessageBox.Show(
                $"¿Eliminar de la biblioteca el perfil {nombre}?",
                "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            _bibliotecaTrabajo.Remove(entrada);
            _filasNuevasSinGuardar.Remove(entrada);

            RefrescarGridBiblioteca();
        }

        private void btn_Guardar_Click(object sender, EventArgs e)
        {
            dataGridViewBiblioteca.EndEdit();
            _bindingBiblioteca.EndEdit();

            // Las filas nuevas ("Nuevo perfil") o duplicadas se dejan a propósito con la Referencia
            // base vacía; si se guarda sin rellenarla, se descartan en silencio al escribir el
            // fichero (no tiene sentido guardar un perfil sin la referencia con la que se busca),
            // así que se avisa aquí antes de continuar.
            List<PerfilLibreriaEntry> filasSinReferencia = _bibliotecaTrabajo
                .Where(p => string.IsNullOrWhiteSpace(p.ReferenciaBase))
                .ToList();

            if (filasSinReferencia.Count > 0)
            {
                DialogResult respuestaSinReferencia = MessageBox.Show(
                    $"Hay {filasSinReferencia.Count} fila(s) sin 'Referencia base'." + Environment.NewLine +
                    "Esas filas NO se guardarán en el fichero hasta que se les asigne una referencia." + Environment.NewLine + Environment.NewLine +
                    "¿Continuar guardando el resto igualmente?",
                    "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (respuestaSinReferencia != DialogResult.Yes)
                    return;
            }

            // La Referencia base es la clave con la que se busca cada perfil (ver
            // Cam3DHelpers.CargarBibliotecaPerfiles3D): si hay varias filas con la misma, solo la
            // última "gana" al cargarse, así que se avisa también de esto.
            List<string> referenciasDuplicadas = _bibliotecaTrabajo
                .Where(p => !string.IsNullOrWhiteSpace(p.ReferenciaBase))
                .GroupBy(p => p.ReferenciaBase.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (referenciasDuplicadas.Count > 0)
            {
                DialogResult respuestaDuplicadas = MessageBox.Show(
                    "Hay referencias repetidas en la biblioteca: " + string.Join(", ", referenciasDuplicadas) + "." + Environment.NewLine +
                    "Al cargarse, solo se tendrá en cuenta la última de cada una." + Environment.NewLine + Environment.NewLine +
                    "¿Continuar guardando igualmente?",
                    "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (respuestaDuplicadas != DialogResult.Yes)
                    return;
            }

            string ruta = _rutaArchivoBiblioteca;

            if (string.IsNullOrEmpty(ruta))
            {
                using SaveFileDialog dialogo = new SaveFileDialog
                {
                    Title = "Guardar BibliotecaPerfiles3D.json",
                    Filter = "Fichero JSON (*.json)|*.json",
                    FileName = "BibliotecaPerfiles3D.json"
                };

                if (dialogo.ShowDialog(this) != DialogResult.OK)
                    return;

                ruta = dialogo.FileName;
            }

            try
            {
                List<PerfilLibreriaEntry> aGuardar = _bibliotecaTrabajo
                    .Where(p => !string.IsNullOrWhiteSpace(p.ReferenciaBase))
                    .OrderBy(p => p.ReferenciaBase, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                JsonSerializerOptions opciones = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(aGuardar, opciones);

                File.WriteAllText(ruta, json, new UTF8Encoding(false));

                _rutaArchivoBiblioteca = ruta;

                // La sesión en curso ya usa la biblioteca actualizada (sin necesidad de reiniciar),
                // aunque el recurso embebido en el ensamblado no se actualice hasta la próxima
                // compilación, una vez subido el cambio al repositorio.
                Cam3DHelpers.ActualizarCacheBibliotecaPerfiles(aGuardar.Select(ClonarEntrada).ToList());

                MessageBox.Show(
                    "Biblioteca de perfiles guardada correctamente en:" + Environment.NewLine + ruta + Environment.NewLine + Environment.NewLine +
                    "Recuerde subir este cambio al repositorio (git) para que quede disponible en la próxima compilación." + Environment.NewLine +
                    "Mientras tanto, esta sesión ya utiliza la biblioteca actualizada.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Ya está guardado: ninguna fila sigue "sin guardar", así que se quita el resaltado
                // de todas.
                _filasNuevasSinGuardar.Clear();

                RefrescarGridBiblioteca();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la biblioteca de perfiles:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Volver_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Private methods
        private void ConfigurarGridBiblioteca()
        {
            dataGridViewBiblioteca.AutoGenerateColumns = false;
            dataGridViewBiblioteca.Columns.Clear();

            dataGridViewBiblioteca.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReferenciaBase",
                HeaderText = "Referencia base",
                DataPropertyName = "ReferenciaBase",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            var columnaRolBiblioteca = new DataGridViewComboBoxColumn
            {
                Name = "Role",
                HeaderText = "Rol",
                DataPropertyName = "Role",
                Width = 220,
                FlatStyle = FlatStyle.Flat,
                DropDownWidth = 220
            };

            // Además de los roles "oficiales" del catálogo de operaciones (RolesMecanizado3D), se
            // incluye cualquier otro valor que ya exista en la biblioteca (p.ej. "Elevadora",
            // "Lift Sash", "Sash") para no dejar sin representar en el combo ningún perfil ya
            // guardado, y así evitar el error de la grid al enlazar un valor que no esté en la lista.
            List<string> opcionesRolBiblioteca = new List<string> { "" };
            opcionesRolBiblioteca.AddRange(Cam3DHelpers.RolesMecanizado3D);
            opcionesRolBiblioteca.AddRange(
                _bibliotecaTrabajo
                    .Select(p => p.Role)
                    .Where(r => !string.IsNullOrWhiteSpace(r))
                    .Where(r => !opcionesRolBiblioteca.Contains(r, StringComparer.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(r => r, StringComparer.OrdinalIgnoreCase));
            columnaRolBiblioteca.DataSource = opcionesRolBiblioteca;
            dataGridViewBiblioteca.Columns.Add(columnaRolBiblioteca);

            dataGridViewBiblioteca.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PosicionCanalHerraje",
                HeaderText = "Canal herraje (altura)",
                DataPropertyName = "PosicionCanalHerraje",
                Width = 180
            });

            dataGridViewBiblioteca.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Duplicar",
                HeaderText = "",
                Text = "",
                UseColumnTextForButtonValue = true,
                Width = 50
            });

            dataGridViewBiblioteca.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Eliminar",
                HeaderText = "",
                Text = "",
                UseColumnTextForButtonValue = true,
                Width = 50
            });

            _bindingBiblioteca = new BindingSource();
            dataGridViewBiblioteca.DataSource = _bindingBiblioteca;
            dataGridViewBiblioteca.CellPainting += dataGridViewBiblioteca_CellPainting;
            dataGridViewBiblioteca.CellFormatting += dataGridViewBiblioteca_CellFormatting;
            dataGridViewBiblioteca.CellContentClick += dataGridViewBiblioteca_CellContentClick;
            dataGridViewBiblioteca.CellToolTipTextNeeded += dataGridViewBiblioteca_CellToolTipTextNeeded;
        }

        private void CargarGridBiblioteca()
        {
            RefrescarGridBiblioteca();
        }

        /// <summary>
        /// Vuelve a enlazar la grid con una copia de _bibliotecaTrabajo (filtrada por el texto del
        /// buscador) ordenada por Referencia base. Conserva la fila seleccionada (por referencia de
        /// objeto, ya que son las mismas instancias que las de _bibliotecaTrabajo) si sigue
        /// existiendo tras el refresco.
        /// </summary>
        private void RefrescarGridBiblioteca()
        {
            dataGridViewBiblioteca.EndEdit();

            PerfilLibreriaEntry seleccionActual = dataGridViewBiblioteca.CurrentRow?.DataBoundItem as PerfilLibreriaEntry;

            string texto = txt_BuscarBiblioteca.Text.Trim();

            IEnumerable<PerfilLibreriaEntry> query = _bibliotecaTrabajo;

            if (!string.IsNullOrWhiteSpace(texto))
            {
                query = query.Where(p => !string.IsNullOrWhiteSpace(p.ReferenciaBase) &&
                    p.ReferenciaBase.Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            List<PerfilLibreriaEntry> lista = query
                .OrderBy(p => p.ReferenciaBase, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _bindingBiblioteca.DataSource = lista;

            if (seleccionActual != null)
            {
                int indice = lista.FindIndex(p => ReferenceEquals(p, seleccionActual));

                if (indice >= 0)
                    _bindingBiblioteca.Position = indice;
            }

            // Fuerza un repintado completo, igual que en Cam3DCatalogoAdmin: al añadir/quitar filas
            // el cambio de tamaño/orden de la lista enlazada puede dejar alguna celda con el
            // contenido dibujado de la fila anterior hasta que algo repinta la grid entera.
            dataGridViewBiblioteca.Refresh();
        }

        /// <summary>
        /// Selecciona, en la grid, la fila que corresponde exactamente a esa instancia de
        /// PerfilLibreriaEntry (comparando por referencia, no por valor), si sigue visible con el
        /// filtro de búsqueda actual.
        /// </summary>
        private void SeleccionarEntradaEnGridBiblioteca(PerfilLibreriaEntry entrada)
        {
            if (_bindingBiblioteca.DataSource is not List<PerfilLibreriaEntry> lista)
                return;

            int indice = lista.FindIndex(p => ReferenceEquals(p, entrada));

            if (indice >= 0)
                _bindingBiblioteca.Position = indice;
        }

        /// <summary>
        /// Busca el fichero fuente Resources\Mecanizados3D\BibliotecaPerfiles3D.json subiendo desde
        /// la carpeta de ejecución (p.ej. bin\Debug\net8.0-windows\) hacia la raíz del proyecto, tal
        /// y como se ejecuta habitualmente en Visual Studio durante el desarrollo. Si no se
        /// encuentra (por ejemplo, ejecutando una copia instalada/publicada), se pedirá la ruta
        /// manualmente al pulsar 'Guardar'.
        /// </summary>
        private static string ResolverRutaArchivoBiblioteca()
        {
            string rutaRelativa = Path.Combine("Resources", "Mecanizados3D", "BibliotecaPerfiles3D.json");

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

        private static PerfilLibreriaEntry ClonarEntrada(PerfilLibreriaEntry original)
        {
            return new PerfilLibreriaEntry
            {
                ReferenciaBase = original.ReferenciaBase,
                Role = original.Role,
                PosicionCanalHerraje = original.PosicionCanalHerraje
            };
        }

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

        private static Bitmap CrearIconoEliminar()
        {
            var bmp = new Bitmap(18, 18, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using Pen pen = new Pen(Color.FromArgb(180, 40, 40), 1.4f)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };

                // Tapa y asa
                g.DrawLine(pen, 3, 5, 15, 5);
                g.DrawLine(pen, 7, 3, 11, 3);

                // Cuerpo de la papelera
                g.DrawLine(pen, 4, 5, 5, 16);
                g.DrawLine(pen, 14, 5, 13, 16);
                g.DrawLine(pen, 5, 16, 13, 16);

                // Líneas verticales internas
                g.DrawLine(pen, 7.5f, 7, 8, 14);
                g.DrawLine(pen, 10.5f, 7, 10, 14);
            }

            return bmp;
        }
        #endregion
    }
}
