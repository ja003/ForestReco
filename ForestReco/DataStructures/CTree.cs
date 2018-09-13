using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	/// <summary>
	/// Y = height
	/// </summary>
	public class CTree : CBoundingBoxObject
	{
		public CPeak peak;
		//public List<CTreePoint> points = new List<CTreePoint>();
		private List<CBranch> branches = new List<CBranch>();
		//private CBranch stem;

		public static int BRANCH_ANGLE_STEP = 45;
		private const float MAX_STEM_POINT_DISTANCE = 0.1f;

		public Vector3 possibleNewPoint;

		public int treeIndex;

		public List<Vector3> Points = new List<Vector3>();

		public CTree(Vector3 pPoint, int pTreeIndex) : base(pPoint)
		{
			peak = new CPeak(pPoint);
			//stem = new CBranch(this, 0, 0);

			//points.Add(peak);
			if (CTreeManager.DEBUG) Console.WriteLine("new tree " + pTreeIndex);

			treeIndex = pTreeIndex;
			for (int i = 0; i < 360; i += BRANCH_ANGLE_STEP)
			{
				branches.Add(new CBranch(this, i, i + BRANCH_ANGLE_STEP));
			}
			//add stem as the last branch
			branches.Add(new CBranch(this, 0, 0));

			AddPoint(pPoint);
		}

		public void MergeWith(CTree pSubTree)
		{
			if (CTreeManager.DEBUG) Console.WriteLine(this.ToString(false, false, true, false) + " MergeWith " +
				pSubTree.ToString(false, false, true, false));
			//todo: make effective
			if (pSubTree.Equals(this))
			{
				Console.WriteLine("Error. cant merge with itself.");
				return;
			}

			//todo: check if first point of pSubTree is lower than last point of this tree
			Points.AddRange(pSubTree.Points);
			//update extents
			OnAddPoint(pSubTree.minBB);
			OnAddPoint(pSubTree.maxBB);
		}

		public List<CTreePoint> GetAllPoints()
		{
			List<CTreePoint> allPoints = new List<CTreePoint>();
			allPoints.Add(peak);
			foreach (CBranch b in branches)
			{
				foreach (CTreePoint p in b.points)
				{
					allPoints.Add(p);
				}
			}
			return allPoints;
		}

		public void ForceAddPoint(Vector3 pPoint)
		{
			//points.Add(new CTreePoint(pPoint));
		}


		public bool TryAddPoint(Vector3 pPoint)
		{
			if (BelongsToTree(pPoint))
			{
				AddPoint(pPoint);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Assignes all points to branches/peak
		/// </summary>
		public void Process()
		{
			foreach (Vector3 point in Points)
			{
				if (peak.Includes(point))
				{
					peak.AddPoint(point);
				}
				else
				{
					GetBranchFor(point).AddPoint(point);
				}
			}
		}

		/*private void SetNewPeak(CTreePoint pPoint)
		{
			if (CTreeManager.DEBUG) Console.WriteLine("-- SetNewPeak " + pPoint);
			CPeak oldPeak = peak.Clone();
			//first set new peak then move old one to appropriate branch
			//but only if new peak is not merged old one
			bool isPointMergedWithPeak = peak.Includes(pPoint);
			AddPoint(pPoint, false); //this defines new peak

			if (!isPointMergedWithPeak)
			{
				peak = new CPeak(pPoint.Points[0]);
				for (int i = 1; i < pPoint.Points.Count; i++)
				{
					peak.AddPoint(pPoint.Points[i]);
				}
				//stem = new CBranch(this, 0, 0);
				GetBranchFor(oldPeak).AddPoint(oldPeak);
			}
		}*/

		private bool IsNewPeak(Vector3 pPoint)
		{
			if (peak.Includes(pPoint)) { return true; }
			if (pPoint.Y < peak.Y) { return false; }
			float angle = CUtils.AngleBetweenThreePoints(
				new List<Vector3> { pPoint - Vector3.UnitY, pPoint, peak.Center }, Vector3.UnitY);
			return Math.Abs(angle) < CTreeManager.MAX_BRANCH_ANGLE;
		}

		private void AddPoint(Vector3 pPoint)
		{
			Points.Add(pPoint);
			OnAddPoint(pPoint);

			//todo: rozdělit body na Points a na peak. teď jsou v peaku duplicitně a při mergi se nesmažou
			if (peak.Includes(pPoint))
			{
				peak.AddPoint(pPoint);
			}

			/*if (peak.Includes(pPoint))
			{
				peak.AddPoint(pPoint);
			}

			if (pAddToBranch) { GetBranchFor(pPoint).AddPoint(pPoint); }
			OnAddPoint(pPoint);*/
		}
		
		/// <summary>
		/// Adds point and updates tree extents.
		/// 'pAddToBranch' adds this point to its appropriate branch. Should be false
		/// for example when this point is peak
		/// </summary>
		//private void AddPoint(CTreePoint pPoint, bool pAddToBranch = true)
		//{
		//	if (peak.Includes(pPoint))
		//	{
		//		peak.AddPoint(pPoint);
		//		pAddToBranch = false;
		//	}

		//	if (pAddToBranch) { GetBranchFor(pPoint).AddPoint(pPoint); }
		//	OnAddPoint(pPoint.Center);
		//}

		public bool BelongsToTree(Vector3 pPoint, bool pDebug = true)
		{
			if (IsNewPeak(pPoint))
			{
				return true;
			}

			const float MAX_DIST_TO_TREE_BB = 0.1f;
			float dist2D = CUtils.Get2DDistance(peak.Center, pPoint);
			float distToBB = Get2DDistanceFromBBTo(pPoint);
			bool contains = Contains(pPoint);
			//it must be close to peak of some tree or to its BB
			if (dist2D > CTreeManager.MAX_TREE_EXTENT / 2 && distToBB > MAX_DIST_TO_TREE_BB)
			{
				if (CTreeManager.DEBUG && pDebug) Console.WriteLine("- dist too high " + dist2D + "|" + distToBB);
				return false;
			}

			Vector3 suitablePoint = peak.GetClosestPointTo(pPoint);
			float angle = CUtils.AngleBetweenThreePoints(new List<Vector3>
			{
				suitablePoint - Vector3.UnitY, suitablePoint, pPoint
			}, Vector3.UnitY);
			float maxBranchAngle = GetMaxBranchAngle(suitablePoint, pPoint);
			if (angle > maxBranchAngle)
			{
				if (CTreeManager.DEBUG && pDebug) Console.WriteLine("- angle too high " + angle + "°/" + maxBranchAngle + "°. dist = " +
					Vector3.Distance(suitablePoint, pPoint));
				return false;
			}

			return true;
		}

		public static float GetMaxBranchAngle(Vector3 pPeakPoint, Vector3 pNewPoint)
		{
			float distance = Vector3.Distance(pPeakPoint, pNewPoint);
			const float DIST_STEP = 0.15f;
			float maxAngle = 100 - 5 * distance / DIST_STEP;

			return Math.Max(CTreeManager.MAX_BRANCH_ANGLE, maxAngle);
		}

		private bool DoesntBelongToTree(Vector3 pPoint, Vector3 pFromPoint, float pDistance)
		{
			if (CTreeManager.DEBUG) Console.WriteLine("point " + pPoint + " is too far from " + pFromPoint +
				". dist = " + pDistance);
			return false;
		}
		
		private CBranch GetBranchFor(Vector3 pPoint)
		{
			if (peak.maxHeight.Y < pPoint.Y)
			{
				Console.WriteLine("Error. " + pPoint + " is higher than peak " + peak);
			}
			Vector2 peak2D = new Vector2(peak.X, peak.Z);
			Vector2 point2D = new Vector2(pPoint.X, pPoint.Z);
			Vector2 dir = point2D - peak2D;
			if (dir.Length() < MAX_STEM_POINT_DISTANCE)
			{
				if (CTreeManager.DEBUG) Console.WriteLine("- branch = stem");
				return branches[branches.Count - 1]; //stem
			}

			dir = Vector2.Normalize(dir);
			float angle = CUtils.GetAngle(Vector2.UnitX, dir);
			//if (CTreeManager.DEBUG) Console.WriteLine("angle " + peak2D + " - " + point2D + " = " + angle);
			if (angle < 0)
			{
				angle = 360 + angle;
			}
			return branches[(int)(angle / BRANCH_ANGLE_STEP)];
		}

		/*public int GetPointCount()
		{
			int count = 0;
			count += peak.Points.Count;
			count += GetBranchesPointCount();
			return count;
		}*/

		private int GetBranchesCount()
		{
			int count = 0;
			foreach (CBranch b in branches)
			{
				if (b.points.Count > 0) { count++; }
			}
			return count;
		}

		private int GetBranchesPointCount()
		{
			int count = 0;
			foreach (CBranch b in branches)
			{
				count += b.GetPointCount();
			}
			return count;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			CTree t = (CTree)obj;
			return treeIndex == t.treeIndex;
		}


		public Obj GetObj(string pName, bool pExportBranches, bool pExportBB)
		{
			//if (CTreeManager.DEBUG) Console.WriteLine("GetObj " + pName);

			Obj obj = new Obj(pName);

			List<CTreePoint> allTreePoints = GetAllPoints();

			//display all peak points
			foreach (Vector3 peakPoint in peak.Points)
			{
				//allTreePoints.Add(new CTreePoint(peakPoint));
			}
			//display highest peak point
			allTreePoints.Add(new CTreePoint(peak.maxHeight));

			//add centers of all tree points
			List<Vector3> vectorPoints = new List<Vector3>();
			foreach (CTreePoint p in allTreePoints)
			{
				vectorPoints.Add(p.Center);
			}
			CObjExporter.AddPointsToObj(ref obj, vectorPoints);

			if (pExportBB)
			{
				CObjExporter.AddBBToObj(ref obj, allTreePoints);
			}

			if (pExportBranches)
			{
				foreach (CBranch b in branches)
				{
					CObjExporter.AddBranchToObj(ref obj, b);
				}
			}

			return obj;
		}


		public override string ToString()
		{
			return ToString(true, true, true, true);
		}

		public string ToString(bool pIndex, bool pPoints, bool pPeak, bool pBranches)
		{
			string indexS = pIndex ? treeIndex.ToString("000") : "";
			string pointsS = pPoints ? (" [" + GetAllPoints().Count.ToString("000") + "]") : "";
			string peakS = pPeak ? "||peak = " + peak : "";
			string branchesS = pBranches ? "||BR=" + GetBranchesCount() +
				"[" + GetBranchesPointCount().ToString("000") + "]" + "_|" : "";
			if (pBranches)
			{
				foreach (CBranch b in branches)
				{
					if (branches.IndexOf(b) == branches.Count - 1) { branchesS += ". Stem = "; }
					branchesS += b;
				}
			}
			return indexS + pointsS + peakS + branchesS;
		}
	}
}