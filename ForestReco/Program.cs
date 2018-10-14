using System;
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

			//CPlatformManager.platform = EPlatform.Notebook;
			//CPlatformManager.platform = EPlatform.HomePC;
			CPlatformManager.platform = EPlatform.Tiarra;

			CProjectData.maxLinesToLoad = 3000000; //for now just informative
			CProjectData.useMaxLines = false;

			CObjPartition.partitionStep = 40;

			//Input params
			CTreeManager.AVERAGE_TREE_HEIGHT = 15;
			CTreeManager.TREE_EXTENT_MERGE_MULTIPLY = 1.5F;
			CTreeManager.BASE_TREE_EXTENT = 1.5f;
			CTreeManager.MIN_TREE_EXTENT = 0.5f;

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
			CProjectData.exportInvalidTrees = true;
			CProjectData.exportSimpleTreeModel = false;
			CProjectData.validateTrees = true;
			//merge
			CProjectData.tryMergeTrees = false;
			CProjectData.mergeOnlyInvalidTrees = true;
			CProjectData.mergeContaingTrees = false; //todo: not used anymore
			CProjectData.mergeBelongingTrees = false; //todo: not used anymore
			CProjectData.mergeGoodAddFactorTrees = true;

			CProjectData.processTrees = false; //todo: not used anymore

			//REF TREES
			CProjectData.loadRefTrees = false;
			CProjectData.assignRefTrees = false;
			CProjectData.assignRandomRefTree = true;
			CProjectData.useReducedRefTreeObjs = true;
			CProjectData.exportRefTrees = true;

			//CHECK TREES
			CProjectData.loadCheckTrees = false;
			CProjectData.exportCheckTrees = true;

			//source xyz-files
			CProjectData.refTreeFirst = true;
			CProjectData.refTreeLast = true;
			CProjectData.refTreeFront = true;
			CProjectData.refTreeBack = true;
			CProjectData.refTreeJehlici = true;
			CProjectData.refTreeKmeny = true;

			//GENERAL
			CProjectData.useMaterial = true;
			CProjectData.exportPoints = true;
			CObjExporter.simplePointsObj = false;
			CProjectData.exportRefTreePoints = false; //to debug reftree shape. WARNING: BIG FILE

			CProgramLoader.fileName = "BK_1000AGL_59_72_97_x90_y62";
			CProgramLoader.fileName = "BK_1000AGL_7559_182972_37797";
			//CProgramLoader.fileName = "BK_1000AGL_range_for_checktrees";
			//CProgramLoader.fileName = "BK_1000AGL_checktreesPart1";

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

			CDebug.WriteLine("\n==============\n");
			CDebug.Duration("Press any key to exit. Complete time = ", start);
			Console.ReadKey();
		}
	}
}
