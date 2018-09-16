﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
#pragma warning disable 659

namespace ForestReco
{
	public class CPointField
	{
		public CPointField Left;
		public CPointField Right;
		public CPointField Top;
		public CPointField Bot;
		private List<CPointField> neighbours;

		private List<Vector3> pointsVege = new List<Vector3>(); //high vegetation (class 5)
		private List<Vector3> pointsGround = new List<Vector3>(); //ground (class 1)

		public float? MinVege;
		public float? MaxVege; //todo: delete and replace with MaxVegePoint
		public float? SumVege;
		public Vector3 MaxVegePoint;

		public float? MinGround;
		public float? MaxGround;
		public float? SumGround;


		public bool IsLocalMax;
		public bool IsLocalMin;
		public int VertexIndex = -1;

		private readonly Tuple<int, int> indexInField;

		public float? TreeHeight;
		public CPointField Tree; //tree, which this point belongs to.
		public List<CPointField> TreeFields = new List<CPointField>();
		public List<Vector3> TreePoints = new List<Vector3>();

		//--------------------------------------------------------------

		public CPointField(Tuple<int, int> pIndexInField)
		{
			indexInField = pIndexInField;
		}

		//TREE

		public void AssignTree(CPointField pTree)
		{
			//if(HasAssignedTree()){ return; }
			//if(GetHeight(EHeight.VegeMax) == null){ return; }
			Tree = pTree;
			Tree.TreeFields.Add(this);
		}

		public void AssignTreeToNeighbours()
		{
			if (!HasAllNeighbours()) { return; }

			foreach (CPointField n in GetNeighbours())
			{
				//already belongs to other tree
				if (!n.HasAssignedTree())
				{
					float? height = GetHeight(EHeight.VegeMax);
					float? neighbourHeight = n.GetHeight(EHeight.VegeMax) ?? n.GetHeight(EHeight.GroundMax);

					if (height != null && neighbourHeight != null)
					{
						float heightDiff = (float)height - (float)neighbourHeight;
						//this point is higher (if lower => tree1-tree2) and difference is not big (big => tree-ground)
						const float MAX_HEIGHT_DIFF = 2.5f;
						if (heightDiff > 0 && heightDiff < MAX_HEIGHT_DIFF)
						{
							n.AssignTree(Tree);
							n.AssignTreeToNeighbours();
							//Console.WriteLine(TreeIndex + " : " + n);
						}
					}
					else
					{
						//Console.WriteLine("XXXXX " + TreeIndex + " : " + n);
					}
				}
			}
		}

		/// <summary>
		/// Try to assign all vege points to its tree
		/// </summary>
		public void AssignPointsToTree()
		{
			foreach (Vector3 vegePoint in pointsVege)
			{
				Tree.AssignPointToTree(vegePoint);
			}
		}

		private void AssignPointToTree(Vector3 pVegePoint)
		{
			if (!Tree.Equals(this))
			{
				Console.WriteLine("Error. You are trying to assing point " + pVegePoint + " to field " + this + ", which is not tree.");
				return;
			}
			if (CUtils.PointBelongsToTree(pVegePoint, Tree.MaxVegePoint))
			{
				Tree.TreePoints.Add(pVegePoint);
			}
		}

		public bool HasAssignedTree()
		{
			return Tree != null;
		}

		public float? GetTreeHeight()
		{
			return GetHeight(EHeight.VegeMax) - GetHeight(EHeight.GroundMax);
			//todo: not actually height of the tree
			/*if (TreeHeight == null)
			{
				if (HasAssignedTree())
				{
					float? heightTree = Tree.MaxVege;
					if (Tree.Equals(this))
					{
						TreeHeight = heightTree;
					}
					else
					{
						TreeHeight = heightTree - GetDistanceToTree() * 0.5f;
					}
				}
			}
			return TreeHeight;*/
		}

		private int GetDistanceToTree()
		{
			if (Tree == null)
			{
				Console.Write(this + " Error. Tree not defined.");
				return -1;
			}
			return GetDistanceTo(Tree);
		}

		//NEIGHBOUR

		public bool IsAnyNeighbourDefined(EHeight pHeight)
		{
			foreach (CPointField n in GetNeighbours())
			{
				if (n.IsDefined(pHeight)) { return true; }
			}
			return false;
		}

		private CPointField GetNeighbour(EDirection pNeighbour)
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


		private List<CPointField> GetNeighbours()
		{
			if (neighbours != null) { return this.neighbours; }

			neighbours = new List<CPointField>();
			var directions = Enum.GetValues(typeof(EDirection));
			foreach (EDirection d in directions)
			{
				CPointField neighour = GetNeighbour(d);
				if (neighour != null) { neighbours.Add(neighour); }
			}

			return neighbours;
		}

		//PUBLIC

