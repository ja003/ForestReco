using System;
using System.Globalization;
using System.IO;
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
		private const string DEFAULT_FILENAME = "try";

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
					float? averageHeight = field[i, j].GetAverageHeight();
					if (averageHeight != null)
					{
						v.LoadFromStringArray(new[]
						{
							"v", (i*(int)stepSize).ToString(), averageHeight.ToString(), (j*(int)stepSize).ToString()
						});
						obj.VertexList.Add(v);
						field[i, j].VertexIndex = obj.VertexList.Count;
					}
				}
			}

			for (int i = 0; i < fieldLengthWidth - 1; i++)
			{
				for (int j = 0; j < fieldLengthHeight - 1; j++)
				{
					int ind1 = field[i, j].VertexIndex;
					if (ind1 != -1)
					{
						int ind2 = field[i + 1, j].VertexIndex;
						if (ind2 != -1)
						{
							int ind3 = field[i, j + 1].VertexIndex;
							if (ind3 != -1)
							{
								Face f = new Face();
								f.LoadFromStringArray(new[]
								{
									"f", ind1.ToString(),ind2.ToString(),ind3.ToString()   //ind1+"//"+ind3, ind3+"//"+ind2, ind2+"//"+ind1
								});

								obj.FaceList.Add(f);
							}
						}
					}
				}
			}

			string fileName = DEFAULT_FILENAME;
			string extension = ".obj";
			string path = Path.GetDirectoryName(
				System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\output\\";
			string fullPath = path + fileName + extension;
			int fileIndex = 0;
			while (File.Exists(fullPath))
			{
				fileName = DEFAULT_FILENAME + fileIndex;
				fullPath = path + fileName + extension;
				fileIndex++;
			}

			Console.WriteLine("write to " + fullPath);

			//String myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			//path = myDocumentPath + "\\try.obj";
			//Console.WriteLine("write to " + path);

			obj.WriteObjFile(fullPath, new[] { "ADAM" });
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