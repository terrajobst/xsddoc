using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace XsdDocumentation.Model
{
	internal sealed class DocumentationManager : Manager
	{
		private XmlDocument _documentationOwner = new XmlDocument();
		private Dictionary<object, DocumentationInfo> _documentationInfos = new Dictionary<object, DocumentationInfo>();
		private XslCompiledTransform _compiledTransform;

		public DocumentationManager(Context context)
			: base(context)
		{
		}

		public override void Initialize()
		{
			_compiledTransform = new XslCompiledTransform();
			var path = Assembly.GetExecutingAssembly().Location;
			Debug.Assert(path != null);
			var transformPath = string.IsNullOrEmpty(Context.Configuration.AnnotationTransformFileName)
			                    	? GetDefaultTransform(path)
			                    	: Context.Configuration.AnnotationTransformFileName;
			using (var reader = new XmlTextReader(transformPath))
				_compiledTransform.Load(reader);

			var documentatbleObjects = GetDocumentatbleObjects(Context.SchemaSetManager.SchemaSet);
			LoadEmbeddedDocumentation(documentatbleObjects);
			LoadExternalDocumentation();
		}

		private static string GetDefaultTransform(string path)
		{
			return Path.Combine(Path.GetDirectoryName(path), "AnnotationTranform.xslt");
		}

		private static HashSet<XmlSchemaAnnotated> GetDocumentatbleObjects(XmlSchemaSet schemaSet)
		{
			var documentatbleObjects = new HashSet<XmlSchemaAnnotated>();
			var documentatbleObjectFinder = new DocumentatbleObjectFinder(documentatbleObjects);
			documentatbleObjectFinder.Traverse(schemaSet);
			return documentatbleObjects;
		}

		private static XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
		{
			var namespaceManager = new XmlNamespaceManager(doc.NameTable);
			namespaceManager.AddNamespace("xsd", Namespaces.XsdDoc);
			namespaceManager.AddNamespace("ddue", Namespaces.Maml);
			return namespaceManager;
		}

		private DocumentationInfo GetOrCreateDocumentationInfo(object key)
		{
			DocumentationInfo documentationInfo;
			if (!_documentationInfos.TryGetValue(key, out documentationInfo))
			{
				documentationInfo = new DocumentationInfo();
				_documentationInfos.Add(key, documentationInfo);
			}
			return documentationInfo;
		}

		private void LoadEmbeddedDocumentation(IEnumerable<XmlSchemaAnnotated> documentatbleObjects)
		{
			var namespaceManager = GetNamespaceManager(_documentationOwner);

			foreach (var schema in Context.SchemaSetManager.SchemaSet.GetAllSchemas())
				AddEmbeddedDocumenation(namespaceManager, schema);

			foreach (var documentatbleObject in documentatbleObjects)
				AddEmbeddedDocumenation(namespaceManager, documentatbleObject);
		}

		private void AddEmbeddedDocumenation(XmlNamespaceManager namespaceManager, XmlSchemaObject documentatbleObject)
		{
			string sourceCode = Context.SourceCodeManager.GetSourceCode(documentatbleObject);
			if (string.IsNullOrEmpty(sourceCode))
				return;

			using (var stringReader = new StringReader(sourceCode))
			using (var xmlReader = new XmlTextReader(stringReader))
			using (var stringWriter = new StringWriter())
			using (var xmlTextWriter = new XmlTextWriter(stringWriter))
			{
				var argumentList = GetXsltArgumentList(documentatbleObject);
				_compiledTransform.Transform(xmlReader, argumentList, xmlTextWriter);
				var doc = new XmlDocument();
				try
				{
					doc.LoadXml(stringWriter.ToString());
				}
				catch (Exception)
				{
					return;
				}
				var documentationInfo = GetOrCreateDocumentationInfo(documentatbleObject);
				var schemaDocElement = doc.SelectSingleNode("xsd:schemaDoc", namespaceManager);
				InitializeDocumentationInfo(documentatbleObject, documentationInfo, schemaDocElement, namespaceManager);
			}
		}

		private XsltArgumentList GetXsltArgumentList(XmlSchemaObject documentatbleObject)
		{
			var parentObject = documentatbleObject.Parent;
			while (parentObject != null && Context.TopicManager.GetTopic(parentObject) == null)
				parentObject = parentObject.Parent;

			var argumentList = new XsltArgumentList();
			FillXsltArgumentList(argumentList, "parent", parentObject);
			FillXsltArgumentList(argumentList, "current", documentatbleObject);
			return argumentList;
		}

		private void FillXsltArgumentList(XsltArgumentList argumentList, string prefix, XmlSchemaObject obj)
		{
			var topic = (obj == null) ? null : Context.TopicManager.GetTopic(obj);
			var itemType = (obj == null) ? string.Empty : obj.GetType().Name.Substring(9).ToLower();
			var itemNamespace = (obj == null) ? string.Empty : obj.GetSchema().TargetNamespace ?? string.Empty;
			var itemUri = (topic == null) ? string.Empty : topic.LinkUri;

			argumentList.AddParam(prefix + "ItemType", string.Empty, itemType);
			argumentList.AddParam(prefix + "ItemNamespace", string.Empty, itemNamespace);
			argumentList.AddParam(prefix + "ItemUri", string.Empty, itemUri);
		}

		private void LoadExternalDocumentation()
		{
			foreach (var externalDocFileName in Context.Configuration.DocFileNames)
			{
				var doc = new XmlDocument();
				doc.Load(externalDocFileName);

				const string SchemaSetUri = "##SchemaSet";
				const string NamespaceUri = "##Namespace";

				var namespaceManager = GetNamespaceManager(doc);
				var namespaceKey = doc.SelectSingleNode("//xsd:namespace/xsd:name", namespaceManager).InnerText;
				var memberNodes = doc.SelectNodes("//xsd:member", namespaceManager);

				if (memberNodes != null)
				{
					foreach (XmlNode memberNode in memberNodes)
					{
						var partialDocUri = memberNode.Attributes["uri"].Value;

						XmlSchemaObject documentableObject;
						DocumentationInfo documentationInfo;

						if (partialDocUri == NamespaceUri)
						{
							if (namespaceKey == SchemaSetUri)
							{
								// TODO: Report invalid member Uri
								documentableObject = null;
								documentationInfo = null;
							}
							else
							{
								documentableObject = null;
								documentationInfo = GetOrCreateDocumentationInfo(namespaceKey);
							}
						}
						else if (partialDocUri == SchemaSetUri)
						{
							documentableObject = null;
							documentationInfo = GetOrCreateDocumentationInfo(Context.SchemaSetManager.SchemaSet);
						}
						else
						{
							var docUri = (namespaceKey == SchemaSetUri)
							             	? partialDocUri
							             	: namespaceKey + "#" + partialDocUri;
							var topic = Context.TopicManager.GetTopicByUri(docUri);

							if (topic == null)
							{
								documentableObject = null;
								documentationInfo = null;
							}
							else
							{
								documentableObject = topic.SchemaObject;
								documentationInfo = (topic.TopicType == TopicType.Namespace)
								                    	? GetOrCreateDocumentationInfo(topic.Namespace)
								                    	: GetOrCreateDocumentationInfo(topic.SchemaObject);
							}
						}

						if (documentationInfo != null)
							InitializeDocumentationInfo(documentableObject, documentationInfo, memberNode, namespaceManager);
					}
				}
			}
		}

		private void InitializeDocumentationInfo(XmlSchemaObject documentatbleObject, DocumentationInfo documentationInfo, XmlNode schemaDocElement, XmlNamespaceManager namespaceManager)
		{
			documentationInfo.SummaryNode = schemaDocElement.SelectSingleNode("ddue:summary", namespaceManager);
			documentationInfo.RemarksNode = schemaDocElement.SelectSingleNode("ddue:remarks", namespaceManager);
			documentationInfo.RelatedTopicsNode = schemaDocElement.SelectSingleNode("ddue:relatedTopics", namespaceManager);

			// schemaset and namespaces do not have an XmlSchemaObject counterpart.
			if (documentatbleObject != null)
			{
				var obsoleteNode = schemaDocElement.SelectSingleNode("xsd:obsolete", namespaceManager);
				if (obsoleteNode != null)
				{
					documentationInfo.IsObsolete = true;

					var uriAttribute = obsoleteNode.Attributes["uri"];
					if (uriAttribute != null)
					{
						var uri = uriAttribute.Value;
						var nonObsoleteTopic = Context.TopicManager.GetTopicByUri(uri);

						if (nonObsoleteTopic != null)
                            documentationInfo.NonObsoleteAlternative = nonObsoleteTopic.SchemaObject;
						else
						{
							// TODO: Report invalid obsolete URI.
						}
					}
				}

				var parentNodes = schemaDocElement.SelectNodes("xsd:parent", namespaceManager);
				if (parentNodes != null)
				{
					foreach (XmlNode parentNode in parentNodes)
					{
						var uri = parentNode.Attributes["uri"].Value;
						var parentTopic = Context.TopicManager.GetTopicByUri(uri);
						if (parentTopic != null)
						{
							if (parentTopic.SchemaObject != null)
								Context.SchemaSetManager.RegisterExtension(parentTopic.SchemaObject, documentatbleObject);
						}
						else
						{
							// TODO: Report invalid parent URI.
						}
					}
				}
			}
		}

		private DocumentationInfo InternalGetDocumentationInfo(object key)
		{
			DocumentationInfo documentationInfo;
			_documentationInfos.TryGetValue(key, out documentationInfo);
			return documentationInfo;
		}

		public DocumentationInfo GetSchemaSetDocumentationInfo()
		{
			return InternalGetDocumentationInfo(Context.SchemaSetManager.SchemaSet);
		}

		public DocumentationInfo GetNamespaceDocumentationInfo(string targetNamespace)
		{
			return InternalGetDocumentationInfo(targetNamespace ?? string.Empty);
		}

		public DocumentationInfo GetObjectDocumentationInfo(XmlSchemaObject obj)
		{
			var documentationInfo = InternalGetDocumentationInfo(obj);
			if (documentationInfo != null)
				return documentationInfo;

			XmlSchemaElement element;
			XmlSchemaAttribute attribute;

			if (Casting.TryCast(obj, out element) && !element.RefName.IsEmpty)
				return InternalGetDocumentationInfo(Context.SchemaSetManager.SchemaSet.GlobalElements[element.RefName]);

			if (Casting.TryCast(obj, out attribute) && !attribute.RefName.IsEmpty)
				return InternalGetDocumentationInfo(Context.SchemaSetManager.SchemaSet.GlobalAttributes[attribute.RefName]);

			return null;
		}
	}
}