using System.Collections.Generic;
using System.Numerics;

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
		/// Tree isLocalMax and localMin is in close neighbourhood
		/// </summary>
		public float GetTreeHeight(int pKernelSize)
		{
			if (IsLocalMax)
			{
				if (IsNearExtrema(false, pKernelSize)) { return 10; }
			}
			return 1;
		}

		public bool IsNearExtrema(bool pMax, int pKernelSize)
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
	}
}