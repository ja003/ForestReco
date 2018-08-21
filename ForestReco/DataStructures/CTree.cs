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

		private const int BRANCH_ANGLE_STEP = 36 * 2;

		//private const float MAX_TREE_EXTENT = 3;
		//private const float MAX_ANGLE = 45;

		private Vector3 mostLeft;
		private Vector3 mostTop;
		private Vector3 mostRight;
		private Vector3 mostBot;

		public CTree(Vector3 pPoint)
		{
			peak = pPoint;
			points.Add(pPoint);
			for (int i = 0; i < 360; i += BRANCH_ANGLE_STEP)
			{
				branches.Add(new CBranch(this));
			}

			mostLeft = pPoint;
			mostTop = pPoint;
			mostRight = pPoint;
			mostBot = pPoint;
		}

		public void MergeWith(CTree pSubTree)
		{
			//todo: make effective
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
			peak = pPoint;
		}

		private bool IsNewPeak(Vector3 pPoint)
		{
			if (pPoint.Y < peak.Y) { return false; }
			float angle = CUtils.AngleBetweenThreePoints(
				new List<Vector3> { pPoint - Vector3.UnitY, pPoint, peak }, Vector3.UnitY);
			return angle < CTreeManager.MAX_BRANCH_ANGLE;
		}

		private void AddPoint(Vector3 pPoint)
		{
			points.Add(pPoint);
			if (pPoint.Y > peak.Y) { peak = pPoint; }
			if (pPoint.X < mostLeft.X) { mostLeft = pPoint; }
			if (pPoint.Z > mostTop.Z) { mostTop = pPoint; }
			if (pPoint.X > mostRight.X) { mostRight = pPoint; }
			if (pPoint.Z < mostBot.Z) { mostBot = pPoint; }

			GetBranchFor(pPoint).AddPoint(pPoint);
		}


		private bool BelongsToTree(Vector3 pPoint)
		{
			//is close
			float distToLeft = Vector3.Distance(pPoint, mostLeft);
			if (distToLeft > CTreeManager.MAX_TREE_EXTENT) { return false; }
			float distToTop = Vector3.Distance(pPoint, mostTop);
			if (distToTop > CTreeManager.MAX_TREE_EXTENT) { return false; }
			float distToRight = Vector3.Distance(pPoint, mostRight);
			if (distToRight > CTreeManager.MAX_TREE_EXTENT) { return false; }
			float distToBot = Vector3.Distance(pPoint, mostBot);
			if (distToBot > CTreeManager.MAX_TREE_EXTENT) { return false; }

			float angle = CUtils.AngleBetweenThreePoints(new List<Vector3> { peak - Vector3.UnitY, peak, pPoint }, Vector3.UnitY);
			if (angle > CTreeManager.MAX_BRANCH_ANGLE) { return false; }

			return true;
		}

		private CBranch GetBranchFor(Vector3 pPoint)
		{
			float angle = CUtils.AngleBetweenThreePoints(new List<Vector3> { peak + Vector3.UnitY, peak, pPoint }, Vector3.UnitY);
			return branches[(int)(angle / BRANCH_ANGLE_STEP)];
		}

		public override string ToString()
		{
			return "[" + points.Count + "]" + " | top = " + peak.Y;
		}

		private const float POINT_OFFSET = 0.1f;
		public Obj GetObj(string pName, CPointArray pArray)
		{
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
			obj.updateSize();
			return obj;
		}
	}
}