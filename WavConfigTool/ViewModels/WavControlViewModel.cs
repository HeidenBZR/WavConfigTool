using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WavConfigTool.Classes;
using WavConfigCore;
using System.Activities.Statements;
using System;

namespace WavConfigTool.ViewModels
{
    public class WavControlViewModel : WavControlBaseViewModel
    {
        public ProjectLine ProjectLine { get; private set; }

        public string Filename { get; private set; }
        public ObservableCollection<Phoneme> Phonemes { get; private set; }

        public bool IsLoading { get; set; } = false;
        public bool IsImageEnabled { get; set; } = false;
        public bool IsReady { get; set; } = false;

        public bool IsLoaded { get; set; } = false;
        public bool IsImagesLoaded { get; set; } = false;
        public bool IsPointsLoaded { get; set; } = false;

        public override bool IsCompleted => ProjectLine != null && ProjectLine.IsCompleted;
        public override bool IsEnabled => ProjectLine != null && ProjectLine.IsEnabled;
        public bool IsDisabled => !IsEnabled;
        public bool EditEnabled => IsEnabled && !IsLoading && IsLoaded;

        public ObservableCollection<WavPointViewModel> ConsonantPoints { get; set; } = new ObservableCollection<WavPointViewModel>();
        public ObservableCollection<WavPointViewModel> VowelPoints { get; set; } = new ObservableCollection<WavPointViewModel>();
        public ObservableCollection<WavPointViewModel> RestPoints { get; set; } = new ObservableCollection<WavPointViewModel>();

        public List<WavZoneViewModel> ConsonantZones => GetZones(PhonemeType.Consonant);
        public List<WavZoneViewModel> VowelZones => GetZones(PhonemeType.Vowel);
        public List<WavZoneViewModel> RestZones => GetZones(PhonemeType.Rest);

        public int NumberView { get; private set; }

        public int Width => WaveForm?.VisualWidth ?? 4000;
        public ImageSource WavImage { get; private set; }
        public ImageSource FrqImage { get; private set; }
        public ImageSource SpectrumImage { get; private set; }

        public PhonemeType PhonemeTypeRest => PhonemeType.Rest;
        public PhonemeType PhonemeTypeVowel => PhonemeType.Vowel;
        public PhonemeType PhonemeTypeConsonant => PhonemeType.Consonant;

        public bool ConsonantPointsVisible  => EditEnabled && ProjectLine.ConsonantPoints.Count > 0;
        public bool VowelPointsVisible      => EditEnabled && ProjectLine.VowelPoints.Count > 0;
        public bool RestPointsVisible       => EditEnabled && ProjectLine.RestPoints.Count > 0;
        public bool ConsonantZonesVisible   => EditEnabled && ProjectLine.ConsonantPoints.Count > 1;
        public bool VowelZonesVisible       => EditEnabled && ProjectLine.VowelPoints.Count > 1;
        public bool RestZonesVisible        => EditEnabled && ProjectLine.RestPoints.Count > 0;

        public string WavChannels => WaveForm?.ChannelsString;
        public string WavBitRate => WaveForm?.BitRate.ToString();
        public string WavSampleRate => WaveForm?.SampleRate.ToString();

        public bool DoShowWaveform => IsReadyToDrawPoints && ViewOptions.DoShowWaveform && IsImageEnabled;
        public bool DoShowPitch     => IsReadyToDrawPoints && ViewOptions.DoShowPitch;
        public bool DoShowSpectrum  => IsReadyToDrawPoints && ViewOptions.DoShowSpectrum;
        public bool DoShowCompleted => IsCompleted && IsReadyToDrawPoints;

        public WavPlayer WavPlayer;

        public event SimpleHandler OnLoaded = delegate { };

