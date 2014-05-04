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
        private SandcastleProject _tempProject;

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
            _tempProject = new SandcastleProject(Path.Combine(_buildProcess.WorkingFolder, "XSDTemp.shfbproj"), false);
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

            var contentLayoutItem = AddLinkedItem(BuildAction.ContentLayout, contentGenerator.ContentFile);
            contentLayoutItem.SetMetadataValue("SortOrder", Convert.ToString(_configuration.SortOrder, CultureInfo.InvariantCulture));

            foreach (var topicFileName in contentGenerator.TopicFiles)
                AddLinkedItem(BuildAction.None, topicFileName);

            foreach (var mediaItem in contentGenerator.MediaItems)
            {
                var mediaFileItem = AddLinkedItem(BuildAction.Image, mediaItem.FileName);
                mediaFileItem.SetMetadataValue("ImageId", mediaItem.ArtItem.Id);
                mediaFileItem.SetMetadataValue("AlternateText", mediaItem.ArtItem.AlternateText);
            }

            var componentConfig = GetComponentConfiguration(contentGenerator.IndexFile);

            _buildProcess.CurrentProject.ComponentConfigurations.Add(GetComponentId(), true, componentConfig);

            // Needed so that all links are properly evaluated before processed by SHFB.
            _tempProject.MSBuildProject.ReevaluateIfNecessary();

            // Add the items to the conceptual content settings
            _buildProcess.ConceptualContent.MergeContentFrom(_tempProject);
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

        private ProjectItem AddLinkedItem(BuildAction buildAction, string fileName)
        {
            var project = _tempProject.MSBuildProject;
            var itemName = buildAction.ToString();
            var buildItems = project.AddItem(itemName, fileName, new[] { new KeyValuePair<string, string>("Link", fileName) });
            Debug.Assert(buildItems.Count == 1);
            return buildItems[0];
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
            if(_tempProject != null)
                _tempProject.Dispose();
        }
    }
}