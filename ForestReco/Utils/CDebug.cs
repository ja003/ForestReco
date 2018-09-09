using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public static class CDebug
	{
		public static List<Tuple<int, Vector3>> GetTreeStraight()
		{
			List<Tuple<int, Vector3>> points = new List<Tuple<int, Vector3>>();
			for (int i = 0; i < 10; i++)
			{
				const float POINT_STEP = 0.05f;
				points.Add(new Tuple<int, Vector3>(5, new Vector3(0,0,i*POINT_STEP)));
			}
			return points;
		}
	}
}