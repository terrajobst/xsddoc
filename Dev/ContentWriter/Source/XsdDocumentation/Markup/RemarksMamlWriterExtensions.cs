using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class RemarksMamlWriterExtensions
	{
		public static void WriteRemarksForSchemaSet(this MamlWriter writer, Context context)
		{
			var documentationInfo = context.DocumentationManager.GetSchemaSetDocumentationInfo();
			writer.WriteRemarks(documentationInfo);
		}

		public static void WriteRemarksForNamespace(this MamlWriter writer, Context context, string targetNamespace)
		{
			var documentationInfo = context.DocumentationManager.GetNamespaceDocumentationInfo(targetNamespace);
			writer.WriteRemarks(documentationInfo);
		}

		public static void WriteRemarksForObject(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			writer.WriteRemarks(documentationInfo);
		}

		private static void WriteRemarks(this MamlWriter writer, DocumentationInfo documentationInfo)
		{
			if (documentationInfo == null || documentationInfo.RemarksNode == null)
				return;

			var contentNode = documentationInfo.RemarksNode.ChildNodes[0];
			writer.StartSection("Remarks", "remarks");
			writer.WriteRawContent(contentNode);
			writer.EndSection();
		}
	}
}
