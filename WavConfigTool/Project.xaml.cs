using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        Close
    }
    public partial class Project : Window
    {

        public Result Result;
        public string VB;
        public string WS;
        public string Path;

        public Project()
        {
            InitializeComponent();
        }
        public Project(string vb, string ws)
        {
            InitializeComponent();
            TextBoxVB.Text = vb;
            TextBoxWS.Text = ws;
        }

        bool CheckValid()
        {
            return System.IO.File.Exists(TextBoxWS.Text) &&
                System.IO.Directory.EnumerateFiles(TextBoxVB.Text, ".wav").ToArray().Length > 0;
        }

        private void ButtonOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            if (Result == Result.New)
            {
                if (CheckValid()) Close();
            }
        }

        private void ButtonSettings(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = TextBoxWS.Text;
            openFileDialog.Filter = "WavConfig Settings files (*.wsettings)|*.wsettings|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            TextBoxWS.Text = openFileDialog.FileName;
            Result = Result.New;
        }

        private void ButtonVoicebank(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = TextBoxVB.Text;
            openFileDialog.Filter = "Voicebank samples (*.wav)|*.wav";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            TextBoxVB.Text = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
            Result = Result.New;
        }

        private void ButtonCancel(object sender, RoutedEventArgs e)
        {
            Result = Result.Close;
            Close();
        }

        private void ButtonOpen(object sender, RoutedEventArgs e)
        {
            Result = Result.Open;
        }
    }
}
