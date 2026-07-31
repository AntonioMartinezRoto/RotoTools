using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using RotoEntities;

namespace RotoTools.Suite.Views.ConectorHerraje
{
    /// <summary>
    /// Sustituye a ConectorHerrajeCombinar.cs/.Designer.cs (WinForms): combina varios conectores
    /// de herraje ya guardados en base de datos, bien creando uno nuevo o insertándolos en uno
    /// existente. Reutiliza tal cual RotoTools.Helpers (conexión, (de)serialización XML) y
    /// RotoEntities.Connector/ConnectorNode vía ProjectReference.
    /// </summary>
    public partial class ConectorHerrajeCombinarWindow : Window
    {
        private readonly ObservableCollection<string> _conectoresDisponibles = new();
        private readonly ObservableCollection<string> _conectoresACombinar = new();

        public ConectorHerrajeCombinarWindow()
        {
            InitializeComponent();

            ListDisponibles.ItemsSource = _conectoresDisponibles;
            ListACombinar.ItemsSource = _conectoresACombinar;

            CargarTextos();
            ActualizarInfoConexion();
            CargarConectoresExistentesCombo();

            // Igual que el original no deja ningún RadioButton marcado por defecto (ambos grupos
            // quedan habilitados a la vez hasta que el usuario elige uno). Aquí se prefiere un
            // estado inicial sin ambigüedad: "Crear nuevo conector" marcado desde el principio.
            RbNuevoConector.IsChecked = true;
        }

        #region Localización / cabecera

        private void CargarTextos()
        {
            Title = RotoTools.LocalizationManager.GetString("L_CombinarConectores");
            TxtTitulo.Text = Title;

            RbNuevoConector.Content = RotoTools.LocalizationManager.GetString("L_CrearConector");
            LblNombreConector.Text = RotoTools.LocalizationManager.GetString("L_Nombre");
            ChkPredefinido.Content = RotoTools.LocalizationManager.GetString("L_PonerPredefinido");

            RbInsertarExistente.Content = RotoTools.LocalizationManager.GetString("L_InsertarEnConector");
            LblSeleccionarConector.Text = RotoTools.LocalizationManager.GetString("L_Nombre");

            LblConectoresBD.Text = RotoTools.LocalizationManager.GetString("L_ConectoresBBDD");
            LblConectoresCombinar.Text = RotoTools.LocalizationManager.GetString("L_ConectoresCombinar");
            TxtBtnGuardar.Text = RotoTools.LocalizationManager.GetString("L_Guardar");
        }

        private void ActualizarInfoConexion()
        {
            string servidor = RotoTools.Helpers.GetServer();
            string baseDatos = RotoTools.Helpers.GetDataBase();
            string conectorActivo = RotoTools.Helpers.GetConectorActivo() ?? "";
            LblInfoConexion.Text = $@"{servidor}\{baseDatos}    ·    " +
                RotoTools.Suite.Services.SuiteLocalization.GetString("L_Suite_ConectorActivo") + ": " + conectorActivo;
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => Close();

        #endregion

        #region Modo (crear nuevo / insertar en existente)

        /// <summary>Igual que CambiarModo (ConectorHerrajeCombinar.cs): habilita el panel del modo
        /// elegido y, al pasar a "crear nuevo", limpia el combo de conector destino y vacía la
        /// lista de combinar (empezar de cero). Siempre se recarga la lista de disponibles para
        /// reflejar el nuevo estado (ver comentario de la mejora deliberada al inicio del XAML).</summary>
        private void ModoCombinar_Changed(object sender, RoutedEventArgs e)
        {
            bool nuevo = RbNuevoConector.IsChecked == true;
            PanelCrearNuevo.IsEnabled = nuevo;
            PanelExistente.IsEnabled = !nuevo;

            if (nuevo)
            {
                CmbConectorExistente.SelectedIndex = -1;
                _conectoresACombinar.Clear();
            }

            CargarListaDisponibles();
        }

        private void CmbConectorExistente_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => CargarListaDisponibles();

        #endregion

        #region Listas de transferencia

        private void CargarConectoresExistentesCombo()
        {
            var items = new List<string>();
            using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            {
                conexion.Open();
                using var cmd = new SqlCommand("SELECT Codigo, XML FROM ConectorHerrajes", conexion);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) items.Add(reader[0].ToString());
            }

            CmbConectorExistente.ItemsSource = items;
        }

        /// <summary>Igual que CargarListaConectores, con la mejora comentada arriba: excluye tanto
        /// el conector destino (combo) como los que ya están en "Conectores a combinar".</summary>
        private void CargarListaDisponibles()
        {
            var conectoresList = new List<string>();
            using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
            {
                conexion.Open();
                using var cmd = new SqlCommand("SELECT Codigo, XML FROM ConectorHerrajes", conexion);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) conectoresList.Add(reader[0].ToString());
            }

            string? objetivo = CmbConectorExistente.SelectedItem as string;

            var disponibles = conectoresList
                .Where(c => c != objetivo && !_conectoresACombinar.Contains(c))
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase);

