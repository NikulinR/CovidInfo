using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Utility
{
    [Serializable]
    public class JsonParser
    {
        private readonly bool update;
        private string apiUrl = "https://api.covid19api.com/dayone/country/";
        public string Country { get; }
        [NonSerialized]
        public JArray? jResponse;
        [NonSerialized]
        public JArray? jCountries;

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
            try {
                var jOneDay = this.jResponse[0];
                DayInfo last = new DayInfo();


                if (Country == "china") {
                    DateTime curDate = DateTime.Parse(jOneDay["Date"].ToString());
                    int deaths = 0;
                    int confirmed = 0;
                    int recovered = 0;
                    
                    while (jOneDay != null)
                    {
                        if(DateTime.Parse(jOneDay["Date"].ToString()) == curDate)
                        {
                            deaths += int.Parse(jOneDay["Deaths"].ToString());
                            confirmed += int.Parse(jOneDay["Confirmed"].ToString());
                            recovered += int.Parse(jOneDay["Recovered"].ToString());
                        }
                        else
                        {
                            last = new DayInfo(ref last,
                                               deaths,
                                               confirmed,
                                               recovered,
                                               curDate);
                            info.Add(last.date, last);
                            deaths = int.Parse(jOneDay["Deaths"].ToString());
                            confirmed = int.Parse(jOneDay["Confirmed"].ToString());
                            recovered = int.Parse(jOneDay["Recovered"].ToString());

                            curDate = DateTime.Parse(jOneDay["Date"].ToString());
                        }
                        
                        jOneDay = jOneDay.Next;
                    }
                }
                else
                {
                    while (jOneDay != null)
                    {
                        if (jOneDay["Province"].ToString() == String.Empty)
                        {
                            last = new DayInfo(ref last,
                                               int.Parse(jOneDay["Deaths"].ToString()),
                                               int.Parse(jOneDay["Confirmed"].ToString()),
                                               int.Parse(jOneDay["Recovered"].ToString()),
                                               DateTime.Parse(jOneDay["Date"].ToString()));
                            info.Add(last.date, last);
                        }

                        jOneDay = jOneDay.Next;
                    }
                }
                

            }
            catch (Exception ex)
            {
                throw new Exception("No data found.", ex);
            }

            
            return info;
        }

        public Dictionary<string, string> getCountries()
        {
            Dictionary<string, string> countries = new Dictionary<string, string>();

            string path = String.Format("{0}/countryData.json", System.AppDomain.CurrentDomain.BaseDirectory);
            if (!File.Exists(path) || update)
            {
                var webRequest = WebRequest.Create("https://api.covid19api.com/countries") as HttpWebRequest;
                if (webRequest is null)
                {
                    throw new Exception(String.Format("No data for countries"));
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
                jCountries = JArray.Parse(resp);
            }


            var jCountry = this.jCountries[0];
            string sysName, userName;
            while (jCountry != null)
            {
                
                userName = jCountry["Country"].ToString();
                sysName = jCountry["Slug"].ToString();
                countries.Add(userName, sysName);

                jCountry = jCountry.Next;
            }

            return countries;
        }
    }
}