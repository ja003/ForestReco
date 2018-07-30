
using System;
using System.Globalization;
using System.Numerics;

namespace ForestReco
{
	public static class CCoordinatesParser
	{
		public static Tuple<int, SVector3> ParseLine(string pLine, CHeaderInfo pHeader)
		{
			string[] split = pLine.Split(null);
			double x = double.Parse(split[0]);
			double y = double.Parse(split[1]);
			double z = double.Parse(split[2]);
			int cathegory = int.Parse(split[3]);
			//we don't use prescribed coordinate parsing as it produces badly visualisable terrain (with offset etc)
			//it should not have any effect on data processing
			return new Tuple<int, SVector3>(cathegory, 
				new SVector3(x - pHeader.Offset.X, y - pHeader.Offset.Y, z - pHeader.Offset.Z)); 
			///* * pHeader.ScaleFactor*/ - pHeader.Offset);
		}
	}
}