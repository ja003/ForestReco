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
		public static List<CTree> FakeTrees { get; } = new List<CTree>();

		public static List<Vector3> invalidVegePoints = new List<Vector3>();

		public const float TREE_POINT_EXTENT = 0.1f;

		public static float TREE_EXTENT_MERGE_MULTIPLY = 2f;
		public static float BASE_TREE_EXTENT = 1f;
		public static float MIN_TREE_EXTENT = 0.5f;
		//public const float DEFAULT_TREE_EXTENT = 1f;
		public static float AVERAGE_TREE_HEIGHT = 10;
		public static int MIN_BRANCH_POINT_COUNT = 5;
		public static int MIN_TREE_POINT_COUNT = 20;

		public static float MIN_FAKE_TREE_HEIGHT = 20;


		//public const float MIN_PEAKS_DISTANCE = DEFAULT_TREE_EXTENT;
		public static float GetMinPeakDistance(float pMultiply)
		{
			return BASE_TREE_EXTENT * pMultiply;
		}

		/// <summary>
		/// Calculates minimal peak distance for given trees to be merged
		/// </summary>
		public static float GetMinPeakDistance(CTree pTree1, CTree pTree2)
		{
			float treeHeight = Math.Max(pTree1.GetTreeHeight(), pTree2.GetTreeHeight());
			float ratio = treeHeight / AVERAGE_TREE_HEIGHT;
			if (ratio < 1) { return BASE_TREE_EXTENT; }
			const float EXTENT_VALUE_STEP = 1.5f;

			return GetMinPeakDistance(1) + (ratio - 1) * EXTENT_VALUE_STEP;
		}

		public const float MAX_BRANCH_ANGLE = 45;
		private static int treeIndex;

		public static bool DEBUG = false;
		private static int pointCounter;

		private const int MAX_DEBUG_COUNT = 5;
		private const int MAX_DISTANCE_FOR_POSSIBLE_TREES = 5;

		public static void AddPoint(Vector3 pPoint, int pPointIndex)
		{
			//DebugPoint(pPoint, pPointIndex);

			//if(pPointIndex > 500){ return; }

			pointCounter++;
			CTree selectedTree = null;

			/*if (!CheckPoint(pPoint))
			{
				//CDebug.WriteLine(pPoint + " is not valid");
				invalidVegePoints.Add(pPoint);
				return;
			}*/
			CProjectData.array.AddPointInField(pPoint, CGroundArray.EPointType.Vege);

			List<CTree> possibleTrees = GetPossibleTreesFor(pPoint, EPossibleTreesMethos.ClosestHigher);

			//List<CTree> toDiscardTree = new List<CTree>();

			float bestAddPointFactor = 0;
			foreach (CTree t in possibleTrees)
			{
				if (DEBUG) { CDebug.WriteLine("- try add to : " + t.ToString(CTree.EDebug.Peak)); }

				if (pPointIndex == 255)
				{
					Console.Write("");
				}

				float addPointFactor = t.GetAddPointFactor(pPoint, false);
				if (addPointFactor > 0.5f)
				{
					if (t.Equals(0))
					{
						Console.Write("");
					}
					//if (t.GetTreeHeight() > MIN_FAKE_TREE_HEIGHT && !t.IsPeakValidWith(pPoint))
					//{
					//	//toDiscardTree.Add(t);
						
					//	t.isFake = true;
					//	CDebug.WriteLine("FAKE: " + t);
					//}
					else if (addPointFactor > bestAddPointFactor)
					{
						selectedTree = t;
						bestAddPointFactor = addPointFactor;
					}
				}
			}


			if (selectedTree != null)
			{
				if (DEBUG)
				{
					CDebug.WriteLine(bestAddPointFactor + " SELECTED TREE " +
						selectedTree + " for " + pPointIndex + ": " + pPoint);
				}
				selectedTree.AddPoint(pPoint);
			}
			else
			{
				bool debugFrequency = false;
				if (treeIndex < MAX_DEBUG_COUNT || (debugFrequency && treeIndex % MAX_DEBUG_COUNT == 0))
				{
					CDebug.WriteLine("NEW TREE " + treeIndex + ": point[" + pPointIndex + "]: " +
									  pPoint + ". Best factor = " + bestAddPointFactor);
				}
				if (treeIndex == MAX_DEBUG_COUNT)
				{
					CDebug.WriteLine("....");
				}
				CreateNewTree(pPoint);
			}

			//check if first tree was asigned correct point count
			if (Trees.Count == 1 && pPointIndex != Trees[0].Points.Count - 1 + invalidVegePoints.Count)
			{
				CDebug.Error(pPointIndex + " Incorrect point count. " + pPoint);
			}
		}

		/*private static bool CheckPoint(Vector3 pPoint)
		{
			float? pointHeight = CProjectData.array.GetPointHeight(pPoint);
			if (pointHeight == null)
			{
				CDebug.Error(pPoint + " ground not defined");
				return true;
			}
			return pointHeight < MAX_TREE_HEIGHT;
		}*/

		private static void CreateNewTree(Vector3 pPoint)
		{
			CTree newTree = new CTree(pPoint, treeIndex, TREE_POINT_EXTENT);
			Trees.Add(newTree);
			treeIndex++;

			CGroundField element = CProjectData.array.GetElementContainingPoint(pPoint);
			element.DetectedTrees.Add(newTree);

			newTree.groundField = element;

			if (newTree.treeIndex == 98)
			{
				//CDebug.WriteLine("!");
			}
		}

		private static void DeleteTree(CTree pTree)
		{
			if (!Trees.Contains(pTree))
			{
				CDebug.Error("Trees dont contain " + pTree);
				return;
			}
			CGroundField element = pTree.groundField;
			Trees.Remove(pTree);
			if (!element.DetectedTrees.Contains(pTree))
			{
				CDebug.Error("element " + element + " doesnt contain " + pTree);
				return;
			}
			element.DetectedTrees.Remove(pTree);
		}

		private static void DebugPoint(Vector3 pPoint, int pPointIndex)
		{
			if (DEBUG) { CDebug.WriteLine("\n" + pointCounter + " AddPoint " + pPoint); }

			Vector3 debugPoint = CObjExporter.GetMovedPoint(pPoint);
			debugPoint.Z *= -1;
		}

		private static List<CTree> GetPossibleTreesFor(CTree pTree, EPossibleTreesMethos pMethod)
		{
			return GetPossibleTreesFor(pTree.peak.Center, pMethod, pTree);
		}

		/// <summary>
		/// TODO: refactor
		/// </summary>
		public static List<CTree> GetPossibleTreesFor(Vector3 pPoint, EPossibleTreesMethos pMethod,
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
				possibleTrees.AddRange(CProjectData.array.GetTreesInDistanceFrom(pPoint, MAX_DISTANCE_FOR_POSSIBLE_TREES));
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
				//CDebug.WriteLine("SELECT FROM " + possibleTrees.Count);

				List<CTree> closestTrees = new List<CTree>();
				//no reason to select more. small chance that point would fit better to further tree
				const int maxClosestTreesCount = 3;
				int counter = 0;
				foreach (CTree possibleTree in possibleTrees)
				{
					if (possibleTree.Equals(pExcludeTree)) { continue; }
					//if (possibleTree.isFake) { continue; }
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
			CDebug.WriteLine("TryMergeAllTrees");

			Trees.Sort((x, y) => y.peak.Center.Y.CompareTo(x.peak.Center.Y));

			int treeCountBeforeMerge = Trees.Count;

			if (CProjectData.mergeContaingTrees)
			{
				//CDebug.WriteLine("\n MergeContainingTrees");
				MergeContainingTrees();
			}
			if (CProjectData.mergeBelongingTrees)
			{
				//CDebug.WriteLine("\n MergeBelongingTrees");
				MergeBelongingTrees();
			}
			if (CProjectData.mergeGoodAddFactorTrees)
			{
				//CDebug.WriteLine("\n MergeGoodAddFactorTrees");
				MergeGoodAddFactorTrees();
			}
			CDebug.Duration("Trees merge", mergeStartTime);
			CDebug.Count("Number of trees merged", treeCountBeforeMerge - Trees.Count);
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
					//CDebug.WriteLine("Tree was deleted");
					continue;
				}
				CTree tree = Trees[i];
				if (tree.treeIndex == 406)
				{
					CDebug.WriteLine("__");
				}

				//if (CProjectData.mergeOnlyInvalidTrees && tree.isValid) { continue; }

				List<CTree> possibleTrees = GetPossibleTreesFor(tree, EPossibleTreesMethos.ClosestHigher);
				Vector3 pPoint = tree.peak.Center;
				float bestAddPointFactor = 0;
				CTree selectedTree = null;
				foreach (CTree t in possibleTrees)
				{
					if (CProjectData.mergeOnlyInvalidTrees)
					{
						//todo: seems better this way. test
						if (tree.isValid && t.isValid) { continue; }
					}

					float addPointFactor = t.GetAddPointFactor(pPoint, true);
					if (addPointFactor > 0.5f && addPointFactor > bestAddPointFactor)
					{
						selectedTree = t;
						bestAddPointFactor = addPointFactor;
					}
				}
				if (selectedTree != null)
				{
					tree = MergeTrees(ref tree, ref selectedTree);
				}
			}
		}

		private static void MergeBelongingTrees()
		{
			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				if (i >= Trees.Count)
				{
					CDebug.WriteLine("Tree was deleted");
					continue;
				}
				CTree tree = Trees[i];

				List<CTree> possibleTrees = GetPossibleTreesFor(tree, EPossibleTreesMethos.Belongs);

				//foreach (CTree t in possibleTrees)
				for (int j = possibleTrees.Count - 1; j >= 0; j--)
				{
					CTree t = possibleTrees[j];

					if (!Equals(tree, t))
					{
						tree = MergeTrees(ref tree, ref t);
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
					CDebug.WriteLine("Tree was deleted");
					continue;
				}
				CTree tree = Trees[i];

				List<CTree> possibleTrees = GetPossibleTreesFor(tree, EPossibleTreesMethos.Contains);

				//foreach (CTree t in possibleTrees)
				for (int j = possibleTrees.Count - 1; j >= 0; j--)
				{
					CTree t = possibleTrees[j];
					if (!Equals(tree, t))
					{
						float peaksDist = CUtils.Get2DDistance(tree.peak, t.peak);
						float minPeakDistance = GetMinPeakDistance(tree, t);

						//CTree higherTree = tree.peak.Y >= t.peak.Y ? tree : t;
						//CTree lowerTree = tree.peak.Y < t.peak.Y ? tree : t;

						if (peaksDist < minPeakDistance)
						//if (higherTree.GetAddPointFactor(lowerTree.peak.Center) > 0.5f)
						{
							tree = MergeTrees(ref tree, ref t);
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
					CDebug.WriteLine("\nMerge " + higherTree + " with " + lowerTree);
					CDebug.WriteLine("distBetweenPeaks = " + distBetweenPeaks + ". angle = " + angle);
				}
				higherTree.MergeWith(lowerTree);
				Trees.Remove(lowerTree);
			}
			return higherTree;
		}

		private static CTree MergeTrees(ref CTree pTree1, ref CTree pTree2)
		{
			CTree higherTree = pTree1.peak.maxHeight.Y >= pTree2.peak.maxHeight.Y ? pTree1 : pTree2;
			CTree lowerTree = pTree1.peak.maxHeight.Y < pTree2.peak.maxHeight.Y ? pTree1 : pTree2;

			//CDebug.WriteLine("\nMerge " + higherTree + " with " + lowerTree);
			higherTree.MergeWith(lowerTree);
			DeleteTree(lowerTree);
			//Trees.Remove(lowerTree);
			//lowerTree = null;

			return higherTree;
		}

		public static void ValidateTrees(bool pCathegorize)
		{
			CDebug.WriteLine("Detect invalid trees", true);

			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				CTree tree = Trees[i];

				if (!tree.Validate(true))
				{
					//if (tree.IsTreeFake())
					//{
					//	DiscartedTrees.Add(tree);
					//	Trees.RemoveAt(i);
					//	CDebug.WriteLine("Discard tree " + tree);
					//	continue;
					//}

					if (pCathegorize)
					{
						InvalidTrees.Add(tree);
						Trees.RemoveAt(i);
					}
				}
			}
		}

		public static void ProcessAllTrees()
		{
			DateTime processTreesStartTime = DateTime.Now;
			CDebug.WriteLine("ProcessAllTrees", true);
			foreach (CTree t in Trees)
			{
				t.Process();
			}
			CDebug.Duration("Trees processed", processTreesStartTime);

		}

		public static void DebugTrees()
		{
			CDebug.WriteLine("\n===============\n");
			foreach (CTree t in Trees)
			{
				CDebug.WriteLine(Trees.IndexOf(t).ToString("00") + ": " + t);
				if (Trees.IndexOf(t) > MAX_DEBUG_COUNT)
				{
					CDebug.WriteLine("too much to debug...total = " + Trees.Count);
					return;
				}
			}
			CDebug.WriteLine("\n===============\n");
		}

		public static void CheckAllTrees()
		{
			foreach (CTree t in Trees)
			{
				t.CheckTree();
			}
		}
	}
}