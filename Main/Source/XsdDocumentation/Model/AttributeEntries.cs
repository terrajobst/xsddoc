using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class AttributeEntries
	{
		private List<XmlSchemaAttribute> _attributes = new List<XmlSchemaAttribute>();
		private List<XmlSchemaAttribute> _extensionAttributes = new List<XmlSchemaAttribute>();

		public List<XmlSchemaAttribute> Attributes
		{
			get { return _attributes; }
		}

		public XmlSchemaAnyAttribute AnyAttribute { get; set; }

		public List<XmlSchemaAttribute> ExtensionAttributes
		{
			get { return _extensionAttributes; }
		}
	}
}