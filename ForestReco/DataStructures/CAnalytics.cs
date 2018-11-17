using System;
using System.Collections.Generic;
using System.IO;

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


		public static void Write(bool pToFile)
		{
			string output = " - ANALYTICS - \n\n";
			output += $"loadedPoints = {loadedPoints} \n";
			output += $"vegePoints = {vegePoints} \n";
			output += $"groundPoints = {groundPoints} \n";
			output += $"filteredPoints = {filteredPoints} \n\n";

			output += $"arrayWidth = {arrayWidth} m\n";
			output += $"arrayHeight = {arrayHeight} m\n\n";

			output += $"detectedTrees = {detectedTrees} \n";
			output += $"trees density = 1 per {GetTreesDensity():0.00} m\xB2 \n";
			output += $"invalidTrees = {invalidTrees} ({invalidTreesAtBorder} of them at border)\n\n";

			output += $"inputAverageTreeHeight = {inputAverageTreeHeight} \n";
			output += $"averageTreeHeight = {averageTreeHeight} \n";
			output += $"maxTreeHeight = {maxTreeHeight} \n";
			output += $"minTreeHeight = {minTreeHeight} \n\n";

			output += $"loadedCheckTrees = {loadedCheckTrees} \n";
			output += $"assignedCheckTrees = {assignedCheckTrees} \n";
			output += $"invalidCheckTrees = {invalidCheckTrees} \n";

			output += $"\nERRORS\n";
			foreach (string error in errors)
			{
				output += $"- {error} \n";
			}

			CDebug.WriteLine(output);
			if (pToFile)
			{
				WriteToFile(output);
			}

			errors.Clear(); //reset, so errors dont stack with previous error
		}

		public static void WriteErrors()
		{
			if (CProjectData.mainForm == null)
			{
				return;
			}
			string progressText = CProjectData.mainForm.textProgress.Text;
			progressText += " - ERRORS: \n";
			foreach (string error in errors)
			{
				progressText += error + "\n";
			}
			CProjectData.mainForm.textProgress.Text = progressText;
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