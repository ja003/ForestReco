using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public static class CTreeManager
	{
		private static List<CTree> trees = new List<CTree>();
		public static List<CTree> Trees => trees;

		public const float MAX_TREE_EXTENT = 3;
		public const float MIN_PEAKS_DISTANCE = MAX_TREE_EXTENT / 2;
		public static float MAX_BRANCH_ANGLE = 45;
		private static int treeIndex;

		public static bool DEBUG = false;

		private static bool simpleExport = false;

		private static int pointCounter;
		public static void AddPoint(Vector3 pPoint, int pPointIndex)
		{
			CTreePoint treePoint = new CTreePoint(pPoint);

			if (simpleExport)
			{
				if (trees.Count == 0) { trees.Add(new CTree(pPoint, 0)); }
				trees[0].ForceAddPoint(pPoint);
				return;
			}

			if (DEBUG) Console.WriteLine("\n" + pointCounter + " AddPoint " + pPoint);
			pointCounter++;
			CTree selectedTree = null;

			if (Vector3.Distance(pPoint, new Vector3(677.99f, 136.17f, 1160.24f)) < 0.01f)
			{
				Console.WriteLine("¨¨");
			}
			List<CTree> possibleTrees = GetPossibleTreesFor(pPoint);


			foreach (CTree t in possibleTrees)
			{
				if (DEBUG) Console.WriteLine("- try add to : " + t.ToString(false, false, true, false));
				//CTreePoint peak = t.peak;
				if (t.TryAddPoint(treePoint))
				{
					selectedTree = t;
					break;
				}
			}
			if (selectedTree != null)
			{
				foreach (CTree t in possibleTrees)
				{
					if (!Equals(selectedTree, t))
					{
						selectedTree = TryMergeTrees(selectedTree, t);
					}
				}
			}
			else// if (selectedTree == null)
			{
				trees.Add(new CTree(pPoint, treeIndex));
				treeIndex++;
			}

			if (trees.Count == 1 && pPointIndex != trees[0].GetPointCount() - 1)
			{
				Console.WriteLine(pPointIndex + "Error. Incorrect point count. " + pPoint);
			}
		}

		private static CTree TryMergeTrees(CTree pTree1, CTree pTree2)
		{
			CTree higherTree = pTree1.peak.Y >= pTree2.peak.Y ? pTree1 : pTree2;
			CTree lowerTree = pTree1.peak.Y < pTree2.peak.Y ? pTree1 : pTree2;

			Vector3 lowerTreePeak = lowerTree.peak.Center;
			Vector3 higherTreePeak = higherTree.peak.GetClosestPointTo(lowerTreePeak);
			float angle = CUtils.AngleBetweenThreePoints(new List<Vector3>
			{
				higherTreePeak - Vector3.UnitY, higherTree.peak.Center, lowerTreePeak
			}, Vector3.UnitY);

			float maxBranchAngle = CTree.GetMaxBranchAngle(higherTreePeak, lowerTreePeak);
			float distBetweenPeaks = CUtils.Get2DDistance(pTree1.peak.Center, pTree2.peak.Center);

			//tree peaks must be close to each other and lower peak must be in appropriate angle with higher peak
			if (distBetweenPeaks < MAX_TREE_EXTENT / 2 && angle < maxBranchAngle)
			{
				higherTree.MergeWith(lowerTree);
				trees.Remove(lowerTree);
			}
			return higherTree;
		}

		private static List<CTree> GetPossibleTreesFor(Vector3 pPoint)
		{
			List<CTree> possibleTrees = new List<CTree>();
			foreach (CTree t in trees)
			{
				//const float MAX_DIST_TO_TREE_BB = 0.1f;
				//it must be close to peak of some tree
				/*if (CUtils.Get2DDistance(pPoint, t.peak.Center) < MAX_TREE_EXTENT / 2 ||
					//or to its BB
				    t.Get2DDistanceFromBBTo(pPoint) < MAX_DIST_TO_TREE_BB)*/

				if (t.BelongsToTree(new CTreePoint(pPoint), false))
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
			foreach (CTree t in trees)
			{
				Console.WriteLine(trees.IndexOf(t).ToString("00") + ": " + t);
				if (trees.IndexOf(t) > 100)
				{
					Console.WriteLine("too much...");
					return;
				}
			}
		}

		//TODO: not used anymore from array
		public static List<Obj> GetTreeObjsFromField()
		{
			List<Obj> treesObjs = new List<Obj>();
			//foreach (CPointField t in pArray.Maximas)
			foreach (CTree t in trees)
			{
				Obj treeObj = t.GetObj("Tree_" + trees.IndexOf(t), true, false);
				//move obj so it is at 0,0,0
				//Vector3 arrayCenter = (pArray.botLeftCorner + pArray.topRightCorner) / 2;
				//treeObj.Position -= arrayCenter.ToVector3(true);
				//move Y position so the tree touches the ground
				//treeObj.Position -= new Vector3(0, (float)pArray.minHeight, 2 * treeObj.Position.Z);

				treesObjs.Add(treeObj);
			}
			//treesObjs.Add(trees[trees.Count - 1].GetObj("last", pArray));


			return treesObjs;
		}
	}
}