using iTextSharp.text;
using iTextSharp.text.pdf;
using RotoEntities;
using System.Reflection;
using System.Xml;

namespace RotoTools
{
    public partial class ControlCambiosMenu : Form
    {
        XmlData xmlOrigen = new();
        XmlData xmlNuevo = new();
        bool xmlOrigenCargado = false;
        bool xmlNuevoCargado = false;
        private XmlNamespaceManager nsmgr;
        public bool compareColours { get; set; } = true;
        public ComparaSetsProperties compareSets { get; set; } = new ComparaSetsProperties();
        public ComparaFittingsProperties compareFittings { get; set; } = new ComparaFittingsProperties();
        public bool compareOptions { get; set; } = true;
        public bool compareFittingGroups { get; set; } = true;

        private enum enumTipoXml
        {
            origen = 0,
            nuevo = 1
        }
        public enum enumTipoDiferencia
        {
            opcionGlobal = 1,
            descripcionFitting = 2,
            cambioReferenciaOpcion = 3,
            fittingNoExistente = 4,
            manufacturerDistinto = 5,
            opcionFittingNoGenerada = 6,
            referenciaNoGeneradaFitting = 7,
            setsDiferentes = 8,
            atributosSetDiferente = 9,
            openingSetDiferente = 10,
            setDescriptionDiferente = 11,
            colorDiferente = 12,
            fittingGroupDiferente = 13,
            locationFittingDistinto = 14,
            lengthFittingDistinto = 15
        }
        public enum enumSeveridadDiferencia
        {
            warning = 1,
            error = 2
        }
        public enum enumOrigenXMLDiferencia
        {
            anterior = 1,
            actual = 2,
            ambos = 3
        }
        public ControlCambiosMenu()
        {
            InitializeComponent();
            InitializeCompareFittings();
            InitializeCompareSets();
        }

        private void InitializeCompareFittings()
        {
            compareFittings.compararFittings = true;
            compareFittings.compararFittingsManufacturer = true;
            compareFittings.compararFittingsLength = true;
            compareFittings.compararFittingsLocation = true;
            compareFittings.compararFittingsDescription = true;
            compareFittings.compararFittingsArticles = true;
        }
        private void InitializeCompareSets()
        {
            compareSets.compararSets = true;
            compareSets.compararCantidadSetDescriptions = true;
        }

        private XmlData LoadXml(string xmlPath, int tipoXml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);

                nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

                XmlData xmlData = new XmlData();

                xmlData.FittingGroupList = LoadFittingGroups(doc, tipoXml);
                xmlData.ColourList = LoadColourMaps(doc, tipoXml);
                xmlData.OptionList = LoadDocOptions(doc, tipoXml);
                xmlData.FittingList = LoadFittings(doc, xmlData.FittingGroupList, tipoXml);
                xmlData.SetList = LoadSets(doc, xmlData.FittingList, tipoXml);
                xmlData.FittingsVersion = LoadFittingsVersion(doc);

