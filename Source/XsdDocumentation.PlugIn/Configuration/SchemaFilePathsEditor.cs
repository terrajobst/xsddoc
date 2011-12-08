using System;

using XsdDocumentation.PlugIn.Properties;

namespace XsdDocumentation.PlugIn
{
    internal sealed class SchemaFilePathsEditor : FilePathsEditor
    {
        public override string Title
        {
            get { return Resources.SchemaFilePathsEditorTitle; }
        }

        public override string Filter
        {
            get { return Resources.SchemaFilePathsEditorFilter; }
        }

        public override string HelpKeyword
        {
            get { return HelpTopics.ConfigureSchemaFiles; }
        }
    }
}