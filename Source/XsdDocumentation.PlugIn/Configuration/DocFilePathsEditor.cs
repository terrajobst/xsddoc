using System;

using XsdDocumentation.PlugIn.Properties;

namespace XsdDocumentation.PlugIn
{
    internal sealed class DocFilePathsEditor : FilePathsEditor
    {
        public override string Title
        {
            get { return Resources.DocFilePathsEditorTitle; }
        }

        public override string Filter
        {
            get { return Resources.DocFilePathsEditorFilter; }
        }

        public override string HelpKeyword
        {
            get { return HelpTopics.ConfigureDocFiles; }
        }
    }
}