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

        public ContentGenerator(IMessageReporter messageReporter, Configuration configuration)
        {
            _context = new Context(messageReporter, configuration);
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
                    writer.WriteIntroductionForSchemaSet(_context);
                    writer.WriteRemarksSectionForSchemaSet(_context);
                    writer.WriteExamplesSectionForSchemaSet(_context);
                    writer.WriteNamespacesSection(_context, _context.SchemaSetManager.GetNamespaces());
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
                    writer.WriteIntroductionForSchemaSet(_context);
                    writer.WriteRemarksSectionForSchemaSet(_context);
                    writer.WriteExamplesSectionForSchemaSet(_context);
                    writer.WriteRootSchemasSection(_context, _context.SchemaSetManager.GetNamespaceRootSchemas(topic.Namespace));
                    writer.WriteRootElementsSection(_context, _context.SchemaSetManager.GetNamespaceRootElements(topic.Namespace));
                    writer.WriteSchemasSection(_context, contentFinder.Schemas);
                    writer.WriteElementsSection(_context, contentFinder.Elements);
                    writer.WriteAttributesSection(_context, contentFinder.Attributes);
                    writer.WriteGroupsSection(_context, contentFinder.Groups);
                    writer.WriteAttributeGroupsSection(_context, contentFinder.AttributeGroups);
                    writer.WriteSimpleTypesSection(_context, contentFinder.SimpleTypes);
                    writer.WriteComplexTypesSection(_context, contentFinder.ComplexTypes);
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
                writer.WriteIntroductionForNamespace(_context, topic.Namespace);
                writer.WriteRemarksSectionForNamespace(_context, topic.Namespace);
                writer.WriteExamplesSectionForNamespace(_context, topic.Namespace);
                writer.WriteRootSchemasSection(_context, _context.SchemaSetManager.GetNamespaceRootSchemas(topic.Namespace));
                writer.WriteRootElementsSection(_context, _context.SchemaSetManager.GetNamespaceRootElements(topic.Namespace));
                writer.WriteSchemasSection(_context, contentFinder.Schemas);
                writer.WriteElementsSection(_context, contentFinder.Elements);
                writer.WriteAttributesSection(_context, contentFinder.Attributes);
                writer.WriteGroupsSection(_context, contentFinder.Groups);
                writer.WriteAttributeGroupsSection(_context, contentFinder.AttributeGroups);
                writer.WriteSimpleTypesSection(_context, contentFinder.SimpleTypes);
                writer.WriteComplexTypesSection(_context, contentFinder.ComplexTypes);
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
                writer.WriteIntroductionForSchema(_context, schema);
                writer.WriteRemarksSectionForObject(_context, schema);
                writer.WriteExamplesSectionForObject(_context, schema);
                writer.WriteElementsSection(_context, contentFinder.Elements);
                writer.WriteAttributesSection(_context, contentFinder.Attributes);
                writer.WriteGroupsSection(_context, contentFinder.Groups);
                writer.WriteAttributeGroupsSection(_context, contentFinder.AttributeGroups);
                writer.WriteSimpleTypesSection(_context, contentFinder.SimpleTypes);
                writer.WriteComplexTypesSection(_context, contentFinder.ComplexTypes);
                writer.EndTopic();
            }
        }

        private void GenerateOverviewTopic(Topic topic)
        {
            var contentFinder = new NamespaceContentFinder(_context.SchemaSetManager, topic.Namespace);
            contentFinder.Traverse(_context.SchemaSetManager.SchemaSet);

            using (var stream = File.Create(topic.FileName))
            using (var writer = new MamlWriter(stream))
            {
                writer.StartTopic(topic.Id);
                writer.WriteIntroductionForOverview(_context, topic.Namespace);

                switch (topic.TopicType)
                {
                    case TopicType.RootSchemasSection:
                        writer.WriteRootSchemasSection(_context, _context.SchemaSetManager.GetNamespaceRootSchemas(topic.Namespace));
                        break;
                    case TopicType.RootElementsSection:
                        writer.WriteRootElementsSection(_context, _context.SchemaSetManager.GetNamespaceRootElements(topic.Namespace));
                        break;
                    case TopicType.SchemasSection:
                        writer.WriteSchemasSection(_context, contentFinder.Schemas);
                        break;
                    case TopicType.ElementsSection:
                        writer.WriteElementsSection(_context, contentFinder.Elements);
                        break;
                    case TopicType.AttributesSection:
                        writer.WriteAttributesSection(_context, contentFinder.Attributes);
                        break;
                    case TopicType.AttributeGroupsSection:
                        writer.WriteAttributeGroupsSection(_context, contentFinder.AttributeGroups);
                        break;
                    case TopicType.GroupsSection:
                        writer.WriteGroupsSection(_context, contentFinder.Groups);
                        break;
                    case TopicType.SimpleTypesSection:
                        writer.WriteSimpleTypesSection(_context, contentFinder.SimpleTypes);
                        break;
                    case TopicType.ComplexTypesSection:
                        writer.WriteComplexTypesSection(_context, contentFinder.ComplexTypes);
                        break;
                    default:
                        throw ExceptionBuilder.UnhandledCaseLabel(topic.TopicType);
                }

                writer.EndTopic();
            }
        }

        private void GenerateElementTopic(Topic topic)
        {
            var element = (XmlSchemaElement)topic.SchemaObject;
            var parents = _context.SchemaSetManager.GetObjectParents(element);
            var simpleTypeStructureRoot = _context.SchemaSetManager.GetSimpleTypeStructure(element.ElementSchemaType);
            var children = _context.SchemaSetManager.GetChildren(element);
            var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(element);
            var constraints = element.Constraints;

            using (var stream = File.Create(topic.FileName))
            using (var writer = new MamlWriter(stream))
            {
                writer.StartTopic(topic.Id);
                writer.WriteIntroductionForObject(_context, element);
                writer.WriteTypeSection(_context, element);
                writer.WriteContentTypeSection(_context, simpleTypeStructureRoot);
                writer.WriteParentsSection(_context, parents);
                writer.WriteChildrenSection(_context, children);
                writer.WriteAttributesSection(_context, attributeEntries);
                writer.WriteConstraintsSection(_context, constraints);
                writer.WriteRemarksSectionForObject(_context, element);
                writer.WriteExamplesSectionForObject(_context, element);
                writer.WriteSyntaxSection(_context, element);
                writer.WriteRelatedTopics(_context, element);
                writer.EndTopic();
            }
        }

        private void GenerateAttributeTopic(Topic topic)
        {
            var attribute = (XmlSchemaAttribute)topic.SchemaObject;
            var parents = _context.SchemaSetManager.GetObjectParents(attribute);
            var simpleTypeStructureRoot = _context.SchemaSetManager.GetSimpleTypeStructure(attribute.AttributeSchemaType);

            using (var stream = File.Create(topic.FileName))
            using (var writer = new MamlWriter(stream))
            {
                writer.StartTopic(topic.Id);
                writer.WriteIntroductionForObject(_context, attribute);
                writer.WriteContentTypeSection(_context, simpleTypeStructureRoot);
                writer.WriteParentsSection(_context, parents);
                writer.WriteRemarksSectionForObject(_context, attribute);
                writer.WriteExamplesSectionForObject(_context, attribute);
                writer.WriteSyntaxSection(_context, attribute);
                writer.WriteRelatedTopics(_context, attribute);
                writer.EndTopic();
            }
        }

        private void GenerateGroupTopic(Topic topic)
        {
            var group = (XmlSchemaGroup)topic.SchemaObject;
            var parents = _context.SchemaSetManager.GetObjectParents(group);
            var children = _context.SchemaSetManager.GetChildren(group);

            using (var stream = File.Create(topic.FileName))
            using (var writer = new MamlWriter(stream))
            {
                writer.StartTopic(topic.Id);
                writer.WriteIntroductionForObject(_context, group);
                writer.WriteUsagesSection(_context, parents);
                writer.WriteChildrenSection(_context, children);
                writer.WriteRemarksSectionForObject(_context, group);
                writer.WriteExamplesSectionForObject(_context, group);
                writer.WriteSyntaxSection(_context, group);
                writer.WriteRelatedTopics(_context, group);
                writer.EndTopic();
            }
        }

        private void GenerateAttributeGroup(Topic topic)
        {
            var attributeGroup = (XmlSchemaAttributeGroup)topic.SchemaObject;
            var usages = _context.SchemaSetManager.GetObjectParents(attributeGroup);
            var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(attributeGroup);

            using (var stream = File.Create(topic.FileName))
            using (var writer = new MamlWriter(stream))
            {
                writer.StartTopic(topic.Id);
                writer.WriteIntroductionForObject(_context, attributeGroup);
                writer.WriteUsagesSection(_context, usages);
                writer.WriteAttributesSection(_context, attributeEntries);
                writer.WriteRemarksSectionForObject(_context, attributeGroup);
                writer.WriteExamplesSectionForObject(_context, attributeGroup);
                writer.WriteSyntaxSection(_context, attributeGroup);
                writer.WriteRelatedTopics(_context, attributeGroup);
                writer.EndTopic();
            }
        }

        private void GenerateSimpleTypeTopic(Topic topic)
        {
            var simpleType = (XmlSchemaSimpleType)topic.SchemaObject;
            var usages = _context.SchemaSetManager.GetTypeUsages(simpleType);
            var simpleTypeStructureRoot = _context.SchemaSetManager.GetSimpleTypeStructure(simpleType.Content);

            using (var stream = File.Create(topic.FileName))
            using (var writer = new MamlWriter(stream))
            {
                writer.StartTopic(topic.Id);
                writer.WriteIntroductionForObject(_context, simpleType);
                writer.WriteContentTypeSection(_context, simpleTypeStructureRoot);
                writer.WriteUsagesSection(_context, usages);
                writer.WriteRemarksSectionForObject(_context, simpleType);
                writer.WriteExamplesSectionForObject(_context, simpleType);
                writer.WriteSyntaxSection(_context, simpleType);
                writer.WriteRelatedTopics(_context, simpleType);
                writer.EndTopic();
            }
        }

        private void GenerateComplexTypeTopic(Topic topic)
        {
            var complexType = (XmlSchemaComplexType)topic.SchemaObject;
            var usages = _context.SchemaSetManager.GetTypeUsages(complexType);
            var simpleTypeStructureRoot = _context.SchemaSetManager.GetSimpleTypeStructure(complexType);
            var children = _context.SchemaSetManager.GetChildren(complexType);
            var attributeEntries = _context.SchemaSetManager.GetAttributeEntries(complexType);

            using (var stream = File.Create(topic.FileName))
            using (var writer = new MamlWriter(stream))
            {
                writer.StartTopic(topic.Id);
                writer.WriteIntroductionForObject(_context, complexType);
                writer.WriteBaseTypeSection(_context, complexType);
                writer.WriteContentTypeSection(_context, simpleTypeStructureRoot);
                writer.WriteUsagesSection(_context, usages);
                writer.WriteChildrenSection(_context, children);
                writer.WriteAttributesSection(_context, attributeEntries);
                writer.WriteRemarksSectionForObject(_context, complexType);
                writer.WriteExamplesSectionForObject(_context, complexType);
                writer.WriteSyntaxSection(_context, complexType);
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