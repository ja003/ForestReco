using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForestReco
{
	public static class CDebug
	{
		private static int stepCallCount;

		public static void Init()
		{
			stepCallCount = 0;
		}

		public static void Count(string pText, int pCount, int pOutOf = -1)
		{
			WriteLine(pText + ": " + pCount + (pOutOf > 0 ? " out of " + pOutOf : ""));
		}


		public static void WriteLine(string pText, bool pBreakLineBefore = false, bool pBreakLineAfter = false)
		{
			Console.WriteLine((pBreakLineBefore ? "\n" : "") + pText + (pBreakLineAfter ? "\n" : ""));
		}

		public static void Action(string pAction, string pText)
		{
			WriteLine(pAction + ": " + pText, true);
		}



		internal static void Warning(string pText)
		{
			WriteLine("WARNING: " + pText, true);
		}

		internal static void Error(string pText, bool pWriteInAnalytics = true)
		{
			WriteLine("ERROR: " + pText, true);
			if (pWriteInAnalytics)
			{
				CAnalytics.AddError(pText);
			}
		}

		internal static void Duration(string pText, DateTime pStartTime)
		{
			double totalSeconds = CAnalytics.GetDuration(pStartTime);
			WriteLine(pText + " | duration = " + totalSeconds);
		}

		public static void Progress(int pIteration, int pMaxIteration, int pDebugFrequency, ref DateTime pPreviousDebugStart, DateTime pStart, string pText, bool pShowInConsole = false)
		{
			if (pIteration % pDebugFrequency == 0 && pIteration > 0)
			{
				
				double lastIterationBatchTime = (DateTime.Now - pPreviousDebugStart).TotalSeconds;

				double timeFromStart = (DateTime.Now - pStart).TotalSeconds;

				float remainsRatio = ((float)pMaxIteration / pIteration);
				double estimatedTotalSeconds = remainsRatio * timeFromStart;

				int percentage = pIteration * 100 / pMaxIteration;

				string comment = "\n" + pText + " " + pIteration + " out of " + pMaxIteration;
				if (pShowInConsole)
				{
					WriteLine(comment);
					WriteLine("- time of last " + pDebugFrequency + " = " + lastIterationBatchTime);
					WriteLine($"- total time = {timeFromStart}");
				}
				WriteExtimatedTimeLeft(percentage, estimatedTotalSeconds - timeFromStart, comment, pShowInConsole);
				pPreviousDebugStart = DateTime.Now;
			}

			//after last iteration set progressbar to 0.
			//next step doesnt have to use progressbar and it wouldnt get refreshed
			if (pIteration == pMaxIteration - 1)
			{
				WriteExtimatedTimeLeft(100, 0, "done", pShowInConsole);
			}
		}

		private static string lastTextProgress;
		private static void WriteExtimatedTimeLeft(int pPercentage, double pSecondsLeft, string pComment, bool pShowInConsole)
		{
			TimeSpan ts = new TimeSpan(0, 0, 0, (int)pSecondsLeft);
			string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";

			string timeLeftString =
				$"- estimated time left = {timeString}\n";
			if(pShowInConsole){WriteLine(timeLeftString);}

			CProjectData.backgroundWorker.ReportProgress(pPercentage, new[]
			{
				lastTextProgress , pComment , timeLeftString
			});
		}

		public static void Step(EProgramStep pStep)
		{
			lastTextProgress = GetStepText(pStep);
			string[] message = new[] { lastTextProgress };
			if (pStep == EProgramStep.Exception)
			{
				CAnalytics.WriteErrors();
				return;
			}

			CProjectData.backgroundWorker.ReportProgress(0, message);
		}

		public static void WriteProblems(List<string> problems)
		{
			string message = "Problems:" + Environment.NewLine;

			foreach (string p in problems)
			{
				message += p + Environment.NewLine;
			}
			WriteLine(message);
			try
			{
				CProjectData.backgroundWorker.ReportProgress(0, new string[] { message });
			}
			//should not happen
			catch (Exception e)
			{
				Error(e.Message, false);
			}
		}

		private static string GetStepText(EProgramStep pStep)
		{
			if (pStep == EProgramStep.Exception)
			{
				return "EXCEPTION";
			}

			stepCallCount++;
			//-2 for abort states
			int maxSteps = (Enum.GetNames(typeof(EProgramStep)).Length - 2);
			stepCallCount = Math.Min(stepCallCount, maxSteps); //bug: sometimes writes higher value
			string progress = stepCallCount + "/" + maxSteps + ": ";
			string text;
			switch (pStep)
			{
				case EProgramStep.LoadLines:
					text = "load forest file lines";
					break;
				case EProgramStep.ParseLines:
					text = "parse forest file lines";
					break;
				case EProgramStep.ProcessGroundPoints:
					text = "process ground points";
					break;
				case EProgramStep.ProcessVegePoints:
					text = "process vege points";
					break;
				case EProgramStep.PreprocessVegePoints:
					text = "preprocess vege points";
					break;
				case EProgramStep.ValidateTrees1:
					text = "first tree validation";
					break;
				case EProgramStep.MergeTrees1:
					text = "first tree merge";
					break;
				case EProgramStep.ValidateTrees2:
					text = "second tree validation";
					break;
				case EProgramStep.MergeTrees2:
					text = "second tree merging";
					break;
				case EProgramStep.ValidateTrees3:
					text = "final tree validation";
					break;
				case EProgramStep.LoadReftrees:
					text = "loading reftrees";
					break;
				case EProgramStep.AssignReftrees:
					text = "assigning reftrees";
					break;
				case EProgramStep.LoadCheckTrees:
					text = "loading checktrees";
					break;
				case EProgramStep.AssignCheckTrees:
					text = "assigning checktrees";
					break;
				case EProgramStep.Export:
					text = "exporting";
					break;
				case EProgramStep.Bitmap:
					text = "generating bitmaps";
					break;
				case EProgramStep.Done:
					text = "DONE";
					break;

				default:
					text = "comment not specified";
					break;
			}

			return progress + text;
		}
	}

	public enum EProgramStep
	{
		LoadLines = 1,
		LoadReftrees = 2,
		ParseLines = 3,
		ProcessGroundPoints = 4,
		PreprocessVegePoints = 5,
		ProcessVegePoints = 6,
		ValidateTrees1 = 7,
		MergeTrees1 = 8,
		ValidateTrees2 = 9,
		MergeTrees2 = 10,
		ValidateTrees3 = 11,
		AssignReftrees = 12,
		LoadCheckTrees = 13,
		AssignCheckTrees = 14,
		Export = 15,
		Bitmap = 16,
		Done = 17,

		Cancelled,
		Exception
	}
}
