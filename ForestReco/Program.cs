using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
			
			string[] lines = File.ReadAllLines(@"D:\Adam\projects\SDIPR\ForestReco\ForestReco\src\ANE_1000AGL.las");
			
			//foreach (string line in lines)
			for(int i = 0; i < 5; i++)
			{
				// Use a tab to indent each line of the file.
				Console.WriteLine(lines[i]);
			}
			
			Console.WriteLine("Press any key to exit.");
			System.Console.ReadKey();
		}
	}
}
