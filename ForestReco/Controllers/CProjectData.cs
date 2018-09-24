using ObjParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	/// <summary>
	/// Data accessible from anywhere in project.
	/// </summary>
	public static class CProjectData
	{
		public static string saveFileName;

		//during one session is always processed one array file
		//public static List<Vector3> allPoints = new List<Vector3>();
		public static List<Vector3> groundPoints = new List<Vector3>();
		public static List<Vector3> vegePoints = new List<Vector3>();
		public static CGroundArray array;
		public static CHeaderInfo header;

		public static List<Obj> objsToExport = new List<Obj>();

		public static bool detectTrees;
		public static bool processTrees;
		public static bool tryMergeTrees;
		
		public static bool loadRefTrees;
		public static bool useRefTrees;

		public static bool setArray;
		public static bool exportArray;
		public static bool fillArray;

		public static bool exportPoints;
		public static bool exportTrees;

		public static float lowestHeight = int.MaxValue;
		public static float highestHeight = int.MinValue;

		public static Vector3 GetOffset()
		{
			return header?.Offset ?? Vector3.Zero;
		}

		public static Vector3 GetArrayCenter()
		{
			return header?.Center ?? Vector3.Zero;
		}

		public static float GetMinHeight()
		{
			return header?.MinHeight ?? 0;
		}

		public static void AddPoint(Tuple<EClass, Vector3> pParsedLine)
		{
			//1 = unclassified
			//2 = ground
			//5 = high vegetation
			Vector3 point = pParsedLine.Item2;

			if (pParsedLine.Item1 == EClass.Ground)
			{
				groundPoints.Add(point);
			}
			else if (pParsedLine.Item1 == EClass.Vege)
			{
				vegePoints.Add(point);
			}

			if (point.Y < lowestHeight)
			{
				lowestHeight = point.Y;
			}
			if (point.Y > highestHeight)
			{
				highestHeight = point.Y;
			}
		}
	}
}
