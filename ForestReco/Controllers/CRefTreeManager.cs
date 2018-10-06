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
		private const float TREE_POINT_EXTENT = 0.2f;

		public static bool DEBUG = false;

		public static void Init()
		{
			//string podkladyPath = CPlatformManager.GetPodkladyPath();
			List<string> treeFileNames = new List<string>()
			{
				"R1",
				"R2",
				//"R3",
				//"R4",
				//"R5",
				//"R6",
				//"R7",
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

		/// <summary>
		/// Assigns to each of detected trees the most suitable refTree
		/// </summary>
		public static void AssignRefTrees()
		{
			DateTime addTreeObjModelsStart = DateTime.Now;
			Console.WriteLine("\nGet ref tree models");

			const int debugFrequency = 10;

			DateTime assignRefTreesStart = DateTime.Now;
			Console.WriteLine("\nAssignRefTrees start = " + assignRefTreesStart);

			DateTime previousDebugStart = DateTime.Now;

			int counter = 0;
			foreach (CTree t in CTreeManager.Trees)
			{
				if (DEBUG) { Console.WriteLine("\n mostSuitableRefTree"); }

				if (t.treeIndex == 64)
				{
					Console.WriteLine("§");
				}

				CRefTree mostSuitableRefTree = GetMostSuitableRefTree(t);
				if (mostSuitableRefTree == null)
				{
					Console.WriteLine("Error: no reftrees assigned!");
					continue;
				}

				SetRefTreeObjTransform(ref mostSuitableRefTree, t);

				Obj suitableTreeObj = mostSuitableRefTree.Obj.Clone();
				suitableTreeObj.Name += "_" + t.treeIndex;
				t.mostSuitableRefTreeObj = suitableTreeObj;


				if (counter % debugFrequency == 0 && counter > 0)
				{
					Console.WriteLine("\nAssigned reftree " + counter + " out of " + CTreeManager.Trees.Count);
					double lastAssignmentProcessTime = (DateTime.Now - previousDebugStart).TotalSeconds;
					Console.WriteLine("- time of last " + debugFrequency + " trees = " + lastAssignmentProcessTime);

					float remainsRatio = (float)(CTreeManager.Trees.Count - counter) / debugFrequency;
					double totalSeconds = remainsRatio * lastAssignmentProcessTime;
					TimeSpan ts = new TimeSpan(0, 0, 0, (int)totalSeconds);
					string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";
					Console.WriteLine("- estimated time left = " + timeString);

					previousDebugStart = DateTime.Now;
				}
				counter++;


				//counter++;

				if (DEBUG) { Console.WriteLine("\n mostSuitableRefTree = " + mostSuitableRefTree); }

				//treeObjs.Add(suitableTree);

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

			Console.WriteLine("\nAssign ref tree models time = " + (DateTime.Now - addTreeObjModelsStart).TotalSeconds);

			//return treeObjs;
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
				CRefTree refTree = deserializedRefTree ?? new CRefTree(fileName, pFileNames.IndexOf(fileName), TREE_POINT_EXTENT, true);

				Trees.Add(refTree);
				Console.WriteLine("Loaded tree: " + fileName);

				if (CProjectData.exportRefTreePoints)
				{
					Console.WriteLine("TODO: exportRefTreePoints not used");
					//Obj reftreePoints = new Obj(fileName + "_points");
					//CObjExporter.AddPointsToObj(ref reftreePoints, refTree.Points);
					//CProjectData.objsToExport.Add(reftreePoints);
				}
			}
			Console.WriteLine("\nduration = " + (DateTime.Now - loadTreesStartTime).TotalSeconds);

			DebugRefTrees();
		}

		private static void DebugRefTrees()
		{
			Console.WriteLine("\nLoaded reftrees: ");
			foreach (CRefTree refTree in Trees)
			{
				Console.WriteLine(refTree);
			}
		}

		//private static int counter;

		private static CRefTree GetMostSuitableRefTree(CTree pTree)
		{
			if (Trees.Count == 0)
			{
				Console.WriteLine("Error: no reftrees defined!");
				return null;
			}

			CRefTree mostSuitableTree = Trees[0];
			float bestSimilarity = 0;
			if (Trees.Count == 1)
			{
				return mostSuitableTree;
			}
			if (CProjectData.assignRandomRefTree)
			{
				int random = new Random().Next(0, Trees.Count);
				return Trees[random];

			}

			//Console.WriteLine(pTree.treeIndex + " similarities = \n");

			foreach (CRefTree refTree in Trees)
			{
				float similarity = CTreeMath.GetSimilarityWith(refTree, pTree);
				//Console.WriteLine(similarity);
				if (similarity > bestSimilarity)
				{
					mostSuitableTree = refTree;
					bestSimilarity = similarity;
				}
				if (bestSimilarity > 0.9f) { break; }
			}

			//Console.WriteLine("Most suitable ref tree = " + mostSuitableTree.Obj.Name + ". similarity = " + bestSimilarity);

			//Obj suitableTree = bestTree.Obj.Clone();
			//suitableTree.Name += "_" + counter;
			//counter++;
			return mostSuitableTree;
		}

		/*public static void ExportTrees()
		{
			Console.WriteLine("\nAdd ref trees to export ");
			foreach (CRefTree t in Trees)
			{
				//Obj tObj = t.GetObj("tree_" + Trees.IndexOf(t), true, false);
				Obj tObj = t.GetObj("refTree_" + t.Obj.Name, true, false);
				CProjectData.objsToExport.Add(tObj);
			}
		}*/

		/// <summary>
		/// Sets position, scale and todo: rotation of tree obj to match given pTargetTree 
		/// </summary>
		private static void SetRefTreeObjTransform(ref CRefTree pRefTree, CTree pTargetTree)
		{
			Vector3 arrayCenter = CProjectData.GetArrayCenter();
			float minHeight = CProjectData.GetMinHeight();



			//float treeHeight = pTargetTree.peak.maxHeight.Y - (float)groundHeight;
			float treeHeight = pTargetTree.GetTreeHeight();
			float heightRatio = treeHeight / pRefTree.GetTreeHeight();
			//todo: scale X and Z based on pTargetTree extents
			pRefTree.Obj.Scale = heightRatio * Vector3.One;


			//align position to tree
			pRefTree.Obj.Position = pTargetTree.peak.Center;
			pRefTree.Obj.Position.Y -= pRefTree.GetTreeHeight() * heightRatio;
			//pRefTree.Obj.Position.Y = pTargetTree.GetGroundHeight();

			//move obj so it is at 0,0,0
			pRefTree.Obj.Position -= arrayCenter;
			pRefTree.Obj.Position -= new Vector3(0, minHeight, 2 * pRefTree.Obj.Position.Z);

			pRefTree.Obj.Rotation = new Vector3(0, -CTreeMath.GetOffsetAngleTo(pRefTree, pTargetTree), 0);

			if (DEBUG)
			{
				Console.WriteLine(pRefTree.treeIndex +
					"[" + pRefTree.Obj.Position + "], " +
					"[" + pRefTree.Obj.Rotation + "]" +
					". treeHeight = " + treeHeight + ". heightRatio = " + heightRatio);
			}
		}
	}
}