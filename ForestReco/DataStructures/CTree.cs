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

		public int treeIndex { get; protected set; }
		//public bool isPeakInvalid;

		public List<Vector3> Points = new List<Vector3>();

		//public CRefTree mostSuitableRefTree;
		public Obj mostSuitableRefTreeObj;

		public CCheckTree assignedCheckTree;
		public string assignedMaterial;

		//public bool isValid = false; //invalid by default - until Validate is called
		public bool isValid = false;
		//public bool isValidScale = false; 
		//public bool isValidBranches = false; 

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
			//isValid = false;

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
			CDebug.WriteLine($"{this} color = {assignedMaterial}");
			//if (mostSuitableRefTreeObj != null)
			//{
			//	mostSuitableRefTreeObj.UseMtl = assignedMaterial;
			//	CDebug.WriteLine($"{mostSuitableRefTreeObj.Name} color = {assignedMaterial}");
			//}

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
			//CDebug.WriteLine("OnProcess");
		}

		//public bool CheckPeak(Vector3 pPoint)
		//{
		//	if(GetAddPointFactor)
		//	return true;
		//}

		/*public bool TryAddPoint(Vector3 pPoint, bool pForce)
		{
			if (pForce || BelongsToTree(pPoint))
			{
				AddPoint(pPoint);
				return true;
			}
			return false;
		}*/

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
				//if (GetTreeExtentFor(pPoint, 1) < CUtils.Get2DDistance(pPoint, peak.Center))
				if (CParameterSetter.treeExtent < distToPeak)
				{
					return 0;
				}
			}

			else if (pTreeToMerge != null && pTreeToMerge.isValid)
			{
				if (pTreeToMerge.Equals(140))
				{
					CDebug.WriteLine("");
				}

				float peakPointDist = CUtils.Get2DDistance(pPoint, peak.Center);
				if (peakPointDist > GetTreeExtentFor(pPoint, CParameterSetter.treeExtentMultiply))
				{
					return 0;
				}

				float furthestPointDistance = -int.MaxValue;

				CBranch rightNeighbour = branchForPoint.GetNeigbourBranch(1);
				CBranch leftNeighbour = branchForPoint.GetNeigbourBranch(-1);


				//use this criterium only if furthest point is close
				const float maxFurthestPointDistance = 1;
				if (Vector3.Distance(pPoint, branchForPoint.furthestPoint) < maxFurthestPointDistance)
				{
					furthestPointDistance = branchForPoint.furthestPointDistance;
				}
				if (Vector3.Distance(pPoint, rightNeighbour.furthestPoint) < maxFurthestPointDistance)
				{
					furthestPointDistance = Math.Max(furthestPointDistance, rightNeighbour.furthestPointDistance);
				}
				if (Vector3.Distance(pPoint, leftNeighbour.furthestPoint) < maxFurthestPointDistance)
				{
					furthestPointDistance = Math.Max(furthestPointDistance, leftNeighbour.furthestPointDistance);
				}

				//todo: was not very efficient, produced some buggy results
				//measure only if point is further from peak than the furthest point
				/*if (distToPeak - furthestPointDistance > 0.2f)
				{
					Vector3 closestHigherPoint = branchForPoint.GetClosestHigherTo(pPoint);
					Vector3 closestHigherPoinNeighbour1 = rightNeighbour.GetClosestHigherTo(pPoint);
					Vector3 closestHigherPoinNeighbour2 = leftNeighbour.GetClosestHigherTo(pPoint);

					//choose closest from neighbours
					float distToClosest = Vector3.Distance(pPoint, closestHigherPoint);
					distToClosest = Math.Min(distToClosest,
						Vector3.Distance(pPoint, closestHigherPoinNeighbour1));
					distToClosest = Math.Min(distToClosest,
						Vector3.Distance(pPoint, closestHigherPoinNeighbour2));

					float distToClosest2D = CUtils.Get2DDistance(pPoint, closestHigherPoint);
					distToClosest2D = Math.Min(distToClosest2D,
						CUtils.Get2DDistance(pPoint, closestHigherPoinNeighbour1));
					distToClosest2D = Math.Min(distToClosest2D,
						CUtils.Get2DDistance(pPoint, closestHigherPoinNeighbour2));

					if (distToClosest > 0.5f && distToClosest2D > 0.2f)
					{
						return 0;
					}
				}*/
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
			//if (peak.Includes(pPoint))
			if (peak.Includes(pPoint) || pPoint.Y > peak.minBB.Y) //todo: test if doesnt screw up
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
			//todo: make effective
			if (pSubTree.Equals(this))
			{
				CDebug.Error("cant merge with itself.");
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


		//public bool isFake;
		//const float MAX_POINTS_HEIGHT_DIFF = 1;

		/*public bool IsPeakValidWith(Vector3 pNewPoint)
		{
			if (peak.Includes(pNewPoint)) { return true; }
			if(Points.Count - peak.Points.Count > 5){ return true; }

			float newPointLowestPointHeightDiff = minBB.Y - pNewPoint.Y;
			return newPointLowestPointHeightDiff < MAX_POINTS_HEIGHT_DIFF;
			//CBranch branch = GetBranchFor(pNewPoint);
			//return branch.IsPeakValidWith(pNewPoint);
		}*/

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
			//float treeHeight = peak.Center.Y - GetGroundHeight();
			float treeHeight = maxBB.Y - GetGroundHeight();
			//float treeHeight = Extent.Y;
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
			//return peak.Center - GetTreeHeight() * Vector3.UnitY;
		}

		public Obj GetObj(bool pExportBranches, bool pExportBB)
		{
			//if (CTreeManager.DEBUG) CDebug.WriteLine("GetObj " + pName);

			string prefix = isValid ? "tree_" : "invalidTree_";
			//if (isFake) { prefix = "fake_"; }

			Obj obj = new Obj(prefix + treeIndex);

			//obj.UseMtl = CMaterialManager.GetTreeMaterial(this);
			obj.UseMtl = assignedMaterial;

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

		public bool Validate(bool pRestrictive, bool pFinal = false)
		{
			if (Equals(debugTree))
			{
				Console.WriteLine("");
			}
			isValid = ValidateBranches(pRestrictive);

			if (pFinal && !isValid && !IsAtBorder())
			{
				isValid = ValidatePoints();
			}



			if (Equals(debugTree))
			{
				CDebug.WriteLine(isValid + " Validate " + this);
			}

			//if (pRestrictive)
			//{ isValid = ValidateScale() && ValidateBranches(); }
			//else
			//{ isValid = ValidateScale() || ValidateBranches(); }

			return isValid;
		}

		private bool ValidatePoints()
		{
			if (Equals(debugTree))
			{
				Console.WriteLine("");
			}
			int totalPointCount = GetBranchesPointCount();
			//float definedHeight = GetTreeHeight() / 2;
			float definedHeight = Extent.Y;
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
			//if (isFake) { return false; }
			if (Equals(50))
			{
				CDebug.WriteLine("");
			}

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
			//isValidScale = true;
			return true;
		}

		private int debugTree = 54;

		/// <summary>
		/// Determines whether the tree is defined enough.
		/// pAllBranchDefined = if one of branches is not defined => tree is not valid.
		/// All trees touching the boundaries should be eliminated by this
		/// </summary>
		private bool ValidateBranches(bool pAllBranchesDefined)
		{
			if (Equals(debugTree) && pAllBranchesDefined)
			{
				Console.WriteLine("");
			}
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
					//isValidBranches = false;
					//if (pAllBranchesDefined)
					//{
					//	return false;
					//}
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
			//isValidBranches = validFactor > 0.5f;
			return validFactor > 0.5f;
			//return isValid;
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
			//const float MIN_TREE_EXTENT = .5f;
			const float Y_DIFF_STEP = 0.1f;
			const float EXTENT_VALUE_STEP = 0.12f;

			float extent = CTreeManager.MIN_TREE_EXTENT + EXTENT_VALUE_STEP * yDiff / Y_DIFF_STEP;

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

		//public bool IsTreeFake()
		//{
		//	if (GetTreeHeight() > CTreeManager.MIN_FAKE_TREE_HEIGHT)
		//	{
		//		if (GetAllPoints().Count < 5)
		//		{
		//			return true;
		//		}
		//	}
		//	return false;
		//}


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

			if (treeIndex == 236)
			{
				Console.WriteLine();
			}
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

			/*bool isCornerAtBorder = !CProjectData.array.GetElementContainingPoint(b000).HasAllNeighbours();
			if (isCornerAtBorder) { return true; }
			isCornerAtBorder = !CProjectData.array.GetElementContainingPoint(b111).HasAllNeighbours();
			if (isCornerAtBorder) { return true; }

			return false;*/
		}
	}
}