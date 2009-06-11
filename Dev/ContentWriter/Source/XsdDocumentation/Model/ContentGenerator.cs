using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using XsdDocumentation.Markup;

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
						throw ExceptionBuilder.UnhandledCaseLabel(topic.TopicType);
				}

				GenerateTopicFiles(topic.Children);
			}
		}

		private void GenerateSchemaSetTopic(Topic topic)
		{
			if (_context.Configuration.NamespaceContainer)
			{
				using (var stream = File.Create(topic.FileName))
				using (var writer = new MamlWriter(stream))
				{
					writer.StartTopic(topic.Id);

					writer.StartIntroduction();
					writer.WriteSummaryForSchemaSet(_context);
					writer.EndIntroduction();

					writer.WriteRemarksForSchemaSet(_context);

					writer.WriteTableSection(_context, _context.SchemaSetManager.GetNamespaces(), "Namespaces");

					writer.EndTopic();
				}
			}
			else
			{
				var contentFinder = new NamespaceContentFinder(_context.SchemaSetManager, topic.Namespace);
				contentFinder.Traverse(_context.SchemaSetManager.SchemaSet);

				using (var stream = File.Create(topic.FileName))
				using (var writer = new MamlWriter(stream))
				{
					writer.StartTopic(topic.Id);

					writer.StartIntroduction();
					writer.WriteSummaryForSchemaSet(_context);
					writer.EndIntroduction();

					writer.WriteRemarksForSchemaSet(_context);

					writer.WriteTableSection(_context, _context.SchemaSetManager.GetNamespaceRootSchemas(topic.Namespace), "Root Schemas");
					writer.WriteTableSection(_context, _context.SchemaSetManager.GetNamespaceRootElements(topic.Namespace), "Root Elements");
					writer.WriteTableSection(_context, contentFinder.Schemas, "Schemas");
					writer.WriteTableSection(_context, contentFinder.Elements, "Elements");
					writer.WriteTableSection(_context, contentFinder.Attributes, "Attributes");
					writer.WriteTableSection(_context, contentFinder.Groups, "Groups");
					writer.WriteTableSection(_context, contentFinder.AttributeGroups, "Attribute Groups");
					writer.WriteTableSection(_context, contentFinder.SimpleTypes, "Simple Types");
					writer.WriteTableSection(_context, contentFinder.ComplexTypes, "Complex Types");

					writer.EndTopic();
				}
			}
		}

		private void GenerateNamespaceTopic(Topic topic)
		{
			var contentFinder = new NamespaceContentFinder(_context.SchemaSetManager, topic.Namespace);
			contentFinder.Traverse(_context.SchemaSetManager.SchemaSet);

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.WriteSummaryForNamespace(_context, topic.Namespace);
				writer.WriteObsoleteInfo(_context, topic.Namespace);
				writer.EndIntroduction();

				writer.WriteRemarksForNamespace(_context, topic.Namespace);

				writer.WriteTableSection(_context, _context.SchemaSetManager.GetNamespaceRootSchemas(topic.Namespace), "Root Schemas");
				writer.WriteTableSection(_context, _context.SchemaSetManager.GetNamespaceRootElements(topic.Namespace), "Root Elements");
				writer.WriteTableSection(_context, contentFinder.Schemas, "Schemas");
				writer.WriteTableSection(_context, contentFinder.Elements, "Elements");
				writer.WriteTableSection(_context, contentFinder.Attributes, "Attributes");
				writer.WriteTableSection(_context, contentFinder.Groups, "Groups");
				writer.WriteTableSection(_context, contentFinder.AttributeGroups, "Attribute Groups");
				writer.WriteTableSection(_context, contentFinder.SimpleTypes, "Simple Types");
				writer.WriteTableSection(_context, contentFinder.ComplexTypes, "Complex Types");

				writer.EndTopic();
			}
		}

		private void GenerateSchemaTopic(Topic topic)
		{
			var schema = (XmlSchema)topic.SchemaObject;

			var contentFinder = new SchemaContentFinder(schema);
			contentFinder.Traverse(schema);

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.WriteSummaryForObject(_context, schema);
				writer.WriteObsoleteInfo(_context, schema);
				writer.WriteNamespaceInfo(_context, topic.Namespace);
				writer.EndIntroduction();

				writer.WriteRemarksForObject(_context, schema);

				writer.WriteTableSection(_context, contentFinder.Elements, "Elements");
				writer.WriteTableSection(_context, contentFinder.Attributes, "Attributes");
				writer.WriteTableSection(_context, contentFinder.Groups, "Groups");
				writer.WriteTableSection(_context, contentFinder.AttributeGroups, "Attribute Groups");
				writer.WriteTableSection(_context, contentFinder.SimpleTypes, "Simple Types");
				writer.WriteTableSection(_context, contentFinder.ComplexTypes, "Complex Types");

				writer.EndTopic();
			}
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
					throw ExceptionBuilder.UnhandledCaseLabel(topic.TopicType);
			}

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.StartParagraph();
				writer.WriteString("The ");
				writer.WriteNamespaceLink(_context, topic.Namespace);
				writer.WriteString(" namespace exposes the following members.");
				writer.EndParagraph();
				writer.EndIntroduction();

				writer.WriteTableSection(_context, schemaObjects, topicTitle);

				writer.EndTopic();
			}
		}

		private void GenerateElementTopic(Topic topic)
		{
			var element = (XmlSchemaElement) topic.SchemaObject;
			var parents = _context.SchemaSetManager.GetObjectParents(element);
			var simpleTypeStructureRoots = _context.SchemaSetManager.GetSimpleTypeStructure(element.ElementSchemaType);
			var children = _context.SchemaSetManager.GetChildren(element);
			var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(element);

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.WriteSummaryForObject(_context, element);
				writer.WriteObsoleteInfo(_context, topic.Namespace);
				writer.WriteNamespaceAndSchemaInfo(_context, element);
				writer.EndIntroduction();

				writer.StartSection("Type", "type");
				writer.WriteElementTypeName(_context, element);
				writer.EndSection();

				writer.StartSection("Parents", "parents");
				writer.WriteList(_context, parents);
				writer.EndSection();

				writer.StartSection("Children", "children");
				writer.WriteChildren(_context, children);
				writer.EndSection();

				writer.StartSection("Content Type", "contentType");
				writer.WriteSimpleTypeStrucure(_context, simpleTypeStructureRoots);
				writer.EndSection();

				writer.StartSection("Attributes", "attributes");
				writer.WriteAttributes(_context, attributeEntries);
				writer.EndSection();

				writer.StartSection("Constraints", "constraints");
				writer.WriteConstraints(_context, element);
				writer.EndSection();

				writer.WriteRemarksForObject(_context, element);

				writer.StartSection("Syntax", "syntax");
				writer.WriteCode(_context, element);
				writer.EndSection();

				writer.WriteRelatedTopics(_context, element);

				writer.EndTopic();
			}
		}

		private void GenerateAttributeTopic(Topic topic)
		{
			var attribute = (XmlSchemaAttribute) topic.SchemaObject;
			var usages = _context.SchemaSetManager.GetObjectParents(attribute);
			var simpleTypeStructureRoots = _context.SchemaSetManager.GetSimpleTypeStructure(attribute.AttributeSchemaType);

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.WriteSummaryForObject(_context, attribute);
				writer.WriteObsoleteInfo(_context, topic.Namespace);
				writer.WriteNamespaceAndSchemaInfo(_context, attribute);
				writer.EndIntroduction();

				writer.StartSection("Usages", "usages");
				writer.WriteList(_context, usages);
				writer.EndSection();

				writer.StartSection("Type", "type");
				writer.WriteSimpleTypeStrucure(_context, simpleTypeStructureRoots);
				writer.EndSection();

				writer.WriteRemarksForObject(_context, attribute);

				writer.StartSection("Syntax", "syntax");
				writer.WriteCode(_context, attribute);
				writer.EndSection();

				writer.WriteRelatedTopics(_context, attribute);

				writer.EndTopic();
			}
		}

		private void GenerateGroupTopic(Topic topic)
		{
			var group = (XmlSchemaGroup) topic.SchemaObject;
			var parents = _context.SchemaSetManager.GetObjectParents(group);
			var children = _context.SchemaSetManager.GetChildren(group);

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.WriteSummaryForObject(_context, group);
				writer.WriteObsoleteInfo(_context, topic.Namespace);
				writer.WriteNamespaceAndSchemaInfo(_context, group);
				writer.EndIntroduction();

				writer.StartSection("Usages", "usages");
				writer.WriteList(_context, parents);
				writer.EndSection();

				writer.StartSection("Children", "children");
				writer.WriteChildren(_context, children);
				writer.EndSection();

				writer.WriteRemarksForObject(_context, group);

				writer.StartSection("Syntax", "syntax");
				writer.WriteCode(_context, group);
				writer.EndSection();

				writer.WriteRelatedTopics(_context, group);

				writer.EndTopic();
			}
		}

		private void GenerateAttributeGroup(Topic topic)
		{
			var attributeGroup = (XmlSchemaAttributeGroup) topic.SchemaObject;
			var parents = _context.SchemaSetManager.GetObjectParents(attributeGroup);
			var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(attributeGroup);

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.WriteSummaryForObject(_context, attributeGroup);
				writer.WriteObsoleteInfo(_context, topic.Namespace);
				writer.WriteNamespaceAndSchemaInfo(_context, attributeGroup);
				writer.EndIntroduction();

				writer.StartSection("Usages", "usages");
				writer.WriteList(_context, parents);
				writer.EndSection();

				writer.StartSection("Attributes", "attributes");
				writer.WriteAttributes(_context, attributeEntries);
				writer.EndSection();

				writer.WriteRemarksForObject(_context, attributeGroup);

				writer.StartSection("Syntax", "syntax");
				writer.WriteCode(_context, attributeGroup);
				writer.EndSection();

				writer.WriteRelatedTopics(_context, attributeGroup);

				writer.EndTopic();
			}
		}

		private void GenerateSimpleTypeTopic(Topic topic)
		{
			var simpleType = (XmlSchemaSimpleType) topic.SchemaObject;
			var usages = _context.SchemaSetManager.GetTypeUsages(simpleType);
			var simpleTypeStructureRoots = _context.SchemaSetManager.GetSimpleTypeStructure(simpleType.Content);

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.WriteSummaryForObject(_context, simpleType);
				writer.WriteObsoleteInfo(_context, topic.Namespace);
				writer.WriteNamespaceAndSchemaInfo(_context, simpleType);
				writer.EndIntroduction();

				writer.StartSection("Usages", "usages");
				writer.WriteList(_context, usages);
				writer.EndSection();

				writer.StartSection("Content Type", "contentType");
				writer.WriteSimpleTypeStrucure(_context, simpleTypeStructureRoots);
				writer.EndSection();

				writer.WriteRemarksForObject(_context, simpleType);

				writer.StartSection("Syntax", "syntax");
				writer.WriteCode(_context, simpleType);
				writer.EndSection();

				writer.WriteRelatedTopics(_context, simpleType);

				writer.EndTopic();
			}
		}

		private void GenerateComplexTypeTopic(Topic topic)
		{
			var complexType = (XmlSchemaComplexType) topic.SchemaObject;
			var usages = _context.SchemaSetManager.GetTypeUsages(complexType);
			var simpleTypeStructureRoots = _context.SchemaSetManager.GetSimpleTypeStructure(complexType);
			var children = _context.SchemaSetManager.GetChildren(complexType);
			var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(complexType);

			using (var stream = File.Create(topic.FileName))
			using (var writer = new MamlWriter(stream))
			{
				writer.StartTopic(topic.Id);

				writer.StartIntroduction();
				writer.WriteSummaryForObject(_context, complexType);
				writer.WriteObsoleteInfo(_context, topic.Namespace);
				writer.WriteNamespaceAndSchemaInfo(_context, complexType);
				writer.EndIntroduction();

				writer.StartSection("Base Type", "baseType");
				writer.WriteTypeName(_context, complexType.BaseXmlSchemaType);
				writer.EndSection();

				writer.StartSection("Usages", "usages");
				writer.WriteList(_context, usages);
				writer.EndSection();

				writer.StartSection("Children", "children");
				writer.WriteChildren(_context, children);
				writer.EndSection();

				writer.StartSection("Content Type", "contentType");
				writer.WriteSimpleTypeStrucure(_context, simpleTypeStructureRoots);
				writer.EndSection();

				writer.StartSection("Attributes", "attributes");
				writer.WriteAttributes(_context, attributeEntries);
				writer.EndSection();

				writer.WriteRemarksForObject(_context, complexType);

				writer.StartSection("Syntax", "syntax");
				writer.WriteCode(_context, complexType);
				writer.EndSection();

				writer.WriteRelatedTopics(_context, complexType);

				writer.EndTopic();
			}
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

		private static string GetAbsoluteFileName(string topicsFolder, Topic topic)
		{
			return Path.Combine(topicsFolder, Path.ChangeExtension(topic.Id, ".aml"));
		}
	}
}