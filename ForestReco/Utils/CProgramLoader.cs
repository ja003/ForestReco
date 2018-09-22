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

		public static string fileName;
		//@"BK_1000AGL_classified";
		//@"BK_1000AGL_cl_split_s_mezerou";
		//@"BK_1000AGL_classified_0007559_0182972";
		//@"BK_1000AGL_classified_0007559_0182972_0037797";
		//fileName = "debug_tree_04";
		//"debug_tree_03";
		//"debug_tree_06";
		//"BK_1000AGL_59_72_97_x90_y62";
		//"R2-F-1-j_fix";
		//fileName = "debug_tree_05";

		public static string[] GetFileLines()
		{
			CProjectData.saveFileName = fileName;
			//string saveFileName = "BK_1000AGL_";


			string podkladyPath = CPlatformManager.GetPodkladyPath();
			string fullFilePath = podkladyPath + @"\data-small\TXT\" + fileName + @".txt";
			string[] lines = File.ReadAllLines(fullFilePath);
			Console.WriteLine("load: " + fullFilePath + "\n");

			return lines;
		}

		public static List<Tuple<int, Vector3>> LoadParsedLines(string[] lines, bool pArray, bool pUseHeader)
		{
			float stepSize = .4f; //in meters
			if (pArray) { CProjectData.array = new CGroundArray(stepSize); }

			//store coordinates to corresponding data strucures based on their class
			const int DEFAULT_START_LINE = 19;
			int startLine = pUseHeader && CProjectData.header != null ? DEFAULT_START_LINE : 0;
			int linesToRead = lines.Length;
			//linesToRead = startLine + 500;

			bool classesCorect = true;
			List<Tuple<int, Vector3>> parsedLines = new List<Tuple<int, Vector3>>();
			if (useDebugData)
			{
				parsedLines = CDebug.GetStandartTree();
				CDebug.DefineArray(true, 0);
			}
			else
			{
				for (int i = startLine; i < Math.Min(lines.Length, linesToRead); i++)
				{
					// <class, coordinate>
					Tuple<int, Vector3> c = CLazTxtParser.ParseLine(lines[i], pUseHeader);
					//some files have different class counting. we are interested only in these classes
					if (c.Item1 != 0 && c.Item1 != 1 && c.Item1 != 5)
					{
						c = new Tuple<int, Vector3>(5, c.Item2);
						classesCorect = false;
					}
					if (c == null) { continue; }
					parsedLines.Add(c);
				}
			}

			if (!classesCorect) { Console.WriteLine("classes not correct. using default class"); }
			Console.WriteLine("parsedLines: " + parsedLines.Count);
			parsedLines.Sort((y, x) => x.Item2.Y.CompareTo(y.Item2.Y)); //sort descending by height
			Console.WriteLine("\n=======sorted========\n");
			//Console.ReadKey();
			return parsedLines;
		}

		public static void ProcessParsedLines(List<Tuple<int, Vector3>> parsedLines)
		{
			AddPointsFromLines(parsedLines);

			if (CProjectData.exportArray)
			{
				CProjectData.objsToExport.Add(CGroundFieldExporter.ExportToObj("arr", EExportStrategy.None));
			}

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

			if (CProjectData.useRefTrees)
			{
				Console.WriteLine("Add tree obj models " + " | " + DateTime.Now);

				List<Obj> trees = CRefTreeManager.GetTreeObjs();
				CProjectData.objsToExport.AddRange(trees);
			}



			if (CProjectData.exportPoints)
			{
				Obj justPoints = new Obj("points");
				CObjExporter.AddPointsToObj(ref justPoints, CProjectData.allPoints);
				CProjectData.objsToExport.Add(justPoints);
			}
		}

		private static void FillArray()
		{
			if (CProjectData.array == null)
			{
				Console.WriteLine("Error: no array to export");
				return;
			}
			Console.WriteLine("Export array" + " | " + DateTime.Now);

			int counter = 0;
			while (CProjectData.array != null && !CProjectData.array.IsAllDefined())
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
		}

		private static void AddPointsFromLines(List<Tuple<int, Vector3>> pParsedLines)
		{
			if (!CProjectData.detectTrees && !CProjectData.setArray && !CProjectData.exportPoints) { return; }

			List<Vector3> groundPoints = GetPointsOfClass(pParsedLines, 1);
			List<Vector3> vegePoints = GetPointsOfClass(pParsedLines, 5);

			DateTime processStartTime = DateTime.Now;
			Console.WriteLine("ProcessParsedLines " + pParsedLines.Count + ". Start = " + processStartTime);
			ProcessGroundPoints(groundPoints);
			ProcessVegePoints(vegePoints);

			Console.WriteLine("All points added | duration = " + (DateTime.Now - processStartTime));
		}

		private static void ProcessVegePoints(List<Vector3> vegePoints)
		{
			if (!CProjectData.detectTrees) { return; }

			for (int i = 0; i < vegePoints.Count; i++)
			{

				Vector3 point = vegePoints[i];
				CTreeManager.AddPoint(point, i);
				CProjectData.allPoints.Add(point);
			}
		}

		private static void ProcessGroundPoints(List<Vector3> groundPoints)
		{
			if (!CProjectData.setArray) { return; }

			for (int i = 0; i < groundPoints.Count; i++)
			{

				Vector3 point = groundPoints[i];
				CProjectData.array?.AddPointInField(point);

				CProjectData.allPoints.Add(point);
			}

			if (CProjectData.array == null)
			{
				//CDebug.DefineArray(true);
				CDebug.DefineArray(true, -8);
			}
			FillArray();



		}

		private static List<Vector3> GetPointsOfClass(List<Tuple<int, Vector3>> pParsedLines, int pClass)
		{
			List<Vector3> classPoints = new List<Vector3>();
			int pointsToAddCount = pParsedLines.Count;
			for (int i = 0; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				Tuple<int, Vector3> parsedLine = pParsedLines[i];

				//1 = unclassified
				//2 = ground
				//5 = high vegetation
				if (parsedLine.Item1 == pClass)
				{
					Vector3 point = parsedLine.Item2;
					classPoints.Add(point);
				}

			}
			return classPoints;
		}
	}
}
