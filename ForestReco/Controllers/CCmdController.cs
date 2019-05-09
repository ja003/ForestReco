using System;
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
			string infoFilePath = tmpFolder + infoFileName;

			string command =
					"lasinfo " +
					pForestFileFullPath +
					" -o " +
					infoFilePath;

			RunLasToolsCmd(command, infoFilePath);

			return CProgramLoader.GetFileLines(infoFilePath, 20);
		}

		public static void RunLasToolsCmd(string pLasToolCommand, string pOutputFilePath)
		{
			//string outputFilePath = tmpFolder + pOutputFilePath;
			bool outputFileExists = File.Exists(pOutputFilePath);
			CDebug.WriteLine($"split file: {pOutputFilePath} exists = {outputFileExists}");

			if(!outputFileExists)
			{
				string command = "/C " + pLasToolCommand;

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
					throw new Exception($"Command {command} resulted in error");
				}
				// Check if command generated desired result
				outputFileExists = File.Exists(pOutputFilePath);
				if(!outputFileExists)
				{
					throw new Exception($"File {pOutputFilePath} not created");
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
