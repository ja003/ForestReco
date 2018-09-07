using System;
using System.Numerics;

namespace ForestReco
{
	public class CBoundingBoxObject
	{

		//bounding box corners
		protected Vector3 minBB;
		protected Vector3 maxBB;

		public Vector3 Center => new Vector3((minBB.X + maxBB.X) / 2, (minBB.Y + maxBB.Y) / 2, (minBB.Z + maxBB.Z) / 2);

		public Vector3 Extent => 2 * new Vector3(maxBB.X - Center.X, maxBB.Y - Center.Y, maxBB.Z - Center.Z);

		public CBoundingBoxObject(Vector3 pPoint)
		{
			minBB = pPoint;
			maxBB = pPoint;
		}

		public void OnAddPoint(Vector3 pPoint)
		{
			if (pPoint.X < minBB.X) { minBB.X = pPoint.X; }
			if (pPoint.Y < minBB.Y) { minBB.Y = pPoint.Y; }
			if (pPoint.Z < minBB.Z) { minBB.Z = pPoint.Z; }

			if (pPoint.X > maxBB.X) { maxBB.X = pPoint.X; }
			if (pPoint.Y > maxBB.Y) { maxBB.Y = pPoint.Y; }
			if (pPoint.Z > maxBB.Z) { maxBB.Z = pPoint.Z; }
		}

		protected bool Contains(Vector3 pPoint)
		{
			return pPoint.X > minBB.X && pPoint.X < maxBB.X &&
					 pPoint.Y > minBB.Y && pPoint.Y < maxBB.Y &&
					 pPoint.Z > minBB.Z && pPoint.Z < maxBB.Z;
		}



		/// <summary>
		/// Returns Manhattan distance from center to point minus BB extents.
		/// Distance = point is on BB or inside
		/// </summary>
		public float Get2DDistanceFromBBTo(Vector3 pPoint)
		{
			float xDist = Math.Abs(pPoint.X - Center.X) - Extent.X / 2;
			xDist = Math.Max(0, xDist);
			float zDist = Math.Abs(pPoint.Z - Center.Z) - Extent.Z / 2;
			zDist = Math.Max(0, zDist);
			return xDist + zDist;
		}

		public bool IsPointInside(Vector3 pPoint)
		{
			return IsPointInsideX(pPoint) && IsPointInsideY(pPoint) && IsPointInsideZ(pPoint);
		}

		private bool IsPointInsideX(Vector3 pPoint)
		{
			return IsPointInside1D(pPoint.X, Center.X, Extent.X);
		}
		private bool IsPointInsideZ(Vector3 pPoint)
		{
			return IsPointInside1D(pPoint.Z, Center.Z, Extent.Z);
		}
		private bool IsPointInsideY(Vector3 pPoint)
		{
			return IsPointInside1D(pPoint.Y, Center.Y, Extent.Y);
		}

		private static bool IsPointInside1D(float pPoint, float pCenter, float pExtent)
		{
			return Math.Abs(pPoint - pCenter) < pExtent / 2;
		}
	}
}