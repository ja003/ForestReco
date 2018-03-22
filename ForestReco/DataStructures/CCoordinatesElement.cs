using System.Numerics;

namespace ForestReco
{
	public class CCoordinatesElement
	{
		public float HeightSum;
		public int CoordinatesCount;

		public void AddCoordinate(Vector3 pCoordinate)
		{
			HeightSum += pCoordinate.Y;
			CoordinatesCount++;
		}

		public float? GetAverageHeight()
		{
			if (CoordinatesCount == 0)
			{
				return null;
			}
			else
			{
				return HeightSum / CoordinatesCount;
			}
		}
	}
}