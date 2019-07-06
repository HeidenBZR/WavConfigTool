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
using WavConfigTool.Classes;
using WavConfigTool.Tools;
using WavConfigTool.Windows;

namespace WavConfigTool.Windows
{


    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        //public static void MessageBoxError(Exception ex, string name)
        //{
        //    MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", name,
        //        MessageBoxButton.OK, MessageBoxImage.Error);
        //}

        //void ScrollView()
        //{
        //    double offset = WavControl.MostLeft - 100 * WavControl.ScaleX;
        //    if (offset < 0) offset = 0;
        //    ScrollViewer.ScrollToHorizontalOffset(offset);
        //    ScrollContent(offset);
        //}

        //void ScrollContent(double offset = -1)
        //{
        //    if (offset == -1)
        //        offset = ScrollViewer.HorizontalOffset;
        //    for (int i = Settings.CurrentPage * Settings.ItemsOnPage; i < (Settings.CurrentPage + 1) * Settings.ItemsOnPage; i++)
        //        if (i < WavControls.Count)
        //            WavControls[i].LabelName.Margin = new Thickness(offset, 0, 0, 0);
        //}


        //bool OpenProjectWindow(string voicebank = "", string wavsettings = "", string path = "", bool open = false)
        //{
        //    try
        //    {
        //        ClearWavcontrols();
        //        ProjectWindow project = new ProjectWindow(voicebank, wavsettings, path, open);
        //        project.ShowDialog();
        //        if (project.Result == Result.Cancel)
        //        {
        //            DrawPageAsync();
        //            return true;
        //        }
        //        if (project.Result == Result.Close)
        //            return false;
        //        else if (project.Result == Result.Open)
        //            if (Open(project.Settings, project.Path))
        //                return true;
        //            else { }
        //        else
        //        {
        //            NewProject(project.Settings, project.Voicebank);
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBoxError(ex, "Error on open project window");
        //        return false;
        //    }
        //}


        //public static string SaveOtoDialog()
        //{
        //    try
        //    {

        //        SaveFileDialog dialog = new SaveFileDialog();
        //        dialog.Filter = "oto.ini file (oto.ini)|*oto*.ini";
        //        dialog.InitialDirectory = Reclist.Current.VoicebankPath;
        //        dialog.FileName = "oto.ini";
        //        var result = dialog.ShowDialog();
        //        if (!result.HasValue || !result.Value) return "";
        //        return dialog.FileName;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on save oto dialog");
        //        return "";
        //    }
        //}

        //public void SaveFileDialog()
        //{
        //    throw new NotImplementedException();

            //string lastpath = Settings.ProjectFile;
            //SaveFileDialog openFileDialog = new SaveFileDialog();
            //openFileDialog.Filter = "WavConfig Project (*.wconfig)|*.wconfig";
            //openFileDialog.RestoreDirectory = true;
            //openFileDialog.ShowDialog();
            //if (openFileDialog.FileName == "") return;
            //Settings.ProjectFile = openFileDialog.FileName;
            //Save();
            //File.Delete(lastpath);
            //IsUnsaved = false;
        //}


        //public void ChangeVoicebankDialog()
        //{
        //    throw new NotImplementedException();

            //string old_s = Settings.WavSettings;
            //var s = ProjectWindow.SettingsDialog();
            //if (s != null)
            //    if (File.Exists(s))
            //    {
            //        Settings.WavSettings = s;
            //        ClearWavcontrols();
            //        loaded = (CheckSettings() && (Settings.IsUnsaved && OpenBackup()
            //            || ReadProject(Settings.ProjectFile)));
            //        if (!loaded)
            //        {
            //            Settings.WavSettings = old_s;
            //            ClearWavcontrols();
            //            loaded = (CheckSettings() && (Settings.IsUnsaved && OpenBackup()
            //                || ReadProject(Settings.ProjectFile)));
            //            MessageBox.Show("WavSettings changing failed");
            //        }
            //    }
        //}

