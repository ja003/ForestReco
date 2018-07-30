using System;
using System.Numerics;

namespace ForestReco
{
	public static class CLineParser
	{

		public static SVector3 ParseVector3(string pXstring, string pYstring, string pZstring)
		{
			double x = double.Parse(pXstring);
			double y = double.Parse(pYstring);
			double z = double.Parse(pZstring);
			return new SVector3(x, y, z);
		}

		public static Tuple<int, SVector3> ParseCoordinates(string pLine)
		{
			string[] split = pLine.Split(null);
			int cathegory = int.Parse(split[3]);
			return new Tuple<int, SVector3>(cathegory, ParseVector3(split[0], split[1], split[2]));
		}
	}
}