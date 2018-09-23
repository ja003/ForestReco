using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public static class CTreeManager
	{
		public static List<CTree> Trees { get; } = new List<CTree>();

		//public const float DEFAULT_TREE_EXTENT = 1.5f;
		public const float DEFAULT_TREE_EXTENT = 1f;
		public const float DEFAULT_TREE_HEIGHT = 10;

		//public const float MIN_PEAKS_DISTANCE = DEFAULT_TREE_EXTENT;
		public static float GetMinPeakDistance(float pMultiply)
		{
			return DEFAULT_TREE_EXTENT * pMultiply;
		}

		public static float MAX_BRANCH_ANGLE = 45;
		private static int treeIndex;

		public static bool DEBUG = false;
		private static int pointCounter;

		public static void AddPoint(Vector3 pPoint, int pPointIndex)
		{
			//CTreePoint treePoint = new CTreePoint(pPoint);

			//if (pPointIndex == 32)
			if (pPointIndex == 335)
			{
				Console.Write("!");
			}
			if (Vector3.Distance(pPoint, new Vector3(679.07f, 135.63f, 1159.93f)) < 0.1f)
			{
				Console.Write("!");
			}

			if (DEBUG) Console.WriteLine("\n" + pointCounter + " AddPoint " + pPoint);
			pointCounter++;
			CTree selectedTree = null;

			List<CTree> possibleTrees = GetPossibleTreesFor(pPoint);


			foreach (CTree t in possibleTrees)
			{
				if (DEBUG) Console.WriteLine("- try add to : " + t.ToString(false, false, true, false));
				//CTreePoint peak = t.peak;
				if (t.TryAddPoint(pPoint, false))
				{
					selectedTree = t;
					if (t.treeIndex == 6)
					{
						float dist = CUtils.Get2DDistance(pPoint, t.Center);
						float yDiff = t.Center.Y - pPoint.Y;
						if (dist > 1.3f)
						{
							Console.WriteLine("\nAdd " + pPoint + " [" + pPointIndex + "]");
							Console.WriteLine("dist = " + dist);
							Console.WriteLine("yDiff = " + yDiff);
						}
					}
					break;
				}
			}
			if (selectedTree != null)
			{
				/*foreach (CTree t in possibleTrees)
				{
					if (!Equals(selectedTree, t))
					{
						selectedTree = TryMergeTrees(selectedTree, t);
					}
				}*/
			}
			else// if (selectedTree == null)
			{
				Console.WriteLine("TREE " + treeIndex + ": " + pPointIndex + " new tree " + pPoint);
				Trees.Add(new CTree(pPoint, treeIndex));
				treeIndex++;
			}

			if (Trees.Count == 1 && pPointIndex != Trees[0].Points.Count - 1)
			{
				Console.WriteLine(pPointIndex + "Error. Incorrect point count. " + pPoint);
			}
		}

		private static CTree TryMergeTrees(CTree pTree1, CTree pTree2)
		{
			CTree higherTree = pTree1.peak.Y >= pTree2.peak.Y ? pTree1 : pTree2;
			CTree lowerTree = pTree1.peak.Y < pTree2.peak.Y ? pTree1 : pTree2;

			if(higherTree.treeIndex == 5 && lowerTree.treeIndex == 7){
				Console.Write("!");
			}

			Vector3 lowerTreePeak = lowerTree.peak.Center;
			//Vector3 higherTreePeak = higherTree.peak.GetClosestPointTo(lowerTreePeak);
			Vector3 higherTreePeak = higherTree.peak.Center;
			float angle = CUtils.AngleBetweenThreePoints(new List<Vector3>
			{
				higherTreePeak - Vector3.UnitY, higherTree.peak.Center, lowerTreePeak
			}, Vector3.UnitY);

			float maxBranchAngle = CTree.GetMaxBranchAngle(higherTreePeak, lowerTreePeak);
			float distBetweenPeaks = CUtils.Get2DDistance(pTree1.peak.Center, pTree2.peak.Center);

			//tree peaks must be close to each other and lower peak must be in appropriate angle with higher peak
			if (distBetweenPeaks < GetMinPeakDistance(1.5f) && angle < maxBranchAngle)
			{
				if (lowerTree.treeIndex == 7)
				{
					Console.WriteLine("\nMerge " + higherTree + " with " + lowerTree);
					Console.WriteLine("distBetweenPeaks = " + distBetweenPeaks + ". angle = " + angle);
				}
				higherTree.MergeWith(lowerTree);
				Trees.Remove(lowerTree);
			}
			return higherTree;
		}

		private static List<CTree> GetPossibleTreesFor(Vector3 pPoint)
		{
			List<CTree> possibleTrees = new List<CTree>();
			foreach (CTree t in Trees)
			{
				//const float MAX_DIST_TO_TREE_BB = 0.1f;
				//it must be close to peak of some tree
				/*if (CUtils.Get2DDistance(pPoint, t.peak.Center) < MAX_TREE_EXTENT / 2 ||
					//or to its BB
				    t.Get2DDistanceFromBBTo(pPoint) < MAX_DIST_TO_TREE_BB)*/

				if (t.BelongsToTree(pPoint, false))
				{
					possibleTrees.Add(t);
					t.possibleNewPoint = pPoint;
				}
			}
			//sort trees by angle of given point to tree
			//possibleTrees.Sort((x, y) => CUtils.GetAngleToTree(x, x.possibleNewPoint).CompareTo(
			//	CUtils.GetAngleToTree(y, y.possibleNewPoint)));

			//sort trees by 2D distance of given point to them
			possibleTrees.Sort((x, y) => CUtils.Get2DDistance(x.peak.Center, pPoint).CompareTo(
				CUtils.Get2DDistance(y.peak.Center, pPoint)));
			return possibleTrees;
		}

		public static void WriteResult()
		{
			foreach (CTree t in Trees)
			{
				Console.WriteLine(Trees.IndexOf(t).ToString("00") + ": " + t);
				if (Trees.IndexOf(t) > 100)
				{
					Console.WriteLine("too much...");
					return;
				}
			}
		}

		public static void TryMergeAllTrees()
		{
			DateTime mergeStartTime = DateTime.Now;
			Console.WriteLine("TryMergeAllTrees. Start = " + mergeStartTime);

			Trees.Sort((x, y) => x.peak.Center.Y.CompareTo(y.peak.Center.Y));
			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				if (i >= Trees.Count)
				{
					Console.WriteLine("Tree was deleted");
					continue;
				}
				CTree tree = Trees[i];
				float treeHeight = tree.GetTreeHeight();
				Console.WriteLine(i + " = " + treeHeight);

				List<CTree> possibleTrees = GetPossibleTreesFor(tree.peak.Center);

				foreach (CTree t in possibleTrees)
				{
					if (!Equals(tree, t))
					{
						tree = TryMergeTrees(tree, t);
						//they were merged
						//if (tree.treeIndex == t.treeIndex)
						//{
						//	Console.WriteLine("Merge");
						//}
					}
				}
			}
			Console.WriteLine("Trees merged | duration = " + (DateTime.Now - mergeStartTime));
		}

		public static void ProcessAllTrees()
		{
			DateTime processTreesStartTime = DateTime.Now;
			Console.WriteLine("ProcessAllTrees. Start = " + processTreesStartTime); foreach (CTree t in Trees)
			{
				bool testTranslate = false;
				if (testTranslate)
				{
					//t.Rotate(-45);
					t.Scale(2);
				}
				t.Process();
			}
			Console.WriteLine("Trees processed | duration = " + (DateTime.Now - processTreesStartTime));

			if (CProjectData.exportTrees)
			{
				Console.WriteLine("\nAdd trees to export " + Trees.Count + " | " + DateTime.Now);
				foreach (CTree t in Trees)
				{
					Obj tObj = t.GetObj("tree_" + Trees.IndexOf(t), true, false);
					CProjectData.objsToExport.Add(tObj);
				}
			}
		}
	}
}