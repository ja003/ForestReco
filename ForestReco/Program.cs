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

namespace ForestReco
{
	class Program
	{
		static void Main(string[] args)
		{
			// Example #1
			// Read the file as one string.
			//string text = File.ReadAllText(@"D:\Adam\projects\SDIPR\ForestReco\ForestReco\src\ANE_1000AGL.las");

			CultureInfo ci = new CultureInfo("en");
			Thread.CurrentThread.CurrentCulture = ci;


			string[] lines = File.ReadAllLines(@"D:\Adam\projects\SDIPR\podklady\data-small\TXT\ANE_1000AGL_txt.txt");
			
			CHeaderInfo header = new CHeaderInfo(lines[15], lines[16], lines[17], lines[18]);
			Console.WriteLine(header);

			List<Vector3> groundCoord = new List<Vector3>();
			List<Vector3> uncathCoord = new List<Vector3>();

			for (int i = 19; i < lines.Length; i++)
			{
				Tuple<int, Vector3> c = CCoordinatesParser.ParseLine(lines[i]);
				if(c.Item1 == 1){ uncathCoord.Add(c.Item2);}
				else if(c.Item1 == 2){ groundCoord.Add(c.Item2); }
				if(i%10000 == 0) {Console.WriteLine(c);}
			}
			Console.WriteLine(groundCoord.Count);


			Console.WriteLine("Press any key to exit.");
			System.Console.ReadKey();
		}
	}
}
