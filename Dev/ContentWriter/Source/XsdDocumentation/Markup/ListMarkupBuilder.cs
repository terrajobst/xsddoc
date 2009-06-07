using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class ListMarkupBuilder
	{
		public static string Build(Context context, IEnumerable<XmlSchemaObject> schemaObjects)
		{
			var listItems = ListItemBuilder.Build(context, schemaObjects);

			var sb = new StringBuilder();
			foreach (var item in listItems)
			{
				if (sb.Length > 0)
					sb.Append(" ");

				string markup = GetItemMarkup(item);
				sb.Append(markup);
			}

			return sb.ToString();
		}

		private static string GetItemMarkup(ListItem item)
		{
			return MarkupHelper.GetHtmlImageWithTopicLink(item.ArtItem, item.Topic);
		}
	}
}