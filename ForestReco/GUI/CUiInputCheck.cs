using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public static class CUiInputCheck
	{
		private static List<string> problems = new List<string>();

		private static void Reset()
		{
			problems = new List<string>();
		}

		private static bool CheckPath(string pTitle, string pPath, bool pFile) //false = folder
		{
			if (pFile)
			{
				bool fileExists = File.Exists(pPath);
				if (!fileExists)
				{
					problems.Add($"{pTitle} file not found: {pPath}");
					return false;
				}
			}
			else
			{
				bool folderExists = Directory.Exists(pPath);
				if (!folderExists)
				{
					problems.Add($"{pTitle} folder not found: {pPath}");
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// True = everything ok
		/// </summary>
		public static bool CheckProblems()
		{
			Reset();
			CheckPath("Forrest", CParameterSetter.GetStringSettings(ESettings.forrestFilePath), true);
			CheckPath("Reftree", CParameterSetter.GetStringSettings(ESettings.reftreeFolderPath), false);
			CheckPath("Output", CParameterSetter.GetStringSettings(ESettings.outputFolderPath), false);
			if (CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile))
			{
				CheckPath("Checktree", CParameterSetter.GetStringSettings(ESettings.checkTreeFilePath), true);
			}

			CheckExportTrees();

			bool hasProblems = problems.Count > 0;
			if (hasProblems)
			{
				CDebug.WriteProblems(problems);
			}
			return !hasProblems;
		}

		private static void CheckExportTrees()
		{
			bool exportTreeStructures = CParameterSetter.GetBoolSettings(ESettings.exportTreeStructures);
			bool exportReftrees = CParameterSetter.GetBoolSettings(ESettings.exportRefTrees);
			bool exportTreeBoxes = CParameterSetter.GetBoolSettings(ESettings.exportTreeBoxes);
			if (!exportTreeStructures && !exportReftrees && !exportTreeBoxes)
			{
				problems.Add($"No reason to process when exportReftrees, exportTreeStructures and exportTreeBoxes are false. Result will be empty.");
			}
		}
	}
}
