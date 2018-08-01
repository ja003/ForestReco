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
		private List<CPointElement> elements;

		public SVector3 botLeftCorner; //lower corner
		public SVector3 topRightCorner; //upper corner
		public double minHeight;
		public double maxHeight;

		private double stepSize;
		public int fieldXRange { get; }
		public int fieldYRange { get; }
		// ReSharper disable once NotAccessedField.Local
		private int coordinatesCount;

		private List<CPointElement> maximas = new List<CPointElement>();
		private List<CPointElement> minimas = new List<CPointElement>();

		private const float MIN_TREES_DISTANCE = 1; //meter

		//--------------------------------------------------------------

		public CPointField(CHeaderInfo pHeader, double pStepSize)
		{
			stepSize = pStepSize;
			botLeftCorner = pHeader.GetBotLeftCorner();
			topRightCorner = pHeader.GetTopRightCorner();

			minHeight = pHeader.GetMinHeight();
			maxHeight = pHeader.GetMaxHeight();

			double w = topRightCorner.X - botLeftCorner.X;
			double h = topRightCorner.Y - botLeftCorner.Y;

			//TODO: if not +2, GetPositionInField is OOR
			fieldXRange = (int)(w / pStepSize) + 2;
			fieldYRange = (int)(h / pStepSize) + 2;

			field = new CPointElement[fieldXRange, fieldYRange];
			elements = new List<CPointElement>();
			for (int x = 0; x < fieldXRange; x++)
			{
				for (int y = 0; y < fieldYRange; y++)
				{
					CPointElement newElement = new CPointElement(new Tuple<int, int>(x, y));
					field[x, y] = newElement;
					elements.Add(newElement);
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

		//TREE

		public void AssignTrees()
		{
			foreach (CPointElement m in maximas)
			{
				m.Tree = m;
				m.TreeElementsCount++;
			}
			Console.WriteLine("maximas = " + maximas.Count);

			foreach (CPointElement m in maximas)
			{
				m.AssignTreeToNeighbours();
				//Console.WriteLine(m + " = " + m.TreeElementsCount);
			}

			//Console.WriteLine("================");
			//Console.WriteLine("AssignTrees");

			//foreach (CPointElement e in elements)
			//{
			//	Console.WriteLine(e);
			//}
			//Console.WriteLine("================");
		}

		public void AssignTreesToAll()
		{
			Console.WriteLine("================");
			Console.WriteLine("AssignTreesToAll");

			foreach (CPointElement e in elements)
			{
				if (!e.HasAssignedTree() && e.GetHeight(EHeight.VegeMax) != null)
				{
					e.AssignTree(GetClosestTree(e));
				}
			}

			Console.WriteLine("================");
		}

		private CPointElement GetClosestTree(CPointElement pPoint)
		{
			CPointElement closestTree = maximas[0];
			int distanceToClosestTree = Int32.MaxValue;
			foreach (CPointElement m in maximas)
			{
				int distanceToTree = m.GetDistanceTo(pPoint);
				if (distanceToTree < distanceToClosestTree)
				{
					closestTree = m;
					distanceToClosestTree = distanceToTree;
				}
			}
			return closestTree;
		}

		///GETTER
		public CPointElement GetElement(int pX, int pY)
		{
			return field[pX, pY];
		}
		
		private Tuple<int, int> GetPositionInField(SVector3 pPoint)
		{
			int xPos = (int)((pPoint.X - botLeftCorner.X) / stepSize);
			//due to field orientation
			int yPos = fieldYRange - (int)((pPoint.Y - botLeftCorner.Y) / stepSize) - 1;
			//int yPos = (int)((pPoint.Y - controller.botLeftCorner.Y) / stepSize);
			return new Tuple<int, int>(xPos, yPos);
		}
		
		private SVector3 GetCenterOffset()
		{
			return new SVector3(fieldXRange / 2f * stepSize, fieldYRange / 2f * stepSize);
		}

		/// <summary>
		/// Calculates appropriate kernel size base on step size
		/// </summary>
		private int GetKernelSize()
		{
			return (int)(MIN_TREES_DISTANCE / stepSize);
		}

		//PUBLIC

		public void AddPointInField(int pClass, SVector3 pPoint)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			field[index.Item1, index.Item2].AddPoint(pClass, pPoint);
			//Console.WriteLine(index + " = " + pPoint);
			coordinatesCount++;
		}

		public void CalculateLocalExtrems()
		{
			CalculateLocalExtrems(GetKernelSize());
		}

		public void FillMissingHeights(EHeight pHeight)
		{
			for (int x = 0; x < fieldXRange; x++)
			{
				for (int y = 0; y < fieldYRange; y++)
				{
					CPointElement el = field[x, y];
					el.FillMissingHeight(pHeight);
				}
			}
		}

		///PRIVATE
		
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
					CPointElement el = field[x, y];
					if (el.IsDefined(EClass.Vege))
					{
						bool isMaximum = field[x, y].CalculateLocalExtrem(true, pKernelSize);
						bool isMinimum = field[x, y].CalculateLocalExtrem(false, pKernelSize);
						if (isMaximum) { this.maximas.Add(el); }
						if (isMinimum) { this.minimas.Add(el); }
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
		
		//OTHER
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
		
		public override string ToString()
		{
			return "Field " + fieldXRange + " x " + fieldYRange;
		}

	}
}