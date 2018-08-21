using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CBranch
	{
		List<Vector3> points = new List<Vector3>();
		private CTree tree;

		public CBranch(CTree pTree)
		{
			tree = pTree;
		}

		public void AddPoint(Vector3 pPoint)
		{
			for (int i = 0; i < points.Count; i++)
			{
				if (pPoint.Y > points[i].Y)
				{
					points.Insert(i, pPoint);
					return;
				}
			}
			points.Add(pPoint);
		}
	}
}