using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class ObsoleteMarkupBuilder
	{
		public static string BuildForNamespace(Context context, string targetNamespace)
		{
			var documentationInfo = context.DocumentationManager.GetNamespaceDocumentationInfo(targetNamespace);
			if (documentationInfo == null)
				return string.Empty;

			return GetObsoleteMarkup(context, documentationInfo);
		}

		public static string BuildForObject(Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			if (documentationInfo == null)
				return string.Empty;

			return GetObsoleteMarkup(context, documentationInfo);
		}

		private static string GetObsoleteMarkup(Context context, DocumentationInfo documentationInfo)
		{
			const string simpleMarkup = @"
<alert class=""note"">
	<para>
		This XML entity is now obsolete.
	</para>
</alert>";

			const string advancedMarkup = @"
<alert class=""note"">
	<para>
		This XML entity is now obsolete. The non-obsolete alternative is ${TopicLink}.
	</para>
</alert>";
			if (!documentationInfo.IsObsolete)
				return string.Empty;

			if (documentationInfo.NonObsoleteAlternative == null)
				return simpleMarkup;

			var topic = context.TopicManager.GetTopic(documentationInfo.NonObsoleteAlternative);
			if (topic == null)
				return simpleMarkup;

			return advancedMarkup.Replace("${TopicLink}", MarkupHelper.GetTopicLink(topic));
		}		
	}
}