using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

// ReSharper disable PossibleInvalidOperationException - resharper doesnt recognise IsDefined functionality
#pragma warning disable 659

namespace ForestReco
{
	public class CGroundField
	{
		public CGroundField Left;
		public CGroundField Right;
		public CGroundField Top;
		public CGroundField Bot;
		private List<CGroundField> neighbours;

		public List<Vector3> goundPoints = new List<Vector3>();
		public List<Vector3> vegePoints = new List<Vector3>();
		public List<Vector3> fakePoints = new List<Vector3>();
		public List<Vector3> validPoints = new List<Vector3>();

		public List<Vector3> preProcessPoints = new List<Vector3>();

		public float? MaxPreProcessVege;

		public float? MinGround;
		public float? MaxGround;
		public float? SumGround;

		public int VertexIndex = -1;

		public readonly Tuple<int, int> indexInField;

		private Vector3 center;


		//--------------------------------------------------------------

		public CGroundField(Tuple<int, int> pIndexInField, Vector3 pCenter)
		{
			indexInField = pIndexInField;
			center = pCenter;
		}

		public List<CTree> DetectedTrees = new List<CTree>();
		public List<CCheckTree> CheckTrees = new List<CCheckTree>();

		//NEIGHBOUR

		public bool IsAnyNeighbourDefined()
		{
			List<CGroundField> _neighbours = GetNeighbours();
			foreach (CGroundField n in _neighbours)
			{
				if (n.IsDefined()) { return true; }
			}
			return false;
		}

		public bool AreAllNeighboursDefined()
		{
			List<CGroundField> _neighbours = GetNeighbours();
			foreach (CGroundField n in _neighbours)
			{
				if (!n.IsDefined()) { return false; }
			}
			return true;
		}

		private CGroundField GetNeighbour(EDirection pNeighbour)
		{
			switch (pNeighbour)
			{
				case EDirection.Bot: return Bot;
				case EDirection.Left: return Left;
				case EDirection.Right: return Right;
				case EDirection.Top: return Top;

				case EDirection.LeftTop: return Left?.Top;
				case EDirection.RightTop: return Right?.Top;
				case EDirection.RightBot: return Right?.Bot;
				case EDirection.LeftBot: return Left?.Bot;

			}
			return null;
		}

		/// <summary>
		/// All points but those at edge should have assigned neigbours
		/// </summary>
		private bool HasAllNeighbours()
		{
			return Left != null && Right != null && Top != null && Bot != null;
		}

		private bool HasAllNeighboursDefined(bool p8neighbourhood)
		{
			bool neighbourhood4Defined = Left.IsDefined() && Right.IsDefined() && Top.IsDefined() && Bot.IsDefined();
			if (!p8neighbourhood)
			{
				return neighbourhood4Defined;
			}
			else
			{
				return neighbourhood4Defined && Left.Top.IsDefined() && Right.Top.IsDefined() && Left.Bot.IsDefined() &&
					   Right.Bot.IsDefined();
			}
		}
		
		/*public int GetPointCountInNeighbourhood()
		{
			int count = vegePoints.Count;
			foreach (CGroundField neighbour in GetNeighbours())
			{
				count += neighbour.vegePoints.Count;
			}
			return count;
		}*/

		public List<CGroundField> GetNeighbours()
		{
			if (neighbours != null) { return this.neighbours; }

			neighbours = new List<CGroundField>();
			var directions = Enum.GetValues(typeof(EDirection));
			foreach (EDirection d in directions)
			{
				CGroundField neighour = GetNeighbour(d);
				if (neighour != null) { neighbours.Add(neighour); }
			}

			return neighbours;
		}

		//PUBLIC

		public void AddGroundPoint(Vector3 pPoint)
		{
			float height = pPoint.Y;

			goundPoints.Add(pPoint);
			if (SumGround != null) { SumGround += height; }
			else { SumGround = height; }
			if (height > MaxGround || MaxGround == null) { MaxGround = height; }
			if (height < MinGround || MinGround == null) { MinGround = height; }
		}

		public void AddVegePoint(Vector3 pPoint)
		{
			vegePoints.Add(pPoint);
		}

		public void AddPreProcessVegePoint(Vector3 pPoint)
		{
			preProcessPoints.Add(pPoint);
			if (MaxPreProcessVege != null)
			{
				if (pPoint.Y > MaxPreProcessVege)
				{
					MaxPreProcessVege = pPoint.Y;
				}
			}
			else
			{
				MaxPreProcessVege = pPoint.Y;
			}
		}


		public void SortPreProcessPoints()
		{
			preProcessPoints.Sort((a, b) => a.Y.CompareTo(b.Y));

		}

