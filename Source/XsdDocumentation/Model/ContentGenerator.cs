using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using XsdDocumentation.Markup;
using XsdDocumentation.Properties;

namespace XsdDocumentation.Model
{
	public sealed class ContentGenerator
	{
		private Context _context;
		private string _contentFile;
		private string _indexFile;
		private string _topicsFolder;
		private string _mediaFolder;
		private List<string> _topicFiles = new List<string>();
		private List<MediaItem> _mediaItems = new List<MediaItem>();

		public ContentGenerator(Configuration configuration)
		{
			_context = new Context(configuration);
		}

		public string ContentFile
		{
			get { return _contentFile; }
		}

		public string IndexFile
		{
			get { return _indexFile; }
		}

		public string TopicsFolder
		{
			get { return _topicsFolder; }
		}

		public List<string> TopicFiles
		{
			get { return _topicFiles; }
		}

		public List<MediaItem> MediaItems
		{
			get { return _mediaItems; }
		}

		public void Generate()
		{
			_topicsFolder = Path.Combine(_context.Configuration.OutputFolderPath, "xsdTopics");
			_contentFile = Path.Combine(_topicsFolder, "xsd.content");
			_indexFile = Path.Combine(_topicsFolder, "xsd.index");
			_mediaFolder = Path.Combine(_context.Configuration.OutputFolderPath, "xsdMedia");
			GenerateIndex();
			GenerateContentFile();
			GenerateTopicFiles();
			GenerateMediaFiles();
		}

		private void GenerateIndex()
		{
			var topicIndex = new TopicIndex();
			topicIndex.Load(_context.TopicManager);
			topicIndex.Save(_indexFile);
		}

		private void GenerateTopicFiles()
		{
			Directory.CreateDirectory(_topicsFolder);

			GenerateTopicFiles(_context.TopicManager.Topics);
		}

