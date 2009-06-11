using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class ConstraintsMamlWriterExtensions
	{
		private sealed class ConstraintRowWriter : XmlSchemaSetVisitor
		{
			private MamlWriter _writer;
			private Context _context;

			public ConstraintRowWriter(MamlWriter writer, Context context)
			{
				_writer = writer;
				_context = context;
			}

			protected override void Visit(XmlSchemaKey key)
			{
				_writer.WriteConstraintRow(_context, ArtItem.Key, "Key", key);
			}

			protected override void Visit(XmlSchemaKeyref keyref)
			{
				_writer.WriteConstraintRow(_context, ArtItem.KeyRef, "Key Reference", keyref);
			}

			protected override void Visit(XmlSchemaUnique unique)
			{
				_writer.WriteConstraintRow(_context, ArtItem.Unique, "Unique", unique);
			}
		}

		public static void WriteConstraints(this MamlWriter writer, Context context, XmlSchemaElement element)
		{
			if (element.Constraints.Count == 0)
				return;

			writer.StartTable();
			writer.StartTableHeader();
			writer.StartTableRow();

			writer.StartTableRowEntry();
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Type");
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Description");
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Selector");
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Fields");
			writer.EndTableRowEntry();

			writer.EndTableRow();
			writer.EndTableHeader();

			writer.WriteConstraintRows(context, element);

			writer.EndTable();
		}

		private static void WriteConstraintRows(this MamlWriter writer, Context context, XmlSchemaElement element)
		{
			var rowBuilder = new ConstraintRowWriter(writer, context);
			rowBuilder.Traverse(element);
		}

		private static void WriteConstraintRow(this MamlWriter writer, Context context, ArtItem artItem, string constrainedType, XmlSchemaIdentityConstraint constraint)
		{
			writer.StartTableRow();

			writer.StartTableRowEntry();
			writer.WriteMediaInline(artItem.Id);
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString(constrainedType);
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteSummaryForObject(context, constraint);
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString(constraint.Selector.XPath);
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteConstraintFieldList(constraint.Fields);
			writer.EndTableRowEntry();

			writer.EndTableRow();
		}

		private static void WriteConstraintFieldList(this MamlWriter writer, XmlSchemaObjectCollection fields)
		{
			if (fields.Count == 1)
			{
				var field = (XmlSchemaXPath)fields[0];
				writer.WriteString(field.XPath);
				return;
			}

			writer.StartList(ListClass.Ordered);

			foreach (XmlSchemaXPath field in fields)
			{
				writer.StartListItem();
				writer.StartParagraph();
				writer.WriteString(field.XPath);
				writer.EndParagraph();
				writer.EndListItem();
			}

			writer.EndList();
		}
	}
}