		/// <summary>
		/// Remove points, which were not filtered and dont have any close neighbour defined under them.
		/// Apply only on a few top points. this criterium would discard the most of the lower valid points
		/// </summary>
		public void TryRemoveValidPoints()
		{
			bool tryRemoveValidPoints = true;
			int validBefore = validPoints.Count;
			validPoints.Sort((a, b) => a.Y.CompareTo(b.Y)); //sort ascending
			int indexInvalid = -1;
			if (tryRemoveValidPoints)
			{
				int maxRemoveCount = 7;
				int minIndex = Math.Max(0, validPoints.Count - maxRemoveCount);
				for (int i = validPoints.Count - 2; i >= minIndex; i--)
				{
					Vector3 validPoint = validPoints[i];
					Vector3 previousValidPoint = validPoints[i + 1];
					if (Vector3.Distance(validPoint, previousValidPoint) > 1)
					{
						indexInvalid = i;
						break;
					}
				}
			}

			if (indexInvalid > 0)
			{
				List<Vector3> removedPoints = validPoints.GetRange(indexInvalid, validPoints.Count - indexInvalid);
				fakePoints.AddRange(removedPoints);

				validPoints.RemoveRange(indexInvalid, validPoints.Count - indexInvalid);
			}


			int validAfter = validPoints.Count;
			if (validAfter != validBefore)
			{
				//CDebug.Count(this + " devalidated ", validBefore - validAfter, validBefore);
			}
		}

		/// <summary>
		/// Checks if fake points are close to some valid point.
		/// If so, the point is considered valid.
		/// All valid points are added in vegePoints
		/// </summary>
		public void TryAddFakePoints()
		{
			bool tryAddFakePoints = true;
			int fakeBefore = fakePoints.Count;
			if (tryAddFakePoints)
			{
				//List<CGroundField> neighbours = GetNeighbours();
				for (int i = fakePoints.Count - 1; i >= 0; i--)
				{
					Vector3 fakePoint = fakePoints[i];
					bool validated = false;
					foreach (CGroundField neighbour in GetNeighbours())
					{
						float lastDistance = int.MaxValue;
						//validPoints are sorted ascending. start with the highest
						for (int j = neighbour.validPoints.Count - 1; j >= 0; j--)
						{
							Vector3 validPoint = neighbour.validPoints[j];
							float distance = Vector3.Distance(fakePoint, validPoint);
							if (distance < 0.6f)
							{
								fakePoints.RemoveAt(i);
								validPoints.Add(fakePoint);
								validated = true;
							}
							else if (distance > lastDistance)
							{
								break;
							}
							if (validated) { break; }
						}
						if (validated) { break; }
					}
				}
			}
			int fakeAfter = fakePoints.Count;
			if (fakeAfter != fakeBefore)
			{
				//CDebug.Count(this + " validated ", fakeBefore - fakeAfter, fakeBefore);
			}

			CProjectData.vegePoints.AddRange(validPoints);
			CProjectData.fakePoints.AddRange(fakePoints);
		}

		const float MIN_FAKE_POINT_HEIGHT_OFFSET = 3;

		/// <summary>
		/// Adds all points higher than pMaxHeight in fakePoints and other in validPoints
		/// </summary>
		public void FilterFakeVegePoints(float pMaxHeight)
		{
			List<Vector3> okPoints = new List<Vector3>();
			List<Vector3> nokPoints = new List<Vector3>();
			float? groundHeight = GetHeight();

			Vector3 maxOkPoint = new Vector3(-666);
			for (int i = 0; i < preProcessPoints.Count; i++)
			{
				Vector3 point = preProcessPoints[i];
				bool isPointTooMuchAboveLimit = point.Y - groundHeight > pMaxHeight + MIN_FAKE_POINT_HEIGHT_OFFSET;
				//bool isPointTooFarFromMaxOkPoint = point.Y - maxOkPointHeight > 0.3f;
				bool isPointTooFarFromMaxOkPoint = Vector3.Distance(point, maxOkPoint) > 0.3f;
				if (groundHeight != null && isPointTooMuchAboveLimit && isPointTooFarFromMaxOkPoint)
				{
					nokPoints.Add(point);
				}
				else
				{
					okPoints.Add(point);
					maxOkPoint = point;
				}
			}

			fakePoints = nokPoints;
			validPoints = okPoints;
			//CProjectData.vegePoints.AddRange(okPoints);
			//CProjectData.fakePoints.AddRange(nokPoints);
		}

		public bool IsDefined()
		{
			return goundPoints.Count > 0;
		}

