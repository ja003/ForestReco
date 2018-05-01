using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CPointElement
	{
		public CPointElement Left;
		public CPointElement Right;
		public CPointElement Top;
		public CPointElement Bot;

		private List<Vector3> points = new List<Vector3>();

		public float? Min;
		public float? Max;
		public float? Sum;

		public bool IsLocalMax;
		public bool IsLocalMin;
		public int VertexIndex = -1;

		public void AddPoint(Vector3 pPoint)
		{
			points.Add(pPoint);
			float height = pPoint.Z;
			if (Sum != null) { Sum += height; }
			else { Sum = height;}
			if (height > Max || Max == null) { Max = height; }
			if (height < Min || Min == null) { Min = height; }
		}

		public bool IsDefined()
		{
			return points.Count > 0;
		}

		public float? GetHeight(EHeight pHeight)
		{
			switch (pHeight)
			{
				case EHeight.Max: return Max;
				case EHeight.Average: return GetAverage();
			}
			return null;
		}

		private float? GetAverage()
		{
			if (!IsDefined()) { return null; }
			return Sum / points.Count;
		}
	}
}