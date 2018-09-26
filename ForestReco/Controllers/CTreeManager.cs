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
		public static List<CTree> InvalidTrees { get; } = new List<CTree>();

		public const float DEFAULT_TREE_EXTENT = 1.5f;
		//public const float DEFAULT_TREE_EXTENT = 1f;
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

		private const int MAX_DEBUG_COUNT = 30;

		public static void AddPoint(Vector3 pPoint, int pPointIndex)
		{
			//DebugPoint(pPoint, pPointIndex);

			pointCounter++;
			CTree selectedTree = null;

			List<CTree> possibleTrees = GetPossibleTreesFor(pPoint, EPossibleTreesMethos.ClosestHigher);

			float bestAddPointFactor = 0;
			foreach (CTree t in possibleTrees)
			{
				if (DEBUG) { Console.WriteLine("- try add to : " + t.ToString(false, false, true, false)); }

				float addPointFactor = t.GetAddPointFactor(pPoint, false);
				if (addPointFactor > 0.5f && addPointFactor > bestAddPointFactor)
				{
					selectedTree = t;
					bestAddPointFactor = addPointFactor;
				}
			}
			if (selectedTree != null)
			{
				if (DEBUG)
				{
					Console.WriteLine(bestAddPointFactor + " SELECTED TREE " +
						selectedTree + " for " + pPointIndex + ": " + pPoint);
				}
				selectedTree.AddPoint(pPoint);
			}
			else
			{
				if (treeIndex < MAX_DEBUG_COUNT || treeIndex % MAX_DEBUG_COUNT == 0)
				{
					Console.WriteLine("NEW TREE " + treeIndex + ": point[" + pPointIndex + "]: " +
					                  pPoint + ". Best factor = " + bestAddPointFactor);
				}
				if (treeIndex == MAX_DEBUG_COUNT)
				{
					Console.WriteLine("....");
				}
				Trees.Add(new CTree(pPoint, treeIndex));
				treeIndex++;
			}

			if (Trees.Count == 1 && pPointIndex != Trees[0].Points.Count - 1)
			{
				Console.WriteLine(pPointIndex + "Error. Incorrect point count. " + pPoint);
			}
		}

		private static void DebugPoint(Vector3 pPoint, int pPointIndex)
		{
			if (DEBUG) { Console.WriteLine("\n" + pointCounter + " AddPoint " + pPoint); }

			//if (pPointIndex == 32)
			if (pPointIndex == 423)
			{
				Console.Write("!");
			}

			Vector3 debugPoint = CObjExporter.GetMovedPoint(pPoint);
			debugPoint.Z *= -1;
			if (Vector3.Distance(debugPoint, new Vector3(-6.39f, 14.248f, 6.87f)) < .1f)
			{
				Console.Write("!");
			}
		}

		private static List<CTree> GetPossibleTreesFor(CTree pTree, EPossibleTreesMethos pMethod)
		{
			return GetPossibleTreesFor(pTree.peak.Center, pMethod, pTree);
		}

		/// <summary>
		/// TODO: refactor
		/// </summary>
		private static List<CTree> GetPossibleTreesFor(Vector3 pPoint, EPossibleTreesMethos pMethod,
			CTree pExcludeTree = null)
		{
			List<CTree> possibleTrees = new List<CTree>();
			if (pMethod == EPossibleTreesMethos.Belongs)
			{
				foreach (CTree t in Trees)
				{
					if (pExcludeTree != null && pExcludeTree.Equals(t))
					{
						continue;
					}

					if (t.BelongsToTree(pPoint, false))
					{
						possibleTrees.Add(t);
						t.possibleNewPoint = pPoint;
					}
				}
			}
			else if (pMethod == EPossibleTreesMethos.ClosestHigher)
			{
				possibleTrees.AddRange(Trees);
			}
			else if (pMethod == EPossibleTreesMethos.Contains)
			{
				foreach (CTree t in Trees)
				{
					if (pExcludeTree != null && pExcludeTree.Equals(t))
					{
						continue;
					}
					if (t.Contains(pPoint))
					{
						possibleTrees.Add(t);
					}
				}
			}
			/*else if (pMethod == EPossibleTreesMethos.GoodAddFactor)
			{
				foreach (CTree t in Trees)
				{
					if (pExcludeTree != null && pExcludeTree.Equals(t))
					{
						continue;
					}
					if (t.GetAddPointFactor(pPoint) > 0.5f)
					{
						possibleTrees.Add(t);
					}
				}
			}*/

			//sort trees by 2D distance of given point to them
			possibleTrees.Sort((x, y) => CUtils.Get2DDistance(x.peak.Center, pPoint).CompareTo(
				CUtils.Get2DDistance(y.peak.Center, pPoint)));

			if (pMethod == EPossibleTreesMethos.ClosestHigher)
			{
				List<CTree> closestTrees = new List<CTree>();
				const int maxClosestTreesCount = 3;
				int counter = 0;
				foreach (CTree possibleTree in possibleTrees)
				{
					if (possibleTree.Equals(pExcludeTree)) { continue; }
					//we dont want trees that are lower than given point
					if (possibleTree.peak.Center.Y < pPoint.Y) { continue; }

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
			Belongs, //finds trees, in which given point belongs
			ClosestHigher, //finds closest trees which are higher than given point
			Contains, //finds trees, which contains given point
					  //GoodAddFactor
		}


		//MERGE

		public static void TryMergeAllTrees()
		{
			DateTime mergeStartTime = DateTime.Now;
			Console.WriteLine("\nTryMergeAllTrees. Start = " + mergeStartTime);

			Trees.Sort((x, y) => y.peak.Center.Y.CompareTo(x.peak.Center.Y));

			int treeCountBeforeMerge = CTreeManager.Trees.Count;

			if (CProjectData.mergeContaingTrees)
			{
				//Console.WriteLine("\n MergeContainingTrees");
				MergeContainingTrees();
			}
			if (CProjectData.mergeBelongingTrees)
			{
				//Console.WriteLine("\n MergeBelongingTrees");
				MergeBelongingTrees();
			}
			if (CProjectData.mergeGoodAddFactorTrees)
			{
				//Console.WriteLine("\n MergeGoodAddFactorTrees");
				MergeGoodAddFactorTrees();
			}
			Console.WriteLine("Trees merged | duration = " + (DateTime.Now - mergeStartTime).TotalSeconds);
			Console.WriteLine("Number of trees merged = " + (treeCountBeforeMerge - Trees.Count));
		}

		/// <summary>
		/// Merges all trees, whose peak has good AddFactor to another tree
		/// </summary>
		private static void MergeGoodAddFactorTrees()
		{
			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				if (i >= Trees.Count)
				{
					//Console.WriteLine("Tree was deleted");
					continue;
				}
				CTree tree = Trees[i];
				//if (tree.treeIndex == 71)
				//{
				//	Console.WriteLine("!");
				//}

				List<CTree> possibleTrees = GetPossibleTreesFor(tree, EPossibleTreesMethos.ClosestHigher);
				Vector3 pPoint = tree.peak.Center;
				float bestAddPointFactor = 0;
				CTree selectedTree = null;
				foreach (CTree t in possibleTrees)
				{
					float addPointFactor = t.GetAddPointFactor(pPoint, true);
					if (addPointFactor > 0.5f && addPointFactor > bestAddPointFactor)
					{
						selectedTree = t;
						bestAddPointFactor = addPointFactor;
					}
				}
				if (selectedTree != null)
				{
					tree = MergeTrees(tree, selectedTree);
				}
			}
		}

		private static void MergeBelongingTrees()
		{
			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				if (i >= Trees.Count)
				{
					Console.WriteLine("Tree was deleted");
					continue;
				}
				CTree tree = Trees[i];
				//if (tree.treeIndex == 90)
				//{
				//	Console.WriteLine("!");
				//}

				List<CTree> possibleTrees = GetPossibleTreesFor(tree, EPossibleTreesMethos.Belongs);

				foreach (CTree t in possibleTrees)
				{
					if (!Equals(tree, t))
					{
						tree = MergeTrees(tree, t);
						//tree = TryMergeTrees(tree, t);
					}
				}
			}
		}

		/// <summary>
		/// Merges trees, that overlaps and are close to each¨other
		/// </summary>
		private static void MergeContainingTrees()
		{
			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				if (i >= Trees.Count)
				{
					Console.WriteLine("Tree was deleted");
					continue;
				}
				CTree tree = Trees[i];
				//if (tree.treeIndex == 3)
				//{
				//	Console.WriteLine("!");
				//}

				List<CTree> possibleTrees = GetPossibleTreesFor(tree, EPossibleTreesMethos.Contains);

				foreach (CTree t in possibleTrees)
				{
					if (!Equals(tree, t))
					{
						float peaksDist = CUtils.Get2DDistance(tree.peak, t.peak);
						float minPeakDistance = GetMinPeakDistance(tree, t);

						//CTree higherTree = tree.peak.Y >= t.peak.Y ? tree : t;
						//CTree lowerTree = tree.peak.Y < t.peak.Y ? tree : t;

						if (peaksDist < minPeakDistance)
						//if (higherTree.GetAddPointFactor(lowerTree.peak.Center) > 0.5f)
						{
							tree = MergeTrees(tree, t);
						}
					}
				}
			}
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
			//bool distOK = distBetweenPeaks < GetMinPeakDistance(1.5f); //todo: zlepšit toto kritérium
			bool distOK = distBetweenPeaks < GetMinPeakDistance(higherTree, lowerTree);
			bool angleOK = angle < maxBranchAngle;
			//bool lowerPeakIsInsideHigherTree = higherTree.Contains(lowerTreePeak);

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

			//Console.WriteLine("\nMerge " + higherTree + " with " + lowerTree);

			higherTree.MergeWith(lowerTree);
			Trees.Remove(lowerTree);

			return higherTree;
		}

		public static void DetectInvalidTrees()
		{
			Console.WriteLine("DetectInvalidTrees");

			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				CTree tree = Trees[i];
				if (!tree.IsValid(true))
				{
					InvalidTrees.Add(tree);
					Trees.RemoveAt(i);
				}
			}
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
			Console.WriteLine("\nAdd trees to export ");
			foreach (CTree t in Trees)
			{
				//Obj tObj = t.GetObj("tree_" + Trees.IndexOf(t), true, false);
				Obj tObj = t.GetObj("tree_" + t.treeIndex, true, false);
				CProjectData.objsToExport.Add(tObj);
			}
			foreach (CTree t in InvalidTrees)
			{
				//Obj tObj = t.GetObj("tree_" + Trees.IndexOf(t), true, false);
				Obj tObj = t.GetObj("invalidTree_" + t.treeIndex, true, false);
				CProjectData.objsToExport.Add(tObj);
			}
		}

		public static void WriteResult()
		{
			Console.WriteLine("\n===============\n");
			foreach (CTree t in Trees)
			{
				Console.WriteLine(Trees.IndexOf(t).ToString("00") + ": " + t);
				if (Trees.IndexOf(t) > MAX_DEBUG_COUNT)
				{
					Console.WriteLine("too much to debug...total = " + Trees.Count);
					return;
				}
			}
			Console.WriteLine("\n===============\n");
		}
	}
}