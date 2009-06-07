using System;
using System.Xml;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class RelatedTopicsMarkupBuilder
	{
		public static string Build(Context context, XmlSchemaObject obj)
		{
			var relatedTopicsDocument = new XmlDocument();
			var relatedTopicsElement = relatedTopicsDocument.CreateElement("ddue", "relatedTopics", Namespaces.Maml);
			relatedTopicsDocument.AppendChild(relatedTopicsElement);

			var rootItem = FindRootItem(obj);
			if (rootItem != null && rootItem != obj)
			{
				var rootItemTopic = context.TopicManager.GetTopic(rootItem);
				AddXmlTopicLink(relatedTopicsElement, rootItemTopic);
			}

			var targetNamespace = obj.GetSchema().TargetNamespace;
			var targetNamespaceTopic = context.TopicManager.GetNamespaceTopic(targetNamespace);
			AddXmlTopicLink(relatedTopicsElement, targetNamespaceTopic);

			var info = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			if (info != null && info.RelatedTopicsNode != null)
			{
				foreach (XmlElement linkElement in info.RelatedTopicsNode.ChildNodes)
				{
					var importedLinkElement = (XmlElement) relatedTopicsDocument.ImportNode(linkElement, true);
					relatedTopicsElement.AppendChild(importedLinkElement);
				}
			}

			return MarkupHelper.GetMarkup(relatedTopicsDocument);
		}

		private static XmlSchemaObject FindRootItem(XmlSchemaObject obj)
		{
			var lastObject = obj;
			var parent = lastObject.Parent;
			while (parent != null)
			{
				var parentAsSchema = parent as XmlSchema;
				if (parentAsSchema != null)
					return lastObject;

				lastObject = parent;
				parent = parent.Parent;
			}

			return null;
		}

		private static void AddXmlTopicLink(XmlNode element, Topic topic)
		{
			if (topic != null)
			{
				var linkElement = element.OwnerDocument.CreateElement("ddue", "link", Namespaces.Maml);
				element.AppendChild(linkElement);
				linkElement.SetAttribute("href", Namespaces.XLink, topic.Id);
				linkElement.SetAttribute("topicType_id", "3272D745-2FFC-48C4-9E9D-CF2B2B784D5F");
			}
		}
	}
}
