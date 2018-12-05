using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

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

		public static int loadedReftrees;
		public static float averageReftreeSimilarity;


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

		private static string newLine => Environment.NewLine;
		private static string newLine2 => newLine + newLine;

		public static void Write(bool pToFile)
		{
			string output = " - ANALYTICS - " + newLine2;
			output += $"treeExtent = {CParameterSetter.GetFloatSettings(ESettings.treeExtent)} " + newLine;
			output += $"treeExtentMultiply = {CParameterSetter.GetFloatSettings(ESettings.treeExtentMultiply)} " + newLine2;

			output += $"loadedPoints = {loadedPoints} " + newLine;
			output += $"vegePoints = {vegePoints} " + newLine;
			output += $"groundPoints = {groundPoints} " + newLine;
			output += $"filteredPoints = {filteredPoints}" + newLine2;

			output += $"arrayWidth = {arrayWidth} m" + newLine;
			output += $"arrayHeight = {arrayHeight} m" + newLine2;

			output += $"firstDetectedTrees = {firstDetectedTrees} " + newLine;
			output += $"firstMergedCount = {GetFirstMergedCount()} " + newLine;
			output += $"secondMergedCount = {GetSecondMergedCount()} " + newLine;
			output += $"detectedTrees = {detectedTrees} " + newLine;


			output += $"trees density = 1 per {GetTreesDensity():0.00} m\xB2 " + newLine;
			output += $"invalidTrees = {invalidTrees} ({invalidTreesAtBorder} of them at border)\n" + newLine;

			output += $"inputAverageTreeHeight = {inputAverageTreeHeight} " + newLine;
			output += $"averageTreeHeight = {averageTreeHeight} " + newLine;
			output += $"maxTreeHeight = {maxTreeHeight} " + newLine;
			output += $"minTreeHeight = {minTreeHeight}" + newLine2;

			output += $"loadedReftrees = {loadedReftrees} " + newLine;
			output += $"averageReftreeSimilarity = {averageReftreeSimilarity} " + newLine2;


			output += "Duration" + newLine;
			output += $"load reftrees = {loadReftreesDuration} " + newLine;
			output += $"fill missing ground = {fillAllHeightsDuration} " + newLine;
			output += $"add vege points = {processVegePointsDuration} " + newLine;
			output += $"first merge = {firstMergeDuration} " + newLine;
			output += $"second merge = {secondMergeDuration} " + newLine;
			output += $"reftree assignment = {reftreeAssignDuration} " + newLine;
			output += $"bitmap export = {bitmapExportDuration} " + newLine;
			output += $"-------------------" + newLine;
			output += $"total = {totalDuration} " + newLine;

			if (CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile))
			{
				output += "Checktree" + newLine;
				output += $"loadedCheckTrees = {loadedCheckTrees} " + newLine;
				output += $"assignedCheckTrees = {assignedCheckTrees} " + newLine;
				output += $"invalidCheckTrees = {invalidCheckTrees} " + newLine;
			}

			output += $"\nERRORS" + newLine;
			foreach (string error in errors)
			{
				output += $"- {error} " + newLine;
			}

			//before WriteToFile (it can fail there too)
			errors.Clear(); //reset, so errors dont stack with previous error

			CDebug.WriteLine(output);
			if (pToFile)
			{
				WriteToFile(output);
				//ExportCsv(ECsvAnalytics.InputParams);
				//ExportCsv(ECsvAnalytics.ComputationTime);
				ExportCsv(ECsvAnalytics.Summary); //probably enough
			}

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
			//todo: only Summary is used in project
			InputParams,
			ComputationTime,
			Summary
		}

		private static void ExportCsv(ECsvAnalytics pType)
		{
			switch (pType)
			{
				/*case ECsvAnalytics.InputParams:
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
							reftreeAssignDuration,
							totalDuration
						},
						pType.ToString());
					break;*/

				case ECsvAnalytics.Summary:
					ExportCsv(new List<Tuple<string, object>>
						{
							new Tuple<string, object>("width", CProjectData.header.Width),
							new Tuple<string, object>("Height",CProjectData.header.Height),
							new Tuple<string, object>("treeExtent",
								CParameterSetter.GetFloatSettings(ESettings.treeExtent)),
							new Tuple<string, object>("treeExtentMultiply",
								CParameterSetter.GetFloatSettings(ESettings.treeExtentMultiply)),
							new Tuple<string, object>("loadedPoints",loadedPoints),

							new Tuple<string, object>("firstDetectedTrees",firstDetectedTrees),
							new Tuple<string, object>("FirstMergedCount",GetFirstMergedCount()),
							new Tuple<string, object>("SecondMerged",GetSecondMergedCount()),
							new Tuple<string, object>("detectedTrees",detectedTrees),

							new Tuple<string, object>("loadedReftrees",loadedReftrees),
							new Tuple<string, object>("averageReftreeSimilarity",averageReftreeSimilarity),

							new Tuple<string, object>("processVege",processVegePointsDuration),
							new Tuple<string, object>("firstMerge",firstMergeDuration),
							new Tuple<string, object>("secondMerge",secondMergeDuration),
							new Tuple<string, object>("reftreeAssign",reftreeAssignDuration),
							new Tuple<string, object>("totalDuration",totalDuration)
						},
						pType.ToString(), true);
					break;
			}
		}


		private static void ExportCsv(List<Tuple<string, object>> pParams, string pName, bool pExportGlobal = false)
		{
			string fileName = pName + ".csv";
			string filePath = CObjPartition.folderPath + "/" + fileName;
			string[] pathSplit = CObjPartition.folderPath.Split('\\');
			string folderName = pathSplit[pathSplit.Length - 2];

			string line;
			using (var outStream = File.OpenWrite(filePath))
			using (var writer = new StreamWriter(outStream))
			{
				writer.WriteLine(GetHeaderString(pParams));

				writer.Write(folderName);
				line = folderName;
				foreach (Tuple<string, object> param in pParams)
				{
					string val = "," + param.Item2;
					writer.Write(val);
					line += val;
				}
			}

			string mainSummaryFile = CParameterSetter.GetStringSettings(ESettings.analyticsFilePath);
			FileMode fileMode = FileMode.Append;
			if (!File.Exists(mainSummaryFile))
			{
				CDebug.WriteLine("analytics file not defined");
				if (!string.IsNullOrEmpty(mainSummaryFile))
				{
					fileMode = FileMode.Create;
					CDebug.WriteLine(" - creating");
				}
				else
				{
					return;
				}
			}

			//if Append => file exists
			//just check if already contains some text
			//we expect, that if it already has text, it also contains header
			bool hasHeader = fileMode == FileMode.Append && File.ReadAllLines(mainSummaryFile).Length != 0;

			using (FileStream fs = new FileStream(mainSummaryFile, fileMode, FileAccess.Write))
			using (var writer = new StreamWriter(fs))
			{
				if (!hasHeader)
				{
					writer.WriteLine(GetHeaderString(pParams));
				}
				writer.WriteLine(line);
			}
		}

		private static string GetHeaderString(List<Tuple<string, object>> pParams)
		{
			string header = "name";
			foreach (Tuple<string, object> param in pParams)
			{
				header += "," + param.Item1;
			}
			return header;
		}

		public static void WriteErrors()
		{
			string message = "ERRORS:" + Environment.NewLine;
			foreach (string error in errors)
			{
				message += error + Environment.NewLine;
			}
			CProjectData.backgroundWorker.ReportProgress(0, new string[] { message });
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