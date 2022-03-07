using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Utility
{
    public class JsonParser
    {
        private readonly bool update;
        private string apiUrl = "https://api.covid19api.com/dayone/country/"
        public string Country { get; }
        public JObject jResponse;

        public JsonParser(string country, bool update = false)
        {
            Country = country;
            this.update = update;
        }

        public void Init()
        {
            string path = String.Format(@".\JSON\{country}Data.json", Country);
            if (!File.Exists(path) || update)
            {
                var webRequest = WebRequest.Create(String.Concat(apiUrl, Country)) as HttpWebRequest;
                if (webRequest is null)
                {
                    throw new Exception(String.Format("No data for {Country}", Country));
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
                jResponse = JObject.Parse(sr.ReadToEnd());
            }

        }

    }
}