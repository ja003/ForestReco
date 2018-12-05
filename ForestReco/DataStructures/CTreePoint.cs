using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CTreePoint : CBoundingBoxObject
	{
		public List<Vector3> Points = new List<Vector3>();

		public float X => Center.X;
		public float Y => Center.Y;
		public float Z => Center.Z;
		
		public Vector3 maxHeight;
		public Vector3 minHeight;

		public float treePointExtent;

		public CTreePoint(Vector3 pPoint, float pTreePointExtent) : base(pPoint)
		{
			treePointExtent = pTreePointExtent;
			maxHeight = pPoint;
			minHeight = pPoint;
			AddPoint(pPoint);
		}


		public static CTreePoint Deserialize(string pLine, float pTreePointExtent)
		{
			string[] split = pLine.Split(null);

			Vector3 _minBB = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));

			Vector3 _maxBB = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));

			CTreePoint treePoint = new CTreePoint((_minBB + _maxBB)/2, pTreePointExtent);
			treePoint.OnAddPoint(_minBB);
			treePoint.OnAddPoint(_maxBB);
			return treePoint;
		}

		public void AddPoint(Vector3 pPoint)
		{
			Points.Add(pPoint);
			OnAddPoint(pPoint);

			if (pPoint.Y > maxHeight.Y) { maxHeight = pPoint; }
			if (pPoint.Y < minHeight.Y) { minHeight = pPoint; }
		}
		

		public Vector3 GetClosestPointTo(Vector3 pPoint)
		{
			Vector3 closestPoint = Points[0];
			foreach (Vector3 p in Points)
			{
				if (Vector3.Distance(p, pPoint) < Vector3.Distance(closestPoint, pPoint))
				{
					closestPoint = p;
				}
			}
			return closestPoint;
		}

		public virtual bool Includes(Vector3 pPoint, float pToleranceMultiply = 1)
		{
			return Vector3.Distance(Center, pPoint) < treePointExtent * pToleranceMultiply || Contains(pPoint);
		}
		
		public override string ToString()
		{
			return Center.ToString("0.00") + " [" + Points.Count + "]";
		}

		public CTreePoint Clone()
		{
			CTreePoint cloneTreePoint = new CTreePoint(Center, treePointExtent);
			cloneTreePoint.Points = Points;
			foreach (Vector3 p in Points)
			{
				OnAddPoint(p);
			}
			return cloneTreePoint;
		}
	}
}