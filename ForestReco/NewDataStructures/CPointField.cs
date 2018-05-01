using System;
using System.Diagnostics;
using System.Numerics;

namespace ForestReco
{
	public class CPointField
	{
		private CPointElement[,] field;
		private CPointFieldController controller;

		private float stepSize;
		public int fieldXRange { get; }
		public int fieldYRange { get; }
		private int coordinatesCount;

		public CPointField(CPointFieldController pController, float pStepSize)
		{
			controller = pController;
			stepSize = pStepSize;

			float w = pController.topRightCorner.X - pController.botLeftCorner.X;
			float h = pController.topRightCorner.Y - pController.botLeftCorner.Y;

			fieldXRange = (int)(w / pStepSize) + 1;
			fieldYRange = (int)(h / pStepSize) + 1;
			
			field = new CPointElement[fieldXRange, fieldYRange];
			for (int x = 0; x < fieldXRange; x++)
			{
				for (int y = 0; y < fieldYRange; y++)
				{
					field[x, y] = new CPointElement();//x, y);
				}
			}
			for (int x = 0; x < fieldXRange; x++)
			{
				for (int y = 0; y < fieldYRange; y++)
				{
					if (x > 0) { field[x, y].Left = field[x - 1, y]; }
					if (x < fieldXRange - 1) { field[x, y].Right = field[x + 1, y]; }
					if (y > 0) { field[x, y].Top = field[x, y - 1]; }
					if (y < fieldYRange - 1) { field[x, y].Bot = field[x, y + 1]; }
				}
			}
		}

		public void AddPointInField(Vector3 pPoint)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			field[index.Item1, index.Item2].AddPoint(pPoint);
			//Console.WriteLine(index + " = " + pPoint);
			coordinatesCount++;
		}

		/// <summary>
		/// Returns string for x coordinate in field moved by offset
		/// </summary>
		public string GetXElementString(int pXindex)
		{
			return (pXindex * stepSize - GetCenterOffset().X).ToString();
		}

		/// <summary>
		/// Returns string for y coordinate in field moved by offset
		/// </summary>
		public string GetYElementString(int pYindex)
		{
			//TODO: not sure why I have to use '-pY' and '+GetCenterOffset'. 
			//But result doesn't match the original file without it
			return (-pYindex * stepSize + GetCenterOffset().Y).ToString();
		}

		public CPointElement GetElement(int pX, int pY)
		{
			return field[pX, pY];
		}

		private Tuple<int, int> GetPositionInField(Vector3 pPoint)
		{
			int xPos = (int)((pPoint.X - controller.botLeftCorner.X) / stepSize);
			int yPos = (int)((pPoint.Y - controller.botLeftCorner.Y) / stepSize);
			return new Tuple<int, int>(xPos, yPos);
		}

		private Vector2 GetCenterOffset()
		{
			return new Vector2(fieldXRange / 2f * stepSize, fieldYRange / 2f * stepSize);
		}
	}
}