using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace ForestReco
{
	public class CBranch : CBoundingBoxObject
	{
		public List<CTreePoint> TreePoints { get; } = new List<CTreePoint>();

		public Vector3 furthestPoint;
		public float furthestPointDistance => CUtils.Get2DDistance(furthestPoint, tree.peak);

		public CTree tree { get; }

		public int angleFrom { get; }
		private int angleTo;

		private const float UNDEFINED_SIMILARITY = -1;

		public CBranch(CTree pTree, int pAngleFrom, int pAngleTo)
		{
			tree = pTree;
			furthestPoint = tree.peak.Center;
			angleFrom = pAngleFrom;
			angleTo = pAngleTo;
		}

		public new List<string> Serialize()
		{
			List<string> lines = new List<string>();

			foreach (CTreePoint tp in TreePoints)
			{
				lines.Add(tp.Serialize());
			}

			return lines;
		}

		/// <summary>
		/// Use this only in deserialization
		/// </summary>
		/// <param name="pTreepointsOnBranch"></param>
		public void SetTreePoints(List<CTreePoint> pTreepointsOnBranch)
		{
			foreach (CTreePoint tp in pTreepointsOnBranch)
			{
				TreePoints.Add(tp);
				OnAddPoint(tp.minBB);
				OnAddPoint(tp.maxBB);
			}
		}

		/// <summary>
		/// Calculates factor, showing how much does given point fit to this branch.
		/// Range = 0-1. 1 = best fit.
		/// pMerging = uses criterium of pUseDistToPeakDiff (viz GetAddPointFactorInRefTo)
		/// </summary>
		public float GetAddPointFactor(Vector3 pPoint, bool pSameBranch, CTree pTreeToMerge = null)
		{
			float bestFactor = 0;
			bool merging = pTreeToMerge != null;
			
			Vector3 refPoint1 = furthestPoint;
			float refPoint1Factor = GetAddPointFactorInRefTo(pPoint, refPoint1, pSameBranch, merging, pTreeToMerge);
			bestFactor = refPoint1Factor;
			if (bestFactor > .99f) { return bestFactor; }

			if (merging)
			{
				Vector3 closestHigher = GetClosestHigherTo(pTreeToMerge.peak.Center);
				float distToHigher = Vector3.Distance(pPoint, closestHigher);
				const float maxDistToHigher = 0.5f;
				float distToHigherFactor = maxDistToHigher / distToHigher;
				if (distToHigher < maxDistToHigher) { return 1; }
				float closestHigherFactor = GetAddPointFactorInRefTo(pPoint, closestHigher, pSameBranch, merging, pTreeToMerge);
				closestHigherFactor = Math.Max(closestHigherFactor, distToHigherFactor);
				bestFactor = Math.Max(bestFactor, closestHigherFactor);

				if (bestFactor > .99f) { return bestFactor; }

				Vector3? closestLower = GetClosestLowerTo(pTreeToMerge.peak.Center);
				if (closestLower != null)
				{
					float closestLowerFactor = GetAddPointFactorInRefTo(pPoint, (Vector3)closestLower, pSameBranch, merging, pTreeToMerge);
					bestFactor = Math.Max(bestFactor, closestLowerFactor);
					if (bestFactor > .99f) { return bestFactor; }
				}
			}

			return bestFactor;
		}
		
		public CBranch GetNeigbourBranch(int pIndexIncrement)
		{
			int indexOfthis = tree.Branches.IndexOf(this);
			int neighbourBranchIndex = (indexOfthis + pIndexIncrement) % tree.Branches.Count;
			if (neighbourBranchIndex < 0) { neighbourBranchIndex += tree.Branches.Count; }
			return tree.Branches[neighbourBranchIndex];
		}

		private Vector3 GetClosestPointTo(Vector3 pPoint, int pMaxIterationCount = -1)
		{
			if (pMaxIterationCount == -1)
			{
				pMaxIterationCount = TreePoints.Count;
			}
			Vector3 closestPoint = tree.peak.Center;
			for (int i = TreePoints.Count - 1; i > TreePoints.Count - pMaxIterationCount; i--)
			{
				Vector3 treePoint = TreePoints[i].Center;
				if (Vector3.Distance(treePoint, pPoint) < Vector3.Distance(closestPoint, pPoint))
				{
					closestPoint = treePoint;
				}
			}
			return closestPoint;
		}

		public Vector3 GetClosestHigherTo(Vector3 pPoint)
		{
			Vector3 closestPoint = tree.peak.Center;
			float distToClosest = Vector3.Distance(closestPoint, pPoint);
			for (int i = 0; i < TreePoints.Count; i++)
			{
				Vector3 treePoint = TreePoints[i].Center;
				if (treePoint.Y < pPoint.Y) { break; }
				if (Vector3.Distance(treePoint, pPoint) < distToClosest)
				{
					closestPoint = treePoint;
					distToClosest = Vector3.Distance(closestPoint, pPoint);
				}
			}
			return closestPoint;
		}
		public Vector3? GetClosestLowerTo(Vector3 pPoint)
		{
			Vector3? closestPoint = null;
			for (int i = TreePoints.Count - 1; i >= 0; i--)
			{
				Vector3 treePoint = TreePoints[i].Center;
				if (treePoint.Y > pPoint.Y) { break; }
				if (closestPoint == null || Vector3.Distance(treePoint, pPoint) < Vector3.Distance((Vector3)closestPoint, pPoint))
				{
					closestPoint = treePoint;
				}
			}
			return closestPoint;
		}
		
		private Vector3 GetLastPoint()
		{
			return TreePoints.Count == 0 ? tree.peak.Center : TreePoints.Last().Center;
		}

		private float GetAddPointFactorInRefTo(Vector3 pPoint, Vector3 pReferencePoint,
			bool pSameBranch, bool pMerging, CTree pTreeToMerge = null)
		{
			//during merging it is expected, that added peak will be higher
			if (!pMerging && pPoint.Y > pReferencePoint.Y)
			{
				//points are added in descending order. if true => pPoint belongs to another tree
				return 0;
			}
			float pointDistToRef = Vector3.Distance(pPoint, pReferencePoint);
			if (pointDistToRef < 0.2)
			{
				return 1;
			}

			float refDistToPeak = CUtils.Get2DDistance(pReferencePoint, tree.peak);
			float pointDistToPeak = CUtils.Get2DDistance(pPoint, tree.peak);
			if (!pMerging && pSameBranch)
			{
				if (pointDistToPeak < refDistToPeak)
				{
					return 1;
				}
			}
			float distToPeakDiff = pointDistToPeak - refDistToPeak;
			if (!pMerging && distToPeakDiff < 0.3)
			{
				return 1;
			}
			if (!pMerging && distToPeakDiff > 0.5f && refDistToPeak > 0.5f)
			{
				float peakRefPointAngle = CUtils.AngleBetweenThreePoints(tree.peak.Center, pReferencePoint, pPoint);
				//new point is too far from furthest point and goes too much out of direction of peak->furthestPoint
				if (peakRefPointAngle < 180 - 45)
				{
					return 0;
				}
			}
			
			float refAngleToPoint =
				CUtils.AngleBetweenThreePoints(pReferencePoint - Vector3.UnitY, pReferencePoint, pPoint);

			Vector3 suitablePeakPoint = tree.peak.Center;

			float peakAngleToPoint =
				CUtils.AngleBetweenThreePoints(suitablePeakPoint - Vector3.UnitY, suitablePeakPoint, pPoint);
			float angle = Math.Min(refAngleToPoint, peakAngleToPoint);

			float maxBranchAngle = CTree.GetMaxBranchAngle(suitablePeakPoint, pPoint);
			float unacceptableAngle = maxBranchAngle;
			if (!pMerging && angle > unacceptableAngle) { return 0; }
			unacceptableAngle += 30;
			unacceptableAngle = Math.Min(unacceptableAngle, 100);

			float angleFactor = (unacceptableAngle - angle) / unacceptableAngle;

			float unacceptableDistance = tree.GetTreeExtentFor(pPoint,
				pMerging ? CParameterSetter.treeExtentMultiply : 1);
			unacceptableDistance += 0.5f;
			if (pointDistToPeak > unacceptableDistance) { return 0; }
			unacceptableDistance += 0.5f;
			float distFactor = (unacceptableDistance - pointDistToPeak) / unacceptableDistance;

			Vector3 closestPoint = GetClosestPointTo(pPoint);
			float distFromClosestPoint = Vector3.Distance(pPoint, closestPoint);
			float maxDistFromClosest = 0.5f;
			float distToClosestFactor = 2 * (maxDistFromClosest + 0.2f - distFromClosestPoint);
			distToClosestFactor = Math.Max(0, distToClosestFactor);

			float totalFactor;

			if (pMerging)
			{
				if (distFromClosestPoint < maxDistFromClosest && distToPeakDiff < maxDistFromClosest)
				{
					return 1;
				}
			}

			if (pTreeToMerge != null && pMerging && pTreeToMerge.isValid)
			{
				int factorCount = 3;
				if(distToClosestFactor < .1f){ factorCount -= 1; }
				totalFactor = (distToClosestFactor + angleFactor + distFactor) / factorCount;
			}
			else
			{
				//let dist factor have higher influence
				totalFactor = (angleFactor + 1.5f * distFactor) / 2.5f;
			}

			return totalFactor;
		}

		public void AddPoint(Vector3 pPoint)
		{
			if (CTreeManager.DEBUG)
				CDebug.WriteLine("--- AddPoint " + pPoint.ToString("#+0.00#;-0.00") + " to " + this);

			RefreshFurthestPoint(pPoint);
			OnAddPoint(pPoint);
			
			int insertAtIndex = 0;
			//find appropriate insert at index
			if (TreePoints.Count > 0)
			{
				for (int i = TreePoints.Count - 1; i >= -1; i--)
				{
					insertAtIndex = i + 1;
					if (insertAtIndex == 0)
					{
						break;
					}
					CTreePoint pointOnBranch = TreePoints[i];
					if (pointOnBranch.Includes(pPoint))
					{
						pointOnBranch.AddPoint(pPoint);
						//boundaries of points are changed, check if the order has to be changed

						if (i > 0)
						{
							CTreePoint previousPoint = TreePoints[i - 1];
							//if(previousPoint.Contains(pointOnBranch.Center))
							if (pointOnBranch.Y > previousPoint.Y)
							{
								TreePoints.RemoveAt(i);
								TreePoints.Insert(i - 1, pointOnBranch);
							}
						}
						if (i < TreePoints.Count - 1)
						{
							CTreePoint nextPoint = TreePoints[i + 1];
							if (pointOnBranch.Y < nextPoint.Y)
							{
								TreePoints.RemoveAt(i);
								TreePoints.Insert(i + 1, pointOnBranch);
							}
						}
						CheckAddedPoint();
						return;

					}
					if (pPoint.Y < pointOnBranch.Y)
					{
						if (i == TreePoints.Count - 1 || TreePoints[i + 1].Y <= pPoint.Y)
						{
							break;
						}
					}
				}
			}

			CTreePoint newPoint = new CTreePoint(pPoint, tree.treePointExtent);
			TreePoints.Insert(insertAtIndex, newPoint);

			CheckAddedPoint();

			if (CTreeManager.DEBUG) { CDebug.WriteLine("---- new point"); }

		}

		private void CheckAddedPoint()
		{
			if (TreePoints[0].minHeight.Y > tree.peak.Y)
			{
				//not error, can happen after merging when peak is expanded
				//CDebug.Error($"CheckAddedPoint. tree {tree.treeIndex} : first point {TreePoints[0]} is higher than peak {tree.peak}");
			}

			if (TreePoints.Count < 2) { return; }
			CTreePoint previousTp = TreePoints[TreePoints.Count - 2];
			CTreePoint tp = TreePoints[TreePoints.Count - 1];
			if (tp.minHeight.Y > previousTp.maxHeight.Y)
			{
				CDebug.Error("CheckAddedPoint. tree " + tree.treeIndex + ": " + tp + " is higher than " + previousTp);
			}
		}

		public void CheckBranch()
		{
			for (int i = 1; i < TreePoints.Count; i++)
			{
				CTreePoint previousTp = TreePoints[i - 1];
				CTreePoint tp = TreePoints[i];
				if (tp.minHeight.Y > previousTp.maxHeight.Y)
				{
					CDebug.Error("- CheckBranch. tree " + tree.treeIndex + ": " + tp + " is higher than " + previousTp);
				}
			}
		}

		private void RefreshFurthestPoint(Vector3 pPoint)
		{
			float pointDistToPeak = CUtils.Get2DDistance(pPoint, tree.peak);
			if (pointDistToPeak > furthestPointDistance)
			{
				furthestPoint = pPoint;
			}
		}


		/// <summary>
		/// If given tree point is included in one of points on this branch
		/// </summary>
		public bool Contains(Vector3 pPoint, float pToleranceMultiply)
		{
			float treePointExtent = tree.peak.treePointExtent * pToleranceMultiply;
			int approxIndex = GetAproxIndexOfPoint(pPoint, treePointExtent);

			//which direction on branch should we search
			int dir = 1; //actually to be sure we need to check both directions...final treepoints doesnt have to be neccessarily Y-ordered
						 //if (TreePoints[approxIndex].Y < pPoint.Y) { dir = -1; }
			CTreePoint pointOnBranch = TreePoints[approxIndex];
			bool isPointOnBranchWithinRange = Math.Abs(pointOnBranch.Y - pPoint.Y) < treePointExtent + 1;
			for (int i = approxIndex; isPointOnBranchWithinRange && i > 0 && i < TreePoints.Count; i += dir)
			{
				pointOnBranch = TreePoints[i];
				if (pointOnBranch.Includes(pPoint, pToleranceMultiply)) { return true; }
				isPointOnBranchWithinRange = Math.Abs(pointOnBranch.Y - pPoint.Y) < treePointExtent + 1;
			}

			dir = -1;
			pointOnBranch = TreePoints[approxIndex];
			isPointOnBranchWithinRange = Math.Abs(pointOnBranch.Y - pPoint.Y) < treePointExtent + 1;
			for (int i = approxIndex; isPointOnBranchWithinRange && i > 0 && i < TreePoints.Count; i += dir)
			{
				pointOnBranch = TreePoints[i];
				if (pointOnBranch.Includes(pPoint, pToleranceMultiply)) { return true; }
				isPointOnBranchWithinRange = Math.Abs(pointOnBranch.Y - pPoint.Y) < treePointExtent + 1;
			}

			return false;
		}

		/// <summary>
		/// Calculates approximate index on branch where the give point should be placed with given tolerance
		/// </summary>
		private int GetAproxIndexOfPoint(Vector3 pPoint, float pMaxDiff)
		{
			int fromIndex = 0;
			int toIndex = TreePoints.Count;
			int selectedIndex = (fromIndex + toIndex) / 2;
			CTreePoint selectedTreePoint = TreePoints[selectedIndex];
			int counter = 0;
			while (Math.Abs(selectedTreePoint.Y - pPoint.Y) > pMaxDiff && toIndex - fromIndex > 1)
			{
				if (selectedTreePoint.Y > pPoint.Y)
				{
					fromIndex += (toIndex - fromIndex) / 2;
				}
				else
				{
					toIndex -= (toIndex - fromIndex) / 2;
				}
				selectedIndex = (fromIndex + toIndex) / 2;
				selectedTreePoint = TreePoints[selectedIndex];
				counter++;
			}

			if (counter > 20) { CDebug.Warning("GetAproxIndexOfPoint " + pPoint + " = " + counter); }

			return selectedIndex;
		}

		public int GetPointCount()
		{
			int count = 0;
			foreach (CTreePoint p in TreePoints)
			{
				count += p.Points.Count;
			}
			return count;
		}

		/// <summary>
		/// Calculates similarity with other branch.
		/// Range = [0,1]. 1 = Best match.
		/// </summary>
		public float GetSimilarityWith(CBranch pOtherBranch, Vector3 offsetToThisTree, Matrix4x4 scaleMatrix)
		{
			if (pOtherBranch.TreePoints.Count == 0)
			{
				if (TreePoints.Count == 0) { return 1; }
				//situation when other branch has no points.
				//this can mean that data in this part of tree are just missing therefore it should
				return UNDEFINED_SIMILARITY;
			}

			float similarity = 0;

			//CreateRotationY rotates point counter-clockwise => -pAngleOffset
			//rotation has to be calculated in each branch
			float angleOffsetRadians = CUtils.ToRadians(-(angleFrom - pOtherBranch.angleFrom));
			Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationY(angleOffsetRadians, tree.peak.Center);

			foreach (CTreePoint p in pOtherBranch.TreePoints)
			{
				Vector3 movedPoint = p.Center + offsetToThisTree;
				Vector3 scaledPoint = Vector3.Transform(movedPoint, scaleMatrix);
				Vector3 rotatedPoint = Vector3.Transform(scaledPoint, rotationMatrix);

				const int branchToleranceMultiply = 2;
				if (Contains(rotatedPoint, branchToleranceMultiply))
				{
					similarity += 1f / pOtherBranch.TreePoints.Count;
				}
			}
			if (similarity - 1 > 0.1f) //similarity can be > 1 due to float imprecision
			{
				CDebug.Error("Similarity rounding error too big. " + similarity);
			}
			similarity = Math.Min(1, similarity);
			return similarity;
		}

		public float GetDistanceTo(Vector3 pPoint)
		{
			float distance = int.MaxValue;
			foreach (CTreePoint p in TreePoints)
			{
				float dist = Vector3.Distance(p.Center, pPoint);
				if (dist < distance)
				{
					distance = dist;
				}
				//else
				//{
				//	break;
				//}
			}
			return distance;
		}

		public override string ToString()
		{
			return "br_<" + angleTo / CTree.BRANCH_ANGLE_STEP + "> " +
					//GetPointCount().ToString("000") + 
					"[" + TreePoints.Count.ToString("00") + "] |";
		}

		public bool IsPointInExtent(Vector3 pPoint)
		{
			float pointDistToPeak = CUtils.Get2DDistance(pPoint, tree.peak);
			bool thisBranchInExtent = furthestPointDistance > pointDistToPeak;
			if (thisBranchInExtent) { return true; }

			bool leftBranchInExtent = GetNeigbourBranch(-1).furthestPointDistance > pointDistToPeak;
			if (leftBranchInExtent) { return true; }

			bool rightBranchInExtent = GetNeigbourBranch(1).furthestPointDistance > pointDistToPeak;
			return rightBranchInExtent;
		}

		public float GetDefinedFactor()
		{
			if (TreePoints.Count == 0)
			{
				return 0;
			}
			if (TreePoints.Count < CTreeManager.MIN_BRANCH_POINT_COUNT)
			{
				int allTreePointsCount = tree.GetAllPoints().Count;
				if (allTreePointsCount < CTreeManager.MIN_TREE_POINT_COUNT)
				{
					return 0;
				}
			}

			float height = tree.GetTreeHeight();
			float distLowestToPeak = Vector3.Distance(TreePoints.Last().Center, tree.peak.Center);

			float lowestPointRatio = (distLowestToPeak + GetMinDefinedHeightOffset(height)) / height;

			int treePointCount = TreePoints.Count;
			const int minPointsPerMeter = 3;
			//float pointCountRatio = treePointCount / (height * minPointsPerMeter);
			float pointCountRatio = treePointCount / (distLowestToPeak * minPointsPerMeter);
			pointCountRatio = Math.Min(pointCountRatio, 1);

			float factor = (lowestPointRatio + pointCountRatio) / 2;
			return factor;
		}

		private float GetMinDefinedHeightOffset(float pTreeHeight)
		{
			float offset = 5 + pTreeHeight / CTreeManager.AVERAGE_TREE_HEIGHT;
			return offset;
		}
	}
}