		public void AddPoint(int pClass, Vector3 pPoint)
		{
			float height = pPoint.Y;

			if (pClass == 2)
			{
				pointsGround.Add(pPoint);
				if (SumGround != null) { SumGround += height; }
				else { SumGround = height; }
				if (height > MaxGround || MaxGround == null) { MaxGround = height; }
				if (height < MinGround || MinGround == null) { MinGround = height; }
			}
			else if (pClass == 5)
			{
				pointsVege.Add(pPoint);
				if (SumVege != null) { SumVege += height; }
				else { SumVege = height; }
				if (height > MaxVege || MaxVege == null)
				{
					MaxVege = height;
					MaxVegePoint = pPoint;
				}
				if (height < MinVege || MinVege == null) { MinVege = height; }
			}
		}

		public bool IsDefined(EHeight pHeight)
		{
			bool isDefined = true;
			switch (pHeight)
			{
				case EHeight.GroundMax:
				case EHeight.GroundMin:
					isDefined = IsDefined(EClass.Ground);
					break;
				case EHeight.VegeAverage:
				case EHeight.VegeMax:
				case EHeight.VegeMin:
					isDefined = IsDefined(EClass.Vege);
					break;
				case EHeight.Tree:
					isDefined = GetTreeHeight() != null;
					break;
			}
			return isDefined;
		}

		public bool IsDefined(EClass pClass)
		{
			switch (pClass)
			{
				case EClass.Ground:
					return pointsGround.Count > 0 || MaxGround != null || MinGround != null;
				case EClass.Vege:
					return pointsVege.Count > 0 || MaxVege != null || MinVege != null;
			}
			return false;
		}

		/// <summary>
		/// Determines whether this point is local extrem in area defined by given kernel size
		/// TODO: oddělat parametr pExtrem a vracet EExtrem
		/// </summary>
		/// <param name="pExtrem">True = Max, False = Min</param>
		/// <param name="pKernelSize"></param>
		public bool CalculateLocalExtrem(bool pExtrem, int pKernelSize)
		{
			//dont calculate extrem if not all neighbours are defined (at border)
			if (!HasAllNeighbours()) { return false; }

			if (pExtrem) { IsLocalMax = true; }
			else { IsLocalMin = true; }

			for (int x = -pKernelSize; x <= pKernelSize; x++)
			{
				for (int y = -pKernelSize; y <= pKernelSize; y++)
				{
					CPointField otherEl = GetPointWithOffset(x, y);

					if (pExtrem)
					{
						if (otherEl != null && otherEl.MaxVege > MaxVege)
						{
							IsLocalMax = false;
							return false;
						}
					}
					else
					{
						if (otherEl != null && otherEl.MinVege < MinVege)
						{
							IsLocalMin = false;
							return false;
						}
					}
				}
			}
			return true;
		}


