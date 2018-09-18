using System;
using System.Collections.Generic;
using System.IO;
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

		public CRefTree(string pFileName, int pTreeIndex)
		{
			treeIndex = pTreeIndex;
			string[] lines = GetFileLines(pFileName);

			//todo: porovnávání stromů ještě není implementováno, takže toto je nanic
			bool processLines = true;
			if (processLines)
			{
				List<Tuple<int, Vector3>> parsedLines = CProgramLoader.LoadParsedLines(lines, false, false);
				AddPointsFromLines(parsedLines);
				DateTime processStartTime = DateTime.Now;
				Console.WriteLine("Process. Start = " + processStartTime);
				Process();
				Console.WriteLine("Processed | duration = " + (DateTime.Now - processStartTime));
			}
			Obj = new Obj(pFileName);
			Obj.LoadObj(GetRefTreePath(pFileName) + ".obj");
		}
		
		/// <summary>
		/// In reference tree the tree is defined in great detail from peak to the ground.
		/// </summary>
		public override float GetGroundHeight()
		{
			return minBB.Y;
		}
		
		//INIT PROCESSING

		private static string GetRefTreePath(string pFileName)
		{
			return CPlatformManager.GetPodkladyPath() + "\\tree_models\\" + pFileName;
		}

		private static string[] GetFileLines(string pFileName)
		{
			//todo: firt try to load serialised file
			string fullFilePath = GetRefTreePath(pFileName) + @".txt";
			string[] lines = File.ReadAllLines(fullFilePath);
			Console.WriteLine("load: " + fullFilePath + "\n");
			return lines;
		}

		private void AddPointsFromLines(List<Tuple<int, Vector3>> pParsedLines)
		{
			DateTime addStartTime = DateTime.Now;
			Console.WriteLine("AddPointsFromLines " + pParsedLines.Count + ". Start = " + addStartTime);
			int pointsToAddCount = pParsedLines.Count;

			//lines are sorted. first point is peak for sure
			Init(pParsedLines[0].Item2, treeIndex);

			for (int i = 1; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				DateTime lineStartTime = DateTime.Now;

				Tuple<int, Vector3> parsedLine = pParsedLines[i];
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