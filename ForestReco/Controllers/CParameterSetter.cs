using System;
using System.Drawing;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Windows.Forms;

namespace ForestReco
{
	public static class CParameterSetter
	{
		public static float treeExtent => GetFloatSettings(ESettings.treeExtent);
		public static float treeExtentMultiply => GetFloatSettings(ESettings.treeExtentMultiply);
		public static float groundArrayStep => GetFloatSettings(ESettings.groundArrayStep);
		
		public static void Init()
		{
			if (!GetBoolSettings(ESettings.consoleVisible))
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
			if (dr == DialogResult.OK)
			{
				return fbd.SelectedPath;
			}

			return "";
		}

		public static string SelectFile(string pTitle, string pExtension, string pFileDescription)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.RestoreDirectory = true;
			ofd.Filter = $"{pFileDescription} files (*.{pExtension})|*.{pExtension}";
			ofd.Title = pTitle;
			ofd.ShowHelp = true;
			DialogResult dr = ofd.ShowDialog();
			if (dr == DialogResult.OK)
			{
				return ofd.FileName;
			}
			return "";
		}


		const int SW_HIDE = 0;
		const int SW_SHOW = 5;

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

		//ints
		partitionStep,
		avgTreeHeigh,

		//floats
		groundArrayStep,
		treeExtent,
		treeExtentMultiply,

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