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
			CultureInfo ci = new CultureInfo("en");
			Thread.CurrentThread.CurrentCulture = ci;

			string fileName = @"BK_1000AGL_classified";
			//fileName = @"BK_1000AGL_cl_split_s_mezerou";
			//fileName = @"BK_1000AGL_classified_0007559_0182972";
			fileName = @"BK_1000AGL_classified_0007559_0182972_0037797";
			
			string saveFileName = "BKAGL_59_72_97";
			//string saveFileName = "BK_1000AGL_";


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

			CPointField combinedField = new CPointField(header, stepSize);

			bool processCombined = true;

			//store coordinates to corresponding data strucures based on their class
			int linesToRead = lines.Length;
			//linesToRead = 10000;

			for (int i = 19; i < linesToRead; i++)
			{
				// <class, coordinate>
				Tuple<int, SVector3> c = CCoordinatesParser.ParseLine(lines[i], header);

				if (c.Item1 == 2 || c.Item1 == 5 && processCombined) //high vegetation
				{
					combinedField.AddPointInField(c.Item1, c.Item2);
				}
				//if(i%10000 == 0) {Console.WriteLine(c);}
			}


			if (processCombined)
			{
				Console.WriteLine("combinedField: " + combinedField);
				combinedField.FillMissingHeights(EHeight.GroundMax);
				combinedField.FillMissingHeights(EHeight.GroundMax);
				combinedField.CalculateLocalExtrems();
				combinedField.AssignTrees();
				//combinedField.AssignTreesToAll();

				//combinedField.ExportToObj(saveFileName + "_comb",
				//	EExportStrategy.None, new List<EHeight> { EHeight.GroundMax });
				CPointFieldExporter.ExportToObj(combinedField, saveFileName + "_comb",
					EExportStrategy.FillHeightsAroundDefined, new List<EHeight> { EHeight.Tree, EHeight.GroundMax });
			}

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}
