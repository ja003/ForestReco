using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CTreePoint
	{
		public List<Vector3> Points = new List<Vector3>();

		public Vector3 Center;
		public float X => Center.X;
		public float Y => Center.Y;
		public float Z => Center.Z;

		private const float POINT_EXTENT = 0.1f;

		public CTreePoint(Vector3 pPoint)
		{
			Points.Add(pPoint);
			Center = pPoint;
		}

		public CTreePoint(CTreePoint pPoint)
		{
			Points = pPoint.Points;
			Center = pPoint.Center;
		}

		public void AddPoint(Vector3 pPoint)
		{
			Points.Add(pPoint);
			Center = (Center + pPoint) / 2;
		}

		public void AddPoint(CTreePoint pPoint)
		{
			if (CTreeManager.DEBUG)
				Console.WriteLine("- add tp " + pPoint + " to " + this);
			Points.AddRange(pPoint.Points);
			Center = (Center + pPoint.Center) / 2;
			if (CTreeManager.DEBUG)
				Console.WriteLine("- new center = " + Center);
		}

		public bool Includes(CTreePoint pPoint)
		{
			return Includes(pPoint.Center);
		}

		private bool Includes(Vector3 pPoint)
		{
			return Vector3.Distance(Center, pPoint) < POINT_EXTENT;
		}

		public override string ToString()
		{
			return Center + " [" + Points.Count+"]";
		}

		public CTreePoint Clone()
		{
			CTreePoint cloneTreePoint = new CTreePoint(Center);
			cloneTreePoint.Points = Points;
			return cloneTreePoint;
		}
	}
}