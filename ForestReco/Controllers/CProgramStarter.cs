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
		public static void PrepareSequence()
		{
			CSequenceController.Init();
		}

		public static EProcessResult Start()
		{
			CSequenceController.SetValues();

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

				bool linesOk = lines != null && lines.Length > 0 && !string.IsNullOrEmpty(lines[0]);
				if (linesOk && CHeaderInfo.HasHeader(lines[0]))
				{
					CProjectData.header = new CHeaderInfo(lines);
				}
				else
				{
					CDebug.Error("No header is defined");
					throw new Exception("No header is defined");
				}

				CReftreeManager.Init();
				if (CProjectData.backgroundWorker.CancellationPending) { return EProcessResult.Cancelled; }

				List<Tuple<EClass, Vector3>> parsedLines = CProgramLoader.ParseLines(lines, CProjectData.header != null, true);
				CProgramLoader.ProcessParsedLines(parsedLines);

				if (CProjectData.backgroundWorker.CancellationPending) { return EProcessResult.Cancelled; }


				//has to be called after array initialization
				CCheckTreeManager.Init();
				if (CProjectData.backgroundWorker.CancellationPending) { return EProcessResult.Cancelled; }

				CTreeManager.DebugTrees();

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

				CDartTxt.Export();

			}
			catch (Exception e)
			{
				CDebug.Error(
					$"{Environment.NewLine}exception: {e.Message} {Environment.NewLine}{Environment.NewLine}" +
					$"StackTrace: {e.StackTrace}{Environment.NewLine}");
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