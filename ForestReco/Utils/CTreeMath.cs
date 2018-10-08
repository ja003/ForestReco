﻿using System;
using System.Numerics;

namespace ForestReco
{
	/// <summary>
	/// Calculations related to 2 different trees
	/// </summary>
	public static class CTreeMath
	{
		/// <summary>
		/// Calculates similarity between this reference tree and given tree
		/// Returns [0,1]. 1 = best match
		/// </summary>
		public static STreeSimilarity GetSimilarityWith(CRefTree pRefTree, CTree pOtherTree)
		{
			//Vector3 offsetToOtherTree = Get2DOffsetTo(pRefTree, pOtherTree);
			//float scaleRatio = GetScaleRatioTo(pRefTree, pOtherTree);
			int indexOffset = GetIndexOffsetBetweenBestMatchBranches(pRefTree, pOtherTree);

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
					offsetBranchIndex = pRefTree.Branches.Count + offsetBranchIndex;
				}
				CBranch refBranch = pRefTree.Branches[offsetBranchIndex % pRefTree.Branches.Count];
				float similarityWithOtherBranch = refBranch.GetSimilarityWith(otherBranch);
				if (similarityWithOtherBranch >= 0)
				{
					similarity += similarityWithOtherBranch;
					definedSimilarityCount++;
				}
			}
			Console.WriteLine("\n---------------\nsimilarity of \n" + pRefTree + "\nwith \n" + pOtherTree);

			if (definedSimilarityCount == 0)
			{
				Console.WriteLine("Error. no similarity defined");
				return new STreeSimilarity(0,0);
			}
			similarity /= definedSimilarityCount;

			//Console.WriteLine("similarity = " + similarity + ". defined = " + definedSimilarityCount + "\n--------");

			return new STreeSimilarity(similarity, indexOffset * CTree.BRANCH_ANGLE_STEP);
		}

		

		/// <summary>
		/// Returns offset angle between best defined branch of pOtherTree and best matching branch 
		/// from this tree
		/// </summary>
		//public static int GetOffsetAngleTo(CRefTree pRefTree, CTree pOtherTree)
		//{
		//	return GetIndexOffsetBetweenBestMatchBranches(pRefTree, pOtherTree) * CTree.BRANCH_ANGLE_STEP;
		//}

		/// <summary>
		/// First finds most defined branch of pOtherTree.
		/// Then finds the branch on this reference tree which best matches found most defined branch.
		/// Returns index offset between these branches.
		/// </summary>
		private static int GetIndexOffsetBetweenBestMatchBranches(CRefTree pRefTree, CTree pOtherTree)
		{
			CBranch mostDefinedBranch = pOtherTree.GetMostDefinedBranch();

			//todo: try rotate other tree to find bestMatch and include this rotation in similarity calculation
			CBranch bestMatchBranch = GetBestMatchBranch(pRefTree, mostDefinedBranch);

			int indexOfMostDefined = pOtherTree.Branches.IndexOf(mostDefinedBranch);
			int indexOfBestMatch = pRefTree.Branches.IndexOf(bestMatchBranch);
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
		private static CBranch GetBestMatchBranch(CRefTree pRefTree, CBranch pOtherBranch)
		{
			Vector3 offsetToRefTree = GetOffsetTo(pOtherBranch.tree, pRefTree);
			float scaleRatio = GetScaleRatioTo(pOtherBranch.tree, pRefTree);
			
			//Console.WriteLine("offsetToRefTree = " + offsetToRefTree);
			//Console.WriteLine("scaleRatio = " + scaleRatio);

			float bestSimilarity = 0;
			CBranch bestMatchBranch = pRefTree.Branches[0];
			foreach (CBranch b in pRefTree.Branches)
			{
				float similarity = b.GetSimilarityWith(pOtherBranch);
				if (similarity > bestSimilarity)
				{
					bestSimilarity = similarity;
					bestMatchBranch = b;
				}
			}
			//Console.WriteLine(bestSimilarity + " GetBestMatchBranch = " + bestMatchBranch);
			return bestMatchBranch;
		}


		/// <summary>
		/// Returns ratio of tree heights
		/// </summary>
		public static float GetScaleRatioTo(CTree pTree, CTree pRefTree)
		{
			float treeHeight = pTree.GetTreeHeight();
			float refTreeHeight = pRefTree.GetTreeHeight();

			float heightRatio = treeHeight / refTreeHeight;
			return heightRatio;
		}

		/// <summary>
		/// We use only 2D offset. 
		/// </summary>
		public static Vector3 GetOffsetTo(CTree pFromTree, CTree pToTree)
		{
			Vector3 offset = pToTree.peak.Center - pFromTree.peak.Center;
			return offset;
		}
	}
}