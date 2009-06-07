using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class SyntaxMarkupBuilder
	{
		public static string Build(Context context, XmlSchemaObject obj)
		{
			return MarkupHelper.XmlEncode(context.SourceCodeManager.GetSourceCodeAbridged(obj));
		}
	}
}