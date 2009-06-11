using System;
using System.Collections.Generic;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class ChildrenMamlWriterExtensions
	{
		public static void WriteChildren(this MamlWriter writer, Context context, List<ChildEntry> childEntries)
		{
			if (childEntries == null || childEntries.Count == 0)
				return;

			SortAllAndChoiceChildren(childEntries);

			writer.StartTable();
			writer.StartTableHeader();
			writer.StartTableRow();

			writer.StartTableRowEntry();
			writer.WriteString("Name");
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Occurrences");
			writer.EndTableRowEntry();

			writer.StartTableRowEntry();
			writer.WriteString("Description");
			writer.EndTableRowEntry();

			writer.EndTableRow();
			writer.EndTableHeader();

			writer.WriteChildren(context, childEntries, 0);

			writer.EndTable();
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
				var xElement = (XmlSchemaElement)x.Particle;
				var yElement = (XmlSchemaElement)y.Particle;
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

		private static void WriteChildren(this MamlWriter writer, Context context, IEnumerable<ChildEntry> childEntries, int level)
		{
			foreach (var childEntry in childEntries)
			{
				writer.StartTableRow();

				writer.StartTableRowEntry();
				writer.WriteHtmlIndent(level);
				writer.WriteName(childEntry, context.TopicManager);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteOccurrence(childEntry);
				writer.EndTableRowEntry();

				writer.StartTableRowEntry();
				writer.WriteDescription(childEntry, context);
				writer.EndTableRowEntry();

				writer.EndTableRow();

				writer.WriteChildren(context, childEntry.Children, level + 1);
			}
		}

		private static void WriteName(this MamlWriter writer, ChildEntry entry, TopicManager topicManager)
		{
			switch (entry.ChildType)
			{
				case ChildType.Element:
				case ChildType.ElementExtension:
					var element = (XmlSchemaElement)entry.Particle;
					var isExtension = (entry.ChildType == ChildType.ElementExtension);
					writer.WriteElementLink(topicManager, element, isExtension);
					break;
				case ChildType.Any:
					writer.WriteHtmlImageWithText(ArtItem.AnyElement.Id, "Any");
					break;
				case ChildType.All:
					writer.WriteHtmlImageWithText(ArtItem.All.Id, "All");
					break;
				case ChildType.Choice:
					writer.WriteHtmlImageWithText(ArtItem.Choice.Id, "Choice");
					break;
				case ChildType.Sequence:
					writer.WriteHtmlImageWithText(ArtItem.Sequence.Id, "Sequence");
					break;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(entry.ChildType);
			}
		}

		private static void WriteElementLink(this MamlWriter writer, TopicManager topicManager, XmlSchemaElement element, bool isExtension)
		{
			var artItem = element.RefName.IsEmpty && !isExtension
							? ArtItem.Element
							: ArtItem.ElementRef;
			var topic = topicManager.GetTopic(element);
			if (topic != null)
				writer.WriteHtmlImageWithLink(artItem.Id, topic.Id, topic.LinkTitle);
			else
				writer.WriteHtmlImageWithText(artItem.Id, element.QualifiedName.Name);
		}

		private static void WriteOccurrence(this MamlWriter writer, ChildEntry entry)
		{
			if (entry.MinOccurs == 1 && entry.MaxOccurs == 1)
				return;

			if (entry.MaxOccurs == decimal.MaxValue)
				writer.WriteString("[{0}, *]", entry.MinOccurs);
			else
				writer.WriteString("[{0}, {1}]", entry.MinOccurs, entry.MaxOccurs);
		}

		private static void WriteDescription(this MamlWriter writer, ChildEntry entry, Context context)
		{
			if (entry.Particle != null)
				writer.WriteSummaryForObject(context, entry.Particle);
		}
	}
}