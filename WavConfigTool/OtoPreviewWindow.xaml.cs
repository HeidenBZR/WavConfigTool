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
    /// Логика взаимодействия для OtoPreviewWindow.xaml
    /// </summary>
    public partial class OtoPreviewWindow : Window
    {
        public WavControl CurrentWavcontrol;
        public WavControl SourceWavConrol;
        List<OtoPreviewControl> Controls;
        double MostLeft = 0;

        public OtoPreviewWindow(string filename)
        {
            InitializeComponent();
            Controls = new List<OtoPreviewControl>();
            Title = $"Oto Preview [{filename}]";
            Loaded += delegate
            {
                DrawOto();
                InitScroll();
            };
        }

        public void DrawOto()
        {
            CurrentWavcontrol.Recline.Reclist.Aliases.Clear();
            OtoPreviewView.Items.Clear();
            Controls.Clear();
            string oto = SourceWavConrol.GenerateOto();
            foreach (string line in oto.Split(new[] { '\r', '\n' }))
            {
                if (line.Length == 0) continue;
                var ops = line.Split('=');
                var ops2 = ops[1].Split(',');
                var ops3 = ops2.Skip(1);
                int[] opsi = ops3.Select(n => int.Parse(n)).ToArray();
                if (opsi[0] < 0 || opsi[1] < 0 || opsi[3] < 0 || opsi[4] < 0)
                    continue;
                OtoPreviewControl control = new OtoPreviewControl(CurrentWavcontrol.WavImage.Source, ops2[0], opsi, CurrentWavcontrol.Length);
                Add(control);
            }
        }

        public void SetWavControl(WavControl wavControl)
        {
            SourceWavConrol = wavControl;
            CurrentWavcontrol = new WavControl(wavControl.Recline)
            {
                Ds = wavControl.Ds,
                Vs = wavControl.Vs,
                Cs = wavControl.Cs,
                ImagePath = wavControl.ImagePath,
                IsEnabled = wavControl.IsEnabled,
                IsCompleted = wavControl.IsCompleted,
                IsToDraw = wavControl.IsToDraw,
                Length = wavControl.Length,
                AllowOtoPreview = false
            };
            CurrentWavcontrol.MenuItemPreview.IsEnabled = false;
            WavControlScrollViewer.Content = CurrentWavcontrol;
            CurrentWavcontrol.WavControlChanged += delegate ()
            {
                SourceWavConrol.Ds = CurrentWavcontrol.Ds;
                DrawOto();
            };
            CurrentWavcontrol.WavControlChanged += MainWindow.Current.SaveBackup;
            CurrentWavcontrol.WavControlChanged += () =>
            {
                if (Settings.ProjectFile != MainWindow.Current.TempProject)
                    MainWindow.Current.Save();
            };
            CurrentWavcontrol.Draw();
        }

        public void Add(OtoPreviewControl control)
        {
            Controls.Add(control);
            OtoPreviewView.Items.Add(control);
            if (control.Left < MostLeft) MostLeft = control.Left;
        }

        void InitScroll()
        {
            MostLeft = 99999;
            var offset = ScrollViewer.ScrollableWidth / 2 - ScrollViewer.ActualWidth / 2;
            if (offset < 0) offset = 0;
            offset = MostLeft - 20;
            ScrollViewer.ScrollToHorizontalOffset(offset);
            ScrollContent(offset);
        }

        void ScrollContent()
        {
            foreach (var control in Controls)
                control.Filename.Margin = new Thickness(ScrollViewer.HorizontalOffset, 0, 0, 0);
            WavControlScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset);
        }
        void ScrollContent(double offset)
        {
            foreach (var control in Controls)
                control.Filename.Margin = new Thickness(offset, 0, 0, 0);
            WavControlScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset);
        }


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollContent();
        }

        private void WavControlCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.V))
                MainWindow.Current.SetMode(WavConfigPoint.V);
            else if (Keyboard.IsKeyDown(Key.C))
                MainWindow.Current.SetMode(WavConfigPoint.C);
            else if (Keyboard.IsKeyDown(Key.D))
                MainWindow.Current.SetMode(WavConfigPoint.D);

            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.S))
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        MainWindow.Current.SaveAs();
                    else
                        MainWindow.Current.Save();
                }
            }
        }
    }
}
