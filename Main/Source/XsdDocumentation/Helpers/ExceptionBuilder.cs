using System;
using System.Globalization;
using System.Xml.Schema;

using XsdDocumentation.Properties;

namespace XsdDocumentation
{
    internal static class ExceptionBuilder
    {
        public static Exception UnhandledCaseLabel(object value)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ExceptionBuilderUnhandledCaseLabel, value);
            return new NotImplementedException(message);
        }

        public static Exception UnexpectedSchemaObjectType(XmlSchemaObject schemaObject)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ExceptionBuilderUnexpectedSchemaObjectType, schemaObject.GetType());
            return new NotImplementedException(message);
        }

        public static Exception CannotReadSchemaFile(string fileName, Exception exception)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ExceptionBuilderCannotReadSchemaFile, fileName);
            return new XsdDocumentationException(message, exception);
        }

        public static Exception ErrorBuildingSchemaSet(Exception exception)
        {
            var message = Resources.ExceptionBuilderErrorBuildingSchemaSet;
            return new XsdDocumentationException(message, exception);
        }

        public static Exception ErrorReadingExternalDocumentation(string fileName, Exception exception)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ExceptionBuilderErrorReadingExternalDocumentation, fileName);
            return new XsdDocumentationException(message, exception);
        }

        public static Exception ErrorTransformingInlineDocumentation(string fileName, Exception exception)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ExceptionBuilderErrorTransformingInlineDocumentation, fileName);
            return new XsdDocumentationException(message, exception);
        }

        public static Exception CannotReadTransform(string fileName, Exception exception)
        {
            var message = string.Format(CultureInfo.CurrentCulture, Resources.ExceptionBuilderCannotReadTransform, fileName);
            return new XsdDocumentationException(message, exception);
        }
    }
}