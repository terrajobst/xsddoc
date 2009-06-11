using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal sealed class SimpleTypeNameMamlWriter : XmlSchemaSetVisitor
	{
		private Context _context;
		private MamlWriter _writer;

		public SimpleTypeNameMamlWriter(MamlWriter writer, Context context)
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
					_writer.WriteHtmlImageWithLink(ArtItem.SimpleType.Id, topic.Id, topic.LinkTitle);
				}
				else
				{
					_writer.WriteHtmlImageWithText(ArtItem.SimpleType.Id, type.QualifiedName.Name);
				}
			}
		}

		protected override void Visit(XmlSchemaSimpleTypeList list)
		{
			if (list.BaseItemType.QualifiedName.IsEmpty)
			{
				_writer.WriteHtmlImageWithText(ArtItem.List.Id, "List");
			}
			else
			{
				var topic = _context.TopicManager.GetTopic(list.BaseItemType);
				if (topic != null)
				{
					_writer.WriteHtmlImageWithLink(ArtItem.List.Id, topic.Id, topic.LinkTitle);
				}
				else
				{
					_writer.WriteHtmlImageWithText(ArtItem.List.Id, list.BaseItemType.QualifiedName.Name);
				}
			}
		}

		protected override void Visit(XmlSchemaSimpleTypeRestriction restriction)
		{
			var typeMediaLink = ArtItem.Restriction;

			var baseType = _context.SchemaSetManager.SchemaSet.ResolveType(restriction.BaseType, restriction.BaseTypeName);

			if (baseType != null && baseType.QualifiedName.IsEmpty)
			{
				_writer.WriteHtmlImageWithText(typeMediaLink.Id, "Restriction");
			}
			else if (baseType == null)
			{
				_writer.WriteHtmlImageWithText(typeMediaLink.Id, restriction.BaseTypeName.Name);
			}
			else
			{
				var topic = _context.TopicManager.GetTopic(baseType);
				if (topic != null)
				{
					_writer.WriteHtmlImageWithLink(typeMediaLink.Id, topic.Id, topic.LinkTitle);
				}
				else
				{
					_writer.WriteHtmlImageWithText(typeMediaLink.Id, baseType.QualifiedName.Name);
				}
			}
		}

		protected override void Visit(XmlSchemaSimpleTypeUnion union)
		{
			foreach (var baseMemberType in union.BaseMemberTypes)
			{
				if (baseMemberType.QualifiedName.IsEmpty)
				{
					_writer.WriteHtmlImageWithText(ArtItem.Union.Id, "Union");
					return;
				}
			}

			_writer.StartHtmlImage(ArtItem.Union.Id);

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
					_writer.WriteHtmlLink(topic.Id, topic.LinkTitle);
				}
				else
				{
					_writer.WriteString(baseMemberType.QualifiedName.Name);
				}
			}

			_writer.EndHtmlImage();
		}
	}
}