using ObjParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		public static BackgroundWorker backgroundWorker;

		//during one session is always processed one array file
		public static List<Vector3> groundPoints = new List<Vector3>();
		public static List<Vector3> vegePoints = new List<Vector3>();
		public static List<Vector3> fakePoints = new List<Vector3>();

		public static CGroundArray array;
		public static CGroundArray detailArray;

		public static CHeaderInfo header;

		public static bool tryMergeTrees = true; //default true, user dont choose
		public static bool tryMergeTrees2 = true;//default true, user dont choose
		public static bool exportBeforeMerge = false;
		
		public static bool useMaterial;

		public static float lowestHeight = int.MaxValue;
		public static float highestHeight = int.MinValue;

		public static void Init()
		{
			array = null;
			header = null;

			vegePoints.Clear();
			groundPoints.Clear();
			fakePoints.Clear();

			lowestHeight = int.MaxValue;
			highestHeight = int.MinValue;
		}

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

		public static float GetMaxHeight()
		{
			return header?.MaxHeight ?? 1;
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
