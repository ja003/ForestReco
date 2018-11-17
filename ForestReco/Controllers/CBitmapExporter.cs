using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public static class CBitmapExporter
	{

		public static void Export()
		{
			CGroundArray array = CProjectData.detailArray;
			Bitmap bitmap = new Bitmap(array.arrayXRange, array.arrayYRange);

			int maxValue = 0;
			for (int x = 0; x < array.arrayXRange; x++)
			{
				for (int y = 0; y < array.arrayYRange; y++)
				{
					CGroundField groundElement = array.GetElement(x, y);
					int? colorVal = groundElement.GetColorValue();//from detailed array

					if (colorVal == null)
					{
						continue;
					}
					int colorVaInt = (int)colorVal;
					if (colorVaInt > maxValue) { maxValue = colorVaInt; }
					Color color = Color.FromArgb(colorVaInt, colorVaInt, colorVaInt);
					bitmap.SetPixel(x, y, color);
				}
			}

			StretchColorRange(ref bitmap, maxValue);
			//ExportBitmap(bitmap, "tree_beforeMax");

			FilterBitmap(ref bitmap, GetKernelSize(array.stepSize, .2f), EFilter.Max);

			ExportBitmap(bitmap, "heightmap");

			Bitmap bitmapTreePos = new Bitmap(bitmap);
			AddTreesToBitmap(array, bitmapTreePos, true, false);
			ExportBitmap(bitmapTreePos, "tree_positions");

			Bitmap bitmapTreeBorder = new Bitmap(bitmap);
			AddTreesToBitmap(array, bitmapTreeBorder, true, true);
			ExportBitmap(bitmapTreeBorder, "tree_borders");
		}

		private static void AddTreesToBitmap(CGroundArray pArray, Bitmap pBitmap, bool pTreePostition, bool pTreeBorder)
		{
			Color treeColor = new Color();
			treeColor = Color.FromArgb(255, 0, 0);
			Color treeBorderColor = new Color();
			treeBorderColor = Color.FromArgb(0, 0, 255);
			Color branchColor = new Color();
			branchColor = Color.FromArgb(0, 255, 0);

			int treeMarkerSize = GetTreeBrushSize(pArray.stepSize);

			SolidBrush treeBorderBrush = new SolidBrush(treeBorderColor);
			SolidBrush branchBrush = new SolidBrush(branchColor);
			SolidBrush treeBrush = new SolidBrush(treeColor);
			Pen treeBorderPen = new Pen(treeBorderBrush);
			Pen branchPen = new Pen(branchBrush);

			foreach (CTree tree in CTreeManager.Trees)
			{
				CGroundField fieldWithTree = pArray.GetElementContainingPoint(tree.peak.Center);
				int x = fieldWithTree.indexInField.Item1;
				int y = fieldWithTree.indexInField.Item2;

				//draw branch extents
				if (pTreeBorder)
				{
					List<Vector3> furthestPoints = new List<Vector3>();
					foreach (CBranch branch in tree.Branches)
					{
						furthestPoints.Add(branch.furthestPoint);
					}
					for (int i = 0; i < furthestPoints.Count; i++)
					{
						Vector3 furthestPoint = furthestPoints[i];
						Vector3 nextFurthestPoint = furthestPoints[(i + 1) % furthestPoints.Count];

						CGroundField fieldWithFP1 = pArray.GetElementContainingPoint(furthestPoint);
						CGroundField fieldWithFP2 = pArray.GetElementContainingPoint(nextFurthestPoint);
						int x1 = fieldWithFP1.indexInField.Item1;
						int y1 = fieldWithFP1.indexInField.Item2;
						int x2 = fieldWithFP2.indexInField.Item1;
						int y2 = fieldWithFP2.indexInField.Item2;

						using (Graphics g = Graphics.FromImage(pBitmap))
						{
							g.DrawLine(treeBorderPen, x1, y1, x2, y2);
						}
					}

					foreach (CBranch branch in tree.Branches)
					{
						CGroundField fieldWithBranch = pArray.GetElementContainingPoint(branch.furthestPoint);
						int _x = fieldWithBranch.indexInField.Item1;
						int _y = fieldWithBranch.indexInField.Item2;

						using (Graphics g = Graphics.FromImage(pBitmap))
						{
							g.DrawLine(branchPen, x, y, _x, _y);
						}
					}
				}
				//mark tree position
				if (pTreePostition)
				{
					pBitmap.SetPixel(x, y, treeColor);
					using (Graphics g = Graphics.FromImage(pBitmap))
					{
						//g.FillRectangle(treeBrush, x, y, treeMarkerSize, treeMarkerSize);
						int _x = x - treeMarkerSize / 2;
						if(_x < 0){ _x = x; }
						int _y = y - treeMarkerSize / 2;
						if(_y < 0){ _y = y; }
						g.FillRectangle(treeBrush, _x, _y, treeMarkerSize, treeMarkerSize);
					}
				}
			}
		}

		public enum EFilter
		{
			Blur,
			Max,
			Min
		}

		private static void FilterBitmap(ref Bitmap pBitmap, int pKernelSize, EFilter pFilter)
		{
			Bitmap copyBitmap = new Bitmap(pBitmap);
			for (int x = 0; x < pBitmap.Width; x++)
			{
				for (int y = 0; y < pBitmap.Height; y++)
				{
					Color color = pBitmap.GetPixel(x, y);
					int origVal = color.R;
					if (origVal > 0) { continue; }
					int definedCount = 0;
					int valueSum = 0;
					int maxValue = 0;
					int minValue = 0;
					for (int i = -pKernelSize; i < pKernelSize; i++)
					{
						for (int j = -pKernelSize; j < pKernelSize; j++)
						{
							int _x = x + i;
							int _y = y + j;
							if (_x < 0 || _x >= pBitmap.Width || _y < 0 || _y >= pBitmap.Height) { continue; }
							int val = pBitmap.GetPixel(_x, _y).R;
							if (val > 0)
							{
								valueSum += val;
								definedCount++;
								if (val > maxValue) { maxValue = val; }
								if (val < minValue) { minValue = val; }
							}
						}
					}

					int newVal = 0;
					switch (pFilter)
					{
						case EFilter.Blur:
							newVal = valueSum / definedCount;
							break;
						case EFilter.Max:
							newVal = maxValue;
							break;
						case EFilter.Min:
							newVal = minValue;
							break;
					}

					Color newColor = Color.FromArgb(newVal, newVal, newVal);

					copyBitmap.SetPixel(x, y, newColor);
				}
			}
			pBitmap = copyBitmap;
		}

		private static void StretchColorRange(ref Bitmap pBitmap, int pMaxValue)
		{
			float scale = 255f / pMaxValue;
			for (int x = 0; x < pBitmap.Width; x++)
			{
				for (int y = 0; y < pBitmap.Height; y++)
				{
					Color color = pBitmap.GetPixel(x, y);
					int origVal = color.R;
					int scaledVal = (int)(origVal * scale);
					Color newColor = Color.FromArgb(scaledVal, scaledVal, scaledVal);

					pBitmap.SetPixel(x, y, newColor);
				}
			}
		}

		private static int GetTreeBrushSize(float pArrayStepSize)
		{
			int size = (int)(0.6f / pArrayStepSize);
			size = Math.Max(1, size);
			return size;
		}

		private static int GetKernelSize(float pArrayStepSize, float pRadius)
		{
			int size = (int)(pRadius / pArrayStepSize);
			size = Math.Max(1, size);
			return size;
		}

		private static void ResizeBitmap(ref Bitmap pBitmap)
		{
			const int resultWidth = 800; //todo: set from GUI
			float scale = resultWidth / pBitmap.Width;
			Bitmap resized = new Bitmap(pBitmap, new Size((int)(pBitmap.Width * scale), (int)(pBitmap.Height * scale)));
			pBitmap = resized;
		}

		private static void ExportBitmap(Bitmap pBitmap, string pName)
		{
			ResizeBitmap(ref pBitmap);

			string fileName = pName + ".jpg";
			string filePath = CObjPartition.folderPath + "/" + fileName;
			pBitmap.Save(filePath, ImageFormat.Jpeg);
		}
	}
}
