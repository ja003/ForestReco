using System;
using System.Numerics;

namespace ForestReco
{
	public class CPeak : CTreePoint
	{

		public CPeak(Vector3 pPoint, float pTreePointExtent) : base(pPoint, pTreePointExtent) { }

		public new static CPeak Deserialize(string pLine, float pTreePointExtent)
		{
			string[] split = pLine.Split(null);

			Vector3 _minBB = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));

			Vector3 _maxBB = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));

			CPeak peak = new CPeak((_minBB + _maxBB) / 2, pTreePointExtent);
			peak.OnAddPoint(_minBB);
			peak.OnAddPoint(_maxBB);
			return peak;
		}

		public override bool Includes(Vector3 pPoint, float pToleranceMultiply = 1)
		{
			float yDiff = Math.Abs(Center.Y - pPoint.Y);
			if (yDiff > MAX_PEAK_Y_DIFF) { return false;} //just try if it makes processing faster
			return base.Includes(pPoint, pToleranceMultiply) || IsPartOfPeak(pPoint);
		}

		private const float MAX_PEAK_Y_DIFF = 0.25f;

		private bool IsPartOfPeak(Vector3 pPointCenter)
		{
			float distance2D = CUtils.Get2DDistance(Center, pPointCenter);
			float yDiff = Math.Abs(Center.Y - pPointCenter.Y);
			return distance2D < GetMaxPeakExtent() && yDiff < MAX_PEAK_Y_DIFF;
		}

		private float GetMaxPeakExtent()
		{
			float extent = CParameterSetter.treeExtent;
			return extent;
		}

		public new CPeak Clone()
		{
			CPeak clonePeak = new CPeak(Center, treePointExtent);
			clonePeak.Points = Points;
			foreach (Vector3 p in Points)
			{
				OnAddPoint(p);
			}
			return clonePeak;
		}
	}
}