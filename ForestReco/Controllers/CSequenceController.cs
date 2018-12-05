using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ForestReco
{
	public static class CSequenceController
	{
		public const string SEQ_EXTENSION = ".seq";

		private const int oneSequenceLength = 5;

		public static List<SSequenceConfig> configs;

		public static int currentConfigIndex;

		public static void SetValues()
		{
			if (configs.Count == 0) { return; }

			CDebug.WriteLine("SetValues from config");

			SSequenceConfig currentConfig = configs[currentConfigIndex];
			CParameterSetter.SetParameter(ESettings.forestFilePath, currentConfig.path);
			int treeHeight = currentConfig.treeHeight;

			CParameterSetter.SetParameter(ESettings.autoAverageTreeHeight, treeHeight <= 0);
			if (treeHeight > 0)
			{
				CParameterSetter.SetParameter(ESettings.avgTreeHeigh, treeHeight);
			}
			CParameterSetter.SetParameter(ESettings.treeExtent, currentConfig.treeExtent);
			CParameterSetter.SetParameter(ESettings.treeExtentMultiply, currentConfig.treeExtentMultiply);
		}

		public static void OnLastSequenceEnd()
		{
			if(string.IsNullOrEmpty(lastSequenceFile)){ return;}

			CParameterSetter.SetParameter(ESettings.forestFilePath, lastSequenceFile);
		}

		private static string lastSequenceFile;

		public static void Init()
		{
			configs = new List<SSequenceConfig>();
			currentConfigIndex = 0;
			if (!IsSequence()) { return; }

			lastSequenceFile = CParameterSetter.GetStringSettings(ESettings.forestFilePath);

			string[] lines = File.ReadAllLines(lastSequenceFile);

			for (int i = 0; i < lines.Length; i += oneSequenceLength)
			{
				string[] configLines = new string[oneSequenceLength];
				configLines[0] = lines[i];
				configLines[1] = lines[i + 1];
				configLines[2] = lines[i + 2];
				configLines[3] = lines[i + 3];
				SSequenceConfig config = GetConfig(configLines);
				configs.Add(config);
			}
		}

		private static SSequenceConfig GetConfig(string[] pLines)
		{
			SSequenceConfig config = new SSequenceConfig();
			config.path = GetValue(pLines[0]);
			config.treeHeight = int.Parse(GetValue(pLines[1]));
			config.treeExtent = float.Parse(GetValue(pLines[2]));
			config.treeExtentMultiply = float.Parse(GetValue(pLines[3]));
			return config;
		}


		private static string GetValue(string pLine)
		{
			string[] split = pLine.Split('=');
			return split.Last();
		}

		public static bool IsSequence()
		{
			string mainFile = CParameterSetter.GetStringSettings(ESettings.forestFilePath);
			if (!File.Exists(mainFile)) { return false; }
			return Path.GetExtension(mainFile) == SEQ_EXTENSION;
		}

		public static bool IsLastSequence()
		{
			return configs.Count == 0 || currentConfigIndex == configs.Count - 1;
		}

	}

	public struct SSequenceConfig
	{
		public string path;
		public int treeHeight;
		public float treeExtent;
		public float treeExtentMultiply;
	}
}