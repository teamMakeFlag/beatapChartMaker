using System;
using System.Collections.Generic;
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
    /// EditFunctionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EditFunctionWindow : Window
    {
        private int measure;
        private int time;
        public EditFunctionWindow()
        {
            InitializeComponent();
            this.Activate();
            this.Topmost = true;
            ModeComboBox.Items.Add("");
            ModeComboBox.Items.Add("BPMCHANGE");
            ModeComboBox.Items.Add("SCROLL");
        }

        public void Load(int m, int t) {
            ModeComboBox.SelectedValue = ((MainWindow)this.Owner).FunctionMode;
            ValueTBox.Text = ((MainWindow)this.Owner).FunctionValue;
            measure = m;
            time = t;
        }

        private void ValueTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModeComboBox.SelectedValue == null)
            {
                ((MainWindow)this.Owner).FunctionMode = "";
            }
            else
            {
                ((MainWindow)this.Owner).FunctionMode = ModeComboBox.SelectedValue.ToString();
            }
            ((MainWindow)this.Owner).FunctionValue = ValueTBox.Text;
            this.Topmost = false;
            ((MainWindow)this.Owner).Activate();
            ((MainWindow)this.Owner).SaveFunction(measure, time);
            this.Close();
        }
    }
}
