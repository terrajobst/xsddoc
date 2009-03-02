using System;
using System.Xml;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class RemarksMarkupBuilder
	{
		public static string BuildForSchemaSet(Context context)
		{
			var documentationInfo = context.DocumentationManager.GetSchemaSetDocumentationInfo();
			if (documentationInfo == null)
				return string.Empty;

			return GetRemarksMarkup(documentationInfo);
		}

		public static string BuildForNamespace(Context context, string targetNamespace)
		{
			var documentationInfo = context.DocumentationManager.GetNamespaceDocumentationInfo(targetNamespace);
			if (documentationInfo == null)
				return string.Empty;

			return GetRemarksMarkup(documentationInfo);
		}

		public static string BuildForObject(Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			if (documentationInfo == null)
				return string.Empty;

			return GetRemarksMarkup(documentationInfo);
		}

		private static string GetRemarksMarkup(DocumentationInfo documentationInfo)
		{
			if (documentationInfo.RemarksNode == null)
				return string.Empty;

			var contentNode = documentationInfo.RemarksNode.ChildNodes[0];
			return MarkupHelper.GetMarkup(contentNode);
		}
	}
}
