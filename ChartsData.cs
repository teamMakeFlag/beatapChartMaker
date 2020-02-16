using System;

namespace BeatapChartMaker
{
    public class ChartsData
    {
        public int ID { get; private set; }
        public String ChartName { get; private set; }
        public String DesignerName { get; private set; }
        public int ChartLevel { get; private set; }
        public ChartsData(int id, String chartname, String designername, int chartlevel)
        {
            this.ID = id;
            this.ChartName = chartname;
            this.DesignerName = designername;
            this.ChartLevel = chartlevel;
        }
    }
}
