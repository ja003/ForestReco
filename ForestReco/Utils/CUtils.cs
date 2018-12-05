using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public static class CUtils
	{
		public static bool PointBelongsToTree(Vector3 pPoint, Vector3 pTreetop)
		{
			Vector3 botPoint = new Vector3(pTreetop.X, pPoint.Y, pTreetop.Z);
			float angleBetweenPointAndTreetop = AngleBetweenThreePoints(botPoint, pTreetop, pPoint);
			return angleBetweenPointAndTreetop < 45;
		}
		
		public static float GetAngle(Vector2 a, Vector2 b)
		{
			a = Vector2.Normalize(a);
			b = Vector2.Normalize(b);
			double atan2 = Math.Atan2(b.Y, b.X) - Math.Atan2(a.Y, a.X);
			return ToDegree((float) atan2);
		}

		public static float GetAngleToTree(CTree pTree, Vector3 pPoint)
		{
			return AngleBetweenThreePoints(pTree.peak.Center - Vector3.UnitY * 100, pTree.peak.Center, pPoint);
		}

		//https://stackoverflow.com/questions/19729831/angle-between-3-points-in-3d-space
		public static float AngleBetweenThreePoints(Vector3 pA, Vector3 pB, Vector3 pC, bool pToDegree = true)
		{
			Vector3 v1 = new Vector3(pA.X - pB.X, pA.Y - pB.Y, pA.Z - pB.Z);
			//Similarly the vector BC(call it v2) is:

			Vector3 v2 = new Vector3(pC.X - pB.X, pC.Y - pB.Y, pC.Z - pB.Z);
			//The dot product of v1 and v2 is a function of the cosine of the angle between them(it's scaled by the product of their magnitudes). So first normalize v1 and v2:

			float v1mag = (float) Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
			Vector3 v1norm = new Vector3(v1.X / v1mag, v1.Y / v1mag, v1.Z / v1mag);

			float v2mag = (float) Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);
			Vector3 v2norm = new Vector3(v2.X / v2mag, v2.Y / v2mag, v2.Z / v2mag);
			//Then calculate the dot product:

			float res = v1norm.X * v2norm.X + v1norm.Y * v2norm.Y + v1norm.Z * v2norm.Z;
			//And finally, recover the angle:

			float angle = (float) Math.Acos(res);
			if (pToDegree)
			{
				return ToDegree(angle);
			}
			return angle;
		}

		public static float ToRadians(float val)
		{
			return (float) Math.PI / 180 * val;
		}

		private static float ToDegree(float angle)
		{
			return (float) (angle * (180.0 / Math.PI));
		}

		public static float Get2DDistance(Vector3 a, CTreePoint b)
		{
			return Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));
		}

		public static float Get2DDistance(CTreePoint a, CTreePoint b)
		{
			return Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));
		}

		public static float Get2DDistance(Vector3 a, Vector3 b)
		{
			return Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));
		}

		public static float GetOverlapRatio(CBoundingBoxObject pOfObject, CBoundingBoxObject pWithObject)
		{
			float overlapVolume = GetOverlapVolume(pOfObject, pWithObject);
			float ofObjectVolume = pOfObject.Volume;

			if (ofObjectVolume == 0)
			{
				CDebug.Error("object " + pWithObject + " has no volume");
				return 0;
			}
			float ratio = overlapVolume / ofObjectVolume;
			return ratio;
		}

		/// <summary>
		/// copied from: https://stackoverflow.com/questions/5556170/finding-shared-volume-of-two-overlapping-cuboids
		/// </summary>
		private static float GetOverlapVolume(CBoundingBoxObject pObject1, CBoundingBoxObject pObject2)
		{
			float o1minX = pObject1.b000.X;
			float o1minY = pObject1.b000.Y;
			float o1minZ = pObject1.b000.Z;

			float o1maxX = pObject1.b111.X;
			float o1maxY = pObject1.b111.Y;
			float o1maxZ = pObject1.b111.Z;

			float o2minX = pObject2.b000.X;
			float o2minY = pObject2.b000.Y;
			float o2minZ = pObject2.b000.Z;

			float o2maxX = pObject2.b111.X;
			float o2maxY = pObject2.b111.Y;
			float o2maxZ = pObject2.b111.Z;

			float xOverlap = Math.Min(o2maxX, o1maxX) - Math.Max(o2minX, o1minX);
			float yOverlap = Math.Min(o2maxY, o1maxY) - Math.Max(o2minY, o1minY);
			float zOverlap = Math.Min(o2maxZ, o1maxZ) - Math.Max(o2minZ, o1minZ);
			if (xOverlap < 0)
			{
				return 0;
			}
			if (yOverlap < 0)
			{
				return 0;
			}
			if (zOverlap < 0)
			{
				return 0;
			}
			float volume = xOverlap * yOverlap * zOverlap;
			return volume;
		}

		public static double[,] CalculateGaussKernel(int lenght, double weight)
		{
			if (lenght % 2 == 0)
			{
				CDebug.Error("CalculateGaussKernel - lenght cant be even. " + lenght);
			}

			double[,] Kernel = new double[lenght, lenght];
			double sumTotal = 0;

			int kernelRadius = lenght / 2;
			double distance = 0;

			double calculatedEuler = 1.0 /
			                         (2.0 * Math.PI * Math.Pow(weight, 2));

			for (int filterY = -kernelRadius;
				filterY <= kernelRadius;
				filterY++)
			{
				for (int filterX = -kernelRadius;
					filterX <= kernelRadius;
					filterX++)
				{
					distance = ((filterX * filterX) +
					            (filterY * filterY)) /
					           (2 * (weight * weight));

					Kernel[filterY + kernelRadius,
							filterX + kernelRadius] =
						calculatedEuler * Math.Exp(-distance);

					sumTotal += Kernel[filterY + kernelRadius,
						filterX + kernelRadius];
				}
			}

			for (int y = 0; y < lenght; y++)
			{
				for (int x = 0; x < lenght; x++)
				{
					double finalVal = Kernel[y, x] * (1.0 / sumTotal);
					//CDebug.WriteLine(finalVal.ToString("0.00000"));
					Kernel[y, x] = finalVal;
				}
			}

			return Kernel;
		}

		private static Random rng = new Random();

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static string SerializeVector3(Vector3 pVector)
		{
			return pVector.X + " " + pVector.Y + " " + pVector.Z;
		}

		public static bool IsPoint(Vector3 pPoint, Vector3 pEqual, float pTolerance = 0.1f)
		{
			return Vector3.Distance(pPoint, new Vector3(675.94f, 128.04f, 1140.9f)) < pTolerance;
		}
	}
}
