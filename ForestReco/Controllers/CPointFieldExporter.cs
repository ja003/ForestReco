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


		public static void ExportToObj(CPointField pField, string pOutputFileName,
			EExportStrategy pStrategy, List<EHeight> pHeights, double pMinHeight)
		{
			Obj obj = new Obj();

			int missingCoordCount = 0;

			foreach (EHeight pHeight in pHeights)
			{
				//prepare vertices
				for (int x = 0; x < pField.fieldXRange; x++)
				{
					for (int y = 0; y < pField.fieldYRange; y++)
					{
						Vertex v = new Vertex();
						CPointElement el = pField.GetElement(x, y);
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
							if (height == null && el.IsAnyNeigbourDefined(pHeight))
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
								height -= pMinHeight;
							}

							v.LoadFromStringArray(new[]{"v", pField.GetXElementString(x),
								height.ToString(), pField.GetYElementString(y)});
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
				for (int x = 0; x < pField.fieldXRange - 1; x++)
				{
					for (int y = 0; y < pField.fieldYRange - 1; y++)
					{
						//create face only if all necessary vertices has been defined. -1 = not defined
						//| /|	3:[0,1]	2:[1,1]
						//|/ |  1:[0,0] 4:[1,0]
						//we create 2 faces: (1,2,3) and (1,2,4) 
						int ind1 = pField.GetElement(x, y).VertexIndex;
						if (ind1 != -1)
						{
							int ind2 = pField.GetElement(x + 1, y + 1).VertexIndex;
							if (ind2 != -1)
							{
								int ind3 = pField.GetElement(x, y + 1).VertexIndex;
								if (ind3 != -1)
								{
									Face f = new Face();
									f.LoadFromStringArray(new[]
									{
									"f", ind1.ToString(),ind2.ToString(),ind3.ToString()   //ind1+"//"+ind3, ind3+"//"+ind2, ind2+"//"+ind1
								});

									obj.FaceList.Add(f);
								}
								int ind4 = pField.GetElement(x + 1, y).VertexIndex;
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