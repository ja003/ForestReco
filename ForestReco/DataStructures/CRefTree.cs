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

		public CRefTree() { }

		public CRefTree(string pFileName, int pTreeIndex)
		{
			treeIndex = pTreeIndex;
			string[] lines = GetFileLines(pFileName);
			LoadObj(pFileName);

			bool processLines = true;
			if (processLines)
			{
				List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.LoadParsedLines(lines, false, false);
				AddPointsFromLines(parsedLines);
				DateTime processStartTime = DateTime.Now;
				Console.WriteLine("\nProcess. Start = " + processStartTime);
				Process();
				Console.WriteLine("Processed | duration = " + (DateTime.Now - processStartTime));
			}
		}

		private void LoadObj(string pFileName)
		{
			Obj = new Obj(pFileName);

			string refTreePath = GetRefTreePath(pFileName + ".obj");

			if (!File.Exists(refTreePath))
			{
				Console.WriteLine("Ref tree " + refTreePath + " OBJ does not exist.");
				string reducedObjFileName = pFileName + "_reduced.obj";
				Console.WriteLine("Try reduced file: " + reducedObjFileName);
				refTreePath = GetRefTreePath(reducedObjFileName);
				if (!File.Exists(refTreePath))
				{
					Console.WriteLine("ERROR: No ref tree OBJ found!");
					return;
				}
			}

			Obj.LoadObj(refTreePath);
		}

		public CRefTree(string pFileName, string[] pSerializedLines)
		{
			DeserialiseMode currentMode = DeserialiseMode.None;

			branches = new List<CBranch>();
			List<CTreePoint> _treepointsOnBranch = new List<CTreePoint>();

			foreach (string line in pSerializedLines)
			{
				switch (line)
				{
					case "treeIndex":
						currentMode = DeserialiseMode.TreeIndex;
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
					case DeserialiseMode.Peak:
						peak = CPeak.Deserialize(line);
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
							CTreePoint treePointOnBranch = CTreePoint.Deserialize(line);
							_treepointsOnBranch.Add(treePointOnBranch);
							Points.Add(treePointOnBranch.Center);
						}
						break;
					case DeserialiseMode.Stem:
						_treepointsOnBranch.Add(CTreePoint.Deserialize(line));
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
			BoundingBox
		}

		public static CRefTree Deserialize(string pFileName)
		{
			string filePath = GetRefTreePath(pFileName + ".reftree");
			Console.WriteLine("\nDeserialize. filePath = " + filePath);

			if (!File.Exists(filePath))
			{
				Console.WriteLine(".reftree file does not exist.");
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

		/// <summary>
		/// In reference tree the tree is defined in great detail from peak to the ground.
		/// </summary>
		public override float GetGroundHeight()
		{
			return minBB.Y;
		}

		protected override void OnProcess()
		{
			string filePath = GetRefTreePath(Obj.Name + ".reftree");
			Console.WriteLine("\nfilePath = " + filePath);

			if (File.Exists(filePath))
			{
				Console.WriteLine("ERROR: .reftree file already exists");
				return;
			}

			DateTime processStartTime = DateTime.Now;
			Console.WriteLine("Serialization");
			List<string> serializedTree = Serialize();

			using (StreamWriter file = new StreamWriter(filePath, false))
			{
				foreach (string line in serializedTree)
				{
					file.WriteLine(line);
				}
			}
			Console.WriteLine("Serialized | duration = " + (DateTime.Now - processStartTime));
			Console.WriteLine("saved to: " + filePath);
		}

		//INIT PROCESSING

		public static string GetRefTreePath(string pFileName)
		{
			return GetRefTreeFolder() + pFileName;
		}

		private static string GetRefTreeFolder()
		{
			return CPlatformManager.GetPodkladyPath() + "\\tree_models\\";
		}

		private static string[] GetFileLines(string pFileName)
		{

			string refTreeTxtPath = GetRefTreePath(pFileName + ".txt");

			if (!File.Exists(refTreeTxtPath))
			{
				Console.WriteLine("Ref tree " + refTreeTxtPath + " TXT does not exist.");
				refTreeTxtPath = GetRefTreePath(pFileName + "_F_1+2.txt");
				Console.WriteLine("Try file: " + refTreeTxtPath);
				if (!File.Exists(refTreeTxtPath))
				{
					Console.WriteLine("ERROR: No ref tree TXT found!");
					throw new Exception(); //todo: exception
				}
			}

			string[] lines = File.ReadAllLines(refTreeTxtPath);
			Console.WriteLine("load: " + refTreeTxtPath + "\n");
			return lines;
		}

		private void AddPointsFromLines(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			DateTime addStartTime = DateTime.Now;
			Console.WriteLine("AddPointsFromLines " + pParsedLines.Count + ". Start = " + addStartTime);
			int pointsToAddCount = pParsedLines.Count;

			//lines are sorted. first point is peak for sure
			Init(pParsedLines[0].Item2, treeIndex);

			for (int i = 1; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				DateTime lineStartTime = DateTime.Now;

				Tuple<EClass, Vector3> parsedLine = pParsedLines[i];
				Vector3 point = parsedLine.Item2;

				//all points belong to 1 tree. force add it
				TryAddPoint(point, true);

				TimeSpan duration = DateTime.Now - lineStartTime;
				if (duration.Milliseconds > 1) { Console.WriteLine(i + ": " + duration); }
			}
			Console.WriteLine("All points added | duration = " + (DateTime.Now - addStartTime));
		}
	}
}