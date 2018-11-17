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
			Bitmap bitmap = new Bitmap(CProjectData.array.arrayXRange, CProjectData.array.arrayYRange);

			CGroundArray array = CProjectData.array;
			for (int x = 0; x < array.arrayXRange; x++)
			{
				for (int y = 0; y < array.arrayYRange; y++)
				{
					bitmap.SetPixel(x, y, array.GetElement(x, y).GetColor());
				}
			}

			Color treeColor = new Color();
			treeColor = Color.FromArgb(255, 0, 0);
			int treeBrushSize = GetTreeBrushSize();
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

		private static int GetTreeBrushSize()
		{
			return 1;
		}

		private static void ExportBitmap(Bitmap bitmap)
		{
			int minWidth = 500;


			var brush = new SolidBrush(Color.Black);


			if (bitmap.Width < minWidth)
			{
				string fileName0 = "trees_orig.Jpeg";
				string filePath0 = CObjPartition.folderPath + "/" + fileName0;
				bitmap.Save(filePath0, ImageFormat.Jpeg);

				float scale = minWidth / bitmap.Width;
				//Bitmap original = (Bitmap)Image.FromFile("DSC_0002.jpg");
				Bitmap resized = new Bitmap(bitmap, new Size((int)(bitmap.Width * scale), (int)(bitmap.Height  * scale)));
				bitmap = resized;
			}


			string fileName = "trees.Jpeg";
			string filePath = CObjPartition.folderPath + "/" + fileName;
			bitmap.Save(filePath, ImageFormat.Jpeg);
		}
	}
}
