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
	public class CPointArray
	{
		private CPointField[,] array;
		private List<CPointField> fields;

		private double stepSize;
		public int arrayXRange { get; }
		public int arrayYRange { get; }
		// ReSharper disable once NotAccessedField.Local
		private int coordinatesCount;

		private List<CPointField> maximas = new List<CPointField>();
		public List<CPointField> Maximas => maximas;
		private List<CPointField> minimas = new List<CPointField>();

		private const float MIN_TREES_DISTANCE = 1; //meter

		SVector3 botLeftCorner;
		SVector3 topRightCorner;

		//--------------------------------------------------------------

		public CPointArray(double pStepSize)
		{
			stepSize = pStepSize;

			botLeftCorner = CProjectData.header.BotLeftCorner;
			topRightCorner = CProjectData.header.TopRightCorner;

			double w = topRightCorner.X - botLeftCorner.X;
			double h = topRightCorner.Y - botLeftCorner.Y;

			//TODO: if not +2, GetPositionInField is OOR
			arrayXRange = (int)(w / pStepSize) + 2;
			arrayYRange = (int)(h / pStepSize) + 2;

			array = new CPointField[arrayXRange, arrayYRange];
			fields = new List<CPointField>();
			for (int x = 0; x < arrayXRange; x++)
			{
				for (int y = 0; y < arrayYRange; y++)
				{
					CPointField newPointField = new CPointField(new Tuple<int, int>(x, y));
					array[x, y] = newPointField;
					fields.Add(newPointField);
				}
			}
			for (int x = 0; x < arrayXRange; x++)
			{
				for (int y = 0; y < arrayYRange; y++)
				{
					if (x > 0) { array[x, y].Left = array[x - 1, y]; }
					if (x < arrayXRange - 1) { array[x, y].Right = array[x + 1, y]; }
					if (y > 0) { array[x, y].Top = array[x, y - 1]; }
					if (y < arrayYRange - 1) { array[x, y].Bot = array[x, y + 1]; }
				}
			}
		}

		//TREE

		public void AssignTreesToNeighbourFields()
		{
			foreach (CPointField m in maximas)
			{
				m.AssignTree(m);
			}
			Console.WriteLine("maximas = " + maximas.Count);

			foreach (CPointField m in maximas)
			{
				m.AssignTreeToNeighbours();
				//Console.WriteLine(m + " = " + m.TreeFieldCount);
			}

			//Console.WriteLine("================");
			//Console.WriteLine("AssignTreesToNeighbourFields");

			//foreach (CPointField e in fields)
			//{
			//	Console.WriteLine(e);
			//}
			//Console.WriteLine("================");
		}

		public void AssignTreesToAllFields()
		{
			Console.WriteLine("================");
			Console.WriteLine("AssignTreesToAllFields");

			foreach (CPointField e in fields)
			{
				if (!e.HasAssignedTree() && e.GetHeight(EHeight.VegeMax) != null)
				{
					e.AssignTree(GetClosestTree(e));
				}
			}

			Console.WriteLine("================");
		}

		private CPointField GetClosestTree(CPointField pPointField)
		{
			CPointField closestTree = maximas[0];
			int distanceToClosestTree = Int32.MaxValue;
			foreach (CPointField m in maximas)
			{
				int distanceToTree = m.GetDistanceTo(pPointField);
				if (distanceToTree < distanceToClosestTree)
				{
					closestTree = m;
					distanceToClosestTree = distanceToTree;
				}
			}
			return closestTree;
		}

		public void AssignPointsToTrees()
		{
			Console.WriteLine("AssignPointsToTrees");

			foreach (CPointField t in maximas)
			{
				foreach (CPointField treeField in t.TreeFields)
				{
					treeField.AssignPointsToTree();
				}
				//Console.WriteLine(t + " has " + t.TreePoints.Count + " tree points.");
			}
		}

		///GETTER
		public CPointField GetElement(int pXindex, int pYindex)
		{
			return array[pXindex, pYindex];
		}

		public CPointField GetElementContainingPoint(SVector3 pPoint)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			return array[index.Item1, index.Item2];
		}

		private Tuple<int, int> GetPositionInField(SVector3 pPoint)
		{
			int xPos = (int)((pPoint.X - botLeftCorner.X) / stepSize);
			//due to array orientation
			int yPos = arrayYRange - (int)((pPoint.Y - botLeftCorner.Y) / stepSize) - 1;
			//int yPos = (int)((pPointField.Y - controller.botLeftCorner.Y) / stepSize);
			return new Tuple<int, int>(xPos, yPos);
		}

		public SVector3 GetCenterOffset()
		{
			return new SVector3(arrayXRange / 2f * stepSize, arrayYRange / 2f * stepSize);
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
			array[index.Item1, index.Item2].AddPoint(pClass, pPoint);
			//Console.WriteLine(index + " = " + pPointField);
			coordinatesCount++;
		}

		public void CalculateLocalExtrems()
		{
			CalculateLocalExtrems(GetKernelSize());
		}

		public void FillMissingHeights(EHeight pHeight)
		{
			foreach (CPointField el in fields)
			{
				if (!el.IsDefined(pHeight))
				{
					el.FillMissingHeight(pHeight);
				}
			}
		}

		public bool IsAllDefined(EHeight pHeight)
		{
			foreach (CPointField f in fields)
			{
				if (!f.IsDefined(pHeight)) { return false; }
			}
			return true;
		}

		///PRIVATE

		/// <summary>
		/// Marks all fields in array with min/max mark
		/// </summary>
		private void CalculateLocalExtrems(int pKernelSize)
		{
			if (pKernelSize < 1)
			{
				Console.WriteLine("Kernel cant be < 1!");
				pKernelSize = 1;
			}
			for (int x = 0; x < arrayXRange; x++)
			{
				for (int y = 0; y < arrayYRange; y++)
				{
					CPointField el = array[x, y];
					if (el.IsDefined(EClass.Vege))
					{
						bool isMaximum = array[x, y].CalculateLocalExtrem(true, pKernelSize);
						bool isMinimum = array[x, y].CalculateLocalExtrem(false, pKernelSize);
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
		/// Returns string for x coordinate in array moved by offset
		/// </summary>
		public double GetFieldXCoord(int pXindex)
		{
			return pXindex * stepSize - GetCenterOffset().X;
		}

		/// <summary>
		/// Returns string for y coordinate in array moved by offset
		/// </summary>
		public double GetFieldYCoord(int pYindex)
		{
			return pYindex * stepSize - GetCenterOffset().Y;
		}

		public override string ToString()
		{
			return "Field " + arrayXRange + " x " + arrayYRange;
		}


	}
}