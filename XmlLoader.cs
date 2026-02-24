using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static RotoTools.Enums;

namespace RotoTools
{
    public class XmlLoader
    {
        private readonly XmlNamespaceManager _nsmgr;

        // Evento para notificar el progreso
        public event Action<string, string> OnLoadingInfo;

        public XmlLoader(XmlNamespaceManager namespaceManager)
        {
            _nsmgr = namespaceManager;
        }

        private void ShowLoadingInfo(string type, string value)
        {
            OnLoadingInfo?.Invoke(type, value);
        }

        #region Métodos de carga de XML
        public string LoadSupplier(XmlDocument doc)
        {
            XmlNode nodePrefHardware = doc.SelectSingleNode("//hw:PrefHardware", _nsmgr);
            if (nodePrefHardware == null) return string.Empty;
            return nodePrefHardware.Attributes["supplier"]?.Value;
        }
        public int LoadHardwareType(string supplier)
        {
            if (supplier.ToUpper().Contains("ALU"))
            {
                return (int)enumHardwareType.Aluminio;
            }
            else if (supplier.ToUpper().Contains("PAX"))
            {
                return (int)enumHardwareType.PAX;
            }
            else
            {
                return (int)enumHardwareType.PVC;
            }
        }
        public List<FittingGroup> LoadFittingGroups(XmlDocument doc)
        {
            try
            {
                List<FittingGroup> fittingGroupsList = new List<FittingGroup>();
                XmlNodeList nodosFittingGroupList = doc.SelectNodes("//hw:FittingGroups/hw:FittingGroup", _nsmgr);

                foreach (XmlNode fittingGroupNode in nodosFittingGroupList)
                {
                    FittingGroup fittingGroup = new FittingGroup();
                    fittingGroup.Id = Helpers.TryParseInt(fittingGroupNode.Attributes["id"]?.Value);
                    fittingGroup.Class = fittingGroupNode.Attributes["class"]?.Value;

                    ShowLoadingInfo("FittingGroup", fittingGroup.Class);

                    fittingGroupsList.Add(fittingGroup);
                }

                return fittingGroupsList;
            }
            catch
            {
                return null;
            }
        }
        public List<Colour> LoadColourMaps(XmlDocument doc)
        {
            try
            {
                List<Colour> colourList = new List<Colour>();
                XmlNodeList nodosColourList = doc.SelectNodes("//hw:ColourMaps/hw:Colour", _nsmgr);

                foreach (XmlNode colourNode in nodosColourList)
                {
                    Colour colour = new Colour();
                    colour.Name = colourNode.Attributes["name"]?.Value;

                    ShowLoadingInfo("Colour", colour.Name);

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
        public List<Article> GetArticles(XmlNode parentNode)
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
                    if (articleNode.Name == "hw:Generation")
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
                                    articleGeneration.XPosition = Helpers.TryParseDouble(generationArticleNode?.Attributes["x"]?.Value);
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
        public List<Option> LoadArticleOptions(XmlNode generationArticleNode)
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
        public List<Option> LoadDocOptions(XmlDocument doc)
        {
            try
            {
                List<Option> optionList = new List<Option>();
                XmlNodeList nodosOptionsList = doc.SelectNodes("//hw:Options/hw:Option", _nsmgr);
                if (nodosOptionsList != null)
                {
                    foreach (XmlNode optionNode in nodosOptionsList)
                    {
                        Option option = new Option();
                        option.Name = optionNode.Attributes["Name"]?.Value;

                        ShowLoadingInfo("Option", option.Name);
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
        public List<Value> GetOptionValues(XmlNode optionNode)
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
        public List<Fitting> LoadFittings(XmlDocument doc)
        {
            try
            {
                List<Fitting> fittingList = new List<Fitting>();
                XmlNodeList nodosFittingList = doc.SelectNodes("//hw:Fittings/hw:Fitting", _nsmgr);
                int i = 0;
                foreach (XmlNode fittingNode in nodosFittingList)
                {
                    i++;
                    Fitting fitting = new Fitting();
                    fitting.Id = Helpers.TryParseInt(fittingNode.Attributes["id"]?.Value);
                    fitting.Ref = fittingNode.Attributes["ref"]?.Value;
                    fitting.Description = fittingNode.Attributes["Description"]?.Value;
                    fitting.Manufacturer = fittingNode.Attributes["Manufacturer"]?.Value;
                    fitting.FittingGroupId = int.Parse(fittingNode.Attributes["fittingGroupId"]?.Value);
                    fitting.Location = fittingNode.Attributes["location"]?.Value;
                    fitting.FittingType = fittingNode.Attributes["fittingType"]?.Value;
                    fitting.System = fittingNode.Attributes["system"]?.Value;
                    fitting.HandUseable = fittingNode.Attributes["handUseable"]?.Value;
                    fitting.Lenght = Helpers.TryParseDouble(fittingNode.Attributes["length"]?.Value);
                    fitting.StartCuttable = Helpers.TryParseBool(fittingNode.Attributes["StartCuttable"]?.Value);
                    fitting.EndCuttable = Helpers.TryParseBool(fittingNode.Attributes["EndCuttable"]?.Value);

                    ShowLoadingInfo("Fitting", fitting.Description);
                    fitting.ArticleList = GetArticles(fittingNode);
                    fitting.OperationList = GetOperations(fittingNode);
                    fittingList.Add(fitting);
                }

                foreach (Fitting fitting in fittingList)
                {
                    if (!fitting.ArticleList.Any()) continue;

                    foreach (Article article in fitting.ArticleList)
                    {
                        article.Fitting = fittingList.FirstOrDefault(f => f.Ref == article.Ref);
                    }
                }

                return fittingList;
            }
            catch
            {
                return null;
            }
        }
        public List<Operation> GetOperations(XmlNode parentNode)
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
        public List<Set> LoadSets(XmlDocument doc, List<Fitting> fittingList)
        {
            try
            {
                List<Set> setList = new List<Set>();
                XmlNodeList nodosSetList = doc.SelectNodes("//hw:Sets/hw:Set", _nsmgr);

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
                    set.Version = LoadSetVersion(setNode);
                    set.Opening = GetSetOpening(setNode);
                    
                    if (set.Code != null) 
                        set.WindowType = SetWindowType(set.Code, set.Opening);
                    
                    set.SetDescriptionList = GetSetDescripcionList(setNode, fittingList);
                    ShowLoadingInfo("Set", set.Code);

                    setList.Add(set);
                }

                return setList;
            }
            catch
            {
                return null;
            }
        }

        public string LoadSetVersion(XmlNode setNode)
        {
            try
            {
                foreach (XmlNode node in setNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        string comentario = node.Value?.Trim();

                        if (!string.IsNullOrEmpty(comentario) && comentario.StartsWith("VER:"))
                        {
                            // Parsear DetalleDiferenciaAtributos -> "valor1@valor2"
                            var partes = comentario?.Split(new[] { ":" }, StringSplitOptions.None);
                            if (partes != null && partes.Length == 2)
                            {
                                return partes[1];
                            }
                        }
                    }
                    return "";
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
        private int SetWindowType(string setCode, Opening opening)
        {
            if (opening.Turn != null && opening.Tilt != null && opening.Right != null)
            {
                return SetWindowTypeFromCode(setCode);
            }
            if (opening.Turn != null && opening.Tilt != null && opening.Left != null)
            {
                return SetWindowTypeFromCode(setCode);
            }
            if (opening.Turn != null && opening.Tilt == null && opening.Right != null && opening.Outer == null)
            {
                return SetWindowTypeFromCode(setCode);
            }
            if (opening.Turn != null && opening.Tilt == null && opening.Left != null && opening.Outer == null)
            {
                return SetWindowTypeFromCode(setCode);
            }
            if (opening.Turn != null && opening.Tilt == null && opening.Right != null && opening.Outer != null)
            {
                return SetWindowTypeFromCode(setCode);
            }
            if (opening.Turn != null && opening.Tilt == null && opening.Left != null && opening.Outer != null)
            {
                return SetWindowTypeFromCode(setCode);
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Left != null && opening.Sliding != null && opening.Lift == null)
            {
                return (int)enumWindowType.Corredera;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Right != null && opening.Sliding != null && opening.Lift == null)
            {
                return (int)enumWindowType.Corredera;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Right == null && opening.Left == null && opening.Sliding != null)
            {
                return (int)enumWindowType.Corredera;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Right == null && opening.Left != null && opening.Sliding != null && opening.Lift != null)
            {
                return (int)enumWindowType.Elevable;
            }
            if (opening.Turn == null && opening.Tilt == null && opening.Right != null && opening.Left == null && opening.Sliding != null && opening.Lift != null)
            {
                return (int)enumWindowType.Elevable;
            }
            if (opening.Turn == null && opening.Tilt != null && opening.Bottom != null && opening.Sliding == null)
            {
                return (int)enumWindowType.Abatible;
            }
            if (opening.Turn == null && opening.Tilt != null && opening.Right != null && opening.Left == null && opening.Sliding != null && opening.Bottom != null)
            {
                return (int)enumWindowType.Osciloparalela;
            }
            if (opening.Turn == null && opening.Tilt != null && opening.Right == null && opening.Left != null && opening.Sliding != null && opening.Bottom != null)
            {
                return (int)enumWindowType.Osciloparalela;
            }

            return (int)enumWindowType.Otro;
        }

        private int SetWindowTypeFromCode(string setCode)
        {
            if (setCode.ToUpper().Contains("BALC"))
            {
                return (int)enumWindowType.Balconera;
            }
            if (setCode.ToUpper().Contains("SEC"))
            {
                return (int)enumWindowType.PuertaSecundaria;
            }
            if (setCode.ToUpper().Contains("PUERTA"))
            {
                return (int)enumWindowType.Puerta;
            }
            if (setCode.ToUpper().Contains("PLG"))
            {
                return (int)enumWindowType.Plegable;
            }
            if (setCode.ToUpper().Contains("V)2P"))
            {
                return (int)enumWindowType.Ventana;
            }
            if (setCode.ToUpper().Contains("OSCILOBATIENTE"))
            {
                return (int)enumWindowType.Ventana;
            }
            if (setCode.ToUpper().Contains("PRACTICABLE"))
            {
                return (int)enumWindowType.Ventana;
            }

            return (int)enumWindowType.Otro;
        }

        public string LoadFittingsVersion(XmlDocument doc)
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
        public string LoadOptionsVersion(XmlDocument doc)
        {
            // Navega al nodo Fittings
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

            XmlNode optionsNode = doc.SelectSingleNode("//hw:Options", nsmgr);

            if (optionsNode != null)
            {
                foreach (XmlNode node in optionsNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        string comentario = node.Value?.Trim();

                        if (!string.IsNullOrEmpty(comentario) && comentario.StartsWith("O:"))
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
        public string LoadColoursVersion(XmlDocument doc)
        {
            // Navega al nodo Fittings
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

            XmlNode optionsNode = doc.SelectSingleNode("//hw:ColourMaps", nsmgr);

            if (optionsNode != null)
            {
                foreach (XmlNode node in optionsNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        string comentario = node.Value?.Trim();

                        if (!string.IsNullOrEmpty(comentario) && comentario.StartsWith("C:"))
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
        public string LoadFittingGroupVersion(XmlDocument doc)
        {
            // Navega al nodo Fittings
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("hw", "http://www.preference.com/XMLSchemas/2006/Hardware");

            XmlNode optionsNode = doc.SelectSingleNode("//hw:ColourMaps", nsmgr);

            if (optionsNode != null)
            {
                foreach (XmlNode node in optionsNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        string comentario = node.Value?.Trim();

                        if (!string.IsNullOrEmpty(comentario) && comentario.StartsWith("C:"))
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
        public Opening GetSetOpening(XmlNode setNode)
        {
            try
            {
                XmlNode openingNode = setNode.SelectSingleNode("hw:Opening", _nsmgr);
                if (openingNode != null)
                {
                    Opening opening = new Opening();
                    opening.Active = openingNode.Attributes["active"]?.Value;
                    opening.Turn = openingNode.Attributes["turn"]?.Value;
                    opening.Right = openingNode.Attributes["right"]?.Value;
                    opening.Left = openingNode.Attributes["left"]?.Value;
                    opening.Tilt = openingNode.Attributes["tilt"]?.Value;
                    opening.Bottom = openingNode.Attributes["bottom"]?.Value;
                    opening.Outer = openingNode.Attributes["outer"]?.Value;
                    opening.Sliding = openingNode.Attributes["sliding"]?.Value;
                    opening.Lift = openingNode.Attributes["lift"]?.Value;
                    return opening;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public List<SetDescription> GetSetDescripcionList(XmlNode setNode, List<Fitting> fittingList)
        {
            List<SetDescription> setDescriptionsList = new List<SetDescription>();
            try
            {
                XmlNodeList setDescriptions = setNode.SelectNodes("hw:SetDescription", _nsmgr);
                foreach (XmlNode descNode in setDescriptions)
                {
                    var attrs = descNode.Attributes;
                    var setDescription = new SetDescription
                    {
                        Id = Helpers.TryParseInt(attrs["id"]?.Value),
                        FittingId = Helpers.TryParseInt(attrs["fittingId"]?.Value),
                        MinHeight = Helpers.TryParseDouble(attrs["minHeight"]?.Value),
                        MaxHeight = Helpers.TryParseDouble(attrs["maxHeight"]?.Value),
                        MinWidth = Helpers.TryParseDouble(attrs["minWidth"]?.Value),
                        MaxWidth = Helpers.TryParseDouble(attrs["maxWidth"]?.Value),
                        Horizontal = Helpers.TryParseBool(attrs["horizontal"]?.Value),
                        Position = Helpers.TryParseInt(attrs["position"]?.Value),
                        ReferencePoint = attrs["referencePoint"]?.Value,
                        ChainPosition = Helpers.TryParseInt(attrs["chainPosition"]?.Value),
                        Movement = attrs["movement"]?.Value,
                        Inverted = Helpers.TryParseBool(attrs["inverted"]?.Value),
                        XPosition = Helpers.TryParseDouble(attrs["x"]?.Value),
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
        public List<Option> LoadSetDescriptionOptions(XmlNode setDescriptionNode)
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

        #endregion
    }
}
