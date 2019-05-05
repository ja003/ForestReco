using System;

namespace ForestReco
{
	public class CUiRangeController
	{
		private CMainForm form;

		public CUiRangeController(CMainForm pForm)
		{
			form = pForm;

			form.trackBarRangeXmin.Scroll += new EventHandler(trackBarRangeXmin_Scroll);
			form.trackBarRangeXmin.ValueChanged += new EventHandler(trackBarRangeXmin_Scroll);
			form.trackBarRangeXmax.Scroll += new EventHandler(trackBarRangeXmax_Scroll);
			form.trackBarRangeYmax.Scroll += new EventHandler(trackBarRangeYmax_Scroll);
			form.trackBarRangeYmin.Scroll += new System.EventHandler(trackBarRangeYmin_Scroll);

			UpdateRangeBounds();
		}


		private void SetRangeX()
		{
			form.textRangeX.Text =
				$"[{(form.trackBarRangeXmin.Value / 10f).ToString("0.0")}] - " +
				$"[{(form.trackBarRangeXmax.Value / 10f).ToString("0.0")}]";
			CParameterSetter.SetParameter(ESettings.rangeXmin, form.trackBarRangeXmin.Value);
			CParameterSetter.SetParameter(ESettings.rangeXmax, form.trackBarRangeXmax.Value);
		}

		private void SetRangeY()
		{
			form.textRangeY.Text =
				$"[{(form.trackBarRangeYmin.Value / 10f).ToString("0.0")}] - " +
				$"[{(form.trackBarRangeYmax.Value / 10f).ToString("0.0")}]";
			CParameterSetter.SetParameter(ESettings.rangeYmin, form.trackBarRangeYmin.Value);
			CParameterSetter.SetParameter(ESettings.rangeYmax, form.trackBarRangeYmax.Value);
		}

		public void UpdateRangeBounds()
		{
			//X
			form.trackBarRangeXmin.SetRange((int)CProjectData.header.Min.X * 10, (int)CProjectData.header.Max.X * 10);
			form.trackBarRangeXmax.SetRange((int)CProjectData.header.Min.X * 10, (int)CProjectData.header.Max.X * 10);

			int minValue = CParameterSetter.GetIntSettings(ESettings.rangeXmin);
			int maxValue = CParameterSetter.GetIntSettings(ESettings.rangeXmax);

			form.trackBarRangeXmin.Value = (int)CUtils.LimitToRange(minValue,
				form.trackBarRangeXmin.Minimum, form.trackBarRangeXmin.Maximum);
			form.trackBarRangeXmax.Value = (int)CUtils.LimitToRange(maxValue,
				form.trackBarRangeXmax.Minimum, form.trackBarRangeXmax.Maximum);

			SetRangeX();

			//Y
			//in project Y = elevation, but in lidar it is Z 
			form.trackBarRangeYmin.SetRange((int)CProjectData.header.Min.Z * 10, (int)CProjectData.header.Max.Z * 10);
			form.trackBarRangeYmax.SetRange((int)CProjectData.header.Min.Z * 10, (int)CProjectData.header.Max.Z * 10);

			minValue = CParameterSetter.GetIntSettings(ESettings.rangeYmin);
			maxValue = CParameterSetter.GetIntSettings(ESettings.rangeYmax);

			form.trackBarRangeYmin.Value = (int)CUtils.LimitToRange(minValue,
				form.trackBarRangeYmin.Minimum, form.trackBarRangeYmin.Maximum);
			form.trackBarRangeYmax.Value = (int)CUtils.LimitToRange(maxValue,
				form.trackBarRangeYmax.Minimum, form.trackBarRangeYmax.Maximum);

			SetRangeY();
		}

		public void trackBarRangeXmax_Scroll(object sender, EventArgs e)
		{
			SetRangeX();
		}

		public void trackBarRangeXmin_Scroll(object sender, EventArgs e)
		{
			SetRangeX();
		}

		public void trackBarRangeYmin_Scroll(object sender, EventArgs e)
		{
			SetRangeY();
		}

		public void trackBarRangeYmax_Scroll(object sender, EventArgs e)
		{
			SetRangeY();
		}
	}
}
