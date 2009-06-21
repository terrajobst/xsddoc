using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace XsdDocumentation.PlugIn
{
	internal sealed class XsdPlugInConfiguration
	{
		private XsdPlugInConfiguration(XsdPlugInConfiguration other)
		{
			BasePathProvider = other.BasePathProvider;
			DocumentRootSchemas = other.DocumentRootSchemas;
			DocumentRootElements = other.DocumentRootElements;
			DocumentConstraints = other.DocumentConstraints;
			DocumentSchemas = other.DocumentSchemas;
			DocumentSyntax = other.DocumentSyntax;
			SchemaSetContainer = other.SchemaSetContainer;
			SchemaSetTitle = other.SchemaSetTitle;
			NamespaceContainer = other.NamespaceContainer;
			SortOrder = other.SortOrder;
			IncludeLinkUriInKeywordK = other.IncludeLinkUriInKeywordK;
			AnnotationTransformFilePath = (FilePath) other.AnnotationTransformFilePath.Clone();
			SchemaFilePaths = other.SchemaFilePaths.Clone();
			SchemaDependencyFilePaths = other.SchemaDependencyFilePaths.Clone();
			DocFilePaths = other.DocFilePaths.Clone();
		}

		private XsdPlugInConfiguration(IBasePathProvider basePathProvider, XPathNavigator navigator)
		{
			BasePathProvider = basePathProvider;
			DocumentRootSchemas = GetBoolean(navigator, "configuration/document/@rootSchemas", true);
			DocumentRootElements = GetBoolean(navigator, "configuration/document/@rootElements", true);
			DocumentConstraints = GetBoolean(navigator, "configuration/document/@constraints", true);
			DocumentSchemas = GetBoolean(navigator, "configuration/document/@schemas", true);
			DocumentSyntax = GetBoolean(navigator, "configuration/document/@syntax", true);
			SchemaSetContainer = GetBoolean(navigator, "configuration/schemaSet/@container", false);
			SchemaSetTitle = GetString(navigator, "configuration/schemaSet/@title", string.Empty);
			NamespaceContainer = GetBoolean(navigator, "configuration/namespace/@container", true);
			SortOrder = GetInt32(navigator, "configuration/sortOrder", 1);
			IncludeLinkUriInKeywordK = GetBoolean(navigator, "configuration/includeLinkUriInKeywordK", false);
			AnnotationTransformFilePath = GetFilePath(basePathProvider, navigator, "configuration/annotationTransformFile/@path");
			SchemaFilePaths = GetFilePaths(basePathProvider, navigator, "configuration/schemaFiles/schemaFile/@path");
			SchemaDependencyFilePaths = GetFilePaths(basePathProvider, navigator, "configuration/dependencyFiles/schemaFile/@path");
			DocFilePaths = GetFilePaths(basePathProvider, navigator, "configuration/docFiles/docFile/@path");
		}

		[Browsable(false)]
		public IBasePathProvider BasePathProvider { get; private set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionDocumentRootSchemas")]
		[DefaultValue(true)]
		public bool DocumentRootSchemas { get; set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionDocumentRootElements")]
		[DefaultValue(true)]
		public bool DocumentRootElements { get; set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionDocumentConstraints")]
		[DefaultValue(true)]
		public bool DocumentConstraints { get; set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionDocumentSchemas")]
		[DefaultValue(true)]
		public bool DocumentSchemas { get; set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionDocumentSyntax")]
		[DefaultValue(true)]
		public bool DocumentSyntax { get; set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionSchemaSetContainer")]
		[DefaultValue(false)]
		public bool SchemaSetContainer { get; set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionSchemaSetTitle")]
		[DefaultValue("")]
		public string SchemaSetTitle { get; set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionNamespaceContainer")]
		[DefaultValue(true)]
		public bool NamespaceContainer { get; set; }

		[LocalizableCategory("ConfigCategoryAppearance")]
		[LocalizableDescription("ConfigDescriptionSortOrder")]
		[DefaultValue(1)]
		public int SortOrder { get; set; }

		[LocalizableCategory("ConfigCategoryIndex")]
		[LocalizableDescription("ConfigDescriptionIncludeLinkUriInKeywordK")]
		[DefaultValue(false)]
		public bool IncludeLinkUriInKeywordK { get; set; }

		[Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor))]
		[LocalizableCategory("ConfigCategoryFiles")]
		[LocalizableDescription("ConfigDescriptionAnnotationTransformFilePath")]
		public FilePath AnnotationTransformFilePath { get; set; }

		[Editor(typeof(SchemaFilePathsEditor), typeof(UITypeEditor))]
		[LocalizableCategory("ConfigCategoryFiles")]
		[LocalizableDescription("ConfigDescriptionSchemaFilePaths")]
		public FilePathCollection SchemaFilePaths { get; private set; }

		[Editor(typeof(SchemaDependencyFilePathsEditor), typeof(UITypeEditor))]
		[LocalizableCategory("ConfigCategoryFiles")]
		[LocalizableDescription("ConfigDescriptionSchemaDependencyFilePaths")]
		public FilePathCollection SchemaDependencyFilePaths { get; private set; }

		[Editor(typeof(DocFilePathsEditor), typeof(UITypeEditor))]
		[LocalizableCategory("ConfigCategoryFiles")]
		[LocalizableDescription("ConfigDescriptionDocFilePaths")]
		public FilePathCollection DocFilePaths { get; private set; }

		[Browsable(false)]
		private bool ShouldSerializeAnnotationTransformFilePath()
		{
			return !string.IsNullOrEmpty(AnnotationTransformFilePath.ExpandedPath);
		}

		[Browsable(false)]
		private bool ShouldSerializeSchemaFilePaths()
		{
			return SchemaFilePaths.Count > 0;
		}

		[Browsable(false)]
		private bool ShouldSerializeSchemaDependencyFilePaths()
		{
			return SchemaDependencyFilePaths.Count > 0;
		}

		[Browsable(false)]
		private bool ShouldSerializeDocFilePaths()
		{
			return DocFilePaths.Count > 0;
		}

		public static XsdPlugInConfiguration FromXml(IBasePathProvider basePathProvider, XPathNavigator configuration)
		{
			return new XsdPlugInConfiguration(basePathProvider, configuration);
		}

		public static XsdPlugInConfiguration FromXml(IBasePathProvider basePathProvider, string configuration)
		{
			var stringReader = new StringReader(configuration);
			var document = new XPathDocument(stringReader);
			var navigator = document.CreateNavigator();
			return FromXml(basePathProvider, navigator);
		}

		private static bool GetBoolean(XPathNavigator navigator, string xpath, bool defaultValue)
		{
			var value = navigator.SelectSingleNode(xpath);
			return (value == null)
					? defaultValue
					: value.ValueAsBoolean;
		}

		private static int GetInt32(XPathNavigator navigator, string xpath, int defaultValue)
		{
			var value = navigator.SelectSingleNode(xpath);
			return (value == null)
					? defaultValue
					: value.ValueAsInt;
		}

		private static string GetString(XPathNavigator navigator, string xpath, string defaultValue)
		{
			var value = navigator.SelectSingleNode(xpath);
			return (value == null)
					? defaultValue
					: value.Value;
		}

		private static FilePath GetFilePath(IBasePathProvider basePathProvider, XPathNavigator navigator, string xpath)
		{
			var path = navigator.SelectSingleNode(xpath);
			if (path == null)
				return new FilePath(string.Empty, basePathProvider);

			return new FilePath(path.Value, basePathProvider);
		}

		private static FilePathCollection GetFilePaths(IBasePathProvider basePathProvider, XPathNavigator navigator, string xpath)
		{
			var schemaFilePaths = new FilePathCollection();
			var schemaFilePathsNodeList = navigator.Select(xpath);
			foreach (XPathNavigator pathAttribute in schemaFilePathsNodeList)
			{
				var filePath = new FilePath(pathAttribute.Value, basePathProvider);
				schemaFilePaths.Add(filePath);
			}
			return schemaFilePaths;
		}

		public static string ToXml(XsdPlugInConfiguration configuration)
		{
			var doc = new XmlDocument();
			var configurationNode = doc.CreateElement("configuration");
			doc.AppendChild(configurationNode);

			var documentNode = doc.CreateElement("document");
			documentNode.SetAttribute("rootSchemas", XmlConvert.ToString(configuration.DocumentRootSchemas));
			documentNode.SetAttribute("rootElements", XmlConvert.ToString(configuration.DocumentRootElements));
			documentNode.SetAttribute("constraints", XmlConvert.ToString(configuration.DocumentConstraints));
			documentNode.SetAttribute("schemas", XmlConvert.ToString(configuration.DocumentSchemas));
			documentNode.SetAttribute("syntax", XmlConvert.ToString(configuration.DocumentSyntax));
			configurationNode.AppendChild(documentNode);

			var schemaSetNode = doc.CreateElement("schemaSet");
			schemaSetNode.SetAttribute("container", XmlConvert.ToString(configuration.SchemaSetContainer));
			schemaSetNode.SetAttribute("title", configuration.SchemaSetTitle);
			configurationNode.AppendChild(schemaSetNode);

			var namespaceNode = doc.CreateElement("namespace");
			namespaceNode.SetAttribute("container", XmlConvert.ToString(configuration.NamespaceContainer));
			configurationNode.AppendChild(namespaceNode);

			var sortOrderNode = doc.CreateElement("sortOrder");
			sortOrderNode.InnerText = XmlConvert.ToString(configuration.SortOrder);
			configurationNode.AppendChild(sortOrderNode);

			var includeLinkUriInKeywordKNode = doc.CreateElement("includeLinkUriInKeywordK");
			includeLinkUriInKeywordKNode.InnerText = XmlConvert.ToString(configuration.IncludeLinkUriInKeywordK);
			configurationNode.AppendChild(includeLinkUriInKeywordKNode);

			var annotationTransformFileNode = doc.CreateElement("annotationTransformFile");
			annotationTransformFileNode.SetAttribute("path", configuration.AnnotationTransformFilePath.PersistablePath);
			configurationNode.AppendChild(annotationTransformFileNode);

			var schemaFilesNode = doc.CreateElement("schemaFiles");
			configurationNode.AppendChild(schemaFilesNode);

			foreach (var filePath in configuration.SchemaFilePaths)
			{
				var schemaFileNode = doc.CreateElement("schemaFile");
				schemaFileNode.SetAttribute("path", filePath.PersistablePath);
				schemaFilesNode.AppendChild(schemaFileNode);
			}

			var dependencyFilesNode = doc.CreateElement("dependencyFiles");
			configurationNode.AppendChild(dependencyFilesNode);

			foreach (var filePath in configuration.SchemaDependencyFilePaths)
			{
				var schemaFileNode = doc.CreateElement("schemaFile");
				schemaFileNode.SetAttribute("path", filePath.PersistablePath);
				dependencyFilesNode.AppendChild(schemaFileNode);
			}

			var docFilesNode = doc.CreateElement("docFiles");
			configurationNode.AppendChild(docFilesNode);

			foreach (var filePath in configuration.DocFilePaths)
			{
				var docFileNode = doc.CreateElement("docFile");
				docFileNode.SetAttribute("path", filePath.PersistablePath);
				docFilesNode.AppendChild(docFileNode);
			}

			return doc.OuterXml;
		}

		public XsdPlugInConfiguration Clone()
		{
			return new XsdPlugInConfiguration(this);
		}
	}
}