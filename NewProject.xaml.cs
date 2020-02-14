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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace BeatapChartMaker
{
    /// <summary>
    /// NewProject.xaml の相互作用ロジック
    /// </summary>
    public partial class NewProject : Window
    {
        private String WorkSpaceDirectoryName = "";
        private String SongName = "";
        private String ArtistName = "";
        private String ThumbFilePath = "";
        private String AudioFilePath = "";
        private String ChartDesignerName = "";
        private String ChartDifficulities = "";
        private String ChartJudges = "";
        private String ChartLevels = "";
        private String StandardBPM = "";
        private static String WSP = "WorkSpace\\";
        public NewProject()
        {
            InitializeComponent();
            this.Activate();
            this.Topmost = true;
        }

        private Boolean isStringLegitimateAsWFName(String name)
        {
            if (name.IndexOf('\\') == -1) return false;
            if (name.IndexOf('/') == -1) return false;
            if (name.IndexOf(':') == -1) return false;
            if (name.IndexOf('*') == -1) return false;
            if (name.IndexOf('?') == -1) return false;
            if (name.IndexOf('"') == -1) return false;
            if (name.IndexOf('<') == -1) return false;
            if (name.IndexOf('>') == -1) return false;
            if (name.IndexOf('|') == -1) return false;
            return true;
        }

        private void WorkSpaceDirectoryNameTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[^/:*?\"<>|]").IsMatch(e.Text);
        }

        private void ChartLevelsTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9,]").IsMatch(e.Text);
        }

        private void StandardBPMTBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void CreateButton_Clicked(object sender, RoutedEventArgs e)
        {
            string[] difs = ChartDifficulities.Split(',');
            string[] juds = ChartJudges.Split(',');
            string[] levs = ChartLevels.Split(',');
            if (difs.Length == juds.Length && levs.Length == difs.Length && WorkSpaceDirectoryName != "" && SongName != "" && ArtistName != "" && ThumbFilePath !="" && AudioFilePath != "" && ChartDesignerName != "" && ChartDifficulities != "" && ChartLevels != "" && StandardBPM != "")
            {
                Directory.CreateDirectory(WSP + WorkSpaceDirectoryName);
                StreamWriter fs = new StreamWriter(@WSP + WorkSpaceDirectoryName + "\\music.txt", false, System.Text.Encoding.Default);
                fs.Write(SongName + "\n");
                fs.Write(ArtistName);
                fs.Close();
                if(ThumbFilePath!="None")File.Copy(@ThumbFilePath, @WSP + WorkSpaceDirectoryName + "\\thumb" + ThumbFilePath.Substring(ThumbFilePath.LastIndexOf(".")));
                File.Copy(@AudioFilePath, @WSP + WorkSpaceDirectoryName + "\\music" + AudioFilePath.Substring(AudioFilePath.LastIndexOf(".")));
                for (int j = 0; j < difs.Length; j++)
                {
                    String path = WSP + WorkSpaceDirectoryName + "\\" + difs[j] + ".csv";
                    StreamWriter cfs = new StreamWriter(@path, false, System.Text.Encoding.Default);
                    cfs.Write(difs[j] + "," + levs[j] + "," + ChartDesignerName + "," + StandardBPM + ",0," + juds[j] + ",\n");
                    cfs.Write("START,,,,,,\n");
                    cfs.Write("END,,,,,,\n");
                    cfs.Close();
                }
                if (File.Exists(System.IO.Path.GetFullPath("config.ini")))
                {
                    ReadWriteIni rwIni = new ReadWriteIni(System.IO.Path.GetFullPath("config.ini"));
                    rwIni.WriteString("Path", "DefaultWorkSpace", WSP + WorkSpaceDirectoryName);
                }
                else
                {
                    StreamWriter cfs = new StreamWriter(@System.IO.Path.GetFullPath("config.ini"), false, System.Text.Encoding.Default);
                    cfs.Close();
                    ReadWriteIni rwIni = new ReadWriteIni(System.IO.Path.GetFullPath("config.ini"));
                    rwIni.WriteString("Path", "DefaultWorkSpace", WSP + WorkSpaceDirectoryName);
                }
                this.Owner.Activate();
                ((MainWindow)this.Owner).OpenProject(WSP + WorkSpaceDirectoryName);
                this.Close();
            }
            else MessageBox.Show("不正な値です.すべての欄に記入したか確かめてください.");
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

        private void ClearButton_Clicked(object sender, RoutedEventArgs e)
        {
            WorkSpaceDirectoryNameTBox.Text = "";
            SongNameTBox.Text = "";
            ArtistNameTBox.Text = "";
            ThumbFilePathTBox.Text = "";
            AudioFilePathTBox.Text = "";
            ChartDesignerNameTBox.Text = "";
            ChartDifficulitiesTBox.Text = "";
            ChartLevelsTBox.Text = "";
            StandardBPMTBox.Text = "";
            WSDNErrorLabel.Content = "";
            AFPErrorLabel.Content = "";
            WorkSpaceDirectoryName = "";
            SongName = "";
            ArtistName = "";
            ThumbFilePath = "";
            AudioFilePath = "";
            ChartDesignerName = "";
            ChartDifficulities = "";
            ChartLevels = "";
            StandardBPM = "";
        }

        private void WorkSpaceDirectoryNameTBox_Changed(object sender, TextChangedEventArgs e)
        {
            if (WorkSpaceDirectoryNameTBox.Text.ToString() == "" || Directory.Exists(WSP + WorkSpaceDirectoryNameTBox.Text.ToString())) { WSDNErrorLabel.Content = "そのフォルダ名は使用できません!"; WorkSpaceDirectoryName = ""; }
            else { WorkSpaceDirectoryName = WorkSpaceDirectoryNameTBox.Text.ToString(); WSDNErrorLabel.Content = ""; }
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
            if (!File.Exists(ThumbFilePathTBox.Text.ToString())) { TFPErrorLabel.Content = "ファイルが存在しません!(「None」と入力すると使用しません)"; ThumbFilePath = ""; }
            else { TFPErrorLabel.Content = ""; ThumbFilePath = ThumbFilePathTBox.Text.ToString(); }
        }

        private void AudioFilePathTBox_Changed(object sender, TextChangedEventArgs e)
        {
            if (!File.Exists(AudioFilePathTBox.Text.ToString())) { AFPErrorLabel.Content = "ファイルが存在しません!"; AudioFilePath = ""; }
            else { AFPErrorLabel.Content = ""; AudioFilePath = AudioFilePathTBox.Text.ToString(); }
        }

        private void ChartDesignerNameTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartDesignerName = ChartDesignerNameTBox.Text.ToString();
        }

        private void ChartDifficulitiesTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartDifficulities = ChartDifficulitiesTBox.Text.ToString();
        }

        private void ChartJudgesTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartJudges = ChartJudgesTBox.Text.ToString();
        }

        private void ChartLevelsTBox_Changed(object sender, TextChangedEventArgs e)
        {
            ChartLevels = ChartLevelsTBox.Text.ToString();
        }

        private void StandardBPMTBox_Changed(object sender, TextChangedEventArgs e)
        {
            StandardBPM = StandardBPMTBox.Text.ToString();
        }
    }
}
