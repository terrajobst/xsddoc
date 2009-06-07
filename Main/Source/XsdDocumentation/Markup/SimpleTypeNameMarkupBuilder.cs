using System;
using System.IO;
using System.Text;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal sealed class SimpleTypeNameMarkupBuilder : XmlSchemaSetVisitor
	{
		private Context _context;
		private StringWriter _markupWriter = new StringWriter();

		public SimpleTypeNameMarkupBuilder(Context context)
		{
			_context = context;
		}

		public string GetMarkup()
		{
			return _markupWriter.ToString();
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
					var nameMarkup = MarkupHelper.GetHtmlImageWithTopicLink(ArtItem.SimpleType, topic);
					_markupWriter.Write(nameMarkup);
				}
				else
				{
					var nameMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.SimpleType, type.QualifiedName.Name);
					_markupWriter.Write(nameMarkup);
				}
			}
		}

		protected override void Visit(XmlSchemaSimpleTypeList list)
		{
			if (list.BaseItemType.QualifiedName.IsEmpty)
			{
				var nameMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.List, "List");
				_markupWriter.Write(nameMarkup);
			}
			else
			{
				var topic = _context.TopicManager.GetTopic(list.BaseItemType);
				if (topic != null)
				{
					var topicLink = MarkupHelper.GetHtmlTopicLink(topic);
					var nameMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.List, topicLink);
					_markupWriter.Write(nameMarkup);
				}
				else
				{
					var nameMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.List, list.BaseItemType.QualifiedName.Name);
					_markupWriter.Write(nameMarkup);
				}
			}
		}

		protected override void Visit(XmlSchemaSimpleTypeRestriction restriction)
		{
			var typeMediaLink = ArtItem.Restriction;

			var baseType = _context.SchemaSetManager.SchemaSet.ResolveType(restriction.BaseType, restriction.BaseTypeName);

			if (baseType != null && baseType.QualifiedName.IsEmpty)
			{
				var nameMarkup = MarkupHelper.GetHtmlImageWithText(typeMediaLink, "Restriction");
				_markupWriter.Write(nameMarkup);
			}
			else if (baseType == null)
			{
				var nameMarkup = MarkupHelper.GetHtmlImageWithText(typeMediaLink, restriction.BaseTypeName.Name);
				_markupWriter.Write(nameMarkup);
			}
			else
			{
				var topic = _context.TopicManager.GetTopic(baseType);
				if (topic != null)
				{
					var nameMarkup = MarkupHelper.GetHtmlImageWithTopicLink(typeMediaLink, topic);
					_markupWriter.Write(nameMarkup);
				}
				else
				{
					var nameMarkup = MarkupHelper.GetHtmlImageWithText(typeMediaLink, baseType.QualifiedName.Name);
					_markupWriter.Write(nameMarkup);
				}
			}
		}

		protected override void Visit(XmlSchemaSimpleTypeUnion union)
		{
			foreach (var baseMemberType in union.BaseMemberTypes)
			{
				if (baseMemberType.QualifiedName.IsEmpty)
				{
					var nameMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Union, "Union");
					_markupWriter.Write(nameMarkup);
					return;
				}
			}

			var sb = new StringBuilder();

			foreach (var baseMemberType in union.BaseMemberTypes)
			{
				if (sb.Length > 0)
					sb.Append(", ");

				var topic = _context.TopicManager.GetTopic(baseMemberType);
				if (topic != null)
				{
					var topicLink = MarkupHelper.GetHtmlTopicLink(topic);
					sb.Append(topicLink);
				}
				else
				{
					sb.Append(baseMemberType.QualifiedName.Name);
				}
			}
			
			var fullMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Union, sb.ToString());
			_markupWriter.Write(fullMarkup);
		}
	}
}