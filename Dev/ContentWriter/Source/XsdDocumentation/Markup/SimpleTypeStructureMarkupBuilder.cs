using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal sealed class SimpleTypeStructureMarkupBuilder
	{
		private const string _header = @"
<table>
	<tableHeader>
		<row>
			<entry>Item</entry>
			<entry>Facet Value</entry>
			<entry>Description</entry>
		</row>
	</tableHeader>";
		private const string _row = @"
	<row>
		<entry>
			${Indent}${ItemMarkup}
		</entry>
		<entry>
			${FacetValueMarkup}
		</entry>
		<entry>
			${DescriptionMarkup}
		</entry>
	</row>";

		private const string _footer = @"
</table>";

		public static string Build(Context context, SimpleTypeStructureNode root)
		{
			if (root == null || root.Children.Count == 0)
				return string.Empty;

			if (root.Children.Count == 1)
			{
				var node = root.Children[0];
				var simpleMarkup = GetSimpleMarkup(context.TopicManager, node);
				if (simpleMarkup != null)
					return simpleMarkup;
			}

			var stringWriter = new StringWriter();
			stringWriter.Write(_header);
			Build(context, stringWriter, root.Children, 0);
			stringWriter.Write(_footer);
			return stringWriter.ToString();
		}

		private static void Build(Context context, TextWriter writer, IEnumerable<SimpleTypeStructureNode> children, int level)
		{
			var indent = MarkupHelper.GenerateIndent(level);

			foreach (var childEntry in children)
			{
				if (childEntry.NodeType == SimpleTypeStructureNodeType.NamedType)
					continue;

				var itemMarkup = GetSimpleMarkup(context.TopicManager, childEntry);
				var isSimpleMarkup = (itemMarkup != null);

				string valueMarkup;
				if (!isSimpleMarkup)
					GetComplexMarkup(context.TopicManager, childEntry, out itemMarkup, out valueMarkup);
				else
					valueMarkup = null;

				var rowMarkupBuilder = new StringBuilder(_row);
				rowMarkupBuilder.Replace("${Indent}", indent);
				rowMarkupBuilder.Replace("${ItemMarkup}", itemMarkup);
				rowMarkupBuilder.Replace("${FacetValueMarkup}", valueMarkup);
				rowMarkupBuilder.Replace("${DescriptionMarkup}", SummaryMarkupBuilder.BuildForObject(context, childEntry.Node));
				writer.Write(rowMarkupBuilder.ToString());

				if (!isSimpleMarkup)
					Build(context, writer, childEntry.Children, level + 1);
			}
		}

		private static string GetSimpleMarkup(TopicManager topicManager, SimpleTypeStructureNode node)
		{
			switch (node.NodeType)
			{
				case SimpleTypeStructureNodeType.Any:
					return "Any";
				case SimpleTypeStructureNodeType.Mixed:
					return "Mixed";
				case SimpleTypeStructureNodeType.NamedType:
					return TypeNameMarkupBuilder.Build(topicManager, (XmlSchemaType) node.Node);
				case SimpleTypeStructureNodeType.List:
				case SimpleTypeStructureNodeType.Union:
					// Will be handled below.
					break;
				default:
					return null;
			}

			foreach (var child in node.Children)
			{
				if (child.NodeType == SimpleTypeStructureNodeType.Restriction ||
					child.NodeType == SimpleTypeStructureNodeType.List ||
					child.NodeType == SimpleTypeStructureNodeType.Union)
					return null;
			}

			var nameMarkup = GetTypeNamesMarkup(topicManager, node.Children);
			switch (node.NodeType)
			{
				case SimpleTypeStructureNodeType.List:
					return MarkupHelper.GetHtmlImageWithText(ArtItem.List, nameMarkup);
				case SimpleTypeStructureNodeType.Union:
					return MarkupHelper.GetHtmlImageWithText(ArtItem.Union, nameMarkup);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static string GetTypeNamesMarkup(TopicManager topicManager, IEnumerable<SimpleTypeStructureNode> children)
		{
			var sb = new StringBuilder();
			foreach (var node in children)
			{
				if (node.NodeType == SimpleTypeStructureNodeType.NamedType)
				{
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append(GetTypeTopicLink(topicManager, (XmlSchemaSimpleType) node.Node));
				}
			}
			return sb.ToString();
		}

		private static void GetComplexMarkup(TopicManager topicManager, SimpleTypeStructureNode node, out string itemMarkup, out string valueMarkup)
		{
			string typeNamesMarkup;
			switch (node.NodeType)
			{
				case SimpleTypeStructureNodeType.Restriction:
					valueMarkup = null;
					typeNamesMarkup = GetTypeNamesMarkup(topicManager, node.Children);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Restriction, string.IsNullOrEmpty(typeNamesMarkup) ? "Restriction" : typeNamesMarkup);
					break;
				case SimpleTypeStructureNodeType.List:
					valueMarkup = null;
					typeNamesMarkup = GetTypeNamesMarkup(topicManager, node.Children);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.List, string.IsNullOrEmpty(typeNamesMarkup) ? "List" : typeNamesMarkup);
					break;
				case SimpleTypeStructureNodeType.Union:
					valueMarkup = null;
					typeNamesMarkup = GetTypeNamesMarkup(topicManager, node.Children);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Union, string.IsNullOrEmpty(typeNamesMarkup) ? "Union" : typeNamesMarkup);
					break;
				case SimpleTypeStructureNodeType.FacetEnumeration:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Enumeration");
					break;
				case SimpleTypeStructureNodeType.FacetMaxExclusive:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Max Exclusive");
					break;
				case SimpleTypeStructureNodeType.FacetMaxInclusive:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Max Inclusive");
					break;
				case SimpleTypeStructureNodeType.FacetMinExclusive:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Min Exclusive");
					break;
				case SimpleTypeStructureNodeType.FacetMinInclusive:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Min Inclusive");
					break;
				case SimpleTypeStructureNodeType.FacetFractionDigits:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Fraction Digits");
					break;
				case SimpleTypeStructureNodeType.FacetLength:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Length");
					break;
				case SimpleTypeStructureNodeType.FacetMaxLength:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Max Length");
					break;
				case SimpleTypeStructureNodeType.FacetMinLength:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Min Length");
					break;
				case SimpleTypeStructureNodeType.FacetTotalDigits:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Total Digits");
					break;
				case SimpleTypeStructureNodeType.FacetPattern:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "Pattern");
					break;
				case SimpleTypeStructureNodeType.FacetWhiteSpace:
					valueMarkup = GetFacetValueMarkup(node);
					itemMarkup = MarkupHelper.GetHtmlImageWithText(ArtItem.Facet, "White Space");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static string GetFacetValueMarkup(SimpleTypeStructureNode node)
		{
			return MarkupHelper.XmlEncode(((XmlSchemaFacet) node.Node).Value);
		}

		private static string GetTypeTopicLink(TopicManager topicManager, XmlSchemaType type)
		{
			var topic = topicManager.GetTopic(type);
			return (topic != null)
			       	? MarkupHelper.GetHtmlTopicLink(topic)
			       	: type.QualifiedName.Name;
		}
	}
}