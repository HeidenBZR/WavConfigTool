﻿using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WavConfigTool.Classes;
using WavConfigTool.Tools;

namespace WavConfigTool.ViewModels
{
    public class WavControlViewModel : WavControlBaseViewModel
    {
        private ProjectLine _projectLine;

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
                return Settings.ViewToRealX(Width);
            }
        }

        public bool IsOtoBase { get; set; } = false;

        public ObservableCollection<WavPointViewModel> ConsonantPoints { get; set; } = new ObservableCollection<WavPointViewModel>();
        public ObservableCollection<WavPointViewModel> VowelPoints { get; set; } = new ObservableCollection<WavPointViewModel>();
        public ObservableCollection<WavPointViewModel> RestPoints { get; set; } = new ObservableCollection<WavPointViewModel>();

        public List<WavZoneViewModel> ConsonantZones { get { return GetZones(PhonemeType.Consonant, ConsonantPoints); } }
        public List<WavZoneViewModel> VowelZones { get { return GetZones(PhonemeType.Vowel, VowelPoints); } }
        public List<WavZoneViewModel> RestZones { get { return GetZones(PhonemeType.Rest, RestPoints); } }

        public List<WavZoneViewModel> GetZones(PhonemeType point, ObservableCollection<WavPointViewModel> points)
        {
            var zones = new List<WavZoneViewModel>();
            SortPoints(points);
            for (int i = 0; i + 1 < points.Count; i += 2)
            {
                zones.Add(new WavZoneViewModel(point, points[i].Position, points[i + 1].Position));
            }
            return zones;
        }

        public string Filename { get => ProjectLine.Recline.Filename; }
        public List<Phoneme> Phonemes { get => ProjectLine.Recline.Phonemes; }

        public Visibility LoadingProperty { get => IsLoading ? Visibility.Visible : Visibility.Hidden; }
        public Visibility DisabledProperty { get => ProjectLine.IsEnabled ? Visibility.Hidden : Visibility.Visible; }
        public Visibility EnabledProperty { get => ProjectLine.IsEnabled ? Visibility.Visible : Visibility.Hidden; }

        public bool IsCompleted { get => ProjectLine.IsCompleted; }
        public bool IsLoading { get; set; } = false;
        public int Number { get; set; }
        public int NumberView => Number + 1;

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

        public WavControlViewModel()
        {
            OnOtoMode += delegate { };
            PointsChanged += OnPointsChanged;
        }

        public WavControlViewModel(ProjectLine projectLine) : base()
        {
            PointsChanged += OnPointsChanged;
            ProjectLine = projectLine;
            if (ProjectLine.IsEnabled)
                Width = (int)(Settings.RealToViewX(ProjectLine.WaveForm.Length / (float)ProjectLine.WaveForm.BitRate));
            ApplyPoints();
        }

        public override void Load()
        {
            LoadImageAsync();
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

        internal ObservableCollection<WavControlBaseViewModel> GenerateOtoPreview()
        {
            var collection = new ObservableCollection<WavControlBaseViewModel>();
            foreach (Oto oto in ProjectLine.Recline.OtoList)
            {
                collection.Add(new OtoPreviewControlViewModel(oto, WavImage));
            }
            return collection;
        }

        public delegate void PointsChangedHandler();
        public event SimpleHandler PointsChanged;

        public delegate void SimpleHandler();
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
                () => ProjectLine.Recline.Phonemes,
                () => Phonemes
            );
            foreach (var phoneme in Phonemes)
            {
                RaisePropertyChanged(() => phoneme.HasZone);
            }
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
            return phonemes.Count * 2 > i && i % 2 == 0 ? phonemes[i / 2] : "";
        }

        private void FillPoints(PhonemeType type)
        {
            var points = PointsOfType(type);
            var projectPoints = ProjectLine.PointsOfType(type);
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

            var point = new WavPointViewModel(p, type, GetPointLabel(type, i));
            point.WavPointChanged += delegate (double position1, double position2)
            {
                MovePoint(position1, position2, type);
            };
            point.WavPointDeleted += delegate (double position)
            {
                DeletePoint(position, type);
            };
            return point;
        }

        public double CheckPosition(double position)
        {
            if (position < 0)
                position = 5;
            if (position > Width)
                position = Width - 5;
            return position;
        }

        public void AddPoint(double position, PhonemeType type, bool checkRest = true)
        {
            position = CheckPosition(position);
            if (checkRest)
                ChechExtraRestPoint(type);
            var i = ProjectLine.AddPoint(Settings.ViewToRealX(position), type);
            if (i == -1)
                return;
            var points = PointsOfType(type);
            points.Add(CreatePoint(position, type, i));
            PointsChanged();
        }

        void ChechExtraRestPoint(PhonemeType type)
        {
            if (type != PhonemeType.Rest)
                return;
            if (RestPoints.Count == 0)
            {
                AddPoint(0, type, false);
            }
            else if (RestPoints.Count == 2)
            {
                AddPoint(Length, type, false);
            }
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
            PointsChanged();
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
            PointsChanged();
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
                return new DelegateCommand<Point>(
                    delegate (Point point)
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

        public delegate void OtoModeHandler(WavControlViewModel wavControlViewModel);
        public event OtoModeHandler OnOtoMode;

        public ICommand OtoModeCommand => new DelegateCommand(() =>
        {
            OnOtoMode(this);
        }, () => !IsLoading);

        public ICommand RegenerateOtoCommand => new DelegateCommand(() =>
        {
            RegenerateOtoRequest();
        }, () => IsOtoBase);
    }
}
