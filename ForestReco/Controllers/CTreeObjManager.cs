using System;
using System.Collections.Generic;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public class CTreeObjManager
	{
		public List<CTreeObj> Trees = new List<CTreeObj>();

		public void LoadTrees(List<string> pPaths)
		{
			foreach (string path in pPaths)
			{
				CTreeObj tree = new CTreeObj();
				tree.LoadObj(path);
				Trees.Add(tree);
				Console.WriteLine("Loaded tree: " + path);
			}
			//test
			Trees[1].Position = Vector3.One * 5;
		}
	}
}