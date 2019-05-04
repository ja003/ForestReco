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
		protected List<CBranch> branches = new List<CBranch>();
		public List<CBranch> Branches => branches;
		public CBranch stem { get; protected set; }
		public float treePointExtent = CTreeManager.TREE_POINT_EXTENT; //default if not set

		public static int BRANCH_ANGLE_STEP = 45;
		private const float MAX_STEM_POINT_DISTANCE = 0.1f;

		public Vector3 possibleNewPoint;

		public int treeIndex { get; protected set; }

		public List<Vector3> Points = new List<Vector3>();

		public Obj mostSuitableRefTreeObj;
		public string RefTreeTypeName;

		public CCheckTree assignedCheckTree;
		public string assignedMaterial;

		public bool isValid; //invalid by default - until Validate is called

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

			if (CTreeManager.DEBUG) { CDebug.WriteLine("new tree " + pTreeIndex); }

			treeIndex = pTreeIndex;
			for (int i = 0; i < 360; i += BRANCH_ANGLE_STEP)
			{
				branches.Add(new CBranch(this, i, i + BRANCH_ANGLE_STEP));
			}

			stem = new CBranch(this, 0, 0);

			AddPoint(pPoint);
		}

		public void AssignMaterial()
		{
			assignedMaterial = CMaterialManager.GetTreeMaterial(this);
			//CDebug.WriteLine($"{this} color = {assignedMaterial}");
		}

		//MOST IMPORTANT

		/// <summary>
		/// Used only in reftree
		/// </summary>
		public void Process()
		{
			OnProcess();
		}

		protected virtual void OnProcess()
		{
			//CDebug.WriteLine("OnProcess");
		}
		
		/// <summary>
		/// pMerging is used during merging process
		/// </summary>
		public float GetAddPointFactor(Vector3 pPoint, CTree pTreeToMerge = null)
		{
			if (IsNewPeak(pPoint)) { return 1; }
			bool merging = pTreeToMerge != null;

			CBranch branchForPoint = GetBranchFor(pPoint);

			float distToPeak = CUtils.Get2DDistance(pPoint, peak.Center);
			if (!merging)
			{
				if (CParameterSetter.treeExtent < distToPeak)
				{
					return 0;
				}
			}

			else if (pTreeToMerge.isValid)
			{
				float peakPointDist = CUtils.Get2DDistance(pPoint, peak.Center);
				if (peakPointDist > GetTreeExtentFor(pPoint, CParameterSetter.treeExtentMultiply))
				{
					return 0;
				}
			}

			float bestFactor = 0;

			float branchFactor = branchForPoint.GetAddPointFactor(pPoint, true, pTreeToMerge);
			if (branchFactor > bestFactor) { bestFactor = branchFactor; }
			if (bestFactor > 0.9f) { return bestFactor; }

			branchFactor = branchForPoint.GetNeigbourBranch(1).GetAddPointFactor(pPoint, false, pTreeToMerge);
			if (branchFactor > bestFactor) { bestFactor = branchFactor; }
			if (bestFactor > 0.9f) { return bestFactor; }

			branchFactor = branchForPoint.GetNeigbourBranch(-1).GetAddPointFactor(pPoint, false, pTreeToMerge);
			if (branchFactor > bestFactor) { bestFactor = branchFactor; }
			if (bestFactor > 0.9f) { return bestFactor; }

			return bestFactor;
		}

		public void AddPoint(Vector3 pPoint)
		{
			if (peak.Includes(pPoint) || pPoint.Y > peak.minBB.Y)
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
			int debugIndex = -1;
			if (CTreeManager.DEBUG || Equals(debugIndex) || pSubTree.Equals(debugIndex))
			{
				CDebug.WriteLine(this.ToString(EDebug.Peak) + " MergeWith " + pSubTree.ToString(EDebug.Peak));
			}

			if (pSubTree.Equals(this))
			{
				CDebug.Error("cant merge with itself.");
				return;
			}

			foreach (Vector3 point in pSubTree.Points)
			{
				AddPoint(point);
			}
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
			float distToPeak = CUtils.Get2DDistance(pPoint, peak.Center);

			float distanceToAnyPoint = int.MaxValue;
			foreach (Vector3 p in Points)
			{
				float dist = Vector3.Distance(p, pPoint);
				if (dist < distanceToAnyPoint)
				{
					distanceToAnyPoint = dist;
				}
			}

			return Math.Min(distToPeak, distanceToAnyPoint);
		}

		public virtual float GetTreeHeight()
		{
			float treeHeight = maxBB.Y - GetGroundHeight();
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
				CDebug.Error(pPoint + " is higher than peak " + peak);
			}
			Vector2 peak2D = new Vector2(peak.X, peak.Z);
			Vector2 point2D = new Vector2(pPoint.X, pPoint.Z);
			Vector2 dir = point2D - peak2D;
			if (dir.Length() < MAX_STEM_POINT_DISTANCE)
			{
				if (CTreeManager.DEBUG) CDebug.WriteLine("- branch = stem");
				//return branches[branches.Count - 1]; //stem
				return stem; //stem
			}

			dir = Vector2.Normalize(dir);
			float angle = CUtils.GetAngle(Vector2.UnitX, dir);
			//if (CTreeManager.DEBUG) CDebug.WriteLine("angle " + peak2D + " - " + point2D + " = " + angle);
			if (angle < 0)
			{
				angle = 360 + angle;
			}
			int branchIndex = (int)(angle / BRANCH_ANGLE_STEP);
			//CDebug.WriteLine(pPoint + " goes to branch " + branchIndex + ". angle = " + angle);
			return branches[branchIndex];
		}

		private float? groundHeight;
		public CGroundField groundField;

		/// <summary>
		/// Returns height of ground under peak of this tree
		/// </summary>
		public float GetGroundHeight()
		{
			if (groundHeight != null) { return (float)this.groundHeight; }
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
		}

		public Obj GetObj(bool pExportBranches, bool pExportPoints, bool pExportSimple)
		{
			//if (CTreeManager.DEBUG) CDebug.WriteLine("GetObj " + pName);

			string prefix = isValid ? "tree_" : "invalidTree_";

			Obj obj = new Obj(prefix + treeIndex);

			obj.UseMtl = assignedMaterial;

			List<CTreePoint> allTreePoints = GetAllPoints();

			//display all peak points
			foreach (Vector3 peakPoint in peak.Points)
			{
				allTreePoints.Add(new CTreePoint(peakPoint, treePointExtent));
			}

			//display highest peak point
			allTreePoints.Add(new CTreePoint(peak.maxHeight, treePointExtent));

			if (pExportPoints)
			{
				CObjExporter.AddTreePointsBBToObj(ref obj, allTreePoints);
			}

			if (pExportBranches)
			{
				//add centers of all tree points
				List<Vector3> vectorPoints = new List<Vector3>();
				foreach (CTreePoint p in allTreePoints)
				{
					vectorPoints.Add(p.Center);
				}
				CObjExporter.AddPointsToObj(ref obj, vectorPoints);

				foreach (CBranch b in branches)
				{
					CObjExporter.AddBranchToObj(ref obj, b);
				}
			}
			if (pExportSimple)
			{
				Vector3 point1 = b000;
				Vector3 point2 = b100;
				Vector3 point3 = b101;
				Vector3 point4 = b001;

				float? goundHeight = groundField.GetHeight();
				if (groundHeight != null)
				{
					point1.Y = (float)goundHeight;
					point2.Y = (float)goundHeight;
					point3.Y = (float)goundHeight;
					point4.Y = (float)goundHeight;
				}

				CObjExporter.AddLFaceToObj(ref obj, point1, point2, peak.Center);
				CObjExporter.AddLFaceToObj(ref obj, point1, point4, peak.Center);
				CObjExporter.AddLFaceToObj(ref obj, point2, point3, peak.Center);
				CObjExporter.AddLFaceToObj(ref obj, point4, point3, peak.Center);
			}

			return obj;
		}

		//BOOLS

		public bool Validate(bool pRestrictive, bool pFinal = false)
		{
			isValid = ValidateBranches(pRestrictive);

			if (pFinal && !isValid && !IsAtBorder())
			{
				isValid = ValidatePoints();
			}

			if (Equals(debugTree))
			{
				CDebug.WriteLine(isValid + " Validate " + this);
			}

			return isValid;
		}

		private bool ValidateFirstBranchPoints()
		{
			//too many branch points are too far from peak
			const float maxDistOfFirstBranchPoint = 1.5f;
			int tooFarPointsCount = 0;
			foreach (CBranch branch in branches)
			{
				if (branch.TreePoints.Count == 0) { continue; }
				float dist = peak.Y - branch.TreePoints[0].Y;
				if (dist > maxDistOfFirstBranchPoint)
				{
					tooFarPointsCount++;
				}
			}
			return tooFarPointsCount < 6;
		}

		private bool ValidatePoints()
		{
			int totalPointCount = GetBranchesPointCount();
			float definedHeight = Extent.Y;

			//in case only few points are defined at bottom. in this case the mniddle part is almost not defined (its ok)
			//and validation is affected
			definedHeight = Math.Min(GetTreeHeight() / 2, definedHeight);

			if (definedHeight < 1) { return false; }

			int requiredPointsPerMeter = 3;
			int requiredPointCount = (int)definedHeight * requiredPointsPerMeter;
			return totalPointCount > requiredPointCount;
		}
		
		/// <summary>
		/// Checks if all branches have good scale ration with its opposite branch
		/// </summary>
		private bool ValidateScale()
		{
			foreach (CBranch b in branches)
			{
				if (b.GetPointCount() == 0) { return false; }
			}

			for (int i = 0; i < Branches.Count / 2; i++)
			{
				float br1Scale = Branches[i].furthestPointDistance;
				CBranch oppositeBranch = Branches[i + Branches.Count / 2];
				float br2Scale = oppositeBranch.furthestPointDistance;
				float branchIScaleRatio = br1Scale / br2Scale;

				if (br1Scale > 0.5f && br2Scale > 0.5f)
				{
					continue;
				}

				//ideal scale ratio = 1
				//if X > Y => scaleRatio > 1
				float idealScaleOffset = Math.Abs(1 - branchIScaleRatio);
				if (Math.Abs(idealScaleOffset) > 0.5f)
				{
					//isValidScale = false;
					return false;
				}
			}
			return true;
		}

		private int debugTree = -1;

		/// <summary>
		/// Determines whether the tree is defined enough.
		/// pAllBranchDefined = if one of branches is not defined => tree is not valid.
		/// All trees touching the boundaries should be eliminated by this
		/// </summary>
		private bool ValidateBranches(bool pAllBranchesDefined)
		{
			if (!ValidateFirstBranchPoints()) { return false; }

			float branchDefinedFactor = 0;
			int undefinedBranchesCount = 0;
			int wellDefinedBranchesCount = 0;

			foreach (CBranch b in branches)
			{
				float branchFactor = b.GetDefinedFactor();
				//if (Equals(debugTree))
				//{
				//	CDebug.WriteLine(b + "-- branchFactor " + branchFactor);
				//}

				if (Math.Abs(branchFactor) < 0.1f)
				{
					undefinedBranchesCount++;
					continue;
				}
				if (branchFactor > 0.5f)
				{
					wellDefinedBranchesCount++;
				}
				branchDefinedFactor += branchFactor;
			}
			if (pAllBranchesDefined)
			{
				if (undefinedBranchesCount > 1) { return false; }
			}

			if (undefinedBranchesCount > 2) { return false; }
			if (wellDefinedBranchesCount > 2) { return true; }

			float validFactor = branchDefinedFactor / (branches.Count - undefinedBranchesCount);
			//CDebug.WriteLine("VALID " + treeIndex + " height = " + height + " validFactor = " + validFactor);
			return validFactor > 0.5f;
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
			//it must be close to peak of some tree or to its BB

			float maxTreeExtent = GetTreeExtentFor(pPoint, 1);

			if (dist2D > maxTreeExtent && distToBB > MAX_DIST_TO_TREE_BB)
			{
				if (CTreeManager.DEBUG && pDebug) CDebug.WriteLine("- dist too high " + dist2D + "|" + distToBB);
				return false;
			}

			Vector3 suitablePoint = peak.GetClosestPointTo(pPoint);
			float angle = CUtils.AngleBetweenThreePoints(suitablePoint - Vector3.UnitY, suitablePoint, pPoint);
			float maxBranchAngle = GetMaxBranchAngle(suitablePoint, pPoint);
			if (angle > maxBranchAngle)
			{
				if (CTreeManager.DEBUG && pDebug) CDebug.WriteLine("- angle too high " + angle + "°/" + maxBranchAngle + "°. dist = " +
					Vector3.Distance(suitablePoint, pPoint));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates max acceptable 2D distance from peak for the new point to be added
		/// </summary>
		public float GetTreeExtentFor(Vector3 pNewPoint, float pMaxExtentMultiplier)
		{
			float treeHeight = GetTreeHeight();
			float ratio = treeHeight / CTreeManager.AVERAGE_TREE_HEIGHT;
			float maxExtent = Math.Max(CParameterSetter.treeExtent, ratio * CParameterSetter.treeExtent);
			maxExtent *= pMaxExtentMultiplier;
			float yDiff = peak.Center.Y - pNewPoint.Y;
			const float Y_DIFF_STEP = 0.1f;
			const float EXTENT_VALUE_STEP = 0.12f;

			float extent = CTreeManager.MIN_TREE_EXTENT + EXTENT_VALUE_STEP * yDiff / Y_DIFF_STEP;

			return Math.Min(extent, maxExtent);
		}

		//DEBUG TRANSFORMATIONS - NOT CORRECT ON ALL TREES

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
				CDebug.Error("Cant Rotate after process!");
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
				CDebug.Error("Cant Scale after process!");
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
				CDebug.Error("Cant move after process!");
			}

			peak.Points[0] += pOffset;
			peak.ResetBounds(peak.Points[0]);
		}

		public override string ToString()
		{
			return ToString(EDebug.Index);
		}

		public string ToString(EDebug pDebug)
		{
			switch (pDebug)
			{
				case EDebug.Height: return ToString(true, false, false, false, false, false, true);
				case EDebug.Peak: return ToString(true, false, true, false, false, false, false);
				case EDebug.Index: return ToString(true, false, false, false, false, false, false);

			}
			return ToString();
		}

		public enum EDebug
		{
			Height,
			Peak,
			Index
		}

		public string ToString(bool pIndex, bool pPoints, bool pPeak, bool pBranches, bool pReftree, bool pValid, bool pHeight)
		{
			string indexS = pIndex ? treeIndex.ToString("000") + "-" + groundField.ToStringIndex() : "";
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

		public void CheckTree()
		{
			//CDebug.WriteLine("Check tree " + ToString());
			foreach (CBranch b in branches)
			{
				b.CheckBranch();
			}
		}

		public bool Equals(int pIndex)
		{
			return treeIndex == pIndex;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			CTree t = (CTree)obj;
			return treeIndex == t.treeIndex;
		}

		public bool IsAtBorder()
		{
			float distanceToBorder = CProjectData.array.GetDistanceToBorderFrom(this.peak.Center);
			float borderDistExtentDiff = distanceToBorder - Math.Min(Extent.X, Extent.Z);

			return borderDistExtentDiff < 0;
		}
	}
}