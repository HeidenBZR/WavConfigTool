using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WavConfigTool.Classes;
using WavConfigTool.Tools;

namespace WavConfigTool.ViewModels
{
    class OtoPreviewControlViewModel : WavControlBaseViewModel
    {
        public string Filename { get => Oto.Filename; }
        public string Alias { get => Oto.Alias; }

        public double Offset { get => Oto.Offset * Settings.ScaleX; set => Oto.Offset = value / Settings.ScaleX; }
        public double Consonant { get => Oto.Consonant * Settings.ScaleX; set => Oto.Consonant = value / Settings.ScaleX; }
        public double Cutoff { get => Oto.Cutoff * Settings.ScaleX; set => Oto.Cutoff = value / Settings.ScaleX; }
        public double Preutterance { get => Oto.Preutterance * Settings.ScaleX; set => Oto.Preutterance = value / Settings.ScaleX; }
        public double Overlap { get => Oto.Overlap * Settings.ScaleX; set => Oto.Overlap = value / Settings.ScaleX; }

        public double Length { get; set; }
        public double CutoffLength { get => Length - Cutoff; }
        public double ConsonantLength { get => Consonant - Offset; }

        public double Height { get; } = 100;

        public ImageSource WavImage { get; set; }

        public Oto Oto { get; set; }

        public OtoPreviewControlViewModel() { }

        public OtoPreviewControlViewModel(Oto oto, ImageSource image)
        {
            if (oto != null)
            {
                Oto = oto;
                WavImage = image;
                Length = image.Width;
            }
        }

        public OtoPreviewControlViewModel(string otoLine, double length, ImageSource image) : this(Oto.Read(otoLine, length), image) { }

        public override void Load()
        {
            
        }

        public override string ToString()
        {
            return $"{(Oto == null ? "(null)" : Oto.Write())} : OtoPreviewViewModel";
        }
    }
}
