using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{

    public class Reclist
    {
        public List<Phoneme> Phonemes;
        public List<Recline> Reclines;
        public List<Phoneme> Vowels { get { return Phonemes.Where(n => n.IsVowel).ToList(); } }
        public List<Phoneme> Consonants { get { return Phonemes.Where(n => n.IsConsonant).ToList(); } }

        public string Name { get; set; } = "(Reclist is not avialable)";
        public string Location { get; set; }

        private Dictionary<string, Recline> _reclineByFilename;

        public bool IsLoaded { get; set; }


        public WavMask WavMask { get; set; } = new WavMask(false);

        public Reclist()
        {
            Phonemes = new List<Phoneme>();
            Reclines = new List<Recline>();
            _reclineByFilename = new Dictionary<string, Recline>();
            IsLoaded = true;
        }

        public void AddRecline(Recline recline)
        {
            _reclineByFilename[recline.Filename] = recline;
            Reclines.Add(recline);
        }

        Recline AddUnknownRecline(string filename)
        {
            var recline = new Recline(this, filename);
            AddRecline(recline);
            return recline;
        }

        public Phoneme GetPhoneme(string rawAlias)
        {
            // HACK: can't find how to escape ~ in yaml
            var alias = rawAlias != null ? rawAlias : "~";
            var phoneme = Phonemes.Find(n => n.Alias == alias);
            if (phoneme is null)
            {
                Phonemes.Add(new Consonant(alias));
                phoneme = Phonemes.Find(n => n.Alias == alias);
            }
            var clone = phoneme.Clone();
            return clone;
        }

        public Recline GetRecline(string filename)
        {
            if (_reclineByFilename.TryGetValue(filename, out Recline recline))
                return recline;
            else
                return AddUnknownRecline(filename);
        }

        internal void SetReclines(List<Recline> reclines, Dictionary<string, Recline> reclineByFilename)
        {
            Reclines = reclines;
            _reclineByFilename = reclineByFilename;
        }
    }
}
