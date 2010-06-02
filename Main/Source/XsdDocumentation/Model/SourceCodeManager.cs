using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal sealed class SourceCodeManager : Manager
    {
        private const string TempLookupIdNamespace = "558d3140-e819-4535-97c8-305d208918a1";

        private Dictionary<XmlSchema, XmlDocument> _schemaSources = new Dictionary<XmlSchema, XmlDocument>();
        private Dictionary<XmlSchema, XmlNamespaceManager> _namespaceManagers = new Dictionary<XmlSchema, XmlNamespaceManager>();
        private Dictionary<XmlSchemaAnnotated, string> _lookupIds;

        #region Lookup Id Helper Classes

        private abstract class IdVisitor : XmlSchemaSetVisitor
        {
            private XmlSchema _schema;
            private HashSet<XmlSchemaType> _processedTypes = new HashSet<XmlSchemaType>();

            protected IdVisitor(XmlSchema schema)
            {
                _schema = schema;
            }

            private void InternalHandleId(XmlSchemaAnnotated element)
            {
                if (element.GetSchema() == _schema)
                    HandleId(element);
            }

            protected abstract void HandleId(XmlSchemaAnnotated element);

            protected bool AlreadyProcessed(XmlSchemaType type)
            {
                return !_processedTypes.Add(type);
            }

            protected override void Visit(XmlSchemaAnnotated annotated)
            {
                var type = annotated as XmlSchemaType;
                if (type != null && AlreadyProcessed(type))
                    return;

                InternalHandleId(annotated);
                base.Visit(annotated);
            }
        }

        private sealed class LookupIdAssigner : IdVisitor
        {
            private Dictionary<XmlSchemaAnnotated, string> _lookupIds;
            private XmlDocument _lookupIdAttributeOwner = new XmlDocument();

            public LookupIdAssigner(XmlSchema schema, Dictionary<XmlSchemaAnnotated, string> lookupIds)
                : base(schema)
            {
                _lookupIds = lookupIds;
            }

            protected override void HandleId(XmlSchemaAnnotated element)
            {
                var lookupId = Guid.NewGuid().ToString();

                var lookupAttributeId = _lookupIdAttributeOwner.CreateAttribute("temp", "lookupId", TempLookupIdNamespace);
                lookupAttributeId.Value = lookupId;

                XmlAttribute[] newUnhandledAttributes;

                if (element.UnhandledAttributes == null)
                    newUnhandledAttributes = new XmlAttribute[1];
                else
                {
                    newUnhandledAttributes = new XmlAttribute[element.UnhandledAttributes.Length + 1];
                    Array.Copy(element.UnhandledAttributes, newUnhandledAttributes, element.UnhandledAttributes.Length);
                }

                newUnhandledAttributes[newUnhandledAttributes.Length - 1] = lookupAttributeId;
                element.UnhandledAttributes = newUnhandledAttributes;

                _lookupIds[element] = lookupId;
            }
        }

        private sealed class LookupIdRemover : IdVisitor
        {
            public LookupIdRemover(XmlSchema schema)
                : base(schema)
            {
            }

            protected override void HandleId(XmlSchemaAnnotated element)
            {
                if (element.UnhandledAttributes.Length == 1)
                    element.UnhandledAttributes = null;
                else
                {
                    var newUnhandledAttributes = new XmlAttribute[element.UnhandledAttributes.Length - 1];
                    Array.Copy(element.UnhandledAttributes, newUnhandledAttributes, newUnhandledAttributes.Length);
                    element.UnhandledAttributes = newUnhandledAttributes;
                }
            }
        }

        #endregion

        [Flags]
        private enum PostProcessingOptions
        {
            RemoveAnnotations = 0x01,
            CollapseElements = 0x02,
            CollapseAttributes = 0x03,
            Format = 0x04
        }

        public SourceCodeManager(Context context)
            : base(context)
        {
        }

        public override void Initialize()
        {
            _lookupIds = new Dictionary<XmlSchemaAnnotated, string>();

            foreach (var schema in Context.SchemaSetManager.SchemaSet.GetAllSchemas())
            {
                var schemaIdGenerator = new LookupIdAssigner(schema, _lookupIds);
                schemaIdGenerator.Traverse(schema);

                var schemaSource = GenerateSchemaSource(schema);

                var schemaIdRestorer = new LookupIdRemover(schema);
                schemaIdRestorer.Traverse(schema);

                var namespaceManager = new XmlNamespaceManager(schemaSource.NameTable);
                namespaceManager.AddNamespace("xs", XmlSchema.Namespace);
                namespaceManager.AddNamespace("temp", TempLookupIdNamespace);

                _schemaSources.Add(schema, schemaSource);
                _namespaceManagers.Add(schema, namespaceManager);
            }
        }

        private static XmlDocument GenerateSchemaSource(XmlSchema schema)
        {
            var doc = new XmlDocument();
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = new XmlTextWriter(stringWriter))
            {
                schema.Write(xmlTextWriter);
                doc.LoadXml(stringWriter.ToString());
            }

            return doc;
        }

        public string GetSourceCode(XmlSchemaObject obj)
        {
            return InternalGetSourceCode(obj, PostProcessingOptions.Format);
        }

        public string GetSourceCodeAbridged(XmlSchemaObject obj)
        {
            return InternalGetSourceCode(obj, PostProcessingOptions.RemoveAnnotations |
                                              PostProcessingOptions.CollapseElements |
                                              PostProcessingOptions.CollapseAttributes |
                                              PostProcessingOptions.Format);
        }

        private string InternalGetSourceCode(XmlSchemaObject obj, PostProcessingOptions processingOptions)
        {
            var schema = obj as XmlSchema;
            if (schema != null)
                return GetSchemaSourceCode(schema, processingOptions);

            var annotated = obj as XmlSchemaAnnotated;
            if (annotated != null)
                return GetObjectSourceCode(annotated, processingOptions);

            return string.Empty;
        }

        private string GetSchemaSourceCode(XmlSchema schema, PostProcessingOptions processingOptions)
        {
            XmlDocument schemaSource;
            if (!_schemaSources.TryGetValue(schema, out schemaSource))
                return string.Empty;

            var namespaceManager = _namespaceManagers[schema];

            return PostProcess(namespaceManager, schemaSource, processingOptions);
        }

        private string GetObjectSourceCode(XmlSchemaAnnotated annotated, PostProcessingOptions processingOptions)
        {
            var schema = annotated.GetSchema();
            if (schema == null)
                return string.Empty;

            XmlDocument schemaSource;
            if (!_schemaSources.TryGetValue(schema, out schemaSource))
                return string.Empty;

            var namespaceManager = _namespaceManagers[schema];

            var lookupId = _lookupIds[annotated];
            var source = GetObjectSourceCode(schemaSource, namespaceManager, lookupId, processingOptions);
            return source;
        }

        private static string GetObjectSourceCode(XmlNode schemaSource, XmlNamespaceManager namespaceManager, string objLookupId, PostProcessingOptions processingOptions)
        {
            var node = schemaSource.SelectSingleNode(string.Format("//*[@temp:lookupId='{0}']", objLookupId), namespaceManager);
            return PostProcess(namespaceManager, node, processingOptions);
        }

        private static string PostProcess(XmlNamespaceManager namespaceManager, XmlNode node, PostProcessingOptions processingOptions)
        {
            var removeAnnotations = (processingOptions & PostProcessingOptions.RemoveAnnotations) == PostProcessingOptions.RemoveAnnotations;
            var collapseElements = (processingOptions & PostProcessingOptions.CollapseElements) == PostProcessingOptions.CollapseElements;
            var collapseAttributes = (processingOptions & PostProcessingOptions.CollapseAttributes) == PostProcessingOptions.CollapseAttributes;
            var format = (processingOptions & PostProcessingOptions.Format) == PostProcessingOptions.Format;

            // Create new document to perform post processing.
            var objSource = new XmlDocument();
            objSource.LoadXml(node.OuterXml);

            RemoveLookupIds(objSource, namespaceManager);

            if (removeAnnotations)
                RemoveAnnotations(objSource, namespaceManager);

            if (collapseElements)
                CollapseElements(objSource, namespaceManager, "//xs:element|//xs:any");

            if (collapseAttributes)
                CollapseElements(objSource, namespaceManager, "//xs:attribute|//xs:anyAttribute");

            return format
                    ? FormatXml(objSource)
                    : objSource.OuterXml;
        }

        private static void RemoveLookupIds(XmlDocument objSource, XmlNamespaceManager namespaceManager)
        {
            // Remove lookup ids.
            var lookupIdNodes = objSource.SelectNodes("//@temp:lookupId", namespaceManager);
            Debug.Assert(lookupIdNodes != null);
            foreach (XmlAttribute lookupIdNode in lookupIdNodes)
            {
                var ownerElement = lookupIdNode.OwnerElement;
                Debug.Assert(ownerElement != null);
                ownerElement.Attributes.RemoveNamedItem("lookupId", TempLookupIdNamespace);
            }

            objSource.LoadXml(objSource.OuterXml);

            // Remove namespace binding to our temp lookup id namespace.
            Debug.Assert(objSource.DocumentElement != null);
            objSource.DocumentElement.RemoveAttribute("xmlns:temp");
        }

        private static void RemoveAnnotations(XmlNode document, XmlNamespaceManager namespaceManager)
        {
            var annotations = document.SelectNodes("//xs:annotation", namespaceManager);
            if (annotations != null)
            {
                foreach (XmlNode annotation in annotations)
                    annotation.ParentNode.RemoveChild(annotation);
            }
        }

        private static void CollapseElements(XmlNode document, XmlNamespaceManager namespaceManager, string xpath)
        {
            var rootNode = document.ChildNodes[0];
            var nodes = document.SelectNodes(xpath, namespaceManager);
            if (nodes != null)
            {
                foreach (XmlElement node in nodes)
                {
                    if (node != rootNode)
                        node.IsEmpty = true;
                }
            }
        }

        private static string FormatXml(XmlDocument document)
        {
            // Now we write it the document to a string, but we make sure the 
            // XML declaration is omitted, as this XML only represents a fragment.
            using (var stringWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                                                                      {
                                                                          OmitXmlDeclaration = true,
                                                                          Indent = true
                                                                      }))
                {
                    Debug.Assert(xmlWriter != null);
                    document.Save(xmlWriter);
                }

                return stringWriter.ToString();
            }
        }
    }
}