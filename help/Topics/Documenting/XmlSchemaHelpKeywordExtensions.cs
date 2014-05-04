using System;
using System.Text;
using System.Xml.Schema;

namespace XsdDocumentation
{
    internal static class XmlSchemaHelpKeywordExtensions
    {
        public static string GetHelpKeyword(this XmlSchemaSet schemaSet, XmlSchemaElement schemaElement)
        {
            return GetHelpKeyword(schemaSet, schemaElement.QualifiedName.Namespace, schemaElement);
        }

        public static string GetHelpKeyword(this XmlSchemaSet schemaSet, XmlSchemaAttribute schemaElement)
        {
            return GetHelpKeyword(schemaSet, schemaElement.QualifiedName.Namespace, schemaElement);
        }

        private static string GetHelpKeyword(XmlSchemaSet schemaSet, string xmlNamespace, XmlSchemaObject xmlSchemaObject)
        {
            var xmlLastObject = xmlSchemaObject;
            var sb = new StringBuilder();
            while (xmlSchemaObject != null && !(xmlSchemaObject is XmlSchema))
            {
                xmlSchemaObject = ResolveLink(schemaSet, xmlSchemaObject);
                var name = GetName(xmlSchemaObject);
                if (name != null)
                {
                    if (sb.Length > 0)
                        sb.Insert(0, "/");
                    sb.Insert(0, name);
                }

                xmlLastObject = xmlSchemaObject;
                xmlSchemaObject = xmlSchemaObject.Parent;
            }

            if (xmlLastObject is XmlSchemaGroup)
                sb.Insert(0, "#G/");
            else if (xmlLastObject is XmlSchemaAttributeGroup)
                sb.Insert(0, "#AG/");
            else if (xmlLastObject is XmlSchemaSimpleType || xmlLastObject is XmlSchemaComplexType)
                sb.Insert(0, "#T/");
            else
                sb.Insert(0, "#E/");

            sb.Insert(0, xmlNamespace ?? String.Empty);
            return sb.ToString();
        }

        private static XmlSchemaObject ResolveLink(XmlSchemaSet schemaSet, XmlSchemaObject schemaObject)
        {
            var element = schemaObject as XmlSchemaElement;
            if (element != null && !element.RefName.IsEmpty)
                return schemaSet.GlobalElements[element.RefName];

            var attribute = schemaObject as XmlSchemaAttribute;
            if (attribute != null && !attribute.RefName.IsEmpty)
                return schemaSet.GlobalAttributes[attribute.RefName];

            return schemaObject;
        }

        private static string GetName(XmlSchemaObject xmlSchemaObject)
        {
            var isGlobal = xmlSchemaObject.Parent is XmlSchema;

            var soAsAttribute = xmlSchemaObject as XmlSchemaAttribute;
            if (soAsAttribute != null)
                if (isGlobal)
                    return soAsAttribute.QualifiedName.Name;
                else
                    return "@" + soAsAttribute.QualifiedName.Name;

            var soAsElement = xmlSchemaObject as XmlSchemaElement;
            if (soAsElement != null)
                return soAsElement.QualifiedName.Name;

            var soAsGroup = xmlSchemaObject as XmlSchemaGroup;
            if (soAsGroup != null)
                return soAsGroup.QualifiedName.Name;

            var soAsAttributeGroup = xmlSchemaObject as XmlSchemaAttributeGroup;
            if (soAsAttributeGroup != null)
                return soAsAttributeGroup.QualifiedName.Name;

            var soAsComplexType = xmlSchemaObject as XmlSchemaComplexType;
            if (soAsComplexType != null && !soAsComplexType.QualifiedName.IsEmpty)
                return soAsComplexType.QualifiedName.Name;

            return null;
        }
    }
}