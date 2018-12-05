using System;
using System.Collections.Generic;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public static class CObjPartition
	{
		public static List<Obj>[,] objPartition;
		private static int partitionXRange;
		private static int partitionYRange;

		private static int partitionStepSize;

		/// <summary>
		/// Calculate partition step size in proportion to used ground array step
		/// </summary>
		private static void SetPartitionStepSize()
		{
			float scaledPartitionStep =
				CParameterSetter.GetIntSettings(ESettings.partitionStep) /
				CParameterSetter.GetFloatSettings(ESettings.groundArrayStep);
			partitionStepSize = Math.Max(1, (int)scaledPartitionStep);
		}

		public static void Init()
		{
			SetPartitionStepSize();

			partitionXRange = CProjectData.array.arrayXRange / partitionStepSize + 1; //has to be +1
			partitionYRange = CProjectData.array.arrayYRange / partitionStepSize + 1;

			objPartition = new List<Obj>[partitionXRange, partitionYRange];
			Console.WriteLine($"objPartition: {partitionXRange} x {partitionYRange}");
			for (int x = 0; x < partitionXRange; x++)
			{
				for (int y = 0; y < partitionYRange; y++)
				{
					objPartition[x, y] = new List<Obj>();
				}
			}
		}

		public static void AddArray()
		{
			for (int x = 0; x < CProjectData.array.arrayXRange; x += partitionStepSize)
			{
				for (int y = 0; y < CProjectData.array.arrayYRange; y += partitionStepSize)
				{
					Obj groundArrayPartObj = CGroundFieldExporter.ExportToObj("array_[" + x + "," + y + "]",
						EExportStrategy.ZeroAroundDefined, true,
						new Tuple<int, int>(x, y), new Tuple<int, int>(x + partitionStepSize, y + partitionStepSize));

					//int partitionIndexX = x / partitionStepSize;
					//int partitionIndexY = y / partitionStepSize;
					AddObj(x, y, groundArrayPartObj);

					if (CParameterSetter.GetBoolSettings(ESettings.exportPoints))
					{
						List<Vector3> vegePoints = new List<Vector3>();
						List<Vector3> groundPoints = new List<Vector3>();
						List<Vector3> fakePoints = new List<Vector3>();
						for (int _x = x; _x < x + partitionStepSize; _x++)
						{
							for (int _y = y; _y < y + partitionStepSize; _y++)
							{
								CGroundField element = CProjectData.array.GetElement(_x, _y);
								if (element != null)
								{
									vegePoints.AddRange(element.vegePoints);
									groundPoints.AddRange(element.goundPoints);
									fakePoints.AddRange(element.fakePoints);
								}
							}
						}
						Obj vegePointsObj = new Obj("vegePoints");
						Obj groundPointsObj = new Obj("groundPoints");
						Obj fakePointsObj = new Obj("fakePoints");
						CObjExporter.AddPointsToObj(ref vegePointsObj, vegePoints);
						CObjExporter.AddPointsToObj(ref groundPointsObj, groundPoints);
						CObjExporter.AddPointsToObj(ref fakePointsObj, fakePoints);

						AddObj(x, y, vegePointsObj);
						AddObj(x, y, groundPointsObj);
						AddObj(x, y, fakePointsObj);
					}
				}
			}
		}

		public static void AddTrees(bool pValid)//, bool pFake)
		{
			List<Tuple<Tuple<int, int>, CTree>> treesToExport = new List<Tuple<Tuple<int, int>, CTree>>();

			bool exportTreeStrucure = CParameterSetter.GetBoolSettings(ESettings.exportTreeStructures);
			bool exportBoxes = CParameterSetter.GetBoolSettings(ESettings.exportTreeBoxes);

			if (!exportBoxes && !exportTreeStrucure) { return; }

			foreach (CGroundField f in CProjectData.array.fields)
			{
				foreach (CTree t in f.DetectedTrees)
				{
					if (t.isValid == pValid)
					{
						treesToExport.Add(new Tuple<Tuple<int, int>, CTree>(f.indexInField, t));
					}
				}
			}
			treesToExport.Sort((x, y) => x.Item2.treeIndex.CompareTo(y.Item2.treeIndex));
			foreach (Tuple<Tuple<int, int>, CTree> exportTree in treesToExport)
			{
				Obj obj = exportTree.Item2.GetObj(exportTreeStrucure, false, exportBoxes);
				if (!pValid) { obj.UseMtl = CMaterialManager.GetInvalidMaterial(); }

				AddObj(exportTree.Item1, obj);
			}
		}

		public static void AddRefTrees()
		{
			foreach (CGroundField f in CProjectData.array.fields)
			{
				foreach (CTree t in f.DetectedTrees)
				{
					if (t.isValid)
					{
						if (t.mostSuitableRefTreeObj == null)
						{
							//not error if reftrees were not loaded
							if (CReftreeManager.Trees.Count > 0)
							{
								CDebug.Error($"{t} has no reftree assigned");
							}
							return;
						}
						AddObj(f.indexInField, t.mostSuitableRefTreeObj);
					}
				}
			}
		}

		public static void AddCheckTrees(bool pAllCheckTrees)
		{
			foreach (CGroundField f in CProjectData.array.fields)
			{
				foreach (CCheckTree tree in f.CheckTrees)
				{
					Obj treeObj = tree.GetObj();
					if (tree.assignedTree != null) { treeObj.UseMtl = CMaterialManager.GetCheckTreeMaterial(); }
					else if (tree.isInvalid) { treeObj.UseMtl = CMaterialManager.GetInvalidMaterial(); }
					else { treeObj.UseMtl = CMaterialManager.GetAlarmMaterial(); }
					AddObj(f.indexInField, treeObj);

				}
			}

			if (pAllCheckTrees)
			{
				foreach (CCheckTree tree in CCheckTreeManager.Trees)
				{
					AddToPartition(tree.GetObj(), new Tuple<int, int>(0, 0));
				}
			}
		}

		public static void AddObj(Tuple<int, int> pArrayIndex, Obj pObj)
		{
			AddObj(pArrayIndex.Item1, pArrayIndex.Item2, pObj);
		}

		public static void AddObj(int pArrayIndexX, int pArrayIndexY, Obj pObj)
		{
			if (pObj == null)
			{
				CDebug.Error("AddObj is null!");
			}
			Tuple<int, int> index = GetIndexInArray(pArrayIndexX, pArrayIndexY);
			AddToPartition(pObj, index);
		}

		private static void AddToPartition(Obj pObj, Tuple<int, int> pIndex)
		{
			//TODO: error, incorrect partition
			if (pIndex.Item1 >= partitionXRange)
			{
				CDebug.Error($"obj {pObj.Name} has partition index {pIndex} OOB");
				pIndex = new Tuple<int, int>(partitionXRange - 1, pIndex.Item2);
			}
			if (pIndex.Item2 >= partitionYRange)
			{
				CDebug.Error($"obj {pObj.Name} has partition index {pIndex} OOB");
				pIndex = new Tuple<int, int>(pIndex.Item1, partitionYRange - 1);
			}
			objPartition[pIndex.Item1, pIndex.Item2].Add(pObj);
		}

		public static string folderPath;

		public static void ExportPartition(string pFileSuffix = "")
		{
			folderPath = CObjExporter.CreateFolder(CProjectData.saveFileName);

			//just creates a folder (for analytics etc)
			if (!CParameterSetter.GetBoolSettings(ESettings.export3d))
			{
				CDebug.WriteLine("Skipping export");
				return;
			}

			int counter = 0;
			DateTime exportPartitionStart = DateTime.Now;
			DateTime previousDebugStart = DateTime.Now;
			int partsCount = partitionXRange * partitionYRange;

			int debugFrequency = 1;
			for (int x = 0; x < partitionXRange; x++)
			{
				for (int y = 0; y < partitionYRange; y++)
				{
					if (CProjectData.backgroundWorker.CancellationPending) { return; }

					counter++;
					List<Obj> objsInPartition = objPartition[x, y];
					//export only if partition contains some objects (doesn't have to)
					if (objsInPartition.Count > 0)
					{
						CObjExporter.ExportObjs(objsInPartition, CProjectData.saveFileName +
						 "_[" + x + "," + y + "]" + pFileSuffix, folderPath);
					}

					CDebug.Progress(counter, partsCount, debugFrequency, ref previousDebugStart, exportPartitionStart, "Export of part");
				}
			}
		}

		private static Tuple<int, int> GetIndexInArray(int pArrayIndexX, int pArrayIndexY)
		{
			return new Tuple<int, int>(pArrayIndexX / partitionStepSize, pArrayIndexY / partitionStepSize);
		}
	}
}