using System;
using System.IO;
using System.Linq;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal sealed class AttributeMarkupBuilder
	{
		private const string _header = @"
<table>
	<tableHeader>
		<row>
			<entry>Name</entry>
			<entry>Type</entry>
			<entry>Required</entry>
			<entry>Description</entry>
		</row>
	</tableHeader>";
		private const string _row = @"
	<row>
		<entry>
			${Indent}${NameMarkup}
		</entry>
		<entry>
			${TypeMarkup}
		</entry>
		<entry>
			${RequiredMarkup}
		</entry>
		<entry>
			${DescriptionMarkup}
		</entry>
	</row>";

		private const string _footer = @"
</table>";

		public static string Build(Context context, AttributeEntries attributeEntries)
		{
			if (attributeEntries.Attributes.Count == 0 && attributeEntries.AnyAttribute == null)
				return string.Empty;

			var stringWriter = new StringWriter();
			stringWriter.Write(_header);

			var sortedAttributes = from a in attributeEntries.Attributes
								   orderby a.QualifiedName.Name
			                       select a;
			foreach (var attribute in sortedAttributes)
			{
				var markup = _row;
				markup = markup.Replace("${Indent}", string.Empty);
				markup = markup.Replace("${NameMarkup}", GetAttributeTopicLink(context.TopicManager, attribute, false));
				markup = markup.Replace("${TypeMarkup}", GetTypeMarkup(context, attribute.AttributeSchemaType));
				markup = markup.Replace("${RequiredMarkup}", GetRequiredMarkup(attribute.Use));
				markup = markup.Replace("${DescriptionMarkup}", SummaryMarkupBuilder.BuildForObject(context, attribute));
				stringWriter.Write(markup);
			}

			if (attributeEntries.AnyAttribute != null)
			{
				var markup = _row;
				markup = markup.Replace("${Indent}", string.Empty);
				markup = markup.Replace("${NameMarkup}", MarkupHelper.GetHtmlImageWithText(ArtItem.AnyAttribute, "Any"));
				markup = markup.Replace("${TypeMarkup}", string.Empty);
				markup = markup.Replace("${RequiredMarkup}", string.Empty);
				markup = markup.Replace("${DescriptionMarkup}", SummaryMarkupBuilder.BuildForObject(context, attributeEntries.AnyAttribute));
				stringWriter.Write(markup);
			}

			var sortedExtensionAttributes = from a in attributeEntries.ExtensionAttributes
			                                orderby a.QualifiedName.Name
			                                select a;
			foreach (var attribute in sortedExtensionAttributes)
			{
				var markup = _row;
				markup = markup.Replace("${Indent}", MarkupHelper.GenerateIndent(1));
				markup = markup.Replace("${NameMarkup}", GetAttributeTopicLink(context.TopicManager, attribute, true));
				markup = markup.Replace("${TypeMarkup}", GetTypeMarkup(context, attribute.AttributeSchemaType));
				markup = markup.Replace("${RequiredMarkup}", GetRequiredMarkup(attribute.Use));
				markup = markup.Replace("${DescriptionMarkup}", SummaryMarkupBuilder.BuildForObject(context, attribute));
				stringWriter.Write(markup);
			}

			stringWriter.Write(_footer);
			return stringWriter.ToString();
		}

		private static string GetAttributeTopicLink(TopicManager topicManager, XmlSchemaAttribute attribute, bool isExtension)
		{
			var artItem = attribute.RefName.IsEmpty && !isExtension
			              	? ArtItem.Attribute
			              	: ArtItem.AttributeRef;

			var topic = topicManager.GetTopic(attribute);
			if (topic != null)
				return MarkupHelper.GetHtmlImageWithTopicLink(artItem, topic);

			return MarkupHelper.GetHtmlImageWithText(artItem, attribute.QualifiedName.Name);
		}

		private static string GetRequiredMarkup(XmlSchemaUse use)
		{
			switch (use)
			{
				case XmlSchemaUse.None:
				case XmlSchemaUse.Optional:
					return string.Empty;
				case XmlSchemaUse.Required:
					return "Yes";
				default:
					throw new ArgumentOutOfRangeException("use");
			}
		}

		private static string GetTypeMarkup(Context context, XmlSchemaSimpleType type)
		{
			var simpleTypeNameBuilder = new SimpleTypeNameMarkupBuilder(context);
			simpleTypeNameBuilder.Traverse(type);
			return simpleTypeNameBuilder.GetMarkup();
		}
	}
}