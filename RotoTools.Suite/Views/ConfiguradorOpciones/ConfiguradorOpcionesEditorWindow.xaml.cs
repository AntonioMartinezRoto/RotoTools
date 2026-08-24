using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using RotoEntities;

namespace RotoTools.Suite.Views.ConfiguradorOpciones
{
    /// <summary>
    /// Sustituye a ConfiguradorOpciones.cs (WinForms): mismo comportamiento migrado tal cual
    /// (listado de Opciones de la BBDD, filtro por nombre, tabla de ContenidoOpcion editable con
    /// las columnas "oculta en lista"/"oculta en árbol", Guardar a XML + aplicar, y Ejecutar
    /// directo contra la BBDD), reutilizando Helpers y RotoEntities del proyecto original.
    /// </summary>
    public partial class ConfiguradorOpcionesEditorWindow : Window
    {
        private List<Opcion> _opcionesList = new();

        public ConfiguradorOpcionesEditorWindow()
        {
            InitializeComponent();

            InitializeInfoConnection();
            CargarTextos();
            FillOpcionesList();
            FillContenidoOpciones();

            ListaOpciones.ItemsSource = _opcionesList;
        }

        private void InitializeInfoConnection()
        {
            try
            {
                TxtConexion.Text = RotoTools.Helpers.GetServer() + @"\" + RotoTools.Helpers.GetDataBase();
            }
            catch
            {
                TxtConexion.Text = "";
            }
        }

        private void CargarTextos()
        {
            LblFiltrar.Text = RotoTools.LocalizationManager.GetString("L_Buscar");
            Title = RotoTools.LocalizationManager.GetString("L_ConfigurarOpciones");
            TxtTitulo.Text = Title;
            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
            TxtBtnEjecutar.Text = RotoTools.LocalizationManager.GetString("L_Ejecutar");
            ColValor.Header = RotoTools.LocalizationManager.GetString("L_Valor");
            ColTexto.Header = RotoTools.LocalizationManager.GetString("L_Texto");
            ColOcultaEnLista.Header = RotoTools.LocalizationManager.GetString("L_OcultaList");
            ColOcultaEnArbol.Header = RotoTools.LocalizationManager.GetString("L_OcultaArbol");
        }

        private void TxtFiltro_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = TxtFiltro.Text.Trim().ToUpper();

            var filtradas = _opcionesList
                .Where(o => o.Name.ToUpper().Contains(filtro))
                .ToList();

            ListaOpciones.ItemsSource = null;
            ListaOpciones.ItemsSource = filtradas;
        }

        private void ListaOpciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaOpciones.SelectedItem is Opcion opcionSeleccionada)
            {
                GridContenidoOpciones.ItemsSource = opcionSeleccionada.ContenidoOpcionesList;
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string configPath = SaveOpcionesConfig();
                if (!string.IsNullOrEmpty(configPath))
                {
                    RotoTools.Helpers.RestoreOpcionesDesdeXml(configPath);
                    MessageBox.Show(RotoTools.LocalizationManager.GetString("L_GuardadoCorrectamente"), "",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ErrorGuardarConfiguracion") + ex.Message, "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEjecutar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExecuteOpcionesConfig();
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_GuardadoCorrectamente"), "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ErrorGuardarConfiguracion") + ex.Message, "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Lógica portada tal cual de ConfiguradorOpciones.cs (WinForms)

        private void FillOpcionesList()
        {
            List<Opcion> optionsList = new List<Opcion>();
            using SqlConnection conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using SqlCommand cmd = new SqlCommand(@"SELECT NOMBRE, DESCRIPCION, NIVEL1, NIVEL2, NIVEL3, NIVEL4, NIVEL5, FLAGS FROM OPCIONES WHERE NOMBRE LIKE 'RO\_%' ESCAPE '\' ORDER BY NOMBRE", conexion);
            using SqlDataReader reader = cmd.ExecuteReader();

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

            _opcionesList = optionsList.OrderBy(c => c.Name).ToList();
        }

        private void FillContenidoOpciones()
        {
            foreach (Opcion opcion in _opcionesList)
            {
                using SqlConnection conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
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

        private string SaveOpcionesConfig()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivo XML (*.xml)|*.xml",
                    Title = "Guardar archivo XML"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string ruta = saveFileDialog.FileName;

                    var doc = new XDocument(
                        new XElement("Opciones",
                            from opcion in _opcionesList
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
                                    new XAttribute("flags", RotoTools.Helpers.CalcularFlags(contenido)),
                                    new XAttribute("orden", contenido.Orden),
                                    new XAttribute("id", contenido.Id),
                                    new XAttribute("invalid", contenido.Invalid),
                                    new XAttribute("desauto", contenido.DesAuto)
                                )
                            )
                        )
                    );

                    doc.Save(ruta);
                    return ruta;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error guardando archivo de configuración: " + ex.Message, "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private void ExecuteOpcionesConfig()
        {
            foreach (Opcion opcion in _opcionesList)
            {
                foreach (ContenidoOpcion contenidoOpcion in opcion.ContenidoOpcionesList)
                {
                    contenidoOpcion.Flags = RotoTools.Helpers.CalcularFlags(contenidoOpcion);
                    RotoTools.Helpers.UpdateContenidoOpcion(opcion.Name, contenidoOpcion);
                }
            }
        }

        #endregion
    }
}
