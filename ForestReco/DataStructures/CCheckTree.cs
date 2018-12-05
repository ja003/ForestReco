using System;
using System.Collections.Generic;
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
				CDebug.Error($"{this} has no ground field assigned", false);
				return;
			}
			if (!groundField.HasAllNeighbours())
			{
				//CDebug.Error($"{this} is at the border");
				isInvalid = true;
				return;
			}

			const int minVegePointsPerField = 10;

			int vegePointsCount = 0;
			int undefinedNeighboursCount = 0;
			int smallHeightCount = 0;
			List<CGroundField> neighbours = groundField.GetNeighbours(true);

			foreach (CGroundField neighbour in neighbours)
			{
				int neighbourVegeCount = neighbour.vegePoints.Count;
				if (neighbourVegeCount < minVegePointsPerField)
				{
					undefinedNeighboursCount++;
				}
				float? vegeHeight = neighbour.MaxPreProcessVege - neighbour.GetHeight();
				if (vegeHeight < CTreeManager.AVERAGE_TREE_HEIGHT)
				{
					smallHeightCount++;
				}

				vegePointsCount += neighbourVegeCount;
			}
			//if there is a tree, almost all of neighbours should have enough points
			if (undefinedNeighboursCount > 1)
			{
				isInvalid = true;
				return;
			}
			//not many fields ahve height enough for tree to be there
			if (smallHeightCount > 5)
			{
				isInvalid = true;
				return;
			}

			//measure minimal required point count
			int minTotalPointsCount = minVegePointsPerField * groundField.GetNeighbours().Count;
			isInvalid = vegePointsCount < minTotalPointsCount;
		}
	}
}