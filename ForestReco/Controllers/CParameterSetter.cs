using System;
using System.Drawing;
using System.IO;
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
		public static float groundArrayStep;
		public static float treeExtent;
		public static float treeExtentMultiply;

		public const string forrestFilePathKey = "forrestFilePath";
		public const string reftreeFolderPathKey = "reftreeFolderPath";
		public const string outputFolderPathKey = "outputFolderPath";
		public const string consoleVisibleKey = "consoleVisible";
		public const string partitionStepKey = "partitionStep";
		public const string groundArrayStepKey = "groundArrayStep";
		public const string treeExtentKey = "treeExtent";
		public const string treeExtentMultiplyKey = "treeExtentMultiply";


		public const string checkTreeFilePathKey = "checkTreeFilePath";
		
		public static void Init()
		{
			forrestFilePath = (string)GetSettings(forrestFilePathKey);
			reftreeFolderPath = (string)GetSettings(reftreeFolderPathKey);
			outputFolderPath = (string)GetSettings(outputFolderPathKey);
			checkTreeFilePath = (string)GetSettings(checkTreeFilePathKey);
			consoleVisible = (bool)GetSettings(consoleVisibleKey);
			partitionStep = (int)GetSettings(partitionStepKey);
			groundArrayStep = (float)GetSettings(groundArrayStepKey);
			treeExtent = (float)GetSettings(treeExtentKey);
			treeExtentMultiply = (float)GetSettings(treeExtentMultiplyKey);

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

		public static void SetParameter(string pParamKey, object pArg)
		{
			switch (pParamKey)
			{
				case forrestFilePathKey:
					forrestFilePath = (string)pArg;

					break;
				case reftreeFolderPathKey:
					reftreeFolderPath = (string)pArg;
					break;

				case outputFolderPathKey:
					outputFolderPath = (string)pArg;
					break;

				case consoleVisibleKey:
					consoleVisible = (bool)pArg;
					break;

				case checkTreeFilePathKey:
					checkTreeFilePath = (string)pArg;
					break;

				case groundArrayStepKey:
					groundArrayStep = (float)pArg;
					break;

				case partitionStepKey:
					partitionStep = (int)pArg;
					break;

				case treeExtentKey:
					treeExtent = (float)pArg;
					break;
				case treeExtentMultiplyKey:
					treeExtentMultiply = (float)pArg;
					break;

				default:
				CDebug.Error($"key {pParamKey} not set");
				break;
			}

			Properties.Settings.Default[pParamKey] = pArg;
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
			SetParameter(consoleVisibleKey, !consoleVisible);
		}
	}
}