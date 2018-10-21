using System.Windows.Forms;

namespace ForestReco
{
	public static class CHintManager
	{
		public static void ShowHint(EHint pHint)
		{
			string hintText;
			switch (pHint)
			{
				case EHint.PartitionStep:
					hintText = "Size of one part of final OBJ file.\n";
					hintText += "Files with too big size are almost impossible to work with.";
					break;

				default:
					CDebug.Error("hint not defined!");
					return;
			}
			MessageBox.Show(hintText);
		}
	}

	public enum EHint
	{
		PartitionStep
	}
}