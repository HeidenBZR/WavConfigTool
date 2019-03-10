using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using WavConfigTool.ViewModels;

namespace WavConfigTool.Windows
{
    /// <summary>
    /// Логика взаимодействия для Project.xaml
    /// </summary>

    public partial class ProjectWindow : Window
    {
        public ProjectWindow()
        {
            InitializeComponent();
        }

        public static string VoicebankDialog(string initialFile = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (initialFile != null && initialFile != "")
                if (Directory.Exists(Path.GetDirectoryName(initialFile)))
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(initialFile);
            openFileDialog.Filter = "Voicebank samples (*.wav)|*.wav";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = false;
            var result = openFileDialog.ShowDialog();
            if (result == null || !result.Value || openFileDialog.FileName == "")
                return null;
            else
                return Path.GetDirectoryName(openFileDialog.FileName);
        }



        private void ButtonVoicebank(object sender, RoutedEventArgs e)
        {
            //string vb = VoicebankDialog(Project.Voicebank.Location);
            //if (vb != null)
            //    Project.ChangeVoicebank(vb);
        }
    }
}
