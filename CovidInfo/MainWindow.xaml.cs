using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        ScottPlot.Plottable.ScatterPlot plotSummary;
        ScottPlot.Plottable.VLine lineDateFrom;
        ScottPlot.Plottable.VLine lineDateTo;
        ScottPlot.Plottable.Annotation summaryAnnotation;
        ScottPlot.Plottable.Crosshair chSummary;
        ScottPlot.Plottable.ScatterPlot plotHist;
        ScottPlot.Plottable.ScatterPlot plotHistTheor;
        ScottPlot.Plottable.Crosshair chHist;
        ScottPlot.Plottable.Crosshair chHistTheor;
        System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CreateSpecificCulture("ru");
        BinaryFormatter formatter = new BinaryFormatter();
        OpenFileDialog openFileDialog = new OpenFileDialog();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        enum ModeChangeDate
        {
            Nothing,
            DateFrom,
            DateTo
        }

        ModeChangeDate modeChangeDate = ModeChangeDate.Nothing;

        public void drawSummary()
        {
            WpfPlotCount.Plot.Clear();
            WpfPlotCount.Plot.SetCulture(culture);
            WpfPlotCount.Plot.XAxis.DateTimeFormat(true);
            int max = int.MaxValue;
            switch (set.Parameter)
            {
                case Utility.Histogram.Parameters.Deaths:
                    plotSummary = WpfPlotCount.Plot.AddScatter(set.Init_info.Keys.Select(x => x.ToOADate()).ToArray(),
                                    set.Init_info.Values.Select(x => (double)x.deathCases).ToArray(),
                                    markerSize: 0);
                    max = set.Init_info.Values.Max(x => x.deathCases);
                    param = "Смерти";
                    break;
                case Utility.Histogram.Parameters.Confirmed:
                    plotSummary = WpfPlotCount.Plot.AddScatter(set.Init_info.Keys.Select(x => x.ToOADate()).ToArray(),
                                    set.Init_info.Values.Select(x => (double)x.infectedCases).ToArray(),
                                    markerSize: 0);
                    max = set.Init_info.Values.Max(x => x.infectedCases);
                    param = "Заражения";
                    break;
                case Utility.Histogram.Parameters.Recovered:
                    plotSummary = WpfPlotCount.Plot.AddScatter(set.Init_info.Keys.Select(x => x.ToOADate()).ToArray(),
                                    set.Init_info.Values.Select(x => (double)x.recoveredCases).ToArray(),
                                    markerSize: 0);
                    max = set.Init_info.Values.Max(x => x.recoveredCases);
                    param = "Выздоровления";
                    break;
                default:
                    break;
            }
            summaryAnnotation = WpfPlotCount.Plot.AddAnnotation("", 0, 0);

            lineDateFrom = WpfPlotCount.Plot.AddVerticalLine(set.DateFrom.ToOADate(), color: System.Drawing.Color.Red, width: 4);
            lineDateTo = WpfPlotCount.Plot.AddVerticalLine(set.DateTo.ToOADate(), color: System.Drawing.Color.Red, width: 4);
            WpfPlotCount.Plot.YAxis.Label(param);
            WpfPlotCount.Plot.SetOuterViewLimits(yMin: 0, yMax: max, xMin: set.DateMin.ToOADate(), xMax: set.DateMax.ToOADate());
            WpfPlotCount.Plot.XAxis.Label("Дата");
            WpfPlotCount.Plot.XAxis2.Label(String.Format("Статистика по {0}", set.countries.FirstOrDefault(x => x.Value == set.Country).Key));
            WpfPlotCount.Refresh();
        }

        public void drawHist()
        {
            WpfPlotHist.Plot.Clear();
            WpfPlotHist.Plot.SetCulture(culture);

            //WpfPlotHist.Plot.AddBar(set.Histogram.intervals.Select(x => (double)x.value_actual).ToArray());
            plotHistTheor = WpfPlotHist.Plot.AddScatter(set.Histogram.intervals.Select(x => (double)x.index).ToArray(),
                                         set.Histogram.intervals.Select(x => (double)x.value_theor).ToArray(),
                                         markerSize: 5, 
                                         color: System.Drawing.Color.Red,
                                         lineWidth: 4,
                                         label: "Теоретические частоты");

            plotHist = WpfPlotHist.Plot.AddScatterPoints(set.Histogram.intervals.Select(x => (double)x.index).ToArray(),
                                         set.Histogram.intervals.Select(x => (double)x.value_actual).ToArray(),
                                         markerSize: 10,
                                         color: System.Drawing.Color.Blue,
                                         markerShape: ScottPlot.MarkerShape.cross,
                                         label: "Эмпирические частоты");

            WpfPlotHist.Plot.YAxis.Label("Частота");
            WpfPlotHist.Plot.XAxis.Label("Индекс");
            WpfPlotHist.Plot.XAxis.ManualTickSpacing(1);
            WpfPlotHist.Plot.SetOuterViewLimits(yMin: 0, xMin: -0.8, xMax: set.Histogram.intervalCount);
            WpfPlotHist.Plot.XAxis2.Label(String.Format("Статистика по {0}", set.countries.FirstOrDefault(x => x.Value == set.Country).Key));
            WpfPlotHist.Plot.AddAnnotation(String.Format("Xi^2 = {0}", Math.Round(set.Crit), 4), 0, 0);
            WpfPlotHist.Plot.AddAnnotation(String.Format("Xi^2 критическое = {0}", Math.Round(set.DefaultCrit), 4), 0, 20);
            WpfPlotHist.Plot.Legend(location: ScottPlot.Alignment.UpperRight);
            WpfPlotHist.Refresh();
        }


        private void updateSettings()
        {
            DateFromPicker.SelectedDate = set.DateFrom;
            DateToPicker.SelectedDate = set.DateTo;
            //case type
            switch (set.Parameter)
            {
                case Histogram.Parameters.Confirmed:
                    cbParam.Text = "Зараженные";
                    break;
                case Histogram.Parameters.Deaths:
                    cbParam.Text = "Умершие";
                    break;
                case Histogram.Parameters.Recovered:
                    cbParam.Text = "Выздоровевшие";
                    break;
                default:
                    break;
            }
            //country
            cbCountry.Text = set.countries.FirstOrDefault(x => x.Value == set.Country).Key;
            //column_count
            NumBins.Text = set.BinCnt.ToString();
            //laplas
            cbLaplas.IsChecked = set.UseLaplas;
            //shrink
            cbShrink.IsChecked = set.Shrink;
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


            this.cbCountry.ItemsSource = set.countries.Keys;
            this.cbCountry.Text = "Brazil";
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

            if(lineDateFrom is not null)
            {
                lineDateFrom.DateTime = set.DateFrom;
            }            
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

            if(lineDateTo is not null)
            {
                lineDateTo.DateTime = set.DateTo;
            }            
        }

        private void recalc()
        {
            try
            {
                set.UpdateCountry();
                set.Recalc();
                drawSummary();
                drawHist();
                if(NumSkips is not null)
                {
                    set.Crit = set.Histogram.GetCrit(set.Skip);
                }
                if (dg is not null)
                    dg.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRecalc_Click(object sender, RoutedEventArgs e)
        {
            chSummary = null;
            chHist = null;
            chHistTheor = null;
            recalc();
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
            if (set is not null & this.cbCountry is not null & cbCountry.SelectedItem is not null)
            {
                set.Country = set.countries[cbCountry.SelectedValue.ToString()];
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

        private void NumSkips_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NumSkips is null | set is null)
            {
                return;
            }
            int skips = set.Skip;
            if (!int.TryParse(NumSkips.Text, out skips))
                NumSkips.Text = skips.ToString();
            set.Skip = skips;
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


        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            saveFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            saveFileDialog.FileName = $"{set.Country}_{set.Parameter.ToString()}_{set.DateFrom.ToString("yy-MM")} - {set.DateTo.ToString("yy-MM")}";
            Nullable<bool> result = saveFileDialog.ShowDialog();
            string fileName;
            if (result == true)
            {
                fileName = saveFileDialog.FileName;
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, set);

                    Console.WriteLine("Объект сериализован");
                }
            }
        }

        private void MenuItemLoad_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            Nullable<bool> result = openFileDialog.ShowDialog();
            string fileName;
            if (result == true)
            {
                fileName = openFileDialog.FileName;
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    set = (Settings)formatter.Deserialize(fs);
                    updateSettings();
                    recalc();
                }
            }
        }

        private void WpfPlotCount_MouseMove(object sender, MouseEventArgs e)
        {
            (double mouseCoordX, double mouseCoordY) = WpfPlotCount.GetMouseCoordinates();
            double xyRatio = WpfPlotCount.Plot.XAxis.Dims.PxPerUnit / WpfPlotCount.Plot.YAxis.Dims.PxPerUnit;
            (double pointX, double pointY, int pointIndex) = plotSummary.GetPointNearestX(mouseCoordX);

            if (chSummary is null)
            {
                chSummary = WpfPlotCount.Plot.AddCrosshair(pointX, pointY);
                chSummary.Color = plotSummary.Color;
                chSummary.VerticalLine.PositionFormatter = pos => DateTime.FromOADate(pos).ToString("d");
                chSummary.HorizontalLine.PositionFormatter = pos => ((int)pos).ToString();
            }
            else
            {
                chSummary.X = pointX;
                chSummary.Y = pointY;
            }

            switch (modeChangeDate)
            {                
                case ModeChangeDate.DateFrom:
                    DateFromPicker.Text = DateTime.FromOADate(pointX).ToString("d");
                    break;
                case ModeChangeDate.DateTo:
                    DateToPicker.Text = DateTime.FromOADate(pointX).ToString("d");
                    break;
                default:
                    break;
            }

            WpfPlotCount.Refresh(lowQuality: true);
        }

        private void WpfPlotHist_MouseMove(object sender, MouseEventArgs e)
        {
            (double mouseCoordX, double mouseCoordY) = WpfPlotHist.GetMouseCoordinates();
            (double pointX, double pointY, int pointIndex) = plotHist.GetPointNearestX(mouseCoordX);
            if (chHist is null) {
                chHist = WpfPlotHist.Plot.AddCrosshair(pointX, pointY);
                chHist.Color = plotHist.Color;
                chHist.VerticalLine.PositionFormatter = pos => ((int)pos).ToString();
                chHist.HorizontalLine.PositionFormatter = pos => ((int)pos).ToString();
            } 
            else
            {
                chHist.X = pointX;
                chHist.Y = pointY;
            }

            (double pointXTheor, double pointYTheor, int pointIndexTheor) = plotHistTheor.GetPointNearestX(mouseCoordX);
            if (chHistTheor is null)
            {
                chHistTheor = WpfPlotHist.Plot.AddCrosshair(pointXTheor, pointYTheor);
                chHistTheor.Color = plotHistTheor.Color;
                chHistTheor.VerticalLine.PositionFormatter = pos => ((int)pos).ToString();
                chHistTheor.HorizontalLine.PositionFormatter = pos => ((int)pos).ToString();
            }
            else
            {
                chHistTheor.X = pointXTheor;
                chHistTheor.Y = pointYTheor;
            }

            WpfPlotHist.Refresh();
        }

        private void WpfPlotCount_MouseLeave(object sender, MouseEventArgs e)
        {
            if (chSummary is not null)
            {
                chSummary.IsVisible = false;
            }
            modeChangeDate = ModeChangeDate.Nothing;

            WpfPlotCount.Refresh();
        }

        private void WpfPlotCount_MouseEnter(object sender, MouseEventArgs e)
        {
            if (chSummary is not null)
            {
                chSummary.IsVisible = true;
            }

            WpfPlotCount.Refresh();
        }

        private void WpfPlotHist_MouseLeave(object sender, MouseEventArgs e)
        {
            if (chHist is not null)
            {
                chHist.IsVisible = false;
                chHistTheor.IsVisible = false;
            }

            WpfPlotHist.Refresh();
        }

        private void WpfPlotHist_MouseEnter(object sender, MouseEventArgs e)
        {
            if (chHist is not null)
            {
                chHist.IsVisible = true;
                chHistTheor.IsVisible = true;
            }

            WpfPlotHist.Refresh();
        }

        private void WpfPlotCount_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                (double mouseCoordX, double mouseCoordY) = WpfPlotCount.GetMouseCoordinates();
                (double pointX, double pointY, int pointIndex) = plotSummary.GetPointNearestX(mouseCoordX);
                DateTime date = DateTime.FromOADate(pointX);

                var fromDifference = date - set.DateFrom;
                var toDifference = date - set.DateTo;

                if (Math.Abs(fromDifference.TotalDays) > Math.Abs(toDifference.TotalDays))
                    modeChangeDate = ModeChangeDate.DateTo;
                else
                    modeChangeDate = ModeChangeDate.DateFrom;
            }
        }

        private void WpfPlotCount_MouseUp(object sender, MouseButtonEventArgs e)
        {
            modeChangeDate = ModeChangeDate.Nothing;
        }
    }
}
