using Microsoft.Win32;
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

namespace WavConfigTool
{
    /// <summary>
    /// Логика взаимодействия для Project.xaml
    /// </summary>
    public enum Result
    {
        Open,
        New,
        Close,
        Cancel
    }
    public partial class Project : Window
    {

        public Result Result = Result.Close;
        public string Voicebank;
        public string Settings;
        public string Path;

        string GetTempPath(string path)
        {
            return  System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                path);
        }

        public Project(bool open = false)
        {
            InitializeComponent();
            Result = Result.Cancel;
            if (open) TabOpen.IsSelected = true;
        }

        void CheckPath()
        {
            if (!Directory.Exists(GetTempPath(@"WavConfigTool")))
                Directory.CreateDirectory(GetTempPath(@"WavConfigTool"));
            if (!Directory.Exists(GetTempPath(@"WavConfigTool\WavSettings\")))
                Directory.CreateDirectory(GetTempPath(@"WavConfigTool\WavSettings\"));
        }


        public Project(string vb, string ws, string path, bool open = false) : this(open)
        {
            InitializeComponent();
            if (ws == "")
                ws = WavConfigTool.Settings.WavSettings;
            if (path == "")
                path = WavConfigTool.Settings.ProjectFile;
            Voicebank = vb;
            Settings = ws;
            Path = path;
            TextBoxVB.Text = vb;
            TextBoxWS.Text = ws;
            TextBoxPath.Text = path;
            Result = Result.Close;

            //TextBoxVB.Text = @"D:\DISCS\YandexDisk\Heiden\UTAU\_voicebanks\() Minto Arpasing RUS";
        }

        private void ButtonSettings(object sender, RoutedEventArgs e)
        {
            CheckPath();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = GetTempPath(@"WavConfigTool\WavSettings\");
            openFileDialog.Filter = "WavConfig Settings files (*.wsettings)|*.wsettings|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == "") return;
            TextBoxWS.Text = openFileDialog.FileName;
        }

        private void ButtonVoicebank(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = TextBoxVB.Text;
            openFileDialog.Filter = "Voicebank samples (*.wav)|*.wav";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == "") return;
            TextBoxVB.Text = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
        }

        bool CheckSettings()
        {
            Voicebank = TextBoxVB.Text;
            Settings = TextBoxWS.Text;
            Path = TextBoxPath.Text;

            if (!File.Exists(Settings))
            {
                MessageBox.Show("Необходимо выбрать файл конфигурации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            else return true;
        }

        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            Result = Result.Cancel;
            Close();
        }

        private void ButtonProject(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "WavConfig Project (*.wconfig)|*.wconfig";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == "") return;
            TextBoxPath.Text = openFileDialog.FileName;
        }


        private void ButtonOpen(object sender, RoutedEventArgs e)
        {
            if (!CheckSettings()) return;
            Result = Result.Open;
            Close();
        }
        private void ButtonNew(object sender, RoutedEventArgs e)
        {
            if (!CheckSettings()) return;
            Result = Result.New;
            Close();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
