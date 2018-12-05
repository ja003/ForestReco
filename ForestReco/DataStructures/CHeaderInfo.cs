using System;
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

		public Vector3 BotLeftCorner => new Vector3(Min.X, 0, Min.Z);
		public Vector3 TopRightCorner => new Vector3(Max.X, 0, Max.Z);
		public Vector3 Center => (BotLeftCorner + TopRightCorner) / 2;
		public float MinHeight => Min.Y;
		public float MaxHeight => Max.Y;
		public float Width => TopRightCorner.X - BotLeftCorner.X;
		public float Height => TopRightCorner.Z - BotLeftCorner.Z;

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
			//transfer to format Y = height
			float tmpY = Min.Y;
			Min.Y = Min.Z;
			Min.Z = tmpY;
			tmpY = Max.Y;
			Max.Y = Max.Z;
			Max.Z = tmpY;

			if (Min == Vector3.Zero && Max == Vector3.Zero)
			{
				CDebug.WriteLine("Invalid header. Creating default header.");
				const int defaultArraySize = 15;
				Min = new Vector3(-defaultArraySize, 0, -defaultArraySize);
				Max = new Vector3(defaultArraySize, 0, defaultArraySize);
				Offset = Vector3.Zero;
			}
		}

		private string GetLine(string[] lines, EHeaderAttribute pAttribute)
		{
			switch (pAttribute)
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


		private Vector3 ParseLineVector3(string pLine)
		{
			if (string.IsNullOrEmpty(pLine)) { return Vector3.Zero;}

			string[] split = pLine.Split(null);
			int length = split.Length;
			//											X					Y					Z
			return CLazTxtParser.ParseHeaderVector3(split[length - 3], split[length - 2], split[length - 1]);
		}

		public override string ToString()
		{
			return "ScaleFactor: " + ScaleFactor + "\nOffset: " + Offset + "\nMin: " + Min + "\nMax: " + Max;
		}

		/// <summary>
		/// First line of file with header should look like:
		/// % file signature:            'LASF'
		/// </summary>
		public static bool HasHeader(string pFirstLine)
		{
			string[] split = pFirstLine.Split(null);
			string firstSign = split[0];
			bool result = string.Equals(firstSign, "%") || string.Equals(firstSign, "#");
			return result;
		}
	}
}