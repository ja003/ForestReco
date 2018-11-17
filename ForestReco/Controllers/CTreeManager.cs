﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public static class CTreeManager
	{
		public static List<CTree> Trees { get; private set; }
		public static List<CTree> InvalidTrees { get; private set; }

		public static List<Vector3> invalidVegePoints;

		public const float TREE_POINT_EXTENT = 0.1f;

		//public static float TREE_EXTENT_MERGE_MULTIPLY = 2f;
		//public static float BASE_TREE_EXTENT = 1f;

		public const float MIN_TREE_EXTENT = 0.5f;
		//public const float DEFAULT_TREE_EXTENT = 1f;
		//public static float AVERAGE_TREE_HEIGHT => CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh);
		public static float AVERAGE_TREE_HEIGHT;
		public static int MIN_BRANCH_POINT_COUNT = 5;
		public static int MIN_TREE_POINT_COUNT = 20;

		public static float MIN_FAKE_TREE_HEIGHT = 20;

		public static float AVERAGE_MAX_TREE_HEIGHT = 40;

		public static void Init()
		{
			Trees = new List<CTree>();
			treeIndex = 0;
			InvalidTrees = new List<CTree>();
			invalidVegePoints = new List<Vector3>();
			pointCounter = 0;
		}

		//public const float MIN_PEAKS_DISTANCE = DEFAULT_TREE_EXTENT;
		public static float GetMinPeakDistance(float pMultiply)
		{
			return CParameterSetter.treeExtent * pMultiply;
		}

		/// <summary>
		/// Calculates minimal peak distance for given trees to be merged
		/// </summary>
		public static float GetMinPeakDistance(CTree pTree1, CTree pTree2)
		{
			float treeHeight = Math.Max(pTree1.GetTreeHeight(), pTree2.GetTreeHeight());
			float ratio = treeHeight / AVERAGE_TREE_HEIGHT;
			if (ratio < 1) { return CParameterSetter.treeExtent; }
			const float EXTENT_VALUE_STEP = 1.5f;

			return GetMinPeakDistance(1) + (ratio - 1) * EXTENT_VALUE_STEP;
		}

		public static void AssignMaterials()
		{
			foreach (CTree tree in Trees)
			{
				tree.AssignMaterial();
			}
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
			CProjectData.detailArray.AddPointInField(pPoint, CGroundArray.EPointType.Vege);

			List<CTree> possibleTrees = GetPossibleTreesFor(pPoint, EPossibleTreesMethos.ClosestHigher);

			//List<CTree> toDiscardTree = new List<CTree>();

			float bestAddPointFactor = 0;
			foreach (CTree t in possibleTrees)
			{
				if (DEBUG) { CDebug.WriteLine("- try add to : " + t.ToString(CTree.EDebug.Peak)); }

				if (pPointIndex == 173)
				{
					Console.Write("");
				}

				float addPointFactor = t.GetAddPointFactor(pPoint);
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

			if (newTree.treeIndex == 27)
			{
				CDebug.WriteLine("");
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

		public static void TryMergeAllTrees(bool pOnlyInvalid)
		{
			DateTime mergeStartTime = DateTime.Now;
			CDebug.WriteLine("TryMergeAllTrees");

			Trees.Sort((x, y) => y.peak.Center.Y.CompareTo(x.peak.Center.Y));

			int treeCountBeforeMerge = Trees.Count;

			MergeGoodAddFactorTrees(pOnlyInvalid);

			CDebug.Duration("Trees merge", mergeStartTime);
			CDebug.Count("Number of trees merged", treeCountBeforeMerge - Trees.Count);
		}

		/// <summary>
		/// Merges all trees, whose peak has good AddFactor to another tree
		/// </summary>
		private static void MergeGoodAddFactorTrees(bool pOnlyInvalid)
		{
			DateTime mergeStart = DateTime.Now;
			DateTime previousMergeStart = DateTime.Now;
			int iteration = 0;
			int maxIterations = Trees.Count;
			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				if (CProgramStarter.abort) { return; }

				if (i >= Trees.Count)
				{
					//CDebug.WriteLine("Tree was deleted");
					continue;
				}
				CTree treeToMerge = Trees[i];

				if (treeToMerge.Equals(13))
				{
					Console.WriteLine("");
				}


				if (pOnlyInvalid && !treeToMerge.isValid && treeToMerge.IsAtBorder())
				{
					//CDebug.Warning(treeToMerge + " is at border");
					continue;
				}


				List<CTree> possibleTrees = GetPossibleTreesFor(treeToMerge, EPossibleTreesMethos.ClosestHigher);
				Vector3 pPoint = treeToMerge.peak.Center;
				float bestAddPointFactor = 0;
				CTree selectedTree = null;



				foreach (CTree possibleTree in possibleTrees)
				{
					bool isFar = false;
					bool isSimilarHeight = false;

					if (possibleTree.isValid && treeToMerge.isValid)
					{
						float mergeFactor = GetMergeValidFacor(treeToMerge, possibleTree);
						if (mergeFactor > 0.9f)
						{
							selectedTree = possibleTree;
							break;
						}
					}
					if (pOnlyInvalid && possibleTree.isValid && treeToMerge.isValid)
					{
						continue;
					}



					if (treeToMerge.isValid)
					{
						//const float minPeakHeightDiffForMerge = 2;
						//treeToMerge is always lower
						float possibleTreeHeight = possibleTree.GetTreeHeight();
						float treeToMergeHeight = treeToMerge.GetTreeHeight();

						//todo: maybe good for smthing
						//float possibleTreePeakMin = possibleTree.peak.minBB.Y;
						//float treeToMergePeakMax = treeToMerge.peak.maxBB.Y;
						//if (possibleTreePeakMin - treeToMergePeakMax < minPeakHeightDiffForMerge)

						//todo: fucks up
						/*if (possibleTreeHeight - treeToMergeHeight < minPeakHeightDiffForMerge)
						{
							continue;
						}*/

						const float maxPeaksDistance = 1;
						float peaksDist = CUtils.Get2DDistance(treeToMerge.peak, possibleTree.peak);
						if (peaksDist > maxPeaksDistance)
						{
							isFar = true;
						}

						const float maxPeakHeightDiff = 1;
						if (possibleTreeHeight - treeToMergeHeight < maxPeakHeightDiff)
						{
							isSimilarHeight = true;
						}
					}

					if (treeToMerge.Equals(21))
					{
						Console.WriteLine("");
					}

					float addPointFactor = possibleTree.GetAddPointFactor(pPoint, treeToMerge);
					float requiredFactor = 0.5f;
					if (isFar) { requiredFactor += 0.1f; }
					if (isSimilarHeight) { requiredFactor += 0.1f; }
					if (pOnlyInvalid) { requiredFactor -= 0.2f; }

					if (addPointFactor > requiredFactor && addPointFactor > bestAddPointFactor)
					{
						selectedTree = possibleTree;
						bestAddPointFactor = addPointFactor;
						if (bestAddPointFactor > 0.9f) { break; }
					}
				}
				if (selectedTree != null)
				{
					treeToMerge = MergeTrees(ref treeToMerge, ref selectedTree, pOnlyInvalid);
				}

				CDebug.Progress(iteration, maxIterations, 50, ref previousMergeStart, mergeStart, "merge");
				iteration++;
			}
		}

		/// <summary>
		/// Merge trees with similar height which have peaks too close
		/// </summary>
		private static float GetMergeValidFacor(CTree pTreeToMerge, CTree pPossibleTree)
		{
			float factor = 0;
			if (pTreeToMerge.treeIndex == 13)
			{
				Console.Write("");
			}
			float peakHeightDiff = pPossibleTree.peak.Y - pTreeToMerge.peak.Y;
			float treeExtent = CParameterSetter.treeExtent * CParameterSetter.treeExtentMultiply;
			float similarHeightFactor = (treeExtent - peakHeightDiff) / treeExtent;
			bool isSimilarHeight = similarHeightFactor > 0.5f;
			if (!isSimilarHeight) { return 0; }

			float peakDist2D = CUtils.Get2DDistance(pTreeToMerge.peak, pPossibleTree.peak);
			if (peakDist2D < treeExtent)
			{
				return 1;
			}

			/*float peakHeightDiff = pPossibleTree.peak.Y - pTreeToMerge.peak.Y;
			float similarHeightFactor = CParameterSetter.treeExtent - peakHeightDiff;
			similarHeightFactor = Math.Max(0.1f, similarHeightFactor);

			float peakDist2D = CUtils.Get2DDistance(pTreeToMerge.peak, pPossibleTree.peak);
			float treeExtent = CParameterSetter.treeExtent * CParameterSetter.treeExtentMultiply;
			if (peakDist2D > treeExtent)
			{
				return 0;
			}
			float distFactor = (treeExtent - peakDist2D) / treeExtent;
			factor = (distFactor + similarHeightFactor) / 2;*/

			return factor;
		}

		private static CTree MergeTrees(ref CTree pTree1, ref CTree pTree2, bool pValidateRestrictive)
		{
			CTree higherTree = pTree1.peak.maxHeight.Y >= pTree2.peak.maxHeight.Y ? pTree1 : pTree2;
			CTree lowerTree = pTree1.peak.maxHeight.Y < pTree2.peak.maxHeight.Y ? pTree1 : pTree2;

			int index = 112;
			if (pTree1.Equals(index) || pTree2.Equals(index))
			{
				CDebug.WriteLine("\nMerge " + higherTree + " with " + lowerTree);

			}

			higherTree.MergeWith(lowerTree);
			DeleteTree(lowerTree);
			//Trees.Remove(lowerTree);
			//lowerTree = null;

			higherTree.Validate(pValidateRestrictive);

			return higherTree;
		}

		public enum EValidation
		{
			Scale,
			BranchDefine
		}

		public static void ValidateTrees(bool pCathegorize, bool pRestrictive, bool pFinal = false)
		{
			CDebug.WriteLine("Detect invalid trees", true);

			for (int i = Trees.Count - 1; i >= 0; i--)
			{
				CTree tree = Trees[i];

				//switch(pMethod){
				//	case EValidation.BranchDefine:
				//		tree.ValidateBranches();
				//		break;
				//	case EValidation.Scale:
				//		tree.ValidateScale();
				//		break;
				//}
				bool isValid = tree.Validate(pRestrictive, pFinal);

				if (!isValid)
				{
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

		public static void DebugTree(int pIndex)
		{
			foreach (CTree tree in Trees)
			{
				if (tree.treeIndex == pIndex)
				{
					CDebug.WriteLine("DebugTree " + tree);
					return;
				}
			}
		}

		public static int GetInvalidTreesAtBorderCount()
		{
			return InvalidTrees.Count(tree => tree.IsAtBorder());
		}

		public static float GetAverageTreeHeight()
		{
			float sum = 0;
			foreach (CTree tree in Trees)
			{
				sum += tree.GetTreeHeight();
			}
			return sum / Trees.Count;
		}

		public static float GetMinTreeHeight()
		{
			float min = 666;
			foreach (CTree tree in Trees)
			{
				if (tree.GetTreeHeight() < min)
				{
					min = tree.GetTreeHeight();
				}
			}
			return min;
		}

		public static float GetMaxTreeHeight()
		{
			float max = 0;
			foreach (CTree tree in Trees)
			{
				if (tree.GetTreeHeight() > max)
				{
					max = tree.GetTreeHeight();
				}
			}
			return max;
		}
	}
}