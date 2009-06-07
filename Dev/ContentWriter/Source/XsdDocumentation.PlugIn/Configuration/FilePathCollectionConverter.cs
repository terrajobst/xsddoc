using System;
using System.ComponentModel;
using System.Globalization;

using XsdDocumentation.PlugIn.Properties;

namespace XsdDocumentation.PlugIn
{
	internal sealed class FilePathCollectionConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			var collection = (FilePathCollection) value;

			return (collection.Count == 0)
			       	? Resources.FilePathCollectionConverterNoFiles
			       	: string.Format(CultureInfo.CurrentCulture, Resources.FilePathCollectionConverterFileCountFormatted, collection.Count);
		}
	}
}