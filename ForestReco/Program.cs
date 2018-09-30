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
			DateTime start = DateTime.Now;

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en"); ;

			CPlatformManager.platform = EPlatform.Notebook;
			//CPlatformManager.platform = EPlatform.HomePC;
			//CPlatformManager.platform = EPlatform.Tiarra;

			CProjectData.maxLinesToLoad = 30000;


			CProjectData.detectTrees = true;
			CProjectData.exportTrees = true;
			CProjectData.tryMergeTrees = true;
			CProjectData.mergeContaingTrees = false; //todo: not used anymore
			CProjectData.mergeBelongingTrees = false; //todo: not used anymore
			CProjectData.mergeGoodAddFactorTrees = true;
			CProjectData.detectInvalidTrees = true;

			CProjectData.processTrees = false; //todo: not used anymore

			CProjectData.setArray = true;
			CProjectData.fillArray = true;
			CProjectData.maxFillArrayIterations = 3;
			CProjectData.exportArray = true;
			CProjectData.groundArrayStep = 1;
			//.4f; //todo: pro větší krok (1,2,..) se array zobrazuje s velkým offsetem

			CProjectData.loadRefTrees = true;
			CProjectData.useRefTrees = true;
			CProjectData.useReducedRefTreeObjs = true;

			CProjectData.exportPoints = true;

			CProgramLoader.fileName = "BK_1000AGL_59_72_97_x90_y62";
			//CProgramLoader.fileName = "BK_1000AGL_7559_182972_37797";
			//CProgramLoader.fileName = "BK_1000AGL_classified";
			//CProgramLoader.fileName = "R7_F_1+2";
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
			
			CTreeManager.WriteResult();

			CObjExporter.ExportObjsToExport();

			Console.WriteLine("\n==============\n");
			Console.WriteLine("Press any key to exit." + " | Complete time = " + (DateTime.Now - start).TotalSeconds + " seconds");
			Console.ReadKey();
		}
	}
}
