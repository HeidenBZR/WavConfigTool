﻿using System.Windows.Media;
using WavConfigTool.Classes;
using WavConfigCore;

namespace WavConfigTool.ViewModels
{
    class OtoPreviewControlViewModel : WavControlBaseViewModel
    {
        public string Filename { get => Oto.Filename; }
        public string Alias { get => Oto.Alias; }

        public string Suffix => Project.Current?.Suffix;
        public string Prefix => Project.Current?.Prefix;
        public string WavSuffix => Project.Current?.WavSuffix;
        public string WavPrefix => Project.Current?.WavPrefix;

        public double Offset { get => Settings.RealToViewX(Oto.Offset); set => Oto.Offset = value / Settings.ScaleX; }
        public double Consonant { get => Settings.RealToViewX(Oto.Consonant); set => Oto.Consonant = value / Settings.ScaleX; }
        public double Cutoff
        {
            get => Oto.Cutoff == 0 ? Length - Settings.RealToViewX(Oto.Cutoff) : Settings.RealToViewX(Oto.Cutoff);
            set => Oto.Cutoff = value / Settings.ScaleX;
        }
        public double Preutterance { get => Settings.RealToViewX(Oto.Preutterance); set => Oto.Preutterance = value / Settings.ScaleX; }
        public double Overlap { get => Settings.RealToViewX(Oto.Overlap); set => Oto.Overlap = value / Settings.ScaleX; }

        public double Length { get; set; }
        public double CutoffLength { get => Length - Cutoff; }
        public double ConsonantLength { get => Consonant - Offset; }

        public string NumberView => Oto.NumberView;
        public double Height { get; } = 100; // TODO: All Height:100 must be moved to some variable
        public ImageSource WavImage { get; set; }
        public Oto Oto { get; set; }

        public OtoPreviewControlViewModel() { }

        public OtoPreviewControlViewModel(Oto oto, ImageSource image)
        {
            WavImage = image;
            Length = image.Width;
            if (oto != null)
            {
                Oto = oto;
            }
        }

        public override string ToString()
        {
            return $"{(Oto == null ? "(null)" : Oto.Write())} : OtoPreviewViewModel";
        }
    }
}