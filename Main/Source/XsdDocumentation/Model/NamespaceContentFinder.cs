using System;
using System.Collections.Generic;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal sealed class NamespaceContentFinder : XmlSchemaSetVisitor
    {
        private SchemaSetManager _schemaSetManager;
        private string _targetNamespace;

        public NamespaceContentFinder(SchemaSetManager schemaSetManager, string targetNamespace)
        {
            _schemaSetManager = schemaSetManager;
            _targetNamespace = targetNamespace;
            ComplexTypes = new List<XmlSchemaObject>();
            SimpleTypes = new List<XmlSchemaObject>();
            AttributeGroups = new List<XmlSchemaObject>();
            Groups = new List<XmlSchemaObject>();
            Elements = new List<XmlSchemaObject>();
            Attributes = new List<XmlSchemaObject>();
            Schemas = new List<XmlSchemaObject>();
        }

        public List<XmlSchemaObject> Schemas { get; private set; }
        public List<XmlSchemaObject> Attributes { get; private set; }
        public List<XmlSchemaObject> Elements { get; private set; }
        public List<XmlSchemaObject> Groups { get; private set; }
        public List<XmlSchemaObject> AttributeGroups { get; private set; }
        public List<XmlSchemaObject> SimpleTypes { get; private set; }
        public List<XmlSchemaObject> ComplexTypes { get; private set; }

        protected override void Visit(XmlSchema schema)
        {
            if (schema.TargetNamespace != _targetNamespace ||
                _schemaSetManager.IsDependencySchema(schema))
                return;

            Schemas.Add(schema);
            base.Visit(schema);
        }

        protected override void Visit(XmlSchemaAttribute attribute)
        {
            Attributes.Add(attribute);
        }

        protected override void Visit(XmlSchemaElement element)
        {
            Elements.Add(element);
        }

        protected override void Visit(XmlSchemaGroup group)
        {
            Groups.Add(group);
        }

        protected override void Visit(XmlSchemaAttributeGroup group)
        {
            AttributeGroups.Add(group);
        }

        protected override void Visit(XmlSchemaSimpleType type)
        {
            SimpleTypes.Add(type);
        }

        protected override void Visit(XmlSchemaComplexType type)
        {
            ComplexTypes.Add(type);
        }
    }
}