
using System;
using System.Globalization;
using System.Numerics;

namespace ForestReco
{
	public static class CLazTxtParser
	{
		public static Tuple<int, Vector3> ParseLine(string pLine, CHeaderInfo pHeader)
		{
			string[] split = pLine.Split(null);
			if (split.Length < 4 || (split.Length > 0 && split[0].Contains("#")))
			{
				Console.WriteLine(pLine + " not valid");
				return null;
			}
			float x = float.Parse(split[0]);
			float y = float.Parse(split[1]);
			float z = float.Parse(split[2]);
			int cathegory = int.Parse(split[3]);
			//we don't use prescribed coordinate parsing as it produces badly visualisable terrain (with offset etc)
			//it should not have any effect on data processing
			return new Tuple<int, Vector3>(cathegory, 
				new Vector3(x - pHeader.Offset.X, y - pHeader.Offset.Y, z - pHeader.Offset.Z)); 
			///* * pHeader.ScaleFactor*/ - pHeader.Offset);
		}
	}
}