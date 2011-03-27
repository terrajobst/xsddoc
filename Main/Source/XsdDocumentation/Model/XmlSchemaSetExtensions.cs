using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal static class XmlSchemaSetExtensions
    {
        public static List<XmlSchema> GetAllSchemas(this XmlSchemaSet schemaSet)
        {
            // The method XmlSchemaSet.Schemas() will only include all schemas
            // directly added to th schema set, it does not contain any schema
            // that is only indirectly included or imported.
            //
            // So the first thing is to recursively process all schemas and add
            // all schemas included or imported.

            var schemas = new HashSet<XmlSchema>();
            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                schemas.Add(schema);
                AddIncludedSchemas(schemas, schema);
            }

            // However, now there are still schemas missing: so-called chameleon
            // schemas. A chameleon schema is a schema that does not declare
            // a target namespace. If such a schema is included into another
            // schema that declares a target namespace the included schema
            // "inherits" that target namespace. System.Xml.Schema accomplishes
            // that by cloning the schema object and updating all the
            // namespaces. The problem is that we don't find such a schema
            // via XmlSchemaSet.Schemas() or schema.Includes. Instead we have
            // to look at every declared entity and search up their parents
            // until we find the declaring schema. This is sad and ineffecient
            // but it seems to be the only possible option.

            var topLevelSchemas = schemas.ToList();
            foreach (var schema in topLevelSchemas)
            {
                var allItems = schema.Elements.Values.Cast<XmlSchemaObject>()
                       .Concat(schema.Attributes.Values.Cast<XmlSchemaObject>())
                       .Concat(schema.Groups.Values.Cast<XmlSchemaObject>())
                       .Concat(schema.AttributeGroups.Values.Cast<XmlSchemaObject>())
                       .Concat(schema.SchemaTypes.Values.Cast<XmlSchemaObject>())
                       .Concat(schema.Notations.Values.Cast<XmlSchemaObject>());

                foreach (var item in allItems)
                {
                    var declaredSchema = item.GetSchema();
                    if (declaredSchema != null)
                        schemas.Add(declaredSchema);
                }
            }

            return schemas.ToList();
        }

        private static void AddIncludedSchemas(HashSet<XmlSchema> schemas, XmlSchema schema)
        {
            foreach (XmlSchemaExternal external in schema.Includes)
            {
                if (external.Schema == null)
                    continue;

                if (schemas.Add(external.Schema))
                    AddIncludedSchemas(schemas, external.Schema);
            }
        }

        public static XmlSchemaSimpleType ResolveType(this XmlSchemaSet schemaSet, XmlSchemaSimpleType type, XmlQualifiedName typeName)
        {
            if (type != null)
                return type;

            var resolvedType = schemaSet.GlobalTypes[typeName] as XmlSchemaSimpleType;
            if (resolvedType != null)
                return resolvedType;

            var builtInSimpleType = XmlSchemaType.GetBuiltInSimpleType(typeName);
            if (builtInSimpleType != null)
                return builtInSimpleType;

            return null;
        }

        public static XmlSchemaComplexType ResolveType(this XmlSchemaSet schemaSet, XmlSchemaComplexType type, XmlQualifiedName typeName)
        {
            if (type != null)
                return type;

            var resolvedType = schemaSet.GlobalTypes[typeName] as XmlSchemaComplexType;
            if (resolvedType != null)
                return resolvedType;

            var builtInComplexType = XmlSchemaType.GetBuiltInComplexType(typeName);
            if (builtInComplexType != null)
                return builtInComplexType;

            return null;
        }

        public static XmlSchemaGroup ResolveGroup(this XmlSchemaSet schemaSet, XmlSchemaGroupRef groupRef)
        {
            foreach (var schema in schemaSet.GetAllSchemas())
            {
                var group = (XmlSchemaGroup)schema.Groups[groupRef.RefName];
                if (group != null)
                    return group;
            }

            return null;
        }

        public static XmlSchemaAttributeGroup ResolveGroup(this XmlSchemaSet schemaSet, XmlSchemaAttributeGroupRef groupRef)
        {
            foreach (var schema in schemaSet.GetAllSchemas())
            {
                var group = (XmlSchemaAttributeGroup)schema.AttributeGroups[groupRef.RefName];
                if (group != null)
                    return group;
            }

            return null;
        }
    }
}