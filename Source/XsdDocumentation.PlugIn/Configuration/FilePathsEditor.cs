using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace XsdDocumentation.PlugIn
{
    internal abstract class FilePathsEditor : UITypeEditor
    {
        public abstract string Title { get; }
        public abstract string Filter { get; }
        public abstract string HelpKeyword { get; }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var configuration = (XsdPlugInConfiguration)context.Instance;
            var schemaFilePaths = (FilePathCollection)value;

            using (var dlg = new FilePathListEditorForm())
            {
                dlg.Text = Title;
                dlg.HelpKeyword = HelpKeyword;
                dlg.BasePathProvider = configuration.BasePathProvider;
                dlg.FilePathsList = schemaFilePaths;
                dlg.Filter = Filter;
                dlg.ShowDialog();
            }

            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}