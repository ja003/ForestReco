using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public static class CGroundFieldExporter
	{
		private const string DEFAULT_FILENAME = "try";

		public static Obj ExportToObj(string pArrayName, EExportStrategy pStrategy, bool pUseSmoothHeight, 
			Tuple<int, int> pStartIndex, Tuple<int, int> pEndIndex)
		{
			CGroundArray pArray = CProjectData.array;
			Obj obj = new Obj(pArrayName);
			float minHeight = CProjectData.GetMinHeight();

			int missingCoordCount = 0;

			int xStart = pStartIndex.Item1;
			int yStart = pStartIndex.Item2;
			int xEnd = pEndIndex.Item1;
			int yEnd = pEndIndex.Item2;

			xEnd = Math.Min(xEnd, pArray.arrayXRange);
			yEnd = Math.Min(yEnd, pArray.arrayYRange);

			//prepare vertices
			for (int x = xStart; x < xEnd; x++)
			{
				for (int y = yStart; y < yEnd; y++)
				{
					Vertex v = new Vertex();
					CGroundField el = pArray.GetElement(x, y);
					float? height = el.GetHeight(pUseSmoothHeight);
					bool coord = false;
					if (coord) { height = y; minHeight = 0; }

					if (pStrategy == EExportStrategy.FillMissingHeight)
					{
						if (height == null)
						{
							//height = el.GetHeight(EHeight.GroundMax, true);
							height = el.GetAverageHeightFromClosestDefined(3, false);
						}
					}
					else if (pStrategy == EExportStrategy.FillHeightsAroundDefined)
					{
						if (height == null && el.IsAnyNeighbourDefined())
						{
							height = el.GetHeight();
							//Console.WriteLine("Fill " + el + " = " + height);
						}
					}
					else if (pStrategy == EExportStrategy.ZeroAroundDefined)
					{
						if (height == null && el.IsAnyNeighbourDefined())
						{
							//height = el.GetHeight(true) - 1;
							height = 0;
							//Console.WriteLine("Fill " + el + " = " + height);
						}
					}
					else if (pStrategy == EExportStrategy.CoordHeights)
					{
						height = y;
					}

					//create vertex only if height is defined
					if (height != null)
					{
						//TODO: ATTENTION! in OBJ the height value = Y, while in LAS format it is Z and X,Y are space coordinates
						//move heights so the lowest point touches the 0
						//if (pHeight != EHeight.Tree)
						{
							height -= minHeight;
						}

						v.LoadFromStringArray(new[]{"v", pArray.GetFieldXCoord(x).ToString(),
								height.ToString(), pArray.GetFieldZCoord(y).ToString()});
						obj.AddVertex(v);
						//record the index of vertex associated with this field position
						el.VertexIndex = obj.VertexList.Count; //first index = 1 (not 0)!
					}
					else
					{
						missingCoordCount++;
						//Console.WriteLine("missing = " + el);
					}
				}
			}
			//Console.WriteLine("missingCoordCount = " + missingCoordCount);
			int faceCount = 0;
			//generate faces
			for (int x = xStart; x < xEnd - 1; x++)
			{
				for (int y = yStart; y < yEnd - 1; y++)
				{
					//create face only if all necessary vertices has been defined. -1 = not defined
					//| /|	3:[0,1]	2:[1,1]
					//|/ |  1:[0,0] 4:[1,0]
					//we create 2 faces: (1,2,3) and (1,2,4) 
					int ind1 = pArray.GetElement(x, y).VertexIndex;
					if (ind1 != -1)
					{
						int ind2 = pArray.GetElement(x + 1, y + 1).VertexIndex;
						if (ind2 != -1)
						{
							int ind3 = pArray.GetElement(x, y + 1).VertexIndex;
							if (ind3 != -1)
							{
								Face f = new Face();
								f.LoadFromStringArray(new[]
								{
									"f", ind1.ToString(),ind2.ToString(),ind3.ToString()   //ind1+"//"+ind3, ind3+"//"+ind2, ind2+"//"+ind1
								});

								obj.FaceList.Add(f);
								faceCount++;
							}
							int ind4 = pArray.GetElement(x + 1, y).VertexIndex;
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
			//Console.WriteLine("faceCount = " + faceCount);

			return obj;
			//WriteObjFile(pOutputFileName, obj);
		}

		private static void WriteObjFile(string pOutputFileName, ObjParser.Obj pObj)
		{
			string fileName = pOutputFileName.Length > 0 ? pOutputFileName : DEFAULT_FILENAME;
			string chosenFileName = fileName;
			string extension = ".Obj";
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
			//path = myDocumentPath + "\\try.pObj";
			//Console.WriteLine("write to " + path);

			pObj.WriteObjFile(fullPath, new[] { "ExportTreePointsToObj" });
		}
	}
}