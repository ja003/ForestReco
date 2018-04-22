using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public class CCoordinates2DElement : CCoordinatesElement
	{
		private CCoordinatesDepthElement[] depthElements;

		public CCoordinates2DElement(int pZDepth)
		{
			depthElements = new CCoordinatesDepthElement[pZDepth];
			for (int i = 0; i < pZDepth; i++)
			{
				depthElements[i] = new CCoordinatesDepthElement();
			}
		}


		protected override void OnAddCoordinate(Vector3 pCoordinate, int pZindex)
		{
			depthElements[pZindex].AddCoordinate(pCoordinate, pZindex);
		}
	}
}