            _conectoresDisponibles.Clear();
            foreach (var c in disponibles) _conectoresDisponibles.Add(c);
        }

        /// <summary>listBox_AllConectores tenía Sorted=true en el original: cualquier alta vuelve
        /// a insertarse en orden alfabético, no simplemente al final.</summary>
        private void AgregarADisponibles(string item)
        {
            int index = 0;
            while (index < _conectoresDisponibles.Count &&
                   string.Compare(_conectoresDisponibles[index], item, StringComparison.OrdinalIgnoreCase) < 0)
                index++;
            _conectoresDisponibles.Insert(index, item);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ListDisponibles.SelectedItem is not string item) return;
            _conectoresACombinar.Add(item);
            _conectoresDisponibles.Remove(item);
        }

        private void BtnAddAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _conectoresDisponibles.ToList())
                _conectoresACombinar.Add(item);
            _conectoresDisponibles.Clear();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ListACombinar.SelectedItem is not string item) return;
            AgregarADisponibles(item);
            _conectoresACombinar.Remove(item);
        }

        private void BtnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _conectoresACombinar.ToList())
                AgregarADisponibles(item);
            _conectoresACombinar.Clear();
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (ListACombinar.SelectedItem is not string item) return;
            int index = _conectoresACombinar.IndexOf(item);
            if (index <= 0) return;
            _conectoresACombinar.Move(index, index - 1);
            ListACombinar.SelectedIndex = index - 1;
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (ListACombinar.SelectedItem is not string item) return;
            int index = _conectoresACombinar.IndexOf(item);
            if (index < 0 || index >= _conectoresACombinar.Count - 1) return;
            _conectoresACombinar.Move(index, index + 1);
            ListACombinar.SelectedIndex = index + 1;
        }

        #endregion

        #region Guardar (equivalente a bnt_Guardar_Click / CrearNuevoConector / InsertarEnConectorExistente)

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (RbNuevoConector.IsChecked == true) CrearNuevoConector();
            else InsertarEnConectorExistente();
        }

        private void CrearNuevoConector()
        {
            if (string.IsNullOrEmpty(TxtConectorName.Text))
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_AsignarNombreConector"), "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_conectoresACombinar.Count == 0) return;

            var conectorFinal = new Connector { Message = "true", Nodes = new List<ConnectorNode>() };

            foreach (var codigo in _conectoresACombinar)
            {
                var fusion = CargarConectorDesdeBaseDeDatos(codigo);
                if (fusion?.Nodes != null) conectorFinal.Nodes.AddRange(fusion.Nodes);
            }

            string xml = RotoTools.Helpers.SerializarXml(conectorFinal);
            GuardarConectorEnBD(TxtConectorName.Text, xml);
        }

        private void InsertarEnConectorExistente()
        {
            if (CmbConectorExistente.SelectedItem is not string objetivo)
            {
                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_SeleccioneConector"), "",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var conectorFinal = CargarConectorDesdeBaseDeDatos(objetivo);
            if (conectorFinal == null) return;

            foreach (var codigo in _conectoresACombinar)
            {
                var fusion = CargarConectorDesdeBaseDeDatos(codigo);
                if (fusion?.Nodes != null) conectorFinal.Nodes.AddRange(fusion.Nodes);
            }

            string xml = RotoTools.Helpers.SerializarXml(conectorFinal);
            ActualizarConectorExistente(objetivo, xml);
        }

        private Connector? CargarConectorDesdeBaseDeDatos(string conectorName)
        {
            try
            {
                string? xmlString = null;

                using (var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                {
                    conexion.Open();
                    using var cmd = new SqlCommand("SELECT XML FROM ConectorHerrajes WHERE Codigo = @codigo", conexion);
                    cmd.Parameters.AddWithValue("@codigo", conectorName);

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                        xmlString = result.ToString();
                }

                return !string.IsNullOrWhiteSpace(xmlString)
                    ? RotoTools.Helpers.DeserializarXML<Connector>(xmlString)
                    : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (40): " + Environment.NewLine + ex.Message);
                return null;
            }
        }

        private void GuardarConectorEnBD(string conectorName, string xml)
        {
            try
            {
                string sql = @"INSERT INTO ConectorHerrajes (DataVerId, Codigo, XML) VALUES (dbo.GetCurrentDVID(), @Codigo, @Xml);";

                if (ExisteConectorEnBD(conectorName))
                {
                    if (MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ExisteConector"), "",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        sql = @"UPDATE ConectorHerrajes SET XML = @Xml Where Codigo = @Codigo;";
                    }
                    else
                    {
                        return;
                    }
                }

                if (ChkPredefinido.IsChecked == true && !string.IsNullOrEmpty(conectorName))
                {
                    sql += @"UPDATE VARIABLESGLOBALES SET VALOR = '" + conectorName + "' WHERE NOMBRE = 'Conector Herraje';";
                }

                using (var connection = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Codigo", conectorName);
                    command.Parameters.AddWithValue("@Xml", xml);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ConectorInsertado"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (41): " + Environment.NewLine + ex.Message);
            }
        }

        private void ActualizarConectorExistente(string conectorName, string xml)
        {
            try
            {
                const string sql = @"UPDATE ConectorHerrajes SET XML = @Xml Where Codigo = @Codigo;";

                using (var connection = new SqlConnection(RotoTools.Helpers.GetConnectionString()))
                using (var command = new SqlCommand(sql, connection))
                {
                    connection.Open();
                    command.Parameters.AddWithValue("@Codigo", conectorName);
                    command.Parameters.AddWithValue("@Xml", xml);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show(RotoTools.LocalizationManager.GetString("L_ConectorActualizado"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error (42): " + Environment.NewLine + ex.Message);
            }
        }

        private bool ExisteConectorEnBD(string conectorName)
        {
            using var conexion = new SqlConnection(RotoTools.Helpers.GetConnectionString());
            conexion.Open();

            using var cmd = new SqlCommand("SELECT Count(*) FROM ConectorHerrajes WHERE Codigo = '" + conectorName + "'", conexion);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                return Convert.ToInt32(reader[0]?.ToString()) > 0;

            return false;
        }

        #endregion
    }
}
