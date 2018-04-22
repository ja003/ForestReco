﻿using System;
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
			//Offset.Z = 0; //given Z offset will not be used
			Min = ParseLineVector3(pMinLine) - Offset;
			Max = ParseLineVector3(pMaxLine) - Offset;
			//we set Z offset so the lowest point will have height 0 (better visualization)
			//Offset.Z = ParseLineVector3(pMinLine).Z;
		}

		public Vector2 GetBotLeftCorner() { return new Vector2(Min.X, Min.Y); }

		public Vector2 GetTopRightCorner() { return new Vector2(Max.X, Max.Y); }

		public float GetMinHeight() { return Min.Z; }

		public float GetMaxHeight() { return Max.Z; }

		private Vector3 ParseLineVector3(string pLine)
		{
			string[] split = pLine.Split(null);
			int length = split.Length;
			//									X					Y					Z
			return CLineParser.ParseVector3(split[length - 3], split[length - 2], split[length - 1]);
		}

		public override string ToString()
		{
			return "ScaleFactor: " + ScaleFactor + "\nOffset: " + Offset + "\nMin: " + Min + "\nMax: " + Max;
		}
	}
}