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
		private const string REQUIRED = "[REQUIRED] ";
		private const string OPTIONAL = "[OPTIONAL] ";

		public static string GetTooltip(ESettings pSettings)
		{
			switch (pSettings)
			{
				//buttons
				case ESettings.forestFilePath:
					return REQUIRED + "The main processed file.\n" +
					       "Read the specification of required file format in the documentation.";
				case ESettings.reftreeFolderPath:
					return REQUIRED + "A folder containing subfolders with reftrees you want to use in the process.\n" +
						   "Read the specification of required reftree folder format in the documentation. \n" +
					       "To ignore some reftrees simply move them to folder 'ignore' or anywhere else.";
				case ESettings.outputFolderPath:
					return REQUIRED + "Folder where an output will be exported. \n" +
						   "A subfolder with the same name as forest file will be created. If it already exists, it will have a suffix.";
				case ESettings.analyticsFilePath:
					return OPTIONAL + "CSV file in which some analytics information about the process will be exported. \n" +
						   "If such file already exists, it will append the information to a new line. \n" +
					       "Otherwise, it will create a new file. This analytics will be also exported in the output folder.";
				case ESettings.checkTreeFilePath:
					return OPTIONAL + "The checktree file, see the documentation for its functionality. \n" +
						   "The result of the check will be summarized in 'summary.txt' file in the output folder and visualized in a special bitmap. \n" +
					       "If you want to see a checktree result visualization on the exported model, check 'export checktrees'.";

				//bools
				case ESettings.assignRefTreesRandom:
					return "The selection of most appropriate reftree to every detected tree (the most time-consuming part of a process) is skipped and reftree is selected randomly.";
				case ESettings.useReducedReftreeModels:
					return "Use reduced file of reftree in final export. This option should be checked as the full quality model \n" +
					       "doesn't usually have that much effect on the final look and the final size is reduced greatly by using reduced models.";
				case ESettings.export3d:
					return "Export 3D models. If false -> only analytics and bitmap will be produced.";
				case ESettings.exportTreeStructures:
					return "Include tree detection structures in final export. It is only for visualisation of points assigned to trees.";
				case ESettings.exportTreeBoxes:
					return "Include simple box surrounding tree in final export. It is only for visualisation of tree extent.";
				case ESettings.colorTrees:
					return "Assign color material to trees. No neighbouring tree should have the same color. Good for visualization.";
				case ESettings.exportInvalidTrees:
					return "Include detected invalid trees in final export. Only for check if some trees which should have been detected were detected correctly.";
				case ESettings.exportRefTrees:
					return "Include reftrees in final export. \n\n" +
						   "With reftree models the size of the resulting OBJ grows rapidly and the export process takes much longer. \n" +
					       "Uncheck this only if you want to have quicker results and for result visualisation check 'exportTreeStructures'";
				case ESettings.exportPoints:
					return "Include all points in final export. Only for visualization.";
				case ESettings.filterPoints:
					return "Filters points which are too much above average height and points which do not have many points defined under them (in this case these points are probably not part of vegetation, but some unwanted source like flying animal).\n" +
						   "Use this only if you see some unwanted points in the result.";
				case ESettings.exportBitmap:
					return "Export bitmap representation of a result. Creates a) heightmap, b) tree positions and c) tree borders.";

				case ESettings.useCheckTreeFile:
					return "Loads checktree file defined above and evaluates an accuracy of the detection.";
				case ESettings.exportCheckTrees:
					return "Include checktree visualization in final export.\n\n " +
						   "Only for visualization. Correct assignment = blue, incorrect = red and invalid checktree = gray.";
				case ESettings.autoAverageTreeHeight:
					return "The average tree height is calculated automatically.\n\n" +
						   "The average height is used in tree extents calculation so it is good to have this value as close to real data as possible. " +
						   "Use this if you don't have this knowledge about the data. This option disables manual selection of a tree height.";

				//floats
				case ESettings.avgTreeHeigh:
					return "[adviced value = 10-30m] Expected average tree height in given forest file. (disabled when 'autoAverageTreeHeight' is checked)\n\n" +
						   "The average height is used in tree extents calculation so it is good to have this value as close to real data as possible. " +
						   "Use this if you have this knowledge about the data. Otherwise use 'autoAverageTreeHeight'";

				case ESettings.partitionStep:
					return "[adviced value = 20-50m] Size of on part of the final OBJ [in meters].\n\n" +
						   "Too big OBJ files are hard to handle in post-processing so it is advised to generate files with smaller sizes.\n" +
					       "All parts can be merged in a 3D editor if desired. No information is lost.";

				case ESettings.groundArrayStep:
					return "[adviced value = 1m] Size of ground array sampling [in meters].\n\n" +
						   "With smaller sampling, the reconstructed ground gets closer to real one but the processing takes much longer and it doesn't have any considerable impact on tree detection process.";

				case ESettings.treeExtent:
					return "[adviced value = 1-2 m] The maximal distance of point belonging to tree with height specified in 'average height'.\n\n" +
						   "This value grows/decreases with actual tree height in proportion to 'average tree height'. To detect trees which are very close to each other choose a smaller value. In a sparse forest, this value can be bigger.";

				case ESettings.treeExtentMultiply:
					return "[adviced value = 1,2 - 2] The multiplicator of tree extent during merging process.";
							//"\n\n" +
						   //"Should result in invalid trees being attached to some higher valid tree which was too far during the first processing.";
			}

			return "- no tooltip defined";
		}

		public static string GetTooltip(ETooltip pTooltip)
		{
			switch (pTooltip)
			{
				case ETooltip.EstimatedTotalSize:
					return "Estimated total size of resulting OBJ files.\n\n" +
							"The actual size can be very different. Estimation is based on average tree density which is different for every forest.";
				case ETooltip.EstimatedPartitionSize:
					return "Estimated size of 1 file partition.";
				case ETooltip.avgTreeHeighSlider:
					return "Set tree height manually or let it be calculated automatically by checking 'automatic tree height' checkbox.";

				case ETooltip.sequenceFile:
					return "A file containing a configuration of a sequence process. Replaces the forest file. Read a required file format in the documentation.";

				case ETooltip.toggleConsole:
					return "Toggles visibility of a console. In the console are logged some extra information about the process.";
				case ETooltip.openResult:
					return "Opens a folder where the last result has been exported.";

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
		sequenceFile,
		toggleConsole,
		openResult
	}
}
