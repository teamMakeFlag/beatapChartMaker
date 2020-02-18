using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BeatapChartMaker
{
    /// <summary>
    /// NewChartWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class NewChartWindow : Window
    {
        public NewChartWindow()
        {
            InitializeComponent();
            this.Activate();
            this.Topmost = true;
            judgecombo.Items.Add("easy");
            judgecombo.Items.Add("normal");
            judgecombo.Items.Add("hard");
            judgecombo.Items.Add("gambol");
        }

        private void ChartLevelTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void ChartStandardBPMTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void ChartOffsetTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void CreateChartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChartNameTBox.Text != "" && DesignerNameTBox.Text != "" && ChartLevelTBox.Text != "" && ChartOffsetTBox.Text != "" && ChartStandardBPMTBox.Text != "")
            {
                StreamWriter cfs = new StreamWriter(((MainWindow)this.Owner).DefaultWorkSpacePath+"\\"+ChartNameTBox.Text+".csv", false, System.Text.Encoding.Default);
                cfs.Write(ChartNameTBox.Text + "," + ChartLevelTBox.Text + "," + DesignerNameTBox.Text + "," + ChartStandardBPMTBox.Text + ","+ChartOffsetTBox.Text+"," + judgecombo.SelectedValue + ",,\n");
                cfs.Write("START,,,,,,,\n");
                cfs.Write("END,,,,,,,\n");
                cfs.Close();
                this.Topmost = false;
                this.Owner.Activate();
                ((MainWindow)this.Owner).LoadChart(((MainWindow)this.Owner).DefaultWorkSpacePath + "\\" + ChartNameTBox.Text + ".csv");
                this.Close();
            }
            else
            {
                ErrorLabel.Content = "すべての項目を埋めてください!";
            }
        }
    }
}
