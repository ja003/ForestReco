using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public static class CRefTreeManager
	{
		public static List<CRefTree> Trees = new List<CRefTree>();

		public static void Init()
		{
			//string podkladyPath = CPlatformManager.GetPodkladyPath();
			List<string> treeFileNames = new List<string>()
			{
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\tree_dummy.obj",
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\tree_dummy_02.obj",
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\m1__2013-01-04_00-54-51.obj",
				//podkladyPath + @"\tree_models\m1_reduced"
				//"m1_reduced"
				"debug_tree_06"
			};
			//todo: uncomment to load tree obj from db
			LoadTrees(treeFileNames);
		}

		private static void LoadTrees(List<string> pFileNames)
		{
			foreach (string fileName in pFileNames)
			{
				CRefTree refTree = new CRefTree(fileName, pFileNames.IndexOf(fileName));

				Trees.Add(refTree);
				Console.WriteLine("Loaded tree: " + fileName);
			}
			//test
			//Trees[0].Rotation = new Vector3(0, 10, 0);

			//Trees[1].Position = new Vector3(10, 5, 0);
			//Trees[1].Scale = new Vector3(3, 5, 1);
			//Trees[1].Rotation = new Vector3(10, 0, 0);
		}

		private static int counter;
		
		private static Obj GetSuitableTreeObj(CTree pTree)
		{
			CRefTree bestTree = Trees[0];
			float bestSimilarity = 0;

			foreach (CRefTree refTree in Trees)
			{
				if (refTree.GetSimilarityWith(pTree) > bestSimilarity)
				{
					bestTree = refTree;
				}
			}

			Obj suitableTree = bestTree.Obj.Clone();
			suitableTree.Name += "_" + counter;

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
		
			Vector3 arrayCenter = CProjectData.GetArrayCenter();
			float minHeight = CProjectData.GetMinHeight();

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