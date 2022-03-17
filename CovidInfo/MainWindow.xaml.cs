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
        public Settings set;
        string param;
        public DataGrids? dg;
        System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CreateSpecificCulture("ru");

        public void drawSummary()
        {
            WpfPlotCount.Plot.Clear();
            WpfPlotCount.Plot.SetCulture(culture);
            WpfPlotCount.Plot.XAxis.DateTimeFormat(true);
            switch (set.Parameter)
            {
                case Utility.Histogram.Parameters.Deaths:
                    WpfPlotCount.Plot.AddScatter(set.Init_info.Keys.Select(x => x.ToOADate()).ToArray(),
                                    set.Init_info.Values.Select(x => (double)x.deathCases).ToArray(),
                                    markerSize: 0);
                    param = "Смерти";
                    break;
                case Utility.Histogram.Parameters.Confirmed:
                    WpfPlotCount.Plot.AddScatter(set.Init_info.Keys.Select(x => x.ToOADate()).ToArray(),
                                    set.Init_info.Values.Select(x => (double)x.infectedCases).ToArray(),
                                    markerSize: 0);
                    param = "Заражения";
                    break;
                case Utility.Histogram.Parameters.Recovered:
                    WpfPlotCount.Plot.AddScatter(set.Init_info.Keys.Select(x => x.ToOADate()).ToArray(),
                                    set.Init_info.Values.Select(x => (double)x.recoveredCases).ToArray(),
                                    markerSize: 0);
                    param = "Выздоровления";
                    break;
                default:
                    break;
            }
            WpfPlotCount.Plot.AddVerticalLine(set.DateFrom.ToOADate(), color: System.Drawing.Color.Red, width: 4);
            WpfPlotCount.Plot.AddVerticalLine(set.DateTo.ToOADate(), color: System.Drawing.Color.Red, width: 4);
            WpfPlotCount.Plot.YAxis.Label(param);
            WpfPlotCount.Plot.SetOuterViewLimits(yMin: 0, xMin: set.DateMin.ToOADate(), xMax: set.DateMax.ToOADate());
            WpfPlotCount.Plot.XAxis.Label("Дата");
            WpfPlotCount.Plot.XAxis2.Label(String.Format("Статистика по {0}", set.Country));
            
            WpfPlotCount.Refresh();
        }

        public void drawHist()
        {
            WpfPlotHist.Plot.Clear();
            WpfPlotHist.Plot.SetCulture(culture);

            //WpfPlotHist.Plot.AddBar(set.Histogram.intervals.Select(x => (double)x.value_actual).ToArray());
            WpfPlotHist.Plot.AddScatter(set.Histogram.intervals.Select(x => (double)x.index).ToArray(),
                                         set.Histogram.intervals.Select(x => (double)x.value_theor).ToArray(),
                                         markerSize: 5, 
                                         color: System.Drawing.Color.Red,
                                         lineWidth: 4);
            WpfPlotHist.Plot.AddScatter(set.Histogram.intervals.Select(x => (double)x.index).ToArray(),
                                         set.Histogram.intervals.Select(x => (double)x.value_actual).ToArray(),
                                         markerSize: 10,
                                         color: System.Drawing.Color.Blue,
                                         markerShape: ScottPlot.MarkerShape.cross,
                                         lineWidth: 2);

            WpfPlotHist.Plot.YAxis.Label("Частота");
            WpfPlotHist.Plot.XAxis.Label("Индекс");
            WpfPlotHist.Plot.SetOuterViewLimits(yMin: 0, xMin: -0.8);
            WpfPlotHist.Plot.XAxis2.Label(String.Format("Статистика по {0}", set.Country));
            WpfPlotHist.Plot.AddAnnotation(String.Format("Xi^2 = {0}", Math.Round(set.Crit), 4), 0, 0);
            WpfPlotHist.Plot.AddAnnotation(String.Format("Xi^2 критическое = {0}", Math.Round(set.DefaultCrit), 4), 0, 20);
            WpfPlotHist.Refresh();
        }


        public MainWindow()
        {
            InitializeComponent();

            set = new Settings("brazil", Histogram.Parameters.Confirmed);
            set.UpdateCountry();
            set.Recalc();

            DateFromPicker.SelectedDate = set.DateMin;
            DateToPicker.SelectedDate = set.DateMax;

            drawSummary();
            drawHist();
        }

        private void DateFromPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateFromPicker.SelectedDate <= DateTime.MinValue)
            {
                set.DateFrom = set.DateMin;
                DateFromPicker.SelectedDate = set.DateMin;
                return;
            }
            
            if (DateFromPicker.SelectedDate > set.DateTo & DateToPicker.SelectedDate <= DateTime.MinValue)
            {
                DateFromPicker.SelectedDate = DateToPicker.SelectedDate;
                return;
            }

            set.DateFrom = DateFromPicker.SelectedDate ?? set.DateMin;
            drawSummary();
        }

        private void DateToPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateToPicker.SelectedDate <= DateTime.MinValue)
            {
                set.DateTo = set.DateMax;
                DateToPicker.SelectedDate = set.DateMax;
                return;
            }
            if (DateToPicker.SelectedDate < set.DateFrom & DateToPicker.SelectedDate <= DateTime.MinValue)
            {
                DateToPicker.SelectedDate = DateFromPicker.SelectedDate;
                return;
            }

            set.DateTo = DateToPicker.SelectedDate ?? set.DateMax;
            drawSummary();
        }

        private void btnRecalc_Click(object sender, RoutedEventArgs e)
        {
            set.UpdateCountry();
            set.Recalc();
            drawSummary();
            drawHist();
            if (dg is not null)
                dg.Update();
        }

        private void cbParam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (set is not null)
            {
                ComboBoxItem item = cbParam.SelectedItem as ComboBoxItem;
                switch (item.Content)
                {
                    case "Зараженные":
                        set.Parameter = Histogram.Parameters.Confirmed;
                        break;
                    case "Умершие":
                        set.Parameter = Histogram.Parameters.Deaths;
                        break;
                    case "Выздоровевшие":
                        set.Parameter = Histogram.Parameters.Recovered;
                        break;
                    default:
                        break;
                }
            }
        }

        private void cbCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (set is not null)
            {
                ComboBoxItem item = cbCountry.SelectedItem as ComboBoxItem;
                set.Country = item.Content.ToString().ToLower();
            }
        }

        private void NumBins_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NumBins is null | set is null)
            {
                return;
            }
            int bincnt = set.BinCnt;
            if (!int.TryParse(NumBins.Text, out bincnt))
                NumBins.Text = bincnt.ToString();
            if (bincnt > 2)
                set.BinCnt = bincnt;
        }

        private void btnShowGrid_Click(object sender, RoutedEventArgs e)
        {
            if (dg is null)
            {
                dg = new DataGrids();
                dg.Owner = this;
                dg.Init();
            }
            dg.Show();
            dg.Update();
        }

        private void cbLaplas_Checked(object sender, RoutedEventArgs e)
        {
            set.UseLaplas = true;
        }

        private void cbLaplas_UnChecked(object sender, RoutedEventArgs e)
        {
            set.UseLaplas = false;
        }

        private void cbShrink_Checked(object sender, RoutedEventArgs e)
        {
            set.Shrink = true;
        }

        private void cbShrink_UnChecked(object sender, RoutedEventArgs e)
        {
            set.Shrink = false;
        }
    }
}
