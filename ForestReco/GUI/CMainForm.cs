﻿using ForestReco.GUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	public class CMainForm : Form
	{
		#region properties
		public TextBox textForestFilePath;
		private Button btnReftreesFolder;
		public TextBox textReftreeFolder;
		private Button btnStart;
		public TextBox textOutputFolder;
		private Button btnOutputFolder;
		public ProgressBar progressBar;
		public TextBox textProgress;
		private Button btnAbort;
		private Button btnToggleConsole;
		private Label labelPartition;
		private TextBox textPartition;
		private TrackBar trackBarPartition;
		public TextBox textCheckTreePath;
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
		private Button btnOpenResult;
		private CheckBox checkBoxAutoTreeHeight;
		private TextBox textBoxEstimatedSize;
		private Label labelEstimatedTotalSize;
		private Label labelEstimatedPartitionSize;
		private TextBox textBoxPartitionSize;
		private CheckBox checkBoxExportTreeBoxes;
		private CheckBox checkBoxColorTrees;
		private CheckBox checkBoxExport3d;
		private Button btnSequence;
		public TextBox textAnalyticsFile;
		public Button btnAnalytics;
		private CheckBox checkBoxExportBitmap;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private Button button1;
		public TrackBar trackBarRangeXmin;
		public TextBox textRangeX;
		private Label label1;
		public TrackBar trackBarRangeXmax;
		public TrackBar trackBarRangeYmax;
		public TrackBar trackBarRangeYmin;
		public TextBox textRangeY;
		private Label label2;
		private Button btnSellectForest;

		private TextBox textLasTools;
		private Button btnLasTools;
		#endregion


		private TextBox textTmpFolder;
		private Button btnTmpFolder;

		private CUiRangeController rangeController;
		private CUiPathSelection pathSelection;

		public CMainForm()
		{

			rangeController = new CUiRangeController(this);
			pathSelection = new CUiPathSelection(this);

			InitializeComponent();
			InitializeValues();

			CProjectData.backgroundWorker = backgroundWorker1;
			backgroundWorker1.WorkerSupportsCancellation = true;
			backgroundWorker1.WorkerReportsProgress = true;

		}

		private void InitializeValues()
		{
			CParameterSetter.Init();
			textForestFilePath.Text = CParameterSetter.GetStringSettings(ESettings.forestFilePath);
			textReftreeFolder.Text = CParameterSetter.GetStringSettings(ESettings.reftreeFolderPath);
			textOutputFolder.Text = CParameterSetter.GetStringSettings(ESettings.outputFolderPath);
			textCheckTreePath.Text = CParameterSetter.GetStringSettings(ESettings.checkTreeFilePath);
			textAnalyticsFile.Text = CParameterSetter.GetStringSettings(ESettings.analyticsFilePath);
			
			textLasTools.Text = CParameterSetter.GetStringSettings(ESettings.lasToolsFolderPath);
			textTmpFolder.Text = CParameterSetter.GetStringSettings(ESettings.tmpFilesFolderPath);


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
			checkBoxExport3d.Checked =
				CParameterSetter.GetBoolSettings(ESettings.export3d);
			checkBoxExort3d_CheckedChanged(this, EventArgs.Empty); //force refresh

			checkBoxExportBitmap.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportBitmap);
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
			checkBoxUseCheckTree_CheckedChanged(this, EventArgs.Empty); //force refresh

			checkBoxExportCheckTrees.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportCheckTrees);
			checkBoxExportTreeBoxes.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportTreeBoxes);
			checkBoxColorTrees.Checked =
			CParameterSetter.GetBoolSettings(ESettings.colorTrees);
			checkBoxReducedReftrees.Checked =
				CParameterSetter.GetBoolSettings(ESettings.useReducedReftreeModels);
			checkBoxFilterPoints.Checked =
				CParameterSetter.GetBoolSettings(ESettings.filterPoints);
			checkBoxExportPoints.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportPoints);
			checkBoxAutoTreeHeight.Checked =
				CParameterSetter.GetBoolSettings(ESettings.autoAverageTreeHeight);

			SetStartBtnEnabled(true);

			#region tooltip
			CTooltipManager.AssignTooltip(myToolTip, btnSellectForest, ESettings.forestFilePath);
			CTooltipManager.AssignTooltip(myToolTip, btnSequence, ETooltip.sequenceFile);
			CTooltipManager.AssignTooltip(myToolTip, btnReftreesFolder, ESettings.reftreeFolderPath);
			CTooltipManager.AssignTooltip(myToolTip, btnOutputFolder, ESettings.outputFolderPath);
			CTooltipManager.AssignTooltip(myToolTip, btnAnalytics, ESettings.analyticsFilePath);
			CTooltipManager.AssignTooltip(myToolTip, btnSelectCheckTree, ESettings.checkTreeFilePath);

			CTooltipManager.AssignTooltip(myToolTip, btnToggleConsole, ETooltip.toggleConsole);
			CTooltipManager.AssignTooltip(myToolTip, btnOpenResult, ETooltip.openResult);


			CTooltipManager.AssignTooltip(myToolTip, checkBoxExport3d, ESettings.export3d);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxExportBitmap, ESettings.exportBitmap);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxAssignRefTreesRandom, ESettings.assignRefTreesRandom);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxExportInvalidTrees, ESettings.exportInvalidTrees);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxExportRefTrees, ESettings.exportRefTrees);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxExportTreeStructures, ESettings.exportTreeStructures);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxColorTrees, ESettings.colorTrees);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxReducedReftrees, ESettings.useReducedReftreeModels);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxExportTreeBoxes, ESettings.exportTreeBoxes);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxUseCheckTree, ESettings.useCheckTreeFile);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxExportCheckTrees, ESettings.exportCheckTrees);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxFilterPoints, ESettings.filterPoints);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxExportPoints, ESettings.exportPoints);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxAutoTreeHeight, ESettings.autoAverageTreeHeight);

			CTooltipManager.AssignTooltip(myToolTip, labelPartition, ESettings.partitionStep);
			CTooltipManager.AssignTooltip(myToolTip, labelAvgTreeHeight, ESettings.avgTreeHeigh);
			CTooltipManager.AssignTooltip(myToolTip, labelGroundArrayStep, ESettings.groundArrayStep);
			CTooltipManager.AssignTooltip(myToolTip, labelTreeExtent, ESettings.treeExtent);
			CTooltipManager.AssignTooltip(myToolTip, labelTreeExtentMultiply, ESettings.treeExtentMultiply);

			CTooltipManager.AssignTooltip(myToolTip, labelEstimatedTotalSize, ETooltip.EstimatedTotalSize);
			CTooltipManager.AssignTooltip(myToolTip, labelEstimatedPartitionSize, ETooltip.EstimatedPartitionSize);
			CTooltipManager.AssignTooltip(myToolTip, trackBarAvgTreeHeight, ETooltip.avgTreeHeighSlider);
			#endregion
		}

		private void MainForm_Load(object sender, EventArgs e)
		{

		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			if(!CUiInputCheck.CheckProblems())
			{
				return;
			}
			CProgramStarter.PrepareSequence();
			SetStartBtnEnabled(false);

			if(backgroundWorker1.IsBusy != true)
			{
				// Start the asynchronous operation.
				backgroundWorker1.RunWorkerAsync();
			}

			//CProgramStarter.Start();
		}

		private void btnAbort_Click(object sender, EventArgs e)
		{
			//CProjectData.backgroundWorker.CancellationPending();
			if(backgroundWorker1.WorkerSupportsCancellation)
			{
				// Cancel the asynchronous operation.
				backgroundWorker1.CancelAsync();
			}
		}

		private void btnToggleConsole_Click(object sender, EventArgs e)
		{
			CParameterSetter.ToggleConsoleVisibility();
		}

		#region INIT
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CMainForm));
			this.btnSellectForest = new System.Windows.Forms.Button();
			this.textForestFilePath = new System.Windows.Forms.TextBox();
			this.btnReftreesFolder = new System.Windows.Forms.Button();
			this.textReftreeFolder = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.textOutputFolder = new System.Windows.Forms.TextBox();
			this.btnOutputFolder = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.textProgress = new System.Windows.Forms.TextBox();
			this.btnAbort = new System.Windows.Forms.Button();
			this.btnToggleConsole = new System.Windows.Forms.Button();
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
			this.checkBoxAutoTreeHeight = new System.Windows.Forms.CheckBox();
			this.checkBoxExportTreeBoxes = new System.Windows.Forms.CheckBox();
			this.checkBoxExport3d = new System.Windows.Forms.CheckBox();
			this.checkBoxExportBitmap = new System.Windows.Forms.CheckBox();
			this.btnOpenResult = new System.Windows.Forms.Button();
			this.textBoxEstimatedSize = new System.Windows.Forms.TextBox();
			this.labelEstimatedTotalSize = new System.Windows.Forms.Label();
			this.labelEstimatedPartitionSize = new System.Windows.Forms.Label();
			this.textBoxPartitionSize = new System.Windows.Forms.TextBox();
			this.checkBoxColorTrees = new System.Windows.Forms.CheckBox();
			this.btnSequence = new System.Windows.Forms.Button();
			this.textAnalyticsFile = new System.Windows.Forms.TextBox();
			this.btnAnalytics = new System.Windows.Forms.Button();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.button1 = new System.Windows.Forms.Button();
			this.trackBarRangeXmin = new System.Windows.Forms.TrackBar();
			this.textRangeX = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.trackBarRangeXmax = new System.Windows.Forms.TrackBar();
			this.trackBarRangeYmax = new System.Windows.Forms.TrackBar();
			this.trackBarRangeYmin = new System.Windows.Forms.TrackBar();
			this.textRangeY = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textLasTools = new System.Windows.Forms.TextBox();
			this.btnLasTools = new System.Windows.Forms.Button();
			this.textTmpFolder = new System.Windows.Forms.TextBox();
			this.btnTmpFolder = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.trackBarPartition)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarGroundArrayStep)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtent)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtentMultiply)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAvgTreeHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeXmin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeXmax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeYmax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeYmin)).BeginInit();
			this.SuspendLayout();
			// 
			// btnSellectForest
			// 
			this.btnSellectForest.Location = new System.Drawing.Point(8, 10);
			this.btnSellectForest.Name = "btnSellectForest";
			this.btnSellectForest.Size = new System.Drawing.Size(121, 31);
			this.btnSellectForest.TabIndex = 0;
			this.btnSellectForest.Text = "forest file";
			this.btnSellectForest.UseVisualStyleBackColor = true;
			this.btnSellectForest.Click += new System.EventHandler(this.btnSellectForest_Click);
			// 
			// textForestFilePath
			// 
			this.textForestFilePath.Location = new System.Drawing.Point(143, 14);
			this.textForestFilePath.Name = "textForestFilePath";
			this.textForestFilePath.Size = new System.Drawing.Size(616, 22);
			this.textForestFilePath.TabIndex = 1;
			this.textForestFilePath.TextChanged += new System.EventHandler(this.textForestFilePath_TextChanged);
			// 
			// btnReftreesFolder
			// 
			this.btnReftreesFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnReftreesFolder.Location = new System.Drawing.Point(8, 46);
			this.btnReftreesFolder.Name = "btnReftreesFolder";
			this.btnReftreesFolder.Size = new System.Drawing.Size(121, 31);
			this.btnReftreesFolder.TabIndex = 2;
			this.btnReftreesFolder.Text = "reftrees folder";
			this.btnReftreesFolder.UseVisualStyleBackColor = true;
			this.btnReftreesFolder.Click += new System.EventHandler(this.btnSellectReftreeFodlers_Click);
			// 
			// textReftreeFolder
			// 
			this.textReftreeFolder.Location = new System.Drawing.Point(143, 50);
			this.textReftreeFolder.Name = "textReftreeFolder";
			this.textReftreeFolder.Size = new System.Drawing.Size(723, 22);
			this.textReftreeFolder.TabIndex = 4;
			this.textReftreeFolder.TextChanged += new System.EventHandler(this.textReftreeFolder_TextChanged);
			// 
			// btnStart
			// 
			this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(237)))), ((int)(((byte)(124)))));
			this.btnStart.Location = new System.Drawing.Point(440, 275);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(332, 48);
			this.btnStart.TabIndex = 5;
			this.btnStart.Text = "START";
			this.btnStart.UseVisualStyleBackColor = false;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// textOutputFolder
			// 
			this.textOutputFolder.Location = new System.Drawing.Point(143, 86);
			this.textOutputFolder.Name = "textOutputFolder";
			this.textOutputFolder.Size = new System.Drawing.Size(723, 22);
			this.textOutputFolder.TabIndex = 7;
			this.textOutputFolder.TextChanged += new System.EventHandler(this.textOutputFolder_TextChanged);
			// 
			// btnOutputFolder
			// 
			this.btnOutputFolder.Location = new System.Drawing.Point(8, 82);
			this.btnOutputFolder.Name = "btnOutputFolder";
			this.btnOutputFolder.Size = new System.Drawing.Size(121, 31);
			this.btnOutputFolder.TabIndex = 6;
			this.btnOutputFolder.Text = "output folder";
			this.btnOutputFolder.UseVisualStyleBackColor = true;
			this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(439, 455);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(427, 23);
			this.progressBar.TabIndex = 9;
			// 
			// textProgress
			// 
			this.textProgress.Location = new System.Drawing.Point(440, 331);
			this.textProgress.Multiline = true;
			this.textProgress.Name = "textProgress";
			this.textProgress.ReadOnly = true;
			this.textProgress.Size = new System.Drawing.Size(427, 118);
			this.textProgress.TabIndex = 10;
			// 
			// btnAbort
			// 
			this.btnAbort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(124)))), ((int)(((byte)(112)))));
			this.btnAbort.Location = new System.Drawing.Point(778, 275);
			this.btnAbort.Name = "btnAbort";
			this.btnAbort.Size = new System.Drawing.Size(89, 48);
			this.btnAbort.TabIndex = 11;
			this.btnAbort.Text = "ABORT";
			this.btnAbort.UseVisualStyleBackColor = false;
			this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
			// 
			// btnToggleConsole
			// 
			this.btnToggleConsole.Location = new System.Drawing.Point(632, 484);
			this.btnToggleConsole.Name = "btnToggleConsole";
			this.btnToggleConsole.Size = new System.Drawing.Size(109, 32);
			this.btnToggleConsole.TabIndex = 12;
			this.btnToggleConsole.Text = "toggle console";
			this.btnToggleConsole.UseVisualStyleBackColor = true;
			this.btnToggleConsole.Click += new System.EventHandler(this.btnToggleConsole_Click);
			// 
			// labelPartition
			// 
			this.labelPartition.AutoSize = true;
			this.labelPartition.Location = new System.Drawing.Point(545, 205);
			this.labelPartition.Name = "labelPartition";
			this.labelPartition.Size = new System.Drawing.Size(90, 17);
			this.labelPartition.TabIndex = 14;
			this.labelPartition.Text = "partition step";
			// 
			// textPartition
			// 
			this.textPartition.Location = new System.Drawing.Point(636, 203);
			this.textPartition.Name = "textPartition";
			this.textPartition.ReadOnly = true;
			this.textPartition.Size = new System.Drawing.Size(40, 22);
			this.textPartition.TabIndex = 16;
			this.textPartition.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// trackBarPartition
			// 
			this.trackBarPartition.AutoSize = false;
			this.trackBarPartition.LargeChange = 10;
			this.trackBarPartition.Location = new System.Drawing.Point(538, 228);
			this.trackBarPartition.Maximum = 200;
			this.trackBarPartition.Minimum = 10;
			this.trackBarPartition.Name = "trackBarPartition";
			this.trackBarPartition.Size = new System.Drawing.Size(140, 30);
			this.trackBarPartition.SmallChange = 5;
			this.trackBarPartition.TabIndex = 19;
			this.trackBarPartition.TickFrequency = 5;
			this.trackBarPartition.Value = 30;
			this.trackBarPartition.Scroll += new System.EventHandler(this.trackBarPartition_Scroll);
			// 
			// textCheckTreePath
			// 
			this.textCheckTreePath.Location = new System.Drawing.Point(143, 161);
			this.textCheckTreePath.Name = "textCheckTreePath";
			this.textCheckTreePath.Size = new System.Drawing.Size(723, 22);
			this.textCheckTreePath.TabIndex = 21;
			this.textCheckTreePath.TextChanged += new System.EventHandler(this.textCheckTreePath_TextChanged);
			// 
			// btnSelectCheckTree
			// 
			this.btnSelectCheckTree.Location = new System.Drawing.Point(8, 157);
			this.btnSelectCheckTree.Name = "btnSelectCheckTree";
			this.btnSelectCheckTree.Size = new System.Drawing.Size(121, 31);
			this.btnSelectCheckTree.TabIndex = 20;
			this.btnSelectCheckTree.Text = "checktree file";
			this.btnSelectCheckTree.UseVisualStyleBackColor = true;
			this.btnSelectCheckTree.Click += new System.EventHandler(this.btnSelectCheckTree_Click);
			// 
			// trackBarGroundArrayStep
			// 
			this.trackBarGroundArrayStep.AutoSize = false;
			this.trackBarGroundArrayStep.LargeChange = 10;
			this.trackBarGroundArrayStep.Location = new System.Drawing.Point(695, 228);
			this.trackBarGroundArrayStep.Maximum = 30;
			this.trackBarGroundArrayStep.Minimum = 5;
			this.trackBarGroundArrayStep.Name = "trackBarGroundArrayStep";
			this.trackBarGroundArrayStep.Size = new System.Drawing.Size(171, 30);
			this.trackBarGroundArrayStep.SmallChange = 5;
			this.trackBarGroundArrayStep.TabIndex = 25;
			this.trackBarGroundArrayStep.TickFrequency = 5;
			this.trackBarGroundArrayStep.Value = 10;
			this.trackBarGroundArrayStep.Scroll += new System.EventHandler(this.trackBarGroundArrayStep_Scroll);
			// 
			// textGroundArrayStep
			// 
			this.textGroundArrayStep.Location = new System.Drawing.Point(826, 203);
			this.textGroundArrayStep.Name = "textGroundArrayStep";
			this.textGroundArrayStep.ReadOnly = true;
			this.textGroundArrayStep.Size = new System.Drawing.Size(40, 22);
			this.textGroundArrayStep.TabIndex = 24;
			this.textGroundArrayStep.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelGroundArrayStep
			// 
			this.labelGroundArrayStep.AutoSize = true;
			this.labelGroundArrayStep.Location = new System.Drawing.Point(702, 205);
			this.labelGroundArrayStep.Name = "labelGroundArrayStep";
			this.labelGroundArrayStep.Size = new System.Drawing.Size(121, 17);
			this.labelGroundArrayStep.TabIndex = 23;
			this.labelGroundArrayStep.Text = "ground array step";
			// 
			// trackBarTreeExtent
			// 
			this.trackBarTreeExtent.AutoSize = false;
			this.trackBarTreeExtent.LargeChange = 10;
			this.trackBarTreeExtent.Location = new System.Drawing.Point(199, 228);
			this.trackBarTreeExtent.Maximum = 30;
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
			this.textTreeExtent.Location = new System.Drawing.Point(318, 203);
			this.textTreeExtent.Name = "textTreeExtent";
			this.textTreeExtent.ReadOnly = true;
			this.textTreeExtent.Size = new System.Drawing.Size(40, 22);
			this.textTreeExtent.TabIndex = 28;
			this.textTreeExtent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelTreeExtent
			// 
			this.labelTreeExtent.AutoSize = true;
			this.labelTreeExtent.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.labelTreeExtent.Location = new System.Drawing.Point(206, 204);
			this.labelTreeExtent.Name = "labelTreeExtent";
			this.labelTreeExtent.Size = new System.Drawing.Size(86, 17);
			this.labelTreeExtent.TabIndex = 27;
			this.labelTreeExtent.Text = "tree extent";
			// 
			// trackBarTreeExtentMultiply
			// 
			this.trackBarTreeExtentMultiply.AutoSize = false;
			this.trackBarTreeExtentMultiply.LargeChange = 10;
			this.trackBarTreeExtentMultiply.Location = new System.Drawing.Point(363, 228);
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
			this.textTreeExtentMultiply.Location = new System.Drawing.Point(489, 203);
			this.textTreeExtentMultiply.Name = "textTreeExtentMultiply";
			this.textTreeExtentMultiply.ReadOnly = true;
			this.textTreeExtentMultiply.Size = new System.Drawing.Size(40, 22);
			this.textTreeExtentMultiply.TabIndex = 32;
			this.textTreeExtentMultiply.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelTreeExtentMultiply
			// 
			this.labelTreeExtentMultiply.AutoSize = true;
			this.labelTreeExtentMultiply.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.labelTreeExtentMultiply.Location = new System.Drawing.Point(370, 204);
			this.labelTreeExtentMultiply.Name = "labelTreeExtentMultiply";
			this.labelTreeExtentMultiply.Size = new System.Drawing.Size(112, 17);
			this.labelTreeExtentMultiply.TabIndex = 31;
			this.labelTreeExtentMultiply.Text = "extent multiply";
			// 
			// trackBarAvgTreeHeight
			// 
			this.trackBarAvgTreeHeight.AutoSize = false;
			this.trackBarAvgTreeHeight.BackColor = System.Drawing.SystemColors.Control;
			this.trackBarAvgTreeHeight.LargeChange = 10;
			this.trackBarAvgTreeHeight.Location = new System.Drawing.Point(9, 228);
			this.trackBarAvgTreeHeight.Maximum = 50;
			this.trackBarAvgTreeHeight.Minimum = 5;
			this.trackBarAvgTreeHeight.Name = "trackBarAvgTreeHeight";
			this.trackBarAvgTreeHeight.Size = new System.Drawing.Size(192, 30);
			this.trackBarAvgTreeHeight.TabIndex = 37;
			this.trackBarAvgTreeHeight.TickFrequency = 5;
			this.trackBarAvgTreeHeight.Value = 15;
			this.trackBarAvgTreeHeight.Scroll += new System.EventHandler(this.trackBarAvgTreeHeight_Scroll);
			// 
			// textAvgTreeHeight
			// 
			this.textAvgTreeHeight.Location = new System.Drawing.Point(161, 203);
			this.textAvgTreeHeight.Name = "textAvgTreeHeight";
			this.textAvgTreeHeight.ReadOnly = true;
			this.textAvgTreeHeight.Size = new System.Drawing.Size(40, 22);
			this.textAvgTreeHeight.TabIndex = 36;
			this.textAvgTreeHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelAvgTreeHeight
			// 
			this.labelAvgTreeHeight.AutoSize = true;
			this.labelAvgTreeHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.labelAvgTreeHeight.Location = new System.Drawing.Point(12, 204);
			this.labelAvgTreeHeight.Name = "labelAvgTreeHeight";
			this.labelAvgTreeHeight.Size = new System.Drawing.Size(151, 17);
			this.labelAvgTreeHeight.TabIndex = 35;
			this.labelAvgTreeHeight.Text = "average tree height";
			// 
			// checkBoxExportTreeStructures
			// 
			this.checkBoxExportTreeStructures.AutoSize = true;
			this.checkBoxExportTreeStructures.Location = new System.Drawing.Point(256, 302);
			this.checkBoxExportTreeStructures.Name = "checkBoxExportTreeStructures";
			this.checkBoxExportTreeStructures.Size = new System.Drawing.Size(122, 21);
			this.checkBoxExportTreeStructures.TabIndex = 38;
			this.checkBoxExportTreeStructures.Text = "tree structures";
			this.myToolTip.SetToolTip(this.checkBoxExportTreeStructures, "hh");
			this.checkBoxExportTreeStructures.UseVisualStyleBackColor = true;
			this.checkBoxExportTreeStructures.CheckedChanged += new System.EventHandler(this.checkBoxExportTreeStructures_CheckedChanged);
			// 
			// myToolTip
			// 
			this.myToolTip.AutoPopDelay = 32767;
			this.myToolTip.InitialDelay = 500;
			this.myToolTip.ReshowDelay = 100;
			this.myToolTip.ShowAlways = true;
			this.myToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			// 
			// checkBoxExportInvalidTrees
			// 
			this.checkBoxExportInvalidTrees.AutoSize = true;
			this.checkBoxExportInvalidTrees.Location = new System.Drawing.Point(256, 352);
			this.checkBoxExportInvalidTrees.Name = "checkBoxExportInvalidTrees";
			this.checkBoxExportInvalidTrees.Size = new System.Drawing.Size(106, 21);
			this.checkBoxExportInvalidTrees.TabIndex = 39;
			this.checkBoxExportInvalidTrees.Text = "invalid trees";
			this.myToolTip.SetToolTip(this.checkBoxExportInvalidTrees, "hh");
			this.checkBoxExportInvalidTrees.UseVisualStyleBackColor = true;
			this.checkBoxExportInvalidTrees.CheckedChanged += new System.EventHandler(this.checkBoxExportInvalidTrees_CheckedChanged);
			// 
			// checkBoxExportRefTrees
			// 
			this.checkBoxExportRefTrees.AutoSize = true;
			this.checkBoxExportRefTrees.Location = new System.Drawing.Point(256, 377);
			this.checkBoxExportRefTrees.Name = "checkBoxExportRefTrees";
			this.checkBoxExportRefTrees.Size = new System.Drawing.Size(79, 21);
			this.checkBoxExportRefTrees.TabIndex = 40;
			this.checkBoxExportRefTrees.Text = "reftrees";
			this.myToolTip.SetToolTip(this.checkBoxExportRefTrees, "hh");
			this.checkBoxExportRefTrees.UseVisualStyleBackColor = true;
			this.checkBoxExportRefTrees.CheckedChanged += new System.EventHandler(this.checkBoxExportRefTrees_CheckedChanged);
			// 
			// checkBoxAssignRefTreesRandom
			// 
			this.checkBoxAssignRefTreesRandom.AutoSize = true;
			this.checkBoxAssignRefTreesRandom.Location = new System.Drawing.Point(12, 343);
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
			this.checkBoxUseCheckTree.Location = new System.Drawing.Point(12, 402);
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
			this.checkBoxExportCheckTrees.Location = new System.Drawing.Point(256, 427);
			this.checkBoxExportCheckTrees.Name = "checkBoxExportCheckTrees";
			this.checkBoxExportCheckTrees.Size = new System.Drawing.Size(99, 21);
			this.checkBoxExportCheckTrees.TabIndex = 43;
			this.checkBoxExportCheckTrees.Text = "checktrees";
			this.myToolTip.SetToolTip(this.checkBoxExportCheckTrees, "hh");
			this.checkBoxExportCheckTrees.UseVisualStyleBackColor = true;
			this.checkBoxExportCheckTrees.CheckedChanged += new System.EventHandler(this.checkBoxExportCheckTrees_CheckedChanged);
			// 
			// checkBoxReducedReftrees
			// 
			this.checkBoxReducedReftrees.AutoSize = true;
			this.checkBoxReducedReftrees.Location = new System.Drawing.Point(12, 370);
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
			this.checkBoxFilterPoints.Location = new System.Drawing.Point(12, 302);
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
			this.checkBoxExportPoints.Location = new System.Drawing.Point(256, 402);
			this.checkBoxExportPoints.Name = "checkBoxExportPoints";
			this.checkBoxExportPoints.Size = new System.Drawing.Size(68, 21);
			this.checkBoxExportPoints.TabIndex = 46;
			this.checkBoxExportPoints.Text = "points";
			this.myToolTip.SetToolTip(this.checkBoxExportPoints, "include all points into final export file");
			this.checkBoxExportPoints.UseVisualStyleBackColor = true;
			this.checkBoxExportPoints.CheckedChanged += new System.EventHandler(this.checkBoxExportPoints_CheckedChanged);
			// 
			// checkBoxAutoTreeHeight
			// 
			this.checkBoxAutoTreeHeight.AutoSize = true;
			this.checkBoxAutoTreeHeight.Location = new System.Drawing.Point(12, 275);
			this.checkBoxAutoTreeHeight.Name = "checkBoxAutoTreeHeight";
			this.checkBoxAutoTreeHeight.Size = new System.Drawing.Size(163, 21);
			this.checkBoxAutoTreeHeight.TabIndex = 48;
			this.checkBoxAutoTreeHeight.Text = "automatic tree height";
			this.myToolTip.SetToolTip(this.checkBoxAutoTreeHeight, "include all points into final export file");
			this.checkBoxAutoTreeHeight.UseVisualStyleBackColor = true;
			this.checkBoxAutoTreeHeight.CheckedChanged += new System.EventHandler(this.checkBoxAutoTreeHeight_CheckedChanged);
			// 
			// checkBoxExportTreeBoxes
			// 
			this.checkBoxExportTreeBoxes.AutoSize = true;
			this.checkBoxExportTreeBoxes.Location = new System.Drawing.Point(256, 327);
			this.checkBoxExportTreeBoxes.Name = "checkBoxExportTreeBoxes";
			this.checkBoxExportTreeBoxes.Size = new System.Drawing.Size(96, 21);
			this.checkBoxExportTreeBoxes.TabIndex = 53;
			this.checkBoxExportTreeBoxes.Text = "tree boxes";
			this.myToolTip.SetToolTip(this.checkBoxExportTreeBoxes, "hh");
			this.checkBoxExportTreeBoxes.UseVisualStyleBackColor = true;
			this.checkBoxExportTreeBoxes.CheckedChanged += new System.EventHandler(this.checkBoxExportTreeBoxes_CheckedChanged);
			// 
			// checkBoxExport3d
			// 
			this.checkBoxExport3d.AutoSize = true;
			this.checkBoxExport3d.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.checkBoxExport3d.Location = new System.Drawing.Point(228, 278);
			this.checkBoxExport3d.Name = "checkBoxExport3d";
			this.checkBoxExport3d.Size = new System.Drawing.Size(118, 21);
			this.checkBoxExport3d.TabIndex = 55;
			this.checkBoxExport3d.Text = "EXPORT 3D";
			this.myToolTip.SetToolTip(this.checkBoxExport3d, "hh");
			this.checkBoxExport3d.UseVisualStyleBackColor = true;
			this.checkBoxExport3d.CheckedChanged += new System.EventHandler(this.checkBoxExort3d_CheckedChanged);
			// 
			// checkBoxExportBitmap
			// 
			this.checkBoxExportBitmap.AutoSize = true;
			this.checkBoxExportBitmap.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.checkBoxExportBitmap.Location = new System.Drawing.Point(228, 456);
			this.checkBoxExportBitmap.Name = "checkBoxExportBitmap";
			this.checkBoxExportBitmap.Size = new System.Drawing.Size(154, 21);
			this.checkBoxExportBitmap.TabIndex = 59;
			this.checkBoxExportBitmap.Text = "EXPORT BITMAP";
			this.myToolTip.SetToolTip(this.checkBoxExportBitmap, "hh");
			this.checkBoxExportBitmap.UseVisualStyleBackColor = true;
			this.checkBoxExportBitmap.CheckedChanged += new System.EventHandler(this.checkBoxExportBitmap_CheckedChanged);
			// 
			// btnOpenResult
			// 
			this.btnOpenResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(161)))), ((int)(((byte)(212)))));
			this.btnOpenResult.Location = new System.Drawing.Point(758, 484);
			this.btnOpenResult.Name = "btnOpenResult";
			this.btnOpenResult.Size = new System.Drawing.Size(109, 32);
			this.btnOpenResult.TabIndex = 47;
			this.btnOpenResult.Text = "open result";
			this.btnOpenResult.UseVisualStyleBackColor = false;
			this.btnOpenResult.Click += new System.EventHandler(this.btnOpenResult_Click);
			// 
			// textBoxEstimatedSize
			// 
			this.textBoxEstimatedSize.Location = new System.Drawing.Point(113, 465);
			this.textBoxEstimatedSize.Name = "textBoxEstimatedSize";
			this.textBoxEstimatedSize.ReadOnly = true;
			this.textBoxEstimatedSize.Size = new System.Drawing.Size(75, 22);
			this.textBoxEstimatedSize.TabIndex = 49;
			// 
			// labelEstimatedTotalSize
			// 
			this.labelEstimatedTotalSize.AutoSize = true;
			this.labelEstimatedTotalSize.Location = new System.Drawing.Point(9, 465);
			this.labelEstimatedTotalSize.Name = "labelEstimatedTotalSize";
			this.labelEstimatedTotalSize.Size = new System.Drawing.Size(98, 17);
			this.labelEstimatedTotalSize.TabIndex = 50;
			this.labelEstimatedTotalSize.Text = "estimated size";
			// 
			// labelEstimatedPartitionSize
			// 
			this.labelEstimatedPartitionSize.AutoSize = true;
			this.labelEstimatedPartitionSize.Location = new System.Drawing.Point(9, 493);
			this.labelEstimatedPartitionSize.Name = "labelEstimatedPartitionSize";
			this.labelEstimatedPartitionSize.Size = new System.Drawing.Size(88, 17);
			this.labelEstimatedPartitionSize.TabIndex = 52;
			this.labelEstimatedPartitionSize.Text = "partition size";
			// 
			// textBoxPartitionSize
			// 
			this.textBoxPartitionSize.Location = new System.Drawing.Point(113, 493);
			this.textBoxPartitionSize.Name = "textBoxPartitionSize";
			this.textBoxPartitionSize.ReadOnly = true;
			this.textBoxPartitionSize.Size = new System.Drawing.Size(75, 22);
			this.textBoxPartitionSize.TabIndex = 51;
			// 
			// checkBoxColorTrees
			// 
			this.checkBoxColorTrees.AutoSize = true;
			this.checkBoxColorTrees.Location = new System.Drawing.Point(13, 433);
			this.checkBoxColorTrees.Name = "checkBoxColorTrees";
			this.checkBoxColorTrees.Size = new System.Drawing.Size(97, 21);
			this.checkBoxColorTrees.TabIndex = 54;
			this.checkBoxColorTrees.Text = "color trees";
			this.checkBoxColorTrees.UseVisualStyleBackColor = true;
			this.checkBoxColorTrees.CheckedChanged += new System.EventHandler(this.checkBoxColorTrees_CheckedChanged);
			// 
			// btnSequence
			// 
			this.btnSequence.Location = new System.Drawing.Point(773, 10);
			this.btnSequence.Name = "btnSequence";
			this.btnSequence.Size = new System.Drawing.Size(93, 31);
			this.btnSequence.TabIndex = 56;
			this.btnSequence.Text = "sequence";
			this.btnSequence.UseVisualStyleBackColor = true;
			this.btnSequence.Click += new System.EventHandler(this.btnSequence_Click);
			// 
			// textAnalyticsFile
			// 
			this.textAnalyticsFile.Location = new System.Drawing.Point(143, 123);
			this.textAnalyticsFile.Name = "textAnalyticsFile";
			this.textAnalyticsFile.Size = new System.Drawing.Size(723, 22);
			this.textAnalyticsFile.TabIndex = 58;
			this.textAnalyticsFile.TextChanged += new System.EventHandler(this.textAnalyticsFile_TextChanged);
			// 
			// btnAnalytics
			// 
			this.btnAnalytics.Location = new System.Drawing.Point(7, 119);
			this.btnAnalytics.Name = "btnAnalytics";
			this.btnAnalytics.Size = new System.Drawing.Size(121, 31);
			this.btnAnalytics.TabIndex = 57;
			this.btnAnalytics.Text = "analytics file";
			this.btnAnalytics.UseVisualStyleBackColor = true;
			this.btnAnalytics.Click += new System.EventHandler(this.buttonAnalytics_Click);
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
			this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(901, 10);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(117, 62);
			this.button1.TabIndex = 60;
			this.button1.Text = "test 1";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.buttonTest1_Click);
			// 
			// trackBarRangeXmin
			// 
			this.trackBarRangeXmin.AutoSize = false;
			this.trackBarRangeXmin.LargeChange = 10;
			this.trackBarRangeXmin.Location = new System.Drawing.Point(901, 123);
			this.trackBarRangeXmin.Maximum = 30;
			this.trackBarRangeXmin.Minimum = 5;
			this.trackBarRangeXmin.Name = "trackBarRangeXmin";
			this.trackBarRangeXmin.Size = new System.Drawing.Size(92, 30);
			this.trackBarRangeXmin.SmallChange = 5;
			this.trackBarRangeXmin.TabIndex = 63;
			this.trackBarRangeXmin.TickFrequency = 5;
			this.trackBarRangeXmin.Value = 10;
			this.trackBarRangeXmin.Scroll += new System.EventHandler(this.trackBarRangeXmin_Scroll);
			// 
			// textRangeX
			// 
			this.textRangeX.Location = new System.Drawing.Point(972, 98);
			this.textRangeX.Name = "textRangeX";
			this.textRangeX.ReadOnly = true;
			this.textRangeX.Size = new System.Drawing.Size(119, 22);
			this.textRangeX.TabIndex = 62;
			this.textRangeX.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(908, 100);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 17);
			this.label1.TabIndex = 61;
			this.label1.Text = "range X";
			// 
			// trackBarRangeXmax
			// 
			this.trackBarRangeXmax.AutoSize = false;
			this.trackBarRangeXmax.LargeChange = 10;
			this.trackBarRangeXmax.Location = new System.Drawing.Point(999, 123);
			this.trackBarRangeXmax.Maximum = 30;
			this.trackBarRangeXmax.Minimum = 5;
			this.trackBarRangeXmax.Name = "trackBarRangeXmax";
			this.trackBarRangeXmax.Size = new System.Drawing.Size(92, 30);
			this.trackBarRangeXmax.SmallChange = 5;
			this.trackBarRangeXmax.TabIndex = 64;
			this.trackBarRangeXmax.TickFrequency = 5;
			this.trackBarRangeXmax.Value = 10;
			this.trackBarRangeXmax.Scroll += new System.EventHandler(this.trackBarRangeXmax_Scroll);
			// 
			// trackBarRangeYmax
			// 
			this.trackBarRangeYmax.AutoSize = false;
			this.trackBarRangeYmax.LargeChange = 10;
			this.trackBarRangeYmax.Location = new System.Drawing.Point(999, 195);
			this.trackBarRangeYmax.Maximum = 30;
			this.trackBarRangeYmax.Minimum = 5;
			this.trackBarRangeYmax.Name = "trackBarRangeYmax";
			this.trackBarRangeYmax.Size = new System.Drawing.Size(92, 30);
			this.trackBarRangeYmax.SmallChange = 5;
			this.trackBarRangeYmax.TabIndex = 68;
			this.trackBarRangeYmax.TickFrequency = 5;
			this.trackBarRangeYmax.Value = 10;
			this.trackBarRangeYmax.Scroll += new System.EventHandler(this.trackBarRangeYmax_Scroll);
			// 
			// trackBarRangeYmin
			// 
			this.trackBarRangeYmin.AutoSize = false;
			this.trackBarRangeYmin.LargeChange = 10;
			this.trackBarRangeYmin.Location = new System.Drawing.Point(901, 195);
			this.trackBarRangeYmin.Maximum = 30;
			this.trackBarRangeYmin.Minimum = 5;
			this.trackBarRangeYmin.Name = "trackBarRangeYmin";
			this.trackBarRangeYmin.Size = new System.Drawing.Size(92, 30);
			this.trackBarRangeYmin.SmallChange = 5;
			this.trackBarRangeYmin.TabIndex = 67;
			this.trackBarRangeYmin.TickFrequency = 5;
			this.trackBarRangeYmin.Value = 10;
			this.trackBarRangeYmin.Scroll += new System.EventHandler(this.trackBarRangeYmin_Scroll);
			// 
			// textRangeY
			// 
			this.textRangeY.Location = new System.Drawing.Point(972, 170);
			this.textRangeY.Name = "textRangeY";
			this.textRangeY.ReadOnly = true;
			this.textRangeY.Size = new System.Drawing.Size(119, 22);
			this.textRangeY.TabIndex = 66;
			this.textRangeY.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(908, 172);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(58, 17);
			this.label2.TabIndex = 65;
			this.label2.Text = "range Y";
			// 
			// textLasTools
			// 
			this.textLasTools.Location = new System.Drawing.Point(1021, 232);
			this.textLasTools.Name = "textLasTools";
			this.textLasTools.Size = new System.Drawing.Size(149, 22);
			this.textLasTools.TabIndex = 70;
			this.textLasTools.TextChanged += new System.EventHandler(this.textLasTools_TextChanged);
			// 
			// btnLasTools
			// 
			this.btnLasTools.Location = new System.Drawing.Point(886, 228);
			this.btnLasTools.Name = "btnLasTools";
			this.btnLasTools.Size = new System.Drawing.Size(121, 31);
			this.btnLasTools.TabIndex = 69;
			this.btnLasTools.Text = "LAStools";
			this.btnLasTools.UseVisualStyleBackColor = true;
			this.btnLasTools.Click += new System.EventHandler(this.btnLasTools_Click);
			// 
			// textTmpFolder
			// 
			this.textTmpFolder.Location = new System.Drawing.Point(1021, 273);
			this.textTmpFolder.Name = "textTmpFolder";
			this.textTmpFolder.Size = new System.Drawing.Size(149, 22);
			this.textTmpFolder.TabIndex = 72;
			this.textTmpFolder.TextChanged += new System.EventHandler(this.textTmpFolder_TextChanged);
			// 
			// btnTmpFolder
			// 
			this.btnTmpFolder.Location = new System.Drawing.Point(886, 269);
			this.btnTmpFolder.Name = "btnTmpFolder";
			this.btnTmpFolder.Size = new System.Drawing.Size(121, 31);
			this.btnTmpFolder.TabIndex = 71;
			this.btnTmpFolder.Text = "TMP folder";
			this.btnTmpFolder.UseVisualStyleBackColor = true;
			this.btnTmpFolder.Click += new System.EventHandler(this.btnTmpFolder_Click);
			// 
			// CMainForm
			// 
			this.BackColor = System.Drawing.SystemColors.MenuBar;
			this.ClientSize = new System.Drawing.Size(1182, 528);
			this.Controls.Add(this.textTmpFolder);
			this.Controls.Add(this.btnTmpFolder);
			this.Controls.Add(this.textLasTools);
			this.Controls.Add(this.btnLasTools);
			this.Controls.Add(this.trackBarRangeYmax);
			this.Controls.Add(this.trackBarRangeYmin);
			this.Controls.Add(this.textRangeY);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.trackBarRangeXmax);
			this.Controls.Add(this.trackBarRangeXmin);
			this.Controls.Add(this.textRangeX);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.checkBoxExportBitmap);
			this.Controls.Add(this.textAnalyticsFile);
			this.Controls.Add(this.btnAnalytics);
			this.Controls.Add(this.btnSequence);
			this.Controls.Add(this.checkBoxExport3d);
			this.Controls.Add(this.checkBoxColorTrees);
			this.Controls.Add(this.checkBoxExportTreeBoxes);
			this.Controls.Add(this.labelEstimatedPartitionSize);
			this.Controls.Add(this.textBoxPartitionSize);
			this.Controls.Add(this.labelEstimatedTotalSize);
			this.Controls.Add(this.textBoxEstimatedSize);
			this.Controls.Add(this.checkBoxAutoTreeHeight);
			this.Controls.Add(this.btnOpenResult);
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
			this.Controls.Add(this.btnToggleConsole);
			this.Controls.Add(this.btnAbort);
			this.Controls.Add(this.textProgress);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.textOutputFolder);
			this.Controls.Add(this.btnOutputFolder);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.textReftreeFolder);
			this.Controls.Add(this.btnReftreesFolder);
			this.Controls.Add(this.textForestFilePath);
			this.Controls.Add(this.btnSellectForest);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "CMainForm";
			this.Text = "ForestReco";
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.trackBarPartition)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarGroundArrayStep)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtent)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtentMultiply)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAvgTreeHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeXmin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeXmax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeYmax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeYmin)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		#region path selection
		private void textOutputFolder_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.outputFolderPath, textOutputFolder.Text);
		}

		private void btnOutputFolder_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFolder(textOutputFolder);
		}
		
		private void btnSellectReftreeFodlers_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFolder(textReftreeFolder);
		}

		private void textReftreeFolder_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.reftreeFolderPath, textReftreeFolder.Text);
		}

		private void btnSellectForest_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFile(textForestFilePath, "Select forest file", new List<string>() { "las", "laz" }, "forest");
		}

		private void btnSequence_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFile(textForestFilePath, "Select sequence config", "seq", "sequence");
		}

		private void textForestFilePath_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(
				ESettings.forestFilePath, textForestFilePath.Text);

			string fullFilePath = CParameterSetter.GetStringSettings(ESettings.forestFilePath);

			string[] lines = CCmdController.GetHeaderLines(fullFilePath);

			//string[] lines = CProgramLoader.GetFileLines(fullFilePath, 20);
			if(lines == null)
				return;

			if(CSequenceController.IsSequence())
			{ return; }

			CProjectData.header = new CHeaderInfo(lines);
			RefreshEstimatedSize();

			//dont update if not inited yet
			rangeController?.UpdateRangeBounds();
		}
		
		private void btnSelectCheckTree_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFile(textForestFilePath, "Select checktree file", "txt", "checktree");
		}

		private void buttonAnalytics_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFile(textForestFilePath, "Select analytics file (CSV)", "csv", "csv");
		}

		private void textAnalyticsFile_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.analyticsFilePath, textAnalyticsFile.Text);
		}

		private void textCheckTreePath_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.checkTreeFilePath, textCheckTreePath.Text);
		}

		private void btnLasTools_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFolder(textLasTools);
		}

		private void textLasTools_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.lasToolsFolderPath, textLasTools.Text);
		}

		private void btnTmpFolder_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFolder(textTmpFolder);
		}

		private void textTmpFolder_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.tmpFilesFolderPath, textTmpFolder.Text);
		}


		#endregion

		private void RefreshEstimatedSize()
		{
			CResultSize.WriteEstimatedSize(textBoxEstimatedSize, textBoxPartitionSize);
		}
		
		private void trackBarPartition_Scroll(object sender, EventArgs e)
		{
			if(blockRecursion)
			{ return; }
			trackValue = trackBarPartition.Value;
			if(trackValue % smallChangeValue != 0)
			{
				trackValue = trackValue / smallChangeValue * smallChangeValue;

				blockRecursion = true;
				trackBarPartition.Value = trackValue;
				blockRecursion = false;
			}

			textPartition.Text = trackBarPartition.Value + " m";
			CParameterSetter.SetParameter(ESettings.partitionStep, trackBarPartition.Value);
			RefreshEstimatedSize();
		}

		//snap to multiply of 5 implementation
		private bool blockRecursion;
		private int smallChangeValue = 5;
		private int trackValue;
		private void trackBarGroundArrayStep_Scroll(object sender, EventArgs e)
		{
			if(blockRecursion)
			{ return; }
			trackValue = trackBarGroundArrayStep.Value;
			if(trackValue % smallChangeValue != 0)
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

		#region checkboxes
		private void checkBoxExportTreeStructures_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportTreeStructures,
				checkBoxExportTreeStructures.Checked);

			RefreshEstimatedSize();
		}

		private void checkBoxExportInvalidTrees_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportInvalidTrees,
				checkBoxExportInvalidTrees.Checked);
		}

		private void checkBoxExportRefTrees_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportRefTrees, checkBoxExportRefTrees.Checked);
			RefreshEstimatedSize();
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

			btnSelectCheckTree.Enabled = checkBoxUseCheckTree.Checked;
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

		private void checkBoxExportTreeBoxes_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportTreeBoxes, checkBoxExportTreeBoxes.Checked);
			RefreshEstimatedSize();
		}

		
		private void checkBoxAutoTreeHeight_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.autoAverageTreeHeight, checkBoxAutoTreeHeight.Checked);

			trackBarAvgTreeHeight.Enabled = !checkBoxAutoTreeHeight.Checked;
			trackBarAvgTreeHeight.BackColor = checkBoxAutoTreeHeight.Checked ?
				System.Drawing.Color.Gray : trackBarPartition.BackColor; //dont know color code of 'enabled color'
		}

		private void checkBoxColorTrees_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.colorTrees, checkBoxColorTrees.Checked);

		}

		private void checkBoxExort3d_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.export3d, checkBoxExport3d.Checked);

			SetExport3DchekboxesEnabled(checkBoxExport3d.Checked);
		}

		#endregion

		private void SetExport3DchekboxesEnabled(bool pValue)
		{
			checkBoxExportTreeStructures.Enabled = pValue;
			checkBoxExportTreeBoxes.Enabled = pValue;
			checkBoxExportInvalidTrees.Enabled = pValue;
			checkBoxExportRefTrees.Enabled = pValue;
			checkBoxExportPoints.Enabled = pValue;
			checkBoxExportCheckTrees.Enabled = pValue;
		}

		private void checkBoxExportBitmap_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportBitmap, checkBoxExportBitmap.Checked);
		}

		public void SetStartBtnEnabled(bool pValue)
		{
			btnStart.Enabled = pValue;
			btnAbort.Enabled = !pValue;
		}

		private void btnOpenResult_Click(object sender, EventArgs e)
		{
			string folderPath = CObjPartition.folderPath;
			if(string.IsNullOrEmpty(folderPath))
			{ return; }
			if(!Directory.Exists(folderPath))
			{ return; }
			Process.Start(folderPath);
		}


		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			EProcessResult processResult = CProgramStarter.Start();
			switch(processResult)
			{
				//case EProcessResult.Exception:
				case EProcessResult.Cancelled:
					e.Cancel = true;
					break;
			}
		}

		// This event handler updates the progress.
		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//resultLabel.Text = (e.ProgressPercentage.ToString() + "%");
			progressBar.Value = e.ProgressPercentage;

			string[] results = null;
			/*string result = null;

			try
			{
				result = (string)e.UserState;
				results = new string[1] { result };
			}
			catch (Exception ex)
			{

			}
			if (string.IsNullOrEmpty(result))
			{
				try
				{
					results = (string[])e.UserState;
				}
				catch (Exception ex)
				{
					CDebug.Error("backgroundWorker exception. " + ex.Message);
					results = new string[1] { ex.Message };
				}
			}*/
			try
			{
				results = (string[])e.UserState;
			}
			catch(Exception ex)
			{
				CDebug.Error("backgroundWorker exception. " + ex.Message);
				results = new string[1] { ex.Message };
			}

			if(results == null)
			{ return; }

			string resultText = "";
			for(int i = 0; i < results.Length; i++)
			{
				resultText += results[i] + Environment.NewLine;
			}
			if(resultText.Length > 0)
			{
				textProgress.Text = resultText;
			}

		}

		// This event handler deals with the results of the background operation.
		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			SetStartBtnEnabled(true);

			if(e.Cancelled)
			{
				//CDebug.Step(EProgramStep.Cancelled); //cant call from this thread!
				textProgress.Text = "CANCELLED";
			}

			//ERROR and DONE messages should be handelend during the process. no need to write to textProgress
			else if(e.Error != null)
			{
				CDebug.WriteLine("Error: " + e.Error.Message);
			}
			else
			{
				CDebug.WriteLine("Done!");
			}
		}

		/// <summary>
		/// https://stackoverflow.com/questions/1469764/run-command-prompt-commands
		/// TODO: check spaces in commands!
		/// TODO: add folder selection for lastools EXE files
		/// </summary>
		private void buttonTest1_Click(object sender, EventArgs e)
		{
			Process currentProcess;

			//X: 506000		- 506350
			//Y: 6489940	- 5490200
			string fileName = "ANE_2000AGL";

			float min_x, min_y, max_x, max_y;
			min_x = 506100;
			max_x = 506150;
			min_y = 5489950;
			max_y = 5490000;

			string splitFileName = $"{fileName}_s[{min_x},{max_x}]-[{min_y},{max_y}]";
			const string LAS = ".las";
			bool splitFileExists = File.Exists(splitFileName + LAS);
			CDebug.WriteLine($"split file: {splitFileName} exists = {splitFileExists}");


			if(!splitFileExists)
			{
				string keepXY = $"-keep_xy {min_x} {min_y} {max_x} {max_y}";
				string split =
					"/C " +
					"lassplit -i " +
					"D:\\Resources\\ForestReco\\podklady\\LAStools\\MINE\\" + fileName + ".laz " +
					keepXY +
					" -o " +
					splitFileName + LAS;
				currentProcess = Process.Start("CMD.exe", split);

				while(!currentProcess.HasExited)
				{
					Thread.Sleep(1000);
					CDebug.WriteLine("waiting for lassplit to finish");
				}
			}
			//rename split file
			//for some reason output split file gets appendix: "_0000000" => remove it

			//todo: move to Utils
			// Source file to be renamed  
			string sourceFile = splitFileName + "_0000000" + LAS;
			// Create a FileInfo  
			FileInfo fi = new FileInfo(sourceFile);
			// Check if file is there  
			if(fi.Exists)
			{
				// Move file with a new name. Hence renamed.  
				fi.MoveTo(splitFileName + LAS);
				Console.WriteLine("Split file Renamed.");
			}

			string heightFileName = splitFileName + "_h";
			bool heightFileExists = File.Exists(heightFileName + LAS);
			CDebug.WriteLine($"height file: {heightFileName} exists = {heightFileExists}");

			if(!heightFileExists)
			{
				string height =
					"/C " +
					"lasheight -i " +
					splitFileName + LAS +
					" -o " +
					heightFileName + LAS;
				currentProcess = System.Diagnostics.Process.Start("CMD.exe", height);

				while(!currentProcess.HasExited)
				{
					Thread.Sleep(1000);
					CDebug.WriteLine("waiting for lasheight to finish");
				}
			}


			string classifyFileName = heightFileName + "_c";
			bool classifyFileExists = File.Exists(classifyFileName + LAS);
			CDebug.WriteLine($"classify file: {classifyFileName} exists = {classifyFileExists}");

			if(!classifyFileExists)
			{
				string classify =
				"/C " +
				"lasclassify -i " +
				heightFileName + LAS +
				" -o " +
				classifyFileName + LAS;
				currentProcess = Process.Start("CMD.exe", classify);

				while(!currentProcess.HasExited)
				{
					Thread.Sleep(1000);
					CDebug.WriteLine("waiting for lasclassify to finish");
				}
			}

			//use split file name to get unique file name
			string txtFileName = splitFileName + ".txt";
			bool txtFileExists = File.Exists(txtFileName);
			CDebug.WriteLine($"txt file: {txtFileName} exists = {txtFileExists}");

			if(!txtFileExists)
			{
				string toTxt =
				"/C " +
				"las2txt -i " +
				classifyFileName + LAS +
				" -o " +
				txtFileName +
				" -parse xyzcu -sep tab -header percent";
				currentProcess = Process.Start("CMD.exe", toTxt);
			}
		}

		#region range
		private void trackBarRangeXmin_Scroll(object sender, EventArgs e)
		{
			rangeController.trackBarRangeXmin_Scroll(sender, e);
		}

		private void trackBarRangeXmax_Scroll(object sender, EventArgs e)
		{
			rangeController.trackBarRangeXmax_Scroll(sender, e);
		}

		private void trackBarRangeYmin_Scroll(object sender, EventArgs e)
		{
			rangeController.trackBarRangeYmin_Scroll(sender, e);
		}

		private void trackBarRangeYmax_Scroll(object sender, EventArgs e)
		{
			rangeController.trackBarRangeYmax_Scroll(sender, e);
		}

		#endregion


		//TODO: test if CUiRangeController is ok, then delete
		/*
		private void SetRangeX()
		{
			textRangeX.Text = 
				$"[{(trackBarRangeXmin.Value / 10f).ToString("0.0")}] - " +
				$"[{(trackBarRangeXmax.Value / 10f).ToString("0.0")}]";
			CParameterSetter.SetParameter(ESettings.rangeXmin, trackBarRangeXmin.Value);
			CParameterSetter.SetParameter(ESettings.rangeXmax, trackBarRangeXmax.Value);
		}

		private void SetRangeY()
		{
			textRangeY.Text =
				$"[{(trackBarRangeYmin.Value / 10f).ToString("0.0")}] - " +
				$"[{(trackBarRangeYmax.Value / 10f).ToString("0.0")}]";
			CParameterSetter.SetParameter(ESettings.rangeYmin, trackBarRangeYmin.Value);
			CParameterSetter.SetParameter(ESettings.rangeYmax, trackBarRangeYmax.Value);
		}

		private void UpdateRangeBounds()
		{
			//X
			trackBarRangeXmin.SetRange((int)CProjectData.header.Min.X * 10, (int)CProjectData.header.Max.X * 10);
			trackBarRangeXmax.SetRange((int)CProjectData.header.Min.X * 10, (int)CProjectData.header.Max.X * 10);

			int minValue = CParameterSetter.GetIntSettings(ESettings.rangeXmin);
			int maxValue = CParameterSetter.GetIntSettings(ESettings.rangeXmax);

			trackBarRangeXmin.Value = (int)CUtils.LimitToRange(minValue, 
				trackBarRangeXmin.Minimum, trackBarRangeXmin.Maximum);
			trackBarRangeXmax.Value = (int)CUtils.LimitToRange(maxValue,
				trackBarRangeXmax.Minimum, trackBarRangeXmax.Maximum);

			SetRangeX();

			//Y
			//in project Y = elevation, but in lidar it is Z 
			trackBarRangeYmin.SetRange((int)CProjectData.header.Min.Z * 10, (int)CProjectData.header.Max.Z * 10);
			trackBarRangeYmax.SetRange((int)CProjectData.header.Min.Z * 10, (int)CProjectData.header.Max.Z * 10);

			minValue = CParameterSetter.GetIntSettings(ESettings.rangeYmin);
			maxValue = CParameterSetter.GetIntSettings(ESettings.rangeYmax);

			trackBarRangeYmin.Value = (int)CUtils.LimitToRange(minValue,
				trackBarRangeYmin.Minimum, trackBarRangeYmin.Maximum);
			trackBarRangeYmax.Value = (int)CUtils.LimitToRange(maxValue,
				trackBarRangeYmax.Minimum, trackBarRangeYmax.Maximum);

			SetRangeY();
		}

		private void trackBarRangeXmax_Scroll(object sender, EventArgs e)
		{
			SetRangeX();
		}

		private void trackBarRangeXmin_Scroll(object sender, EventArgs e)
		{
			SetRangeX();
		}

		private void trackBarRangeYmin_Scroll(object sender, EventArgs e)
		{
			SetRangeY();
		}

		private void trackBarRangeYmax_Scroll(object sender, EventArgs e)
		{
			SetRangeY();
		}*/
	}
}