		public float? GetAverageHeightFromNeighbourhood(EHeight pHeight, int pKernelSize)
		{
			int defined = 0;
			float heightSum = 0;
			for (int x = -pKernelSize; x < pKernelSize; x++)
			{
				for (int y = -pKernelSize; y < pKernelSize; y++)
				{
					int xIndex = indexInField.Item1 + x;
					int yIndex = indexInField.Item2 + y;
					CPointField el = CProjectData.array.GetElement(xIndex, yIndex);
					if (el != null && el.IsDefined(pHeight))
					{
						defined++;
						// ReSharper disable once PossibleInvalidOperationException
						//is checked
						heightSum += (float)el.GetHeight(pHeight);
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
		public float? GetAverageHeightFromClosestDefined(EHeight pHeight, int pMaxSteps, bool pDiagonal)
		{
			if (IsDefined(pHeight)) { return GetHeight(pHeight); }
			//
			CPointField closestFirst = null;
			CPointField closestSecond = null;
			CPointField closestLeft = GetClosestDefined(pHeight, pDiagonal ? EDirection.LeftTop : EDirection.Left, pMaxSteps);
			CPointField closestRight = GetClosestDefined(pHeight, pDiagonal ? EDirection.RightBot : EDirection.Right, pMaxSteps);
			CPointField closestTop = GetClosestDefined(pHeight, pDiagonal ? EDirection.RightTop : EDirection.Top, pMaxSteps);
			CPointField closestBot = GetClosestDefined(pHeight, pDiagonal ? EDirection.LeftBot : EDirection.Bot, pMaxSteps);

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
				CPointField smaller = closestFirst;
				CPointField higher = closestSecond;
				if (closestSecond.GetHeight(pHeight) < closestFirst.GetHeight(pHeight))
				{
					higher = closestFirst;
					smaller = closestSecond;
				}
				int totalDistance = smaller.GetDistanceTo(higher);
				float? heightDiff = higher.GetHeight(pHeight) - smaller.GetHeight(pHeight);
				if (heightDiff != null)
				{
					float? smallerHeight = smaller.GetHeight(pHeight);
					float distanceToSmaller = GetDistanceTo(smaller);

					//if (totalDistance == 0) { return smallerHeight; }

					return smallerHeight + distanceToSmaller / totalDistance * heightDiff;
				}
			}
			else if (!HasAllNeighbours())
			{
				if (closestLeft != null) { return closestLeft.GetHeight(pHeight); }
				if (closestTop != null) { return closestTop.GetHeight(pHeight); }
				if (closestRight != null) { return closestRight.GetHeight(pHeight); }
				if (closestBot != null) { return closestBot.GetHeight(pHeight); }
			}
			if (!pDiagonal)
			{
				return GetAverageHeightFromClosestDefined(pHeight, pMaxSteps, true);
			}

			return null;
		}

		/// <summary>
		/// Returns height of given type.
		/// pGetHeightFromNeighbour: True = ifNotDefined => closest defined height will be used (runs DFS)
		/// pVisited: dont use these points in DFS
		/// </summary>
		public float? GetHeight(EHeight pHeight, bool pGetHeightFromNeighbour = false,
			List<CPointField> pVisited = null)
		{
			if (!IsDefined(pHeight) && pGetHeightFromNeighbour)
			{
				if (pVisited == null) { pVisited = new List<CPointField>(); }

				foreach (CPointField n in GetNeighbours())
				{
					if (!pVisited.Contains(n))
					{
						pVisited.Add(this);
						return n.GetHeight(pHeight, true, pVisited);
					}
				}
				return null;
			}
			switch (pHeight)
			{
				case EHeight.VegeMax: return MaxVege;
				case EHeight.VegeAverage: return GetHeightAverage(EClass.Vege);
				case EHeight.Tree: return GetTreeHeight();
				case EHeight.GroundMin: return GetHeightExtrem(false, EClass.Ground);
				case EHeight.GroundMax: return GetHeightExtrem(true, EClass.Ground);
				case EHeight.IndexX: return indexInField.Item1;
				case EHeight.IndexY: return indexInField.Item2;
				case EHeight.Maxima: return Tree != null && Tree.Equals(this) ? GetTreeHeight() : null;
			}
			return null;
		}

		public int GetDistanceTo(CPointField pPointField)
		{
			return Math.Abs(indexInField.Item1 - pPointField.indexInField.Item1) +
				   Math.Abs(indexInField.Item2 - pPointField.indexInField.Item2);
		}

		private float? MaxGroundFilled;

		public void ApplyFillMissingHeight()
		{
			if (IsDefined(EClass.Ground)) { return; }
			MaxGround = MaxGroundFilled;
		}

		public void FillMissingHeight(EHeight pHeight, EFillMethod pMethod)
		{
			if (IsDefined(pHeight)) { return; }
			switch (pHeight)
			{
				case EHeight.GroundMax:
					int maxSteps = 1;
					switch (pMethod)
					{
						case EFillMethod.ClosestDefined:
							//todo: maybe maxSteps should be infinite
							MaxGroundFilled = GetAverageHeightFromClosestDefined(pHeight, 10*maxSteps, false);
							break;
						case EFillMethod.FromNeighbourhood:
							MaxGroundFilled = GetAverageHeightFromNeighbourhood(pHeight, maxSteps);
							break;
					}
					break;
				default:
					Console.WriteLine("FillMissingHeight not defined for " + pHeight);
					return;
			}
		}

		public enum EFillMethod
		{
			ClosestDefined,
			FromNeighbourhood
		}

		///PRIVATE

		private CPointField GetClosestDefined(EHeight pHeight, EDirection pDirection, int pMaxSteps)
		{
			if (IsDefined(pHeight)) { return this; }
			if (pMaxSteps == 0) { return null; }
			return GetNeighbour(pDirection)?.GetClosestDefined(pHeight, pDirection, pMaxSteps - 1);
		}

		/// <summary>
		/// Returns extrem of given class.
		/// pMax: True = maximum, False = minimum
		/// </summary>
		private float? GetHeightExtrem(bool pMax, EClass pClass)
		{
			switch (pClass)
			{
				case EClass.Ground: return pMax ? MaxGround : MinGround;
				case EClass.Vege: return pMax ? MaxVege : MinVege;
			}
			return null;
		}

		private float? GetHeightAverage(EClass pClass)
		{
			if (!IsDefined(pClass)) { return null; }
			switch (pClass)
			{
				case EClass.Ground:
					return SumGround / pointsGround.Count;

				case EClass.Vege:
					return SumVege / pointsVege.Count;
			}
			return null;
		}


		/// <summary>
		/// Returnd point with given local position to this point
		/// </summary>
		private CPointField GetPointWithOffset(int pIndexOffsetX, int pIndexOffsetY)
		{
			CPointField el = this;
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

		private bool IsNeighbourLocalMax(EDirection pNeighbour)
		{
			return GetNeighbour(pNeighbour) != null && GetNeighbour(pNeighbour).IsLocalMax;
		}

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
			return ToStringIndex() + " Ground = " + GetHeight(EHeight.GroundMax) ?? "null";
			//return ToStringIndex() + " Tree = " + (Tree?.ToStringIndex() ?? "null");
		}

		public override bool Equals(object obj)
		{
			// Check for null values and compare run-time types.
			if (obj == null || GetType() != obj.GetType())
				return false;

			CPointField e = (CPointField)obj;
			return (indexInField.Item1 == e.indexInField.Item1) && (indexInField.Item2 == e.indexInField.Item2);
		}


	}
}