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

		public static int partitionStep = 10;

		public static void Init()
		{
			partitionXRange = CProjectData.array.arrayXRange / partitionStep + 1;
			partitionYRange = CProjectData.array.arrayYRange / partitionStep + 1;
			objPartition = new List<Obj>[partitionXRange, partitionYRange];
			for (int x = 0; x < partitionXRange; x++)
			{
				for (int y = 0; y < partitionYRange; y++)
				{
					objPartition[x,y] = new List<Obj>();
				}
			}
		}

		public static void AddArray()
		{
			for (int x = 0; x < CProjectData.array.arrayXRange; x += partitionStep)
			{
				for (int y = 0; y < CProjectData.array.arrayYRange; y += partitionStep)
				{
					Obj groundArrayPartObj = CGroundFieldExporter.ExportToObj("array_[" + x + "," + y + "]",
						EExportStrategy.ZeroAroundDefined, true,
						new Tuple<int, int>(x, y), new Tuple<int, int>(x + partitionStep, y + partitionStep));

					//int partitionIndexX = x / partitionStep;
					//int partitionIndexY = y / partitionStep;
					AddObj(x, y, groundArrayPartObj);

					if (CProjectData.exportPoints)
					{
						List<Vector3> vegePoints = new List<Vector3>();
						List<Vector3> groundPoints = new List<Vector3>();
						for (int _x = x; _x < x + partitionStep; _x++)
						{
							for (int _y = y; _y < y+partitionStep; _y++)
							{
								CGroundField element = CProjectData.array.GetElement(_x, _y);
								if (element != null)
								{
									vegePoints.AddRange(element.vegePoints);
									groundPoints.AddRange(element.goundPoints);
								}
							}
						}
						Obj vegePointsObj = new Obj("vegePoints");
						Obj groundPointsObj = new Obj("groundPoints");
						CObjExporter.AddPointsToObj(ref vegePointsObj, vegePoints);
						CObjExporter.AddPointsToObj(ref groundPointsObj, groundPoints);

						AddObj(x, y, vegePointsObj);
						AddObj(x, y, groundPointsObj);
					}
				}
			}
		}
		
		public static void AddTrees(bool pValid)
		{
			List<Tuple<Tuple<int, int>, CTree>> treesToExport = new List<Tuple<Tuple<int, int>, CTree>>();

			foreach (CGroundField f in CProjectData.array.fields)
			{
				//todo: k ničemu, výsledek stejně není seřaděn
				//- do OBJ předat informaci o pořadí a pak je sesortit
				//f.DetectedTrees.Sort((x, y) => x.treeIndex.CompareTo(y.treeIndex));
				foreach (CTree t in f.DetectedTrees)
				{
					if (t.isValid == pValid)
					{
						treesToExport.Add(new Tuple<Tuple<int, int>, CTree>(f.indexInField, t));
						//AddObj(f.indexInField, t.GetObj(true, false));
					}
				}
			}
			treesToExport.Sort((x,y) => x.Item2.treeIndex.CompareTo(y.Item2.treeIndex));
			foreach (Tuple<Tuple<int, int>, CTree> exportTree in treesToExport)
			{
				Obj obj = exportTree.Item2.GetObj(true, false);
				if (!pValid) { obj.UseMtl = CMaterialManager.GetInvalidMaterial();}
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
					AddObj(f.indexInField, treeObj);

				}
			}

			if (pAllCheckTrees)
			{
				foreach (CCheckTree tree in CCheckTreeManager.Trees)
				{
					AddToPartiotion(tree.GetObj(), new Tuple<int, int>(0,0));
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
				Console.WriteLine("AddObj is null!");
			}
			Tuple<int, int> index = GetIndexInArray(pArrayIndexX, pArrayIndexY);
			AddToPartiotion(pObj, index);
		}

		private static void AddToPartiotion(Obj pObj, Tuple<int, int> pIndex)
		{
			objPartition[pIndex.Item1, pIndex.Item2].Add(pObj);
		}

		public static void ExportPartition()
		{
			string folderPath = CObjExporter.CreateFolder(CProjectData.saveFileName);
			int counter = 0;
			DateTime previousDebugStart = DateTime.Now;
			int partsCount = partitionXRange * partitionYRange;

			for (int x = 0; x < partitionXRange; x++)
			{
				for (int y = 0; y < partitionYRange; y++)
				{
					counter++;
					CObjExporter.ExportObjs(objPartition[x, y], CProjectData.saveFileName + "_[" + x + "," + y + "]", folderPath);

					//if (counter % debugFrequency == 0 && counter > 0)
					{
						Console.WriteLine("\nExport of part " + counter + " out of " + partsCount);
						double lastExportTime = (DateTime.Now - previousDebugStart).TotalSeconds;
						Console.WriteLine("- export time of this part = " + lastExportTime);

						float remainsRatio = (float)(partsCount - counter) / 1;
						double totalSeconds = remainsRatio * lastExportTime;
						TimeSpan ts = new TimeSpan(0, 0, 0, (int)totalSeconds);
						string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";
						Console.WriteLine("- estimated time left = " + timeString + "\n");

						previousDebugStart = DateTime.Now;
					}
				}
			}
		}

		private static Tuple<int, int> GetIndexInArray(int pArrayIndexX, int pArrayIndexY)
		{
			return new Tuple<int, int>(pArrayIndexX / partitionStep, pArrayIndexY / partitionStep);
		}

	}
}