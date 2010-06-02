using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Linq;

namespace XsdDocumentation.Model
{
    internal sealed class AttributeFinder : XmlSchemaSetVisitor
    {
        private SchemaSetManager _schemaSetManager;
        private AttributeEntries _attributeEntries = new AttributeEntries();
        private int _inRestrictionCounter;

        public AttributeFinder(SchemaSetManager schemaSetManager)
        {
            _schemaSetManager = schemaSetManager;
        }

        public AttributeEntries GetAttributeEntries()
        {
            if (_attributeEntries.AnyAttribute != null)
            {
                var extensionAttributes = _schemaSetManager.GetExtensionAttributes(_attributeEntries.AnyAttribute).Cast<XmlSchemaAttribute>();
                _attributeEntries.ExtensionAttributes.AddRange(extensionAttributes);
            }

            return _attributeEntries;
        }

        private void ProcessExtension(XmlQualifiedName baseTypeName, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute)
        {
            var baseType = (XmlSchemaType)_schemaSetManager.SchemaSet.GlobalTypes[baseTypeName];
            if (baseType != null)
                Traverse(baseType);

            Traverse(attributes);

            if (anyAttribute != null)
                Traverse(anyAttribute);
        }

        private void ProcessRestriction(XmlQualifiedName baseTypeName, XmlSchemaObjectCollection attributes, XmlSchemaAnyAttribute anyAttribute)
        {
            var baseType = (XmlSchemaComplexType)_schemaSetManager.SchemaSet.GlobalTypes[baseTypeName];
            if (baseType != null)
            {
                Traverse(baseType);

                var attributeDictionary = new Dictionary<XmlQualifiedName, XmlSchemaAttribute>();
                foreach (var attribute in _attributeEntries.Attributes)
                    attributeDictionary.Add(attribute.QualifiedName, attribute);

                _attributeEntries.Attributes.Clear();

                _inRestrictionCounter++;
                Traverse(attributes);
                _inRestrictionCounter--;

                if (anyAttribute != null)
                    Traverse(anyAttribute);

                for (int i = _attributeEntries.Attributes.Count - 1; i >= 0; i--)
                {
                    var attribute = _attributeEntries.Attributes[i];
                    if (attribute.Use == XmlSchemaUse.Prohibited)
                    {
                        _attributeEntries.Attributes.RemoveAt(i);
                        attributeDictionary.Remove(attribute.QualifiedName);
                    }
                    else
                    {
                        if (!attributeDictionary.ContainsKey(attribute.QualifiedName))
                            _attributeEntries.AnyAttribute = null;

                        attributeDictionary[attribute.QualifiedName] = attribute;
                    }
                }

                _attributeEntries.Attributes.Clear();
                _attributeEntries.Attributes.AddRange(attributeDictionary.Values);
            }
        }

        protected override void Visit(XmlSchemaAttribute attribute)
        {
            if (attribute.Use != XmlSchemaUse.Prohibited ||
                _inRestrictionCounter > 0)
                _attributeEntries.Attributes.Add(attribute);
        }

        protected override void Visit(XmlSchemaAnyAttribute attribute)
        {
            _attributeEntries.AnyAttribute = attribute;
        }

        protected override void Visit(XmlSchemaAttributeGroupRef groupRef)
        {
            var group = _schemaSetManager.SchemaSet.ResolveGroup(groupRef);
            Traverse(group.Attributes);
            if (group.AnyAttribute != null)
                Traverse(group.AnyAttribute);
        }

        protected override void Visit(XmlSchemaComplexContentExtension extension)
        {
            ProcessExtension(extension.BaseTypeName, extension.Attributes, extension.AnyAttribute);
        }

        protected override void Visit(XmlSchemaSimpleContentExtension extension)
        {
            ProcessExtension(extension.BaseTypeName, extension.Attributes, extension.AnyAttribute);
        }

        protected override void Visit(XmlSchemaSimpleContentRestriction restriction)
        {
            ProcessRestriction(restriction.BaseTypeName, restriction.Attributes, restriction.AnyAttribute);
        }

        protected override void Visit(XmlSchemaComplexContentRestriction restriction)
        {
            ProcessRestriction(restriction.BaseTypeName, restriction.Attributes, restriction.AnyAttribute);
        }

        protected override void Visit(XmlSchemaAll particle)
        {
            // Don't visit children.
        }

        protected override void Visit(XmlSchemaChoice particle)
        {
            // Don't visit children.
        }

        protected override void Visit(XmlSchemaSequence particle)
        {
            // Don't visit children.
        }

        protected override void Visit(XmlSchemaGroupRef groupRef)
        {
            // Don't visit children.
        }
    }
}