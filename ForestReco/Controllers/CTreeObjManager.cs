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
				Trees.Add(tree);
				Console.WriteLine("Loaded tree: " + treeFileName);
			}
			//test
			//Trees[0].Rotation = new Vector3(0, 10, 0);

			//Trees[1].Position = new Vector3(10, 5, 0);
			//Trees[1].Scale = new Vector3(3, 5, 1);
			//Trees[1].Rotation = new Vector3(10, 0, 0);
		}
	}
}