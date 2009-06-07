using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal sealed class ChildrenMarkupBuilder
	{
		private const string _header = @"
<table>
	<tableHeader>
		<row>
			<entry>Name</entry>
			<entry>Occurrences</entry>
			<entry>Description</entry>
		</row>
	</tableHeader>";
		private const string _row = @"
	<row>
		<entry>
			${Indent}${NameMarkup}
		</entry>
		<entry>
			${OccurrencesMarkup}
		</entry>
		<entry>
			${DescriptionMarkup}
		</entry>
	</row>";

		private const string _footer = @"
</table>";

		public static string Build(Context context, List<ChildEntry> childEntries)
		{
			if (childEntries == null || childEntries.Count == 0)
				return string.Empty;

			SortAllAndChoiceChildren(childEntries);

			var stringWriter = new StringWriter();
			stringWriter.Write(_header);
			Build(context, stringWriter, childEntries, 0);
			stringWriter.Write(_footer);
			return stringWriter.ToString();
		}

		private static void SortAllAndChoiceChildren(IEnumerable<ChildEntry> childEntries)
		{
			foreach (var childEntry in childEntries)
			{
				SortAllAndChoiceChildren(childEntry.Children);

				if (childEntry.ChildType == ChildType.All ||
				    childEntry.ChildType == ChildType.Choice ||
					childEntry.ChildType == ChildType.Any)
				{
					childEntry.Children.Sort(CompareChildEntries);
				}
			}
		}

		private static int CompareChildEntries(ChildEntry x, ChildEntry y)
		{
			if (x.ChildType == ChildType.Element &&
			    y.ChildType == ChildType.Element ||
				x.ChildType == ChildType.ElementExtension &&
				y.ChildType == ChildType.ElementExtension)
			{
				var xElement = (XmlSchemaElement) x.Particle;
				var yElement = (XmlSchemaElement) y.Particle;
				return xElement.QualifiedName.Name.CompareTo(yElement.QualifiedName.Name);
			}

			if (x.ChildType == y.ChildType)
				return 0;

			if (x.ChildType == ChildType.Any)
				return 1;

			if (y.ChildType == ChildType.Any)
				return -1;

			if (x.ChildType != ChildType.Element)
				return -1;

			if (y.ChildType != ChildType.Element)
				return 1;

			return 0;
		}

		private static void Build(Context context, TextWriter writer, IEnumerable<ChildEntry> childEntries, int level)
		{
			var indent = MarkupHelper.GenerateIndent(level);

			foreach (var childEntry in childEntries)
			{
				var rowMarkupBuilder = new StringBuilder(_row);
				rowMarkupBuilder.Replace("${Indent}", indent);
				rowMarkupBuilder.Replace("${NameMarkup}", GetNameMarkup(childEntry, context.TopicManager));
				rowMarkupBuilder.Replace("${OccurrencesMarkup}", GetOccurrenceMarkup(childEntry));
				rowMarkupBuilder.Replace("${DescriptionMarkup}", GetDescriptionMarkup(childEntry, context));
				writer.Write(rowMarkupBuilder.ToString());

				Build(context, writer, childEntry.Children, level + 1);
			}
		}

		private static string GetNameMarkup(ChildEntry entry, TopicManager topicManager)
		{
			switch (entry.ChildType)
			{
				case ChildType.Element:
				case ChildType.ElementExtension:
					var element = (XmlSchemaElement)entry.Particle;
					var isExtension = (entry.ChildType == ChildType.ElementExtension);
					return GetElementTopicLink(topicManager, element, isExtension);
				case ChildType.Any:
					return MarkupHelper.GetHtmlImageWithText(ArtItem.AnyElement, "Any");
				case ChildType.All:
					return MarkupHelper.GetHtmlImageWithText(ArtItem.All, "All");
				case ChildType.Choice:
					return MarkupHelper.GetHtmlImageWithText(ArtItem.Choice, "Choice");
				case ChildType.Sequence:
					return MarkupHelper.GetHtmlImageWithText(ArtItem.Sequence, "Sequence");
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static string GetElementTopicLink(TopicManager topicManager, XmlSchemaElement element, bool isExtension)
		{
			var artItem = element.RefName.IsEmpty && !isExtension
			              	? ArtItem.Element
			              	: ArtItem.ElementRef;
			var topic = topicManager.GetTopic(element);
			if (topic != null)
				return MarkupHelper.GetHtmlImageWithTopicLink(artItem, topic);
			
			return MarkupHelper.GetHtmlImageWithText(artItem, element.QualifiedName.Name);
		}

		private static string GetOccurrenceMarkup(ChildEntry entry)
		{
			if (entry.MinOccurs == 1 && entry.MaxOccurs == 1)
				return string.Empty;

			if (entry.MaxOccurs == decimal.MaxValue)
				return string.Format("[{0}, *]", entry.MinOccurs);

			return string.Format("[{0}, {1}]", entry.MinOccurs, entry.MaxOccurs);

		}

		private static string GetDescriptionMarkup(ChildEntry entry, Context context)
		{
			return (entry.Particle == null)
			       	? null
			       	: SummaryMarkupBuilder.BuildForObject(context, entry.Particle);
		}
	}
}