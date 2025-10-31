using Microsoft.Data.SqlClient;
using RotoEntities;
using System.ComponentModel;
using System.Data;
using System.Xml.Linq;

namespace RotoTools
{
    public partial class ConfiguradorOpciones : Form
    {
        #region Private properties

        private List<Opcion> opcionesList = new List<Opcion>();

        #endregion

        #region Events
        private void ConfiguradorOpciones_Load(object sender, EventArgs e)
        {
            InitializeInfoConnection();
            //CargarTextos();
            FillOpcionesList();
            FillContenidoOpciones();
            ConfigurarDataGridView();

            // Enlazamos la lista al ListBox
            listBox_Opciones.DataSource = opcionesList;
            listBox_Opciones.DisplayMember = "Name";
            listBox_Opciones.ValueMember = "Name";
        }
        private void txt_Filter_TextChanged(object sender, EventArgs e)
        {
            string filtro = txt_Filter.Text.Trim().ToUpper();

            var filtradas = opcionesList
                .Where(o => o.Name.ToUpper().Contains(filtro))
                .ToList();

            listBox_Opciones.DataSource = null;  // hay que resetear primero
            listBox_Opciones.DataSource = filtradas;
            listBox_Opciones.DisplayMember = "Name";
        }
        private void listBox_Opciones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Opciones.SelectedIndex < 0) return;
            if (listBox_Opciones.SelectedItem is Opcion opcionSeleccionada)
            {
                datagrid_ContenidoOpciones.DataSource = new BindingList<ContenidoOpcion>(opcionSeleccionada.ContenidoOpcionesList);
            }
        }
        private void btn_SaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                string configPath = SaveOpcionesConfig();
                if (!String.IsNullOrEmpty(configPath))
                {
                    Helpers.RestoreOpcionesDesdeXml(configPath);
                    MessageBox.Show(LocalizationManager.GetString("L_GuardadoCorrectamente"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationManager.GetString("L_ErrorGuardarConfiguracion") + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Constructor
        public ConfiguradorOpciones()
        {
            InitializeComponent();
        }

        #endregion

        #region Private methods
        private void CargarTextos()
        {
            lbl_Filtrar.Text = LocalizationManager.GetString("L_Buscar");
            this.Text = LocalizationManager.GetString("L_ConfigurarOpciones");
            btn_SaveConfig.Text = LocalizationManager.GetString("L_Guardar");
        }
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void FillOpcionesList()
        {
            List<Opcion> optionsList = new List<Opcion>();
            using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand(@"SELECT NOMBRE, DESCRIPCION, NIVEL1, NIVEL2, NIVEL3, NIVEL4, NIVEL5, FLAGS FROM OPCIONES WHERE NOMBRE LIKE 'RO\_%' ESCAPE '\' ORDER BY NOMBRE", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

            optionsList.Clear();

            while (reader.Read())
            {
                Opcion opcion = new Opcion(reader[0].ToString().Trim(),
                                            reader[1].ToString().Trim(),
                                            reader[2].ToString().Trim(),
                                            reader[3].ToString().Trim(),
                                            reader[4].ToString().Trim(),
                                            reader[5].ToString().Trim(),
                                            reader[6].ToString().Trim(),
                                            reader[7].ToString().Trim());

                optionsList.Add(opcion);
            }

            listBox_Opciones.Items.Clear();
            foreach (Opcion option in optionsList.OrderBy(c => c.Name))
            {
                listBox_Opciones.Items.Add(option);
            }
            opcionesList.Clear();
            opcionesList = optionsList;
        }
        private void FillContenidoOpciones()
        {
            foreach (Opcion opcion in opcionesList)
            {
                using SqlConnection conexion = new SqlConnection(Helpers.GetConnectionString());
                conexion.Open();

                using SqlCommand cmd = new SqlCommand(@"SELECT VALOR, TEXTO, FLAGS, ORDEN, INVALID, DESAUTO FROM CONTENIDOOPCIONES WHERE OPCION = '" + opcion.Name + "' ORDER BY ORDEN", conexion);
                using SqlDataReader reader = cmd.ExecuteReader();

                List<ContenidoOpcion> contenidoOpcionList = new List<ContenidoOpcion>();
                while (reader.Read())
                {
                    ContenidoOpcion contenidoOpcion = new ContenidoOpcion(opcion.Name, reader[0].ToString().Trim(), reader[1].ToString().Trim(), 
                                                                            reader[2].ToString().Trim(), reader[3].ToString().Trim(), reader[4].ToString().Trim(), reader[5].ToString().Trim());
                    contenidoOpcionList.Add(contenidoOpcion);
                }

                opcion.ContenidoOpcionesList = contenidoOpcionList;
            }
        }
        private void ConfigurarDataGridView()
        {
            datagrid_ContenidoOpciones.AutoGenerateColumns = false;
            datagrid_ContenidoOpciones.RowHeadersVisible = false;
            // Ajustar la altura de filas (menos espacio vertical)
            datagrid_ContenidoOpciones.RowTemplate.Height = 25;

            var colValor = new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Valor"),
                DataPropertyName = "Valor",
                ReadOnly = true,
                Width = 144
            };

            var colTexto = new DataGridViewTextBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_Texto"),
                DataPropertyName = "Texto",
                ReadOnly = true,
                Width = 144
            };


            var colOcultaEnLista = new DataGridViewCheckBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_OcultaList"),
                DataPropertyName = "OcultaEnLista",
                Width = 90
            };

            var colOcultaEnArbol = new DataGridViewCheckBoxColumn
            {
                HeaderText = LocalizationManager.GetString("L_OcultaArbol"),
                DataPropertyName = "OcultaEnArbol",
                Width = 95
            };

            datagrid_ContenidoOpciones.Columns.AddRange(new DataGridViewColumn[]
            {
                colValor, colTexto, colOcultaEnLista, colOcultaEnArbol
            });

            datagrid_ContenidoOpciones.AllowUserToAddRows = false;
            datagrid_ContenidoOpciones.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }
        private string SaveOpcionesConfig()
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Archivo XML (*.xml)|*.xml";
                    saveFileDialog.Title = "Guardar archivo XML";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Ruta donde guardar el archivo (misma carpeta del .exe)
                        string ruta = saveFileDialog.FileName;

                        // Crear el documento XML
                        var doc = new XDocument(
                            new XElement("Opciones",
                                from opcion in opcionesList
                                select new XElement("Opcion",
                                    new XAttribute("nombre", opcion.Name),
                                    new XAttribute("nivel1", opcion.Nivel1 ?? ""),
                                    new XAttribute("nivel2", opcion.Nivel2 ?? ""),
                                    new XAttribute("nivel3", opcion.Nivel3 ?? ""),
                                    new XAttribute("nivel4", opcion.Nivel4 ?? ""),
                                    new XAttribute("nivel5", opcion.Nivel5 ?? ""),
                                    new XAttribute("flags", opcion.Flags),
                                    from contenido in opcion.ContenidoOpcionesList
                                    select new XElement("ContenidoOpcion",
                                        new XAttribute("valor", contenido.Valor),
                                        new XAttribute("texto", contenido.Texto),
                                        new XAttribute("flags", Helpers.CalcularFlags(contenido)),
                                        new XAttribute("orden", contenido.Orden),
                                        new XAttribute("id", contenido.Id),
                                        new XAttribute("invalid", contenido.Invalid),
                                        new XAttribute("desauto", contenido.DesAuto)
                                    )
                                )
                            )
                        );

                        // Guardar con formato indentado
                        doc.Save(ruta);
                        return ruta;
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Error guardando archivo de configuración: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return String.Empty;
            }

        }

        #endregion

    }
}
