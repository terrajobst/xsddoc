using System;

namespace XsdDocumentation.PlugIn
{
	internal sealed partial class XsdConfigurationForm : HelpAwareForm
	{
		private XsdPlugInConfiguration _newConfiguration;

		public XsdConfigurationForm(XsdPlugInConfiguration configuration)
		{
			InitializeComponent();

			_newConfiguration = configuration.Clone();
			_propertyGrid.SelectedObject = _newConfiguration;
			HelpKeyword = HelpTopics.ConfigurePlugIn;
		}

		public XsdPlugInConfiguration NewConfiguration
		{
			get { return _newConfiguration; }
		}
	}
}