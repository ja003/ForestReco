using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

namespace ForestReco
{
	public static class CProgramStarter
	{
		public static bool abort;
		
		public static void Start()
		{
			DateTime start = DateTime.Now;
			abort = false;
			CProjectData.Init();
			CDebug.Init();
			CTreeManager.Init();

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en"); ;

			//CPlatformManager.platform = EPlatform.Notebook;
			CPlatformManager.platform = EPlatform.HomePC;
			//CPlatformManager.platform = EPlatform.Tiarra;
			
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
			CProjectData.loadRefTrees = true;
			CProjectData.assignRandomRefTree = true;
			CProjectData.useReducedRefTreeObjs = true;
			CProjectData.exportRefTrees = true;

			//CHECK TREES
			CProjectData.loadCheckTrees = true;
			CProjectData.exportCheckTrees = true;

			//GENERAL
			CProjectData.useMaterial = true;
			CProjectData.exportPoints = true;
			CObjExporter.simplePointsObj = false;

			CProgramLoader.forrestFullFilePath = "BK_1000AGL_59_72_97_x90_y62";
			//CProgramLoader.forrestFullFilePath = "BK_1000AGL_7559_182972_37797";
			//CProgramLoader.forrestFullFilePath = "BK_1000AGL_range_for_checktrees";
			//CProgramLoader.forrestFullFilePath = "BK_1000AGL_checktreesPart1";
			//CProgramLoader.forrestFullFilePath = "BK_1000AGL_helpTreeRange_02_32";

			//CProgramLoader.forrestFullFilePath = "BK_1000AGL_classified";
			//CProgramLoader.forrestFullFilePath = "R7_F_1+2";
			//CProgramLoader.forrestFullFilePath = "R7";
			//CProgramLoader.forrestFullFilePath = "R7_test";
			//CProgramLoader.forrestFullFilePath = "R2_F_1+2";

			CProgramLoader.forrestFullFilePath = CParameterSetter.forrestFilePath;

			CCheckTreeManager.checkFileName = "vysledek_export_UTM33N";


			CMaterialManager.Init();


			string[] lines = CProgramLoader.GetFileLines();

			if (abort)
			{
				OnAborted();
				return;
			}

			if (CHeaderInfo.HasHeader(lines[0]))
			{
				CProjectData.header = new CHeaderInfo(lines);
			}
			else
			{
				CDebug.Error("No header is defined");
			}

			CRefTreeManager.Init();

			if (abort)
			{
				OnAborted();
				return;
			}

			List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.ParseLines(lines, CProjectData.header != null, true);
			CProgramLoader.ProcessParsedLines(parsedLines);

			if (abort)
			{
				OnAborted();
				return;
			}

			//has to be called after array initialization
			CCheckTreeManager.Init();

			if (abort)
			{
				OnAborted();
				return;
			}

			CTreeManager.DebugTrees();

			//CObjExporter.ExportObjsToExport();
			CDebug.Step(EProgramStep.Export);
			CObjPartition.ExportPartition();

			CAnalytics.Write();

			CDebug.Step(EProgramStep.Done);

		}

		public static void Abort()
		{
			CDebug.Warning("ABORT");
			if(abort){ return;}
			abort = true;
			CDebug.Step(EProgramStep.Aborting);
		}

		public static void OnAborted()
		{
			CDebug.Step(EProgramStep.Aborted);
		}
	}
}