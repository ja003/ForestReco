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

			if (CProjectData.exportCheckTrees)
			{
				CObjPartition.AddCheckTrees(false);
			}
		}

		private static void LoadTrees(string pFileName)
		{
			DateTime loadTreesStartTime = DateTime.Now;
			Console.WriteLine("Load check trees: ");
			Console.WriteLine(" - " + pFileName);

			string fullFilepath = CPlatformManager.GetPodkladyPath() + "\\check\\" + pFileName + ".txt";

			string[] allLines = File.ReadAllLines(fullFilepath);

			CProjectData.array.WriteBounds();

			foreach (string line in allLines)
			{
				Tuple<int, Vector3> parsedLine = CCheckTreeTxtParser.ParseLine(line, true);
				if (parsedLine != null && IsCheckTree(parsedLine))
				{
					AddNewTree(parsedLine);
				}
			}
			CCheckTreeTxtParser.Debug();

			Console.WriteLine("\nduration = " + (DateTime.Now - loadTreesStartTime).TotalSeconds);
			
		}

		//public static void AddTreesToExport()
		//{
		//	foreach (CCheckTree tree in Trees)
		//	{
		//		CProjectData.
		//	}
		//}

		private static void AddNewTree(Tuple<int, Vector3> pParsedLine)
		{
			CCheckTree newTree = new CCheckTree(pParsedLine.Item1, pParsedLine.Item2, Trees.Count);
			Trees.Add(newTree);

			if (CProjectData.array == null)
			{
				Console.WriteLine("Error: array not initialized");
				return;
			}
			CProjectData.array.AddCheckTree(newTree);
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