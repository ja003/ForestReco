using System;
using System.Globalization;
using System.Net.Mime;
using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public class CCoordinatesField
	{
		private CCoordinatesElement[,] field;
		private Vector3 min;
		private Vector3 max;
		private float stepSize;

		private int fieldLengthWidth;
		private int fieldLengthHeight;

		private int coordinatesCount;

		public CCoordinatesField(Vector3 pMin, Vector3 pMax, float pStepSize)
		{
			min = pMin;
			max = pMax;
			stepSize = pStepSize;
			float width = max.X - min.X;
			float height = max.Z - min.Z;
			fieldLengthWidth = (int)(width / pStepSize) + 1;
			fieldLengthHeight = (int)(height / pStepSize) + 1;
			field = new CCoordinatesElement[fieldLengthWidth, fieldLengthHeight];
			for (int i = 0; i < fieldLengthWidth; i++)
			{
				for (int j = 0; j < fieldLengthHeight; j++)
				{
					field[i, j] = new CCoordinatesElement();
				}
			}
		}

		public void AddCoordinate(Vector3 pCoordinate)
		{
			Tuple<int, int> index = GetPositionInField(pCoordinate);
			field[index.Item1, index.Item2].AddCoordinate(pCoordinate);
			coordinatesCount++;
			if (coordinatesCount % 1000 == 0)
			{
				Console.WriteLine(index.Item1 + "," + index.Item2 + " = " + pCoordinate);
			}
		}

		public void ExportToObj()
		{
			Obj obj = new Obj();

			for (int i = 0; i < fieldLengthWidth; i++)
			{
				for (int j = 0; j < fieldLengthHeight; j++)
				{
					Vertex v = new Vertex();
					float? averageHeight = field[i,j].GetAverageHeight();
					if (averageHeight != null)
					{
						v.LoadFromStringArray(new[]
						{
							"v", i.ToString(), averageHeight.ToString(), j.ToString()
						});
						obj.VertexList.Add(v);
					}
				}
			}
			string path = System.IO.Path.GetDirectoryName(
				System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\output\\try.obj";
			Console.WriteLine("write to " + path);

			//String myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			//path = myDocumentPath + "\\try.obj";
			//Console.WriteLine("write to " + path);

			obj.WriteObjFile(path, new[] { "ADAM" });
		}

		public override string ToString()
		{
			return "FIELD[" + coordinatesCount + "]. min = " + min + ", max = " + max + ", stepSize = " + stepSize;
		}

		private Tuple<int, int> GetPositionInField(Vector3 pCoordinate)
		{
			int xPos = (int)((pCoordinate.X - min.X) / stepSize);
			int yPos = (int)((pCoordinate.Z - min.Z) / stepSize);
			return new Tuple<int, int>(xPos, yPos);
		}
	}
}