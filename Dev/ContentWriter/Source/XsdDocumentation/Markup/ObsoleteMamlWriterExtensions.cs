using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class ObsoleteMamlWriterExtensions
	{
		public static void WriteObsoleteInfo(this MamlWriter writer, Context context, string targetNamespace)
		{
			var documentationInfo = context.DocumentationManager.GetNamespaceDocumentationInfo(targetNamespace);
			if (documentationInfo == null)
				return;

			writer.WriteObsoleteInfo(context, documentationInfo);
		}

		public static void WriteObsoleteInfo(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			if (documentationInfo == null)
				return;

			writer.WriteObsoleteInfo(context, documentationInfo);
		}

		private static void WriteObsoleteInfo(this MamlWriter writer, Context context, DocumentationInfo documentationInfo)
		{
			if (!documentationInfo.IsObsolete)
				return;

			if (documentationInfo.NonObsoleteAlternative == null)
			{
				writer.WriteObsoleteInfo();
				return;
			}

			var topic = context.TopicManager.GetTopic(documentationInfo.NonObsoleteAlternative);
			if (topic == null)
			{
				writer.WriteObsoleteInfo();
				return;
			}

			writer.WriteObsoleteInfo(topic);
		}

		private static void WriteObsoleteInfo(this MamlWriter writer)
		{
			writer.StartAlert(AlertClass.Note);
			writer.StartParagraph();
			writer.WriteString("This XML entity is now obsolete.");
			writer.EndParagraph();
			writer.EndAlert();
		}

		private static void WriteObsoleteInfo(this MamlWriter writer, Topic topic)
		{
			writer.StartAlert(AlertClass.Note);
			writer.StartParagraph();
			writer.WriteString("This XML entity is now obsolete. The non-obsolete alternative is ");
			writer.WriteLink(topic.LinkIdUri, topic.LinkTitle);
			writer.EndParagraph();
			writer.EndAlert();
		}
	}
}