using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CBranch
	{
		public List<Vector3> points = new List<Vector3>();
		private CTree tree;

		public CBranch(CTree pTree)
		{
			tree = pTree;
		}

		public void AddPoint(Vector3 pPoint)
		{
			Console.Write("AddPoint " + pPoint + " to " + this);
			for (int i = 0; i < points.Count; i++)
			{
				if (pPoint.Y > points[i].Y)
				{
					points.Insert(i, pPoint);
					Console.WriteLine(" at " + i);
					return;
				}
			}
			Console.WriteLine("--");

			points.Add(pPoint);
		}

		public override string ToString()
		{
			return points.Count > 0 ? points.Count + "|" : "";
		}
	}
}