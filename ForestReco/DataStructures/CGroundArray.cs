﻿using System;
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
		private List<CGroundField> fields;

		private float stepSize;
		public int arrayXRange { get; }
		public int arrayYRange { get; }
		// ReSharper disable once NotAccessedField.Local
		private int coordinatesCount;

		Vector3 botLeftCorner;
		Vector3 topRightCorner;

		//--------------------------------------------------------------

		public CGroundArray(float pStepSize)
		{
			stepSize = pStepSize;

			botLeftCorner = CProjectData.header.BotLeftCorner;
			topRightCorner = CProjectData.header.TopRightCorner;

			float w = topRightCorner.X - botLeftCorner.X;
			float h = topRightCorner.Z - botLeftCorner.Z;

			//TODO: if not +2, GetPositionInField is OOR
			arrayXRange = (int)(w / pStepSize) + 2;
			arrayYRange = (int)(h / pStepSize) + 2;

			array = new CGroundField[arrayXRange, arrayYRange];
			fields = new List<CGroundField>();
			for (int x = 0; x < arrayXRange; x++)
			{
				for (int y = 0; y < arrayYRange; y++)
				{
					CGroundField newGroundField = new CGroundField(new Tuple<int, int>(x, y),
						new Vector3(x * stepSize, 0, y * stepSize));
					array[x, y] = newGroundField;
					fields.Add(newGroundField);
				}
			}
			for (int x = 0; x < arrayXRange; x++)
			{
				for (int y = 0; y < arrayYRange; y++)
				{
					if (x > 0) { array[x, y].Left = array[x - 1, y]; }
					if (x < arrayXRange - 1) { array[x, y].Right = array[x + 1, y]; }
					if (y > 0) { array[x, y].Top = array[x, y - 1]; }
					if (y < arrayYRange - 1) { array[x, y].Bot = array[x, y + 1]; }
				}
			}
		}

		///GETTER
		public CGroundField GetElement(int pXindex, int pYindex)
		{
			if (!IsWithinBounds(pXindex, pYindex)) { return null; }
			return array[pXindex, pYindex];
		}

		private bool IsWithinBounds(int pXindex, int pYindex)
		{
			return pXindex >= 0 && pXindex < arrayXRange && pYindex >= 0 && pYindex < arrayYRange;
		}

		public CGroundField GetElementContainingPoint(Vector3 pPoint)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			return array[index.Item1, index.Item2];
		}

		public float? GetHeight(Vector3 pPoint, bool pGetHeightFromNeighbour = false)
		{
			return GetElementContainingPoint(pPoint).GetHeight(pGetHeightFromNeighbour);
		}

		private Tuple<int, int> GetPositionInField(Vector3 pPoint)
		{
			int xPos = (int)((pPoint.X - botLeftCorner.X) / stepSize);
			//due to array orientation
			int yPos = arrayYRange - (int)((pPoint.Z - botLeftCorner.Z) / stepSize) - 1;
			return new Tuple<int, int>(xPos, yPos);
		}

		public Vector3 GetCenterOffset()
		{
			return new Vector3(arrayXRange / 2f * stepSize, 0, arrayYRange / 2f * stepSize);
		}

		//PUBLIC

		public void AddPointInField(Vector3 pPoint)
		{
			Tuple<int, int> index = GetPositionInField(pPoint);
			array[index.Item1, index.Item2].AddPoint(pPoint);
			//Console.WriteLine(index + " = " + pPointField);
			coordinatesCount++;
		}

		public void SetHeight(float pHeight, int pXindex, int pYindex)
		{
			array[pXindex, pYindex].MaxGround = pHeight;
		}

		public void FillMissingHeights()
		{
			FillMissingHeights(CGroundField.EFillMethod.ClosestDefined);
			FillMissingHeights(CGroundField.EFillMethod.FromNeighbourhood);
			FillMissingHeights(CGroundField.EFillMethod.ClosestDefined);
		}

		public bool IsAllDefined()
		{
			foreach (CGroundField f in fields)
			{
				if (!f.IsDefined()) { return false; }
			}
			return true;
		}

		///PRIVATE

		private void ApplyFillMissingHeights()
		{
			foreach (CGroundField f in fields)
			{
				f.ApplyFillMissingHeight();
			}
		}

		private void FillMissingHeights(CGroundField.EFillMethod pMethod)
		{
			List<CGroundField> fieldsRandom = fields;
			//fieldsRandom.Shuffle();
			foreach (CGroundField el in fieldsRandom)
			{
				if (!el.IsDefined())
				{
					el.FillMissingHeight(pMethod);
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
			return pXindex * stepSize - GetCenterOffset().X;
		}

		/// <summary>
		/// Returns string for y coordinate in array moved by offset
		/// </summary>
		public float GetFieldZCoord(int pYindex)
		{
			return pYindex * stepSize - GetCenterOffset().Z;
		}

		public override string ToString()
		{
			return "Field " + arrayXRange + " x " + arrayYRange;
		}

	}
}