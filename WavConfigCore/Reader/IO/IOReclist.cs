using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigCore.Reader.IO
{

    [Serializable]
    public class IOPhonemes
    {
        public string[] Vowels;
        public string[] Consonants;
    }

    [Serializable]
    public class IOWavParams
    {
        public string Filename;
        public string[] Phonemes;
        public string Description = "";
    }

    [Serializable]
    public class IOReclist
    {
        public IOPhonemes Phonemes;
        public IOWavParams[] Files;
    }
}
