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
			Obj obj = new Obj("checkTree_" + index);
			{
				float offsetHeight = 30;
				if (assignedTree != null)
				{
					offsetHeight = assignedTree.GetTreeHeight() + 1;
					CObjExporter.AddLineToObj(ref obj, position + Vector3.UnitY * offsetHeight, assignedTree.peak.Center);
				}
				CObjExporter.AddLineToObj(ref obj, position + Vector3.UnitY * offsetHeight, position);

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
	}
}