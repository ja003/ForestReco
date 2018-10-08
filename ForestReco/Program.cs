﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	class Program
	{
		static void Main()
		{
			DateTime start = DateTime.Now;

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en"); ;

			CPlatformManager.platform = EPlatform.Notebook;
			//CPlatformManager.platform = EPlatform.HomePC;
			//CPlatformManager.platform = EPlatform.Tiarra;

			CProjectData.maxLinesToLoad = 3000000; //for now just informative
			CProjectData.useMaxLines = false;

			CObjPartition.partitionStep = 30;

			//ARRAY
			CProjectData.setArray = true;
			CProjectData.fillArray = true;
			CProjectData.smoothArray = true;
			CProjectData.maxFillArrayIterations = 3;
			CProjectData.exportArray = true;
			CProjectData.groundArrayStep = 1f;

			//TREES
			CProjectData.detectTrees = true;
			CProjectData.exportTrees = true;
			CProjectData.exportSimpleTreeModel = false;
			CProjectData.validateTrees = true;
			//merge
			CProjectData.tryMergeTrees = true;
			CProjectData.mergeContaingTrees = false; //todo: not used anymore
			CProjectData.mergeBelongingTrees = false; //todo: not used anymore
			CProjectData.mergeGoodAddFactorTrees = true;

			CProjectData.processTrees = false; //todo: not used anymore

			//REF TREES
			CProjectData.loadRefTrees = true;
			CProjectData.assignRefTrees = true;
			CProjectData.assignRandomRefTree = false;
			CProjectData.useReducedRefTreeObjs = true;
			CProjectData.exportRefTrees = true; 
			//source xyz-files
			CProjectData.refTreeFirst = true;
			CProjectData.refTreeLast = true;
			CProjectData.refTreeFront = true;
			CProjectData.refTreeBack = true;
			CProjectData.refTreeJehlici = true;
			CProjectData.refTreeKmeny = true;
			
			CProjectData.exportPoints = true;
			CObjExporter.simplePointsObj = false;
			CProjectData.exportRefTreePoints = false; //to debug reftree shape. WARNING: BIG FILE

			CProgramLoader.fileName = "BK_1000AGL_59_72_97_x90_y62";
			CProgramLoader.fileName = "BK_1000AGL_7559_182972_37797";
			CProgramLoader.fileName = "BK_1000AGL_classified";
			//CProgramLoader.fileName = "R7_F_1+2";
			//CProgramLoader.fileName = "R7";
			//CProgramLoader.fileName = "R7_test";
			//CProgramLoader.fileName = "R2_F_1+2";


			string[] lines = CProgramLoader.GetFileLines();



			if (CHeaderInfo.HasHeader(lines[0]))
			{
				CProjectData.header = new CHeaderInfo(lines);
			}
			else
			{
				Console.WriteLine("No header is defined");
			}

			CRefTreeManager.Init();

			List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.LoadParsedLines(lines, CProjectData.header != null, true);
			CProgramLoader.ProcessParsedLines(parsedLines);

			CTreeManager.DebugTrees();

			//CObjExporter.ExportObjsToExport();
			CObjPartition.ExportPartition();

			Console.WriteLine("\n==============\n");
			Console.WriteLine("Press any key to exit." + " | Complete time = " + (DateTime.Now - start));
			Console.ReadKey();
		}
	}
}
