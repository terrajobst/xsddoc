using System;

using XsdDocumentation.PlugIn.Properties;

namespace XsdDocumentation.PlugIn
{
    internal sealed class SchemaDependencyFilePathsEditor : FilePathsEditor
    {
        public override string Title
        {
            get { return Resources.SchemaDependencyFilePathsEditorTitle; }
        }
        public override string Filter
        {
            get { return Resources.SchemaDependencyFilePathsEditorFilter; }
        }
        public override string HelpKeyword
        {
            get { return HelpTopics.ConfigureSchemaDependencyFiles; }
        }
    }
}