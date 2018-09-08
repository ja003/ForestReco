using System;
using System.Globalization;
using System.Numerics;

namespace ForestReco
{
	public class CHeaderInfo
	{
		public SVector3 ScaleFactor;
		public SVector3 Offset;
		public SVector3 Min;
		public SVector3 Max;

		//public float Width;
		//public float Height;

		public CHeaderInfo(string[] lines)
		{
			string pScaleFactorLine = GetLine(lines, EHeaderAttribute.Scale);
			string pOffsetLine = GetLine(lines, EHeaderAttribute.Offset);
			string pMinLine = GetLine(lines, EHeaderAttribute.Min);
			string pMaxLine = GetLine(lines, EHeaderAttribute.Max);

			ScaleFactor = ParseLineVector3(pScaleFactorLine);
			Offset = ParseLineVector3(pOffsetLine);
			//Offset.Z = 0; //given Z offset will not be used
			Min = ParseLineVector3(pMinLine) - Offset;
			Max = ParseLineVector3(pMaxLine) - Offset;
			//we set Z offset so the lowest point will have height 0 (better visualization)
			//Offset.Z = ParseLineVector3(pMinLine).Z;
			Console.WriteLine(CProjectData.header);
		}

		private string GetLine(string[] lines, EHeaderAttribute pAttribute)
		{
			switch(pAttribute)
			{
				case EHeaderAttribute.Scale: return lines[15];
				case EHeaderAttribute.Offset: return lines[16];
				case EHeaderAttribute.Min: return lines[17];
				case EHeaderAttribute.Max: return lines[18];
			}
			return "";
		}

		public enum EHeaderAttribute
		{
			Scale,
			Offset,
			Min,
			Max
		}

		public SVector3 BotLeftCorner => new SVector3(Min.X, Min.Y); 
		public SVector3 TopRightCorner => new SVector3(Max.X, Max.Y); 
		public SVector3 Center => (BotLeftCorner + TopRightCorner) / 2;

		//public SVector3 GetTopLeftCorner() { return new SVector3(Min.X, Max.Y); }

		public float MinHeight => (float)Min.Z; 
		//public double GetMinHeight() { return Min.Z; }

		//public double GetMaxHeight() { return Max.Z; }

		private SVector3 ParseLineVector3(string pLine)
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