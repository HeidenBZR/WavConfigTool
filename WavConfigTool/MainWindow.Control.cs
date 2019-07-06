using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public partial class MainWindow : Window
    {
        public Reclist Reclist { get; private set; }
        public string TempProject
        {
            get
            {
                var tempdir = System.IO.Path.GetTempPath();
                tempdir = System.IO.Path.Combine(tempdir, "WavConfigTool");
                if (!Directory.Exists(tempdir))
                    Directory.CreateDirectory(tempdir);
                tempdir = System.IO.Path.Combine(tempdir, @"temp.wconfig");
                return tempdir;

            }
        }
        List<WavControl> WavControls;
        public static WavConfigPoint Mode = WavConfigPoint.V;

        public readonly Version Version = new Version(0, 1, 6, 2);

        public static string TempDir
        {
            get
            {
                var tempdir = System.IO.Path.GetTempPath();
                tempdir = System.IO.Path.Combine(tempdir, "WavConfigTool");
                if (!Directory.Exists(tempdir))
                    Directory.CreateDirectory(tempdir);
                tempdir = System.IO.Path.Combine(tempdir, "waveform");
                if (!Directory.Exists(tempdir))
                    Directory.CreateDirectory(tempdir);
                return tempdir;
            }
        }

        Point PrevMousePosition;

        public static MainWindow Current;

        public delegate void ProjectLoadedEventHandler();
        public event ProjectLoadedEventHandler ProjectLoaded;

        public int PageTotal;

        bool IsUnsaved { get => Settings.IsUnsaved; set => Settings.IsUnsaved = value; }

        public bool loaded = false;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                if (Settings.ItemsOnPage == 0)
                    Settings.ItemsOnPage = 4;
                Current = this;
                Init();
                if (OpenBackup())
                    return;
                if (OpenLast())
                    return;
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on MainWindow");
            }
        }

        public static void MessageBoxError(Exception ex, string name)
        {
            Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", name,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        void Init()
        {
            try
            {
                if (File.Exists("icon.bmp"))
                    Icon = new BitmapImage(new Uri("icon.bmp", UriKind.Relative));

                Width = Settings.WindowSize.X;
                Height = Settings.WindowSize.Y;
                Left = Settings.WindowPosition.X;
                Top = Settings.WindowPosition.Y;
                WindowState = Settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
                PrevMousePosition = Mouse.GetPosition(this);
                InitTextBoxes();
                ProjectLoaded += delegate
                {
                    SetTitle();
                    //LabelCurrentSettings.Content = System.IO.Path.GetFileName(Settings.WavSettings);
                    if (Reclist != null && Reclist.IsLoaded)
                    {
                        //LabelCurrentVoicebank.Content = System.IO.Path.GetFileName(Reclist.VoicebankPath);
                        InitWavcontrolsAsync(true);
                        DrawPageAsync();
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on init");
            }
        }

        void InitTextBoxes()
        {
            TextBoxFadeC.Text = Settings.ConsonantAttack.ToString();
            TextBoxFadeV.Text = Settings.VowelAttack.ToString();
            TextBoxFadeD.Text = Settings.RestAttack.ToString();
            TextBoxSustain.Text = WavControl.VowelSustain.ToString();
            TextBoxItemsOnPage.Text = Settings.ItemsOnPage.ToString();
            TextBoxPage.Text = Settings.CurrentPage.ToString();
            TextBoxMultiplier.Text = Settings.WAM.ToString("f2");
        }

        void ScrollView()
        {
            double offset = WavControl.MostLeft - 100 * WavControl.ScaleX;
            if (offset < 0) offset = 0;
            ScrollViewer.ScrollToHorizontalOffset(offset);
            ScrollContent(offset);
        }

        void ScrollContent(double offset = -1)
        {
            if (offset == -1)
                offset = ScrollViewer.HorizontalOffset;
            for (int i = Settings.CurrentPage * Settings.ItemsOnPage; i < (Settings.CurrentPage + 1) * Settings.ItemsOnPage; i++)
                if (i < WavControls.Count)
                    WavControls[i].LabelName.Margin = new Thickness(offset, 0, 0, 0);
        }


        void AddFile(string filename, string phonemes)
        {
            Recline recline = new Recline(filename, phonemes);
            recline.Reclist = Reclist;
            Reclist.Reclines.Add(recline);
            AddWavControl(recline);
        }
        void AddFile(string filename, string phonemes, string description)
        {
            Recline recline = new Recline(filename, phonemes, description);
            recline.Reclist = Reclist;
            Reclist.Reclines.Add(recline);
            AddWavControl(recline);
        }

        void AddWavControl(Recline recline)
        {
            WavControl control = new WavControl(recline);
            control.WavControlChanged += SaveBackup;
            control.WavControlChanged += () =>
            {
                if (Settings.ProjectFile != TempProject)
                    Save();
            };
            WavControls.Add(control);
        }

        void ClearWavcontrols()
        {
            Dispatcher.Invoke((ThreadStart)delegate
            {
                WaveControlStackPanel.Children.Clear();
                WaveControlStackPanel.Children.Capacity = 0;
            });
        }

        bool OpenProjectWindow(string voicebank = "", string wavsettings = "", string path = "", bool open = false)
        {
            try
            {
                ClearWavcontrols();
                ProjectWindow project = new ProjectWindow(voicebank, wavsettings, path, open);
                project.ShowDialog();
                if (project.Result == Result.Cancel)
                {
                    DrawPageAsync();
                    return true;
                }
                if (project.Result == Result.Close)
                    return false;
                else if (project.Result == Result.Open)
                    if (Open(project.Settings, project.Path))
                        return true;
                    else { }
                else
                {
                    NewProject(project.Settings, project.Voicebank);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on open project window");
                return false;
            }
        }

        public void SetMode(WavConfigPoint mode)
        {
            Mode = mode;
            LabelMode.Content = mode;
        }

        void SetTitle(bool unsaved = false)
        {
            Title = $"WavConfig v.{Version.ToString()} - {System.IO.Path.GetFileName(Settings.ProjectFile)}{(IsUnsaved ? "*" : "")}";
            if (Reclist is null || !Reclist.IsLoaded)
                return;
            try { Title += $" [{new DirectoryInfo(Reclist.VoicebankPath).Name}]"; }
            catch { }
        }

        public static string SaveOtoDialog()
        {
            try
            {

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "oto.ini file (oto.ini)|*oto*.ini";
                dialog.InitialDirectory = Reclist.Current.VoicebankPath;
                dialog.FileName = "oto.ini";
                var result = dialog.ShowDialog();
                if (!result.HasValue || !result.Value) return "";
                return dialog.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on save oto dialog");
                return "";
            }
        }

        void SetFade(WavConfigPoint point, int value)
        {
            if (point == WavConfigPoint.V) Settings.VowelAttack = value;
            else if (point == WavConfigPoint.C) Settings.ConsonantAttack = value;
            else if (point == WavConfigPoint.D) Settings.RestAttack = value;
            DrawPageAsync();
        }

        void ClearTemp()
        {
            try
            {
                foreach (string filename in Directory.GetFiles(TempDir))
                    File.Delete(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on clear cache",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        async void ClearTempAsync()
        {
            try
            {
                foreach (string filename in Directory.GetFiles(TempDir))
                    await Task.Run(delegate { File.Delete(filename); });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on clear cache async",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool DoEvenIfUnsaved(string message = "Продолжить?")
        {
            if (!IsUnsaved) return true;
            var result = MessageBox.Show("Имеются несохраненные изменения. " + message, "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                File.Delete(TempProject);
                return true;
            }
            else return false;
        }

        void ToggleTools()
        {
            if (ToolsPanel.Height == new GridLength(80)) ToolsPanel.Height = new GridLength(0);
            else ToolsPanel.Height = new GridLength(80);
        }

        void SetPage(int page)
        {
            try
            {
                if (page < PageTotal && page >= 0 && page != Settings.CurrentPage)
                {
                    UndrawPage();
                    Settings.CurrentPage = page;
                    DrawPageAsync(manual: true);
                    if (TextBoxPage.Text != (page + 1).ToString())
                        TextBoxPage.Text = (page + 1).ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on SetPage");
            }
        }

        bool SetSustain(int sustain)
        {
            if (sustain <= 0)
                return false;
            WavControl.VowelSustain = sustain;
            DrawPageAsync(manual: true);
            return true;
        }

        void GotoWav(WavControl control)
        {
            GotoWav(control.Recline);
        }
        void GotoWav(Recline recline)
        {
            int ind = Reclist.Reclines.IndexOf(recline);
            int page = ind / Settings.ItemsOnPage;
            SetPage(page);
        }

        void SetItemsOnPage(byte items)
        {
            if (items <= 1000 && items > 0)
            {
                int current = Settings.CurrentPage * Settings.ItemsOnPage;
                Settings.ItemsOnPage = items;
                if (Settings.CurrentPage > PageTotal)
                    Settings.CurrentPage = PageTotal - 1;
                SetPageTotal();
                GotoWav(WavControls[current]);
                DrawPageAsync(manual: true);
            }
            else TextBoxItemsOnPage.Text = Settings.ItemsOnPage.ToString();
        }

        void SetPageTotal()
        {
            double temp = (double)WavControls.Count / Settings.ItemsOnPage;
            PageTotal = temp % 1 > 0 ? (int)(temp + 1) : (int)(temp);
            LabelPageTotal.Content = (PageTotal).ToString();
            if (Settings.CurrentPage >= PageTotal)
                SetPage(PageTotal - 1);
        }

        void SetWaveformAmplitudeMultiplayer(float value)
        {
            if (value > 0 && value < 50f)
            {
                Settings.WAM = value;
                UndrawPage();
                InitWavcontrols(force: true);
                DrawPageAsync();
            }
        }

        void ContentMove(Point position)
        {
            double deltaX = PrevMousePosition.X - position.X;
            double deltaY = PrevMousePosition.Y - position.Y;
            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + deltaX);
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.VerticalOffset + deltaY);
        }

        void ToggleWaveform() { }
        void ToggleSpectrum() { }
        void TogglePitch() { }

        void FindUmcompleted()
        {
            int ind = WavControls.FindIndex(n => !n.IsCompleted);
            if (ind == -1)
                MessageBox.Show("Все аудиофайлы настроены.", "Не найдено", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                GotoWav(WavControls[ind]);
        }

        void FindWav()
        {
            var dialog = new FindWavDialog(Reclist);
            dialog.ShowDialog();
            if (dialog.Index != -1) GotoWav(WavControls[dialog.Index]);
        }
    }
}
