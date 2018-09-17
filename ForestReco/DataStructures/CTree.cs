﻿using System;
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
		protected List<CBranch> branches = new List<CBranch>();
		public List<CBranch> Branches => branches;
		public CBranch stem { get; private set; }

		public static int BRANCH_ANGLE_STEP = 45;
		private const float MAX_STEM_POINT_DISTANCE = 0.1f;

		public Vector3 possibleNewPoint;

		public int treeIndex;

		public List<Vector3> Points = new List<Vector3>();

		public CTree(){ }

		public CTree(Vector3 pPoint, int pTreeIndex) : base(pPoint)
		{
			Init(pPoint, pTreeIndex);
		}

		protected void Init(Vector3 pPoint, int pTreeIndex)
		{
			peak = new CPeak(pPoint);

			if (CTreeManager.DEBUG) {Console.WriteLine("new tree " + pTreeIndex);}

			treeIndex = pTreeIndex;
			for (int i = 0; i < 360; i += BRANCH_ANGLE_STEP)
			{
				branches.Add(new CBranch(this, i, i + BRANCH_ANGLE_STEP));
			}

			stem = new CBranch(this, 0, 0);

			AddPoint(pPoint);
		}

		/// <summary>
		/// Returns a branch with biggest number of tree points
		/// </summary>
		/// <returns></returns>
		public CBranch GetMostDefinedBranch()
		{
			CBranch mostDefinedBranch = branches[0];
			foreach (CBranch b in branches)
			{
				if (b.TreePoints.Count > mostDefinedBranch.TreePoints.Count)
				{
					mostDefinedBranch = b;
				}
			}
			return mostDefinedBranch;
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

			Points.AddRange(pSubTree.Points);
			//sort in descending order
			Points.Sort((b, a) => a.Y.CompareTo(b.Y));
			//update extents
			OnAddPoint(pSubTree.minBB);
			OnAddPoint(pSubTree.maxBB);
		}

		protected Vector3 GetOffsetTo(CTree pOtherTree)
		{
			return pOtherTree.peak.Center - peak.Center;
		}

		public List<CTreePoint> GetAllPoints()
		{
			List<CTreePoint> allPoints = new List<CTreePoint>();
			allPoints.Add(peak);
			foreach (CBranch b in branches)
			{
				foreach (CTreePoint p in b.TreePoints)
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

		public bool TryAddPoint(Vector3 pPoint, bool pForce)
		{
			if (pForce || BelongsToTree(pPoint))
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
			//todo: rozdělit body na Points a na peak. teď jsou v peaku duplicitně a při mergi se nesmažou
			if (peak.Includes(pPoint))
			{
				peak.AddPoint(pPoint);
			}
			Points.Add(pPoint);
			OnAddPoint(pPoint);
		}

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
				//return branches[branches.Count - 1]; //stem
				return stem; //stem
			}

			dir = Vector2.Normalize(dir);
			float angle = CUtils.GetAngle(Vector2.UnitX, dir);
			//if (CTreeManager.DEBUG) Console.WriteLine("angle " + peak2D + " - " + point2D + " = " + angle);
			if (angle < 0)
			{
				angle = 360 + angle;
			}
			int branchIndex = (int)(angle / BRANCH_ANGLE_STEP);
			//Console.WriteLine(pPoint + " goes to branch " + branchIndex + ". angle = " + angle);
			return branches[branchIndex];
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
				if (b.TreePoints.Count > 0) { count++; }
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
		
		public void Rotate(int pYangle)
		{
			for (int i = 0; i < Points.Count; i++)
			{
				float angleRadians = CUtils.ToRadians(pYangle);
				Vector3 point = Points[i];
				Vector3 rotatedPoint = Vector3.Transform(point, Matrix4x4.CreateRotationY(angleRadians, peak.Center));
				Points[i] = rotatedPoint;
			}
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