using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class MamlWriterExtensions
	{
		private const string NonBlockingSpaceEntityName = "&#160;";

		public static void WriteHtmlIndent(this MamlWriter writer, int level)
		{
			writer.WriteStartElement("markup", Namespaces.Maml);
			for (var i = 0; i < level * 6; i++)
				writer.WriteRaw(NonBlockingSpaceEntityName);
			writer.WriteEndElement();
		}

		public static void WriteHtmlTopicLink(this MamlWriter writer, Topic topic)
		{
			var url = String.Format(CultureInfo.InvariantCulture, "/html/{0}.htm", topic.Id);
			writer.WriteStartElement("a", Namespaces.Maml);
			writer.WriteAttributeString("href", url);
			writer.WriteString(topic.LinkTitle);
			writer.WriteEndElement();
		}

		public static void StartHtmlArtItem(this MamlWriter writer, ArtItem artItem)
		{
			writer.WriteStartElement("markup", Namespaces.Maml);
			writer.WriteStartElement("nobr", String.Empty);
			writer.WriteStartElement("artLink");
			writer.WriteAttributeString("target", artItem.Id);
			writer.WriteEndElement();
			writer.WriteRaw(NonBlockingSpaceEntityName);
		}

		public static void EndHtmlArtItem(this MamlWriter writer)
		{
			writer.WriteEndElement(); // nobr
			writer.WriteEndElement(); // markup
		}

		public static void WriteHtmlArtItemWithText(this MamlWriter writer, ArtItem artItem, string text)
		{
			writer.StartHtmlArtItem(artItem);
			writer.WriteString(text);
			writer.EndHtmlArtItem();
		}

		public static void WriteHtmlArtItemWithTopicLink(this MamlWriter writer, ArtItem artItem, Topic topic)
		{
			writer.StartHtmlArtItem(artItem);
			writer.WriteHtmlTopicLink(topic);
			writer.EndHtmlArtItem();
		}

		public static void WriteArtItemInline(this MamlWriter writer, ArtItem artItem)
		{
			writer.WriteMediaLinkInline(artItem.Id);
		}

		public static void WriteTopicLink(this MamlWriter writer, Topic topic)
		{
			writer.WriteLink(topic.Id, topic.LinkTitle);
		}

		private static void WriteTopicLinkWithReferenceMarker(this MamlWriter writer, Topic rootItemTopic)
		{
			const string xmlTopicType = "3272D745-2FFC-48C4-9E9D-CF2B2B784D5F";

			writer.WriteLink(rootItemTopic.Id, rootItemTopic.LinkTitle, xmlTopicType);
		}

		public static void WriteSummary(this MamlWriter writer, DocumentationInfo documentationInfo)
		{
			if (documentationInfo != null && documentationInfo.SummaryNode != null)
				writer.WriteRawContent(documentationInfo.SummaryNode);
		}

		public static void WriteSummaryForObject(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			writer.WriteSummary(documentationInfo);
		}

		public static void WriteNamespaceInfo(this MamlWriter writer, Context context, string namespaceUri)
		{
			writer.WriteRaw(@"<markup><p /><b>Namespace:</b> ");
			writer.WriteNamespaceLink(context, namespaceUri);
			writer.WriteRaw(@"<br/></markup>");
		}

		public static void WriteNamespaceAndSchemaInfo(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var targetNamespace = obj.GetSchema().TargetNamespace;

			writer.WriteRaw(@"<markup><p /><b>Namespace:</b> ");
			writer.WriteNamespaceLink(context, targetNamespace);
			writer.WriteRaw(@"<br/><b>Schema:</b> ");
			writer.WriteSchemaLink(context, obj);
			writer.WriteRaw(@"<br/></markup>");
		}

		public static void WriteNamespaceLink(this MamlWriter writer, Context context, string namespaceUri)
		{
			var topic = context.TopicManager.GetNamespaceTopic(namespaceUri);
			if (topic == null)
				writer.WriteString(namespaceUri ?? "Empty");
			else
				writer.WriteHtmlTopicLink(topic);
		}

		private static void WriteSchemaLink(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var topic = context.TopicManager.GetTopic(obj.GetSchema());
			writer.WriteHtmlTopicLink(topic);
		}

		public static void WriteLinksForObject(this MamlWriter writer, XmlSchemaObject obj, Context context)
		{
			var root = obj.GetRoot();
			if (root != null && root != obj)
			{
				var rootItemTopic = context.TopicManager.GetTopic(root);
				if (rootItemTopic != null)
					writer.WriteTopicLinkWithReferenceMarker(rootItemTopic);
			}

			var targetNamespace = obj.GetSchema().TargetNamespace;
			var targetNamespaceTopic = context.TopicManager.GetNamespaceTopic(targetNamespace);
			if (targetNamespaceTopic != null)
				writer.WriteTopicLinkWithReferenceMarker(targetNamespaceTopic);

			var info = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			if (info != null && info.RelatedTopicsNode != null)
				writer.WriteRawContent(info.RelatedTopicsNode);
		}

		public static void WriteList(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> schemaObjects)
		{
			var listItems = ListItemBuilder.Build(context, schemaObjects);

			var isFirst = true;
			foreach (var item in listItems)
			{
				if (isFirst)
				{
					writer.WriteString(" ");
					isFirst = false;
				}

				writer.WriteHtmlArtItemWithTopicLink(item.ArtItem, item.Topic);
			}
		}
	}
}