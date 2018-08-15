using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public class CTreeObj : Obj
	{
		public SVector3 Position;
		public SVector3 Rotation;
		public SVector3 Scale = new SVector3(Vector3.One);

		public SVertexTransform GetVertexTransform()
		{
			return new SVertexTransform(Position, Scale,
				//bot center point
				new SVector3((Size.XMin + Size.XMax)/2, Size.YMin, (Size.ZMin + Size.ZMax) / 2));
		}
	}
}