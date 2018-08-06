using System;
using System.Collections.Generic;
using System.IO;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public static class CPointFieldExporter
	{
		private const string DEFAULT_FILENAME = "try";
		
		public static void ExportToObj(CPointArray pArray, string pOutputFileName,
			EExportStrategy pStrategy, List<EHeight> pHeights)
		{
			Obj obj = new Obj();

			int missingCoordCount = 0;

			foreach (EHeight pHeight in pHeights)
			{
				//prepare vertices
				for (int x = 0; x < pArray.arrayXRange; x++)
				{
					for (int y = 0; y < pArray.arrayYRange; y++)
					{
						Vertex v = new Vertex();
						CPointField el = pArray.GetElement(x, y);
						double? height = el.GetHeight(pHeight);

						if (pStrategy == EExportStrategy.FillMissingHeight)
						{
							if (height == null)
							{
								//height = el.GetHeight(EHeight.GroundMax, true);
								height = el.GetAverageHeightFromClosestDefined(EHeight.GroundMax);
							}
						}
						else if (pStrategy == EExportStrategy.FillHeightsAroundDefined)
						{
							if (height == null && el.IsAnyNeighbourDefined(pHeight))
							{
								height = el.GetHeight(EHeight.GroundMax, true);
								//Console.WriteLine("Fill " + el + " = " + height);
							}
						}

						//create vertex only if height is defined
						if (height != null)
						{
							//TODO: ATTENTION! in OBJ the height value = Y, while in LAS format it is Z and X,Y are space coordinates
							//move heights so the lowest point touches the 0
							//if (pHeight != EHeight.Tree)
							{
								height -= pArray.minHeight;
							}

							v.LoadFromStringArray(new[]{"v", pArray.GetXElementString(x),
								height.ToString(), pArray.GetYElementString(y)});
							obj.VertexList.Add(v);
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

				//generate faces
				for (int x = 0; x < pArray.arrayXRange - 1; x++)
				{
					for (int y = 0; y < pArray.arrayYRange - 1; y++)
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
	}
}