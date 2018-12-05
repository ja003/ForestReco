using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	/// <summary>
	/// Reference tree object.
	/// This tree is scanned in great detail.
	/// </summary>
	public class CRefTree : CTree
	{
		public Obj Obj;
		public string fileName;

		public CRefTree() { }

		public CRefTree(string pFileName, int pTreeIndex, float pTreePointExtent, bool pLoadFromFile)
		{
			treeIndex = pTreeIndex;
			fileName = pFileName;
			treePointExtent = pTreePointExtent;
			isValid = true;

			if (pLoadFromFile)
			{
				string[] lines = GetFileLines(pFileName);
				if (lines.Length == 0)
				{
					isValid = false;
					return;
				}

				LoadObj(pFileName);

				List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.ParseLines(lines, false, false);
				AddPointsFromLines(parsedLines);
				DateTime processStartTime = DateTime.Now;
				CDebug.WriteLine("Process");
				Process();
				CDebug.Duration("Processed", processStartTime);
			}
		}

		private void LoadObj(string pFileName)
		{
			Obj = new Obj(pFileName);

			string refTreePath = GetRefTreeFilePath(pFileName, pFileName + ".obj");

			bool useReducedReftreeModels = CParameterSetter.GetBoolSettings(ESettings.useReducedReftreeModels);
			if (useReducedReftreeModels || !File.Exists(refTreePath))
			{
				Obj.Name += "_reduced";

				string reducedObjFileName = pFileName + "_reduced.obj";

				if (!useReducedReftreeModels)
				{
					CDebug.WriteLine("Reftree " + refTreePath + " OBJ does not exist.");
					CDebug.WriteLine("Try reduced file: " + reducedObjFileName);
				}
				refTreePath = GetRefTreeFilePath(pFileName, reducedObjFileName);
				if (!File.Exists(refTreePath))
				{
					CDebug.Error("No ref tree OBJ found!");
					return;
				}
			}

			Obj.LoadObj(refTreePath);
		}

		public CRefTree(string pFileName, string[] pSerializedLines)
		{
			DeserialiseMode currentMode = DeserialiseMode.None;
			fileName = pFileName;
			isValid = true;

			branches = new List<CBranch>();
			List<CTreePoint> _treepointsOnBranch = new List<CTreePoint>();

			foreach (string line in pSerializedLines)
			{
				switch (line)
				{
					case "treeIndex":
						currentMode = DeserialiseMode.TreeIndex;
						continue;
					case "treePointExtent":
						currentMode = DeserialiseMode.TreePointExtent;
						continue;
					case "peak":
						currentMode = DeserialiseMode.Peak;
						continue;
					case "branches":
						currentMode = DeserialiseMode.Branches;
						continue;
					case "stem":
						currentMode = DeserialiseMode.Stem;
						branches.Last().SetTreePoints(_treepointsOnBranch);
						_treepointsOnBranch = new List<CTreePoint>();
						continue;
					case "boundingBox":
						currentMode = DeserialiseMode.BoundingBox;
						continue;
				}

				switch (currentMode)
				{
					case DeserialiseMode.TreeIndex:
						treeIndex = int.Parse(line);
						break;
					case DeserialiseMode.TreePointExtent:
						treePointExtent = float.Parse(line);
						break;
					case DeserialiseMode.Peak:
						peak = CPeak.Deserialize(line, treePointExtent);
						stem = new CBranch(this, 0, 0);
						break;
					case DeserialiseMode.Branches:
						if (line.Contains("branch "))
						{
							int branchIndex = branches.Count;
							if (branchIndex > 0)
							{
								branches.Last().SetTreePoints(_treepointsOnBranch);
							}

							branches.Add(new CBranch(
								this,
								branchIndex * BRANCH_ANGLE_STEP,
								branchIndex * BRANCH_ANGLE_STEP + BRANCH_ANGLE_STEP));
							_treepointsOnBranch = new List<CTreePoint>();
						}
						else
						{
							CTreePoint treePointOnBranch = CTreePoint.Deserialize(line, treePointExtent);
							_treepointsOnBranch.Add(treePointOnBranch);
							Points.Add(treePointOnBranch.Center);
						}
						break;
					case DeserialiseMode.Stem:
						_treepointsOnBranch.Add(CTreePoint.Deserialize(line, treePointExtent));
						break;
					case DeserialiseMode.BoundingBox:
						string[] split = line.Split(null);
						minBB = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
						maxBB = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
						break;
				}
			}
			stem.SetTreePoints(_treepointsOnBranch);
			LoadObj(pFileName);
		}

		private enum DeserialiseMode
		{
			None,
			TreeIndex,
			Peak,
			Branches,
			Stem,
			BoundingBox,
			TreePointExtent
		}

		public static CRefTree Deserialize(string pFileName)
		{
			string filePath = GetRefTreeFilePath(pFileName, pFileName + ".reftree");
			CDebug.WriteLine("\nDeserialize. filePath = " + filePath);

			if (!File.Exists(filePath))
			{
				//not error. reftree files doesnt have to be generated yet
				CDebug.Warning(".reftree file does not exist.");
				return null;
			}

			string[] serialisedLines = File.ReadAllLines(filePath);

			CRefTree refTree = new CRefTree(pFileName, serialisedLines);

			return refTree;
		}

		public new List<string> Serialize()
		{
			List<string> lines = new List<string>();
			lines.Add("treeIndex");
			lines.Add(treeIndex.ToString());
			lines.Add("treePointExtent");
			lines.Add(treePointExtent.ToString());
			lines.Add("peak");
			lines.Add(peak.Serialize());
			lines.Add("branches");
			foreach (CBranch b in branches)
			{
				lines.Add("branch " + branches.IndexOf(b));
				lines.AddRange(b.Serialize());
			}
			lines.Add("stem");
			lines.AddRange(stem.Serialize());

			lines.Add("boundingBox");
			lines.Add(base.Serialize());

			return lines;
		}

		public override float GetTreeHeight()
		{
			return Obj.Size.YMax - Obj.Size.YMin;
		}
		
		protected override void OnProcess()
		{
			string filePath = GetRefTreeFilePath(fileName, fileName + ".reftree");
			CDebug.WriteLine("\nfilePath = " + filePath);

			if (File.Exists(filePath))
			{
				CDebug.Error(".reftree file already exists");
				return;
			}

			DateTime processStartTime = DateTime.Now;
			CDebug.WriteLine("Serialization");
			List<string> serializedTree = Serialize();

			using (StreamWriter file = new StreamWriter(filePath, false))
			{
				foreach (string line in serializedTree)
				{
					file.WriteLine(line);
				}
			}
			CDebug.Duration("Serialized", processStartTime);
			CDebug.WriteLine("saved to: " + filePath);
		}

		//INIT PROCESSING

		public static string GetRefTreeFilePath(string pSubfolder, string pFileName)
		{
			return GetRefTreeFolder(pSubfolder) + "\\" + pFileName;
		}

		private static string GetRefTreeFolder(string pSubfolder)
		{
			return CParameterSetter.GetStringSettings(ESettings.reftreeFolderPath) + "\\" + pSubfolder;
		}

		//use all by default
		private static bool refTreeFirst = true;
		private static bool refTreeLast = true;

		private static bool refTreeFront = true;
		private static bool refTreeBack = true;

		private static bool refTreeJehlici = true;
		private static bool refTreeKmeny = true;


		private static string GetXyzFileName(string pFileName, bool pFirst, bool pFront, bool pJehlici)
		{
			string firstString = pFirst ? "F" : "L";
			string frontString = pFront ? "1" : "2";
			string jehliciString = pJehlici ? "J" : "K";
			return pFileName + "-" + firstString + "-" + frontString + "-" + jehliciString + ".xyz";
		}

		private static string[] GetFileLines(string pFileName)
		{
			string firstFrontJehliciPath = GetRefTreeFilePath(pFileName, GetXyzFileName(pFileName, true, true, true));
			string firstFrontKmenyPath = GetRefTreeFilePath(pFileName, GetXyzFileName(pFileName, true, true, false));
			string firstBackJehliciPath = GetRefTreeFilePath(pFileName, GetXyzFileName(pFileName, true, false, true));
			string firstBackKmenyPath = GetRefTreeFilePath(pFileName, GetXyzFileName(pFileName, true, false, false));

			string lastFrontJehliciPath = GetRefTreeFilePath(pFileName, GetXyzFileName(pFileName, false, true, true));
			string lastFrontKmenyPath = GetRefTreeFilePath(pFileName, GetXyzFileName(pFileName, false, true, false));
			string lastBackJehliciPath = GetRefTreeFilePath(pFileName, GetXyzFileName(pFileName, false, false, true));
			string lastBackKmenyPath = GetRefTreeFilePath(pFileName, GetXyzFileName(pFileName, false, false, false));

			List<string> lines = new List<string>();

			if (refTreeFirst)
			{
				if (refTreeFront)
				{
					if (refTreeJehlici)
					{
						string[] firstFrontJehliciLines = GetReftreeLines(firstFrontJehliciPath);
						lines.AddRange(firstFrontJehliciLines);
					}
					else if (refTreeKmeny)
					{
						string[] firstFrontKmenyLines = GetReftreeLines(firstFrontKmenyPath);
						lines.AddRange(firstFrontKmenyLines);
					}
				}
				if (refTreeBack)
				{
					if (refTreeJehlici)
					{
						string[] firstBackJehliciLines = GetReftreeLines(firstBackJehliciPath);
						lines.AddRange(firstBackJehliciLines);
					}
					if (refTreeKmeny)
					{
						string[] firstBackKmenyLines = GetReftreeLines(firstBackKmenyPath);
						lines.AddRange(firstBackKmenyLines);
					}
				}
			}
			if (refTreeLast)
			{
				if (refTreeFront)
				{
					if (refTreeJehlici)
					{
						string[] lastFrontJehliciLines = GetReftreeLines(lastFrontJehliciPath);
						lines.AddRange(lastFrontJehliciLines);
					}
					if (refTreeKmeny)
					{
						string[] lastFrontKmenyLines = GetReftreeLines(lastFrontKmenyPath);
						lines.AddRange(lastFrontKmenyLines);
					}
				}
				if (refTreeBack)
				{
					if (refTreeJehlici)
					{
						string[] lastBackJehliciLines = GetReftreeLines(lastBackJehliciPath);
						lines.AddRange(lastBackJehliciLines);
					}
					if (refTreeKmeny)
					{
						string[] lastBackKmenyLines = GetReftreeLines(lastBackKmenyPath);
						lines.AddRange(lastBackKmenyLines);
					}
				}
			}

			return lines.ToArray();
		}


		private static string[] GetReftreeLines(string refTreeXyzPath)
		{
			if (!File.Exists(refTreeXyzPath))
			{
				//not errror. all files doesnt have to be specified
				CDebug.Warning("Reftree " + refTreeXyzPath + " XYZ does not exist.");
				return new string[0];
			}

			string[] lines = File.ReadAllLines(refTreeXyzPath);
			CDebug.WriteLine("load: " + refTreeXyzPath);
			return lines;
		}

		private void AddPointsFromLines(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			DateTime addStartTime = DateTime.Now;
			CDebug.WriteLine("AddPointsFromLines " + pParsedLines.Count);
			int pointsToAddCount = pParsedLines.Count;

			pParsedLines.Sort((a, b) => b.Item2.Y.CompareTo(a.Item2.Y));
			//lines are sorted. first point is peak for sure
			Init(pParsedLines[0].Item2, treeIndex, treePointExtent);

			DateTime lineStartTime = DateTime.Now;

			for (int i = 1; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				Tuple<EClass, Vector3> parsedLine = pParsedLines[i];
				Vector3 point = parsedLine.Item2;

				//all points belong to 1 tree. force add it
				AddPoint(point);
				
				CDebug.Progress(i, pParsedLines.Count, 100000, ref lineStartTime, addStartTime, "added point:");
			}
			CDebug.Duration("All points added", addStartTime);
		}
		
		public override string ToString()
		{
			return $"[{treeIndex}] : {GetTreeHeight()}m. {Obj.Name}";
		}
	}
}