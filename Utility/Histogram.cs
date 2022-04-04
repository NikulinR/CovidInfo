using System;
using MathNet.Numerics;

namespace Utility
{
	[Serializable]
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
			this.dayInfo = dayInfo;
			this.cropDayInfo = dayInfo;

			if (!dateFrom.HasValue | dateFrom <= DateTime.MinValue)
				this.dateFrom = dayInfo.Keys.Min();
			else this.dateFrom = dateFrom;

			if (!dateTo.HasValue | dateTo <= DateTime.MinValue)
				this.dateTo = dayInfo.Keys.Max();
			else this.dateTo = dateTo;

			if (this.dateFrom > this.dateTo)
            {
				this.dateFrom = dayInfo.Keys.Min();
				this.dateTo = dayInfo.Keys.Max();
			}

			cropDayInfo = cropDayInfo.Where(item => item.Key >= this.dateFrom).ToDictionary(item => item.Key, item => item.Value);
			cropDayInfo = cropDayInfo.Where(item => item.Key <= this.dateTo).ToDictionary(item => item.Key, item => item.Value);				

            this.cur_parameter = cur_parameter;
            this.intervalCount = intervalCount;
			intervals = new List<Interval>();
			
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

		private void Init(int min = -1, int max = -1)
        {
            if (min > -1 && max > -1)
            {
				this.minVal = min;
				this.maxVal = max;
			}
			else
				getMinMax(cur_parameter);

			infectionSpan = (float)(maxVal - minVal + 1) / intervalCount;
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
				bool zero = false;
				switch (cur_parameter)
                {
                    case Parameters.Deaths:
						index = intervals.FindIndex(x => x.l_boundary <= day.Value.deathCases &&
													     x.r_boundary > day.Value.deathCases);
						zero = day.Value.deathCases == 0;
						break;
                    case Parameters.Confirmed:
						index = intervals.FindIndex(x => x.l_boundary <= day.Value.infectedCases &&
														 x.r_boundary > day.Value.infectedCases);
						zero = day.Value.infectedCases == 0;
						break;
                    case Parameters.Recovered:
						index = intervals.FindIndex(x => x.l_boundary <= day.Value.recoveredCases &&
														 x.r_boundary > day.Value.recoveredCases);
						zero = day.Value.recoveredCases == 0;
						break;
                    default:
                        break;
                }
                
				if (index >= 0 //& (!zero | true)
					)
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

                    interval.value_theor = (int)Math.Round(n_sum * (phi_2 - phi_1));

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

					interval.value_theor = (int)Math.Round(n_sum * h * phi / S);

					if (interval.value_theor != 0)
					{
						crit += (float)(Math.Pow(interval.value_actual - interval.value_theor, 2) / interval.value_theor);
					}
				}
			}
			return crit;
        }

		public double Calculate(bool useLaplas = true, int min = -1, int max = -1, bool shrink = false, int border = 5)
        {
			Init(min, max);
            if (shrink)
            {
				intervals = Shrink(border);
			}
			return setTheorFreq(useLaplas);
        }

		public List<Interval> Shrink(int border = 5)
		{
			List<Interval> ret_intervals = new List<Interval>();

			Interval? frame_interval = null;

			int ret_index = 0;
			bool skipMode = false;

			for (int i = 0; i < intervals.Count(); i++)
            {
				skipMode = intervals[i].value_actual < border;

                if (!skipMode)
                {
					if (frame_interval is not null) //выход из режима сжатия
					{
						frame_interval.index = ret_index;
						ret_index++;
						ret_intervals.Add(frame_interval);
						frame_interval = null;
					}
					Interval cur_interval = intervals[i];
					cur_interval.index = ret_index;
					ret_index++;
					ret_intervals.Add(cur_interval);
				}
                else
                {
                    if (frame_interval is not null) //в режиме сжатия
                    {
						frame_interval.r_boundary = intervals[i].r_boundary;
						frame_interval.value_actual += intervals[i].value_actual;
					}
                    else //вход в режим сжатия
                    {
						frame_interval = intervals[i];
					}

					if(i == intervals.Count() - 1)
                    {
						ret_intervals.Add(frame_interval);
					}
                } 
			}
			return ret_intervals;
		}
	}
}