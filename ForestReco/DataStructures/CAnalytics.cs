using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ForestReco
{
	public static class CAnalytics
	{
		public static int loadedPoints;
		public static int vegePoints;
		public static int groundPoints;
		public static int filteredPoints;

		public static float arrayWidth;
		public static float arrayHeight;

		public static int firstDetectedTrees;
		public static int afterFirstMergedTrees;
		public static int detectedTrees;

		public static int invalidTrees;
		public static int invalidTreesAtBorder;
		public static float inputAverageTreeHeight;
		public static float averageTreeHeight;
		public static float maxTreeHeight;
		public static float minTreeHeight;

		public static int loadedCheckTrees;
		public static int assignedCheckTrees;
		public static int invalidCheckTrees;

		public static List<string> errors = new List<string>();

		//durations
		public static double loadReftreesDuration;
		public static double fillAllHeightsDuration;

		public static double processVegePointsDuration;
		public static double firstMergeDuration;
		public static double secondMergeDuration;

		public static double reftreeAssignDuration;

		public static double bitmapExportDuration;
		public static double totalDuration;

		public static void Write(bool pToFile)
		{
			string output = " - ANALYTICS - \n\n";
			output += $"loadedPoints = {loadedPoints} \n";
			output += $"vegePoints = {vegePoints} \n";
			output += $"groundPoints = {groundPoints} \n";
			output += $"filteredPoints = {filteredPoints} \n\n";

			output += $"arrayWidth = {arrayWidth} m\n";
			output += $"arrayHeight = {arrayHeight} m\n\n";

			output += $"firstDetectedTrees = {firstDetectedTrees} \n";
			output += $"firstMergedCount = {GetFirstMergedCount()} \n";
			output += $"secondMergedCount = {GetSecondMergedCount()} \n";
			output += $"detectedTrees = {detectedTrees} \n";


			output += $"trees density = 1 per {GetTreesDensity():0.00} m\xB2 \n";
			output += $"invalidTrees = {invalidTrees} ({invalidTreesAtBorder} of them at border)\n\n";

			output += $"inputAverageTreeHeight = {inputAverageTreeHeight} \n";
			output += $"averageTreeHeight = {averageTreeHeight} \n";
			output += $"maxTreeHeight = {maxTreeHeight} \n";
			output += $"minTreeHeight = {minTreeHeight} \n\n";


			output += "Duration\n";
			output += $"load reftrees = {loadReftreesDuration} \n";
			output += $"fill missing ground = {fillAllHeightsDuration} \n";
			output += $"add vege points = {processVegePointsDuration} \n";
			output += $"first merge = {firstMergeDuration} \n";
			output += $"second merge = {secondMergeDuration} \n";
			output += $"reftree assignment = {reftreeAssignDuration} \n";
			output += $"bitmap export = {bitmapExportDuration} \n";
			output += $"-------------------\n";
			output += $"total = {totalDuration} \n";
			//todo: other


			if (CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile))
			{
				output += "Checktree";
				output += $"loadedCheckTrees = {loadedCheckTrees} \n";
				output += $"assignedCheckTrees = {assignedCheckTrees} \n";
				output += $"invalidCheckTrees = {invalidCheckTrees} \n";
			}

			output += $"\nERRORS\n";
			foreach (string error in errors)
			{
				output += $"- {error} \n";
			}

			CDebug.WriteLine(output);
			if (pToFile)
			{
				WriteToFile(output);
				ExportCsv(ECsvAnalytics.InputParams);
				ExportCsv(ECsvAnalytics.ComputationTime);
			}

			errors.Clear(); //reset, so errors dont stack with previous error
		}

		private static int GetSecondMergedCount()
		{
			return afterFirstMergedTrees - detectedTrees;
		}

		private static int GetFirstMergedCount()
		{
			return firstDetectedTrees - afterFirstMergedTrees;
		}

		public enum ECsvAnalytics
		{
			InputParams,
			ComputationTime
		}

		private static void ExportCsv(ECsvAnalytics pType)
		{
			switch (pType)
			{
				case ECsvAnalytics.InputParams:
					ExportCsv(new List<object>
						{
							CProjectData.header.Width,
							CProjectData.header.Height,
							CParameterSetter.GetFloatSettings(ESettings.treeExtent),
							CParameterSetter.GetFloatSettings(ESettings.treeExtentMultiply),
							firstDetectedTrees,
							GetFirstMergedCount(),
							GetSecondMergedCount(),
							detectedTrees
						},
						pType.ToString());
					break;
				case ECsvAnalytics.ComputationTime:
					ExportCsv(new List<object>
						{
							CProjectData.header.Width,
							CProjectData.header.Height,
							loadedPoints,
							detectedTrees,
							processVegePointsDuration,
							firstMergeDuration,
							secondMergeDuration,
							totalDuration
						},
						pType.ToString());
					break;
			}
		}

		private static void ExportCsv(List<object> pObjs, string pName)
		{
			string fileName = pName + ".csv";
			string filePath = CObjPartition.folderPath + "/" + fileName;
			string[] pathSplit = CObjPartition.folderPath.Split('\\');
			string folderName = pathSplit[pathSplit.Length - 2];

			using (var outStream = File.OpenWrite(filePath))
			using (var writer = new StreamWriter(outStream))
			{
				//todo: colums name?
				/*writer.Write("file");
				foreach (object obj in pObjs)
				{
					writer.Write("," + obj.get);
				}
				writer.WriteLine(filePath);
				*/

				writer.Write(folderName);
				foreach (object obj in pObjs)
				{
					writer.Write("," + obj);
				}
			}
		}

		public static void WriteErrors()
		{
			if (CProjectData.mainForm == null)
			{
				return;
			}
			string progressText = CProjectData.mainForm.textProgress.Text;
			progressText += "\n - ERRORS: \n";
			foreach (string error in errors)
			{
				progressText += error + "\n";
			}
			CProjectData.mainForm.textProgress.Text = progressText;
		}

		public static double GetDuration(DateTime pStartTime)
		{
			return (DateTime.Now - pStartTime).TotalSeconds;
		}

		internal static void AddError(string pText)
		{
			errors.Add(pText);
		}

		private static void WriteToFile(string pText)
		{
			string fileName = "analytics.txt";
			string filePath = CObjPartition.folderPath + "/" + fileName;
			using (var outStream = File.OpenWrite(filePath))
			using (var writer = new StreamWriter(outStream))
			{
				writer.Write(pText);
			}
		}

		private static float GetTreesDensity()
		{
			float area = arrayHeight * arrayWidth;
			float density = area / detectedTrees;
			return density;
		}
	}
}