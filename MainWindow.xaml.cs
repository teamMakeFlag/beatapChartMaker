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
using Microsoft.VisualBasic.FileIO;
using System.Collections.ObjectModel;

namespace BeatapChartMaker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] ApprovalExt = { ".csv", ".txt" };
        private string[] ApprovalAudioExt = { ".mp3", ".ogg", ".wav", ".aif" };
        private string[] ApprovalImageExt = { ".psd", ".tga", ".tiff", ".png", ".pict", ".jpg", ".jpeg", ".iff", ".hdr", ".gif", ".exr", ".bmp" };
        private String ProjectDirectoryPath = "";
        private String SongName = "";
        private String ArtistName = "";
        private String AudioFilePath = "";
        private String SavedAudioFilePath = "";
        private String ThumbFilePath = "";
        private String SavedThumbFilePath = "";
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
        private List<List<Tuple<String, String>>> ChartsOptions = new List<List<Tuple<String, String>>>();
        private List<List<List<Button>>> DButtons = new List<List<List<Button>>>();
        private List<int> ChartStandardBPMs = new List<int>();
        private List<int> ChartLevels = new List<int>();
        private List<int> ChartOffsets = new List<int>();
        private ChartsData _SelectedChart;
        private ChartsData SelectedChart;
        private Tuple<String, String> SelectedTimeOption;
        private Tuple<String, String> SelectedChartOption;
        private int EdittingID = -1;
        public String DefaultWorkSpacePath = "";
        public String FunctionMode = "";
        public String FunctionValue = "";
        //      譜面       行数  小節       分子 分母 分割  拍 レーン ノーツ
        private List<Tuple<int, List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>>>> ChartData = new List<Tuple<int, List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>>>>();
        readonly List<ChartsData> cdlist = new List<ChartsData>();
        readonly List<Tuple<String, String>> tolist = new List<Tuple<string, string>>();
        readonly List<Tuple<String, String>> colist = new List<Tuple<string, string>>();
        List<Tuple<int, int, int>> StartLongNotePoint = new List<Tuple<int, int, int>>();
        List<Tuple<int, int, int>> FinishLongNotePoint = new List<Tuple<int, int, int>>();
        private int SelectedMeasureIndex = -1;
        private int PenMode = -1;
        private bool IsLongNoteMode = false;
        private Tuple<int, int, int> FirstLongModeSelected = null;

        public MainWindow()
        {
            InitializeComponent();
            ChartOptionsComboBox.Items.Add("TOTAL");
            ChartOptionsComboBox.Items.Add("SCROLL");
            ChartOptionsComboBox.Items.Add("HOSEI_RATE");
            ChartOptionsComboBox.Items.Add("HOSEI_BORDER");
            ChartOptionsComboBox.Items.Add("START_GAUGE");
            ChartOptionsComboBox.Items.Add("GAUGE_TYPE");
            TimeOptionsComboBox.Items.Add("BPMCHANGE");
            TimeOptionsComboBox.Items.Add("SCROLL");
            ActionSelectComboBox.Items.Add("末尾に小節を作成");
            ActionSelectComboBox.Items.Add("選択中の小節の前に小節を作成");
            ActionSelectComboBox.Items.Add("選択中の小節の後ろに小節を作成");
            ActionSelectComboBox.Items.Add("選択中の小節のノーツを削除");
            ActionSelectComboBox.Items.Add("選択中の小節を削除");
            ActionSelectComboBox.Items.Add("選択中の小節をコピー");
            ActionSelectComboBox.Items.Add("選択中の小節の前にコピーした小節を貼り付け");
            ActionSelectComboBox.Items.Add("選択中の小節の後ろにコピーした小節を貼り付け");
            GetConfigData();
            if (DefaultWorkSpacePath != "") OpenProject(DefaultWorkSpacePath);
        }

        private void UpdateDataList()
        {
            chartListGrid.ItemsSource = new ReadOnlyCollection<ChartsData>(cdlist);
        }

        private void UpdateTODataList()
        {
            TimeOptionsGrid.ItemsSource = new ReadOnlyCollection<Tuple<String, String>>(tolist);
        }

        private void UpdateCODataList()
        {
            ChartOptionsGrid.ItemsSource = new ReadOnlyCollection<Tuple<String, String>>(colist);
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
                ChartDataGrid.Children.Clear();
                SelectedChart = null;
                _SelectedChart = null;
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
                EdittingID = _SelectedChart.ID;
                ChartNameTBox.Text = ChartNames[_SelectedChart.ID];
                ChartDesignerNameTBox.Text = ChartDesigners[_SelectedChart.ID];
                ChartJudgeTBox.Text = ChartJudges[_SelectedChart.ID];
                ChartLevelTBox.Text = ChartLevels[_SelectedChart.ID].ToString();
                ChartStandardBPMTBox.Text = ChartStandardBPMs[_SelectedChart.ID].ToString();
                ChartOffsetTBox.Text = ChartOffsets[_SelectedChart.ID].ToString();
                for(int i = 0; i < ChartsOptions[_SelectedChart.ID].Count(); i++)
                {
                    colist.Add(ChartsOptions[_SelectedChart.ID][i]);
                }
                DrawChartData();
                UpdateCODataList();
            }
        }

        private void DeleteChartButton_Clicked(object sender, RoutedEventArgs e)
        {
            if(SelectedChart != null && Forms.DialogResult.Yes == Forms.MessageBox.Show("選択した譜面\n「"+ChartNames[SelectedChart.ID]+"」を削除してもいいですか？", "確認", Forms.MessageBoxButtons.YesNo, Forms.MessageBoxIcon.Exclamation, Forms.MessageBoxDefaultButton.Button2))
            {
                if (EdittingID == SelectedChart.ID)
                {
                    ChartDataGrid.Children.Clear();
                    ChartName = "";
                    ChartJudge = "";
                    ChartDesignerName = "";
                    ChartStandardBPM = 0;
                    ChartLevel = 0;
                    ChartOffset = 0;
                    EdittingID = -1;
                    SelectedMeasureIndex = -1;
                    PenMode = -1;
                    ChartNameTBox.Text = "";
                    ChartJudgeTBox.Text = "";
                    ChartDesignerNameTBox.Text = "";
                    ChartStandardBPMTBox.Text = "";
                    ChartLevelTBox.Text = "";
                    ChartOffsetTBox.Text = "";
                    denom.Text = "";
                    denom_R.Text = "";
                    mole.Text = "";
                    mole_R.Text = "";
                    sep.Text = "";
                    sep_R.Text = "";
                    _SelectedChart = null;
                }
                try
                {
                    FileSystem.DeleteFile(System.IO.Path.GetFullPath(ChartPaths[SelectedChart.ID]),Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    MessageBox.Show("削除対象がすでに移動もしくは削除されたようです\n" + ex.Message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                MessageBox.Show("譜面は正常に削除されました.\n元に戻したいときはごみ箱よりどうぞ!","システムメッセージ");
                cdlist.RemoveAt(SelectedChart.ID);
                SelectedChart = null;
                UpdateDataList();
            }
        }
        private void CreateChartButton_Clicked(object sender, RoutedEventArgs e)
        {
            NewChartWindow ncw = new NewChartWindow();
            ncw.Owner = this;
            ncw.Show();
            UpdateDataList();
        }

        private void MeasureButton_Clicked(object sender,String d, String m, String s, int index)
        {
            denom_R.Text = d;
            mole_R.Text = m;
            sep_R.Text = s;
            SelectedMeasureIndex = index-1;
            SelectedTimeTBox.Text = "";
            SelectedTimeOption = null;
            tolist.Clear();
            UpdateTODataList();
            SelectedMeasureIndexTBox.Text = (SelectedMeasureIndex+1).ToString();
        }

        private void DoActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionSelectComboBox.SelectedValue.ToString() != "")
            {
                if (ActionSelectComboBox.SelectedValue.ToString() == "末尾に小節を作成")
                {
                    if (_SelectedChart != null)
                    {
                        List<List<Button>> meab = new List<List<Button>>();
                        List<Button> timb = new List<Button>();
                        List<Tuple<List<int>, List<Tuple<String, String>>>> measure = new List<Tuple<List<int>, List<Tuple<string, string>>>>();
                        for (int i = 0; i < int.Parse(sep.Text); i++)
                        {
                            List<int> row = new List<int>();
                            timb = new List<Button>();
                            ChartDataGrid.RowDefinitions.Add(new RowDefinition());
                            int index = ChartData[_SelectedChart.ID].Item2.Count;
                            if (i == 0)
                            {
                                Button label = new Button();
                                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                                label.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220));
                                String d = denom.Text;
                                String m = mole.Text;
                                String s = sep.Text;
                                label.Content = index + 1;
                                label.Click += (lsender, le) => MeasureButton_Clicked(lsender, d, m, s, index + 1);
                                Grid.SetColumn(label, 0);
                                Grid.SetRow(label, ChartData[_SelectedChart.ID].Item1);
                                ChartDataGrid.Children.Add(label);
                            }
                            else
                            {
                                Label label = new Label();
                                label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                                label.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                                label.Content = " ";
                                Grid.SetColumn(label, 0);
                                Grid.SetRow(label, ChartData[_SelectedChart.ID].Item1 + i);
                                ChartDataGrid.Children.Add(label);
                            }
                            for (int j = 1; j < 6; j++)
                            {
                                Button note = new Button();
                                note.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                                int t = i;
                                int l = j - 1;
                                note.Click += (lsender, le) => NoteButton_Clicked(lsender, index, t, l);
                                Grid.SetColumn(note, j);
                                Grid.SetRow(note, i + ChartData[_SelectedChart.ID].Item1);
                                ChartDataGrid.Children.Add(note);
                                timb.Add(note);
                                row.Add(0);
                            }
                            measure.Add(Tuple.Create<List<int>, List<Tuple<String, String>>>(row, new List<Tuple<string, string>>()));
                            Button func = new Button();
                            String time = (i + 1).ToString();
                            int mea = ChartData[_SelectedChart.ID].Item2.Count;
                            int tm = i;
                            func.Content = time;
                            func.Click += (lsender, le) => FunctionButton_Click(lsender, mea, tm);
                            Grid.SetColumn(func, 6);
                            Grid.SetRow(func, i + ChartData[_SelectedChart.ID].Item1);
                            ChartDataGrid.Children.Add(func);
                            meab.Add(timb);
                        }
                        DButtons.Add(meab);
                        ChartData[_SelectedChart.ID].Item2.Add(Tuple.Create<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>(int.Parse(denom.Text), int.Parse(mole.Text), int.Parse(sep.Text), measure));
                        ChartData[_SelectedChart.ID] = Tuple.Create<int, List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>>>(ChartData[_SelectedChart.ID].Item1 + int.Parse(sep.Text), ChartData[_SelectedChart.ID].Item2);
                    }
                }
                else if (ActionSelectComboBox.SelectedValue.ToString() == "選択中の小節の前に小節を作成")
                {
                    if (_SelectedChart != null && SelectedMeasureIndex != -1)
                    {
                        List<Tuple<List<int>, List<Tuple<String, String>>>> measure = new List<Tuple<List<int>, System.Collections.Generic.List<Tuple<string, string>>>>();
                        for (int i = 0; i < int.Parse(sep.Text); i++)
                        {
                            List<int> time = new List<int>();
                            for (int j = 0; j < 5; j++) time.Add(0);
                            measure.Add(Tuple.Create<List<int>, List<Tuple<String, String>>>(time, new List<Tuple<string, string>>()));
                        }
                        ChartData[_SelectedChart.ID].Item2.Insert(SelectedMeasureIndex, Tuple.Create<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>(int.Parse(denom.Text), int.Parse(mole.Text), int.Parse(sep.Text), measure));
                        ChartData[_SelectedChart.ID] = Tuple.Create<int, List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>>>(ChartData[_SelectedChart.ID].Item1 + int.Parse(sep.Text), ChartData[_SelectedChart.ID].Item2);
                        DrawChartData();
                    }
                }
                else if (ActionSelectComboBox.SelectedValue.ToString() == "選択中の小節の後ろに小節を作成")
                {
                    if (_SelectedChart != null && SelectedMeasureIndex != -1)
                    {
                        List<Tuple<List<int>, List<Tuple<String, String>>>> measure = new List<Tuple<List<int>, List<Tuple<String, String>>>>();
                        for (int i = 0; i < int.Parse(sep.Text); i++)
                        {
                            List<int> time = new List<int>();
                            for (int j = 0; j < 5; j++) time.Add(0);
                            measure.Add(Tuple.Create<List<int>, List<Tuple<String, String>>>(time, new List<Tuple<String, String>>()));
                        }
                        ChartData[_SelectedChart.ID].Item2.Insert(SelectedMeasureIndex + 1, Tuple.Create<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>(int.Parse(denom.Text), int.Parse(mole.Text), int.Parse(sep.Text), measure));
                        ChartData[_SelectedChart.ID] = Tuple.Create<int, List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>>>(ChartData[_SelectedChart.ID].Item1 + int.Parse(sep.Text), ChartData[_SelectedChart.ID].Item2);
                        DrawChartData();
                    }
                }
                else if (ActionSelectComboBox.SelectedValue.ToString() == "選択中の小節のノーツを削除")
                {
                    if (_SelectedChart != null && SelectedMeasureIndex != -1)
                    {
                        for (int i = 0; i < ChartData[_SelectedChart.ID].Item2[SelectedMeasureIndex].Item4.Count; i++)
                        {
                            for (int j = 0; j < 5; j++)
                            {
                                ChartData[_SelectedChart.ID].Item2[SelectedMeasureIndex].Item4[i].Item1[j] = 0;
                            }
                        }
                        DrawChartData();
                    }
                }
                else if (ActionSelectComboBox.SelectedValue.ToString() == "選択中の小節を削除")
                {
                    if (_SelectedChart != null && SelectedMeasureIndex != -1)
                    {
                        int ds = ChartData[_SelectedChart.ID].Item2[SelectedMeasureIndex].Item3;
                        ChartData[_SelectedChart.ID].Item2.RemoveAt(SelectedMeasureIndex);
                        ChartData[_SelectedChart.ID] = Tuple.Create<int, List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>>>(ChartData[_SelectedChart.ID].Item1 - ds, ChartData[_SelectedChart.ID].Item2);
                        SelectedMeasureIndex = -1;
                        SelectedMeasureIndexTBox.Text = "";
                        denom_R.Text = "";
                        mole_R.Text = "";
                        sep_R.Text = "";
                        DrawChartData();
                    }
                }
                else if (ActionSelectComboBox.SelectedValue.ToString() == "選択中の小節をコピー")
                {

                }
                else if (ActionSelectComboBox.SelectedValue.ToString() == "選択中の小節の前にコピーした小節を貼り付け")
                {

                }
                else if (ActionSelectComboBox.SelectedValue.ToString() == "選択中の小節の後ろにコピーした小節を貼り付け")
                {

                }
            }
        }

        private void NoteButton_Clicked(object sender, int measure, int time, int lane)
        {
            if (IsLongNoteMode)
            {
                if(!(FirstLongModeSelected.Item1 == measure && FirstLongModeSelected.Item2 == time ) && FirstLongModeSelected.Item3 == lane)
                {
                    bool isOK = true;
                    if(FirstLongModeSelected.Item1<measure || (FirstLongModeSelected.Item1 == measure && FirstLongModeSelected.Item2 < time))
                    {
                        for(int i = FirstLongModeSelected.Item1; i <= measure; i++)
                        {
                            for(int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count(); j++)
                            {
                                if (i == FirstLongModeSelected.Item1 && j == 0) j = FirstLongModeSelected.Item2+1;
                                if (i == measure && j == time) break;
                                else if (DButtons[i][j][lane].Background == new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 210, 131)))
                                {
                                    isOK = false;
                                }
                            }
                        }
                        if (isOK)
                        {
                            for (int i = FirstLongModeSelected.Item1; i <= measure; i++)
                            {
                                for (int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count(); j++)
                                {
                                    if (i == FirstLongModeSelected.Item1 && j == 0) j = FirstLongModeSelected.Item2 + 1;
                                    if (i == measure && j == time) break;
                                    else
                                    {
                                        DButtons[i][j][lane].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 170, 91));
                                    }
                                }
                            }
                            ChartData[_SelectedChart.ID].Item2[measure].Item4[time].Item1[lane] = 2;
                            DButtons[measure][time][lane].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 210, 131));
                            StartLongNotePoint.Add(FirstLongModeSelected);
                            FinishLongNotePoint.Add(Tuple.Create<int,int,int>(measure,time,lane));
                            IsLongNoteMode = false;
                            FirstLongModeSelected = null;
                        }
                    }
                    else
                    {
                        for (int i = measure; i <= FirstLongModeSelected.Item1; i++)
                        {
                            for (int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count(); j++)
                            {
                                if (i == measure && j == 0) j = time + 1;
                                if (i == FirstLongModeSelected.Item1 && j == FirstLongModeSelected.Item2) break;
                                else if (DButtons[i][j][lane].Background == new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 210, 131)))
                                {
                                    isOK = false;
                                }
                            }
                        }
                        if (isOK)
                        {
                            for (int i = measure; i <= FirstLongModeSelected.Item1; i++)
                            {
                                for (int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count(); j++)
                                {
                                    if (i == measure && j == 0) j = time + 1;
                                    if (i == FirstLongModeSelected.Item1 && j == FirstLongModeSelected.Item2) break;
                                    else
                                    {
                                        DButtons[i][j][lane].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 170, 91));
                                    }
                                }
                            }
                            ChartData[_SelectedChart.ID].Item2[measure].Item4[time].Item1[lane] = 2;
                            DButtons[measure][time][lane].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 210, 131));
                            StartLongNotePoint.Add(Tuple.Create<int, int, int>(measure, time, lane));
                            FinishLongNotePoint.Add(FirstLongModeSelected);
                            IsLongNoteMode = false;
                            FirstLongModeSelected = null;
                        }
                    }
                }
            }
            else if (PenMode != -1)
            {
                switch (PenMode)
                {
                    case 0:
                        int sindex = StartLongNotePoint.IndexOf(Tuple.Create<int, int, int>(measure, time, lane));
                        int findex = FinishLongNotePoint.IndexOf(Tuple.Create<int, int, int>(measure, time, lane));
                        if (sindex > -1){
                            for (int i = StartLongNotePoint[sindex].Item1; i <= FinishLongNotePoint[sindex].Item1; i++)
                            {
                                for (int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count(); j++)
                                {
                                    if (StartLongNotePoint[sindex].Item1 == FinishLongNotePoint[sindex].Item1 && j == 0) j = StartLongNotePoint[sindex].Item2 + 1;
                                    if (StartLongNotePoint[sindex].Item1 == FinishLongNotePoint[sindex].Item1 && j == FinishLongNotePoint[sindex].Item2) break;
                                    else
                                    {
                                        DButtons[i][j][lane].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                                    }
                                }
                            }
                            ChartData[_SelectedChart.ID].Item2[StartLongNotePoint[sindex].Item1].Item4[StartLongNotePoint[sindex].Item2].Item1[StartLongNotePoint[sindex].Item3] = 0;
                            ChartData[_SelectedChart.ID].Item2[FinishLongNotePoint[sindex].Item1].Item4[FinishLongNotePoint[sindex].Item2].Item1[FinishLongNotePoint[sindex].Item3] = 0;
                            DButtons[StartLongNotePoint[sindex].Item1][StartLongNotePoint[sindex].Item2][StartLongNotePoint[sindex].Item3].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                            DButtons[FinishLongNotePoint[sindex].Item1][FinishLongNotePoint[sindex].Item2][FinishLongNotePoint[sindex].Item3].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        }
                        else if(findex >-1){
                            for (int i = StartLongNotePoint[findex].Item1; i <= FinishLongNotePoint[findex].Item1; i++)
                            {
                                for (int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count(); j++)
                                {
                                    if (StartLongNotePoint[findex].Item1 == FinishLongNotePoint[findex].Item1 && j == 0) j = StartLongNotePoint[findex].Item2 + 1;
                                    if (StartLongNotePoint[findex].Item1 == FinishLongNotePoint[findex].Item1 && j == FinishLongNotePoint[findex].Item2) break;
                                    else
                                    {
                                        DButtons[i][j][lane].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                                    }
                                }
                            }
                            ChartData[_SelectedChart.ID].Item2[StartLongNotePoint[findex].Item1].Item4[StartLongNotePoint[findex].Item2].Item1[StartLongNotePoint[findex].Item3] = 0;
                            ChartData[_SelectedChart.ID].Item2[FinishLongNotePoint[findex].Item1].Item4[FinishLongNotePoint[findex].Item2].Item1[FinishLongNotePoint[findex].Item3] = 0;
                            DButtons[StartLongNotePoint[findex].Item1][StartLongNotePoint[findex].Item2][StartLongNotePoint[findex].Item3].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                            DButtons[FinishLongNotePoint[findex].Item1][FinishLongNotePoint[findex].Item2][FinishLongNotePoint[findex].Item3].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        }
                        else {
                            ((Button)sender).Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        }
                        break;
                    case 1:
                        ((Button)sender).Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 54, 255));
                        break;
                    case 2:
                        ((Button)sender).Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 210, 131));
                        IsLongNoteMode = true;
                        FirstLongModeSelected = Tuple.Create<int, int, int>(measure, time,lane);
                        break;
                    case 3:
                        ((Button)sender).Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 13, 13));
                        break;
                    default:
                        break;
                }
                ChartData[_SelectedChart.ID].Item2[measure].Item4[time].Item1[lane] = PenMode;
            }
        }

        private void SaveMusicDataButton_Click(object sender, RoutedEventArgs e)
        {
            StreamWriter fs = new StreamWriter(@DefaultWorkSpacePath + "\\music.txt", false, System.Text.Encoding.Default);
            fs.Write(SongName + "\n");
            fs.Write(ArtistName);
            fs.Close();
        }

        private void FunctionButton_Click(object sender, int measure, int time)
        {
            denom_R.Text = ChartData[_SelectedChart.ID].Item2[measure].Item2.ToString();
            mole_R.Text = ChartData[_SelectedChart.ID].Item2[measure].Item1.ToString();
            sep_R.Text = ChartData[_SelectedChart.ID].Item2[measure].Item3.ToString();
            SelectedMeasureIndex = measure;
            SelectedMeasureIndexTBox.Text = (SelectedMeasureIndex + 1).ToString();
            SelectedTimeTBox.Text = (time + 1).ToString();
            SelectedTimeTBox.Text = (time + 1).ToString();
            tolist.Clear();
            for(int i=0;i< ChartData[_SelectedChart.ID].Item2[measure].Item4[time].Item2.Count(); i++)
            {
                tolist.Add(Tuple.Create<String, String>(ChartData[_SelectedChart.ID].Item2[measure].Item4[time].Item2[i].Item1, ChartData[_SelectedChart.ID].Item2[measure].Item4[time].Item2[i].Item2));
            }
            UpdateTODataList();
        }

        private void SetPenModeSingleNote_Click(object sender, RoutedEventArgs e)
        {
            if (IsLongNoteMode)
            {
                DButtons[FirstLongModeSelected.Item1][FirstLongModeSelected.Item2][FirstLongModeSelected.Item3].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                IsLongNoteMode = false;
                FirstLongModeSelected = null;
            }
            PenMode = 1;
        }

        private void SetPenModeLongNote_Click(object sender, RoutedEventArgs e)
        {
            PenMode = 2;
        }

        private void SetPenModeDoubleNote_Click(object sender, RoutedEventArgs e)
        {
            if (IsLongNoteMode)
            {
                DButtons[FirstLongModeSelected.Item1][FirstLongModeSelected.Item2][FirstLongModeSelected.Item3].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                IsLongNoteMode = false;
                FirstLongModeSelected = null;
            }
            PenMode = 3;
        }

        private void SetPenModeEraseNote_Click(object sender, RoutedEventArgs e)
        {
            if (IsLongNoteMode)
            {
                DButtons[FirstLongModeSelected.Item1][FirstLongModeSelected.Item2][FirstLongModeSelected.Item3].Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                IsLongNoteMode = false;
                FirstLongModeSelected = null;
            }
            PenMode = 0;
        }
        
        private void SaveChartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_SelectedChart != null)
            {
                StreamWriter cfs = new StreamWriter(@ChartPaths[_SelectedChart.ID], false, System.Text.Encoding.Default);
                cfs.Write(ChartName + "," + ChartLevel.ToString() + "," + ChartDesignerName + "," + ChartStandardBPM.ToString() + "," + ChartOffset + "," + ChartJudge + "\n");
                for(int i = 0; i < ChartsOptions[_SelectedChart.ID].Count(); i++)
                {
                    cfs.Write(ChartsOptions[_SelectedChart.ID][i].Item1+","+ ChartsOptions[_SelectedChart.ID][i].Item2+"\n");
                }
                cfs.Write("START\n");
                for (int i = 0; i < ChartData[_SelectedChart.ID].Item2.Count(); i++)
                {
                    cfs.Write(ChartData[_SelectedChart.ID].Item2[i].Item1.ToString() + "," + ChartData[_SelectedChart.ID].Item2[i].Item2.ToString() + "," + ChartData[_SelectedChart.ID].Item2[i].Item3.ToString() + "\n");
                    for (int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count; j++)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            cfs.Write(ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item1[k]);
                            cfs.Write(",");
                        }
                        if (ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item2.Count() != 0)
                        {
                            for(int l = 0;l< ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item2.Count(); l++)
                            {
                                cfs.Write(ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item2[l].Item1);
                                if (l < ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item2.Count() - 1) cfs.Write("+");
                                else cfs.Write(",");
                            }
                            for (int l = 0; l < ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item2.Count(); l++)
                            {
                                cfs.Write(ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item2[l].Item2);
                                if (l < ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item2.Count() - 1) cfs.Write("+");
                            }
                        }
                        cfs.Write("\n");
                    }
                }
                cfs.Write("END\n");
                cfs.Close();
            }
        }

        public void LoadChart(String path)
        {
            StreamReader sr = new StreamReader(@path, System.Text.Encoding.Default);
            int idummy = 0;
            float fdummy = 0;
            List<String[]> values = new List<string[]>();
            List<Tuple<String, String>> chartoptions = new List<Tuple<String, String>>();
            while (sr.EndOfStream == false)
            {
                values.Add(sr.ReadLine().Split(','));
            }
            sr.Close();
            int l = 1;
            if (values[0].Count() < 6) return;
            if (!int.TryParse(values[0][1], out idummy)) return;
            if (!int.TryParse(values[0][3], out idummy)) return;
            if (!int.TryParse(values[0][4], out idummy)) return;
            ChartNames.Add(values[0][0]);
            ChartLevels.Add(int.Parse(values[0][1]));
            ChartDesigners.Add(values[0][2]);
            ChartStandardBPMs.Add(int.Parse(values[0][3]));
            ChartOffsets.Add(int.Parse(values[0][4]));
            ChartJudges.Add(values[0][5].Replace(Environment.NewLine, ""));
            while (values[l][0].Replace(Environment.NewLine, "") != "START")
            {
                if (values[l].Count() >= 2)
                {
                    if (values[l][0] == "TOTAL" && float.TryParse(values[l][1].Replace(Environment.NewLine, ""), out fdummy))
                    {
                        if (fdummy >= 0) chartoptions.Add(Tuple.Create<String, String>(values[l][0], fdummy.ToString()));
                    }
                    if (values[l][0] == "SCROLL" && float.TryParse(values[l][1].Replace(Environment.NewLine, ""), out fdummy))
                    {
                        if (fdummy >= 0) chartoptions.Add(Tuple.Create<String, String>(values[l][0], fdummy.ToString()));
                    }
                    if (values[l][0] == "HOSEI_RATE" && float.TryParse(values[l][1].Replace(Environment.NewLine, ""), out fdummy))
                    {
                        if (fdummy >= 0) chartoptions.Add(Tuple.Create<String, String>(values[l][0], fdummy.ToString()));
                    }
                    if (values[l][0] == "HOSEI_BORDER" && float.TryParse(values[l][1].Replace(Environment.NewLine, ""), out fdummy))
                    {
                        if (fdummy >= 0) chartoptions.Add(Tuple.Create<String, String>(values[l][0], fdummy.ToString()));
                    }
                    if (values[l][0] == "START_GAUGE" && float.TryParse(values[l][1].Replace(Environment.NewLine, ""), out fdummy))
                    {
                        if (fdummy >= 0) chartoptions.Add(Tuple.Create<String, String>(values[l][0], fdummy.ToString()));
                    }
                    if (values[l][0] == "GAUGE_TYPE" && int.TryParse(values[l][1].Replace(Environment.NewLine, ""), out idummy))
                    {
                        if (idummy >= 0 && idummy <= 4) chartoptions.Add(Tuple.Create<String, String>(values[l][0], idummy.ToString()));
                    }
                }
                l++;
            }
            ChartsOptions.Add(chartoptions);
            l++;
            int measure_d = 0;
            int measure_m = 0;
            int separate = 0;
            int rcount = 0;
            List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>> chartData = new List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>>();
            List<Tuple<List<int>, List<Tuple<String, String>>>> measureData = new List<Tuple<List<int>, List<Tuple<String, String>>>>();
            int timeIndex = 1;
            while (values[l][0].Replace(Environment.NewLine, "") != "END")
            {
                if (measure_d == 0 && values[l].Count() >= 3)
                {
                    if (int.TryParse(values[l][0], out idummy)) if (idummy > 0) measure_m = idummy;
                    if (int.TryParse(values[l][1], out idummy)) if (idummy > 0) measure_d = idummy;
                    if (int.TryParse(values[l][2].Replace(Environment.NewLine, ""), out idummy)) if (idummy > 0) separate = idummy;
                    rcount += separate;
                }
                else if (values[l].Count() >= 5)
                {
                    List<int> timeData = new List<int>();
                    List<Tuple<String, String>> timeoptions = new List<Tuple<String, String>>();
                    if (int.TryParse(values[l][0], out idummy)) if (idummy >= 0 && idummy <= 3) timeData.Add(idummy);
                    if (int.TryParse(values[l][1], out idummy)) if (idummy >= 0 && idummy <= 3) timeData.Add(idummy);
                    if (int.TryParse(values[l][2], out idummy)) if (idummy >= 0 && idummy <= 3) timeData.Add(idummy);
                    if (int.TryParse(values[l][3], out idummy)) if (idummy >= 0 && idummy <= 3) timeData.Add(idummy);
                    if (int.TryParse(values[l][4].Replace(Environment.NewLine, ""), out idummy)) if (idummy >= 0 && idummy <= 4) timeData.Add(idummy);
                    if (values[l].Count() >= 7)
                    {
                        String[] optionns = values[l][5].Split('+');
                        String[] optionvs = values[l][6].Split('+');
                        if (optionns.Count() > 0 && optionns.Count() == optionvs.Count())
                        {
                            for (int i = 0; i < optionns.Count(); i++)
                            {
                                if (optionns[i] == "BPMCHANGE") if (float.TryParse(optionvs[i].Replace(Environment.NewLine, ""), out fdummy)) if (fdummy > 0) timeoptions.Add(Tuple.Create<String, String>(optionns[i], optionvs[i].Replace(Environment.NewLine, "")));
                                if (optionns[i] == "SCROLL") if (float.TryParse(optionvs[i].Replace(Environment.NewLine, ""), out fdummy)) if (fdummy > 0) timeoptions.Add(Tuple.Create<String, String>(optionns[i], optionvs[i].Replace(Environment.NewLine, "")));
                            }
                        }
                    }
                    measureData.Add(Tuple.Create<List<int>, List<Tuple<String, String>>>(timeData, timeoptions));
                    if (timeIndex == separate)
                    {
                        chartData.Add(Tuple.Create<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>(measure_m, measure_d, separate, measureData));
                        measure_d = 0;
                        measure_m = 0;
                        separate = 0;
                        timeIndex = 1;
                    }
                    else timeIndex++;
                }
                l++;
            }
            ChartData.Add(Tuple.Create<int, List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<String, String>>>>>>>(rcount, chartData));
            cdlist.Add(new ChartsData(ChartData.Count - 1, ChartNames[ChartData.Count - 1], ChartDesigners[ChartData.Count - 1], ChartLevels[ChartData.Count - 1]));
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
        private void DrawChartData()
        {
            ChartDataGrid.Children.Clear();
            DButtons.Clear();
            int count = 0;
            List<List<Button>> mbuttons = new List<List<Button>>();
            List<Button> tbuttons = new List<Button>();
            for (int i = 0; i < ChartData[_SelectedChart.ID].Item2.Count; i++)
            {
                mbuttons = new List<List<Button>>();
                for (int j = 0; j < ChartData[_SelectedChart.ID].Item2[i].Item4.Count; j++)
                {
                    tbuttons = new List<Button>();
                    ChartDataGrid.RowDefinitions.Add(new RowDefinition());
                    int m = i;
                    int t = j;
                    if (j == 0)
                    {
                        Button label = new Button();
                        label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        label.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                        String ds = ChartData[_SelectedChart.ID].Item2[i].Item2.ToString();
                        String ms = ChartData[_SelectedChart.ID].Item2[i].Item1.ToString();
                        String ss = ChartData[_SelectedChart.ID].Item2[i].Item3.ToString();
                        int index = i+1;
                        label.Content = index;
                        label.Click += (lsender, le) => MeasureButton_Clicked(lsender, ds, ms, ss, index);
                        Grid.SetColumn(label, 0);
                        Grid.SetRow(label, count);
                        ChartDataGrid.Children.Add(label);
                    }
                    else
                    {
                        Label label = new Label();
                        label.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                        label.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                        label.Content = " ";
                        Grid.SetColumn(label, 0);
                        Grid.SetRow(label, count);
                        ChartDataGrid.Children.Add(label);
                    }
                    for (int k = 1; k < 6; k++)
                    {
                        Button note = new Button();
                        switch(ChartData[_SelectedChart.ID].Item2[i].Item4[j].Item1[k - 1])
                        {
                            case 0:
                                note.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                                break;
                            case 1:
                                note.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 54, 255));
                                break;
                            case 2:
                                note.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 210, 131));
                                break;
                            case 3:
                                note.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 13, 13));
                                break;
                            default:
                                break;
                        }
                        int l = k - 1;
                        note.Click += (lsender, le) => NoteButton_Clicked(lsender, m, t, l);
                        Grid.SetColumn(note, k);
                        Grid.SetRow(note, count);
                        ChartDataGrid.Children.Add(note);
                        tbuttons.Add(note);
                    }
                    mbuttons.Add(tbuttons);
                    Button func = new Button();
                    String time = (j+1).ToString();
                    func.Content = time;
                    func.Click += (lsender, le) => FunctionButton_Click(lsender, m,t);
                    Grid.SetColumn(func, 6);
                    Grid.SetRow(func, count);
                    ChartDataGrid.Children.Add(func);
                    count++;
                }
                DButtons.Add(mbuttons);
            }
        }
        private void ClearAllVariable()
        {
            SongName = "";
            ArtistName = "";
            AudioFilePath = "";
            ThumbFilePath = "";
            ChartName = "";
            ChartJudge = "";
            ChartDesignerName = "";
            ChartStandardBPM = 0;
            ChartLevel = 0;
            ChartOffset = 0;
            EdittingID = -1;
            SelectedMeasureIndex = -1;
            PenMode = -1;
            SongNameTBox.Text = "";
            ArtistNameTBox.Text = "";
            AudioFilePathTBox.Text = "";
            ThumbFilePathTBox.Text = "";
            ChartNameTBox.Text = "";
            ChartJudgeTBox.Text = "";
            ChartDesignerNameTBox.Text = "";
            ChartStandardBPMTBox.Text = "";
            ChartLevelTBox.Text = "";
            ChartOffsetTBox.Text = "";
            SelectedMeasureIndexTBox.Text = "";
            denom.Text = "";
            denom_R.Text = "";
            mole.Text = "";
            mole_R.Text = "";
            sep.Text = "";
            sep_R.Text = "";
            ChartPaths = new List<String>();
            ChartNames = new List<String>();
            ChartJudges = new List<String>();
            ChartDesigners = new List<String>();
            ChartStandardBPMs = new List<int>();
            ChartLevels = new List<int>();
            ChartOffsets = new List<int>();
            ChartData = new List<Tuple<int, List<Tuple<int, int, int, List<Tuple<List<int>, List<Tuple<string, string>>>>>>>>();
            ChartsOptions = new List<List<Tuple<string, string>>>();
            cdlist.Clear();
            _SelectedChart = null;
            SelectedChart = null;
        }

        public void OpenProject(String ProjectPath)
        {
            Boolean isMusicTxtRead = false;
            Boolean isAudioFileRead = false;
            Boolean isThumbFileRead = false;
            DirectoryInfo directory = new DirectoryInfo(ProjectPath);
            ClearAllVariable();
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
                        Forms.MessageBox.Show("曲情報が記載されたと推測されるテキストファイルが2個以上あります!\n修正してください!", "プロジェクト読み込みエラー", Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
                else if (CheckExt(ApprovalAudioExt, infstr))
                {
                    if (!isAudioFileRead)
                    {
                        AudioFilePath = infstr;
                        SavedAudioFilePath = infstr;
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
                    LoadChart(ProjectPath + "\\" + infstr);
                    ChartPaths.Add(ProjectPath + "\\" + infstr);
                }
                else if (CheckExt(ApprovalImageExt, infstr))
                {
                    if (!isThumbFileRead)
                    {
                        ThumbFilePath = infstr;
                        SavedThumbFilePath = infstr;
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
            DefaultWorkSpacePath = ProjectPath;
            UpdateDataList();
            if (!File.Exists(System.IO.Path.GetFullPath("config.ini")))
            {
                StreamWriter cfs = new StreamWriter(@System.IO.Path.GetFullPath("config.ini"), false, System.Text.Encoding.Default);
                cfs.Close();
            }
            ReadWriteIni rwIni = new ReadWriteIni(System.IO.Path.GetFullPath("config.ini"));
            rwIni.WriteString("Path", "DefaultWorkSpace", DefaultWorkSpacePath);
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
            if (ChartLevelTBox.Text != "")
            {
                ChartLevel = Convert.ToInt32(ChartLevelTBox.Text.ToString());
            }
        }

        private void ChartOffsetTBox_Changed(object sender, TextChangedEventArgs e)
        {
            if (ChartOffsetTBox.Text != "")
            {
                ChartOffset = Convert.ToInt32(ChartOffsetTBox.Text.ToString());
            }
        }

        private void ChartStandardBPMTBox_Changed(object sender, TextChangedEventArgs e)
        {
            if (ChartStandardBPMTBox.Text != "")
            {
                ChartStandardBPM = Convert.ToInt32(ChartStandardBPMTBox.Text.ToString());
            }
        }

        private void ChartListGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectedChart = (ChartsData)chartListGrid.SelectedItem;
        }

        private void TimeOptionsValueTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void AddOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTimeTBox.Text != "")
            {
                int idummy;
                if (TimeOptionsValueTBox.Text != "" && TimeOptionsComboBox.SelectedValue.ToString() != "" && int.TryParse(TimeOptionsValueTBox.Text, out idummy))
                {
                    tolist.Add(Tuple.Create<String, String>(TimeOptionsComboBox.SelectedValue.ToString(), TimeOptionsValueTBox.Text.ToString()));
                    UpdateTODataList();
                }
            }
        }

        private void DeleteOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTimeOption != null)
            {
                tolist.Remove(SelectedTimeOption);
                UpdateTODataList();
                SelectedTimeOption = null;
            }
        }

        private void TimeOptionsGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectedTimeOption = (Tuple<String,String>)TimeOptionsGrid.SelectedItem;
        }

        private void SaveTimeOptions_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTimeTBox.Text != "")
            {
                ChartData[_SelectedChart.ID].Item2[SelectedMeasureIndex].Item4[int.Parse(SelectedTimeTBox.Text)-1].Item2.Clear();
                for (int i = 0; i < tolist.Count(); i++)
                {
                    ChartData[_SelectedChart.ID].Item2[SelectedMeasureIndex].Item4[int.Parse(SelectedTimeTBox.Text)-1].Item2.Add(tolist[i]);
                }
            }
        }

        private void ChartOptionValueTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void AddChartOptionButton_Click(object sender, RoutedEventArgs e)
        {
            int idummy;
            if (_SelectedChart != null && ChartOptionsValueTBox.Text != "" && ChartOptionsComboBox.SelectedValue != null && int.TryParse(ChartOptionsValueTBox.Text, out idummy))
            {
                colist.Add(Tuple.Create<String, String>(ChartOptionsComboBox.SelectedValue.ToString(), ChartOptionsValueTBox.Text.ToString()));
                UpdateCODataList();
            }
        }

        private void DeleteChartOptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedChartOption != null)
            {
                colist.Remove(SelectedChartOption);
                UpdateCODataList();
                SelectedChartOption = null;
            }
        }

        private void ChartOptionsGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectedChartOption = (Tuple<String, String>)ChartOptionsGrid.SelectedItem;
        }

        private void SaveChartOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_SelectedChart != null)
            {
                ChartsOptions[_SelectedChart.ID] = colist;
            }
        }
    }
}
