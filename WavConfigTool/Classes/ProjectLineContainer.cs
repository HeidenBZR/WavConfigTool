﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore;

namespace WavConfigTool.Classes
{
    public class ProjectLineContainer : PagerContentBase
    {
        public delegate void OtoModeHandler(ProjectLineContainer container);
        public delegate void OnAddPointRequestHandler(double position, PhonemeType type);
        public delegate void OnPointAddedHandler(double position, PhonemeType type, int i);
        public delegate void ImageLoadedHandler(bool isLoaded);

        public event OtoModeHandler OnOtoRequested = delegate { };
        public event SimpleHandler OnLoaded = delegate { };
        public event SimpleHandler OnGenerateOtoRequested = delegate { };
        public event ImageLoadedHandler OnImageLoaded = delegate { };
        public event PhonemeTypeArgHandler OnChangePhonemeModeRequested = delegate { };
        public event OnAddPointRequestHandler OnAddPointRequested = delegate { };
        public event OnPointAddedHandler OnPointAdded = delegate { };

        public ProjectLine ProjectLine { get; private set; }
        public WaveForm WaveForm { get; private set; }

        public bool IsLoadingImages;
        public bool IsLoadedImages;

        public readonly int Number;

        public ProjectLineContainer(ProjectLine projectLine, ImagesLibrary imagesLibrary, WavPlayer wavPlayer, string sampleName, string hash, int number)
        {
            ProjectLine = projectLine;
            Number = number;
            this.imagesLibrary = imagesLibrary;
            this.wavPlayer = wavPlayer;
            this.hash = hash;
            this.sampleName = sampleName;
            viewName = !string.IsNullOrEmpty(ProjectLine?.Recline?.Description) ? $"{ProjectLine?.Recline?.Description} [{ProjectLine?.Recline?.Name}]" : ProjectLine?.Recline?.Name;
            WaveForm = new WaveForm(sampleName);
        }

        public override string GetViewName()
        {
            return viewName;
        }

        public List<PagerContentBase> GenerateOtoPreview()
        {
            var collection = new List<PagerContentBase>();
            foreach (Oto oto in ProjectLine.Recline.OtoList)
            {
                collection.Add(new OtoContainer(oto, this));
            }
            return collection;
        }

        public void AddPoint(double position, PhonemeType type)
        {
            position = CheckPosition(position);
            if (position == -1)
                return;
            var i = ProjectLine.AddPoint(Settings.ViewToRealX(position), type);
            if (i == -1)
                return;
            OnPointAdded(position, type, i);
        }

        public void RequestGenerateOto()
        {
            OnGenerateOtoRequested();
        }

        public async void LoadImages(int height)
        {
            IsLoadingImages = true;
            IsLoadedImages = false;
            ProjectLine.UpdateEnabled();
            if (ProjectLine.IsEnabled)
            {
                await Task.Run(() => ExceptionCatcher.Current.CatchOnAsyncCallback(() =>
                {
                    imagesLibrary.Load(WaveForm, height, hash);
                })).ContinueWith(delegate
                {
                    FinishImagesLoading(true);
                });
            }
            else
            {
                FinishImagesLoading(false);
            }
        }

        public double CheckPosition(double position)
        {
            if (WaveForm == null || !WaveForm.IsEnabled)
                return -1;
            if (position < 0)
                position = 5;
            if (position > WaveForm.VisualWidth)
                position = WaveForm.VisualWidth - 5;
            return position;
        }

        public void PlaySound(double pos)
        {
            var startPosition = 0;
            var length = WaveForm.Width;
            var endPosition = length;

            var points = new List<int>();
            points.AddRange(ProjectLine.ConsonantPoints);
            points.AddRange(ProjectLine.VowelPoints);
            points.AddRange(ProjectLine.RestPoints);
            points.Sort();

            if (points.Count > 0)
                endPosition = points[0];
            for (int i = 0; i < points.Count && points[i] < pos; i++)
            {
                startPosition = points[i];
                if (i + 1 < points.Count)
                    endPosition = points[i + 1];
                else
                    endPosition = length;
            }
            Console.WriteLine($"pos {pos}, play from {startPosition} to {endPosition} on {ProjectLine}");

            wavPlayer.Play(WaveForm, startPosition, endPosition - startPosition);
        }

        public void RequestOtoMode()
        {
            OnOtoRequested(this);
        }

        public void RequestChangeMode(PhonemeType mode)
        {
            OnChangePhonemeModeRequested(mode);
        }

        private readonly ImagesLibrary imagesLibrary;
        private readonly WavPlayer wavPlayer;
        private readonly string sampleName;
        private readonly string hash;
        private readonly string viewName;

        private void FinishImagesLoading(bool isLoaded)
        {
            IsLoadingImages = false;
            IsLoadedImages = true;
            OnImageLoaded(isLoaded);
        }
    }
}
