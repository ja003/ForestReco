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
			List<Vector3> botTreetopPoint = new List<Vector3> { botPoint, pTreetop.ToVector3(), pPoint.ToVector3() };
			float angleBetweenPointAndTreetop = AngleBetweenThreePoints(botTreetopPoint, Vector3.UnitY);
			return angleBetweenPointAndTreetop < 45;
		}

		//TODO: check if correct
		//copied from: https://stackoverflow.com/questions/43493711/the-angle-between-two-3d-vectors-with-a-result-range-0-360
		//("The answer is to provide a reference UP vector:")
		/*public static float AngleBetweenThreePoints(List<Vector3> points, Vector3 up, bool pToDegree = true)
		{
			Vector3 v1 = points[1] - points[0];
			Vector3 v2 = points[2] - points[1];

			Vector3 cross = Vector3.Cross(v1, v2);
			float dot = Vector3.Dot(v1, v2);

			var angle = Math.Atan2(cross.Length(), dot);

			var test = Vector3.Dot(up, cross);
			if (test < 0.0) angle = -angle;

			if (pToDegree) { return (float)ToDegree(angle); }
			return (float)angle;
		}*/

		public static double GetAngle(Vector2 a, Vector2 b)
		{
			a = Vector2.Normalize(a);
			b = Vector2.Normalize(b);
			return ToDegree(Math.Atan2(b.Y - a.Y, b.X - a.X));
		}

		public static float AngleBetweenThreePoints(List<Vector3> points, Vector3 up)
		{
			return AngleBetweenThreePoints(points);
		}

		public static float GetAngleToTree(CTree pTree, Vector3 pPoint)
		{
			List<Vector3> points = new List<Vector3>
			{
				pTree.peak-Vector3.UnitY,
				pTree.peak,
				pPoint
			};
			return AngleBetweenThreePoints(points);
		}

		//https://stackoverflow.com/questions/19729831/angle-between-3-points-in-3d-space
		public static float AngleBetweenThreePoints(List<Vector3> points, bool pToDegree = true)
		{
			Vector3 A = points[0];
			Vector3 B = points[1];
			Vector3 C = points[2];

			Vector3 v1 = new Vector3(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
			//Similarly the vector BC(call it v2) is:

			Vector3 v2 = new Vector3(C.X - B.X, C.Y - B.Y, C.Z - B.Z);
			//The dot product of v1 and v2 is a function of the cosine of the angle between them(it's scaled by the product of their magnitudes). So first normalize v1 and v2:

			float v1mag = (float)Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
			Vector3 v1norm = new Vector3(v1.X / v1mag, v1.Y / v1mag, v1.Z / v1mag);

			float v2mag = (float)Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);
			Vector3 v2norm = new Vector3(v2.X / v2mag, v2.Y / v2mag, v2.Z / v2mag);
			//Then calculate the dot product:

			float res = v1norm.X * v2norm.X + v1norm.Y * v2norm.Y + v1norm.Z * v2norm.Z;
			//And finally, recover the angle:

			float angle = (float)Math.Acos(res);
			if (pToDegree) { return (float)ToDegree(angle); }
			return angle;
		}

		public static double ToRadians(double val)
		{
			return Math.PI / 180 * val;
		}

		private static double ToDegree(double angle)
		{
			return angle * (180.0 / Math.PI);
		}

		public static float Get2DDistance(Vector3 a, Vector3 b)
		{
			return Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));
		}
	}
}
