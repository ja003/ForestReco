﻿using System;
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
			
			string[] lines = File.ReadAllLines(@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\data-small\TXT\" + fileName + @".txt");

			CHeaderInfo header = new CHeaderInfo(lines[15], lines[16], lines[17], lines[18]);
			Console.WriteLine(header);

			//TODO: uncommnent to see just header info
			//Console.ReadKey();
			//return;

			//prepare data structures 

			float stepSize = .25f; //in meters

			//no need to record depth information in groundField
			CPointFieldController groundField = new CPointFieldController(header, stepSize, 0);
			CPointFieldController highVegetationField = new CPointFieldController(header, stepSize, 0);
			//CCoordinatesField highVegetationField = new CCoordinatesField(header, stepSize, true);

			bool processGround = false;
			bool processHighVegetation = true;

			//store coordinates to corresponding data strucures based on their class
			int linesToRead = lines.Length;
			//linesToRead = 10000;

			for (int i = 19; i < linesToRead; i++)
			{
				// <class, coordinate>
				Tuple<int, Vector3> c = CCoordinatesParser.ParseLine(lines[i], header);

				if (c.Item1 == 2 && processGround) //ground
				{
					groundField.AddPointInFields(c.Item2);
				}
				else if (c.Item1 == 5 && processHighVegetation) //high vegetation
				{
					highVegetationField.AddPointInFields(c.Item2);
				}
				//if(i%10000 == 0) {Console.WriteLine(c);}
			}

			//TODO: find local maxima in highVegetationField
			//highVegetationField.DetectLocalMaximas()

			string saveFileName = "Cesta_";

			if (processGround)
			{
				Console.WriteLine("groundField: " + groundField);
				//TODO: to fill missong coordinates use FillMissingHeight startegy
				groundField.ExportToObj(saveFileName, EExportStrategy.None, EHeight.Max);
			}
			if (processHighVegetation)
			{
				Console.WriteLine("highVegetationField: " + highVegetationField);
				highVegetationField.CalculateLocalExtrems(0);
				//highVegetationField.AssignTrees(stepSize);
				highVegetationField.ExportToObj(saveFileName + "_trees", 
					EExportStrategy.FillHeightsAroundDefined, EHeight.Tree);
			}
			
			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}
