using System;
using System.Numerics;

namespace ForestReco
{
	public class CUiRangeController
	{
		private CMainForm form;

		public CUiRangeController(CMainForm pForm)
		{
			form = pForm;
		}

		private void SetRangeX()
		{			
			CParameterSetter.SetParameter(ESettings.rangeXmin, form.trackBarRangeXmin.Value);
			CParameterSetter.SetParameter(ESettings.rangeXmax, form.trackBarRangeXmax.Value);

			SSplitRange range = CParameterSetter.GetSplitRange();
			form.textRangeX.Text = range.ToStringX();
		}

		private void SetRangeY()
		{
			CParameterSetter.SetParameter(ESettings.rangeYmin, form.trackBarRangeYmin.Value);
			CParameterSetter.SetParameter(ESettings.rangeYmax, form.trackBarRangeYmax.Value);

			SSplitRange range = CParameterSetter.GetSplitRange();
			form.textRangeY.Text = range.ToStringY();
		}

		public void UpdateRangeBounds()
		{
			//X
			//range has to match file coordinates
			//in project are used coordinates moved by offset
			Vector3 min = CProjectData.header.Min_orig;
			Vector3 max = CProjectData.header.Max_orig;
			form.trackBarRangeXmin.SetRange((int)min.X * 10, (int)max.X * 10);
			form.trackBarRangeXmax.SetRange((int)min.X * 10, (int)max.X * 10);

			int minValue = CParameterSetter.GetIntSettings(ESettings.rangeXmin);
			int maxValue = CParameterSetter.GetIntSettings(ESettings.rangeXmax);

			form.trackBarRangeXmin.Value = 
				minValue.LimitToRange(form.trackBarRangeXmin.Minimum, form.trackBarRangeXmin.Maximum);
			form.trackBarRangeXmax.Value = CUtils.LimitToRange(maxValue,
				form.trackBarRangeXmax.Minimum, form.trackBarRangeXmax.Maximum);

			SetRangeX();

			//Y
			//note: in project Y = elevation, but in lidar it is Z 
			form.trackBarRangeYmin.SetRange((int)min.Y * 10, (int)max.Y * 10);
			form.trackBarRangeYmax.SetRange((int)min.Y * 10, (int)max.Y * 10);

			minValue = CParameterSetter.GetIntSettings(ESettings.rangeYmin);
			maxValue = CParameterSetter.GetIntSettings(ESettings.rangeYmax);

			//WARNING: when float is used there is a possible value overflow
			form.trackBarRangeYmin.Value = CUtils.LimitToRange(minValue,
				form.trackBarRangeYmin.Minimum, form.trackBarRangeYmin.Maximum);
			form.trackBarRangeYmax.Value = CUtils.LimitToRange(maxValue,
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
