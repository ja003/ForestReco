using System;
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
			AddMaterial("invalid", .3f);

			AddMaterial("red", 1, 0, 0);
			AddMaterial("orange", 1, .5f, 0);
			AddMaterial("pink", 1, 0, .5f);
			AddMaterial("purple", .5f, 0, .5f);

			AddMaterial("green", 0, 1, 0);

			AddMaterial("blue", 0, 0, 1);
			AddMaterial("lightBlue", 0, .5f, 1);
		}

		private static void AddMaterial(string pName, float pColorIntensity)
		{
			Material red = new Material(pName);
			red.DiffuseReflectivity = new Color(pColorIntensity, pColorIntensity, pColorIntensity);
			materials.MaterialList.Add(red);
		}

		private static void AddMaterial(string pName, float pR, float pG, float pB)
		{
			Material red = new Material(pName);
			red.DiffuseReflectivity = new Color(pR, pG, pB);
			materials.MaterialList.Add(red);
		}

		public static string GetTreeMaterial(int pIndex)
		{
			int matIndex = pIndex % materials.MaterialList.Count;
			matIndex = Math.Max(1, matIndex); //todo: better management
			return materials.MaterialList[matIndex].Name;
		}

		public static string GetRefTreeMaterial(int pIndex)
		{
			int matIndex = pIndex % materials.MaterialList.Count;
			matIndex = Math.Max(1, matIndex);
			return materials.MaterialList[matIndex].Name;
		}

		public static string GetInvalidMaterial()
		{
			return materials.MaterialList[0].Name;
		}
	}
}