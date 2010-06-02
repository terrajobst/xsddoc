using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal sealed class ParentFinder : XmlSchemaSetVisitor
    {
        private XmlSchemaSet _schemaSet;
        private Dictionary<XmlSchemaObject, HashSet<XmlSchemaObject>> _objectParents;
        private HashSet<XmlSchemaObject> _globalElements = new HashSet<XmlSchemaObject>();
        private Stack<XmlSchemaObject> _parentStack = new Stack<XmlSchemaObject>();
        private Stack<XmlSchemaElement> _parentElementStack = new Stack<XmlSchemaElement>();

        public ParentFinder(XmlSchemaSet schemaSet, Dictionary<XmlSchemaObject, HashSet<XmlSchemaObject>> objectParents)
        {
            _schemaSet = schemaSet;
            _objectParents = objectParents;
        }

        private void PushParent(XmlSchemaObject obj)
        {
            _parentStack.Push(obj);
        }

        private void PopParent()
        {
            _parentStack.Pop();
        }

        private void PushElementParent(XmlSchemaElement element)
        {
            PushParent(element);
            _parentElementStack.Push(element);
        }

        private void PopElementParent()
        {
            PopParent();
            _parentElementStack.Pop();
        }

        private bool AddParent(XmlSchemaObject child)
        {
            if (_parentStack.Count == 0 && _parentElementStack.Count == 0)
                return _globalElements.Add(child);

            return AddParent(_parentStack, child) |
                   AddParent(_parentElementStack, child);
        }

        private bool AddParent<T>(Stack<T> parentStack, XmlSchemaObject child)
            where T : XmlSchemaObject
        {
            if (parentStack.Count == 0)
                return false;

            var parent = parentStack.Peek();

            HashSet<XmlSchemaObject> parents;
            if (!_objectParents.TryGetValue(child, out parents))
            {
                parents = new HashSet<XmlSchemaObject>();
                _objectParents.Add(child, parents);
            }

            return parents.Add(parent);
        }

        private void TraverseBaseType(XmlQualifiedName name)
        {
            var baseType = (XmlSchemaType)_schemaSet.GlobalTypes[name];
            if (baseType != null)
                Traverse(baseType);
        }

        protected override void Visit(XmlSchemaAttribute attribute)
        {
            if (attribute.RefName.IsEmpty)
            {
                AddParent(attribute);
            }
            else
            {
                var referencedAttribute = (XmlSchemaAttribute)_schemaSet.GlobalAttributes[attribute.RefName];
                AddParent(referencedAttribute);
            }
        }

        protected override void Visit(XmlSchemaElement element)
        {
            if (!element.RefName.IsEmpty)
            {
                var referencedElement = (XmlSchemaElement)_schemaSet.GlobalElements[element.RefName];
                AddParent(referencedElement);
            }
            else
            {
                if (!AddParent(element))
                    return;

                PushElementParent(element);
                base.Visit(element);
                PopElementParent();
            }
        }

        protected override void Visit(XmlSchemaGroup group)
        {
            PushParent(group);
            base.Visit(group);
            PopParent();
        }

        protected override void Visit(XmlSchemaAttributeGroup group)
        {
            PushParent(group);
            base.Visit(group);
            PopParent();
        }

        protected override void Visit(XmlSchemaGroupRef groupRef)
        {
            var group = _schemaSet.ResolveGroup(groupRef);
            AddParent(group);
            Traverse(group);
        }

        protected override void Visit(XmlSchemaAttributeGroupRef groupRef)
        {
            var group = _schemaSet.ResolveGroup(groupRef);
            AddParent(group);
            Traverse(group);
        }

        protected override void Visit(XmlSchemaSimpleContentExtension extension)
        {
            TraverseBaseType(extension.BaseTypeName);
            base.Visit(extension);
        }

        protected override void Visit(XmlSchemaSimpleContentRestriction restriction)
        {
            // Don't visit the base type. Complex Type restrictions replace
            // the base type's nodes and attributes.
        }

        protected override void Visit(XmlSchemaComplexContentExtension extension)
        {
            TraverseBaseType(extension.BaseTypeName);
            base.Visit(extension);
        }

        protected override void Visit(XmlSchemaComplexContentRestriction restriction)
        {
            // Don't visit the base type. Complex Type restrictions replace
            // the base type's nodes and attributes.
        }

        protected override void Visit(XmlSchemaComplexType type)
        {
            if (type.QualifiedName.IsEmpty)
                base.Visit(type);
            else
            {
                PushParent(type);
                base.Visit(type);
                PopParent();
            }
        }
    }
}