using System;
using System.Collections.Generic;
using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public class CBranch
	{
		private List<CTreePoint> treePoints = new List<CTreePoint>();
		public List<CTreePoint> TreePoints => treePoints;
		public CTree tree { get; }

		public int angleFrom { get; }
		private int angleTo;

		private const float UNDEFINED_SIMILARITY = -1;

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

			for (int i = 0; i < treePoints.Count; i++)
			{
				CTreePoint pointOnBranch = treePoints[i];
				if (pointOnBranch.Includes(pPoint))
				{
					pointOnBranch.AddPoint(pPoint);
					if (CTreeManager.DEBUG) Console.WriteLine("---- added at " + i);
					return;
				}
				if (pPoint.Y > pointOnBranch.Y)
				{
					CTreePoint newPointOnBranch = new CTreePoint(pPoint);
					treePoints.Insert(i, newPointOnBranch);
					if (CTreeManager.DEBUG) { Console.WriteLine("---- inserted at " + i); }
					return;
				}
			}
			CTreePoint newPoint = new CTreePoint(pPoint);
			treePoints.Add(newPoint);
			if (CTreeManager.DEBUG) { Console.WriteLine("---- new point"); }

		}

		/// <summary>
		/// If given tree point is included in one of points on this branch
		/// </summary>
		private bool Contains(CTreePoint pPoint, Vector3 pOffset, int pAngleOffset, float pScale)
		{
			foreach (CTreePoint p in treePoints)
			{
				Vector3 movedPoint = pPoint.Center + pOffset;

				//scale point in reference to the ground point
				Vector3 scaledPoint = Vector3.Transform(movedPoint,
					Matrix4x4.CreateScale(pScale, pScale, pScale, tree.GetGroundPosition()));

				//CreateRotationY rotates point counter-clockwise => -pAngleOffset
				float angleOffsetRadians = CUtils.ToRadians(-pAngleOffset);
				Vector3 rotatedPoint = Vector3.Transform(
					scaledPoint, Matrix4x4.CreateRotationY(angleOffsetRadians, tree.peak.Center));

				//todo: include complete rotation based on tree orientation
				if (p.Includes(rotatedPoint)) { return true; }
			}
			return false;
		}

		public int GetPointCount()
		{
			int count = 0;
			foreach (CTreePoint p in treePoints)
			{
				count += p.Points.Count;
			}
			return count;
		}

		/// <summary>
		/// Calculates similarity with other branch.
		/// Range = [0,1]. 1 = Best match.
		/// </summary>
		public float GetSimilarityWith(CBranch pOtherBranch, Vector3 pMoveOffset, float pScale)
		{
			if (pOtherBranch.TreePoints.Count == 0)
			{
				if (TreePoints.Count == 0) { return 1; }
				//todo: situation when other branch has no points.
				//this can mean that data in this part of tree are just missing therefore it should
				return UNDEFINED_SIMILARITY;
			}

			float similarity = 0;

			foreach (CTreePoint p in pOtherBranch.TreePoints)
			{
				if (Contains(p, pMoveOffset, angleFrom - pOtherBranch.angleFrom, pScale))
				{
					similarity += 1f / pOtherBranch.TreePoints.Count;
				}
			}
			return similarity;
		}

		public override string ToString()
		{
			//return "br_<" + angleFrom + "," + angleTo + "> " + points.Count + " [" + GetPointCount() + "] |";
			return "br_<" + angleTo / CTree.BRANCH_ANGLE_STEP + "> " +
				   //GetPointCount().ToString("000") + 
				   "[" + treePoints.Count.ToString("00") + "] |";
		}

	}
}