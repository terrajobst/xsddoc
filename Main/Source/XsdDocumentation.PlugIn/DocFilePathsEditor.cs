using System;

namespace XsdDocumentation.PlugIn
{
	internal sealed class DocFilePathsEditor : FilePathsEditor
	{
		public override string Title
		{
			get { return "Edit Documentation Files"; }
		}

		public override string Filter
		{
			get { return "Documentation Files (*.xml)|*.xml|All Files (*.*)|*.*"; }
		}

		public override string HelpKeyword
		{
			get { return HelpTopics.ConfigureDocFiles; }
		}
	}
}