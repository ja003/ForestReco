﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace ForestReco
{
	public class CPointElement
	{
		public CPointElement Left;
		public CPointElement Right;
		public CPointElement Top;
		public CPointElement Bot;

		private List<Vector3> pointsVege = new List<Vector3>(); //high vegetation (class 5)
		private List<Vector3> pointsGround = new List<Vector3>(); //ground (class 1)

		public float? MinVege;
		public float? MaxVege;
		public float? SumVege;

		public float? MinGround;
		public float? MaxGround;
		public float? SumGround;

		public bool IsLocalMax;
		public bool IsLocalMin;
		public int VertexIndex = -1;

		private readonly Tuple<int, int> indexInField;

		//public int TreeIndex = -1; //tree, which this element belongs.
		public CPointElement Tree; //tree, which this element belongs.
		public int TreeElementsCount;


		public CPointElement(Tuple<int, int> pIndexInField)
		{
			indexInField = pIndexInField;
		}


		public void AddPoint(int pClass, Vector3 pPoint)
		{
			float height = pPoint.Z;

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
				if (height > MaxVege || MaxVege == null) { MaxVege = height; }
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
			//if(indexInField == null){ Console.WriteLine("!");}
			//dont calculate extrem if not all neighbours are defined (at border)
			if (!HasAllNeighbours()) { return false; }

			if (pExtrem) { IsLocalMax = true; }
			else { IsLocalMin = true; }

			for (int x = -pKernelSize; x <= pKernelSize; x++)
			{
				for (int y = -pKernelSize; y <= pKernelSize; y++)
				{
					CPointElement otherEl = GetElementWithOffset(x, y);

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

		private CPointElement GetClosestDefined(EHeight pHeight, EDirection pDirection)
		{
			if (IsDefined(pHeight)) { return this; }
			return GetNeighbour(pDirection)?.GetClosestDefined(pHeight, pDirection);
		}

		public float? GetAverageHeightFromClosestDefined(EHeight pHeight)
		{
			if (this.Equals(new CPointElement(new Tuple<int, int>(10, 2))))
			{
				Console.Write("!");
			}

			if (IsDefined(pHeight)) { return GetHeight(pHeight); }
			//
			CPointElement closestFirst = GetClosestDefined(pHeight, EDirection.Left);
			CPointElement closestSecond = GetClosestDefined(pHeight, EDirection.Right);
			//
			if (closestFirst == null || closestSecond == null)
			{
				closestFirst = GetClosestDefined(pHeight, EDirection.Top);
				closestSecond = GetClosestDefined(pHeight, EDirection.Bot);
			}

			if (closestFirst != null && closestSecond != null)
			{
				CPointElement smaller = closestFirst;
				CPointElement higher = closestSecond;
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
					return smallerHeight + distanceToSmaller / totalDistance * heightDiff;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns height of given type.
		/// pGetHeightFromNeighbour: True = ifNotDefined => closest defined height will be used (runs DFS)
		/// pVisited: dont use these elements in DFS
		/// </summary>
		public float? GetHeight(EHeight pHeight, bool pGetHeightFromNeighbour = false,
			List<CPointElement> pVisited = null)
		{
			if (!IsDefined(pHeight) && pGetHeightFromNeighbour)
			{
				if (pVisited == null) { pVisited = new List<CPointElement>(); }

				foreach (CPointElement n in GetNeighbours())
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
				case EHeight.Tree: return GetHeightTree();
				case EHeight.GroundMin: return GetHeightExtrem(false, EClass.Ground);
				case EHeight.GroundMax: return GetHeightExtrem(true, EClass.Ground);
				case EHeight.IndexX: return indexInField.Item1;
				case EHeight.IndexY: return indexInField.Item2;
			}
			return null;
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

		private float? GetHeightTree()
		{
			if (HasAssignedTree())
			{
				float? heightTree = Tree.MaxVege;// - (Tree.GetHeightExtrem(true, EClass.Ground, true) ?? Tree.MaxVege + 10);
				if (Tree == this) { return heightTree; }
				else { return heightTree - GetDistanceToTree() * 1f; }
			}
			return null;

			if (IsNeighbourLocalMax(EDirection.Left) ||
				IsNeighbourLocalMax(EDirection.Top) ||
				IsNeighbourLocalMax(EDirection.Right) ||
				IsNeighbourLocalMax(EDirection.Bot))
			{
				return 0;
			}
			return null;
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

		private int GetDistanceTo(CPointElement pElement)
		{
			return Math.Abs(indexInField.Item1 - pElement.indexInField.Item1) +
					 Math.Abs(indexInField.Item2 - pElement.indexInField.Item2);
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

		public void AssignTreeToNeighbours()
		{
			if (!HasAllNeighbours()) { return; }

			List<CPointElement> neighbours = GetNeighbours();
			foreach (CPointElement n in neighbours)
			{
				//already belongs to other tree
				if (!n.HasAssignedTree())
				{
					float? height = GetHeight(EHeight.VegeMax);
					float? neighbourHeight = n.GetHeight(EHeight.VegeMax) ?? n.GetHeight(EHeight.GroundMax);

					//TODO: zkontrolovat, proč je tam tolik undefined polí
					/*if (TreeIndex == 32)
					{
						Console.WriteLine(this);
					}*/

					if (height != null && neighbourHeight != null)
					{
						float heightDiff = (float)height - (float)neighbourHeight;
						//this element is higher (if lower => tree1-tree2) and difference is not big (big => tree-ground)
						const float MIN_HEIGHT_DIFF = 1.5f;
						if (heightDiff > 0 && heightDiff < MIN_HEIGHT_DIFF)
						{
							n.Tree = Tree;
							Tree.TreeElementsCount++;
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

		public bool HasAssignedTree()
		{
			return Tree != null;
		}

		/// <summary>
		/// All elements but those at edge should have assigned neigbours
		/// </summary>
		private bool HasAllNeighbours()
		{
			return Left != null && Right != null && Top != null && Bot != null;
		}

		/// <summary>
		/// Returnd element with given local position to this element
		/// </summary>
		private CPointElement GetElementWithOffset(int pIndexOffsetX, int pIndexOffsetY)
		{
			CPointElement el = this;
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


		private bool IsNeighbourLocalMax(EDirection pNeighbour)
		{
			return GetNeighbour(pNeighbour) != null && GetNeighbour(pNeighbour).IsLocalMax;
		}

		private List<CPointElement> GetNeighbours()
		{
			List<CPointElement> neighbours = new List<CPointElement>();

			if (GetNeighbour(EDirection.Left) != null) { neighbours.Add(GetNeighbour(EDirection.Left)); }
			if (GetNeighbour(EDirection.Top) != null) { neighbours.Add(GetNeighbour(EDirection.Top)); }
			if (GetNeighbour(EDirection.Right) != null) { neighbours.Add(GetNeighbour(EDirection.Right)); }
			if (GetNeighbour(EDirection.Bot) != null) { neighbours.Add(GetNeighbour(EDirection.Bot)); }

			return neighbours;
		}

		private CPointElement GetNeighbour(EDirection pNeighbour)
		{
			switch (pNeighbour)
			{
				case EDirection.Bot: return Bot;
				case EDirection.Left: return Left;
				case EDirection.Right: return Right;
				case EDirection.Top: return Top;
			}
			return null;
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

		public void FillMissingHeight(EHeight pHeight)
		{
			if (IsDefined(pHeight)) { return; }
			switch (pHeight)
			{
				case EHeight.GroundMax:
					MaxGround = GetAverageHeightFromClosestDefined(pHeight);
					break;
			}
		}

		public override string ToString()
		{
			string maxV = "-";
			if (MaxVege != null) { maxV = MaxVege.ToString(); }
			string maxG = "-";
			if (MaxGround != null) { maxG = MaxGround.ToString(); }
			return "[" + indexInField + "]";
			return indexInField + ": MaxVege = " + maxV + "," + "MaxGround = " + maxG;
		}

		public override bool Equals(object obj)
		{
			// Check for null values and compare run-time types.
			if (obj == null || GetType() != obj.GetType())
				return false;

			CPointElement e = (CPointElement)obj;
			return (indexInField.Item1 == e.indexInField.Item1) && (indexInField.Item2 == e.indexInField.Item2);
		}
	}


	/*public enum EExtrem
	{
		None,
		Min,
		Max
	}*/

	public enum EDirection
	{
		None = 0,
		Left,
		Right,
		Top,
		Bot
	}

	public enum EClass
	{
		Ground,
		Vege
	}
}