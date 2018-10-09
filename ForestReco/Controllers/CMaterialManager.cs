using System.Runtime.CompilerServices;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public static class CMaterialManager
	{
		public static Mtl materials;

		public static void Init()
		{
			materials = new Mtl("colors");

			Material red = new Material("red");
			red.DiffuseReflectivity = new Color(1, 0, 0);
			materials.MaterialList.Add(red);

			Material green = new Material("green");
			green.DiffuseReflectivity = new Color(0, 1, 0);
			materials.MaterialList.Add(green);

			Material blue = new Material("blue");
			blue.DiffuseReflectivity = new Color(0, 0, 1);
			materials.MaterialList.Add(blue);
		}

		public static string GetMaterial(CTree pTree)
		{
			return materials.MaterialList[0].Name;
		}

		public static string GetRefTreeMaterial(int pIndex)
		{
			int matIndex = pIndex % materials.MaterialList.Count;
			return materials.MaterialList[matIndex].Name;
		}
	}
}