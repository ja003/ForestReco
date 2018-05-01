﻿using System;
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

		public Vector2 botLeftCorner; //lower corner
		public Vector2 topRightCorner; //upper corner
		private float minHeight;
		private float maxHeight;

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
		public void AddPointInFields(Vector3 pPoint)
		{
			foreach (CPointField field in fields)
			{
				field.AddPointInField(pPoint);
			}
		}

		public void ExportToObj(string pFileName, EExportStrategy pStrategy, EHeight pHeight)
		{
			CPointFieldExporter.ExportToObj(fields[0], pFileName, pStrategy, pHeight, minHeight);
		}

		public void DetectLocalExtrems(float pStepSize, int pLOD)
		{
			fields[pLOD].DetectLocalExtrems(pStepSize);
		}
	}
}