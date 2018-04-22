using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CCoordinates2DElement : CCoordinatesElement
	{
		private CCoordinatesDepthElement[] depthElements;
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


		protected override void OnAddCoordinate(Vector3 pCoordinate, int pZindex)
		{
			if (!storeDepthCoordinates) { return; }
			depthElements[pZindex].AddCoordinate(pCoordinate, pZindex);
		}
	}
}