        public override void Update(PagerContentBase pagerContent)
        {
            if (PagerContent != null)
            {
                UnsubscribePagerContent();
            }

            IsLoaded = false;
            IsImagesLoaded = false;
            IsPointsLoaded = false;

            PagerContent = pagerContent;
            var projectLineContainer = GetProjectLineContainer();

            ProjectLine = projectLineContainer.ProjectLine;
            NumberView = projectLineContainer.Number + 1;
            WaveForm = projectLineContainer.WaveForm;

            Filename = ProjectLine.Recline.Name;
            RaisePropertyChanged(nameof(Filename));
            Phonemes = new ObservableCollection<Phoneme>(ProjectLine.Recline.Phonemes);
            RaisePropertyChanged(nameof(Phonemes));

            CreatePoints();
            UpdatePoints();
            FirePointsChanged();

            ProjectLine.ProjectLineChanged += HandleProjectLineChanged;
            SubscribePagerContent();

            SetIsLoading();
            projectLineContainer.LoadImages(Height);
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

        public void MovePoint(double position1, double position2, PhonemeType type)
        {
            position2 = GetProjectLineContainer().CheckPosition(position2);
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
            position = GetProjectLineContainer().CheckPosition(position);
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
                () => DoShowCompleted,
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
                () => DoShowCompleted
            );
            RaisePropertiesChanged(
                () => ConsonantZones,
                () => VowelZones,
                () => RestZones,
                () => Phonemes
            );
            RaisePropertiesChanged(
                () => ConsonantPointsVisible,
                () => VowelPointsVisible,
                () => RestPointsVisible
            );
            RaisePropertiesChanged(
                () => ConsonantZonesVisible,
                () => VowelZonesVisible,
                () => RestZonesVisible
            );
            UpdatePoints();
        }

        public override void HandleViewChanged()
        {
            base.HandleViewChanged();
            RaisePropertiesChanged(
                () => DoShowPitch,
                () => DoShowSpectrum,
                () => DoShowWaveform
            );
        }
        public override void SetReady(bool ready)
        {
            base.SetReady(ready);
            RaisePropertiesChanged(nameof(IsReadyToDrawPoints), nameof(DoShowCompleted), nameof(DoShowPitch), nameof(DoShowSpectrum), nameof(DoShowWaveform));
        }

        #region private

        private void SubscribePagerContent()
        {
            var projectLineContainer = GetProjectLineContainer();
            projectLineContainer.OnImageLoaded += HandleImagesLoaded;
            projectLineContainer.OnPointAdded += HandlePointAdded;
            projectLineContainer.PointsChanged += HandlePointsChanged;
        }

        private void UnsubscribePagerContent()
        {
            var projectLineContainer = GetProjectLineContainer();
            projectLineContainer.OnImageLoaded -= HandleImagesLoaded;
            projectLineContainer.OnPointAdded += HandlePointAdded;
            projectLineContainer.PointsChanged -= HandlePointsChanged;
            WavImage = null;
            SpectrumImage = null;
            FrqImage = null;
            RaisePropertiesChanged(nameof(WavImage), nameof(SpectrumImage), nameof(FrqImage));
        }

        private void SetIsLoading()
        {
            IsLoaded = false;
            IsLoading = true;
            IsImageEnabled = false;
            RaisePropertiesChanged(
                () => IsLoading,
                () => IsLoaded,
                () => EditEnabled
            );
            RaisePropertiesChanged(
                () => WavImage,
                () => FrqImage,
                () => SpectrumImage
            );
            HandleViewChanged();
        }

        private void HandleImagesLoaded(bool isLoaded)
        {
            IsImageEnabled = isLoaded;
            IsLoading = false;
            if (IsImageEnabled)
            {
                WavImage = ImagesLibrary.TryGetImage(WaveForm, WavImageType.WAVEFORM);
                SpectrumImage = ImagesLibrary.TryGetImage(WaveForm, WavImageType.SPECTRUM);
                FrqImage = ImagesLibrary.TryGetImage(WaveForm, WavImageType.FRQ);
                RaisePropertiesChanged(nameof(WavImage), nameof(SpectrumImage), nameof(FrqImage));

                OnLoaded();
                FirePointsChanged();
            }

            RaisePropertyChanged(nameof(IsImageEnabled));
            RaisePropertyChanged(nameof(IsLoading));
            RaisePropertyChanged(nameof(WavImage));
            RaisePropertyChanged(nameof(SpectrumImage));
            RaisePropertyChanged(nameof(FrqImage));
            HandleViewChanged();

            ProjectLine.Width = WaveForm.Width;
            RaisePropertyChanged(nameof(Width));
            UpdatePoints();
            IsImagesLoaded = true;
            CheckLoaded();
        }

        private void HandlePointAdded(double position, PhonemeType type, int i)
        {
            var points = PointsOfType(type);
            points.Add(CreatePoint(position, type, i));
            FirePointsChanged();
        }

