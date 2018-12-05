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

		public static bool simplePointsObj;

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
				
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v3 }));

				//create 4-side representation of point - otherwise just triangle - big data save
				if (!simplePointsObj)
				{
					Vertex v4 = new Vertex(clonePoint + Vector3.UnitY * POINT_OFFSET, obj.GetNextVertexIndex());
					obj.AddVertex(v4); obj.FaceList.Add(new Face(new List<Vertex> {v1, v2, v4}));
					obj.FaceList.Add(new Face(new List<Vertex> {v1, v3, v4}));
					obj.FaceList.Add(new Face(new List<Vertex> {v2, v3, v4}));
				}
			}
		}

		public static void AddTreePointsBBToObj(ref Obj obj, List<CTreePoint> pTreePoints)
		{
			foreach (CTreePoint p in pTreePoints)
			{
				//big performance improve and space reduction
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

		public static void AddLineToObj(ref Obj obj, Vector3 pFrom, Vector3 pTo, float pWidthMultiply = 1)
		{
			MoveToCenter(ref pFrom);
			float pointOffset = POINT_OFFSET * pWidthMultiply;

			Vertex v1 = new Vertex(pFrom, obj.GetNextVertexIndex());
			obj.AddVertex(v1);
			Vertex v2 = new Vertex(pFrom + Vector3.UnitX * pointOffset, obj.GetNextVertexIndex());
			obj.AddVertex(v2);
			Vertex v3 = new Vertex(pFrom + Vector3.UnitZ * pointOffset, obj.GetNextVertexIndex());
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

			//create 4-side representation of point
			obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v4 }));
			obj.FaceList.Add(new Face(new List<Vertex> { v2, v3, v4 }));
			obj.FaceList.Add(new Face(new List<Vertex> { v3, v1, v4 }));

			//obj.FaceList.Add(new Face(new List<Vertex> { v4, v5, v3 }));
			//obj.FaceList.Add(new Face(new List<Vertex> { v5, v6, v3 }));
			//obj.FaceList.Add(new Face(new List<Vertex> { v6, v4, v3 }));
		}

		public static void AddLFaceToObj(ref Obj obj, Vector3 pPoint1, Vector3 pPoint2, Vector3 pPoint3)
		{
			MoveToCenter(ref pPoint1);
			MoveToCenter(ref pPoint2);
			MoveToCenter(ref pPoint3);
			float pointOffset = POINT_OFFSET;

			Vertex v1 = new Vertex(pPoint1, obj.GetNextVertexIndex());
			obj.AddVertex(v1);
			Vertex v2 = new Vertex(pPoint2, obj.GetNextVertexIndex());
			obj.AddVertex(v2);
			Vertex v3 = new Vertex(pPoint3, obj.GetNextVertexIndex());
			obj.AddVertex(v3);
			
			obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v3 }));
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
		
		public static void ExportObj(Obj pObj, string pFileName)
		{
			ExportObjs(new List<Obj> { pObj }, pFileName, "");
		}

		public static void ExportObjs(List<Obj> pObjs, string pFileName, string pFolderPath)
		{
			string filePath = GetFileExportPath(pFileName, pFolderPath);

			using (var outStream = File.OpenWrite(filePath))
			using (var writer = new StreamWriter(outStream))
			{
				// Write some header data
				WriteHeader(writer, pObjs);

				if (CProjectData.useMaterial)
				{
					writer.WriteLine(CMaterialManager.materials);
					CMaterialManager.materials.WriteMtlFile(pFolderPath, new[] {"materials"});
				}

				int vertexIndexOffset = 0;
				foreach (Obj obj in pObjs)
				{
					if (CProjectData.backgroundWorker.CancellationPending) { return; }

					if (obj == null)
					{
						CDebug.WriteLine("Error: obj is null...WTF!");
						continue;
					}
					writer.WriteLine("o " + obj.Name);

					int thisTreeVertexIndexOffset = vertexIndexOffset;
					foreach (Vertex v in obj.VertexList)
					{
						string vertexString = v.ToString(obj.GetVertexTransform());
						writer.WriteLine(vertexString);
						vertexIndexOffset++;
					}

					if (CProjectData.useMaterial)
					{
						writer.WriteLine("usemtl " + obj.UseMtl);
					}

					foreach (Face f in obj.FaceList)
					{
						writer.WriteLine(f.ToString(thisTreeVertexIndexOffset));
					}
				}
			}
			CDebug.WriteLine("Exported to " + filePath, true);
		}

		public static string CreateFolder(string pFileName)
		{
			string path = CParameterSetter.GetStringSettings(ESettings.outputFolderPath);

			int folderIndex = 0;
			string chosenFolderName = path + "\\" + pFileName + "_" + folderIndex;
			while (Directory.Exists(chosenFolderName))
			{
				chosenFolderName = path + "\\" + pFileName + "_" + folderIndex;
				folderIndex++;
			}
			Directory.CreateDirectory(chosenFolderName);
			return chosenFolderName + "\\";
		}

		private static string GetFileExportPath(string pFileName, string pFolder)
		{
			string fileName = pFileName.Length > 0 ? pFileName : DEFAULT_FILENAME;
			string chosenFileName = fileName;
			string extension = ".Obj";
			string path = pFolder;
			if (!Directory.Exists(path))
			{
				CDebug.Error("Given folder does not exist! " + pFolder);
				path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\output\\";
			}
			
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