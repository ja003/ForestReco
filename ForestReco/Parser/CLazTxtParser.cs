
using System;
using System.Globalization;
using System.Numerics;

namespace ForestReco
{
	public static class CLazTxtParser
	{
		public static Vector3 ParseHeaderVector3(string pXstring, string pYstring, string pZstring)
		{
			float x = float.Parse(pXstring);
			float y = float.Parse(pYstring);
			float z = float.Parse(pZstring);
			return new Vector3(x, y, z);
		}

		public static Tuple<int, Vector3> ParseLine(string pLine)
		{
			string[] split = pLine.Split(null);
			if (split.Length < 4 || (split.Length > 0 && split[0].Contains("#")))
			{
				Console.WriteLine(pLine + " not valid");
				return null;
			}
			double x = double.Parse(split[0]);
			double y = double.Parse(split[1]);
			double z = double.Parse(split[2]);
			int cathegory = int.Parse(split[3]);
			//we don't use prescribed coordinate parsing as it produces badly visualisable terrain (with offset etc)
			//it should not have any effect on data processing
			float xFloat = (float)(x - CProjectData.header.Offset.X);
			float yFloat = (float)(y - CProjectData.header.Offset.Y);
			float zFloat = (float)(z - CProjectData.header.Offset.Z);
			return new Tuple<int, Vector3>(cathegory, new Vector3(xFloat, yFloat, zFloat)); 
			///* * pHeader.ScaleFactor*/ - pHeader.Offset);
		}
	}
}