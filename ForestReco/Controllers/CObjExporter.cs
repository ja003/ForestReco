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

		private static Vector3 arrayCenter => CProjectData.GetArrayCenter();
		//private static float minHeight => CProjectData.GetMinHeight();

		public static void ExportPoints(List<Vector3> pPoints, string pFileName)
		{
			Obj obj = new Obj(pFileName);
			AddPointsToObj(ref obj, pPoints, Vector3.Zero);
			ExportObj(obj, pFileName);
		}

		public static void AddPointsToObj(ref Obj obj, List<Vector3> pPoints)
		{
			AddPointsToObj(ref obj, pPoints, Vector3.Zero);
		}

		public static void AddPointsToObj(ref Obj obj, List<Vector3> pPoints, Vector3 pOffset, bool pMoveToCenter = true)
		{
			foreach (Vector3 p in pPoints)
			{
				Vector3 clonePoint = p;
				if (pMoveToCenter) { MoveToCenter(ref clonePoint); }
				else { MoveByOffset(ref clonePoint, pOffset); }

				Vertex v1 = new Vertex(clonePoint, obj.GetNextVertexIndex());
				obj.AddVertex(v1);

				Vertex v2 = new Vertex(clonePoint + Vector3.UnitX * POINT_OFFSET, obj.GetNextVertexIndex());
				obj.AddVertex(v2);

				Vertex v3 = new Vertex(clonePoint + Vector3.UnitZ * POINT_OFFSET, obj.GetNextVertexIndex());
				obj.AddVertex(v3);

				Vertex v4 = new Vertex(clonePoint + Vector3.UnitY * POINT_OFFSET, obj.GetNextVertexIndex());
				obj.AddVertex(v4);

				//create 4-side representation of point
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v3 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v3, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v2, v3, v4 }));
			}
		}

		public static void AddBBToObj(ref Obj obj, List<CTreePoint> pTreePoints)
		{
			foreach (CTreePoint p in pTreePoints)
			{
				//big performance improve and space reduction
				//todo: investigate
				if(p.Points.Count < 2){ continue; }

				//bot side
				AddPointsToObj(ref obj, p.GetBBPoints(), Vector3.Zero);
				AddLineToObj(ref obj, p.b000, p.b100);
				AddLineToObj(ref obj, p.b100, p.b101);
				AddLineToObj(ref obj, p.b101, p.b001);
				AddLineToObj(ref obj, p.b001, p.b000);
				//mid edges
				AddLineToObj(ref obj, p.b000, p.b010);
				AddLineToObj(ref obj, p.b100, p.b110);
				AddLineToObj(ref obj, p.b101, p.b111);
				AddLineToObj(ref obj, p.b001, p.b011);
				//top side
				AddLineToObj(ref obj, p.b010, p.b110);
				AddLineToObj(ref obj, p.b110, p.b111);
				AddLineToObj(ref obj, p.b111, p.b011);
				AddLineToObj(ref obj, p.b011, p.b010);
			}
		}

		private static void AddLineToObj(ref Obj obj, Vector3 pFrom, Vector3 pTo)
		{
			MoveToCenter(ref pFrom);

			Vertex v1 = new Vertex(pFrom, obj.GetNextVertexIndex());
			obj.AddVertex(v1);
			Vertex v2 = new Vertex(pFrom + Vector3.UnitX * POINT_OFFSET, obj.GetNextVertexIndex());
			obj.AddVertex(v2);
			Vertex v3 = new Vertex(pFrom + Vector3.UnitZ * POINT_OFFSET, obj.GetNextVertexIndex());
			obj.AddVertex(v3);

			MoveToCenter(ref pTo);

			Vertex v4 = new Vertex(pTo, obj.GetNextVertexIndex());
			obj.AddVertex(v4);
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

		public static void AddBranchToObj(ref Obj obj, CBranch pBranch)
		{
			for (int i = 0; i < pBranch.TreePoints.Count; i++)
			{
				//for first point in branch use peak as a first point
				Vector3 p = i == 0 ? pBranch.tree.peak.Center : pBranch.TreePoints[i - 1].Center;
				//for first point set first point to connect to peak
				Vector3 nextP = i == 0 ? pBranch.TreePoints[0].Center : pBranch.TreePoints[i].Center;

				AddLineToObj(ref obj, p, nextP);
			}
		}

		public static void ExportObjsToExport()
		{
			DateTime start = DateTime.Now;
			ExportObjs(CProjectData.objsToExport, CProjectData.saveFileName);
			Console.WriteLine("Export time = " + (DateTime.Now - start).TotalSeconds + " seconds");
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

					int thisTreeVertexIndexOffset = vertexIndexOffset;
					foreach (Vertex v in obj.VertexList)
					{
						string vertexString = v.ToString(obj.GetVertexTransform());
						if (vertexString == "v 0.2059677 -0.004917747 0.1736171")
						{
							Console.WriteLine("§");
						}
						writer.WriteLine(vertexString);
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
							  System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\output\\";
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

		private static void MoveToCenter(ref Vector3 pPoint)
		{
			if (CProgramLoader.useDebugData) { return; }
			pPoint = GetMovedPoint(pPoint);
			//pPoint -= arrayCenter;
			//pPoint -= new Vector3(0, CProjectData.GetMinHeight(), 2 * pPoint.Z);
		}

		private static void MoveByOffset(ref Vector3 pPoint, Vector3 pOffset)
		{
			pPoint += pOffset;
		}

		public static Vector3 GetMovedPoint(Vector3 pPoint)
		{
			pPoint -= arrayCenter;
			pPoint -= new Vector3(0, CProjectData.GetMinHeight(), 2 * pPoint.Z);
			return pPoint;
		}

		private static void WriteHeader(StreamWriter pWriter, List<Obj> pTrees)
		{
			pWriter.WriteLine("# Exporting " + pTrees.Count + " trees.");
		}

	}
}