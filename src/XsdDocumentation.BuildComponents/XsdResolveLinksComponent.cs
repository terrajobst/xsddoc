using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

using XsdDocumentation.BuildComponents.Properties;
using XsdDocumentation.Model;

namespace XsdDocumentation.BuildComponents
{
    public sealed class XsdResolveLinksComponent : BuildComponentCore
    {
        [BuildComponentExport("XsdResolveLinks",
            Copyright = XsdDocMetadata.Copyright,
            IsConfigurable = false,
            IsVisible = false,
            Version = XsdDocMetadata.Version)]
        public sealed class Factory : BuildComponentFactory
        {
            public Factory()
            {
                ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Before, "XSL Transform Component");
                ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Before, "XSL Transform Component");
            }

            public override BuildComponentCore Create()
            {
                return new XsdResolveLinksComponent(BuildAssembler);
            }
        }

        private TopicIndex _topicIndex;

        public XsdResolveLinksComponent(BuildAssemblerCore assembler)
            : base(assembler)
        {
            var versionInfo = GetVersionInfo();
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ComponentLogoFormatted, versionInfo.ProductName, versionInfo.ProductVersion, versionInfo.LegalCopyright);
            WriteMessage(MessageLevel.Info, message);
        }

        private static FileVersionInfo GetVersionInfo()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            Debug.Assert(location != null);
            return FileVersionInfo.GetVersionInfo(location);
        }

        public override void Initialize(XPathNavigator configuration)
        {
            var fileName = configuration.SelectSingleNode("indexFile/@location").Value;
            _topicIndex = new TopicIndex();
            _topicIndex.Load(fileName);
        }

        public override void Apply(XmlDocument document, string key)
        {
            var namespaceManager = new XmlNamespaceManager(document.NameTable);
            namespaceManager.AddNamespace("xsd", Namespaces.XsdDoc);

            var nodes = document.SelectNodes("//xsd:xmlEntityReference", namespaceManager);
            if (nodes == null)
                return;

            foreach (XmlElement node in nodes)
            {
                var parentNode = node.ParentNode;
                var uri = node.InnerText;

                var entry = _topicIndex.FindEntry(uri);
                if (entry == null)
                {
                    WriteMessage(MessageLevel.Warn, Resources.CouldNotResolveXmlEntity, uri);
                }
                else
                {
                    var linkElement = document.CreateElement("ddue", "link", Namespaces.Maml);
                    linkElement.SetAttribute("href", Namespaces.XLink, entry.TopicId);
                    linkElement.SetAttribute("topicType_id", "3272D745-2FFC-48C4-9E9D-CF2B2B784D5F");
                    if (node.HasAttribute("linkText"))
                        linkElement.InnerText = node.GetAttribute("linkText");
                    else
                        linkElement.InnerText = entry.LinkTitle;
                    parentNode.ReplaceChild(linkElement, node);
                }
            }
        }
    }
}