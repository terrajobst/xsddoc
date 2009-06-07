using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
	internal sealed class NamespaceContentFinder : XmlSchemaSetVisitor
	{
		private SchemaSetManager _schemaSetManager;
		private string _targetNamespace;
		private List<XmlSchemaObject> _schemas = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _attributes = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _elements = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _groups = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _attributeGroups = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _simpleTypes = new List<XmlSchemaObject>();
		private List<XmlSchemaObject> _complexTypes = new List<XmlSchemaObject>();

		public NamespaceContentFinder(SchemaSetManager schemaSetManager, string targetNamespace)
		{
			_schemaSetManager = schemaSetManager;
			_targetNamespace = targetNamespace;
		}

		public List<XmlSchemaObject> Schemas
		{
			get { return _schemas; }
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
			if (schema.TargetNamespace == _targetNamespace &&
				!_schemaSetManager.IsDependencySchema(schema))
			{
				_schemas.Add(schema);
				base.Visit(schema);
			}
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
