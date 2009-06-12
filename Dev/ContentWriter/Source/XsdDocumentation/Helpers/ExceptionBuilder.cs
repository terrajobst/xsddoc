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
	}
}