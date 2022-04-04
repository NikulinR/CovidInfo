// See https://aka.ms/new-console-template for more information
using Utility;

JsonParser jsonParser = new JsonParser("brazil");
Dictionary<string, string> data = jsonParser.getCountries();
Histogram histogram = new Histogram(jsonParser.createInfoArray(), 20, DateTime.Parse("2020-06-21"), DateTime.Parse("2020-10-07"), Histogram.Parameters.Confirmed);
double crit = histogram.Calculate();

Console.WriteLine("Hello, World!");
