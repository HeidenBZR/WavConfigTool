using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigCore.Reader
{
    public class IOWavOptions
    {
        public string WavPrefix { get; set; }
        public string WavSuffix { get; set; }
        public double UserScaleY { get; set; } = 1;
        public double UserScaleX { get; set; } = 1;
    }

    public class IOOtoOptions
    {
        public string OtoPrefix { get; set; }
        public string OtoSuffix { get; set; }
        public int VowelDecay { get; set; } = 170;
        public int ConsonantDecay { get; set; } = 80;
        public int RestDecay { get; set; } = 250;
        public int VowelAttack { get; set; } = 60;
        public int ConsonantAttack { get; set; } = 40;
        public int RestAttack { get; set; } = 50;
    }

    public class IOWavConfig
    {
        public string File { get; set; } = "";
        public int[] Vowels { get; set; } = new int[0];
        public int[] Consonants { get; set; } = new int[0];
        public int[] Rests { get; set; } = new int[0];
    }

    public class IOProjectOptions
    {
        public int PageSize { get; set; } = 5;
        public int OtoPageSize { get; set; } = 5;
        public int LastPage { get; set; } = 0;
        public bool MustHideCompleted { get; set; }
        public bool MustHideNotEnabled { get; set; }
    }

    public class IOViewOptions
    {
        public bool DoShowPitch { get; set; } = true;
        public bool DoShowSpectrum { get; set; } = false;
        public bool DoShowWaveform { get; set; } = true;
    }

    public class IOProject
    {
        public string Voicebank { get; set; }
        public string Reclist { get; set; }
        public string Replacer { get; set; } = "";
        public IOWavOptions WavOptions { get; set; } = new IOWavOptions();
        public IOOtoOptions OtoOptions { get; set; } = new IOOtoOptions();
        public IOProjectOptions ProjectOptions { get; set; } = new IOProjectOptions();
        public IOViewOptions ViewOptions { get; set; } = new IOViewOptions();
        public IOWavConfig[] WavConfigs { get; set; } = new IOWavConfig[0];
    }
}
