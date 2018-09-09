using System;
using System.Numerics;

namespace ForestReco
{
	public static class CLineParser
	{

		public static Vector3 ParseVector3(string pXstring, string pYstring, string pZstring)
		{
			float x = float.Parse(pXstring);
			float y = float.Parse(pYstring);
			float z = float.Parse(pZstring);
			return new Vector3(x, y, z);
		}

		public static Tuple<int, Vector3> ParseCoordinates(string pLine)
		{
			string[] split = pLine.Split(null);
			int cathegory = int.Parse(split[3]);
			return new Tuple<int, Vector3>(cathegory, ParseVector3(split[0], split[1], split[2]));
		}
	}
}