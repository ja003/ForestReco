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

		public static string fileName;

		//@"BK_1000AGL_classified";
		//@"BK_1000AGL_cl_split_s_mezerou";
		//@"BK_1000AGL_classified_0007559_0182972";
		//@"BK_1000AGL_classified_0007559_0182972_0037797";
		//fileName = "debug_tree_04";
		//"debug_tree_03";
		//"debug_tree_06";
		//"BK_1000AGL_59_72_97_x90_y62";
		//"R2-F-1-j_fix";
		//fileName = "debug_tree_05";

		public static string[] GetFileLines()
		{
			CProjectData.saveFileName = fileName;
			//string saveFileName = "BK_1000AGL_";


			string podkladyPath = CPlatformManager.GetPodkladyPath();
			string fullFilePath = podkladyPath + @"\data-small\TXT\" + fileName + @".txt";
			string[] lines = File.ReadAllLines(fullFilePath);
			CDebug.Action("load", fullFilePath);

			return lines;
		}

		/// <summary>
		/// Reads parsed lines and loads class and point list.
		/// Result is sorted in descending order.
		/// </summary>
		public static List<Tuple<EClass, Vector3>> LoadParsedLines(string[] lines, bool pArray, bool pUseHeader)
		{
			//float stepSize = .4f; //in meters
			if (pArray)
			{
				CProjectData.array = new CGroundArray();

				//CObjPartition is dependent on Array initialization
				CObjPartition.Init();
			}


			//store coordinates to corresponding data strucures based on their class
			const int DEFAULT_START_LINE = 19;
			int startLine = pUseHeader && CProjectData.header != null ? DEFAULT_START_LINE : 0;

			if (lines.Length > CProjectData.maxLinesToLoad / 2)
			{
				CDebug.Warning("loading " + lines.Length + " lines!");
			}

			int linesToRead = lines.Length;
			if (CProjectData.useMaxLines)
			{
				linesToRead = Math.Min(CProjectData.maxLinesToLoad, lines.Length);
			}

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
					//some files have different class counting. we are interested only in classes in EClass
					if (c.Item1 == EClass.Other)
					{
						c = new Tuple<EClass, Vector3>(EClass.Vege, c.Item2);
						classesCorect = false;
					}
					if (c == null) { continue; }
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

			if (CProjectData.exportArray)
			{
				CObjPartition.AddArray();
			}

			CDebug.Count("Trees", CTreeManager.Trees.Count);

			CTreeManager.CheckAllTrees();

			//dont move invalid trees to invalid list yet, some invalid trees will be merged
			if (CProjectData.validateTrees)
			{
				CTreeManager.ValidateTrees(false, false);
			}

			CTreeManager.DebugTree(43);
			CTreeManager.DebugTree(220);


			//export before merge
			if (CProjectData.exportBeforeMerge)
			{
				CObjPartition.AddTrees(true);
				CObjPartition.AddTrees(false);
				CObjPartition.ExportPartition("_noMerge");
				CObjPartition.Init();
				CObjPartition.AddArray();
			}

			if (CProjectData.tryMergeTrees)
			{
				//try merge all (even valid)
				CTreeManager.TryMergeAllTrees(false);

				if (CProjectData.validateTrees)
				{
					//validate restrictive
					// ReSharper disable once ReplaceWithSingleAssignment.False
					bool cathegorize = false;
					if (!CProjectData.tryMergeTrees2) { cathegorize = true;}
					CTreeManager.ValidateTrees(cathegorize, true);
				}

				if (CProjectData.tryMergeTrees2)
				{
					//merge only invalid
					CTreeManager.TryMergeAllTrees(true);

					//cathegorize invalid trees
					if (CProjectData.validateTrees)
					{
						//validate restrictive
						//cathegorize invalid trees
						CTreeManager.ValidateTrees(true, true);
					}
				}

			}
			/*else
			{
				//just during testing so validation doesnt change
				if (CProjectData.validateTrees)
				{
					CTreeManager.ValidateTrees(true, false);
				}
			}*/

			CTreeManager.CheckAllTrees();

			CAnalytics.detectedTrees = CTreeManager.Trees.Count;
			CAnalytics.invalidTrees = CTreeManager.InvalidTrees.Count;
			CAnalytics.invalidTreesAtBorder = CTreeManager.GetInvalidTreesAtBorderCount();

			CDebug.Count("Trees", CTreeManager.Trees.Count);
			CDebug.Count("InvalidTrees", CTreeManager.InvalidTrees.Count);
			//CProjectData.array.DebugDetectedTrees();

			if (CProjectData.assignRefTrees)
			{
				CRefTreeManager.AssignRefTrees();
				//CProjectData.objsToExport.AddRange(trees);
				if (CProjectData.exportRefTrees) //no reason to export when no refTrees were assigned
				{
					//CRefTreeManager.ExportTrees();
					CObjPartition.AddRefTrees();
				}
			}
			if (CProjectData.exportTrees)
			{
				//CTreeManager.ExportTrees();
				CObjPartition.AddTrees(true);
				if (CProjectData.exportInvalidTrees)
				{
					CObjPartition.AddTrees(false);
				}
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

			int counter = 1;
			while (!CProjectData.array.IsAllDefined())
			{
				DateTime fillHeightsStart = DateTime.Now;

				CDebug.Count("FillMissingHeights", counter);
				CProjectData.array.FillMissingHeights(counter);
				counter++;
				if (counter > CProjectData.maxFillArrayIterations + 1)
				{
					CDebug.Error("FillMissingHeights");
					CDebug.Count("too many iterations", counter);
					break;
				}
				CDebug.Duration("FillMissingHeights", fillHeightsStart);
			}
		}

		private static void AddPointsFromLines(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			if (!CProjectData.detectTrees && !CProjectData.setArray && !CProjectData.exportPoints) { return; }

			ClassifyPoints(pParsedLines);

			DateTime processStartTime = DateTime.Now;
			CDebug.Count("ProcessParsedLines", pParsedLines.Count);
			//+ ". Start = " + processStartTime);
			ProcessGroundPoints();
			FilterVegePoints();

			CAnalytics.vegePoints = CProjectData.vegePoints.Count;
			CAnalytics.groundPoints = CProjectData.groundPoints.Count;
			CAnalytics.filteredPoints = CProjectData.fakePoints.Count;

			ProcessVegePoints();

			CDebug.Duration("All points added", processStartTime);
		}

		/// <summary>
		/// Assigns all vege points in array and filters fake points.
		/// </summary>
		private static void FilterVegePoints()
		{
			if (!CProjectData.detectTrees) { return; }

			const int debugFrequency = 10000;

			DateTime processVegePointsStart = DateTime.Now;
			CDebug.WriteLine("FilterVegePoints", true);

			DateTime previousDebugStart = DateTime.Now;

			for (int i = 0; i < CProjectData.vegePoints.Count; i++)
			{
				Vector3 point = CProjectData.vegePoints[i];
				CProjectData.array.AddPointInField(point, CGroundArray.EPointType.PreProcess);
				
				CDebug.Progress(i, CProjectData.vegePoints.Count, debugFrequency, ref previousDebugStart, "preprocessed point");		
			}
			CProjectData.array.SortPreProcessPoints();

			CDebug.Duration("FilterVegePoints", processVegePointsStart);

			CProjectData.array.FilterFakeVegePoints();
		}


		private static void ProcessVegePoints()
		{
			if (!CProjectData.detectTrees) { return; }

			//todo: check if has to be sorted somewhere else as well
			CProjectData.vegePoints.Sort((y, x) => x.Y.CompareTo(y.Y)); //sort descending by height

			const int debugFrequency = 10000;

			DateTime processVegePointsStart = DateTime.Now;
			CDebug.WriteLine("ProcessVegePoints", true);

			DateTime previousDebugStart = DateTime.Now;

			for (int i = 0; i < CProjectData.vegePoints.Count; i++)
			{
				Vector3 point = CProjectData.vegePoints[i];
				CTreeManager.AddPoint(point, i);

				CDebug.Progress(i, CProjectData.vegePoints.Count, debugFrequency, ref previousDebugStart, "added point");
				//if (i % debugFrequency == 0 && i > 0)
				//{
				//	CDebug.WriteLine("\nAdded point " + i + " out of " + CProjectData.vegePoints.Count);
				//	double lastPointBatchProcessTime = (DateTime.Now - previousDebugStart).TotalSeconds;
				//	CDebug.WriteLine("- time of last " + debugFrequency + " points = " + lastPointBatchProcessTime);

				//	//double totalTime = (DateTime.Now - previousDebugStart).TotalSeconds;
				//	float remainsRatio = (float)(CProjectData.vegePoints.Count - i) / debugFrequency;
				//	double totalSeconds = remainsRatio * lastPointBatchProcessTime;
				//	TimeSpan ts = new TimeSpan(0, 0, 0, (int)totalSeconds);
				//	string timeString = ts.Hours + " hours " + ts.Minutes + " minutes " + ts.Seconds + " seconds.";
				//	CDebug.WriteLine("- estimated time left = " + timeString + "\n");

				//	previousDebugStart = DateTime.Now;
				//}
			}
			CDebug.Duration("ProcessVegePoints", processVegePointsStart);
		}

		private static void ProcessGroundPoints()
		{
			if (!CProjectData.setArray) { return; }

			for (int i = 0; i < CProjectData.groundPoints.Count; i++)
			{
				Vector3 point = CProjectData.groundPoints[i];
				CProjectData.array?.AddPointInField(point, CGroundArray.EPointType.Ground);
			}

			if (CProjectData.array == null)
			{
				CDebug.Error("No array defined");
				CDebug.WriteLine("setting height to " + CProjectData.lowestHeight);
				CDebugData.DefineArray(true, CProjectData.lowestHeight);
			}
			if (CProjectData.fillArray)
			{
				FillArray();
			}
			if (CProjectData.smoothArray)
			{
				CProjectData.array?.SmoothenArray(1);
			}
		}

		private static void ClassifyPoints(List<Tuple<EClass, Vector3>> pParsedLines)
		{
			int pointsToAddCount = pParsedLines.Count;
			for (int i = 0; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				Tuple<EClass, Vector3> parsedLine = pParsedLines[i];
				CProjectData.AddPoint(parsedLine);
			}
		}
	}
}
