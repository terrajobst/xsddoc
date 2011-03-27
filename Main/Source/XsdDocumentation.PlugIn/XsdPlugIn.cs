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
using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.ConceptualContent;
using SandcastleBuilder.Utils.PlugIn;

using XsdDocumentation.Model;
using XsdDocumentation.PlugIn.Properties;

using System.Linq;

namespace XsdDocumentation.PlugIn
{
    public sealed class XsdPlugIn : IPlugIn
    {
        private ExecutionPointCollection _executionPoints;
        private BuildProcess _buildProcess;
        private XsdPlugInConfiguration _configuration;
        //private ContentGenerator _contentGenerator;

        public static string GetHelpFilePath()
        {
            var asm = Assembly.GetExecutingAssembly();
            Debug.Assert(asm.Location != null);
            return Path.Combine(Path.GetDirectoryName(asm.Location), "Help.chm");
        }

        /// <summary>
        /// This read-only property returns a friendly name for the plug-in
        /// </summary>
        public string Name
        {
            get { return Resources.PlugInName; }
        }

        /// <summary>
        /// This read-only property returns the version of the plug-in
        /// </summary>
        public Version Version
        {
            get
            {
                // Use the assembly version
                var asm = Assembly.GetExecutingAssembly();
                Debug.Assert(asm.Location != null);
                var fvi = FileVersionInfo.GetVersionInfo(asm.Location);

                return new Version(fvi.ProductVersion);
            }
        }

