using System;
using System.Collections.Generic;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class ListMamlWriterExtensions
	{
		public static void WriteList(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> schemaObjects)
		{
			var listItems = ListItemBuilder.Build(context, schemaObjects);

			var isFirst = true;
			foreach (var item in listItems)
			{
				if (isFirst)
				{
					writer.WriteString(" ");
					isFirst = false;
				}

				writer.WriteHtmlImageWithLink(item.ArtItem.Id, item.Topic.Id, item.Topic.LinkTitle);
			}
		}
	}
}