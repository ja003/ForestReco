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
				
		public void SelectFolder(TextBox pText)
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
			  
		public void SelectFile(TextBox pText, string pTitle, string pExtension, string pDescription)
		 => SelectFile(pText, pTitle, new List<string>() { pExtension }, pDescription);

		public void SelectFile(TextBox pText, string pTitle, List<string> pExtensions, string pDescription)
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

	}
}
