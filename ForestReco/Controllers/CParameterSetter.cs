using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace ForestReco
{
	public static class CParameterSetter
	{
		public static float treeExtent => GetFloatSettings(ESettings.treeExtent);
		public static float treeExtentMultiply => GetFloatSettings(ESettings.treeExtentMultiply);
		public static float groundArrayStep => GetFloatSettings(ESettings.groundArrayStep);

		/// <summary>
		/// Returns saved "tmp files" folder path with added "\\" 
		/// - if no value is specified => creates "tmp" folder in StartupPath
		/// </summary>
		public static string TmpFolder
		{
			get
			{
				string savedVal = GetStringSettings(ESettings.tmpFilesFolderPath);
				//use app path as tmp folder if no folder is specified
				if(!Directory.Exists(savedVal))
				{
					string newTmpFolder = Application.StartupPath + "\\tmp";
					Directory.CreateDirectory(newTmpFolder);
					SetParameter(ESettings.tmpFilesFolderPath, newTmpFolder);
					savedVal = newTmpFolder;
				};
				return savedVal + "\\";
			}
		}

		private static float GetRangeSettings(ESettings pRangeSetting)
		{
			switch(pRangeSetting)
			{
				case ESettings.rangeXmax:
					return GetIntSettings(ESettings.rangeXmax) / 10f;
				case ESettings.rangeXmin:
					return GetIntSettings(ESettings.rangeXmin) / 10f;
				case ESettings.rangeYmax:
					return GetIntSettings(ESettings.rangeYmax) / 10f;
				case ESettings.rangeYmin:
					return GetIntSettings(ESettings.rangeYmin) / 10f;
			}
			CDebug.Error("GetRange - invalid arg");
			return 0;
		}

		/// <summary>
		/// Returns CSplitRange structure with valid values
		/// - min < max
		/// </summary>
		/// <returns></returns>
		public static SSplitRange GetSplitRange()
		{
			float min_x = GetRangeSettings(ESettings.rangeXmin);
			float max_x = GetRangeSettings(ESettings.rangeXmax);
			float min_y = GetRangeSettings(ESettings.rangeYmin);
			float max_y = GetRangeSettings(ESettings.rangeYmax);

			return new SSplitRange(
				Math.Min(min_x, max_x),
				Math.Min(min_y, max_y),
				Math.Max(min_x, max_x),
				Math.Max(min_y, max_y));
		}

		public static void Init()
		{
			if(!GetBoolSettings(ESettings.consoleVisible))
			{
				IntPtr handle = CConsole.GetConsoleWindow();
				CConsole.ShowWindow(handle, SW_HIDE);
			}
		}

		private static object GetSettings(string pKey)
		{
			return Properties.Settings.Default[pKey];
		}

		public static string GetStringSettings(ESettings pKey)
		{
			return (string)GetSettings(pKey.ToString());
		}

		public static float GetFloatSettings(ESettings pKey)
		{
			return (float)GetSettings(pKey.ToString());
		}

		public static int GetIntSettings(ESettings pKey)
		{
			return (int)GetSettings(pKey.ToString());
		}

		public static bool GetBoolSettings(ESettings pKey)
		{
			return (bool)GetSettings(pKey.ToString());
		}

		public static void SetParameter(ESettings pKey, object pArg)
		{
			SetParameter(pKey.ToString(), pArg);
		}

		public static void SetParameter(string paramKey, object pArg)
		{
			Properties.Settings.Default[paramKey] = pArg;
			Properties.Settings.Default.Save();
		}

		public static string SelectFolder()
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			DialogResult dr = fbd.ShowDialog();
			if(dr == DialogResult.OK)
			{
				return fbd.SelectedPath;
			}

			return "";
		}

		public static string SelectFile(string pTitle, string pExtension, string pFileDescription) =>
			SelectFile(pTitle, new List<string>() { pExtension }, pFileDescription);

		public static string SelectFile(string pTitle, List<string> pExtensions, string pFileDescription)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.RestoreDirectory = true;
			string extensionString = "";
			foreach(string e in pExtensions)
			{
				//extensionString += $"(*.{e})|*.{e}" + (pExtensions.IndexOf(e) == pExtensions.Count - 1 ? "" : ";");
				bool isLast = pExtensions.IndexOf(e) == pExtensions.Count - 1;
				extensionString += $"*.{e}" + (isLast ? "" : ";");
			}

			string filterText = $"{pFileDescription} files [{extensionString}] |{extensionString}";
			//string filterText = $"{extensionString}";
			ofd.Filter = filterText;
			//ofd.Filter = $"{pFileDescription} files (*.{pExtension})|*.{pExtension}";
			ofd.Title = pTitle;
			ofd.ShowHelp = true;
			DialogResult dr = ofd.ShowDialog();
			if(dr == DialogResult.OK)
			{
				return ofd.FileName;
			}
			return "";
		}

		private const int SW_HIDE = 0;
		private const int SW_SHOW = 5;

		public static void ToggleConsoleVisibility()
		{
			IntPtr handle = CConsole.GetConsoleWindow();
			bool consoleVisible = GetBoolSettings(ESettings.consoleVisible);
			CConsole.ShowWindow(handle, consoleVisible ? SW_HIDE : SW_SHOW);

			CDebug.WriteLine("ToggleConsoleVisibility " + !consoleVisible);
			SetParameter(ParamInfo.Name(() => consoleVisible), !consoleVisible);
		}
	}

	public static class ParamInfo
	{
		public static string Name<T>(Expression<Func<T>> memberExpression)
		{
			MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
			return expressionBody.Member.Name;
		}
	}

	public enum ESettings
	{
		None,
		//strings
		forestFilePath,
		reftreeFolderPath,
		outputFolderPath,
		checkTreeFilePath,
		analyticsFilePath,
		tmpFilesFolderPath,
		lasToolsFolderPath,

		//ints
		partitionStep,
		avgTreeHeigh,

		//floats
		groundArrayStep,
		treeExtent,
		treeExtentMultiply,
		rangeXmin,
		rangeXmax,
		rangeYmin,
		rangeYmax,

		//bools
		export3d,
		exportBitmap,
		exportTreeStructures,
		exportInvalidTrees,
		exportRefTrees,
		assignRefTreesRandom,
		useReducedReftreeModels,
		useCheckTreeFile,
		exportCheckTrees,
		consoleVisible,
		filterPoints,
		exportPoints,
		autoAverageTreeHeight,
		exportTreeBoxes,
		colorTrees,
	}
}