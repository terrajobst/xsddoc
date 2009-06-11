using System;
using System.Linq;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class AttributeMamlWriterExtensions
	{
		public static void WriteAttributes(this MamlWriter writer, Context context, AttributeEntries attributeEntries)
		{
			if (attributeEntries.Attributes.Count == 0 && attributeEntries.AnyAttribute == null)
				return;

			writer.StartTable();
			writer.StartTableHeader();
			writer.StartTableRow();

			writer.StartTableRowEntry();
			writer.WriteString("Name");
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Type");
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Required");
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Description");
			writer.EndTableRowEntry();

			writer.EndTableRow();
			writer.EndTableHeader();

			var sortedAttributes = from a in attributeEntries.Attributes
								   orderby a.QualifiedName.Name
								   select a;

			foreach (var attribute in sortedAttributes)
			{
				writer.StartTableRow();

				writer.StartTableRowEntry();
				writer.WriteAttributeTopicLink(context.TopicManager, attribute, false);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteType(context, attribute.AttributeSchemaType);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteRequiredText(attribute.Use);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteSummaryForObject(context, attribute);
				writer.EndTableRowEntry();

				writer.EndTableRow();
			}

			if (attributeEntries.AnyAttribute != null)
			{
				writer.StartTableRow();

				writer.StartTableRowEntry();
				writer.WriteHtmlImageWithText(ArtItem.AnyAttribute.Id, "Any");
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteSummaryForObject(context, attributeEntries.AnyAttribute);
				writer.EndTableRowEntry();

				writer.EndTableRow();
			}

			var sortedExtensionAttributes = from a in attributeEntries.ExtensionAttributes
											orderby a.QualifiedName.Name
											select a;
			foreach (var attribute in sortedExtensionAttributes)
			{
				writer.StartTableRow();

				writer.StartTableRowEntry();
				writer.WriteHtmlIndent(1);
				writer.WriteAttributeTopicLink(context.TopicManager, attribute, true);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteType(context, attribute.AttributeSchemaType);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteRequiredText(attribute.Use);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteSummaryForObject(context, attribute);
				writer.EndTableRowEntry();

				writer.EndTableRow();
			}

			writer.EndTable();
		}

		private static void WriteAttributeTopicLink(this MamlWriter writer, TopicManager topicManager, XmlSchemaAttribute attribute, bool isExtension)
		{
			var artItem = attribute.RefName.IsEmpty && !isExtension
							? ArtItem.Attribute
							: ArtItem.AttributeRef;

			var topic = topicManager.GetTopic(attribute);
			if (topic != null)
				writer.WriteHtmlImageWithLink(artItem.Id, topic.Id, topic.LinkTitle);
			else
				writer.WriteHtmlImageWithText(artItem.Id, attribute.QualifiedName.Name);
		}

		private static void WriteRequiredText(this MamlWriter writer, XmlSchemaUse use)
		{
			switch (use)
			{
				case XmlSchemaUse.None:
				case XmlSchemaUse.Optional:
					break;
				case XmlSchemaUse.Required:
					writer.WriteString("Yes");
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(use);
			}
		}

		private static void WriteType(this MamlWriter writer, Context context, XmlSchemaSimpleType type)
		{
			var typeNameWriter = new SimpleTypeNameMamlWriter(writer, context);
			typeNameWriter.Traverse(type);
		}
	}
}