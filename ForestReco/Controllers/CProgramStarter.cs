﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;

namespace ForestReco
{
	public static class CProgramStarter
	{

		public static void Start()
		{
			DateTime start = DateTime.Now;

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en"); ;

			//CPlatformManager.platform = EPlatform.Notebook;
			CPlatformManager.platform = EPlatform.HomePC;
			//CPlatformManager.platform = EPlatform.Tiarra;


			CObjPartition.partitionStep = 45;

			//Input params
			CTreeManager.AVERAGE_TREE_HEIGHT = 15;
			CTreeManager.TREE_EXTENT_MERGE_MULTIPLY = 1.5F; //good: 1.5 - 2 
			CTreeManager.BASE_TREE_EXTENT = 1.5f; //good: 1.5 - 2 
			CTreeManager.MIN_TREE_EXTENT = 0.5f;

			//ARRAY
			//CProjectData.setArray = true;
			//CProjectData.fillArray = true;
			CProjectData.smoothArray = true;
			//CProjectData.maxFillArrayIterations = 3;
			//CProjectData.exportArray = true;
			CProjectData.groundArrayStep = 1f;

			//TREES
			CProjectData.exportTrees = true;
			CProjectData.exportInvalidTrees = true;
			//merge
			//CProjectData.exportBeforeMerge = true;

			//REF TREES
			CProjectData.loadRefTrees = false;
			CProjectData.assignRandomRefTree = false;
			CProjectData.useReducedRefTreeObjs = true;
			CProjectData.exportRefTrees = true;

			//CHECK TREES
			CProjectData.loadCheckTrees = true;
			CProjectData.exportCheckTrees = true;

			//GENERAL
			CProjectData.useMaterial = true;
			CProjectData.exportPoints = true;
			CObjExporter.simplePointsObj = false;

			CProgramLoader.fileName = "BK_1000AGL_59_72_97_x90_y62";
			//CProgramLoader.fileName = "BK_1000AGL_7559_182972_37797";
			//CProgramLoader.fileName = "BK_1000AGL_range_for_checktrees";
			//CProgramLoader.fileName = "BK_1000AGL_checktreesPart1";
			//CProgramLoader.fileName = "BK_1000AGL_helpTreeRange_02_32";

			//CProgramLoader.fileName = "BK_1000AGL_classified";
			//CProgramLoader.fileName = "R7_F_1+2";
			//CProgramLoader.fileName = "R7";
			//CProgramLoader.fileName = "R7_test";
			//CProgramLoader.fileName = "R2_F_1+2";

			CCheckTreeManager.checkFileName = "vysledek_export_UTM33N";


			CMaterialManager.Init();

			string[] lines = CProgramLoader.GetFileLines();

			if (CHeaderInfo.HasHeader(lines[0]))
			{
				CProjectData.header = new CHeaderInfo(lines);
			}
			else
			{
				CDebug.Error("No header is defined");
			}

			CRefTreeManager.Init();


			List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.LoadParsedLines(lines, CProjectData.header != null, true);
			CProgramLoader.ProcessParsedLines(parsedLines);

			//has to be called after array initialization
			CCheckTreeManager.Init();

			CTreeManager.DebugTrees();

			//CObjExporter.ExportObjsToExport();
			CObjPartition.ExportPartition();

			CAnalytics.Write();

			CDebug.WriteLine("\n==============\n");
			CDebug.Duration("Press any key to exit. Complete time = ", start);
			Console.ReadKey();
		}
	}
}