using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
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

		private float stepSize;
		public int arrayXRange { get; }
		public int arrayYRange { get; }
		// ReSharper disable once NotAccessedField.Local
		private int coordinatesCount;

		Vector3 botLeftCorner;
		Vector3 topRightCorner;
		Vector3 topLeftCorner;

		private Vector3 CenterOffset;

		//--------------------------------------------------------------

		public CGroundArray()
		{
			stepSize = CProjectData.groundArrayStep;

			botLeftCorner = CProjectData.header.BotLeftCorner;
			topRightCorner = CProjectData.header.TopRightCorner;
			topLeftCorner = new Vector3(botLeftCorner.X, 0, topRightCorner.Z);

			float w = topRightCorner.X - botLeftCorner.X;
			float h = topRightCorner.Z - botLeftCorner.Z;

			//TODO: if not +2, GetPositionInField is OOR
			//todo: 2 is incorrect, all array was shifted
			arrayXRange = (int)(w / stepSize) + 1;
			arrayYRange = (int)(h / stepSize) + 1;

			CenterOffset = new Vector3(arrayXRange / 2f * stepSize, 0, arrayYRange / 2f * stepSize);
			CenterOffset += new Vector3(-stepSize / 2, 0, -stepSize / 2); //better visualization

			array = new CGroundField[arrayXRange, arrayYRange];
			fields = new List<CGroundField>();
			for (int x = 0; x < arrayXRange; x++)
			{
				for (int y = 0; y < arrayYRange; y++)
				{
					CGroundField newGroundField = new CGroundField(new Tuple<int, int>(x, y),
						new Vector3(topLeftCorner.X + x * stepSize, 0, topLeftCorner.Z - y * stepSize));
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
						//todo: check if change (Bot-Top) is ok
						//array[x, y].Top = array[x, y + 1];
						array[x, y].Top = array[x, y - 1]; //orig
					}
					if (y < arrayYRange - 1)
					{
						//array[x, y].Bot = array[x, y - 1];
						array[x, y].Bot = array[x, y + 1]; //orig
					}
				}
			}

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
			if (!IsWithinBounds(index)) { return null; }
			return array[index.Item1, index.Item2];
		}

		public float? GetHeight(Vector3 pPoint)
		{
			return GetElementContainingPoint(pPoint).GetHeight(pPoint);
		}

		private Tuple<int, int> GetPositionInField(Vector3 pPoint)
		{
			int xPos = (int)((pPoint.X - topLeftCorner.X) / stepSize);
			//due to array orientation
			//int yPos = arrayYRange - (int)((pPoint.Z - botLeftCorner.Z) / stepSize) - 1;
			int yPos = (int)((topLeftCorner.Z - pPoint.Z) / stepSize);
			return new Tuple<int, int>(xPos, yPos);
		}


		//PUBLIC

		public enum EPointType
		{
			Ground,
			Vege,
			PreProcess
		}

		public void AddPointInField(Vector3 pPoint, EPointType pType)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			switch (pType)
			{
				case EPointType.Ground:
					array[index.Item1, index.Item2].AddGroundPoint(pPoint);
					break;
				case EPointType.Vege:
					array[index.Item1, index.Item2].AddVegePoint(pPoint);
					break;
				case EPointType.PreProcess:
					array[index.Item1, index.Item2].AddPreProcessVegePoint(pPoint);
					break;
			}
		}

		/// <summary>
		/// Filters points, which are fake (unnaturally higher than average vege poins).
		/// Assigns them in vegePoints and fakePoints
		/// </summary>
		public void FilterFakeVegePoints()
		{
			CDebug.WriteLine("FilterFakeVegePoints", true);
			CProjectData.vegePoints.Clear();

			float averageHeight = GetAveragePreProcessVegeHeight();
			CDebug.WriteLine("Average vege height = " + averageHeight, true, true);

			CDebug.Count("vegePoints", CProjectData.vegePoints.Count);
			CDebug.Count("fakePoints", CProjectData.fakePoints.Count);

			foreach (CGroundField field in fields)
			{
				field.FilterFakeVegePoints(averageHeight);
			}

			CDebug.Count("vegePoints", CProjectData.vegePoints.Count);
			CDebug.Count("fakePoints", CProjectData.fakePoints.Count);
		}

		//range of vege height that will be counted in average vege height
		//reason is to restrict undefined or too outOfNorm values to affect average height
		const float MIN_PREPROCESS_VEGE_HEIGHT = 10;
		const float MAX_PREPROCESS_VEGE_HEIGHT = 30;
		const float PREPROCESS_VEGE_HEIGHT_OFFSET = 1;

		/// <summary>
		/// TODO: count weighted average. areas with no trees affect average height 
		/// </summary>
		private int GetAveragePreProcessVegeHeight()
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
					//if (vegeHeight > CTreeManager.AVERAGE_MAX_TREE_HEIGHT - PREPROCESS_VEGE_HEIGHT_OFFSET &&
					//	vegeHeight < CTreeManager.AVERAGE_MAX_TREE_HEIGHT + PREPROCESS_VEGE_HEIGHT_OFFSET)
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
			//round to int so the result is same in most of the situatiuons (problem with float percision)
			return (int)(sumHeight / definedCount);
		}

		public void SetHeight(float pHeight, int pXindex, int pYindex)
		{
			//array[pXindex, pYindex].MaxGround = pHeight;
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
			int kernelSize = KernelSize;
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

		public static int KernelSize => (int)(DEFAULT_KERNEL_SIZE / CProjectData.groundArrayStep);

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
							//if (!tree.isPeakInvalid)
							{
								trees.Add(tree);
							}
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
			int totalCheckTreesCount = 0;
			foreach (CGroundField f in fields)
			{
				foreach (CCheckTree tree in f.CheckTrees)
				{
					if (tree.assignedTree != null)
					{
						assignedCheckTreesCount++;
					}
					totalCheckTreesCount++;
				}
			}
			CDebug.Count("assigned checkTrees", assignedCheckTreesCount, totalCheckTreesCount);
		}

		public void AddCheckTree(ref CCheckTree pCheckTree)
		{
			CGroundField el = GetElementContainingPoint(pCheckTree.position);
			if (el != null)
			{
				el.AddCheckTree(pCheckTree);
			}
			else
			{
				//CDebug.WriteLine(pCheckTree + " is out of bounds. " + WriteBounds());
			}
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
	}
}