		public float? GetAverageHeightFromNeighbourhood(int pKernelSizeMultiplier)
		{
			int pKernelSize = CGroundArray.KernelSize;
			pKernelSize *= pKernelSizeMultiplier;

			int defined = 0;
			float heightSum = 0;
			for (int x = -pKernelSize; x < pKernelSize; x++)
			{
				for (int y = -pKernelSize; y < pKernelSize; y++)
				{
					int xIndex = indexInField.Item1 + x;
					int yIndex = indexInField.Item2 + y;
					CGroundField el = CProjectData.array.GetElement(xIndex, yIndex);
					if (el != null && el.IsDefined())
					{
						defined++;
						// ReSharper disable once PossibleInvalidOperationException
						//is checked
						heightSum += (float)el.GetHeight();
					}
					//average height will be also affected by filled values
					//result should be smoother
					//todo: doesnt help much
					/*else if (el?.MaxGroundFilled != null)
					{
						defined++;
						heightSum += (float)el.MaxGroundFilled;
					}*/
				}
			}
			if (defined == 0) { return null; }
			return heightSum / defined;
		}


		/// <summary>
		/// Finds closest defined fields in direction based on pDiagonal parameter.
		/// Returns average of 2 found heights considering their distance from this field.
		/// </summary>
		public float? GetAverageHeightFromClosestDefined(int pMaxSteps, bool pDiagonal)
		{
			if (IsDefined()) { return GetHeight(); }

			CGroundField closestFirst = null;
			CGroundField closestSecond = null;
			CGroundField closestLeft = GetClosestDefined(pDiagonal ? EDirection.LeftTop : EDirection.Left, pMaxSteps);
			CGroundField closestRight = GetClosestDefined(pDiagonal ? EDirection.RightBot : EDirection.Right, pMaxSteps);
			CGroundField closestTop = GetClosestDefined(pDiagonal ? EDirection.RightTop : EDirection.Top, pMaxSteps);
			CGroundField closestBot = GetClosestDefined(pDiagonal ? EDirection.LeftBot : EDirection.Bot, pMaxSteps);

			closestFirst = closestLeft;
			closestSecond = closestRight;
			if ((closestFirst == null || closestSecond == null) && closestTop != null && closestBot != null)
			{
				closestFirst = closestTop;
				closestSecond = closestBot;
			}

			//if (closestFirst == null) { closestFirst = closestTop; }
			//if (closestSecond == null) { closestSecond = closestTop; }
			//if (closestFirst == null) { closestFirst = closestBot; }
			//if (closestSecond == null) { closestSecond = closestBot; }

			if (closestFirst != null && closestSecond != null)
			{
				CGroundField smaller = closestFirst;
				CGroundField higher = closestSecond;
				if (closestSecond.GetHeight() < closestFirst.GetHeight())
				{
					higher = closestFirst;
					smaller = closestSecond;
				}
				int totalDistance = smaller.GetDistanceTo(higher);
				float? heightDiff = higher.GetHeight() - smaller.GetHeight();
				if (heightDiff != null)
				{
					float? smallerHeight = smaller.GetHeight();
					float distanceToSmaller = GetDistanceTo(smaller);

					//if (totalDistance == 0) { return smallerHeight; }

					return smallerHeight + distanceToSmaller / totalDistance * heightDiff;
				}
			}
			else if (!HasAllNeighbours())
			{
				if (closestLeft != null) { return closestLeft.GetHeight(); }
				if (closestTop != null) { return closestTop.GetHeight(); }
				if (closestRight != null) { return closestRight.GetHeight(); }
				if (closestBot != null) { return closestBot.GetHeight(); }
			}
			if (!pDiagonal)
			{
				return GetAverageHeightFromClosestDefined(pMaxSteps, true);
			}

			return null;
		}

		public void SetHeight(float pHeight)
		{
			AddGroundPoint(new Vector3(center.X, pHeight, center.Z));
		}

		///// <summary>
		///// Returns height of given type.
		///// pGetHeightFromNeighbour: True = ifNotDefined => closest defined height will be used (runs DFS)
		///// pVisited: dont use these points in DFS
		///// </summary>
		//public float? GetHeight(bool pGetHeightFromNeighbour = false, List<CGroundField> pVisited = null, int pMaxIterations = 5)
		//{
		//	pMaxIterations--;
		//	if (!IsDefined() && pGetHeightFromNeighbour)
		//	{
		//		if (pMaxIterations <= 0) { return null;}

		//		if (pVisited == null) { pVisited = new List<CGroundField>(); }

		//		if (pVisited.Contains(this)) { return null;}

