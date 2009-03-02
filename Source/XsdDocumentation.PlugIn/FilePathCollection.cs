using System.Collections.ObjectModel;
using System.ComponentModel;

using SandcastleBuilder.Utils;

namespace XsdDocumentation.PlugIn
{
	[TypeConverter(typeof(FilePathCollectionConverter))]
	internal sealed class FilePathCollection : Collection<FilePath>
	{
		public FilePathCollection Clone()
		{
			var result = new FilePathCollection();
			foreach (var filePath in this)
			{
				var clonedFilePath = (FilePath) filePath.Clone();
				result.Add(clonedFilePath);
			}
			return result;
		}
	}
}