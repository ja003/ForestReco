using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public static class CDebug
	{

		public static void Count(string pText, int pCount)
		{
			WriteLine(pText + ": " + pCount);
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
				CDebug.WriteLine("\n" + pText + " " + pIteration + " out of " + pMaxIteration);
				double lastIterationBatchTime = (DateTime.Now - pPreviousDebugStart).TotalSeconds;
				WriteLine("- time of last " + pDebugFrequency + " = " + lastIterationBatchTime);

				//double totalTime = (DateTime.Now - previousDebugStart).TotalSeconds;
				float remainsRatio = (float)(pMaxIteration - pIteration) / pDebugFrequency;
				double totalSeconds = remainsRatio * lastIterationBatchTime;
				TimeSpan ts = new TimeSpan(0, 0, 0, (int)totalSeconds);
				string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";

				WriteLine("- estimated time left = " + timeString + "\n");

				pPreviousDebugStart = DateTime.Now;
			}
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
	}
}
