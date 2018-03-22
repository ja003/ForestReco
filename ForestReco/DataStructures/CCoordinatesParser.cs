
using System;
using System.Numerics;

namespace ForestReco
{
	public static class CCoordinatesParser
	{
		public static Tuple<int, Vector3> ParseLine(string pLine)
		{
			string[] split = pLine.Split(null);
			float x = float.Parse(split[0]);
			float y = float.Parse(split[1]);
			float z = float.Parse(split[2]);
			int cathegory = int.Parse(split[3]);
			return new Tuple<int, Vector3>(cathegory, new Vector3(x, y, z));
		}
	}
}