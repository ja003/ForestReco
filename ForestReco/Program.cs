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
			string[] lines = File.ReadAllLines(@"D:\ja004\OneDrive - MUNI\ŠKOLA [old]\SDIPR\podklady\data-small\TXT\" + fileName + @".txt");

			CHeaderInfo header = new CHeaderInfo(lines[15], lines[16], lines[17], lines[18]);
			Console.WriteLine(header);

			//TODO: uncommnent to see just header info
			//Console.ReadKey();
			//return;

			//prepare data structures 

			float stepSize = .5f; //in meters

			//no need to record depth information in groundField
			CCoordinatesField groundField = new CCoordinatesField(header, stepSize, false);
			CCoordinatesField highVegetationField = new CCoordinatesField(header, stepSize, false);

			//store coordinates to corresponding data strucures based on their class
			int linesToRead = lines.Length;
			//linesToRead = 10000;

			for (int i = 19; i < linesToRead; i++)
			{
				// <class, coordinate>
				Tuple<int, Vector3> c = CCoordinatesParser.ParseLine(lines[i], header);

				if (c.Item1 == 2) //ground
				{
					//groundField.AddCoordinate(c.Item2);
				}
				else if (c.Item1 == 5) //high vegetation
				{
					highVegetationField.AddCoordinate(c.Item2);
				}
				//if(i%10000 == 0) {Console.WriteLine(c);}
			}

			//TODO: find local maxima in highVegetationField
			//highVegetationField.DetectLocalMaximas()


			//Console.WriteLine("groundField: " + groundField);
			Console.WriteLine("highVegetationField: " + highVegetationField);

			//TODO: uncommnet for OBJ export
			//groundField.ExportToObj(fileName);
			highVegetationField.ExportToObj(fileName + "_trees");

			/*
			Console.WriteLine("groundField: ");
			groundField.DebugStringArray(EHeight.Average);
			Console.WriteLine("highVegetationField MAX: ");
			highVegetationField.DebugStringArray(EHeight.Max);
			Console.WriteLine("highVegetationField MIN: ");
			highVegetationField.DebugStringArray(EHeight.Min);
			*/

			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
		}
	}
}
