using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

using XsdDocumentation.BuildComponents.Properties;
using XsdDocumentation.Model;

namespace XsdDocumentation.BuildComponents
{
	public sealed class XsdResolveLinksComponent : BuildComponent
	{
		private TopicIndex _topicIndex;

		public XsdResolveLinksComponent(BuildAssembler assembler, XPathNavigator configuration)
			: base(assembler, configuration)
		{
			var versionInfo = GetVersionInfo();
			var message = string.Format(CultureInfo.CurrentCulture, Resources.ComponentLogoFormatted, versionInfo.ProductName, versionInfo.ProductVersion, versionInfo.LegalCopyright);
			var fileName = configuration.SelectSingleNode("indexFile/@location").Value;

			WriteMessage(MessageLevel.Info, message);
			_topicIndex = new TopicIndex();
			_topicIndex.Load(fileName);
		}

		private static FileVersionInfo GetVersionInfo()
		{
			var location = Assembly.GetExecutingAssembly().Location;
			Debug.Assert(location != null);
			return FileVersionInfo.GetVersionInfo(location);
		}

		public override void Apply(XmlDocument document, string key)
		{
			var namespaceManager = new XmlNamespaceManager(document.NameTable);
			namespaceManager.AddNamespace("xsd", Namespaces.XsdDoc);

			var nodes = document.SelectNodes("//xsd:xmlEntityReference", namespaceManager);
			if (nodes != null)
			{
				foreach (XmlNode node in nodes)
				{
					var parentNode = node.ParentNode;
					var uri = node.InnerText;

					var entry = _topicIndex.FindEntry(uri);
					if (entry == null)
					{
						var message = string.Format(CultureInfo.CurrentCulture, Resources.CouldNotResolveXmlEntity, uri);
						BuildAssembler.MessageHandler(GetType(), MessageLevel.Warn, message);
					}
					else
					{
						var linkElement = document.CreateElement("ddue", "link", Namespaces.Maml);
						linkElement.SetAttribute("href", Namespaces.XLink, entry.TopicId);
						linkElement.SetAttribute("topicType_id", "3272D745-2FFC-48C4-9E9D-CF2B2B784D5F");
						linkElement.InnerText = entry.LinkTitle;
						parentNode.ReplaceChild(linkElement, node);
					}
				}
			}
		}
	}
}