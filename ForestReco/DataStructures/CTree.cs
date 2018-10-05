using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Principal;
using ObjParser;
using ObjParser.Types;
#pragma warning disable 659

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
		public CBranch stem { get; protected set; }
		public float treePointExtent = CTreeManager.TREE_POINT_EXTENT; //default if not set

		public static int BRANCH_ANGLE_STEP = 45;
		private const float MAX_STEM_POINT_DISTANCE = 0.1f;

		public Vector3 possibleNewPoint;

		public int treeIndex;

		public List<Vector3> Points = new List<Vector3>();

		//public CRefTree mostSuitableRefTree;
		public Obj mostSuitableRefTreeObj;

		public bool isValid = true; //valid by default - until Validate is called

		//INIT

		public CTree() { }

		public CTree(Vector3 pPoint, int pTreeIndex, float pTreePointExtent) : base(pPoint)
		{
			Init(pPoint, pTreeIndex, pTreePointExtent);
		}

		protected void Init(Vector3 pPoint, int pTreeIndex, float pTreePointExtent)
		{
			treePointExtent = pTreePointExtent;
			peak = new CPeak(pPoint, treePointExtent);

			if (CTreeManager.DEBUG) { Console.WriteLine("new tree " + pTreeIndex); }

			treeIndex = pTreeIndex;
			for (int i = 0; i < 360; i += BRANCH_ANGLE_STEP)
			{
				branches.Add(new CBranch(this, i, i + BRANCH_ANGLE_STEP));
			}

			stem = new CBranch(this, 0, 0);

			AddPoint(pPoint);
		}

		//MOST IMPORTANT

		/// <summary>
		/// Assigns all points to branches/peak
		/// </summary>
		public void Process()
		{
			//TODO: Process not used anymore
			/*
			foreach (Vector3 point in Points)
			{
				if (peak.Includes(point))
				{
					peak.AddPoint(point); //this should already be done
				}
				else
				{
					GetBranchFor(point).AddPoint(point);
				}
			}
			*/
			OnProcess();
		}

		protected virtual void OnProcess()
		{
			//Console.WriteLine("OnProcess");
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

		public float GetAddPointFactor(Vector3 pPoint, bool pUseDistToPeakDiff)
		{
			if (IsNewPeak(pPoint)) { return 1; }
			CBranch branchForPoint = GetBranchFor(pPoint);
			float branchFactor = branchForPoint.GetAddPointFactor(pPoint, pUseDistToPeakDiff);
			return branchFactor;
		}

		public void AddPoint(Vector3 pPoint)
		{
			if (treeIndex == 36)
			{
				if (Math.Abs(CUtils.Get2DDistance(pPoint, peak.Center) - 1.8f) < 0.2f)
				{
					if (peak.Y - pPoint.Y > 4.5f)
					{
						Console.WriteLine("!");
					}
				}
			}

			if (peak.Includes(pPoint))
			{
				peak.AddPoint(pPoint);
			}
			else
			{
				GetBranchFor(pPoint).AddPoint(pPoint);
			}
			Points.Add(pPoint);
			OnAddPoint(pPoint);
		}

		public void MergeWith(CTree pSubTree)
		{
			if (CTreeManager.DEBUG)
			{
				Console.WriteLine(this.ToString(EDebug.Peak) + " MergeWith " + pSubTree.ToString(EDebug.Peak));
			}
			//todo: make effective
			if (pSubTree.Equals(this))
			{
				Console.WriteLine("Error. cant merge with itself.");
				return;
			}

			foreach (Vector3 point in pSubTree.Points)
			{
				AddPoint(point);
			}

			/*Points.AddRange(pSubTree.Points);

			//only points from subTree peak can be part of this tree peak. Try to add them
			List<Vector3> peakPoints = pSubTree.peak.Points;
			for (int i = peakPoints.Count - 1; i >= 0; i--)
			{
				if (peak.Includes(peakPoints[i]))
				{
					peak.AddPoint(peakPoints[i]);
					//peakPoints.RemoveAt(i);
				}
			}
			//Points.AddRange(peakPoints); //no need, all points are in pSubTree.Points

			//sort in descending order. //TODO: this step can be inefective
			Points.Sort((b, a) => a.Y.CompareTo(b.Y));
			//update extents
			OnAddPoint(pSubTree.minBB);
			OnAddPoint(pSubTree.maxBB);*/
		}

		//GETTERS

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

		public float GetDistanceTo(Vector3 pPoint)
		{
			//todo: pick closest point
			float distToPeak = CUtils.Get2DDistance(pPoint, peak.Center);
			float distToBranch = GetBranchFor(pPoint).GetDistanceTo(pPoint);

			float distanceToAnyPoint = int.MaxValue;
			foreach (Vector3 p in Points)
			{
				float dist = Vector3.Distance(p, pPoint);
				if (dist < distanceToAnyPoint)
				{
					distanceToAnyPoint = dist;
				}
				//else
				//{
				//	break;
				//}
			}

			return Math.Min(distToPeak, distanceToAnyPoint);
		}

		public virtual float GetTreeHeight()
		{
			float treeHeight = peak.Center.Y - GetGroundHeight();
			return treeHeight;
		}

		public static float GetMaxBranchAngle(Vector3 pPeakPoint, Vector3 pNewPoint)
		{
			//float distance = Vector3.Distance(pPeakPoint, pNewPoint);
			float heightDiff = pPeakPoint.Y - pNewPoint.Y;
			float distance = CUtils.Get2DDistance(pPeakPoint, pNewPoint);
			const float HEIGHT_DIFF_STEP = 0.2f;
			const float DIST_STEP = 0.15f;
			const float ANGLE_STEP = 2.5f;
			const int MAX_ANGLE = 100;
			if (heightDiff < 0.5f)
			{
				return MAX_ANGLE;
			}

			float maxAngle = MAX_ANGLE - ANGLE_STEP * heightDiff / HEIGHT_DIFF_STEP;
			maxAngle -= ANGLE_STEP * distance / DIST_STEP;

			return Math.Max(CTreeManager.MAX_BRANCH_ANGLE, maxAngle);
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

		private float? groundHeight;
		public CGroundField groundField;

		/// <summary>
		/// Returns height of ground under peak of this tree
		/// </summary>
		public virtual float GetGroundHeight()
		{
			if (groundHeight != null) { return (float)this.groundHeight;}
			groundHeight = CProjectData.array?.GetHeight(peak.Center);
			return groundHeight ?? peak.Center.Y;
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

		public Vector3 GetGroundPosition()
		{
			Vector3 gp = new Vector3(peak.X, GetGroundHeight(), peak.Z);
			return gp;
			//return peak.Center - GetTreeHeight() * Vector3.UnitY;
		}

		public Obj GetObj(bool pExportBranches, bool pExportBB)
		{
			//if (CTreeManager.DEBUG) Console.WriteLine("GetObj " + pName);

			string prefix = isValid ? "tree_" : "invalidTree_";

			Obj obj = new Obj(prefix + treeIndex);

			if (CProjectData.exportSimpleTreeModel)
			{
				CObjExporter.AddLineToObj(ref obj, peak.Center, GetGroundPosition());
				return obj;
			}

			List<CTreePoint> allTreePoints = GetAllPoints();

			//display all peak points
			foreach (Vector3 peakPoint in peak.Points)
			{
				allTreePoints.Add(new CTreePoint(peakPoint, treePointExtent));
			}

			//display highest peak point
			allTreePoints.Add(new CTreePoint(peak.maxHeight, treePointExtent));

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

		//BOOLS


		/// <summary>
		/// Determines whether the tree is defined enough.
		/// pAllBranchDefined = if one of branches is not defined => tree is not valid.
		/// All trees touching the boundaries should be eliminated by this
		/// </summary>
		public bool Validate(bool pAllBranchDefined)
		{
			//float height = GetTreeHeight();
			//if (treeIndex == 12)
			//{
			//	Console.WriteLine("!");
			//}
			float branchDefinedFactor = 0;
			foreach (CBranch b in branches)
			{
				float branchFactor = b.GetDefinedFactor();
				if (pAllBranchDefined && Math.Abs(branchFactor) < 0.1f)
				{
					isValid = false;
					return false;
				}
				branchDefinedFactor += branchFactor;
			}
			float validFactor = branchDefinedFactor / branches.Count;
			//Console.WriteLine("VALID " + treeIndex + " height = " + height + " validFactor = " + validFactor);
			isValid = validFactor > 0.5f;
			return isValid;
		}

		public override bool Contains(Vector3 pPoint)
		{
			return base.Contains(pPoint) && GetBranchFor(pPoint).IsPointInExtent(pPoint);
		}

		private bool IsNewPeak(Vector3 pPoint)
		{
			if (peak.Includes(pPoint)) { return true; }
			if (pPoint.Y < peak.Y) { return false; }
			float angle = CUtils.AngleBetweenThreePoints(pPoint - Vector3.UnitY, pPoint, peak.Center);
			return Math.Abs(angle) < CTreeManager.MAX_BRANCH_ANGLE;
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
			//bool contains = Contains(pPoint);
			//it must be close to peak of some tree or to its BB

			//float treeHeight = GetTreeHeight();
			float maxTreeExtent = GetTreeExtentFor(pPoint);

			if (dist2D > maxTreeExtent && distToBB > MAX_DIST_TO_TREE_BB)
			{
				if (CTreeManager.DEBUG && pDebug) Console.WriteLine("- dist too high " + dist2D + "|" + distToBB);
				return false;
			}

			Vector3 suitablePoint = peak.GetClosestPointTo(pPoint);
			float angle = CUtils.AngleBetweenThreePoints(suitablePoint - Vector3.UnitY, suitablePoint, pPoint);
			float maxBranchAngle = GetMaxBranchAngle(suitablePoint, pPoint);
			if (angle > maxBranchAngle)
			{
				if (CTreeManager.DEBUG && pDebug) Console.WriteLine("- angle too high " + angle + "°/" + maxBranchAngle + "°. dist = " +
					Vector3.Distance(suitablePoint, pPoint));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates max acceptable 2D distance from peak for the new point to be added
		/// </summary>
		public float GetTreeExtentFor(Vector3 pNewPoint)
		{
			float treeHeight = GetTreeHeight();
			float ratio = treeHeight / CTreeManager.DEFAULT_TREE_HEIGHT;
			float maxExtent = Math.Max(CTreeManager.DEFAULT_TREE_EXTENT, ratio * CTreeManager.DEFAULT_TREE_EXTENT);
			float yDiff = peak.Center.Y - pNewPoint.Y;
			const float MIN_TREE_EXTENT = .5f;
			const float Y_DIFF_STEP = 0.1f;
			const float EXTENT_VALUE_STEP = 0.12f;

			float extent = MIN_TREE_EXTENT + EXTENT_VALUE_STEP * yDiff / Y_DIFF_STEP;

			return Math.Min(extent, maxExtent);
		}

		//DEBUG TRANSLATIONS - NOT CORRECT ON ALL TREES

		public void Rotate(int pYangle)
		{
			for (int i = 0; i < Points.Count; i++)
			{
				float angleRadians = CUtils.ToRadians(pYangle);
				Vector3 point = Points[i];
				Vector3 rotatedPoint = Vector3.Transform(point, Matrix4x4.CreateRotationY(angleRadians, peak.Center));
				Points[i] = rotatedPoint;
			}
			ResetBounds(Points);
			if (peak.Points.Count > 1)
			{
				Console.WriteLine("Cant Rotate after process!");
			}
		}

		public void Scale(int pScale)
		{
			//scale with center point = ground point
			//if center point = peak, resulting points might be set under ground level
			Vector3 groundPosition = GetGroundPosition();
			for (int i = 0; i < Points.Count; i++)
			{
				Vector3 point = Points[i];
				Vector3 scaledPoint = Vector3.Transform(point, Matrix4x4.CreateScale(
					pScale, pScale, pScale, groundPosition));
				Points[i] = scaledPoint;
			}
			ResetBounds(Points);
			if (peak.Points.Count > 1)
			{
				Console.WriteLine("Cant Scale after process!");
			}
			peak = new CPeak(Points[0], treePointExtent);
		}

		public void Move(Vector3 pOffset)
		{
			for (int i = 0; i < Points.Count; i++)
			{
				Vector3 point = Points[i];
				Vector3 movedPoint = point + pOffset;
				Points[i] = movedPoint;
			}
			ResetBounds(Points);
			if (peak.Points.Count > 1)
			{
				Console.WriteLine("Cant move after process!");
			}

			peak.Points[0] += pOffset;
			peak.ResetBounds(peak.Points[0]);
		}

		public override string ToString()
		{
			return ToString(true, true, true, false, true, true, true);
		}

		public string ToString(EDebug pDebug)
		{
			switch (pDebug)
			{
				case EDebug.Height: return ToString(true, false, false, false, false, false, true);
				case EDebug.Peak: return ToString(true, false, true, false, false, false, false);

			}
			return ToString();
		}

		public enum EDebug
		{
			Height,
			Peak
		}

		public string ToString(bool pIndex, bool pPoints, bool pPeak, bool pBranches, bool pReftree, bool pValid, bool pHeight)
		{
			string indexS = pIndex ? treeIndex.ToString("000") : "";
			string pointsS = pPoints ? (" [" + GetAllPoints().Count.ToString("000") + "]") : "";
			string validS = pValid ? (isValid ? "|<+>" : "<->") : "";
			string peakS = pPeak ? "||peak = " + peak : "";
			string branchesS = pBranches ? "||BR=" + GetBranchesCount() +
				"[" + GetBranchesPointCount().ToString("000") + "]" + "_|" : "";
			string refTreeS = pReftree && mostSuitableRefTreeObj != null ? "||reftree = " + mostSuitableRefTreeObj.Name : "";
			string heightS = pHeight ? "||height = " + GetTreeHeight() : "";

			if (pBranches)
			{
				foreach (CBranch b in branches)
				{
					if (branches.IndexOf(b) == branches.Count - 1) { branchesS += ". Stem = "; }
					branchesS += b;
				}
			}
			return indexS + pointsS + validS + peakS + branchesS + refTreeS + heightS;
		}

		//OTHERS

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			CTree t = (CTree)obj;
			return treeIndex == t.treeIndex;
		}

	}
}