using System.Globalization;
using System.Numerics;

namespace ForestReco
{
	public class CHeaderInfo
	{
		public Vector3 ScaleFactor;
		public Vector3 Offset;
		public Vector3 Min;
		public Vector3 Max;

		//public float Width;
		//public float Height;

		public CHeaderInfo(string pScaleFactorLine, string pOffsetLine, string pMinLine, string pMaxLine)
		{
			ScaleFactor = ParseLineVector3(pScaleFactorLine);
			Offset = ParseLineVector3(pOffsetLine);
			Min = ParseLineVector3(pMinLine) - Offset;
			Max = ParseLineVector3(pMaxLine) - Offset;

			//Width = Max.X - Min.X;
			//Height = Max.Z - Min.Z;
		}

		private Vector3 ParseLineVector3(string pLine)
		{
			string[] split = pLine.Split(null);
			int length = split.Length;
			return CLineParser.ParseVector3(split[length - 3], split[length - 2], split[length - 1]);
		}

		public override string ToString()
		{
			return "ScaleFactor: " + ScaleFactor + "\nOffset: " + Offset + "\nMin: " + Min + "\nMax: " + Max;
		}
	}
}