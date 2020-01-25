using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WavConfigCore.Tools;

namespace WavConfigCore
{

    public class Reclist
    {
        public List<Phoneme> Phonemes { get; private set; }
        public List<Phoneme> Vowels { get; private set; }
        public List<Phoneme> Consonants { get; private set; }

        public List<Recline> Reclines { get; private set; }

        public string Name { get; set; }
        public WavMask WavMask { get; set; }
        public bool IsLoaded { get; set; }

        public const string EMPTY_NAME = "(Reclist is not avialable)";

        public Reclist()
        {
            Name = EMPTY_NAME;
            WavMask = new WavMask(false);
            Phonemes = new List<Phoneme>();
            Reclines = new List<Recline>();
            reclineByFilename = new Dictionary<string, Recline>();
            IsLoaded = true;
        }

        public void AddRecline(Recline recline)
        {
            reclineByFilename[recline.Name] = recline;
            Reclines.Add(recline);
        }

        public void SetPhonemes(List<Phoneme> phonemes)
        {
            Phonemes = phonemes;
            Vowels = Phonemes.Where(n => n.IsVowel).ToList();
            Consonants = Phonemes.Where(n => n.IsConsonant).ToList();
        }

        public Phoneme GetPhoneme(string rawAlias)
        {
            // HACK: can't find how to escape ~ in yaml
            var alias = rawAlias ?? "~";
            var phoneme = Phonemes.Find(n => n.Alias == alias);
            if (phoneme is null)
            {
                phoneme = new Consonant(alias);
                Phonemes.Add(phoneme);
                Consonants.Add(phoneme);
            }
            var clone = phoneme.Clone();
            return clone;
        }

        public Recline GetRecline(string filename)
        {
            if (reclineByFilename.TryGetValue(filename, out Recline recline))
                return recline;
            else
                return CreateUnknownRecline(filename);
        }

        #region private

        private readonly Dictionary<string, Recline> reclineByFilename;

        private Recline CreateUnknownRecline(string filename)
        {
            var recline = new Recline(this, filename);
            AddRecline(recline);
            return recline;
        }

        #endregion
    }
}
