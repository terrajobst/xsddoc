using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class TypeNameMarkupBuilder
	{
		private static string GetImageWithTopicLink(TopicManager topicManager, ArtItem artItem, XmlSchemaType type)
		{
			var topic = topicManager.GetTopic(type);
			if (topic != null)
				return MarkupHelper.GetHtmlImageWithTopicLink(artItem, topic);

			return MarkupHelper.GetHtmlImageWithText(artItem, type.QualifiedName.Name);
		}

		public static string Build(TopicManager topicManager, XmlSchemaType schemaType)
		{
			XmlSchemaSimpleType simpleType;
			XmlSchemaComplexType complexType;

			if (Casting.TryCast(schemaType, out simpleType))
			{
				if (simpleType.QualifiedName.IsEmpty)
					return string.Empty;

				return GetImageWithTopicLink(topicManager, ArtItem.SimpleType, simpleType);
			}

			if (Casting.TryCast(schemaType, out complexType))
			{
				if (!complexType.QualifiedName.IsEmpty)
					return GetImageWithTopicLink(topicManager, ArtItem.ComplexType, complexType);

				var baseType = complexType.BaseXmlSchemaType;
				if (baseType == null || complexType.ContentModel == null)
					return string.Empty;

				var isExtension = complexType.ContentModel.Content is XmlSchemaComplexContentExtension ||
				                  complexType.ContentModel.Content is XmlSchemaSimpleContentExtension;

				var artItem = isExtension
				              	? ArtItem.Extension
				              	: ArtItem.Restriction;

				return GetImageWithTopicLink(topicManager, artItem, baseType);
			}

			throw ExceptionBuilder.UnexpectedSchemaObjectType(schemaType);
		}
	}
}