using System;
using System.Collections.Generic;
using System.IO;
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
using Forms = System.Windows.Forms;

namespace BeatapChartMaker
{
    /// <summary>
    /// ConfigWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window
    {
        private List<Tuple<int, int, int>> localcbs = new List<Tuple<int, int, int>>();
        public ConfigWindow()
        {
            InitializeComponent();
            this.Activate();
            this.Topmost = true;
        }

        public void SetValues()
        {
            DWFTBox.Text = ((MainWindow)Owner).DefaultWorkSpacePath;
            localcbs = ((MainWindow)Owner).ColorBrush;
            SingleRSlider.Value = localcbs[0].Item1;
            SingleGSlider.Value = localcbs[0].Item2;
            SingleBSlider.Value = localcbs[0].Item3;
            SingleRValue.Content = localcbs[0].Item1.ToString();
            SingleGValue.Content = localcbs[0].Item2.ToString();
            SingleBValue.Content = localcbs[0].Item3.ToString();
            SingleSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[0].Item1), Convert.ToByte(localcbs[0].Item2), Convert.ToByte(localcbs[0].Item3)));
            LongRSlider.Value = localcbs[1].Item1;
            LongGSlider.Value = localcbs[1].Item2;
            LongBSlider.Value = localcbs[1].Item3;
            LongRValue.Content = localcbs[1].Item1.ToString();
            LongGValue.Content = localcbs[1].Item2.ToString();
            LongBValue.Content = localcbs[1].Item3.ToString();
            LongSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[1].Item1), Convert.ToByte(localcbs[1].Item2), Convert.ToByte(localcbs[1].Item3)));
            LongSampleMidColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[3].Item1), Convert.ToByte(localcbs[3].Item2), Convert.ToByte(localcbs[3].Item3)));
            DoubleRSlider.Value = localcbs[2].Item1;
            DoubleGSlider.Value = localcbs[2].Item2;
            DoubleBSlider.Value = localcbs[2].Item3;
            DoubleRValue.Content = localcbs[2].Item1.ToString();
            DoubleGValue.Content = localcbs[2].Item2.ToString();
            DoubleBValue.Content = localcbs[2].Item3.ToString();
            DoubleSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[2].Item1), Convert.ToByte(localcbs[2].Item2), Convert.ToByte(localcbs[2].Item3)));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(DWFTBox.Text)||DWFTBox.Text == "")
            {
                ((MainWindow)Owner).ColorBrush = localcbs;
                if (!File.Exists(System.IO.Path.GetFullPath("config.ini")))
                {
                    StreamWriter cfs = new StreamWriter(@System.IO.Path.GetFullPath("config.ini"), false, System.Text.Encoding.Default);
                    cfs.Close();
                }
                ReadWriteIni rwIni = new ReadWriteIni(System.IO.Path.GetFullPath("config.ini"));
                rwIni.WriteString("Path", "DefaultWorkSpace", DWFTBox.Text);
                rwIni.WriteString("ColorTheme", "SingleNote", localcbs[0].Item1.ToString()+","+localcbs[0].Item1.ToString()+","+localcbs[0].Item3.ToString());
                rwIni.WriteString("ColorTheme", "LongNote", localcbs[1].Item1.ToString() + "," + localcbs[1].Item1.ToString() + "," + localcbs[1].Item3.ToString());
                rwIni.WriteString("ColorTheme", "DoubleNote", localcbs[2].Item1.ToString() + "," + localcbs[2].Item1.ToString() + "," + localcbs[2].Item3.ToString());
                rwIni.WriteString("ColorTheme", "LongMidNote", localcbs[3].Item1.ToString() + "," + localcbs[3].Item1.ToString() + "," + localcbs[3].Item3.ToString());
                this.Topmost = false;
                if (((MainWindow)Owner)._SelectedChart != null) ((MainWindow)Owner).DrawChartData();
                Owner.Activate();
                this.Close();
            }
            else
            {
                MessageBox.Show("指定されたフォルダが存在しません!", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DWFRefButton_Click(object sender, RoutedEventArgs e)
        {
            Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog();
            fbd.Description = "作業(プロジェクト)フォルダを選択してください";
            if (fbd.ShowDialog() == Forms.DialogResult.OK)
            {
                DWFTBox.Text = fbd.SelectedPath;
            }
        }

        private Tuple<int,int,int> ChangeColorToMid(Tuple<int, int, int> rgb)
        {
            int r = rgb.Item1;
            int g = rgb.Item2;
            int b = rgb.Item3;
            if (r - 40 >= 0) r -= 40;
            else r = 255 - (40 - r);
            if (g - 40 >= 0) g -= 40;
            else g = 255 - (40 - g);
            if (b - 40 >= 0) b -= 40;
            else b = 255 - (40 - b);
            rgb = Tuple.Create<int, int, int>(r,g,b);
            return rgb;
        }

        private void SingleRSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SingleRValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[0] = Tuple.Create<int, int, int>(int.Parse(Math.Round(((Slider)sender).Value,0).ToString()),localcbs[0].Item2,localcbs[0].Item3);
            SingleSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[0].Item1), Convert.ToByte(localcbs[0].Item2), Convert.ToByte(localcbs[0].Item3)));
        }

        private void SingleGSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SingleGValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[0] = Tuple.Create<int, int, int>(localcbs[0].Item1, int.Parse(Math.Round(((Slider)sender).Value, 0).ToString()),localcbs[0].Item3);
            SingleSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[0].Item1), Convert.ToByte(localcbs[0].Item2), Convert.ToByte(localcbs[0].Item3)));
        }

        private void SingleBSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SingleBValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[0] = Tuple.Create<int, int, int>(localcbs[0].Item1, localcbs[0].Item2, int.Parse(Math.Round(((Slider)sender).Value, 0).ToString()));
            SingleSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[0].Item1), Convert.ToByte(localcbs[0].Item2), Convert.ToByte(localcbs[0].Item3)));
        }

        private void LongRSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LongRValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[1] = Tuple.Create<int, int, int>(int.Parse(Math.Round(((Slider)sender).Value, 0).ToString()), localcbs[1].Item2, localcbs[1].Item3);
            localcbs[3] = ChangeColorToMid(localcbs[1]);
            LongSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[1].Item1), Convert.ToByte(localcbs[1].Item2), Convert.ToByte(localcbs[1].Item3)));
            LongSampleMidColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[3].Item1), Convert.ToByte(localcbs[3].Item2), Convert.ToByte(localcbs[3].Item3)));
        }

        private void LongGSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LongGValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[1] = Tuple.Create<int, int, int>(localcbs[1].Item1, int.Parse(Math.Round(((Slider)sender).Value, 0).ToString()), localcbs[1].Item3);
            localcbs[3] = ChangeColorToMid(localcbs[1]);
            LongSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[1].Item1), Convert.ToByte(localcbs[1].Item2), Convert.ToByte(localcbs[1].Item3)));
            LongSampleMidColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[3].Item1), Convert.ToByte(localcbs[3].Item2), Convert.ToByte(localcbs[3].Item3)));
        }

        private void LongBSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LongBValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[1] = Tuple.Create<int, int, int>(localcbs[1].Item1, localcbs[1].Item2, int.Parse(Math.Round(((Slider)sender).Value, 0).ToString()));
            localcbs[3] = ChangeColorToMid(localcbs[1]);
            LongSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[1].Item1), Convert.ToByte(localcbs[1].Item2), Convert.ToByte(localcbs[1].Item3)));
            LongSampleMidColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[3].Item1), Convert.ToByte(localcbs[3].Item2), Convert.ToByte(localcbs[3].Item3)));
        }

        private void DoubleRSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DoubleRValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[2] = Tuple.Create<int, int, int>(int.Parse(Math.Round(((Slider)sender).Value, 0).ToString()), localcbs[2].Item2, localcbs[2].Item3);
            DoubleSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[2].Item1), Convert.ToByte(localcbs[2].Item2), Convert.ToByte(localcbs[2].Item3)));
        }

        private void DoubleGSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DoubleGValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[2] = Tuple.Create<int, int, int>(localcbs[2].Item1, int.Parse(Math.Round(((Slider)sender).Value, 0).ToString()), localcbs[2].Item3);
            DoubleSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[2].Item1), Convert.ToByte(localcbs[2].Item2), Convert.ToByte(localcbs[2].Item3)));
        }

        private void DoubleBSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DoubleBValue.Content = Math.Round(((Slider)sender).Value, 0).ToString();
            localcbs[2] = Tuple.Create<int, int, int>(localcbs[2].Item1, localcbs[2].Item2, int.Parse(Math.Round(((Slider)sender).Value, 0).ToString()));
            DoubleSampleColor.Background = new SolidColorBrush(Color.FromRgb(Convert.ToByte(localcbs[2].Item1), Convert.ToByte(localcbs[2].Item2), Convert.ToByte(localcbs[2].Item3)));
        }
    }
}
