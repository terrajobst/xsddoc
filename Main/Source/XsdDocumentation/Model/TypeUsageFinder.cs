using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal sealed class TypeUsageFinder : XmlSchemaSetVisitor
    {
        private XmlSchemaSet _schemaSet;
        private Dictionary<XmlSchemaType, HashSet<XmlSchemaObject>> _typeUsages;
        private HashSet<XmlSchemaObject> _globalTypeUsages = new HashSet<XmlSchemaObject>();
        private Stack<XmlSchemaObject> _namedObjectStack = new Stack<XmlSchemaObject>();

        public TypeUsageFinder(XmlSchemaSet schemaSet, Dictionary<XmlSchemaType, HashSet<XmlSchemaObject>> typeUsages)
        {
            _schemaSet = schemaSet;
            _typeUsages = typeUsages;
        }

        private void PushNamedObject(XmlSchemaObject namedObject)
        {
            _namedObjectStack.Push(namedObject);
        }

        private void PopNamedObject()
        {
            _namedObjectStack.Pop();
        }

        private bool AddUsage(XmlSchemaType type)
        {
            if (_namedObjectStack.Count == 0)
                return _globalTypeUsages.Add(type);

            XmlSchemaObject usage = _namedObjectStack.Peek();

            HashSet<XmlSchemaObject> usages;
            if (!_typeUsages.TryGetValue(type, out usages))
            {
                usages = new HashSet<XmlSchemaObject>();
                _typeUsages.Add(type, usages);
            }

            return usages.Add(usage);
        }

        private void AddBaseTypeUsage(XmlQualifiedName baseTypeName)
        {
            var baseType = (XmlSchemaType)_schemaSet.GlobalTypes[baseTypeName];
            if (baseType != null)
                AddUsage(baseType);
        }

        protected override void Visit(XmlSchemaAttribute attribute)
        {
            if (!attribute.RefName.IsEmpty)
                return;

            PushNamedObject(attribute);
            base.Visit(attribute);
            PopNamedObject();
        }

        protected override void Visit(XmlSchemaElement element)
        {
            if (!element.RefName.IsEmpty)
                return;

            PushNamedObject(element);
            base.Visit(element);
            PopNamedObject();
        }

        protected override void Visit(XmlSchemaSimpleType type)
        {
            if (type.QualifiedName.IsEmpty)
                base.Visit(type);
            else
            {
                if (!AddUsage(type))
                    return;

                PushNamedObject(type);
                base.Visit(type);
                PopNamedObject();
            }
        }

        protected override void Visit(XmlSchemaComplexType type)
        {
            if (type.QualifiedName.IsEmpty)
                base.Visit(type);
            else
            {
                if (!AddUsage(type))
                    return;

                PushNamedObject(type);
                base.Visit(type);
                PopNamedObject();
            }
        }

        protected override void Visit(XmlSchemaSimpleContentExtension extension)
        {
            AddBaseTypeUsage(extension.BaseTypeName);
            base.Visit(extension);
        }

        protected override void Visit(XmlSchemaSimpleContentRestriction restriction)
        {
            AddBaseTypeUsage(restriction.BaseTypeName);
            base.Visit(restriction);
        }

        protected override void Visit(XmlSchemaComplexContentExtension extension)
        {
            AddBaseTypeUsage(extension.BaseTypeName);
            base.Visit(extension);
        }

        protected override void Visit(XmlSchemaComplexContentRestriction restriction)
        {
            AddBaseTypeUsage(restriction.BaseTypeName);
            base.Visit(restriction);
        }
    }
}