        //public void GenerateOtoDialog()
        //{
        //    throw new NotImplementedException();
            //string oto = Project.GenerateOto();
            //string filename = SaveOtoDialog();
            //File.WriteAllText(filename, oto, Encoding.ASCII);
        //}
        //public static string ProjectDialog(string initialFile = "")
        //{
        //    throw new NotImplementedException();
            //try
            //{
            //    OpenFileDialog openFileDialog = new OpenFileDialog();
            //    openFileDialog.Filter = "WavConfig Project (*.wconfig)|*.wconfig";
            //    openFileDialog.RestoreDirectory = false;
            //    if (initialFile != null && initialFile != "")
            //        if (Directory.Exists(System.IO.Path.GetDirectoryName(initialFile)))
            //            openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(initialFile);
            //    var result = openFileDialog.ShowDialog();
            //    if (result == null || !result.Value || openFileDialog.FileName == "")
            //        return null;
            //    else
            //        return openFileDialog.FileName;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on project openfile dialog");
            //    return null;
            //}

        //}



        //void UndrawPage()
        //{
        //    throw new NotImplementedException();
            //try
            //{
            //    Dispatcher.Invoke(() => { WaveControlStackPanel.Children.Clear(); });
            //    var min = Settings.ItemsOnPage * Settings.CurrentPage;
            //    var max = min + Settings.ItemsOnPage;
            //    var count = WavControls.Count;
            //    for (int i = min; i < max && i < count; i++)
            //        if (WavControls[i].Recline.IsEnabled)
            //            WavControls[i].Undraw();
            //}
            //catch (Exception ex)
            //{
            //    MessageBoxError(ex, "Error on draw page");
            //}
        //}

        //void DrawPage()
        //{
        //    throw new NotImplementedException();
            //try
            //{
            //    var min = Settings.ItemsOnPage * Settings.CurrentPage;
            //    var max = min + Settings.ItemsOnPage;
            //    var count = WavControls.Count;
            //    for (int i = min; i < max && i < count; i++)
            //    {
            //        if (WavControls[i].Recline.IsEnabled)
            //        {
            //            Dispatcher.Invoke(() => { WaveControlStackPanel.Children.Add(WavControls[i]); });
            //            WavControls[i].Draw();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBoxError(ex, "Error on draw page");
            //}
        //}


        //async void DrawPageAsync(bool manual = true)
        //{
        //    throw new NotImplementedException();
            //try
            //{
            //    if (!IsInitialized) return;
            //    //CanvasLoading.Visibility = Visibility.Visible;
            //    WaveControlStackPanel.Children.Clear();
            //    WaveControlStackPanel.Children.Capacity = 0;
            //    SetPageTotal();
            //    DrawPage();
            //    //await Task.Run(() => { DrawPage(count); });
            //    //CanvasLoading.Visibility = Visibility.Hidden;
            //}
            //catch (Exception ex)
            //{
            //    MessageBoxError(ex, "Error on Draw Page Async");
            //}
        //}

        //async void InitWavcontrols(bool force)
        //{
        //    throw new NotImplementedException();
            //if (WavControls is null)
            //    return;
            //try
            //{

            //    var min = Settings.CurrentPage * Settings.ItemsOnPage;
            //    var max = (Settings.CurrentPage + 1) * Settings.ItemsOnPage;
            //    var count = WavControls.Count;


            //    Parallel.For(min, max, delegate (int i)
            //    {
            //        if (max < count)
            //            WavControls[i].Init(true);
            //    });
            //    await Task.Run(delegate
            //    {
            //        Parallel.For(0, count, delegate (int i)
            //        {
            //            if (i < min || i >= max)
            //                WavControls[i].Init();
            //        });
            //    });

            //}
            //catch (Exception ex)
            //{
            //    MessageBoxError(ex, "Error on GenerateWaveformsAsync");
            //    CanvasLoading.Visibility = Visibility.Hidden;
            //}
        //}

        //async void InitWavcontrolsAsync(bool force = true)
        //{
        //    throw new NotImplementedException();
            //InitWavcontrols(force);
        //}
        #region Events

        //private void MenuSave_Click(object sender, RoutedEventArgs e)
        //{
        //    Save();
        //}

