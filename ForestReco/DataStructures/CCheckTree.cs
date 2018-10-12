using System.Numerics;

namespace ForestReco
{
	public class CCheckTree
	{
		private EClass treeClass;
		private Vector3 position;

		public CCheckTree(int pTreeClass, Vector3 pPosition)
		{
			this.treeClass = (EClass)pTreeClass;
			this.position = pPosition;
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