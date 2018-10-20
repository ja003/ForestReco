﻿using System;
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
		private Button btnSellectReftreeFodlers;
		private TextBox textReftreeFolder;
		private Button btnStart;
		private TextBox textOutputFolder;
		private Button btnOutputFolder;
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
			textReftreeFolder.Text = CParameterSetter.reftreeFolderPath;
			textOutputFolder.Text = CParameterSetter.outputFolderPath;
		}

		private void InitializeComponent()
		{
			this.btnSellectForrest = new System.Windows.Forms.Button();
			this.textForrestFilePath = new System.Windows.Forms.TextBox();
			this.btnSellectReftreeFodlers = new System.Windows.Forms.Button();
			this.textReftreeFolder = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.textOutputFolder = new System.Windows.Forms.TextBox();
			this.btnOutputFolder = new System.Windows.Forms.Button();
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
			// btnSellectReftreeFodlers
			// 
			this.btnSellectReftreeFodlers.Location = new System.Drawing.Point(0, 37);
			this.btnSellectReftreeFodlers.Name = "btnSellectReftreeFodlers";
			this.btnSellectReftreeFodlers.Size = new System.Drawing.Size(116, 31);
			this.btnSellectReftreeFodlers.TabIndex = 2;
			this.btnSellectReftreeFodlers.Text = "select reftree folders";
			this.btnSellectReftreeFodlers.UseVisualStyleBackColor = true;
			this.btnSellectReftreeFodlers.Click += new System.EventHandler(this.btnSellectReftreeFodlers_Click);
			// 
			// textReftreeFolder
			// 
			this.textReftreeFolder.Location = new System.Drawing.Point(122, 43);
			this.textReftreeFolder.Name = "textReftreeFolder";
			this.textReftreeFolder.Size = new System.Drawing.Size(341, 20);
			this.textReftreeFolder.TabIndex = 4;
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(0, 320);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(463, 50);
			this.btnStart.TabIndex = 5;
			this.btnStart.Text = "START";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// textOutputFolder
			// 
			this.textOutputFolder.Location = new System.Drawing.Point(122, 80);
			this.textOutputFolder.Name = "textOutputFolder";
			this.textOutputFolder.Size = new System.Drawing.Size(341, 20);
			this.textOutputFolder.TabIndex = 7;
			// 
			// btnOutputFolder
			// 
			this.btnOutputFolder.Location = new System.Drawing.Point(0, 74);
			this.btnOutputFolder.Name = "btnOutputFolder";
			this.btnOutputFolder.Size = new System.Drawing.Size(116, 31);
			this.btnOutputFolder.TabIndex = 6;
			this.btnOutputFolder.Text = "select output folder";
			this.btnOutputFolder.UseVisualStyleBackColor = true;
			this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
			// 
			// Program
			// 
			this.ClientSize = new System.Drawing.Size(475, 373);
			this.Controls.Add(this.textOutputFolder);
			this.Controls.Add(this.btnOutputFolder);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.textReftreeFolder);
			this.Controls.Add(this.btnSellectReftreeFodlers);
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

		private void btnSellectReftreeFodlers_Click(object sender, EventArgs e)
		{
			string folder = CParameterSetter.SelectFolder(CParameterSetter.reftreeFolderPathKey);
			textReftreeFolder.Clear();
			textReftreeFolder.Text = folder;
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			CProgramStarter.Start();
		}

		private void btnOutputFolder_Click(object sender, EventArgs e)
		{
			string folder = CParameterSetter.SelectFolder(CParameterSetter.outputFolderPathKey);
			textOutputFolder.Clear();
			textOutputFolder.Text = folder;
		}
	}
}
