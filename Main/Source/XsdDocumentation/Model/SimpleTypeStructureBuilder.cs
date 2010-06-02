using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal sealed class SimpleTypeStructureBuilder : XmlSchemaSetVisitor
    {
        private Stack<SimpleTypeStructureNode> _nodeStack = new Stack<SimpleTypeStructureNode>();
        private XmlSchemaSet _schemaSet;

        public SimpleTypeStructureBuilder(XmlSchemaSet schemaSet)
        {
            _schemaSet = schemaSet;
        }

        public SimpleTypeStructureNode GetRoot()
        {
            return _nodeStack.Count == 0
                       ? null
                       : _nodeStack.Pop();
        }

        private SimpleTypeStructureNode AddLeaf(SimpleTypeStructureNodeType nodeType, XmlSchemaObject obj)
        {
            if (_nodeStack.Count == 0)
            {
                var root = new SimpleTypeStructureNode { NodeType = SimpleTypeStructureNodeType.Root };
                _nodeStack.Push(root);
            }

            var node = new SimpleTypeStructureNode
                       {
                           NodeType = nodeType,
                           Node = obj
                       };

            _nodeStack.Peek().Children.Add(node);
            return node;
        }

        private void PushNode(SimpleTypeStructureNodeType nodeType, XmlSchemaObject obj)
        {
            var node = AddLeaf(nodeType, obj);
            _nodeStack.Push(node);
        }

        private void PopNode()
        {
            _nodeStack.Pop();
        }

        protected override void Visit(XmlSchemaSimpleType type)
        {
            if (type.QualifiedName.IsEmpty)
                Traverse(type.Content);
            else
                AddLeaf(SimpleTypeStructureNodeType.NamedType, type);
        }

        protected override void Visit(XmlSchemaSimpleTypeList list)
        {
            PushNode(SimpleTypeStructureNodeType.List, list);

            Traverse(list.BaseItemType);

            PopNode();
        }

        protected override void Visit(XmlSchemaSimpleTypeRestriction restriction)
        {
            PushNode(SimpleTypeStructureNodeType.Restriction, restriction);

            var baseType = _schemaSet.ResolveType(restriction.BaseType, restriction.BaseTypeName);
            Traverse(baseType);
            Traverse(restriction.Facets);

            PopNode();
        }

        protected override void Visit(XmlSchemaSimpleTypeUnion union)
        {
            PushNode(SimpleTypeStructureNodeType.Union, union);

            foreach (var baseMemberType in union.BaseMemberTypes)
                Traverse(baseMemberType);

            PopNode();
        }

        protected override void Visit(XmlSchemaEnumerationFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetEnumeration, facet);
        }

        protected override void Visit(XmlSchemaMaxExclusiveFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetMaxExclusive, facet);
        }

        protected override void Visit(XmlSchemaMaxInclusiveFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetMaxInclusive, facet);
        }

        protected override void Visit(XmlSchemaMinExclusiveFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetMinExclusive, facet);
        }

        protected override void Visit(XmlSchemaMinInclusiveFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetMinInclusive, facet);
        }

        protected override void Visit(XmlSchemaFractionDigitsFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetFractionDigits, facet);
        }

        protected override void Visit(XmlSchemaLengthFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetLength, facet);
        }

        protected override void Visit(XmlSchemaMaxLengthFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetMaxLength, facet);
        }

        protected override void Visit(XmlSchemaMinLengthFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetMinLength, facet);
        }

        protected override void Visit(XmlSchemaTotalDigitsFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetTotalDigits, facet);
        }

        protected override void Visit(XmlSchemaPatternFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetPattern, facet);
        }

        protected override void Visit(XmlSchemaWhiteSpaceFacet facet)
        {
            AddLeaf(SimpleTypeStructureNodeType.FacetWhiteSpace, facet);
        }

        protected override void Visit(XmlSchemaComplexType type)
        {
            if (type.IsMixed)
                AddLeaf(SimpleTypeStructureNodeType.Mixed, type);
            else if (type.ContentModel != null)
                Traverse(type.ContentModel);
            else if (type == XmlSchemaType.GetBuiltInComplexType(XmlTypeCode.Item))
                AddLeaf(SimpleTypeStructureNodeType.Any, type);
        }

        protected override void Visit(XmlSchemaSimpleContentExtension extension)
        {
            TraverseBaseType(extension.BaseTypeName);
        }

        private void TraverseBaseType(XmlQualifiedName baseTypeName)
        {
            var resolvedSimpleType = _schemaSet.ResolveType((XmlSchemaSimpleType)null, baseTypeName);
            var resolvedComplexType = _schemaSet.ResolveType((XmlSchemaComplexType)null, baseTypeName);

            if (resolvedSimpleType != null)
                Traverse(resolvedSimpleType);
            else
                Traverse(resolvedComplexType);
        }

        protected override void Visit(XmlSchemaSimpleContentRestriction restriction)
        {
            if (restriction.Facets.Count == 0)
                TraverseBaseType(restriction.BaseTypeName);
            else
            {
                PushNode(SimpleTypeStructureNodeType.Restriction, restriction);

                TraverseBaseType(restriction.BaseTypeName);
                Traverse(restriction.Facets);

                PopNode();
            }
        }

        protected override void Visit(XmlSchemaComplexContentExtension extension)
        {
            // Does not have text.
        }

        protected override void Visit(XmlSchemaComplexContentRestriction restriction)
        {
            // Does not have text.
        }
    }
}