using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Utility;

namespace CovidInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            JsonParser jsonParser = new JsonParser("brazil");
            Dictionary<DateTime, DayInfo> init_info = jsonParser.createInfoArray();
            Histogram histogram = new Histogram(init_info, 20, DateTime.Parse("2020-06-21"), DateTime.Parse("2020-10-07"), Histogram.Parameters.Confirmed);
            double crit = histogram.Calculate();
            WpfPlotCount.Plot.AddScatter(init_info.Keys.Select(x => x.ToOADate()).ToArray(), 
                                         init_info.Values.Select(x => (double)x.infectedCases).ToArray(), 
                                         markerSize: 0);
            var culture = System.Globalization.CultureInfo.CreateSpecificCulture("ru");
            WpfPlotCount.Plot.SetCulture(culture);

            WpfPlotCount.Plot.XAxis.DateTimeFormat(true);
            WpfPlotCount.Plot.YAxis.Label("Cases");
            WpfPlotCount.Plot.XAxis.Label("Date");
            WpfPlotCount.Plot.XAxis2.Label("Статистика по Бразилии");

            WpfPlotCount.Refresh();

        }
    }
}
