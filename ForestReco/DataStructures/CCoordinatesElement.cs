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
	}
}