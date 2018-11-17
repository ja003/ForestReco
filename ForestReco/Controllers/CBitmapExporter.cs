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
						/*//if not defined (expected on many fields, use array used in project with filled in values) 
						pixelColor = CProjectData.array.GetElementContainingPoint(groundElement.center).GetColor();
						if (pixelColor == null)
						{
							//undefined - should not happen
							pixelColor = new Color();
						}*/
					}
					int colorVaInt = (int)colorVal;
					if (colorVaInt > maxValue) { maxValue = colorVaInt; }
					Color color = Color.FromArgb(colorVaInt, colorVaInt, colorVaInt);
					bitmap.SetPixel(x, y, color);
				}
			}

			StretchColorRange(ref bitmap, maxValue);
			BlurBitmap(ref bitmap);

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

			ExportBitmap(bitmap);
		}

		private static void BlurBitmap(ref Bitmap pBitmap)
		{
			//pBitmap.

		}

		private static void StretchColorRange(ref Bitmap pBitmap, int pMaxValue)
		{
			float scale = 255 / pMaxValue;
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

		private static void ResizeBitmap(ref Bitmap pBitmap)
		{
			int minWidth = 500;
			if (pBitmap.Width < minWidth)
			{
				string fileName0 = "trees_orig.Jpeg";
				string filePath0 = CObjPartition.folderPath + "/" + fileName0;
				pBitmap.Save(filePath0, ImageFormat.Jpeg);

				float scale = minWidth / pBitmap.Width;
				//Bitmap original = (Bitmap)Image.FromFile("DSC_0002.jpg");
				Bitmap resized = new Bitmap(pBitmap, new Size((int)(pBitmap.Width * scale), (int)(pBitmap.Height * scale)));
				pBitmap = resized;
			}
		}

		private static void ExportBitmap(Bitmap pBitmap)
		{
			ResizeBitmap(ref pBitmap);


			string fileName = "trees.Jpeg";
			string filePath = CObjPartition.folderPath + "/" + fileName;
			pBitmap.Save(filePath, ImageFormat.Jpeg);
		}
	}
}
