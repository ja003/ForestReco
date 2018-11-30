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

		public static void Progress(int pIteration, int pMaxIteration, int pDebugFrequency, ref DateTime pPreviousDebugStart, DateTime pStart, string pText)
		{
			if (pIteration % pDebugFrequency == 0 && pIteration > 0)
			{
				//WriteProgress(pIteration, pMaxIteration);

				string comment = "\n" + pText + " " + pIteration + " out of " + pMaxIteration;
				WriteLine(comment);
				double lastIterationBatchTime = (DateTime.Now - pPreviousDebugStart).TotalSeconds;
				WriteLine("- time of last " + pDebugFrequency + " = " + lastIterationBatchTime);

				double timeFromStart = (DateTime.Now - pStart).TotalSeconds;
				WriteLine($"- total time = {timeFromStart}");

				//double totalTime = (DateTime.Now - previousDebugStart).TotalSeconds;
				float remainsRatio = ((float)pMaxIteration / pIteration);
				double estimatedTotalSeconds = remainsRatio * timeFromStart;

				int percentage = pIteration * 100 / pMaxIteration;

				WriteExtimatedTimeLeft(percentage, estimatedTotalSeconds - timeFromStart, comment);
				pPreviousDebugStart = DateTime.Now;
			}

			//after last iteration set progressbar to 0.
			//next step doesnt have to use progressbar and it wouldnt get refreshed
			if (pIteration == pMaxIteration - 1)
			{
				WriteExtimatedTimeLeft(100, 0, "done");
			}
		}

		/*private static void WriteProgress(int pIteration, int pMaxIteration)
		{
			//CProjectData.mainForm.progressBar.Minimum = 0;
			//CProjectData.mainForm.progressBar.Maximum = pMaxIteration;
			//CProjectData.mainForm.progressBar.Value = pIteration;
			CProjectData.backgroundWorker.ReportProgress(pIteration * 100 / pMaxIteration);
		}*/

		private static string lastTextProgress;
		private static void WriteExtimatedTimeLeft(int pPercentage, double pSecondsLeft, string pComment)
		{
			TimeSpan ts = new TimeSpan(0, 0, 0, (int)pSecondsLeft);
			string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";

			string timeLeftString =
				$"- estimated time left = {timeString}\n";
			WriteLine(timeLeftString);

			CProjectData.backgroundWorker.ReportProgress(pPercentage, new[]
			{
				lastTextProgress , pComment , timeLeftString
			});

			//CProjectData.mainForm.textProgress.Text = lastTextProgress
			//	+ Environment.NewLine + pComment
			//	+ Environment.NewLine + timeLeftString;
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
			lastTextProgress = GetStepText(pStep);
			string[] message = new[] { lastTextProgress };
			if (pStep == EProgramStep.Exception)
			{
				CAnalytics.WriteErrors();
				return;
			}

			CProjectData.backgroundWorker.ReportProgress(0, message);
			//CProjectData.mainForm.textProgress.Text = lastTextProgress;

			//Application.DoEvents();
			//Thread.Sleep(100);
		}

		public static void WriteProblems(List<string> problems)
		{
			string message = "Problems:" + Environment.NewLine;

			foreach (string p in problems)
			{
				message += p + Environment.NewLine;
			}
			WriteLine(message);
			CProjectData.backgroundWorker.ReportProgress(0, new string[] { message });

			//CProjectData.mainForm.textProgress.Text = message;
		}


		//private static List<EProgramStep> calledSteps = new List<EProgramStep>();

		private static string GetStepText(EProgramStep pStep)
		{
			if (pStep == EProgramStep.Exception)
			{
				return "EXCEPTION";
			}

			//calledSteps.Add(pStep);

			stepCallCount++;
			int maxSteps = (Enum.GetNames(typeof(EProgramStep)).Length - 2);
			stepCallCount = Math.Min(stepCallCount, maxSteps); //bug: sometimes writes higher value
			string progress = stepCallCount + "/" + maxSteps + ": ";
			//-2 for abort states
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

			//foreach (EProgramStep step in calledSteps)
			//{
			//	WriteLine(step.ToString());
			//}

			return progress + text;
		}
	}

	public enum EProgramStep
	{
		LoadLines = 1,
		LoadReftrees = 2,
		ParseLines = 3,
		ProcessGroundPoints = 4,
		FilterVegePoints = 5,
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
