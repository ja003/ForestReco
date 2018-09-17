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

		private static CRefTree GetMostSuitableRefTree(CTree pTree)
		{
			CRefTree mostSuitableTree = Trees[0];
			float bestSimilarity = 0;

			foreach (CRefTree refTree in Trees)
			{
				if (refTree.GetSimilarityWith(pTree) > bestSimilarity)
				{
					mostSuitableTree = refTree;
				}
			}

			//Obj suitableTree = bestTree.Obj.Clone();
			//suitableTree.Name += "_" + counter;
			//counter++;
			return mostSuitableTree;
		}

		internal static List<Obj> GetTreeObjs()
		{
			List<Obj> treeObjs = new List<Obj>();

			foreach (CTree t in CTreeManager.Trees)
			{
				CRefTree mostSuitableRefTree = GetMostSuitableRefTree(t);

				SetRefTreeObjTransform(ref mostSuitableRefTree, t);

				Obj suitableTree = mostSuitableRefTree.Obj.Clone();
				suitableTree.Name += "_" + counter;
				counter++;

				treeObjs.Add(suitableTree);
			}
			return treeObjs;
		}

		/// <summary>
		/// Sets position, scale and todo: rotation of tree obj to match given pTargetTree 
		/// </summary>
		private static void SetRefTreeObjTransform(ref CRefTree pRefTree, CTree pTargetTree)
		{
			Vector3 arrayCenter = CProjectData.GetArrayCenter();
			float minHeight = CProjectData.GetMinHeight();

			//align position to tree
			pRefTree.Obj.Position = pTargetTree.peak.maxHeight;

			/*float? groundHeight = CProjectData.array?.GetElementContainingPoint(pRefTree.Obj.Position).
				GetHeight(EHeight.GroundMax, true);
			groundHeight = groundHeight ?? pRefTree.Obj.Position.Y;
			pRefTree.Obj.Position.Y = (float)groundHeight;*/

			pRefTree.Obj.Position.Y = pTargetTree.GetGroundHeight();
			
			//float treeHeight = pTargetTree.peak.maxHeight.Y - (float)groundHeight;
			float treeHeight = pTargetTree.GetTreeHeight();
			float heightRatio = treeHeight / (pRefTree.Obj.Size.YMax - pRefTree.Obj.Size.YMin);
			//todo: scale X and Z based on pTargetTree extents
			pRefTree.Obj.Scale = heightRatio * Vector3.One;


			//move obj so it is at 0,0,0
			pRefTree.Obj.Position -= arrayCenter;
			pRefTree.Obj.Position -= new Vector3(0, minHeight, 2 * pRefTree.Obj.Position.Z);

			pRefTree.Obj.Rotation = new Vector3(0, -pRefTree.GetOffsetAngleTo(pTargetTree), 0);

			Console.WriteLine(counter +
				"[" + pRefTree.Obj.Position + "], " +
				"[" + pRefTree.Obj.Rotation + "]" +
				". treeHeight = " + treeHeight + ". heightRatio = " + heightRatio);
		}
	}
}