        /// <summary>
        /// This read-only property returns the copyright information for the
        /// plug-in.
        /// </summary>
        public string Copyright
        {
            get
            {
                // Use the assembly copyright
                var asm = Assembly.GetExecutingAssembly();
                var copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute));
                return copyright.Copyright;
            }
        }

        /// <summary>
        /// This read-only property returns a brief description of the plug-in
        /// </summary>
        public string Description
        {
            get { return Resources.PlugInDescription; }
        }

        /// <summary>
        /// This read-only property returns true if the plug-in should run in
        /// a partial build or false if it should not.
        /// </summary>
        /// <value>If this returns false, the plug-in will not be loaded when
        /// a partial build is performed.</value>
        public bool RunsInPartialBuild
        {
            get { return false; }
        }

        /// <summary>
        /// This read-only property returns a collection of execution points
        /// that define when the plug-in should be invoked during the build
        /// process.
        /// </summary>
        public ExecutionPointCollection ExecutionPoints
        {
            get
            {
                return _executionPoints ?? (_executionPoints = new ExecutionPointCollection
                                                                   {
                                                                       new ExecutionPoint(BuildStep.FindingTools,
                                                                                          ExecutionBehaviors.After),

                                                                       //new ExecutionPoint(BuildStep.Initializing, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.ClearWorkFolder, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.FindingTools, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CopyConceptualContent, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateSharedContent, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateApiFilter, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateReflectionInfo, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateNamespaceSummaries, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.ApplyVisibilityProperties, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateInheritedDocumentation, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.TransformReflectionInfo, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.ModifyHelpTopicFilenames, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CopyStandardContent, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CopyConceptualContent, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CreateConceptualTopicConfigs, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CopyAdditionalContent, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.MergeTablesOfContents, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateIntermediateTableOfContents, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CreateBuildAssemblerConfigs, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.MergeCustomConfigs, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.BuildConceptualTopics, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.BuildReferenceTopics, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CombiningIntermediateTocFiles, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.ExtractingHtmlInfo, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateHelpFormatTableOfContents, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateHelpFileIndex, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateHelpProject, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CompilingHelpFile, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.GenerateFullTextIndex, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CopyingWebsiteFiles, ExecutionBehaviors.Before),
                                                                       //new ExecutionPoint(BuildStep.CleanIntermediates, ExecutionBehaviors.Before)
                                                                   });
            }
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
            _buildProcess.ReportProgress(Resources.PlugInVersionFormatted, Name, Version, Copyright);
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="executionContext">The current execution context</param>
        public void Execute(ExecutionContext executionContext)
        {
            //if (executionContext.BuildStep == BuildStep.CopyConceptualContent)
            //{
            //    _buildProcess.ReportProgress("Adding XSD topic files...");

            //    var myContentFile = _buildProcess.ConceptualContent.ContentLayoutFiles.Last();
            //    _buildProcess.ReportProgress("XSD: Using content file " + myContentFile.FullPath);

            //    var collection = new TopicCollection(myContentFile);
            //    foreach (var fileName in contentGenerator.TopicFiles)
            //    {
            //        var fileItem = _buildProcess.CurrentProject.AddFileToProject(fileName, fileName);
            //        var topicFile = new TopicFile(fileItem);
            //        var topic = new Topic { TopicFile = topicFile };
            //        collection.Add(topic);
            //    }

            //    return;
            //}

            //_buildProcess.ReportProgress("****** XSD DOC");
            //_buildProcess.ReportProgress(executionContext.BuildStep.ToString());
            //_buildProcess.ReportProgress("_buildProcess.ConceptualContent == null --- " + (_buildProcess.ConceptualContent == null));
            //_buildProcess.ReportProgress("****** XSD DOC");
            //return;

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

            //var contentLayoutFile = _buildProcess.CurrentProject.AddFileToProject(contentGenerator.ContentFile, contentGenerator.ContentFile);
            //contentLayoutFile.SortOrder = _configuration.SortOrder;
            //_buildProcess.ConceptualContent.ContentLayoutFiles.Add(contentLayoutFile);

            foreach (var topicFileName in contentGenerator.TopicFiles)
                AddLinkedItem(BuildAction.None, topicFileName);

            //foreach (var topicFile in _contentGenerator.TopicFiles)
            //{
            //    var file = _buildProcess.CurrentProject.AddFileToProject(topicFile, topicFile);
            //    var topicCollection = new TopicCollection(file);
            //    _buildProcess.ConceptualContent.Topics.Add(topicCollection);
            //}

            foreach (var mediaItem in contentGenerator.MediaItems)
            {
                var mediaFileItem = AddLinkedItem(BuildAction.Image, mediaItem.FileName);
                mediaFileItem.SetMetadataValue("ImageId", mediaItem.ArtItem.Id);
                mediaFileItem.SetMetadataValue("AlternateText", mediaItem.ArtItem.AlternateText);
            }

            //foreach (var mediaItem in contentGenerator.MediaItems)
            //{
            //    var file = _buildProcess.CurrentProject.AddFileToProject(mediaItem.FileName, mediaItem.FileName);
            //    var imageReference = new ImageReference(file)
            //                             {
            //                                 Id = mediaItem.ArtItem.Id,
            //                                 AlternateText = mediaItem.ArtItem.AlternateText
            //                             };
            //    _buildProcess.ConceptualContent.ImageFiles.Add(imageReference);
            //}

            var componentConfig = GetComponentConfiguration(contentGenerator.IndexFile);
            _buildProcess.CurrentProject.ComponentConfigurations.Add(GetComponentId(), true, componentConfig);

            //_buildProcess.CurrentProject.MSBuildProject.Save(@"P:\Test.proj");
            _buildProcess.CurrentProject.MSBuildProject.ReevaluateIfNecessary();
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
            var project = _buildProcess.CurrentProject.MSBuildProject;
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
            const string name = @"XsdDocumentation.BuildComponents.XsdResolveLinksComponent";
            const string componentDllName = "XsdDocumentation.BuildComponents.dll";
            var plugInDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var componentPath = Path.Combine(plugInDirectoryPath, componentDllName);
            var componentConfig = string.Format(@"<component id=""{0}"" type=""{1}"" assembly=""{2}""><indexFile location=""{3}"" /></component>", id, name, componentPath, indexFileName);
            return componentConfig;
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of
        /// the plug-in object.
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
        }
    }
}