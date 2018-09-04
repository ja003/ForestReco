using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public class CTreeManager
	{
		private List<CTree> trees = new List<CTree>();
		public static float MAX_TREE_EXTENT = 3;
		public static float MAX_BRANCH_ANGLE = 45;
		private int treeIndex;

		public static bool DEBUG = true;

		private bool simpleExport = false;

		private int pointCounter;
		public void AddPoint(SVector3 pPoint)
		{
			//convert to format Y = height
			Vector3 point = new Vector3((float)pPoint.X, (float)pPoint.Z, (float)pPoint.Y);
			CTreePoint treePoint = new CTreePoint(point);

			if (simpleExport)
			{
				if (trees.Count == 0) { trees.Add(new CTree(point, 0)); }
				trees[0].ForceAddPoint(point);
				return;
			}

			if (DEBUG) Console.WriteLine("\n" + pointCounter + " AddPoint " + point);
			pointCounter++;
			CTree selectedTree = null;
			List<CTree> possibleTrees = GetPossibleTreesFor(point);

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
			else
			{
				trees.Add(new CTree(point, treeIndex));
				treeIndex++;
			}
		}

		private CTree TryMergeTrees(CTree pTree1, CTree pTree2)
		{
			CTree higherTree = pTree1.peak.Y >= pTree2.peak.Y ? pTree1 : pTree2;
			CTree lowerTree = pTree1.peak.Y < pTree2.peak.Y ? pTree1 : pTree2;

			float angle = CUtils.AngleBetweenThreePoints(new List<Vector3>
			{
				higherTree.peak.Center - Vector3.UnitY, higherTree.peak.Center, lowerTree.peak.Center
			}, Vector3.UnitY);
			if (angle < MAX_BRANCH_ANGLE)
			{
				higherTree.MergeWith(lowerTree);
				trees.Remove(lowerTree);
			}
			return higherTree;
		}

		private List<CTree> GetPossibleTreesFor(Vector3 pPoint)
		{
			List<CTree> possibleTrees = new List<CTree>();
			foreach (CTree t in trees)
			{
				//it must be close to peak of some tree
				if (CUtils.Get2DDistance(pPoint, t.peak.Center) < MAX_TREE_EXTENT / 2)
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

		public void WriteResult()
		{
			foreach (CTree t in trees)
			{
				Console.WriteLine(trees.IndexOf(t) + ": " + t);
				if (trees.IndexOf(t) > 100)
				{
					Console.WriteLine("too much...");
					return;
				}
			}
		}

		public List<Obj> GetTreeObjsFromField(CPointArray pArray)
		{
			List<Obj> treesObjs = new List<Obj>();
			//foreach (CPointField t in pArray.Maximas)
			foreach (CTree t in trees)
			{
				Obj treeObj = t.GetObj("Tree_" + trees.IndexOf(t), pArray, true);
				//move obj so it is at 0,0,0
				//SVector3 arrayCenter = (pArray.botLeftCorner + pArray.topRightCorner) / 2;
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