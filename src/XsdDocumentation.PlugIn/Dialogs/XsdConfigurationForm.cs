using System.Linq;
using System.Xml.Linq;
using System.Windows.Forms;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;

namespace XsdDocumentation.PlugIn
{
    internal sealed partial class XsdConfigurationForm : HelpAwareForm
    {
        #region Plug-in configuration editor factory for MEF
        //=====================================================================

        /// <summary>
        /// This allows editing of the plug-in configuration
        /// </summary>
        [PlugInConfigurationEditorExport("XML Schema Documenter")]
        public sealed class Factory : IPlugInConfigurationEditor
        {
            /// <inheritdoc />
            public bool EditConfiguration(SandcastleProject project, XElement configuration)
            {
                var currentConfig = XsdPlugInConfiguration.FromXml(project, configuration.ToString());

                using(var dlg = new XsdConfigurationForm(currentConfig))
                {
                    if(dlg.ShowDialog() == DialogResult.OK)
                    {
                        var newConfig = XElement.Parse(XsdPlugInConfiguration.ToXml(dlg.NewConfiguration));

                        configuration.RemoveAll();

                        foreach(var child in newConfig.Elements().ToList())
                        {
                            child.Remove();
                            configuration.Add(child);
                        }

                        return true;
                    }
                }

                return false;
            }
        }
        #endregion


        public XsdConfigurationForm(XsdPlugInConfiguration configuration)
        {
            InitializeComponent();

            NewConfiguration = configuration.Clone();
            HelpKeyword = HelpTopics.ConfigurePlugIn;

            _propertyGrid.SelectedObject = NewConfiguration;
        }

        public XsdPlugInConfiguration NewConfiguration { get; private set; }
    }
}
