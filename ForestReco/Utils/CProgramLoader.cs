using ObjParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public static class CProgramLoader
	{
		public static EPlatform platform;

		internal static string[] GetFileLines()
		{
			string fileName = @"BK_1000AGL_classified";
			fileName = @"BK_1000AGL_cl_split_s_mezerou";
			//fileName = @"BK_1000AGL_classified_0007559_0182972";
			//fileName = @"BK_1000AGL_classified_0007559_0182972_0037797";
			//fileName = "debug_tree_03";
			//fileName = "R2-F-1-j_fix";
			fileName = "BK_1000AGL_59_72_97_x90_y62";

			//string saveFileName = "BKAGL_59_72_97";
			CProjectData.saveFileName = "BKAGL_59_72_97_x90_y62_treeObjs";
			//string saveFileName = "BK_1000AGL_";


			string podkladyPath = CPlatformManager.GetPodkladyPath(platform);
			string fullFilePath = podkladyPath + @"\data-small\TXT\" + fileName + @".txt";
			string[] lines = File.ReadAllLines(fullFilePath);
			Console.WriteLine("load: " + fullFilePath + "\n");

			return lines;
		}

		public static bool useDebugData = false;

		internal static List<Tuple<int, Vector3>> LoadParsedLines(string[] lines)
		{
			float stepSize = .4f; //in meters
			CProjectData.array = new CPointArray(stepSize);

			//store coordinates to corresponding data strucures based on their class
			const int startLine = 19;
			int linesToRead = lines.Length;
			//linesToRead = startLine + 500;

			List<Tuple<int, Vector3>> parsedLines = new List<Tuple<int, Vector3>>();
			if (useDebugData)
			{
				parsedLines = CDebug.GetTreeStraight2();
			}
			else
			{
				for (int i = startLine; i < Math.Min(lines.Length, linesToRead); i++)
				{
					// <class, coordinate>
					Tuple<int, Vector3> c = CLazTxtParser.ParseLine(lines[i], CProjectData.header);
					if (c == null) { continue; }
					parsedLines.Add(c);
				}
			}

			Console.WriteLine("parsedLines: " + parsedLines.Count);
			parsedLines.Sort((y, x) => x.Item2.Z.CompareTo(y.Item2.Z)); //sort descending by height
			Console.WriteLine("\n=======sorted========\n");
			//Console.ReadKey();
			return parsedLines;
		}

		internal static void ProcessParsedLines(List<Tuple<int, Vector3>> parsedLines)
		{
			Console.WriteLine("ProcessParsedLines " + parsedLines.Count);
			bool processArray = false;
			CPointArray array = CProjectData.array;

			int pointsToAddCount = parsedLines.Count;
			for (int i = 0; i < Math.Min(parsedLines.Count, pointsToAddCount); i++)
			{
				Tuple<int, Vector3> parsedLine = parsedLines[i];
				Vector3 point = parsedLine.Item2;
				float tmpY = point.Y;
				point.Y = point.Z;
				point.Z = tmpY;

				//2 = ground
				//5 = high vegetation
				bool pForceTreePoint = true;
				if (parsedLine.Item1 == 5 || pForceTreePoint)
				{
					CTreeManager.AddPoint(point, i);
				}
				if(!useDebugData)
					array.AddPointInField(parsedLine.Item1, point);

				/*if (processArray && (parsedLine.Item1 == 2 || parsedLine.Item1 == 5))
				{
					array.AddPointInField(parsedLine.Item1, parsedLine.Item2);
				}*/

				CProjectData.allPoints.Add(point);
			}
			Obj treesObj = new Obj("trees_");

			Console.WriteLine("Add trees to export " + CTreeManager.Trees.Count);
			foreach (CTree t in CTreeManager.Trees)
			{
				Obj tObj = t.GetObj("tree_" + CTreeManager.Trees.IndexOf(t), true, true);
				CProjectData.objsToExport.Add(tObj);
			}

			bool addTreeObjModels = true;
			if (addTreeObjModels && !useDebugData)
			{
				Console.WriteLine("Add tree obj models");

				int counter = 0;
				while (!array.IsAllDefined(EHeight.GroundMax))
				{
					Console.WriteLine("FillMissingHeights " + counter);
					array.FillMissingHeights(EHeight.GroundMax);
					counter++;
					if(counter > 10){
						Console.WriteLine("FillMissingHeights ERROR. too many iterations: " + counter);
						break;
					}
				}
				List<Obj> trees = CTreeObjManager.GetTreeObjs();
				//CProjectData.objsToExport.AddRange(trees);
			}

			bool exportArray = true;
			if (exportArray && !useDebugData)
			{
				Console.WriteLine("Export array");
				CProjectData.objsToExport.Add(
					CPointFieldExporter.ExportToObj("arr", EExportStrategy.FillMissingHeight, new List<EHeight> { EHeight.GroundMax }));
			}

			bool exportBasic = true;
			if (exportBasic)
			{
				Obj justPoints = new Obj("points");
				CObjExporter.AddPointsToObj(ref justPoints, CProjectData.allPoints);
				CProjectData.objsToExport.Add(justPoints);
			}

			if (processArray)
			{
				Console.WriteLine("Process array: " + array);
				array.FillMissingHeights(EHeight.GroundMax);
				array.FillMissingHeights(EHeight.GroundMax);
				array.CalculateLocalExtrems();
				array.AssignTreesToNeighbourFields();
				array.AssignPointsToTrees();

				Obj treePoints = CPointFieldExporter.ExportTreePointsToObj(array, CProjectData.saveFileName + "Tree points");
				CProjectData.objsToExport.Add(treePoints);

				//select tree models based on trees in array
				//todo: uncomment to add tree models from loaded db
				//List<Obj> trees = treeObjManager.GetTreeObjsFromField(combinedArray);
				//objsToExport.AddRange(trees);

				//combinedArray.AssignTreesToAllFields();

				//combinedArray.ExportToObj(saveFileName + "_comb",
				//	EExportStrategy.None, new List<EHeight> { EHeight.GroundMax });

				Obj field = CPointFieldExporter.ExportToObj(CProjectData.saveFileName + "_ground",
					EExportStrategy.None, new List<EHeight> { EHeight.GroundMax });
				CProjectData.objsToExport.Add(field);
			}
		}
	}
}
