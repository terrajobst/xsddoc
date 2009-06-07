using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XsdDocumentation.Build
{
	public sealed class GetFileVersion : Task
	{
		[Required]
		public ITaskItem File { get; set; }

		[Output]
		public string Version { get; set; }

		public override bool Execute()
		{
			try
			{
				var info = FileVersionInfo.GetVersionInfo(File.ItemSpec);
				Version = info.FileVersion;
			}
			catch (Exception ex)
			{
					Log.LogErrorFromException(ex);
			}

			return !Log.HasLoggedErrors;
		}
	}
}