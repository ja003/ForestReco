using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public class CTreeObjManager
	{
		public List<Obj> Trees = new List<Obj>();

		public void LoadTrees(List<string> pPaths)
		{
			foreach (string path in pPaths)
			{
				string[] pathSplit = path.Split('\\');
				string treeFileName = pathSplit.Last();
				Obj tree = new Obj(treeFileName);
				tree.LoadObj(path);
				tree.Scale = new Vector3(.2f, .5f, .2f);
				Trees.Add(tree);
				Console.WriteLine("Loaded tree: " + treeFileName);
			}
			//test
			//Trees[0].Rotation = new Vector3(0, 10, 0);

			//Trees[1].Position = new Vector3(10, 5, 0);
			//Trees[1].Scale = new Vector3(3, 5, 1);
			//Trees[1].Rotation = new Vector3(10, 0, 0);
		}

		public List<Obj> GetTreeObjsFromField(CPointArray pArray)
		{
			List<Obj> trees = new List<Obj>();
			foreach (CPointField t in pArray.Maximas)
			{
				Obj suitableTree = GetSuitableTreeObj(t.Tree);
				//move obj so it is at 0,0,0
				SVector3 arrayCenter = (pArray.botLeftCorner + pArray.topRightCorner) / 2;
				suitableTree.Position -= arrayCenter.ToVector3();
				//swap Y-Z (Y=height in .OBJ)
				float tmp = suitableTree.Position.Y;
				suitableTree.Position.Y = suitableTree.Position.Z;
				suitableTree.Position.Z = tmp;
				//move Y position so the tree touches the ground
				suitableTree.Position -= new Vector3(0, (float)pArray.minHeight, 2 * suitableTree.Position.Z);

				trees.Add(suitableTree);
			}
			return trees;
		}

		private int counter;
		private Obj GetSuitableTreeObj(CPointField pTree)
		{
			if (counter == 55)
			{
				Console.WriteLine("!");
			}
			//todo:implement selection
			Random r = new Random();
			Obj suitableTree = Trees[r.Next(0, Trees.Count)].Clone();
			suitableTree.Name += "_" + counter;
			//align position to tree
			suitableTree.Position = pTree.MaxVegePoint.ToVector3();
			double? groundHeight = pTree.GetHeight(EHeight.GroundMax, true);
			//Console.WriteLine("\nTree " + pTree);
			//Console.WriteLine(suitableTree.Position);
			//Console.WriteLine(groundHeight);
			//set Z position so the tree touches the ground (Z=height)
			suitableTree.Position.Z = (groundHeight == null ? suitableTree.Position.Z : (float)groundHeight);

			counter++;
			return suitableTree;
		}
	}
}