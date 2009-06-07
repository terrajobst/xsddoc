using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class SimpleTypeStructureNode
	{
		public SimpleTypeStructureNode()
		{
			Children = new List<SimpleTypeStructureNode>();
		}

		public SimpleTypeStructureNodeType NodeType { get; set; }
		public XmlSchemaObject Node { get; set; }
		public List<SimpleTypeStructureNode> Children { get; private set; }
	}
}