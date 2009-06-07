using System;
using System.Xml.Schema;

namespace XsdDocumentation
{
	internal static class ExceptionBuilder
	{
		public static Exception UnhandledCaseLabel(object value)
		{
			return new NotImplementedException(string.Format("Unhandled case label '{0}'", value));
		}

		public static Exception UnexpectedSchemaObjectType(XmlSchemaObject schemaObject)
		{
			return new Exception(string.Format("Unexpected schema object type '{0}'.", schemaObject.GetType()));
		}
	}
}