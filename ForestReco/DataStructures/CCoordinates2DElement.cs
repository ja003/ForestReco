using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Win32.SafeHandles;

namespace ForestReco
{
	public class CCoordinates2DElement : CCoordinatesElement
	{
		private CCoordinatesDepthElement[] depthElements;
		//private int mostAddedDepthElementsIndex = -1;
		private bool storeDepthCoordinates;

		public CCoordinates2DElement leftNeighbor;
		public CCoordinates2DElement rightNeighbor;
		public CCoordinates2DElement topNeighbor;
		public CCoordinates2DElement botNeighbor;

		private int xPositionInField;
		private int yPositionInField;

		public bool IsLocalMax;
		public bool IsLocalMin;

		public CCoordinates2DElement(int pXPositionInField, int pYPositionInField, int pZDepth, bool pStoreDepthCoordinates)
		{
			xPositionInField = pXPositionInField;
			yPositionInField = pYPositionInField;
			storeDepthCoordinates = pStoreDepthCoordinates;
			if (!pStoreDepthCoordinates) { return; }

			depthElements = new CCoordinatesDepthElement[pZDepth];
			for (int i = 0; i < pZDepth; i++)
			{
				depthElements[i] = new CCoordinatesDepthElement();
			}
		}

		/// <summary>
		/// Returns weighted average of all stored heights.
		/// If storeDepthCoordinates = false, it returns 0
		/// </summary>
		/// <returns></returns>
		public float GetWeightedAverage()
		{
			if (!storeDepthCoordinates) { return 0; }
			float sum = 0;
			int count = 0;
			foreach (CCoordinatesDepthElement e in depthElements)
			{
				if (e.CoordinatesCount > 0)
				{
					sum += e.HeightSum;
					count += e.CoordinatesCount;
				}
			}
			if (count == 0) { return 0; }
			return sum / count;
		}


		protected override void OnAddCoordinate(Vector3 pCoordinate, int pZindex)
		{
			if (!storeDepthCoordinates) { return; }
			depthElements[pZindex].AddCoordinate(pCoordinate, pZindex);
			/*if (mostAddedDepthElementsIndex == -1 || 
				depthElements[pZindex].CoordinatesCount > depthElements[mostAddedDepthElementsIndex].CoordinatesCount)
			{
				mostAddedDepthElementsIndex = pZindex;
			}*/
		}

		public CCoordinates2DElement tree;
		public List<CCoordinates2DElement> subTree = new List<CCoordinates2DElement>();

		public void AssignTree(int pKernelSize)
		{
			//tree = GetMaxNeighbour();
			tree = GetTreeNeighbour(pKernelSize);
			tree?.AssignSubtree(this);
		}

		public void AssignSubtree(CCoordinates2DElement pSubtree)
		{
			if (tree != null)
			{
				if (tree == this)
				{
					pSubtree.tree = tree; //== this
					tree.subTree.Add(pSubtree);
					tree.subTree.AddRange(pSubtree.subTree);
					pSubtree.subTree.Clear(); //dont need anymore
					return;
				}
				tree.AssignSubtree(pSubtree);
			}
			else
			{
				subTree.Add(pSubtree);
				subTree.AddRange(pSubtree.subTree);
				pSubtree.subTree.Clear(); //dont need anymore
			}
		}

		private CCoordinates2DElement GetTreeNeighbour(int pKernelSize)
		{
			if (IsLocalMax) { return this;}
			//if (leftNeighbor != null && leftNeighbor.IsLocalMax) { return leftNeighbor; }
			//if (rightNeighbor != null && rightNeighbor.IsLocalMax) { return rightNeighbor; }
			//if (topNeighbor != null && topNeighbor.IsLocalMax) { return topNeighbor; }
			//if (botNeighbor != null && botNeighbor.IsLocalMax) { return botNeighbor; }
			if (pKernelSize > 0)
			{
				CCoordinates2DElement leftTree = leftNeighbor?.GetTreeNeighbour(pKernelSize - 1);
				if (leftTree != null) { return leftTree; }
				CCoordinates2DElement rightTree = rightNeighbor?.GetTreeNeighbour(pKernelSize - 1);
				if (rightTree != null) { return rightTree; }
				CCoordinates2DElement topTree = topNeighbor?.GetTreeNeighbour(pKernelSize - 1);
				if (topTree != null) { return topTree; }
				CCoordinates2DElement botTree = botNeighbor?.GetTreeNeighbour(pKernelSize - 1);
				if (botTree != null) { return botTree; }
			}
			return null;
		}

