using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class SummaryMarkupBuilder
	{
		public static string BuildForSchemaSet(Context context)
		{
			var documentationInfo = context.DocumentationManager.GetSchemaSetDocumentationInfo();
			if (documentationInfo == null)
				return string.Empty;

			return GetSummaryMarkup(documentationInfo);
		}

		public static string BuildForNamespace(Context context, string targetNamespace)
		{
			var documentationInfo = context.DocumentationManager.GetNamespaceDocumentationInfo(targetNamespace);
			if (documentationInfo == null)
				return string.Empty;

			return GetSummaryMarkup(documentationInfo);
		}

		public static string BuildForObject(Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			if (documentationInfo == null)
				return string.Empty;

			return GetSummaryMarkup(documentationInfo);
		}

		private static string GetSummaryMarkup(DocumentationInfo documentationInfo)
		{
			if (documentationInfo.SummaryNode == null)
				return string.Empty;

			var childNode = documentationInfo.SummaryNode.ChildNodes[0];
			return MarkupHelper.GetMarkup(childNode);
		}
	}
}
