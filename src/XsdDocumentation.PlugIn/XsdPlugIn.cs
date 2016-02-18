using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.XPath;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

using XsdDocumentation.Model;
using XsdDocumentation.PlugIn.Properties;

namespace XsdDocumentation.PlugIn
{
    using SandcastleBuilder.Utils.ConceptualContent;

    [HelpFileBuilderPlugInExport(
        "XML Schema Documenter",
        Copyright = XsdDocMetadata.Copyright,
        Description = "This plug-in creates reference documentation for an XML schema set.",
        IsConfigurable = true,
        RunsInPartialBuild = false,
        Version = XsdDocMetadata.Version)]
    public sealed class XsdPlugIn : IPlugIn
    {
        private ExecutionPoint[] _executionPoints;
        private BuildProcess _buildProcess;
        private XsdPlugInConfiguration _configuration;

        public static string GetHelpFilePath()
        {
            var asm = Assembly.GetExecutingAssembly();
            Debug.Assert(asm.Location != null);
            return Path.Combine(Path.GetDirectoryName(asm.Location), "Help.chm");
        }

        /// <summary>
        /// This read-only property returns a collection of execution points
        /// that define when the plug-in should be invoked during the build
        /// process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get { return _executionPoints ?? (_executionPoints = new[] {new ExecutionPoint(BuildStep.GenerateSharedContent, ExecutionBehaviors.Before)}); }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the
        /// plug-in perform its own configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file
        /// builder project.</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            var configuration = XsdPlugInConfiguration.FromXml(project, currentConfig);

            using (var dlg = new XsdConfigurationForm(configuration))
            {
                return (dlg.ShowDialog() == DialogResult.OK)
                        ? XsdPlugInConfiguration.ToXml(dlg.NewConfiguration)
                        : currentConfig;
            }
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the
        /// build process.
        /// </summary>
        /// <param name="buildProcess">A reference to the current build
        /// process.</param>
        /// <param name="configuration">The configuration data that the plug-in
        /// should use to initialize itself.</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            _configuration = XsdPlugInConfiguration.FromXml(buildProcess.CurrentProject, configuration);
            _buildProcess = buildProcess;
            _buildProcess.ReportProgress(Resources.PlugInVersionFormatted, Resources.PlugInName, XsdDocMetadata.Version, XsdDocMetadata.Copyright);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="executionContext">The current execution context</param>
        public void Execute(ExecutionContext executionContext)
        {
            _buildProcess.ReportProgress(Resources.PlugInBuildProgress);
            var messageReporter = new MessageReporter(_buildProcess);
            var configuration = new Configuration
                                {
                                    OutputFolderPath = _buildProcess.WorkingFolder,
                                    DocumentRootSchemas = _configuration.DocumentRootSchemas,
                                    DocumentRootElements = _configuration.DocumentRootElements,
                                    DocumentConstraints = _configuration.DocumentConstraints,
                                    DocumentSchemas = _configuration.DocumentSchemas,
                                    DocumentSyntax = _configuration.DocumentSyntax,
                                    UseTypeDocumentationForUndocumentedAttributes = _configuration.UseTypeDocumentationForUndocumentedAttributes,
                                    UseTypeDocumentationForUndocumentedElements = _configuration.UseTypeDocumentationForUndocumentedElements,
                                    SchemaSetContainer = _configuration.SchemaSetContainer,
                                    SchemaSetTitle = _configuration.SchemaSetTitle,
                                    NamespaceContainer = _configuration.NamespaceContainer,
                                    IncludeLinkUriInKeywordK = _configuration.IncludeLinkUriInKeywordK,
                                    AnnotationTransformFileName = _configuration.AnnotationTransformFilePath.ExpandedPath,
                                    SchemaFileNames = ExpandFiles(_configuration.SchemaFilePaths),
                                    SchemaDependencyFileNames = ExpandFiles(_configuration.SchemaDependencyFilePaths),
                                    DocFileNames = ExpandFiles(_configuration.DocFilePaths)
                                };

            var contentGenerator = new ContentGenerator(messageReporter, configuration);
            contentGenerator.Generate();

            var contentFileProfider = new ContentFileProvider(_buildProcess.CurrentProject);

            var contentLayoutFile = contentFileProfider.Add(BuildAction.ContentLayout, contentGenerator.ContentFile);
            contentLayoutFile.SortOrder = _configuration.SortOrder;
            _buildProcess.ConceptualContent.ContentLayoutFiles.Add(contentLayoutFile);

            foreach (var topicFileName in contentGenerator.TopicFiles)
                contentFileProfider.Add(BuildAction.None, topicFileName);

            _buildProcess.ConceptualContent.Topics.Add(new TopicCollection(contentLayoutFile));

            foreach (var mediaItem in contentGenerator.MediaItems)
            {
                var imageFilePath = new FilePath(mediaItem.FileName, _buildProcess.CurrentProject);
                var imageReference = new ImageReference(imageFilePath, mediaItem.ArtItem.Id) { AlternateText = mediaItem.ArtItem.AlternateText };
                _buildProcess.ConceptualContent.ImageFiles.Add(imageReference);
            }

            var componentConfig = GetComponentConfiguration(contentGenerator.IndexFile);

            _buildProcess.CurrentProject.ComponentConfigurations.Add(GetComponentId(), true, componentConfig);
        }

        private static List<string> ExpandFiles(IEnumerable<FilePath> filePaths)
        {
            var result = new List<string>();
            foreach (var path in filePaths)
            {
                if (path.IsFixedPath)
                {
                    result.Add(path.ExpandedPath);
                }
                else
                {
                    var directory = Path.GetDirectoryName(path.ExpandedPath);
                    var pattern = Path.GetFileName(path.ExpandedPath);
                    result.AddRange(Directory.GetFiles(directory, pattern));
                }
            }

            return result;
        }

        private static string GetComponentId()
        {
            return @"XsdResolveLinks";
        }

        private static string GetComponentConfiguration(string indexFileName)
        {
            var id = GetComponentId();
            return string.Format(@"<component id=""{0}""><indexFile location=""{1}"" /></component>", id, indexFileName);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of
        /// the plug-in object.
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
        }

        /// <summary>
        /// This replaces the temporary project file.
        /// </summary>
        private class ContentFileProvider : IContentFileProvider
        {
            private readonly IBasePathProvider basePathProvider;

            private readonly Dictionary<BuildAction, List<ContentFile>> data = new Dictionary<BuildAction, List<ContentFile>>();

            public ContentFileProvider(IBasePathProvider basePathProvider)
            {
                this.basePathProvider = basePathProvider;
            }

            public IEnumerable<ContentFile> ContentFiles(BuildAction buildAction)
            {
                return this.data[buildAction];
            }

            public ContentFile Add(BuildAction buildAction, string fileName)
            {
                List<ContentFile> itemsForAction;
                if (!this.data.TryGetValue(buildAction, out itemsForAction))
                {
                    itemsForAction = new List<ContentFile>();
                    this.data.Add(buildAction, itemsForAction);
                }

                var contenFile = this.CreateContentFile(fileName);

                itemsForAction.Add(contenFile);
                return contenFile;
            }

            private ContentFile CreateContentFile(string fileName)
            {
                var filePath = new FilePath(fileName, this.basePathProvider);
                var contentFile = new ContentFile(filePath);
                contentFile.LinkPath = filePath; // TODO: do we need this?
                contentFile.ContentFileProvider = this;
                return contentFile;
            }
        }
    }
}