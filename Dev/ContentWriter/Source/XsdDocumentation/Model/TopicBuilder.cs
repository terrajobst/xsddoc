using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class TopicBuilder : XmlSchemaSetVisitor
	{
		private SchemaSetManager _schemaSetManager;
		private Dictionary<string, Topic> _namespaceTopics;
		private Dictionary<XmlSchemaObject, Topic> _topicDictionary;
		private Stack<List<Topic>> _topicStack = new Stack<List<Topic>>();
		private Stack<string> _topicUriStack = new Stack<string>();

		public TopicBuilder(SchemaSetManager schemaSetManager, Dictionary<string, Topic> namespaceTopics, Dictionary<XmlSchemaObject, Topic> topicDictionary)
		{
			_schemaSetManager = schemaSetManager;
			_namespaceTopics = namespaceTopics;
			_topicDictionary = topicDictionary;
		}

		public List<Topic> GetRootTopics()
		{
			if (_topicStack.Count == 0)
				return new List<Topic>();

			return _topicStack.Pop();
		}

		private void PushTopic(TopicType topicType, string objNamespace, XmlSchemaObject obj, string name)
		{
			Topic topic;
			if (topicType != TopicType.Namespace)
			{
				topic = AddTopic(topicType, objNamespace, obj, name);
			}
			else
			{
				var namespaceKey = objNamespace ?? string.Empty;
				if (!_namespaceTopics.TryGetValue(namespaceKey, out topic))
				{
					topic = AddTopic(topicType, objNamespace, obj, name);
					_namespaceTopics.Add(namespaceKey, topic);
				}
			}

			_topicStack.Push(topic.Children);
			_topicUriStack.Push(topic.LinkUri);
		}

		private Topic AddTopic(TopicType topicType, string objNamespace, XmlSchemaObject obj, string name)
		{
			if (_topicStack.Count == 0)
			{
				var root = new List<Topic>();
				_topicStack.Push(root);
			}

			var topic = new Topic
			            {
			            	Title = GetTopicTitle(topicType, name),
			            	LinkTitle = GetTopicLinkTitle(topicType, name),
			            	LinkUri = GetTopicLinkUri(topicType, objNamespace, obj),
			            	LinkIdUri = GetTopicLinkIdUri(topicType, obj),
			            	TopicType = topicType,
			            	Namespace = objNamespace,
			            	SchemaObject = obj
			            };

			if (obj != null)
				_topicDictionary.Add(obj, topic);

			_topicStack.Peek().Add(topic);

			return topic;
		}

		private string GetTopicLinkUri(TopicType type, string objNamespace, XmlSchemaObject obj)
		{
			if (type == TopicType.Namespace)
				return objNamespace ?? string.Empty;

			bool isGlobal = obj.Parent is XmlSchema;
			string parent = _topicUriStack.Peek();

			switch (type)
			{
				case TopicType.Schema:
					return parent + "#" + obj.GetSchemaName();
				case TopicType.Element:
					return isGlobal
					       	? parent + "#E/" + ((XmlSchemaElement) obj).QualifiedName.Name
					       	: parent + "/" + ((XmlSchemaElement) obj).QualifiedName.Name;
				case TopicType.Attribute:
					return isGlobal
					       	? parent + "#A/" + ((XmlSchemaAttribute) obj).QualifiedName.Name
					       	: parent + "/@" + ((XmlSchemaAttribute) obj).QualifiedName.Name;
				case TopicType.AttributeGroup:
					return parent + "#AG/" + ((XmlSchemaAttributeGroup)obj).QualifiedName.Name;
				case TopicType.Group:
					return parent + "#G/" + ((XmlSchemaGroup)obj).QualifiedName.Name;
				case TopicType.SimpleType:
				case TopicType.ComplexType:
					return parent + "#T/" + ((XmlSchemaType)obj).QualifiedName.Name;
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(type);
			}
		}

		private static string GetTopicLinkIdUri(TopicType type, XmlSchemaObject obj)
		{
			if (type == TopicType.Namespace || 
				type == TopicType.Schema)
			{
				return null;
			}
			
			var annotated = (XmlSchemaAnnotated) obj;
			if (string.IsNullOrEmpty(annotated.Id))
				return null;

			var targetNamespace = obj.GetSchema().TargetNamespace;
			var schemaName = obj.GetSchemaName();
			return string.Format("{0}#{1}#{2}", targetNamespace, schemaName, annotated.Id);
		}

		private static string GetTopicTitle(TopicType type, string name)
		{
			switch (type)
			{
				case TopicType.Namespace:
					return String.Format("{0} Namespace", name ?? "Empty");
				case TopicType.Schema:
					return String.Format("{0} Schema", name);
				case TopicType.Element:
					return String.Format("{0} Element", name);
				case TopicType.Attribute:
					return String.Format("{0} Attribute", name);
				case TopicType.AttributeGroup:
					return String.Format("{0} Attribute Group", name);
				case TopicType.Group:
					return String.Format("{0} Group", name);
				case TopicType.SimpleType:
					return String.Format("{0} Simple Type", name);
				case TopicType.ComplexType:
					return String.Format("{0} Complex Type", name);
				default:
					throw ExceptionBuilder.UnhandledCaseLabel(type);
			}
		}

		private static string GetTopicLinkTitle(TopicType type, string name)
		{
			if (type == TopicType.Namespace)
				return name ?? "Empty";

			return name;
		}

		private void PopTopic()
		{
			_topicStack.Pop();
			_topicUriStack.Pop();
		}

		protected override void Visit(XmlSchema schema)
		{
			if (_schemaSetManager.IsDependencySchema(schema))
				return;

			PushTopic(TopicType.Namespace, schema.TargetNamespace, null, schema.TargetNamespace);
			AddTopic(TopicType.Schema, schema.TargetNamespace, schema, schema.GetSchemaName());
			base.Visit(schema);
			PopTopic();
		}

		protected override void Visit(XmlSchemaElement element)
		{
			if (!element.RefName.IsEmpty ||
			    element.MaxOccurs == 0)
				return;

			PushTopic(TopicType.Element, element.QualifiedName.Namespace, element, element.Name);

			if (element.ElementSchemaType is XmlSchemaComplexType &&
			    element.ElementSchemaType.QualifiedName.IsEmpty)
				base.Visit(element);
			PopTopic();
		}

		protected override void Visit(XmlSchemaGroup group)
		{
			PushTopic(TopicType.Group, group.QualifiedName.Namespace, group, group.Name);
			base.Visit(group);
			PopTopic();
		}

		protected override void Visit(XmlSchemaAttribute attribute)
		{
			if (!attribute.RefName.IsEmpty ||
			    attribute.Use == XmlSchemaUse.Prohibited)
				return;

			AddTopic(TopicType.Attribute, attribute.QualifiedName.Namespace, attribute, attribute.Name);
		}

		protected override void Visit(XmlSchemaAttributeGroup group)
		{
			PushTopic(TopicType.AttributeGroup, group.QualifiedName.Namespace, group, group.Name);
			base.Visit(group);
			PopTopic();
		}

		protected override void Visit(XmlSchemaSimpleType type)
		{
			AddTopic(TopicType.SimpleType, type.QualifiedName.Namespace, type, type.Name);
		}

		protected override void Visit(XmlSchemaComplexType type)
		{
			if (type.QualifiedName.IsEmpty)
			{
				base.Visit(type);
			}
			else if (!_topicDictionary.ContainsKey(type))
			{
				PushTopic(TopicType.ComplexType, type.QualifiedName.Namespace, type, type.Name);
				base.Visit(type);
				PopTopic();
			}
		}
	}
}