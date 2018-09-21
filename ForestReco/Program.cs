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

			//CPlatformManager.platform = EPlatform.Notebook;
			CPlatformManager.platform = EPlatform.HomePC;
			//CProgramLoader.platform = EPlatform.Tiarra;

			CProjectData.detectTrees = true;
			CProjectData.setArray = false;
			CProjectData.exportArray = false;
			CProjectData.loadRefTrees = false;
			CProjectData.useRefTrees = false;
			CProjectData.exportPoints = true;

			CProgramLoader.fileName = "BK_1000AGL_59_72_97_x90_y62";
			CProgramLoader.fileName = "R7";

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

			List<Tuple<int, Vector3>> parsedLines = CProgramLoader.LoadParsedLines(lines, CProjectData.header != null, true);
			CProgramLoader.ProcessParsedLines(parsedLines);
			
			Console.WriteLine("\n===============\n");
			CTreeManager.WriteResult();			

			Console.WriteLine("ExportObjsToExport" + " | " + DateTime.Now);
			CObjExporter.ExportObjsToExport();

			Console.WriteLine("Press any key to exit." + " | " + DateTime.Now);
			Console.ReadKey();
		}
	}
}
