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
        public static WavConfigPoint Mode = WavConfigPoint.V;

        int PageCurrent = 0;
        int PageTotal = 0;
        byte ItemsOnPage = 4;

        Point PrevMousePosition;

        public delegate void ProjectLoadedEventHandler();
        public event ProjectLoadedEventHandler ProjectLoaded;

        bool IsUnsaved = false;

        public MainWindow()
        {
            ClearTemp();
            InitializeComponent();
            PrevMousePosition = Mouse.GetPosition(this);
            ProjectLoaded += delegate { GenerateWaveforms(); };
            if (CheckSettings() && CheckLast()) { DrawPage(); }
            else OpenProjectWindow();
        }

        bool CheckSettings()
        {
            if (File.Exists("settings"))
            {
                string filename = File.ReadAllText("settings", Encoding.UTF8);
                if (File.Exists(filename))
                {
                    ReadSettings(filename);
                    return true;
                }
            }
            return false;
        }

        bool CheckLast()
        {
            if (File.Exists("last"))
            {
                string filename = File.ReadAllText("last", Encoding.UTF8);
                if (File.Exists(filename))
                {
                    ReadProject(filename);
                    return true;
                }
            }
            return false;
        }

        void DrawPage(bool manual = true)
        {
            if (!IsInitialized) return;
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
            if (manual)
            {
                LabelItemsOnPage.Text = ItemsOnPage.ToString();
                LabelPage.Text = (PageCurrent + 1).ToString();
                LabelPageTotal.Content = (PageTotal - 1).ToString();
            }
            ScrollViewer.ScrollToHorizontalOffset(WavControl.MostLeft - 100 * WavControl.ScaleX);
            foreach (WavControl control in WavControls)
                control.LabelName.Margin = new Thickness(WavControl.MostLeft - 100 * WavControl.ScaleX, 0, 0, 0);
        }

        void GenerateWaveforms(bool force = false)
        {
            if (!IsLoaded) return;
            foreach (WavControl control in WavControls) control.GenerateWaveform(force);
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
            WavControls.Add(control);
        }

        void OpenProjectWindow(string voicebank = "", string wavsettings = "", string path = "")
        {
            Project project = new Project(voicebank, wavsettings, path);
            while (true)
            { 
                project.ShowDialog();
                if (project.Result == Result.Cancel) return;
                ClearTemp();
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
            SetTitle();
            ProjectLoaded();
        }

        bool Open(string settings, string project)
        {
            ReadSettings(settings);
            if (File.Exists(project))
            {
                ReadProject(project);
                DrawPage();
                SetTitle();
                return true;
            }
            else MessageBox.Show("Ошибка при открытии файла проекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        void SetMode(WavConfigPoint point)
        {
            Mode = point;
            LabelMode.Content = point;
        }

        void Save()
        {
            if (Path == TempPath)
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
            File.WriteAllText(Path, text, Encoding.UTF8);
            IsUnsaved = false;
            SetTitle();
            if (File.Exists(TempPath)) File.Delete(TempPath);
        }

        void SaveAs()
        {
            string lastpath = Path;
            SaveFileDialog openFileDialog = new SaveFileDialog();
            openFileDialog.Filter = "WavConfig Project (*.wconfig)|*.wconfig";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName == "") return;
            Path = openFileDialog.FileName;
            Save();
            File.Delete(lastpath);
            IsUnsaved = false;
        }

        void SetTitle(bool unsaved = false)
        {
            Title = $"WavConfig - {System.IO.Path.GetFileName(Path)}{(unsaved? "*" : "")} [{new DirectoryInfo(Reclist.VoicebankPath).Name}]";
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
            File.WriteAllText(TempPath, text, Encoding.UTF8);
            SetTitle(true);
            IsUnsaved = true;
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
            ProjectLoaded();
        }

        void Generate()
        {
            string text = "";
            Reclist.Aliases = new List<string>();
            foreach (WavControl control in WavControls)
            {
                text += control.Generate();
            }
            File.WriteAllText(System.IO.Path.Combine(Reclist.VoicebankPath, "oto.ini"), text, Encoding.ASCII);
            MessageBox.Show("Файл oto.ini успешно сгенерирован!", "oto.ini", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void SetFade(WavConfigPoint point, int value)
        {
            if (point == WavConfigPoint.V) WavControl.VFade = value;
            else if (point == WavConfigPoint.C) WavControl.CFade = value;
            else if (point == WavConfigPoint.D) WavControl.DFade = value;
            DrawPage();
        }
        
        void ClearTemp()
        {
            foreach (string filename in Directory.GetFiles("Temp"))
                File.Delete(filename);

        }

        bool DoEvenIfUnsaved()
        {
            if (!IsUnsaved) return true;
            var result = MessageBox.Show("Имеются несохраненные изменения. Продолжить?", "Несохраненные изменения", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                File.Delete(TempPath);
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
            if (page <= PageTotal && page > 0)
            {
                PageCurrent = page;
                DrawPage(manual:false);
                if (LabelPage.Text != (page + 1).ToString())
                    LabelPage.Text = (page + 1).ToString();
            }
            //else LabelPage.Text = PageCurrent.ToString();
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
                ItemsOnPage = items;
                PageTotal = WavControls.Count / ItemsOnPage;
                if (PageCurrent > PageTotal) PageCurrent = PageTotal;
                DrawPage(manual:false);
            }
            //else LabelItemsOnPage.Text = ItemsOnPage.ToString();
        }

        void SetWaveformAmplitudeMultiplayer(float value)
        {
            if (value > 0 && value < 50f)
            {
                WavControl.WaveformAmplitudeMultiplayer = value;
                foreach (var control in WavControls) control.Undraw();
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
            if (DoEvenIfUnsaved())
                OpenProjectWindow(Reclist.VoicebankPath, WavSettings, Path);
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
                if (Keyboard.IsKeyDown(Key.N) || Keyboard.IsKeyDown(Key.O))
                    if (DoEvenIfUnsaved())
                        OpenProjectWindow(Reclist.VoicebankPath, WavSettings, Path);

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
            if (!DoEvenIfUnsaved())
                e.Cancel = true;
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
            TextBoxMultiplier.Text = WavControl.WaveformAmplitudeMultiplayer.ToString("f2");

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

        private void LabelPage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (int.TryParse(LabelPage.Text, out int page)) SetPage(page);
            else LabelPage.Text = PageCurrent.ToString();
        }

        private void LabelItemsOnPage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (byte.TryParse(LabelItemsOnPage.Text, out byte items)) SetItemsOnPage(items);
            else LabelItemsOnPage.Text = ItemsOnPage.ToString();
        }

        private void MenuFindUncompleted_Click(object sender, RoutedEventArgs e)
        {
            FindUmcompleted();
        }

        private void MenuFindWav_Click(object sender, RoutedEventArgs e)
        {
            FindWav();
        }

        #endregion

    }
}
