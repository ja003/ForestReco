﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
#pragma warning disable 659

namespace ForestReco
{
	public class CGroundField
	{
		public CGroundField Left;
		public CGroundField Right;
		public CGroundField Top;
		public CGroundField Bot;
		private List<CGroundField> neighbours;

		private List<Vector3> points = new List<Vector3>();

		public float? MinGround;
		public float? MaxGround;
		public float? SumGround;

		public int VertexIndex = -1;

		private readonly Tuple<int, int> indexInField;

		private Vector3 center;

		//--------------------------------------------------------------

		public CGroundField(Tuple<int, int> pIndexInField, Vector3 pCenter)
		{
			indexInField = pIndexInField;
			center = pCenter;
		}

		//NEIGHBOUR

		public bool IsAnyNeighbourDefined()
		{
			foreach (CGroundField n in GetNeighbours())
			{
				if (n.IsDefined()) { return true; }
			}
			return false;
		}

		private CGroundField GetNeighbour(EDirection pNeighbour)
		{
			switch (pNeighbour)
			{
				case EDirection.Bot: return Bot;
				case EDirection.Left: return Left;
				case EDirection.Right: return Right;
				case EDirection.Top: return Top;

				case EDirection.LeftTop: return Left?.Top;
				case EDirection.RightTop: return Right?.Top;
				case EDirection.RightBot: return Right?.Bot;
				case EDirection.LeftBot: return Left?.Bot;

			}
			return null;
		}

		/// <summary>
		/// All points but those at edge should have assigned neigbours
		/// </summary>
		private bool HasAllNeighbours()
		{
			return Left != null && Right != null && Top != null && Bot != null;
		}


		private List<CGroundField> GetNeighbours()
		{
			if (neighbours != null) { return this.neighbours; }

			neighbours = new List<CGroundField>();
			var directions = Enum.GetValues(typeof(EDirection));
			foreach (EDirection d in directions)
			{
				CGroundField neighour = GetNeighbour(d);
				if (neighour != null) { neighbours.Add(neighour); }
			}

			return neighbours;
		}

		//PUBLIC

		public void AddPoint(Vector3 pPoint)
		{
			float height = pPoint.Y;

			points.Add(pPoint);
			if (SumGround != null) { SumGround += height; }
			else { SumGround = height; }
			if (height > MaxGround || MaxGround == null) { MaxGround = height; }
			if (height < MinGround || MinGround == null) { MinGround = height; }

		}

		public bool IsDefined()
		{
			return points.Count > 0;
		}

		public float? GetAverageHeightFromNeighbourhood(int pKernelSize)
		{
			int defined = 0;
			float heightSum = 0;
			for (int x = -pKernelSize; x < pKernelSize; x++)
			{
				for (int y = -pKernelSize; y < pKernelSize; y++)
				{
					int xIndex = indexInField.Item1 + x;
					int yIndex = indexInField.Item2 + y;
					CGroundField el = CProjectData.array.GetElement(xIndex, yIndex);
					if (el != null && el.IsDefined())
					{
						defined++;
						// ReSharper disable once PossibleInvalidOperationException
						//is checked
						heightSum += (float)el.GetHeight();
					}
				}
			}
			if (defined == 0) { return null; }
			return heightSum / defined;
		}

		/// <summary>
		/// Finds closest defined fields in direction based on pDiagonal parameter.
		/// Returns average of 2 found heights considering their distance from this field.
		/// </summary>
		public float? GetAverageHeightFromClosestDefined(int pMaxSteps, bool pDiagonal)
		{
			if (IsDefined()) { return GetHeight(); }
			//
			CGroundField closestFirst = null;
			CGroundField closestSecond = null;
			CGroundField closestLeft = GetClosestDefined(pDiagonal ? EDirection.LeftTop : EDirection.Left, pMaxSteps);
			CGroundField closestRight = GetClosestDefined(pDiagonal ? EDirection.RightBot : EDirection.Right, pMaxSteps);
			CGroundField closestTop = GetClosestDefined(pDiagonal ? EDirection.RightTop : EDirection.Top, pMaxSteps);
			CGroundField closestBot = GetClosestDefined(pDiagonal ? EDirection.LeftBot : EDirection.Bot, pMaxSteps);

			closestFirst = closestLeft;
			closestSecond = closestRight;
			if ((closestFirst == null || closestSecond == null) && closestTop != null && closestBot != null)
			{
				closestFirst = closestTop;
				closestSecond = closestBot;
			}

			//if (closestFirst == null) { closestFirst = closestTop; }
			//if (closestSecond == null) { closestSecond = closestTop; }
			//if (closestFirst == null) { closestFirst = closestBot; }
			//if (closestSecond == null) { closestSecond = closestBot; }

			if (closestFirst != null && closestSecond != null)
			{
				CGroundField smaller = closestFirst;
				CGroundField higher = closestSecond;
				if (closestSecond.GetHeight() < closestFirst.GetHeight())
				{
					higher = closestFirst;
					smaller = closestSecond;
				}
				int totalDistance = smaller.GetDistanceTo(higher);
				float? heightDiff = higher.GetHeight() - smaller.GetHeight();
				if (heightDiff != null)
				{
					float? smallerHeight = smaller.GetHeight();
					float distanceToSmaller = GetDistanceTo(smaller);

					//if (totalDistance == 0) { return smallerHeight; }

					return smallerHeight + distanceToSmaller / totalDistance * heightDiff;
				}
			}
			else if (!HasAllNeighbours())
			{
				if (closestLeft != null) { return closestLeft.GetHeight(); }
				if (closestTop != null) { return closestTop.GetHeight(); }
				if (closestRight != null) { return closestRight.GetHeight(); }
				if (closestBot != null) { return closestBot.GetHeight(); }
			}
			if (!pDiagonal)
			{
				return GetAverageHeightFromClosestDefined(pMaxSteps, true);
			}

			return null;
		}

