using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForestReco
{
	public static class CResultSize
	{
		private static float arrayWidth;
		private static float arrayHeight;

		/// <summary>
		/// Calculates estimated file size
		/// </summary>
		private static float GetResultFileSize()
		{
			CHeaderInfo header = CProjectData.header;
			if (header == null) { return 0; }

			arrayHeight = header.Height;
			arrayWidth = header.Width;
			float area = header.Width * header.Height;
			const float treeStructure = .05f;
			const float reftreeSize = 1;
			const float treeBoxSize = .01f;

			const float treeDensity = .1f; //1 tree per 10 squared meters
			float groundSize = 5; //its just small, no reason to count it in

			float totalSize = groundSize;


			if (CParameterSetter.GetBoolSettings(ESettings.exportTreeStructures))
			{
				totalSize += area * treeDensity * treeStructure;
			}
			if (CParameterSetter.GetBoolSettings(ESettings.exportTreeBoxes))
			{
				totalSize += area * treeDensity * treeBoxSize;
			}
			if (CParameterSetter.GetBoolSettings(ESettings.exportRefTrees))
			{
				totalSize += area * treeDensity * reftreeSize;
			}
			return totalSize;

		}

		private static float GetPartitionSize(float size)
		{
			int partitionStep = CParameterSetter.GetIntSettings(ESettings.partitionStep);
			int xParts = (int)Math.Ceiling(arrayWidth / partitionStep);
			int zParts = (int)Math.Ceiling(arrayHeight / partitionStep);
			int parts = xParts * zParts;
			return size / Math.Max(1, parts);
		}

		public static void WriteEstimatedSize(TextBox pTextBoxTotal, TextBox pTextBoxPartition)
		{
			float size = GetResultFileSize();
			pTextBoxTotal.Text = size.ToString("0.0") + "MB";
			pTextBoxPartition.Text = GetPartitionSize(size).ToString("0.0") + "MB";
		}
	}
}
