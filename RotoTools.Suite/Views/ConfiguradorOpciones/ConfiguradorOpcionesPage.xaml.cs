using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Win32;
using RotoEntities;
using RotoTools.Suite.Services;

namespace RotoTools.Suite.Views.ConfiguradorOpciones
{
    /// <summary>
    /// Página "Configurador de opciones" de la suite: sustituye a ConfiguradorOpcionesMenu.cs
    /// (WinForms). Mismas 3 acciones, mismo comportamiento, migrado tal cual:
    ///  1) Abrir el editor completo de opciones (ConfiguradorOpcionesEditorWindow).
    ///  2) Restaurar una configuración guardada previamente en un XML.
    ///  3) Importar la configuración de opciones visibles a partir de un .rotoconfig de cliente.
    /// Toda la lógica de negocio (Helpers, RotoEntities.Opcion/ContenidoOpcion) se reutiliza sin
    /// cambios desde el proyecto RotoTools original, vía la referencia de proyecto.
    /// </summary>
    public partial class ConfiguradorOpcionesPage : UserControl
    {
        public ConfiguradorOpcionesPage()
        {
            InitializeComponent();
            CargarTextos();
        }

        private void CargarTextos()
        {
            // Mismo texto que en el panel lateral (L_ConfiguradorOpciones), para que el título de
            // esta página y el del menú de navegación coincidan siempre y no líen al usuario.
            TxtTitulo.Text = RotoTools.LocalizationManager.GetString("L_ConfiguradorOpciones");
            TxtSubtitulo.Text = SuiteLocalization.GetString("L_Suite_ConfigOpcionesSubtitulo");
            TxtCard1Titulo.Text = RotoTools.LocalizationManager.GetString("L_ConfigurarGuardarOpciones");
            TxtCard2Titulo.Text = RotoTools.LocalizationManager.GetString("L_RestaurarOpciones");
            TxtCard3Titulo.Text = RotoTools.LocalizationManager.GetString("L_ImportarConfigCliente");
        }

        private void BtnConfigOpciones_Click(object sender, RoutedEventArgs e)
        {
            var editor = new ConfiguradorOpcionesEditorWindow
            {
                Owner = Window.GetWindow(this)
            };
            editor.ShowDialog();
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string rutaXml = openFileDialog.FileName;
                    RotoTools.Helpers.RestoreOpcionesDesdeXml(rutaXml);
                    MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ConfiguracionRestaurada"), "",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ErrorRestaurandoConfiguracion") + Environment.NewLine + ex.Message, "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImportConfigCliente_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Rotoconfig (*.rotoconfig)|*.rotoconfig"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string rutaXml = openFileDialog.FileName;
                    SetOptionsVisibleFromCliente(rutaXml);
                    MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ConfiguracionImportada"), "",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ErrorImportandoConfiguracion") + Environment.NewLine + ex.Message, "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Lógica portada tal cual de ConfiguradorOpcionesMenu.cs (WinForms)

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
                        Valor = "Ag" + ((string?)c.Attribute("Nombre"))?.Trim() ?? string.Empty
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
            List<ContenidoOpcion> contenidoOpcionDbList = RotoTools.Helpers.GetContenidoOpciones(rotoOptionName);

            var valoresConfigSet = valoresConfig
                .Where(o => o.Valor != null)
                .Select(o => o.Valor.Trim().ToUpper())
                .ToHashSet();

            List<ContenidoOpcion> contenidoOpcionToUpdate = contenidoOpcionDbList
                .Where(c => c.Valor != null
                         && c.Valor.Trim().ToUpper() != "OCULTO"
                         && !valoresConfigSet.Contains(c.Valor.Trim().ToUpper()))
                .ToList();

            foreach (var contenidoOpcion in contenidoOpcionToUpdate)
            {
                RotoTools.Helpers.UpdateFlagsContenidoOpcion(rotoOptionName, contenidoOpcion.Valor, 3);
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

                List<ContenidoOpcion> contenidoOpcionDbList = RotoTools.Helpers.GetContenidoOpciones(nombreOpcion);

                foreach (var contenidoOpcion in contenidoOpcionDbList)
                {
                    foreach (var perfil in perfilesXmlSet)
                    {
                        if (!contenidoOpcion.Valor.Trim().ToUpper().Contains(perfil))
                        {
                            RotoTools.Helpers.UpdateFlagsContenidoOpcion(nombreOpcion, contenidoOpcion.Valor, 3);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
