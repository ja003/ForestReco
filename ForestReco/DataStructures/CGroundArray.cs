using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Web.WebSockets;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace ForestReco
{
	/// <summary>
	/// Field orientation is from topLeft -> botRight, topLeft = [0,0]
	/// </summary>
	public class CGroundArray
	{
		private CGroundField[,] array;
		public List<CGroundField> fields { get; private set; }

		public float stepSize { get; }
		public int arrayXRange { get; }
		public int arrayYRange { get; }

		Vector3 botLeftCorner;
		Vector3 topRightCorner;
		Vector3 topLeftCorner;

		private Vector3 CenterOffset;

		//--------------------------------------------------------------

		public CGroundArray(float pStepSize)
		{
			stepSize = pStepSize;

			botLeftCorner = CProjectData.header.BotLeftCorner;
			topRightCorner = CProjectData.header.TopRightCorner;
			topLeftCorner = new Vector3(botLeftCorner.X, 0, topRightCorner.Z);

			float width = topRightCorner.X - botLeftCorner.X;
			float height = topRightCorner.Z - botLeftCorner.Z;

			arrayXRange = (int)(width / stepSize) + 1;
			arrayYRange = (int)(height / stepSize) + 1;

			CenterOffset = new Vector3(arrayXRange / 2f * stepSize, 0, arrayYRange / 2f * stepSize);
			CenterOffset += new Vector3(-stepSize / 2, 0, -stepSize / 2); //better visualization

			array = new CGroundField[arrayXRange, arrayYRange];
			fields = new List<CGroundField>();
			for (int x = 0; x < arrayXRange; x++)
			{
				for (int y = 0; y < arrayYRange; y++)
				{
					CGroundField newGroundField = new CGroundField(new Tuple<int, int>(x, y),
						new Vector3(
							topLeftCorner.X + x * stepSize + stepSize / 2, 0,
							topLeftCorner.Z - y * stepSize - stepSize / 2));
					array[x, y] = newGroundField;
					fields.Add(newGroundField);
				}
			}
			for (int x = 0; x < arrayXRange; x++)
			{
				for (int y = 0; y < arrayYRange; y++)
				{
					if (x > 0)
					{
						array[x, y].Left = array[x - 1, y];
					}
					if (x < arrayXRange - 1)
					{
						array[x, y].Right = array[x + 1, y];
					}
					if (y > 0)
					{
						array[x, y].Top = array[x, y - 1]; //orig
					}
					if (y < arrayYRange - 1)
					{
						array[x, y].Bot = array[x, y + 1]; //orig
					}
				}
			}

			CAnalytics.arrayWidth = width;
			CAnalytics.arrayHeight = height;

		}

		///GETTER
		public CGroundField GetElement(int pXindex, int pYindex)
		{
			if (!IsWithinBounds(pXindex, pYindex)) { return null; }
			return array[pXindex, pYindex];
		}

		private bool IsWithinBounds(Tuple<int, int> pIndex)
		{
			return IsWithinBounds(pIndex.Item1, pIndex.Item2);
		}

		private bool IsWithinBounds(int pXindex, int pYindex)
		{
			return pXindex >= 0 && pXindex < arrayXRange && pYindex >= 0 && pYindex < arrayYRange;
		}

		public CGroundField GetElementContainingPoint(Vector3 pPoint)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			if (!IsWithinBounds(index))
			{
				if (index.Item1 == -1) { index = new Tuple<int, int>(0, index.Item2);}
				if (index.Item2 == -1) { index = new Tuple<int, int>(index.Item1, 0); }
				if (index.Item1 == arrayXRange) { index = new Tuple<int, int>(arrayXRange - 1, index.Item2); }
				if (index.Item2 == arrayYRange) { index = new Tuple<int, int>(index.Item1, arrayYRange - 1); }

				if (!IsWithinBounds(index))
				{
					return null;
				}
				else
				{
					//todo: some points are 1 index away from the range
					CDebug.Error($"pPoint {pPoint} was OOB and was moved to {index}", false);
				}
			}
			return array[index.Item1, index.Item2];
		}

		public float? GetHeight(Vector3 pPoint)
		{
			return GetElementContainingPoint(pPoint).GetHeight(pPoint);
		}

		private Tuple<int, int> GetPositionInField(Vector3 pPoint)
		{
			int xPos = (int)Math.Floor((pPoint.X - topLeftCorner.X) / stepSize);
			//due to array orientation
			int yPos = (int)Math.Floor((topLeftCorner.Z - pPoint.Z) / stepSize);

			CGroundField el = GetElement(xPos, yPos);
			if (el != null && el.IsPointOutOfField(pPoint))
			{
				CDebug.Error($"point {pPoint} is too far from center {el.center}");
			}

			return new Tuple<int, int>(xPos, yPos);
		}

		//PUBLIC

		public enum EPointType
		{
			Ground,
			Vege,
			Preprocess
		}

		public void AddPointInField(Vector3 pPoint, EPointType pType, bool pLogErrorInAnalytics)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			if (!IsWithinBounds(index))
			{
				CDebug.Error($"point {pPoint} is OOB {index}", pLogErrorInAnalytics);
				return;
			}
			switch (pType)
			{
				case EPointType.Ground:
					array[index.Item1, index.Item2].AddGroundPoint(pPoint);
					break;
				case EPointType.Vege:
					array[index.Item1, index.Item2].AddVegePoint(pPoint);
					break;
				case EPointType.Preprocess:
					array[index.Item1, index.Item2].AddPreProcessVegePoint(pPoint);
					break;
			}
		}

		public void SortPreProcessPoints()
		{
			foreach (CGroundField field in fields)
			{
				field.SortPreProcessPoints();
			}
		}

		public void FilterFakeVegePoints()
		{
			//clear vege points - they will be added back in field.ApplyFilteredPoints()
			CProjectData.vegePoints.Clear();

			float averageHeight = GetAveragePreProcessVegeHeight();

			foreach (CGroundField field in fields)
			{
				field.FilterFakeVegePoints(averageHeight);
			}
			
			foreach (CGroundField field in fields)
			{
				field.ApplyFilteredPoints();
			}

			Console.WriteLine($"total =  {GetFakePointsCount()}");
		}
		
		private int GetFakePointsCount()
		{
			int count = 0;
			foreach (CGroundField f in fields)
			{
				count += f.fakePoints.Count;
			}
			return count;
		}

		//range of vege height that will be counted in average vege height
		//reason is to restrict undefined or too outOfNorm values to affect average height
		const float MIN_PREPROCESS_VEGE_HEIGHT = 5;
		const float MAX_PREPROCESS_VEGE_HEIGHT = 35;
		const float PREPROCESS_VEGE_HEIGHT_OFFSET = 1;

		public float GetAveragePreProcessVegeHeight()
		{
			float sumHeight = 0;
			int definedCount = 0;
			
			foreach (CGroundField field in fields)
			{
				float? groundHeight = field.GetHeight();
				float? preProcessVegeHeight = field.MaxPreProcessVege;
				if (preProcessVegeHeight != null && groundHeight != null)
				{
					float vegeHeight = (float)preProcessVegeHeight - (float)groundHeight;
					if (vegeHeight > MIN_PREPROCESS_VEGE_HEIGHT && vegeHeight < MAX_PREPROCESS_VEGE_HEIGHT)
					{
						sumHeight += vegeHeight;
						definedCount++;
					}
					else
					{
						//CDebug.Warning(field + " = " + vegeHeight);
					}
				}
			}
			float averageHeight = sumHeight / definedCount;
			if (averageHeight < MIN_PREPROCESS_VEGE_HEIGHT)
			{
				averageHeight = Math.Max(averageHeight, CTreeManager.MIN_FAKE_TREE_HEIGHT);
			}
			return averageHeight;
		}

		private float GetMaxPreProcessVegeHeight()
		{
			List<Vector3> heights = new List<Vector3>();

			foreach (CGroundField field in fields)
			{
				float? groundHeight = field.GetHeight();
				float? preProcessVegeHeight = field.MaxPreProcessVege;
				if (preProcessVegeHeight != null && groundHeight != null)
				{
					float vegeHeight = (float)preProcessVegeHeight - (float)groundHeight;
					if (vegeHeight > CTreeManager.AVERAGE_MAX_TREE_HEIGHT) { continue; }
					Vector3 fieldCenter = field.center;
					fieldCenter.Y = vegeHeight;
					heights.Add(fieldCenter);
				}
			}
			heights.Sort((a, b) => b.Y.CompareTo(a.Y));
			Vector3 selectedPoint = heights[0];
			int okHeightsInRow = 0;
			int estimatedFakePointsCount = GetEstimatedFakePointsCount();
			const int minDistBetweenFakeAndOkPoint = 1;

			for (int i = 0; i < heights.Count; i++)
			{
				Vector3 currentPoint = heights[i];
				float dist = Vector3.Distance(currentPoint, selectedPoint);
				if (dist < stepSize + 0.5f)
				{
					selectedPoint = currentPoint;
					continue;
				}
				float heightDiff = selectedPoint.Y - currentPoint.Y;
				if (heightDiff > minDistBetweenFakeAndOkPoint)
				{
					okHeightsInRow = 0;
				}
				else
				{
					okHeightsInRow++;
				}
				selectedPoint = currentPoint;

				if (okHeightsInRow > estimatedFakePointsCount)
				{
					break;
				}
			}

			return selectedPoint.Y;
		}

		public int GetEstimatedFakePointsCount()
		{
			int area = arrayXRange * arrayYRange;
			int count = (int)(area / 1000f);
			count = Math.Max(3, count);
			return count;
		}

		public void SetHeight(float pHeight, int pXindex, int pYindex)
		{
			CGroundField field = array[pXindex, pYindex];
			field.SetHeight(pHeight);
		}

		public void FillMissingHeights(int pKernelMultiplier)
		{
			FillMissingHeights(CGroundField.EFillMethod.FromNeighbourhood, pKernelMultiplier);
			//FillMissingHeights(CGroundField.EFillMethod.ClosestDefined);
			//FillMissingHeights(CGroundField.EFillMethod.ClosestDefined);
		}

		//todo: dont use during testing, otherwise result is nondeterministic
		bool useRandomForGroundSmoothing = false;

		public void SmoothenArray(int pKernelMultiplier)
		{
			List<CGroundField> fieldsRandom = fields;

			//todo: dont use during testing, otherwise result is nondeterministic
			if (useRandomForGroundSmoothing)
			{
				fieldsRandom.Shuffle();
			}

			//prepare gauss kernel
			int kernelSize = GetKernelSize();
			kernelSize *= pKernelMultiplier;
			//cant work with even sized kernel
			if (kernelSize % 2 == 0) { kernelSize++; }
			double[,] gaussKernel = CUtils.CalculateGaussKernel(kernelSize, 1);

			foreach (CGroundField el in fieldsRandom)
			{
				el.CalculateSmoothHeight(gaussKernel);
			}
		}

		public bool IsAllDefined()
		{
			foreach (CGroundField f in fields)
			{
				if (!f.IsDefined()) { return false; }
			}
			return true;
		}

		private const float DEFAULT_KERNEL_SIZE = 5; //IN METERS
		
		public static int GetKernelSize()
		{
			int size = (int)(DEFAULT_KERNEL_SIZE / CParameterSetter.groundArrayStep);
			if (size % 2 == 0) { size++; }
			return size;
		}

		///PRIVATE

		private void ApplyFillMissingHeights()
		{
			foreach (CGroundField f in fields)
			{
				f.ApplyFillMissingHeight();
			}
		}

		private void FillMissingHeights(CGroundField.EFillMethod pMethod, int pKernelMultiplier)
		{
			List<CGroundField> fieldsRandom = fields;
			if (useRandomForGroundSmoothing)
			{
				fieldsRandom.Shuffle();
			}

			foreach (CGroundField el in fieldsRandom)
			{
				if (CProjectData.backgroundWorker.CancellationPending) { return; }

				if (!el.IsDefined())
				{
					el.FillMissingHeight(pMethod, pKernelMultiplier);
				}
			}
			ApplyFillMissingHeights();
		}

		//OTHER
		/// <summary>
		/// Returns string for x coordinate in array moved by offset
		/// </summary>
		public float GetFieldXCoord(int pXindex)
		{
			return pXindex * stepSize - CenterOffset.X;
		}

		/// <summary>
		/// Returns string for y coordinate in array moved by offset
		/// </summary>
		public float GetFieldZCoord(int pYindex)
		{
			return pYindex * stepSize - CenterOffset.Z;
		}

		public override string ToString()
		{
			return "Field " + arrayXRange + " x " + arrayYRange + ". Corner = " + topLeftCorner;
		}

		public List<CTree> GetTreesInDistanceFrom(Vector3 pPoint, float pDistance)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			int steps = (int)(pDistance / stepSize);
			steps = Math.Max(1, steps);

			List<CTree> trees = new List<CTree>();

			for (int x = index.Item1 - steps; x < index.Item1 + steps; x++)
			{
				for (int y = index.Item2 - steps; y < index.Item2 + steps; y++)
				{
					List<CTree> detectedTrees = GetElement(x, y)?.DetectedTrees;
					if (detectedTrees != null)
					{
						foreach (CTree tree in detectedTrees)
						{
							trees.Add(tree);
						}
					}
				}
			}

			return trees;
		}

		/// <summary>
		/// just check
		/// </summary>
		public void DebugDetectedTrees()
		{
			int detectedTreesCount = 0;
			int validTreesCount = 0;
			int invalidTreesCount = 0;
			foreach (CGroundField f in fields)
			{
				detectedTreesCount += f.DetectedTrees.Count;
				foreach (CTree tree in f.DetectedTrees)
				{
					if (tree.isValid) { validTreesCount++; }
					else { invalidTreesCount++; }
				}
			}
			CDebug.Count("Detected trees", detectedTreesCount);
			CDebug.Count("valid trees", validTreesCount);
			CDebug.Count("invalid trees", invalidTreesCount);
		}

		/// <summary>
		/// just check
		/// </summary>
		public void DebugCheckTrees()
		{
			int assignedCheckTreesCount = 0;
			int invalidCheckTreesCount = 0;
			int totalCheckTreesCount = 0;
			foreach (CGroundField f in fields)
			{
				foreach (CCheckTree tree in f.CheckTrees)
				{
					if (tree.assignedTree != null)
					{
						assignedCheckTreesCount++;
					}
					else if (tree.isInvalid)
					{
						invalidCheckTreesCount++;
					}
					totalCheckTreesCount++;
				}
			}
			CDebug.Count("assigned checkTrees", assignedCheckTreesCount, totalCheckTreesCount);
			CDebug.Count("invalid checkTrees", invalidCheckTreesCount);
		}

		public bool AddCheckTree(ref CCheckTree pCheckTree)
		{
			CGroundField el = GetElementContainingPoint(pCheckTree.position);
			if (el != null)
			{
				return el.AddCheckTree(pCheckTree);
			}
			else
			{
				//CDebug.WriteLine(pCheckTree + " is out of bounds. " + WriteBounds());
			}
			return false;
		}

		public string WriteBounds(bool pConsole = true)
		{
			string output = "[" + botLeftCorner + "," + topRightCorner + "]";
			if (pConsole) { CDebug.WriteLine(output); }
			return output;
		}

		public float? GetPointHeight(Vector3 pPoint)
		{
			float? groundHeight = GetHeight(pPoint);
			if (groundHeight == null) { return null; }
			return pPoint.Y - groundHeight;
		}

		public bool IsAtBorder(Vector3 pPoint)
		{
			float distanceToBorder = GetDistanceToBorderFrom(pPoint);
			return distanceToBorder < CParameterSetter.GetFloatSettings(ESettings.treeExtent);
		}

		public float GetDistanceToBorderFrom(Vector3 pPoint)
		{
			float xDistToRight = topRightCorner.X - pPoint.X;
			float xDistToLeft = pPoint.X - botLeftCorner.X;
			float xDist = Math.Min(xDistToLeft, xDistToRight);

			float zDistToTop = topRightCorner.Z - pPoint.Z;
			float zDistToBot = pPoint.Z - botLeftCorner.Z;
			float zDist = Math.Min(zDistToBot, zDistToTop);

			float dist = Math.Min(xDist, zDist);
			return dist;
		}

		public static float GetStepSizeForWidth(int pMaxArrayWidth)
		{
			float width = CProjectData.header.Width; //in meters
			const float minStepSize = .1f;
			float stepSize = minStepSize;
			int arrayWidth = (int)(width / stepSize);
			if (arrayWidth > pMaxArrayWidth)
			{
				float scale = arrayWidth / (float)pMaxArrayWidth;
				stepSize *= scale;
			}
			return stepSize;
		}
	}
}