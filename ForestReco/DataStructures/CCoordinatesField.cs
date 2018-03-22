using System;
using System.Numerics;

namespace ForestReco
{
	public class CCoordinatesField
	{
		private CCoordinatesElement[,] field;
		private Vector3 min;
		private Vector3 max;
		private float stepSize;

		private int coordinatesCount;

		public CCoordinatesField(Vector3 pMin, Vector3 pMax, float pStepSize)
		{
			min = pMin;
			max = pMax;
			stepSize = pStepSize;
			float width = max.X - min.X;
			float height = max.Z - min.Z;
			int stepsX = (int)(width / pStepSize) + 1;
			int stepsZ = (int)(height / pStepSize) + 1;
			field = new CCoordinatesElement[stepsX, stepsZ];
			for (int i = 0; i < stepsX; i++)
			{
				for (int j = 0; j < stepsZ; j++)
				{
					field[i, j] = new CCoordinatesElement();
				}
			}
		}

		public void AddCoordinate(Vector3 pCoordinate)
		{
			Tuple<int, int> index = GetPositionInField(pCoordinate);
			field[index.Item1, index.Item2].AddCoordinate(pCoordinate);
			coordinatesCount++;
			if (coordinatesCount % 1000 == 0)
			{
				Console.WriteLine(index.Item1 + "," + index.Item2 + " = " + pCoordinate);
			}
		}

		public override string ToString()
		{
			return "FIELD[" + coordinatesCount + "]. min = " + min + ", max = " + max + ", stepSize = " + stepSize;
		}

		private Tuple<int, int> GetPositionInField(Vector3 pCoordinate)
		{
			int xPos = (int)((pCoordinate.X - min.X) / stepSize);
			int yPos = (int)((pCoordinate.Z - min.Z) / stepSize);
			return new Tuple<int, int>(xPos, yPos);
		}
	}
}