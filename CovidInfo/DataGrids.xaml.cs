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
using System.Windows.Shapes;

namespace CovidInfo
{
    /// <summary>
    /// Interaction logic for DataGrids.xaml
    /// </summary>
    public partial class DataGrids : Window
    {
        MainWindow mainWindow;
        public DataGrids()
        {
            InitializeComponent();
            
        }

        public void Init()
        {
            mainWindow = this.Owner as MainWindow;
        }

        public void Update()
        {
            GridSummary.ItemsSource = (from item in mainWindow.set.Init_info
                                       where item.Key >= mainWindow.set.DateFrom & item.Key <= mainWindow.set.DateTo
                                       select new { Дата = item.Key.ToString("dd MMM yyyy"), Заражено = item.Value.infectedCases, Умерло = item.Value.deathCases, Умерло_100 = Math.Round(item.Value.deathCases / 53.67580, 2), Выздоровело = item.Value.recoveredCases });
            
            GridHist.ItemsSource = (from item in mainWindow.set.Histogram.intervals
                                    select new {Индекс = item.index, 
                                                Правая_граница = Math.Round(item.l_boundary, 2), 
                                                Левая_граница = Math.Round(item.r_boundary, 2),
                                                Эмпирические_частоты = Math.Round(item.value_actual, 2),
                                                Теоретические_частоты = Math.Round(item.value_theor, 2)});
            GridHist.Columns[0].Header = "Индекс";
            GridHist.Columns[1].Header = "Правая граница";
            GridHist.Columns[2].Header = "Левая граница";
            GridHist.Columns[3].Header = "Эмпирические частоты";
            GridHist.Columns[4].Header = "Теоретические частоты";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainWindow.dg = null;
        }
    }
}
