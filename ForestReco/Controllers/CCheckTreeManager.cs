using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using ObjParser;

namespace ForestReco
{
	public static class CCheckTreeManager
	{
		public static List<CCheckTree> Trees = new List<CCheckTree>();
		public static string checkFileName;


		public static void Init()
		{
			if (CProjectData.loadCheckTrees)
			{
				LoadTrees(checkFileName);
			}
		}

		private static void LoadTrees(string pFileName)
		{
			DateTime loadTreesStartTime = DateTime.Now;
			Console.WriteLine("Load check trees: ");
			Console.WriteLine(" - " + pFileName);

			string fullFilepath = CPlatformManager.GetPodkladyPath() + "\\check\\" + pFileName + ".txt";

			string[] allLines = File.ReadAllLines(fullFilepath);
			foreach (string line in allLines)
			{
				Tuple<int, Vector3> parsedLine = CCheckTreeTxtParser.ParseLine(line, false);
				if (parsedLine != null && IsCheckTree(parsedLine))
				{
					Trees.Add(new CCheckTree(parsedLine.Item1, parsedLine.Item2));
				}
			}

			Console.WriteLine("\nduration = " + (DateTime.Now - loadTreesStartTime).TotalSeconds);
			
		}

		private static bool IsCheckTree(Tuple<int, Vector3> pParsedLine)
		{
			switch (pParsedLine.Item1)
			{
				case 11:
				case 12:
				case 13:
					return true;
			}
			return false;
		}

	}
}