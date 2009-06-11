using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class TableMamlWriterExtensions
	{
		public static void WriteTableSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> rows, string title)
		{
			var listItems = ListItemBuilder.Build(context, rows.Cast<XmlSchemaObject>());
			writer.WriteTableSection(listItems, title);
		}

		public static void WriteTableSection(this MamlWriter writer, Context context, IEnumerable<string> namespaces, string title)
		{
			var listItems = ListItemBuilder.Build(context, namespaces);

			writer.WriteTableSection(listItems, title);
		}

		private static void WriteTableSection(this MamlWriter writer, ICollection<ListItem> listItems, string title)
		{
			if (listItems.Count == 0)
				return;

			var address = title.ToLower();
			writer.StartSection(title, address);

			writer.StartTable();

			writer.StartTableHeader();
			writer.StartTableRow();

			writer.StartTableRowEntry();
			writer.EndTableRowEntry();
	
			writer.StartTableRowEntry();
			writer.WriteString("Element");
			writer.EndTableRowEntry();
			
			writer.StartTableRowEntry();
			writer.WriteString("Description");
			writer.EndTableRowEntry();

			writer.EndTableRow();
			writer.EndTableHeader();

			foreach (var listItem in listItems)
			{
				writer.StartTableRow();

				writer.StartTableRowEntry();
				writer.WriteMediaInline(listItem.ArtItem.Id);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteLink(listItem.Topic.Id, listItem.Topic.LinkTitle);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteRaw(listItem.SummaryMarkup);
				writer.EndTableRowEntry();

				writer.EndTableRow();
			}

			writer.EndTable();

			writer.EndSection();
		}
	}
}