using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public static class CMaterialManager
	{
		public static Mtl materials;

		private static Dictionary<EMaterial, List<int>> materialSet = new Dictionary<EMaterial, List<int>>();

		public static void Init()
		{
			materials = new Mtl("colors");
			AddMaterial("invalid", .3f, EMaterial.Invalid);
			AddMaterial("alarm", 1, 0, 0, EMaterial.Alarm);
			AddMaterial("fake", 1, 0, 1, EMaterial.Fake);
			AddMaterial("checkTree", 0, 0, 1, EMaterial.CheckTree);

			AddTreeMaterial("red", 1, 0, 0);
			AddTreeMaterial("orange", 1, .5f, 0);
			AddTreeMaterial("pink", 1, 0, .5f);
			AddTreeMaterial("purple", .5f, 0, .5f);

			AddTreeMaterial("green", 0, 1, 0);

			AddTreeMaterial("lightBlue", 0, .5f, 1);
		}

		private static void AddMaterial(string pName, float pColorIntensity, EMaterial pType = EMaterial.None)
		{
			AddMaterial(pName, pColorIntensity, pColorIntensity, pColorIntensity, pType);
		}

		private static void AddTreeMaterial(string pName, float pR, float pG, float pB)
		{
			AddMaterial(pName, pR, pG, pB, EMaterial.Tree);
		}


		private static void AddMaterial(string pName, float pR, float pG, float pB, EMaterial pType = EMaterial.None)
		{
			Material mat = new Material(pName);
			mat.DiffuseReflectivity = new Color(pR, pG, pB);
			materials.MaterialList.Add(mat);
			int newMatIndex = materials.MaterialList.Count - 1;
			if (pType != EMaterial.None)
			{
				if (materialSet.ContainsKey(pType))
				{
					materialSet[pType].Add(newMatIndex);
				}
				else
				{
					materialSet.Add(pType, new List<int> { newMatIndex });
				}
			}
		}

		public static string GetTreeMaterial(int pIndex)
		{
			List<int> treeIndexes = materialSet[EMaterial.Tree];
			int matIndex = pIndex % treeIndexes.Count;

			return materials.MaterialList[treeIndexes[matIndex]].Name;
		}

		public static string GetRefTreeMaterial(int pIndex)
		{
			return materials.MaterialList[materialSet[EMaterial.RefTree][0]].Name;
		}

		public static string GetInvalidMaterial()
		{
			return materials.MaterialList[materialSet[EMaterial.Invalid][0]].Name;
		}

		public static string GetFakeMaterial()
		{
			return materials.MaterialList[materialSet[EMaterial.Fake][0]].Name;
		}

		public static string GetAlarmMaterial()
		{
			return materials.MaterialList[materialSet[EMaterial.Alarm][0]].Name;
		}

		public static string GetCheckTreeMaterial()
		{
			return materials.MaterialList[materialSet[EMaterial.CheckTree][0]].Name;
		}
	}

	public enum EMaterial
	{
		None,
		Tree,
		RefTree,
		CheckTree,
		Invalid,
		Fake,
		Alarm
	}
}