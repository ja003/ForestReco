using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public class CTreeObj : Obj
	{
		public Vector3 Position;
		public Vector3 Rotation;
		public Vector3 Scale = Vector3.One;

		public SVertexTransform GetVertexTransform()
		{
			return new SVertexTransform(Position, Scale, Rotation,
				//bot center point
				new Vector3((float)(Size.XMin + Size.XMax) / 2, (float)Size.YMin, (float)(Size.ZMin + Size.ZMax) / 2));
		}
	}
}