		private CCoordinates2DElement GetMaxNeighbour()
		{
			List<CCoordinates2DElement> elements = new List<CCoordinates2DElement>();
			elements.Add(this);
			if (leftNeighbor != null) { elements.Add(leftNeighbor); }
			if (rightNeighbor != null) { elements.Add(rightNeighbor); }
			if (topNeighbor != null) { elements.Add(topNeighbor); }
			if (botNeighbor != null) { elements.Add(botNeighbor); }
			return elements.OrderByDescending(x => x.HeightMax).First();
		}

		/// <summary>
		/// Returns height 10 if algorithm thinks its a tree.
		/// </summary>
		public float GetTreeHeight()
		{
			return 10 - GetDistanceToTree();

			if (IsLocalMax)
			{
				return 10 - GetDistanceToTree();
				//if (IsNearExtrema(false, pKernelSize)) { return 10; }
			}
			return 0;
		}

		private float GetDistanceToTree()
		{
			if (tree == null)
			{
				//Console.WriteLine(this + " has no tree assigned.");
				return 10;
			}
			Tuple<int, int> pos = GetPositionInField();
			Tuple<int, int> treePos = tree.GetPositionInField();
			return Vector2.Distance(new Vector2(pos.Item1, pos.Item2),
				new Vector2(treePos.Item1, treePos.Item2));
		}

		public Tuple<int, int> GetPositionInField()
		{
			return new Tuple<int, int>(xPositionInField, yPositionInField);
		}

		//..nonsense
		/*private CCoordinates2DElement GetNearExtrema(bool pMax, int pKernelSize)
		{
			if (pKernelSize > 1)
			{
				return GetNearExtrema(pMax, pKernelSize);
			}
			else
			{
				List<CCoordinates2DElement> elements = new List<CCoordinates2DElement>();
				elements.Add(this);
				elements.Add(leftNeighbor);
				elements.Add(rightNeighbor);
				elements.Add(topNeighbor);
				elements.Add(botNeighbor);
				return pMax ? elements.OrderByDescending(x => x.HeightMax).First() :
					elements.OrderByDescending(x => x.HeightMin).Last();
			}
		}*/

		public float GetHeight(EHeight pHeight)
		{
			switch (pHeight)
			{
				case EHeight.Tree: return GetTreeHeight();
				case EHeight.Average: return GetHeightAverage();
				case EHeight.Max: return GetHeightMax();
				case EHeight.Min: return GetHeightMin();
			}
			return 0;
		}

		public bool IsNeighbourhoodDefined(EHeight pHeight, int pKernelSize = 1)
		{
			if (pKernelSize > 1) { return IsNeighbourhoodDefined(pHeight, pKernelSize - 1); }
			return IsDefined(pHeight) ||
				IsNeighbourDefined(ENeighbour.Left, pHeight) ||
				IsNeighbourDefined(ENeighbour.Right, pHeight) ||
				IsNeighbourDefined(ENeighbour.Top, pHeight) ||
				IsNeighbourDefined(ENeighbour.Bot, pHeight);
		}

		public override bool IsDefined(EHeight pHeight = EHeight.None)
		{
			switch (pHeight)
			{
				case EHeight.Tree: return (int)GetTreeHeight() != 0;
			}
			return base.IsDefined(pHeight);
		}

		private bool IsNeighbourDefined(ENeighbour pNeighbour, EHeight pHeight)
		{
			switch (pNeighbour)
			{
				case ENeighbour.Left:
					return leftNeighbor == null || leftNeighbor.IsDefined(pHeight);
				case ENeighbour.Right:
					return rightNeighbor == null || rightNeighbor.IsDefined(pHeight);
				case ENeighbour.Top:
					return topNeighbor == null || topNeighbor.IsDefined(pHeight);
				case ENeighbour.Bot:
					return botNeighbor == null || botNeighbor.IsDefined(pHeight);
			}
			return false;
		}

		public override string ToString()
		{
			return "[" + xPositionInField + "," + yPositionInField + "]";
		}

		private bool IsNearExtrema(bool pMax, int pKernelSize)
		{
			if (pMax && IsLocalMax) { return true; }
			if (!pMax && IsLocalMin) { return true; }
			if (pKernelSize > 0)
			{
				return leftNeighbor.IsNearExtrema(pMax, pKernelSize - 1) ||
					   rightNeighbor.IsNearExtrema(pMax, pKernelSize - 1) ||
					   topNeighbor.IsNearExtrema(pMax, pKernelSize - 1) ||
					   botNeighbor.IsNearExtrema(pMax, pKernelSize - 1);
			}
			return false;
		}

		public float GetHeightLocalMaxima()
		{
			if (IsLocalMax) { return 10; }
			if (IsLocalMin) { return -10; }
			return 1;
		}

		private enum ENeighbour
		{
			None,
			Left,
			Right,
			Top,
			Bot
		}


	}
}