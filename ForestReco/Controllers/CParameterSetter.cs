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
		public static string forrestFilePath;
		public static string reftreeFolderPath;
		public static string outputFolderPath;
		public static string checkTreeFilePath;

		public static bool consoleVisible;
		
		public static int partitionStep;
		public static int avgTreeHeigh;
		public static float groundArrayStep;
		public static float treeExtent;
		public static float treeExtentMultiply;

		//bools
		public static bool exportTreeStructures;
		public static bool exportInvalidTrees;
		public static bool exportRefTrees;
		public static bool assignRefTreesRandom;
		public static bool useReducedReftreeModels;
		public static bool useCheckTreeFile;
		public static bool exportCheckTrees;


		public static void Init()
		{
			forrestFilePath = (string)GetSettings(ParamInfo.Name(()=>forrestFilePath));
			reftreeFolderPath = (string)GetSettings(ParamInfo.Name(()=>reftreeFolderPath));
			outputFolderPath = (string)GetSettings(ParamInfo.Name(()=>outputFolderPath));
			checkTreeFilePath = (string)GetSettings(ParamInfo.Name(()=>checkTreeFilePath));
			consoleVisible = (bool)GetSettings(ParamInfo.Name(()=>consoleVisible));
			partitionStep = (int)GetSettings(ParamInfo.Name(()=>partitionStep));

			avgTreeHeigh = (int)GetSettings(ParamInfo.Name(()=>avgTreeHeigh));
			groundArrayStep = (float)GetSettings(ParamInfo.Name(()=>groundArrayStep));
			treeExtent = (float)GetSettings(ParamInfo.Name(()=>treeExtent));
			treeExtentMultiply = (float)GetSettings(ParamInfo.Name(()=>treeExtentMultiply));

			//bools
			exportTreeStructures = (bool)GetSettings(ParamInfo.Name(()=>exportTreeStructures));
			exportInvalidTrees = (bool)GetSettings(ParamInfo.Name(()=> exportInvalidTrees));
			exportRefTrees = (bool)GetSettings(ParamInfo.Name(()=> exportRefTrees));
			assignRefTreesRandom = (bool)GetSettings(ParamInfo.Name(()=> assignRefTreesRandom));
			useReducedReftreeModels = (bool)GetSettings(ParamInfo.Name(()=> useReducedReftreeModels));
			useCheckTreeFile = (bool)GetSettings(ParamInfo.Name(()=> useCheckTreeFile));
			exportCheckTrees = (bool)GetSettings(ParamInfo.Name(()=> exportCheckTrees));

			if (!consoleVisible)
			{
				IntPtr handle = CConsole.GetConsoleWindow();
				CConsole.ShowWindow(handle, SW_HIDE);
			}
		}

		private static object GetSettings(string pKey)
		{
			return Properties.Settings.Default[pKey];
		}

		public static void SetParameter(string paramKey, object pArg)
		{
			if (paramKey == ParamInfo.Name(()=>forrestFilePath))
			{
				forrestFilePath = (string)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>reftreeFolderPath))
			{
				reftreeFolderPath = (string)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>partitionStep))
			{
				partitionStep = (int)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>treeExtent))
			{
				treeExtent = (float)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>groundArrayStep))
			{
				groundArrayStep = (float)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>consoleVisible))
			{
				consoleVisible = (bool)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>outputFolderPath))
			{
				outputFolderPath = (string)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>checkTreeFilePath))
			{
				checkTreeFilePath = (string)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>treeExtentMultiply))
			{
				treeExtentMultiply = (float)pArg;
			}
			else if (paramKey == ParamInfo.Name(()=>avgTreeHeigh))
			{
				avgTreeHeigh = (int)pArg;
			}

			//bools
			else if (paramKey == ParamInfo.Name(()=>exportTreeStructures))
			{
				exportTreeStructures = (bool)pArg;
			}
			else if (paramKey == ParamInfo.Name(() => exportInvalidTrees))
			{
				exportInvalidTrees = (bool)pArg;
			}
			else if (paramKey == ParamInfo.Name(() => exportRefTrees))
			{
				exportRefTrees = (bool)pArg;
			}
			else if (paramKey == ParamInfo.Name(() => assignRefTreesRandom))
			{
				assignRefTreesRandom = (bool)pArg;
			}
			else if (paramKey == ParamInfo.Name(() => useReducedReftreeModels))
			{
				useReducedReftreeModels = (bool)pArg;
			}
			else if (paramKey == ParamInfo.Name(() => useCheckTreeFile))
			{
				useCheckTreeFile = (bool)pArg;
			}
			else if (paramKey == ParamInfo.Name(() => exportCheckTrees))
			{
				exportCheckTrees = (bool)pArg;
			}

			else
			{
				CDebug.Error($"key {paramKey} not set");
			}
			
			Properties.Settings.Default[paramKey] = pArg;
			Properties.Settings.Default.Save();
			//return pArg;
		}

		//public static string SelectFolder(string pParamKey)
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

		public static string SelectFile(string pTitle)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.RestoreDirectory = true;
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
}