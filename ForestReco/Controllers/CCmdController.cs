using System.Diagnostics;
using System.IO;

namespace ForestReco
{
	public static class CCmdController
	{
		private static string LasToolsFolder => CParameterSetter.GetStringSettings(ESettings.lasToolsFolderPath);

		public static string[] GetHeaderLines(string pForestFileFullPath)
		{
			string infoFileName = Path.GetFileNameWithoutExtension(pForestFileFullPath) + "_i.txt";
			string tmpFolder = CParameterSetter.GetStringSettings(ESettings.tmpFilesFolderPath);
			string infoFileFullPath = tmpFolder + "//" + infoFileName;
			bool infoFileExists = File.Exists(infoFileFullPath);
			CDebug.WriteLine($"info file: {infoFileName} exists = {infoFileExists}");

			if(!infoFileExists)
			{
				string info =
					"/C " +
					"lasinfo " +
					pForestFileFullPath +
					" -o " +
					infoFileFullPath;

				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					WorkingDirectory = LasToolsFolder,
					FileName = "cmd.exe",
					Arguments = info
				};

				Process currentProcess = Process.Start(processStartInfo);
				//currentProcess = Process.Start("CMD.exe", info);
				currentProcess.WaitForExit();

				int result = currentProcess.ExitCode;

				//todo: throw and handle exception?
				if(result == 1) //0 = OK, 1 = error...i.e. the .exe file is missing
				{
					CDebug.Error("GetHeaderLines -lasinfo error");
					return null;
				}

				/*while(!currentProcess.HasExited)
				{
					Thread.Sleep(1000);
					CDebug.WriteLine("waiting for lasinfo to finish");
				}*/
			}

			return CProgramLoader.GetFileLines(infoFileFullPath, 20);
		}


		//TODO: not working! output always = ""
		//	// Start the child process.
		//	Process p = new Process();
		//	// Redirect the output stream of the child process.

		//	p.StartInfo.UseShellExecute = false;
		//		p.StartInfo.RedirectStandardOutput = true;

		//		string info =
		//				"/C " +
		//				"lasinfo " +
		//				fullFilePath
		//		;

		//	p.StartInfo = new ProcessStartInfo()
		//	{
		//		FileName = "CMD.exe",
		//			Arguments = info,
		//			UseShellExecute = false,
		//			RedirectStandardOutput = true,
		//			//CreateNoWindow = true
		//		};

		//	p.Start();

		//		while(!p.HasExited)
		//		{
		//			Thread.Sleep(1000);
		//			CDebug.WriteLine("waiting for lassplit to finish");
		//		}

		//string output = p.StandardOutput.ReadToEnd();
	}
}
