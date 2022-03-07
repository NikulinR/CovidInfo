using System;
using MathNet.Numerics;

namespace Utility
{
	public class Histogram
	{
		public enum Parameters
		{
			Deaths,
			Confirmed,
			Recovered
		}

		public DateTime? dateFrom { get; set; }
		public DateTime? dateTo { get; set; }
		public int intervalCount { get; set; }
		public Parameters cur_parameter { get; set; }
		public List<Interval> intervals { get; set; }
		
		private Dictionary<DateTime, DayInfo> dayInfo;
		private Dictionary<DateTime, DayInfo> cropDayInfo;
		private int minVal, maxVal;
		private float infectionSpan;

		private const float c_0 = 0.3989422804014327f;

		public Histogram(Dictionary<DateTime, DayInfo> dayInfo, int intervalCount, DateTime? dateFrom, DateTime? dateTo, Parameters cur_parameter = Parameters.Confirmed)
        {
            this.dateFrom = dateFrom;
            this.dateTo = dateTo;
            this.cur_parameter = cur_parameter;
            this.intervalCount = intervalCount;
			intervals = new List<Interval>();
			this.dayInfo = dayInfo;
			this.cropDayInfo = dayInfo;
			if (dateFrom != null)
            {
				cropDayInfo = cropDayInfo.Where(item => item.Key >= dateFrom).ToDictionary(item => item.Key, item => item.Value);
            }
			if (dateTo != null)
			{
				cropDayInfo = cropDayInfo.Where(item => item.Key <= dateTo).ToDictionary(item => item.Key, item => item.Value);
			}
		}

		//todo заполнить гистограмму:
		// 1. определить шаг - нужны minmax по числу случаев для DayInfo
		private void getMinMax(Parameters param)
        {
            switch (param)
            {
                case Parameters.Deaths:
					minVal = cropDayInfo.Values.Min(x => x.deathCases);
					maxVal = cropDayInfo.Values.Max(x => x.deathCases);
					break;
                case Parameters.Confirmed:
					minVal = cropDayInfo.Values.Min(x => x.infectedCases);
					maxVal = cropDayInfo.Values.Max(x => x.infectedCases);
					break;
                case Parameters.Recovered:
					minVal = cropDayInfo.Values.Min(x => x.recoveredCases);
					maxVal = cropDayInfo.Values.Max(x => x.recoveredCases);
					break;
                default:
                    break;
            }
        }

		private void Init(Parameters param)
        {
			getMinMax(param);
			infectionSpan = (float)(maxVal - minVal) / intervalCount;
			//init hist
            for (int i = 0; i < intervalCount; i++)
            {
				intervals.Add(new Interval(i,
										   minVal + infectionSpan * i,
										   minVal + infectionSpan * (i + 1),
										   0));
            }
            //fill hist
            foreach (var day in cropDayInfo)
            {
				int index = -1;

				switch (param)
                {
                    case Parameters.Deaths:
						index = intervals.FindIndex(x => x.l_boundary <= day.Value.deathCases &&
													     x.r_boundary >= day.Value.deathCases);
						break;
                    case Parameters.Confirmed:
						index = intervals.FindIndex(x => x.l_boundary <= day.Value.infectedCases &&
														 x.r_boundary >= day.Value.infectedCases);
						break;
                    case Parameters.Recovered:
						index = intervals.FindIndex(x => x.l_boundary <= day.Value.recoveredCases &&
														 x.r_boundary >= day.Value.recoveredCases);
						break;
                    default:
                        break;
                }
                
				if (index >= 0)
                {
					intervals[index].value_actual++;
                }
            }
        }

		private double setTheorFreq(bool useLaplas = true)
        {
			float n_sum = intervals.Sum(x => x.value_actual);
			float xb_avg = intervals.Sum(x => x.value_actual * x.index) / n_sum;

			float h = 1;
			double crit = 0;

			double S = Math.Sqrt(intervals.Sum(x => x.value_actual * Math.Pow(x.index - xb_avg, 2)) / (n_sum - 1));

            if (useLaplas)
            {
                foreach (var interval in intervals)
                {
					double t_1i = (interval.index - h / 2 - xb_avg) / S;
					double t_2i = (interval.index + h / 2 - xb_avg) / S;

					double phi_1 = c_0 * Integrate.DoubleExponential((x => Math.Exp(-x * x / 2)), 0, t_1i);
					double phi_2 = c_0 * Integrate.DoubleExponential((x => Math.Exp(-x * x / 2)), 0, t_2i);

                    interval.value_theor = (float) (n_sum * (phi_2 - phi_1));

					if(interval.value_theor != 0)
                    {
						crit += (float)(Math.Pow(interval.value_actual - interval.value_theor, 2) / interval.value_theor);
					}
				}
            }
            else
            {
				foreach (var interval in intervals)
				{
					double z = (interval.index - xb_avg) / S;
					double phi = c_0 * Math.Exp(-z * z / 2);

					interval.value_theor = (float)(n_sum * h * phi / S);

					if (interval.value_theor != 0)
					{
						crit += (float)(Math.Pow(interval.value_actual - interval.value_theor, 2) / interval.value_theor);
					}
				}
			}
			return crit;
        }

		public double Calculate(Parameters param = Parameters.Confirmed, bool useLaplas = true)
        {
			Init(param);
			return setTheorFreq(useLaplas);
        }
	}
}