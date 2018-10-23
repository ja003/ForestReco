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
		public static List<CRefTree> Trees;
		private const float TREE_POINT_EXTENT = 0.2f;

		public static bool DEBUG = false;

		public static void Init()
		{
			Trees = new List<CRefTree>();
			//string podkladyPath = CPlatformManager.GetPodkladyPath();
			List<string> treeFileNames = new List<string>()
			{
				//"R1",
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


				//"R7_test"
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
			if (Trees.Count == 0)
			{
				CDebug.Error("no reftrees loaded");
				return;
			}

			DateTime addTreeObjModelsStart = DateTime.Now;
			CDebug.WriteLine("Get ref tree models");

			const int debugFrequency = 10;

			DateTime assignRefTreesStart = DateTime.Now;
			CDebug.WriteLine("AssignRefTrees");

			DateTime previousDebugStart = DateTime.Now;

			int counter = 0;
			foreach (CTree t in CTreeManager.Trees)
			{
				Tuple<CRefTree, STreeSimilarity> suitableRefTree = GetMostSuitableRefTree(t);
				CRefTree mostSuitableRefTree = suitableRefTree.Item1;
				if (mostSuitableRefTree == null)
				{
					CDebug.Error("no reftrees assigned!");
					continue;
				}

				SetRefTreeObjTransform(ref mostSuitableRefTree, t, suitableRefTree.Item2.angleOffset);

				Obj suitableTreeObj = mostSuitableRefTree.Obj.Clone();
				suitableTreeObj.Name += "_" + t.treeIndex;
				t.mostSuitableRefTreeObj = suitableTreeObj;

				CDebug.Progress(counter, CTreeManager.Trees.Count, debugFrequency, ref previousDebugStart, "Assigned reftree");
				//if (counter % debugFrequency == 0 && counter > 0)
				//{
				//	CDebug.WriteLine("\nAssigned reftree " + counter + " out of " + CTreeManager.Trees.Count);
				//	double lastAssignmentProcessTime = (DateTime.Now - previousDebugStart).TotalSeconds;
				//	CDebug.WriteLine("- time of last " + debugFrequency + " trees = " + lastAssignmentProcessTime);

				//	float remainsRatio = (float)(CTreeManager.Trees.Count - counter) / debugFrequency;
				//	double totalSeconds = remainsRatio * lastAssignmentProcessTime;
				//	TimeSpan ts = new TimeSpan(0, 0, 0, (int)totalSeconds);
				//	string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";
				//	CDebug.WriteLine("- estimated time left = " + timeString);

				//	previousDebugStart = DateTime.Now;
				//}
				counter++;


				//counter++;

				if (DEBUG) { CDebug.WriteLine("\n mostSuitableRefTree = " + mostSuitableRefTree); }

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

			CDebug.Duration("Assign ref tree models", addTreeObjModelsStart);

			//return treeObjs;
		}


		private static void LoadTrees(List<string> pFileNames)
		{
			CDebug.Step(EProgramStep.LoadReftrees);

			DateTime loadTreesStartTime = DateTime.Now;
			CDebug.WriteLine("Load ref trees: ");
			foreach (string fileName in pFileNames)
			{
				CDebug.WriteLine(" - " + fileName);
			}

			int counter = 0;
			for (int i = 0; i < pFileNames.Count; i++)
			{
				if (CProgramStarter.abort) { return; }

				string fileName = pFileNames[i];
				CDebug.Progress(i, pFileNames.Count, 1, ref loadTreesStartTime, "load reftree");

				CRefTree deserializedRefTree = CRefTree.Deserialize(fileName);
				CRefTree refTree = deserializedRefTree ??
										 new CRefTree(fileName, pFileNames.IndexOf(fileName), TREE_POINT_EXTENT, true);

				refTree.Obj.UseMtl = CMaterialManager.GetRefTreeMaterial(counter);

				Trees.Add(refTree);
				CDebug.WriteLine("Loaded tree: " + fileName);

				counter++;
			}
			CDebug.Duration("Load ref trees", loadTreesStartTime);

			DebugRefTrees();
		}

		private static void DebugRefTrees()
		{
			CDebug.WriteLine("Loaded reftrees: ");
			foreach (CRefTree refTree in Trees)
			{
				CDebug.WriteLine(refTree.ToString());
			}
		}

		//private static int counter;

		private static Tuple<CRefTree, STreeSimilarity> GetMostSuitableRefTree(CTree pTree)
		{
			if (Trees.Count == 0)
			{
				CDebug.Error("no reftrees defined!");
				return null;
			}

			CRefTree mostSuitableTree = Trees[0];
			STreeSimilarity treeSimilarity = new STreeSimilarity();
			float bestSimilarity = 0;
			if (Trees.Count == 1)
			{
				return new Tuple<CRefTree, STreeSimilarity>(mostSuitableTree, treeSimilarity); ;
			}
			if (CProjectData.assignRandomRefTree)
			{
				int random = new Random().Next(0, Trees.Count);
				return new Tuple<CRefTree, STreeSimilarity>(Trees[random], treeSimilarity);
			}

			//CDebug.WriteLine(pTree.treeIndex + " similarities = \n");

			foreach (CRefTree refTree in Trees)
			{
				treeSimilarity = CTreeMath.GetSimilarityWith(refTree, pTree);
				float similarity = treeSimilarity.similarity;
				if (similarity > bestSimilarity)
				{
					mostSuitableTree = refTree;
					bestSimilarity = similarity;
				}
				if (bestSimilarity > 0.9f) { break; }
			}

			//CDebug.WriteLine("Most suitable ref tree = " + mostSuitableTree.Obj.Name + ". similarity = " + bestSimilarity);

			//Obj suitableTree = bestTree.Obj.Clone();
			//suitableTree.Name += "_" + counter;
			//counter++;

			return new Tuple<CRefTree, STreeSimilarity>(mostSuitableTree, treeSimilarity);
		}

		/// <summary>
		/// Sets position, scale and todo: rotation of tree obj to match given pTargetTree 
		/// </summary>
		private static void SetRefTreeObjTransform(ref CRefTree pRefTree, CTree pTargetTree, int pAngleOffset)
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

			pRefTree.Obj.Rotation = new Vector3(0, -pAngleOffset, 0);
			//pRefTree.Obj.Rotation = new Vector3(0, -CTreeMath.GetOffsetAngleTo(pRefTree, pTargetTree), 0);

			if (DEBUG)
			{
				CDebug.WriteLine(pRefTree.treeIndex +
					"[" + pRefTree.Obj.Position + "], " +
					"[" + pRefTree.Obj.Rotation + "]" +
					". treeHeight = " + treeHeight + ". heightRatio = " + heightRatio);
			}
		}
	}
}