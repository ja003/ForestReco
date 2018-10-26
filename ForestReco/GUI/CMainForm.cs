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
		private Label labelPartition;
		private TextBox textPartition;
		private TrackBar trackBarPartition;
		private TextBox textCheckTreePath;
		private Button btnSelectCheckTree;
		private TrackBar trackBarGroundArrayStep;
		private TextBox textGroundArrayStep;
		private Label labelGroundArrayStep;
		private TrackBar trackBarTreeExtent;
		private TextBox textTreeExtent;
		private Label labelTreeExtent;
		private TrackBar trackBarTreeExtentMultiply;
		private TextBox textTreeExtentMultiply;
		private Label labelTreeExtentMultiply;
		private TrackBar trackBarAvgTreeHeight;
		private TextBox textAvgTreeHeight;
		private Label labelAvgTreeHeight;
		private CheckBox checkBoxExportTreeStructures;
		private ToolTip myToolTip;
		private System.ComponentModel.IContainer components;
		private CheckBox checkBoxExportInvalidTrees;
		private CheckBox checkBoxExportRefTrees;
		private CheckBox checkBoxAssignRefTreesRandom;
		private CheckBox checkBoxUseCheckTree;
		private CheckBox checkBoxExportCheckTrees;
		private CheckBox checkBoxReducedReftrees;
		private CheckBox checkBoxFilterPoints;
		private CheckBox checkBoxExportPoints;
		private Button btnSellectForrest;

		public CMainForm()
		{
			CProjectData.mainForm = this;

			InitializeComponent();
			InitializeValues();

			//CProgramStarter.Start();
		}

		private void InitializeValues()
		{
			CParameterSetter.Init();
			textForrestFilePath.Text = CParameterSetter.GetStringSettings(ESettings.forrestFilePath);
			textReftreeFolder.Text = CParameterSetter.GetStringSettings(ESettings.reftreeFolderPath);
			textOutputFolder.Text = CParameterSetter.GetStringSettings(ESettings.outputFolderPath);
			textCheckTreePath.Text = CParameterSetter.GetStringSettings(ESettings.checkTreeFilePath);

			//partition
			textPartition.Text = CParameterSetter.GetIntSettings(ESettings.partitionStep) + " m";
			trackBarPartition.Value = CParameterSetter.GetIntSettings(ESettings.partitionStep);

			//gorund array step
			float groundArrayStep = CParameterSetter.GetFloatSettings(ESettings.groundArrayStep);
			trackBarGroundArrayStep.Value = (int)(groundArrayStep * 10f);
			textGroundArrayStep.Text = groundArrayStep.ToString("0.0") + " m";

			//tree extent
			float treeExtent = CParameterSetter.GetFloatSettings(ESettings.treeExtent);
			trackBarTreeExtent.Value = (int)(treeExtent * 10f);
			textTreeExtent.Text = treeExtent.ToString("0.0") + " m";

			//tree extent multiply
			float treeExtentMultiply = CParameterSetter.GetFloatSettings(ESettings.treeExtentMultiply);
			trackBarTreeExtentMultiply.Value = (int)(treeExtentMultiply * 10f);
			textTreeExtentMultiply.Text = treeExtentMultiply.ToString("0.0");

			//average tree height
			textAvgTreeHeight.Text = CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh) + " m";
			trackBarAvgTreeHeight.Value = CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh);

			//bools
			checkBoxExportTreeStructures.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportTreeStructures);
			checkBoxExportInvalidTrees.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportInvalidTrees);
			checkBoxExportRefTrees.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportRefTrees);
			checkBoxAssignRefTreesRandom.Checked =
				CParameterSetter.GetBoolSettings(ESettings.assignRefTreesRandom);
			checkBoxUseCheckTree.Checked =
				CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile);
			checkBoxExportCheckTrees.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportCheckTrees);
			checkBoxReducedReftrees.Checked =
				CParameterSetter.GetBoolSettings(ESettings.useReducedReftreeModels);
			checkBoxFilterPoints.Checked =
				CParameterSetter.GetBoolSettings(ESettings.filterPoints);
			checkBoxExportPoints.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportPoints);

			SetTooltips();


		}

		private void SetTooltips()
		{
			myToolTip.SetToolTip(checkBoxExportTreeStructures,
				"Include tree detection structures in final export");
		}

		private void MainForm_Load(object sender, EventArgs e)
		{

		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			if (!CUiInputCheck.CheckProblems())
			{
				return;
			}


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
			this.components = new System.ComponentModel.Container();
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
			this.labelPartition = new System.Windows.Forms.Label();
			this.textPartition = new System.Windows.Forms.TextBox();
			this.trackBarPartition = new System.Windows.Forms.TrackBar();
			this.textCheckTreePath = new System.Windows.Forms.TextBox();
			this.btnSelectCheckTree = new System.Windows.Forms.Button();
			this.trackBarGroundArrayStep = new System.Windows.Forms.TrackBar();
			this.textGroundArrayStep = new System.Windows.Forms.TextBox();
			this.labelGroundArrayStep = new System.Windows.Forms.Label();
			this.trackBarTreeExtent = new System.Windows.Forms.TrackBar();
			this.textTreeExtent = new System.Windows.Forms.TextBox();
			this.labelTreeExtent = new System.Windows.Forms.Label();
			this.trackBarTreeExtentMultiply = new System.Windows.Forms.TrackBar();
			this.textTreeExtentMultiply = new System.Windows.Forms.TextBox();
			this.labelTreeExtentMultiply = new System.Windows.Forms.Label();
			this.trackBarAvgTreeHeight = new System.Windows.Forms.TrackBar();
			this.textAvgTreeHeight = new System.Windows.Forms.TextBox();
			this.labelAvgTreeHeight = new System.Windows.Forms.Label();
			this.checkBoxExportTreeStructures = new System.Windows.Forms.CheckBox();
			this.myToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.checkBoxExportInvalidTrees = new System.Windows.Forms.CheckBox();
			this.checkBoxExportRefTrees = new System.Windows.Forms.CheckBox();
			this.checkBoxAssignRefTreesRandom = new System.Windows.Forms.CheckBox();
			this.checkBoxUseCheckTree = new System.Windows.Forms.CheckBox();
			this.checkBoxExportCheckTrees = new System.Windows.Forms.CheckBox();
			this.checkBoxReducedReftrees = new System.Windows.Forms.CheckBox();
			this.checkBoxFilterPoints = new System.Windows.Forms.CheckBox();
			this.checkBoxExportPoints = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.trackBarPartition)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarGroundArrayStep)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtent)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtentMultiply)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAvgTreeHeight)).BeginInit();
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
			this.textForrestFilePath.Size = new System.Drawing.Size(675, 22);
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
			this.textReftreeFolder.Size = new System.Drawing.Size(675, 22);
			this.textReftreeFolder.TabIndex = 4;
			this.textReftreeFolder.TextChanged += new System.EventHandler(this.textReftreeFolder_TextChanged);
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(234, 244);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(460, 48);
			this.btnStart.TabIndex = 5;
			this.btnStart.Text = "START";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// textOutputFolder
			// 
			this.textOutputFolder.Location = new System.Drawing.Point(146, 86);
			this.textOutputFolder.Name = "textOutputFolder";
			this.textOutputFolder.Size = new System.Drawing.Size(675, 22);
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
			this.progressBar.Location = new System.Drawing.Point(12, 525);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(589, 23);
			this.progressBar.TabIndex = 9;
			// 
			// textProgress
			// 
			this.textProgress.Location = new System.Drawing.Point(234, 300);
			this.textProgress.Multiline = true;
			this.textProgress.Name = "textProgress";
			this.textProgress.Size = new System.Drawing.Size(460, 100);
			this.textProgress.TabIndex = 10;
			// 
			// btnAbort
			// 
			this.btnAbort.Location = new System.Drawing.Point(711, 300);
			this.btnAbort.Name = "btnAbort";
			this.btnAbort.Size = new System.Drawing.Size(110, 100);
			this.btnAbort.TabIndex = 11;
			this.btnAbort.Text = "ABORT";
			this.btnAbort.UseVisualStyleBackColor = true;
			this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(715, 252);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(109, 32);
			this.button1.TabIndex = 12;
			this.button1.Text = "toggle console";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.btnToggleConsole_Click);
			// 
			// labelPartition
			// 
			this.labelPartition.AutoSize = true;
			this.labelPartition.Location = new System.Drawing.Point(19, 169);
			this.labelPartition.Name = "labelPartition";
			this.labelPartition.Size = new System.Drawing.Size(90, 17);
			this.labelPartition.TabIndex = 14;
			this.labelPartition.Text = "partition step";
			// 
			// textPartition
			// 
			this.textPartition.Location = new System.Drawing.Point(107, 166);
			this.textPartition.Name = "textPartition";
			this.textPartition.ReadOnly = true;
			this.textPartition.Size = new System.Drawing.Size(34, 22);
			this.textPartition.TabIndex = 16;
			// 
			// trackBarPartition
			// 
			this.trackBarPartition.AutoSize = false;
			this.trackBarPartition.LargeChange = 10;
			this.trackBarPartition.Location = new System.Drawing.Point(12, 198);
			this.trackBarPartition.Maximum = 200;
			this.trackBarPartition.Minimum = 10;
			this.trackBarPartition.Name = "trackBarPartition";
			this.trackBarPartition.Size = new System.Drawing.Size(129, 30);
			this.trackBarPartition.SmallChange = 5;
			this.trackBarPartition.TabIndex = 19;
			this.trackBarPartition.TickFrequency = 5;
			this.trackBarPartition.Value = 30;
			this.trackBarPartition.Scroll += new System.EventHandler(this.trackBarPartition_Scroll);
			// 
			// textCheckTreePath
			// 
			this.textCheckTreePath.Location = new System.Drawing.Point(146, 123);
			this.textCheckTreePath.Name = "textCheckTreePath";
			this.textCheckTreePath.Size = new System.Drawing.Size(675, 22);
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
			this.trackBarGroundArrayStep.Location = new System.Drawing.Point(156, 198);
			this.trackBarGroundArrayStep.Maximum = 100;
			this.trackBarGroundArrayStep.Minimum = 5;
			this.trackBarGroundArrayStep.Name = "trackBarGroundArrayStep";
			this.trackBarGroundArrayStep.Size = new System.Drawing.Size(159, 30);
			this.trackBarGroundArrayStep.SmallChange = 5;
			this.trackBarGroundArrayStep.TabIndex = 25;
			this.trackBarGroundArrayStep.TickFrequency = 5;
			this.trackBarGroundArrayStep.Value = 10;
			this.trackBarGroundArrayStep.Scroll += new System.EventHandler(this.trackBarGroundArrayStep_Scroll);
			// 
			// textGroundArrayStep
			// 
			this.textGroundArrayStep.Location = new System.Drawing.Point(281, 166);
			this.textGroundArrayStep.Name = "textGroundArrayStep";
			this.textGroundArrayStep.ReadOnly = true;
			this.textGroundArrayStep.Size = new System.Drawing.Size(34, 22);
			this.textGroundArrayStep.TabIndex = 24;
			// 
			// labelGroundArrayStep
			// 
			this.labelGroundArrayStep.AutoSize = true;
			this.labelGroundArrayStep.Location = new System.Drawing.Point(163, 169);
			this.labelGroundArrayStep.Name = "labelGroundArrayStep";
			this.labelGroundArrayStep.Size = new System.Drawing.Size(121, 17);
			this.labelGroundArrayStep.TabIndex = 23;
			this.labelGroundArrayStep.Text = "ground array step";
			// 
			// trackBarTreeExtent
			// 
			this.trackBarTreeExtent.AutoSize = false;
			this.trackBarTreeExtent.LargeChange = 10;
			this.trackBarTreeExtent.Location = new System.Drawing.Point(321, 198);
			this.trackBarTreeExtent.Maximum = 50;
			this.trackBarTreeExtent.Minimum = 5;
			this.trackBarTreeExtent.Name = "trackBarTreeExtent";
			this.trackBarTreeExtent.Size = new System.Drawing.Size(159, 30);
			this.trackBarTreeExtent.TabIndex = 29;
			this.trackBarTreeExtent.TickFrequency = 5;
			this.trackBarTreeExtent.Value = 10;
			this.trackBarTreeExtent.Scroll += new System.EventHandler(this.trackBarTreeExtent_Scroll);
			// 
			// textTreeExtent
			// 
			this.textTreeExtent.Location = new System.Drawing.Point(446, 166);
			this.textTreeExtent.Name = "textTreeExtent";
			this.textTreeExtent.ReadOnly = true;
			this.textTreeExtent.Size = new System.Drawing.Size(34, 22);
			this.textTreeExtent.TabIndex = 28;
			// 
			// labelTreeExtent
			// 
			this.labelTreeExtent.AutoSize = true;
			this.labelTreeExtent.Location = new System.Drawing.Point(328, 169);
			this.labelTreeExtent.Name = "labelTreeExtent";
			this.labelTreeExtent.Size = new System.Drawing.Size(110, 17);
			this.labelTreeExtent.TabIndex = 27;
			this.labelTreeExtent.Text = "base tree extent";
			// 
			// trackBarTreeExtentMultiply
			// 
			this.trackBarTreeExtentMultiply.AutoSize = false;
			this.trackBarTreeExtentMultiply.LargeChange = 10;
			this.trackBarTreeExtentMultiply.Location = new System.Drawing.Point(485, 197);
			this.trackBarTreeExtentMultiply.Maximum = 30;
			this.trackBarTreeExtentMultiply.Minimum = 10;
			this.trackBarTreeExtentMultiply.Name = "trackBarTreeExtentMultiply";
			this.trackBarTreeExtentMultiply.Size = new System.Drawing.Size(159, 30);
			this.trackBarTreeExtentMultiply.TabIndex = 33;
			this.trackBarTreeExtentMultiply.TickFrequency = 5;
			this.trackBarTreeExtentMultiply.Value = 10;
			this.trackBarTreeExtentMultiply.Scroll += new System.EventHandler(this.trackBarTreeExtentMultiply_Scroll);
			// 
			// textTreeExtentMultiply
			// 
			this.textTreeExtentMultiply.Location = new System.Drawing.Point(610, 168);
			this.textTreeExtentMultiply.Name = "textTreeExtentMultiply";
			this.textTreeExtentMultiply.ReadOnly = true;
			this.textTreeExtentMultiply.Size = new System.Drawing.Size(34, 22);
			this.textTreeExtentMultiply.TabIndex = 32;
			// 
			// labelTreeExtentMultiply
			// 
			this.labelTreeExtentMultiply.AutoSize = true;
			this.labelTreeExtentMultiply.Location = new System.Drawing.Point(492, 171);
			this.labelTreeExtentMultiply.Name = "labelTreeExtentMultiply";
			this.labelTreeExtentMultiply.Size = new System.Drawing.Size(126, 17);
			this.labelTreeExtentMultiply.TabIndex = 31;
			this.labelTreeExtentMultiply.Text = "tree extent multiply";
			// 
			// trackBarAvgTreeHeight
			// 
			this.trackBarAvgTreeHeight.AutoSize = false;
			this.trackBarAvgTreeHeight.LargeChange = 10;
			this.trackBarAvgTreeHeight.Location = new System.Drawing.Point(643, 199);
			this.trackBarAvgTreeHeight.Maximum = 50;
			this.trackBarAvgTreeHeight.Minimum = 5;
			this.trackBarAvgTreeHeight.Name = "trackBarAvgTreeHeight";
			this.trackBarAvgTreeHeight.Size = new System.Drawing.Size(178, 30);
			this.trackBarAvgTreeHeight.TabIndex = 37;
			this.trackBarAvgTreeHeight.TickFrequency = 5;
			this.trackBarAvgTreeHeight.Value = 15;
			this.trackBarAvgTreeHeight.Scroll += new System.EventHandler(this.trackBarAvgTreeHeight_Scroll);
			// 
			// textAvgTreeHeight
			// 
			this.textAvgTreeHeight.Location = new System.Drawing.Point(787, 171);
			this.textAvgTreeHeight.Name = "textAvgTreeHeight";
			this.textAvgTreeHeight.ReadOnly = true;
			this.textAvgTreeHeight.Size = new System.Drawing.Size(34, 22);
			this.textAvgTreeHeight.TabIndex = 36;
			// 
			// labelAvgTreeHeight
			// 
			this.labelAvgTreeHeight.AutoSize = true;
			this.labelAvgTreeHeight.Location = new System.Drawing.Point(650, 173);
			this.labelAvgTreeHeight.Name = "labelAvgTreeHeight";
			this.labelAvgTreeHeight.Size = new System.Drawing.Size(132, 17);
			this.labelAvgTreeHeight.TabIndex = 35;
			this.labelAvgTreeHeight.Text = "average tree height";
			// 
			// checkBoxExportTreeStructures
			// 
			this.checkBoxExportTreeStructures.AutoSize = true;
			this.checkBoxExportTreeStructures.Location = new System.Drawing.Point(22, 244);
			this.checkBoxExportTreeStructures.Name = "checkBoxExportTreeStructures";
			this.checkBoxExportTreeStructures.Size = new System.Drawing.Size(165, 21);
			this.checkBoxExportTreeStructures.TabIndex = 38;
			this.checkBoxExportTreeStructures.Text = "export tree structures";
			this.myToolTip.SetToolTip(this.checkBoxExportTreeStructures, "hh");
			this.checkBoxExportTreeStructures.UseVisualStyleBackColor = true;
			this.checkBoxExportTreeStructures.CheckedChanged += new System.EventHandler(this.checkBoxExportTreeStructures_CheckedChanged);
			// 
			// checkBoxExportInvalidTrees
			// 
			this.checkBoxExportInvalidTrees.AutoSize = true;
			this.checkBoxExportInvalidTrees.Location = new System.Drawing.Point(22, 271);
			this.checkBoxExportInvalidTrees.Name = "checkBoxExportInvalidTrees";
			this.checkBoxExportInvalidTrees.Size = new System.Drawing.Size(149, 21);
			this.checkBoxExportInvalidTrees.TabIndex = 39;
			this.checkBoxExportInvalidTrees.Text = "export invalid trees";
			this.myToolTip.SetToolTip(this.checkBoxExportInvalidTrees, "hh");
			this.checkBoxExportInvalidTrees.UseVisualStyleBackColor = true;
			this.checkBoxExportInvalidTrees.CheckedChanged += new System.EventHandler(this.checkBoxExportInvalidTrees_CheckedChanged);
			// 
			// checkBoxExportRefTrees
			// 
			this.checkBoxExportRefTrees.AutoSize = true;
			this.checkBoxExportRefTrees.Location = new System.Drawing.Point(22, 298);
			this.checkBoxExportRefTrees.Name = "checkBoxExportRefTrees";
			this.checkBoxExportRefTrees.Size = new System.Drawing.Size(122, 21);
			this.checkBoxExportRefTrees.TabIndex = 40;
			this.checkBoxExportRefTrees.Text = "export reftrees";
			this.myToolTip.SetToolTip(this.checkBoxExportRefTrees, "hh");
			this.checkBoxExportRefTrees.UseVisualStyleBackColor = true;
			this.checkBoxExportRefTrees.CheckedChanged += new System.EventHandler(this.checkBoxExportRefTrees_CheckedChanged);
			// 
			// checkBoxAssignRefTreesRandom
			// 
			this.checkBoxAssignRefTreesRandom.AutoSize = true;
			this.checkBoxAssignRefTreesRandom.Location = new System.Drawing.Point(22, 325);
			this.checkBoxAssignRefTreesRandom.Name = "checkBoxAssignRefTreesRandom";
			this.checkBoxAssignRefTreesRandom.Size = new System.Drawing.Size(176, 21);
			this.checkBoxAssignRefTreesRandom.TabIndex = 41;
			this.checkBoxAssignRefTreesRandom.Text = "assign reftrees random";
			this.myToolTip.SetToolTip(this.checkBoxAssignRefTreesRandom, "hh");
			this.checkBoxAssignRefTreesRandom.UseVisualStyleBackColor = true;
			this.checkBoxAssignRefTreesRandom.CheckedChanged += new System.EventHandler(this.checkBoxAssignRefTreesRandom_CheckedChanged);
			// 
			// checkBoxUseCheckTree
			// 
			this.checkBoxUseCheckTree.AutoSize = true;
			this.checkBoxUseCheckTree.Location = new System.Drawing.Point(22, 378);
			this.checkBoxUseCheckTree.Name = "checkBoxUseCheckTree";
			this.checkBoxUseCheckTree.Size = new System.Drawing.Size(141, 21);
			this.checkBoxUseCheckTree.TabIndex = 42;
			this.checkBoxUseCheckTree.Text = "use checktree file";
			this.myToolTip.SetToolTip(this.checkBoxUseCheckTree, "hh");
			this.checkBoxUseCheckTree.UseVisualStyleBackColor = true;
			this.checkBoxUseCheckTree.CheckedChanged += new System.EventHandler(this.checkBoxUseCheckTree_CheckedChanged);
			// 
			// checkBoxExportCheckTrees
			// 
			this.checkBoxExportCheckTrees.AutoSize = true;
			this.checkBoxExportCheckTrees.Location = new System.Drawing.Point(22, 405);
			this.checkBoxExportCheckTrees.Name = "checkBoxExportCheckTrees";
			this.checkBoxExportCheckTrees.Size = new System.Drawing.Size(142, 21);
			this.checkBoxExportCheckTrees.TabIndex = 43;
			this.checkBoxExportCheckTrees.Text = "export checktrees";
			this.myToolTip.SetToolTip(this.checkBoxExportCheckTrees, "hh");
			this.checkBoxExportCheckTrees.UseVisualStyleBackColor = true;
			this.checkBoxExportCheckTrees.CheckedChanged += new System.EventHandler(this.checkBoxExportCheckTrees_CheckedChanged);
			// 
			// checkBoxReducedReftrees
			// 
			this.checkBoxReducedReftrees.AutoSize = true;
			this.checkBoxReducedReftrees.Location = new System.Drawing.Point(22, 352);
			this.checkBoxReducedReftrees.Name = "checkBoxReducedReftrees";
			this.checkBoxReducedReftrees.Size = new System.Drawing.Size(204, 21);
			this.checkBoxReducedReftrees.TabIndex = 44;
			this.checkBoxReducedReftrees.Text = "use reduced reftree models";
			this.myToolTip.SetToolTip(this.checkBoxReducedReftrees, "hh");
			this.checkBoxReducedReftrees.UseVisualStyleBackColor = true;
			this.checkBoxReducedReftrees.CheckedChanged += new System.EventHandler(this.checkBoxReducedReftrees_CheckedChanged);
			// 
			// checkBoxFilterPoints
			// 
			this.checkBoxFilterPoints.AutoSize = true;
			this.checkBoxFilterPoints.Location = new System.Drawing.Point(22, 432);
			this.checkBoxFilterPoints.Name = "checkBoxFilterPoints";
			this.checkBoxFilterPoints.Size = new System.Drawing.Size(99, 21);
			this.checkBoxFilterPoints.TabIndex = 45;
			this.checkBoxFilterPoints.Text = "filter points";
			this.myToolTip.SetToolTip(this.checkBoxFilterPoints, "hh");
			this.checkBoxFilterPoints.UseVisualStyleBackColor = true;
			this.checkBoxFilterPoints.CheckedChanged += new System.EventHandler(this.checkBoxFilterPoints_CheckedChanged);
			// 
			// checkBoxExportPoints
			// 
			this.checkBoxExportPoints.AutoSize = true;
			this.checkBoxExportPoints.Location = new System.Drawing.Point(22, 458);
			this.checkBoxExportPoints.Name = "checkBoxExportPoints";
			this.checkBoxExportPoints.Size = new System.Drawing.Size(111, 21);
			this.checkBoxExportPoints.TabIndex = 46;
			this.checkBoxExportPoints.Text = "export points";
			this.myToolTip.SetToolTip(this.checkBoxExportPoints, "include all points into final export file");
			this.checkBoxExportPoints.UseVisualStyleBackColor = true;
			this.checkBoxExportPoints.CheckedChanged += new System.EventHandler(this.checkBoxExportPoints_CheckedChanged);
			// 
			// CMainForm
			// 
			this.ClientSize = new System.Drawing.Size(836, 491);
			this.Controls.Add(this.checkBoxExportPoints);
			this.Controls.Add(this.checkBoxFilterPoints);
			this.Controls.Add(this.checkBoxReducedReftrees);
			this.Controls.Add(this.checkBoxExportCheckTrees);
			this.Controls.Add(this.checkBoxUseCheckTree);
			this.Controls.Add(this.checkBoxAssignRefTreesRandom);
			this.Controls.Add(this.checkBoxExportRefTrees);
			this.Controls.Add(this.checkBoxExportInvalidTrees);
			this.Controls.Add(this.checkBoxExportTreeStructures);
			this.Controls.Add(this.trackBarAvgTreeHeight);
			this.Controls.Add(this.textAvgTreeHeight);
			this.Controls.Add(this.labelAvgTreeHeight);
			this.Controls.Add(this.trackBarTreeExtentMultiply);
			this.Controls.Add(this.textTreeExtentMultiply);
			this.Controls.Add(this.labelTreeExtentMultiply);
			this.Controls.Add(this.trackBarTreeExtent);
			this.Controls.Add(this.textTreeExtent);
			this.Controls.Add(this.labelTreeExtent);
			this.Controls.Add(this.trackBarGroundArrayStep);
			this.Controls.Add(this.textGroundArrayStep);
			this.Controls.Add(this.labelGroundArrayStep);
			this.Controls.Add(this.textCheckTreePath);
			this.Controls.Add(this.btnSelectCheckTree);
			this.Controls.Add(this.trackBarPartition);
			this.Controls.Add(this.textPartition);
			this.Controls.Add(this.labelPartition);
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
			((System.ComponentModel.ISupportInitialize)(this.trackBarAvgTreeHeight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void textOutputFolder_TextChanged(object sender, EventArgs e)
		{
			CDebug.Warning("txt change " + textOutputFolder.Text);
			CParameterSetter.SetParameter(
				ESettings.outputFolderPath, textOutputFolder.Text);
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
				ESettings.reftreeFolderPath, textReftreeFolder.Text);
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
				ESettings.forrestFilePath, textForrestFilePath.Text);
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
				ESettings.checkTreeFilePath, textCheckTreePath.Text);
		}

		private void trackBarPartition_Scroll(object sender, EventArgs e)
		{
			if (blockRecursion) { return; }
			trackValue = trackBarPartition.Value;
			if (trackValue % smallChangeValue != 0)
			{
				trackValue = trackValue / smallChangeValue * smallChangeValue;

				blockRecursion = true;
				trackBarPartition.Value = trackValue;
				blockRecursion = false;
			}

			textPartition.Text = trackBarPartition.Value + " m";
			CParameterSetter.SetParameter(ESettings.partitionStep, trackBarPartition.Value);
		}

		//snap to multiply of 5 implementation
		private bool blockRecursion;
		private int smallChangeValue = 5;
		private int trackValue;
		private void trackBarGroundArrayStep_Scroll(object sender, EventArgs e)
		{
			if (blockRecursion) { return; }
			trackValue = trackBarGroundArrayStep.Value;
			if (trackValue % smallChangeValue != 0)
			{
				trackValue = trackValue / smallChangeValue * smallChangeValue;

				blockRecursion = true;
				trackBarGroundArrayStep.Value = trackValue;
				blockRecursion = false;
			}

			float value = trackBarGroundArrayStep.Value / 10f;

			textGroundArrayStep.Text = value.ToString("0.0") + " m";
			CParameterSetter.SetParameter(ESettings.groundArrayStep, value);
		}

		private void trackBarTreeExtent_Scroll(object sender, EventArgs e)
		{
			float value = trackBarTreeExtent.Value / 10f;
			textTreeExtent.Text = value.ToString("0.0") + " m";
			CParameterSetter.SetParameter(ESettings.treeExtent, value);
		}

		private void trackBarTreeExtentMultiply_Scroll(object sender, EventArgs e)
		{
			float value = trackBarTreeExtentMultiply.Value / 10f;
			textTreeExtentMultiply.Text = value.ToString("0.0");
			CParameterSetter.SetParameter(ESettings.treeExtentMultiply, value);
		}

		private void trackBarAvgTreeHeight_Scroll(object sender, EventArgs e)
		{
			textAvgTreeHeight.Text = trackBarAvgTreeHeight.Value + " m";
			CParameterSetter.SetParameter(
				ESettings.avgTreeHeigh, trackBarAvgTreeHeight.Value);
		}

		private void checkBoxExportTreeStructures_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportTreeStructures,
				checkBoxExportTreeStructures.Checked);
		}

		private void checkBoxExportInvalidTrees_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportInvalidTrees,
				checkBoxExportInvalidTrees.Checked);
		}

		private void checkBoxExportRefTrees_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportRefTrees,
				checkBoxExportRefTrees.Checked);
		}

		private void checkBoxExportCheckTrees_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportCheckTrees,
				checkBoxExportCheckTrees.Checked);
		}

		private void checkBoxUseCheckTree_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.useCheckTreeFile,
				checkBoxUseCheckTree.Checked);
		}

		private void checkBoxAssignRefTreesRandom_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.assignRefTreesRandom,
				checkBoxAssignRefTreesRandom.Checked);
		}

		private void checkBoxReducedReftrees_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.useReducedReftreeModels,
				checkBoxReducedReftrees.Checked);
		}

		private void checkBoxFilterPoints_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.filterPoints,
				checkBoxFilterPoints.Checked);
		}

		private void checkBoxExportPoints_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportPoints, checkBoxExportPoints.Checked);
		}
	}
}
