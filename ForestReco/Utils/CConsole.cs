using System;
using System.Runtime.InteropServices;

namespace ForestReco
{
	public class CConsole
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	}
}