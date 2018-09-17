namespace ForestReco
{
	public static class CPlatformManager
	{
		public static EPlatform platform;

		public static string GetPodkladyPath()
		{
			switch (platform)
			{
				case EPlatform.HomePC: return "C:\\Users\\Admin\\OneDrive - MUNI\\ŠKOLA [old]\\SDIPR\\podklady";
				case EPlatform.Notebook: return "D:\\ja004\\OneDrive - MUNI\\ŠKOLA [old]\\SDIPR\\podklady";
				case EPlatform.Tiarra: return "C:\\Users\\Adam\\OneDrive - MUNI\\ŠKOLA [old]\\SDIPR\\podklady";
			}
			return "";
		}
	}

	public enum EPlatform
	{
		HomePC,
		Notebook,
		Tiarra
	}
}