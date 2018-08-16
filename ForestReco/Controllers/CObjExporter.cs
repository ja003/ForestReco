using System;
using System.Collections.Generic;
using System.IO;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public static class CObjExporter
	{
		private const string DEFAULT_FILENAME = "tree";

		public static void ExportObjs(List<Obj> pTrees, string pFileName)
		{
			string filePath = GetFileExportPath(pFileName);

			using (var outStream = File.OpenWrite(filePath))
			using (var writer = new StreamWriter(outStream))
			{
				// Write some header data
				WriteHeader(writer, pTrees);

				int vertexIndexOffset = 0;
				foreach (Obj tree in pTrees)
				{
					int thisTreeVertexIndexOffset = vertexIndexOffset;
					foreach (Vertex v in tree.VertexList)
					{
						writer.WriteLine(v.ToString(tree.GetVertexTransform()));
						vertexIndexOffset++;	
					}

					foreach (Face f in tree.FaceList)
					{
						writer.WriteLine(f.ToString(thisTreeVertexIndexOffset));
					}
				}
			}
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