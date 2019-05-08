using System.Diagnostics;
using System.IO;

namespace ForestReco
{
	public static class CCmdController
	{
		private static string LasToolsFolder => CParameterSetter.GetStringSettings(ESettings.lasToolsFolderPath);
		private static string tmpFolder => CParameterSetter.GetStringSettings(ESettings.tmpFilesFolderPath) + "\\";

		public static string[] GetHeaderLines(string pForestFileFullPath)
		{
			string infoFileName = Path.GetFileNameWithoutExtension(pForestFileFullPath) + "_i.txt";
			string infoFileFullPath = tmpFolder + infoFileName;

			string command =
					"lasinfo " +
					pForestFileFullPath +
					" -o " +
					infoFileFullPath;

			RunLasToolsCmd(command, infoFileName);

			return CProgramLoader.GetFileLines(infoFileFullPath, 20);
		}

		public static void RunLasToolsCmd(string pLasToolCommand, string pOutputFileName)
		{
			string outputFilePath = tmpFolder + pOutputFileName;
			bool outputFileExists = File.Exists(outputFilePath);
			CDebug.WriteLine($"split file: {pOutputFileName} exists = {outputFileExists}");

			if(!outputFileExists)
			{
				string command = "/C " 
					//+"pushd " + LasToolsFolder + " "
					+ pLasToolCommand;

				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					WorkingDirectory = LasToolsFolder,
					FileName = "CMD.exe",
					Arguments = command
				};

				Process currentProcess = Process.Start(processStartInfo);
				//currentProcess = Process.Start("CMD.exe", info);
				currentProcess.WaitForExit();

				int result = currentProcess.ExitCode;

				//todo: throw and handle exception?
				if(result == 1) //0 = OK, 1 = error...i.e. the .exe file is missing
				{
					CDebug.Error("GetHeaderLines -lasinfo error");
					//return null;
				}
			}
			//return outputFilePath;

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
