using System;
using System.Collections.Generic;
using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public class CBranch
	{
		public List<CTreePoint> points = new List<CTreePoint>();
		public CTree tree { get; }

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
			if (CTreeManager.DEBUG)
				Console.WriteLine("--- AddPoint " + pPoint.ToString("#+0.00#;-0.00") + " to " + this);

			for (int i = 0; i < points.Count; i++)
			{
				CTreePoint pointOnBranch = points[i];
				if (pointOnBranch.Includes(pPoint))
				{
					pointOnBranch.AddPoint(pPoint);
					if (CTreeManager.DEBUG) Console.WriteLine("---- added at " + i);
					return;
				}
				if (pPoint.Y > pointOnBranch.Y)
				{
					CTreePoint newPointOnBranch = new CTreePoint(pPoint);
					points.Insert(i, newPointOnBranch);
					if (CTreeManager.DEBUG) { Console.WriteLine("---- inserted at " + i); }
					return;
				}
			}
			CTreePoint newPoint = new CTreePoint(pPoint);
			points.Add(newPoint);
			if (CTreeManager.DEBUG) { Console.WriteLine("---- new point"); }

		}

		public int GetPointCount()
		{
			int count = 0;
			foreach (CTreePoint p in points)
			{
				count += p.Points.Count;
			}
			return count;
		}

		public override string ToString()
		{
			//return "br_<" + angleFrom + "," + angleTo + "> " + points.Count + " [" + GetPointCount() + "] |";
			return "br_<" + angleTo / CTree.BRANCH_ANGLE_STEP + "> " +
				points.Count.ToString("00") + " [" + GetPointCount().ToString("000") + "] |";
		}


	}
}