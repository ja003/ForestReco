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


		public bool IsLocalMax;
		public bool IsLocalMin;

		public CCoordinates2DElement(int pZDepth, bool pStoreDepthCoordinates)
		{
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

		/// <summary>
		/// Returns height 10 if algorithm thinks its a tree.
		/// </summary>
		public float GetTreeHeight()
		{
			if (IsLocalMax)
			{
				return 10;
				//if (IsNearExtrema(false, pKernelSize)) { return 10; }
			}
			return 0;
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