		/// <summary>
		/// Returns height of given type.
		/// pGetHeightFromNeighbour: True = ifNotDefined => closest defined height will be used (runs DFS)
		/// pVisited: dont use these points in DFS
		/// </summary>
		public float? GetHeight(bool pGetHeightFromNeighbour = false, List<CGroundField> pVisited = null)
		{
			if (!IsDefined() && pGetHeightFromNeighbour)
			{
				if (pVisited == null) { pVisited = new List<CGroundField>(); }

				foreach (CGroundField n in GetNeighbours())
				{
					if (!pVisited.Contains(n))
					{
						pVisited.Add(this);
						return n.GetHeight(true, pVisited);
					}
				}
				return null;
			}
			//todo: decide which height to return on default
			return GetHeightAverage();
		}

		public int GetDistanceTo(CGroundField pGroundField)
		{
			return Math.Abs(indexInField.Item1 - pGroundField.indexInField.Item1) +
				   Math.Abs(indexInField.Item2 - pGroundField.indexInField.Item2);
		}

		private float? MaxGroundFilled;

		public void ApplyFillMissingHeight()
		{
			if (IsDefined()) { return; }
			//MaxGround = MaxGroundFilled;
			if (MaxGroundFilled == null) { return; }

			Vector3 filledPoint = center;
			filledPoint.Y = (float)MaxGroundFilled;
			AddPoint(filledPoint);
		}


		public void FillMissingHeight(EFillMethod pMethod)
		{

			if (IsDefined()) { return; }

			int maxSteps = 1;
			switch (pMethod)
			{
				case EFillMethod.ClosestDefined:
					//todo: maybe maxSteps should be infinite
					MaxGroundFilled = GetAverageHeightFromClosestDefined(10 * maxSteps, false);
					break;
				case EFillMethod.FromNeighbourhood:
					MaxGroundFilled = GetAverageHeightFromNeighbourhood(maxSteps);
					break;
			}
		}

		public enum EFillMethod
		{
			ClosestDefined,
			FromNeighbourhood
		}

		///PRIVATE

		private CGroundField GetClosestDefined(EDirection pDirection, int pMaxSteps)
		{
			if (IsDefined()) { return this; }
			if (pMaxSteps == 0) { return null; }
			return GetNeighbour(pDirection)?.GetClosestDefined(pDirection, pMaxSteps - 1);
		}

		/// <summary>
		/// Returns maximal/minimal point in this field.
		/// pMax: True = maximum, False = minimum
		/// </summary>
		private float? GetHeightExtrem(bool pMax)
		{
			return pMax ? MaxGround : MinGround;
		}

		private float? GetHeightAverage()
		{
			if (!IsDefined()) { return null; }
			return SumGround / points.Count;
		}

		/// <summary>
		/// Returnd point with given local position to this point
		/// </summary>
		private CGroundField GetPointWithOffset(int pIndexOffsetX, int pIndexOffsetY)
		{
			CGroundField el = this;
			for (int x = 0; x < Math.Abs(pIndexOffsetX); x++)
			{
				el = pIndexOffsetX > 0 ? el.Right : el.Left;
				if (el == null) { return null; }
			}
			for (int y = 0; y < Math.Abs(pIndexOffsetY); y++)
			{
				el = pIndexOffsetY > 0 ? el.Top : el.Bot;
				if (el == null) { return null; }
			}
			return el;
		}

		//UNUSED

		private EDirection GetOpositeNeighbour(EDirection pNeighbour)
		{
			switch (pNeighbour)
			{
				case EDirection.Bot: return EDirection.Top;
				case EDirection.Top: return EDirection.Bot;
				case EDirection.Left: return EDirection.Right;
				case EDirection.Right: return EDirection.Left;
			}
			return EDirection.None;
		}

		//OTHER

		public string ToStringIndex()
		{
			return "[" + indexInField + "].";
		}

		public override string ToString()
		{
			return ToStringIndex() + " Ground = " + GetHeight();
			//return ToStringIndex() + " Tree = " + (Tree?.ToStringIndex() ?? "null");
		}

		public override bool Equals(object obj)
		{
			// Check for null values and compare run-time types.
			if (obj == null || GetType() != obj.GetType())
				return false;

			CGroundField e = (CGroundField)obj;
			return (indexInField.Item1 == e.indexInField.Item1) && (indexInField.Item2 == e.indexInField.Item2);
		}


	}
}