using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class SummaryMamlWriterExtensions
	{
		public static void WriteSummaryForSchemaSet(this MamlWriter writer, Context context)
		{
			var documentationInfo = context.DocumentationManager.GetSchemaSetDocumentationInfo();
			writer.WriteSummary(documentationInfo);
		}

		public static void WriteSummaryForNamespace(this MamlWriter writer, Context context, string targetNamespace)
		{
			var documentationInfo = context.DocumentationManager.GetNamespaceDocumentationInfo(targetNamespace);
			writer.WriteSummary(documentationInfo);
		}

		public static void WriteSummaryForObject(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			writer.WriteSummary(documentationInfo);
		}

		private static void WriteSummary(this MamlWriter writer, DocumentationInfo documentationInfo)
		{
			if (documentationInfo == null || documentationInfo.SummaryNode == null)
				return;

			writer.WriteRawContent(documentationInfo.SummaryNode);
		}
	}
}
