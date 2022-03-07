using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Utility
{
    public class JsonParser
    {
        private readonly bool update;
        private string apiUrl = "https://api.covid19api.com/dayone/country/";
        public string Country { get; }
        public JArray? jResponse;

        public JsonParser(string country, bool update = false)
        {
            Country = country;
            this.update = update;

            this.Init();
        }

        public void Init()
        {
            string path = String.Format("{0}/{1}Data.json", System.AppDomain.CurrentDomain.BaseDirectory, Country);
            if (!File.Exists(path) || update)
            {
                var webRequest = WebRequest.Create(String.Concat(apiUrl, Country)) as HttpWebRequest;
                if (webRequest is null)
                {
                    throw new Exception(String.Format("No data for {0}", Country));
                }

                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "Nothing";

                using (var s = webRequest.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        using (StreamWriter sw = File.CreateText(path))
                        {
                            var contributorsAsJson = sr.ReadToEnd();
                            sw.Write(contributorsAsJson);
                        }
                    }
                }
            }
            using (StreamReader sr = File.OpenText(path))
            {
                var resp = sr.ReadToEnd();
                jResponse = JArray.Parse(resp);
            }
        }

        public Dictionary<DateTime, DayInfo> createInfoArray()
        {
            Dictionary<DateTime, DayInfo> info = new Dictionary<DateTime, DayInfo>();

            var jOneDay = this.jResponse[0];
            DayInfo last = new DayInfo();
            while (jOneDay != null)
            {
                last = new DayInfo(ref last,
                                   int.Parse(jOneDay["Deaths"].ToString()),
                                   int.Parse(jOneDay["Confirmed"].ToString()),
                                   int.Parse(jOneDay["Recovered"].ToString()),
                                   DateTime.Parse(jOneDay["Date"].ToString()));
                info.Add(last.date, last);

                jOneDay = jOneDay.Next;
            }

            return info;
        }
    }
}