using System.Numerics;
using ObjParser.Types;

namespace ForestReco
{
	public struct SVector3
	{
		public double X;
		public double Y;
		public double Z;

		public SVector3(double x, double y, double z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		public SVector3(double x, double y)
		{
			this.X = x;
			this.Y = y;
			this.Z = 0;
		}

		public SVector3(Vector3 pVector3)
		{
			X = pVector3.X;
			Y = pVector3.Y;
			Z = pVector3.Z;
		}

		public static SVector3 operator +(SVector3 a, SVector3 b)
		{
			return new SVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}
		public static SVector3 operator -(SVector3 a, SVector3 b)
		{
			return new SVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}
		public static SVector3 operator *(SVector3 a, double b)
		{
			return new SVector3(a.X * b, a.Y * b, a.Z * b);
		}
		/// <summary>
		/// "Modified" scalar product
		/// </summary>
		public static SVector3 operator *(SVector3 a, SVector3 b)
		{
			return new SVector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		}

		public static SVector3 operator /(SVector3 a, double b)
		{
			return new SVector3(a.X / b, a.Y / b, a.Z / b);
		}

		public static SVector3 operator -(SVector3 a)
		{
			return new SVector3(-a.X, -a.Y, -a.Z);
		}

		public Vector3 ToVector3(bool pSwapYZ = false)
		{
			if (pSwapYZ)
			{
				return new Vector3((float)X, (float)Z, (float)Y);
			}
			return new Vector3((float)X, (float)Y, (float)Z);
		}

		public Vertex ToVertex(int pIndex, Vector3 pOffset)
		{
			return new Vertex()
			{
				Index = pIndex,
				X = X + pOffset.X,
				Y = Y + pOffset.Y,
				Z = Z + pOffset.Z,
			};
		}

		public void FlipYZ()
		{
			double tmp = Y;
			Y = Z;
			Z = tmp;
		}

		public SVector3 Clone()
		{
			return new SVector3(X, Y, Z);
		}

		public void MoveBy(SVector3 pOffset)
		{
			X += pOffset.X;
			Y += pOffset.Y;
			Z += pOffset.Z;
		}

		public override string ToString()
		{
			return X + "," + Y + "," + Z;
		}
	}
}