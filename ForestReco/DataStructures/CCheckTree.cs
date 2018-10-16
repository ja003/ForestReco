using System;
using System.Numerics;
using ForestReco;
using ObjParser;

namespace ForestReco
{
	public class CCheckTree
	{
		public EClass treeClass { get; }
		public Vector3 position { get; }
		public int index { get; }
		public CTree assignedTree { get; private set; }

		public CCheckTree(int pTreeClass, Vector3 pPosition, int pIndex)
		{
			treeClass = (EClass)pTreeClass;
			position = pPosition;
			index = pIndex;
		}

		public Obj GetObj()
		{
			string name = "checkTree";
			if (assignedTree == null)
			{
				name += isInvalid ? "_[invalid]" : "_[unassigned";
			}

			Obj obj = new Obj(name + "_" + index);
			{
				int lineWidthMultiply = 3;
				float offsetHeight = 30;
				if (assignedTree != null)
				{
					offsetHeight = assignedTree.GetTreeHeight() + 1;
					CObjExporter.AddLineToObj(ref obj, position + Vector3.UnitY * offsetHeight,
						assignedTree.peak.Center, lineWidthMultiply);
				}
				CObjExporter.AddLineToObj(ref obj, position + Vector3.UnitY * offsetHeight, position, lineWidthMultiply);

				return obj;
			}
		}

		public override string ToString()
		{
			return index + " - " + treeClass + " : " + position + (assignedTree == null ? "" : "+");
		}

		public enum EClass
		{
			None = 0,
			Smrk = 11,
			Jedle = 12,
			Buk = 13
		}

		public void AssignTree(CTree pTree)
		{
			assignedTree = pTree;
			if (pTree != null) { pTree.assignedCheckTree = this; }
			//CDebug.WriteLine("Assign to " + this);
		}

		public CGroundField groundField;

		public bool isInvalid;

		public void Validate()
		{
			if (assignedTree != null)
			{
				return;
			}

			//groundField not assigned = checkTree is out of this array 
			if (groundField == null)
			{
				return;
			}

			if (index == 216)
			{
				Console.WriteLine();
			}

			int count = groundField.vegePoints.Count;

			const int minVegePointsPerField = 10;
			if (count < minVegePointsPerField)
			{
				isInvalid = true;
				return;
			}

			foreach (CGroundField neighbour in groundField.GetNeighbours())
			{
				int neighbourVegeCount = neighbour.vegePoints.Count;
				if (neighbourVegeCount < minVegePointsPerField)
				{
					isInvalid = true;
					return;
				}
				count += neighbourVegeCount;
			}
			int minTotalPointsCount = minVegePointsPerField * groundField.GetNeighbours().Count;
			isInvalid = count < minTotalPointsCount;
		}
	}
}