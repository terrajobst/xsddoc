using System;
using System.Collections.Generic;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class SectionMamlWriterExtensions
	{
		#region Introduction

		public static void WriteIntroductionForSchemaSet(this MamlWriter writer, Context context)
		{
			var documentationInfo = context.DocumentationManager.GetSchemaSetDocumentationInfo();

			writer.StartIntroduction();
			writer.WriteSummary(documentationInfo);
			writer.EndIntroduction();
		}

		public static void WriteIntroductionForNamespace(this MamlWriter writer, Context context, string targetNamespace)
		{
			var documentationInfo = context.DocumentationManager.GetNamespaceDocumentationInfo(targetNamespace);

			writer.StartIntroduction();
			writer.WriteSummary(documentationInfo);
			writer.WriteObsoleteInfo(context, targetNamespace);
			writer.EndIntroduction();
		}

		public static void WriteIntroductionForOverview(this MamlWriter writer, Context context, string namespaceUri)
		{
			writer.StartIntroduction();
			writer.StartParagraph();
			writer.WriteString("The ");
			writer.WriteNamespaceLink(context, namespaceUri);
			writer.WriteString(" namespace exposes the following members.");
			writer.EndParagraph();
			writer.EndIntroduction();
		}

		public static void WriteIntroductionForSchema(this MamlWriter writer, Context context, XmlSchema schema)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(schema);

			writer.StartIntroduction();
			writer.WriteSummary(documentationInfo);
			writer.WriteObsoleteInfo(context, schema);
			writer.WriteNamespaceInfo(context, schema.TargetNamespace);
			writer.EndIntroduction();
		}

		public static void WriteIntroductionForObject(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);

			writer.StartIntroduction();
			writer.WriteSummary(documentationInfo);
			writer.WriteObsoleteInfo(context, obj);
			writer.WriteNamespaceAndSchemaInfo(context, obj);
			writer.EndIntroduction();
		}

		#endregion

		#region Type

		public static void WriteTypeSection(this MamlWriter writer, Context context, XmlSchemaElement element)
		{
			if (element.ElementSchemaType is XmlSchemaSimpleType)
			{
				writer.StartSection("Type", "type");
				writer.WriteTypeName(context.TopicManager, element.ElementSchemaType);
				writer.EndSection();
			}
		}

		#endregion

		#region Base Type

		public static void WriteBaseTypeSection(this MamlWriter writer, Context context, XmlSchemaComplexType complexType)
		{
			writer.StartSection("Base Type", "baseType");
			writer.WriteTypeName(context.TopicManager, complexType.BaseXmlSchemaType);
			writer.EndSection();
		}

		#endregion

		#region Parents

		public static void WriteParentsSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> parents)
		{
			writer.StartSection("Parents", "parents");
			writer.WriteList(context, parents);
			writer.EndSection();
		}

		#endregion

		#region Usages

		public static void WriteUsagesSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> usages)
		{
			writer.StartSection("Usages", "usages");
			writer.WriteList(context, usages);
			writer.EndSection();
		}

		#endregion

		#region Children

		public static void WriteChildrenSection(this MamlWriter writer, Context context, List<ChildEntry> children)
		{
			writer.StartSection("Children", "children");
			writer.WriteChildrenTable(context, children);
			writer.EndSection();
		}

		#endregion

		#region Attributes

		public static void WriteAttributesSection(this MamlWriter writer, Context context, AttributeEntries attributeEntries)
		{
			writer.StartSection("Attributes", "attributes");
			writer.WriteAttributeTable(context, attributeEntries);
			writer.EndSection();
		}

		#endregion

		#region Constraints

		public static void WriteConstraintsSection(this MamlWriter writer, Context context, XmlSchemaObjectCollection constraints)
		{
			writer.StartSection("Constraints", "constraints");
			writer.WriteConstraintTable(context, constraints);
			writer.EndSection();
		}

		#endregion

		#region Content Type

		public static void WriteContentTypeSection(this MamlWriter writer, Context context, SimpleTypeStructureNode rootNode)
		{
			writer.StartSection("Content Type", "contentType");
			writer.WriteSimpleTypeStrucure(context, rootNode);
			writer.EndSection();
		}

		#endregion

		#region Remarks

		public static void WriteRemarksSectionForSchemaSet(this MamlWriter writer, Context context)
		{
			var documentationInfo = context.DocumentationManager.GetSchemaSetDocumentationInfo();
			writer.WriteRemarksSection(documentationInfo);
		}

		public static void WriteRemarksSectionForNamespace(this MamlWriter writer, Context context, string targetNamespace)
		{
			var documentationInfo = context.DocumentationManager.GetNamespaceDocumentationInfo(targetNamespace);
			writer.WriteRemarksSection(documentationInfo);
		}

		public static void WriteRemarksSectionForObject(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var documentationInfo = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			writer.WriteRemarksSection(documentationInfo);
		}

		private static void WriteRemarksSection(this MamlWriter writer, DocumentationInfo documentationInfo)
		{
			if (documentationInfo == null || documentationInfo.RemarksNode == null)
				return;

			var contentNode = documentationInfo.RemarksNode.ChildNodes[0];
			writer.StartSection("Remarks", "remarks");
			writer.WriteRawContent(contentNode);
			writer.EndSection();
		}

		#endregion

		#region Syntax

		public static void WriteSyntaxSection(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			var sourceCodeAbridged = context.SourceCodeManager.GetSourceCodeAbridged(obj);

			writer.StartSection("Syntax", "syntax");
			writer.WriteCode(sourceCodeAbridged, "xml");
			writer.EndSection();
		}

		#endregion

		#region Related Topics

		public static void WriteRelatedTopics(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			writer.StartRelatedTopics();
			writer.WriteLinksForObject(obj, context);
			writer.EndRelatedTopics();
		}

		#endregion

		#region Jump Table Sections

		public static void WriteNamespacesSection(this MamlWriter writer, Context context, IEnumerable<string> namespaces)
		{
			writer.WriteJumpTableSection(context, namespaces, "Namespaces", "namespaces");
		}

		public static void WriteRootSchemasSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> rootSchemas)
		{
			writer.WriteJumpTableSection(context, rootSchemas, "Root Schemas", "rootSchemas");
		}

		public static void WriteRootElementsSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> rootElements)
		{
			writer.WriteJumpTableSection(context, rootElements, "Root Elements", "rootElements");
		}

		public static void WriteSchemasSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> schemas)
		{
			writer.WriteJumpTableSection(context, schemas, "Schemas", "schemas");
		}

		public static void WriteElementsSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> elements)
		{
			writer.WriteJumpTableSection(context, elements, "Elements", "elements");
		}

		public static void WriteAttributesSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> attributes)
		{
			writer.WriteJumpTableSection(context, attributes, "Attributes", "attributes");
		}

		public static void WriteGroupsSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> groups)
		{
			writer.WriteJumpTableSection(context, groups, "Groups", "groups");
		}

		public static void WriteAttributeGroupsSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> groups)
		{
			writer.WriteJumpTableSection(context, groups, "Attribute Groups", "attributeGroups");
		}

		public static void WriteSimpleTypesSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> types)
		{
			writer.WriteJumpTableSection(context, types, "Simple Types", "simpleTypes");
		}

		public static void WriteComplexTypesSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> types)
		{
			writer.WriteJumpTableSection(context, types, "Complex Types", "complexTypes");
		}

		private static void WriteJumpTableSection(this MamlWriter writer, Context context, IEnumerable<string> namespaces, string title, string address)
		{
			var listItems = ListItemBuilder.Build(context, namespaces);
			writer.WriteJumpTableSection(listItems, title, address);
		}

		private static void WriteJumpTableSection(this MamlWriter writer, Context context, IEnumerable<XmlSchemaObject> rows, string title, string address)
		{
			var listItems = ListItemBuilder.Build(context, rows);
			writer.WriteJumpTableSection(listItems, title, address);
		}

		private static void WriteJumpTableSection(this MamlWriter writer, ICollection<ListItem> listItems, string title, string address)
		{
			if (listItems.Count == 0)
				return;

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
				writer.WriteArtItemInline(listItem.ArtItem);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteTopicLink(listItem.Topic);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteRaw(listItem.SummaryMarkup);
				writer.EndTableRowEntry();

				writer.EndTableRow();
			}

			writer.EndTable();

			writer.EndSection();
		}

		#endregion
	}
}