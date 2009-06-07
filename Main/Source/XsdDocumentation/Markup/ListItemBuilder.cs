using System;
using System.Collections.Generic;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class ListItemBuilder
	{
		public static List<ListItem> Build(Context context, IEnumerable<XmlSchemaObject> schemaObjects)
		{
			var listItems = new List<ListItem>();
			foreach (var schemaObject in schemaObjects)
			{
				XmlSchema schema;
				XmlSchemaAttribute attribute;
				XmlSchemaElement element;
				XmlSchemaGroup group;
				XmlSchemaAttributeGroup attributeGroup;
				XmlSchemaSimpleType simpleType;
				XmlSchemaComplexType complexType;

				var topic = context.TopicManager.GetTopic(schemaObject);
				var summaryMarkup = SummaryMarkupBuilder.BuildForObject(context, schemaObject);

				if (Casting.TryCast(schemaObject, out schema))
					AddItem(listItems, ArtItem.Schema, topic, summaryMarkup);
				else if (Casting.TryCast(schemaObject, out attribute))
					AddItem(listItems, ArtItem.Attribute, topic, summaryMarkup);
				else if (Casting.TryCast(schemaObject, out element))
					AddItem(listItems, ArtItem.Element, topic, summaryMarkup);
				else if (Casting.TryCast(schemaObject, out group))
					AddItem(listItems, ArtItem.Group, topic, summaryMarkup);
				else if (Casting.TryCast(schemaObject, out attributeGroup))
					AddItem(listItems, ArtItem.AttributeGroup, topic, summaryMarkup);
				else if (Casting.TryCast(schemaObject, out simpleType))
					AddItem(listItems, ArtItem.SimpleType, topic, summaryMarkup);
				else if (Casting.TryCast(schemaObject, out complexType))
					AddItem(listItems, ArtItem.ComplexType, topic, summaryMarkup);
				else
					throw ExceptionBuilder.UnexpectedSchemaObjectType(schemaObject);
			}

			listItems.Sort((x,y) => x.Topic.LinkTitle.CompareTo(y.Topic.LinkTitle));
			return listItems;
		}

		public static List<ListItem> Build(Context context, IEnumerable<string> namespaces)
		{
			var listItems = new List<ListItem>();

			foreach (var ns in namespaces)
			{
				var topic = context.TopicManager.GetNamespaceTopic(ns);
				var summaryMarkup = SummaryMarkupBuilder.BuildForNamespace(context, ns);

				AddItem(listItems, ArtItem.Namespace, topic, summaryMarkup);
			}

			listItems.Sort((x, y) => x.Topic.LinkTitle.CompareTo(y.Topic.LinkTitle));
			return listItems;
		}

		private static void AddItem(ICollection<ListItem> items, ArtItem artItem, Topic topic, string summaryMarkup)
		{
			var listItem = new ListItem
			               {
			               	ArtItem = artItem,
			               	Topic = topic,
							SummaryMarkup = summaryMarkup
			               };
			items.Add(listItem);
		}
	}
}