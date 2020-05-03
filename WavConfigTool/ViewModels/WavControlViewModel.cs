using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WavConfigTool.Classes;
using WavConfigCore;
using WavConfigCore.Tools;
using System.Linq;

namespace WavConfigTool.ViewModels
{
    public class WavControlViewModel : WavControlBaseViewModel
    {
        public ProjectLine ProjectLine { get; private set; }
        public WaveForm WaveForm { get; set; }
        public Frq Frq { get; set; }
        public ObservableCollection<FrqPointViewModel> FrqPoints { get; set; }

        public string Filename { get => ProjectLine.Recline.Name; }
        public ObservableCollection<Phoneme> Phonemes => new ObservableCollection<Phoneme>(ProjectLine.Recline.Phonemes);

        public bool IsOtoBase { get; set; } = false;
        public bool IsLoading { get; set; } = false;
        public bool IsLoaded { get; set; } = false;
        public bool IsImageEnabled { get; set; } = false;
        public bool IsReady { get; set; } = false;

        public override bool IsCompleted => ProjectLine.IsCompleted;
        public override bool IsEnabled => (bool)ProjectLine?.IsEnabled;
        public bool IsDisabled => !IsEnabled;
        public bool EditEnabled => IsEnabled && !IsLoading && IsLoaded;

        public ObservableCollection<WavPointViewModel> ConsonantPoints { get; set; } = new ObservableCollection<WavPointViewModel>();
        public ObservableCollection<WavPointViewModel> VowelPoints { get; set; } = new ObservableCollection<WavPointViewModel>();
        public ObservableCollection<WavPointViewModel> RestPoints { get; set; } = new ObservableCollection<WavPointViewModel>();

        public List<WavZoneViewModel> ConsonantZones => GetZones(PhonemeType.Consonant);
        public List<WavZoneViewModel> VowelZones => GetZones(PhonemeType.Vowel);
        public List<WavZoneViewModel> RestZones => GetZones(PhonemeType.Rest);

        public int Number { get; set; }
        public int NumberView => Number + 1;
        public override string ViewName => $"{ProjectLine?.Recline?.Description} [{ProjectLine?.Recline?.Name}]";

        public int Width => WaveForm != null ? WaveForm.VisualWidth : 4000;
        public ImageSource WavImage => GetWavImage();

        public PhonemeType PhonemeTypeRest => PhonemeType.Rest;
        public PhonemeType PhonemeTypeVowel => PhonemeType.Vowel;
        public PhonemeType PhonemeTypeConsonant => PhonemeType.Consonant;

        public string WavChannels => GetChannelsString(WaveForm?.WaveFormat?.Channels);
        public string WavBitRate => WaveForm?.RealBitsPerSample?.ToString();
        public string WavSampleRate => WaveForm?.WaveFormat?.SampleRate.ToString();

        public delegate void OtoModeHandler(WavControlViewModel wavControlViewModel);
        public event OtoModeHandler OnOtoMode = delegate { };
        public event SimpleHandler OnLoaded = delegate { };
        public event SimpleHandler OnGenerateOtoRequested = delegate { };
        public event PhonemeTypeArgHandler OnChangePhonemeModeRequested = delegate {};

        public int MetaInfoHeight => Height - 18;

        public readonly string SampleName = "";
        public readonly string Hash = "";

        public WavControlViewModel() : base()
        {
            PointsChanged += HandlePointsChanged;
            OnLoaded += HandleLoaded;
        }

        public WavControlViewModel(ProjectLine projectLine, string sampleName, string hash) : this()
        {
            ProjectLine = projectLine;
            ProjectLine.ProjectLineChanged += HandleProjectLineChanged;
            SampleName = sampleName;
            Hash = hash;
            CreatePoints();
            UpdatePoints();
            FirePointsChanged();
        }

        public override void Ready()
        {
            IsReady = true;
            RaisePropertiesChanged(
                () => IsReady,
                () => WavImage
            );
        }

