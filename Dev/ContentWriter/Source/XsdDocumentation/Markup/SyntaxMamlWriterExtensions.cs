using System;
using System.Xml.Schema;

using XsdDocumentation.Model;

namespace XsdDocumentation.Markup
{
	internal static class SyntaxMamlWriterExtensions
	{
		public static void WriteCode(this MamlWriter writer, Context context, XmlSchemaObject obj)
		{
			writer.WriteCode(context.SourceCodeManager.GetSourceCodeAbridged(obj), "xml");
		}
	}
}