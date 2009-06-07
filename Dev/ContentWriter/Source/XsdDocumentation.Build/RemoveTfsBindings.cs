using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XsdDocumentation.Build
{
	public sealed class RemoveTfsBindings : Task
	{
		private ITaskItem[] _solutionFiles;

		#region Helpers

		public static void Remove(string solutionFileName)
		{
			CleanSolution(solutionFileName);
			var projectFileNames = GetProjectsFromSolution(solutionFileName);
			foreach (var projectFileName in projectFileNames)
				CleanProject(projectFileName);
		}

		private static string[] GetProjectsFromSolution(string solutionFilename)
		{
			var projectList = new List<string>();
			var solutionDirectory = Path.GetDirectoryName(solutionFilename);

			var regex = new Regex(@"Project\(\""\{[A-Z|\d|\-|a-z]*\}\""\)\s*=\s*\""\S*\""\s*,\s*\""(?<FileName>[^\""]*)\""\s*,\s*",
			                      RegexOptions.IgnoreCase |
			                      RegexOptions.Multiline |
			                      RegexOptions.IgnorePatternWhitespace |
			                      RegexOptions.Compiled);

			using (var sr = new StreamReader(solutionFilename))
			{
				var line = sr.ReadLine();

				while (line != null)
				{
					if (line == "Global") //Projects definition were before this point
						break;

					var match = regex.Match(line);
					if (match.Success)
					{
						var fileName = match.Groups["FileName"].Value;
						var fullFileName = Path.Combine(solutionDirectory, fileName);

						// Solution folder appear as projects. So we need to check whether
						// the file name is an directory. In this case it is a solution
						// folder an not a project.

						if (!Directory.Exists(fullFileName))
							projectList.Add(fullFileName);
					}

					line = sr.ReadLine();
				}
			}

			return projectList.ToArray();
		}

		private static void RemoveReadOnlyFlag(string path)
		{
			var attributes = File.GetAttributes(path);
			if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				File.SetAttributes(path, attributes & (~FileAttributes.ReadOnly));
		}

		private static void RemoveNodeWhenExists(string xpath, XmlNode node, XmlNamespaceManager namespaceManager)
		{
			var nodeToDelete = node.SelectSingleNode(xpath, namespaceManager);
			if (nodeToDelete != null)
				node.RemoveChild(nodeToDelete);
		}

		private static void CleanSolution(string solutionFileName)
		{
			// Delete solution user options if they exists.
			var suoFileName = Path.ChangeExtension(solutionFileName, ".suo");
			if (File.Exists(suoFileName))
			{
				RemoveReadOnlyFlag(suoFileName);
				File.Delete(suoFileName);
			}

			var oldSolutionFileLines = File.ReadAllLines(solutionFileName, Encoding.UTF8);
			var newSolutionFileLines = new List<string>();
			var inSourceCodeSection = false;
			foreach (var line in oldSolutionFileLines)
			{
				var trimmedLine = line.Trim();

				if (trimmedLine == "GlobalSection(TeamFoundationVersionControl) = preSolution")
				{
					inSourceCodeSection = true;
				}
				else if (trimmedLine == "EndGlobalSection" && inSourceCodeSection)
				{
					inSourceCodeSection = false;
				}
				else if (!inSourceCodeSection)
				{
					newSolutionFileLines.Add(line);
				}
			}

			RemoveReadOnlyFlag(solutionFileName);
			File.WriteAllLines(solutionFileName, newSolutionFileLines.ToArray(), Encoding.UTF8);
		}

		private static void CleanProject(string projectFileName)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.Load(projectFileName);

			var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			namespaceManager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

			var propertyGroups = xmlDocument.SelectNodes("/msbuild:Project/msbuild:PropertyGroup", namespaceManager);
			if (propertyGroups != null)
			{
				foreach (XmlNode propertyGroup in propertyGroups)
				{
					RemoveNodeWhenExists("msbuild:SccProjectName", propertyGroup, namespaceManager);
					RemoveNodeWhenExists("msbuild:SccAuxPath", propertyGroup, namespaceManager);
					RemoveNodeWhenExists("msbuild:SccLocalPath", propertyGroup, namespaceManager);
					RemoveNodeWhenExists("msbuild:SccProvider", propertyGroup, namespaceManager);
				}
			}

			RemoveReadOnlyFlag(projectFileName);
			xmlDocument.Save(projectFileName);
		}

		#endregion

		public override bool Execute()
		{
			foreach (var solutionFile in _solutionFiles)
			{
				try
				{
					Remove(solutionFile.ItemSpec);
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
				}
			}

			return !Log.HasLoggedErrors;
		}

		[Required]
		public ITaskItem[] SolutionFiles
		{
			get { return _solutionFiles; }
			set { _solutionFiles = value; }
		}
	}
}