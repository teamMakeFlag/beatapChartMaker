using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace BeatapChartMaker
{
    public class ChartListDataGrid
    {
        public ObservableCollection<ChartsData> Charts { get; set; }
        private ObservableCollection<ChartsData> DummyCharts { get; set; }
        public ChartListDataGrid()
        {
            Charts = new ObservableCollection<ChartsData> { };
            DummyCharts = new ObservableCollection<ChartsData> { };
        }
        public void AddChart(int id, String chartname, String designername, int chartlevel)
        {
            ChartsData chart = new ChartsData{ID=id, ChartName=chartname, DesignerName=designername, ChartLevel=chartlevel};
            DummyCharts.Add(chart);
        }

        public void SetCharts()
        {
            Charts = null;
            Charts = DummyCharts;
        }

        public void ClearDummy()
        {
            DummyCharts.Clear();
        }
    }
}
