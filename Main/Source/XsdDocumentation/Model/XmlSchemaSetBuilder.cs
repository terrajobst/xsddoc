using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace XsdDocumentation.Model
{
    internal static class XmlSchemaSetBuilder
    {
        public static XmlSchemaSet Build(IEnumerable<string> schemaFileNames)
        {
            var schemas = new Dictionary<string, XmlSchema>(StringComparer.OrdinalIgnoreCase);
            var knownSchemas = new HashSet<string>(schemaFileNames, StringComparer.OrdinalIgnoreCase);
            var referencedSchemas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var schemaQueue = new Queue<string>(schemaFileNames);
            while (schemaQueue.Count > 0)
            {
                var schemaFileName = schemaQueue.Dequeue();
                var schemaDirectory = Path.GetDirectoryName(schemaFileName);
                var schema = GetSchema(schemaFileName);
                schemas.Add(schemaFileName, schema);
                foreach (XmlSchemaExternal external in schema.Includes)
                {
                    if (external.SchemaLocation == null)
                        continue;

                    var referencedSchemaFullPath = GetLocation(schemaDirectory, external.SchemaLocation);
                    referencedSchemas.Add(referencedSchemaFullPath);
                    if (knownSchemas.Add(referencedSchemaFullPath))
                        schemaQueue.Enqueue(referencedSchemaFullPath);
                }
            }

            var schemaSet = new XmlSchemaSet();
            foreach (var schemaFile in schemaFileNames)
            {
                if (referencedSchemas.Contains(schemaFile))
                    continue;

                var schema = schemas[schemaFile];
                schemaSet.Add(schema);
            }
            schemaSet.Compile();
            return schemaSet;
        }

        private static XmlSchema GetSchema(string fileName)
        {
            try
            {
                using (var nodeReader = new XmlTextReader(fileName))
                    return XmlSchema.Read(nodeReader, null);
            }
            catch (Exception ex)
            {
                throw ExceptionBuilder.CannotReadSchemaFile(fileName, ex);
            }
        }

        private static string GetLocation(string directory, string localPathOrUri)
        {
            var localPath = GetLocalPath(localPathOrUri);

            return Path.IsPathRooted(localPath)
                    ? localPath
                    : Path.Combine(directory, localPath);
        }

        private static string GetLocalPath(string localPathOrUri)
        {
            try
            {
                var uri = new Uri(localPathOrUri);
                return uri.LocalPath;
            }
            catch (UriFormatException)
            {
                return localPathOrUri;
            }
        }
    }
}