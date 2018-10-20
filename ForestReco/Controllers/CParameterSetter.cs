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

		public static bool consoleVisible;



		public const string forrestFilePathKey = "forrestFilePath";
		public const string reftreeFolderPathKey = "reftreeFolderPath";
		public const string outputFolderPathKey = "outputFolderPath";
		public const string consoleVisibleKey = "consoleVisible";

		public static void Init()
		{
			forrestFilePath = (string)GetSettings(forrestFilePathKey);
			reftreeFolderPath = (string)GetSettings(reftreeFolderPathKey);
			outputFolderPath = (string)GetSettings(outputFolderPathKey);
			consoleVisible = (bool)GetSettings(consoleVisibleKey);
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

		private static void SetParameter(string pParamKey, object pArg)
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
			}

			Properties.Settings.Default[pParamKey] = pArg;
			Properties.Settings.Default.Save();
			//return pArg;
		}

		public static string SelectFolder(string pParamKey)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			DialogResult dr = fbd.ShowDialog();
			if (dr == DialogResult.OK)
			{
				SetParameter(pParamKey, fbd.SelectedPath);
				return fbd.SelectedPath;
			}

			return "";
		}

		public static string SelectForrestFile()
		{
			OpenFileDialog ofd = new OpenFileDialog();
			//ofd.Multiselect = true;
			//ofd.InitialDirectory = Environment.CurrentDirectory;
			ofd.RestoreDirectory = true;
			ofd.Title = "Open forrest file";
			ofd.ShowHelp = true;
			DialogResult dr = ofd.ShowDialog();
			if (dr == DialogResult.OK)
			{
				SetParameter(forrestFilePathKey, ofd.FileName);
				return ofd.FileName;

				/*foreach (String file in ofd.FileNames)
				{
					try
					{
						if ())
						{
							
						}
						else
						{
							MessageBox.Show("selected file: " + file.ToString() + " is not an image!");
						}

					}
					catch (SecurityException ex)
					{
						// The user lacks appropriate permissions to read files, discover paths, etc.
						MessageBox.Show("Security error. \n\n" + "Error message: " + ex.Message + "\n\n" + ex.StackTrace
						);
					}
					catch (Exception ex)
					{
						// Could not load the image - probably related to Windows file system permissions.
						MessageBox.Show("Cannot load the image: " + file.Substring(file.LastIndexOf('\\'))
							 + ". You may not have permission to read the file, or " +
							 "it may be corrupt.\n\nReported error: " + ex.Message);
					}
				}*/
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