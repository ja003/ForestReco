using System;
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

			CProgramLoader.platform = EPlatform.Notebook;
			//CProgramLoader.platform = EPlatform.HomePC;
			//CProgramLoader.platform = EPlatform.Tiarra;

			string[] lines = CProgramLoader.GetFileLines();
			CProjectData.header = new CHeaderInfo(lines);

			CTreeObjManager.Init();

			List<Tuple<int, Vector3>> parsedLines = CProgramLoader.LoadParsedLines(lines, false);
			CProgramLoader.ProcessParsedLines(parsedLines);

			//todo: replace load from array 
			//List<Obj> treeObjs = treeManager.GetTreeObjsFromField(combinedArray);
			//CObjExporter.ExportObjs(treeObjs, "trees_");

			Console.WriteLine("\n===============\n");
			CTreeManager.WriteResult();			

			Console.WriteLine("ExportObjsToExport" + " | " + DateTime.Now);
			CObjExporter.ExportObjsToExport();

			Console.WriteLine("Press any key to exit." + " | " + DateTime.Now);
			Console.ReadKey();
		}
	}
}
