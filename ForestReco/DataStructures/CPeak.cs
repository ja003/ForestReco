using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CPeak : CTreePoint
	{

		public CPeak(Vector3 pPoint) : base(pPoint){ }

		/// <summary>
		/// TODO: merge with CTreePoint method
		/// </summary>
		public new static CPeak Deserialize(string pLine)
		{
			string[] split = pLine.Split(null);

			Vector3 _minBB = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));

			Vector3 _maxBB = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));

			CPeak peak = new CPeak((_minBB + _maxBB) / 2);
			peak.OnAddPoint(_minBB);
			peak.OnAddPoint(_maxBB);
			return peak;
		}

		public override bool Includes(Vector3 pPoint, float pToleranceMultiply = 1)
		{
			return base.Includes(pPoint, pToleranceMultiply) || IsPartOfPeak(pPoint);
		}

		private bool IsPartOfPeak(Vector3 pPointCenter)
		{
			float distance2D = CUtils.Get2DDistance(Center, pPointCenter);
			float yDiff = Math.Abs(Center.Y - pPointCenter.Y);
			//return distance2D < CTreeManager.GetMinPeakDistance(1) && yDiff < 0.25f;
			return distance2D < MAX_PEAK_EXTENT && yDiff < 0.25f;
		}

		private const float MAX_PEAK_EXTENT = 1;

		public new CPeak Clone()
		{
			CPeak clonePeak = new CPeak(Center);
			clonePeak.Points = Points;
			foreach (Vector3 p in Points)
			{
				OnAddPoint(p);
			}
			return clonePeak;
		}

	}
}