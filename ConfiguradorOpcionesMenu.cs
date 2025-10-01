using RotoEntities;
using System.Data;
using System.Xml.Linq;

namespace RotoTools
{
    public partial class ConfiguradorOpcionesMenu : Form
    {
        #region Constructor
        public ConfiguradorOpcionesMenu()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        private void ConfiguradorOpciones_Load(object sender, EventArgs e)
        {
            InitializeInfoConnection();
        }
        private void btn_ConfigOpciones_Click(object sender, EventArgs e)
        {
            ConfiguradorOpciones configuradorOpcionesForm = new ConfiguradorOpciones();
            configuradorOpcionesForm.ShowDialog();
        }
        private void btn_Restore_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML configuración";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string rutaXml = openFileDialog.FileName;
                RestoreOpcionesDesdeXml(rutaXml);
            }
        }

        #endregion

        #region Private methods
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void RestoreOpcionesDesdeXml(string rutaXml)
        {
            try
            {
                if (!System.IO.File.Exists(rutaXml))
                {
                    MessageBox.Show("Fichero de configuración no encontrado.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                XDocument doc = XDocument.Load(rutaXml);

                var opcionesXml = doc.Descendants("Opcion")
                    .Select(opElem => new Opcion
                    {
                        Name = (string)opElem.Attribute("nombre"),
                        Nivel1 = (string)opElem.Attribute("nivel1"),
                        Nivel2 = (string)opElem.Attribute("nivel2"),
                        Nivel3 = (string)opElem.Attribute("nivel3"),
                        Nivel4 = (string)opElem.Attribute("nivel4"),
                        Nivel5 = (string)opElem.Attribute("nivel5"),
                        Flags = (int?)opElem.Attribute("flags") ?? 0,
                        ContenidoOpcionesList = opElem.Elements("ContenidoOpcion")
                            .Select(c => new ContenidoOpcion
                            {
                                Valor = (string)c.Attribute("valor"),
                                Texto = (string)c.Attribute("texto"),
                                Flags = (int?)c.Attribute("flags") ?? 0,
                                Orden = (int?)c.Attribute("orden") ?? 0,
                                Invalid = (int?)c.Attribute("invalid") ?? 0
                            }).ToList()
                    }).ToList();

                foreach (var opcionXml in opcionesXml)
                {
                    var contenidosDb = Helpers.GetContenidoOpciones(opcionXml.Name);

                    // ¿Falta alguno del XML en BD?
                    bool faltaAlguno = opcionXml.ContenidoOpcionesList
                        .Any(cXml => !contenidosDb.Any(cDb => cDb.Valor.ToUpper().Trim() == cXml.Valor.ToUpper().Trim()));

                    if (faltaAlguno || contenidosDb.Count != opcionXml.ContenidoOpcionesList.Count)
                    {
                        // 1) Copia exacta del XML
                        Helpers.DeleteAllContenidoOpciones(opcionXml.Name);

                        foreach (var contXml in opcionXml.ContenidoOpcionesList.OrderBy(co => co.Orden))
                        {
                            Helpers.InsertContenidoOpcion(opcionXml.Name, contXml);
                        }
                    }
                    else
                    {
                        // 2) Todos están → solo actualizamos Texto y Flags
                        foreach (var contXml in opcionXml.ContenidoOpcionesList)
                        {
                            var contDb = contenidosDb.First(c => c.Valor.ToUpper().Trim() == contXml.Valor.ToUpper().Trim());

                            if (contDb.Texto.ToUpper().Trim() != contXml.Texto.ToUpper().Trim() || contDb.Flags != contXml.Flags)
                            {
                                Helpers.UpdateContenidoOpcion(opcionXml.Name, contXml);
                            }
                        }
                    }
                }

                MessageBox.Show("Opciones restauradas", "",MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error restaurando la configuración: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

    }
}
