using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace ForestReco
{
	/// <summary>
	/// Field orientation is from topLeft -> botRight, topLeft = [0,0]
	/// </summary>
	public class CPointField
	{
		private CPointElement[,] field;
		private CPointFieldController controller;

		private float stepSize;
		public int fieldXRange { get; }
		public int fieldYRange { get; }
		private int coordinatesCount;

		private List<Tuple<int, int>> maximas = new List<Tuple<int, int>>();
		private List<Tuple<int, int>> minimas = new List<Tuple<int, int>>();

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
					field[x, y] = new CPointElement(new Tuple<int, int>(x, y));//x, y);
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

		public void AddPointInField(int pClass, Vector3 pPoint)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			field[index.Item1, index.Item2].AddPoint(pClass, pPoint);
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
			return (pYindex * stepSize - GetCenterOffset().Y).ToString();
		}

		public CPointElement GetElement(int pX, int pY)
		{
			return field[pX, pY];
		}

		private Tuple<int, int> GetPositionInField(Vector3 pPoint)
		{
			int xPos = (int)((pPoint.X - controller.botLeftCorner.X) / stepSize);
			//due to field orientation
			int yPos = fieldYRange - (int)((pPoint.Y - controller.botLeftCorner.Y) / stepSize) - 1;
			return new Tuple<int, int>(xPos, yPos);
		}

		private Vector2 GetCenterOffset()
		{
			return new Vector2(fieldXRange / 2f * stepSize, fieldYRange / 2f * stepSize);
		}

		private const float MIN_TREES_DISTANCE = 1; //meter

		public void CalculateLocalExtrems()
		{
			CalculateLocalExtrems(GetKernelSize());
		}

		public void AssignTrees()
		{
			for (int i = 0; i < maximas.Count; i++)
			{
				field[maximas[i].Item1, maximas[i].Item2].TreeIndex = i;
			}

			foreach (Tuple<int, int> m in maximas)
			{
				field[m.Item1, m.Item2].AssignTreeToNeighbours();
			}

			int[] treeElements = new int[maximas.Count];
			for (int x = 0; x < fieldXRange; x++)
			{
				for (int y = 0; y < fieldYRange; y++)
				{
					if (field[x, y].TreeIndex != -1)
					{
						treeElements[field[x, y].TreeIndex]++;
					}
				}
			}

			for (int i = 0; i < treeElements.Length; i++)
			{
				Console.WriteLine(i + " = " + treeElements[i]);
			}
		}


		/// <summary>
		/// Marks all elements in field with min/max mark
		/// </summary>
		private void CalculateLocalExtrems(int pKernelSize)
		{
			if (pKernelSize < 1)
			{
				Console.WriteLine("Kernel cant be < 1!");
				pKernelSize = 1;
			}
			for (int x = 0; x < fieldXRange; x++)
			{
				for (int y = 0; y < fieldYRange; y++)
				{
					if (field[x, y].IsDefined(EClass.Vege))
					{
						bool isMaximum = field[x, y].CalculateLocalExtrem(true, pKernelSize);
						bool isMinimum = field[x, y].CalculateLocalExtrem(false, pKernelSize);
						if (isMaximum){ this.maximas.Add(new Tuple<int, int>(x, y));}
						if (isMinimum) { this.minimas.Add(new Tuple<int, int>(x, y)); }
					}
				}
			}

			Console.WriteLine("Maximas:" + maximas.Count);
			Console.WriteLine("Minimas:" + minimas.Count);

			/*foreach (Tuple<int, int> m in maximas)
			{
				Console.WriteLine(m);
			}

			foreach (Tuple<int, int> m in minimas)
			{
				Console.WriteLine(m);
			}*/
		}

		/// <summary>
		/// Calculates appropriate kernel size base on step size
		/// </summary>
		private int GetKernelSize()
		{
			return (int)(MIN_TREES_DISTANCE / stepSize);
		}

		public override string ToString()
		{
			return "Field " + fieldXRange + " x " + fieldYRange;
		}
	}
}