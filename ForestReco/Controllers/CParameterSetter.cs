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



		public static void Init()
		{
			bool containsForrestPathKey = Properties.Settings.Default.SettingsKey.Contains("forrestFilePath");
			if (containsForrestPathKey)
			{
				object forrestFilePathSettings = Properties.Settings.Default["forrestFilePath"];
				forrestFilePath = (string)forrestFilePathSettings;
			}
		}

		private static string SetForrestFilePath(string pPath)
		{
			forrestFilePath = pPath;
			Properties.Settings.Default["forrestFilePath"] = pPath;
			Properties.Settings.Default.Save();
			return pPath;
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
				return SetForrestFilePath(ofd.FileName);

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
	}
}