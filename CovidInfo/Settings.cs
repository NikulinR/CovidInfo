using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace CovidInfo
{
    public class Settings
    {
        private JsonParser? jsonParser;
        private Dictionary<DateTime, DayInfo>? init_info;
        private Histogram? histogram;
        private double crit;
        private double defaultCrit;
        public double Crit { get => crit; }
        public double DefaultCrit { get => defaultCrit; }
        public bool updateData { get; set; }
        public DateTime DateFrom { get; set ; }
        public DateTime DateTo { get; set; }
        public DateTime DateMin { get; set; }
        public DateTime DateMax { get; set; }
        public int BinCnt { get; set; }
        public bool UseLaplas { get; set; }
        public bool Shrink { get; set; }
        public string Country { get; set; }
        public Histogram.Parameters Parameter { get; set; }
        public Dictionary<DateTime, DayInfo>? Init_info { get => init_info;  }
        public Histogram? Histogram { get => histogram;  }

        public double alpha = 0.05; 
        public Settings(string country, Utility.Histogram.Parameters param)
        {
            this.Country = country;
            this.Parameter = param;
            updateData = false;
            UseLaplas = false;
            Shrink = false;
            BinCnt = 20;
        }

        public void UpdateCountry(bool updateData = false)
        {
            jsonParser = new JsonParser(Country, updateData);
            init_info = jsonParser.createInfoArray();
            DateMin = init_info.Keys.Min();
            DateMax = init_info.Keys.Max();
        }

        public void Recalc()
        {
            histogram = new Histogram(init_info, BinCnt, DateFrom, DateTo, Parameter);
            crit = histogram.Calculate(UseLaplas, 
                                       shrink: Shrink, 
                                       min: -1,
                                       max: 28,
                                       border:5);

            defaultCrit = MathNet.Numerics.Distributions.ChiSquared.InvCDF(BinCnt - 2, 1 - alpha);
        }
    }
}
