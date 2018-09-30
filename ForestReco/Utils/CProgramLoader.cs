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

		/// <summary>
		/// Reads parsed lines and loads class and point list.
		/// Result is sorted in descending order.
		/// </summary>
		public static List<Tuple<EClass, Vector3>> LoadParsedLines(string[] lines, bool pArray, bool pUseHeader)
		{
			//float stepSize = .4f; //in meters
			if (pArray) { CProjectData.array = new CGroundArray(); }

			//store coordinates to corresponding data strucures based on their class
			const int DEFAULT_START_LINE = 19;
			int startLine = pUseHeader && CProjectData.header != null ? DEFAULT_START_LINE : 0;

			if (lines.Length > CProjectData.maxLinesToLoad)
			{
				Console.WriteLine("WARNING: loading " + lines.Length + " lines!");
			}

			int linesToRead = Math.Min(CProjectData.maxLinesToLoad, lines.Length);

			bool classesCorect = true;
			List<Tuple<EClass, Vector3>> parsedLines = new List<Tuple<EClass, Vector3>>();
			if (useDebugData)
			{
				parsedLines = CDebug.GetStandartTree();
				CDebug.DefineArray(true, 0);
			}
			else
			{
				for (int i = startLine; i < linesToRead; i++)
				{
					// <class, coordinate>
					Tuple<EClass, Vector3> c = CLazTxtParser.ParseLine(lines[i], pUseHeader);
					//some files have different class counting. we are interested only in classes in EClass
					if (c.Item1 == EClass.Other)
					{
						c = new Tuple<EClass, Vector3>(EClass.Vege, c.Item2);
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

		public static void ProcessParsedLines(List<Tuple<EClass, Vector3>> parsedLines)
		{
			AddPointsFromLines(parsedLines);

			if (CProjectData.exportArray)
			{
				CProjectData.objsToExport.Add(CGroundFieldExporter.ExportToObj("arr", EExportStrategy.None));
			}

			Console.WriteLine("\nTrees = " + CTreeManager.Trees.Count);

			if (CProjectData.tryMergeTrees)
			{
				CTreeManager.TryMergeAllTrees();
			}

			if (CProjectData.tryMergeTrees)
			{
				CTreeManager.TryMergeAllTrees();
			}

			if (CProjectData.detectInvalidTrees)
			{
				CTreeManager.DetectInvalidTrees();
			}

			Console.WriteLine("\nTrees = " + CTreeManager.Trees.Count);
			Console.WriteLine("InvalidTrees = " + CTreeManager.InvalidTrees.Count);

			//todo: deprecated
			if (CProjectData.processTrees)
			{
				CTreeManager.ProcessAllTrees();
			}

			if (CProjectData.exportTrees)
			{
				CTreeManager.ExportTrees();
			}

			if (CProjectData.useRefTrees)
			{
				List<Obj> trees = CRefTreeManager.GetRefTreeObjs();
				CProjectData.objsToExport.AddRange(trees);
			}
		}

		private static void FillArray()
		{
			if (CProjectData.array == null)
			{
				Console.WriteLine("Error: no array to export");
				return;
			}

			int counter = 0;
			while (!CProjectData.array.IsAllDefined())
			{
				DateTime fillHeightsStart = DateTime.Now;

				Console.WriteLine("\nFillMissingHeights " + counter);
				CProjectData.array.FillMissingHeights();
				counter++;
				if (counter > CProjectData.maxFillArrayIterations)
				{
					Console.WriteLine("FillMissingHeights ERROR. too many iterations: " + counter);
					break;
				}
				Console.WriteLine("duration = " + (DateTime.Now - fillHeightsStart).TotalSeconds);
			}
		}

		private static void AddPointsFromLines(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			if (!CProjectData.detectTrees && !CProjectData.setArray && !CProjectData.exportPoints) { return; }

			ClassifyPoints(pParsedLines);

			DateTime processStartTime = DateTime.Now;
			Console.WriteLine("ProcessParsedLines " + pParsedLines.Count + ". Start = " + processStartTime);
			ProcessGroundPoints();
			ProcessVegePoints();

			Console.WriteLine("\nAll points added | duration = " + (DateTime.Now - processStartTime).TotalSeconds);
		}

		private static void ProcessVegePoints()
		{
			if (!CProjectData.detectTrees) { return; }

			const int debugFrequency = 10000;

			for (int i = 0; i < CProjectData.vegePoints.Count; i++)
			{
				Vector3 point = CProjectData.vegePoints[i];
				CTreeManager.AddPoint(point, i);
				if (i % debugFrequency == 0)
				{
					Console.WriteLine("\nAdded point " + i + " out of " + CProjectData.vegePoints.Count);
				}
			}

			if (CProjectData.exportPoints)
			{
				Obj vegePointsObj = new Obj("vegePoints");
				CObjExporter.AddPointsToObj(ref vegePointsObj, CProjectData.vegePoints);
				CProjectData.objsToExport.Add(vegePointsObj);
			}
		}

		private static void ProcessGroundPoints()
		{
			if (!CProjectData.setArray) { return; }

			for (int i = 0; i < CProjectData.groundPoints.Count; i++)
			{
				Vector3 point = CProjectData.groundPoints[i];
				CProjectData.array?.AddPointInField(point);
			}

			if (CProjectData.exportPoints)
			{
				Obj groundPointsObj = new Obj("groundPoints");
				CObjExporter.AddPointsToObj(ref groundPointsObj, CProjectData.groundPoints);
				CProjectData.objsToExport.Add(groundPointsObj);
			}

			if (CProjectData.array == null)
			{
				//CDebug.DefineArray(true);
				//CDebug.DefineArray(true, -12.55f); //todo: this is just to test match between source refTree
				//CDebug.DefineArray(true, -8.07f);
				Console.WriteLine("No array defined. setting height to " + CProjectData.lowestHeight);
				CDebug.DefineArray(true, CProjectData.lowestHeight);
			}
			if (CProjectData.fillArray)
			{
				FillArray();
			}
		}

		private static void ClassifyPoints(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			int pointsToAddCount = pParsedLines.Count;
			for (int i = 0; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				Tuple<EClass, Vector3> parsedLine = pParsedLines[i];
				CProjectData.AddPoint(parsedLine);
			}
		}
	}
}
