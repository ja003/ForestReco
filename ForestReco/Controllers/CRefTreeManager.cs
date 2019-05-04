using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public static class CReftreeManager
	{
		public static List<CRefTree> Trees;
		private const float TREE_POINT_EXTENT = 0.2f;

		public static void Init()
		{
			Trees = new List<CRefTree>();
			List<string> treeFileNames = GetTreeFileNames();
			LoadTrees(treeFileNames);
		}

		private static List<string> GetTreeFileNames()
		{
			List<string> names = new List<string>();
			string folderPath = CParameterSetter.GetStringSettings(ESettings.reftreeFolderPath);
			string[] subfolders = Directory.GetDirectories(folderPath);

			for (int i = 0; i < subfolders.Length; i++)
			{
				string subfolderPath = subfolders[i];
				string[] pathSplit = subfolderPath.Split('\\');
				string subfolderName = pathSplit[pathSplit.Length - 1];
				//skip folder named "ignore"
				if(subfolderName.Contains("ignore")){ continue;}
				names.Add(subfolderName);
			}

			return names;
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
			CDebug.WriteLine("Get reftree models");

			const int debugFrequency = 10;

			DateTime assignRefTreesStart = DateTime.Now;
			CDebug.WriteLine("AssignRefTrees");

			DateTime previousDebugStart = DateTime.Now;

			int counter = 0;
			float similaritySum = 0;
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
				similaritySum += suitableRefTree.Item2.similarity;
				//CDebug.WriteLine($"similaritySum += {suitableRefTree.Item2.similarity}");

				Obj suitableTreeObj = mostSuitableRefTree.Obj.Clone();
				suitableTreeObj.Name += "_" + t.treeIndex;
				t.mostSuitableRefTreeObj = suitableTreeObj;
				t.RefTreeTypeName = mostSuitableRefTree.RefTreeTypeName; //copy the type name

				suitableTreeObj.UseMtl = t.assignedMaterial;

				CDebug.Progress(counter, CTreeManager.Trees.Count, debugFrequency, ref previousDebugStart, assignRefTreesStart, "Assigned reftree");
				counter++;


				//CDebug.WriteLine("\n mostSuitableRefTree = " + mostSuitableRefTree); 

			}


			CAnalytics.averageReftreeSimilarity = similaritySum / CTreeManager.Trees.Count;

			CAnalytics.reftreeAssignDuration = CAnalytics.GetDuration(addTreeObjModelsStart);
			CDebug.Duration("Assign reftree models", addTreeObjModelsStart);
		}


		private static void LoadTrees(List<string> pFileNames)
		{
			CDebug.Step(EProgramStep.LoadReftrees);

			DateTime loadTreesStartTime = DateTime.Now;
			DateTime lastDebugTime = DateTime.Now;

			CDebug.WriteLine("Load reftrees: ");
			foreach (string fileName in pFileNames)
			{
				CDebug.WriteLine(" - " + fileName);
			}

			for (int i = 0; i < pFileNames.Count; i++)
			{
				if (CProjectData.backgroundWorker.CancellationPending) { return; }

				string fileName = pFileNames[i];
				CDebug.Progress(i, pFileNames.Count, 1, ref lastDebugTime, loadTreesStartTime, "load reftree");

				CRefTree deserializedRefTree = CRefTree.Deserialize(fileName);
				CRefTree refTree = deserializedRefTree ??
										 new CRefTree(fileName, pFileNames.IndexOf(fileName), TREE_POINT_EXTENT, true);

				refTree.RefTreeTypeName = fileName; //GetTypeName(fileName);

				if (!refTree.isValid)
				{
					//this reftree is not valid. case for reftrees in 'ignore' folder
					CDebug.Warning($"Skipping reftree {fileName}");
					continue;
				}
				//material set durring assigning to tree
				//refTree.Obj.UseMtl = CMaterialManager.GetRefTreeMaterial(counter);

				Trees.Add(refTree);
				CDebug.WriteLine($"Loaded tree: {fileName}. height = {refTree.GetTreeHeight()}");
			}
			CAnalytics.loadReftreesDuration = CAnalytics.GetDuration(loadTreesStartTime);
			CDebug.Duration("Load reftrees", loadTreesStartTime);

			CAnalytics.loadedReftrees = Trees.Count;

			DebugReftrees();
		}
		
		private static void DebugReftrees()
		{
			CDebug.WriteLine("Loaded reftrees: ");
			foreach (CRefTree refTree in Trees)
			{
				CDebug.WriteLine(refTree.ToString());
			}
		}

		static int debugTree = 185;

		public static bool debugSimilarites = true; //todo: nějak omezit?
		public static bool forceAlgorithm = false;

		private static Tuple<CRefTree, STreeSimilarity> GetMostSuitableRefTree(CTree pTree)
		{
			if (Trees.Count == 0)
			{
				CDebug.Error("no reftrees defined!");
				return null;
			}

			CRefTree mostSuitableTree = Trees[0];
			STreeSimilarity treeSimilarity = new STreeSimilarity();
			STreeSimilarity bestSimilarity = new STreeSimilarity();
			if (Trees.Count == 1 && !forceAlgorithm)
			{
				return new Tuple<CRefTree, STreeSimilarity>(mostSuitableTree, treeSimilarity);
			}
			bool forceRandom = false;//pTree.treeIndex != debugTree;

			bool randomReftree = CParameterSetter.GetBoolSettings(ESettings.assignRefTreesRandom)
				&& pTree.treeIndex != debugTree;
			if (forceRandom || randomReftree)
			{
				int random = new Random().Next(0, Trees.Count);
				//if (debugSimilarites) { CDebug.WriteLine($"random = {random}"); }
				return new Tuple<CRefTree, STreeSimilarity>(Trees[random], treeSimilarity);
			}
			if (debugSimilarites) { CDebug.WriteLine("\n" + pTree.treeIndex + " similarities = "); }


			foreach (CRefTree refTree in Trees)
			{
				treeSimilarity = CTreeMath.GetSimilarityWith(refTree, pTree);
				//float similarity = treeSimilarity.similarity;
				if (debugSimilarites) { CDebug.WriteLine($"{refTree.fileName} similarity = {treeSimilarity.similarity}"); }

				if (treeSimilarity.similarity > bestSimilarity.similarity)
				{
					mostSuitableTree = refTree;
					bestSimilarity = treeSimilarity;
				}
				if (bestSimilarity.similarity > 0.9f && !forceAlgorithm) { break; }
				if (CProjectData.backgroundWorker.CancellationPending) { break; }
			}

			if (debugSimilarites)
			{
				CDebug.WriteLine("Most suitable reftree = " + mostSuitableTree.Obj.Name + ". similarity = " + bestSimilarity.similarity);
				CDebug.WriteLine($"tree height = {pTree.GetTreeHeight()}");
				CDebug.WriteLine($"reftree height = {mostSuitableTree.GetTreeHeight()}");
				CDebug.WriteLine($"angle offset = {bestSimilarity.angleOffset}");
			}

			return new Tuple<CRefTree, STreeSimilarity>(mostSuitableTree, bestSimilarity);
		}

		/// <summary>
		/// Sets position, scale and rotation of tree obj to match given pTargetTree 
		/// </summary>
		private static void SetRefTreeObjTransform(ref CRefTree pRefTree, CTree pTargetTree, int pAngleOffset)
		{
			Vector3 arrayCenter = CProjectData.GetArrayCenter();
			float minHeight = CProjectData.GetMinHeight();

			//float treeHeight = pTargetTree.peak.maxHeight.Y - (float)groundHeight;
			float treeHeight = pTargetTree.GetTreeHeight();
			float heightRatio = treeHeight / pRefTree.GetTreeHeight();
			pRefTree.Obj.Scale = heightRatio * Vector3.One;

			//align position to tree
			pRefTree.Obj.Position = pTargetTree.peak.Center;
			pRefTree.Obj.Position.Y -= pRefTree.GetTreeHeight() * heightRatio;

			//move obj so it is at 0,0,0
			pRefTree.Obj.Position -= arrayCenter;
			pRefTree.Obj.Position -= new Vector3(0, minHeight, 2 * pRefTree.Obj.Position.Z);

			pRefTree.Obj.Rotation = new Vector3(0, -pAngleOffset, 0);
		}
	}
}