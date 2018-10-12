using System.Numerics;
using ObjParser;

namespace ForestReco
{
	public class CCheckTree
	{
		public EClass treeClass { get; }
		public Vector3 position { get; }
		public int index { get; }

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
				CObjExporter.AddLineToObj(ref obj, position + Vector3.UnitY*30, position);
				return obj;
			}
		}

		public override string ToString()
		{
			return treeClass + " : " + position;
		}

		public enum EClass
		{
			None = 0,
			Smrk = 11,
			Jedle = 12,
			Buk = 13
		}
	}
}