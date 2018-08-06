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
		
		public Vector3 ToVector3()
		{
			return new Vector3((float)X, (float)Y, (float)Z);
		}

		public Vertex ToVertex(int pIndex, Vector3 pOffset )
		{
			return new Vertex()
			{
				Index = pIndex,
				X = X + pOffset.X,
				Y = Y + pOffset.Y,
				Z = Z + pOffset.Z,
			};
		}
	}
}