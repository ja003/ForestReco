﻿using System;
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


		public static void PrepareSequence()
		{
			CSequenceController.Init();
		}

		public static void Start()
		{
			CSequenceController.SetValues();
			CProjectData.mainForm.SetStartBtnEnabled(false);

			DateTime startTime = DateTime.Now;
			abort = false;
			CProjectData.Init();
			CDebug.Init();
			CTreeManager.Init();

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en");

			//GENERAL
			CProjectData.useMaterial = true;
			CObjExporter.simplePointsObj = false;


			CMaterialManager.Init();

			try
			{

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
					Abort();
				}

				CRefTreeManager.Init();

				if (abort)
				{
					OnAborted();
					return;
				}

				List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.ParseLines(lines, CProjectData.header != null, true);
				CProgramLoader.ProcessParsedLines(parsedLines);

				//CTreeManager.AssignMaterials(); //call before export

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

				//has to be called after ExportPartition where final folder location is determined

				try
				{
					CDebug.Step(EProgramStep.Bitmap);
					if (CParameterSetter.GetBoolSettings(ESettings.exportBitmap))
					{
						CBitmapExporter.Export();
					}
				}
				catch (Exception e)
				{
					CDebug.Error("exception: " + e.Message);
				}

				CAnalytics.totalDuration = CAnalytics.GetDuration(startTime);
				CDebug.Duration("total time", startTime);

				CAnalytics.Write(true);
			}
			catch (Exception e)
			{
				CDebug.Error($"\nexception: {e.Message} \nStackTrace:{e.StackTrace}\n");
				Abort();
			}

			if (abort)
			{
				OnAborted();
				return;
			}

			CDebug.Step(EProgramStep.Done);

			if (CSequenceController.IsLastSequence())
			{
				CSequenceController.OnLastSequenceEnd();
				return;
			}

			CSequenceController.currentConfigIndex++;
			Start();

			CProjectData.mainForm.SetStartBtnEnabled(true);
		}

		public static void Abort()
		{
			CDebug.Warning("ABORT");
			if (abort) { return; }
			abort = true;
			CDebug.Step(EProgramStep.Aborting);
		}

		public static void OnAborted()
		{
			CDebug.Step(EProgramStep.Aborted);
			CAnalytics.WriteErrors();
			CProjectData.mainForm.SetStartBtnEnabled(true);
		}
	}
}