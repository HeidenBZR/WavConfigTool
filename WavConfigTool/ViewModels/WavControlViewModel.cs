using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WavConfigTool.Classes;
using WavConfigTool.Tools;

namespace WavConfigTool.ViewModels
{
    public class WavControlViewModel : ViewModelBase
    {
        private ProjectLine _projectLine = new ProjectLine(new Recline(new Reclist("?"), "default.wav"));

        public ProjectLine ProjectLine
        {
            get => _projectLine;
            set
            {
                _projectLine = value;
                _projectLine.ProjectLineChanged += delegate
                {
                    RaisePropertiesChanged(
                        () => Filename,
                        () => Phonemes,
                        () => IsCompleted,
                        () => WavImagePath,
                        () => WavImage
                    );
                    RaisePropertiesChanged(
                        () => LoadingProperty,
                        () => DisabledProperty,
                        () => EnabledProperty
                    );
                    ApplyPoints();
                    ApplyZones();
                };
            }
        }

        public List<WavPointViewModel> ConsonantPoints { get; set; } = new List<WavPointViewModel>();
        public List<WavPointViewModel> VowelPoints { get; set; } = new List<WavPointViewModel>();
        public List<WavPointViewModel> RestPoints { get; set; } = new List<WavPointViewModel>();

        public List<WavZoneViewModel> ConsonantZones { get; set; } = new List<WavZoneViewModel>();
        public List<WavZoneViewModel> VowelZones { get; set; } = new List<WavZoneViewModel>();
        public List<WavZoneViewModel> RestZones { get; set; } = new List<WavZoneViewModel>();

        public string Filename { get => ProjectLine.Recline.Filename; }
        public List<Phoneme> Phonemes { get => ProjectLine.Recline.Phonemes; }

        public Visibility LoadingProperty { get => IsLoading ? Visibility.Visible : Visibility.Hidden; }
        public Visibility DisabledProperty { get => ProjectLine.IsEnabled ? Visibility.Hidden : Visibility.Visible; } 
        public Visibility EnabledProperty { get => ProjectLine.IsEnabled ? Visibility.Visible : Visibility.Hidden; } 

        public bool IsCompleted { get => ProjectLine.IsCompleted; }
        public bool IsLoading { get; set; } = false;
        public int Number { get; set; }

        public int Width { get; set; } = 1000;
        public string WavImagePath { get; set; } = "";
        public bool IsImageEnabled { get; set; } = false;
        public BitmapImage WavImage
        {
            get
            {
                if (IsImageEnabled)
                    return new BitmapImage(new Uri(WavImagePath));
                else
                    return null;
            }
        }

        public WavControlViewModel() { }
        public WavControlViewModel(ProjectLine projectLine)
        {
            ProjectLine = projectLine;
            if (ProjectLine.IsEnabled)
                Width = (int)(ProjectLine.WaveForm.Length / ProjectLine.WaveForm.BitRate * Settings.ScaleX);
        }

        public void Load()
        {
            LoadImageAsync();
            ApplyPoints();
            ApplyZones();
        }

        public async void LoadImageAsync()
        {
            await Task.Run(delegate { LoadImage(); }).ConfigureAwait(true);
        }

        public void LoadImage()
        {
            IsImageEnabled = false;
            if (!ProjectLine.IsEnabled)
                return;

            string new_image = Path.Combine(Settings.TempDir, $"{ProjectLine.WavImageHash}.jpg");
            if (new_image == WavImagePath)
            {
                if (File.Exists(WavImagePath))
                    IsImageEnabled = true;
                return;
            }
            if (File.Exists(WavImagePath))
            {
                try
                {
                    File.Delete(WavImagePath);
                }
                catch (UnauthorizedAccessException ex) { }
            }



            IsLoading = true;
            RaisePropertyChanged(() => IsLoading);

            WavImagePath = "";
            var points = ProjectLine.WaveForm.GetAudioPoints();
            ProjectLine.WaveForm.PointsToImage(
                    points,
                    Width,
                    100,
                    new_image,
                    new System.Drawing.Pen(System.Drawing.ColorTranslator.FromHtml(WaveForm.WAV_ZONE_COLOR))
            );
            if (File.Exists(new_image))
            {
                WavImagePath = new_image;
                IsImageEnabled = true;
            }
            RaisePropertyChanged(() => WavImage);

            IsLoading = false;
            RaisePropertyChanged(() => IsLoading);
        }


        public void ApplyPoints()
        {
            ConsonantPoints = new List<WavPointViewModel>();
            for (int i = 0; i < ProjectLine.ConsonantPoints.Count; i++)
            {
                ConsonantPoints.Add(
                    new WavPointViewModel(ProjectLine.ConsonantPoints[i] * Settings.ScaleX,
                    WavConfigPoint.C,
                   ProjectLine.Recline.Consonants.Count * 2 > i && i % 2 == 0 ? ProjectLine.Recline.Consonants[i / 2] : ""));
            }

            VowelPoints = new List<WavPointViewModel>();
            for (int i = 0; i < ProjectLine.VowelPoints.Count; i++)
            {
                VowelPoints.Add(
                    new WavPointViewModel(ProjectLine.VowelPoints[i] * Settings.ScaleX,
                    WavConfigPoint.V,
                   ProjectLine.Recline.Vowels.Count * 2 > i && i % 2 == 0 ? ProjectLine.Recline.Vowels[i / 2] : ""));
            }

            RestPoints = new List<WavPointViewModel>();
            for (int i = 0; i < ProjectLine.RestPoints.Count; i++)
                RestPoints.Add(
                    new WavPointViewModel(ProjectLine.RestPoints[i] * Settings.ScaleX,
                    WavConfigPoint.R,
                    ""));

            RaisePropertiesChanged(
                () => ConsonantPoints,
                () => VowelPoints,
                () => RestPoints
            );
        }

        /// <summary>
        /// Переносит зоны из ProjectLine в ModelView
        /// </summary>
        private void ApplyZones()
        {
            VowelZones = new List<WavZoneViewModel>();
            foreach (var zone in ProjectLine.VowelZones)
                VowelZones.Add(new WavZoneViewModel(WavConfigPoint.V, zone.In * Settings.ScaleX, zone.Out * Settings.ScaleX));
            ConsonantZones = new List<WavZoneViewModel>();
            foreach (var zone in ProjectLine.ConsonantZones)
                ConsonantZones.Add(new WavZoneViewModel(WavConfigPoint.C, zone.In * Settings.ScaleX, zone.Out * Settings.ScaleX));
            RestZones = new List<WavZoneViewModel>();
            foreach (var zone in ProjectLine.RestZones)
                RestZones.Add(new WavZoneViewModel(WavConfigPoint.R, zone.In * Settings.ScaleX, zone.Out * Settings.ScaleX));
            RaisePropertiesChanged(
                () => VowelZones,
                () => ConsonantZones,
                () => RestZones
            );
        }

        public override string ToString()
        {
            if (ProjectLine == null || ProjectLine.Recline == null)
                return "{WavControlViewModel}";
            else 
                return $"{ProjectLine.Recline.Name} : WavControlViewModel";
        }
    }
}
