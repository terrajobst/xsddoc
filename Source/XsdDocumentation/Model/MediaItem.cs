using System;

namespace XsdDocumentation.Model
{
	public sealed class MediaItem
	{
		public MediaItem(ArtItem artItem, string fileName)
		{
			ArtItem = artItem;
			FileName = fileName;
		}

		public ArtItem ArtItem { get; private set; }
		public string FileName { get; private set; }
	}
}