        //private void MenuGenerate_Click(object sender, RoutedEventArgs e)
        //{
        //    GenerateOto();
        //}

        //private void MenuNew_Click(object sender, RoutedEventArgs e)
        //{
        //    string reclist = Reclist is null || Reclist.VoicebankPath is null ? "" : Reclist.VoicebankPath;
        //    if (!loaded || DoEvenIfUnsaved())
        //        if ((sender as MenuItem).Tag.ToString() == "New")
        //            OpenProjectWindow(reclist, Settings.WavSettings, Settings.ProjectFile);
        //        else
        //            OpenProjectWindow(reclist, Settings.WavSettings, Settings.ProjectFile, true);
        //}

        //private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    if (!IsLoaded) return;
        //    ScrollContent();
        //}

        //private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveAs();
        //}

        //private void Button_SetMode(object sender, RoutedEventArgs e)
        //{
        //    Button button = sender as Button;
        //    if (button.Content.ToString() == "D") SetMode(WavConfigPoint.D);
        //    else if (button.Content.ToString() == "V") SetMode(WavConfigPoint.V);
        //    else if (button.Content.ToString() == "C") SetMode(WavConfigPoint.C);
        //}

        //private void Window_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (Keyboard.IsKeyDown(Key.V))
        //        SetMode(WavConfigPoint.V);
        //    else if (Keyboard.IsKeyDown(Key.C))
        //        SetMode(WavConfigPoint.C);
        //    else if (Keyboard.IsKeyDown(Key.D))
        //        SetMode(WavConfigPoint.D);
        //    else if (Keyboard.IsKeyDown(Key.OemOpenBrackets))
        //        SetPage(Settings.CurrentPage - 1);
        //    else if (Keyboard.IsKeyDown(Key.OemCloseBrackets))
        //        SetPage(Settings.CurrentPage + 1);
        //    else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        //    {
        //        if (Keyboard.IsKeyDown(Key.S))
        //        {
        //            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        //                SaveAs();
        //            else
        //                Save();
        //        }
        //        else if (Keyboard.IsKeyDown(Key.OemPlus))
        //            SetItemsOnPage((byte)(Settings.ItemsOnPage + 1));
        //        else if (Keyboard.IsKeyDown(Key.OemMinus))
        //            SetItemsOnPage((byte)(Settings.ItemsOnPage - 1));
        //        else if (Keyboard.IsKeyDown(Key.Back))
        //            ToggleTools();
        //        if (Keyboard.IsKeyDown(Key.N))
        //            if (DoEvenIfUnsaved())
        //                OpenProjectWindow(Reclist.VoicebankPath, Settings.WavSettings, Settings.ProjectFile);

        //        if (Keyboard.IsKeyDown(Key.O))
        //            if (DoEvenIfUnsaved())
        //                OpenProjectWindow(Reclist.VoicebankPath, Settings.WavSettings, Settings.ProjectFile, true);

        //        if (Keyboard.IsKeyDown(Key.G))
        //            GenerateOto();

        //        if (Keyboard.IsKeyDown(Key.U))
        //            FindUmcompleted();

        //        if (Keyboard.IsKeyDown(Key.F))
        //            FindWav();
        //    }
        //    else if (Keyboard.IsKeyDown(Key.Enter))
        //        ScrollViewer.Focus();
        //}

        //private void FadeChanged(object sender, RoutedEventArgs e)
        //{
        //    TextBox box = sender as TextBox;
        //    int value;
        //    if (!int.TryParse(box.Text, out value) || value < 0)
        //    {
        //        box.Undo();
        //        return;
        //    }
        //    box.Text = value.ToString();
        //    if (box.Tag.ToString() == "V") SetFade(WavConfigPoint.V, value);
        //    if (box.Tag.ToString() == "C") SetFade(WavConfigPoint.C, value);
        //    if (box.Tag.ToString() == "D") SetFade(WavConfigPoint.D, value);
        //}

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //GenerateWaveformsAsync();
        //}

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    if (loaded && !DoEvenIfUnsaved("Все равно выйти?"))
        //        e.Cancel = true;
        //    else
        //    {
        //        Settings.WindowSize = new System.Drawing.Point((int)Width, (int)Height);
        //        Settings.WindowPosition = new System.Drawing.Point((int)Left, (int)Top);
        //        Settings.IsMaximized = WindowState == WindowState.Maximized;
        //        Properties.Settings.Default.Save();
        //        ClearTemp();
        //    }
        //}

