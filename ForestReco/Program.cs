using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObjParser;
using ObjParser.Types;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	class Program
	{
		static void Main()
		{
			// Example #1
			// Read the file as one string.
			//string text = File.ReadAllText(@"D:\Adam\projects\SDIPR\ForestReco\ForestReco\src\ANE_1000AGL.las");

			CultureInfo ci = new CultureInfo("en");
			Thread.CurrentThread.CurrentCulture = ci;

			string fileName = @"BK_1000AGL_classified";
			fileName = @"BK_1000AGL_cl_split_s_mezerou";
			fileName = @"BK_1000AGL_classified_0007559_0182972";
			fileName = @"BK_1000AGL_classified_0007559_0182972_0037797";
			string saveFileName = "BKAGL_59_72_97";


			//notebook
			string[] lines = File.ReadAllLines(@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\data-small\TXT\" + fileName + @".txt");
			//home PC
			//string[] lines = File.ReadAllLines(@"C:\Users\Admin\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\data-small\TXT\" + fileName + @".txt");

			CHeaderInfo header = new CHeaderInfo(lines[15], lines[16], lines[17], lines[18]);
			Console.WriteLine(header);

			//TODO: uncommnent to see just header info
			//Console.ReadKey();
			//return;

			//prepare data structures 

			float stepSize = .4f; //in meters

			//no need to record depth information in groundField
			CPointFieldController groundField = new CPointFieldController(header, stepSize, 0);
			CPointFieldController highVegetationField = new CPointFieldController(header, stepSize, 0);
			CPointFieldController combinedField = new CPointFieldController(header, stepSize, 0);
			//CCoordinatesField highVegetationField = new CCoordinatesField(header, stepSize, true);

			bool processGround = false;
			bool processHighVegetation = false;
			bool processCombined = true;

			//store coordinates to corresponding data strucures based on their class
			int linesToRead = lines.Length;
			//linesToRead = 10000;

			for (int i = 19; i < linesToRead; i++)
			{
				// <class, coordinate>
				Tuple<int, SVector3> c = CCoordinatesParser.ParseLine(lines[i], header);

				if (c.Item1 == 2 && processGround) //ground
				{
					groundField.AddPointInFields(c.Item1, c.Item2);
				}
				else if (c.Item1 == 5 && processHighVegetation) //ground + vegetation
				{
					highVegetationField.AddPointInFields(c.Item1, c.Item2);
				}
				else if (c.Item1 == 2 || c.Item1 == 5 && processCombined) //high vegetation
				{
					combinedField.AddPointInFields(c.Item1, c.Item2);
				}
				//if(i%10000 == 0) {Console.WriteLine(c);}
			}


			if (processGround)
			{
				Console.WriteLine("groundField: " + groundField);
				//TODO: to fill missong coordinates use FillMissingHeight startegy
				groundField.FillMissingHeights(0, EHeight.GroundMax);
				groundField.FillMissingHeights(0, EHeight.GroundMax);
				groundField.ExportToObj(saveFileName + "_ground", EExportStrategy.None, EHeight.GroundMax);
				//groundField.ExportToObj(saveFileName + "_X", EExportStrategy.None, EHeight.IndexX);
				//groundField.ExportToObj(saveFileName + "_Y", EExportStrategy.None, EHeight.IndexY);
			}
			if (processHighVegetation)
			{
				Console.WriteLine("highVegetationField: " + highVegetationField);
				highVegetationField.CalculateLocalExtrems(0);
				//highVegetationField.AssignTrees(stepSize);
				highVegetationField.ExportToObj(saveFileName + "_trees",
					EExportStrategy.FillHeightsAroundDefined, EHeight.Tree);
			}
			if (processCombined)
			{
				Console.WriteLine("combinedField: " + combinedField);
				combinedField.FillMissingHeights(0, EHeight.GroundMax);
				combinedField.FillMissingHeights(0, EHeight.GroundMax);
				combinedField.CalculateLocalExtrems(0);
				combinedField.AssignTrees(0);
				//combinedField.AssignTreesToAll(0);
				//highVegetationField.AssignTrees(stepSize);
				//combinedField.ExportToObj(saveFileName + "_comb",
				//	EExportStrategy.None, new List<EHeight> { EHeight.Tree });
				combinedField.ExportToObj(saveFileName + "_comb",
					EExportStrategy.FillHeightsAroundDefined, new List<EHeight> { EHeight.Tree, EHeight.GroundMax });
			}

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}
