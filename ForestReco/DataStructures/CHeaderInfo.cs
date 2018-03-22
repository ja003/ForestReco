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

		public CHeaderInfo(string pScaleFactorLine, string pOffsetLine, string pMinLine, string pMaxLine)
		{
			ScaleFactor = ParseLineVector3(pScaleFactorLine);
			Offset = ParseLineVector3(pOffsetLine);
			Min = ParseLineVector3(pMinLine);
			Max = ParseLineVector3(pMaxLine);
		}

		public Vector3 ParseLineVector3(string pLine)
		{
			string[] split = pLine.Split(null);
			int length = split.Length;
			float x = float.Parse(split[length - 3], CultureInfo.InvariantCulture);
			float y = float.Parse(split[length - 2], CultureInfo.InvariantCulture);
			float z = float.Parse(split[length - 1], CultureInfo.InvariantCulture);
			return new Vector3(x, y, z);
		}

		public override string ToString()
		{
			return "ScaleFactor: " + ScaleFactor + "\nOffset: " + Offset + "\nMin: " + Min + "\nMax: " + Max;
		}
	}
}