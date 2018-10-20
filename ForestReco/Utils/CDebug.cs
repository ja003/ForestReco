using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForestReco
{
	public static class CDebug
	{

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

		internal static void Error(string pText)
		{
			WriteLine("ERROR: " + pText, true);
		}

		internal static void Duration(string pText, DateTime pStartTime)
		{
			WriteLine(pText + " | duration = " + (DateTime.Now - pStartTime).TotalSeconds);
		}

		public static void Progress(int pIteration, int pMaxIteration, int pDebugFrequency, ref DateTime pPreviousDebugStart, string pText)
		{
			if (pIteration % pDebugFrequency == 0 && pIteration > 0)
			{
				WriteProgress(pIteration, pMaxIteration);

				string comment = "\n" + pText + " " + pIteration + " out of " + pMaxIteration;
				WriteLine(comment);
				double lastIterationBatchTime = (DateTime.Now - pPreviousDebugStart).TotalSeconds;
				WriteLine("- time of last " + pDebugFrequency + " = " + lastIterationBatchTime);

				//double totalTime = (DateTime.Now - previousDebugStart).TotalSeconds;
				float remainsRatio = (float)(pMaxIteration - pIteration) / pDebugFrequency;
				double totalSeconds = remainsRatio * lastIterationBatchTime;
				WriteExtimatedTimeLeft(totalSeconds, comment);
				pPreviousDebugStart = DateTime.Now;
			}

			//after last iteration set progressbar to 0.
			//next step doesnt have to use progressbar and it wouldnt get refreshed
			if (pIteration == pMaxIteration - 1)
			{
				WriteProgress(0, pMaxIteration);
			}
		}

		private static void WriteProgress(int pIteration, int pMaxIteration)
		{
			CProjectData.mainForm.progressBar.Minimum = 0;
			CProjectData.mainForm.progressBar.Maximum = pMaxIteration;
			CProjectData.mainForm.progressBar.Value = pIteration;
			Application.DoEvents();
		}

		private static string lastTextProgress;
		private static void WriteExtimatedTimeLeft(double pSecondsLeft, string pComment)
		{
			TimeSpan ts = new TimeSpan(0, 0, 0, (int)pSecondsLeft);
			string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";

			string timeLeftString =
				$"- estimated time left = {timeString}\n";
			WriteLine(timeLeftString);


			CProjectData.mainForm.textProgress.Text = lastTextProgress
				+ Environment.NewLine + pComment
				+ Environment.NewLine + timeLeftString;

			Application.DoEvents();
		}


		/*public static void TraceMessage(string message,
		[System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
		[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
		[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			System.Diagnostics.Trace.WriteLine("message: " + message);
			System.Diagnostics.Trace.WriteLine("member name: " + memberName);
			System.Diagnostics.Trace.WriteLine("source file path: " + sourceFilePath);
			System.Diagnostics.Trace.WriteLine("source line number: " + sourceLineNumber);
		}*/
		public static void Step(EProgramStep pStep)
		{
			lastTextProgress = GetStepText(pStep); CProjectData.mainForm.textProgress.Text = lastTextProgress;

			Application.DoEvents();
			Thread.Sleep(100);
		}

		private static int stepCallCount;

		private static string GetStepText(EProgramStep pStep)
		{
			stepCallCount++;
			string progress = stepCallCount + "/" +
				Enum.GetNames(typeof(EProgramStep)).Length + ": ";
			string text;
			switch (pStep)
			{
				case EProgramStep.LoadLines:
					text = "load forrest file lines";
					break;
				case EProgramStep.ParseLines:
					text = "parse forrest file lines";
					break;
				case EProgramStep.ProcessGroundPoints:
					text = "process ground points";
					break;
				case EProgramStep.ProcessVegePoints:
					text = "process vege points";
					break;
				case EProgramStep.FilterVegePoints:
					text = "filter vege points";
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

				default:
					text = "comment not specified";
					break;
			}

			return progress + text;
		}
	}

	public enum EProgramStep
	{
		LoadLines,
		ParseLines,
		ProcessGroundPoints,
		ProcessVegePoints,
		FilterVegePoints,
		ValidateTrees1,
		MergeTrees1,
		ValidateTrees2,
		MergeTrees2,
		ValidateTrees3,
		LoadReftrees,
		AssignReftrees,
		LoadCheckTrees,
		AssignCheckTrees,
		Export,
	}
}
