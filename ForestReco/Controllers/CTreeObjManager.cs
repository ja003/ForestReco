using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public static class CTreeObjManager
	{
		public static List<Obj> Trees = new List<Obj>();

		public static void Init()
		{
			string podkladyPath = CPlatformManager.GetPodkladyPath(CProgramLoader.platform);
			List<string> treePaths = new List<string>()
			{
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\tree_dummy.obj",
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\tree_dummy_02.obj",
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\m1__2013-01-04_00-54-51.obj",
				podkladyPath + @"\tree_models\m1_reduced.obj"
			};
			//todo: uncomment to load tree obj from db
			LoadTrees(treePaths);
		}

		public static void LoadTrees(List<string> pPaths)
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

		//todo: not used anymore. replace with CTreeManager
		public static List<Obj> GetTreeObjsFromField(CPointArray pArray)
		{
			List<Obj> trees = new List<Obj>();
			Vector3 arrayCenter = CProjectData.header.Center;
			float minHeight = CProjectData.header.MinHeight;

			foreach (CPointField t in pArray.Maximas)
			{
				Obj suitableTree = GetSuitableTreeObj(t.Tree);
				//move obj so it is at 0,0,0
				suitableTree.Position -= arrayCenter;
				//move Y position so the tree touches the ground
				suitableTree.Position -= new Vector3(0, minHeight, 2 * suitableTree.Position.Z);

				trees.Add(suitableTree);
			}
			return trees;
		}

		private static int counter;
		private static Obj GetSuitableTreeObj(CPointField pTree)
		{
			//todo:implement selection
			Random r = new Random();
			Obj suitableTree = Trees[r.Next(0, Trees.Count)].Clone();
			suitableTree.Name += "_" + counter;
			//align position to tree
			suitableTree.Position = pTree.MaxVegePoint;
			float? groundHeight = pTree.GetHeight(EHeight.GroundMax, true);
			//Console.WriteLine("\nTree " + pTree);
			//Console.WriteLine(suitableTree.Position);
			//Console.WriteLine(groundHeight);
			//set Z position so the tree touches the ground (Z=height)
			suitableTree.Position.Y = (groundHeight == null ? suitableTree.Position.Y : (float)groundHeight);

			float? treeHeight = pTree.GetTreeHeight();
			float heightRation = (float)treeHeight / (suitableTree.Size.YMax - suitableTree.Size.YMin);
			suitableTree.Scale = new Vector3(1, (float)heightRation, 1);

			counter++;
			return suitableTree;
		}

		private static Obj GetSuitableTreeObj(CTree pTree)
		{
			//todo:implement selection
			Random r = new Random();
			Obj suitableTree = Trees[r.Next(0, Trees.Count)].Clone();
			suitableTree.Name += "_" + counter;
			//align position to tree
			/*suitableTree.Position = pTree.peak.maxHeight;
			Vector3 Vector3Point = new Vector3(suitableTree.Position);
			
			Vector3Point.FlipYZ();
			float? groundHeight = CProjectData.combinedArray.
				GetElementContainingPoint(Vector3Point).GetHeight(EHeight.GroundMax, true);
			suitableTree.Position.Y = (groundHeight == null ? suitableTree.Position.Y : (float)groundHeight);

			float treeHeight = pTree.peak.maxHeight.Y - (float)groundHeight;
			float heightRatio = treeHeight / (suitableTree.Size.YMax - suitableTree.Size.YMin);
			suitableTree.Scale = new Vector3(1, (float)heightRatio, 1);*/

			counter++;
			return suitableTree;
		}

		internal static List<Obj> GetTreeObjs()
		{
			List<Obj> trees = new List<Obj>();

			foreach (CTree t in CTreeManager.Trees)
			{
				Obj suitableTree = GetSuitableTreeObj(t);

				SetTreeObjTransform(ref suitableTree, t);				

				trees.Add(suitableTree);
			}
			return trees;
		}

		/// <summary>
		/// Sets position, scale and (todo) rotation of tree obj to match given pTree 
		/// </summary>
		private static void SetTreeObjTransform(ref Obj pSuitableTree, CTree pTree){
		
			Vector3 arrayCenter = CProjectData.header.Center;
			float minHeight = CProjectData.header.MinHeight;

			//align position to tree
			pSuitableTree.Position = pTree.peak.maxHeight;
			float? groundHeight = CProjectData.array?.GetElementContainingPoint(pSuitableTree.Position).
				GetHeight(EHeight.GroundMax, true);
			groundHeight = groundHeight ?? pSuitableTree.Position.Y;
			pSuitableTree.Position.Y = (float)groundHeight;

			float treeHeight = pTree.peak.maxHeight.Y - (float)groundHeight;
			float heightRatio = treeHeight / (pSuitableTree.Size.YMax - pSuitableTree.Size.YMin);
			pSuitableTree.Scale = new Vector3(1, (float)heightRatio, 1);


			//move obj so it is at 0,0,0
			pSuitableTree.Position -= arrayCenter;
			pSuitableTree.Position -= new Vector3(0, minHeight, 2 * pSuitableTree.Position.Z);

			//Console.WriteLine(counter + "[" + pSuitableTree.Position + "]. treeHeight = " + treeHeight + ". heightRatio = " + heightRatio);
		}
	}
}