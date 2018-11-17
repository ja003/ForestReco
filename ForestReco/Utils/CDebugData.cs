using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public static class CDebugData
	{
		const float POINT_STEP = 0.05f;

		public static List<Tuple<int, Vector3>> GetTreeStraight()
		{
			List<Tuple<int, Vector3>> points = new List<Tuple<int, Vector3>>();
			for (int i = 0; i < 10; i++)
			{
				points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 0, i * POINT_STEP)));
			}
			return points;
		}

		public static List<Tuple<int, Vector3>> GetTreeStraight2()
		{
			List<Tuple<int, Vector3>> points = new List<Tuple<int, Vector3>>();
			points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 0, 1)));
			points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 0, 1 - POINT_STEP)));
			points.Add(new Tuple<int, Vector3>(5, new Vector3(5 * POINT_STEP, 0, 1)));
			points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 5 * POINT_STEP, 1)));

			points.Add(new Tuple<int, Vector3>(5, new Vector3(0, 0, 0)));

			return points;
		}

		public static List<Tuple<EClass, Vector3>> GetStandartTree()
		{
			List<Vector3> points = new List<Vector3>();
			points.Add(new Vector3(0, 0, 1));
			points.Add(new Vector3(0, 0, .5f));

			points.Add(new Vector3(.2f, 0, .5f));
			points.Add(new Vector3(-.2f, 0, .5f));
			points.Add(new Vector3(0, .2f, .5f));
			points.Add(new Vector3(0, -.2f, .5f));


			List<Tuple<EClass, Vector3>> pointTuples = new List<Tuple<EClass, Vector3>>();
			foreach (Vector3 p in points)
			{
				pointTuples.Add(new Tuple<EClass, Vector3>(EClass.Ground, p));
			}

			return pointTuples;
		}

		public static void DefineArray(bool pConstantHeight, float pHeight)
		{
			CDebug.WriteLine("Define debug array");

			CProjectData.header = new CHeaderInfo(new[]
			{
				"","","","","","","","","","","","","","","",
				"0 0 0", "0 0 0" , "0 0 0" , "0 0 0"
			});
			CProjectData.array = new CGroundArray(CParameterSetter.groundArrayStep);

			if (pConstantHeight)
			{
				for (int x = 0; x < CProjectData.array.arrayXRange; x++)
				{
					for (int y = 0; y < CProjectData.array.arrayXRange; y++)
					{
						//CProjectData.array.GetElement(x, y).AddPoint(new Vector3(0, pHeight, 0));
						CProjectData.array.SetHeight(pHeight, x, y);
					}
				}
			}
			else
			{
				CProjectData.array.SetHeight(0, 0, 0);
				CProjectData.array.SetHeight(0, CProjectData.array.arrayXRange - 1, 0);

				CProjectData.array.SetHeight(2, CProjectData.array.arrayXRange / 2, CProjectData.array.arrayYRange / 2);

				CProjectData.array.SetHeight(5, CProjectData.array.arrayXRange - 1, CProjectData.array.arrayYRange - 1);
				CProjectData.array.SetHeight(5, 0, CProjectData.array.arrayYRange - 1);
			}
			CObjPartition.Init();
		}
	}
}