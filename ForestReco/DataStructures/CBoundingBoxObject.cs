using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CBoundingBoxObject
	{
		//bounding box corners
		public Vector3 minBB;
		public Vector3 maxBB;

		public Vector3 botCenter => (b000 + b101) / 2;

		public Vector3 b000 => minBB;
		public Vector3 b100 => new Vector3(maxBB.X, minBB.Y, minBB.Z);
		public Vector3 b010 => new Vector3(minBB.X, maxBB.Y, minBB.Z);
		public Vector3 b001 => new Vector3(minBB.X, minBB.Y, maxBB.Z);
		public Vector3 b110 => new Vector3(maxBB.X, maxBB.Y, minBB.Z);
		public Vector3 b101 => new Vector3(maxBB.X, minBB.Y, maxBB.Z);
		public Vector3 b011 => new Vector3(minBB.X, maxBB.Y, maxBB.Z);
		public Vector3 b111 => new Vector3(maxBB.X, maxBB.Y, maxBB.Z);


		public Vector3 Center => new Vector3((minBB.X + maxBB.X) / 2, (minBB.Y + maxBB.Y) / 2, (minBB.Z + maxBB.Z) / 2);

		public Vector3 Extent => 2 * new Vector3(maxBB.X - Center.X, maxBB.Y - Center.Y, maxBB.Z - Center.Z);

		public float Volume => Extent.X * Extent.Y * Extent.Z;

		public CBoundingBoxObject() { }

		public CBoundingBoxObject(Vector3 pPoint)
		{
			minBB = pPoint;
			maxBB = pPoint;
		}

		public string Serialize()
		{
			return CUtils.SerializeVector3(minBB) + " " + CUtils.SerializeVector3(maxBB);
		}

		public void ResetBounds(Vector3 pPoint)
		{
			minBB = pPoint;
			maxBB = pPoint;
		}

		public void ResetBounds(List<Vector3> pPoints)
		{
			minBB = pPoints[0];
			maxBB = pPoints[0];
			foreach (Vector3 p in pPoints)
			{
				OnAddPoint(p);
			}
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


		public virtual bool Contains(Vector3 pPoint)
		{
			return
				pPoint.X > minBB.X && pPoint.X < maxBB.X &&
				pPoint.Y > minBB.Y && pPoint.Y < maxBB.Y &&
				pPoint.Z > minBB.Z && pPoint.Z < maxBB.Z;
		}

		public virtual bool Contains2D(Vector3 pPoint)
		{
			return
				pPoint.X > minBB.X && pPoint.X < maxBB.X &&
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

		public List<Vector3> GetBBPoints()
		{
			List<Vector3> bbPoints = new List<Vector3>();
			bbPoints.Add(b000);
			bbPoints.Add(b100);
			bbPoints.Add(b010);
			bbPoints.Add(b001);
			bbPoints.Add(b110);
			bbPoints.Add(b101);
			bbPoints.Add(b101);
			bbPoints.Add(b111);
			return bbPoints;
		}
	}
}