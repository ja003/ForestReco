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

			foreach (CTree tree in CTreeManager.Trees)
			{
				CGroundField fieldWithTree = array.GetElementContainingPoint(tree.peak.Center);
				bitmap.SetPixel(fieldWithTree.indexInField.Item1, fieldWithTree.indexInField.Item2, treeColor);
			}


			string fileName = "trees.Jpeg";
			string filePath = CObjPartition.folderPath + "/" + fileName;
			bitmap.Save(filePath, ImageFormat.Jpeg);
		}
	}
}
