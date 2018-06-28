using System;
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

		private Tuple<int, int> indexInField;

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

		public bool IsDefined(EClass pClass)
		{
			switch (pClass)
			{
				case EClass.Ground:
					return pointsGround.Count > 0;
				case EClass.Vege:
					return pointsVege.Count > 0;
			}
			return false;
		}

		/// <summary>
		/// Determines whether this point is local extrem in area defined by given kernel size
		/// </summary>
		/// <param name="pExtrem">True = Max, False = Min</param>
		/// <param name="pKernelSize"></param>
		public void CalculateLocalExtrem(bool pExtrem, int pKernelSize)
		{
			//if(indexInField == null){ Console.WriteLine("!");}
			//dont calculate extrem if not all neighbours are defined (at border)
			if (!HasAllNeighbours()) { return; }
			IsLocalMax = true;
			IsLocalMin = true;
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
							return;
						}
					}
					else
					{
						if (otherEl != null && otherEl.MinVege < MinVege)
						{
							IsLocalMin = false;
							return;
						}
					}
				}
			}
		}

		public float? GetHeight(EHeight pHeight)
		{
			switch (pHeight)
			{
				case EHeight.VegeMax: return MaxVege;
				case EHeight.VegeAverage: return GetHeightAverage(EClass.Vege);
				case EHeight.Tree: return GetHeightTree();
				case EHeight.GroundMin: return GetHeightExtrem(false, EClass.Ground);
				case EHeight.GroundMax: return GetHeightExtrem(true, EClass.Ground);
			}
			return null;
		}

		private float? GetHeightTree()
		{
			//if (IsLocalMax) { return 10; }
			if (IsLocalMax)
			{
				return MaxVege - (GetHeightExtrem(true, EClass.Ground, true) ?? MaxVege + 10);
			}
			if (IsNeighbourLocalMax(ENeigbour.Left) ||
				IsNeighbourLocalMax(ENeigbour.Top) ||
				IsNeighbourLocalMax(ENeigbour.Right) ||
				IsNeighbourLocalMax(ENeigbour.Bot)) { return 0; }
			//return -1;
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
		/// Returns extrem of given class.
		/// pMax: True = maximum, False = minimum
		/// pGetHeightFromNeighbour: True = ifNotDefined => closest defined height will be used
		/// pExcludeNeighbour: dont use this neighbour (prevent cycle)
		/// </summary>
		public float? GetHeightExtrem(bool pMax, EClass pClass, bool pGetHeightFromNeighbour = false, ENeigbour pExcludeNeighbour = ENeigbour.None)// int pGetFromNeigbourMaxDistance = 0)
		{
			if (!IsDefined(pClass))
			{
				if (pGetHeightFromNeighbour)
				{
					for (int i = 1; i <= 4; i++)
					{
						ENeigbour eNeigbour = (ENeigbour)i;
						if (eNeigbour != pExcludeNeighbour)
						{
							CPointElement neighbour = GetNeighbour(eNeigbour);
							if (neighbour != null)
							{
								return neighbour.GetHeightExtrem(pMax, pClass, true, GetOpositeNeighbour(eNeigbour));
							}
						}
					}
				}
				return null;
			}
			switch (pClass)
			{
				case EClass.Ground: return pMax ? MaxGround : MinGround;

				case EClass.Vege: return pMax ? MaxVege : MinVege;
			}
			return null;
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

		public override string ToString()
		{
			string maxV = "-";
			if (MaxVege != null) { maxV = MaxVege.ToString(); }
			string maxG = "-";
			if (MaxGround != null) { maxG = MaxGround.ToString(); }
			return indexInField + ": MaxVege = " + maxV + "," + "MaxGround = " + maxG;
		}

		private bool IsNeighbourLocalMax(ENeigbour pNeighbour)
		{
			return GetNeighbour(pNeighbour) != null && GetNeighbour(pNeighbour).IsLocalMax;
		}

		private CPointElement GetNeighbour(ENeigbour pNeighbour)
		{
			switch (pNeighbour)
			{
				case ENeigbour.Bot: return Bot;
				case ENeigbour.Left: return Left;
				case ENeigbour.Right: return Right;
				case ENeigbour.Top: return Top;
			}
			return null;
		}

		private ENeigbour GetOpositeNeighbour(ENeigbour pNeighbour)
		{
			switch (pNeighbour)
			{
				case ENeigbour.Bot: return ENeigbour.Top;
				case ENeigbour.Top: return ENeigbour.Bot;
				case ENeigbour.Left: return ENeigbour.Right;
				case ENeigbour.Right: return ENeigbour.Left;
			}
			return ENeigbour.None;
		}
	}

	public enum ENeigbour
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