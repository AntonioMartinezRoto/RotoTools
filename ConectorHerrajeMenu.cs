using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static RotoTools.Enums;

namespace RotoTools
{
    public partial class ConectorHerrajeMenu : Form
    {
        #region Private Properties
        private int HardwareType { get; set; }
        private bool xmlCargado = false;
        private XmlData xmlOrigen = new();
        private bool conectorCargado = false;
        private Connector connectorHerraje = new();
        private XmlNamespaceManager nsmgr;

        #endregion

        #region Constructors
        public ConectorHerrajeMenu()
        {
            InitializeComponent();

        }

        #endregion

        #region Events
        private void ConectorHerrajeMenu_Load(object sender, EventArgs e)
        {
            CargarDatos();
        }
        private void btn_LoadXml_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                EnableButtons(false);
                string rutaXml = openFileDialog.FileName;
                xmlOrigen = LoadXml(rutaXml);
                lbl_Xml.Text = rutaXml;
                EnableButtons(true);
            }
        }
        private void btn_Actualizar_Click(object sender, EventArgs e)
        {
            CargarDatos();
        }
        private void btn_GeneraConector_Click(object sender, EventArgs e)
        {
            if (xmlCargado)
            {
                if (TranslateManager.PermitirTraduccionesEnConectorEscandallos)
                {
                    if (MessageBox.Show(LocalizationManager.GetString("L_AplicarPlantillaTraduccion"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "XLS Files (*.xls)|*.xlsx";

                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            EnableButtons(false);

                            TranslateManager.AplicarTraduccion = true;
                            TranslateManager.TraduccionesActuales = Helpers.CargarTraducciones(openFileDialog.FileName);

                            EnableButtons(true);
                            Cursor.Current = Cursors.Default;                            
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        TranslateManager.AplicarTraduccion = false;
                    }
                }


                List<Set> setsConector = GetSetsToConnector();
                ConectorHerrajeGenerador generaConectorForm = new ConectorHerrajeGenerador(xmlOrigen, setsConector, xmlOrigen.Supplier);

                generaConectorForm.ShowDialog();
            }
        }
        private void btn_CombinarConectores_Click(object sender, EventArgs e)
        {
            ConectorHerrajeCombinar conectorHerrajeCombinarForm = new ConectorHerrajeCombinar();
            conectorHerrajeCombinarForm.ShowDialog();
        }
        private void btn_SetsNoUtilizados_Click(object sender, EventArgs e)
        {
            if (xmlCargado)
            {
                ConectorHerrajeRevisionSets conectorHerrajeRevisionSetsForm = new ConectorHerrajeRevisionSets(xmlOrigen);
                conectorHerrajeRevisionSetsForm.ShowDialog();
            }
        }
        #endregion

        #region Private Methods

        private void CargarDatos()
        {
            InitializeInfoConnection();
            if (Helpers.IsVersionPrefSuiteCompatible())
            {
                EnableButtons(true);
            }
            else
            {
                EnableButtons(false);
                ShowVersionNoCompatible();
            }
        }
        private void InitializeInfoConnection()
        {
            statusStrip1.BackColor = System.Drawing.Color.Transparent;
            lbl_Conexion.Text = Helpers.GetServer() + @"\" + Helpers.GetDataBase();
        }
        private void EnableButtons(bool enable)
        {
            btn_LoadXml.Enabled = enable;
            btn_SetsNoUtilizados.Enabled = enable;
            btn_GeneraConector.Enabled = enable;
            btn_CombinarConectores.Enabled = enable;
        }
        private void ShowVersionNoCompatible()
        {
            statusStrip1.BackColor = System.Drawing.Color.IndianRed;
            lbl_Conexion.Text += "   BASE DE DATOS NO COMPATIBLE (v2020 requerida)";
        }
        private XmlData LoadXml(string xmlPath)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);

                nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

                XmlLoader loader = new XmlLoader(nsmgr);
                // Vinculamos el evento para actualizar la Label del formulario
                loader.OnLoadingInfo += (type, value) =>
                {
                    lbl_Xml.Visible = true;
                    lbl_Xml.Text = $"Cargando... {type} {value.TrimEnd()}";
                    Application.DoEvents(); // refresca la UI
                };

                XmlData xmlData = new XmlData();
                xmlData.Supplier = loader.LoadSupplier(doc);
                xmlData.HardwareType = loader.LoadHardwareType(xmlData.Supplier);
                this.HardwareType = xmlData.HardwareType;
                xmlData.FittingGroupList = loader.LoadFittingGroups(doc);
                xmlData.ColourList = loader.LoadColourMaps(doc);
                xmlData.OptionList = loader.LoadDocOptions(doc);
                xmlData.FittingList = loader.LoadFittings(doc);
                xmlData.SetList = loader.LoadSets(doc, xmlData.FittingList);

                xmlCargado = true;
                return xmlData;
            }
            catch
            {
                return null;
            }

        }
        private List<Set> GetSetsToConnector()
        {
            List<Set> setList = new List<Set>();

            if (this.HardwareType == (int)enumHardwareType.PVC)
            {
                setList = GetSetsToConnectorPVC();
            }
            else if (this.HardwareType == (int)enumHardwareType.Aluminio)
            {
                setList = GetSetsToConnectorAluminio();
            }
            else if (this.HardwareType == (int)enumHardwareType.PAX)
            {
                setList = GetSetsToConnectorPAX();
            }
            return setList;
        }


        #endregion

        #region Get Sets to Connector
        private List<Set> GetSetsToConnectorPVC()
        {
            List<Set> setList = new List<Set>();

            #region COTA FIJA 

            #region VENTANAS

            List<Set> sets1HVentanaOsciloBatienteActivaCotaFija = GetSetsCF1HActivaVentanaOsciloBatiente();

            if (sets1HVentanaOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V1H OSCILOBATIENTE"));
                setList.AddRange(sets1HVentanaOsciloBatienteActivaCotaFija);
            }

            List<Set> sets1HVentanaPracticableActivaCotaFija = GetSetsCF1HActivaVentanaPracticable();

            if (sets1HVentanaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V1H PRACTICABLE"));
                setList.AddRange(sets1HVentanaPracticableActivaCotaFija);
            }

            List<Set> sets2HVentanaOsciloBatienteActivaCotaFija = GetSetsCF2HActivaVentanaOsciloBatiente();
            if (sets2HVentanaOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H OSCILOBATIENTE"));
                setList.AddRange(sets2HVentanaOsciloBatienteActivaCotaFija);
            }

            List<Set> sets2HVentanaPracticableActivaCotaFija = GetSetsCF2HActivaVentanaPracticable();
            if (sets2HVentanaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H PRACTICABLE"));
                setList.AddRange(sets2HVentanaPracticableActivaCotaFija);
            }


            List<Set> sets2HVentanaPasivaCotaFija = GetSetsCF2HPasivaVentanaPracticable();
            if (sets2HVentanaPasivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H PASIVA"));
                setList.AddRange(sets2HVentanaPasivaCotaFija);
            }

            #endregion

            #region BALCONERAS
            List<Set> sets1HBalconeraOsciloBatienteActivaCotaFija = GetSetsCF1HActivaBalconeraOsciloBatiente();
            if (sets1HBalconeraOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B1H OSCILOBATIENTE"));
                setList.AddRange(sets1HBalconeraOsciloBatienteActivaCotaFija);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaFija = GetSetsCF1HActivaBalconeraPracticable();
            if (sets1HBalconeraPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B1H PRACTICABLE"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaFija);
            }

            List<Set> sets2HBalconeraOsciloBatienteActivaCotaFija = GetSetsCF2HActivaBalconeraOsciloBatiente();
            if (sets2HBalconeraOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B2H OSCILOBATIENTE"));
                setList.AddRange(sets2HBalconeraOsciloBatienteActivaCotaFija);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaFija = GetSetsCF2HActivaBalconeraPracticable();
            if (sets2HBalconeraPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B2H PRACTICABLE"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaFija);
            }

            List<Set> sets2HBalconeraPasivaCotaFija = GetSetsCF2HPasivaBalconeraPracticable();
            if (sets2HBalconeraPasivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR B2H PASIVA"));
                setList.AddRange(sets2HBalconeraPasivaCotaFija);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaFijaAperturaExterior = GetSetsCF1HActivaBalconeraPracticableAperturaExterior();
            if (sets1HBalconeraPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B1H APERTURA EXTERIOR"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaFijaAperturaExterior = GetSetsCF2HActivaBalconeraPracticableAperturaExterior();
            if (sets2HBalconeraPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B2H APERTURA EXTERIOR"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior = GetSetsCF2HPasivaBalconeraPracticableAperturaExterior();
            if (sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B2H PASIVA APERTURA EX"));
                setList.AddRange(sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior);
            }

            #endregion

            #region PUERTAS SECUNDARIAS
            List<Set> sets1HPuertaSecundariaPracticableActivaCotaFija = GetSetsCF1HActivaPuertaSecundariaPracticable();
            if (sets1HPuertaSecundariaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 1H"));
                setList.AddRange(sets1HPuertaSecundariaPracticableActivaCotaFija);
            }

            List<Set> sets2HPuertaSecundariaPracticableActivaCotaFija = GetSetsCF2HActivaPuertaSecundariaPracticable();
            if (sets1HPuertaSecundariaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 2H"));
                setList.AddRange(sets2HPuertaSecundariaPracticableActivaCotaFija);
            }

            List<Set> sets2HPuertaSecundariaPracticablePasivaCotaFija = GetSetsCF2HPasivaPuertaSecundariaPracticable();
            if (sets2HPuertaSecundariaPracticablePasivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 2H PASIVA"));
                setList.AddRange(sets2HPuertaSecundariaPracticablePasivaCotaFija);
            }

            List<Set> sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior = GetSetsCF1HActivaPuertaSecundariaPracticableAperturaExterior();
            if (sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 1H A.E."));
                setList.AddRange(sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior = GetSetsCF2HActivaPuertaSecundariaPracticableAperturaExterior();
            if (sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 2H A.E."));
                setList.AddRange(sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior = GetSetsCF2HPasivaPuertaSecundariaPracticableAperturaExterior();
            if (sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 2H PASIVA A.E."));
                setList.AddRange(sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior);
            }

            #endregion

            #region PUERTAS

            List<Set> sets1HPuertaCotaFija = GetSetsCF1HActivaPuerta();
            if (sets1HPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P1H"));
                setList.AddRange(sets1HPuertaCotaFija);
            }

            List<Set> sets2HActivaPuertaCotaFija = GetSetsCF2HActivaPuerta();
            if (sets2HActivaPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P2H"));
                setList.AddRange(sets2HActivaPuertaCotaFija);
            }

            List<Set> sets2HPasivaPuertaCotaFija = GetSetsCF2HPasivaPuerta();
            if (sets2HPasivaPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P2H PASIVA"));
                setList.AddRange(sets2HPasivaPuertaCotaFija);
            }

            List<Set> sets1HPuertaCotaFijaAperturaExterior = GetSetsCF1HPuertaAperturaExterior();
            if (sets1HPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P1H APERTURA EXT"));
                setList.AddRange(sets1HPuertaCotaFijaAperturaExterior);
            }

            List<Set> sets2HActivaPuertaCotaFijaAperturaExterior = GetSetsCF2HActivaPuertaAperturaExterior();
            if (sets2HActivaPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P2H APERTURA EXT"));
                setList.AddRange(sets2HActivaPuertaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPasivaPuertaCotaFijaAperturaExterior = GetSetsCF2HPasivaPuertaAperturaExterior();
            if (sets2HPasivaPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P2H PASIVA APERTURA EXT"));
                setList.AddRange(sets2HPasivaPuertaCotaFijaAperturaExterior);
            }
            #endregion

            #region CORREDERAS

            List<Set> setsCorrederaCotaFija = GetSetsCFCorredera();

            if (setsCorrederaCotaFija.Any())
            {
                setList.Add(new Set("CF CORREDERA"));
                setList.AddRange(setsCorrederaCotaFija);
            }

            #endregion

            #region PATIO LIFT

            List<Set> setsPatioLiftCotaFija = GetSetsCFPatioLift();

            if (setsCorrederaCotaFija.Any())
            {
                setList.Add(new Set("CF PATIO LIFT"));
                setList.AddRange(setsPatioLiftCotaFija);
            }

            #endregion

            #region PLEGABLES

            List<Set> setsPlegablesCotaFija = GetSetsCFPlegables();

            if (setsPlegablesCotaFija.Any())
            {
                setList.Add(new Set("CF PLEGABLE"));
                setList.AddRange(setsPlegablesCotaFija);
            }

            #endregion

            #endregion

            #region COTA VARIABLE

            #region ABATIBLES

            List<Set> setsAbatiblesCotaVariable = GetSetsCVAbatibles();

            if (setsAbatiblesCotaVariable.Any())
            {
                setList.Add(new Set("CV ABATIBLES"));
                setList.AddRange(setsAbatiblesCotaVariable);
            }


            #endregion

            #region VENTANAS

            List<Set> sets1HVentanaOsciloBatienteActivaCotaVariable = GetSetsCV1HActivaVentanaOsciloBatiente();

            if (sets1HVentanaOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V1H OSCILOBATIENTE"));
                setList.AddRange(sets1HVentanaOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets1HVentanaPracticableActivaCotaVariable = GetSetsCV1HActivaVentanaPracticable();

            if (sets1HVentanaPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V1H PRACTICABLE"));
                setList.AddRange(sets1HVentanaPracticableActivaCotaVariable);
            }

            List<Set> sets2HVentanaOsciloBatienteActivaCotaVariable = GetSetsCV2HActivaVentanaOsciloBatiente();
            if (sets2HVentanaOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H OSCILOBATIENTE"));
                setList.AddRange(sets2HVentanaOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets2HVentanaPracticableActivaCotaVariable = GetSetsCV2HActivaVentanaPracticable();
            if (sets2HVentanaPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H PRACTICABLE"));
                setList.AddRange(sets2HVentanaPracticableActivaCotaVariable);
            }


            List<Set> sets2HVentanaPasivaCotaVariable = GetSetsCV2HPasivaVentanaPracticable();
            if (sets2HVentanaPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H PASIVA"));
                setList.AddRange(sets2HVentanaPasivaCotaVariable);
            }

            #endregion

            #region BALCONERAS

            List<Set> sets1HBalconeraOsciloBatienteActivaCotaVariable = GetSetsCV1HActivaBalconeraOsciloBatiente();
            if (sets1HBalconeraOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B1H OSCILOBATIENTE"));
                setList.AddRange(sets1HBalconeraOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaVariable = GetSetsCV1HActivaBalconeraPracticable();
            if (sets1HBalconeraPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B1H PRACTICABLE"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaVariable);
            }

            List<Set> sets2HBalconeraOsciloBatienteActivaCotaVariable = GetSetsCV2HActivaBalconeraOsciloBatiente();
            if (sets2HBalconeraOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H OSCILOBATIENTE"));
                setList.AddRange(sets2HBalconeraOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaVariable = GetSetsCV2HActivaBalconeraPracticable();
            if (sets2HBalconeraPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H PRACTICABLE"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaVariable);
            }

            List<Set> sets2HBalconeraPasivaCotaVariable = GetSetsCV2HPasivaBalconeraPracticable();
            if (sets2HBalconeraPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H PASIVA"));
                setList.AddRange(sets2HBalconeraPasivaCotaVariable);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaVariableAperturaExterior = GetSetsCV1HActivaBalconeraPracticableAperturaExterior();
            if (sets1HBalconeraPracticableActivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B1H APERTURA EXT"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaVariableAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaVariableAperturaExterior = GetSetsCV2HActivaBalconeraPracticableAperturaExterior();
            if (sets2HBalconeraPracticableActivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B2H APERTURA EXTERIOR"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaVariableAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior = GetSetsCV2HPasivaBalconeraPracticableAperturaExterior();
            if (sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B2H PASIVA AP. EXT"));
                setList.AddRange(sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior);
            }
            #endregion

            #region PUERTAS

            List<Set> setsPuerta1HCotaVariable = GetSetsCV1HActivaPuerta();

            if (setsPuerta1HCotaVariable.Any())
            {
                setList.Add(new Set("CV P1H"));
                setList.AddRange(setsPuerta1HCotaVariable);
            }

            List<Set> setsPuerta2HActivaCotaVariable = GetSetsCV2HActivaPuerta();

            if (setsPuerta2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV P2H"));
                setList.AddRange(setsPuerta2HActivaCotaVariable);
            }

            List<Set> sets2HPasivaPuertaCotaVariable = GetSetsCV2HPasivaPuerta();
            if (sets2HPasivaPuertaCotaVariable.Any())
            {
                setList.Add(new Set("CV P2H PASIVA"));
                setList.AddRange(sets2HPasivaPuertaCotaVariable);
            }

            List<Set> sets1HPuertaCotaVariableAperturaExterior = GetSetsCV1HPuertaAperturaExterior();
            if (sets1HPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P1H APERTURA EXT"));
                setList.AddRange(sets1HPuertaCotaVariableAperturaExterior);
            }

            List<Set> sets2HActivaPuertaCotaVariableAperturaExterior = GetSetsCV2HActivaPuertaAperturaExterior();
            if (sets2HActivaPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P2H APERTURA EXT"));
                setList.AddRange(sets2HActivaPuertaCotaVariableAperturaExterior);
            }

            List<Set> sets2HPasivaPuertaCotaVariableAperturaExterior = GetSetsCV2HPasivaPuertaAperturaExterior();
            if (sets2HPasivaPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P2H PASIVA APERTURA EXT"));
                setList.AddRange(sets2HPasivaPuertaCotaVariableAperturaExterior);
            }
            #endregion

            #region OSCILOPARALELAS

            List<Set> setsOsciloParalela1HCotaVariable = GetSetsCVOsciloParalela1H();

            if (setsOsciloParalela1HCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 1 HOJA"));
                setList.AddRange(setsOsciloParalela1HCotaVariable);
            }

            List<Set> setsOsciloParalela2HActivaCotaVariable = GetSetsCVOsciloParalela2HActiva();

            if (setsOsciloParalela2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 2H ACT."));
                setList.AddRange(setsOsciloParalela2HActivaCotaVariable);
            }
            List<Set> setsOsciloParalela2HPasivaCotaVariable = GetSetsCVOsciloParalela2HPasiva();

            if (setsOsciloParalela2HPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 2H PAS."));
                setList.AddRange(setsOsciloParalela2HPasivaCotaVariable);
            }

            #endregion

            #region PARALELAS CORREDERAS

            List<Set> setsParalelaCorredera1HCotaVariable = GetSetsCVParalelaCorredera1H();

            if (setsParalelaCorredera1HCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 1 HOJA"));
                setList.AddRange(setsParalelaCorredera1HCotaVariable);
            }

            List<Set> setsParalelaCorredera2HActivaCotaVariable = GetSetsCVParalelaCorredera2HActiva();

            if (setsParalelaCorredera2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 2H ACT."));
                setList.AddRange(setsParalelaCorredera2HActivaCotaVariable);
            }
            List<Set> setsParalelaCorredera2HPasivaCotaVariable = GetSetsCVParalelaCorredera2HPasiva();

            if (setsParalelaCorredera2HPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 2H PAS."));
                setList.AddRange(setsParalelaCorredera2HPasivaCotaVariable);
            }

            #endregion

            #region PLEGABLES

            List<Set> setsPlegablesCotaVariable = GetSetsCVPlegables();

            if (setsPlegablesCotaVariable.Any())
            {
                setList.Add(new Set("CV PLEGABLE"));
                setList.AddRange(setsPlegablesCotaVariable);
            }

            #endregion

            #endregion

            return setList;

        }
        private List<Set> GetSetsToConnectorAluminio()
        {
            List<Set> setList = new List<Set>();
            #region COTA FIJA 

            #region VENTANAS

            List<Set> sets1HVentanaOsciloBatienteActivaCotaFija = GetSetsCF1HActivaVentanaOsciloBatienteALU();

            if (sets1HVentanaOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V1H OSCILOBATIENTE"));
                setList.AddRange(sets1HVentanaOsciloBatienteActivaCotaFija);
            }

            List<Set> sets1HVentanaPracticableActivaCotaFija = GetSetsCF1HActivaVentanaPracticableALU();

            if (sets1HVentanaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V1H PRACTICABLE"));
                setList.AddRange(sets1HVentanaPracticableActivaCotaFija);
            }

            List<Set> sets2HVentanaOsciloBatienteActivaCotaFija = GetSetsCF2HActivaVentanaOsciloBatienteALU();
            if (sets2HVentanaOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H OSCILOBATIENTE"));
                setList.AddRange(sets2HVentanaOsciloBatienteActivaCotaFija);
            }

            List<Set> sets2HVentanaPracticableActivaCotaFija = GetSetsCF2HActivaVentanaPracticableALU();
            if (sets2HVentanaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H PRACTICABLE"));
                setList.AddRange(sets2HVentanaPracticableActivaCotaFija);
            }


            List<Set> sets2HVentanaPasivaCotaFija = GetSetsCF2HPasivaVentanaPracticableALU();
            if (sets2HVentanaPasivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H PASIVA"));
                setList.AddRange(sets2HVentanaPasivaCotaFija);
            }

            #endregion

            #region BALCONERAS
            List<Set> sets1HBalconeraOsciloBatienteActivaCotaFija = GetSetsCF1HActivaBalconeraOsciloBatienteALU();
            if (sets1HBalconeraOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B1H OSCILOBATIENTE"));
                setList.AddRange(sets1HBalconeraOsciloBatienteActivaCotaFija);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaFija = GetSetsCF1HActivaBalconeraPracticableALU();
            if (sets1HBalconeraPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B1H PRACTICABLE"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaFija);
            }

            List<Set> sets2HBalconeraOsciloBatienteActivaCotaFija = GetSetsCF2HActivaBalconeraOsciloBatienteALU();
            if (sets2HBalconeraOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B2H OSCILOBATIENTE"));
                setList.AddRange(sets2HBalconeraOsciloBatienteActivaCotaFija);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaFija = GetSetsCF2HActivaBalconeraPracticableALU();
            if (sets2HBalconeraPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B2H PRACTICABLE"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaFija);
            }

            List<Set> sets2HBalconeraPasivaCotaFija = GetSetsCF2HPasivaBalconeraPracticableALU();
            if (sets2HBalconeraPasivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR B2H PASIVA"));
                setList.AddRange(sets2HBalconeraPasivaCotaFija);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaFijaAperturaExterior = GetSetsCF1HActivaBalconeraPracticableAperturaExteriorALU();
            if (sets1HBalconeraPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B1H APERTURA EXTERIOR"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaFijaAperturaExterior = GetSetsCF2HActivaBalconeraPracticableAperturaExteriorALU();
            if (sets2HBalconeraPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B2H APERTURA EXTERIOR"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior = GetSetsCF2HPasivaBalconeraPracticableAperturaExteriorALU();
            if (sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B2H PASIVA APERTURA EX"));
                setList.AddRange(sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior);
            }

            #endregion

            #region PUERTAS SECUNDARIAS
            List<Set> sets1HPuertaSecundariaPracticableActivaCotaFija = GetSetsCF1HActivaPuertaSecundariaPracticableALU();
            if (sets1HPuertaSecundariaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 1H"));
                setList.AddRange(sets1HPuertaSecundariaPracticableActivaCotaFija);
            }

            List<Set> sets2HPuertaSecundariaPracticableActivaCotaFija = GetSetsCF2HActivaPuertaSecundariaPracticableALU();
            if (sets1HPuertaSecundariaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 2H"));
                setList.AddRange(sets2HPuertaSecundariaPracticableActivaCotaFija);
            }

            List<Set> sets2HPuertaSecundariaPracticablePasivaCotaFija = GetSetsCF2HPasivaPuertaSecundariaPracticableALU();
            if (sets2HPuertaSecundariaPracticablePasivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 2H PASIVA"));
                setList.AddRange(sets2HPuertaSecundariaPracticablePasivaCotaFija);
            }

            List<Set> sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior = GetSetsCF1HActivaPuertaSecundariaPracticableAperturaExteriorALU();
            if (sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 1H A.E."));
                setList.AddRange(sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior = GetSetsCF2HActivaPuertaSecundariaPracticableAperturaExteriorALU();
            if (sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 2H A.E."));
                setList.AddRange(sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior = GetSetsCF2HPasivaPuertaSecundariaPracticableAperturaExteriorALU();
            if (sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 2H PASIVA A.E."));
                setList.AddRange(sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior);
            }

            #endregion

            #region PUERTAS

            List<Set> sets1HPuertaCotaFija = GetSetsCF1HActivaPuertaALU();
            if (sets1HPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P1H"));
                setList.AddRange(sets1HPuertaCotaFija);
            }

            List<Set> sets2HActivaPuertaCotaFija = GetSetsCF2HActivaPuertaALU();
            if (sets2HActivaPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P2H"));
                setList.AddRange(sets2HActivaPuertaCotaFija);
            }

            List<Set> sets2HPasivaPuertaCotaFija = GetSetsCF2HPasivaPuertaALU();
            if (sets2HPasivaPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P2H PASIVA"));
                setList.AddRange(sets2HPasivaPuertaCotaFija);
            }

            List<Set> sets1HPuertaCotaFijaAperturaExterior = GetSetsCF1HPuertaAperturaExteriorALU();
            if (sets1HPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P1H APERTURA EXT"));
                setList.AddRange(sets1HPuertaCotaFijaAperturaExterior);
            }

            List<Set> sets2HActivaPuertaCotaFijaAperturaExterior = GetSetsCF2HActivaPuertaAperturaExteriorALU();
            if (sets2HActivaPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P2H APERTURA EXT"));
                setList.AddRange(sets2HActivaPuertaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPasivaPuertaCotaFijaAperturaExterior = GetSetsCF2HPasivaPuertaAperturaExteriorALU();
            if (sets2HPasivaPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P2H PASIVA APERTURA EXT"));
                setList.AddRange(sets2HPasivaPuertaCotaFijaAperturaExterior);
            }
            #endregion

            #region CORREDERAS

            List<Set> setsCorrederaCotaFija = GetSetsCFCorredera();

            if (setsCorrederaCotaFija.Any())
            {
                setList.Add(new Set("CF CORREDERA"));
                setList.AddRange(setsCorrederaCotaFija);
            }

            #endregion

            #region PATIO LIFT

            List<Set> setsPatioLiftCotaFija = GetSetsCFPatioLift();

            if (setsCorrederaCotaFija.Any())
            {
                setList.Add(new Set("CF PATIO LIFT"));
                setList.AddRange(setsPatioLiftCotaFija);
            }

            #endregion

            #region PLEGABLES

            List<Set> setsPlegablesCotaFija = GetSetsCFPlegablesALU();

            if (setsPlegablesCotaFija.Any())
            {
                setList.Add(new Set("CF PLEGABLE"));
                setList.AddRange(setsPlegablesCotaFija);
            }

            #endregion

            #endregion

            #region COTA VARIABLE

            #region ABATIBLES

            List<Set> setsAbatiblesCotaVariable = GetSetsCVAbatibles();

            if (setsAbatiblesCotaVariable.Any())
            {
                setList.Add(new Set("CV ABATIBLES"));
                setList.AddRange(setsAbatiblesCotaVariable);
            }


            #endregion

            #region VENTANAS

            List<Set> sets1HVentanaOsciloBatienteActivaCotaVariable = GetSetsCV1HActivaVentanaOsciloBatienteALU();

            if (sets1HVentanaOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V1H OSCILOBATIENTE"));
                setList.AddRange(sets1HVentanaOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets1HVentanaPracticableActivaCotaVariable = GetSetsCV1HActivaVentanaPracticableALU();

            if (sets1HVentanaPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V1H PRACTICABLE"));
                setList.AddRange(sets1HVentanaPracticableActivaCotaVariable);
            }

            List<Set> sets2HVentanaOsciloBatienteActivaCotaVariable = GetSetsCV2HActivaVentanaOsciloBatienteALU();
            if (sets2HVentanaOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H OSCILOBATIENTE"));
                setList.AddRange(sets2HVentanaOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets2HVentanaPracticableActivaCotaVariable = GetSetsCV2HActivaVentanaPracticableALU();
            if (sets2HVentanaPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H PRACTICABLE"));
                setList.AddRange(sets2HVentanaPracticableActivaCotaVariable);
            }


            List<Set> sets2HVentanaPasivaCotaVariable = GetSetsCV2HPasivaVentanaPracticableALU();
            if (sets2HVentanaPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H PASIVA"));
                setList.AddRange(sets2HVentanaPasivaCotaVariable);
            }

            #endregion

            #region BALCONERAS

            List<Set> sets1HBalconeraOsciloBatienteActivaCotaVariable = GetSetsCV1HActivaBalconeraOsciloBatienteALU();
            if (sets1HBalconeraOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B1H OSCILOBATIENTE"));
                setList.AddRange(sets1HBalconeraOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaVariable = GetSetsCV1HActivaBalconeraPracticableALU();
            if (sets1HBalconeraPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B1H PRACTICABLE"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaVariable);
            }

            List<Set> sets2HBalconeraOsciloBatienteActivaCotaVariable = GetSetsCV2HActivaBalconeraOsciloBatienteALU();
            if (sets2HBalconeraOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H OSCILOBATIENTE"));
                setList.AddRange(sets2HBalconeraOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaVariable = GetSetsCV2HActivaBalconeraPracticableALU();
            if (sets2HBalconeraPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H PRACTICABLE"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaVariable);
            }

            List<Set> sets2HBalconeraPasivaCotaVariable = GetSetsCV2HPasivaBalconeraPracticableALU();
            if (sets2HBalconeraPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H PASIVA"));
                setList.AddRange(sets2HBalconeraPasivaCotaVariable);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaVariableAperturaExterior = GetSetsCV1HActivaBalconeraPracticableAperturaExteriorALU();
            if (sets1HBalconeraPracticableActivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B1H APERTURA EXT"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaVariableAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaVariableAperturaExterior = GetSetsCV2HActivaBalconeraPracticableAperturaExteriorALU();
            if (sets2HBalconeraPracticableActivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B2H APERTURA EXTERIOR"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaVariableAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior = GetSetsCV2HPasivaBalconeraPracticableAperturaExteriorALU();
            if (sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B2H PASIVA AP. EXT"));
                setList.AddRange(sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior);
            }
            #endregion

            #region PUERTAS

            List<Set> setsPuerta1HCotaVariable = GetSetsCV1HActivaPuertaALU();

            if (setsPuerta1HCotaVariable.Any())
            {
                setList.Add(new Set("CV P1H"));
                setList.AddRange(setsPuerta1HCotaVariable);
            }

            List<Set> setsPuerta2HActivaCotaVariable = GetSetsCV2HActivaPuertaALU();

            if (setsPuerta2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV P2H"));
                setList.AddRange(setsPuerta2HActivaCotaVariable);
            }

            List<Set> sets2HPasivaPuertaCotaVariable = GetSetsCV2HPasivaPuertaALU();
            if (sets2HPasivaPuertaCotaVariable.Any())
            {
                setList.Add(new Set("CV P2H PASIVA"));
                setList.AddRange(sets2HPasivaPuertaCotaVariable);
            }

            List<Set> sets1HPuertaCotaVariableAperturaExterior = GetSetsCV1HPuertaAperturaExteriorALU();
            if (sets1HPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P1H APERTURA EXT"));
                setList.AddRange(sets1HPuertaCotaVariableAperturaExterior);
            }

            List<Set> sets2HActivaPuertaCotaVariableAperturaExterior = GetSetsCV2HActivaPuertaAperturaExteriorALU();
            if (sets2HActivaPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P2H APERTURA EXT"));
                setList.AddRange(sets2HActivaPuertaCotaVariableAperturaExterior);
            }

            List<Set> sets2HPasivaPuertaCotaVariableAperturaExterior = GetSetsCV2HPasivaPuertaAperturaExteriorALU();
            if (sets2HPasivaPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P2H PASIVA APERTURA EXT"));
                setList.AddRange(sets2HPasivaPuertaCotaVariableAperturaExterior);
            }
            #endregion

            #region OSCILOPARALELAS

            List<Set> setsOsciloParalela1HCotaVariable = GetSetsCVOsciloParalela1HALU();

            if (setsOsciloParalela1HCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 1 HOJA"));
                setList.AddRange(setsOsciloParalela1HCotaVariable);
            }

            List<Set> setsOsciloParalela2HActivaCotaVariable = GetSetsCVOsciloParalela2HActivaALU();

            if (setsOsciloParalela2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 2H ACT."));
                setList.AddRange(setsOsciloParalela2HActivaCotaVariable);
            }
            List<Set> setsOsciloParalela2HPasivaCotaVariable = GetSetsCVOsciloParalela2HPasivaALU();

            if (setsOsciloParalela2HPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 2H PAS."));
                setList.AddRange(setsOsciloParalela2HPasivaCotaVariable);
            }

            #endregion

            #region PARALELAS CORREDERAS

            List<Set> setsParalelaCorredera1HCotaVariable = GetSetsCVParalelaCorredera1HALU();

            if (setsParalelaCorredera1HCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 1 HOJA"));
                setList.AddRange(setsParalelaCorredera1HCotaVariable);
            }

            List<Set> setsParalelaCorredera2HActivaCotaVariable = GetSetsCVParalelaCorredera2HActivaALU();

            if (setsParalelaCorredera2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 2H ACT."));
                setList.AddRange(setsParalelaCorredera2HActivaCotaVariable);
            }
            List<Set> setsParalelaCorredera2HPasivaCotaVariable = GetSetsCVParalelaCorredera2HPasivaALU();

            if (setsParalelaCorredera2HPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 2H PAS."));
                setList.AddRange(setsParalelaCorredera2HPasivaCotaVariable);
            }

            #endregion

            #region PLEGABLES

            List<Set> setsPlegablesCotaVariable = GetSetsCVPlegablesALU();

            if (setsPlegablesCotaVariable.Any())
            {
                setList.Add(new Set("CV PLEGABLE"));
                setList.AddRange(setsPlegablesCotaVariable);
            }

            #endregion

            #endregion

            return setList;
        }
        private List<Set> GetSetsToConnectorPAX()
        {
            List<Set> setList = new List<Set>();
            #region COTA FIJA 

            #region VENTANAS

            List<Set> sets1HVentanaOsciloBatienteActivaCotaFija = GetSetsCF1HActivaVentanaOsciloBatientePAX();

            if (sets1HVentanaOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V1H OSCILOBATIENTE"));
                setList.AddRange(sets1HVentanaOsciloBatienteActivaCotaFija);
            }

            List<Set> sets1HVentanaPracticableActivaCotaFija = GetSetsCF1HActivaVentanaPracticablePAX();

            if (sets1HVentanaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V1H PRACTICABLE"));
                setList.AddRange(sets1HVentanaPracticableActivaCotaFija);
            }

            List<Set> sets2HVentanaOsciloBatienteActivaCotaFija = GetSetsCF2HActivaVentanaOsciloBatientePAX();
            if (sets2HVentanaOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H OSCILOBATIENTE"));
                setList.AddRange(sets2HVentanaOsciloBatienteActivaCotaFija);
            }

            List<Set> sets2HVentanaPracticableActivaCotaFija = GetSetsCF2HActivaVentanaPracticablePAX();
            if (sets2HVentanaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H PRACTICABLE"));
                setList.AddRange(sets2HVentanaPracticableActivaCotaFija);
            }


            List<Set> sets2HVentanaPasivaCotaFija = GetSetsCF2HPasivaVentanaPracticablePAX();
            if (sets2HVentanaPasivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR V2H PASIVA"));
                setList.AddRange(sets2HVentanaPasivaCotaFija);
            }

            #endregion

            #region BALCONERAS
            List<Set> sets1HBalconeraOsciloBatienteActivaCotaFija = GetSetsCF1HActivaBalconeraOsciloBatientePAX();
            if (sets1HBalconeraOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B1H OSCILOBATIENTE"));
                setList.AddRange(sets1HBalconeraOsciloBatienteActivaCotaFija);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaFija = GetSetsCF1HActivaBalconeraPracticablePAX();
            if (sets1HBalconeraPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B1H PRACTICABLE"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaFija);
            }

            List<Set> sets2HBalconeraOsciloBatienteActivaCotaFija = GetSetsCF2HActivaBalconeraOsciloBatientePAX();
            if (sets2HBalconeraOsciloBatienteActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B2H OSCILOBATIENTE"));
                setList.AddRange(sets2HBalconeraOsciloBatienteActivaCotaFija);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaFija = GetSetsCF2HActivaBalconeraPracticablePAX();
            if (sets2HBalconeraPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF B2H PRACTICABLE"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaFija);
            }

            List<Set> sets2HBalconeraPasivaCotaFija = GetSetsCF2HPasivaBalconeraPracticablePAX();
            if (sets2HBalconeraPasivaCotaFija.Any())
            {
                setList.Add(new Set("CF KSR B2H PASIVA"));
                setList.AddRange(sets2HBalconeraPasivaCotaFija);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaFijaAperturaExterior = GetSetsCF1HActivaBalconeraPracticableAperturaExteriorPAX();
            if (sets1HBalconeraPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B1H APERTURA EXTERIOR"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaFijaAperturaExterior = GetSetsCF2HActivaBalconeraPracticableAperturaExteriorPAX();
            if (sets2HBalconeraPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B2H APERTURA EXTERIOR"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior = GetSetsCF2HPasivaBalconeraPracticableAperturaExteriorPAX();
            if (sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF B2H PASIVA APERTURA EX"));
                setList.AddRange(sets2HBalconeraPracticablePasivaCotaFijaAperturaExterior);
            }

            #endregion

            #region PUERTAS SECUNDARIAS
            List<Set> sets1HPuertaSecundariaPracticableActivaCotaFija = GetSetsCF1HActivaPuertaSecundariaPracticablePAX();
            if (sets1HPuertaSecundariaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 1H"));
                setList.AddRange(sets1HPuertaSecundariaPracticableActivaCotaFija);
            }

            List<Set> sets2HPuertaSecundariaPracticableActivaCotaFija = GetSetsCF2HActivaPuertaSecundariaPracticablePAX();
            if (sets1HPuertaSecundariaPracticableActivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 2H"));
                setList.AddRange(sets2HPuertaSecundariaPracticableActivaCotaFija);
            }

            List<Set> sets2HPuertaSecundariaPracticablePasivaCotaFija = GetSetsCF2HPasivaPuertaSecundariaPracticablePAX();
            if (sets2HPuertaSecundariaPracticablePasivaCotaFija.Any())
            {
                setList.Add(new Set("CF P SEC. 2H PASIVA"));
                setList.AddRange(sets2HPuertaSecundariaPracticablePasivaCotaFija);
            }

            List<Set> sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior = GetSetsCF1HActivaPuertaSecundariaPracticableAperturaExteriorPAX();
            if (sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 1H A.E."));
                setList.AddRange(sets1HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior = GetSetsCF2HActivaPuertaSecundariaPracticableAperturaExteriorPAX();
            if (sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 2H A.E."));
                setList.AddRange(sets2HPuertaSecundariaPracticableActivaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior = GetSetsCF2HPasivaPuertaSecundariaPracticableAperturaExteriorPAX();
            if (sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P SEC. 2H PASIVA A.E."));
                setList.AddRange(sets2HPuertaSecundariaPracticablePasivaCotaFijaAperturaExterior);
            }

            #endregion

            #region PUERTAS

            List<Set> sets1HPuertaCotaFija = GetSetsCF1HActivaPuertaPAX();
            if (sets1HPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P1H"));
                setList.AddRange(sets1HPuertaCotaFija);
            }

            List<Set> sets2HActivaPuertaCotaFija = GetSetsCF2HActivaPuertaPAX();
            if (sets2HActivaPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P2H"));
                setList.AddRange(sets2HActivaPuertaCotaFija);
            }

            List<Set> sets2HPasivaPuertaCotaFija = GetSetsCF2HPasivaPuertaPAX();
            if (sets2HPasivaPuertaCotaFija.Any())
            {
                setList.Add(new Set("CF P2H PASIVA"));
                setList.AddRange(sets2HPasivaPuertaCotaFija);
            }

            List<Set> sets1HPuertaCotaFijaAperturaExterior = GetSetsCF1HPuertaAperturaExteriorPAX();
            if (sets1HPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P1H APERTURA EXT"));
                setList.AddRange(sets1HPuertaCotaFijaAperturaExterior);
            }

            List<Set> sets2HActivaPuertaCotaFijaAperturaExterior = GetSetsCF2HActivaPuertaAperturaExteriorPAX();
            if (sets2HActivaPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P2H APERTURA EXT"));
                setList.AddRange(sets2HActivaPuertaCotaFijaAperturaExterior);
            }

            List<Set> sets2HPasivaPuertaCotaFijaAperturaExterior = GetSetsCF2HPasivaPuertaAperturaExteriorPAX();
            if (sets2HPasivaPuertaCotaFijaAperturaExterior.Any())
            {
                setList.Add(new Set("CF P2H PASIVA APERTURA EXT"));
                setList.AddRange(sets2HPasivaPuertaCotaFijaAperturaExterior);
            }
            #endregion

            #region CORREDERAS

            List<Set> setsCorrederaCotaFija = GetSetsCFCorredera();

            if (setsCorrederaCotaFija.Any())
            {
                setList.Add(new Set("CF CORREDERA"));
                setList.AddRange(setsCorrederaCotaFija);
            }

            #endregion

            #region PATIO LIFT

            List<Set> setsPatioLiftCotaFija = GetSetsCFPatioLift();

            if (setsCorrederaCotaFija.Any())
            {
                setList.Add(new Set("CF PATIO LIFT"));
                setList.AddRange(setsPatioLiftCotaFija);
            }

            #endregion

            #region PLEGABLES

            List<Set> setsPlegablesCotaFija = GetSetsCFPlegables();

            if (setsPlegablesCotaFija.Any())
            {
                setList.Add(new Set("CF PLEGABLE"));
                setList.AddRange(setsPlegablesCotaFija);
            }

            #endregion

            #endregion

            #region COTA VARIABLE

            #region ABATIBLES

            List<Set> setsAbatiblesCotaVariable = GetSetsCVAbatibles();

            if (setsAbatiblesCotaVariable.Any())
            {
                setList.Add(new Set("CV ABATIBLES"));
                setList.AddRange(setsAbatiblesCotaVariable);
            }


            #endregion

            #region VENTANAS

            List<Set> sets1HVentanaOsciloBatienteActivaCotaVariable = GetSetsCV1HActivaVentanaOsciloBatientePAX();

            if (sets1HVentanaOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V1H OSCILOBATIENTE"));
                setList.AddRange(sets1HVentanaOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets1HVentanaPracticableActivaCotaVariable = GetSetsCV1HActivaVentanaPracticablePAX();

            if (sets1HVentanaPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V1H PRACTICABLE"));
                setList.AddRange(sets1HVentanaPracticableActivaCotaVariable);
            }

            List<Set> sets2HVentanaOsciloBatienteActivaCotaVariable = GetSetsCV2HActivaVentanaOsciloBatientePAX();
            if (sets2HVentanaOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H OSCILOBATIENTE"));
                setList.AddRange(sets2HVentanaOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets2HVentanaPracticableActivaCotaVariable = GetSetsCV2HActivaVentanaPracticablePAX();
            if (sets2HVentanaPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H PRACTICABLE"));
                setList.AddRange(sets2HVentanaPracticableActivaCotaVariable);
            }


            List<Set> sets2HVentanaPasivaCotaVariable = GetSetsCV2HPasivaVentanaPracticablePAX();
            if (sets2HVentanaPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV V2H PASIVA"));
                setList.AddRange(sets2HVentanaPasivaCotaVariable);
            }

            #endregion

            #region BALCONERAS

            List<Set> sets1HBalconeraOsciloBatienteActivaCotaVariable = GetSetsCV1HActivaBalconeraOsciloBatientePAX();
            if (sets1HBalconeraOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B1H OSCILOBATIENTE"));
                setList.AddRange(sets1HBalconeraOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaVariable = GetSetsCV1HActivaBalconeraPracticablePAX();
            if (sets1HBalconeraPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B1H PRACTICABLE"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaVariable);
            }

            List<Set> sets2HBalconeraOsciloBatienteActivaCotaVariable = GetSetsCV2HActivaBalconeraOsciloBatientePAX();
            if (sets2HBalconeraOsciloBatienteActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H OSCILOBATIENTE"));
                setList.AddRange(sets2HBalconeraOsciloBatienteActivaCotaVariable);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaVariable = GetSetsCV2HActivaBalconeraPracticablePAX();
            if (sets2HBalconeraPracticableActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H PRACTICABLE"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaVariable);
            }

            List<Set> sets2HBalconeraPasivaCotaVariable = GetSetsCV2HPasivaBalconeraPracticablePAX();
            if (sets2HBalconeraPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV B2H PASIVA"));
                setList.AddRange(sets2HBalconeraPasivaCotaVariable);
            }

            List<Set> sets1HBalconeraPracticableActivaCotaVariableAperturaExterior = GetSetsCV1HActivaBalconeraPracticableAperturaExteriorPAX();
            if (sets1HBalconeraPracticableActivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B1H APERTURA EXT"));
                setList.AddRange(sets1HBalconeraPracticableActivaCotaVariableAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticableActivaCotaVariableAperturaExterior = GetSetsCV2HActivaBalconeraPracticableAperturaExteriorPAX();
            if (sets2HBalconeraPracticableActivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B2H APERTURA EXTERIOR"));
                setList.AddRange(sets2HBalconeraPracticableActivaCotaVariableAperturaExterior);
            }

            List<Set> sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior = GetSetsCV2HPasivaBalconeraPracticableAperturaExteriorPAX();
            if (sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV B2H PASIVA AP. EXT"));
                setList.AddRange(sets2HBalconeraPracticablePasivaCotaVariableAperturaExterior);
            }
            #endregion

            #region PUERTAS

            List<Set> setsPuerta1HCotaVariable = GetSetsCV1HActivaPuertaPAX();

            if (setsPuerta1HCotaVariable.Any())
            {
                setList.Add(new Set("CV P1H"));
                setList.AddRange(setsPuerta1HCotaVariable);
            }

            List<Set> setsPuerta2HActivaCotaVariable = GetSetsCV2HActivaPuertaPAX();

            if (setsPuerta2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV P2H"));
                setList.AddRange(setsPuerta2HActivaCotaVariable);
            }

            List<Set> sets2HPasivaPuertaCotaVariable = GetSetsCV2HPasivaPuertaPAX();
            if (sets2HPasivaPuertaCotaVariable.Any())
            {
                setList.Add(new Set("CV P2H PASIVA"));
                setList.AddRange(sets2HPasivaPuertaCotaVariable);
            }

            List<Set> sets1HPuertaCotaVariableAperturaExterior = GetSetsCV1HPuertaAperturaExteriorPAX();
            if (sets1HPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P1H APERTURA EXT"));
                setList.AddRange(sets1HPuertaCotaVariableAperturaExterior);
            }

            List<Set> sets2HActivaPuertaCotaVariableAperturaExterior = GetSetsCV2HActivaPuertaAperturaExteriorPAX();
            if (sets2HActivaPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P2H APERTURA EXT"));
                setList.AddRange(sets2HActivaPuertaCotaVariableAperturaExterior);
            }

            List<Set> sets2HPasivaPuertaCotaVariableAperturaExterior = GetSetsCV2HPasivaPuertaAperturaExteriorPAX();
            if (sets2HPasivaPuertaCotaVariableAperturaExterior.Any())
            {
                setList.Add(new Set("CV P2H PASIVA APERTURA EXT"));
                setList.AddRange(sets2HPasivaPuertaCotaVariableAperturaExterior);
            }
            #endregion

            #region OSCILOPARALELAS

            List<Set> setsOsciloParalela1HCotaVariable = GetSetsCVOsciloParalela1H();

            if (setsOsciloParalela1HCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 1 HOJA"));
                setList.AddRange(setsOsciloParalela1HCotaVariable);
            }

            List<Set> setsOsciloParalela2HActivaCotaVariable = GetSetsCVOsciloParalela2HActiva();

            if (setsOsciloParalela2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 2H ACT."));
                setList.AddRange(setsOsciloParalela2HActivaCotaVariable);
            }
            List<Set> setsOsciloParalela2HPasivaCotaVariable = GetSetsCVOsciloParalela2HPasiva();

            if (setsOsciloParalela2HPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV OSCILOPARALELA 2H PAS."));
                setList.AddRange(setsOsciloParalela2HPasivaCotaVariable);
            }

            #endregion

            #region PARALELAS CORREDERAS

            List<Set> setsParalelaCorredera1HCotaVariable = GetSetsCVParalelaCorredera1H();

            if (setsParalelaCorredera1HCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 1 HOJA"));
                setList.AddRange(setsParalelaCorredera1HCotaVariable);
            }

            List<Set> setsParalelaCorredera2HActivaCotaVariable = GetSetsCVParalelaCorredera2HActiva();

            if (setsParalelaCorredera2HActivaCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 2H ACT."));
                setList.AddRange(setsParalelaCorredera2HActivaCotaVariable);
            }
            List<Set> setsParalelaCorredera2HPasivaCotaVariable = GetSetsCVParalelaCorredera2HPasiva();

            if (setsParalelaCorredera2HPasivaCotaVariable.Any())
            {
                setList.Add(new Set("CV PARALELA CORREDERA 2H PAS."));
                setList.AddRange(setsParalelaCorredera2HPasivaCotaVariable);
            }

            #endregion

            #region PLEGABLES

            List<Set> setsPlegablesCotaVariable = GetSetsCVPlegables();

            if (setsPlegablesCotaVariable.Any())
            {
                setList.Add(new Set("CV PLEGABLE"));
                setList.AddRange(setsPlegablesCotaVariable);
            }

            #endregion

            #endregion

            return setList;
        }

        #endregion

        #region GetOpening

        private List<Option> GetOpeningOptions(Opening opening)
        {
            List<Option> openingFlagList = new List<Option>();

            if (opening.Turn != null && opening.Tilt != null && opening.Right != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "TurnRight"));
                openingFlagList.Add(new Option("Opening_Flag", "TiltDown"));
                opening.openingType = (int)enumOpeningType.OscilobatienteDerechaInt;
            }
            if (opening.Turn != null && opening.Tilt != null && opening.Left != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "TurnLeft"));
                openingFlagList.Add(new Option("Opening_Flag", "TiltDown"));
                opening.openingType = (int)enumOpeningType.OscilobatienteIzquierdaInt;
            }
            if (opening.Turn != null && opening.Tilt == null && opening.Right != null && opening.Outer == null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "TurnRight"));
                opening.openingType = (int)enumOpeningType.PracticableDerechaInt;
            }
            if (opening.Turn != null && opening.Tilt == null && opening.Left != null && opening.Outer == null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "TurnLeft"));
                opening.openingType = (int)enumOpeningType.PracticableIzquierdaInt;
            }
            if (opening.Turn != null && opening.Tilt == null && opening.Right != null && opening.Outer != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "TurnLeft"));
                openingFlagList.Add(new Option("Opening_Flag", "Exterior"));
                opening.openingType = (int)enumOpeningType.PracticableDerechaExt;
            }
            if (opening.Turn != null && opening.Tilt == null && opening.Left != null && opening.Outer != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "TurnRight"));
                openingFlagList.Add(new Option("Opening_Flag", "Exterior"));
                opening.openingType = (int)enumOpeningType.PracticableIzquierdaExt;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Left != null && opening.Sliding != null && opening.Lift == null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "Left"));
                opening.openingType = (int)enumOpeningType.CorrederaIzquierda;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Right != null && opening.Sliding != null && opening.Lift == null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "Right"));
                opening.openingType = (int)enumOpeningType.CorrederaDerecha;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Right == null && opening.Left == null && opening.Sliding != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "Left"));
                openingFlagList.Add(new Option("Opening_Flag", "Right"));
                opening.openingType = (int)enumOpeningType.CorrederaIzqDcha;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Right == null && opening.Left != null && opening.Sliding != null && opening.Lift != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "Left"));
                openingFlagList.Add(new Option("Opening_Flag", "Lift"));
                opening.openingType = (int)enumOpeningType.ElevableIzquierda;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Right != null && opening.Left == null && opening.Sliding != null && opening.Lift != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "Right"));
                openingFlagList.Add(new Option("Opening_Flag", "Lift"));
                opening.openingType = (int)enumOpeningType.ElevableDerecha;
            }
            if (opening.Turn == null && opening.Tilt != null && opening.Bottom != null && opening.Sliding == null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "TiltDown"));
                opening.openingType = (int)enumOpeningType.Abatible;
            }
            if (opening.Turn == null && opening.Tilt != null && opening.Right != null && opening.Left == null && opening.Sliding != null && opening.Bottom != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "Right"));
                openingFlagList.Add(new Option("Opening_Flag", "TiltDown"));
                opening.openingType = (int)enumOpeningType.OsciloCorrederaDerecha;
            }
            if (opening.Turn == null && opening.Tilt != null && opening.Right == null && opening.Left != null && opening.Sliding != null && opening.Bottom != null)
            {
                openingFlagList.Add(new Option("Opening_Flag", "Left"));
                openingFlagList.Add(new Option("Opening_Flag", "TiltDown"));
                opening.openingType = (int)enumOpeningType.OsciloCorrederaIzquierda;
            }
            return openingFlagList;
        }

        #endregion

        #region Métodos obtener Sets

        #region VENTANAS

        #region PVC
        private List<Set> GetSetsCF1HActivaVentanaOsciloBatiente()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code).Where(s => (s.Code.ToUpper().StartsWith("(1V)1H") || s.Code.ToUpper().StartsWith("(1V)1H")) &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaVentanaOsciloBatiente()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code).Where(s => s.Code.ToUpper().StartsWith("(2V)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF1HActivaVentanaPracticable()
        {
            List<Set> setCF1HVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1V)1H") &&
                                                                                s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaPracticable)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaPracticable;
        }
        private List<Set> GetSetsCV1HActivaVentanaPracticable()
        {
            List<Set> setCV1HVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                    .Where(s => s.Code.ToUpper().StartsWith("(2V)1H") &&
                                                                                s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaPracticable)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaPracticable;
        }

        private List<Set> GetSetsCF2HActivaVentanaOsciloBatiente()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1V)2A") &&
                                                                                        //!s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaVentanaOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                //set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Asociada
                if (set.Code.ToUpper().Contains("-2P"))
                {
                    // TODO: 2A-2P Validar
                    set.OptionConectorList.Add(new Option("Asociada", "Oscilobatiente"));
                }
                else
                {
                    set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                }

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaVentanaOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaVentanaOsciloBatiente()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2V)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaVentanaOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaVentanaOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HActivaVentanaPracticable()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1V)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HActivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaVentanaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        private List<Set> GetSetsCV2HActivaVentanaPracticable()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2V)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCV2HActivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaVentanaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }

        private List<Set> GetSetsCF2HPasivaVentanaPracticable()
        {
            List<Set> setCF2HPasivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1V)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            return setCF2HPasivaVentanaPracticable;
        }
        private List<Set> GetSetsCV2HPasivaVentanaPracticable()
        {
            List<Set> setCF2HPasivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2V)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("ALV") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            return setCF2HPasivaVentanaPracticable;
        }
        #endregion

        #region ALUMINIO
        private List<Set> GetSetsCF1HActivaVentanaOsciloBatienteALU()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code).Where(s => s.Code.ToUpper().StartsWith("(1AV)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                }

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaVentanaOsciloBatienteALU()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code).Where(s => s.Code.ToUpper().StartsWith("(2AV)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                }

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF1HActivaVentanaPracticableALU()
        {
            List<Set> setCF1HVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1AV)1H") &&
                                                                                s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaPracticable)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                }


                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaPracticable;
        }
        private List<Set> GetSetsCV1HActivaVentanaPracticableALU()
        {
            List<Set> setCV1HVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                    .Where(s => s.Code.ToUpper().StartsWith("(2AV)1H") &&
                                                                                s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaPracticable)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaPracticable;
        }

        private List<Set> GetSetsCF2HActivaVentanaOsciloBatienteALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1AV)2A") &&
                                                                                        //!s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaVentanaOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                //set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Asociada
                if (set.Code.ToUpper().Contains("-2P"))
                {
                    // TODO: 2A-2P Validar
                    set.OptionConectorList.Add(new Option("Asociada", "Oscilobatiente"));
                }
                else
                {
                    set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                }

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    setCopySTDCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));

                    setsResult.Add(setCopySTDCremona);

                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaVentanaOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaVentanaOsciloBatienteALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2AV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaVentanaOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopySTDCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    setCopySTDCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));

                    setsResult.Add(setCopySTDCremona);

                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaVentanaOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HActivaVentanaPracticableALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1AV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HActivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    setCopySTDCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));

                    setsResult.Add(setCopySTDCremona);

                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaVentanaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        private List<Set> GetSetsCV2HActivaVentanaPracticableALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2AV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCV2HActivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    setCopySTDCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));

                    setsResult.Add(setCopySTDCremona);

                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaVentanaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }

        private List<Set> GetSetsCF2HPasivaVentanaPracticableALU()
        {
            List<Set> setCF2HPasivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1AV)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));
                    }
                    else if (set.Code.ToUpper().Contains("PASADOR Y CLIP"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
            }

            return setCF2HPasivaVentanaPracticable;
        }
        private List<Set> GetSetsCV2HPasivaVentanaPracticableALU()
        {
            List<Set> setCF2HPasivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2AV)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("ALV") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));
                    }
                    else if (set.Code.ToUpper().Contains("PASADOR Y CLIP"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
            }

            return setCF2HPasivaVentanaPracticable;
        }
        #endregion

        #region PAX
        private List<Set> GetSetsCF1HActivaVentanaOsciloBatientePAX()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code).Where(s => s.Code.ToUpper().StartsWith("(1XV)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD")
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaVentanaOsciloBatientePAX()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code).Where(s => s.Code.ToUpper().StartsWith("(2XV)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD")
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF1HActivaVentanaPracticablePAX()
        {
            List<Set> setCF1HVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1XV)1H") &&
                                                                                s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaPracticable)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD")
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaPracticable;
        }
        private List<Set> GetSetsCV1HActivaVentanaPracticablePAX()
        {
            List<Set> setCV1HVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                    .Where(s => s.Code.ToUpper().StartsWith("(2XV)1H") &&
                                                                                s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaPracticable)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD")
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaPracticable;
        }

        private List<Set> GetSetsCF2HActivaVentanaOsciloBatientePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1XV)2A") &&
                                                                                        //!s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaVentanaOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                //set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Asociada
                if (set.Code.ToUpper().Contains("-2P"))
                {
                    // TODO: 2A-2P Validar
                    set.OptionConectorList.Add(new Option("Asociada", "Oscilobatiente"));
                }
                else
                {
                    set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                }

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaVentanaOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaVentanaOsciloBatientePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2XV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaVentanaOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaVentanaOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HActivaVentanaPracticablePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1XV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HActivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaVentanaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        private List<Set> GetSetsCV2HActivaVentanaPracticablePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2XV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCV2HActivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaVentanaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }

        private List<Set> GetSetsCF2HPasivaVentanaPracticablePAX()
        {
            List<Set> setCF2HPasivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1XV)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            return setCF2HPasivaVentanaPracticable;
        }
        private List<Set> GetSetsCV2HPasivaVentanaPracticablePAX()
        {
            List<Set> setCF2HPasivaVentanaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2XV)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("ALV") &&
                                                                                        !s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaVentanaPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No"));

                //Aguja
                if (set.Code.Contains("8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
                else if (set.Code.ToUpper().Contains("NUDO FRANCES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag6"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag15"));
                }

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            return setCF2HPasivaVentanaPracticable;
        }
        #endregion

        #endregion

        #region BALCONERAS

        #region PVC
        private List<Set> GetSetsCF1HActivaBalconeraOsciloBatiente()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(1V)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaBalconeraOsciloBatiente()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(2V)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF1HActivaBalconeraPracticable()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(1V)1H") &&
                                                                                    s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                    !s.Code.ToUpper().Contains("AE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaBalconeraPracticable()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(2V)1H") &&
                                                                                    s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                    !s.Code.ToUpper().Contains("AE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF2HActivaBalconeraOsciloBatiente()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaBalconeraOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                .Where(s => s.Code.ToUpper().StartsWith("(1V)2A") &&
                                                                                            !s.Code.ToUpper().Contains("-2P") &&
                                                                                            s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                            s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaBalconeraOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaBalconeraOsciloBatiente()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaBalconeraOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                .Where(s => s.Code.ToUpper().StartsWith("(2V)2A") &&
                                                                                            !s.Code.ToUpper().Contains("-2P") &&
                                                                                            s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                            s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaBalconeraOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HActivaBalconeraPracticable()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1V)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaBalconeraPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaBalconeraPracticable()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2V)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaBalconeraPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HPasivaBalconeraPracticable()
        {
            List<Set> setCF2HPasivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1V)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            return setCF2HPasivaBalconeraPracticable;
        }
        private List<Set> GetSetsCV2HPasivaBalconeraPracticable()
        {
            List<Set> setCV2HPasivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2V)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCV2HPasivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            return setCV2HPasivaBalconeraPracticable;
        }

        private List<Set> GetSetsCF1HActivaBalconeraPracticableAperturaExterior()
        {

            List<Set> setCF1HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1V)1H") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HActivaBalconeraPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HActivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV1HActivaBalconeraPracticableAperturaExterior()
        {

            List<Set> setCV1HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2V)1H") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HActivaBalconeraPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }

                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HActivaBalconeraPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HActivaBalconeraPracticableAperturaExterior()
        {

            List<Set> setCF2HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1V)2A") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }

                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Practicable"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF2HActivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV2HActivaBalconeraPracticableAperturaExterior()
        {

            List<Set> setCV2HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2V)2A") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Practicable"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV2HActivaBalconeraPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HPasivaBalconeraPracticableAperturaExterior()
        {

            List<Set> setCF2HPasivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1V)2P") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HPasivaBalconeraPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));


                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }

            }

            return setCF2HPasivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV2HPasivaBalconeraPracticableAperturaExterior()
        {

            List<Set> setCF2HPasivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2V)2P") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HPasivaBalconeraPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));


                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }

            }

            return setCF2HPasivaBalconeraPracticableAperturaExterior;
        }
        #endregion

        #region ALU
        private List<Set> GetSetsCF1HActivaBalconeraOsciloBatienteALU()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(1AV)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                }


                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaBalconeraOsciloBatienteALU()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(2AV)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                }

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF1HActivaBalconeraPracticableALU()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(1AV)1H") &&
                                                                                    s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                    !s.Code.ToUpper().Contains("AE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                }

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaBalconeraPracticableALU()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(2AV)1H") &&
                                                                                    s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                    !s.Code.ToUpper().Contains("AE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                }

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF2HActivaBalconeraOsciloBatienteALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaBalconeraOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                .Where(s => s.Code.ToUpper().StartsWith("(1AV)2A") &&
                                                                                            !s.Code.ToUpper().Contains("-2P") &&
                                                                                            s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                            s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    setCopySTDCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));

                    setsResult.Add(setCopySTDCremona);

                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaBalconeraOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaBalconeraOsciloBatienteALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaBalconeraOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                .Where(s => s.Code.ToUpper().StartsWith("(2AV)2A") &&
                                                                                            !s.Code.ToUpper().Contains("-2P") &&
                                                                                            s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                            s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    setCopySTDCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));

                    setsResult.Add(setCopySTDCremona);

                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaBalconeraOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HActivaBalconeraPracticableALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1AV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    setCopySTDCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));

                    setsResult.Add(setCopySTDCremona);

                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaBalconeraPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaBalconeraPracticableALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2AV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopySTDCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    setCopySTDCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));

                    setsResult.Add(setCopySTDCremona);

                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaBalconeraPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HPasivaBalconeraPracticableALU()
        {
            List<Set> setCF2HPasivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1AV)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));
                    }
                    else if (set.Code.ToUpper().Contains("PASADOR Y CLIP"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("BISAGRAS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_Cremona con Bisagras"));
                }

            }

            return setCF2HPasivaBalconeraPracticable;
        }
        private List<Set> GetSetsCV2HPasivaBalconeraPracticableALU()
        {
            List<Set> setCV2HPasivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2AV)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCV2HPasivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Cremona"));
                    }
                    else if (set.Code.ToUpper().Contains("PASADOR Y CLIP"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_HOJA PASIVA", "AL_Clip y Pasador"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC2"));
                }
                else if (set.Code.ToUpper().Contains("RC1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_RC1"));
                }
                else if (set.Code.ToUpper().Contains("BISAGRAS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NT_AL_SEGURIDAD", "AL_Cremona con Bisagras"));
                }
            }

            return setCV2HPasivaBalconeraPracticable;
        }

        private List<Set> GetSetsCF1HActivaBalconeraPracticableAperturaExteriorALU()
        {

            List<Set> setCF1HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1AV)1H") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HActivaBalconeraPracticableAperturaExterior)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HActivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV1HActivaBalconeraPracticableAperturaExteriorALU()
        {

            List<Set> setCV1HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2AV)1H") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HActivaBalconeraPracticableAperturaExterior)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HActivaBalconeraPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HActivaBalconeraPracticableAperturaExteriorALU()
        {

            List<Set> setCF2HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1AV)2A") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraPracticableAperturaExterior)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Practicable"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF2HActivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV2HActivaBalconeraPracticableAperturaExteriorALU()
        {

            List<Set> setCV2HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2AV)2A") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraPracticableAperturaExterior)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Practicable"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV2HActivaBalconeraPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HPasivaBalconeraPracticableAperturaExteriorALU()
        {

            List<Set> setCF2HPasivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1AV)2P") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HPasivaBalconeraPracticableAperturaExterior)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));
            }

            return setCF2HPasivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV2HPasivaBalconeraPracticableAperturaExteriorALU()
        {

            List<Set> setCF2HPasivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2AV)2P") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HPasivaBalconeraPracticableAperturaExterior)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

            }

            return setCF2HPasivaBalconeraPracticableAperturaExterior;
        }
        #endregion

        #region PAX
        private List<Set> GetSetsCF1HActivaBalconeraOsciloBatientePAX()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(1XV)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaBalconeraOsciloBatientePAX()
        {

            List<Set> setCF1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(2XV)1H") &&
                                                                                    s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF1HActivaBalconeraPracticablePAX()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(1XV)1H") &&
                                                                                    s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                    !s.Code.ToUpper().Contains("AE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }
        private List<Set> GetSetsCV1HActivaBalconeraPracticablePAX()
        {

            List<Set> setCV1HVentanaOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                        .Where(s => s.Code.ToUpper().StartsWith("(2XV)1H") &&
                                                                                    s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                    !s.Code.ToUpper().Contains("AE") &&
                                                                                    s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HVentanaOscilobatiente)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                    set.Code.Contains("RC2") ? OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2") : OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HVentanaOscilobatiente;
        }

        private List<Set> GetSetsCF2HActivaBalconeraOsciloBatientePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaBalconeraOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                .Where(s => s.Code.ToUpper().StartsWith("(1XV)2A") &&
                                                                                            !s.Code.ToUpper().Contains("-2P") &&
                                                                                            s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                            s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaBalconeraOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaBalconeraOsciloBatientePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaBalconeraOscilobatiente = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                .Where(s => s.Code.ToUpper().StartsWith("(2XV)2A") &&
                                                                                            !s.Code.ToUpper().Contains("-2P") &&
                                                                                            s.Code.ToUpper().Contains("OSCILOBATIENTE") &&
                                                                                            s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraOscilobatiente)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaBalconeraOscilobatiente);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HActivaBalconeraPracticablePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCF2HActivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1XV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCF2HActivaBalconeraPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();

        }
        private List<Set> GetSetsCV2HActivaBalconeraPracticablePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setCV2HActivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2XV)2A") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));
                    setCopySTDPerimetralPlus = new Set(set);
                    setCopySTDCremona = new Set(set);
                    setCopySTDCremonaPlus = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopySTDPerimetralPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    setCopySTDCremona.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    setCopySTDCremonaPlus.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));

                    setsResult.Add(setCopySTDPerimetralPlus);
                    setsResult.Add(setCopySTDCremona);
                    setsResult.Add(setCopySTDCremonaPlus);
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));
                    setCopyRC2 = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    setCopyRC2.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));

                    setsResult.Add(setCopyRC2);
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            setsResult.AddRange(setCV2HActivaBalconeraPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();

        }

        private List<Set> GetSetsCF2HPasivaBalconeraPracticablePAX()
        {
            List<Set> setCF2HPasivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1XV)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCF2HPasivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            return setCF2HPasivaBalconeraPracticable;
        }
        private List<Set> GetSetsCV2HPasivaBalconeraPracticablePAX()
        {
            List<Set> setCV2HPasivaBalconeraPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2XV)2P") &&
                                                                                        !s.Code.ToUpper().Contains("-2P") &&
                                                                                        !s.Code.ToUpper().Contains("AE") &&
                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();
            foreach (Set set in setCV2HPasivaBalconeraPracticable)
            {
                //Asignar opening flags
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }

                //Gestión opciones
                Set setCopyRC2 = new Set();
                Set setCopySTDCremona = new Set();
                Set setCopySTDPerimetralPlus = new Set();
                Set setCopySTDCremonaPlus = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));

                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "STD"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }
                else if (set.Code.ToUpper().Contains("RC2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERRAJE SEGURIDAD", "RC2"));

                    if (set.Code.ToUpper().Contains("PERIMETRAL") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("PERIMETRAL") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Perimetral"));
                    }
                }
                else if (set.Code.ToUpper().Contains("2SC"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Click y Pasador"));
                }
            }

            return setCV2HPasivaBalconeraPracticable;
        }

        private List<Set> GetSetsCF1HActivaBalconeraPracticableAperturaExteriorPAX()
        {

            List<Set> setCF1HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1XV)1H") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF1HActivaBalconeraPracticableAperturaExterior)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF1HActivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV1HActivaBalconeraPracticableAperturaExteriorPAX()
        {

            List<Set> setCV1HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2XV)1H") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV1HActivaBalconeraPracticableAperturaExterior)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Ninguna"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV1HActivaBalconeraPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HActivaBalconeraPracticableAperturaExteriorPAX()
        {

            List<Set> setCF2HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1XV)2A") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HActivaBalconeraPracticableAperturaExterior)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "No"),
                    new Option("Asociada", "Practicable"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCF2HActivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV2HActivaBalconeraPracticableAperturaExteriorPAX()
        {

            List<Set> setCV2HActivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2XV)2A") &&
                                                                                                        s.Code.ToUpper().Contains("PRACTICABLE") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCV2HActivaBalconeraPracticableAperturaExterior)
            {
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("Puerta", "No"),
                    new Option("CotaVariable", "Sí"),
                    new Option("Asociada", "Practicable"),
                    OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"),
                    OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"),
                ];

                set.OptionConectorList = optionList;

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
            }

            return setCV2HActivaBalconeraPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HPasivaBalconeraPracticableAperturaExteriorPAX()
        {

            List<Set> setCF2HPasivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(1XV)2P") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HPasivaBalconeraPracticableAperturaExterior)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));


                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }

            }

            return setCF2HPasivaBalconeraPracticableAperturaExterior;
        }
        private List<Set> GetSetsCV2HPasivaBalconeraPracticableAperturaExteriorPAX()
        {

            List<Set> setCF2HPasivaBalconeraPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                            .Where(s => s.Code.ToUpper().StartsWith("(2XV)2P") &&
                                                                                                        s.Code.ToUpper().Contains("AE") &&
                                                                                                        s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setCF2HPasivaBalconeraPracticableAperturaExterior)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Balconera"));


                //Seguridad
                if (set.Code.ToUpper().Contains("STD"))
                {
                    if (set.Code.ToUpper().Contains("CREMONA") && set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona PLUS"));
                    }
                    else if (set.Code.ToUpper().Contains("CREMONA") && !set.Code.ToUpper().Contains("PLUS"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("NX_HERR. HOJA PASIVA", "Cremona"));
                    }
                }

            }

            return setCF2HPasivaBalconeraPracticableAperturaExterior;
        }
        #endregion

        #endregion

        #region PUERTA SECUNDARIA

        #region PVC
        private List<Set> GetSetsCF1HActivaPuertaSecundariaPracticable()
        {

            List<Set> setsCF1HActivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1)1H") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF1HActivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }
            }

            return setsCF1HActivaPuertaSecundariaPracticable;
        }

        private List<Set> GetSetsCF2HActivaPuertaSecundariaPracticable()
        {

            List<Set> setsCF2HActivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1)2A") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HActivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }
            }

            return setsCF2HActivaPuertaSecundariaPracticable;
        }

        private List<Set> GetSetsCF2HPasivaPuertaSecundariaPracticable()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setsCF2HPasivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1)2P") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }

                //Gestión opciones
                Set setCopyPueSecunBombillo = new Set();
                Set setCopyPueSecunCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));

                if (set.Code.ToUpper().Contains("NX"))
                {
                    setCopyPueSecunBombillo = new Set(set);
                    setCopyPueSecunCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                    setCopyPueSecunBombillo.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                    setCopyPueSecunCremona.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));

                    setsResult.Add(setCopyPueSecunBombillo);
                    setsResult.Add(setCopyPueSecunCremona);
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaSecundariaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }

        private List<Set> GetSetsCF1HActivaPuertaSecundariaPracticableAperturaExterior()
        {

            List<Set> setsCF1HActivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1)1H") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF1HActivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }

            }

            return setsCF1HActivaPuertaSecundariaPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HActivaPuertaSecundariaPracticableAperturaExterior()
        {

            List<Set> setsCF2HActivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1)2A") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HActivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }

            }

            return setsCF2HActivaPuertaSecundariaPracticableAperturaExterior;

        }

        private List<Set> GetSetsCF2HPasivaPuertaSecundariaPracticableAperturaExterior()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1)2P") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }

                //Gestión opciones
                Set setCopyPueSecunBombillo = new Set();
                Set setCopyPueSecunCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));

                if (set.Code.ToUpper().Contains("NX"))
                {
                    setCopyPueSecunBombillo = new Set(set);
                    setCopyPueSecunCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                    setCopyPueSecunBombillo.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                    setCopyPueSecunCremona.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));

                    setsResult.Add(setCopyPueSecunBombillo);
                    setsResult.Add(setCopyPueSecunCremona);
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        #endregion

        #region ALU
        private List<Set> GetSetsCF1HActivaPuertaSecundariaPracticableALU()
        {

            List<Set> setsCF1HActivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1A)1H") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF1HActivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }
            }

            return setsCF1HActivaPuertaSecundariaPracticable;
        }

        private List<Set> GetSetsCF2HActivaPuertaSecundariaPracticableALU()
        {

            List<Set> setsCF2HActivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1A)2A") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HActivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }
            }

            return setsCF2HActivaPuertaSecundariaPracticable;
        }

        private List<Set> GetSetsCF2HPasivaPuertaSecundariaPracticableALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setsCF2HPasivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1A)2P") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }

                //Gestión opciones
                Set setCopyPueSecunBombillo = new Set();
                Set setCopyPueSecunCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));

                if (set.Code.ToUpper().Contains("NX"))
                {
                    setCopyPueSecunBombillo = new Set(set);
                    setCopyPueSecunCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                    setCopyPueSecunBombillo.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                    setCopyPueSecunCremona.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));

                    setsResult.Add(setCopyPueSecunBombillo);
                    setsResult.Add(setCopyPueSecunCremona);
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaSecundariaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }

        private List<Set> GetSetsCF1HActivaPuertaSecundariaPracticableAperturaExteriorALU()
        {

            List<Set> setsCF1HActivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1A)1H") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF1HActivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }

            }

            return setsCF1HActivaPuertaSecundariaPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HActivaPuertaSecundariaPracticableAperturaExteriorALU()
        {

            List<Set> setsCF2HActivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1A)2A") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HActivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }

            }

            return setsCF2HActivaPuertaSecundariaPracticableAperturaExterior;

        }

        private List<Set> GetSetsCF2HPasivaPuertaSecundariaPracticableAperturaExteriorALU()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1A)2P") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }

                //Gestión opciones
                Set setCopyPueSecunBombillo = new Set();
                Set setCopyPueSecunCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));

                if (set.Code.ToUpper().Contains("NX"))
                {
                    setCopyPueSecunBombillo = new Set(set);
                    setCopyPueSecunCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                    setCopyPueSecunBombillo.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                    setCopyPueSecunCremona.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));

                    setsResult.Add(setCopyPueSecunBombillo);
                    setsResult.Add(setCopyPueSecunCremona);
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        #endregion

        #region PAX

        private List<Set> GetSetsCF1HActivaPuertaSecundariaPracticablePAX()
        {

            List<Set> setsCF1HActivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1X)1H") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF1HActivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }
            }

            return setsCF1HActivaPuertaSecundariaPracticable;
        }

        private List<Set> GetSetsCF2HActivaPuertaSecundariaPracticablePAX()
        {

            List<Set> setsCF2HActivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1X)2A") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HActivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }
            }

            return setsCF2HActivaPuertaSecundariaPracticable;
        }

        private List<Set> GetSetsCF2HPasivaPuertaSecundariaPracticablePAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setsCF2HPasivaPuertaSecundariaPracticable = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1X)2P") &&
                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                !s.Code.ToUpper().Contains("AE") &&
                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaSecundariaPracticable)
            {

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }

                //Gestión opciones
                Set setCopyPueSecunBombillo = new Set();
                Set setCopyPueSecunCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));

                if (set.Code.ToUpper().Contains("NX"))
                {
                    setCopyPueSecunBombillo = new Set(set);
                    setCopyPueSecunCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                    setCopyPueSecunBombillo.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                    setCopyPueSecunCremona.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));

                    setsResult.Add(setCopyPueSecunBombillo);
                    setsResult.Add(setCopyPueSecunCremona);
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaSecundariaPracticable);
            return setsResult.OrderBy(s => s.Code).ToList();
        }

        private List<Set> GetSetsCF1HActivaPuertaSecundariaPracticableAperturaExteriorPAX()
        {

            List<Set> setsCF1HActivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1X)1H") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF1HActivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }

            }

            return setsCF1HActivaPuertaSecundariaPracticableAperturaExterior;
        }

        private List<Set> GetSetsCF2HActivaPuertaSecundariaPracticableAperturaExteriorPAX()
        {

            List<Set> setsCF2HActivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1X)2A") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HActivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                //Seguridad
                if (set.Code.ToUpper().Contains("H600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                }
                else if (set.Code.ToUpper().Contains("C600"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                }
                else if (set.Code.ToUpper().Contains("EM NX"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));
                }

            }

            return setsCF2HActivaPuertaSecundariaPracticableAperturaExterior;

        }

        private List<Set> GetSetsCF2HPasivaPuertaSecundariaPracticableAperturaExteriorPAX()
        {
            List<Set> setsResult = new List<Set>();
            List<Set> setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                                                    .Where(s => s.Code.ToUpper().StartsWith("(1X)2P") &&
                                                                                                                s.Code.ToUpper().Contains("SEC") &&
                                                                                                                s.Code.ToUpper().Contains("AE") &&
                                                                                                                !s.Code.ToUpper().Contains("BALC")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior)
            {
                if (chk_ConfigAE.Checked)
                {
                    set.Code = GetEquivalenciaAI(set.Code);
                }
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }

                //Gestión opciones
                Set setCopyPueSecunBombillo = new Set();
                Set setCopyPueSecunCremona = new Set();

                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "No"));
                set.OptionConectorList.Add(OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si"));

                if (set.Code.ToUpper().Contains("NX"))
                {
                    setCopyPueSecunBombillo = new Set(set);
                    setCopyPueSecunCremona = new Set(set);

                    set.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Manilla"));
                    setCopyPueSecunBombillo.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Acc. Bombillo"));
                    setCopyPueSecunCremona.OptionConectorList.Add(OpcionHelper.Crear("SEC_TIPO BALCONERA", "Puerta Secundaria Cremona"));

                    setsResult.Add(setCopyPueSecunBombillo);
                    setsResult.Add(setCopyPueSecunCremona);
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaSecundariaPracticableAperturaExterior);
            return setsResult.OrderBy(s => s.Code).ToList();
        }

        #endregion

        #endregion

        #region PUERTAS

        #region PVC
        private List<Set> GetSetsCF1HActivaPuerta()
        {

            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV1HActivaPuerta()
        {

            List<Set> setsCV1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuerta;
        }

        private List<Set> GetSetsCF2HActivaPuerta()
        {
            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));

                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV2HActivaPuerta()
        {
            List<Set> setsCV2HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV2HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }
            return setsCV2HActivaPuerta;
        }


        private List<Set> GetSetsCF2HPasivaPuerta()
        {
            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Pasador"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("PG"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV2HPasivaPuerta()
        {
            List<Set> setsCV1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuerta;
        }

        private List<Set> GetSetsCF1HPuertaAperturaExterior()
        {

            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();
            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV1HPuertaAperturaExterior()
        {

            List<Set> setsCV1HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();
            foreach (Set set in setsCV1HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuertaAE;
        }

        private List<Set> GetSetsCF2HActivaPuertaAperturaExterior()
        {
            List<Set> setsCF2HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                }
            }

            return setsCF2HActivaPuertaAE;
        }
        private List<Set> GetSetsCV2HActivaPuertaAperturaExterior()
        {
            List<Set> setsCV2HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV2HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV2HActivaPuertaAE;
        }

        private List<Set> GetSetsCF2HPasivaPuertaAperturaExterior()
        {
            List<Set> setsResult = new List<Set>();

            List<Set> setsCF2HPasivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaAE)
            {
                Set setCopyA700 = new Set();

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Pasador"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("PG"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaAE);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        private List<Set> GetSetsCV2HPasivaPuertaAperturaExterior()
        {
            List<Set> setsResult = new List<Set>();

            List<Set> setsCF2HPasivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }

            }

            setsResult.AddRange(setsCF2HPasivaPuertaAE);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        #endregion

        #region ALU
        private List<Set> GetSetsCF1HActivaPuertaALU()
        {

            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1A)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV1HActivaPuertaALU()
        {

            List<Set> setsCV1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2A)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuerta;
        }

        private List<Set> GetSetsCF2HActivaPuertaALU()
        {
            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1A)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));

                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV2HActivaPuertaALU()
        {
            List<Set> setsCV2HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2A)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV2HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }
            return setsCV2HActivaPuerta;
        }


        private List<Set> GetSetsCF2HPasivaPuertaALU()
        {
            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1A)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Pasador"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("PG"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV2HPasivaPuertaALU()
        {
            List<Set> setsCV1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2A)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuerta;
        }

        private List<Set> GetSetsCF1HPuertaAperturaExteriorALU()
        {

            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1A)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();
            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV1HPuertaAperturaExteriorALU()
        {

            List<Set> setsCV1HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2A)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();
            foreach (Set set in setsCV1HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuertaAE;
        }

        private List<Set> GetSetsCF2HActivaPuertaAperturaExteriorALU()
        {
            List<Set> setsCF2HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1A)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                }
            }

            return setsCF2HActivaPuertaAE;
        }
        private List<Set> GetSetsCV2HActivaPuertaAperturaExteriorALU()
        {
            List<Set> setsCV2HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2A)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV2HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV2HActivaPuertaAE;
        }

        private List<Set> GetSetsCF2HPasivaPuertaAperturaExteriorALU()
        {
            List<Set> setsResult = new List<Set>();

            List<Set> setsCF2HPasivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1A)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaAE)
            {
                Set setCopyA700 = new Set();

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Pasador"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("PG"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaAE);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        private List<Set> GetSetsCV2HPasivaPuertaAperturaExteriorALU()
        {
            List<Set> setsResult = new List<Set>();

            List<Set> setsCF2HPasivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2A)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }

            }

            setsResult.AddRange(setsCF2HPasivaPuertaAE);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        #endregion

        #region PAX
        private List<Set> GetSetsCF1HActivaPuertaPAX()
        {

            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1X)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV1HActivaPuertaPAX()
        {

            List<Set> setsCV1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2X)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuerta;
        }

        private List<Set> GetSetsCF2HActivaPuertaPAX()
        {
            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1X)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));

                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV2HActivaPuertaPAX()
        {
            List<Set> setsCV2HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2X)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV2HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }
            return setsCV2HActivaPuerta;
        }


        private List<Set> GetSetsCF2HPasivaPuertaPAX()
        {
            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1X)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Pasador"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("PG"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV2HPasivaPuertaPAX()
        {
            List<Set> setsCV1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2X)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            !s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuerta;
        }

        private List<Set> GetSetsCF1HPuertaAperturaExteriorPAX()
        {

            List<Set> setsCF1HActivaPuerta = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1X)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();
            foreach (Set set in setsCF1HActivaPuerta)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
            }

            return setsCF1HActivaPuerta;
        }
        private List<Set> GetSetsCV1HPuertaAperturaExteriorPAX()
        {

            List<Set> setsCV1HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2X)1H") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();
            foreach (Set set in setsCV1HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV1HActivaPuertaAE;
        }

        private List<Set> GetSetsCF2HActivaPuertaAperturaExteriorPAX()
        {
            List<Set> setsCF2HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1X)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("BULONES"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Bulones"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "A700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PLETINA", "P16"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO A"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    if (set.Code.ToUpper().Contains("KF"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Komfort"));
                    }
                    else
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    }
                }
                else if (set.Code.ToUpper().Contains("ENEO E700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "E700"));
                    if (set.Code.ToUpper().Contains("PG"))
                    {
                        set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                    }
                }
            }

            return setsCF2HActivaPuertaAE;
        }
        private List<Set> GetSetsCV2HActivaPuertaAperturaExteriorPAX()
        {
            List<Set> setsCV2HActivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2X)2A") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCV2HActivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("Asociada", "Practicable"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("1PUNTO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }
            }

            return setsCV2HActivaPuertaAE;
        }

        private List<Set> GetSetsCF2HPasivaPuertaAperturaExteriorPAX()
        {
            List<Set> setsResult = new List<Set>();

            List<Set> setsCF2HPasivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(1X)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaAE)
            {
                Set setCopyA700 = new Set();

                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));


                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADERO", "Clasico"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Pasador"));
                }
                else if (set.Code.ToUpper().Contains("TANDEO"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Tandeo"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("A700"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Eneo A"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("COMBINADA"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "Combinada"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
                else if (set.Code.ToUpper().Contains("PG"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_PASIVA", "Cremona (2CC)"));
                }
            }

            setsResult.AddRange(setsCF2HPasivaPuertaAE);
            return setsResult.OrderBy(s => s.Code).ToList();
        }
        private List<Set> GetSetsCV2HPasivaPuertaAperturaExteriorPAX()
        {
            List<Set> setsResult = new List<Set>();

            List<Set> setsCF2HPasivaPuertaAE = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                                .Where(s => s.Code.ToUpper().StartsWith("(2X)2P") &&
                                                                            s.Code.ToUpper().Contains("PUERTA") &&
                                                                            s.Code.ToUpper().Contains("AE")).ToList();

            foreach (Set set in setsCF2HPasivaPuertaAE)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "No"));
                set.OptionConectorList.Add(new Option("AsociadaCotaVariable", "No"));
                set.OptionConectorList.Add(new Option("Puerta", "Sí"));

                //CERRADURAS
                if (set.Code.ToUpper().Contains("COMUN"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PU_CERRADURA PUERTA", "1 punto"));
                }

            }

            setsResult.AddRange(setsCF2HPasivaPuertaAE);
            return setsResult.OrderBy(s => s.Code).ToList();
        }

        #endregion

        #endregion

        #region CORREDERAS

        private List<Set> GetSetsCFCorredera()
        {
            List<Set> setCFCorredera = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.ToUpper().Contains("CORREDERA")).ToList();
            foreach (Set set in setCFCorredera)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));


                if (set.Opening?.Active != null && set.Opening?.Active == "true")
                {
                    set.OptionConectorList.Add(new Option("Activa", "Sí"));
                }
                else if (set.Opening?.Active != null && set.Opening?.Active == "false")
                {
                    set.OptionConectorList.Add(new Option("Activa", "No"));
                    set.OptionConectorList.Add(OpcionHelper.Crear("AGUJA", "Ag8"));
                }
            }

            return setCFCorredera;
        }

        #endregion

        #region PATIO LIFT

        private List<Set> GetSetsCFPatioLift()
        {
            List<Set> setCFPatioLift = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.ToUpper().Contains("PATIO LIFT")).ToList();
            foreach (Set set in setCFPatioLift)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));



                if (set.Code.ToUpper().Contains("1H"))
                {
                    set.OptionConectorList.Add(new Option("Activa", "Sí"));
                    set.OptionConectorList.Add(new Option("Asociada", "Ninguna"));
                }
                else if (set.Code.ToUpper().Contains("2A"))
                {
                    set.OptionConectorList.Add(new Option("Activa", "Sí"));
                    set.OptionConectorList.Add(new Option("Asociada", "Corredera"));
                }
                else if (set.Code.ToUpper().Contains("2P"))
                {
                    set.OptionConectorList.Add(new Option("Activa", "No"));
                }

                if (set.Code.ToUpper().Contains("300/400"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ELV_TIPO", "Estandar 300/400"));
                }
            }

            return setCFPatioLift;
        }

        #endregion

        #region PLEGABLES

        #region PVC
        private List<Set> GetSetsCFPlegables()
        {
            List<Set> setCFPlegables = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.ToUpper().Contains("PLG") &&
                                                                    (s.Code.ToUpper().Contains("(1)") || s.Code.ToUpper().Contains("(1V)"))).ToList();
            foreach (Set set in setCFPlegables)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                if (set.Code.ToUpper().Contains("PLG 1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "1"));
                }
                else if (set.Code.ToUpper().Contains("PLG 2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "2"));
                }
                else if (set.Code.ToUpper().Contains("PLG 3"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "3"));
                }
                else if (set.Code.ToUpper().Contains("PLG 4"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "4"));
                }
                else if (set.Code.ToUpper().Contains("PLG 5"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "5"));
                }
                else if (set.Code.ToUpper().Contains("PLG 6"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "6"));
                }
                else if (set.Code.ToUpper().Contains("PLG 7"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "7"));
                }
                else if (set.Code.ToUpper().Contains("PLG 8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "8"));
                }
                else if (set.Code.ToUpper().Contains("PLG 9"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "9"));
                }
            }

            return setCFPlegables;
        }

        private List<Set> GetSetsCVPlegables()
        {
            List<Set> setCFPlegables = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.ToUpper().Contains("PLG") &&
                                                                    (s.Code.ToUpper().Contains("(2)") || s.Code.ToUpper().Contains("(2V)"))).ToList();
            foreach (Set set in setCFPlegables)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));


                if (set.Code.ToUpper().Contains("PLG 1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "1"));
                }
                else if (set.Code.ToUpper().Contains("PLG 2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "2"));
                }
                else if (set.Code.ToUpper().Contains("PLG 3"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "3"));
                }
                else if (set.Code.ToUpper().Contains("PLG 4"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "4"));
                }
                else if (set.Code.ToUpper().Contains("PLG 5"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "5"));
                }
                else if (set.Code.ToUpper().Contains("PLG 6"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "6"));
                }
                else if (set.Code.ToUpper().Contains("PLG 7"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "7"));
                }
                else if (set.Code.ToUpper().Contains("PLG 8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "8"));
                }
                else if (set.Code.ToUpper().Contains("PLG 9"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "9"));
                }
            }

            return setCFPlegables;
        }

        #endregion

        #region ALU
        private List<Set> GetSetsCFPlegablesALU()
        {
            List<Set> setCFPlegables = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.ToUpper().Contains("PLG") &&
                                                                    (s.Code.ToUpper().Contains("(1A)") || s.Code.ToUpper().Contains("(1AV)"))).ToList();
            foreach (Set set in setCFPlegables)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "No"));


                if (set.Code.ToUpper().Contains("PLG 1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "1"));
                }
                else if (set.Code.ToUpper().Contains("PLG 2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "2"));
                }
                else if (set.Code.ToUpper().Contains("PLG 3"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "3"));
                }
                else if (set.Code.ToUpper().Contains("PLG 4"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "4"));
                }
                else if (set.Code.ToUpper().Contains("PLG 5"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "5"));
                }
                else if (set.Code.ToUpper().Contains("PLG 6"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "6"));
                }
                else if (set.Code.ToUpper().Contains("PLG 7"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "7"));
                }
                else if (set.Code.ToUpper().Contains("PLG 8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "8"));
                }
                else if (set.Code.ToUpper().Contains("PLG 9"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "9"));
                }
            }

            return setCFPlegables;
        }

        private List<Set> GetSetsCVPlegablesALU()
        {
            List<Set> setCFPlegables = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.ToUpper().Contains("PLG") &&
                                                                    (s.Code.ToUpper().Contains("(2A)") || s.Code.ToUpper().Contains("(2AV)"))).ToList();
            foreach (Set set in setCFPlegables)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);

                }
                set.OptionConectorList = new List<Option>();
                set.OptionConectorList.Add(new Option("HardwareSupplier", xmlOrigen.Supplier));
                set.OptionConectorList.Add(new Option("Activa", "Sí"));
                set.OptionConectorList.Add(new Option("CotaVariable", "Sí"));


                if (set.Code.ToUpper().Contains("PLG 1"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "1"));
                }
                else if (set.Code.ToUpper().Contains("PLG 2"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "2"));
                }
                else if (set.Code.ToUpper().Contains("PLG 3"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "3"));
                }
                else if (set.Code.ToUpper().Contains("PLG 4"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "4"));
                }
                else if (set.Code.ToUpper().Contains("PLG 5"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "5"));
                }
                else if (set.Code.ToUpper().Contains("PLG 6"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "6"));
                }
                else if (set.Code.ToUpper().Contains("PLG 7"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "7"));
                }
                else if (set.Code.ToUpper().Contains("PLG 8"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "8"));
                }
                else if (set.Code.ToUpper().Contains("PLG 9"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("PLG_HOJAS PLEGABLE", "9"));
                }
            }

            return setCFPlegables;
        }

        #endregion

        #endregion

        #region ABATIBLES

        private List<Set> GetSetsCVAbatibles()
        {
            List<Set> setCVAbatibles = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2") &&
                                                                    s.Code.ToUpper().Contains("ABATIBLE")).ToList();
            foreach (Set set in setCVAbatibles)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    set.Code.Contains("8") ? OpcionHelper.Crear("AGUJA", "Ag8") : OpcionHelper.Crear("AGUJA", "Ag15")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("BC"))
                {
                    // TODO: ABATIBLES DIFERENCIA ENTRE BC y FC
                    set.OptionConectorList.Add(OpcionHelper.Crear("OPCION_ABATIBLE", "BC"));
                }
                else if (set.Code.ToUpper().Contains("FC"))
                {
                    // TODO: ABATIBLES DIFERENCIA ENTRE BC y FC
                    set.OptionConectorList.Add(OpcionHelper.Crear("OPCION_ABATIBLE", "FC"));
                }
            }

            return setCVAbatibles;
        }

        #endregion

        #region OSCILOPARALELAS

        #region PVC
        private List<Set> GetSetsCVOsciloParalela1H()
        {
            List<Set> setCVOsciloParalelas1H = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2V") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("KS") &&
                                                                    s.Code.ToUpper().Contains("1H")).ToList();
            foreach (Set set in setCVOsciloParalelas1H)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("AsociadaOpuesta", "Ninguna"),
                    new Option("CotaVariable", "Sí"),
                    set.Code.ToUpper().Contains("EM") ? OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si") : OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("100 KS") || set.Code.ToUpper().Contains("100KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-100"));
                }
                else if (set.Code.ToUpper().Contains("160 KS") || set.Code.ToUpper().Contains("160KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-160"));
                }
            }

            return setCVOsciloParalelas1H;
        }

        private List<Set> GetSetsCVOsciloParalela2HActiva()
        {
            List<Set> setCVOsciloParalelas2HActiva = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2V") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("KS") &&
                                                                    s.Code.ToUpper().Contains("2A")).ToList();
            foreach (Set set in setCVOsciloParalelas2HActiva)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("AsociadaOpuesta", "Oscilobatiente"),
                    new Option("CotaVariable", "Sí"),
                    set.Code.ToUpper().Contains("EM") ? OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si") : OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("100 KS") || set.Code.ToUpper().Contains("100KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-100"));
                }
                else if (set.Code.ToUpper().Contains("160 KS") || set.Code.ToUpper().Contains("160KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-160"));
                }

                if (set.Opening?.Right != null && set.Opening?.Right == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Derecha"));
                }
                else if (set.Opening?.Left != null && set.Opening?.Left == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Izquierda"));
                }
            }

            return setCVOsciloParalelas2HActiva;
        }

        private List<Set> GetSetsCVOsciloParalela2HPasiva()
        {
            List<Set> setCVOsciloParalelas2HPasiva = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2V") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("KS") &&
                                                                    s.Code.ToUpper().Contains("2P")).ToList();
            foreach (Set set in setCVOsciloParalelas2HPasiva)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "No"),
                    new Option("AsociadaOpuesta", "Oscilobatiente"),
                    new Option("CotaVariable", "Sí")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("100 KS") || set.Code.ToUpper().Contains("100KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-100"));
                }
                else if (set.Code.ToUpper().Contains("160 KS") || set.Code.ToUpper().Contains("160KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-160"));
                }


                if (set.Opening?.Right != null && set.Opening?.Right == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Izquierda"));
                }
                else if (set.Opening?.Left != null && set.Opening?.Left == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Derecha"));
                }
            }

            return setCVOsciloParalelas2HPasiva;
        }
        #endregion

        #region ALU
        private List<Set> GetSetsCVOsciloParalela1HALU()
        {
            List<Set> setCVOsciloParalelas1H = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2AV") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("KS") &&
                                                                    s.Code.ToUpper().Contains("1H")).ToList();
            foreach (Set set in setCVOsciloParalelas1H)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("AsociadaOpuesta", "Ninguna"),
                    new Option("CotaVariable", "Sí"),
                    set.Code.ToUpper().Contains("EM") ? OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si") : OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("100 KS") || set.Code.ToUpper().Contains("100KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-100"));
                }
                else if (set.Code.ToUpper().Contains("160 KS") || set.Code.ToUpper().Contains("160KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-160"));
                }
            }

            return setCVOsciloParalelas1H;
        }

        private List<Set> GetSetsCVOsciloParalela2HActivaALU()
        {
            List<Set> setCVOsciloParalelas2HActiva = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2AV") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("KS") &&
                                                                    s.Code.ToUpper().Contains("2A")).ToList();
            foreach (Set set in setCVOsciloParalelas2HActiva)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("AsociadaOpuesta", "Oscilobatiente"),
                    new Option("CotaVariable", "Sí"),
                    set.Code.ToUpper().Contains("EM") ? OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si") : OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("100 KS") || set.Code.ToUpper().Contains("100KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-100"));
                }
                else if (set.Code.ToUpper().Contains("160 KS") || set.Code.ToUpper().Contains("160KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-160"));
                }

                if (set.Opening?.Right != null && set.Opening?.Right == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Derecha"));
                }
                else if (set.Opening?.Left != null && set.Opening?.Left == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Izquierda"));
                }
            }

            return setCVOsciloParalelas2HActiva;
        }

        private List<Set> GetSetsCVOsciloParalela2HPasivaALU()
        {
            List<Set> setCVOsciloParalelas2HPasiva = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2AV") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("KS") &&
                                                                    s.Code.ToUpper().Contains("2P")).ToList();
            foreach (Set set in setCVOsciloParalelas2HPasiva)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "No"),
                    new Option("AsociadaOpuesta", "Oscilobatiente"),
                    new Option("CotaVariable", "Sí")
                ];

                set.OptionConectorList = optionList;

                if (set.Code.ToUpper().Contains("100 KS") || set.Code.ToUpper().Contains("100KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-100"));
                }
                else if (set.Code.ToUpper().Contains("160 KS") || set.Code.ToUpper().Contains("160KS"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "KS-160"));
                }


                if (set.Opening?.Right != null && set.Opening?.Right == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Izquierda"));
                }
                else if (set.Opening?.Left != null && set.Opening?.Left == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Derecha"));
                }
            }

            return setCVOsciloParalelas2HPasiva;
        }
        #endregion

        #endregion

        #region PARALELAS CORREDERAS

        #region PVC
        private List<Set> GetSetsCVParalelaCorredera1H()
        {
            List<Set> setCVParalelaCorredera1H = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2V") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("PS") &&
                                                                    s.Code.ToUpper().Contains("1H")).ToList();
            foreach (Set set in setCVParalelaCorredera1H)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("AsociadaOpuesta", "Ninguna"),
                    new Option("CotaVariable", "Sí"),
                    set.Code.ToUpper().Contains("EM") ? OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si") : OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No")
                ];

                set.OptionConectorList = optionList;

                if ((set.Code.ToUpper().Contains("160 PS") || set.Code.ToUpper().Contains("160PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-160"));
                }
                else if ((set.Code.ToUpper().Contains("200 PS") || set.Code.ToUpper().Contains("200PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-160"));
                }
                else if (set.Code.ToUpper().Contains("200") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS Air-160"));
                }
            }

            return setCVParalelaCorredera1H;
        }

        private List<Set> GetSetsCVParalelaCorredera2HActiva()
        {
            List<Set> setCVParalelaCorredera2HActiva = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2V") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("PS") &&
                                                                    s.Code.ToUpper().Contains("2A")).ToList();
            foreach (Set set in setCVParalelaCorredera2HActiva)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("AsociadaOpuesta", "Oscilobatiente"),
                    new Option("CotaVariable", "Sí"),
                    set.Code.ToUpper().Contains("EM") ? OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si") : OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No")
                ];

                set.OptionConectorList = optionList;

                if ((set.Code.ToUpper().Contains("160 PS") || set.Code.ToUpper().Contains("160PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-160"));
                }
                else if ((set.Code.ToUpper().Contains("200 PS") || set.Code.ToUpper().Contains("200PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-160"));
                }
                else if (set.Code.ToUpper().Contains("200") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS Air-160"));
                }

                if (set.Opening?.Right != null && set.Opening?.Right == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Derecha"));
                }
                else if (set.Opening?.Left != null && set.Opening?.Left == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Izquierda"));
                }
            }

            return setCVParalelaCorredera2HActiva;
        }

        private List<Set> GetSetsCVParalelaCorredera2HPasiva()
        {
            List<Set> setCVParalelaCorredera2HPasiva = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2V") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("PS") &&
                                                                    s.Code.ToUpper().Contains("2P")).ToList();
            foreach (Set set in setCVParalelaCorredera2HPasiva)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "No"),
                    new Option("AsociadaOpuesta", "Oscilobatiente"),
                    new Option("CotaVariable", "Sí")
                ];

                set.OptionConectorList = optionList;

                if ((set.Code.ToUpper().Contains("160 PS") || set.Code.ToUpper().Contains("160PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-160"));
                }
                else if ((set.Code.ToUpper().Contains("200 PS") || set.Code.ToUpper().Contains("200PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-160"));
                }
                else if (set.Code.ToUpper().Contains("200") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS Air-160"));
                }

                if (set.Opening?.Right != null && set.Opening?.Right == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Izquierda"));
                }
                else if (set.Opening?.Left != null && set.Opening?.Left == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Derecha"));
                }
            }

            return setCVParalelaCorredera2HPasiva;
        }

        #endregion

        #region ALU
        private List<Set> GetSetsCVParalelaCorredera1HALU()
        {
            List<Set> setCVParalelaCorredera1H = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2AV") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("PS") &&
                                                                    s.Code.ToUpper().Contains("1H")).ToList();
            foreach (Set set in setCVParalelaCorredera1H)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("AsociadaOpuesta", "Ninguna"),
                    new Option("CotaVariable", "Sí"),
                    set.Code.ToUpper().Contains("EM") ? OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si") : OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No")
                ];

                set.OptionConectorList = optionList;

                if ((set.Code.ToUpper().Contains("160 PS") || set.Code.ToUpper().Contains("160PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-160"));
                }
                else if ((set.Code.ToUpper().Contains("200 PS") || set.Code.ToUpper().Contains("200PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-160"));
                }
                else if (set.Code.ToUpper().Contains("200") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS Air-160"));
                }
            }

            return setCVParalelaCorredera1H;
        }

        private List<Set> GetSetsCVParalelaCorredera2HActivaALU()
        {
            List<Set> setCVParalelaCorredera2HActiva = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2AV") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("PS") &&
                                                                    s.Code.ToUpper().Contains("2A")).ToList();
            foreach (Set set in setCVParalelaCorredera2HActiva)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "Sí"),
                    new Option("AsociadaOpuesta", "Oscilobatiente"),
                    new Option("CotaVariable", "Sí"),
                    set.Code.ToUpper().Contains("EM") ? OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_Si") : OpcionHelper.Crear("NX_EASY MIX", "Easy Mix_No")
                ];

                set.OptionConectorList = optionList;

                if ((set.Code.ToUpper().Contains("160 PS") || set.Code.ToUpper().Contains("160PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-160"));
                }
                else if ((set.Code.ToUpper().Contains("200 PS") || set.Code.ToUpper().Contains("200PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-160"));
                }
                else if (set.Code.ToUpper().Contains("200") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS Air-160"));
                }

                if (set.Opening?.Right != null && set.Opening?.Right == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Derecha"));
                }
                else if (set.Opening?.Left != null && set.Opening?.Left == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Izquierda"));
                }
            }

            return setCVParalelaCorredera2HActiva;
        }

        private List<Set> GetSetsCVParalelaCorredera2HPasivaALU()
        {
            List<Set> setCVParalelaCorredera2HPasiva = xmlOrigen.SetList.OrderBy(x => x.Code)
                                                        .Where(s => s.Code.StartsWith("(2AV") &&
                                                                    s.Code.ToUpper().Contains("ALV") &&
                                                                    s.Code.ToUpper().Contains("PS") &&
                                                                    s.Code.ToUpper().Contains("2P")).ToList();
            foreach (Set set in setCVParalelaCorredera2HPasiva)
            {
                if (set.Opening != null)
                {
                    set.OpeningFlagConectorList = GetOpeningOptions(set.Opening);
                }
                List<Option> optionList =
                [
                    new Option("HardwareSupplier", xmlOrigen.Supplier),
                    new Option("Activa", "No"),
                    new Option("AsociadaOpuesta", "Oscilobatiente"),
                    new Option("CotaVariable", "Sí")
                ];

                set.OptionConectorList = optionList;

                if ((set.Code.ToUpper().Contains("160 PS") || set.Code.ToUpper().Contains("160PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-160"));
                }
                else if ((set.Code.ToUpper().Contains("200 PS") || set.Code.ToUpper().Contains("200PS")) && !set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-160"));
                }
                else if (set.Code.ToUpper().Contains("200") && set.Code.ToUpper().Contains("AIRCOM"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS AirCom-200"));
                }
                else if (set.Code.ToUpper().Contains("160") && set.Code.ToUpper().Contains("AIR"))
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_TIPO", "PS Air-160"));
                }

                if (set.Opening?.Right != null && set.Opening?.Right == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Izquierda"));
                }
                else if (set.Opening?.Left != null && set.Opening?.Left == "true")
                {
                    set.OptionConectorList.Add(OpcionHelper.Crear("ALV_ESQ.C", "Esq. C Derecha"));
                }
            }

            return setCVParalelaCorredera2HPasiva;
        }

        #endregion
        #endregion

        private string GetEquivalenciaAI(string setCode)
        {
            // Quitamos "AE " de la cadena de búsqueda
            string codeAI = setCode.Replace("AE ", "");
            Set setEquivalente = xmlOrigen.SetList.Where(s => s.Code == codeAI).FirstOrDefault();
            if (setEquivalente != null)
            {
                return setEquivalente.Code;
            }
            else
            {
                return setCode;
            }

        }
        #endregion

        private void lbl_Xml_Click(object sender, EventArgs e)
        {

        }
    }
}
