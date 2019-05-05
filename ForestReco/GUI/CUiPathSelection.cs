using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ForestReco.GUI
{
	internal class CUiPathSelection
	{

		private CMainForm form;

		public CUiPathSelection(CMainForm pForm)
		{
			form = pForm;
		}

		public void btnSelectCheckTree_Click()
		{
			SelectFile(form.textForestFilePath, "Select checktree file", "txt", "checktree");
		}

		public void buttonAnalytics_Click()
		{
			SelectFile(form.textForestFilePath, "Select analytics file (CSV)", "csv", "csv");
		}

		public void textCheckTreePath_TextChanged()
		{
			CParameterSetter.SetParameter(ESettings.checkTreeFilePath, form.textCheckTreePath.Text);
		}

		public void textAnalyticsFile_TextChanged()
		{
			CParameterSetter.SetParameter(
				ESettings.analyticsFilePath, form.textAnalyticsFile.Text);
		}

		public void btnSellectForest_Click()
		{
			SelectFile(form.textForestFilePath, "Select forest file", new List<string>() { "las", "laz" }, "forest");
			//string path = CParameterSetter.SelectFile("Select forest file", "txt", "forest");
			//if(path.Length == 0)
			//{
			//	CDebug.Warning("no path selected");
			//	return;
			//}
			//form.textForestFilePath.Clear();
			//form.textForestFilePath.Text = path;
		}

		public void btnSequence_Click()
		{
			SelectFile(form.textForestFilePath, "Select sequence config", "seq", "sequence");
			//string path = CParameterSetter.SelectFile("Select sequence config", "seq", "sequence");
			//if(path.Length == 0)
			//{
			//	CDebug.Warning("no path selected");
			//	return;
			//}
			//form.textForestFilePath.Clear();
			//form.textForestFilePath.Text = path;
		}

		public void btnSellectReftreeFodlers_Click()
		{
			SelectFolder(form.textReftreeFolder);
		}

		public void btnOutputFolder_Click()
		{
			SelectFolder(form.textOutputFolder);
		}

		private void SelectFolder(TextBox pText)
		{
			string folder = CParameterSetter.SelectFolder();
			if(folder.Length == 0)
			{
				CDebug.Warning("no folder selected");
				return;
			}
			pText.Clear();
			pText.Text = folder;
		}



		private void SelectFile(TextBox pText, string pTitle, string pExtension, string pDescription)
		 => SelectFile(pText, pTitle, new List<string>() { pExtension }, pDescription);

		private void SelectFile(TextBox pText, string pTitle, List<string> pExtensions, string pDescription)
		{
			string path = CParameterSetter.SelectFile(pTitle, pExtensions, pDescription);
			if(path.Length == 0)
			{
				CDebug.Warning("no path selected");
				return;
			}
			pText.Clear(); //necessary?
			pText.Text = path;
		}

		public void textOutputFolder_TextChanged()
		{
			CParameterSetter.SetParameter(ESettings.outputFolderPath, form.textOutputFolder.Text);
		}

		public void textReftreeFolder_TextChanged()
		{
			CParameterSetter.SetParameter(ESettings.reftreeFolderPath, form.textReftreeFolder.Text);
		}
	}
}
