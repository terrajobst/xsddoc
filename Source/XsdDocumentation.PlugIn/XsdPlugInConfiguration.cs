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
			RootDocumentation = other.RootDocumentation;
			SchemaSetContainer = other.SchemaSetContainer;
			SchemaSetTitle = other.SchemaSetTitle;
			SortOrder = other.SortOrder;
			AnnotationTransformFilePath = (FilePath) other.AnnotationTransformFilePath.Clone();
			SchemaFilePaths = other.SchemaFilePaths.Clone();
			SchemaDependencyFilePaths = other.SchemaDependencyFilePaths.Clone();
			DocFilePaths = other.DocFilePaths.Clone();
		}

		private XsdPlugInConfiguration(IBasePathProvider basePathProvider, XPathNavigator navigator)
		{
			BasePathProvider = basePathProvider;
			RootDocumentation = GetBoolean(navigator, "configuration/roots/@document", true);
			SchemaSetContainer = GetBoolean(navigator, "configuration/schemaSet/@container", false);
			SchemaSetTitle = GetString(navigator, "configuration/schemaSet/@title", string.Empty);
			SortOrder = GetInt32(navigator, "configuration/sortOrder", 1);
			AnnotationTransformFilePath = GetFilePath(basePathProvider, navigator, "configuration/annotationTransformFile/@path");
			SchemaFilePaths = GetFilePaths(basePathProvider, navigator, "configuration/schemaFiles/schemaFile/@path");
			SchemaDependencyFilePaths = GetFilePaths(basePathProvider, navigator, "configuration/dependencyFiles/schemaFile/@path");
			DocFilePaths = GetFilePaths(basePathProvider, navigator, "configuration/docFiles/docFile/@path");
		}

		[Browsable(false)]
		public IBasePathProvider BasePathProvider { get; private set; }

		[Category("Appearance")]
		[Description("If true, the root schemas and root elements get their own entry in the table of contents.")]
		[DefaultValue(true)]
		public bool RootDocumentation { get; set; }

		[Category("Appearance")]
		[Description("If true, a root \"Schema Set\" table of content entry will be created as the container of the namespaces in the documented schema set. If false, the default, the namespaces are listed in the table of content as root entries.")]
		[DefaultValue(false)]
		public bool SchemaSetContainer { get; set; }

		[Category("Appearance")]
		[Description("An alternate title for the \"Schema Set\" page and the root table of content container.")]
		[DefaultValue("")]
		public string SchemaSetTitle { get; set; }

		[Category("Appearance")]
		[Description("This defines the sort order for merging the XML schema topics with the main help file.")]
		[DefaultValue(1)]
		public int SortOrder { get; set; }

		[Editor(typeof(FilePathObjectEditor), typeof(UITypeEditor))]
		[Category("Files")]
		[Description("Specifies an XSLT transformation file that is used to translate inline schema documentation into the schemaDoc element.")]
		public FilePath AnnotationTransformFilePath { get; set; }

		[Editor(typeof(SchemaFilePathsEditor), typeof(UITypeEditor))]
		[Category("Files")]
		[Description("Specifies the schema files that are part of the schema set.")]
		public FilePathCollection SchemaFilePaths { get; private set; }

		[Editor(typeof(SchemaDependencyFilePathsEditor), typeof(UITypeEditor))]
		[Category("Files")]
		[Description("Specifies the schema files the schema set will depend on. They will be added to the internal schema set in order to be able to process it but the schemas fill not being documented.")]
		public FilePathCollection SchemaDependencyFilePaths { get; private set; }

		[Editor(typeof(DocFilePathsEditor), typeof(UITypeEditor))]
		[Category("Files")]
		[Description("Specifies additional documentation files to be used.")]
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

			var rootsNode = doc.CreateElement("roots");
			rootsNode.SetAttribute("document", XmlConvert.ToString(configuration.RootDocumentation));
			configurationNode.AppendChild(rootsNode);

			var schemaSetNode = doc.CreateElement("schemaSet");
			schemaSetNode.SetAttribute("container", XmlConvert.ToString(configuration.SchemaSetContainer));
			schemaSetNode.SetAttribute("title", configuration.SchemaSetTitle);
			configurationNode.AppendChild(schemaSetNode);

			var sortOrderNode = doc.CreateElement("sortOrder");
			sortOrderNode.InnerText = XmlConvert.ToString(configuration.SortOrder);
			configurationNode.AppendChild(sortOrderNode);

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