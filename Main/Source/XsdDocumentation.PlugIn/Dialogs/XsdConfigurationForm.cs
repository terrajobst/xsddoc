using System;

namespace XsdDocumentation.PlugIn
{
    internal sealed partial class XsdConfigurationForm : HelpAwareForm
    {
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