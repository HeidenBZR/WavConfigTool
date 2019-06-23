using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Classes;
using WavConfigTool.Tools;

namespace WavConfigTool.ViewModels
{
    class OtoPreviewControlViewModel : WavControlBaseViewModel
    {
        public string Filename { get; set; }
        public string Alias { get; set; }

        public double Offset { get; set; }
        public double Consonant { get; set; }
        public double Cutoff { get; set; }
        public double Preutterance { get; set; }
        public double Overlap { get; set; }

        public double Length { get; set; }
        public double CutoffLength { get => Length - Cutoff; }

        public double Height { get; } = 100;

        public Oto Oto { get; set; }

        public OtoPreviewControlViewModel() { }

        public OtoPreviewControlViewModel(Oto oto, double length)
        {
            if (oto != null)
            {
                Filename = oto.Filename;
                Alias = oto.Alias;
                Offset = oto.Offset;
                Consonant = oto.Consonant + oto.Offset;
                Cutoff = oto.Cutoff < 0 ? oto.Offset - oto.Cutoff : length - oto.Cutoff;
                Preutterance = oto.Preutterance + oto.Offset;
                Overlap = oto.Overlap + oto.Offset;
                Length = length;
                Oto = oto;
            }
        }

        public OtoPreviewControlViewModel(string otoLine, double length) : this(Oto.Read(otoLine), length) { }

        public override void Load()
        {
            
        }

        public override string ToString()
        {
            return $"{(Oto == null ? "(null)" : Oto.Write())} : OtoPreviewViewModel";
        }
    }
}
