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
		public static List<CCheckTree> Trees { get; private set; }
		public static string checkFileName;

		public static void Init()
		{
			Trees = new List<CCheckTree>();
			if (CProjectData.loadCheckTrees)
			{
				CDebug.Step(EProgramStep.LoadCheckTrees);
				LoadTrees(checkFileName);
			}

			CDebug.Step(EProgramStep.AssignCheckTrees);
			AssignTrees();

			ValidateTrees();

			CProjectData.array.DebugCheckTrees();

			if (CProjectData.exportCheckTrees)
			{
				CObjPartition.AddCheckTrees(false);
			}

			CAnalytics.loadedCheckTrees = Trees.Count;
			CAnalytics.assignedCheckTrees = GetAssignedTreesCount();
			CAnalytics.invalidCheckTrees = GetInvalidTreesCount();
		}

		public static void ValidateTrees()
		{
			foreach (CCheckTree tree in Trees)
			{
				tree.Validate();
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

			if (CProjectData.array == null)
			{
				CDebug.Error("array not initialized");
				return;
			}
			bool checkTreeAdded = CProjectData.array.AddCheckTree(ref newTree);
			if (checkTreeAdded)
			{
				Trees.Add(newTree);
			}
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

		private static int GetAssignedTreesCount()
		{
			return Trees.Count(tree => tree.assignedTree != null);
		}

		private static int GetInvalidTreesCount()
		{
			return Trees.Count(tree => tree.isInvalid);
		}

	}
}