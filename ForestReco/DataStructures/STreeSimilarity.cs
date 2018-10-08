namespace ForestReco
{
	public struct STreeSimilarity
	{
		public float similarity;
		public int angleOffset;

		public STreeSimilarity(float pSimilarity, int pAngleOffset)
		{
			similarity = pSimilarity;
			angleOffset = pAngleOffset;
		}
	}
}