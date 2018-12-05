
using System;
using System.Globalization;
using System.Numerics;

namespace ForestReco
{
	public static class CCheckTreeTxtParser
	{
		public static double minInputX = double.MaxValue;
		public static double minInputY = double.MaxValue;
		public static double maxInputX = double.MinValue;
		public static double maxInputY = double.MinValue;

		public static Vector3 ParseHeaderVector3(string pXstring, string pYstring, string pZstring)
		{
			float x = float.Parse(pXstring);
			float y = float.Parse(pYstring);
			float z = float.Parse(pZstring);
			return new Vector3(x, y, z);
		}

		//class, position, note
		public static Tuple<int, Vector3, string> ParseLine(string pLine, bool pUseHeader)
		{
			string[] split = pLine.Split(null);

			//line example: 
			//ID	X				Y				Z			POZN			TYP	CISLO_
			//556	756123.256	5489291.262	923.47	*STROM403	11		403
			//1	756168.829	5489339.169	936.49	rozhrani_plotu	52	

			if (!double.TryParse(split[1], out double x)) { return null; }
			if (!double.TryParse(split[2], out double y)) { return null; }
			if (!double.TryParse(split[3], out double z)) { return null; }
			string _note = split[4];
			if (string.IsNullOrEmpty(_note)) { return null; }
			if (!int.TryParse(split[5], out int _class)) { return null; }

			if (x > maxInputX) { maxInputX = x; }
			if (y > maxInputX) { maxInputY = y; }
			if (x < minInputX) { minInputX = x; }
			if (y < minInputY) { minInputY = y; }

			//we don't use prescribed coordinate parsing as it produces badly visualisable terrain (with offset etc)
			//it should not have any effect on data processing
			Vector3 headerOffset = pUseHeader ? CProjectData.GetOffset() : Vector3.Zero;
			float xFloat = (float)(x - headerOffset.X);
			float yFloat = (float)(y - headerOffset.Y);
			float zFloat = (float)(z - headerOffset.Z);

			//swap Y-Z. Y = height in this project
			float tmp = yFloat;
			yFloat = zFloat;
			zFloat = tmp;

			return new Tuple<int, Vector3, string>(_class, new Vector3(xFloat, yFloat, zFloat), _note);
		}

		public static void Debug()
		{
			CDebug.WriteLine(minInputX + "," + minInputY + " - " + maxInputX + "," + maxInputY);
		}
	}
}