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

        public readonly Version Version = new Version(0, 1, 5, 2);

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


        #region Events

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateOto();
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
                    GenerateOto();

                if (Keyboard.IsKeyDown(Key.U))
                    FindUmcompleted();

                if (Keyboard.IsKeyDown(Key.F))
                    FindWav();
            }
            else if (Keyboard.IsKeyDown(Key.Enter))
                ScrollViewer.Focus();
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
            //GenerateWaveformsAsync();
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
                ClearTemp();
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
