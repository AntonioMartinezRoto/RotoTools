using NPOI.SS.Formula.Functions;
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
            CargarTextos();
        }
        private void btn_ConfigOpciones_Click(object sender, EventArgs e)
        {
            ConfiguradorOpciones configuradorOpcionesForm = new ConfiguradorOpciones();
            configuradorOpcionesForm.ShowDialog();
        }
        private void btn_ImportConfigCliente_Click(object sender, EventArgs e)
        {
            SetOptionsVisibleFromCliente();
        }

        private void btn_Restore_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "XML Files (*.xml)|*.xml";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string rutaXml = openFileDialog.FileName;
                    Helpers.RestoreOpcionesDesdeXml(rutaXml);
                    MessageBox.Show(LocalizationManager.GetString("L_ConfiguracionRestaurada"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationManager.GetString("L_ErrorRestaurandoConfiguracion") + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Private methods
        private void InitializeInfoConnection()
        {
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void CargarTextos()
        {
            lbl_ConfigOpciones.Text = LocalizationManager.GetString("L_ConfigurarGuardarOpciones");
            this.Text = LocalizationManager.GetString("L_MenuConfigurarOpciones");
            lbl_RestoreOptions.Text = LocalizationManager.GetString("L_RestaurarOpciones");
        }

        private void SetOptionsVisibleFromCliente()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Rotoconfig (*.rotoconfig)|*.rotoconfig";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string rutaXml = openFileDialog.FileName;
                    SetOptionsVisibleFromCliente(rutaXml);
                    MessageBox.Show(LocalizationManager.GetString("L_ConfiguracionImportada"), "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(LocalizationManager.GetString("L_ErrorImportandoConfiguracion") + Environment.NewLine + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetOptionsVisibleFromCliente(string rutaXml)
        {
            XDocument doc = XDocument.Load(rutaXml);

            var valoresTipoPerfil = doc.Descendants("TiposPerfil")
                .Select(opElem => new Opcion
                {
                    Name = "RO_TIPOPERFIL",
                    ContenidoOpcionesList = opElem.Elements("TipoPerfil")
                        .Select(c => new ContenidoOpcion
                        {
                            Valor = ((string?)c.Attribute("Nombre"))?.Trim() ?? string.Empty
                        }).ToList()
                }).ToList();

            var valoresPerfiles = doc.Descendants("Perfiles")
                .Select(opElem => new Opcion
                {
                    Name = "RO_1PERFIL",
                    ContenidoOpcionesList = opElem.Elements("Perfil")
                        .Select(c => new ContenidoOpcion
                        {
                            Valor = ((string?)c.Attribute("Nombre"))?.Trim() ?? string.Empty,
                            Texto = ((string?)c.Attribute("Tipo"))?.Trim() ?? string.Empty
                        }).ToList()
                }).ToList();

            var valoresCerradura = doc.Descendants("CerradurasPuerta")
                .Select(opElem => new Opcion
                {
                    Name = "RO_PU_CERRADURA PUERTA",
                    ContenidoOpcionesList = opElem.Elements("Cerradura")
                        .Select(c => new ContenidoOpcion
                        {
                            Valor = ((string?)c.Attribute("Nombre"))?.Trim() ?? string.Empty
                        }).ToList()
                }).ToList();

            var valoresBisagrasPuerta = doc.Descendants("BisagrasPuerta")
                    .Select(opElem => new Opcion
                    {
                        Name = "RO_PU_BISAGRA",
                        ContenidoOpcionesList = opElem.Elements("Bisagra")
                            .Select(c => new ContenidoOpcion
                            {
                                Valor = ((string?)c.Attribute("Nombre"))?.Trim() ?? string.Empty
                            }).ToList()
                    }).ToList();

            var valoresSoporteCompas = doc.Descendants("SoporteCompas")
                    .Select(opElem => new Opcion
                    {
                        Name = "RO_NX_SOPORTE COMPAS P",
                        ContenidoOpcionesList = opElem.Elements("Soporte")
                            .Select(c => new ContenidoOpcion
                            {
                                Valor = ((string?)c.Attribute("Nombre"))?.Trim() ?? string.Empty
                            }).ToList()
                    }).ToList();

            var opcionAgujasGlobal = new Opcion
            {
                Name = "RO_AGUJA",
                ContenidoOpcionesList = doc.Descendants("Aguja")
                    .Select(c => new ContenidoOpcion
                    {
                        Valor = "Ag" +((string?)c.Attribute("Nombre"))?.Trim() ?? string.Empty
                    }).ToList()
            };
            opcionAgujasGlobal.ContenidoOpcionesList.Add(new ContenidoOpcion("RO_AGUJA", "Ag8"));
            opcionAgujasGlobal.ContenidoOpcionesList.Add(new ContenidoOpcion("RO_AGUJA", "Ag15"));

            var opcionPasivaGlobal = new Opcion
            {
                Name = "RO_NX_HERR. HOJA PASIVA",
                ContenidoOpcionesList = doc.Descendants("Pasiva")
                    .Select(c => new ContenidoOpcion
                    {
                        Valor = ((string?)c.Attribute("Nombre"))?.Trim() ?? string.Empty
                    }).ToList()
            };

            UpdateValores(valoresCerradura.First().ContenidoOpcionesList, "RO_PU_CERRADURA PUERTA");
            UpdateValores(valoresBisagrasPuerta.First().ContenidoOpcionesList, "RO_PU_BISAGRA");
            UpdateValores(valoresSoporteCompas.First().ContenidoOpcionesList, "RO_NX_SOPORTE COMPAS P");
            UpdateValores(opcionAgujasGlobal.ContenidoOpcionesList, "RO_AGUJA");
            UpdateValores(opcionPasivaGlobal.ContenidoOpcionesList, "RO_NX_HERR. HOJA PASIVA");
            foreach (var tipoPerfil in valoresTipoPerfil.First().ContenidoOpcionesList)
            {
                UpdateValoresPerfiles(valoresTipoPerfil.First().ContenidoOpcionesList, valoresPerfiles.First().ContenidoOpcionesList, tipoPerfil.Valor, tipoPerfil.Valor == "PVC" ? "RO_1PERFIL" : "RO_1PERFIL_ALU");
            }
            
        }
        private void UpdateValores(List<ContenidoOpcion> valoresConfig, string rotoOptionName)
        {
            List<ContenidoOpcion> contenidoOpcionDbList = Helpers.GetContenidoOpciones(rotoOptionName);

            // 1. Extraemos los valores a un HashSet para optimizar la búsqueda (ignora mayúsculas/minúsculas si es necesario)
            var valoresConfigSet = valoresConfig
                .Where(o => o.Valor != null)
                .Select(o => o.Valor.Trim().ToUpper())
                .ToHashSet();

            // 2. Filtramos la lista de la BBDD
            List<ContenidoOpcion> contenidoOpcionToUpdate = contenidoOpcionDbList
                .Where(c => c.Valor != null
                         && c.Valor.Trim().ToUpper() != "OCULTO"
                         && !valoresConfigSet.Contains(c.Valor.Trim().ToUpper()))
                .ToList();

            // 3. Actualizamos solo los registros necesarios
            foreach (var contenidoOpcion in contenidoOpcionToUpdate)
            {
                Helpers.UpdateFlagsContenidoOpcion(rotoOptionName, contenidoOpcion.Valor, 3);
            }
        }

        private void UpdateValoresPerfiles(List<ContenidoOpcion> valoresTipoPerfil, List<ContenidoOpcion> valoresPerfiles, string tipoPerfil, string nombreOpcion)
        {
            bool tieneTipoPerfil = valoresPerfiles.Any(c => c.Texto.Trim().ToUpper() == tipoPerfil.Trim().ToUpper());

            if (tieneTipoPerfil)
            {
                var perfilesXmlSet = valoresPerfiles
                    .Where(p => p.Texto == tipoPerfil)
                    .Select(p => p.Valor.Trim().ToUpper() ?? string.Empty)
                    .ToHashSet();

                List<ContenidoOpcion> contenidoOpcionDbList = Helpers.GetContenidoOpciones(nombreOpcion);

                foreach (var contenidoOpcion in contenidoOpcionDbList)
                {
                    foreach (var perfil in perfilesXmlSet)
                    {
                        if (!contenidoOpcion.Valor.Trim().ToUpper().Contains(perfil))
                        {
                            Helpers.UpdateFlagsContenidoOpcion(nombreOpcion, contenidoOpcion.Valor, 3);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
