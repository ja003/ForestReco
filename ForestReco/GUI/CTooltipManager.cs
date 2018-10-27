using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ForestReco
{
	public static class CTooltipManager
	{
		public static string GetTooltip(ESettings pSettings)
		{
			switch (pSettings)
			{
				//bools
				case ESettings.assignRefTreesRandom:
					return "The selection of most appropriate reftree to every detected tree (the most time consuming part of process) is skipped and reftree is selected randomly.";
				case ESettings.useReducedReftreeModels:
					return "Use reduced file of reftree in final export. This option should be checked as the full quality model doesn't usually have that much effect on final look and the final size is reduced greatly by using reduced models.";
				case ESettings.exportTreeStructures:
					return "Include tree detection structures in final export. It is only for visualisation of points assigned to trees.";
				case ESettings.exportTreeBoxes:
					return "Include simple box surrounding tree in final export. It is only for visualisation of tree extent.";
				case ESettings.exportInvalidTrees:
					return "Include detected invalid trees in final export. Only for check if some trees which should have been detected were detected correctly.";
				case ESettings.exportRefTrees:
					return "Include reftrees in final export. \n\n" +
							"With reftree models the size of the resulting OBJ grows rapidly and export process takes much longer. Uncheck this only of you want to have quicker results and for result visualisation check 'exportTreeStructures'";
				case ESettings.exportPoints:
					return "Include all points in final export. Only for visualization.";
				case ESettings.filterPoints:
					return "Filters points which are too much above average height and points which do not have many points defined under them (in this case these points are probably not part of vegetation, but some unwanted source like flying animal).\n" +
						"Use this only if you see some unwanted points in result.";


				case ESettings.useCheckTreeFile:
					return "Loads checktree file defined above and evaluates an accuracy of the detection.";
				case ESettings.exportCheckTrees:
					return "Include checktree visualization in final export.\n\n " +
							"Only for visualization. Correct assignment = blue, incorrect = red and invalid checktree = gray.";
				case ESettings.autoAverageTreeHeight:
					return "The average tree height is calculated automatically.\n\n" +
							"The average height is used in tree extents calculation so it is good to have this value as close to real data as possible. " +
							"Use this if you dont have this knowledge about the data. This option disables manual selection of average tree height.";

				//floats
				case ESettings.avgTreeHeigh:
					return "[adviced value = 15-30m] Expected average tree height in given forrest file.\n\n" +
							"The average height is used in tree extents calculation so it is good to have this value as close to real data as possible. " +
							"Use this if you have this knowledge about the data. Otherwise use 'autoAverageTreeHeight'";

				case ESettings.partitionStep:
					return "[adviced value = 20-50m] Size of on part of the final OBJ [in meters].\n\n" +
							"Too big OBJ files are hard to handle in post-processing so it is adviced to generate files with smaller sizes. All parts can be merged in 3D editor if desired. No information is lost.";

				case ESettings.groundArrayStep:
					return "[adviced value = 1m] Size of ground array sampling [in meters].\n\n" +
							"With smaller sampling the roconstructed ground gets closer to real one but the processing takes much longer and it doesn't have any considerable impact on tree detection process.";

				case ESettings.treeExtent:
					return "[adviced value = 1-2,5m] The maximal distance of point belonging to tree with height specified in 'average height'.\n\n" +
							"This value is grows/decreases with actual tree height in proportion to 'average height'. To detect trees which are very close to each other choose smaller value. In sparse forrest this value can be bigger.";

				case ESettings.treeExtentMultiply:
					return "[adviced value = 1,5-2,5m] The multiplicator of tree extent during merging process.\n\n" +
							"Should result in invalid trees being attached to some higher valid tree which was too far during the first processing.";


			}

			return "- no tooltip defined";
		}

		public static string GetTooltip(ETooltip pTooltip)
		{
			switch (pTooltip)
			{
				case ETooltip.EstimatedTotalSize:
					return "Estimated total size of resulting OBJ files.\n\n" +
							"The actual size can be very different. Estimation is based on average tree density which can be very different.";
				case ETooltip.EstimatedPartitionSize:
					return "Estimated size of 1 file partition.";
				case ETooltip.avgTreeHeighSlider:
					return "Set average tree height manually or let it be calculated automatically by checking 'automatic average tree height' checkbox.";

			}

			return "- no tooltip defined";
		}

		public static void AssignTooltip(ToolTip pTooltipForm, Control pControl, ESettings pSetting)
		{
			pTooltipForm.SetToolTip(pControl, GetTooltip(pSetting));
		}

		public static void AssignTooltip(ToolTip pTooltipForm, Control pControl, ETooltip pTooltip)
		{
			pTooltipForm.SetToolTip(pControl, GetTooltip(pTooltip));
		}
	}

	public enum ETooltip
	{
		None,
		EstimatedTotalSize,
		EstimatedPartitionSize,
		avgTreeHeighSlider,
	}
}
