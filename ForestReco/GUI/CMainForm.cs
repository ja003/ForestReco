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
		private TrackBar trackBarGroundArrayStep;
		private TextBox textGroundArrayStep;
		private Label labelGroundArrayStep;
		private Button btnHintGroundArraySTep;
		private TrackBar trackBarTreeExtent;
		private TextBox textTreeExtent;
		private Label labelTreeExtent;
		private Button button2;
		private TrackBar trackBarTreeExtentMultiply;
		private TextBox textTreeExtentMultiply;
		private Label labelTreeExtentMultiply;
		private Button button3;
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

			//partition
			textPartition.Text = CParameterSetter.partitionStep + " m";
			trackBarPartition.Value = CParameterSetter.partitionStep;

			//gorund array step
			trackBarGroundArrayStep.Value = (int)(CParameterSetter.groundArrayStep * 10f);
			textGroundArrayStep.Text =
				CParameterSetter.groundArrayStep.ToString("0.00") + " m";

			//tree extent
			trackBarTreeExtent.Value = (int)(CParameterSetter.treeExtent * 10f);
			textTreeExtent.Text =
				CParameterSetter.treeExtent.ToString("0.00") + " m";

			//tree extent multiply
			trackBarTreeExtentMultiply.Value = (int)(CParameterSetter.treeExtentMultiply * 10f);
			textTreeExtentMultiply.Text =
				CParameterSetter.treeExtentMultiply.ToString("0.00");


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
			this.trackBarGroundArrayStep = new System.Windows.Forms.TrackBar();
			this.textGroundArrayStep = new System.Windows.Forms.TextBox();
			this.labelGroundArrayStep = new System.Windows.Forms.Label();
			this.btnHintGroundArraySTep = new System.Windows.Forms.Button();
			this.trackBarTreeExtent = new System.Windows.Forms.TrackBar();
			this.textTreeExtent = new System.Windows.Forms.TextBox();
			this.labelTreeExtent = new System.Windows.Forms.Label();
			this.button2 = new System.Windows.Forms.Button();
			this.trackBarTreeExtentMultiply = new System.Windows.Forms.TrackBar();
			this.textTreeExtentMultiply = new System.Windows.Forms.TextBox();
			this.labelTreeExtentMultiply = new System.Windows.Forms.Label();
			this.button3 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.trackBarPartition)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarGroundArrayStep)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtent)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtentMultiply)).BeginInit();
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
			this.textPartition.Location = new System.Drawing.Point(143, 224);
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
			this.trackBarPartition.Size = new System.Drawing.Size(165, 30);
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
			// trackBarGroundArrayStep
			// 
			this.trackBarGroundArrayStep.AutoSize = false;
			this.trackBarGroundArrayStep.LargeChange = 10;
			this.trackBarGroundArrayStep.Location = new System.Drawing.Point(192, 256);
			this.trackBarGroundArrayStep.Maximum = 100;
			this.trackBarGroundArrayStep.Minimum = 5;
			this.trackBarGroundArrayStep.Name = "trackBarGroundArrayStep";
			this.trackBarGroundArrayStep.Size = new System.Drawing.Size(195, 30);
			this.trackBarGroundArrayStep.TabIndex = 25;
			this.trackBarGroundArrayStep.TickFrequency = 5;
			this.trackBarGroundArrayStep.Value = 10;
			this.trackBarGroundArrayStep.Scroll += new System.EventHandler(this.trackBarGroundArrayStep_Scroll);
			// 
			// textGroundArrayStep
			// 
			this.textGroundArrayStep.Location = new System.Drawing.Point(353, 226);
			this.textGroundArrayStep.Name = "textGroundArrayStep";
			this.textGroundArrayStep.ReadOnly = true;
			this.textGroundArrayStep.Size = new System.Drawing.Size(34, 22);
			this.textGroundArrayStep.TabIndex = 24;
			// 
			// labelGroundArrayStep
			// 
			this.labelGroundArrayStep.AutoSize = true;
			this.labelGroundArrayStep.Location = new System.Drawing.Point(235, 229);
			this.labelGroundArrayStep.Name = "labelGroundArrayStep";
			this.labelGroundArrayStep.Size = new System.Drawing.Size(121, 17);
			this.labelGroundArrayStep.TabIndex = 23;
			this.labelGroundArrayStep.Text = "ground array step";
			// 
			// btnHintGroundArraySTep
			// 
			this.btnHintGroundArraySTep.Location = new System.Drawing.Point(192, 221);
			this.btnHintGroundArraySTep.Name = "btnHintGroundArraySTep";
			this.btnHintGroundArraySTep.Size = new System.Drawing.Size(37, 29);
			this.btnHintGroundArraySTep.TabIndex = 22;
			this.btnHintGroundArraySTep.Text = "?";
			this.btnHintGroundArraySTep.UseVisualStyleBackColor = true;
			// 
			// trackBarTreeExtent
			// 
			this.trackBarTreeExtent.AutoSize = false;
			this.trackBarTreeExtent.LargeChange = 10;
			this.trackBarTreeExtent.Location = new System.Drawing.Point(406, 255);
			this.trackBarTreeExtent.Maximum = 30;
			this.trackBarTreeExtent.Minimum = 5;
			this.trackBarTreeExtent.Name = "trackBarTreeExtent";
			this.trackBarTreeExtent.Size = new System.Drawing.Size(195, 30);
			this.trackBarTreeExtent.TabIndex = 29;
			this.trackBarTreeExtent.TickFrequency = 5;
			this.trackBarTreeExtent.Value = 10;
			this.trackBarTreeExtent.Scroll += new System.EventHandler(this.trackBarTreeExtent_Scroll);
			// 
			// textTreeExtent
			// 
			this.textTreeExtent.Location = new System.Drawing.Point(567, 225);
			this.textTreeExtent.Name = "textTreeExtent";
			this.textTreeExtent.ReadOnly = true;
			this.textTreeExtent.Size = new System.Drawing.Size(34, 22);
			this.textTreeExtent.TabIndex = 28;
			// 
			// labelTreeExtent
			// 
			this.labelTreeExtent.AutoSize = true;
			this.labelTreeExtent.Location = new System.Drawing.Point(449, 228);
			this.labelTreeExtent.Name = "labelTreeExtent";
			this.labelTreeExtent.Size = new System.Drawing.Size(110, 17);
			this.labelTreeExtent.TabIndex = 27;
			this.labelTreeExtent.Text = "base tree extent";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(406, 220);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(37, 29);
			this.button2.TabIndex = 26;
			this.button2.Text = "?";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// trackBarTreeExtentMultiply
			// 
			this.trackBarTreeExtentMultiply.AutoSize = false;
			this.trackBarTreeExtentMultiply.LargeChange = 10;
			this.trackBarTreeExtentMultiply.Location = new System.Drawing.Point(613, 259);
			this.trackBarTreeExtentMultiply.Maximum = 30;
			this.trackBarTreeExtentMultiply.Minimum = 10;
			this.trackBarTreeExtentMultiply.Name = "trackBarTreeExtentMultiply";
			this.trackBarTreeExtentMultiply.Size = new System.Drawing.Size(195, 30);
			this.trackBarTreeExtentMultiply.TabIndex = 33;
			this.trackBarTreeExtentMultiply.TickFrequency = 5;
			this.trackBarTreeExtentMultiply.Value = 10;
			this.trackBarTreeExtentMultiply.Scroll += new System.EventHandler(this.trackBarTreeExtentMultiply_Scroll);
			// 
			// textTreeExtentMultiply
			// 
			this.textTreeExtentMultiply.Location = new System.Drawing.Point(774, 229);
			this.textTreeExtentMultiply.Name = "textTreeExtentMultiply";
			this.textTreeExtentMultiply.ReadOnly = true;
			this.textTreeExtentMultiply.Size = new System.Drawing.Size(34, 22);
			this.textTreeExtentMultiply.TabIndex = 32;
			// 
			// labelTreeExtentMultiply
			// 
			this.labelTreeExtentMultiply.AutoSize = true;
			this.labelTreeExtentMultiply.Location = new System.Drawing.Point(656, 232);
			this.labelTreeExtentMultiply.Name = "labelTreeExtentMultiply";
			this.labelTreeExtentMultiply.Size = new System.Drawing.Size(126, 17);
			this.labelTreeExtentMultiply.TabIndex = 31;
			this.labelTreeExtentMultiply.Text = "tree extent multiply";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(613, 224);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(37, 29);
			this.button3.TabIndex = 30;
			this.button3.Text = "?";
			this.button3.UseVisualStyleBackColor = true;
			// 
			// CMainForm
			// 
			this.ClientSize = new System.Drawing.Size(815, 449);
			this.Controls.Add(this.trackBarTreeExtentMultiply);
			this.Controls.Add(this.textTreeExtentMultiply);
			this.Controls.Add(this.labelTreeExtentMultiply);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.trackBarTreeExtent);
			this.Controls.Add(this.textTreeExtent);
			this.Controls.Add(this.labelTreeExtent);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.trackBarGroundArrayStep);
			this.Controls.Add(this.textGroundArrayStep);
			this.Controls.Add(this.labelGroundArrayStep);
			this.Controls.Add(this.btnHintGroundArraySTep);
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
			((System.ComponentModel.ISupportInitialize)(this.trackBarGroundArrayStep)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtent)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtentMultiply)).EndInit();
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

		private void trackBarPartition_Scroll(object sender, EventArgs e)
		{
			textPartition.Text = trackBarPartition.Value + " m";
			CParameterSetter.SetParameter(
				CParameterSetter.partitionStepKey, trackBarPartition.Value);
		}

		private void trackBarGroundArrayStep_Scroll(object sender, EventArgs e)
		{
			float value = trackBarGroundArrayStep.Value / 10f;
			textGroundArrayStep.Text = value.ToString("0.0") + " m";
			CParameterSetter.SetParameter(CParameterSetter.groundArrayStepKey, value);
		}

		private void trackBarTreeExtent_Scroll(object sender, EventArgs e)
		{
			float value = trackBarTreeExtent.Value / 10f;
			textTreeExtent.Text = value.ToString("0.0") + " m";
			CParameterSetter.SetParameter(CParameterSetter.treeExtentKey, value);
		}

		private void trackBarTreeExtentMultiply_Scroll(object sender, EventArgs e)
		{
			float value = trackBarTreeExtentMultiply.Value / 10f;
			textTreeExtentMultiply.Text = value.ToString("0.0");
			CParameterSetter.SetParameter(CParameterSetter.treeExtentMultiplyKey, value);
		}
	}
}
