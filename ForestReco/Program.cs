using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	public class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en");

			CMainForm mainForm = new CMainForm();
			Application.Run(mainForm);

			Console.ReadKey();
		}
	}
}
