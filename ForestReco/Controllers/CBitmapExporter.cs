using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
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
			ExportBitmap(bitmap, "tree_beforeMax");

			FilterBitmap(ref bitmap, GetKernelSize(array.stepSize, .2f), EFilter.Max);
			//ExportBitmap(bitmap, "tree_beforeMax2");
			//FilterBitmap(ref bitmap, GetKernelSize(array.stepSize, .2f), EFilter.Max);

			Color treeColor = new Color();
			treeColor = Color.FromArgb(255, 0, 0);
			int treeBrushSize = GetTreeBrushSize(array.stepSize);
			foreach (CTree tree in CTreeManager.Trees)
			{
				CGroundField fieldWithTree = array.GetElementContainingPoint(tree.peak.Center);
				int x = fieldWithTree.indexInField.Item1;
				int y = fieldWithTree.indexInField.Item2;
				bitmap.SetPixel(x, y, treeColor);

				using (Graphics g = Graphics.FromImage(bitmap))
				{
					SolidBrush shadowBrush = new SolidBrush(treeColor);
					g.FillRectangle(shadowBrush, x, y, treeBrushSize, treeBrushSize);
				}
			}
			ExportBitmap(bitmap, "tree_beforeResize");
			ResizeBitmap(ref bitmap);

			ExportBitmap(bitmap, "tree");
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
			int size = (int)(0.5f / pArrayStepSize);
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
			const int resultWidth = 800;
			float scale = resultWidth / pBitmap.Width;
			Bitmap resized = new Bitmap(pBitmap, new Size((int)(pBitmap.Width * scale), (int)(pBitmap.Height * scale)));
			pBitmap = resized;
		}

		private static void ExportBitmap(Bitmap pBitmap, string pName)
		{
			string fileName = pName + ".jpg";
			string filePath = CObjPartition.folderPath + "/" + fileName;
			pBitmap.Save(filePath, ImageFormat.Jpeg);
		}
	}
}
