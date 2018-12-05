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

		private static Dictionary<EMaterial, List<int>> materialSet;

		private static bool useTreeMaterial;

		public static void Init()
		{
			useTreeMaterial = CParameterSetter.GetBoolSettings(ESettings.colorTrees);

			materialSet = new Dictionary<EMaterial, List<int>>();

			materials = new Mtl("colors");
			AddMaterial("invalid", .3f, EMaterial.Invalid);
			AddMaterial("alarm", 1, 0, 0, EMaterial.Alarm);
			AddMaterial("fake", 1, 0, 1, EMaterial.Fake);
			AddMaterial("checkTree", 0, 0, 1, EMaterial.CheckTree);

			AddTreeMaterial("red", 1, 0, 0);
			AddTreeMaterial("orange", 1, .5f, 0);
			AddTreeMaterial("lightOrange", 1, .2f, 0);
			AddTreeMaterial("pink", 1, 0, .5f);
			AddTreeMaterial("purple", .5f, 0, .5f);

			AddTreeMaterial("green", 0, 1, 0);
			AddTreeMaterial("yellow", 1, 1, 0);

			AddTreeMaterial("azure", 0, 1, 1);
			AddTreeMaterial("lightBlue", 0, .5f, 1);
			AddTreeMaterial("darkBlue", 0, 0, .3f);
			AddTreeMaterial("mediumBlue", 0, 0, .7f);
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

		public static string GetTreeMaterial(CTree pTree)
		{
			int selectedIndex = pTree.treeIndex;

			List<CTree> neighbourTrees = CProjectData.array.GetTreesInDistanceFrom(pTree.Center, 5);		
			List<string> assignedMaterials = new List<string>();
			foreach (CTree tree in neighbourTrees)
			{
				if(tree.Equals(pTree)){ continue; }
				assignedMaterials.Add(tree.assignedMaterial);
			}

			string selectedMaterial = GetTreeMaterial(selectedIndex);
			for (int i = 0; i < materialSet[EMaterial.Tree].Count; i++)
			{
				selectedMaterial = GetTreeMaterial(selectedIndex + i);
				if (!assignedMaterials.Contains(selectedMaterial))
				{
					return selectedMaterial;
				}
			}
			//CDebug.Warning("No material left to assign. it will be same as neighbour");
			return selectedMaterial;
		}

		private static string GetTreeMaterial(int pIndex)
		{
			if(!useTreeMaterial){ return ""; }

			List<int> treeIndexes = materialSet[EMaterial.Tree];
			int matIndex = (pIndex % treeIndexes.Count + treeIndexes.Count) % treeIndexes.Count;
			if (matIndex < 0 || matIndex > treeIndexes.Count - 1)
			{
				CDebug.Error("matIndex OOR");
				matIndex = 0;
			}

			return materials.MaterialList[treeIndexes[matIndex]].Name;
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