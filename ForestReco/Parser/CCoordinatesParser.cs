
using System;
using System.Numerics;

namespace ForestReco
{
	public static class CCoordinatesParser
	{
		public static Tuple<int, Vector3> ParseLine(string pLine, CHeaderInfo pHeader)
		{
			string[] split = pLine.Split(null);
			float x = float.Parse(split[0]);
			float y = float.Parse(split[1]);
			float z = float.Parse(split[2]);
			int cathegory = int.Parse(split[3]);
			//we don't use prescribed coordinate parsing as it produces badly visualisable terrain (with offset etc)
			//it should not have any effect on data processing
			return new Tuple<int, Vector3>(cathegory, new Vector3(x, y, z) /* * pHeader.ScaleFactor*/ - pHeader.Offset);
		}
	}
}