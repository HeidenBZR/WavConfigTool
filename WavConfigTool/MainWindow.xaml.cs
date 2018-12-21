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


    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Reclist Reclist;
        string TempProject
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

        public readonly Version Version = new Version(0, 1, 5, 1);

        int PageCurrent = 0;
        int PageTotal = 0;
        byte ItemsOnPage = 4;

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

        bool IsUnsaved { get => Settings.IsUnsaved; set => Settings.IsUnsaved = value; }

        public bool loaded = false;

        public MainWindow()
        {
            try
            {

                InitializeComponent();
                Current = this;
                ClearTemp();
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

        bool OpenBackup()
        {
            try
            {
                if (IsUnsaved && File.Exists(TempProject))
                {
                    if (!CheckSettings())
                        return false;
                    if (!ReadProject(TempProject))
                        return false;
                    MessageBox.Show("Найдены несохраненные изменения. Пересохраните проект.");
                    loaded = true;
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on openbackup", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        bool OpenLast()
        {
            try
            {
                if (CheckSettings() && CheckLast())
                {
                    loaded = true;
                    DrawPage();
                    return loaded;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on open last",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
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
                    if (Reclist != null && Reclist.IsLoaded)
                        LabelCurrentVoicebank.Content = System.IO.Path.GetFileName(Reclist.VoicebankPath);
                    LabelCurrentSettings.Content = System.IO.Path.GetFileName(Settings.WavSettings);
                    GenerateWaveforms();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on init",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void ChangeVoicebank()
        {
            string old_vb = Reclist.VoicebankPath;
            SaveBackup();
            var vb = Project.VoicebankDialog(Reclist.VoicebankPath);
            if (vb != null)
                if (Directory.Exists(vb))
                {
                    Reclist.SetVoicebank(vb);
                    if (Settings.IsUnsaved)
                        SaveBackup();
                    else
                        Save();
                    ClearTemp();
                    ClearWavcontrols();
                    if (CheckSettings() && (Settings.IsUnsaved && ReadProject(TempProject)
                        || ReadProject(Settings.ProjectFile)))
                        DrawPage();
                    else
                    {
                        Reclist.SetVoicebank(old_vb);
                        if (Settings.IsUnsaved)
                            SaveBackup();
                        else
                            Save();
                        ClearTemp();
                        ClearWavcontrols();
                        DrawPage();
                        MessageBox.Show("Voicebank changing failed");
                    }
                }
        }

        void ChangeSettings()
        {
            string old_s = Settings.WavSettings;
            var s = Project.SettingsDialog();
            if (s != null)
                if (File.Exists(s))
                {
                    Settings.WavSettings = s;
                    ClearTemp();
                    ClearWavcontrols();
                    if (CheckSettings() && (Settings.IsUnsaved && OpenBackup()
                        || ReadProject(Settings.ProjectFile)))
                        DrawPage();
                    else
                    {
                        Settings.WavSettings = old_s;
                        ClearTemp();
                        ClearWavcontrols();
                        DrawPage();
                        MessageBox.Show("WavSettings changing failed");
                    }
                }
        }

        void InitTextBoxes()
        {
            TextBoxFadeC.Text = Settings.FadeC.ToString();
            TextBoxFadeV.Text = Settings.FadeV.ToString();
            TextBoxFadeD.Text = Settings.FadeD.ToString();
            TextBoxItemsOnPage.Text = Settings.ItemsOnPage.ToString();
            TextBoxPage.Text = Settings.CurrentPage.ToString();
            TextBoxMultiplier.Text = Settings.WAM.ToString("f2");
        }

        bool CheckSettings()
        {
            if (Settings.WavSettings != "" && File.Exists(Settings.WavSettings))
            {
                ReadSettings(Settings.WavSettings);
                return true;
            }
            return false;
        }

        bool CheckLast()
        {
            if (Settings.ProjectFile != "" && File.Exists(Settings.ProjectFile))
            {
                bool result = ReadProject(Settings.ProjectFile);
                return result;
            }
            return false;
        }

        void DrawPage(bool manual = true)
        {
            if (!IsInitialized) return;
            WaveControlStackPanel.Children.Clear();
            WaveControlStackPanel.Children.Capacity = 0;
            SetPageTotal();
            int count = ItemsOnPage;
            while (ItemsOnPage * PageCurrent + count > WavControls.Count)
                count--;
            foreach (WavControl control in WavControls.GetRange(ItemsOnPage * PageCurrent, count))
            {
                if (control.IsEnabled)
                {
                    WaveControlStackPanel.Children.Add(control);
                    control.Draw();
                }
            }
            //if (manual)
            //{
            //    TextBoxItemsOnPage.Text = ItemsOnPage.ToString();
            //    TextBoxPage.Text = (PageCurrent + 1).ToString();
            //}
            double offset = WavControl.MostLeft - 100 * WavControl.ScaleX;
            if (offset < 0) offset = 0;
            ScrollViewer.ScrollToHorizontalOffset(offset);
            ScrollContent(offset);
        }

        void ScrollContent(double offset = -1)
        {
            if (offset == -1)
                offset = ScrollViewer.HorizontalOffset;
            for (int i = PageCurrent * ItemsOnPage; i <  (PageCurrent+1) * ItemsOnPage; i++)
                if (i < WavControls.Count)
                    WavControls[i].LabelName.Margin = new Thickness(offset, 0, 0, 0);
        }

        void GenerateWaveforms(bool force = false)
        {
            if (!IsLoaded) return;
            GenerateWaveformsAsync(force);
        }

        Thread Thread;

        async void GenerateWaveformsAsync(bool force)
        {
            if (WavControls is null)
                return;
            try
            {
                Dispatcher.Invoke(() => { CanvasLoading.Visibility = Visibility.Visible; });
                await Task.Run(() =>
                {
                    if (Thread != null && Thread.IsAlive)
                        Thread.Join();
                    Dispatcher.Invoke(() => { CanvasLoading.Visibility = Visibility.Hidden; });
                    Thread = Thread.CurrentThread;
                    foreach (WavControl control in WavControls)
                    {
                        control.GenerateWaveformAsync(force);
                        control.OpenImageAsync();
                    }
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on GenerateWaveformsAsync",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.Invoke(() => { CanvasLoading.Visibility = Visibility.Hidden; });
            }
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
                Project project = new Project(voicebank, wavsettings, path, open);
                project.ShowDialog();
                if (project.Result == Result.Cancel)
                {
                    DrawPage();
                    return true;
                }
                ClearTemp();
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

        void NewProject(string settings, string voicebank)
        {
            try
            {
                ReadSettings(settings);
                Reclist.SetVoicebank(voicebank);
                Settings.ProjectFile = TempProject;
                DrawPage();
                SetTitle();
                if (Settings.ProjectFile != TempProject && File.Exists(TempProject)) File.Delete(TempProject);
                ProjectLoaded();
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on new project");
            }
        }

        bool Open(string settings, string project)
        {
            ReadSettings(settings);
            if (File.Exists(project))
            {
                bool result = ReadProject(project);
                if (!result) return false;
                DrawPage();
                SetTitle();
                if (Settings.ProjectFile != TempProject && File.Exists(TempProject)) File.Delete(TempProject);
                return true;
            }
            else MessageBox.Show("Ошибка при открытии файла проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        void SetMode(WavConfigPoint mode)
        {
            Mode = mode;
            LabelMode.Content = mode;
        }

        void Save()
        {
            try
            {

                if (Settings.ProjectFile == TempProject)
                {
                    SaveAs();
                    return;
                }
                string text = "";
                text += $"{Reclist.VoicebankPath}\r\n";

                foreach (WavControl control in WavControls)
                {
                    text += $"{control.Recline.Filename}\r\n";
                    text += $"{String.Join(" ", control.Ds.Select(n => n.ToString("f0")))}\r\n";
                    text += $"{ String.Join(" ", control.Vs.Select(n => n.ToString("f0"))) }\r\n";
                    text += $"{String.Join(" ", control.Cs.Select(n => n.ToString("f0")))}\r\n";
                }
                File.WriteAllText(Settings.ProjectFile, text, Encoding.UTF8);
                IsUnsaved = false;
                SetTitle();
                if (File.Exists(TempProject))
                    File.Delete(TempProject);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on save project");
                return;
            }
        }

        void SaveAs()
        {
            try
            {
                string lastpath = Settings.ProjectFile;
                SaveFileDialog openFileDialog = new SaveFileDialog();
                openFileDialog.Filter = "WavConfig Project (*.wconfig)|*.wconfig";
                openFileDialog.RestoreDirectory = true;
                openFileDialog.ShowDialog();
                if (openFileDialog.FileName == "") return;
                Settings.ProjectFile = openFileDialog.FileName;
                Save();
                File.Delete(lastpath);
                IsUnsaved = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on save as");
                return;
            }
        }

        void SetTitle(bool unsaved = false)
        {
            Title = $"WavConfig v.{Version.ToString()} - {System.IO.Path.GetFileName(Settings.ProjectFile)}{(IsUnsaved? "*" : "")}";
            if (Reclist is null || !Reclist.IsLoaded)
                return;
            try { Title += $" [{new DirectoryInfo(Reclist.VoicebankPath).Name}]"; }
            catch { }
        }

        void SaveBackup()
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
            File.WriteAllText(TempProject, text, Encoding.UTF8);
            SetTitle(true);
            IsUnsaved = true;
        }

        void ReadSettings(string settings)
        {
            try
            {

                string[] lines = File.ReadAllLines(settings);
                var vs = lines[0].Split(' ');
                var cs = lines[1].Split(' ');
                Reclist = new Reclist(vs, cs);
                WavControls = new List<WavControl>();
                for (int i = 2; i < lines.Length; i++)
                {
                    string[] items = lines[i].Split('\t');
                    if (items.Length != 3)
                        continue;
                    AddFile(items[0], items[1], items[2]);
                }
                var name = System.IO.Path.GetFileNameWithoutExtension(settings);
                Reclist.Name = name;
                Settings.WavSettings = settings;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on wavsettings reading");
            }
        }

        bool ReadProject(string project)
        {
            try
            {

                Settings.ProjectFile = project;
                string[] lines = File.ReadAllLines(project);
                Reclist.SetVoicebank(lines[0]);
                for (int i = 1; i + 3 < lines.Length; i += 4)
                {
                    string filename = lines[i];
                    string pds = lines[i + 1];
                    string pvs = lines[i + 2];
                    string pcs = lines[i + 3];
                    WavControl control = WavControls.Find(n => n.Recline.Filename == filename);
                    if (control != null)
                    {
                        //MessageBox.Show($"Some sample missing: \r\n{control.Recline.Path}", "Error on project reading", MessageBoxButton.OK, MessageBoxImage.Error);

                        if (pds.Length > 0) control.Ds = pds.Split(' ').Select(n => double.Parse(n)).ToList();
                        if (pvs.Length > 0) control.Vs = pvs.Split(' ').Select(n => double.Parse(n)).ToList();
                        if (pcs.Length > 0) control.Cs = pcs.Split(' ').Select(n => double.Parse(n)).ToList();
                    }
                }
                SetTitle();
                ProjectLoaded();
                return true;
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on project reading");
                return false;
            }
        }

        string SaveOtoDialog()
        {
            try
            {

                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "oto.ini file (oto.ini)|*oto*.ini";
                dialog.InitialDirectory = Reclist.VoicebankPath;
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

        void Generate()
        {
            try
            {
                string text = "";
                Reclist.Aliases = new List<string>();
                foreach (WavControl control in WavControls)
                {
                    text += control.Generate();
                }
                string filename = SaveOtoDialog();
                if (filename != "")
                    File.WriteAllText(System.IO.Path.Combine(Reclist.VoicebankPath, filename), text, Encoding.ASCII);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on oto.ini", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void SetFade(WavConfigPoint point, int value)
        {
            if (point == WavConfigPoint.V) Settings.FadeV = value;
            else if (point == WavConfigPoint.C) Settings.FadeC = value;
            else if (point == WavConfigPoint.D) Settings.FadeD = value;
            DrawPage();
        }
        
        void ClearTemp()
        {
            try
            {
                Dispatcher.Invoke((ThreadStart)delegate
                {
                    WaveControlStackPanel.Children.Clear();
                    WaveControlStackPanel.Children.Capacity = 0;
                });
                foreach (string filename in Directory.GetFiles(TempDir))
                    File.Delete(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on clear cache",
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
                ClearTemp();
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
            if (page < PageTotal && page >= 0)
            {
                PageCurrent = page;
                DrawPage(manual:true);
                if (TextBoxPage.Text != (page + 1).ToString())
                    TextBoxPage.Text = (page + 1).ToString();
            }
            //else TextBoxPage.Text = PageCurrent.ToString();
        }

        void GotoWav(WavControl control)
        {
            GotoWav(control.Recline);
        }
        void GotoWav(Recline recline)
        {
            int ind = Reclist.Reclines.IndexOf(recline);
            int page = ind / ItemsOnPage;
            SetPage(page);
        }

        void SetItemsOnPage(byte items)
        {
            if (items <= 100 && items > 0)
            {
                int current = PageCurrent * ItemsOnPage;
                ItemsOnPage = items;
                if (PageCurrent > PageTotal) PageCurrent = PageTotal;
                SetPageTotal();
                GotoWav(WavControls[current]);
            }
            //else TextBoxItemsOnPage.Text = ItemsOnPage.ToString();
        }

        void SetPageTotal()
        {
            double temp = (double)WavControls.Count / ItemsOnPage;
            PageTotal = temp % 1 > 0? (int)(temp + 1) : (int)(temp);
            LabelPageTotal.Content = (PageTotal).ToString();
            if (PageCurrent >= PageTotal)
                SetPage(PageTotal - 1);
        }

        void SetWaveformAmplitudeMultiplayer(float value)
        {
            if (value > 0 && value < 50f)
            {
                Settings.WAM = value;
                ClearTemp();
                GenerateWaveforms(force: true);
                DrawPage();
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

        #region Events

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuGenerate_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            string reclist = Reclist is null || Reclist.VoicebankPath is null ? "" : Reclist.VoicebankPath;
            if (!loaded || DoEvenIfUnsaved())
                if ((sender as MenuItem).Tag.ToString() == "New")
                    OpenProjectWindow(reclist, Settings.WavSettings, Settings.ProjectFile);
                else
                    OpenProjectWindow(reclist, Settings.WavSettings, Settings.ProjectFile, true);
        }

        private void NextPage(object sender, RoutedEventArgs e)
        {
            SetPage(PageCurrent + 1);
        }

        private void PrevPage(object sender, RoutedEventArgs e)
        {
            SetPage(PageCurrent - 1);
        }
        
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ScrollContent();
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        private void Button_SetMode(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.Content.ToString() == "D") SetMode(WavConfigPoint.D);
            else if (button.Content.ToString() == "V") SetMode(WavConfigPoint.V);
            else if (button.Content.ToString() == "C") SetMode(WavConfigPoint.C);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.V))
                SetMode(WavConfigPoint.V);
            else if (Keyboard.IsKeyDown(Key.C))
                SetMode(WavConfigPoint.C);
            else if (Keyboard.IsKeyDown(Key.D))
                SetMode(WavConfigPoint.D);
            else if (Keyboard.IsKeyDown(Key.OemOpenBrackets))
                PrevPage(new object(), new RoutedEventArgs());
            else if (Keyboard.IsKeyDown(Key.OemCloseBrackets))
                NextPage(new object(), new RoutedEventArgs());
            else if (Keyboard.IsKeyDown(Key.OemPlus))
                SetItemsOnPage((byte)(ItemsOnPage + 1));
            else if (Keyboard.IsKeyDown(Key.OemMinus))
                SetItemsOnPage((byte)(ItemsOnPage - 1));
            else if (Keyboard.IsKeyDown(Key.Back))
                ToggleTools();
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.S))
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        SaveAs();
                    else
                        Save();
                }
                if (Keyboard.IsKeyDown(Key.N))
                    if (DoEvenIfUnsaved())
                        OpenProjectWindow(Reclist.VoicebankPath, Settings.WavSettings, Settings.ProjectFile);

                if (Keyboard.IsKeyDown(Key.O))
                    if (DoEvenIfUnsaved())
                        OpenProjectWindow(Reclist.VoicebankPath, Settings.WavSettings, Settings.ProjectFile, true);

                if (Keyboard.IsKeyDown(Key.G))
                    Generate();

                if (Keyboard.IsKeyDown(Key.U))
                    FindUmcompleted();

                if (Keyboard.IsKeyDown(Key.F))
                    FindWav();
            }
            else if (Keyboard.IsKeyDown(Key.Enter))
                Focus();
        }

        private void FadeChanged(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            int value;
            if (!int.TryParse(box.Text, out value) || value < 0)
            {
                box.Undo();
                return;
            }
            box.Text = value.ToString();
            if (box.Tag.ToString() == "V") SetFade(WavConfigPoint.V, value);
            if (box.Tag.ToString() == "C") SetFade(WavConfigPoint.C, value);
            if (box.Tag.ToString() == "D") SetFade(WavConfigPoint.D, value);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateWaveforms();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (loaded && !DoEvenIfUnsaved("Все равно выйти?"))
                e.Cancel = true;
            else
            {
                Settings.WindowSize = new System.Drawing.Point((int)Width, (int)Height);
                Settings.WindowPosition = new System.Drawing.Point((int)Left, (int)Top);
                Settings.IsMaximized = WindowState == WindowState.Maximized;
                Properties.Settings.Default.Save();
            }
        }

        private void AliasChanged(object sender, RoutedEventArgs e)
        {
            WavControl.Suffix = TextBoxSuffix.Text;
            WavControl.Prefix = TextBoxPrefix.Text;
        }

        private void Button_HideTools(object sender, RoutedEventArgs e)
        {
            ToggleTools();
        }

        private void ToggleWaveform_Click(object sender, RoutedEventArgs e)
        {
            ToggleWaveform();
        }

        private void ToggleSpectrum_Click(object sender, RoutedEventArgs e)
        {
            ToggleSpectrum();
        }

        private void TogglePitch_Click(object sender, RoutedEventArgs e)
        {
            TogglePitch();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            SetPage(PageCurrent + 1);
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            SetPage(PageCurrent - 1);
        }

        private void MoreItems_Click(object sender, RoutedEventArgs e)
        {
            SetItemsOnPage((byte)(ItemsOnPage + 1));
        }

        private void LessItems_Click(object sender, RoutedEventArgs e)
        {
            SetItemsOnPage((byte)(ItemsOnPage - 1));
        }

        private void ToggleToolsPanel_Click(object sender, RoutedEventArgs e)
        {
            ToggleTools();
        }

        private void TextBoxMultiplier_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (float.TryParse(TextBoxMultiplier.Text, out float value))
                SetWaveformAmplitudeMultiplayer(value);
            TextBoxMultiplier.Text = Settings.WAM.ToString("f2");

        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Space) && e.LeftButton == MouseButtonState.Pressed)
            {
                ContentMove(e.GetPosition(this));
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = null;
            }
            PrevMousePosition = e.GetPosition(this);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBoxPage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (int.TryParse(TextBoxPage.Text, out int page))
                SetPage(page - 1);
            else TextBoxPage.Text = PageCurrent.ToString();
        }

        private void TextBoxItemsOnPage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (byte.TryParse(TextBoxItemsOnPage.Text, out byte items))
                SetItemsOnPage( (byte)(items) );
            else TextBoxItemsOnPage.Text = ItemsOnPage.ToString();
        }

        private void MenuFindUncompleted_Click(object sender, RoutedEventArgs e)
        {
            FindUmcompleted();
        }

        private void MenuFindWav_Click(object sender, RoutedEventArgs e)
        {
            FindWav();
        }

        private void ButtonLoadVoicebank_Click(object sender, RoutedEventArgs e)
        {
            ChangeVoicebank();
        }

        private void ButtonLoadSettings_Click(object sender, RoutedEventArgs e)
        {
            ChangeSettings();
        }

        #endregion


    }
}
