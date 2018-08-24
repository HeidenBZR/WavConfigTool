﻿using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WavConfigTool
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Reclist Reclist;
        string WavSettings;
        string Path;
        string TempPath = "temp.wconfig";
        List<WavControl> WavControls;

        int PageCurrent = 0;
        int PageTotal = 0;
        byte ItemsOnPage = 4;

        public MainWindow()
        {
            InitializeComponent();
            if (CheckSettings() && CheckLast()) { DrawPage(); }
            else OpenProjectWindow();
        }

        bool CheckSettings()
        {
            if (File.Exists("settings")) ReadSettings(File.ReadAllText("settings", Encoding.UTF8));
            return File.Exists("settings");
        }

        bool CheckLast()
        {
            if (File.Exists("last")) ReadProject(File.ReadAllText("last", Encoding.UTF8));
            return File.Exists("last");
        }

        void DrawPage()
        {
            WaveControlStackPanel.Children.Clear();
            WaveControlStackPanel.Children.Capacity = 0;
            PageTotal = WavControls.Count / ItemsOnPage;
            int count = ItemsOnPage;
            if (ItemsOnPage * PageCurrent + count > WavControls.Count) count = WavControls.Count - ItemsOnPage * PageCurrent;
            foreach (WavControl control in WavControls.GetRange(ItemsOnPage * PageCurrent, count))
            {
                WaveControlStackPanel.Children.Add(control);
                control.Draw();
            }
            LabelItemsOnPage.Text = ItemsOnPage.ToString();
            LabelPage.Text = PageCurrent.ToString();
            LabelPageTotal.Content = PageTotal.ToString();
            ScrollViewer.ScrollToHorizontalOffset(WavControl.MostLeft - 200 * WavControl.ScaleX);
        }

        /// <summary>
        /// Надо асинхронно
        /// </summary>
        void DrawAsync()
        {
            foreach (WavControl control in WavControls)
            {
                control.Draw();
            }
        }

        void AddFile(string filename, string phonemes)
        {
            Recline recline = new Recline(filename, phonemes);
            recline.Reclist = Reclist;
            AddWavControl(recline);
        }
        void AddFile(string filename, string phonemes, string description)
        {
            Recline recline = new Recline(filename, phonemes, description);
            recline.Reclist = Reclist;
            AddWavControl(recline);
        }
        void AddWavControl(Recline recline)
        {
            WavControl control = new WavControl(recline);
            recline.Reclist = Reclist;
            Reclist.Reclines.Add(recline);
            WavControls.Add(control);
        }

        void OpenProjectWindow(string voicebank = "", string wavsettings = "", string path = "")
        {
            Project project = new Project(voicebank, wavsettings, path);
            while (true)
            { 
                project.ShowDialog();
                if (project.Result == Result.Close) { Close(); return; }
                else if (project.Result == Result.Open)
                    if (Open(project.Settings, project.Path))
                        return;
                    else { }
                else
                {
                    NewProject(project.Settings, project.Voicebank);
                    return;
                }
            }
        }

        void NewProject(string settings, string voicebank)
        {
            ReadSettings(settings);
            Reclist.VoicebankPath = voicebank;
            Path = TempPath;
            DrawPage();
            Title = $"WavConfig - {System.IO.Path.GetFileName(Path)} [{new DirectoryInfo(Reclist.VoicebankPath).Name}]";
        }

        bool Open(string settings, string project)
        {
            ReadSettings(settings);
            if (File.Exists(project))
            {
                ReadProject(project);
                DrawPage();
                return true;
            }
            else MessageBox.Show("Ошибка при открытии файла проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        void SaveAs()
        {
            string lastpath = Path;
            SaveFileDialog openFileDialog = new SaveFileDialog();
            openFileDialog.Filter = "WavConfig Project (*.wconfig)|*.wconfig";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            Path = openFileDialog.FileName;
            Save();
            Title = $"WavConfig - {System.IO.Path.GetFileName(Path)} [{new DirectoryInfo(Reclist.VoicebankPath).Name}]";
            File.Delete(lastpath);
        }

        void Save()
        {
            string text = "";
            text += $"{Reclist.VoicebankPath}\r\n";

            foreach (WavControl control in WavControls)
            {
                text += $"{control.Recline.Filename}\r\n";
                text += $"{String.Join(" ", control.Ds.Select(n => n.ToString("f0")))}\r\n";
                text += $"{ String.Join(" ", control.Vs.Select(n => n.ToString("f0"))) }\r\n";
                text += $"{String.Join(" ", control.Cs.Select(n => n.ToString("f0")))}\r\n";
            }
            File.WriteAllText(Path, text, Encoding.UTF8);
        }

        void ReadSettings(string settings)
        {
            string[] lines = File.ReadAllLines(settings);
            var vs = lines[0].Split(' ');
            var cs = lines[1].Split(' ');
            Reclist = new Reclist(vs, cs);
            WavControls = new List<WavControl>();
            for (int i = 2; i < lines.Length; i++)
            {
                string[] items = lines[i].Split('\t');
                if (items.Length != 3) continue;
                AddFile(items[0], items[1], items[2]);
            }
            WavSettings = settings;
            File.WriteAllText("settings", WavSettings, Encoding.UTF8);
        }

        void ReadProject(string project)
        {
            Path = project;
            string[] lines = File.ReadAllLines(project);
            Reclist.VoicebankPath = lines[0];
            for (int i = 1; i + 3 < lines.Length;  i += 4)
            {
                string filename = lines[i];
                string pds = lines[i + 1];
                string pvs = lines[i + 2];
                string pcs = lines[i + 3];
                WavControl control = WavControls.Find(n => n.Recline.Filename == filename);
                if (control != null)
                {
                    if (pds.Length > 0) control.Ds = pds.Split(' ').Select(n => double.Parse(n)).ToList();
                    if (pvs.Length > 0) control.Vs = pvs.Split(' ').Select(n => double.Parse(n)).ToList();
                    if (pcs.Length > 0) control.Cs = pcs.Split(' ').Select(n => double.Parse(n)).ToList();
                }
            }
            File.WriteAllText("last", Path, Encoding.UTF8);
            Title = $"WavConfig - {System.IO.Path.GetFileName(Path)} [{new DirectoryInfo(Reclist.VoicebankPath).Name}]";
        }

        void Generate()
        {
            string text = "";
            Reclist.Aliases = new List<string>();
            foreach (WavControl control in WavControls)
            {
                text += control.Generate();
            }
            File.WriteAllText(System.IO.Path.Combine(Reclist.VoicebankPath, "oto.ini"), text, Encoding.UTF8);
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            if (Path == TempPath)
                SaveAs();
            else
                Save();
        }

        private void MenuGenerate_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Открыть окно проекта? Несохраненные данные будут потеряны", "OpenProjectWindow project", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes) OpenProjectWindow(Reclist.VoicebankPath, WavSettings, Path);
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            if (PageCurrent < PageTotal) PageCurrent++;
            DrawPage();
        }

        private void PrevPage(object sender, RoutedEventArgs e)
        {
            if (PageCurrent > 0) PageCurrent--;
            DrawPage();
        }

        private void PageChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (int.TryParse(LabelPage.Text, out int page) && page <= PageTotal)
            {
                PageCurrent = page;
                DrawPage();
            }
            else LabelPage.Text = PageCurrent.ToString();
        }

        private void ItemsOnPageChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            if (byte.TryParse(LabelItemsOnPage.Text, out byte items))
            {
                ItemsOnPage = items;
                PageTotal = WavControls.Count / ItemsOnPage;
                if (PageCurrent > PageTotal) PageCurrent = PageTotal;
                DrawPage();
            }
            else LabelItemsOnPage.Text = ItemsOnPage.ToString();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!IsLoaded) return;
            foreach (WavControl control in WavControls)
                control.LabelName.Margin = new Thickness(e.HorizontalOffset,0,0,0);
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }
    }
}
