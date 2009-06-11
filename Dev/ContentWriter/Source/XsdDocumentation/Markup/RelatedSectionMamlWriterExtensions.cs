using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class RelatedSectionMamlWriterExtensions
	{
		public static void WriteRelatedTopics(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			writer.StartRelatedTopics();
			const string xmlTopicType = "3272D745-2FFC-48C4-9E9D-CF2B2B784D5F";
			var rootItem = FindRootItem(obj);
			if (rootItem != null && rootItem != obj)
			{
				var rootItemTopic = context.TopicManager.GetTopic(rootItem);
				if (rootItemTopic != null)
					writer.WriteLink(rootItemTopic.Id, rootItemTopic.LinkTitle, xmlTopicType);
			}

			var targetNamespace = obj.GetSchema().TargetNamespace;
			var targetNamespaceTopic = context.TopicManager.GetNamespaceTopic(targetNamespace);
			if (targetNamespaceTopic != null)
				writer.WriteLink(targetNamespaceTopic.Id, targetNamespaceTopic.LinkTitle, xmlTopicType);

			var info = context.DocumentationManager.GetObjectDocumentationInfo(obj);
			if (info != null && info.RelatedTopicsNode != null)
				writer.WriteRawContent(info.RelatedTopicsNode);
			writer.EndRelatedTopics();
		}

		private static XmlSchemaObject FindRootItem(XmlSchemaObject obj)
		{
			var lastObject = obj;
			var parent = lastObject.Parent;
			while (parent != null)
			{
				var parentAsSchema = parent as XmlSchema;
				if (parentAsSchema != null)
					return lastObject;

				lastObject = parent;
				parent = parent.Parent;
			}

			return null;
		}
	}
}
