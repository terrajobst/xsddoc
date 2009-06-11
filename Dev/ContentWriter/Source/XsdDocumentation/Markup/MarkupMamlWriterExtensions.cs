using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class MarkupMamlWriterExtensions
	{
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
				writer.WriteHtmlLink(topic.Id, topic.LinkTitle);
		}

		private static void WriteSchemaLink(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var topic = context.TopicManager.GetTopic(obj.GetSchema());
			writer.WriteHtmlLink(topic.Id, topic.LinkTitle);
		}

		public static void WriteElementTypeName(this MamlWriter writer, Context context, XmlSchemaElement element)
		{
			if (element.ElementSchemaType is XmlSchemaSimpleType)
				writer.WriteTypeName(context, element.ElementSchemaType);
		}

		public static void WriteTypeName(this MamlWriter writer, Context context, XmlSchemaType type)
		{
			if (type == null || type == XmlSchemaType.GetBuiltInComplexType(XmlTypeCode.Item))
				return;

			writer.WriteTypeName(context.TopicManager, type);
		}
	}
}