using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CBranch
	{
		public List<Vector3> points = new List<Vector3>();
		private CTree tree;

		private int angleFrom;
		private int angleTo;

		public CBranch(CTree pTree, int pAngleFrom, int pAngleTo)
		{
			tree = pTree;
			angleFrom = pAngleFrom;
			angleTo = pAngleTo;
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
			return "br_<" + angleFrom + "," + angleTo + "> " + points.Count + "|";
		}
	}
}