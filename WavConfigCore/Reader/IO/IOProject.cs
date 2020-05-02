using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigCore.Reader
{

    [Serializable]
    public class IOWavOptions
    {
        public string WavPrefix;
        public string WavSuffix;
        public double UserScaleY = 1;
        public double UserScaleX = 1;
    }
    [Serializable]
    public class IOOtoOptions
    {
        public string OtoPrefix;
        public string OtoSuffix;
        public int VowelDecay = 170;
        public int ConsonantDecay = 80;
        public int RestDecay = 250;
        public int VowelAttack = 60;
        public int ConsonantAttack = 40;
        public int RestAttack = 50;
    }

    [Serializable]
    public class IOWavConfig
    {
        public string File = "";
        public int[] Vowels = new int[0];
        public int[] Consonants = new int[0];
        public int[] Rests = new int[0];
    }

    [Serializable]
    public class IOProjectOptions
    {
        public int PageSize = 5;
        public int OtoPageSize = 5;
        public int LastPage = 0;
        public bool MustHideCompleted;
        public bool MustHideNotEnabled;
    }

    [Serializable]
    public class IOProject
    {
        public string Voicebank;
        public string Reclist;
        public string Replacer = "";
        public IOWavOptions WavOptions = new IOWavOptions();
        public IOOtoOptions OtoOptions = new IOOtoOptions();
        public IOProjectOptions ProjectOptions = new IOProjectOptions();
        public IOWavConfig[] WavConfigs = new IOWavConfig[0];
    }
}
