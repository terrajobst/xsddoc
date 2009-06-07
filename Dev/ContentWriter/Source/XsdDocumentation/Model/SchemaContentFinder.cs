using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class SchemaContentFinder : XmlSchemaSetVisitor
	{
		private XmlSchema _schema;
		private List<XmlSchemaObject> _attributes = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _elements = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _groups = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _attributeGroups = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _simpleTypes = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _complexTypes = new List<XmlSchemaObject>();

		public SchemaContentFinder(XmlSchema schema)
		{
			_schema = schema;
		}

		public List<XmlSchemaObject> Attributes
		{
			get { return _attributes; }
		}

		public List<XmlSchemaObject> Elements
		{
			get { return _elements; }
		}

		public List<XmlSchemaObject> Groups
		{
			get { return _groups; }
		}

		public List<XmlSchemaObject> AttributeGroups
		{
			get { return _attributeGroups; }
		}

		public List<XmlSchemaObject> SimpleTypes
		{
			get { return _simpleTypes; }
		}

		public List<XmlSchemaObject> ComplexTypes
		{
			get { return _complexTypes; }
		}

		protected override void Visit(XmlSchema schema)
		{
			if (schema == _schema)
				base.Visit(schema);
		}

		protected override void Visit(XmlSchemaAttribute attribute)
		{
			_attributes.Add(attribute);
		}

		protected override void Visit(XmlSchemaElement element)
		{
			_elements.Add(element);
		}

		protected override void Visit(XmlSchemaGroup group)
		{
			_groups.Add(group);
		}

		protected override void Visit(XmlSchemaAttributeGroup group)
		{
			_attributeGroups.Add(group);
		}

		protected override void Visit(XmlSchemaSimpleType type)
		{
			_simpleTypes.Add(type);
		}

		protected override void Visit(XmlSchemaComplexType type)
		{
			_complexTypes.Add(type);
		}
	}
}