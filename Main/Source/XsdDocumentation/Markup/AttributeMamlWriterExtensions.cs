using System;
using System.Linq;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
    internal static class AttributeMamlWriterExtensions
    {
        #region AttributeTypeNameWriter

        private sealed class AttributeTypeNameWriter : XmlSchemaSetVisitor
        {
            private Context _context;
            private MamlWriter _writer;

            public AttributeTypeNameWriter(MamlWriter writer, Context context)
            {
                _writer = writer;
                _context = context;
            }

            protected override void Visit(XmlSchemaSimpleType type)
            {
                if (type.QualifiedName.IsEmpty)
                    Traverse(type.Content);
                else
                {
                    var topic = _context.TopicManager.GetTopic(type);
                    if (topic != null)
                    {
                        _writer.WriteHtmlArtItemWithTopicLink(ArtItem.SimpleType, topic);
                    }
                    else
                    {
                        _writer.WriteHtmlArtItemWithText(ArtItem.SimpleType, type.QualifiedName.Name);
                    }
                }
            }

            protected override void Visit(XmlSchemaSimpleTypeList list)
            {
                if (list.BaseItemType.QualifiedName.IsEmpty)
                {
                    _writer.WriteHtmlArtItemWithText(ArtItem.List, "List");
                }
                else
                {
                    var topic = _context.TopicManager.GetTopic(list.BaseItemType);
                    if (topic != null)
                    {
                        _writer.WriteHtmlArtItemWithTopicLink(ArtItem.List, topic);
                    }
                    else
                    {
                        _writer.WriteHtmlArtItemWithText(ArtItem.List, list.BaseItemType.QualifiedName.Name);
                    }
                }
            }

            protected override void Visit(XmlSchemaSimpleTypeRestriction restriction)
            {
                var typeArtItem = ArtItem.Restriction;

                var baseType = _context.SchemaSetManager.SchemaSet.ResolveType(restriction.BaseType, restriction.BaseTypeName);

                if (baseType != null && baseType.QualifiedName.IsEmpty)
                {
                    _writer.WriteHtmlArtItemWithText(typeArtItem, "Restriction");
                }
                else if (baseType == null)
                {
                    _writer.WriteHtmlArtItemWithText(typeArtItem, restriction.BaseTypeName.Name);
                }
                else
                {
                    var topic = _context.TopicManager.GetTopic(baseType);
                    if (topic != null)
                    {
                        _writer.WriteHtmlArtItemWithTopicLink(typeArtItem, topic);
                    }
                    else
                    {
                        _writer.WriteHtmlArtItemWithText(typeArtItem, baseType.QualifiedName.Name);
                    }
                }
            }

            protected override void Visit(XmlSchemaSimpleTypeUnion union)
            {
                foreach (var baseMemberType in union.BaseMemberTypes)
                {
                    if (baseMemberType.QualifiedName.IsEmpty)
                    {
                        _writer.WriteHtmlArtItemWithText(ArtItem.Union, "Union");
                        return;
                    }
                }

                _writer.StartHtmlArtItem(ArtItem.Union);

                var isFirst = true;
                foreach (var baseMemberType in union.BaseMemberTypes)
                {
                    if (isFirst)
                    {
                        _writer.WriteString(", ");
                        isFirst = false;
                    }

                    var topic = _context.TopicManager.GetTopic(baseMemberType);
                    if (topic != null)
                    {
                        _writer.WriteHtmlTopicLink(topic);
                    }
                    else
                    {
                        _writer.WriteString(baseMemberType.QualifiedName.Name);
                    }
                }

                _writer.EndHtmlArtItem();
            }
        }

        #endregion

        public static void WriteAttributeTable(this MamlWriter writer, Context context, AttributeEntries attributeEntries)
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
                writer.WriteHtmlArtItemWithText(ArtItem.AnyAttribute, "Any");
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
                writer.WriteHtmlArtItemWithTopicLink(artItem, topic);
            else
                writer.WriteHtmlArtItemWithText(artItem, attribute.QualifiedName.Name);
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
            var typeNameWriter = new AttributeTypeNameWriter(writer, context);
            typeNameWriter.Traverse(type);
        }
    }
}