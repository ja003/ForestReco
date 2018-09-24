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

		/// <summary>
		/// Calculates minimal peak distance for given trees to be merged
		/// </summary>
		public static float GetMinPeakDistance(CTree pTree1, CTree pTree2)
		{
			float treeHeight = Math.Max(pTree1.GetTreeHeight(), pTree2.GetTreeHeight());
			float ratio = treeHeight / DEFAULT_TREE_HEIGHT;
			if (ratio < 1) { return DEFAULT_TREE_EXTENT; }
			const float EXTENT_VALUE_STEP = 1.5f;

			return GetMinPeakDistance(1) + (ratio - 1) * EXTENT_VALUE_STEP;
		}

		public const float MAX_BRANCH_ANGLE = 45;
		private static int treeIndex;

		public static bool DEBUG = false;
		private static int pointCounter;

		public static void AddPoint(Vector3 pPoint, int pPointIndex)
		{
			//CTreePoint treePoint = new CTreePoint(pPoint);

			//if (pPointIndex == 32)
			if (pPointIndex == 16572)
			{
				Console.Write("!");
			}
			//if (Vector3.Distance(pPoint, new Vector3(679.07f, 135.63f, 1159.93f)) < 0.1f)
			//{
			//	Console.Write("!");
			//}

			if (DEBUG) Console.WriteLine("\n" + pointCounter + " AddPoint " + pPoint);
			pointCounter++;
			CTree selectedTree = null;

			List<CTree> possibleTrees = GetPossibleTreesFor(pPoint, EPossibleTreesMethos.Closest);

			float bestAddPointFactor = 0;
			foreach (CTree t in possibleTrees)
			{
				if (DEBUG) Console.WriteLine("- try add to : " + t.ToString(false, false, true, false));
				//CTreePoint peak = t.peak;
				float addPointFactor = t.GetAddPointFactor(pPoint);
				if (addPointFactor > 0.5f && addPointFactor > bestAddPointFactor)
				{
					selectedTree = t;
					bestAddPointFactor = addPointFactor;
				}
				/*if (t.TryAddPoint(pPoint, false))
				{
					selectedTree = t;
					break;
				}*/
			}
			if (selectedTree != null)
			{
				if (DEBUG) { Console.WriteLine(bestAddPointFactor + " SELECTED TREE " + selectedTree + " for " + pPointIndex + ": " + pPoint); }
				selectedTree.AddPoint(pPoint);
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

		private static List<CTree> GetPossibleTreesFor(Vector3 pPoint, EPossibleTreesMethos pMethod)
		{
			List<CTree> possibleTrees = new List<CTree>();
			if (pMethod == EPossibleTreesMethos.Belongs)
			{
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
			}
			else if (pMethod == EPossibleTreesMethos.Closest)
			{
				possibleTrees.AddRange(Trees);
			}
			else if (pMethod == EPossibleTreesMethos.Contains)
			{
				foreach (CTree t in Trees)
				{
					if (t.Contains(pPoint))
					{
						possibleTrees.Add(t);
					}
				}
			}

			//sort trees by angle of given point to tree
			//possibleTrees.Sort((x, y) => CUtils.GetAngleToTree(x, x.possibleNewPoint).CompareTo(
			//	CUtils.GetAngleToTree(y, y.possibleNewPoint)));

			//sort trees by 2D distance of given point to them
			possibleTrees.Sort((x, y) => CUtils.Get2DDistance(x.peak.Center, pPoint).CompareTo(
				CUtils.Get2DDistance(y.peak.Center, pPoint)));

			if (pMethod == EPossibleTreesMethos.Closest)
			{
				List<CTree> closestTrees = new List<CTree>();
				const int maxClosestTreesCount = 3;
				int counter = 0;
				foreach (CTree possibleTree in possibleTrees)
				{
					closestTrees.Add(possibleTree);
					counter++;
					if (counter > maxClosestTreesCount)
					{
						break;
					}

				}
				return closestTrees;
			}

			return possibleTrees;
		}

		public enum EPossibleTreesMethos
		{
			Belongs,
			Closest,
			Contains
		}


		//MERGE

		public static void TryMergeAllTrees()
		{
			DateTime mergeStartTime = DateTime.Now;
			Console.WriteLine("TryMergeAllTrees. Start = " + mergeStartTime);

			Trees.Sort((x, y) => y.peak.Center.Y.CompareTo(x.peak.Center.Y));
			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				if (i >= Trees.Count)
				{
					Console.WriteLine("Tree was deleted");
					continue;
				}
				CTree tree = Trees[i];

				List<CTree> possibleTrees = GetPossibleTreesFor(tree.peak.Center, EPossibleTreesMethos.Contains);

				foreach (CTree t in possibleTrees)
				{
					if (!Equals(tree, t))
					{
						float peaksDist = CUtils.Get2DDistance(tree.peak, t.peak);
						float minPeakDistance = GetMinPeakDistance(tree, t);


						Console.WriteLine("\nTry merge " + tree + " and " + t);
						Console.WriteLine("peaksDist = " + peaksDist);
						Console.WriteLine("minPeakDistance = " + minPeakDistance);
						if (peaksDist < minPeakDistance)
						{
							tree = MergeTrees(tree, t);
						}
					}
				}
			}
			Console.WriteLine("Trees merged | duration = " + (DateTime.Now - mergeStartTime));
		}

		private static CTree TryMergeTrees(CTree pTree1, CTree pTree2)
		{
			CTree higherTree = pTree1.peak.Y >= pTree2.peak.Y ? pTree1 : pTree2;
			CTree lowerTree = pTree1.peak.Y < pTree2.peak.Y ? pTree1 : pTree2;

			Vector3 lowerTreePeak = lowerTree.peak.Center;
			//Vector3 higherTreePeak = higherTree.peak.GetClosestPointTo(lowerTreePeak);
			Vector3 higherTreePeak = higherTree.peak.Center;
			float angle = CUtils.AngleBetweenThreePoints(higherTreePeak - Vector3.UnitY, higherTree.peak.Center, lowerTreePeak);

			float maxBranchAngle = CTree.GetMaxBranchAngle(higherTreePeak, lowerTreePeak);
			float distBetweenPeaks = CUtils.Get2DDistance(pTree1.peak.Center, pTree2.peak.Center);

			//tree peaks must be close to each other and lower peak must be in appropriate angle with higher peak
			bool distOK = distBetweenPeaks < GetMinPeakDistance(1.5f); //todo: zlepšit toto kritérium
			bool angleOK = angle < maxBranchAngle;
			//bool lowerPeakIsInsideHigherTree = higherTree.Contains(lowerTreePeak);

			if (higherTree.treeIndex == 9 && lowerTree.treeIndex == 20)
			{
				Console.Write("!");
			}
			//measure how much does lower tree overlap with higher tree
			float overlapRatio = CUtils.GetOverlapRatio(lowerTree, higherTree);
			bool overlapRatioOK = overlapRatio > 0.5f;
			//todo: using overlapRatio results in too much merging...doladit
			overlapRatioOK = false;

			if ((distOK && angleOK) || overlapRatioOK)
			{
				//if (lowerTree.treeIndex == 666)
				{
					Console.WriteLine("\nMerge " + higherTree + " with " + lowerTree);
					Console.WriteLine("distBetweenPeaks = " + distBetweenPeaks + ". angle = " + angle);
				}
				higherTree.MergeWith(lowerTree);
				Trees.Remove(lowerTree);
			}
			return higherTree;
		}

		private static CTree MergeTrees(CTree pTree1, CTree pTree2)
		{
			CTree higherTree = pTree1.peak.Y >= pTree2.peak.Y ? pTree1 : pTree2;
			CTree lowerTree = pTree1.peak.Y < pTree2.peak.Y ? pTree1 : pTree2;

			Console.WriteLine("\nMerge " + higherTree + " with " + lowerTree);

			higherTree.MergeWith(lowerTree);
			Trees.Remove(lowerTree);

			return higherTree;
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

		}

		public static void ExportTrees()
		{
			Console.WriteLine("\nAdd trees to export " + Trees.Count + " | " + DateTime.Now);
			foreach (CTree t in Trees)
			{
				//Obj tObj = t.GetObj("tree_" + Trees.IndexOf(t), true, false);
				Obj tObj = t.GetObj("tree_" + t.treeIndex, true, false);
				CProjectData.objsToExport.Add(tObj);
			}
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

	}
}