        private List<WavZoneViewModel> GetZones(PhonemeType type)
        {
            if (ProjectLine == null)
                return null;
            var points = ProjectLine.PointsOfType(type);
            var zones = new List<WavZoneViewModel>();
            var kindaWitdh = Width > 0 ? Width : 2000;
            for (int i = 0; i + 1 < points.Count; i += 2)
            {
                var pIn = Settings.RealToViewX(points[i]);
                var pOut = Settings.RealToViewX(points[i + 1]);
                zones.Add(new WavZoneViewModel(type, pIn, pOut, kindaWitdh, (int)Height));
            }
            return zones;
        }

        private string GetPointLabel(PhonemeType type, int i)
        {
            var phonemes = ProjectLine.Recline.PhonemesOfType(type);
            return phonemes.Count > i / 2 ? phonemes[i / 2] : "/PH/";
        }

        private async void CreatePoints()
        {
            var points = new Dictionary<PhonemeType, List<WavPointViewModel>>();

            await Task.Run(delegate
            {
                foreach (var phonemeType in new[] { PhonemeType.Consonant, PhonemeType.Vowel, PhonemeType.Rest })
                {
                    points[phonemeType] = new List<WavPointViewModel>();
                    var phonemes = ProjectLine.Recline.PhonemesOfType(phonemeType);
                    for (var i = 0; i < phonemes.Count; i++)
                    {
                        if (phonemeType == PhonemeType.Rest && i == 0)
                        {
                            points[phonemeType].Add(CreatePoint(-1, phonemeType, 0));
                        }
                        else if (phonemeType == PhonemeType.Rest && i == phonemes.Count - 1)
                        {
                            points[phonemeType].Add(CreatePoint(-1, phonemeType, 1));
                        }
                        else
                        {
                            points[phonemeType].Add(CreatePoint(-1, phonemeType, 0));
                            points[phonemeType].Add(CreatePoint(-1, phonemeType, 1));
                        }
                    }
                }
            }).ContinueWith(delegate
            {
                App.MainDispatcher.Invoke(delegate
                {
                    foreach (var phonemeType in new[] { PhonemeType.Consonant, PhonemeType.Vowel, PhonemeType.Rest })
                    {
                        var pointsOfType = PointsOfType(phonemeType);
                        foreach (var point in points[phonemeType])
                        {
                            pointsOfType.Add(point);
                        };
                    };
                    IsPointsLoaded = true;
                    CheckLoaded();
                });
            });
        }

        private WavPointViewModel CreatePoint(double p, PhonemeType type, int i)
        {

            var point = new WavPointViewModel(p, type, GetPointLabel(type, i), PointIsLeft(type, i), Height);
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
                GetProjectLineContainer().RequestChangeMode(point.Type);
                GetProjectLineContainer().RequestGenerateOto();
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

        private void CheckLoaded()
        {
            if (IsImagesLoaded && IsPointsLoaded)
            {
                IsLoaded = true;
                IsLoading = false;
                RaisePropertiesChanged(nameof(IsLoaded), nameof(IsLoading));
                HandleProjectLineChanged();
            }
        }

        private void FirePointsChanged()
        {
            GetProjectLineContainer().FirePointsChanged();
            RaisePropertyChanged(nameof(IsCompleted));
        }

        private ProjectLineContainer GetProjectLineContainer()
        {
            return (ProjectLineContainer)PagerContent;
        }

        #endregion

        #region Commands

        public ICommand WavControlClickCommand => new DelegateCommand<Point>
        (
            delegate (System.Windows.Point point)
            {
                GetProjectLineContainer().AddPoint((int)point.X, Settings.Mode);
            },
            delegate (Point point)
            {
                return ProjectLine.IsEnabled && !ProjectLine.IsCompleted;
            }
        );

        public ICommand PlayCommand => new DelegateCommand<Point>
        (
            delegate (System.Windows.Point point)
            {
                GetProjectLineContainer().PlaySound(Settings.ViewToRealX(point.X));
            },
            delegate (Point point)
            {
                return ProjectLine.IsEnabled;
            }
        );

        public ICommand OtoModeCommand => new DelegateCommand(() =>
        {
            GetProjectLineContainer().RequestOtoMode();
        }, () => !IsLoading);

        public ICommand RegenerateOtoCommand => new DelegateCommand(() =>
        {
            GetProjectLineContainer().RequestGenerateOto();
        }, true);

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
            GetProjectLineContainer().LoadImages(Height);
        }, () => !IsLoading);

        #endregion
    }
}
