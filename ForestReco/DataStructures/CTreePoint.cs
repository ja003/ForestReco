using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CTreePoint : CBoundingBoxObject
	{
		public List<Vector3> Points = new List<Vector3>();

		//public Vector3 Center;
		public float X => Center.X;
		public float Y => Center.Y;
		public float Z => Center.Z;
		
		//private Vector3 botLeft => minBB;
		//private Vector3 botLeft => minBB;
		
		public Vector3 maxHeight;

		private const float POINT_EXTENT = 0.1f;

		public CTreePoint(Vector3 pPoint) : base(pPoint)
		{
			AddPoint(pPoint);
		}

		/// <summary>
		/// TODO: is maxHeight neccessary?
		/// </summary>
		public string Serialize()
		{
			return CUtils.SerializeVector3(minBB) + " " + CUtils.SerializeVector3(maxBB);
		}

		public static CTreePoint Deserialize(string pLine)
		{
			string[] split = pLine.Split(null);

			Vector3 _minBB = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));

			Vector3 _maxBB = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));

			CTreePoint treePoint = new CTreePoint((_minBB + _maxBB)/2);
			treePoint.OnAddPoint(_minBB);
			treePoint.OnAddPoint(_maxBB);
			return treePoint;
		}

		public void AddPoint(Vector3 pPoint)
		{
			Points.Add(pPoint);
			OnAddPoint(pPoint);

			if (pPoint.Y > maxHeight.Y) { maxHeight = pPoint; }
		}

		/*public void AddPoint(CTreePoint pPoint)
		{
			if (CTreeManager.DEBUG)
				Console.WriteLine("---- add tp " + pPoint + " to " + this);

			if (pPoint.Points.Count > 1000)
			{
				Console.WriteLine("1000!");
			}

			foreach (Vector3 p in pPoint.Points)
			{
				AddPoint(p);
			}
			//Points.AddRange(pPoint.Points);
			//Center = (Center + pPoint.Center) / 2;
			if (CTreeManager.DEBUG)
				Console.WriteLine("---- new center = " + Center.ToString("#+0.00#;-0.00"));
		}*/


		public Vector3 GetClosestPointTo(Vector3 pPoint)
		{
			Vector3 closestPoint = Points[0];
			//todo: maybe better to return point on bounding box?
			foreach (Vector3 p in Points)
			{
				if (Vector3.Distance(p, pPoint) < Vector3.Distance(closestPoint, pPoint))
				{
					closestPoint = p;
				}
			}
			return closestPoint;
		}

		public virtual bool Includes(Vector3 pPoint)
		{
			return Vector3.Distance(Center, pPoint) < POINT_EXTENT || Contains(pPoint);
		}
		
		public override string ToString()
		{
			return Center.ToString("0.00") + " [" + Points.Count + "]";
		}

		public CTreePoint Clone()
		{
			CTreePoint cloneTreePoint = new CTreePoint(Center);
			cloneTreePoint.Points = Points;
			foreach (Vector3 p in Points)
			{
				OnAddPoint(p);
			}
			return cloneTreePoint;
		}
	}
}