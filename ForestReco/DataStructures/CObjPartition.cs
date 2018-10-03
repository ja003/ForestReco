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
		
		public static void AddTrees()
		{
			foreach (CGroundField f in CProjectData.array.fields)
			{
				foreach (CTree t in f.DetectedTrees)
				{
					AddObj(f.indexInField, t.GetObj(true, false));
				}
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

		public static void AddObj(Tuple<int, int> pArrayIndex, Obj pObj)
		{
			AddObj(pArrayIndex.Item1, pArrayIndex.Item2, pObj);
		}

		public static void AddObj(int pArrayIndexX, int pArrayIndexY, Obj pObj)
		{
			if (pObj == null)
			{
				Console.WriteLine("!");
			}
			Tuple<int, int> index = GetIndexInArray(pArrayIndexX, pArrayIndexY);
			objPartition[index.Item1, index.Item2].Add(pObj);
		}

		public static void ExportPartition()
		{
			string folderPath = CObjExporter.CreateFolder(CProjectData.saveFileName);
			for (int x = 0; x < partitionXRange; x++)
			{
				for (int y = 0; y < partitionYRange; y++)
				{
					CObjExporter.ExportObjs(objPartition[x, y], CProjectData.saveFileName + "_[" + x + "," + y + "]", folderPath);
				}
			}
		}

		private static Tuple<int, int> GetIndexInArray(int pArrayIndexX, int pArrayIndexY)
		{
			return new Tuple<int, int>(pArrayIndexX / partitionStep, pArrayIndexY / partitionStep);
		}

	}
}