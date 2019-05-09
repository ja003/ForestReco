namespace ForestReco
{
	public struct SSplitRange
	{
		public float MinX;
		public float MinY;
		public float MaxX;
		public float MaxY;

		public SSplitRange(float minX, float minY, float maxX, float maxY)
		{
			MinX = minX;
			MinY = minY;
			MaxX = maxX;
			MaxY = maxY;
		}

		public string ToStringX()
		{
			return $"[{MinX.ToString("0.0")}] - [{MaxX.ToString("0.0")}]";
		}

		public string ToStringY()
		{
			return $"[{MinY.ToString("0.0")}] - [{MaxY.ToString("0.0")}]";
		}

		public override string ToString()
		{
			return ToStringX() + "," + ToStringY();
		}
	}
}
