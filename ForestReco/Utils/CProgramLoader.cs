﻿using ObjParser;
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
		public static bool useDebugData = false;

		private static string fileName = 
		//@"BK_1000AGL_classified";
		//@"BK_1000AGL_cl_split_s_mezerou";
		//fileName = @"BK_1000AGL_classified_0007559_0182972";
		//@"BK_1000AGL_classified_0007559_0182972_0037797";
		//fileName = "debug_tree_04";
		fileName = "debug_tree_06";
		//fileName = "debug_tree_05";
		//fileName = "R2-F-1-j_fix";
		//"BK_1000AGL_59_72_97_x90_y62";

		internal static string[] GetFileLines()
		{
			CProjectData.saveFileName = fileName;
			//string saveFileName = "BK_1000AGL_";


			string podkladyPath = CPlatformManager.GetPodkladyPath();
			string fullFilePath = podkladyPath + @"\data-small\TXT\" + fileName + @".txt";
			string[] lines = File.ReadAllLines(fullFilePath);
			Console.WriteLine("load: " + fullFilePath + "\n");

			return lines;
		}


		internal static List<Tuple<int, Vector3>> LoadParsedLines(string[] lines, bool pArray, bool pUseHeader)
		{
			float stepSize = .4f; //in meters
			if (pArray) { CProjectData.array = new CPointArray(stepSize); }

			//store coordinates to corresponding data strucures based on their class
			int startLine = CProjectData.header != null ? 19 : 0;
			int linesToRead = lines.Length;
			//linesToRead = startLine + 500;

			List<Tuple<int, Vector3>> parsedLines = new List<Tuple<int, Vector3>>();
			if (useDebugData)
			{
				parsedLines = CDebug.GetStandartTree();
				CDebug.DefineArray(true);
			}
			else
			{
				for (int i = startLine; i < Math.Min(lines.Length, linesToRead); i++)
				{
					// <class, coordinate>
					Tuple<int, Vector3> c = CLazTxtParser.ParseLine(lines[i], pUseHeader);
					if (c == null) { continue; }
					parsedLines.Add(c);
				}
			}

			Console.WriteLine("parsedLines: " + parsedLines.Count);
			parsedLines.Sort((y, x) => x.Item2.Y.CompareTo(y.Item2.Y)); //sort descending by height
			Console.WriteLine("\n=======sorted========\n");
			//Console.ReadKey();
			return parsedLines;
		}

		internal static void ProcessParsedLines(List<Tuple<int, Vector3>> parsedLines)
		{
			AddPointsFromLines(parsedLines);
			
			Console.WriteLine("\nTrees = " + CTreeManager.Trees.Count);

			CTreeManager.TryMergeAllTrees();

			Console.WriteLine("\nTrees = " + CTreeManager.Trees.Count);
			
			CTreeManager.ProcessAllTrees();
			
			Console.WriteLine("\nAdd trees to export " + CTreeManager.Trees.Count + " | " + DateTime.Now);
			foreach (CTree t in CTreeManager.Trees)
			{
				Obj tObj = t.GetObj("tree_" + CTreeManager.Trees.IndexOf(t), true, false);
				CProjectData.objsToExport.Add(tObj);
			}

			bool addTreeObjModels = true;
			if (addTreeObjModels && !useDebugData)
			{
				Console.WriteLine("Add tree obj models " + " | " + DateTime.Now);

				/*int counter = 0;
				while (CProjectData.array != null && !CProjectData.array.IsAllDefined(EHeight.GroundMax))
				{
					Console.WriteLine("FillMissingHeights " + counter);
					CProjectData.array?.FillMissingHeights();
					counter++;
					if (counter > 10)
					{
						Console.WriteLine("FillMissingHeights ERROR. too many iterations: " + counter);
						break;
					}
				}*/
				if (CProjectData.array == null)
				{
					CDebug.DefineArray(true);
				}

				List<Obj> trees = CRefTreeManager.GetTreeObjs();
				CProjectData.objsToExport.AddRange(trees);
			}

			bool exportArray = true;
			if (exportArray && CProjectData.array != null)
			{
				Console.WriteLine("Export array" + " | " + DateTime.Now);

				int counter = 0;
				while (CProjectData.array != null && !CProjectData.array.IsAllDefined(EHeight.GroundMax))
				{
					Console.WriteLine("FillMissingHeights " + counter);
					CProjectData.array?.FillMissingHeights();
					counter++;
					if (counter > 2)
					{
						Console.WriteLine("FillMissingHeights ERROR. too many iterations: " + counter);
						break;
					}
				}

				CProjectData.objsToExport.Add(
					CPointFieldExporter.ExportToObj("arr", EExportStrategy.None, new List<EHeight> { EHeight.GroundMax }));
			}

			bool exportAllPoints = true;
			if (exportAllPoints)
			{
				Obj justPoints = new Obj("points");
				CObjExporter.AddPointsToObj(ref justPoints, CProjectData.allPoints);
				CProjectData.objsToExport.Add(justPoints);
			}

			bool processArray = false;
			if (processArray && CProjectData.array != null)
			{
				CPointArray array = CProjectData.array;
				Console.WriteLine("Process array: " + array);
				array.FillMissingHeights();
				array.FillMissingHeights();
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

		private static void AddPointsFromLines(List<Tuple<int, Vector3>> pParsedLines)
		{
			DateTime processStartTime = DateTime.Now;
			Console.WriteLine("ProcessParsedLines " + pParsedLines.Count + ". Start = " + processStartTime);
			int pointsToAddCount = pParsedLines.Count;
			for (int i = 0; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				DateTime lineStartTime = DateTime.Now;

				Tuple<int, Vector3> parsedLine = pParsedLines[i];
				Vector3 point = parsedLine.Item2;
				/*float tmpY = point.Y;
				point.Y = point.Z;
				point.Z = tmpY;*/

				//1 = unclassified
				//2 = ground
				//5 = high vegetation
				bool pForceTreePoint = true;
				if (parsedLine.Item1 == 5 || (pForceTreePoint && parsedLine.Item1 != 2 && parsedLine.Item1 != 1))
				{
					CTreeManager.AddPoint(point, i);
				}
				if (!useDebugData) { CProjectData.array?.AddPointInField(parsedLine.Item1, point); }

				/*if (processArray && (parsedLine.Item1 == 2 || parsedLine.Item1 == 5))
				{
					array.AddPointInField(parsedLine.Item1, parsedLine.Item2);
				}*/

				CProjectData.allPoints.Add(point);

				TimeSpan duration = DateTime.Now - lineStartTime;
				if (duration.Milliseconds > 1) { Console.WriteLine(i + ": " + duration); }
			}
			Console.WriteLine("All points added | duration = " + (DateTime.Now - processStartTime));
		}
	}
}
