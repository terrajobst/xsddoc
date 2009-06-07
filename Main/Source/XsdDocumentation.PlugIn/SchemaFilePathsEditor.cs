using System;

namespace XsdDocumentation.PlugIn
{
	internal sealed class SchemaFilePathsEditor : FilePathsEditor
	{
		public override string Title
		{
			get { return "Edit Schema Files"; }
		}

		public override string Filter
		{
			get { return "XML Schema Files (*.xsd)|*.xsd|All Files (*.*)|*.*"; }
		}

		public override string HelpKeyword
		{
			get { return HelpTopics.ConfigureSchemaFiles; }
		}
	}
}