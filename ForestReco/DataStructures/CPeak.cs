using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CPeak : CTreePoint
	{

		public CPeak(Vector3 pPoint) : base(pPoint){ }
		

		public override bool Includes(Vector3 pPoint)
		{
			return base.Includes(pPoint) || IsPartOfPeak(pPoint);
		}

		private bool IsPartOfPeak(Vector3 pPointCenter)
		{
			float distance2D = CUtils.Get2DDistance(Center, pPointCenter);
			float yDiff = Math.Abs(Math.Abs(Center.Y) - Math.Abs(pPointCenter.Y));
			return distance2D < CTreeManager.MIN_PEAKS_DISTANCE && yDiff < 0.25f;
		}

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