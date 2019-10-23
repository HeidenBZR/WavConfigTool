using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.Reader.IO
{

    [Serializable]
    class IOPhonemes
    {
        public string[] Vowels;
        public string[] Consonants;
    }

    [Serializable]
    class IOWavParams
    {
        public string Filename;
        public string[] Phonemes;
        public string Description = "";
    }

    [Serializable]
    class IOReclist
    {
        public IOPhonemes Phonemes;
        public IOWavParams[] Files;
    }
}
