using System;
using System.IO;
using System.Reflection;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal static class XmlSchemaObjectExtensions
    {
        public static XmlSchema GetSchema(this XmlSchemaObject obj)
        {
            while (obj != null)
            {
                var schema = obj as XmlSchema;
                if (schema != null)
                    return schema;

                obj = obj.Parent;
            }

            return null;
        }

        public static XmlSchemaObject GetRoot(this XmlSchemaObject obj)
        {
            var lastObject = obj;
            var parent = lastObject.Parent;
            while (parent != null)
            {
                var parentAsSchema = parent as XmlSchema;
                if (parentAsSchema != null)
                    return lastObject;

                lastObject = parent;
                parent = parent.Parent;
            }

            return null;
        }


        public static string GetBaseUri(this XmlSchemaObject obj)
        {
            if (obj.SourceUri != null)
                return obj.SourceUri;

            var schema = obj.GetSchema();
            return (schema == null)
                    ? null
                    : GetBaseUri(schema);
        }

        private static string GetBaseUri(XmlSchema schema)
        {
            var baseUriProperty = typeof(XmlSchema).GetProperty("BaseUri", BindingFlags.Instance | BindingFlags.NonPublic);
            var baseUri = (Uri)baseUriProperty.GetValue(schema, null);
            return baseUri.ToString();
        }

        public static string GetLocalPath(this XmlSchemaObject obj)
        {
            var baseUri = obj.GetBaseUri();

            return (baseUri == null)
                    ? null
                    : new Uri(baseUri).LocalPath;
        }

        public static string GetSchemaName(this XmlSchemaObject obj)
        {
            var localPath = obj.GetLocalPath();

            return (localPath == null)
                    ? null
                    : Path.GetFileName(localPath);
        }
    }
}