		//		List<CGroundField> _neighbours = GetNeighbours();
		//		foreach (CGroundField n in _neighbours)
		//		{
		//			if (!pVisited.Contains(n))
		//			{
		//				pVisited.Add(this);
		//				return n.GetHeight(true, pVisited, pMaxIterations);
		//			}
		//		}
		//		return null;
		//	}
		//	//todo: decide which height to return on default
		//	return MinGround;
		//	//return MaxGround;
		//	//return GetHeightAverage();
		//}

		public float? GetHeight(bool pUseSmoothHeight = true)
		{
			if (pUseSmoothHeight && SmoothHeight != null)
			{
				return SmoothHeight;
			}
			return MaxGround;
		}

		/// <summary>
		/// Returns the interpolated height.
		/// Interpolation = bilinear.
		/// </summary>
		public float? GetHeight(Vector3 pPoint)
		{
			if (!HasAllNeighbours() || !HasAllNeighboursDefined(true))
			{
				if (!IsDefined())
				{
					return null;
				}
				return GetHeight();
			}
			//return GetHeight(); //to cancel interpolation

			//http://www.geocomputation.org/1999/082/gc_082.htm
			//3.4 Bilinear interpolation

			List<CGroundField> bilinearFields = GetBilinearFieldsFor(pPoint);
			CGroundField h1 = bilinearFields[0];
			CGroundField h2 = bilinearFields[1];
			CGroundField h3 = bilinearFields[2];
			CGroundField h4 = bilinearFields[3];

			float a00 = (float)h1.GetHeight();
			float a10 = (float)h2.GetHeight() - (float)h1.GetHeight();
			float a01 = (float)h3.GetHeight() - (float)h1.GetHeight();
			float a11 = (float)h1.GetHeight() - (float)h2.GetHeight() - (float)h3.GetHeight() + (float)h4.GetHeight();

			float x = pPoint.X - center.X;// + CProjectData.groundArrayStep;
										  //float z = pPoint.Z - center.Z;//+ CProjectData.groundArrayStep;
			float z = CProjectData.groundArrayStep - (center.Z - pPoint.Z);

			if (x < 0 || x > 1 || z < 0 || z > 1)
			{
				CDebug.Error("field " + this + " interpolation is incorrect! x = " + x + " z = " + z);
			}

			//pPoint space coords are X and Z, Y = height
			float hi = a00 + a10 * x + a01 * z + a11 * x * z;
			return hi;
		}

		private List<CGroundField> GetBilinearFieldsFor(Vector3 pPoint)
		{
			List<CGroundField> fields = new List<CGroundField>();
			if (pPoint.X > center.X)
			{
				if (pPoint.Z > center.Z)
				{
					fields.Add(this);
					fields.Add(GetNeighbour(EDirection.Right));
					fields.Add(GetNeighbour(EDirection.Top));
					fields.Add(GetNeighbour(EDirection.RightTop));
				}
				else
				{
					fields.Add(GetNeighbour(EDirection.Bot));
					fields.Add(GetNeighbour(EDirection.RightBot));
					fields.Add(this);
					fields.Add(GetNeighbour(EDirection.Right));
				}
			}
			else
			{
				if (pPoint.Z > center.Z)
				{
					fields.Add(GetNeighbour(EDirection.Left));
					fields.Add(this);
					fields.Add(GetNeighbour(EDirection.LeftTop));
					fields.Add(GetNeighbour(EDirection.Top));
				}
				else
				{
					fields.Add(GetNeighbour(EDirection.LeftBot));
					fields.Add(GetNeighbour(EDirection.Bot));
					fields.Add(GetNeighbour(EDirection.Left));
					fields.Add(this);
				}
			}
			return fields;
		}

		public int GetDistanceTo(CGroundField pGroundField)
		{
			return Math.Abs(indexInField.Item1 - pGroundField.indexInField.Item1) +
				   Math.Abs(indexInField.Item2 - pGroundField.indexInField.Item2);
		}

		public float? SmoothHeight;