        public IList<WavPointViewModel> PointsOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? ConsonantPoints :
                (type == PhonemeType.Rest ? RestPoints : VowelPoints);
        }


        public List<WavZoneViewModel> ZonesOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? ConsonantZones :
                (type == PhonemeType.Rest ? RestZones : VowelZones);
        }

        public void AddPoint(double position, PhonemeType type)
        {
            position = CheckPosition(position);
            var i = ProjectLine.AddPoint(Settings.ViewToRealX(position), type);
            if (i == -1)
                return;
            var points = PointsOfType(type);
            points.Add(CreatePoint(position, type, i));
            FirePointsChanged();
        }

        public void MovePoint(double position1, double position2, PhonemeType type)
        {
            position2 = CheckPosition(position2);
            ProjectLine.MovePoint(Settings.ViewToRealX(position1), Settings.ViewToRealX(position2), type);
            var points = PointsOfType(type);
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Position == position1)
                {
                    points[i].Position = position2;
                    break;
                }
            }
            FirePointsChanged();
        }

        public void DeletePoint(double position, PhonemeType type)
        {
            position = CheckPosition(position);
            ProjectLine.DeletePoint(Settings.ViewToRealX(position), type);
            var points = PointsOfType(type);
            foreach (var point in points)
            {
                if (point.Position == position)
                {
                    points.Remove(point);
                    break;
                }
            }
            FirePointsChanged();
        }

        public List<WavControlBaseViewModel> GenerateOtoPreview()
        {
            var collection = new List<WavControlBaseViewModel>();
            foreach (Oto oto in ProjectLine.Recline.OtoList)
            {
                collection.Add(new OtoPreviewControlViewModel(oto, WavImage));
            }
            return collection;
        }

        public void LoadExternal()
        {
            Load();
        }

        public override string ToString()
        {
            if (ProjectLine == null || ProjectLine.Recline == null)
                return "{WavControlViewModel}";
            else
                return $"{Filename} : WavControlViewModel";
        }

        public void HandleProjectLineChanged()
        {
            RaisePropertiesChanged(
                () => Filename,
                () => IsCompleted,
                () => WavImage,
                () => EditEnabled
            );
            RaisePropertiesChanged(
                () => WavChannels,
                () => WavBitRate,
                () => WavSampleRate
            );
            RaisePropertiesChanged(
                () => EditEnabled,
                () => IsEnabled,
                () => IsLoaded,
                () => IsLoading,
                () => IsDisabled
            );
            UpdatePoints();
        }

        public override void HandlePointsChanged()
        {
            RaisePropertiesChanged(
                () => ConsonantPoints,
                () => VowelPoints,
                () => RestPoints,
                () => IsCompleted
            );
            RaisePropertiesChanged(
                () => ConsonantZones,
                () => VowelZones,
                () => RestZones,
                () => Phonemes
            );
            UpdatePoints();
        }

        public void RequestGenerateOto()
        {
            OnGenerateOtoRequested();
        }

        #region private

        private async void Load()
        {
            App.MainDispatcher.Invoke(() =>
            {
                IsLoaded = false;
                IsLoading = true;
                IsImageEnabled = false;
                RaisePropertiesChanged(
                    () => IsLoading,
                    () => IsLoaded,
                    () => EditEnabled
                );
            });

            await Task.Run(() => ExceptionCatcher.Current.CatchOnAsyncCallback(() =>
            {
                ProjectLine.UpdateEnabled();
                if (!ProjectLine.IsEnabled)
                {
                    IsLoading = false;
                    return;
                }

                WaveForm = new WaveForm(SampleName);
                WaveForm.Start(Height);
                WaveForm.CollectData();
                LoadFrq(SampleName);
            })).ConfigureAwait(true);

            App.MainDispatcher.Invoke(() =>
            {
                if (IsLoading)
                {
                    WaveForm.DrawWaveform();
                    WaveForm.Finish(Hash);
                    IsImageEnabled = true;
                    OnLoaded();
                    FirePointsChanged();
                }
            });

        }

        private List<WavZoneViewModel> GetZones(PhonemeType type)
        {
            var points = ProjectLine.PointsOfType(type);
            var zones = new List<WavZoneViewModel>();
            var kindaWitdh = Width > 0 ? Width : 2000;
            for (int i = 0; i + 1 < points.Count; i += 2)
            {
                var pIn = Settings.RealToViewX(points[i]);
                var pOut = Settings.RealToViewX(points[i + 1]);
                zones.Add(new WavZoneViewModel(type, pIn, pOut, kindaWitdh));
            }
            return zones;
        }

        private ImageSource GetWavImage()
        {
            if (IsImageEnabled && Hash == WaveForm?.ImageHash)
                return WaveForm.BitmapImage;
            if (IsEnabled && !IsLoading && Hash != WaveForm?.ImageHash && Hash != null)
                Load();
            return null;
        }

        private string GetPointLabel(PhonemeType type, int i)
        {
            var phonemes = ProjectLine.Recline.PhonemesOfType(type);
            return phonemes.Count > i / 2 ? phonemes[i / 2] : "/PH/";
        }

        private void CreatePoints()
        {
            foreach (var phonemeType in new[] { PhonemeType.Consonant, PhonemeType.Vowel, PhonemeType.Rest})
            {
                var phonemes = ProjectLine.Recline.PhonemesOfType(phonemeType);
                var points = PointsOfType(phonemeType);
                for (var i = 0; i < phonemes.Count; i++)
                {
                    if (phonemeType == PhonemeType.Rest && i == 0)
                    {
                        points.Add(CreatePoint(-1, phonemeType, 0));
                    }
                    else if (phonemeType == PhonemeType.Rest && i == phonemes.Count - 1)
                    {
                        points.Add(CreatePoint(-1, phonemeType, 1));
                    }
                    else
                    {
                        points.Add(CreatePoint(-1, phonemeType, 0));
                        points.Add(CreatePoint(-1, phonemeType, 1));
                    }
                }
            }
        }

        private WavPointViewModel CreatePoint(double p, PhonemeType type, int i)
        {

            var point = new WavPointViewModel(p, type, GetPointLabel(type, i), PointIsLeft(type, i));
            point.WavPointChanged += delegate (double position1, double position2)
            {
                MovePoint(position1, position2, type);
            };
            point.WavPointDeleted += delegate (double position)
            {
                DeletePoint(position, type);
            };
            point.RegenerateOtoRequest += delegate
            {
                OnChangePhonemeModeRequested(point.Type);
                RequestGenerateOto();
            };
            return point;
        }

        private void UpdatePoints()
        {
            UpdatePointsOfType(PhonemeType.Consonant);
            UpdatePointsOfType(PhonemeType.Rest);
            UpdatePointsOfType(PhonemeType.Vowel);
        }

        private void UpdatePointsOfType(PhonemeType type)
        {
            var points = PointsOfType(type);
            var projectPoints = ProjectLine.PointsOfType(type, virtuals: false);
            var phonemes = ProjectLine.Recline.PhonemesOfType(type);
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                var wasEnabled = point.IsEnabled;
                point.IsEnabled = i < projectPoints.Count;
                if (point.IsEnabled)
                {
                    var position = Settings.RealToViewX(projectPoints[i]);
                    point.Position = position;
                    point.Update(PointIsLeft(type, i), phonemes[i / 2].Alias);
                }
                else
                {
                    point.Position = -1;
                }
                if (IsEnabled != wasEnabled)
                {
                    point.FireChanged();
                }
            }
        }

        private void LoadFrq(string sampleName)
        {
            Frq = new Frq();
            Frq.Load(sampleName);
            if (Frq.Points != null)
            {
                CreateFrqPoints();
            }
            RaisePropertyChanged(() => FrqPoints);
        }

        private void CreateFrqPoints()
        {
            var points = new List<FrqPointViewModel>();
            double x = 0;
            var height = WavControlBaseViewModel.GlobalHeight;
            var middleHeight = WavControlBaseViewModel.GlobalHeight / 2;

            double maxVal = 0;
            var averagedPoints = new List<double>();
            foreach (var point in Frq.Points)
            {
                var average = point - Frq.AverageFrequency;
                averagedPoints.Add(average);
                if (average != 0 && maxVal < Math.Abs(average))
                    maxVal = Math.Abs(average);
            }

            foreach (var point in averagedPoints)
            {
                var saturatedY = point / maxVal;
                var y = middleHeight - saturatedY * middleHeight;
                if (y != 0)
                {
                    if (y < 0 || y > height)
                        throw new Exception("error on draw frq");
                    points.Add(new FrqPointViewModel(Settings.RealToViewX(x), y));
                }
                x += Settings.RealToViewX(WaveForm.WaveFormat.Channels * 1000.0 * Frq.SamplesPerFrq / WaveForm.WaveFormat.SampleRate);
            }
            this.FrqPoints = new ObservableCollection<FrqPointViewModel>(points);
        }

        private bool PointIsLeft(PhonemeType type, int i)
        {
            return type == PhonemeType.Rest ? i % 2 == 1 : i % 2 == 0;
        }

        private void ResetPoints()
        {
            ResetPointsOfType(PhonemeType.Vowel);
            ResetPointsOfType(PhonemeType.Consonant);
            ResetPointsOfType(PhonemeType.Rest);
        }

        private void ResetPointsOfType(PhonemeType type)
        {
            ProjectLine.PointsOfType(type, false).Clear();
            ProjectLine.ZonesOfType(type).Clear();
            PointsOfType(type).Clear();
            ZonesOfType(type).Clear();
            FirePointsChanged();
        }

        private void HandleLoaded()
        {
            App.MainDispatcher.Invoke(() =>
            {
                ProjectLine.Width = WaveForm.Width;
                RaisePropertyChanged(() => Width);
                RaisePropertyChanged(() => WavImage);
                UpdatePoints();
                IsLoaded = true;
                IsLoading = false;
                RaisePropertyChanged(() => FrqPoints);
                HandleProjectLineChanged();
            });
        }

        private string GetChannelsString(int? channels)
        {
            return !channels.HasValue ? null : channels.Value == 1 ? "Mono" : channels.Value == 2 ? "Stereo" : channels.Value.ToString();
        }

        private double CheckPosition(double position)
        {
            if (position < 0)
                position = 5;
            if (position > Width)
                position = Width - 5;
            return position;
        }

        #endregion

        #region Commands

        public ICommand WavControlClickCommand
        {
            get
            {
                return new DelegateCommand<Point>(
                    delegate (System.Windows.Point point)
                    {
                        AddPoint((int)point.X, Settings.Mode);
                    },
                    delegate (Point point)
                    {
                        return ProjectLine.IsEnabled && !ProjectLine.IsCompleted;
                    }
                );
            }
        }

        public ICommand OtoModeCommand => new DelegateCommand(() =>
        {
            OnOtoMode(this);
        }, () => !IsLoading);

        public ICommand RegenerateOtoCommand => new DelegateCommand(() =>
        {
            OnGenerateOtoRequested();
        }, () => IsOtoBase);

        public ICommand ResetPointsCommand => new DelegateCommand<PhonemeType>((PhonemeType type) =>
        {
            ResetPointsOfType(type);
            ProjectLine.UpdateZones();
            FirePointsChanged();
        }, (type) => !IsLoading);

        public ICommand ResetAllPointsCommand => new DelegateCommand(() =>
        {
            ResetPoints();
            ProjectLine.UpdateZones();
            FirePointsChanged();
        }, () => !IsLoading);

        public ICommand ReloadCommand => new DelegateCommand(() =>
        {
            Load();
        }, () => !IsLoading);

        #endregion
    }
}