		private void GenerateTopicFiles(IEnumerable<Topic> topics)
		{
			foreach (var topic in topics)
			{
				topic.FileName = GetAbsoluteFileName(_topicsFolder, topic);
				_topicFiles.Add(topic.FileName);

				switch (topic.TopicType)
				{
					case TopicType.SchemaSet:
						GenerateSchemaSetTopic(topic);
						break;
					case TopicType.Namespace:
						GenerateNamespaceTopic(topic);
						break;
					case TopicType.Schema:
						GenerateSchemaTopic(topic);
						break;
					case TopicType.Element:
						GenerateElementTopic(topic);
						break;
					case TopicType.Attribute:
						GenerateAttributeTopic(topic);
						break;
					case TopicType.AttributeGroup:
						GenerateAttributeGroup(topic);
						break;
					case TopicType.Group:
						GenerateGroupTopic(topic);
						break;
					case TopicType.SimpleType:
						GenerateSimpleTypeTopic(topic);
						break;
					case TopicType.ComplexType:
						GenerateComplexTypeTopic(topic);
						break;
					case TopicType.RootSchemasSection:
					case TopicType.RootElementsSection:
					case TopicType.SchemasSection:
					case TopicType.ElementsSection:
					case TopicType.AttributesSection:
					case TopicType.AttributeGroupsSection:
					case TopicType.GroupsSection:
					case TopicType.SimpleTypesSection:
					case TopicType.ComplexTypesSection:
						GenerateOverviewTopic(topic);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				GenerateTopicFiles(topic.Children);
			}
		}

		private void GenerateSchemaSetTopic(Topic topic)
		{
			var content = Resources.SchemaSetTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForSchemaSet(_context));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForSchemaSet(_context));
			content = content.Replace("${NamespaceSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, _context.SchemaSetManager.GetNamespaces(), "Namespaces"));
			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateNamespaceTopic(Topic topic)
		{
			var contentFinder = new NamespaceContentFinder(_context.SchemaSetManager, topic.Namespace);
			contentFinder.Traverse(_context.SchemaSetManager.SchemaSet);

			var content = Resources.NamespaceTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForNamespace(_context, topic.Namespace));
			content = content.Replace("${Obsolete}", ObsoleteMarkupBuilder.BuildForNamespace(_context, topic.Namespace));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForNamespace(_context, topic.Namespace));
			content = content.Replace("${RootSchemaSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, _context.SchemaSetManager.GetNamespaceRootSchemas(topic.Namespace), "Root Schemas"));
			content = content.Replace("${RootElementSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, _context.SchemaSetManager.GetNamespaceRootElements(topic.Namespace), "Root Elements"));
			content = content.Replace("${SchemaSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.Schemas, "Schemas"));
			content = content.Replace("${ElementSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.Elements, "Elements"));
			content = content.Replace("${AttributeSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.Attributes, "Attributes"));
			content = content.Replace("${GroupSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.Groups, "Groups"));
			content = content.Replace("${AttributeGroupSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.AttributeGroups, "Attribute Groups"));
			content = content.Replace("${SimpleTypesSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.SimpleTypes, "Simple Types"));
			content = content.Replace("${ComplexTypesSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.ComplexTypes, "Complex Types"));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateSchemaTopic(Topic topic)
		{
			var schema = (XmlSchema)topic.SchemaObject;

			var contentFinder = new SchemaContentFinder(schema);
			contentFinder.Traverse(schema);

			var content = Resources.SchemaTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForObject(_context, schema));
			content = content.Replace("${Obsolete}", ObsoleteMarkupBuilder.BuildForObject(_context, schema));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForObject(_context, schema));
			content = content.Replace("${Namespace}", GetNamespaceTopicLink(topic.Namespace));
			content = content.Replace("${ElementSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.Elements, "Elements"));
			content = content.Replace("${AttributeSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.Attributes, "Attributes"));
			content = content.Replace("${GroupSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.Groups, "Groups"));
			content = content.Replace("${AttributeGroupSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.AttributeGroups, "Attribute Groups"));
			content = content.Replace("${SimpleTypesSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.SimpleTypes, "Simple Types"));
			content = content.Replace("${ComplexTypesSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, contentFinder.ComplexTypes, "Complex Types"));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateOverviewTopic(Topic topic)
		{
			var schemaContentFinder = new NamespaceContentFinder(_context.SchemaSetManager, topic.Namespace);
			schemaContentFinder.Traverse(_context.SchemaSetManager.SchemaSet);

			string topicTitle;
			IEnumerable<XmlSchemaObject> schemaObjects;

			switch (topic.TopicType)
			{
				case TopicType.RootSchemasSection:
					topicTitle = "Root Schemas";
					schemaObjects = _context.SchemaSetManager.GetNamespaceRootSchemas(topic.Namespace);
					break;
				case TopicType.RootElementsSection:
					topicTitle = "Root Elements";
					schemaObjects = _context.SchemaSetManager.GetNamespaceRootElements(topic.Namespace);
					break;
				case TopicType.SchemasSection:
					topicTitle = "Schemas";
					schemaObjects = schemaContentFinder.Schemas;
					break;
				case TopicType.ElementsSection:
					topicTitle = "Elements";
					schemaObjects = schemaContentFinder.Elements;
					break;
				case TopicType.AttributesSection:
					topicTitle = "Attributes";
					schemaObjects = schemaContentFinder.Attributes;
					break;
				case TopicType.AttributeGroupsSection:
					topicTitle = "Attribute Groups";
					schemaObjects = schemaContentFinder.AttributeGroups;
					break;
				case TopicType.GroupsSection:
					topicTitle = "Groups";
					schemaObjects = schemaContentFinder.Groups;
					break;
				case TopicType.SimpleTypesSection:
					topicTitle = "Simple Types";
					schemaObjects = schemaContentFinder.SimpleTypes;
					break;
				case TopicType.ComplexTypesSection:
					topicTitle = "Complex Types";
					schemaObjects = schemaContentFinder.ComplexTypes;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var content = Resources.OverviewTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Namespace}", GetNamespaceTopicLink(topic.Namespace));
			content = content.Replace("${ItemSection}", TableMarkupBuilder.GetTableSectionMarkup(_context, schemaObjects, topicTitle));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateElementTopic(Topic topic)
		{
			var element = (XmlSchemaElement) topic.SchemaObject;
			var parents = _context.SchemaSetManager.GetObjectParents(element);
			var simpleTypeStructureRoots = _context.SchemaSetManager.GetSimpleTypeStructure(element.ElementSchemaType);
			var children = _context.SchemaSetManager.GetChildren(element);
			var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(element);

			var content = Resources.ElementTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForObject(_context, element));
			content = content.Replace("${Obsolete}", ObsoleteMarkupBuilder.BuildForObject(_context, element));
			content = content.Replace("${Namespace}", GetNamespaceTopicLink(element));
			content = content.Replace("${Schema}", GetSchemaTopicLink(element));
			content = content.Replace("${Type}", GetElementType(element));
			content = content.Replace("${Parents}", ListMarkupBuilder.Build(_context, parents));
			content = content.Replace("${Children}", ChildrenMarkupBuilder.Build(_context, children));
			content = content.Replace("${ContentType}", SimpleTypeStructureMarkupBuilder.Build(_context, simpleTypeStructureRoots));
			content = content.Replace("${Attributes}", AttributeMarkupBuilder.Build(_context, attributeEntries));
			content = content.Replace("${Constraints}", ConstrainedMarkupBuilder.Build(_context, element));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForObject(_context, element));
			content = content.Replace("${Syntax}", SyntaxMarkupBuilder.Build(_context, element));
			content = content.Replace("${RelatedTopics}", RelatedTopicsMarkupBuilder.Build(_context, element));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateAttributeTopic(Topic topic)
		{
			var attribute = (XmlSchemaAttribute) topic.SchemaObject;
			var usages = _context.SchemaSetManager.GetObjectParents(attribute);
			var simpleTypeStructureRoots = _context.SchemaSetManager.GetSimpleTypeStructure(attribute.AttributeSchemaType);

			var content = Resources.AttributeTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForObject(_context, attribute));
			content = content.Replace("${Obsolete}", ObsoleteMarkupBuilder.BuildForObject(_context, attribute));
			content = content.Replace("${Namespace}", GetNamespaceTopicLink(attribute));
			content = content.Replace("${Schema}", GetSchemaTopicLink(attribute));
			content = content.Replace("${Usages}", ListMarkupBuilder.Build(_context, usages));
			content = content.Replace("${Type}", SimpleTypeStructureMarkupBuilder.Build(_context, simpleTypeStructureRoots));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForObject(_context, attribute));
			content = content.Replace("${Syntax}", SyntaxMarkupBuilder.Build(_context, attribute));
			content = content.Replace("${RelatedTopics}", RelatedTopicsMarkupBuilder.Build(_context, attribute));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateGroupTopic(Topic topic)
		{
			var group = (XmlSchemaGroup) topic.SchemaObject;
			var parents = _context.SchemaSetManager.GetObjectParents(group);
			var children = _context.SchemaSetManager.GetChildren(group);

			var content = Resources.GroupTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForObject(_context, group));
			content = content.Replace("${Obsolete}", ObsoleteMarkupBuilder.BuildForObject(_context, group));
			content = content.Replace("${Namespace}", GetNamespaceTopicLink(group));
			content = content.Replace("${Schema}", GetSchemaTopicLink(group));
			content = content.Replace("${Usages}", ListMarkupBuilder.Build(_context, parents));
			content = content.Replace("${Children}", ChildrenMarkupBuilder.Build(_context, children));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForObject(_context, group));
			content = content.Replace("${Syntax}", SyntaxMarkupBuilder.Build(_context, group));
			content = content.Replace("${RelatedTopics}", RelatedTopicsMarkupBuilder.Build(_context, group));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateAttributeGroup(Topic topic)
		{
			var attributeGroup = (XmlSchemaAttributeGroup) topic.SchemaObject;
			var parents = _context.SchemaSetManager.GetObjectParents(attributeGroup);
			var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(attributeGroup);

			var content = Resources.AttributeGroupTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForObject(_context, attributeGroup));
			content = content.Replace("${Obsolete}", ObsoleteMarkupBuilder.BuildForObject(_context, attributeGroup));
			content = content.Replace("${Namespace}", GetNamespaceTopicLink(attributeGroup));
			content = content.Replace("${Schema}", GetSchemaTopicLink(attributeGroup));
			content = content.Replace("${Usages}", ListMarkupBuilder.Build(_context, parents));
			content = content.Replace("${Attributes}", AttributeMarkupBuilder.Build(_context, attributeEntries));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForObject(_context, attributeGroup));
			content = content.Replace("${Syntax}", SyntaxMarkupBuilder.Build(_context, attributeGroup));
			content = content.Replace("${RelatedTopics}", RelatedTopicsMarkupBuilder.Build(_context, attributeGroup));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateSimpleTypeTopic(Topic topic)
		{
			var simpleType = (XmlSchemaSimpleType) topic.SchemaObject;
			var usages = _context.SchemaSetManager.GetTypeUsages(simpleType);
			var simpleTypeStructureRoots = _context.SchemaSetManager.GetSimpleTypeStructure(simpleType.Content);

			var content = Resources.SimpleTypeTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForObject(_context, simpleType));
			content = content.Replace("${Obsolete}", ObsoleteMarkupBuilder.BuildForObject(_context, simpleType));
			content = content.Replace("${Namespace}", GetNamespaceTopicLink(simpleType));
			content = content.Replace("${Schema}", GetSchemaTopicLink(simpleType));
			content = content.Replace("${Usages}", ListMarkupBuilder.Build(_context, usages));
			content = content.Replace("${ContentType}", SimpleTypeStructureMarkupBuilder.Build(_context, simpleTypeStructureRoots));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForObject(_context, simpleType));
			content = content.Replace("${Syntax}", SyntaxMarkupBuilder.Build(_context, simpleType));
			content = content.Replace("${RelatedTopics}", RelatedTopicsMarkupBuilder.Build(_context, simpleType));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateComplexTypeTopic(Topic topic)
		{
			var complexType = (XmlSchemaComplexType) topic.SchemaObject;
			var usages = _context.SchemaSetManager.GetTypeUsages(complexType);
			var simpleTypeStructureRoots = _context.SchemaSetManager.GetSimpleTypeStructure(complexType);
			var children = _context.SchemaSetManager.GetChildren(complexType);
			var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(complexType);

			var content = Resources.ComplexTypeTopic;
			content = content.Replace("${TopicId}", topic.Id);
			content = content.Replace("${Summary}", SummaryMarkupBuilder.BuildForObject(_context, complexType));
			content = content.Replace("${Obsolete}", ObsoleteMarkupBuilder.BuildForObject(_context, complexType));
			content = content.Replace("${Namespace}", GetNamespaceTopicLink(complexType));
			content = content.Replace("${Schema}", GetSchemaTopicLink(complexType));
			content = content.Replace("${BaseType}", GetTypeMarkup(complexType.BaseXmlSchemaType));
			content = content.Replace("${Usages}", ListMarkupBuilder.Build(_context, usages));
			content = content.Replace("${Children}", ChildrenMarkupBuilder.Build(_context, children));
			content = content.Replace("${ContentType}", SimpleTypeStructureMarkupBuilder.Build(_context, simpleTypeStructureRoots));
			content = content.Replace("${Attributes}", AttributeMarkupBuilder.Build(_context, attributeEntries));
			content = content.Replace("${Remarks}", RemarksMarkupBuilder.BuildForObject(_context, complexType));
			content = content.Replace("${Syntax}", SyntaxMarkupBuilder.Build(_context, complexType));
			content = content.Replace("${RelatedTopics}", RelatedTopicsMarkupBuilder.Build(_context, complexType));

			File.WriteAllText(topic.FileName, content);
		}

		private void GenerateMediaFiles()
		{
			var mediaFolder = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Media");
			Directory.CreateDirectory(_mediaFolder);
			foreach (var artItem in ArtItem.ArtItems)
			{
				var sourceFile = Path.Combine(mediaFolder, artItem.FileName);
				var destinationFile = Path.Combine(_mediaFolder, artItem.FileName);
				File.Copy(sourceFile, destinationFile);

				var mediaItem = new MediaItem(artItem, destinationFile);
				_mediaItems.Add(mediaItem);
			}
		}

		private void GenerateContentFile()
		{
			var doc = new XmlDocument();
			var rootNode = doc.CreateElement("Topics");
			doc.AppendChild(rootNode);

			GenerateContentFileElements(rootNode, _context.TopicManager.Topics);

			var directory = Path.GetDirectoryName(_contentFile);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);
			doc.Save(_contentFile);
		}

		private static void GenerateContentFileElements(XmlNode parentNode, IEnumerable<Topic> topics)
		{
			foreach (var topic in topics)
			{
				var doc = parentNode.OwnerDocument;
				var topicElement = doc.CreateElement("Topic");
				topicElement.SetAttribute("id", topic.Id);
				topicElement.SetAttribute("visible", XmlConvert.ToString(true));
				topicElement.SetAttribute("title", topic.Title);
				parentNode.AppendChild(topicElement);

				if (topic.KeywordsK.Count > 0 ||
					topic.KeywordsF.Count > 0)
				{
					var helpKeywordsElement = doc.CreateElement("HelpKeywords");
					topicElement.AppendChild(helpKeywordsElement);
					AddKeywords(helpKeywordsElement, topic.KeywordsK, "K");
					AddKeywords(helpKeywordsElement, topic.KeywordsF, "F");
				}
				GenerateContentFileElements(topicElement, topic.Children);
			}
		}

		private static void AddKeywords(XmlNode helpKeywordsElement, IEnumerable<string> keywordsF, string index)
		{
			foreach (var keywordF in keywordsF)
			{
				var helpKeywordElement = helpKeywordsElement.OwnerDocument.CreateElement("HelpKeyword");
				helpKeywordElement.SetAttribute("index", index);
				helpKeywordElement.SetAttribute("term", keywordF);
				helpKeywordsElement.AppendChild(helpKeywordElement);
			}
		}

		private string GetNamespaceTopicLink(XmlSchemaObject schemaObject)
		{
			var targetNamespace = schemaObject.GetSchema().TargetNamespace;
			return GetNamespaceTopicLink(targetNamespace);
		}

		private string GetNamespaceTopicLink(string namespaceUri)
		{
			return MarkupHelper.GetHtmlTopicLink(_context.TopicManager.GetNamespaceTopic(namespaceUri));
		}

		private string GetSchemaTopicLink(XmlSchemaObject obj)
		{
			return MarkupHelper.GetHtmlTopicLink(_context.TopicManager.GetTopic(obj.GetSchema()));
		}

		private static string GetAbsoluteFileName(string topicsFolder, Topic topic)
		{
			return Path.Combine(topicsFolder, Path.ChangeExtension(topic.Id, ".aml"));
		}

		private string GetElementType(XmlSchemaElement element)
		{
			return element.ElementSchemaType is XmlSchemaSimpleType
			       	? String.Empty
			       	: GetTypeMarkup(element.ElementSchemaType);
		}

		private string GetTypeMarkup(XmlSchemaType type)
		{
			if (type == null || type == XmlSchemaType.GetBuiltInComplexType(XmlTypeCode.Item))
				return String.Empty;

			return TypeNameMarkupBuilder.Build(_context.TopicManager, type);
		}
	}
}