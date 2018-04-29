using System.Numerics;

namespace ForestReco
{
	public abstract class CCoordinatesElement
	{
		public float HeightSum;
		public float HeightMax = float.MinValue;
		public float HeightMin = float.MaxValue;
		public int CoordinatesCount;
		public int VertexIndex;

		private const int HEIGHT_MULTIPLY = 1; //only for better visualisation of height difference

		protected CCoordinatesElement()
		{
			VertexIndex = -1;
		}

		/// <summary>
		/// Adds coordinate height to the sum of this field position
		/// </summary>
		public void AddCoordinate(Vector3 pCoordinate, int pZindex)
		{
			float height = pCoordinate.Z;
			HeightSum += height;
			if (HeightMax < height) { HeightMax = height; }
			if (HeightMin > height) { HeightMin = height; }
			CoordinatesCount++;

			OnAddCoordinate(pCoordinate, pZindex);
		}

		public virtual bool IsDefined(EHeight pHeight)
		{
			return CoordinatesCount > 0;
		}

		protected abstract void OnAddCoordinate(Vector3 pCoordinate, int pZindex);

		public float GetHeightAverage()
		{
			if (CoordinatesCount == 0)
			{
				return 0;
			}
			else
			{
				return HeightSum * HEIGHT_MULTIPLY / CoordinatesCount;
			}
		}

		/// <summary>
		/// Returns 0 if not defined
		/// </summary>
		public float GetHeightMax()
		{
			float heightMax = HeightMax > float.MinValue ? HeightMax : 0;

			return heightMax;
		}

		/// <summary>
		/// Returns 0 if not defined
		/// </summary>
		public float GetHeightMin() { return HeightMin < float.MaxValue ? HeightMin : 0; }
	}
}