using Microsoft.Data.SqlClient;
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
    public partial class Cam3D : Form
    {
        #region Public properties
        private List<MaterialBaseTreeRow> _materialesBase;
        private BindingSource _bindingMateriales;
        private List<PerfilAInstalarRow> _perfilesAInstalar;
        private BindingSource _bindingResultado;
        private List<OperationInstalarGridITem> _operacionesSeleccionadas;
        #endregion

        #region Constructors
        public Cam3D()
        {
            InitializeComponent();
        }

        public Cam3D(List<OperationInstalarGridITem> operacionesSeleccionadas) : this()
        {
            _operacionesSeleccionadas = operacionesSeleccionadas ?? new List<OperationInstalarGridITem>();
        }
        #endregion

        #region Events
        private void Cam3D_Load(object sender, EventArgs e)
        {
            InitializeInfoConnection();

            MostrarOperacionesSeleccionadas();

            CargarMaterialesBase();

            ConfigurarTreeView();
            CargarTreeViewMateriales();

            ConfigurarGridMateriales();
            CargarGridMateriales();

            ConfigurarGridResultado();
            CargarGridResultado();
        }
        private void treeViewMateriales_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null)
                return;

            if (e.Node.Tag != null)
            {
                AgregarPerfilAResultado(e.Node.Tag.ToString());
            }
            else
            {
                // Doble clic sobre una carpeta (nivel del árbol, sin Tag propio): añadir de golpe
                // todos los perfiles que cuelguen de ella, en vez de tener que ir uno a uno.
                AgregarPerfilesDeNodoAResultado(e.Node);
            }
        }
        private void treeViewMateriales_DoubleClick(object sender, EventArgs e)
        {
            var nodo = treeViewMateriales.SelectedNode;
            if (nodo == null)
                return;

            if (nodo.Tag != null)
            {
                AgregarPerfilAResultado(nodo.Tag.ToString());
            }
            else
            {
                AgregarPerfilesDeNodoAResultado(nodo);
            }
        }
        private void dataGridViewMateriales_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dataGridViewMateriales.Rows[e.RowIndex].DataBoundItem is MaterialBaseTreeRow fila)
            {
                AgregarPerfilAResultado(fila.ReferenciaBase);
            }
        }
        private void dataGridViewMateriales_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewMateriales.CurrentRow?.DataBoundItem is MaterialBaseTreeRow fila)
            {
                SeleccionarNodoEnTreeView(fila.ReferenciaBase);
            }
        }
        private void dataGridViewResultado_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dataGridViewResultado.Columns[e.ColumnIndex].Name == "Quitar")
            {
                QuitarPerfilDeResultado(e.RowIndex);
            }
        }
        private void txt_Buscar_TextChanged(object sender, EventArgs e)
        {
            string texto = txt_Buscar.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                _bindingMateriales.DataSource = _materialesBase;
                return;
            }

            var resultados = _materialesBase
                .Where(x => !string.IsNullOrEmpty(x.ReferenciaBase)
                         && x.ReferenciaBase.Contains(
                             texto,
                             StringComparison.OrdinalIgnoreCase))
                .ToList();

            _bindingMateriales.DataSource = resultados;
        }
        private void btn_Volver_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_CatalogoOperaciones_Click(object sender, EventArgs e)
        {
            // Pantalla exclusiva de administración: permite añadir al catálogo embebido
            // (CatalogoOperaciones3D.json) las combinaciones Operación/Rol que todavía no
            // tienen una plantilla definida, para las operaciones seleccionadas actualmente.
            Cam3DCatalogoAdmin formCatalogo = new Cam3DCatalogoAdmin(_operacionesSeleccionadas);
            formCatalogo.ShowDialog();
        }

        private void btn_InstalarOperaciones_Click(object sender, EventArgs e)
        {
            if (_operacionesSeleccionadas == null || !_operacionesSeleccionadas.Any())
            {
                MessageBox.Show("No hay operaciones seleccionadas para instalar. Cierre esta ventana, seleccione operaciones en la grid del CAM y vuelva a pulsar 'Instalar 3D'.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dataGridViewResultado.EndEdit();
            _bindingResultado.EndEdit();

            List<PerfilAInstalarRow> perfilesAInstalar = _perfilesAInstalar.ToList();

            if (!perfilesAInstalar.Any())
            {
                MessageBox.Show("Añada al menos un perfil a la lista de instalación, haciendo doble clic sobre él en el árbol o en la grid de perfiles.",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<PerfilAInstalarRow> perfilesSinRol = perfilesAInstalar
                .Where(r => string.IsNullOrWhiteSpace(r.RolMecanizado))
                .ToList();

            if (perfilesSinRol.Any())
            {
                MessageBox.Show("Indique el 'Rol mecanizado' para todos los perfiles de la lista:" + Environment.NewLine +
                    string.Join(", ", perfilesSinRol.Select(r => r.ReferenciaBase)),
                    "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<PerfilAInstalarRow> perfilesSinDescuento = perfilesAInstalar
                .Where(r => Cam3DHelpers.RolesConCanalHerraje.Contains(r.RolMecanizado) && r.DescuentoCanalHerraje == null)
                .ToList();

            if (perfilesSinDescuento.Any())
            {
                MessageBox.Show("Indique el 'Descuento canal de herraje' para los siguientes perfiles (rol de hoja):" + Environment.NewLine +
                    string.Join(", ", perfilesSinDescuento.Select(r => r.ReferenciaBase)),
                    "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<PerfilAInstalarRow> perfilesSinCanal = perfilesAInstalar
                .Where(r => Cam3DHelpers.RolesConCanalHerraje.Contains(r.RolMecanizado) && r.PosicionCanalHerraje == null)
                .ToList();

            if (perfilesSinCanal.Any())
            {
                MessageBox.Show("Indique la 'Posición canal de herraje' para los siguientes perfiles (rol de hoja):" + Environment.NewLine +
                    string.Join(", ", perfilesSinCanal.Select(r => r.ReferenciaBase)),
                    "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ResultadoInstalacion3D resultado = new ResultadoInstalacion3D();

            try
            {
                Cursor = Cursors.WaitCursor;
                EnableControls(false);

                progress_Instalar3D.Visible = true;
                progress_Instalar3D.Value = 0;
                progress_Instalar3D.Maximum = perfilesAInstalar.Count > 0 ? perfilesAInstalar.Count : 1;

                // 1. Catálogo de plantillas de mecanizado 3D (embebido)
                List<Operacion3DTemplate> catalogo = Cam3DHelpers.CargarCatalogoOperaciones3D();

                // Para cada operación seleccionada, indica si se ha encontrado una plantilla en el
                // catálogo para AL MENOS uno de los roles de los perfiles de la lista (da igual
                // cuál). Solo se informará al usuario de una operación como "no instalada" si no ha
                // encontrado definición para NINGÚN perfil/rol de los seleccionados: que falte la
                // definición para algún rol concreto no se considera un problema, porque puede que
                // esa operación sencillamente no la necesite.
                Dictionary<string, bool> operacionesConDefinicion = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                foreach (OperationInstalarGridITem opSeleccionada in _operacionesSeleccionadas)
                {
                    string nombreOperacion = "RO_" + opSeleccionada.OperationName;
                    if (!operacionesConDefinicion.ContainsKey(nombreOperacion))
                        operacionesConDefinicion[nombreOperacion] = false;
                }

                // 2. Asegurar que la definición 2D de cada operación seleccionada está instalada
                List<MechanizedOperation> mechanizedOperationsEmbebidos = Helpers.CargarMechanizedOperationsRotoEmbebidos();
                List<MechanizedOperation> macrosEmbeddedMechanizedOperations = Helpers.CargarMacrosMechanizedOperationsEmbebidos();
                List<OperationsShapes> macroOperationsShapesEmbeddedList = Helpers.CargarMacrosOperationsShapesEmbebidos();

                foreach (OperationInstalarGridITem op in _operacionesSeleccionadas)
                {
                    string nombreCompleto = "RO_" + op.OperationName;

                    Cam3DHelpers.AsegurarDefinicion2DInstalada(
                        nombreCompleto,
                        op.OperationShapeList,
                        op.OperationShapeExtList,
                        mechanizedOperationsEmbebidos,
                        macrosEmbeddedMechanizedOperations,
                        macroOperationsShapesEmbeddedList);
                }

                // 3. Instalación de ProfileOperations (una única transacción). Los datos constructivos
                // y el descuento de canal de herraje ya vienen precargados en cada PerfilAInstalarRow
                // (ver CargarMaterialesBase / AgregarPerfilAResultado), así que no hace falta ninguna
                // consulta adicional para obtenerlos aquí.
                using (var conn = new SqlConnection(Helpers.GetConnectionString()))
                {
                    conn.Open();

                    using (SqlTransaction tx = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (PerfilAInstalarRow perfil in perfilesAInstalar)
                            {
                                if (perfil.ProfileId == Guid.Empty)
                                {
                                    resultado.CombinacionesSinDefinicion.Add($"{perfil.ReferenciaBase}: no se han encontrado datos constructivos.");
                                    progress_Instalar3D.Value++;
                                    progress_Instalar3D.Refresh();
                                    Application.DoEvents();
                                    continue;
                                }

                                resultado.PerfilesProcesados++;

                                var variables = new Dictionary<string, double>
                                {
                                    ["AnchoInterior"] = perfil.AnchoInterior,
                                    ["AnchoExterior"] = perfil.AnchoExterior,
                                    ["CuerpoInterior"] = perfil.CuerpoInterior,
                                    ["CuerpoExterior"] = perfil.CuerpoExterior,
                                    ["Altura"] = perfil.Altura
                                };

                                if (Cam3DHelpers.RolesConCanalHerraje.Contains(perfil.RolMecanizado))
                                {
                                    variables["Ala"] = perfil.DescuentoCanalHerraje ?? 0;
                                    variables["PosicionCanalHerraje"] = perfil.PosicionCanalHerraje ?? 0;
                                }

                                foreach (OperationInstalarGridITem op in _operacionesSeleccionadas)
                                {
                                    string nombreCompleto = "RO_" + op.OperationName;

                                    List<Operacion3DTemplate> plantillas = catalogo
                                        .Where(p => string.Equals(p.OperationName, nombreCompleto, StringComparison.OrdinalIgnoreCase)
                                                 && string.Equals(p.Role, perfil.RolMecanizado, StringComparison.OrdinalIgnoreCase))
                                        .ToList();

                                    if (!plantillas.Any())
                                    {
                                        // No se informa aquí: puede que esta operación, sencillamente,
                                        // no necesite definición para este rol en concreto. Se avisará
                                        // al final únicamente si no se ha encontrado definición para
                                        // NINGÚN perfil/rol de los seleccionados (ver más abajo).
                                        continue;
                                    }

                                    operacionesConDefinicion[nombreCompleto] = true;

                                    foreach (Operacion3DTemplate plantilla in plantillas)
                                    {
                                        if (Cam3DHelpers.ExisteProfileOperation(conn, tx, perfil.ProfileId, plantilla.OperationName, plantilla.Outer))
                                        {
                                            resultado.OperacionesOmitidasPorExistente++;
                                            continue;
                                        }

                                        Cam3DHelpers.InstalarProfileOperation(conn, tx, perfil.ProfileId, perfil.ReferenciaBase, plantilla, variables);
                                        resultado.OperacionesInstaladas++;
                                    }
                                }

                                progress_Instalar3D.Value++;
                                progress_Instalar3D.Refresh();
                                Application.DoEvents();
                            }

                            tx.Commit();
                        }
                        catch
                        {
                            tx.Rollback();
                            throw;
                        }
                    }
                }

                // Una operación seleccionada solo se informa como "no instalada" si no se ha
                // encontrado definición para ninguno de los perfiles/roles de la lista.
                foreach (KeyValuePair<string, bool> operacionSinInstalar in operacionesConDefinicion.Where(kvp => !kvp.Value))
                {
                    resultado.OperacionesSinDefinicionEnCatalogo.Add(operacionSinInstalar.Key);
                }

                // Vaciar la lista de perfiles a instalar tras un proceso correcto
                _perfilesAInstalar.Clear();
                _bindingResultado.ResetBindings(false);

                string resumen = $"Perfiles procesados: {resultado.PerfilesProcesados}" + Environment.NewLine +
                    $"Operaciones instaladas: {resultado.OperacionesInstaladas}" + Environment.NewLine +
                    $"Operaciones omitidas (ya existían): {resultado.OperacionesOmitidasPorExistente}";

                if (resultado.CombinacionesSinDefinicion.Any())
                {
                    resumen += Environment.NewLine + Environment.NewLine +
                        "Perfiles sin datos constructivos:" + Environment.NewLine +
                        string.Join(Environment.NewLine, resultado.CombinacionesSinDefinicion.Take(20));

                    if (resultado.CombinacionesSinDefinicion.Count > 20)
                        resumen += Environment.NewLine + $"... y {resultado.CombinacionesSinDefinicion.Count - 20} más.";
                }

                if (resultado.OperacionesSinDefinicionEnCatalogo.Any())
                {
                    resumen += Environment.NewLine + Environment.NewLine +
                        "No se ha encontrado definición en el catálogo 3D (para ningún rol de los perfiles de la lista) para:" + Environment.NewLine +
                        string.Join(Environment.NewLine, resultado.OperacionesSinDefinicionEnCatalogo.Take(20));

                    if (resultado.OperacionesSinDefinicionEnCatalogo.Count > 20)
                        resumen += Environment.NewLine + $"... y {resultado.OperacionesSinDefinicionEnCatalogo.Count - 20} más.";
                }

                bool hayAvisos = resultado.CombinacionesSinDefinicion.Any() || resultado.OperacionesSinDefinicionEnCatalogo.Any();

                MessageBox.Show(resumen, "Instalación de mecanizados 3D", MessageBoxButtons.OK,
                    hayAvisos ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al instalar los mecanizados 3D:" + Environment.NewLine + Environment.NewLine + ex.Message,
                    "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                EnableControls(true);
                progress_Instalar3D.Value = 0;
                progress_Instalar3D.Visible = false;
            }
        }
        #endregion

        #region Private methods
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void MostrarOperacionesSeleccionadas()
        {
            if (lst_OperacionesInfo == null)
                return;

            lst_OperacionesInfo.Items.Clear();

            if (_operacionesSeleccionadas == null || !_operacionesSeleccionadas.Any())
            {
                lst_OperacionesInfo.Items.Add("No hay operaciones seleccionadas.");
                return;
            }

            foreach (OperationInstalarGridITem op in _operacionesSeleccionadas)
            {
                lst_OperacionesInfo.Items.Add("RO_" + op.OperationName);
            }

            if (grp_OperacionesInfo != null)
            {
                grp_OperacionesInfo.Text = $"Operaciones a instalar en 3D ({_operacionesSeleccionadas.Count})";
            }
        }
        private void CargarMaterialesBase()
        {
            _materialesBase = new List<MaterialBaseTreeRow>();

            // Se traen de una sola vez, para TODOS los perfiles, tanto los datos constructivos
            // (Perfiles) como el descuento de canal de herraje (Distances, mismo LEFT JOIN que antes
            // se hacía por separado en Cam3DHelpers.ObtenerAla). Así, al hacer doble clic para añadir
            // un perfil a la lista de instalación no hace falta ninguna consulta nueva a la base de
            // datos: es instantáneo, incluso con latencia de red alta.
            string queryPerfiles = @"
                    SELECT
                        mb.RowId,
                        mb.ReferenciaBase,
                        mb.Descripcion,
                        mb.Nivel1,
                        mb.Nivel2,
                        mb.Nivel3,
                        mb.Nivel4,
                        mb.Nivel5,
                        mb.Role,
                        p.AnchoInterior,
                        p.AnchoExterior,
                        p.CuerpoInterior,
                        p.CuerpoExterior,
                        p.Altura,
                        d.PDistance AS DescuentoCanalHerraje
                    FROM MaterialesBase mb
                    INNER JOIN Perfiles p
                        ON p.ReferenciaBase = mb.ReferenciaBase
                    LEFT JOIN Distances d
                        ON d.MasterId = mb.RowId AND d.SlaveId = @slaveId
                    WHERE mb.[Role]='frame' OR mb.[Role]='sash' OR mb.[Role]='mullion' OR mb.[Role]='sash stop'
                    ORDER BY
                        mb.Nivel1,
                        mb.Nivel2,
                        mb.Nivel3,
                        mb.Nivel4,
                        mb.Nivel5,
                        mb.ReferenciaBase";

            using (var conn = new SqlConnection(Helpers.GetConnectionString()))
            using (var cmd = new SqlCommand(queryPerfiles, conn))
            {
                cmd.Parameters.Add("@slaveId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(Cam3DHelpers.RowIdDescuentoTipoEsclavoAla);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Los datos vienen muchas veces con espacios en blanco al final desde la base de datos.
                        _materialesBase.Add(new MaterialBaseTreeRow
                        {
                            RowId = reader["RowId"] == DBNull.Value ? Guid.Empty : (Guid)reader["RowId"],
                            ReferenciaBase = reader["ReferenciaBase"]?.ToString().Trim(),
                            Descripcion = reader["Descripcion"]?.ToString().Trim(),
                            Nivel1 = reader["Nivel1"]?.ToString().Trim(),
                            Nivel2 = reader["Nivel2"]?.ToString().Trim(),
                            Nivel3 = reader["Nivel3"]?.ToString().Trim(),
                            Nivel4 = reader["Nivel4"]?.ToString().Trim(),
                            Nivel5 = reader["Nivel5"]?.ToString().Trim(),
                            Role = reader["Role"]?.ToString().Trim(),
                            AnchoInterior = Cam3DHelpers.ConvertirADouble(reader["AnchoInterior"]),
                            AnchoExterior = Cam3DHelpers.ConvertirADouble(reader["AnchoExterior"]),
                            CuerpoInterior = Cam3DHelpers.ConvertirADouble(reader["CuerpoInterior"]),
                            CuerpoExterior = Cam3DHelpers.ConvertirADouble(reader["CuerpoExterior"]),
                            Altura = Cam3DHelpers.ConvertirADouble(reader["Altura"]),
                            DescuentoCanalHerraje = reader["DescuentoCanalHerraje"] == DBNull.Value
                                ? (double?)null
                                : Cam3DHelpers.ConvertirADouble(reader["DescuentoCanalHerraje"])
                        });
                    }
                }
            }
        }
        private void ConfigurarTreeView()
        {
            treeViewMateriales.ImageList = imageList1;

            treeViewMateriales.ImageIndex = 0;
            treeViewMateriales.SelectedImageIndex = 0;

            treeViewMateriales.HideSelection = false;
        }
        private void CargarTreeViewMateriales()
        {
            treeViewMateriales.BeginUpdate();

            try
            {
                treeViewMateriales.Nodes.Clear();

                // Se reutilizan los datos ya cargados en _materialesBase (ya recortados con Trim())
                // para no repetir la consulta ni el recorte de espacios en blanco.
                foreach (MaterialBaseTreeRow fila in _materialesBase)
                {
                    string referenciaBase = fila.ReferenciaBase;

                    if (string.IsNullOrWhiteSpace(referenciaBase))
                        continue;

                    string[] niveles =
                    {
                        fila.Nivel1,
                        fila.Nivel2,
                        fila.Nivel3,
                        fila.Nivel4,
                        fila.Nivel5
                    };

                    TreeNodeCollection nodosActuales = treeViewMateriales.Nodes;

                    TreeNode ultimoNodo = null;

                    // Crear únicamente los niveles que tengan valor
                    foreach (string nivel in niveles)
                    {
                        if (string.IsNullOrWhiteSpace(nivel))
                            break;

                        ultimoNodo = ObtenerOCrearNodo(nodosActuales, nivel);

                        nodosActuales = ultimoNodo.Nodes;
                    }

                    TreeNode nodoMaterial = new TreeNode(referenciaBase)
                    {
                        Tag = referenciaBase,
                        ImageIndex = 1,
                        SelectedImageIndex = 1
                    };

                    // Añadir el material al último nivel existente, o directamente en la raíz si no hay niveles
                    if (ultimoNodo != null)
                        ultimoNodo.Nodes.Add(nodoMaterial);
                    else
                        treeViewMateriales.Nodes.Add(nodoMaterial);
                }

                // No ExpandAll().
                // El TreeView comienza contraído.
            }
            finally
            {
                treeViewMateriales.EndUpdate();
            }
        }
        private TreeNode ObtenerOCrearNodo(TreeNodeCollection nodos, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                texto = "(Sin definir)";
            }

            foreach (TreeNode nodo in nodos)
            {
                if (string.Equals(
                    nodo.Text,
                    texto,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return nodo;
                }
            }

            TreeNode nuevoNodo = new TreeNode(texto)
            {
                ImageIndex = 0,
                SelectedImageIndex = 0
            };

            nodos.Add(nuevoNodo);

            return nuevoNodo;
        }
        /// <summary>
        /// Al seleccionar una fila en la grid de todos los perfiles, navega automáticamente en el
        /// árbol hasta el nodo del perfil correspondiente (expandiendo sus nodos padre si hace
        /// falta) y lo deja visible y seleccionado, para localizarlo dentro de su jerarquía.
        /// </summary>
        private void SeleccionarNodoEnTreeView(string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia))
                return;

            TreeNode nodo = BuscarNodoPorTag(treeViewMateriales.Nodes, referencia);

            if (nodo == null)
                return;

            treeViewMateriales.SelectedNode = nodo;
            nodo.EnsureVisible();
        }
        private TreeNode BuscarNodoPorTag(TreeNodeCollection nodos, string referencia)
        {
            foreach (TreeNode nodo in nodos)
            {
                if (nodo.Tag != null && string.Equals(nodo.Tag.ToString(), referencia, StringComparison.OrdinalIgnoreCase))
                    return nodo;

                TreeNode encontrado = BuscarNodoPorTag(nodo.Nodes, referencia);
                if (encontrado != null)
                    return encontrado;
            }

            return null;
        }
        private void ConfigurarGridMateriales()
        {
            dataGridViewMateriales.AutoGenerateColumns = false;
            dataGridViewMateriales.AllowUserToAddRows = false;
            dataGridViewMateriales.AllowUserToDeleteRows = false;
            dataGridViewMateriales.ReadOnly = true;
            dataGridViewMateriales.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMateriales.MultiSelect = false;

            dataGridViewMateriales.Columns.Clear();

            dataGridViewMateriales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReferenciaBase",
                HeaderText = "Referencia",
                DataPropertyName = "ReferenciaBase",
                Width = 130
            });

            dataGridViewMateriales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Descripcion",
                HeaderText = "Descripción",
                DataPropertyName = "Descripcion",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridViewMateriales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Rol",
                HeaderText = "Rol",
                DataPropertyName = "Role",
                Width = 90
            });

            dataGridViewMateriales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nivel1",
                HeaderText = "Nivel 1",
                DataPropertyName = "Nivel1",
                Width = 120
            });

            dataGridViewMateriales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nivel2",
                HeaderText = "Nivel 2",
                DataPropertyName = "Nivel2",
                Width = 120
            });

            dataGridViewMateriales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nivel3",
                HeaderText = "Nivel 3",
                DataPropertyName = "Nivel3",
                Width = 120
            });

            dataGridViewMateriales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nivel4",
                HeaderText = "Nivel 4",
                DataPropertyName = "Nivel4",
                Width = 120
            });

            dataGridViewMateriales.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nivel5",
                HeaderText = "Nivel 5",
                DataPropertyName = "Nivel5",
                Width = 120
            });

            _bindingMateriales = new BindingSource();

            dataGridViewMateriales.DataSource = _bindingMateriales;
        }
        private void CargarGridMateriales()
        {
            _bindingMateriales.DataSource = _materialesBase;
        }
        private void ConfigurarGridResultado()
        {
            dataGridViewResultado.AutoGenerateColumns = false;
            dataGridViewResultado.AllowUserToAddRows = false;
            dataGridViewResultado.AllowUserToDeleteRows = false;
            dataGridViewResultado.ReadOnly = false;
            dataGridViewResultado.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewResultado.MultiSelect = false;

            dataGridViewResultado.Columns.Clear();

            dataGridViewResultado.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Quitar",
                HeaderText = "",
                Text = "Quitar",
                UseColumnTextForButtonValue = true,
                Width = 80
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReferenciaBase",
                HeaderText = "Referencia",
                DataPropertyName = "ReferenciaBase",
                ReadOnly = true,
                Width = 130
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Descripcion",
                HeaderText = "Descripción",
                DataPropertyName = "Descripcion",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Rol",
                HeaderText = "Rol",
                DataPropertyName = "Role",
                ReadOnly = true,
                Width = 100
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AnchoInterior",
                HeaderText = "Ancho interior",
                DataPropertyName = "AnchoInterior",
                ReadOnly = true,
                Width = 110
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AnchoExterior",
                HeaderText = "Ancho exterior",
                DataPropertyName = "AnchoExterior",
                ReadOnly = true,
                Width = 110
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CuerpoInterior",
                HeaderText = "Cuerpo interior",
                DataPropertyName = "CuerpoInterior",
                ReadOnly = true,
                Width = 110
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CuerpoExterior",
                HeaderText = "Cuerpo exterior",
                DataPropertyName = "CuerpoExterior",
                ReadOnly = true,
                Width = 110
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Altura",
                HeaderText = "Altura",
                DataPropertyName = "Altura",
                ReadOnly = true,
                Width = 110
            });

            var columnaRolMecanizado = new DataGridViewComboBoxColumn
            {
                Name = "RolMecanizado",
                HeaderText = "Rol mecanizado",
                DataPropertyName = "RolMecanizado",
                Width = 160,
                FlatStyle = FlatStyle.Flat,
                DropDownWidth = 160
            };

            List<string> opcionesRol = new List<string> { "" };
            opcionesRol.AddRange(Cam3DHelpers.RolesMecanizado3D);
            columnaRolMecanizado.DataSource = opcionesRol;
            dataGridViewResultado.Columns.Add(columnaRolMecanizado);

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DescuentoCanalHerraje",
                HeaderText = "Descuento canal de herraje",
                DataPropertyName = "DescuentoCanalHerraje",
                Width = 200
            });

            dataGridViewResultado.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PosicionCanalHerraje",
                HeaderText = "Posición canal de herraje",
                DataPropertyName = "PosicionCanalHerraje",
                Width = 200
            });

            _bindingResultado = new BindingSource();

            dataGridViewResultado.DataSource = _bindingResultado;

            dataGridViewResultado.CellContentClick += dataGridViewResultado_CellContentClick;
        }
        private void CargarGridResultado()
        {
            _perfilesAInstalar = new List<PerfilAInstalarRow>();
            _bindingResultado.DataSource = _perfilesAInstalar;
        }
        private void AgregarPerfilAResultado(string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia))
                return;

            referencia = referencia.Trim();

            // Si ya está en la lista de perfiles a instalar, no se añade de nuevo.
            if (_perfilesAInstalar.Any(p => string.Equals(p.ReferenciaBase, referencia, StringComparison.OrdinalIgnoreCase)))
                return;

            MaterialBaseTreeRow filaOrigen = _materialesBase
                .FirstOrDefault(r => string.Equals(r.ReferenciaBase, referencia, StringComparison.OrdinalIgnoreCase));

            if (filaOrigen == null)
                return;

            string rolMecanizado = "";
            double? canal = null;

            // Biblioteca de perfiles ya conocidos (AluEuropa OM, Cortizo, Deceuninck, Kommerling):
            // si la referencia está en la biblioteca, se autocompleta el Rol de mecanizado y la
            // altura del canal de herraje para no volver a pedirlos.
            Dictionary<string, PerfilLibreriaEntry> biblioteca = Cam3DHelpers.CargarBibliotecaPerfiles3D();
            if (biblioteca.TryGetValue(referencia, out PerfilLibreriaEntry entradaLibreria))
            {
                rolMecanizado = Cam3DHelpers.NormalizarRolBiblioteca(entradaLibreria.Role);
                canal = entradaLibreria.PosicionCanalHerraje;
            }

            if (string.IsNullOrEmpty(rolMecanizado))
            {
                rolMecanizado = Cam3DHelpers.RolPorDefecto(filaOrigen.Role);
            }

            // El descuento de canal de herraje (y los datos constructivos) ya están precargados en
            // filaOrigen (ver CargarMaterialesBase): no hace falta ninguna consulta a la base de
            // datos aquí, por eso añadir un perfil a la lista es instantáneo.
            _perfilesAInstalar.Add(new PerfilAInstalarRow
            {
                ProfileId = filaOrigen.RowId,
                ReferenciaBase = filaOrigen.ReferenciaBase,
                Descripcion = filaOrigen.Descripcion,
                Role = filaOrigen.Role,
                RolMecanizado = rolMecanizado,
                AnchoInterior = filaOrigen.AnchoInterior,
                AnchoExterior = filaOrigen.AnchoExterior,
                CuerpoInterior = filaOrigen.CuerpoInterior,
                CuerpoExterior = filaOrigen.CuerpoExterior,
                Altura = filaOrigen.Altura,
                DescuentoCanalHerraje = filaOrigen.DescuentoCanalHerraje,
                PosicionCanalHerraje = canal
            });

            _bindingResultado.ResetBindings(false);
        }
        /// <summary>
        /// Doble clic sobre una carpeta del árbol (un nivel, sin Tag propio): añade a la grid de
        /// "Perfiles a instalar" todos los perfiles (nodos hoja, con Tag) que cuelguen de ella, en
        /// cualquier subnivel, para no tener que ir añadiéndolos uno a uno.
        /// </summary>
        private void AgregarPerfilesDeNodoAResultado(TreeNode nodoCarpeta)
        {
            if (nodoCarpeta == null)
                return;

            List<string> referencias = new List<string>();
            RecolectarReferenciasHoja(nodoCarpeta, referencias);

            foreach (string referencia in referencias)
            {
                AgregarPerfilAResultado(referencia);
            }
        }
        private void RecolectarReferenciasHoja(TreeNode nodo, List<string> referencias)
        {
            if (nodo.Tag != null)
            {
                referencias.Add(nodo.Tag.ToString());
                return;
            }

            foreach (TreeNode hijo in nodo.Nodes)
            {
                RecolectarReferenciasHoja(hijo, referencias);
            }
        }
        private void QuitarPerfilDeResultado(int indiceFila)
        {
            if (indiceFila < 0 || indiceFila >= _perfilesAInstalar.Count)
                return;

            _perfilesAInstalar.RemoveAt(indiceFila);
            _bindingResultado.ResetBindings(false);
        }
        /// <summary>
        /// Botón "Limpiar": vacía de golpe toda la grid de "Perfiles a instalar", para no tener que
        /// pulsar "Quitar" fila a fila.
        /// </summary>
        private void btn_LimpiarResultado_Click(object sender, EventArgs e)
        {
            if (_perfilesAInstalar.Count == 0)
                return;

            DialogResult respuesta = MessageBox.Show(
                $"¿Quitar los {_perfilesAInstalar.Count} perfil(es) de la lista de perfiles a instalar?",
                "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            _perfilesAInstalar.Clear();
            _bindingResultado.ResetBindings(false);
        }
        private void EnableControls(bool enabled)
        {
            btn_InstalarOperaciones.Enabled = enabled;
            dataGridViewMateriales.Enabled = enabled;
            dataGridViewResultado.Enabled = enabled;
            treeViewMateriales.Enabled = enabled;
            txt_Buscar.Enabled = enabled;
        }

        #endregion
    }
    public class MaterialBaseTreeItem
    {
        public string ReferenciaBase { get; set; }
        public string Nivel1 { get; set; }
        public string Nivel2 { get; set; }
        public string Nivel3 { get; set; }
        public string Nivel4 { get; set; }
        public string Nivel5 { get; set; }
    }
    public class MaterialBaseTreeRow
    {
        public Guid RowId { get; set; }
        public string ReferenciaBase { get; set; }
        public string Descripcion { get; set; }
        public string Nivel1 { get; set; }
        public string Nivel2 { get; set; }
        public string Nivel3 { get; set; }
        public string Nivel4 { get; set; }
        public string Nivel5 { get; set; }
        public string Role { get; set; }

        // Datos constructivos (Perfiles) y descuento de canal de herraje (Distances), precargados
        // aquí para TODOS los perfiles en una sola consulta (ver CargarMaterialesBase), de forma que
        // añadir un perfil a la lista de instalación no necesite ninguna consulta adicional.
        public double AnchoInterior { get; set; }
        public double AnchoExterior { get; set; }
        public double CuerpoInterior { get; set; }
        public double CuerpoExterior { get; set; }
        public double Altura { get; set; }
        public double? DescuentoCanalHerraje { get; set; }
    }

    /// <summary>
    /// Fila de la grid resultado del formulario Cam3D: un perfil ya añadido a la lista de instalación,
    /// con el Rol de mecanizado y la altura del canal de herraje (autocompletados desde la biblioteca
    /// de perfiles si la referencia ya es conocida, o a indicar manualmente si no lo es).
    /// No se persiste entre sesiones.
    /// </summary>
    public class PerfilAInstalarRow
    {
        // MaterialesBase.RowId: se copia aquí al añadir el perfil (ya está precargado en
        // MaterialBaseTreeRow) para no tener que volver a consultarlo al instalar.
        public Guid ProfileId { get; set; }
        public string ReferenciaBase { get; set; }
        public string Descripcion { get; set; }
        public string Role { get; set; }
        public string RolMecanizado { get; set; }

        // Datos constructivos (Perfiles), copiados de MaterialBaseTreeRow al añadir el perfil, para
        // evaluar las fórmulas del catálogo de operaciones 3D sin volver a consultar la base de datos.
        public double AnchoInterior { get; set; }
        public double AnchoExterior { get; set; }
        public double CuerpoInterior { get; set; }
        public double CuerpoExterior { get; set; }
        public double Altura { get; set; }

        // Descuento (mismo valor que internamente se calcula como "Ala" a partir de la tabla
        // Distances) que se carga automáticamente al añadir el perfil, pero puede corregirse a mano.
        public double? DescuentoCanalHerraje { get; set; }
        public double? PosicionCanalHerraje { get; set; }
    }
}
