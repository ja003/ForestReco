using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Principal;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	/// <summary>
	/// Y = height
	/// </summary>
	public class CTree
	{
		public Vector3 peak;
		public List<Vector3> points = new List<Vector3>();
		private List<CBranch> branches = new List<CBranch>();

		private const int BRANCH_ANGLE_STEP = 45;

		public Vector3 possibleNewPoint;

		//private const float MAX_TREE_EXTENT = 3;
		//private const float MAX_ANGLE = 45;

		private Vector3 mostLeft;
		private Vector3 mostTop;
		private Vector3 mostRight;
		private Vector3 mostBot;

		public int treeIndex;

		public CTree(Vector3 pPoint, int pTreeIndex)
		{
			peak = pPoint;
			points.Add(pPoint);
			treeIndex = pTreeIndex;
			for (int i = 0; i < 360; i += BRANCH_ANGLE_STEP)
			{
				branches.Add(new CBranch(this, i, i + BRANCH_ANGLE_STEP));
			}

			mostLeft = pPoint;
			mostTop = pPoint;
			mostRight = pPoint;
			mostBot = pPoint;
		}

		public void MergeWith(CTree pSubTree)
		{
			if (CTreeManager.DEBUG) Console.WriteLine(this.ToString(false, false, true, false) + " MergeWith " +
				pSubTree.ToString(false, false, true, false));
			//todo: make effective
			if (pSubTree.Equals(this))
			{
				Console.WriteLine("Error. cant merge with itself.");
				return;
			}

			foreach (Vector3 p in pSubTree.points)
			{
				AddPoint(p);
			}
		}

		public bool TryAddPoint(Vector3 pPoint)
		{
			if (IsNewPeak(pPoint))
			{
				SetNewPeak(pPoint);
				return true;
			}

			if (BelongsToTree(pPoint))
			{
				AddPoint(pPoint);
				return true;
			}
			return false;
		}

		private void SetNewPeak(Vector3 pPoint)
		{
			if (CTreeManager.DEBUG) Console.WriteLine("SetNewPeak " + pPoint);
			Vector3 oldPeak = peak;
			//first set new peak then move old one to appropriate branch
			peak = pPoint;
			AddPoint(pPoint, false);
			GetBranchFor(oldPeak).AddPoint(oldPeak);
		}

		private bool IsNewPeak(Vector3 pPoint)
		{
			if (pPoint.Y < peak.Y) { return false; }
			float angle = CUtils.AngleBetweenThreePoints(
				new List<Vector3> { pPoint - Vector3.UnitY, pPoint, peak }, Vector3.UnitY);
			return Math.Abs(angle) < CTreeManager.MAX_BRANCH_ANGLE;
		}

		/// <summary>
		/// Adds point and updates tree extents.
		/// 'pAddToBranch' adds this point to its appropriate branch. Should be false
		/// for example when this point is peak
		/// </summary>
		private void AddPoint(Vector3 pPoint, bool pAddToBranch = true)
		{
			points.Add(pPoint);
			if (pPoint.Y > peak.Y) { peak = pPoint; }
			if (pPoint.X < mostLeft.X) { mostLeft = pPoint; }
			if (pPoint.Z > mostTop.Z) { mostTop = pPoint; }
			if (pPoint.X > mostRight.X) { mostRight = pPoint; }
			if (pPoint.Z < mostBot.Z) { mostBot = pPoint; }

			if (pAddToBranch) { GetBranchFor(pPoint).AddPoint(pPoint); }
		}


		private bool BelongsToTree(Vector3 pPoint)
		{
			//is close
			float distToLeft = CUtils.Get2DDistance(pPoint, mostLeft);
			if (distToLeft > CTreeManager.MAX_TREE_EXTENT)
			{
				return DoesntBelongToTree(pPoint, mostLeft, distToLeft);
			}
			float distToTop = CUtils.Get2DDistance(pPoint, mostTop);
			if (distToTop > CTreeManager.MAX_TREE_EXTENT)
			{
				return DoesntBelongToTree(pPoint, mostTop, distToTop);
			}
			float distToRight = CUtils.Get2DDistance(pPoint, mostRight);
			if (distToRight > CTreeManager.MAX_TREE_EXTENT)
			{
				return DoesntBelongToTree(pPoint, mostRight, distToRight);
			}
			float distToBot = CUtils.Get2DDistance(pPoint, mostBot);
			if (distToBot > CTreeManager.MAX_TREE_EXTENT)
			{
				return DoesntBelongToTree(pPoint, mostBot, distToBot);
			}

			float angle = CUtils.AngleBetweenThreePoints(new List<Vector3> { peak - Vector3.UnitY, peak, pPoint }, Vector3.UnitY);
			if (angle > CTreeManager.MAX_BRANCH_ANGLE) { return false; }

			return true;
		}

		private bool DoesntBelongToTree(Vector3 pPoint, Vector3 pFromPoint, float pDistance)
		{
			if (CTreeManager.DEBUG) Console.WriteLine("point " + pPoint + " is too far from " + pFromPoint + 
				". dist = " + pDistance);
			return false;
		}

		private CBranch GetBranchFor(Vector3 pPoint)
		{
			//if (Math.Abs(pPoint.X) > 0.1f)

			//float angle = CUtils.AngleBetweenThreePoints(new List<Vector3> { peak + Vector3.UnitX, peak, pPoint }, Vector3.UnitY);
			Vector2 peak2D = new Vector2(peak.X, peak.Z);
			Vector2 point2D = new Vector2(pPoint.X, pPoint.Z);
			Vector2 dir = point2D - peak2D;
			dir = Vector2.Normalize(dir);
			double angle = CUtils.GetAngle(Vector2.UnitX, dir);
			//if (CTreeManager.DEBUG) Console.WriteLine("angle " + peak2D + " - " + point2D + " = " + angle);
			if (angle < 0)
			{
				angle = 360 + angle;
			}
			return branches[(int)(angle / BRANCH_ANGLE_STEP)];
		}


		private int GetBranchesCount()
		{
			int count = 0;
			foreach (CBranch b in branches)
			{
				if (b.points.Count > 0) { count++; }
			}
			return count;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			CTree t = (CTree)obj;
			return treeIndex == t.treeIndex;
		}

		private const float POINT_OFFSET = 0.1f;

		public Obj GetObj(string pName, CPointArray pArray, bool pExportBranches)
		{
			//if (CTreeManager.DEBUG) Console.WriteLine("GetObj " + pName);

			Obj obj = new Obj(pName);
			//obj.Position = peak;
			int vertexIndex = 1;
			SVector3 arrayCenter = (pArray.botLeftCorner + pArray.topRightCorner) / 2;

			foreach (Vector3 p in points)
			{
				Vector3 clonePoint = new Vector3(p.X, p.Y, p.Z);
				clonePoint -= arrayCenter.ToVector3(true);
				clonePoint += new Vector3(0, -(float)pArray.minHeight, -2 * clonePoint.Z);

				List<Vertex> pointVertices = new List<Vertex>();

				Vertex v1 = new Vertex(clonePoint, vertexIndex);
				pointVertices.Add(v1);
				vertexIndex++;


				Vertex v2 = new Vertex(clonePoint + Vector3.UnitX * POINT_OFFSET, vertexIndex);
				pointVertices.Add(v2);
				vertexIndex++;

				Vertex v3 = new Vertex(clonePoint + Vector3.UnitZ * POINT_OFFSET, vertexIndex);
				pointVertices.Add(v3);
				vertexIndex++;

				Vertex v4 = new Vertex(clonePoint + Vector3.UnitY * POINT_OFFSET, vertexIndex);
				pointVertices.Add(v4);
				vertexIndex++;

				foreach (Vertex v in pointVertices)
				{
					obj.VertexList.Add(v);
				}

				//create 4-side representation of point
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v3 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v1, v3, v4 }));
				obj.FaceList.Add(new Face(new List<Vertex> { v2, v3, v4 }));

				//break;
			}

			if (pExportBranches)
			{
				foreach (CBranch b in branches)
				{
					for (int i = 0; i < b.points.Count; i++)
					{
						List<Vertex> pointVertices = new List<Vertex>();

						//for first point in branch use peak as a first point
						Vector3 p = i == 0 ? peak : b.points[i - 1];
						p -= arrayCenter.ToVector3(true);
						p += new Vector3(0, -(float)pArray.minHeight, -2 * p.Z);

						Vertex v1 = new Vertex(p, vertexIndex);
						pointVertices.Add(v1);
						vertexIndex++;
						Vertex v2 = new Vertex(p + Vector3.UnitX * POINT_OFFSET, vertexIndex);
						pointVertices.Add(v2);
						vertexIndex++;
						Vertex v3 = new Vertex(p + Vector3.UnitZ * POINT_OFFSET, vertexIndex);
						pointVertices.Add(v3);
						vertexIndex++;

						//for first point set first point to connect to peak
						Vector3 nextP = i == 0 ? b.points[0] : b.points[i];
						nextP -= arrayCenter.ToVector3(true);
						nextP += new Vector3(0, -(float)pArray.minHeight, -2 * nextP.Z);

						Vertex v4 = new Vertex(nextP, vertexIndex);
						pointVertices.Add(v4);
						vertexIndex++;
						/*Vertex v5 = new Vertex(nextP + Vector3.UnitX * POINT_OFFSET, vertexIndex);
						pointVertices.Add(v5);
						vertexIndex++;
						Vertex v6 = new Vertex(p + Vector3.UnitZ * POINT_OFFSET, vertexIndex);
						pointVertices.Add(v6);
						vertexIndex++;*/

						//Console.WriteLine("branch part " + p + " - " + nextP);

						foreach (Vertex v in pointVertices)
						{
							obj.VertexList.Add(v);
						}

						//create 4-side representation of point
						obj.FaceList.Add(new Face(new List<Vertex> { v1, v2, v4 }));
						obj.FaceList.Add(new Face(new List<Vertex> { v2, v3, v4 }));
						obj.FaceList.Add(new Face(new List<Vertex> { v3, v1, v4 }));

						//obj.FaceList.Add(new Face(new List<Vertex> { v4, v5, v3 }));
						//obj.FaceList.Add(new Face(new List<Vertex> { v5, v6, v3 }));
						//obj.FaceList.Add(new Face(new List<Vertex> { v6, v4, v3 }));

					}
				}
			}

			obj.updateSize();
			return obj;
		}


		public override string ToString()
		{
			return ToString(true, true, true, true);
		}

		public string ToString(bool pIndex, bool pPoints, bool pPeak, bool pBranches)
		{
			string indexS = pIndex ? treeIndex.ToString() : "";
			string pointsS = pPoints ? (" [" + points.Count + "]") : "";
			string peakS = pPeak ? ". peak = " + peak : "";
			string branchesS = pBranches ? " | branches = " + GetBranchesCount() + "_|" : "";
			if (pBranches)
			{
				foreach (CBranch b in branches)
				{
					branchesS += b;
				}
			}
			return indexS + pointsS + peakS + branchesS;
		}
	}
}