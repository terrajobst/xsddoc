using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal sealed class NamespaceRootElementFinder : XmlSchemaSetVisitor
    {
        private XmlSchemaSet _schemaSet;
        private string _targetNamespace;

        private HashSet<XmlSchemaType> _globalTypes = new HashSet<XmlSchemaType>();
        private List<XmlSchemaElement> _elements = new List<XmlSchemaElement>();
        private HashSet<XmlSchemaElement> _referencedElements = new HashSet<XmlSchemaElement>();

        public NamespaceRootElementFinder(XmlSchemaSet schemaSet)
        {
            _schemaSet = schemaSet;
        }

        public Dictionary<string, List<XmlSchemaObject>> GetRootElements()
        {
            var rootElementsByNamespace = from element in _elements
                                          where !_referencedElements.Contains(element)
                                          group element by element.QualifiedName.Namespace;

            var result = new Dictionary<string, List<XmlSchemaObject>>();
            foreach (var rootElementGroup in rootElementsByNamespace)
            {
                var items = new List<XmlSchemaObject>();
                foreach (var rootElement in rootElementGroup)
                    items.Add(rootElement);

                result.Add(rootElementGroup.Key, items);
            }

            return result;
        }

        private static XmlSchemaElement FindOuterMostElement(XmlSchemaObject element)
        {
            var outerMostElement = (XmlSchemaElement)null;
            var parent = element.Parent;
            while (parent != null)
            {
                var parentAsElement = parent as XmlSchemaElement;
                if (parentAsElement != null)
                    outerMostElement = parentAsElement;

                parent = parent.Parent;
            }

            return outerMostElement;
        }

        protected override void Visit(XmlSchema schema)
        {
            _targetNamespace = schema.TargetNamespace;
            base.Visit(schema);
            _targetNamespace = null;
        }

        protected override void Visit(XmlSchemaElement element)
        {
            var sameNamespace = element.QualifiedName.Namespace == _targetNamespace ||
                                string.IsNullOrEmpty(element.QualifiedName.Namespace) && string.IsNullOrEmpty(_targetNamespace);
            if (!sameNamespace)
                return;

            var isGlobal = (element.Parent is XmlSchema);

            if (isGlobal)
            {
                _elements.Add(element);
                base.Visit(element);
            }
            else if (element.RefName.IsEmpty)
            {
                base.Visit(element);
            }
            else
            {
                var elementParent = FindOuterMostElement(element);
                var referencedElement = (XmlSchemaElement)_schemaSet.GlobalElements[element.RefName];
                if (referencedElement != elementParent)
                    _referencedElements.Add(referencedElement);
            }
        }

        protected override void Visit(XmlSchemaSimpleType type)
        {
            if (_globalTypes.Add(type))
                base.Visit(type);
        }

        protected override void Visit(XmlSchemaComplexType type)
        {
            if (_globalTypes.Add(type))
                base.Visit(type);
        }
    }
}