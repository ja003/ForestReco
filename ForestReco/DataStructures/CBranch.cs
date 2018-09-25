using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public class CBranch
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

		public List<string> Serialize()
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
				//todo: check if added ordered
			}
		}


		public float GetAddPointFactor(Vector3 pPoint)
		{
			//Vector3 referencePoint = GetClosestPointTo(pPoint, 5);

			Vector3 refPoint1 = furthestPoint;
			float refPoint1Factor = GetAddPointFactorInRefTo(pPoint, refPoint1);
			float bestFactor = refPoint1Factor;
			if (bestFactor > .99f) { return bestFactor; }

			Vector3 refPoint2 = GetNeigbourBranch(1).furthestPoint;
			float refPoint2Factor = GetAddPointFactorInRefTo(pPoint, refPoint2);
			if (refPoint2Factor > bestFactor)
			{
				bestFactor = refPoint2Factor;
				if (bestFactor > .99f) { return bestFactor; }
			}

			Vector3 refPoint3 = GetNeigbourBranch(-1).furthestPoint;
			float refPoint3Factor = GetAddPointFactorInRefTo(pPoint, refPoint3);
			if (refPoint3Factor > bestFactor)
			{
				bestFactor = refPoint3Factor;
			}

			if (Vector3.Distance(pPoint, new Vector3(-10.74f, 3.011f, 9.02f)) < 0.01f)
			{
				Console.WriteLine("\nP = " + pPoint);
				Console.WriteLine(refPoint1);
				Console.WriteLine(refPoint2);
				Console.WriteLine(refPoint3);
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

		private float GetAddPointFactorInRefTo(Vector3 pPoint, Vector3 pReferencePoint)
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
			if (distToPeakDiff < 0.3)
			{
				return 1;
			}
			//not very effective
			//float unacceptabledistToPeakDiff = 0.5f;
			//bool useDistToPeakDiff = distToPeakDiff < unacceptabledistToPeakDiff;
			//float distToPeakDiffFactor = (unacceptabledistToPeakDiff - distToPeakDiff) / unacceptabledistToPeakDiff;
			//distToPeakDiffFactor = Math.Max(0, distToPeakDiffFactor);

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
			float unacceptableDistance = tree.GetTreeExtentFor(pPoint) + 0.5f;
			if (pointDistToPeak > unacceptableDistance) { return 0; }
			unacceptableDistance += 0.5f;
			float distFactor = (unacceptableDistance - pointDistToPeak) / unacceptableDistance;

			float totalFactor = 0;
			/*if (useDistToPeakDiff)
			{
				totalFactor = (angleFactor + distFactor + distToPeakDiffFactor) / 3;
			}
			else
			{
				totalFactor = (angleFactor + distFactor) / 2;
			}*/
			totalFactor = (angleFactor + distFactor) / 2;

			return totalFactor;
		}

		public void AddPoint(Vector3 pPoint)
		{
			if (CTreeManager.DEBUG)
				Console.WriteLine("--- AddPoint " + pPoint.ToString("#+0.00#;-0.00") + " to " + this);

			RefreshFurthestPoint(pPoint);

			for (int i = 0; i < TreePoints.Count; i++)
			{
				CTreePoint pointOnBranch = TreePoints[i];
				if (pointOnBranch.Includes(pPoint))
				{
					pointOnBranch.AddPoint(pPoint);
					if (CTreeManager.DEBUG) Console.WriteLine("---- added at " + i);
					return;
				}
				if (pPoint.Y > pointOnBranch.Y)
				{
					CTreePoint newPointOnBranch = new CTreePoint(pPoint);
					TreePoints.Insert(i, newPointOnBranch);
					if (CTreeManager.DEBUG) { Console.WriteLine("---- inserted at " + i); }
					return;
				}
			}
			CTreePoint newPoint = new CTreePoint(pPoint);
			TreePoints.Add(newPoint);
			if (CTreeManager.DEBUG) { Console.WriteLine("---- new point"); }

		}

		private void RefreshFurthestPoint(Vector3 pPoint)
		{
			//if (Vector3.Distance(pPoint, new Vector3(-10.019f,3.113f,9.264f)) < 0.1f)
			//{
			//	Console.WriteLine("+++ "+pPoint);
			//}

			float pointDistToPeak = CUtils.Get2DDistance(pPoint, tree.peak);
			if (pointDistToPeak > furthestPointDistance)
			{
				furthestPoint = pPoint;

				//int indexOfThis = tree.Branches.IndexOf(this);
				//if (indexOfThis == 4 || indexOfThis == 5 || indexOfThis == 6)
				//{
				//	Console.WriteLine(tree.Branches.IndexOf(this) + " furthestPoint = " + furthestPoint+ " | " + pointDistToPeak);
				//}

				//Console.WriteLine(tree.Branches.IndexOf(this)  + " furthestPoint = " + furthestPoint);
			}
		}


		/// <summary>
		/// If given tree point is included in one of points on this branch
		/// </summary>
		public bool Contains(Vector3 pPoint, float pToleranceMultiply)
		{
			foreach (CTreePoint p in TreePoints)
			{
				//todo: include complete rotation based on tree orientation
				if (p.Includes(pPoint, pToleranceMultiply)) { return true; }
			}
			return false;
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
		public float GetSimilarityWith(CBranch pOtherBranch)
		{
			Vector3 offsetToThisTree = CTreeMath.GetOffsetTo(pOtherBranch.tree, tree);
			float scaleRatio = CTreeMath.GetScaleRatioTo(pOtherBranch.tree, tree);

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
			float angleOffsetRadians = CUtils.ToRadians(-(angleFrom - pOtherBranch.angleFrom));
			Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scaleRatio, scaleRatio, scaleRatio, tree.peak.Center);
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
				Console.WriteLine("Error. Similarity rounding error too big.");
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
			bool thisBranchInExtent =
				furthestPointDistance > CUtils.Get2DDistance(pPoint, tree.peak);
			if (thisBranchInExtent) { return true; }

			bool leftBranchInExtent =
				GetNeigbourBranch(-1).furthestPointDistance > CUtils.Get2DDistance(pPoint, tree.peak);
			if (leftBranchInExtent) { return true; }

			bool rightBranchInExtent =
				GetNeigbourBranch(1).furthestPointDistance > CUtils.Get2DDistance(pPoint, tree.peak);
			return rightBranchInExtent;
		}
	}
}