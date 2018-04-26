using System;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Numerics;
using ObjParser;
using ObjParser.Types;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace ForestReco
{
	public class CCoordinatesField
	{
		private CCoordinates2DElement[,] field;
		private Vector2 botLeftCorner; //lower corner
		private Vector2 topRightCorner; //upper corner
		private float minHeight;
		private float maxHeight;
		private float stepSize;

		private int fieldXRange;
		private int fieldYRange;
		private int fieldZRange;

		private int coordinatesCount;
		private const string DEFAULT_FILENAME = "try";

		private bool storeDepthCoordinates;

		//PUBLIC

		/// <summary>
		/// Constructor for coordinates field
		/// </summary>
		/// <param name="pHeader">Header info</param>
		/// <param name="pStepSize">Step size in meters</param>
		/// <param name="pStoreDepthCoordinates">Heights will be also stored in Z field</param>
		public CCoordinatesField(CHeaderInfo pHeader, float pStepSize, bool pStoreDepthCoordinates)
		{
			botLeftCorner = pHeader.GetBotLeftCorner();
			topRightCorner = pHeader.GetTopRightCorner();
			stepSize = pStepSize;
			float w = topRightCorner.X - botLeftCorner.X;
			float h = topRightCorner.Y - botLeftCorner.Y;
			fieldXRange = (int)(w / pStepSize) + 1;
			fieldYRange = (int)(h / pStepSize) + 1;
			minHeight = pHeader.GetMinHeight();
			maxHeight = pHeader.GetMaxHeight();
			fieldZRange = (int)((maxHeight - minHeight) / pStepSize) + 1;

			storeDepthCoordinates = pStoreDepthCoordinates;

			field = new CCoordinates2DElement[fieldXRange, fieldYRange];
			for (int i = 0; i < fieldXRange; i++)
			{
				for (int j = 0; j < fieldYRange; j++)
				{
					field[i, j] = new CCoordinates2DElement(fieldZRange, storeDepthCoordinates);
				}
			}
		}

		/// <summary>
		/// Adds coordinate to its position in field
		/// </summary>
		public void AddCoordinate(Vector3 pCoordinate)
		{
			Tuple<int, int, int> index = GetPositionInField(pCoordinate);
			field[index.Item1, index.Item2].AddCoordinate(pCoordinate, index.Item3);
			coordinatesCount++;
			//if (coordinatesCount % 1000 == 0)
			//{
			//	Console.WriteLine(index.Item1 + "," + index.Item2 + " = " + pCoordinate);
			//}
		}

		private int GetNumberOfDefinedFields()
		{
			int count = 0;
			for (int x = 0; x < fieldXRange; x++)
			{
				for (int y = 0; y < fieldYRange; y++)
				{
					if (field[x, y].CoordinatesCount > 0) { count++; }
				}
			}
			return count;
		}

		private Vector2 GetCenterOffset()
		{
			return new Vector2(fieldXRange / 2f * stepSize, fieldYRange / 2f * stepSize);
		}

		public void ExportToObj(string pOutputFileName, EExportStrategy pStrategy = EExportStrategy.None)
		{
			Obj obj = new Obj();

			int missingCoordCount = 0;
			//prepare vertices
			for (int x = 0; x < fieldXRange; x++)
			{
				for (int y = 0; y < fieldYRange; y++)
				{
					Vertex v = new Vertex();
					float height = field[x, y].GetAverageHeight();
					//float height = field[x, y].GetMostAddedHeightAverage();
					//float height = field[x, y].GetWeightedAverage();
					//float height = field[x, y].GetHeightMax();
					//float height = field[x, y].GetHeightMin();

					if (pStrategy == EExportStrategy.FillMissingHeight)
					{
						if ((int)height == 0)
						{
							height = GetAverageHeightFromNeighbours(x, y);
						}
					}

					//create vertex only if height is defined (0 = default)
					if ((int)height != 0)
					{
						//TODO: ATTENTION! in OBJ the height value = Y, while in LAS format it is Z and X,Y are space coordinates
						v.LoadFromStringArray(new[]
						{
							"v", GetXCoordinateString(x), height.ToString(), GetYCoordinateString(y)
						});
						obj.VertexList.Add(v);
						//record the index of vertex associated with this field position
						field[x, y].VertexIndex = obj.VertexList.Count; //first index = 1 (not 0)!
					}
					else
					{
						missingCoordCount++;
					}
				}
			}
			Console.WriteLine("missingCoordCount = " + missingCoordCount);

			//generate faces
			for (int x = 0; x < fieldXRange - 1; x++)
			{
				for (int y = 0; y < fieldYRange - 1; y++)
				{
					//create face only if all necessary vertices has been defined. -1 = not defined
					//| /|	3:[0,1]	2:[1,1]
					//|/ |  1:[0,0] 4:[1,0]
					//we create 2 faces: (1,2,3) and (1,2,4) 
					int ind1 = field[x, y].VertexIndex;
					if (ind1 != -1)
					{
						int ind2 = field[x + 1, y + 1].VertexIndex;
						if (ind2 != -1)
						{
							int ind3 = field[x, y + 1].VertexIndex;
							if (ind3 != -1)
							{
								Face f = new Face();
								f.LoadFromStringArray(new[]
								{
									"f", ind1.ToString(),ind2.ToString(),ind3.ToString()   //ind1+"//"+ind3, ind3+"//"+ind2, ind2+"//"+ind1
								});

								obj.FaceList.Add(f);
							}
							int ind4 = field[x + 1, y].VertexIndex;
							if (ind4 != -1)
							{
								Face f = new Face();
								f.LoadFromStringArray(new[]
								{
									"f", ind1.ToString(),ind4.ToString(),ind2.ToString()
								});

								obj.FaceList.Add(f);
							}
						}
					}
				}
			}


			string fileName = pOutputFileName.Length > 0 ? pOutputFileName : DEFAULT_FILENAME;
			string chosenFileName = fileName;
			string extension = ".obj";
			string path = Path.GetDirectoryName(
				System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\output\\";
			string fullPath = path + chosenFileName + extension;
			int fileIndex = 0;
			while (File.Exists(fullPath))
			{
				chosenFileName = fileName + "_" + fileIndex;
				fullPath = path + chosenFileName + extension;
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
			return "FIELD[" + fieldXRange + "," + fieldYRange + "]. " +
				   "Defined:" + GetNumberOfDefinedFields() + "/" + fieldXRange * fieldYRange + ". " +
				   "Total = " + coordinatesCount + "|" +
				   "min = " + botLeftCorner + ", max = " + topRightCorner + ", stepSize = " + stepSize;
		}

		public void DebugStringArray(EHeight pHeight)
		{
			for (int y = 0; y < fieldYRange; y++)
			{
				for (int x = 0; x < fieldXRange; x++)
				{
					float? height = 0;
					switch (pHeight)
					{
						case EHeight.Average:
							height = field[x, y].GetAverageHeight();
							break;
						case EHeight.Max:
							height = field[x, y].HeightMax;
							break;
						case EHeight.Min:
							height = field[x, y].HeightMin;
							break;
					}
					Console.Write($"{height:000.00}" + " | ");
					//Console.Write(GetRangeString(GetRangeInField(x, y)) + $"{field[x, y].GetAverageHeight():000.00}" + " | ");
				}
				Console.WriteLine(";");
			}
		}

		//PRIVATE

		/// <summary>
		/// Claculates average height from closest defined 4-neighbours 
		/// </summary>
		private float GetAverageHeightFromNeighbours(int pX, int pY)
		{
			int definedHeights = 0;
			
			float heightLeft = 0;
			for (int x = pX - 1; x >= 0; x--)
			{
				heightLeft = field[x, pY].GetAverageHeight();
				if ((int)heightLeft != 0)
				{
					definedHeights++;
					break;
				}
			}
			float heightRight = 0;
			for (int x = pX + 1; x < fieldXRange; x++)
			{
				heightRight = field[x, pY].GetAverageHeight();
				if ((int)heightRight != 0)
				{
					definedHeights++;
					break;
				}
			}

			float heightUp = 0;
			for (int y = pY - 1; y >= 0; y--)
			{
				heightUp = field[pX, y].GetAverageHeight();
				if ((int)heightUp != 0)
				{
					definedHeights++;
					break;
				}
			}
			float heightDown = 0;
			for (int y = pY + 1; y < fieldYRange; y++)
			{
				heightDown = field[pX, y].GetAverageHeight();
				if ((int)heightDown != 0)
				{
					definedHeights++;
					break;
				}
			}
			return (heightLeft + heightRight + heightUp + heightDown) / definedHeights;
		}

		private Tuple<int, int, int> GetPositionInField(Vector3 pCoordinate)
		{
			int xPos = (int)((pCoordinate.X - botLeftCorner.X) / stepSize);
			int yPos = (int)((pCoordinate.Y - botLeftCorner.Y) / stepSize);
			int zPos = (int)((pCoordinate.Z - minHeight) / stepSize);
			return new Tuple<int, int, int>(xPos, yPos, zPos);
		}

		/// <summary>
		/// Returns string for x coordinate in field moved by offset
		/// </summary>
		private string GetXCoordinateString(int pX)
		{
			return (pX * stepSize - GetCenterOffset().X).ToString();
		}

		/// <summary>
		/// Returns string for y coordinate in field moved by offset
		/// </summary>
		private string GetYCoordinateString(int pY)
		{
			//TODO: not sure why I have to use '-pY' and '+GetCenterOffset'. 
			//But result doesn't match the original file without it
			return (-pY * stepSize + GetCenterOffset().Y).ToString();
		}

		private string GetRangeString(Tuple<Vector2, Vector2> pRange)
		{
			return "<" + pRange.Item1 + "," + pRange.Item2 + "> ";
		}

		private Tuple<Vector2, Vector2> GetRangeInField(int pX, int pY)
		{
			Vector2 localMin = botLeftCorner + new Vector2(pX, pY) * stepSize;
			Vector2 localMax = botLeftCorner + new Vector2(pX, pY) * (stepSize + 1);
			return new Tuple<Vector2, Vector2>(localMin, localMax);
		}

	}

	public enum EHeight
	{
		Average,
		Max,
		Min
	}

	public enum EExportStrategy
	{
		None,
		FillMissingHeight
	}
}