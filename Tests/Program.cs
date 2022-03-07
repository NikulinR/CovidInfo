// See https://aka.ms/new-console-template for more information
using Utility;

JsonParser jsonParser = new JsonParser("brazil");
Histogram histogram = new Histogram(jsonParser.createInfoArray(), 20, DateTime.Parse("2020-06-21"), DateTime.Parse("2020-10-07"), Histogram.Parameters.Confirmed);


Console.WriteLine("Hello, World!");
