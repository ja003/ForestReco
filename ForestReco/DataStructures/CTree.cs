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
		public CTreePoint peak;
		//public List<CTreePoint> points = new List<CTreePoint>();
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
			peak = new CTreePoint(pPoint);
			//points.Add(peak);
			if (CTreeManager.DEBUG) Console.WriteLine("new tree "+pTreeIndex);

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
			
			foreach (CTreePoint p in pSubTree.GetAllPoints())
			{
				AddPoint(p);
			}
			//foreach (CTreePoint p in pSubTree.points)
			//{
			//	AddPoint(p);
			//}
		}

		public List<CTreePoint> GetAllPoints()
		{
			List<CTreePoint> allPoints = new List<CTreePoint>();
			allPoints.Add(peak);
			foreach (CBranch b in branches)
			{
				foreach (CTreePoint p in b.points)
				{
					allPoints.Add(p);
				}
			}
			return allPoints;
		}

		public void ForceAddPoint(Vector3 pPoint)
		{
			//points.Add(new CTreePoint(pPoint));
		}

		public bool TryAddPoint(CTreePoint pPoint)
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

		private void SetNewPeak(CTreePoint pPoint)
		{
			if (CTreeManager.DEBUG) Console.WriteLine("SetNewPeak " + pPoint);
			CTreePoint oldPeak = peak.Clone();
			//first set new peak then move old one to appropriate branch
			//but only if new peak is not merged old one
			bool isPointMergedWithPeak = peak.Includes(pPoint);
			AddPoint(pPoint, false); //this defines new peak
			if (!isPointMergedWithPeak) { GetBranchFor(oldPeak).AddPoint(oldPeak); }
		}

		private bool IsNewPeak(CTreePoint pPoint)
		{
			if (peak.Includes(pPoint)) { return true; }
			if (pPoint.Y < peak.Y) { return false; }
			float angle = CUtils.AngleBetweenThreePoints(
				new List<Vector3> { pPoint.Center - Vector3.UnitY, pPoint.Center, peak.Center }, Vector3.UnitY);
			return Math.Abs(angle) < CTreeManager.MAX_BRANCH_ANGLE;
		}

		/// <summary>
		/// Adds point and updates tree extents.
		/// 'pAddToBranch' adds this point to its appropriate branch. Should be false
		/// for example when this point is peak
		/// </summary>
		private void AddPoint(CTreePoint pPoint, bool pAddToBranch = true)
		{
			//points.Add(pPoint);
			if (peak.Includes(pPoint)) { peak.AddPoint(pPoint); }
			else if (pPoint.Y > peak.Y)
			{
				peak = pPoint;
				if (CTreeManager.DEBUG) Console.WriteLine("new peak = " + pPoint);
			}

			if (pPoint.X < mostLeft.X) { mostLeft = pPoint.Center; }
			if (pPoint.Z > mostTop.Z) { mostTop = pPoint.Center; }
			if (pPoint.X > mostRight.X) { mostRight = pPoint.Center; }
			if (pPoint.Z < mostBot.Z) { mostBot = pPoint.Center; }

			if (pAddToBranch) { GetBranchFor(pPoint).AddPoint(pPoint); }
		}


		private bool BelongsToTree(CTreePoint pPoint)
		{
			//is close
			float distToLeft = CUtils.Get2DDistance(pPoint.Center, mostLeft);
			if (distToLeft > CTreeManager.MAX_TREE_EXTENT)
			{
				return DoesntBelongToTree(pPoint.Center, mostLeft, distToLeft);
			}
			float distToTop = CUtils.Get2DDistance(pPoint.Center, mostTop);
			if (distToTop > CTreeManager.MAX_TREE_EXTENT)
			{
				return DoesntBelongToTree(pPoint.Center, mostTop, distToTop);
			}
			float distToRight = CUtils.Get2DDistance(pPoint.Center, mostRight);
			if (distToRight > CTreeManager.MAX_TREE_EXTENT)
			{
				return DoesntBelongToTree(pPoint.Center, mostRight, distToRight);
			}
			float distToBot = CUtils.Get2DDistance(pPoint.Center, mostBot);
			if (distToBot > CTreeManager.MAX_TREE_EXTENT)
			{
				return DoesntBelongToTree(pPoint.Center, mostBot, distToBot);
			}

			float angle = CUtils.AngleBetweenThreePoints(new List<Vector3>
			{
				peak.Center - Vector3.UnitY, peak.Center, pPoint.Center
			}, Vector3.UnitY);
			if (angle > CTreeManager.MAX_BRANCH_ANGLE) { return false; }

			return true;
		}

		private bool DoesntBelongToTree(Vector3 pPoint, Vector3 pFromPoint, float pDistance)
		{
			if (CTreeManager.DEBUG) Console.WriteLine("point " + pPoint + " is too far from " + pFromPoint +
				". dist = " + pDistance);
			return false;
		}

		private CBranch GetBranchFor(CTreePoint pPoint)
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

		private const float POINT_OFFSET = 0.05f;

		public Obj GetObj(string pName, CPointArray pArray, bool pExportBranches)
		{
			//if (CTreeManager.DEBUG) Console.WriteLine("GetObj " + pName);

			Obj obj = new Obj(pName);
			//obj.Position = peak;
			int vertexIndex = 1;
			SVector3 arrayCenter = (pArray.botLeftCorner + pArray.topRightCorner) / 2;

			foreach (CTreePoint p in GetAllPoints())
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
						Vector3 p = i == 0 ? peak.Center : b.points[i - 1].Center;
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
						Vector3 nextP = i == 0 ? b.points[0].Center : b.points[i].Center;
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
			string pointsS = pPoints ? (" [" + GetAllPoints().Count + "]") : "";
			string peakS = pPeak ? "| peak = " + peak : "";
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