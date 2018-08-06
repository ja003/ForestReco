using System;
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

		private List<SVector3> pointsVege = new List<SVector3>(); //high vegetation (class 5)
		private List<SVector3> pointsGround = new List<SVector3>(); //ground (class 1)
		
		public double? MinVege;
		public double? MaxVege; //todo: delete and replace with MaxVegePoint
		public double? SumVege;
		public SVector3 MaxVegePoint;

		public double? MinGround;
		public double? MaxGround;
		public double? SumGround;


		public bool IsLocalMax;
		public bool IsLocalMin;
		public int VertexIndex = -1;

		private readonly Tuple<int, int> indexInField;

		public double? TreeHeight;
		public CPointField Tree; //tree, which this point belongs to.
		public List<CPointField> TreeFields = new List<CPointField>();

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

			if (this.Equals(new CPointField(new Tuple<int, int>(13, 23))))
			{
				Console.Write("!");
			}

			foreach (CPointField n in GetNeighbours())
			{
				//already belongs to other tree
				if (!n.HasAssignedTree())
				{
					double? height = GetHeight(EHeight.VegeMax);
					double? neighbourHeight = n.GetHeight(EHeight.VegeMax) ?? n.GetHeight(EHeight.GroundMax);

					if (height != null && neighbourHeight != null)
					{
						double heightDiff = (double)height - (double)neighbourHeight;
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

		public void AssignPointsToTree()
		{
			foreach (SVector3 vegePoint in pointsVege)
			{
				Tree.AssignPointToTree(vegePoint);
			}
		}

		private void AssignPointToTree(SVector3 pVegePoint)
		{
			if (!Tree.Equals(this))
			{
				Console.WriteLine("Error. You are trying to assing point " + pVegePoint + " to field " + this + ", which is not tree.");
				return;
			}

		}

		public bool HasAssignedTree()
		{
			return Tree != null;
		}

		private double? GetTreeHeight()
		{
			if (TreeHeight == null)
			{
				if (HasAssignedTree())
				{
					double? heightTree = Tree.MaxVege;
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
			return TreeHeight;
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
			if (neighbours != null) { return this.neighbours;}

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

		public void AddPoint(int pClass, SVector3 pPoint)
		{
			double height = pPoint.Z;

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
			//if(indexInField == null){ Console.WriteLine("!");}
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
		
		public double? GetAverageHeightFromClosestDefined(EHeight pHeight)
		{
			if (this.Equals(new CPointField(new Tuple<int, int>(10, 2))))
			{
				Console.Write("!");
			}

			if (IsDefined(pHeight)) { return GetHeight(pHeight); }
			//
			CPointField closestFirst = GetClosestDefined(pHeight, EDirection.Left);
			CPointField closestSecond = GetClosestDefined(pHeight, EDirection.Right);
			//
			if (closestFirst == null || closestSecond == null)
			{
				closestFirst = GetClosestDefined(pHeight, EDirection.Top);
				closestSecond = GetClosestDefined(pHeight, EDirection.Bot);
			}

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
				double? heightDiff = higher.GetHeight(pHeight) - smaller.GetHeight(pHeight);
				if (heightDiff != null)
				{
					double? smallerHeight = smaller.GetHeight(pHeight);
					float distanceToSmaller = GetDistanceTo(smaller);
					return smallerHeight + distanceToSmaller / totalDistance * heightDiff;
				}
			}
			return null;
		}

		/// <summary>
		/// Returns height of given type.
		/// pGetHeightFromNeighbour: True = ifNotDefined => closest defined height will be used (runs DFS)
		/// pVisited: dont use these points in DFS
		/// </summary>
		public double? GetHeight(EHeight pHeight, bool pGetHeightFromNeighbour = false,
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
			}
			return null;
		}
		
		public int GetDistanceTo(CPointField pPointField)
		{
			return Math.Abs(indexInField.Item1 - pPointField.indexInField.Item1) +
			       Math.Abs(indexInField.Item2 - pPointField.indexInField.Item2);
		}
		
		public void FillMissingHeight(EHeight pHeight)
		{
			if (IsDefined(pHeight)) { return; }
			switch (pHeight)
			{
				case EHeight.GroundMax:
					MaxGround = GetAverageHeightFromClosestDefined(pHeight);
					break;
				default:
					Console.WriteLine("FillMissingHeight not defined for " + pHeight);
					return;
			}
		}

		///PRIVATE

		private CPointField GetClosestDefined(EHeight pHeight, EDirection pDirection)
		{
			if (IsDefined(pHeight)) { return this; }
			return GetNeighbour(pDirection)?.GetClosestDefined(pHeight, pDirection);
		}

		/// <summary>
		/// Returns extrem of given class.
		/// pMax: True = maximum, False = minimum
		/// </summary>
		private double? GetHeightExtrem(bool pMax, EClass pClass)
		{
			switch (pClass)
			{
				case EClass.Ground: return pMax ? MaxGround : MinGround;
				case EClass.Vege: return pMax ? MaxVege : MinVege;
			}
			return null;
		}
		
		private double? GetHeightAverage(EClass pClass)
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
			return ToStringIndex() + " Tree = " + (Tree?.ToStringIndex() ?? "null");
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