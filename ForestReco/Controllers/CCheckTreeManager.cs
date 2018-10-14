﻿using System;
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

			AssignTrees();

			CProjectData.array.DebugCheckTrees();

			if (CProjectData.exportCheckTrees)
			{
				CObjPartition.AddCheckTrees(false);
			}
		}

		public static void AssignTrees()
		{
			foreach (CCheckTree checkTree in Trees)
			{
				Vector3 treePosition = checkTree.position;
				List<CTree> possibleTrees = CTreeManager.GetPossibleTreesFor(treePosition, CTreeManager.EPossibleTreesMethos.ClosestHigher);

				if (checkTree.index == 529)
				{
					//CDebug.WriteLine("");
				}

				if (possibleTrees.Count > 0)
				{
					foreach (CTree possibleTree in possibleTrees)
					{
						float distToTree = CUtils.Get2DDistance(treePosition, possibleTree.peak.Center);

						if (possibleTree.assignedCheckTree != null)
						{
							//this checkTree is closer to tree than its current checkTree 
							if (distToTree < CUtils.Get2DDistance(possibleTree.assignedCheckTree.position, possibleTree.peak.Center))
							{
								possibleTree.assignedCheckTree.AssignTree(null);
								checkTree.AssignTree(possibleTree);
								break;
							}
						}

						else if (possibleTree.isValid  && distToTree < CTreeManager.BASE_TREE_EXTENT)
						{
							checkTree.AssignTree(possibleTree);
							break;
						}
					}
				}
			}
		}

		private static void LoadTrees(string pFileName)
		{
			DateTime loadTreesStartTime = DateTime.Now;
			CDebug.WriteLine("Load check trees: " + pFileName, true);

			string fullFilepath = CPlatformManager.GetPodkladyPath() + "\\check\\" + pFileName + ".txt";

			string[] allLines = File.ReadAllLines(fullFilepath);

			//CProjectData.array.WriteBounds();

			foreach (string line in allLines)
			{
				Tuple<int, Vector3> parsedLine = CCheckTreeTxtParser.ParseLine(line, true);
				if (parsedLine != null && IsCheckTree(parsedLine))
				{
					AddNewTree(parsedLine);
				}
			}
			//CCheckTreeTxtParser.Debug();

			CDebug.Duration("Load check trees", loadTreesStartTime);
		}

		private static void AddNewTree(Tuple<int, Vector3> pParsedLine)
		{
			CCheckTree newTree = new CCheckTree(pParsedLine.Item1, pParsedLine.Item2, Trees.Count);
			Trees.Add(newTree);

			if (CProjectData.array == null)
			{
				CDebug.Error("array not initialized");
				return;
			}
			CProjectData.array.AddCheckTree(ref newTree);
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