using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public static class CDebug
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

		public static List<Tuple<int, Vector3>> GetStandartTree()
		{
			List<Vector3> points = new List<Vector3>();
			points.Add(new Vector3(0, 0, 1));
			points.Add(new Vector3(0, 0, .5f));

			points.Add(new Vector3(.2f, 0, .5f));
			points.Add(new Vector3(-.2f, 0, .5f));
			points.Add(new Vector3(0, .2f, .5f));
			points.Add(new Vector3(0, -.2f, .5f));


			List<Tuple<int, Vector3>> pointTuples = new List<Tuple<int, Vector3>>();
			foreach (Vector3 p in points)
			{
				pointTuples.Add(new Tuple<int, Vector3>(5, p));
			}

			return pointTuples;
		}
	}
}