        //private void AliasChanged(object sender, RoutedEventArgs e)
        //{
        //    WavControl.Suffix = TextBoxSuffix.Text;
        //    WavControl.Prefix = TextBoxPrefix.Text;
        //}

        //private void Button_HideTools(object sender, RoutedEventArgs e)
        //{
        //    ToggleTools();
        //}

        //private void ToggleWaveform_Click(object sender, RoutedEventArgs e)
        //{
        //    ToggleWaveform();
        //}

        //private void ToggleSpectrum_Click(object sender, RoutedEventArgs e)
        //{
        //    ToggleSpectrum();
        //}

        //private void TogglePitch_Click(object sender, RoutedEventArgs e)
        //{
        //    TogglePitch();
        //}

        //private void NextPage_Click(object sender, RoutedEventArgs e)
        //{
        //    SetPage(Settings.CurrentPage + 1);
        //}

        //private void PrevPage_Click(object sender, RoutedEventArgs e)
        //{
        //    SetPage(Settings.CurrentPage - 1);
        //}

        //private void MoreItems_Click(object sender, RoutedEventArgs e)
        //{
        //    SetItemsOnPage((byte)(Settings.ItemsOnPage + 1));
        //}

        //private void LessItems_Click(object sender, RoutedEventArgs e)
        //{
        //    SetItemsOnPage((byte)(Settings.ItemsOnPage - 1));
        //}

        //private void ToggleToolsPanel_Click(object sender, RoutedEventArgs e)
        //{
        //    ToggleTools();
        //}

        //private void TextBoxMultiplier_TextChanged(object sender, RoutedEventArgs e)
        //{
        //    if (!IsLoaded) return;
        //    if (float.TryParse(TextBoxMultiplier.Text, out float value))
        //        SetWaveformAmplitudeMultiplayer(value);
        //    TextBoxMultiplier.Text = Settings.WAM.ToString("f2");

        //}

        //private void Window_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (Keyboard.IsKeyDown(Key.Space) && e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        ContentMove(e.GetPosition(this));
        //        this.Cursor = Cursors.Hand;
        //    }
        //    else
        //    {
        //        this.Cursor = null;
        //    }
        //    PrevMousePosition = e.GetPosition(this);
        //}

        //private void MenuExit_Click(object sender, RoutedEventArgs e)
        //{
        //    Close();
        //}

        //private void TextBoxPage_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (!IsLoaded) return;
        //    if (int.TryParse(TextBoxPage.Text, out int page))
        //        SetPage(page - 1);
        //    else TextBoxPage.Text = Settings.CurrentPage.ToString();
        //}

        //private void TextBoxItemsOnPage_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (!IsLoaded) return;
        //    if (byte.TryParse(TextBoxItemsOnPage.Text, out byte items))
        //        SetItemsOnPage( (byte)(items) );
        //    else TextBoxItemsOnPage.Text = Settings.ItemsOnPage.ToString();
        //}

        //private void MenuFindUncompleted_Click(object sender, RoutedEventArgs e)
        //{
        //    FindUmcompleted();
        //}

        //private void MenuFindWav_Click(object sender, RoutedEventArgs e)
        //{
        //    FindWav();
        //}

        //private void ButtonLoadVoicebank_Click(object sender, RoutedEventArgs e)
        //{
        //    ChangeVoicebank();
        //}

        //private void ButtonLoadSettings_Click(object sender, RoutedEventArgs e)
        //{
        //    ChangeSettings();
        //}

        //private void Decay_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (int.TryParse(TextBoxDecay.Text, out int Decay))
        //        if (!SetDecay(Decay))
        //            TextBoxDecay.Text = WavControl.VowelDecay.ToString("f0");
        //}

        #endregion
    }
}
