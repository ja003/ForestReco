using ForestReco;
using ObjParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public static class CProgramLoader
	{
		public static bool useDebugData = false;

		public static string[] GetFileLines()
		{
			CDebug.Step(EProgramStep.LoadLines);

			CProjectData.saveFileName = GetFileName(CParameterSetter.GetStringSettings(ESettings.forestFilePath));

			string fullFilePath = CParameterSetter.GetStringSettings(ESettings.forestFilePath);
			string[] lines = File.ReadAllLines(fullFilePath);
			CDebug.Action("load", fullFilePath);

			return lines;
		}

		public static string[] GetFileLines(string pFile, int pLines)
		{

			string fullFilePath = CParameterSetter.GetStringSettings(ESettings.forestFilePath);
			if (!File.Exists(fullFilePath)) { return null; }

			string[] lines = new string[pLines];

			int count = 0;
			using (StreamReader sr = File.OpenText(pFile))
			{
				string s = "";
				while ((s = sr.ReadLine()) != null && count < pLines)
				{
					lines[count] = s;
					count++;
				}
			}

			return lines;
		}

		private static string GetFileName(string pFullFilePath)
		{
			string[] filePathSplit = pFullFilePath.Split('\\');
			if (filePathSplit.Length < 3)
			{
				CDebug.Error($"Wrong file path format: {pFullFilePath}");
				return "";
			}
			string fileNameAndType = filePathSplit[filePathSplit.Length - 1];
			string fileName = fileNameAndType.Split('.')[0];

			return fileName;
		}

		/// <summary>
		/// Reads parsed lines and loads class and point list.
		/// Result is sorted in descending order.
		/// </summary>
		public static List<Tuple<EClass, Vector3>> ParseLines(string[] lines, bool pArray, bool pUseHeader)
		{
			CDebug.Step(EProgramStep.ParseLines);

			if (pArray)
			{
				CProjectData.array = new CGroundArray(CParameterSetter.groundArrayStep);
				float detailStepSize = CGroundArray.GetStepSizeForWidth(800);
				CProjectData.detailArray = new CGroundArray(detailStepSize);

				//CObjPartition is dependent on Array initialization
				CObjPartition.Init();
			}


			//store coordinates to corresponding data strucures based on their class
			const int DEFAULT_START_LINE = 19;
			int startLine = pUseHeader && CProjectData.header != null ? DEFAULT_START_LINE : 0;

			CDebug.Warning("loading " + lines.Length + " lines!");

			int linesToRead = lines.Length;

			bool classesCorect = true;
			List<Tuple<EClass, Vector3>> parsedLines = new List<Tuple<EClass, Vector3>>();
			if (useDebugData)
			{
				parsedLines = CDebugData.GetStandartTree();
				CDebugData.DefineArray(true, 0);
			}
			else
			{
				for (int i = startLine; i < linesToRead; i++)
				{
					// <class, coordinate>
					Tuple<EClass, Vector3> c = CLazTxtParser.ParseLine(lines[i], pUseHeader);
					if (c == null) { continue; }
					//some files have different class counting. we are interested only in classes in EClass
					if (c.Item1 == EClass.Other)
					{
						c = new Tuple<EClass, Vector3>(EClass.Vege, c.Item2);
						classesCorect = false;
					}
					parsedLines.Add(c);
				}
			}

			if (!classesCorect) { CDebug.WriteLine("classes not correct. using default class"); }
			CDebug.Count("parsedLines", parsedLines.Count);

			//parsedLines.Sort((y, x) => x.Item2.Y.CompareTo(y.Item2.Y)); //sort descending by height
			return parsedLines;
		}

		public static void ProcessParsedLines(List<Tuple<EClass, Vector3>> parsedLines)
		{
			CAnalytics.loadedPoints = parsedLines.Count;
			AddPointsFromLines(parsedLines);

			CObjPartition.AddArray();

			CDebug.Count("Trees", CTreeManager.Trees.Count);

			CTreeManager.CheckAllTrees();

			CDebug.Step(EProgramStep.ValidateTrees1);
			//dont move invalid trees to invalid list yet, some invalid trees will be merged
			CTreeManager.ValidateTrees(false, false);

			//export before merge
			if (CProjectData.exportBeforeMerge)
			{
				CTreeManager.AssignMaterials(); //call before export

				CObjPartition.AddTrees(true);
				CObjPartition.AddTrees(false);
				CObjPartition.ExportPartition("_noMerge");
				CObjPartition.Init();
				CObjPartition.AddArray();
			}

			CAnalytics.firstDetectedTrees = CTreeManager.Trees.Count;

			CDebug.Step(EProgramStep.MergeTrees1);
			//try merge all (even valid)
			if (CProjectData.tryMergeTrees)
			{
				CTreeManager.TryMergeAllTrees(false);
			}
			CAnalytics.afterFirstMergedTrees = CTreeManager.Trees.Count;

			//validate restrictive
			// ReSharper disable once ReplaceWithSingleAssignment.False
			bool cathegorize = false;
			if (!CProjectData.tryMergeTrees2) { cathegorize = true; }

			CDebug.Step(EProgramStep.ValidateTrees2);
			CTreeManager.ValidateTrees(cathegorize, true);

			if (CProjectData.tryMergeTrees2)
			{
				//merge only invalid
				CDebug.Step(EProgramStep.MergeTrees2);
				CTreeManager.TryMergeAllTrees(true);

				CDebug.Step(EProgramStep.ValidateTrees3);
				//validate restrictive
				//cathegorize invalid trees
				CTreeManager.ValidateTrees(true, true, true);
			}

			CTreeManager.CheckAllTrees();

			CAnalytics.detectedTrees = CTreeManager.Trees.Count;
			CAnalytics.invalidTrees = CTreeManager.InvalidTrees.Count;
			CAnalytics.invalidTreesAtBorder = CTreeManager.GetInvalidTreesAtBorderCount();

			CAnalytics.inputAverageTreeHeight = CTreeManager.AVERAGE_TREE_HEIGHT;
			CAnalytics.averageTreeHeight = CTreeManager.GetAverageTreeHeight();
			CAnalytics.maxTreeHeight = CTreeManager.GetMaxTreeHeight();
			CAnalytics.minTreeHeight = CTreeManager.GetMinTreeHeight();

			CDebug.Count("Trees", CTreeManager.Trees.Count);
			CDebug.Count("InvalidTrees", CTreeManager.InvalidTrees.Count);
			//CProjectData.array.DebugDetectedTrees();

			CTreeManager.AssignMaterials();

			CDebug.Step(EProgramStep.AssignReftrees);
			CReftreeManager.AssignRefTrees();
			if (CParameterSetter.GetBoolSettings(ESettings.exportRefTrees)) //no reason to export when no refTrees were assigned
			{
				//CRefTreeManager.ExportTrees();
				CObjPartition.AddRefTrees();
			}

			CObjPartition.AddTrees(true);
			if (CParameterSetter.GetBoolSettings(ESettings.exportInvalidTrees))
			{
				CObjPartition.AddTrees(false);
			}
		}

		private static void FillArray()
		{
			CDebug.WriteLine("FillArray", true);
			if (CProjectData.array == null)
			{
				CDebug.Error("no array to export");
				return;
			}

			DateTime fillAllHeightsStart = DateTime.Now;

			int counter = 1;
			while (!CProjectData.array.IsAllDefined())
			{
				if (CProjectData.backgroundWorker.CancellationPending) { return; }

				DateTime fillHeightsStart = DateTime.Now;

				CDebug.Count("FillMissingHeights", counter);
				CProjectData.array.FillMissingHeights(counter);
				counter++;
				const int maxFillArrayIterations = 5;
				if (counter > maxFillArrayIterations + 1)
				{
					CDebug.Error("FillMissingHeights");
					CDebug.Count("too many iterations", counter);
					break;
				}
				CDebug.Duration("FillMissingHeights", fillHeightsStart);
			}
			CAnalytics.fillAllHeightsDuration = CAnalytics.GetDuration(fillAllHeightsStart);
			CDebug.Duration("fillAllHeights", fillAllHeightsStart);
		}

		private static void AddPointsFromLines(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			ClassifyPoints(pParsedLines);

			DateTime processStartTime = DateTime.Now;
			CDebug.Count("ProcessParsedLines", pParsedLines.Count);

			CDebug.Step(EProgramStep.ProcessGroundPoints);
			ProcessGroundPoints();
			CDebug.Step(EProgramStep.PreprocessVegePoints);
			PreprocessVegePoints();


			CAnalytics.vegePoints = CProjectData.vegePoints.Count;
			CAnalytics.groundPoints = CProjectData.groundPoints.Count;
			CAnalytics.filteredPoints = CProjectData.fakePoints.Count;

			CDebug.Step(EProgramStep.ProcessVegePoints);
			ProcessVegePoints();

			CDebug.Duration("All points added", processStartTime);
		}

		/// <summary>
		/// Assigns all vege points in array and filters fake points.
		/// </summary>
		private static void PreprocessVegePoints()
		{
			const int debugFrequency = 10000;

			DateTime PreprocessVegePointsStart = DateTime.Now;
			CDebug.WriteLine("PreprocessVegePoints", true);

			DateTime preprocessVegePointsStart = DateTime.Now;
			DateTime previousDebugStart = DateTime.Now;

			for (int i = 0; i < CProjectData.vegePoints.Count; i++)
			{
				if (CProjectData.backgroundWorker.CancellationPending) { return; }

				Vector3 point = CProjectData.vegePoints[i];
				CProjectData.array.AddPointInField(point, CGroundArray.EPointType.Preprocess, true);

				CDebug.Progress(i, CProjectData.vegePoints.Count, debugFrequency, ref previousDebugStart, preprocessVegePointsStart, "preprocessed point");
			}
			CProjectData.array.SortPreProcessPoints();

			CDebug.Duration("PreprocessVegePoints", PreprocessVegePointsStart);

			//determine average tree height
			if (CParameterSetter.GetBoolSettings(ESettings.autoAverageTreeHeight))
			{
				CTreeManager.AVERAGE_TREE_HEIGHT = CProjectData.array.GetAveragePreProcessVegeHeight();
				if (float.IsNaN(CTreeManager.AVERAGE_TREE_HEIGHT))
				{
					CDebug.Error("AVERAGE_TREE_HEIGHT = NaN. using input value");
					CTreeManager.AVERAGE_TREE_HEIGHT = CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh);
				}
			}
			else
			{
				CTreeManager.AVERAGE_TREE_HEIGHT = CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh);
			}

			if (CParameterSetter.GetBoolSettings(ESettings.filterPoints))
			{
				CProjectData.array.FilterFakeVegePoints();
			}
		}

		/// <summary>
		/// Assigns vege poins to trees. Handled in TreeManager
		/// </summary>
		private static void ProcessVegePoints()
		{
			CProjectData.vegePoints.Sort((y, x) => x.Y.CompareTo(y.Y)); //sort descending by height

			const int debugFrequency = 10000;

			DateTime processVegePointsStart = DateTime.Now;
			CDebug.WriteLine("ProcessVegePoints", true);

			DateTime previousDebugStart = DateTime.Now;

			for (int i = 0; i < CProjectData.vegePoints.Count; i++)
			{
				if (CProjectData.backgroundWorker.CancellationPending) { return; }

				Vector3 point = CProjectData.vegePoints[i];
				CTreeManager.AddPoint(point, i);

				CDebug.Progress(i, CProjectData.vegePoints.Count, debugFrequency, ref previousDebugStart, processVegePointsStart, "added point");
			}
			CAnalytics.processVegePointsDuration = CAnalytics.GetDuration(processVegePointsStart);
			CDebug.Duration("ProcessVegePoints", processVegePointsStart);
		}

		/// <summary>
		/// Assigns ground points into arrays (main and detailed for precess and later bitmap generation).
		/// Fills missing heights in the array and applies smoothing.
		/// </summary>
		private static void ProcessGroundPoints()
		{
			for (int i = 0; i < CProjectData.groundPoints.Count; i++)
			{
				if (CProjectData.backgroundWorker.CancellationPending) { return; }

				Vector3 point = CProjectData.groundPoints[i];
				CProjectData.array?.AddPointInField(point, CGroundArray.EPointType.Ground, true);
				//some points can be at border of detail array - not error -> dont log
				CProjectData.detailArray?.AddPointInField(point, CGroundArray.EPointType.Ground, false);
			}

			if (CProjectData.array == null)
			{
				CDebug.Error("No array defined");
				CDebug.WriteLine("setting height to " + CProjectData.lowestHeight);
				CDebugData.DefineArray(true, CProjectData.lowestHeight);
			}

			FillArray();

			CProjectData.array?.SmoothenArray(1);
		}

		private static void ClassifyPoints(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			int pointsToAddCount = pParsedLines.Count;
			for (int i = 0; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				Tuple<EClass, Vector3> parsedLine = pParsedLines[i];
				CProjectData.AddPoint(parsedLine);
			}
			if (CProjectData.vegePoints.Count == 0)
			{
				throw new Exception("no vegetation point loaded!");
			}
			if (CProjectData.groundPoints.Count == 0)
			{
				throw new Exception("no ground point loaded!");
			}
		}
	}
}
