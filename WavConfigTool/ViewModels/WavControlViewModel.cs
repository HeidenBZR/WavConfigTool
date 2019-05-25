using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WavConfigTool.Classes;
using WavConfigTool.Tools;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WavConfigTool.ViewTools;

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
                };
            }
        }

        public double Length
        {
            get
            {
                return WavImage == null ? 0 : WavImage.Width;
            }
        }

        public List<WavPointViewModel> ConsonantPoints { get; set; } = new List<WavPointViewModel>();
        public List<WavPointViewModel> VowelPoints { get; set; } = new List<WavPointViewModel>();
        public List<WavPointViewModel> RestPoints { get; set; } = new List<WavPointViewModel>();

        public List<WavZoneViewModel> ConsonantZones { get { return GetZones(PhonemeType.Consonant, ConsonantPoints); } }
        public List<WavZoneViewModel> VowelZones { get { return GetZones(PhonemeType.Vowel, VowelPoints); } }
        public List<WavZoneViewModel> RestZones { get { return GetRestZones(); } }

        public List<WavZoneViewModel> GetZones(PhonemeType point, List<WavPointViewModel> points)
        {
            var zones = new List<WavZoneViewModel>();
            for (int i = 0; i + 1 < points.Count; i += 2)
            {
                zones.Add(new WavZoneViewModel(point, points[i].Position, points[i + 1].Position));
            }
            return zones;
        }

        public List<WavZoneViewModel> GetRestZones()
        {
            List<WavPointViewModel>  local = RestPoints.GetRange(0, RestPoints.Count);
            if (local.Count > 1)
            {
                var p = new WavPointViewModel(0, PhonemeType.Rest, "");
                local = local.Prepend(p).ToList();
                p = new WavPointViewModel(Length, PhonemeType.Rest, "");
                local = local.Append(p).ToList();
            }
            return GetZones(PhonemeType.Rest, local);
        }

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
                Width = (int)(Settings.RealToViewX(ProjectLine.WaveForm.Length / ProjectLine.WaveForm.BitRate));
        }

        public void Load()
        {
            LoadImageAsync();
            ApplyPoints();
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
            FillPoints(PhonemeType.Consonant);
            FillPoints(PhonemeType.Rest);
            FillPoints(PhonemeType.Vowel);
            PointsChanged();
        }

        public void PointsChanged()
        {
            RaisePropertiesChanged(
                () => ConsonantPoints,
                () => VowelPoints,
                () => RestPoints
            );
            RaisePropertiesChanged(
                () => ConsonantZones,
                () => VowelZones,
                () => RestZones
            );
        }

        public List<WavPointViewModel> PointsOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? ConsonantPoints :
                (type == PhonemeType.Rest ? RestPoints : VowelPoints);
        }


        public List<WavZoneViewModel> ZonesOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? ConsonantZones :
                (type == PhonemeType.Rest ? RestZones : VowelZones);
        }

        public string GetPointLabel(PhonemeType type, int i)
        {
            var phonemes = ProjectLine.Recline.PhonemesOfType(type);
            return type == PhonemeType.Rest ? "" : 
                (phonemes.Count * 2 > i && i % 2 == 0 ? phonemes[i / 2] : "");
        }

        private void FillPoints(PhonemeType type)
        {
            var points = PointsOfType(type);
            var projectPoints = ProjectLine.PointsOfType(type);
            points.Clear();
            for (int i = 0; i < projectPoints.Count; i++)
            {
                var point = new WavPointViewModel(
                    Settings.RealToViewX(projectPoints[i]),
                    type,
                    GetPointLabel(type, i));
                point.WavPointChanged += delegate (double position1, double position2)
                {
                    MovePoint(position1, position2, type);
                };
                point.WavPointDeleted += delegate (double position)
                {
                    DeletePoint(position, type);
                };
                points.Add(point);
            }

        }
        
        public void AddPoint(double position, PhonemeType type)
        {
            ProjectLine.AddPoint(Settings.ViewToRealX(position), type);
            ApplyPoints();
        }

        public void MovePoint(double position1, double position2, PhonemeType type)
        {
            ProjectLine.MovePoint(Settings.ViewToRealX(position1), Settings.ViewToRealX(position2), type);
            ApplyPoints();
        }

        public void DeletePoint(double position, PhonemeType type)
        {
            ProjectLine.DeletePoint(Settings.ViewToRealX(position), type);
            ApplyPoints();
        }
        
        public override string ToString()
        {
            if (ProjectLine == null || ProjectLine.Recline == null)
                return "{WavControlViewModel}";
            else 
                return $"{ProjectLine.Recline.Name} : WavControlViewModel";
        }


        public ICommand WavControlClickCommand
        {
            get
            {
                return new DelegateCommand<Point>(WavControlClick, CanWavControlClick);
            }
        }

        public void WavControlClick(Point point)
        {
            AddPoint((int)point.X, Settings.Mode);
        }

        public bool CanWavControlClick(Point point)
        {
            return ProjectLine.IsEnabled && !ProjectLine.IsCompleted;
        }
    }
}
