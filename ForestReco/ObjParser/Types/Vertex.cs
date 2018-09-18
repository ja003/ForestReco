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
		public Vertex() { }

		private Vector3 position => new Vector3(X, Y, Z);

		public Vertex(Vector3 pVector3, int pVertexIndex)
		{
			X = pVector3.X;
			Y = pVector3.Y;
			Z = pVector3.Z;
			Index = pVertexIndex;
		}

		public const int MinimumDataLength = 4;
		public const string Prefix = "v";

		public float X;
		public float Y;
		public float Z;

		public int Index { get; set; }

		public void LoadFromStringArray(string[] data)
		{
			if (data.Length < MinimumDataLength)
				throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

			if (!data[0].ToLower().Equals(Prefix))
				throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

			bool success;

			float x, y, z;

			success = float.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
			if (!success)
				throw new ArgumentException("Could not parse X parameter as float");

			success = float.TryParse(data[2], NumberStyles.Any, CultureInfo.InvariantCulture, out y);
			if (!success)
				throw new ArgumentException("Could not parse Y parameter as float");

			success = float.TryParse(data[3], NumberStyles.Any, CultureInfo.InvariantCulture, out z);
			if (!success)
				throw new ArgumentException("Could not parse Z parameter as float");

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
			Vector3 newPos = position;
			if (pTransform.Defined)
			{
				//1.scale
				newPos = Vector3.Transform(newPos, Matrix4x4.CreateScale(
					pTransform.Scale.X, pTransform.Scale.Y, pTransform.Scale.Z, pTransform.ReferenceVertex));

				//2.rotate
				Vector3 rotate = new Vector3(
					CUtils.ToRadians(pTransform.Rotation.X),
					CUtils.ToRadians(pTransform.Rotation.Y),
					CUtils.ToRadians(pTransform.Rotation.Z));
				newPos = Vector3.Transform(newPos, Matrix4x4.CreateRotationX(rotate.X, pTransform.ReferenceVertex));
				newPos = Vector3.Transform(newPos, Matrix4x4.CreateRotationY(rotate.Y, pTransform.ReferenceVertex));
				newPos = Vector3.Transform(newPos, Matrix4x4.CreateRotationZ(rotate.Z, pTransform.ReferenceVertex));

				//3.translate
				//newPos = Vector3.Transform(newPos, Matrix4x4.CreateTranslation());...
				newPos += pTransform.PositionOffset;
			}
			return $"v {newPos.X} {newPos.Y} {newPos.Z}";
		}
	}

	public struct SVertexTransform
	{
		public bool Defined;

		public Vector3 PositionOffset;
		public Vector3 Scale;
		public Vector3 Rotation;

		public Vector3 ReferenceVertex;

		public SVertexTransform(bool pUndefined = true)
		{
			Defined = false;
			PositionOffset = Vector3.Zero;
			Scale = Vector3.One;
			Rotation = Vector3.Zero;
			ReferenceVertex = Vector3.Zero;
		}

		public SVertexTransform(Vector3 positionOffset, Vector3 scale, Vector3 pRotation, Vector3 pReferenceVertex)
		{
			Defined = true;
			PositionOffset = positionOffset;
			Scale = scale;
			Rotation = pRotation;
			ReferenceVertex = pReferenceVertex;
		}
	}
}
