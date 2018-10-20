using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	public class Program : Form
	{
		private TextBox textForrestFilePath;
		private Button btnSellectForrest;

		public Program()
		{
			InitializeComponent();
			InitializeValues();
		}


		[STAThread]
		static void Main(string[] args)
		{
			Application.Run(new Program());

			return;
			CProgramStarter.Start();
		}



		private void InitializeValues()
		{
			CParameterSetter.Init();
			textForrestFilePath.Text = CParameterSetter.forrestFilePath;
		}

		private void InitializeComponent()
		{
			this.btnSellectForrest = new System.Windows.Forms.Button();
			this.textForrestFilePath = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnSellectForrest
			// 
			this.btnSellectForrest.Location = new System.Drawing.Point(0, 0);
			this.btnSellectForrest.Name = "btnSellectForrest";
			this.btnSellectForrest.Size = new System.Drawing.Size(116, 31);
			this.btnSellectForrest.TabIndex = 0;
			this.btnSellectForrest.Text = "select forrest file";
			this.btnSellectForrest.UseVisualStyleBackColor = true;
			this.btnSellectForrest.Click += new System.EventHandler(this.btnSellectForrest_Click);
			// 
			// textForrestFilePath
			// 
			this.textForrestFilePath.Location = new System.Drawing.Point(122, 6);
			this.textForrestFilePath.Name = "textForrestFilePath";
			this.textForrestFilePath.Size = new System.Drawing.Size(341, 20);
			this.textForrestFilePath.TabIndex = 1;
			this.textForrestFilePath.TextChanged += new System.EventHandler(this.textForrestFilePath_TextChanged);
			// 
			// Program
			// 
			this.ClientSize = new System.Drawing.Size(475, 373);
			this.Controls.Add(this.textForrestFilePath);
			this.Controls.Add(this.btnSellectForrest);
			this.Name = "Program";
			this.Text = "ForrestReco";
			this.Load += new System.EventHandler(this.Program_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void Program_Load(object sender, EventArgs e)
		{

		}

		private void btnSellectForrest_Click(object sender, EventArgs e)
		{
			string path = CParameterSetter.SelectForrestFile();
			textForrestFilePath.Clear();
			textForrestFilePath.Text = path;
		}

		private void textForrestFilePath_TextChanged(object sender, EventArgs e)
		{

		}
	}
}
