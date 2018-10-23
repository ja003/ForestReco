using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	public class CMainForm : Form
	{
		private TextBox textForrestFilePath;
		private Button btnSellectReftreeFodlers;
		private TextBox textReftreeFolder;
		private Button btnStart;
		private TextBox textOutputFolder;
		private Button btnOutputFolder;
		public ProgressBar progressBar;
		public TextBox textProgress;
		private Button btnAbort;
		private Button button1;
		private Button btnHintPartition;
		private Label labelPartition;
		private TextBox textPartition;
		private TrackBar trackBarPartition;
		private TextBox textCheckTreePath;
		private Button btnSelectCheckTree;
		private Button btnSellectForrest;

		public CMainForm()
		{
			InitializeComponent();
			InitializeValues();
		}
		
		private void InitializeValues()
		{
			CParameterSetter.Init();
			textForrestFilePath.Text = CParameterSetter.forrestFilePath;
			textReftreeFolder.Text = CParameterSetter.reftreeFolderPath;
			textOutputFolder.Text = CParameterSetter.outputFolderPath;
			textCheckTreePath.Text = CParameterSetter.checkTreeFilePath;

			textPartition.Text = CParameterSetter.partitionStep + " m";
			trackBarPartition.Value = CParameterSetter.partitionStep;
		}


		private void MainForm_Load(object sender, EventArgs e)
		{

		}



		private void btnStart_Click(object sender, EventArgs e)
		{
			CProgramStarter.Start();
		}

		


		private void btnAbort_Click(object sender, EventArgs e)
		{
			CProgramStarter.Abort();
		}


		

		private void btnToggleConsole_Click(object sender, EventArgs e)
		{
			CParameterSetter.ToggleConsoleVisibility();
		}


		private void trackBarPartition_Scroll(object sender, EventArgs e)
		{
			textPartition.Text = trackBarPartition.Value + " m";
			CParameterSetter.SetParameter("partitionStep", trackBarPartition.Value);
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
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.textProgress = new System.Windows.Forms.TextBox();
			this.btnAbort = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.btnHintPartition = new System.Windows.Forms.Button();
			this.labelPartition = new System.Windows.Forms.Label();
			this.textPartition = new System.Windows.Forms.TextBox();
			this.trackBarPartition = new System.Windows.Forms.TrackBar();
			this.textCheckTreePath = new System.Windows.Forms.TextBox();
			this.btnSelectCheckTree = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.trackBarPartition)).BeginInit();
			this.SuspendLayout();
			// 
			// btnSellectForrest
			// 
			this.btnSellectForrest.Location = new System.Drawing.Point(8, 10);
			this.btnSellectForrest.Name = "btnSellectForrest";
			this.btnSellectForrest.Size = new System.Drawing.Size(120, 31);
			this.btnSellectForrest.TabIndex = 0;
			this.btnSellectForrest.Text = "select forrest file";
			this.btnSellectForrest.UseVisualStyleBackColor = true;
			this.btnSellectForrest.Click += new System.EventHandler(this.btnSellectForrest_Click);
			// 
			// textForrestFilePath
			// 
			this.textForrestFilePath.Location = new System.Drawing.Point(146, 12);
			this.textForrestFilePath.Name = "textForrestFilePath";
			this.textForrestFilePath.Size = new System.Drawing.Size(592, 22);
			this.textForrestFilePath.TabIndex = 1;
			this.textForrestFilePath.TextChanged += new System.EventHandler(this.textForrestFilePath_TextChanged);
			// 
			// btnSellectReftreeFodlers
			// 
			this.btnSellectReftreeFodlers.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnSellectReftreeFodlers.Location = new System.Drawing.Point(8, 47);
			this.btnSellectReftreeFodlers.Name = "btnSellectReftreeFodlers";
			this.btnSellectReftreeFodlers.Size = new System.Drawing.Size(120, 31);
			this.btnSellectReftreeFodlers.TabIndex = 2;
			this.btnSellectReftreeFodlers.Text = "select reftree folders";
			this.btnSellectReftreeFodlers.UseVisualStyleBackColor = true;
			this.btnSellectReftreeFodlers.Click += new System.EventHandler(this.btnSellectReftreeFodlers_Click);
			// 
			// textReftreeFolder
			// 
			this.textReftreeFolder.Location = new System.Drawing.Point(146, 49);
			this.textReftreeFolder.Name = "textReftreeFolder";
			this.textReftreeFolder.Size = new System.Drawing.Size(592, 22);
			this.textReftreeFolder.TabIndex = 4;
			this.textReftreeFolder.TextChanged += new System.EventHandler(this.textReftreeFolder_TextChanged);
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(12, 311);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(589, 50);
			this.btnStart.TabIndex = 5;
			this.btnStart.Text = "START";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// textOutputFolder
			// 
			this.textOutputFolder.Location = new System.Drawing.Point(146, 86);
			this.textOutputFolder.Name = "textOutputFolder";
			this.textOutputFolder.Size = new System.Drawing.Size(592, 22);
			this.textOutputFolder.TabIndex = 7;
			this.textOutputFolder.TextChanged += new System.EventHandler(this.textOutputFolder_TextChanged);
			// 
			// btnOutputFolder
			// 
			this.btnOutputFolder.Location = new System.Drawing.Point(8, 84);
			this.btnOutputFolder.Name = "btnOutputFolder";
			this.btnOutputFolder.Size = new System.Drawing.Size(120, 31);
			this.btnOutputFolder.TabIndex = 6;
			this.btnOutputFolder.Text = "select output folder";
			this.btnOutputFolder.UseVisualStyleBackColor = true;
			this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(12, 418);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(589, 23);
			this.progressBar.TabIndex = 9;
			// 
			// textProgress
			// 
			this.textProgress.Location = new System.Drawing.Point(12, 367);
			this.textProgress.Multiline = true;
			this.textProgress.Name = "textProgress";
			this.textProgress.Size = new System.Drawing.Size(589, 45);
			this.textProgress.TabIndex = 10;
			// 
			// btnAbort
			// 
			this.btnAbort.Location = new System.Drawing.Point(628, 391);
			this.btnAbort.Name = "btnAbort";
			this.btnAbort.Size = new System.Drawing.Size(110, 50);
			this.btnAbort.TabIndex = 11;
			this.btnAbort.Text = "ABORT";
			this.btnAbort.UseVisualStyleBackColor = true;
			this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(629, 353);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(109, 32);
			this.button1.TabIndex = 12;
			this.button1.Text = "toggle console";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.btnToggleConsole_Click);
			// 
			// btnHintPartition
			// 
			this.btnHintPartition.Location = new System.Drawing.Point(12, 219);
			this.btnHintPartition.Name = "btnHintPartition";
			this.btnHintPartition.Size = new System.Drawing.Size(37, 29);
			this.btnHintPartition.TabIndex = 13;
			this.btnHintPartition.Text = "?";
			this.btnHintPartition.UseVisualStyleBackColor = true;
			this.btnHintPartition.Click += new System.EventHandler(this.btnHintPartition_Click);
			// 
			// labelPartition
			// 
			this.labelPartition.AutoSize = true;
			this.labelPartition.Location = new System.Drawing.Point(55, 227);
			this.labelPartition.Name = "labelPartition";
			this.labelPartition.Size = new System.Drawing.Size(90, 17);
			this.labelPartition.TabIndex = 14;
			this.labelPartition.Text = "partition step";
			// 
			// textPartition
			// 
			this.textPartition.Location = new System.Drawing.Point(128, 224);
			this.textPartition.Name = "textPartition";
			this.textPartition.ReadOnly = true;
			this.textPartition.Size = new System.Drawing.Size(34, 22);
			this.textPartition.TabIndex = 16;
			// 
			// trackBarPartition
			// 
			this.trackBarPartition.AutoSize = false;
			this.trackBarPartition.LargeChange = 10;
			this.trackBarPartition.Location = new System.Drawing.Point(12, 254);
			this.trackBarPartition.Maximum = 200;
			this.trackBarPartition.Minimum = 10;
			this.trackBarPartition.Name = "trackBarPartition";
			this.trackBarPartition.Size = new System.Drawing.Size(150, 30);
			this.trackBarPartition.TabIndex = 19;
			this.trackBarPartition.TickFrequency = 5;
			this.trackBarPartition.Value = 30;
			this.trackBarPartition.Scroll += new System.EventHandler(this.trackBarPartition_Scroll);
			// 
			// textCheckTreePath
			// 
			this.textCheckTreePath.Location = new System.Drawing.Point(146, 123);
			this.textCheckTreePath.Name = "textCheckTreePath";
			this.textCheckTreePath.Size = new System.Drawing.Size(592, 22);
			this.textCheckTreePath.TabIndex = 21;
			this.textCheckTreePath.TextChanged += new System.EventHandler(this.textCheckTreePath_TextChanged);
			// 
			// btnSelectCheckTree
			// 
			this.btnSelectCheckTree.Location = new System.Drawing.Point(8, 121);
			this.btnSelectCheckTree.Name = "btnSelectCheckTree";
			this.btnSelectCheckTree.Size = new System.Drawing.Size(120, 31);
			this.btnSelectCheckTree.TabIndex = 20;
			this.btnSelectCheckTree.Text = "select checktree";
			this.btnSelectCheckTree.UseVisualStyleBackColor = true;
			this.btnSelectCheckTree.Click += new System.EventHandler(this.btnSelectCheckTree_Click);
			// 
			// CMainForm
			// 
			this.ClientSize = new System.Drawing.Size(750, 449);
			this.Controls.Add(this.textCheckTreePath);
			this.Controls.Add(this.btnSelectCheckTree);
			this.Controls.Add(this.trackBarPartition);
			this.Controls.Add(this.textPartition);
			this.Controls.Add(this.labelPartition);
			this.Controls.Add(this.btnHintPartition);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.btnAbort);
			this.Controls.Add(this.textProgress);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.textOutputFolder);
			this.Controls.Add(this.btnOutputFolder);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.textReftreeFolder);
			this.Controls.Add(this.btnSellectReftreeFodlers);
			this.Controls.Add(this.textForrestFilePath);
			this.Controls.Add(this.btnSellectForrest);
			this.Name = "CMainForm";
			this.Text = "ForrestReco";
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.trackBarPartition)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void textOutputFolder_TextChanged(object sender, EventArgs e)
		{
			CDebug.Warning("txt change " + textOutputFolder.Text);
			CParameterSetter.SetParameter(
				CParameterSetter.outputFolderPathKey, textOutputFolder.Text);
		}

		private void btnOutputFolder_Click(object sender, EventArgs e)
		{
			string folder = CParameterSetter.SelectFolder();
			if (folder.Length == 0)
			{
				CDebug.Warning("no folder selected");
				return;
			}
			textOutputFolder.Clear();
			textOutputFolder.Text = folder;
		}


		private void btnSellectReftreeFodlers_Click(object sender, EventArgs e)
		{
			string folder = CParameterSetter.SelectFolder();
			if (folder.Length == 0)
			{
				CDebug.Warning("no folder selected");
				return;
			}
			textReftreeFolder.Clear();
			textReftreeFolder.Text = folder;
		}

		private void textReftreeFolder_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(
				CParameterSetter.reftreeFolderPathKey, textReftreeFolder.Text);
		}


		private void btnSellectForrest_Click(object sender, EventArgs e)
		{
			string path = CParameterSetter.SelectFile("Select forrest file");
			if (path.Length == 0)
			{
				CDebug.Warning("no path selected");
				return;
			}
			textForrestFilePath.Clear();
			textForrestFilePath.Text = path;
		}
		

		private void textForrestFilePath_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(
				CParameterSetter.forrestFilePathKey, textForrestFilePath.Text);
		}

		private void btnHintPartition_Click(object sender, EventArgs e)
		{
			CHintManager.ShowHint(EHint.PartitionStep);
		}

		private void btnSelectCheckTree_Click(object sender, EventArgs e)
		{
			string path = CParameterSetter.SelectFile("Select checktree file");
			if (path.Length == 0)
			{
				CDebug.Warning("no path selected");
				return;
			}
			textCheckTreePath.Clear();
			textCheckTreePath.Text = path;
		}

		private void textCheckTreePath_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(
				CParameterSetter.checkTreeFilePathKey, textCheckTreePath.Text);
		}
	}
}
