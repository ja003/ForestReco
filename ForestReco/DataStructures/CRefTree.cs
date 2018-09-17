﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ObjParser;

namespace ForestReco
{
	/// <summary>
	/// Reference tree object.
	/// This tree is scanned in great detail.
	/// </summary>
	public class CRefTree : CTree
	{
		public Obj Obj;

		public CRefTree(string pFileName, int pTreeIndex)
		{
			treeIndex = pTreeIndex;
			string[] lines = GetFileLines(pFileName);

			//todo: porovnávání stromů ještě není implementováno, takže toto je nanic
			bool processLines = true;
			if (processLines)
			{
				List<Tuple<int, Vector3>> parsedLines = CProgramLoader.LoadParsedLines(lines, false, false);
				AddPointsFromLines(parsedLines);
				DateTime processStartTime = DateTime.Now;
				Console.WriteLine("Process. Start = " + processStartTime);
				Process();
				Console.WriteLine("Processed | duration = " + (DateTime.Now - processStartTime));
			}
			Obj = new Obj(pFileName);
			Obj.LoadObj(GetRefTreePath(pFileName) + ".obj");
		}

		/// <summary>
		/// Calculates similarity between this reference tree and given tree
		/// Returns [0,1]. 1 = best match
		/// </summary>
		public float GetSimilarityWith(CTree pOtherTree)
		{
			Vector3 offsetToOtherTree = Get2DOffsetTo(pOtherTree);
			float scaleRatio = GetScaleRatioTo(pOtherTree);
			int indexOffset = GetIndexOffsetBetweenBestMatchBranches(pOtherTree);

			float similarity = 0;
			int definedSimilarityCount = 0;

			//todo: compare stems
			foreach (CBranch otherBranch in pOtherTree.Branches)
			{
				int indexOfOtherBranch = pOtherTree.Branches.IndexOf(otherBranch);
				//int offsetBranchIndex = (indexOfOtherBranch + indexOffset) % branches.Count;
				int offsetBranchIndex = indexOfOtherBranch + indexOffset;
				if (offsetBranchIndex < 0)
				{
					offsetBranchIndex = branches.Count + offsetBranchIndex;
				}
				CBranch refBranch = branches[offsetBranchIndex];
				float similarityWithOtherBranch = refBranch.GetSimilarityWith(otherBranch, offsetToOtherTree, scaleRatio);
				if (similarityWithOtherBranch >= 0)
				{
					similarity += similarityWithOtherBranch;
					definedSimilarityCount++;
				}
			}
			Console.WriteLine("\nsimilarity of \n" + this + "\nwith \n" + pOtherTree);

			if (definedSimilarityCount == 0)
			{
				Console.WriteLine("Error. no similarity defined");
				return 0;
			}
			similarity /= definedSimilarityCount;

			Console.WriteLine("similarity = " + similarity + ". defined = " + definedSimilarityCount + "\n--------");

			return similarity;
		}

		/// <summary>
		/// Returns ratio of tree heights
		/// </summary>
		protected float GetScaleRatioTo(CTree pOtherTree)
		{
			float otherTreeHeight = pOtherTree.GetTreeHeight();
			float refTreeHeight = GetTreeHeight();

			float heightRatio = refTreeHeight / otherTreeHeight;
			return heightRatio;
		}

		/// <summary>
		/// In reference tree the tree is defined in great detail from peak to the ground.
		/// </summary>
		public override float GetGroundHeight()
		{
			return minBB.Y;
		}

		/// <summary>
		/// Returns offset angle between best defined branch of pOtherTree and best matching branch 
		/// from this tree
		/// </summary>
		public int GetOffsetAngleTo(CTree pOtherTree)
		{
			return GetIndexOffsetBetweenBestMatchBranches(pOtherTree) * BRANCH_ANGLE_STEP;
		}

		/// <summary>
		/// First finds most defined branch of pOtherTree.
		/// Then finds the branch on this reference tree which best matches found most defined branch.
		/// Returns index offset between these branches.
		/// </summary>
		private int GetIndexOffsetBetweenBestMatchBranches(CTree pOtherTree)
		{
			Vector3 offsetToOtherTree = Get2DOffsetTo(pOtherTree);
			float scaleRatio = GetScaleRatioTo(pOtherTree);

			Console.WriteLine("offsetToOtherTree = " + offsetToOtherTree);
			Console.WriteLine("scaleRatio = " + scaleRatio);

			CBranch mostDefinedBranch = pOtherTree.GetMostDefinedBranch();

			//todo: try rotate other tree to find bestMatch and include this rotation in similarity calculation
			CBranch bestMatchBranch = GetBestMatchBranch(mostDefinedBranch, offsetToOtherTree, scaleRatio);

			int indexOfMostDefined = pOtherTree.Branches.IndexOf(mostDefinedBranch);
			int indexOfBestMatch = Branches.IndexOf(bestMatchBranch);
			int indexOffset = indexOfBestMatch - indexOfMostDefined;

			//Console.WriteLine("mostDefinedBranch = " + mostDefinedBranch);
			//Console.WriteLine("bestMatchBranch = " + bestMatchBranch);
			//Console.WriteLine("indexOfMostDefined = " + indexOfMostDefined);
			//Console.WriteLine("indexOfBestMatch = " + indexOfBestMatch);
			//Console.WriteLine("indexOffset = " + indexOffset);
			return indexOffset;
		}

		/// <summary>
		/// Returns branch with the highest similarity with other branch
		/// </summary>
		private CBranch GetBestMatchBranch(CBranch pOtherBranch, Vector3 pOffset, float pScale)
		{
			float bestSimilarity = 0;
			CBranch bestMatchBranch = branches[0];
			foreach (CBranch b in branches)
			{
				float similarity = b.GetSimilarityWith(pOtherBranch, pOffset, pScale);
				if (similarity > bestSimilarity)
				{
					bestSimilarity = similarity;
					bestMatchBranch = b;
				}
			}
			Console.WriteLine(bestSimilarity + " GetBestMatchBranch = " + bestMatchBranch);
			return bestMatchBranch;
		}

		//INIT PROCESSING

		private static string GetRefTreePath(string pFileName)
		{
			return CPlatformManager.GetPodkladyPath() + "\\tree_models\\" + pFileName;
		}

		private static string[] GetFileLines(string pFileName)
		{
			//todo: firt try to load serialised file
			string fullFilePath = GetRefTreePath(pFileName) + @".txt";
			string[] lines = File.ReadAllLines(fullFilePath);
			Console.WriteLine("load: " + fullFilePath + "\n");
			return lines;
		}

		private void AddPointsFromLines(List<Tuple<int, Vector3>> pParsedLines)
		{
			DateTime addStartTime = DateTime.Now;
			Console.WriteLine("AddPointsFromLines " + pParsedLines.Count + ". Start = " + addStartTime);
			int pointsToAddCount = pParsedLines.Count;

			//lines are sorted. first point is peak for sure
			Init(pParsedLines[0].Item2, treeIndex);

			for (int i = 1; i < Math.Min(pParsedLines.Count, pointsToAddCount); i++)
			{
				DateTime lineStartTime = DateTime.Now;

				Tuple<int, Vector3> parsedLine = pParsedLines[i];
				Vector3 point = parsedLine.Item2;

				//all points belong to 1 tree. force add it
				TryAddPoint(point, true);

				TimeSpan duration = DateTime.Now - lineStartTime;
				if (duration.Milliseconds > 1) { Console.WriteLine(i + ": " + duration); }
			}
			Console.WriteLine("All points added | duration = " + (DateTime.Now - addStartTime));
		}
	}
}