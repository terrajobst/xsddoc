using System;
using System.Text;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class ConstrainedMarkupBuilder
	{
		private const string header = @"
<table>
	<tableHeader>
		<row>
			<entry></entry>
			<entry>Type</entry>
			<entry>Description</entry>
			<entry>Selector</entry>
			<entry>Fields</entry>
		</row>
	</tableHeader>";

		private const string rowTemplate = @"
	<row>
		<entry>
			${Image}
		</entry>
		<entry>
			${Type}
		</entry>
		<entry>
			${Description}
		</entry>
		<entry>
			${Selector}
		</entry>
		<entry>
			${Fields}
		</entry>
	</row>";

		private const string footer = @"
</table>";

		private sealed class ConstraintRowBuilder: XmlSchemaSetVisitor
		{
			private Context _context;
			private StringBuilder _sb;

			public ConstraintRowBuilder(Context context, StringBuilder sb)
			{
				_context = context;
				_sb = sb;
			}

			private void AddRow(ArtItem artItem, string constrainedType, XmlSchemaIdentityConstraint constraint)
			{
				var row = rowTemplate;
				row = row.Replace("${Image}", MarkupHelper.GetImage(artItem));
				row = row.Replace("${Type}", constrainedType);
				row = row.Replace("${Description}", SummaryMarkupBuilder.BuildForObject(_context, constraint));
				row = row.Replace("${Selector}", MarkupHelper.XmlEncode(constraint.Selector.XPath));
				row = row.Replace("${Fields}", GetFieldList(constraint.Fields));
				_sb.Append(row);
			}

			private static string GetFieldList(XmlSchemaObjectCollection fields)
			{
				if (fields.Count == 1)
				{
					var field = (XmlSchemaXPath) fields[0];
					return MarkupHelper.XmlEncode(field.XPath);
				}

				var sb = new StringBuilder();
				sb.Append("<list class=\"ordered\">");
				foreach (XmlSchemaXPath	field in fields)
				{
					sb.Append("<listItem><para>");
					sb.Append(MarkupHelper.XmlEncode(field.XPath));
					sb.Append("</para></listItem>");
				}

				sb.Append("</list>");
				return sb.ToString();
			}

			protected override void Visit(XmlSchemaKey key)
			{
				AddRow(ArtItem.Key, "Key", key);
			}


			protected override void Visit(XmlSchemaKeyref keyref)
			{
				AddRow(ArtItem.KeyRef, "Key Reference", keyref);
			}

			protected override void Visit(XmlSchemaUnique unique)
			{
				AddRow(ArtItem.Unique, "Unique", unique);
			}
		}

		public static string Build(Context context, XmlSchemaElement element)
		{
			if (element.Constraints.Count == 0)
				return string.Empty;

			var sb = new StringBuilder();
			sb.Append(header);

			var rowBuilder = new ConstraintRowBuilder(context, sb);
			rowBuilder.Traverse(element);

			sb.Append(footer);
			return sb.ToString();
		}
	}
}