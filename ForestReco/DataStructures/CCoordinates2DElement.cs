using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CCoordinates2DElement : CCoordinatesElement
	{
		private CCoordinatesDepthElement[] depthElements;
		//private int mostAddedDepthElementsIndex = -1;
		private bool storeDepthCoordinates;

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

		/// <summary>
		/// Returns average height from depth field with the biggest number of coordinates
		/// TODO: almost same results as with Max heights. DELETE 
		/// </summary>
		/// <returns></returns>
		/*public float GetMostAddedHeightAverage()
		{
			if (!storeDepthCoordinates) { return 0; }

			if (mostAddedDepthElementsIndex != -1)
			{
				return depthElements[mostAddedDepthElementsIndex].GetAverageHeight();
			}
			return 0;
		}*/

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
	}
}