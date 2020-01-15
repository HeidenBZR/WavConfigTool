﻿using DevExpress.Mvvm;
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
using WavConfigTool.Tools;

namespace WavConfigTool.ViewModels
{
    public class WavControlViewModel : WavControlBaseViewModel
    {
        public ProjectLine ProjectLine
        {
            get => _projectLine;
            set
            {
                _projectLine = value;
                _projectLine.ProjectLineChanged += HandleProjectLineChanged;
            }
        }


        public string Filename { get => ProjectLine.Recline.Filename; }
        public double Length =>  Settings.ViewToRealX(Width);
        public ObservableCollection<Phoneme> Phonemes => new ObservableCollection<Phoneme>(ProjectLine.Recline.Phonemes);

        public bool IsOtoBase { get; set; } = false;
        public bool IsLoading { get; set; } = false;
        public bool IsLoaded { get; set; } = false;
        public bool IsImageEnabled { get; set; } = false;

        public bool IsCompleted => ProjectLine.IsCompleted;
        public bool IsEnabled => ProjectLine != null && ProjectLine.IsEnabled;
        public bool IsDisabled => !IsEnabled;
        public bool EditEnabled => IsEnabled && !IsLoading && IsLoaded;

        public ObservableCollection<WavPointViewModel> ConsonantPoints { get; set; } = new ObservableCollection<WavPointViewModel>();
        public ObservableCollection<WavPointViewModel> VowelPoints { get; set; } = new ObservableCollection<WavPointViewModel>();
        public ObservableCollection<WavPointViewModel> RestPoints { get; set; } = new ObservableCollection<WavPointViewModel>();

        public List<WavZoneViewModel> ConsonantZones { get { return GetZones(PhonemeType.Consonant); } }
        public List<WavZoneViewModel> VowelZones { get { return GetZones(PhonemeType.Vowel); } }
        public List<WavZoneViewModel> RestZones { get { return GetZones(PhonemeType.Rest); } }

        public int Number { get; set; }
        public int NumberView => Number + 1;

        public int Width => ProjectLine.IsEnabled ? ProjectLine.WaveForm.VisualWidth : 4000;
        public ImageSource WavImage => IsImageEnabled ? ProjectLine.WaveForm.BitmapImage : null;

        public PhonemeType PhonemeTypeRest => PhonemeType.Rest;
        public PhonemeType PhonemeTypeVowel => PhonemeType.Vowel;
        public PhonemeType PhonemeTypeConsonant => PhonemeType.Consonant;

        public delegate void OtoModeHandler(WavControlViewModel wavControlViewModel);
        public event OtoModeHandler OnOtoMode = delegate { };
        public event SimpleHandler OnLoaded = delegate { };

        public WavControlViewModel() : base()
        {
            PointsChanged += OnPointsChanged;
            OnLoaded += HandleLoaded;
        }

        public WavControlViewModel(ProjectLine projectLine) : this()
        {
            ProjectLine = projectLine;
        }

        public override void Load()
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
            LoadImageAsync();
        }

        public async void LoadImageAsync()
        {
            await Task.Run(() => ExceptionCatcher.Current.CatchOnAsyncCallback(LoadImage)).ConfigureAwait(true);
        }

        public void LoadImage()
        {
            if (!ProjectLine.IsEnabled)
                return;

            ProjectLine.WaveForm.MakeWaveForm(100, ProjectLine.WavImageHash, 
                System.Drawing.ColorTranslator.FromHtml(WaveForm.WAV_ZONE_COLOR));
            IsImageEnabled = true;
            RaisePropertyChanged(() => Width);
            RaisePropertyChanged(() => WavImage);
            OnLoaded();
        }


        public void ApplyPoints()
        {
            FillPoints(PhonemeType.Consonant);
            FillPoints(PhonemeType.Rest);
            FillPoints(PhonemeType.Vowel);
            FirePointsChanged();
        }

        public event SimpleHandler RegenerateOtoRequest;

        public void OnPointsChanged()
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

        public string GetPointLabel(PhonemeType type, int i)
        {
            var phonemes = ProjectLine.Recline.PhonemesOfType(type);
            return phonemes[i / 2];
        }

        public double CheckPosition(double position)
        {
            if (position < 0)
                position = 5;
            if (position > Width)
                position = Width - 5;
            return position;
        }
        private List<WavZoneViewModel> GetZones(PhonemeType type)
        {
            var points = ProjectLine.PointsOfType(type).ShallowClone();
            var zones = new List<WavZoneViewModel>();
            points.Sort();
            for (int i = 0; i + 1 < points.Count; i += 2)
            {
                var pIn = Settings.RealToViewX(points[i]);
                var pOut = Settings.RealToViewX(points[i + 1]);
                zones.Add(new WavZoneViewModel(type, pIn, pOut, Width));
            }
            return zones;   
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

        public override string ToString()
        {
            if (ProjectLine == null || ProjectLine.Recline == null)
                return "{WavControlViewModel}";
            else
                return $"{ProjectLine.Recline.Name} : WavControlViewModel";
        }

        public ObservableCollection<WavControlBaseViewModel> GenerateOtoPreview()
        {
            var collection = new ObservableCollection<WavControlBaseViewModel>();
            foreach (Oto oto in ProjectLine.Recline.OtoList)
            {
                collection.Add(new OtoPreviewControlViewModel(oto, WavImage));
            }
            return collection;
        }

        #region private
        private ProjectLine _projectLine;

        private void FillPoints(PhonemeType type)
        {
            var points = PointsOfType(type);
            var projectPoints = ProjectLine.PointsOfType(type, virtuals: false);
            points.Clear();
            for (int i = 0; i < projectPoints.Count; i++)
            {
                var position = Settings.RealToViewX(projectPoints[i]);
                var point = CreatePoint(position, type, i);
                points.Add(point);
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
                RegenerateOtoRequest();
            };
            return point;
        }

        private bool PointIsLeft(PhonemeType type, int i)
        {
            return type == PhonemeType.Rest ? i % 2 == 1 : i % 2 == 0;
        }

        private void ResetPoints(PhonemeType type)
        {
            ProjectLine.PointsOfType(type, false).Clear();
            ProjectLine.ZonesOfType(type).Clear();
            PointsOfType(type).Clear();
            ZonesOfType(type).Clear();
            FirePointsChanged();
        }

        private void ResetPoints()
        {
            ResetPoints(PhonemeType.Vowel);
            ResetPoints(PhonemeType.Consonant);
            ResetPoints(PhonemeType.Rest);
        }

        private void HandleProjectLineChanged()
        {
            RaisePropertiesChanged(
                () => Filename,
                () => IsCompleted,
                () => WavImage,
                () => EditEnabled
            );
            RaisePropertiesChanged(
                () => EditEnabled,
                () => IsEnabled,
                () => IsLoaded,
                () => IsLoading,
                () => IsDisabled
            );
            ApplyPoints();
        }

        private void HandleLoaded()
        {
            App.MainDispatcher.Invoke(() =>
            {
                ApplyPoints();
                ProjectLine.SetHasZone();
                IsLoaded = true;
                IsLoading = false;
                HandleProjectLineChanged();
            });
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
            RegenerateOtoRequest();
        }, () => IsOtoBase);

        public ICommand ResetPointsCommand => new DelegateCommand<PhonemeType>((PhonemeType type) =>
        {
            ResetPoints(type);
        }, (type) => !IsLoading);

        public ICommand ResetAllPointsCommand => new DelegateCommand(() =>
        {
            ResetPoints();
        }, () => !IsLoading);

        #endregion
    }
}
