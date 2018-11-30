using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

namespace ForestReco
{
	public enum EProcessResult
	{
		Cancelled,
		Exception,
		Done
	}

	public static class CProgramStarter
	{
		//public static bool abort;

		public static void PrepareSequence()
		{
			CSequenceController.Init();
		}

		

		public static EProcessResult Start()
		{
			CSequenceController.SetValues();
			//CProjectData.mainForm.SetStartBtnEnabled(false);

			DateTime startTime = DateTime.Now;
			CProjectData.Init();
			CDebug.Init();
			CTreeManager.Init();

			Thread.CurrentThread.CurrentCulture = new CultureInfo("en");

			//GENERAL
			CProjectData.useMaterial = true;
			CObjExporter.simplePointsObj = false;


			CMaterialManager.Init();

			string[] workerResult = new string[2];
			workerResult[0] = "this string";
			workerResult[1] = "some other string";
			CProjectData.backgroundWorker.ReportProgress(10, workerResult);

			try
			{

				string[] lines = CProgramLoader.GetFileLines();

				if (CProjectData.backgroundWorker.CancellationPending) { return EProcessResult.Cancelled; }

				if (CHeaderInfo.HasHeader(lines[0]))
				{
					CProjectData.header = new CHeaderInfo(lines);
				}
				else
				{
					CDebug.Error("No header is defined");
					throw new Exception("No header is defined");
				}

				CRefTreeManager.Init();
				if (CProjectData.backgroundWorker.CancellationPending) { return EProcessResult.Cancelled; }

				List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.ParseLines(lines, CProjectData.header != null, true);
				CProgramLoader.ProcessParsedLines(parsedLines);

				if (CProjectData.backgroundWorker.CancellationPending) { return EProcessResult.Cancelled; }


				//has to be called after array initialization
				CCheckTreeManager.Init();
				if (CProjectData.backgroundWorker.CancellationPending) { return EProcessResult.Cancelled; }

				CTreeManager.DebugTrees();


				//CObjExporter.ExportObjsToExport();
				CDebug.Step(EProgramStep.Export);
				CObjPartition.ExportPartition();
				if (CProjectData.backgroundWorker.CancellationPending) { return EProcessResult.Cancelled; }

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
				OnException();
				return EProcessResult.Exception;
			}

			if (CProjectData.backgroundWorker.CancellationPending)
			{
				CDebug.Step(EProgramStep.Cancelled);
				return EProcessResult.Cancelled;
			}

			CDebug.Step(EProgramStep.Done);

			if (CSequenceController.IsLastSequence())
			{
				CSequenceController.OnLastSequenceEnd();
				//CProjectData.mainForm.SetStartBtnEnabled(true);
				return EProcessResult.Done;
			}

			CSequenceController.currentConfigIndex++;
			return Start();
		}


		public static void OnException()
		{
			CDebug.Step(EProgramStep.Exception);
		}
	}
}