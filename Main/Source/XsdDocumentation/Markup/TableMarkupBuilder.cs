using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class TableMarkupBuilder
	{
		public static string GetTableSectionMarkup(Context context, IEnumerable<XmlSchemaObject> rows, string title)
		{
			var listItems = ListItemBuilder.Build(context, rows.Cast<XmlSchemaObject>());

			return GetTableSectionMarkup(listItems, title);
		}

		public static string GetTableSectionMarkup(Context context, IEnumerable<string> namespaces, string title)
		{
			var listItems = ListItemBuilder.Build(context, namespaces);

			return GetTableSectionMarkup(listItems, title);
		}

		private static string GetTableSectionMarkup(ICollection<ListItem> listItems, string title)
		{
			const string headerTemplate = @"
<section address=""${Address}"">
	<title>${Title}</title>
	<content>
<table>
	<tableHeader>
		<row>
			<entry></entry>
			<entry>Element</entry>
			<entry>Description</entry>
		</row>
	</tableHeader>";

			const string rowTemplate = @"
	<row>
		<entry>
			${Image}
		</entry>
		<entry>
			${TopicLink}
		</entry>
		<entry>
			${Description}
		</entry>
	</row>";
			const string footerTemplate = @"
</table>	
</content>
</section>";

			if (listItems.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			var header = headerTemplate;
			header = header.Replace("${Address}", title.ToLower());
			header = header.Replace("${Title}", title);
			sb.Append(header);
			foreach (var listItem in listItems)
			{
				string replacedRow = rowTemplate;
				replacedRow = replacedRow.Replace("${Image}", MarkupHelper.GetImage(listItem.ArtItem));
				replacedRow = replacedRow.Replace("${TopicLink}", MarkupHelper.GetTopicLink(listItem.Topic));
				replacedRow = replacedRow.Replace("${Description}", listItem.SummaryMarkup);

				sb.Append(replacedRow);
			}
			sb.Append(footerTemplate);
			return sb.ToString();
		}
	}
}