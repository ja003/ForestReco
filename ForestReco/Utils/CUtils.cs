using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public static class CUtils
	{
		public static bool PointBelongsToTree(Vector3 pPoint, Vector3 pTreetop)
		{
			Vector3 botPoint = new Vector3(pTreetop.X, pPoint.Y, pTreetop.Z);
			List<Vector3> botTreetopPoint = new List<Vector3> { botPoint, pTreetop, pPoint };
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

		public static float GetAngle(Vector2 a, Vector2 b)
		{
			a = Vector2.Normalize(a);
			b = Vector2.Normalize(b);
			double atan2 = Math.Atan2(b.Y, b.X) - Math.Atan2(a.Y, a.X);
			return ToDegree((float)atan2);
		}

		public static float AngleBetweenThreePoints(List<Vector3> points, Vector3 up)
		{
			return AngleBetweenThreePoints(points);
		}

		public static float GetAngleToTree(CTree pTree, Vector3 pPoint)
		{
			List<Vector3> points = new List<Vector3>
			{
				pTree.peak.Center - Vector3.UnitY * 100,
				pTree.peak.Center,
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

		public static float ToRadians(float val)
		{
			return (float)Math.PI / 180 * val;
		}

		private static float ToDegree(float angle)
		{
			return (float)(angle * (180.0 / Math.PI));
		}

		public static float Get2DDistance(Vector3 a, Vector3 b)
		{
			return Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));
		}

		private static Random rng = new Random();

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static string SerializeVector3(Vector3 pVector)
		{
			return pVector.X + " " + pVector.Y + " " + pVector.Z;
		}
	}
}
