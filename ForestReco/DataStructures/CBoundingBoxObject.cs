using System.Numerics;

namespace ForestReco
{
	public class CBoundingBoxObject
	{

		//bounding box
		protected Vector3 minBB;
		protected Vector3 maxBB;

		public Vector3 Center => new Vector3((minBB.X + maxBB.X)/2, (minBB.Y + maxBB.Y) / 2, (minBB.Z + maxBB.Z) / 2);

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
	}
}