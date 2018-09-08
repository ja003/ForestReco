using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public static class CObjExporter
	{
		private const string DEFAULT_FILENAME = "tree";

		public static void ExportPoints(List<Vector3> pPoints, string pFileName)
		{
			Obj obj = new Obj(pFileName);
			//obj.Position = peak;
			int vertexIndex = 1;
			SVector3 arrayCenter = (CProjectData.header.GetBotLeftCorner() + CProjectData.header.GetTopRightCorner()) / 2;
			

			foreach (Vector3 p in pPoints)
			{
				Vector3 clonePoint = new Vector3(p.X, p.Y, p.Z);
				clonePoint -= arrayCenter.ToVector3(true);
				clonePoint += new Vector3(0, -(float)CProjectData.header.GetMinHeight(), -2 * clonePoint.Z);

				List<Vertex> pointVertices = new List<Vertex>();

				Vertex v1 = new Vertex(clonePoint, vertexIndex);
				pointVertices.Add(v1);
				vertexIndex++;


				Vertex v2 = new Vertex(clonePoint + Vector3.UnitX * CTree.POINT_OFFSET, vertexIndex);
				pointVertices.Add(v2);
				vertexIndex++;

				Vertex v3 = new Vertex(clonePoint + Vector3.UnitZ * CTree.POINT_OFFSET, vertexIndex);
				pointVertices.Add(v3);
				vertexIndex++;

				Vertex v4 = new Vertex(clonePoint + Vector3.UnitY * CTree.POINT_OFFSET, vertexIndex);
				pointVertices.Add(v4);
				vertexIndex++;

				foreach (Vertex v in pointVertices)
				{
					obj.VertexList.Add(v);
				}

				//create 4-side representation of point
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v3 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v3, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v2, v3, v4 }));
			}
			obj.updateSize();

			ExportObj(obj, pFileName);
		}

		public static void ExportObjsToExport()
		{
			ExportObjs(CProjectData.objsToExport, CProjectData.saveFileName);
			bool exportBasic = true;
			if (exportBasic)
			{
				CObjExporter.ExportPoints(CProjectData.allPoints, CProjectData.saveFileName + "_basec");
			}
		}

		public static void ExportObj(Obj pObj, string pFileName)
		{
			ExportObjs(new List<Obj> {pObj}, pFileName);
		}

		public static void ExportObjs(List<Obj> pObjs, string pFileName)
		{
			string filePath = GetFileExportPath(pFileName);

			using (var outStream = File.OpenWrite(filePath))
			using (var writer = new StreamWriter(outStream))
			{
				// Write some header data
				WriteHeader(writer, pObjs);

				int vertexIndexOffset = 0;
				foreach (Obj obj in pObjs)
				{
					writer.WriteLine("o " + obj.Name);
					//writer.WriteLine("o XX");

					int thisTreeVertexIndexOffset = vertexIndexOffset;
					foreach (Vertex v in obj.VertexList)
					{
						writer.WriteLine(v.ToString(obj.GetVertexTransform()));
						vertexIndexOffset++;	
					}

					foreach (Face f in obj.FaceList)
					{
						writer.WriteLine(f.ToString(thisTreeVertexIndexOffset));
					}
				}
			}
			Console.WriteLine("Exported to " + filePath);
		}

		private static string GetFileExportPath(string pFileName)
		{
			string fileName = pFileName.Length > 0 ? pFileName : DEFAULT_FILENAME;
			string chosenFileName = fileName;
			string extension = ".Obj";
			string path = Path.GetDirectoryName(
				              System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\output\\trees\\";
			string fullPath = path + chosenFileName + extension;
			int fileIndex = 0;
			while (File.Exists(fullPath))
			{
				chosenFileName = fileName + "_" + fileIndex;
				fullPath = path + chosenFileName + extension;
				fileIndex++;
			}
			return fullPath;
		}


		private static void WriteHeader(StreamWriter pWriter, List<Obj> pTrees)
		{
			pWriter.WriteLine("# Exporting " + pTrees.Count + " trees.");
		}
	}
}