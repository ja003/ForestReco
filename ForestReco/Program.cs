using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	public partial class Program : Form
	{
		public Program()
		{
			InitializeComponent();
		}

		static void Main()
		{
			Application.Run(new Program());
			CProgramStarter.Start();
		}


		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// Program
			// 
			this.ClientSize = new System.Drawing.Size(475, 373);
			this.Name = "Program";
			this.Text = "ForrestReco";
			this.Load += new System.EventHandler(this.Program_Load);
			this.ResumeLayout(false);

		}

		private void Program_Load(object sender, EventArgs e)
		{

		}
	}
}
