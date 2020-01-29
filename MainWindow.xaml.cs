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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NewProjectButton_Clicked(object sender, RoutedEventArgs e)
        {
            NewProject np = new NewProject();
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
        public void OpenProject(String ProjectPath)
        {
            Boolean isMusicTxtRead = false;
            Boolean isAudioFileRead = false;
            Boolean isThumbFileRead = false;
            DirectoryInfo directory = new DirectoryInfo(ProjectPath);
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
    }
}
