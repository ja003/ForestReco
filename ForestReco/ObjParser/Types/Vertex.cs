using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ForestReco;

namespace ObjParser.Types
{
	public class Vertex : IType
	{
		public const int MinimumDataLength = 4;
		public const string Prefix = "v";

		public double X { get; set; }

		public double Y { get; set; }

		public double Z { get; set; }

		public int Index { get; set; }

		public void LoadFromStringArray(string[] data)
		{
			if (data.Length < MinimumDataLength)
				throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

			if (!data[0].ToLower().Equals(Prefix))
				throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

			bool success;

			double x, y, z;

			success = double.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
			if (!success)
				throw new ArgumentException("Could not parse X parameter as double");

			success = double.TryParse(data[2], NumberStyles.Any, CultureInfo.InvariantCulture, out y);
			if (!success)
				throw new ArgumentException("Could not parse Y parameter as double");

			success = double.TryParse(data[3], NumberStyles.Any, CultureInfo.InvariantCulture, out z);
			if (!success)
				throw new ArgumentException("Could not parse Z parameter as double");

			X = x;
			Y = y;
			Z = z;
		}

		public override string ToString()
		{
			return ToString(new SVertexTransform());
		}

		/// <summary>
		/// pOffset is my implementation
		/// </summary>
		public string ToString(SVertexTransform pTransform)
		{
			SVector3 newPos = GetPosition();
			if (pTransform.Defined)
			{
				//1.scale
				SVector3 dir = GetPosition() - pTransform.ReferenceVertex;
				newPos = pTransform.ReferenceVertex + dir * pTransform.Scale;
				//2.move
				newPos += pTransform.PositionOffset;

			}
			return string.Format("v {0} {1} {2}", newPos.X, newPos.Y, newPos.Z);
		}

		private SVector3 GetPosition()
		{
			return new SVector3(X, Y, Z);
		}
	}

	public struct SVertexTransform
	{
		public bool Defined;

		public SVector3 PositionOffset;

		public SVector3 Scale;
		public SVector3 ReferenceVertex;

		public SVertexTransform(bool pUndefined = true)
		{
			Defined = false;
			PositionOffset = new SVector3();
			Scale = new SVector3();
			ReferenceVertex = new SVector3();
		}

		public SVertexTransform(SVector3 positionOffset, SVector3 scale, SVector3 pReferenceVertex)
		{
			Defined = true;
			PositionOffset = positionOffset;
			Scale = scale;
			ReferenceVertex = pReferenceVertex;
		}
	}
}