                return xmlData;
            }
            catch
            {
                return null;
            }

        }
        private string LoadFittingsVersion(XmlDocument doc)
        {
            // Navega al nodo Fittings
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

            XmlNode fittingsNode = doc.SelectSingleNode("//hw:Fittings", nsmgr);

            if (fittingsNode != null)
            {
                foreach (XmlNode node in fittingsNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        string comentario = node.Value?.Trim();

                        if (!string.IsNullOrEmpty(comentario) && comentario.StartsWith("F:"))
                        {
                            // Quitar la parte final '#' si está
                            int fin = comentario.IndexOf('#');
                            return fin >= 0 ? comentario.Substring(2, fin - 2) : comentario.Substring(2);
                        }
                    }
                }
            }

            return "";
        }

        private List<FittingGroup> LoadFittingGroups(XmlDocument doc, int tipoXml)
        {
            try
            {
                List<FittingGroup> fittingGroupsList = new List<FittingGroup>();
                XmlNodeList nodosFittingGroupList = doc.SelectNodes("//hw:FittingGroups/hw:FittingGroup", nsmgr);

                foreach (XmlNode fittingGroupNode in nodosFittingGroupList)
                {
                    FittingGroup fittingGroup = new FittingGroup();
                    fittingGroup.Id = TryParseInt(fittingGroupNode.Attributes["id"]?.Value);
                    fittingGroup.Class = fittingGroupNode.Attributes["class"]?.Value;

                    ShowLoadingInfo("FittingGroup", fittingGroup.Class, tipoXml);

                    fittingGroupsList.Add(fittingGroup);
                }

                return fittingGroupsList;
            }
            catch
            {
                return null;
            }
        }

        private List<Colour> LoadColourMaps(XmlDocument doc, int tipoXml)
        {
            try
            {
                List<Colour> colourList = new List<Colour>();
                XmlNodeList nodosColourList = doc.SelectNodes("//hw:ColourMaps/hw:Colour", nsmgr);

                foreach (XmlNode colourNode in nodosColourList)
                {
                    Colour colour = new Colour();
                    colour.Name = colourNode.Attributes["name"]?.Value;

                    ShowLoadingInfo("Colour", colour.Name, tipoXml);

                    colour.ArticleList = GetArticles(colourNode);
                    colourList.Add(colour);
                }

                return colourList;
            }
            catch
            {
                return null;
            }
        }

        private List<Article> GetArticles(XmlNode parentNode)
        {
            try
            {
                List<Article> articlesList = new List<Article>();
                foreach (XmlNode articleNode in parentNode.ChildNodes)
                {
                    if (articleNode.Attributes == null)
                    {
                        continue;
                    }
                    if (articleNode.Name == "hw:Generation" && articleNode.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode childNodeFitting in articleNode.ChildNodes)
                        {
                            if (childNodeFitting.Name == "hw:Articles")
                            {
                                foreach (XmlNode generationArticleNode in childNodeFitting.ChildNodes)
                                {
                                    if (generationArticleNode.Attributes == null)
                                    {
                                        continue;
                                    }
                                    Article articleGeneration = new Article();
                                    articleGeneration.Ref = generationArticleNode?.Attributes["ref"]?.Value;
                                    articleGeneration.Final = generationArticleNode?.Attributes["final"]?.Value;
                                    articleGeneration.Side = generationArticleNode?.Attributes["side"]?.Value;
                                    articleGeneration.Location = generationArticleNode?.Attributes["location"]?.Value;
                                    articleGeneration.XPosition = TryParseDouble(generationArticleNode?.Attributes["x"]?.Value);
                                    articleGeneration.ReferencePoint = generationArticleNode?.Attributes["referencePoint"]?.Value;
                                    articleGeneration.OptionList = LoadArticleOptions(generationArticleNode);
                                    articlesList.Add(articleGeneration);
                                }
                            }
                        }
                    }
                    else
                    {
                        Article article = new Article();
                        article.Ref = articleNode?.Attributes["ref"]?.Value;
                        article.Final = articleNode?.Attributes["final"]?.Value;
                        articlesList.Add(article);
                    }
                }

                return articlesList;
            }
            catch
            {
                return new List<Article>();
            }
        }

        private List<Operation> GetOperations(XmlNode parentNode)
        {
            try
            {
                List<Operation> operationsList = new List<Operation>();
                foreach (XmlNode articleNode in parentNode.ChildNodes)
                {
                    if (articleNode.Attributes == null || articleNode.ChildNodes.Count == 0)
                    {
                        continue;
                    }
                    if (articleNode.Name == "hw:Generation")
                    {
                        foreach (XmlNode childNodeFitting in articleNode.ChildNodes)
                        {
                            if (childNodeFitting.Name == "hw:Operations")
                            {
                                foreach (XmlNode generationOperationNode in childNodeFitting.ChildNodes)
                                {
                                    if (generationOperationNode.Attributes == null)
                                    {
                                        continue;
                                    }

                                    Operation operation = new Operation();
                                    operation.Name = generationOperationNode?.Attributes["name"]?.Value;
                                    operation.XPosition = generationOperationNode?.Attributes["x"]?.Value;
                                    operation.ReferencePoint = generationOperationNode?.Attributes["referencePoint"]?.Value;
                                    operation.Location = generationOperationNode?.Attributes["location"]?.Value;
                                    operationsList.Add(operation);
                                }
                            }
                        }
                    }
                }

                return operationsList;
            }
            catch
            {
                return new List<Operation>();
            }
        }

        private List<Option> LoadArticleOptions(XmlNode generationArticleNode)
        {
            List<Option> optionList = new List<Option>();
            try
            {
                foreach (XmlNode optionNode in generationArticleNode.ChildNodes)
                {
                    if (optionNode.Attributes == null)
                    {
                        continue;
                    }

                    Option option = new Option();
                    option.Name = optionNode.Attributes["Name"]?.Value;
                    option.Value = optionNode.Attributes["Value"]?.Value;

                    optionList.Add(option);
                }

                return optionList;
            }
            catch
            {
                return new List<Option>();
            }
        }

        private List<Option> LoadDocOptions(XmlDocument doc, int tipoXml)
        {
            try
            {
                List<Option> optionList = new List<Option>();
                XmlNodeList nodosOptionsList = doc.SelectNodes("//hw:Options/hw:Option", nsmgr);
                if (nodosOptionsList != null)
                {
                    foreach (XmlNode optionNode in nodosOptionsList)
                    {
                        Option option = new Option();
                        option.Name = optionNode.Attributes["Name"]?.Value;

                        ShowLoadingInfo("Option", option.Name, tipoXml);
                        option.ValuesList = GetOptionValues(optionNode);
                        optionList.Add(option);
                    }
                }

                return optionList;
            }
            catch
            {
                return null;
            }
        }

        private List<Fitting> LoadFittings(XmlDocument doc, List<FittingGroup> fittingGroupList, int tipoXml)
        {
            try
            {
                List<Fitting> fittingList = new List<Fitting>();
                XmlNodeList nodosFittingList = doc.SelectNodes("//hw:Fittings/hw:Fitting", nsmgr);
                int i = 0;
                foreach (XmlNode fittingNode in nodosFittingList)
                {
                    i++;
                    Fitting fitting = new Fitting();
                    fitting.Id = TryParseInt(fittingNode.Attributes["id"]?.Value);
                    fitting.Ref = fittingNode.Attributes["ref"]?.Value;
                    fitting.Description = fittingNode.Attributes["Description"]?.Value;
                    fitting.Manufacturer = fittingNode.Attributes["manufacturer"]?.Value;
                    fitting.FittingGroupId = int.Parse(fittingNode.Attributes["fittingGroupId"]?.Value);
                    fitting.Location = fittingNode.Attributes["location"]?.Value;
                    fitting.FittingType = fittingNode.Attributes["fittingType"]?.Value;
                    fitting.System = fittingNode.Attributes["system"]?.Value;
                    fitting.HandUseable = fittingNode.Attributes["handUseable"]?.Value;
                    fitting.Lenght = TryParseDouble(fittingNode.Attributes["length"]?.Value);
                    fitting.StartCuttable = TryParseBool(fittingNode.Attributes["StartCuttable"]?.Value);
                    fitting.EndCuttable = TryParseBool(fittingNode.Attributes["EndCuttable"]?.Value);
                    fitting.FittingGroup = fittingGroupList.FirstOrDefault(g => g.Id == fitting.FittingGroupId);
                    ShowLoadingInfo("Fitting", fitting.Description, tipoXml);
                    fitting.ArticleList = GetArticles(fittingNode);
                    fitting.OperationList = GetOperations(fittingNode);
                    fittingList.Add(fitting);
                }

                return fittingList;
            }
            catch
            {
                return null;
            }
        }

        private List<Set> LoadSets(XmlDocument doc, List<Fitting> fittingList, int tipoXml)
        {
            try
            {
                List<Set> setList = new List<Set>();
                XmlNodeList nodosSetList = doc.SelectNodes("//hw:Sets/hw:Set", nsmgr);

                foreach (XmlNode setNode in nodosSetList)
                {
                    Set set = new Set();
                    set.Id = setNode.Attributes["id"]?.Value;
                    set.Code = setNode.Attributes["code"]?.Value;
                    set.Movement = setNode.Attributes["movement"]?.Value;
                    set.Associated = setNode.Attributes["associated"]?.Value;
                    set.MinWidth = setNode.Attributes["minWidth"]?.Value;
                    set.MaxWidth = setNode.Attributes["maxWidth"]?.Value;
                    set.MinHeight = setNode.Attributes["minHeight"]?.Value;
                    set.MaxHeight = setNode.Attributes["maxHeight"]?.Value;
                    set.Opening = GetSetOpening(setNode);
                    set.SetDescriptionList = GetSetDescripcionList(setNode, fittingList);
                    ShowLoadingInfo("Set", set.Code, tipoXml);

                    setList.Add(set);
                }

                return setList;
            }
            catch
            {
                return null;
            }
        }

        private Opening GetSetOpening(XmlNode setNode)
        {
            try
            {
                XmlNode openingNode = setNode.SelectSingleNode("hw:Opening", nsmgr);
                if (openingNode != null)
                {
                    Opening opening = new Opening();
                    opening.Active = openingNode.Attributes["active"]?.Value;
                    opening.Turn = openingNode.Attributes["turn"]?.Value;
                    opening.Right = openingNode.Attributes["right"]?.Value;
                    opening.Left = openingNode.Attributes["left"]?.Value;
                    return opening;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private List<SetDescription> GetSetDescripcionList(XmlNode setNode, List<Fitting> fittingList)
        {
            List<SetDescription> setDescriptionsList = new List<SetDescription>();
            try
            {
                XmlNodeList setDescriptions = setNode.SelectNodes("hw:SetDescription", nsmgr);
                foreach (XmlNode descNode in setDescriptions)
                {
                    var attrs = descNode.Attributes;
                    var setDescription = new SetDescription
                    {
                        Id = TryParseInt(attrs["id"]?.Value),
                        FittingId = TryParseInt(attrs["fittingId"]?.Value),
                        MinHeight = TryParseDouble(attrs["minHeight"]?.Value),
                        MaxHeight = TryParseDouble(attrs["maxHeight"]?.Value),
                        MinWidth = TryParseDouble(attrs["minWidth"]?.Value),
                        MaxWidth = TryParseDouble(attrs["maxWidth"]?.Value),
                        Horizontal = TryParseBool(attrs["horizontal"]?.Value),
                        Position = TryParseInt(attrs["position"]?.Value),
                        ReferencePoint = attrs["referencePoint"]?.Value,
                        ChainPosition = TryParseInt(attrs["chainPosition"]?.Value),
                        Movement = attrs["movement"]?.Value,
                        Inverted = TryParseBool(attrs["inverted"]?.Value),
                        XPosition = TryParseDouble(attrs["x"]?.Value),
                        Alternative = TryParseInt(attrs["alternative"]?.Value),
                        OptionList = LoadSetDescriptionOptions(descNode)
                    };

                    setDescription.Fitting = fittingList.FirstOrDefault(f => f.Id == setDescription.FittingId);
                    setDescriptionsList.Add(setDescription);
                }

                return setDescriptionsList;
            }
            catch
            {
                return null;
            }
        }

        private List<Option> LoadSetDescriptionOptions(XmlNode setDescriptionNode)
        {
            List<Option> optionList = new List<Option>();
            try
            {
                foreach (XmlNode optionNode in setDescriptionNode.ChildNodes)
                {
                    if (optionNode.Attributes == null)
                    {
                        continue;
                    }

                    Option option = new Option();
                    option.Name = optionNode.Attributes["Name"]?.Value;
                    option.Value = optionNode.Attributes["Value"]?.Value;

                    optionList.Add(option);
                }

                return optionList;
            }
            catch
            {
                return new List<Option>();
            }
        }

        private List<Value> GetOptionValues(XmlNode optionNode)
        {
            List<Value> valueList = new List<Value>();
            foreach (XmlNode valueNode in optionNode.ChildNodes)
            {
                if (valueNode.Attributes == null)
                {
                    continue;
                }
                Value value = new Value();
                value.Valor = valueNode?.Attributes["Value"]?.Value;
                valueList.Add(value);
            }

            return valueList;
        }

        private void btn_SelectXml1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                EnableButtons(false);
                string rutaXml = openFileDialog.FileName;
                xmlOrigen = LoadXml(rutaXml, (int)enumTipoXml.origen);
                lbl_Xml1.Text = rutaXml;
                EnableButtons(true);
                xmlOrigenCargado = true;
                FillListasConfiguracion();
            }
        }

        private void EnableButtons(bool enable)
        {
            btn_SelectXml1.Enabled = enable;
            btn_SelectXml2.Enabled = enable;
            btn_Compare.Enabled = enable;
        }

        private void ShowLoadingInfo(string type, string value, int tipoXml)
        {
            string texto = "Cargando... " + type + " " + value.TrimEnd();

            switch (tipoXml)
            {
                case (int)enumTipoXml.origen:
                    lbl_Xml1.Visible = true;
                    lbl_Xml1.Text = texto;
                    //lbl_Xml1.AutoSize = true;
                    Application.DoEvents();
                    break;
                case (int)enumTipoXml.nuevo:
                    lbl_Xml2.Visible = true;
                    lbl_Xml2.Text = texto;
                    //lbl_Xml2.AutoSize = true;
                    Application.DoEvents();
                    break;
            }
        }

        // Métodos auxiliares seguros
        private int TryParseInt(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }
        private double TryParseDouble(string value)
        {
            return double.TryParse(value, out double result) ? result : 0;
        }
        private bool TryParseBool(string value)
        {
            return bool.TryParse(value, out bool result) ? result : false;
        }
        private void btn_SelectXml2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog.Title = "Selecciona XML";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                EnableButtons(false);
                string rutaXml = openFileDialog.FileName;
                xmlNuevo = LoadXml(rutaXml, (int)enumTipoXml.nuevo);
                lbl_Xml2.Text = rutaXml;
                EnableButtons(true);
                xmlNuevoCargado = true;
                FillListasConfiguracion();
            }
        }
        private void btn_Compare_Click(object sender, EventArgs e)
        {
            if (xmlOrigenCargado && xmlNuevoCargado)
            {
                List<DiferenciaXml> diferenciaslist = CompareXmlData(xmlOrigen, xmlNuevo);

                if (diferenciaslist.Count > 0)
                {
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                        saveFileDialog.Title = "Guardar informe de diferencias";
                        saveFileDialog.FileName = "InformeDiferencias.pdf";

                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            string outputPath = saveFileDialog.FileName;
                            GeneratePdf(outputPath, diferenciaslist);
                            MessageBox.Show("Comparación finalizada. Se generó el PDF.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No se encontraron diferencias.");
                }
            }
        }
        private void WriteInPdf(string titulo, int diferenceType, iTextSharp.text.Document doc, iTextSharp.text.Font titleFont, iTextSharp.text.Font textFont, List<DiferenciaXml> diferenciasList)
        {
            List<DiferenciaXml> diferencias = new List<DiferenciaXml>();
            diferencias.AddRange(diferenciasList.Where(op => op.Tipo == diferenceType));
            if (diferencias.Count() > 0)
            {
                doc.Add(new Paragraph(titulo, titleFont));
                switch (diferenceType)
                {
                    case (int)enumTipoDiferencia.colorDiferente:
                        WriteInPdfColors(titulo, diferenceType, doc, titleFont, textFont, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.opcionGlobal:
                        WriteInPdfOptions(titulo, diferenceType, doc, titleFont, textFont, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.setDescriptionDiferente:
                        WriteInPdfSetDescriptions(titulo, diferenceType, doc, titleFont, textFont, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.descripcionFitting:
                        WriteInPdfFittingDescriptions(titulo, diferenceType, doc, titleFont, textFont, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.fittingNoExistente:
                        WriteInPdfFittingNoExistente(titulo, diferenceType, doc, titleFont, textFont, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.cambioReferenciaOpcion:
                        WriteInPdfFittingOptions(doc, diferenceType, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.referenciaNoGeneradaFitting:
                        WriteInPdfGeneracionDiferentesArticulos(doc, diferenceType, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.atributosSetDiferente:
                        WriteInPdfAtributosSets(doc, diferenceType, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.setsDiferentes:
                        WriteInPdfSets(doc, diferenceType, diferenciasList);
                        break;
                    case (int)enumTipoDiferencia.openingSetDiferente:
                        WriteInPdfOpening(doc, diferenceType, diferenciasList);
                        break;
                    default:
                        foreach (DiferenciaXml diferencia in diferencias)
                        {
                            doc.Add(new Paragraph(diferencia.Descripcion, textFont));
                        }
                        doc.Add(Chunk.NEWLINE);
                        break;
                }
            }
        }
        private void WriteInPdfSetDescriptions(string titulo, int diferenceType, iTextSharp.text.Document doc, iTextSharp.text.Font titleFont, iTextSharp.text.Font textFont, List<DiferenciaXml> diferenciasList)
        {
            List<DiferenciaXml> diferencias = new List<DiferenciaXml>();
            diferencias.AddRange(diferenciasList.Where(op => op.Tipo == diferenceType));
            if (diferencias.Count() > 0)
            {
                // Fuentes
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
                var setFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(0, 102, 204));
                var textFont2 = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);

                string tituloSetDescription = "";
                int origenDiferencia = -1;

                foreach (DiferenciaXml diferencia in diferencias
                         .OrderBy(d => d.Titulo)
                         .ThenBy(c => c.OrigenDiferencia)
                         .ThenBy(x => x.DetalleDiferenciaDescription))
                {
                    // Nuevo SET
                    if (tituloSetDescription != diferencia.Titulo)
                    {
                        //doc.Add(Chunk.NEWLINE);
                        doc.Add(new Paragraph("SET: " + diferencia.Titulo, setFont));
                        origenDiferencia = -1;
                    }

                    // Nuevo bloque según origen (Anterior / Actual)
                    if (origenDiferencia != diferencia.OrigenDiferencia)
                    {
                        string label = diferencia.OrigenDiferencia == (int)enumOrigenXMLDiferencia.anterior
                            ? "Solo en XML Anterior:"
                            : "Solo en XML Actual:";

                        var color = diferencia.OrigenDiferencia == (int)enumOrigenXMLDiferencia.anterior
                            ? BaseColor.RED
                            : new BaseColor(0, 128, 0);

                        var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, color);
                        doc.Add(new Paragraph("  - " + label, labelFont));
                    }

                    // Crear tabla para la diferencia
                    PdfPTable table = new PdfPTable(2);
                    table.WidthPercentage = 90;
                    table.SpacingBefore = 5f;
                    table.SpacingAfter = 5f;
                    table.SetWidths(new float[] { 25f, 75f });

                    // Encabezado: SetDescriptionId
                    PdfPCell headerCell = new PdfPCell(new Phrase("SetDescriptionId", subtituloFont));
                    headerCell.BackgroundColor = new BaseColor(230, 230, 230);
                    headerCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(headerCell);

                    PdfPCell idCell = new PdfPCell(new Phrase(diferencia.DetalleDiferenciaDescription, textFont2));
                    idCell.BackgroundColor = new BaseColor(245, 245, 245);
                    idCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    table.AddCell(idCell);

                    // Artículo
                    if (!string.IsNullOrEmpty(diferencia.DetalleDiferenciaArticulo))
                    {
                        PdfPCell artHeader = new PdfPCell(new Phrase("Artículo", subtituloFont));
                        artHeader.BackgroundColor = new BaseColor(230, 230, 230);
                        table.AddCell(artHeader);

                        PdfPCell artValue = new PdfPCell(new Phrase(diferencia.DetalleDiferenciaArticulo, textFont2));
                        artValue.BackgroundColor = BaseColor.WHITE;
                        table.AddCell(artValue);
                    }

                    // Atributos
                    if (!string.IsNullOrEmpty(diferencia.DetalleDiferenciaAtributos))
                    {
                        PdfPCell attrHeader = new PdfPCell(new Phrase("Atributos", subtituloFont));
                        attrHeader.BackgroundColor = new BaseColor(230, 230, 230);
                        table.AddCell(attrHeader);

                        // Opcional: dividir atributos en varias líneas
                        string[] partes = diferencia.DetalleDiferenciaAtributos
                            .Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                        PdfPCell attrValue = new PdfPCell(new Phrase(string.Join("\n", partes), textFont2));
                        attrValue.BackgroundColor = BaseColor.WHITE;
                        table.AddCell(attrValue);
                    }

                    doc.Add(table);

                    origenDiferencia = diferencia.OrigenDiferencia;
                    tituloSetDescription = diferencia.Titulo;
                }
            }
        }
        private void WriteInPdfColors(string titulo, int diferenceType, iTextSharp.text.Document doc, iTextSharp.text.Font titleFont, iTextSharp.text.Font textFont2, List<DiferenciaXml> diferenciasList)
        {
            List<DiferenciaXml> diferencias = new List<DiferenciaXml>();
            diferencias.AddRange(diferenciasList.Where(op => op.Tipo == diferenceType));
            if (diferencias.Count() > 0)
            {
                // Fuentes y colores
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                var headerBackground = new BaseColor(230, 230, 230);

                string tituloActual = "";
                int origenActual = -1;

                var diffs = diferencias
                    .Where(d => d.Tipo == diferenceType)
                    .OrderBy(d => d.Titulo)
                    .ThenBy(d => d.OrigenDiferencia)
                    .ThenBy(d => d.Descripcion);

                foreach (DiferenciaXml diferencia in diffs)
                {
                    // Cabecera según origen
                    if (origenActual != diferencia.OrigenDiferencia)
                    {
                        string label;
                        BaseColor labelColor;

                        if (diferencia.OrigenDiferencia == (int)enumOrigenXMLDiferencia.anterior)
                        {
                            label = "Referencias eliminadas";
                            labelColor = BaseColor.RED;
                        }
                        else if (diferencia.OrigenDiferencia == (int)enumOrigenXMLDiferencia.actual)
                        {
                            label = "Nuevas referencias";
                            labelColor = new BaseColor(0, 128, 0);
                        }
                        else // Ambos
                        {
                            label = "Modificaciones";
                            labelColor = BaseColor.BLUE;
                        }

                        var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, labelColor);

                        Paragraph origenPara = new Paragraph("  - " + label, labelFont);
                        origenPara.SpacingBefore = 6f;
                        origenPara.SpacingAfter = 4f;
                        doc.Add(origenPara);

                        origenActual = diferencia.OrigenDiferencia;
                    }

                    if (diferencia.OrigenDiferencia == (int)enumOrigenXMLDiferencia.ambos)
                    {
                        // Tabla comparativa con 3 columnas
                        PdfPTable table = new PdfPTable(3);
                        table.WidthPercentage = 92;
                        table.SpacingBefore = 3f;
                        table.SpacingAfter = 6f;
                        table.SetWidths(new float[] { 30f, 35f, 35f });

                        // Encabezados
                        var hCampo = new PdfPCell(new Phrase("Campo", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f };
                        var hXml1 = new PdfPCell(new Phrase("Referencia anterior", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f };
                        var hXml2 = new PdfPCell(new Phrase("Referencia actual", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f };
                        table.AddCell(hCampo);
                        table.AddCell(hXml1);
                        table.AddCell(hXml2);

                        // Aquí debes parsear los detalles de diferencia para mostrar el cambio en columnas
                        // Ejemplo: si DetalleDiferenciaAtributos = "Final=123 vs Final=456"
                        var partes = diferencia.DetalleDiferenciaAtributos?.Split(new[] { "," }, StringSplitOptions.None);
                        if (partes != null && partes.Length == 2)
                        {
                            table.AddCell(new PdfPCell(new Phrase("Referencia Final", textFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(partes[0], textFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(partes[1], textFont)) { Padding = 5f });
                        }

                        doc.Add(table);
                    }
                    else
                    {
                        // Formato simple de 2 columnas para anterior / actual
                        PdfPTable table = new PdfPTable(2);
                        table.WidthPercentage = 92;
                        table.SpacingBefore = 3f;
                        table.SpacingAfter = 6f;
                        table.SetWidths(new float[] { 30f, 70f });

                        // Artículo
                        if (!string.IsNullOrEmpty(diferencia.DetalleDiferenciaArticulo))
                        {
                            var artHeader = new PdfPCell(new Phrase("Artículo", subtituloFont));
                            artHeader.BackgroundColor = headerBackground;
                            artHeader.Padding = 5f;
                            table.AddCell(artHeader);

                            var artValue = new PdfPCell(new Phrase(diferencia.DetalleDiferenciaArticulo, textFont));
                            artValue.Padding = 5f;
                            table.AddCell(artValue);
                        }

                        // Atributos
                        if (!string.IsNullOrEmpty(diferencia.DetalleDiferenciaAtributos))
                        {
                            var attrHeader = new PdfPCell(new Phrase("Colores", subtituloFont));
                            attrHeader.BackgroundColor = headerBackground;
                            attrHeader.Padding = 5f;
                            table.AddCell(attrHeader);

                            var attrValue = new PdfPCell(new Phrase(diferencia.DetalleDiferenciaAtributos, textFont));
                            attrValue.Padding = 5f;
                            table.AddCell(attrValue);
                        }

                        doc.Add(table);
                    }
                }
            }
        }
        private void WriteInPdfOptions(string titulo, int diferenceType, iTextSharp.text.Document doc, iTextSharp.text.Font titleFont, iTextSharp.text.Font textFont2, List<DiferenciaXml> diferenciasList)
        {
            List<DiferenciaXml> diferencias = new List<DiferenciaXml>();
            diferencias.AddRange(diferenciasList.Where(op => op.Tipo == diferenceType));
            if (diferencias.Count() > 0)
            {
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                var headerBackground = new BaseColor(230, 230, 230);

                string currentOption = "";
                int currentOrigen = -1;

                foreach (var dif in diferencias
                    .Where(d => d.Tipo == diferenceType)
                    .OrderBy(d => d.DetalleDiferenciaDescription)
                    .ThenBy(d => d.OrigenDiferencia))
                {
                    // Cabecera según origen
                    if (currentOrigen != dif.OrigenDiferencia)
                    {
                        string label;
                        BaseColor labelColor;

                        if (dif.OrigenDiferencia == (int)enumOrigenXMLDiferencia.anterior)
                        {
                            label = "Solo en XML Anterior:";
                            labelColor = BaseColor.RED;
                        }
                        else if (dif.OrigenDiferencia == (int)enumOrigenXMLDiferencia.actual)
                        {
                            label = "Solo en XML Actual:";
                            labelColor = new BaseColor(0, 128, 0);
                        }
                        else
                        {
                            label = "En ambos XMLs (con diferencias):";
                            labelColor = BaseColor.BLUE;
                        }

                        var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, labelColor);
                        Paragraph origenPara = new Paragraph("  - " + label, labelFont)
                        {
                            SpacingBefore = 6f,
                            SpacingAfter = 4f
                        };
                        doc.Add(origenPara);
                    }

                    if (dif.OrigenDiferencia == (int)enumOrigenXMLDiferencia.ambos)
                    {
                        PdfPTable table = new PdfPTable(3);
                        table.WidthPercentage = 92;
                        table.SetWidths(new float[] { 30f, 35f, 35f });

                        // Cabeceras SOLO en este caso
                        table.AddCell(new PdfPCell(new Phrase("Opción", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase("XML Anterior", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase("XML Actual", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });

                        var partes = dif.DetalleDiferenciaAtributos?.Split(new[] { " vs " }, StringSplitOptions.None);
                        if (partes != null && partes.Length == 2)
                        {
                            table.AddCell(new PdfPCell(new Phrase(dif.DetalleDiferenciaDescription, textFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(partes[0], textFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(partes[1], textFont)) { Padding = 5f });
                        }

                        doc.Add(table);
                    }
                    else
                    {
                        // Creamos tabla con cabeceras SOLO una vez por bloque (no por cada fila)
                        PdfPTable table = new PdfPTable(2);
                        table.WidthPercentage = 92;
                        table.SetWidths(new float[] { 30f, 70f });

                        // Solo añadimos cabeceras si es la primera fila del bloque
                        if (currentOrigen != dif.OrigenDiferencia)
                        {
                            table.AddCell(new PdfPCell(new Phrase("Opción", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase("Valor", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                        }

                        // Fila con datos
                        table.AddCell(new PdfPCell(new Phrase(dif.DetalleDiferenciaDescription, textFont)) { Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase(dif.DetalleDiferenciaAtributos, textFont)) { Padding = 5f });

                        doc.Add(table);
                    }
                    currentOrigen = dif.OrigenDiferencia;
                }
            }
        }
        private void WriteInPdfFittingDescriptions(string titulo, int diferenceType, iTextSharp.text.Document doc,
                                           iTextSharp.text.Font titleFont, iTextSharp.text.Font textFont2,
                                           List<DiferenciaXml> diferenciasList)
        {
            var diferencias = diferenciasList.Where(op => op.Tipo == diferenceType).ToList();

            if (diferencias.Count > 0)
            {
                // Fuentes y colores
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                var headerBackground = new BaseColor(230, 230, 230);

                // Crear tabla con 3 columnas
                PdfPTable table = new PdfPTable(3);
                table.WidthPercentage = 92;
                table.SpacingBefore = 5f;
                table.SpacingAfter = 8f;
                table.SetWidths(new float[] { 13f, 43f, 43f });

                // Encabezados SOLO una vez
                table.AddCell(new PdfPCell(new Phrase("Fitting Id", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase("XML Anterior", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase("XML Actual", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });

                // Ordenar diferencias
                var diffs = diferencias
                    .OrderBy(d => d.Titulo)
                    .ThenBy(d => d.OrigenDiferencia)
                    .ThenBy(d => d.Descripcion);

                // Agregar filas
                foreach (DiferenciaXml diferencia in diffs)
                {
                    // Parsear DetalleDiferenciaAtributos -> "valor1@valor2"
                    var partes = diferencia.DetalleDiferenciaAtributos?.Split(new[] { "@" }, StringSplitOptions.None);
                    if (partes != null && partes.Length == 2)
                    {
                        table.AddCell(new PdfPCell(new Phrase(diferencia.DetalleDiferenciaArticulo, textFont)) { Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase(partes[0], textFont)) { Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase(partes[1], textFont)) { Padding = 5f });
                    }
                }

                // Agregar tabla completa al PDF
                doc.Add(table);
            }
        }
        private void WriteInPdfFittingNoExistente(string titulo, int diferenceType, iTextSharp.text.Document doc, iTextSharp.text.Font titleFont, iTextSharp.text.Font textFont2, List<DiferenciaXml> diferenciasList)
        {
            List<DiferenciaXml> diferencias = new List<DiferenciaXml>();
            diferencias.AddRange(diferenciasList.Where(op => op.Tipo == diferenceType));
            if (diferencias.Count() > 0)
            {
                var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
                var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                var headerBackground = new BaseColor(230, 230, 230);

                string currentOption = "";
                int currentOrigen = -1;

                foreach (var dif in diferencias
                    .Where(d => d.Tipo == diferenceType)
                    .OrderBy(d => d.DetalleDiferenciaDescription)
                    .ThenBy(d => d.OrigenDiferencia))
                {
                    // Cabecera según origen
                    if (currentOrigen != dif.OrigenDiferencia)
                    {
                        string label;
                        BaseColor labelColor;

                        if (dif.OrigenDiferencia == (int)enumOrigenXMLDiferencia.anterior)
                        {
                            label = "Solo en XML Anterior:";
                            labelColor = BaseColor.RED;
                        }
                        else if (dif.OrigenDiferencia == (int)enumOrigenXMLDiferencia.actual)
                        {
                            label = "Solo en XML Actual:";
                            labelColor = new BaseColor(0, 128, 0);
                        }
                        else
                        {
                            label = "En ambos XMLs (con diferencias):";
                            labelColor = BaseColor.BLUE;
                        }

                        var labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, labelColor);
                        Paragraph origenPara = new Paragraph("  - " + label, labelFont)
                        {
                            SpacingBefore = 6f,
                            SpacingAfter = 4f
                        };
                        doc.Add(origenPara);
                    }

                    // Creamos tabla con cabeceras SOLO una vez por bloque (no por cada fila)
                    PdfPTable table = new PdfPTable(2);
                    table.WidthPercentage = 92;
                    table.SetWidths(new float[] { 10f, 90f });

                    // Solo añadimos cabeceras si es la primera fila del bloque
                    if (currentOrigen != dif.OrigenDiferencia)
                    {
                        table.AddCell(new PdfPCell(new Phrase("Id", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase("Artículo", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                    }

                    // Fila con datos
                    table.AddCell(new PdfPCell(new Phrase(dif.DetalleDiferenciaArticulo, textFont)) { Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase(dif.DetalleDiferenciaAtributos, textFont)) { Padding = 5f });

                    doc.Add(table);

                    currentOrigen = dif.OrigenDiferencia;
                }
            }
        }
        private void WriteInPdfFittingOptions(Document doc, int diferenceType, List<DiferenciaXml> diferenciasList)
        {
            var subtituloAnterior = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.RED);
            var subtituloActual = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(0, 128, 0));
            var subtituloAmbos = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLUE);
            var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
            var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
            var headerBackground = new BaseColor(230, 230, 230);

            var diffs = diferenciasList
                .Where(d => d.Tipo == diferenceType)
                .OrderBy(d => d.OrigenDiferencia)
                .ThenBy(d => d.DetalleDiferenciaArticulo)
                .ThenBy(d => d.DetalleDiferenciaDescription);

            // Agrupar por origen (Anterior / Actual / Ambos)
            foreach (var bloque in diffs.GroupBy(d => d.OrigenDiferencia))
            {
                // 🔹 Título de bloque
                iTextSharp.text.Font bloqueFont;
                string bloqueTitulo;
                switch (bloque.Key)
                {
                    case (int)enumOrigenXMLDiferencia.anterior:
                        bloqueFont = subtituloAnterior;
                        bloqueTitulo = "Solo en XML Anterior:";
                        break;
                    case (int)enumOrigenXMLDiferencia.actual:
                        bloqueFont = subtituloActual;
                        bloqueTitulo = "Solo en XML Actual:";
                        break;
                    default:
                        bloqueFont = subtituloAmbos;
                        bloqueTitulo = "En ambos XMLs (con diferencias):";
                        break;
                }

                doc.Add(new Paragraph("- " + bloqueTitulo, bloqueFont));

                // Agrupar por fitting
                foreach (var grupo in bloque.GroupBy(d => d.DetalleDiferenciaArticulo))
                {
                    PdfPTable table = new PdfPTable(3);
                    table.WidthPercentage = 92;
                    table.SpacingBefore = 3f;
                    table.SpacingAfter = 6f;
                    table.SetWidths(new float[] { 25f, 35f, 35f });

                    // 🔹 Primera fila: ID + Fitting
                    PdfPCell fittingCell = new PdfPCell(new Phrase(grupo.Key, subtituloFont))
                    {
                        Colspan = 3,
                        BackgroundColor = headerBackground,
                        Padding = 6f
                    };
                    table.AddCell(fittingCell);

                    // 🔹 Encabezados
                    table.AddCell(new PdfPCell(new Phrase("Opción/es", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase("XML Anterior", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase("XML Actual", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });

                    // 🔹 Filas de diferencias
                    foreach (var dif in grupo)
                    {
                        string opciones = dif.DetalleDiferenciaDescription?.Replace("|", "\n") ?? "";

                        var partes = dif.DetalleDiferenciaAtributos?.Split('@');
                        string xml1 = partes != null && partes.Length > 0 ? partes[0] : "—";
                        string xml2 = partes != null && partes.Length > 1 ? partes[1] : "—";

                        table.AddCell(new PdfPCell(new Phrase(opciones, textFont)) { Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase(xml1, textFont)) { Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase(xml2, textFont)) { Padding = 5f });
                    }

                    doc.Add(table);
                }
            }
        }
        private void WriteInPdfGeneracionDiferentesArticulos(Document doc, int diferenceType, List<DiferenciaXml> diferenciasList)
        {
            // Estilos
            var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
            var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
            var headerBackground = new BaseColor(230, 230, 230);

            var diffs = diferenciasList
                .Where(d => d.Tipo == diferenceType)
                .OrderBy(d => d.DetalleDiferenciaArticulo);

            if (!diffs.Any())
                return;

            foreach (var dif in diffs)
            {
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 92;
                table.SpacingBefore = 3f;
                table.SpacingAfter = 6f;
                table.SetWidths(new float[] { 50f, 50f });

                // Fila fitting (ID + Descripción)
                PdfPCell fittingCell = new PdfPCell(new Phrase(dif.DetalleDiferenciaArticulo, subtituloFont))
                {
                    Colspan = 2,
                    BackgroundColor = headerBackground,
                    Padding = 6f
                };
                table.AddCell(fittingCell);

                // Encabezados
                table.AddCell(new PdfPCell(new Phrase("XML Anterior", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase("XML Actual", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });

                // Valores
                var partes = dif.DetalleDiferenciaAtributos?.Split('@');
                string xml1 = partes != null && partes.Length > 0 ? partes[0] : "—";
                string xml2 = partes != null && partes.Length > 1 ? partes[1] : "—";

                table.AddCell(new PdfPCell(new Phrase(xml1, textFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(xml2, textFont)) { Padding = 5f });

                doc.Add(table);
            }
        }
        private void WriteInPdfAtributosSets(Document doc, int diferenceType, List<DiferenciaXml> diferenciasList)
        {
            var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
            var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
            var headerBackground = new BaseColor(230, 230, 230);

            var diffs = diferenciasList
                .Where(d => d.Tipo == diferenceType)
                .OrderBy(d => d.DetalleDiferenciaArticulo)
                .ThenBy(d => d.DetalleDiferenciaAtributos)
                .ToList();

            if (!diffs.Any())
                return;

            string currentSet = "";

            PdfPTable? table = null;

            foreach (var dif in diffs)
            {
                // Nuevo SET → crear nueva tabla
                if (currentSet != dif.DetalleDiferenciaArticulo)
                {
                    // Si había tabla previa, añadirla
                    if (table != null)
                        doc.Add(table);

                    table = new PdfPTable(3);
                    table.WidthPercentage = 92;
                    table.SpacingBefore = 3f;
                    table.SpacingAfter = 6f;
                    table.SetWidths(new float[] { 25f, 37f, 37f });

                    // Fila de título del Set
                    PdfPCell fittingCell = new PdfPCell(new Phrase(dif.DetalleDiferenciaArticulo, subtituloFont))
                    {
                        Colspan = 3,
                        BackgroundColor = headerBackground,
                        Padding = 6f
                    };
                    table.AddCell(fittingCell);

                    // Cabeceras
                    table.AddCell(new PdfPCell(new Phrase("Atributo", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase("XML Anterior", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                    table.AddCell(new PdfPCell(new Phrase("XML Actual", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });

                    currentSet = dif.DetalleDiferenciaArticulo;
                }

                // Extraer valores val1 y val2 de DetalleDiferenciaDescription
                var partes = dif.DetalleDiferenciaDescription?.Split('@');
                string val1 = partes != null && partes.Length > 0 ? partes[0] : "—";
                string val2 = partes != null && partes.Length > 1 ? partes[1] : "—";

                // Fila de diferencia
                table.AddCell(new PdfPCell(new Phrase(dif.DetalleDiferenciaAtributos, textFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(val1, textFont)) { Padding = 5f });
                table.AddCell(new PdfPCell(new Phrase(val2, textFont)) { Padding = 5f });
            }

            // Añadir última tabla si existe
            if (table != null)
                doc.Add(table);
        }
        private void WriteInPdfSets(Document doc, int diferenceType, List<DiferenciaXml> diferenciasList)
        {
            // Fuentes
            var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var subtituloFontAnterior = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.RED);
            var subtituloFontActual = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(0, 128, 0));
            var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
            var headerBackground = new BaseColor(230, 230, 230);

            var diffs = diferenciasList
                .Where(d => d.Tipo == diferenceType)
                .OrderBy(d => d.OrigenDiferencia)
                .ThenBy(d => d.DetalleDiferenciaArticulo)
                .ToList();

            if (!diffs.Any())
                return;

            // Agrupar por origen
            var grupos = diffs.GroupBy(d => d.OrigenDiferencia);

            foreach (var grupo in grupos)
            {
                // Seleccionar título y color según el origen
                string tituloGrupo = grupo.Key == (int)enumOrigenXMLDiferencia.anterior
                    ? "Solo en XML Anterior"
                    : "Solo en XML Actual";

                var fontGrupo = grupo.Key == (int)enumOrigenXMLDiferencia.anterior
                    ? subtituloFontAnterior
                    : subtituloFontActual;

                doc.Add(new Paragraph(tituloGrupo, fontGrupo));

                // Crear tabla de 1 columna
                PdfPTable table = new PdfPTable(1);
                table.WidthPercentage = 92;
                table.SpacingBefore = 3f;
                table.SpacingAfter = 6f;

                // Filas
                foreach (var dif in grupo)
                {
                    table.AddCell(new PdfPCell(new Phrase(dif.DetalleDiferenciaArticulo, textFont)) { Padding = 5f });
                }

                doc.Add(table);
            }
        }
        private void WriteInPdfOpening(Document doc, int diferenceType, List<DiferenciaXml> diferenciasList)
        {
            var diffs = diferenciasList
                    .Where(d => d.Tipo == diferenceType)
                    .OrderBy(d => d.OrigenDiferencia)
                    .ThenBy(d => d.DetalleDiferenciaArticulo)
                    .ToList();

            if (!diffs.Any())
                return;

            // Fuentes
            var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
            var subtituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.DARK_GRAY);
            var subtituloFontAnterior = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.RED);
            var subtituloFontActual = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(0, 128, 0));
            var subtituloFontAmbos = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLUE);
            var textFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
            var headerBackground = new BaseColor(230, 230, 230);

            // Agrupar por origen (Anterior, Actual, Ambos)
            var grupos = diffs.GroupBy(d => d.OrigenDiferencia);

            foreach (var grupo in grupos)
            {
                string tituloGrupo;
                iTextSharp.text.Font fontGrupo;

                switch (grupo.Key)
                {
                    case (int)enumOrigenXMLDiferencia.anterior:
                        tituloGrupo = "- Solo en el XML Anterior";
                        fontGrupo = subtituloFontAnterior;
                        break;
                    case (int)enumOrigenXMLDiferencia.actual:
                        tituloGrupo = "- Solo en el XML Actual";
                        fontGrupo = subtituloFontActual;
                        break;
                    default:
                        tituloGrupo = "- En ambos XMLs";
                        fontGrupo = subtituloFontAmbos;
                        break;
                }

                doc.Add(new Paragraph(tituloGrupo, fontGrupo));

                foreach (var dif in grupo)
                {
                    if (grupo.Key == (int)enumOrigenXMLDiferencia.anterior || grupo.Key == (int)enumOrigenXMLDiferencia.actual)
                    {
                        // Tabla simple de 1 columna
                        PdfPTable table = new PdfPTable(1);
                        table.WidthPercentage = 92;
                        table.SpacingBefore = 3f;
                        table.SpacingAfter = 6f;

                        // Encabezado con el set
                        PdfPCell tituloCell = new PdfPCell(new Phrase(dif.DetalleDiferenciaArticulo, subtituloFont))
                        {
                            BackgroundColor = headerBackground,
                            Padding = 5f
                        };
                        table.AddCell(tituloCell);

                        doc.Add(table);
                    }
                    else
                    {
                        // Ambos → tabla de 3 columnas
                        PdfPTable table = new PdfPTable(3);
                        table.WidthPercentage = 92;
                        table.SpacingBefore = 3f;
                        table.SpacingAfter = 6f;
                        table.SetWidths(new float[] { 25f, 37f, 37f });

                        // Primera fila → Set
                        PdfPCell tituloCell = new PdfPCell(new Phrase(dif.DetalleDiferenciaArticulo, subtituloFont))
                        {
                            Colspan = 3,
                            BackgroundColor = headerBackground,
                            Padding = 5f
                        };
                        table.AddCell(tituloCell);

                        // Encabezados
                        table.AddCell(new PdfPCell(new Phrase("Atributo", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase("XML Anterior", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase("XML Actual", subtituloFont)) { BackgroundColor = headerBackground, Padding = 5f });

                        // Valores de los atributos
                        var partes = dif.DetalleDiferenciaDescription.Split('@');
                        string val1 = partes.Length > 0 ? partes[0] : "";
                        string val2 = partes.Length > 1 ? partes[1] : "";

                        table.AddCell(new PdfPCell(new Phrase(dif.DetalleDiferenciaAtributos, textFont)) { Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase(val1, textFont)) { Padding = 5f });
                        table.AddCell(new PdfPCell(new Phrase(val2, textFont)) { Padding = 5f });

                        doc.Add(table);
                    }
                }
            }
        }
        private void InsertLogo(iTextSharp.text.Document doc)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "RotoTools.images.logo.png"; // usa el namespace + carpeta + nombre exacto del recurso

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new Exception("No se pudo encontrar la imagen embebida: " + resourceName);
                }

                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(System.Drawing.Image.FromStream(stream), System.Drawing.Imaging.ImageFormat.Png);
                logo.ScaleToFit(100f, 50f);
                logo.Alignment = Element.ALIGN_RIGHT;
                doc.Add(logo);
            }
        }
        private void InsertHeader(iTextSharp.text.Document doc, iTextSharp.text.Font titleFont, iTextSharp.text.Font normalFont)
        {
            doc.Add(new Paragraph("Informe de control de cambios", titleFont));
            doc.Add(new Paragraph($"Fecha: {DateTime.Now}", normalFont));
            doc.Add(Chunk.NEWLINE);
            doc.Add(new Paragraph($"XML anterior: " + lbl_Xml1.Text, normalFont));
            doc.Add(new Paragraph($"Fittings Version XML anterior: " + xmlOrigen.FittingsVersion, normalFont));
            doc.Add(new Paragraph($"XML actual: " + lbl_Xml2.Text, normalFont));
            doc.Add(new Paragraph($"Fittings Version XML actual: " + xmlNuevo.FittingsVersion, normalFont));
            doc.Add(Chunk.NEWLINE);
        }
        private void GeneratePdf(string filePath, List<DiferenciaXml> diferenciasList)
        {
            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.A4);
            try
            {
                PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var subTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                InsertLogo(doc);                

                InsertHeader(doc, titleFont, normalFont);               

                //Escribir cambios en colores
                WriteInPdf("Cambios en colores", (int)enumTipoDiferencia.colorDiferente, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en FittingGroups
                WriteInPdf("Cambios en FittingGroups", (int)enumTipoDiferencia.fittingGroupDiferente, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en opciones genéricas
                WriteInPdf("Cambios en opciones genéricas", (int)enumTipoDiferencia.opcionGlobal, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en descripciones
                WriteInPdf("Cambios en descripciones", (int)enumTipoDiferencia.descripcionFitting, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en fittings
                WriteInPdf("Fittings modificados", (int)enumTipoDiferencia.fittingNoExistente, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en la generación de artículos
                WriteInPdf("Cambios en la generación de artículos", (int)enumTipoDiferencia.referenciaNoGeneradaFitting, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en referencias de opciones
                WriteInPdf("Referencias cambiadas en opciones", (int)enumTipoDiferencia.cambioReferenciaOpcion, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en los Sets
                WriteInPdf("Diferencias en los Sets", (int)enumTipoDiferencia.setsDiferentes, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en los atributos de los Sets
                WriteInPdf("Diferencias en los atributos de los Sets", (int)enumTipoDiferencia.atributosSetDiferente, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en los opening de los Sets
                WriteInPdf("Diferencias en los opening de los Sets", (int)enumTipoDiferencia.openingSetDiferente, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir cambios en los opening de los Sets
                WriteInPdf("Diferencias en los SetsDescriptions", (int)enumTipoDiferencia.setDescriptionDiferente, doc, subTitleFont, normalFont, diferenciasList);

                //Escribir el resto de diferencias no encapsuladas
                WriteRestoCambiosInPdf(doc, subTitleFont, normalFont, diferenciasList);                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el PDF: " + ex.Message);
            }
            finally
            {
                doc.Close();
            }
        }
        private void WriteRestoCambiosInPdf(iTextSharp.text.Document doc, iTextSharp.text.Font titleFont, iTextSharp.text.Font textFont, List<DiferenciaXml> diferenciasList)
        {
            List<DiferenciaXml> restoCambiosList = new List<DiferenciaXml>();
            restoCambiosList.AddRange(diferenciasList.Where(d => d.Tipo != (int)enumTipoDiferencia.descripcionFitting &&
                                                                            d.Tipo != (int)enumTipoDiferencia.fittingGroupDiferente &&
                                                                            d.Tipo != (int)enumTipoDiferencia.colorDiferente &&
                                                                            d.Tipo != (int)enumTipoDiferencia.opcionGlobal &&
                                                                            d.Tipo != (int)enumTipoDiferencia.fittingNoExistente &&
                                                                            d.Tipo != (int)enumTipoDiferencia.cambioReferenciaOpcion &&
                                                                            d.Tipo != (int)enumTipoDiferencia.referenciaNoGeneradaFitting &&
                                                                            d.Tipo != (int)enumTipoDiferencia.setsDiferentes &&
                                                                            d.Tipo != (int)enumTipoDiferencia.atributosSetDiferente &&
                                                                            d.Tipo != (int)enumTipoDiferencia.openingSetDiferente &&
                                                                            d.Tipo != (int)enumTipoDiferencia.setDescriptionDiferente));
                                                                            //&& d.Tipo != (int)enumTipoDiferencia.setDescriptionDiferente));
            if (restoCambiosList.Count() > 0)
            {
                doc.Add(new Paragraph("Resto de cambios", titleFont));
                foreach (DiferenciaXml diferencia in restoCambiosList)
                {
                    doc.Add(new Paragraph(diferencia.Descripcion, textFont));
                }
                doc.Add(Chunk.NEWLINE);
            }
        }
        public List<DiferenciaXml> CompareXmlData(XmlData xml1, XmlData xml2)
        {
            var diferencias = new List<string>();
            var diferenciasList = new List<DiferenciaXml>();

            if (compareFittingGroups) diferenciasList.AddRange(CompararFittingGroups(xml1.FittingGroupList, xml2.FittingGroupList));
            if (compareColours) diferenciasList.AddRange(CompararArticlesYColours(xml1.ColourList, xml2.ColourList));
            if (compareOptions) diferenciasList.AddRange(CompareDocOptions(xml1.OptionList, xml2.OptionList));
            if (compareFittings.compararFittings) diferenciasList.AddRange(CompareFittings(xml1.FittingList, xml2.FittingList));
            if (compareSets.compararSets) diferenciasList.AddRange(CompareSets(xml1.SetList, xml2.SetList));

            return diferenciasList;
        }
        public List<DiferenciaXml> CompararFittingGroups(List<FittingGroup> gruposList1, List<FittingGroup> gruposList2)
        {
            var diferencias = new List<DiferenciaXml>();

            var dict1 = gruposList1.ToDictionary(g => g.Id);
            var dict2 = gruposList2.ToDictionary(g => g.Id);

            var todosLosIds = new HashSet<int>(dict1.Keys);
            todosLosIds.UnionWith(dict2.Keys);

            foreach (var id in todosLosIds)
            {
                var existeEn1 = dict1.TryGetValue(id, out var group1);
                var existeEn2 = dict2.TryGetValue(id, out var group2);

                if (existeEn1 && !existeEn2)
                {
                    diferencias.Add(new DiferenciaXml
                    (
                        tipo: (int)enumTipoDiferencia.fittingGroupDiferente,
                        descripcion: $"FittingGroup con ID '{id}' está en XML anterior pero no en XML actual (class: '{group1.Class}').",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.anterior,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true
                    ));
                }
                else if (!existeEn1 && existeEn2)
                {
                    diferencias.Add(new DiferenciaXml
                    (
                        tipo: (int)enumTipoDiferencia.fittingGroupDiferente,
                        descripcion: $"FittingGroup con ID '{id}' está en XML actual pero no en XML anterior (class: '{group2.Class}').",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.actual,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true
                    ));
                }
                else if (!string.Equals(group1.Class?.Trim(), group2.Class?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    diferencias.Add(new DiferenciaXml
                    (
                        tipo: (int)enumTipoDiferencia.fittingGroupDiferente,
                        descripcion: $"FittingGroup con ID '{id}' tiene diferente 'class': '{group1.Class}' (XML anterior) vs '{group2.Class}' (XML actual).",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.ambos,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true
                    ));
                }
            }

            return diferencias;
        }
        private List<DiferenciaXml> CompareFittings(List<Fitting> fittingListXml1, List<Fitting> fittingListXml2)
        {
            var diferenciasList = new List<DiferenciaXml>();

            var dic1 = fittingListXml1.ToDictionary(f => f.Id);
            var dic2 = fittingListXml2.ToDictionary(f => f.Id);

            var todosIds = dic1.Keys.Union(dic2.Keys);

            foreach (var id in todosIds)
            {
                dic1.TryGetValue(id, out var f1);
                dic2.TryGetValue(id, out var f2);

                if (f1 == null)
                {
                    if (compareFittings.compararFittingsFiltrados && !compareFittings.compararFittingsFiltradosList.Contains(f2.FittingGroup?.Class))
                    {
                        continue;
                    }
                    diferenciasList.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.fittingNoExistente,
                        descripcion: $"Fitting ID {id} - {fittingListXml2.Where(f => f.Id == id).FirstOrDefault()?.Description} solo existe en XML actual",
                        detalleDiferenciaArticulo: id.ToString(),
                        detalleDiferenciaAtributos: $"{fittingListXml2.Where(f => f.Id == id).FirstOrDefault()?.Description}",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.actual,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true,
                        titulo: "Fittings no existentes"
                        ));                                 
                }
                else if (f2 == null)
                {
                    if (compareFittings.compararFittingsFiltrados && !compareFittings.compararFittingsFiltradosList.Contains(f1.FittingGroup?.Class))
                    {
                        continue;
                    }
                    diferenciasList.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.fittingNoExistente,
                        descripcion: $"Fitting ID {id} - {fittingListXml1.Where(f => f.Id == id).FirstOrDefault()?.Description} solo existe en XML actual",
                        detalleDiferenciaArticulo: id.ToString(),
                        detalleDiferenciaAtributos: $"{fittingListXml1.Where(f => f.Id == id).FirstOrDefault()?.Description}",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.anterior,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true,
                        titulo: "Fittings no existentes"
                        ));
                }
                else
                {
                    if (compareFittings.compararFittingsFiltrados && !compareFittings.compararFittingsFiltradosList.Contains(f1.FittingGroup?.Class))
                    {
                        continue;
                    }

                    if (f1.Description != f2.Description && compareFittings.compararFittingsDescription)
                    {
                        diferenciasList.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.descripcionFitting,
                        descripcion: $"Ha cambiado la descripción del fitting {id}: '{f1.Description}' vs '{f2.Description}'",
                        detalleDiferenciaArticulo: id.ToString() ,
                        detalleDiferenciaAtributos: $"{f1.Description}@{f2.Description}",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.ambos,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true,
                        titulo: "Cambios en descripciones de fittings"
                    ));
                    }

                    if (f1.Manufacturer != f2.Manufacturer && compareFittings.compararFittingsManufacturer)
                        diferenciasList.Add(new DiferenciaXml((int)enumTipoDiferencia.manufacturerDistinto, $"Fitting ID {id}: Manufacturer distinto: '{f1.Manufacturer}' vs '{f2.Manufacturer}'", (int)enumSeveridadDiferencia.warning, false));

                    if (f1.Location != f2.Location && compareFittings.compararFittingsLocation)
                        diferenciasList.Add(new DiferenciaXml((int)enumTipoDiferencia.locationFittingDistinto, $"Fitting ID {id}: Location distinto: '{f1.Location}' vs '{f2.Location}'", (int)enumSeveridadDiferencia.warning, false));

                    if (f1.Lenght != f2.Lenght && compareFittings.compararFittingsLength)
                        diferenciasList.Add(new DiferenciaXml((int)enumTipoDiferencia.lengthFittingDistinto, $"Fitting ID {id}: Length distinto: '{f1.Lenght}' vs '{f2.Lenght}'", (int)enumSeveridadDiferencia.warning, false));


                    // Comparar Articles del Fitting
                    if (compareFittings.compararFittingsArticles)
                    {
                        var articlesDiff = CompareArticles(f1.ArticleList, f2.ArticleList, id.ToString());
                        diferenciasList.AddRange(articlesDiff);
                    }
                }
            }

            return diferenciasList;
        }
        private List<DiferenciaXml> CompareArticles(List<Article> articleList1, List<Article> articleList2, string fittingId)
        {
            var diferenciasList = new List<DiferenciaXml>();

            // Verifica si hay opciones en alguno de los artículos
            bool hayOpciones = articleList1.Concat(articleList2).Any(a => a.OptionList != null && a.OptionList.Any());

            var fittingOrigen = xmlOrigen.FittingList.FirstOrDefault(f => f.Id == TryParseInt(fittingId));
            var fittingNuevo = xmlNuevo.FittingList.FirstOrDefault(f => f.Id == TryParseInt(fittingId));
            var descripcion = fittingNuevo?.Description ?? fittingOrigen?.Description ?? "(sin descripción)";

            if (hayOpciones)
            {
                var group1 = articleList1.GroupBy(GetOptionKey)
                                  .ToDictionary(g => g.Key, g => g.Select(a => a.Ref).Distinct().OrderBy(r => r).ToList());

                var group2 = articleList2.GroupBy(GetOptionKey)
                                  .ToDictionary(g => g.Key, g => g.Select(a => a.Ref).Distinct().OrderBy(r => r).ToList());

                var allOptionKeys = group1.Keys.Union(group2.Keys);

                foreach (var optKey in allOptionKeys)
                {
                    group1.TryGetValue(optKey, out var refs1);
                    group2.TryGetValue(optKey, out var refs2);

                    var refs1Text = refs1 != null ? string.Join(", ", refs1) : "—";
                    var refs2Text = refs2 != null ? string.Join(", ", refs2) : "—";

                    if (refs1 == null)
                    {
                        diferenciasList.Add(new DiferenciaXml(
                            (int)enumTipoDiferencia.cambioReferenciaOpcion,
                            $"La opción y valor {optKey} solo existe en XML actual.",
                            (int)enumSeveridadDiferencia.warning,
                            true)
                        {
                            OrigenDiferencia = (int)enumOrigenXMLDiferencia.actual,
                            DetalleDiferenciaDescription = optKey,
                            DetalleDiferenciaArticulo = "(" + fittingId + ") " + descripcion,
                            DetalleDiferenciaAtributos = $"—@{refs2Text}"
                        });
                    }
                    else if (refs2 == null)
                    {
                        diferenciasList.Add(new DiferenciaXml(
                            (int)enumTipoDiferencia.cambioReferenciaOpcion,
                            $"La opción y valor {optKey} solo existe en XML anterior.",
                            (int)enumSeveridadDiferencia.warning,
                            true)
                        {
                            OrigenDiferencia = (int)enumOrigenXMLDiferencia.anterior,
                            DetalleDiferenciaDescription = optKey,
                            DetalleDiferenciaArticulo = "(" + fittingId + ") " + descripcion,
                            DetalleDiferenciaAtributos = $"{refs1Text}@—"
                        });
                    }
                    else if (!refs1.SequenceEqual(refs2))
                    {
                        diferenciasList.Add(new DiferenciaXml(
                            (int)enumTipoDiferencia.cambioReferenciaOpcion,
                            $"Para la opción {optKey}, han cambiado las referencias.",
                            (int)enumSeveridadDiferencia.warning,
                            true)
                        {
                            OrigenDiferencia = (int)enumOrigenXMLDiferencia.ambos,
                            DetalleDiferenciaDescription = optKey,
                            DetalleDiferenciaArticulo = "(" + fittingId + ") " + descripcion,
                            DetalleDiferenciaAtributos = $"{refs1Text}@{refs2Text}"
                        });
                    }
                }
            }
            else
            {
                // Comparación simple sin opciones
                var refs1 = articleList1.Select(a => a.Ref).Distinct().OrderBy(r => r).ToList();
                var refs2 = articleList2.Select(a => a.Ref).Distinct().OrderBy(r => r).ToList();

                if (!refs1.SequenceEqual(refs2))
                {
                    var refs1Text = refs1.Any() ? string.Join(", ", refs1) : "—";
                    var refs2Text = refs2.Any() ? string.Join(", ", refs2) : "—";

                    diferenciasList.Add(new DiferenciaXml(
                            (int)enumTipoDiferencia.referenciaNoGeneradaFitting,
                            $"Fitting {fittingId} - {descripcion}: Han cambiado las referencias: XML anterior -> {refs1Text}, XML actual -> {refs2Text}",
                            (int)enumSeveridadDiferencia.warning,
                            true)
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.ambos,
                        DetalleDiferenciaDescription = $"XML anterior -> {refs1Text}, XML actual -> {refs2Text}",
                        DetalleDiferenciaArticulo = "(" + fittingId + ") " + descripcion,
                        DetalleDiferenciaAtributos = $"{refs1Text}@{refs2Text}"
                    });
                }
            }

            return diferenciasList;
        }
        private string GetOptionKey(Article article)
        {
            return string.Join("|", article.OptionList.Select(o => $"{o.Name}={o.Value}"));
        }
        public List<DiferenciaXml> CompararArticlesYColours(List<Colour> colorList1, List<Colour> colorList2)
        {
            var diferencias = new List<DiferenciaXml>();

            var refToFinalXml1 = new Dictionary<string, string>();
            var refToColoursXml1 = new Dictionary<string, List<string>>();

            foreach (var colour in colorList1)
            {
                foreach (var article in colour.ArticleList)
                {
                    if (!refToFinalXml1.ContainsKey(article.Ref))
                        refToFinalXml1[article.Ref] = article.Final;

                    if (!refToColoursXml1.ContainsKey(article.Ref))
                        refToColoursXml1[article.Ref] = new List<string>();

                    if (!refToColoursXml1[article.Ref].Contains(colour.Name))
                        refToColoursXml1[article.Ref].Add(colour.Name);
                }
            }

            var refToFinalXml2 = new Dictionary<string, string>();
            var refToColoursXml2 = new Dictionary<string, List<string>>();

            foreach (var colour in colorList2)
            {
                foreach (var article in colour.ArticleList)
                {
                    if (!refToFinalXml2.ContainsKey(article.Ref))
                        refToFinalXml2[article.Ref] = article.Final;

                    if (!refToColoursXml2.ContainsKey(article.Ref))
                        refToColoursXml2[article.Ref] = new List<string>();

                    if (!refToColoursXml2[article.Ref].Contains(colour.Name))
                        refToColoursXml2[article.Ref].Add(colour.Name);
                }
            }

            // --- Comparar Ref entre los dos XML ---
            var allRefs = new HashSet<string>(refToFinalXml1.Keys);
            allRefs.UnionWith(refToFinalXml2.Keys);

            foreach (var reference in allRefs)
            {
                var existeEn1 = refToFinalXml1.TryGetValue(reference, out var final1);
                var existeEn2 = refToFinalXml2.TryGetValue(reference, out var final2);

                if (existeEn1 && !existeEn2)
                {
                    var colores = string.Join(", ", refToColoursXml1[reference]);
                    diferencias.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.colorDiferente,
                        descripcion: $"Artículo con referencia '{reference}' está en XML anterior pero no en XML actual",
                        detalleDiferenciaArticulo: reference + " (" + xmlOrigen.FittingList?.FirstOrDefault(f => f.Ref == reference)?.Description + ")",
                        detalleDiferenciaAtributos: $"Colores={colores}",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.anterior,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true,
                        titulo: "Comparación de Articles/Colours"
                    ));
                }
                else if (!existeEn1 && existeEn2)
                {
                    var colores = string.Join(", ", refToColoursXml2[reference]);
                    diferencias.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.colorDiferente,
                        descripcion: $"El artículo con referencia '{reference}' está en XML actual pero no en XML anterior",
                        detalleDiferenciaArticulo: reference + " (" + xmlNuevo.FittingList?.FirstOrDefault(f => f.Ref == reference)?.Description + ")",
                        detalleDiferenciaAtributos: $"Colores={colores}",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.actual,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true,
                        titulo: "Comparación de Articles/Colours"
                    ));
                }
                else if (!string.Equals(final1, final2, StringComparison.OrdinalIgnoreCase))
                {
                    diferencias.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.colorDiferente,
                        descripcion: $"El artículo con ref '{reference}' tiene diferente 'Final'",
                        detalleDiferenciaArticulo: reference + " (" + xmlNuevo.FittingList?.FirstOrDefault(f => f.Ref == reference)?.Description + ")",
                        detalleDiferenciaAtributos: $"{final1},{final2}",
                        origenDiferencia: (int)enumOrigenXMLDiferencia.ambos,
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true,
                        titulo: "Comparación de Articles/Colours"
                    ));
                }
            }

            // --- Comparar Colour.Name ---
            var colourNamesXml1 = new HashSet<string>(colorList1.Select(c => c.Name));
            var colourNamesXml2 = new HashSet<string>(colorList2.Select(c => c.Name));

            foreach (var name in colourNamesXml1.Except(colourNamesXml2))
            {
                diferencias.Add(new DiferenciaXml(
                    tipo: (int)enumTipoDiferencia.colorDiferente,
                    descripcion: $"El color '{name}' existe en XML anterior pero no en XML actual",
                    detalleDiferenciaArticulo: "",
                    detalleDiferenciaAtributos: $"Colour={name}",
                    origenDiferencia: (int)enumOrigenXMLDiferencia.anterior,
                    severidad: (int)enumSeveridadDiferencia.warning,
                    visible: true,
                    titulo: "Comparación de Articles/Colours"
                ));
            }

            foreach (var name in colourNamesXml2.Except(colourNamesXml1))
            {
                diferencias.Add(new DiferenciaXml(
                    tipo: (int)enumTipoDiferencia.colorDiferente,
                    descripcion: $"El color '{name}' existe en XML actual pero no en XML anterior",
                    detalleDiferenciaArticulo: "",
                    detalleDiferenciaAtributos: $"Colour={name}",
                    origenDiferencia: (int)enumOrigenXMLDiferencia.actual,
                    severidad: (int)enumSeveridadDiferencia.warning,
                    visible: true,
                    titulo: "Comparación de Articles/Colours"
                ));
            }

            return diferencias;
        }
        private List<DiferenciaXml> CompareDocOptions(List<Option> list1, List<Option> list2)
        {
            var diferenciasList = new List<DiferenciaXml>();

            var dic1 = list1.ToDictionary(f => f.Name);
            var dic2 = list2.ToDictionary(f => f.Name);

            var todosIds = dic1.Keys.Union(dic2.Keys);

            foreach (var id in todosIds)
            {
                dic1.TryGetValue(id, out var f1);
                dic2.TryGetValue(id, out var f2);

                if (f1 == null)
                {
                    diferenciasList.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.opcionGlobal,
                        descripcion: $"La opción '{id}' solo existe en XML actual",
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true
                    )
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.actual,
                        DetalleDiferenciaDescription = id
                    });
                }
                else if (f2 == null)
                {
                    diferenciasList.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.opcionGlobal,
                        descripcion: $"La opción '{id}' solo existe en XML anterior",
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true
                    )
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.anterior,
                        DetalleDiferenciaDescription = id
                    });
                }
                else
                {
                    // Comparar valores de esa opción
                    var valuesDiff = CompareValueOptions(f1.ValuesList, f2.ValuesList, id);
                    diferenciasList.AddRange(valuesDiff);
                }
            }

            return diferenciasList;
        }
        private List<DiferenciaXml> CompareValueOptions(List<Value> options1, List<Value> options2, string optionName)
        {
            var diferenciasList = new List<DiferenciaXml>();

            var dic1 = options1.ToDictionary(o => o.Valor);
            var dic2 = options2.ToDictionary(o => o.Valor);

            var allValues = dic1.Keys.Union(dic2.Keys);

            foreach (var valor in allValues)
            {
                dic1.TryGetValue(valor, out var o1);
                dic2.TryGetValue(valor, out var o2);

                if (o1 == null)
                {
                    diferenciasList.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.opcionGlobal,
                        descripcion: $"Opción '{optionName}': el valor '{valor}' solo existe en XML actual",
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true
                    )
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.actual,
                        DetalleDiferenciaDescription = optionName,
                        DetalleDiferenciaAtributos = valor
                    });
                }
                else if (o2 == null)
                {
                    diferenciasList.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.opcionGlobal,
                        descripcion: $"Opción '{optionName}': el valor '{valor}' solo existe en XML anterior",
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true
                    )
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.anterior,
                        DetalleDiferenciaDescription = optionName,
                        DetalleDiferenciaAtributos = valor
                    });
                }
                else if (!string.Equals(o1.Valor, o2.Valor, StringComparison.OrdinalIgnoreCase))
                {
                    diferenciasList.Add(new DiferenciaXml(
                        tipo: (int)enumTipoDiferencia.opcionGlobal,
                        descripcion: $"Opción '{optionName}' con valores distintos",
                        severidad: (int)enumSeveridadDiferencia.warning,
                        visible: true
                    )
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.ambos,
                        DetalleDiferenciaDescription = optionName,
                        DetalleDiferenciaAtributos = $"{o1.Valor} vs {o2.Valor}"
                    });
                }
            }

            return diferenciasList;
        }
        private List<DiferenciaXml> CompareSets(List<Set> sets1, List<Set> sets2)
        {
            var diferencias = new List<DiferenciaXml>();

            var dic1 = sets1.ToDictionary(s => s.Id);
            var dic2 = sets2.ToDictionary(s => s.Id);

            var allIds = dic1.Keys.Union(dic2.Keys);

            foreach (var id in allIds)
            {
                dic1.TryGetValue(id, out var s1);
                dic2.TryGetValue(id, out var s2);

                if (s1 == null)
                {
                    diferencias.Add(new DiferenciaXml(
                        (int)enumTipoDiferencia.setsDiferentes,
                        $"El set {id} - {sets2.Where(s => s.Id == id).FirstOrDefault()?.Code} solo existe en XML actual",
                        (int)enumSeveridadDiferencia.warning,
                        true)
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.actual,
                        DetalleDiferenciaDescription = $"El set {id} - {sets2.Where(s => s.Id == id).FirstOrDefault()?.Code} solo existe en XML actual",
                        DetalleDiferenciaArticulo = $"({id}){sets2.Where(s => s.Id == id).FirstOrDefault()?.Code}",
                        DetalleDiferenciaAtributos = "Set"
                    });
                    continue;
                }

                if (s2 == null)
                {
                    diferencias.Add(new DiferenciaXml(
                        (int)enumTipoDiferencia.setsDiferentes,
                        $"El set {id} - {sets2.Where(s => s.Id == id).FirstOrDefault()?.Code} solo existe en XML anterior",
                        (int)enumSeveridadDiferencia.warning,
                        true)
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.anterior,
                        DetalleDiferenciaDescription = $"El set {id} - {sets1.Where(s => s.Id == id).FirstOrDefault()?.Code} solo existe en XML anterior",
                        DetalleDiferenciaArticulo = $"({id}){sets1.Where(s => s.Id == id).FirstOrDefault()?.Code}",
                        DetalleDiferenciaAtributos = "Set"
                    });
                    continue;
                }

                if (compareSets.compararSetsFiltrados && !compareSets.compararSetsFiltradosList.Contains(s1.Code))
                {
                    continue;
                }
                // Comparar atributos principales
                CompararAtributosSet(s1, s2, diferencias);

                // Comparar Opening
                CompararOpening(s1.Opening, s2.Opening, id, diferencias, s1.Code);

                // Comparar SetDescription
                diferencias.AddRange(CompararSetDescriptions(s1.SetDescriptionList, s2.SetDescriptionList, id, s1.Code));
            }

            return diferencias;
        }
        private void CompararAtributosSet(Set s1, Set s2, List<DiferenciaXml> diferencias)
        {
            var props = typeof(Set).GetProperties().Where(p => p.PropertyType == typeof(string) || p.PropertyType == typeof(int));

            foreach (var prop in props)
            {
                var val1 = prop.GetValue(s1)?.ToString() ?? "";
                var val2 = prop.GetValue(s2)?.ToString() ?? "";

                if (val1 != val2)
                {
                    diferencias.Add(new DiferenciaXml(
                            (int)enumTipoDiferencia.atributosSetDiferente,
                            $"Set {s1.Id} - {s1.Code}: Diferencia en atributo '{prop.Name}': '{val1}' vs '{val2}'",
                            (int)enumSeveridadDiferencia.warning,
                            true)
                    {
                        OrigenDiferencia = (int)enumOrigenXMLDiferencia.ambos,
                        DetalleDiferenciaDescription = $"{val1}@{val2}",
                        DetalleDiferenciaArticulo = $"({ s1.Id }){ s1.Code }",
                        DetalleDiferenciaAtributos = prop.Name
                    });
                }
            }
        }
        private void CompararOpening(Opening o1, Opening o2, string setId, List<DiferenciaXml> diferencias, string setCode)
        {
            if (o1 == null && o2 != null)
            {
                diferencias.Add(new DiferenciaXml(
                    (int)enumTipoDiferencia.openingSetDiferente,
                     $"Set ({setId}) {setCode}: Opening solo en XML actual",
                    (int)enumSeveridadDiferencia.warning,
                    true)
                {
                    OrigenDiferencia = (int)enumOrigenXMLDiferencia.actual,
                    DetalleDiferenciaDescription = $"Set ({setId}) {setCode}: Opening solo en XML actual",
                    DetalleDiferenciaArticulo = $"({setId}) {setCode}",
                    DetalleDiferenciaAtributos = "Opening no encontrado"
                });
                return;
            }

            if (o1 != null && o2 == null)
            {
                diferencias.Add(new DiferenciaXml(
                    (int)enumTipoDiferencia.openingSetDiferente,
                     $"Set ({setId}) {setCode}: Opening solo en XML anterior",
                    (int)enumSeveridadDiferencia.warning,
                    true)
                {
                    OrigenDiferencia = (int)enumOrigenXMLDiferencia.anterior,
                    DetalleDiferenciaDescription = $"Set ({setId}) {setCode}: Opening solo en XML anterior",
                    DetalleDiferenciaArticulo = $"({setId}) {setCode}",
                    DetalleDiferenciaAtributos = "Opening no encontrado"
                });
                return;
            }

            if (o1 != null && o2 != null)
            {
                var props = typeof(Opening).GetProperties();
                foreach (var prop in props)
                {
                    var val1 = prop.GetValue(o1)?.ToString() ?? "";
                    var val2 = prop.GetValue(o2)?.ToString() ?? "";

                    if (val1 != val2)
                    {
                        diferencias.Add(new DiferenciaXml(
                            (int)enumTipoDiferencia.openingSetDiferente,
                             $"Set {setId} - {setCode}: Opening - Diferencia en '{prop.Name}': '{val1}' vs '{val2}'",
                            (int)enumSeveridadDiferencia.warning,
                            true)
                        {
                            OrigenDiferencia = (int)enumOrigenXMLDiferencia.ambos,
                            DetalleDiferenciaDescription = $"{val1}@{val2}",
                            DetalleDiferenciaArticulo = $"({setId}) {setCode}",
                            DetalleDiferenciaAtributos = $"{prop.Name}"
                        });
                    }
                }
            }
        }
        private List<DiferenciaXml> CompararSetDescriptions(List<SetDescription> list1, List<SetDescription> list2, string setId, string setCode)
        {
            var diferenciasSD = new List<DiferenciaXml>();

            var (comunes, soloLista1, soloLista2, diferencias) = SetDescriptionComparer.Comparar(list1, list2);

            foreach (SetDescription sD in soloLista1)
            {
                diferenciasSD.Add(new DiferenciaXml(
                tipo: (int)enumTipoDiferencia.setDescriptionDiferente,
                titulo: setId + "-" + setCode,
                detalleDiferenciaDescription: $"{sD.Id}",
                detalleDiferenciaArticulo: $"({sD.FittingId}) {sD.Fitting?.Description}",
                detalleDiferenciaAtributos: $"MinWidth {sD.MinWidth}   MaxWidth {sD.MaxWidth}   MinHeight {sD.MinHeight}   MaxHeight {sD.MaxHeight}   Position {sD.Position}",
                descripcion: $"SetDescriptionId {sD.Id} solo en el XML anterior. {sD.Fitting?.Description}-MinWidth:{sD.MinWidth}-MaxWidth:{sD.MaxWidth}-MinHeight:{sD.MinHeight}-MaxHeight:{sD.MaxHeight}-Position:{sD.Position}-XPosition:{sD.XPosition}-Alternative:{sD.Alternative}-Horizontal:{sD.Horizontal}-Inverted:{sD.Inverted}",
                origenDiferencia: (int)enumOrigenXMLDiferencia.anterior,
                severidad: (int)enumSeveridadDiferencia.warning,
                visible: true));
            }
            foreach (SetDescription sD in soloLista2)
            {
                diferenciasSD.Add(new DiferenciaXml(
                tipo: (int)enumTipoDiferencia.setDescriptionDiferente,
                titulo: setId + "-" + setCode,
                detalleDiferenciaDescription: $"{sD.Id}",
                detalleDiferenciaArticulo: $"({sD.FittingId}) {sD.Fitting?.Description}",
                detalleDiferenciaAtributos: $"MinWidth {sD.MinWidth}   MaxWidth {sD.MaxWidth}   MinHeight {sD.MinHeight}   MaxHeight {sD.MaxHeight}   Position {sD.Position}",
                descripcion: $"SetDescriptionId {sD.Id} solo en el XML actual. Artículo: {sD.Fitting?.Description}-MinWidth:{sD.MinWidth}-MaxWidth:{sD.MaxWidth}-MinHeight:{sD.MinHeight}-MaxHeight:{sD.MaxHeight}-Position:{sD.Position}-XPosition:{sD.XPosition}-Alternative:{sD.Alternative}-Horizontal:{sD.Horizontal}-Inverted:{sD.Inverted}",
                origenDiferencia: (int)enumOrigenXMLDiferencia.actual,
                severidad: (int)enumSeveridadDiferencia.warning,
                visible: true));
            }
            

            return diferenciasSD;
        }
        private void btn_Config_Click(object sender, EventArgs e)
        {
            ControlCambiosConfiguracion controlCambiosConfiguracion = new ControlCambiosConfiguracion(compareOptions, compareFittingGroups, compareFittings, compareSets, compareColours);
            if (controlCambiosConfiguracion.ShowDialog() == DialogResult.OK)
            {
                compareColours = controlCambiosConfiguracion.compararColores;
                compareFittingGroups = controlCambiosConfiguracion.compararFittingGroups;
                compareFittings = controlCambiosConfiguracion.compararFittings;
                compareOptions = controlCambiosConfiguracion.compararOpciones;
                compareSets = controlCambiosConfiguracion.compararSets;
            }
        }
        private void SetListasSetsComparaSets()
        {
            if (xmlOrigenCargado && xmlNuevoCargado)
            {
                var setsXml1 = xmlOrigen.SetList.Select(s => s.Code.Trim()).ToHashSet();
                var setsXml2 = xmlNuevo.SetList.Select(s => s.Code.Trim()).ToHashSet();

                compareSets.compararSetsComunesList = setsXml1.Intersect(setsXml2).ToList();
                compareSets.compararSetsSoloXml1List = setsXml1.Except(setsXml2).ToList();
                compareSets.compararSetsSoloXml2List = setsXml2.Except(setsXml1).ToList();  
            }
        }
        private void SetListasFittingsComparaFittings()
        {
            if (xmlOrigenCargado && xmlNuevoCargado)
            {
                var fittingGroupsXml1 = xmlOrigen.FittingGroupList.Select(s => s.Class.Trim()).ToHashSet();
                var fittingGroupsXml2 = xmlNuevo.FittingGroupList.Select(s => s.Class.Trim()).ToHashSet();

                compareFittings.compararFittingsComunesList = fittingGroupsXml1.Intersect(fittingGroupsXml2).ToList();
            }
        }
        private void FillListasConfiguracion()
        {
            SetListasSetsComparaSets();
            SetListasFittingsComparaFittings();
        }

    }
    public class DiferenciaSetDescription
    {
        public string Clave { get; set; }
        public string Propiedad { get; set; }
        public string ValorLista1 { get; set; }
        public string ValorLista2 { get; set; }
        public string FittingDescription { get; set; }
        public string FittingId { get; set; }
        public string SetDescriptionId { get; set; }
        public string Position { get; set; }
    }

    public static class SetDescriptionComparer
    {
        // Genera clave lógica SIN Id
        public static string GenerarClave(SetDescription s)
        {
            return $"{s.FittingId}|{s.MinHeight}|{s.MaxHeight}|{s.MinWidth}|{s.MaxWidth}|{s.Horizontal}|{s.Position}|{s.ChainPosition}|{s.Movement}|{s.Inverted}|{s.Alternative}";
        }

        public static (List<SetDescription> comunes, List<SetDescription> soloLista1, List<SetDescription> soloLista2, List<DiferenciaSetDescription> diferencias) Comparar(List<SetDescription> lista1, List<SetDescription> lista2)
        {
            var dict1 = lista1.GroupBy(s => GenerarClave(s))
                              .ToDictionary(g => g.Key, g => g.ToList());

            var dict2 = lista2.GroupBy(s => GenerarClave(s))
                              .ToDictionary(g => g.Key, g => g.ToList());

            var comunes = new List<SetDescription>();
            var soloLista1 = new List<SetDescription>();
            var soloLista2 = new List<SetDescription>();
            var diferencias = new List<DiferenciaSetDescription>();

            foreach (var kvp in dict1)
            {
                if (dict2.TryGetValue(kvp.Key, out var group2))
                {
                    int countComunes = Math.Min(kvp.Value.Count, group2.Count);

                    for (int i = 0; i < countComunes; i++)
                    {
                        var s1 = kvp.Value[i];
                        var s2 = group2[i];

                        var difs = CompararPorReflexion(kvp.Key, s1, s2);

                        if (difs.Count == 0)
                            comunes.Add(s1);
                        else
                            diferencias.AddRange(difs);
                    }

                    if (kvp.Value.Count > group2.Count)
                        soloLista1.AddRange(kvp.Value.Skip(countComunes));

                    if (group2.Count > kvp.Value.Count)
                        soloLista2.AddRange(group2.Skip(countComunes));
                }
                else
                {
                    soloLista1.AddRange(kvp.Value);
                }
            }

            foreach (var kvp in dict2)
            {
                if (!dict1.ContainsKey(kvp.Key))
                    soloLista2.AddRange(kvp.Value);
            }

            return (comunes, soloLista1, soloLista2, diferencias);
        }

        private static List<DiferenciaSetDescription> CompararPorReflexion(string clave, SetDescription s1, SetDescription s2)
        {
            var diferencias = new List<DiferenciaSetDescription>();

            var propiedades = typeof(SetDescription).GetProperties()
                .Where(p => p.Name != "Id"); // ignoramos el Id

            foreach (var prop in propiedades)
            {
                var v1 = prop.GetValue(s1)?.ToString() ?? "";
                var v2 = prop.GetValue(s2)?.ToString() ?? "";

                if (v1 != v2)
                {
                    diferencias.Add(new DiferenciaSetDescription
                    {
                        Clave = clave,
                        Propiedad = prop.Name,
                        ValorLista1 = v1,
                        ValorLista2 = v2,
                        SetDescriptionId = s1.Id.ToString(),
                        FittingId = s1.FittingId.ToString(),
                        FittingDescription = s1.Fitting.Description,
                        Position = s1.Position.ToString()
                    });
                }
            }

            return diferencias;
        }
    }

}
