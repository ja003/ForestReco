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
			else { Sum = height; }
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
				case EHeight.Tree: return IsLocalMax ? 10 : 0;
			}
			return null;
		}

		private float? GetAverage()
		{
			if (!IsDefined()) { return null; }
			return Sum / points.Count;
		}

		/// <summary>
		/// All elements but those at edge should have assigned neigbours
		/// </summary>
		private bool HasAllNeighbours()
		{
			return Left != null && Right != null && Top != null && Bot != null;
		}

		public void CalculateLocalExtrem(bool pExtrem, int pKernelSize)
		{
			if (!HasAllNeighbours()) { return; }
			IsLocalMax = true;
			IsLocalMin = true;
			for (int x = -pKernelSize; x < pKernelSize; x++)
			{
				for (int y = -pKernelSize; y < pKernelSize; y++)
				{
					CPointElement otherEl = GetElementWithOffset(x, y);

					if (pExtrem)
					{
						if (otherEl != null && otherEl.Max > Max)
						{
							IsLocalMax = false;
							return;
						}
					}
					else
					{
						if (otherEl != null && otherEl.Min < Min)
						{
							IsLocalMin = false;
							return;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returnd element with given local position to this element
		/// </summary>
		private CPointElement GetElementWithOffset(int pIndexOffsetX, int pIndexOffsetY)
		{
			CPointElement el = this;
			for (int x = 0; x < Math.Abs(pIndexOffsetX); x++)
			{
				el = pIndexOffsetX > 0 ? el.Right : el.Left;
				if (el == null) { return null; }
			}
			for (int y = 0; y < Math.Abs(pIndexOffsetY); y++)
			{
				el = pIndexOffsetY > 0 ? el.Top : el.Bot;
				if (el == null) { return null; }
			}
			return el;
		}
	}
}