using System;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;
using Microsoft.Win32;

namespace BeatapChartMaker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private static String WSP = "WorkSpace\\";
        private string[] ApprovalExt = { ".csv", ".txt" };
        private string[] ApprovalAudioExt = { ".mp3", ".ogg", ".wav", ".aif" };
        private string[] ApprovalImageExt = { ".psd", ".tga", ".tiff", ".png", ".pict", ".jpg", ".jpeg", ".iff", ".hdr", ".gif", ".exr", ".bmp" };
        private String ProjectDirectoryPath = "";
        private String SongName = "";
        private String ArtistName = "";
        private String AudioFilePath = "";
        private String ThumbFilePath = "";
        private String ChartPath = "";
        private String ChartName = "";
        private String ChartJudge = "";
        private String ChartDesignerName = "";
        private int ChartStandardBPM = 0;
        private int ChartLevel = 0;
        private int ChartOffset = 0;
        private List<String> ChartPaths = new List<String>();
        private List<String> ChartNames = new List<String>();
        private List<String> ChartJudges = new List<String>();
        private List<String> ChartDesigners = new List<String>();
        private List<int> ChartStandardBPMs = new List<int>();
        private List<int> ChartLevels = new List<int>();
        private List<int> ChartOffsets = new List<int>();
        private ChartsData _SelectedChart;
        private ChartsData SelectedChart;
        private String DefaultWorkSpacePath = "";
        //      譜面       行数  小節       分子 分母 分割  拍 レーン ノーツ
        private List<Tuple<int, List<Tuple<int, int, int, List<List<int>>>>>> ChartData = new List<Tuple<int, List<Tuple<int, int, int, List<List<int>>>>>>();
        private ChartListDataGrid cldg = new ChartListDataGrid();

        public MainWindow()
        {
            InitializeComponent();
            GetConfigData();
            if (DefaultWorkSpacePath != "") OpenProject(DefaultWorkSpacePath);
            this.DataContext = cldg;
        }

        private void GetConfigData()
        {
            if (File.Exists(System.IO.Path.GetFullPath("config.ini"))){
                ReadWriteIni rwIni = new ReadWriteIni(System.IO.Path.GetFullPath("config.ini"));
                DefaultWorkSpacePath = rwIni.GetStringData("Path", "DefaultWorkSpace", "");
            }
            else
            {
                StreamWriter cfs = new StreamWriter(@System.IO.Path.GetFullPath("config.ini"), false, System.Text.Encoding.Default);
                cfs.Close();
                ReadWriteIni rwIni = new ReadWriteIni(System.IO.Path.GetFullPath("config.ini"));
                rwIni.WriteString("Path", "DefaultWorkSpace", "");
            }
        }

        private void NewProjectButton_Clicked(object sender, RoutedEventArgs e)
        {
            NewProject np = new NewProject();
            np.Owner = this;
            np.Show();
        }
        private void OpenProjectButton_Clicked(object sender, RoutedEventArgs e)
        {
            Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog();
            fbd.Description = "作業(プロジェクト)フォルダを選択してください";
            if (fbd.ShowDialog() == Forms.DialogResult.OK)
            {
                ProjectDirectoryPath = fbd.SelectedPath;
                OpenProject(ProjectDirectoryPath);
            }
        }

        private void ThumbRefButton_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "画像ファイル|*.psd;*.tga;*.tiff;*.png;*.pict;*.jpg;*.jpeg;*.iff;*.hdr;*.gif;*.exr;*.bmp";
            if (fd.ShowDialog() == true) { ThumbFilePath = fd.FileName; ThumbFilePathTBox.Text = fd.FileName; }
        }
        private void AudioRefButton_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "楽曲ファイル|*.mp3;*.ogg;*.wav;*.aif";
            if (fd.ShowDialog() == true) { AudioFilePath = fd.FileName; AudioFilePathTBox.Text = fd.FileName; }
        }

        private void SelectChartButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (SelectedChart != null)
            {
                _SelectedChart = SelectedChart;
                ChartNameTBox.Text = ChartNames[_SelectedChart.ID];
                ChartDesignerNameTBox.Text = ChartDesigners[_SelectedChart.ID];
                ChartJudgeTBox.Text = ChartJudges[_SelectedChart.ID];
                ChartLevelTBox.Text = ChartLevels[_SelectedChart.ID].ToString();
                ChartStandardBPMTBox.Text = ChartStandardBPMs[_SelectedChart.ID].ToString();
                ChartOffsetTBox.Text = ChartOffsets[_SelectedChart.ID].ToString();
                int count = 0;
                for (int i = 0; i < ChartData[_SelectedChart.ID].Item2.Count; i++)
                {
                    for(int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count; j++)
                    {
                        ChartDataGrid.RowDefinitions.Add(new RowDefinition());
                        if (j == 0)
                        {
                            Button label = new Button();
                            label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                            label.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                            String d = ChartData[_SelectedChart.ID].Item2[i].Item2.ToString();
                            String m = ChartData[_SelectedChart.ID].Item2[i].Item1.ToString();
                            String s = ChartData[_SelectedChart.ID].Item2[i].Item3.ToString();
                            label.Content = m + "/" + d;
                            label.Click += (lsender, le) => MeasureButton_Clicked(lsender, d, m, s);
                            Grid.SetColumn(label, 0);
                            Grid.SetRow(label, count);
                            ChartDataGrid.Children.Add(label);
                        }
                        for (int k = 1; k < 6; k++)
                        {
                            Button note = new Button();
                            note.Content = ChartData[_SelectedChart.ID].Item2[i].Item4[j][k-1].ToString();
                            Grid.SetColumn(note, k);
                            Grid.SetRow(note, count);
                            ChartDataGrid.Children.Add(note);
                        }
                        count++;
                    }
                }
            }
        }

        private void DeleteChartButton_Clicked(object sender, RoutedEventArgs e)
        {
            if(SelectedChart != null && Forms.DialogResult.Yes == Forms.MessageBox.Show("選択した譜面\n「"+ChartNames[SelectedChart.ID]+"」を削除してもいいですか？", "確認", Forms.MessageBoxButtons.YesNo, Forms.MessageBoxIcon.Exclamation, Forms.MessageBoxDefaultButton.Button2))
            {
                MessageBox.Show("削除しました");
            }
        }
        private void CreateChartButton_Clicked(object sender, RoutedEventArgs e)
        {
            //
        }

        private void CreateMeasureButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (_SelectedChart != null)
            {
                List<List<int>> measure = new List<List<int>>();
                for (int i = 0; i < int.Parse(sep.Text); i++)
                {
                    List<int>row = new List<int>();
                    ChartDataGrid.RowDefinitions.Add(new RowDefinition());
                    if (i == 0)
                    {
                        Button label = new Button();
                        label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                        label.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        String d = denom.Text;
                        String m = mole.Text;
                        String s = sep.Text;
                        label.Content = m + "/" + d;
                        label.Click += (lsender, le) => MeasureButton_Clicked(lsender, d, m, s);
                        label.Click += (lsender, le) => MeasureButton_Clicked(lsender,denom.Text,mole.Text,sep.Text);
                        Grid.SetColumn(label, 0);
                        Grid.SetRow(label, ChartData[_SelectedChart.ID].Item1);
                        ChartDataGrid.Children.Add(label);
                    }
                    for (int j = 1; j < 6; j++)
                    {
                        Button note = new Button();
                        note.Content = 0;
                        Grid.SetColumn(note, j);
                        Grid.SetRow(note, i + ChartData[_SelectedChart.ID].Item1);
                        ChartDataGrid.Children.Add(note);
                        row.Add(0);
                    }
                    measure.Add(row);
                }
                ChartData[_SelectedChart.ID].Item2.Add(Tuple.Create<int, int, int, List<List<int>>>(int.Parse(denom.Text), int.Parse(mole.Text), int.Parse(sep.Text),measure));
                ChartData[_SelectedChart.ID] = Tuple.Create<int, List<Tuple<int, int, int, List<List<int>>>>>(ChartData[_SelectedChart.ID].Item1 + int.Parse(sep.Text), ChartData[_SelectedChart.ID].Item2);
            }
        }

        private void MeasureButton_Clicked(object sender,String d, String m, String s)
        {
            denom_R.Text = d;
            mole_R.Text = m;
            sep_R.Text = s;
        }

        string CheckExt(string str)
        {
            foreach (string ext in ApprovalExt)
            {
                if (str.Length - ext.Length == str.LastIndexOf(ext)) return ext;
            }
            return "";
        }
        Boolean CheckExt(string[] exts, string str)
        {
            foreach (string ext in exts)
            {
                if (str.Length - ext.Length == str.LastIndexOf(ext)) return true;
            }
            return false;
        }
        private void ClearAllVariable()
        {
            SongName = "";
            ArtistName = "";
            AudioFilePath = "";
            ThumbFilePath = "";
            ChartPath = "";
            ChartName = "";
            ChartJudge = "";
            ChartDesignerName = "";
            ChartStandardBPM = 0;
            ChartLevel = 0;
            ChartOffset = 0;
            ChartPaths = new List<String>();
            ChartNames = new List<String>();
            ChartJudges = new List<String>();
            ChartDesigners = new List<String>();
            ChartStandardBPMs = new List<int>();
            ChartLevels = new List<int>();
            ChartOffsets = new List<int>();
            _SelectedChart = null;
            SelectedChart = null;
        }

        public void OpenProject(String ProjectPath)
        {
            Boolean isMusicTxtRead = false;
            Boolean isAudioFileRead = false;
            Boolean isThumbFileRead = false;
            int chartCount = 0;
            DirectoryInfo directory = new DirectoryInfo(ProjectPath);
            ClearAllVariable();
            cldg.ClearDummy();
            foreach (FileInfo inf in directory.GetFiles())
            {
                string infstr = inf + "";
                if (CheckExt(infstr) == ".txt")
                {
                    if (!isMusicTxtRead)
                    {
                        StreamReader sr = new StreamReader(@ProjectPath + "\\" + infstr, System.Text.Encoding.Default);
                        string value = sr.ReadToEnd();
                        sr.Close();
                        string[] values = value.Split('\n');
                        SongName = values[0];
                        ArtistName = values[1];
                        isMusicTxtRead = true;
                    }
                    else if (isAudioFileRead)
                    {
                        Forms.MessageBox.Show("曲情報が記載されたテキストファイルが2個以上あります!\n修正してください!", "プロジェクト読み込みエラー", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
                else if (CheckExt(ApprovalAudioExt, infstr))
                {
                    if (!isAudioFileRead)
                    {
                        AudioFilePath = infstr;
                        isAudioFileRead = true;
                    }
                    else if (isAudioFileRead)
                    {
                        Forms.MessageBox.Show("楽曲ファイルが2個以上あります!\n修正してください!", "プロジェクト読み込みエラー", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
                else if (CheckExt(infstr) == ".csv")
                {
                    ChartPaths.Add(infstr);
                    StreamReader sr = new StreamReader(@ProjectPath + "\\" + infstr, System.Text.Encoding.Default);
                    string value = sr.ReadToEnd();
                    sr.Close();
                    string[] values = value.Split(',');
                    if (values.Length < 6)
                    {
                        Forms.MessageBox.Show("譜面ファイルに不正なフォーマットが使われています!\n修正または削除してください!\nError occurred in " + infstr + " file.", "プロジェクト読み込みエラー", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
                        return;
                    }
                    ChartPaths.Add(infstr);
                    ChartNames.Add(values[0]);
                    ChartLevels.Add(int.Parse(values[1]));
                    ChartDesigners.Add(values[2]);
                    ChartStandardBPMs.Add(int.Parse(values[3]));
                    ChartOffsets.Add(int.Parse(values[4]));
                    ChartJudges.Add(values[5]);
                    int i = 12;
                    int measure_d = 0;
                    int measure_m = 0;
                    int separate = 0;
                    int timeIndex = 1;
                    int max_timeIndex = 0;
                    int rcount = 0;
                    List<Tuple<int, int, int, List<List<int>>>> chartData = new List<Tuple<int, int, int, List<List<int>>>>();
                    List<List<int>> measureData = new List<List<int>>();
                    while (!values[i].Contains("END"))
                    {
                        if(measure_d == 0)
                        {
                            measure_m = int.Parse(values[i]);
                            measure_d = int.Parse(values[i + 1]);
                            separate = int.Parse(values[i + 2]);
                            rcount += separate;
                            max_timeIndex = separate;
                            i += 5;
                        }
                        else
                        {
                            List<int> timeData = new List<int>();
                            timeData.Add(int.Parse(values[i]));
                            timeData.Add(int.Parse(values[i+1]));
                            timeData.Add(int.Parse(values[i+2]));
                            timeData.Add(int.Parse(values[i+3]));
                            timeData.Add(int.Parse(values[i+4]));
                            measureData.Add(timeData);
                            i += 5;
                            if(timeIndex == max_timeIndex) {
                                chartData.Add(Tuple.Create<int, int, int, List<List<int>>>(measure_m, measure_d, separate, measureData));
                                measureData = new List<List<int>>();
                                measure_d = 0;
                                measure_m = 0;
                                separate = 0;
                                max_timeIndex = 0;
                                timeIndex = 1;
                            }
                            else timeIndex++;
                        }
                        i++;
                    }
                    ChartData.Add(Tuple.Create<int, List<Tuple<int, int, int, List<List<int>>>>>(rcount, chartData));
                    cldg.AddChart(chartCount, ChartNames[(int)chartCount], ChartDesigners[(int)chartCount], ChartLevels[(int)chartCount]);
                    chartCount++;
                }
                else if (CheckExt(ApprovalImageExt, infstr))
                {
                    if (!isThumbFileRead)
                    {
                        ThumbFilePath = infstr;
                        isThumbFileRead = true;
                    }
                    else if (isThumbFileRead)
                    {
                        Forms.MessageBox.Show("画像ファイルが2個以上あります!\n修正してください!", "プロジェクト読み込みエラー", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            SongNameTBox.Text = SongName;
            ArtistNameTBox.Text = ArtistName;
            AudioFilePathTBox.Text = AudioFilePath;
            ThumbFilePathTBox.Text = ThumbFilePath;
            cldg.SetCharts();
            this.Title = "Beatap譜面エディタ - " + ProjectPath.Substring(ProjectPath.LastIndexOf('\\')+1, ProjectPath.Length - ProjectPath.LastIndexOf('\\') - 1);
        }

        private void ChartLevelTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9,]").IsMatch(e.Text);
        }

        private void ChartOffsetTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void ChartStandardBPMTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void denom_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void mole_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void sep_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void SongNameTBox_Changed(object sender, TextChangedEventArgs e)
        {
            SongName = SongNameTBox.Text.ToString();
        }

        private void ArtistNameTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ArtistName = ArtistNameTBox.Text.ToString();
        }

        private void ThumbFilePathTBox_Changed(object sender, TextChangedEventArgs e)
        {
            if (!File.Exists(ThumbFilePathTBox.Text.ToString())) ThumbFilePath = "";
            else ThumbFilePath = ThumbFilePathTBox.Text.ToString();
        }

        private void AudioFilePathTBox_Changed(object sender, TextChangedEventArgs e)
        {
            if (!File.Exists(AudioFilePathTBox.Text.ToString())) AudioFilePath = "";
            else AudioFilePath = AudioFilePathTBox.Text.ToString();
        }

        private void ChartDesignerNameTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartDesignerName = ChartDesignerNameTBox.Text.ToString();
        }

        private void ChartNameTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartName = ChartNameTBox.Text.ToString();
        }

        private void ChartJudgeTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartJudge = ChartJudgeTBox.Text.ToString();
        }

        private void ChartLevelTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartLevel = Convert.ToInt32(ChartLevelTBox.Text.ToString());
        }

        private void ChartOffsetTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartOffset = Convert.ToInt32(ChartOffsetTBox.Text.ToString());
        }

        private void ChartStandardBPMTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartStandardBPM = Convert.ToInt32(ChartStandardBPMTBox.Text.ToString());
        }

        private void ChartListGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectedChart = (ChartsData)chartListGrid.SelectedItem;
        }
    }
}
