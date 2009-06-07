using System;
using System.IO;
using System.Text;
using System.Xml;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class MarkupHelper
	{
		public static string XmlEncode(string text)
		{
			using (var sw = new StringWriter())
			using (var xw = new XmlTextWriter(sw))
			{
				xw.WriteString(text);
				return sw.ToString();
			}
		}

		public static string GetMarkup(XmlNode markupNode)
		{
			if (markupNode == null)
				return String.Empty;

			using (var stringWriter = new StringWriter())
			using (var xmlTextWriter = new XmlTextWriter(stringWriter))
			{
				markupNode.WriteContentTo(xmlTextWriter);
				return stringWriter.ToString();
			}
		}

		public static string GetImage(ArtItem artItem)
		{
			var markup = @"<mediaLinkInline><image xlink:href=""${MediaLink}""/></mediaLinkInline>";
			return markup.Replace("${MediaLink}", artItem.Id);
		}

		public static string GetTopicLink(Topic topic)
		{
			var markup = @"<link xlink:href=""${TopicId}"">${TopicName}</link>";
			markup = markup.Replace("${TopicId}", topic.Id);
			markup = markup.Replace("${TopicName}", topic.LinkTitle);
			return markup;
		}

		public static string GetHtmlTopicLink(Topic topic)
		{
			var markup = @"<a href=""/html/${TopicId}.htm"">${Text}</a>";
			markup = markup.Replace("${TopicId}", topic.Id);
			markup = markup.Replace("${Text}", topic.LinkTitle);
			return markup;
		}

		public static string GetHtmlImageWithText(ArtItem artItem, string textMarkup)
		{
			var markup = @"<markup><nobr xmlns=""""><artLink target=""${MediaLink}"" />&#160;${Text}</nobr></markup>";
			markup = markup.Replace("${MediaLink}", artItem.Id);
			markup = markup.Replace("${Text}", textMarkup);
			return markup;
		}

		public static string GetHtmlImageWithTopicLink(ArtItem artItem, Topic topic)
		{
			return GetHtmlImageWithText(artItem, GetHtmlTopicLink(topic));
		}

		public static string GenerateIndent(int level)
		{
			var sb = new StringBuilder();
			sb.Append("<markup>");
			for (int i = 0; i < level * 6; i++)
				sb.Append("&#160;");
			sb.Append("</markup>");

			return sb.ToString();
		}
	}
}