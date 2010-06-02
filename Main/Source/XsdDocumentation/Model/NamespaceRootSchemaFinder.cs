using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal sealed class NamespaceRootSchemaFinder : XmlSchemaSetVisitor
    {
        private readonly HashSet<string> _dependencySchemaFileNames;
        private HashSet<XmlSchema> _schemas = new HashSet<XmlSchema>();
        private HashSet<XmlSchema> _referencedSchemas = new HashSet<XmlSchema>();

        private Stack<string> _targetNamespaces = new Stack<string>();

        public NamespaceRootSchemaFinder(HashSet<string> dependencySchemaFileNames)
        {
            _dependencySchemaFileNames = dependencySchemaFileNames;
        }

        public Dictionary<string, List<XmlSchemaObject>> GetRootSchemas()
        {
            var rootSchemasByNamespace = from schema in _schemas
                                         where !_referencedSchemas.Contains(schema)
                                         group schema by schema.TargetNamespace ?? string.Empty;

            var result = new Dictionary<string, List<XmlSchemaObject>>();
            foreach (var rootSchemaGroup in rootSchemasByNamespace)
            {
                var items = new List<XmlSchemaObject>();
                foreach (var rootSchema in rootSchemaGroup)
                    items.Add(rootSchema);

                result.Add(rootSchemaGroup.Key, items);
            }

            return result;
        }

        private void TraverseExternalSchema(XmlSchemaExternal external)
        {
            if (external.Schema == null ||
                external.Schema.TargetNamespace != _targetNamespaces.Peek())
                return;

            _referencedSchemas.Add(external.Schema);
            Traverse(external.Schema);
        }

        protected override void Visit(XmlSchema schema)
        {
            if (_dependencySchemaFileNames.Contains(schema.GetLocalPath()) || !_schemas.Add(schema))
                return;

            _targetNamespaces.Push(schema.TargetNamespace);
            foreach (XmlSchemaExternal external in schema.Includes)
                Traverse(external);
            _targetNamespaces.Pop();
        }

        protected override void Visit(XmlSchemaImport import)
        {
            TraverseExternalSchema(import);
        }

        protected override void Visit(XmlSchemaInclude include)
        {
            TraverseExternalSchema(include);
        }

        protected override void Visit(XmlSchemaRedefine redefine)
        {
            TraverseExternalSchema(redefine);
        }
    }
}