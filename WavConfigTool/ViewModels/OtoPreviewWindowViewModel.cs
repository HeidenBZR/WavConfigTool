using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.ViewModels
{
    class OtoPreviewWindowViewModel
    {
        public ObservableCollection<OtoPreviewControlViewModel> OtoPreviewControlViewModels { get; set; }
        public WavControlViewModel WavControlViewModel { get; set; }

        public OtoPreviewWindowViewModel() { }

        public OtoPreviewWindowViewModel(WavControlViewModel wavControlViewModel)
        {
            WavControlViewModel = wavControlViewModel;
            DrawOto();
        }

        public void DrawOto()
        {
            string oto = WavControlViewModel.GenerateOto();
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

    }
}
