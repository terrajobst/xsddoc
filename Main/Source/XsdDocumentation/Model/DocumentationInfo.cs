using System;
using System.Xml;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class DocumentationInfo
	{
		public XmlNode SummaryNode { get; set; }
		public XmlNode RemarksNode { get; set; }
		public XmlNode ExamplesNode { get; set; }
		public XmlNode RelatedTopicsNode { get; set; }
		public bool IsObsolete { get; set; }
		public XmlSchemaObject NonObsoleteAlternative { get; set; }
	}
}