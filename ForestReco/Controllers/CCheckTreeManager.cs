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

		public static void Init()
		{
			Trees = new List<CCheckTree>();
			CDebug.Step(EProgramStep.LoadCheckTrees);
			if (CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile))
			{
				LoadTrees();
			}

			CDebug.Step(EProgramStep.AssignCheckTrees);
			AssignTrees();

			ValidateTrees();

			CProjectData.array.DebugCheckTrees();

			if (CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile))
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
			float maxDistToTree = CParameterSetter.treeExtent * CParameterSetter.treeExtentMultiply;

			foreach (CCheckTree checkTree in Trees)
			{
				Vector3 treePosition = checkTree.position;
				List<CTree> possibleTrees = CTreeManager.GetPossibleTreesFor(treePosition, CTreeManager.EPossibleTreesMethos.ClosestHigher);
				
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

						else
						{
							if (possibleTree.isValid && (distToTree < maxDistToTree || possibleTree.Contains2D(checkTree.position)))
							{
								checkTree.AssignTree(possibleTree);
								break;
							}
						}
					}
				}
			}
		}

		private static void LoadTrees()
		{
			DateTime loadTreesStartTime = DateTime.Now;

			string fullFilepath = CParameterSetter.GetStringSettings(ESettings.checkTreeFilePath);
			CDebug.WriteLine("Load check trees: " + fullFilepath, true);

			string[] allLines = File.ReadAllLines(fullFilepath);

			foreach (string line in allLines)
			{
				Tuple<int, Vector3, string> parsedLine = CCheckTreeTxtParser.ParseLine(line, true);
				if (parsedLine != null && IsCheckTree(parsedLine))
				{
					AddNewTree(parsedLine);
				}
			}
			//CCheckTreeTxtParser.Debug();

			CDebug.Duration("Load check trees", loadTreesStartTime);
		}

		private static void AddNewTree(Tuple<int, Vector3, string> pParsedLine)
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

		private static bool IsCheckTree(Tuple<int, Vector3, string> pParsedLine)
		{
			bool classOk = false;
			switch (pParsedLine.Item1)
			{
				case 11:
				case 12:
				case 13:
					classOk = true;
					break;
			}
			if (!classOk) { return false; }

			return true;
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