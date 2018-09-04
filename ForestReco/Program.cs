﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObjParser;
using ObjParser.Types;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	class Program
	{
		static void Main()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en"); ;

			string fileName = @"BK_1000AGL_classified";
			//fileName = @"BK_1000AGL_cl_split_s_mezerou";
			//fileName = @"BK_1000AGL_classified_0007559_0182972";
			//fileName = @"BK_1000AGL_classified_0007559_0182972_0037797";
			//fileName = @"debug_tree_03";
			fileName = "R2-F-1-j_fix";


			string saveFileName = "BKAGL_59_72_97";
			//string saveFileName = "BK_1000AGL_";


			EPlatform platform = EPlatform.Notebook;
			//EPlatform platform = EPlatform.HomePC;

			string podkladyPath = CPlatformManager.GetPodkladyPath(platform);
			string fullFilePath = podkladyPath + @"\data-small\TXT\" + fileName + @".txt";
			string[] lines = File.ReadAllLines(fullFilePath);
			Console.WriteLine("load: " + fullFilePath + "\n");

			CHeaderInfo header = new CHeaderInfo(lines[15], lines[16], lines[17], lines[18]);
			Console.WriteLine(header);

			//TODO: uncommnent to see just header info
			//Console.ReadKey();
			//return;

			//prepare data structures 

			CTreeObjManager treeObjManager = new CTreeObjManager();
			List<string> treePaths = new List<string>()
			{
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\tree_dummy.obj",
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\tree_dummy_02.obj",
				//@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\tree_models\m1__2013-01-04_00-54-51.obj",
				podkladyPath + @"\tree_models\m1_reduced.obj"
			};
			//todo: uncomment to load tree obj from db
			//treeObjManager.LoadTrees(treePaths);

			//CObjExporter.ExportObjs(treeManager.Trees, "tree_dummy");
			//Console.ReadKey();
			//return;

			float stepSize = .4f; //in meters

			CPointArray combinedArray = new CPointArray(header, stepSize);

			CTreeManager treeManager = new CTreeManager();


			bool processCombined = false;

			//store coordinates to corresponding data strucures based on their class
			const int startLine = 19;
			int linesToRead = lines.Length;
			//linesToRead = startLine + 10;

			List<Tuple<int, SVector3>> parsedLines = new List<Tuple<int, SVector3>>();

			for (int i = startLine; i < linesToRead; i++)
			{
				// <class, coordinate>
				Tuple<int, SVector3> c = CLazTxtParser.ParseLine(lines[i], header);
				if (c == null) { continue; }
				parsedLines.Add(c);
			}
			Console.WriteLine("parsedLines: " + parsedLines.Count);
			parsedLines.Sort((y, x) => x.Item2.Z.CompareTo(y.Item2.Z)); //sort descending by height
			Console.WriteLine("\n=======sorted========\n");
			//Console.ReadKey();
			//return;

			foreach (Tuple<int, SVector3> pl in parsedLines)
			{
				//2 = ground
				//5 = high vegetation
				bool pForceTreePoint = true;
				if (pl.Item1 == 5 || pForceTreePoint) { treeManager.AddPoint(pl.Item2); }

				if (processCombined && (pl.Item1 == 2 || pl.Item1 == 5))
				{ combinedArray.AddPointInField(pl.Item1, pl.Item2); }

				//if (parsedLines.IndexOf(pl) % 10 == 0) { Console.ReadKey(); }
				//Console.ReadKey();
				//if(i%10000 == 0) {Console.WriteLine(c);}
			}

			List<Obj> treeObjs = treeManager.GetTreeObjsFromField(combinedArray);
			//CObjExporter.ExportObjs(treeObjs, "trees_");
			Console.WriteLine("\n===============\n");
			treeManager.WriteResult();
			//Console.ReadKey();
			//return;

			List<Obj> objsToExport = new List<Obj>();
			objsToExport.AddRange(treeObjs);
			//objsToExport.AddRange(treeManager.Trees);

			//processCombined = false;
			if (processCombined)
			{
				Console.WriteLine("combinedArray: " + combinedArray);
				combinedArray.FillMissingHeights(EHeight.GroundMax);
				combinedArray.FillMissingHeights(EHeight.GroundMax);
				combinedArray.CalculateLocalExtrems();
				combinedArray.AssignTreesToNeighbourFields();
				combinedArray.AssignPointsToTrees();

				Obj treePoints = CPointFieldExporter.ExportTreePointsToObj(combinedArray, saveFileName + "Tree points");
				objsToExport.Add(treePoints);

				//select tree models based on trees in array
				//todo: uncomment to add tree models from loaded db
				//List<Obj> trees = treeObjManager.GetTreeObjsFromField(combinedArray);
				//objsToExport.AddRange(trees);

				//combinedArray.AssignTreesToAllFields();

				//combinedArray.ExportToObj(saveFileName + "_comb",
				//	EExportStrategy.None, new List<EHeight> { EHeight.GroundMax });

				Obj field = CPointFieldExporter.ExportToObj(combinedArray, saveFileName + "_ground",
					EExportStrategy.None, new List<EHeight> { EHeight.GroundMax });
				objsToExport.Add(field);
			}
			CObjExporter.ExportObjs(objsToExport, "debug_tree");

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}
