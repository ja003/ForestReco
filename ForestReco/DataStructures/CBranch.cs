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

		private Vector3 furthestPoint;
		private float furthestPointDistance => CUtils.Get2DDistance(furthestPoint, tree.peak);

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
				//todo: check if added ordered
			}
		}

		/// <summary>
		/// Calculates factor, showing how much does given point fit to this branch.
		/// Range = 0-1. 1 = best fit.
		/// pUseDistToPeakDiff = uses criterium of pUseDistToPeakDiff (viz GetAddPointFactorInRefTo)
		/// </summary>
		public float GetAddPointFactor(Vector3 pPoint, bool pUseDistToPeakDiff)
		{
			//Vector3 referencePoint = GetClosestPointTo(pPoint, 5);

			Vector3 refPoint1 = furthestPoint;
			float refPoint1Factor = GetAddPointFactorInRefTo(pPoint, refPoint1, pUseDistToPeakDiff);
			float bestFactor = refPoint1Factor;
			if (bestFactor > .99f) { return bestFactor; }

			Vector3 refPoint2 = GetNeigbourBranch(1).furthestPoint;
			float refPoint2Factor = GetAddPointFactorInRefTo(pPoint, refPoint2, pUseDistToPeakDiff);
			if (refPoint2Factor > bestFactor)
			{
				bestFactor = refPoint2Factor;
				if (bestFactor > .99f) { return bestFactor; }
			}

			Vector3 refPoint3 = GetNeigbourBranch(-1).furthestPoint;
			float refPoint3Factor = GetAddPointFactorInRefTo(pPoint, refPoint3, pUseDistToPeakDiff);
			if (refPoint3Factor > bestFactor)
			{
				bestFactor = refPoint3Factor;
			}
			
			return bestFactor;
		}

		private CBranch GetNeigbourBranch(int pIndexIncrement)
		{
			int indexOfthis = tree.Branches.IndexOf(this);
			int neighbourBranchIndex = (indexOfthis + pIndexIncrement) % tree.Branches.Count;
			if (neighbourBranchIndex < 0) { neighbourBranchIndex += tree.Branches.Count; }
			return tree.Branches[neighbourBranchIndex];
		}

		private Vector3 GetClosestPointTo(Vector3 pPoint, int pMaxIterationCount)
		{
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

		private float GetAddPointFactorInRefTo(Vector3 pPoint, Vector3 pReferencePoint, bool pIsTreeProcessed)
		{
			if (pPoint.Y > pReferencePoint.Y)
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
			if (pointDistToPeak < refDistToPeak)
			{
				return 1;
			}
			float distToPeakDiff = pointDistToPeak - refDistToPeak;
			if (!pIsTreeProcessed && distToPeakDiff < 0.3)
			{
				return 1;
			}
			if (!pIsTreeProcessed && distToPeakDiff > 0.5f)
			{
				float peakRefPointAngle = CUtils.AngleBetweenThreePoints(tree.peak.Center, pReferencePoint, pPoint);
				//todo: check this criterium
				//new point is too far from furthest point and goes too much out of direction of peak->furthestPoint
				if (peakRefPointAngle < 180 - 45)
				{
					return 0;
				}
			}



			//TODO:TEST. not very effective
			float unacceptabledistToPeakDiff = 0.5f;
			//bool useDistToPeakDiff = distToPeakDiff < unacceptabledistToPeakDiff;
			float distToPeakDiffFactor = (unacceptabledistToPeakDiff - distToPeakDiff) / unacceptabledistToPeakDiff;
			distToPeakDiffFactor = Math.Max(0, distToPeakDiffFactor);

			float refAngleToPoint =
				CUtils.AngleBetweenThreePoints(pReferencePoint - Vector3.UnitY, pReferencePoint, pPoint);

			Vector3 suitablePeakPoint = tree.peak.GetClosestPointTo(pPoint);
			float peakAngleToPoint =
				CUtils.AngleBetweenThreePoints(suitablePeakPoint - Vector3.UnitY, suitablePeakPoint, pPoint);
			float angle = Math.Min(refAngleToPoint, peakAngleToPoint);

			//const float unacceptableAngle = CTreeManager.MAX_BRANCH_ANGLE * 2;
			float maxBranchAngle = CTree.GetMaxBranchAngle(suitablePeakPoint, pPoint);
			float unacceptableAngle = maxBranchAngle;
			if (angle > unacceptableAngle) { return 0; }
			unacceptableAngle += 30;
			unacceptableAngle = Math.Min(unacceptableAngle, 100);

			float angleFactor = (unacceptableAngle - angle) / unacceptableAngle;

			//const float unacceptableDistance = CTreeManager.DEFAULT_TREE_EXTENT * 3;
			float unacceptableDistance = tree.GetTreeExtentFor(pPoint,
				pIsTreeProcessed ? CTreeManager.TREE_EXTENT_MERGE_MULTIPLY : 1);
			unacceptableDistance += 0.5f;
			if (pointDistToPeak > unacceptableDistance) { return 0; }
			unacceptableDistance += 0.5f;
			float distFactor = (unacceptableDistance - pointDistToPeak) / unacceptableDistance;

			float totalFactor;

			if (pIsTreeProcessed)
			{
				totalFactor = (angleFactor + distFactor + .5f * distToPeakDiffFactor) / 2.5f;
			}
			else
			{
				//let dist factor have higher influence
				totalFactor = (angleFactor + 1.5f * distFactor) / 2.5f;
			}
			//totalFactor = (angleFactor + distFactor) / 2;

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
						return;
					}
					//add point at correct position
					if (pPoint.Y < pointOnBranch.Y)
					{
						//points doesnt have to neccessarily Y-ordered. check close points for possible candidate
						int higherPointIndex = Math.Min(TreePoints.Count - 1, insertAtIndex);
						CTreePoint higherPointOnBranch = TreePoints[higherPointIndex];
						for (int j = higherPointIndex; j > 0 && higherPointOnBranch.Y - pointOnBranch.Y < tree.treePointExtent; j--)
						{
							higherPointOnBranch = TreePoints[j];
							if (higherPointOnBranch.Includes(pPoint))
							{
								higherPointOnBranch.AddPoint(pPoint);
								return;
							}
						}
						break;
					}
				}
			}

			CTreePoint newPoint = new CTreePoint(pPoint, tree.treePointExtent);
			TreePoints.Insert(insertAtIndex, newPoint);

			//CheckBranch(); //todo: delete, expensive!
			CheckAddedPoint();

			if (CTreeManager.DEBUG) { CDebug.WriteLine("---- new point"); }

		}

		private void CheckAddedPoint()
		{
			if (TreePoints.Count < 2) { return; }
			CTreePoint previousTp = TreePoints[TreePoints.Count - 2];
			CTreePoint tp = TreePoints[TreePoints.Count - 1];
			if (tp.Y > previousTp.Y)
			{
				if (Math.Abs(tp.Y - previousTp.Y) > tree.treePointExtent)
				{
					CDebug.Error("CheckAddedPoint. tree " + tree.treeIndex + ": " + tp + " is higher than " + previousTp);
				}
			}
		}

		public void CheckBranch()
		{
			for (int i = 1; i < TreePoints.Count; i++)
			{
				CTreePoint previousTp = TreePoints[i - 1];
				CTreePoint tp = TreePoints[i];
				if (tp.Y > previousTp.Y)
				{

					if (Math.Abs(tp.Y - previousTp.Y) > tree.treePointExtent)
					{
						CDebug.Error("-CheckBranch. tree " + tree.treeIndex + ": " + tp + " is higher than " + previousTp);
					}
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
			//todo: make effective
			//foreach (CTreePoint p in TreePoints)
			//{
			//	if (p.Includes(pPoint, pToleranceMultiply)) { return true; }
			//}

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
				if (pointOnBranch.Includes(pPoint)) { return true; }
				isPointOnBranchWithinRange = Math.Abs(pointOnBranch.Y - pPoint.Y) < treePointExtent + 1;
			}

			dir = -1;
			pointOnBranch = TreePoints[approxIndex];
			isPointOnBranchWithinRange = Math.Abs(pointOnBranch.Y - pPoint.Y) < treePointExtent + 1;
			for (int i = approxIndex; isPointOnBranchWithinRange && i > 0 && i < TreePoints.Count; i += dir)
			{
				pointOnBranch = TreePoints[i];
				if (pointOnBranch.Includes(pPoint)) { return true; }
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
			//float firstLastDiff = TreePoints[fromIndex].Y - TreePoints[toIndex].Y;
			//float step = firstLastDiff / (toIndex - fromIndex);
			//float peakPointDiff = tree.peak.Y - pPoint.Y;
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
			//Vector3 offsetToThisTree = CTreeMath.GetOffsetTo(pOtherBranch.tree, tree);
			//float scaleRatio = CTreeMath.GetScaleRatioTo(pOtherBranch.tree, tree);

			if (pOtherBranch.TreePoints.Count == 0)
			{
				if (TreePoints.Count == 0) { return 1; }
				//todo: situation when other branch has no points.
				//this can mean that data in this part of tree are just missing therefore it should
				return UNDEFINED_SIMILARITY;
			}

			float similarity = 0;
			//Vector3 groundPosition = tree.GetGroundPosition();

			//CreateRotationY rotates point counter-clockwise => -pAngleOffset
			//rotation has to be calculated in each branch
			float angleOffsetRadians = CUtils.ToRadians(-(angleFrom - pOtherBranch.angleFrom));
			//Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scaleRatio, scaleRatio, scaleRatio, tree.peak.Center);
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
			//return "br_<" + angleFrom + "," + angleTo + "> " + points.Count + " [" + GetPointCount() + "] |";
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
			//return rightBranchInExtent && leftBranchInExtent;
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
				//float allPointsToBranchRatio = (float)allTreePointsCount / TreePoints.Count;
				//if (allPointsToBranchRatio > allTreePointsCount / 2f)
				//{
				//	return 0;
				//}
			}


			float height = tree.GetTreeHeight();
			float distLowestToPeak = Vector3.Distance(TreePoints.Last().Center, tree.peak.Center);
			distLowestToPeak += 5; //first meters from ground is not well defined
			distLowestToPeak = Math.Min(height, distLowestToPeak);
			float lowestPointRatio = distLowestToPeak / height;

			int treePointCount = TreePoints.Count;
			const int minPointsPerMeter = 3;
			float pointCountRatio = treePointCount / (height * minPointsPerMeter);
			pointCountRatio = Math.Min(pointCountRatio, 1);

			float factor = (lowestPointRatio + pointCountRatio) / 2;
			return factor;
		}
	}
}