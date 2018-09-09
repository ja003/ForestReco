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
		public const float POINT_OFFSET = 0.05f;

		private static SVector3 arrayCenter => CProjectData.header.Center;
		private static float minHeight => CProjectData.header.MinHeight;

		public static void ExportPoints(List<Vector3> pPoints, string pFileName)
		{
			Obj obj = new Obj(pFileName);
			AddPointsToObj(ref obj, pPoints);
			ExportObj(obj, pFileName);
		}

		public static void AddPointsToObj(ref Obj obj, List<Vector3> pPoints)
		{
			foreach (Vector3 p in pPoints)
			{
				Vector3 clonePoint = new Vector3(p.X, p.Y, p.Z);
				clonePoint -= arrayCenter.ToVector3(true);
				clonePoint += new Vector3(0, -CProjectData.header.MinHeight, -2 * clonePoint.Z);

				Vertex v1 = new Vertex(clonePoint, obj.GetNextVertexIndex());
				obj.VertexList.Add(v1);

				Vertex v2 = new Vertex(clonePoint + Vector3.UnitX * POINT_OFFSET, obj.GetNextVertexIndex());
				obj.VertexList.Add(v2);

				Vertex v3 = new Vertex(clonePoint + Vector3.UnitZ * POINT_OFFSET, obj.GetNextVertexIndex());
				obj.VertexList.Add(v3);

				Vertex v4 = new Vertex(clonePoint + Vector3.UnitY * POINT_OFFSET, obj.GetNextVertexIndex());
				obj.VertexList.Add(v4);

				//create 4-side representation of point
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v3 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v3, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v2, v3, v4 }));
			}
			obj.updateSize();
		}

		public static void AddBranchToObj(ref Obj obj, CBranch pBranch)
		{
			for (int i = 0; i < pBranch.points.Count; i++)
			{
				//for first point in branch use peak as a first point
				Vector3 p = i == 0 ? pBranch.tree.peak.Center : pBranch.points[i - 1].Center;
				p -= arrayCenter.ToVector3(true);
				p += new Vector3(0, -minHeight, -2 * p.Z);

				Vertex v1 = new Vertex(p, obj.GetNextVertexIndex());
				obj.VertexList.Add(v1);
				Vertex v2 = new Vertex(p + Vector3.UnitX * CObjExporter.POINT_OFFSET, obj.GetNextVertexIndex());
				obj.VertexList.Add(v2);
				Vertex v3 = new Vertex(p + Vector3.UnitZ * CObjExporter.POINT_OFFSET, obj.GetNextVertexIndex());
				obj.VertexList.Add(v3);

				//for first point set first point to connect to peak
				Vector3 nextP = i == 0 ? pBranch.points[0].Center : pBranch.points[i].Center;
				nextP -= arrayCenter.ToVector3(true);
				nextP += new Vector3(0, -minHeight, -2 * nextP.Z);

				Vertex v4 = new Vertex(nextP, obj.GetNextVertexIndex());
				obj.VertexList.Add(v4);
				/*Vertex v5 = new Vertex(nextP + Vector3.UnitX * POINT_OFFSET, vertexIndex);
				pointVertices.Add(v5);
				vertexIndex++;
				Vertex v6 = new Vertex(p + Vector3.UnitZ * POINT_OFFSET, vertexIndex);
				pointVertices.Add(v6);
				vertexIndex++;*/

				//Console.WriteLine("branch part " + p + " - " + nextP);

				//create 4-side representation of point
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v2, v3, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v3, v1, v4 }));

				//obj.FaceList.Add(new Face(new List<Vertex> { v4, v5, v3 }));
				//obj.FaceList.Add(new Face(new List<Vertex> { v5, v6, v3 }));
				//obj.FaceList.Add(new Face(new List<Vertex> { v6, v4, v3 }));

			}
			obj.updateSize();
		}

		public static void ExportObjsToExport()
		{			
			ExportObjs(CProjectData.objsToExport, CProjectData.saveFileName);
		}

		public static void ExportObj(Obj pObj, string pFileName)
		{
			ExportObjs(new List<Obj> { pObj }, pFileName);
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