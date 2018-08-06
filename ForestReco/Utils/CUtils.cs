using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public static class CUtils
	{
		public static bool PointBelongsToTree(SVector3 pPoint, SVector3 pTreetop)
		{
			Vector3 botPoint = new Vector3((float)pTreetop.X, (float)pPoint.Y, (float)pTreetop.Z);
			List<Vector3> botTreetopPoint = new List<Vector3> {botPoint, pTreetop.ToVector3(), pPoint.ToVector3()};
			float angleBetweenPointAndTreetop = AngleBetweenThreePoints(botTreetopPoint, Vector3.UnitY);
			return angleBetweenPointAndTreetop < 45;
		}

		//TODO: check if correct
		//copied from: https://stackoverflow.com/questions/43493711/the-angle-between-two-3d-vectors-with-a-result-range-0-360
		//("The answer is to provide a reference UP vector:")
		public static float AngleBetweenThreePoints(List<Vector3> points, Vector3 up)
		{
			Vector3 v1 = points[1] - points[0];
			Vector3 v2 = points[2] - points[1];

			Vector3 cross = Vector3.Cross(v1, v2);
			float dot = Vector3.Dot(v1, v2);

			var angle = Math.Atan2(cross.Length(), dot);

			var test = Vector3.Dot(up, cross);
			if (test < 0.0) angle = -angle;
			return (float)angle;
		}
	}
}