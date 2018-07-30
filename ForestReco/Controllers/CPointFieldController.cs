using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public class CPointFieldController
	{
		private List<CPointField> fields = new List<CPointField>();

		public SVector3 botLeftCorner; //lower corner
		public SVector3 topRightCorner; //upper corner
		private double minHeight;
		private double maxHeight;

		public CPointFieldController(CHeaderInfo pHeader, float pStepSize, int pLOD)
		{
			botLeftCorner = pHeader.GetBotLeftCorner();
			topRightCorner = pHeader.GetTopRightCorner();

			minHeight = pHeader.GetMinHeight();
			maxHeight = pHeader.GetMaxHeight();
			//init fields
			for (int i = 0; i <= pLOD; i++)
			{
				fields.Add(new CPointField(this, pStepSize / (i + 1)));
			}
		}

		/// <summary>
		/// Adds point on all defined fields
		/// </summary>
		public void AddPointInFields(int pClass, SVector3 pPoint)
		{
			foreach (CPointField field in fields)
			{
				field.AddPointInField(pClass, pPoint);
			}
		}

		public void ExportToObj(string pFileName, EExportStrategy pStrategy, EHeight pHeight)
		{
			ExportToObj(pFileName, pStrategy, new List<EHeight> { pHeight });
		}

		public void ExportToObj(string pFileName, EExportStrategy pStrategy, List<EHeight> pHeights)
		{
			CPointFieldExporter.ExportToObj(fields[0], pFileName, pStrategy, pHeights, minHeight);
		}

		public void AssignTrees(int pLOD)
		{
			fields[pLOD].AssignTrees();
		}

		public void AssignTreesToAll(int pLOD)
		{
			fields[pLOD].AssignTreesToAll();
		}

		public void FillMissingHeights(int pLOD, EHeight pHeight)
		{
			fields[pLOD].FillMissingHeights(pHeight);
		}

		public void CalculateLocalExtrems(int pLOD)
		{
			fields[pLOD].CalculateLocalExtrems();
		}

		public override string ToString()
		{
			return this.fields[0].ToString();
		}
	}
}