		/// <summary>
		/// Sets SmoothHeight based on average from neighbourhood
		/// </summary>
		public void CalculateSmoothHeight(double[,] pGaussKernel)
		{
			if (!IsDefined()) { return; }
			//if (!HasAllNeighbours()) { return; } //creates unsmooth step on borders

			//int defined = 0;
			float heightSum = 0;

			//double[,] gaussKernel = CUtils.CalculateGaussKernel(kernelSize, 1);
			float midHeight = (float)GetHeight();

			int kernelSize = CGroundArray.KernelSize;

			for (int x = 0; x < kernelSize; x++)
			{
				for (int y = 0; y < kernelSize; y++)
				{
					int xIndex = indexInField.Item1 + x - kernelSize / 2;
					int yIndex = indexInField.Item2 + y - kernelSize / 2;
					CGroundField el = CProjectData.array.GetElement(xIndex, yIndex);
					float? elHeight = el?.GetHeight();

					//if element is not defined, use height from the middle element
					float definedHeight = midHeight;
					if (elHeight != null)
					{
						definedHeight = (float)elHeight;
					}
					heightSum += definedHeight * (float)pGaussKernel[x, y];
				}
			}
			//if (defined == 0) { return; }
			//SmoothHeight = heightSum / defined;
			SmoothHeight = heightSum;
		}

		public float? MaxGroundFilled;

		public void ApplyFillMissingHeight()
		{
			if (IsDefined()) { return; }
			//MaxGround = MaxGroundFilled;
			if (MaxGroundFilled == null) { return; }

			Vector3 filledPoint = center;
			filledPoint.Y = (float)MaxGroundFilled;
			AddGroundPoint(filledPoint);
		}


		public void FillMissingHeight(EFillMethod pMethod, int pKernelMultiplier)
		{
			if (IsDefined()) { return; }

			int maxSteps = 1;
			switch (pMethod)
			{
				case EFillMethod.ClosestDefined:
					//todo: maybe maxSteps should be infinite
					//todo: does not produce very good results
					MaxGroundFilled = GetAverageHeightFromClosestDefined(10 * maxSteps, false);
					break;
				case EFillMethod.FromNeighbourhood:
					MaxGroundFilled = GetAverageHeightFromNeighbourhood(pKernelMultiplier);
					break;
			}
		}

		public enum EFillMethod
		{
			ClosestDefined,
			FromNeighbourhood
		}


		///PRIVATE

		private CGroundField GetClosestDefined(EDirection pDirection, int pMaxSteps)
		{
			if (IsDefined()) { return this; }
			if (pMaxSteps == 0) { return null; }
			return GetNeighbour(pDirection)?.GetClosestDefined(pDirection, pMaxSteps - 1);
		}

		/// <summary>
		/// Returns maximal/minimal point in this field.
		/// pMax: True = maximum, False = minimum
		/// </summary>
		private float? GetHeightExtrem(bool pMax)
		{
			return pMax ? MaxGround : MinGround;
		}

		private float? GetHeightAverage()
		{
			if (!IsDefined()) { return null; }
			return SumGround / goundPoints.Count;
		}

		/// <summary>
		/// Returnd point with given local position to this point
		/// </summary>
		private CGroundField GetPointWithOffset(int pIndexOffsetX, int pIndexOffsetY)
		{
			CGroundField el = this;
			for (int x = 0; x < Math.Abs(pIndexOffsetX); x++)
			{
				el = pIndexOffsetX > 0 ? el.Right : el.Left;
				if (el == null) { return null; }
			}
			for (int y = 0; y < Math.Abs(pIndexOffsetY); y++)
			{
				el = pIndexOffsetY > 0 ? el.Top : el.Bot;
				if (el == null) { return null; }
			}
			return el;
		}

		//UNUSED

		private EDirection GetOpositeNeighbour(EDirection pNeighbour)
		{
			switch (pNeighbour)
			{
				case EDirection.Bot: return EDirection.Top;
				case EDirection.Top: return EDirection.Bot;
				case EDirection.Left: return EDirection.Right;
				case EDirection.Right: return EDirection.Left;
			}
			return EDirection.None;
		}

		//OTHER

		public string ToStringIndex()
		{
			return "[" + indexInField + "].";
		}

		public override string ToString()
		{
			return ToStringIndex() + " Ground = " + GetHeight() + ". Center = " + center +
				". Trees=" + DetectedTrees.Count + "/" + CheckTrees.Count +
				"|" + fakePoints.Count + "/" + preProcessPoints.Count;
			//return ToStringIndex() + " Tree = " + (Tree?.ToStringIndex() ?? "null");
		}

		public override bool Equals(object obj)
		{
			// Check for null values and compare run-time types.
			if (obj == null || GetType() != obj.GetType())
				return false;

			CGroundField e = (CGroundField)obj;
			return (indexInField.Item1 == e.indexInField.Item1) && (indexInField.Item2 == e.indexInField.Item2);
		}

		public void AddCheckTree(CCheckTree pCheckTree)
		{
			//CDebug.WriteLine("added " + pCheckTree);
			CheckTrees.Add(pCheckTree);
			pCheckTree.groundField = this;
		}
	}
}