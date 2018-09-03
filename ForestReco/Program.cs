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
			CultureInfo ci = new CultureInfo("en");
			Thread.CurrentThread.CurrentCulture = ci;

			string fileName = @"BK_1000AGL_classified";
			//fileName = @"BK_1000AGL_cl_split_s_mezerou";
			//fileName = @"BK_1000AGL_classified_0007559_0182972";
			fileName = @"BK_1000AGL_classified_0007559_0182972_0037797";
			//fileName = @"debug_tree_03";
			//fileName = "R2-F-1-j_fix";

			string saveFileName = "BKAGL_59_72_97";
			//string saveFileName = "BK_1000AGL_";


			//EPlatform platform = EPlatform.Notebook;
			EPlatform platform = EPlatform.HomePC;

			string podkladyPath = CPlatformManager.GetPodkladyPath(platform);
			string[] lines = File.ReadAllLines(podkladyPath + @"\data-small\TXT\" + fileName + @".txt");

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
			int linesToRead = lines.Length;
			//linesToRead = 300;

			for (int i = 19; i < linesToRead; i++)
			{
				// <class, coordinate>
				Tuple<int, SVector3> c = CCoordinatesParser.ParseLine(lines[i], header);
				if (c == null) { continue; }

				//2 = ground
				//5 = high vegetation
				bool pForceTreePoint = true;
				if (c.Item1 == 5 || pForceTreePoint) { treeManager.AddPoint(c.Item2); }

				if (c.Item1 == 2 || c.Item1 == 5 && processCombined)
					{ combinedArray.AddPointInField(c.Item1, c.Item2); }

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
