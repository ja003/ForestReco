using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CBranch
	{
		public List<CTreePoint> points = new List<CTreePoint>();
		private CTree tree;

		private int angleFrom;
		private int angleTo;

		public CBranch(CTree pTree, int pAngleFrom, int pAngleTo)
		{
			tree = pTree;
			angleFrom = pAngleFrom;
			angleTo = pAngleTo;
		}

		public void AddPoint(CTreePoint pPoint)
		{
			if (CTreeManager.DEBUG) 
				Console.Write("AddPoint " + pPoint.Center.ToString("#+0.00#;-0.00") + " to " + this);

			for (int i = 0; i < points.Count; i++)
			{
				CTreePoint pointOnBranch = points[i];
				if (pointOnBranch.Includes(pPoint))
				{
					pointOnBranch.AddPoint(pPoint);
				}
				else if (pPoint.Y > pointOnBranch.Y)
				{
					points.Insert(i, pPoint);
					if (CTreeManager.DEBUG) Console.WriteLine(" at " + i);
					return;
				}
			}
			if (CTreeManager.DEBUG) Console.WriteLine("--");

			points.Add(pPoint);
		}

		public override string ToString()
		{
			return "br_<" + angleFrom + "," + angleTo + "> " + points.Count + "|";
		}
	}
}