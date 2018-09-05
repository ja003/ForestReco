using System;
using System.Collections.Generic;

namespace ForestReco
{
	public static class CDebug
	{
		public static List<Tuple<int, SVector3>> GetTreeStraight()
		{
			List<Tuple<int, SVector3>> points = new List<Tuple<int, SVector3>>();
			for (int i = 0; i < 200; i++)
			{
				const float POINT_STEP = 0.05f;
				points.Add(new Tuple<int, SVector3>(5, new SVector3(0,0,i*POINT_STEP)));
			}
			return points;
		}
	}
}