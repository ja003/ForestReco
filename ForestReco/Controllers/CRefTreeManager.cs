using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public static class CRefTreeManager
	{
		public static List<CRefTree> Trees = new List<CRefTree>();

		public static bool DEBUG = false;

		public static void Init()
		{
			//string podkladyPath = CPlatformManager.GetPodkladyPath();
			List<string> treeFileNames = new List<string>()
			{
				//"R1",
				//"R2",
				//"R3",
				//"R4",
				//"R5",
				//"R6",
				"R7",
				//"R8",
				//"R9",
				//"R10",
				//"R11",
				//"R12",
				//"R13",
				//"R14",
				//"R15",
			};

			if (CProjectData.loadRefTrees)
			{
				LoadTrees(treeFileNames);
			}
		}

		public static List<Obj> GetRefTreeObjs()
		{

			DateTime addTreeObjModelsStart = DateTime.Now;
			Console.WriteLine("\nGet ref tree models");

			List<Obj> treeObjs = new List<Obj>();

			//int maxRefTreePointsCount = 1;
			foreach (CTree t in CTreeManager.Trees)
			{
				if (DEBUG) { Console.WriteLine("\n mostSuitableRefTree"); }

				t.mostSuitableRefTree = GetMostSuitableRefTree(t);

				SetRefTreeObjTransform(ref t.mostSuitableRefTree, t);

				Obj suitableTree = t.mostSuitableRefTree.Obj.Clone();
				suitableTree.Name += "_" + counter;
				counter++;

				if (DEBUG) { Console.WriteLine("\n mostSuitableRefTree = " + t.mostSuitableRefTree); }

				treeObjs.Add(suitableTree);

				//export of refTree points. not very effective, data are not centered and positioning them
				//correctly would be a bit complicated
				/*if (CProjectData.exportPoints)
				{
					if (maxRefTreePointsCount > 0)
					{
						maxRefTreePointsCount--;
						Obj refTreesPoints = new Obj("refTreesPoints");
						CObjExporter.AddPointsToObj(ref refTreesPoints, mostSuitableRefTree.Points,
							-mostSuitableRefTree.botCenter + suitableTree.Position, false);
						CProjectData.objsToExport.Add(refTreesPoints);
					}
				}*/
			}
			
			Console.WriteLine("Get ref tree models time = " + (DateTime.Now - addTreeObjModelsStart).TotalSeconds);

			return treeObjs;
		}

		private static void LoadTrees(List<string> pFileNames)
		{
			DateTime loadTreesStartTime = DateTime.Now;
			Console.WriteLine("Load ref trees: ");
			foreach (string fileName in pFileNames)
			{
				Console.WriteLine(" - " + fileName);
			}

			foreach (string fileName in pFileNames)
			{
				CRefTree deserializedRefTree = CRefTree.Deserialize(fileName);
				CRefTree refTree = deserializedRefTree ?? new CRefTree(fileName, pFileNames.IndexOf(fileName));

				Trees.Add(refTree);
				Console.WriteLine("Loaded tree: " + fileName);

				if (CProjectData.exportRefTreePoints)
				{
					Obj reftreePoints = new Obj(fileName + "_points");
					CObjExporter.AddPointsToObj(ref reftreePoints, refTree.Points);
					CProjectData.objsToExport.Add(reftreePoints);
				}
			}
			Console.WriteLine("\nduration = " + (DateTime.Now - loadTreesStartTime).TotalSeconds);
		}

		private static int counter;

		private static CRefTree GetMostSuitableRefTree(CTree pTree)
		{
			CRefTree mostSuitableTree = Trees[0];
			float bestSimilarity = 0;

			foreach (CRefTree refTree in Trees)
			{
				float similarity = CTreeMath.GetSimilarityWith(refTree, pTree);
				if (similarity > bestSimilarity)
				{
					mostSuitableTree = refTree;
					bestSimilarity = similarity;
				}
			}

			//Console.WriteLine("Most suitable ref tree = " + mostSuitableTree.Obj.Name + ". similarity = " + bestSimilarity);

			//Obj suitableTree = bestTree.Obj.Clone();
			//suitableTree.Name += "_" + counter;
			//counter++;
			return mostSuitableTree;
		}

		public static void ExportTrees()
		{
			Console.WriteLine("\nAdd ref trees to export ");
			foreach (CRefTree t in Trees)
			{
				//Obj tObj = t.GetObj("tree_" + Trees.IndexOf(t), true, false);
				Obj tObj = t.GetObj("refTree_" + t.Obj.Name, true, false);
				CProjectData.objsToExport.Add(tObj);
			}
		}

		/// <summary>
		/// Sets position, scale and todo: rotation of tree obj to match given pTargetTree 
		/// </summary>
		private static void SetRefTreeObjTransform(ref CRefTree pRefTree, CTree pTargetTree)
		{
			Vector3 arrayCenter = CProjectData.GetArrayCenter();
			float minHeight = CProjectData.GetMinHeight();

			//align position to tree
			pRefTree.Obj.Position = pTargetTree.peak.Center;

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

			pRefTree.Obj.Rotation = new Vector3(0, -CTreeMath.GetOffsetAngleTo(pRefTree, pTargetTree), 0);

			if (DEBUG)
			{
				Console.WriteLine(counter +
					"[" + pRefTree.Obj.Position + "], " +
					"[" + pRefTree.Obj.Rotation + "]" +
					". treeHeight = " + treeHeight + ". heightRatio = " + heightRatio);
			}
		}
	}
}