using System.Windows.Media;
using WavConfigTool.Classes;
using WavConfigCore;

namespace WavConfigTool.ViewModels
{
    class OtoPreviewControlViewModel : WavControlBaseViewModel
    {
        public string Filename { get => Oto.Filename; }
        public string Alias { get => Oto.Alias; }

        public string Suffix => ProjectManager.Current.Project?.Suffix;
        public string Prefix => ProjectManager.Current.Project?.Prefix;
        public string WavSuffix => ProjectManager.Current.Project?.WavSuffix;
        public string WavPrefix => ProjectManager.Current.Project?.WavPrefix;

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
        public ImageSource WavImage { get; set; }
        public Oto Oto { get; set; }

        public override void Update(PagerContentBase pagerContent)
        {
            PagerContent = pagerContent;
            var otoContainer = (OtoContainer)pagerContent;
            Oto = otoContainer.Oto;
            WaveForm = otoContainer.BaseProjectLineContainer.WaveForm;
            WavImage = ImagesLibrary.TryGetImage(WaveForm, WavImageType.WAVEFORM);
            Length = WavImage != null ? WavImage.Width : 1880;
        }

        public override string ToString()
        {
            return $"{(Oto == null ? "(null)" : Oto.Write())} : OtoPreviewViewModel";
        }
    }
}
