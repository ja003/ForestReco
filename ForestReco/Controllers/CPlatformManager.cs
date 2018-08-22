namespace ForestReco
{
	public static class CPlatformManager
	{
		public static string GetPodkladyPath(EPlatform pPlatform)
		{
			switch (pPlatform)
			{
				case EPlatform.HomePC: return "C:\\Users\\Admin\\OneDrive - MUNI\\ŠKOLA [old]\\SDIPR\\podklady";
				case EPlatform.Notebook: return "D:\\ja004\\OneDrive - MUNI\\ŠKOLA [old]\\SDIPR\\podklady";
			}
			return "";
		}
	}

	public enum EPlatform
	{
		HomePC,
		Notebook
	}
}