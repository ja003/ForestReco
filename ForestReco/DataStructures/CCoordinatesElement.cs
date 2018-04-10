using System.Numerics;

namespace ForestReco
{
	public class CCoordinatesElement
	{
		public float HeightSum;
		public int CoordinatesCount;
		public int VertexIndex;

		public CCoordinatesElement()
		{
			this.VertexIndex = -1;
		}

		public void AddCoordinate(Vector3 pCoordinate)
		{
			HeightSum += pCoordinate.Z;
			CoordinatesCount++;
		}

		public float? GetAverageHeight()
		{
			if (CoordinatesCount == 0)
			{
				return 0;
				return null;
			}
			else
			{
				return HeightSum / CoordinatesCount;
			}
		}
	}
}