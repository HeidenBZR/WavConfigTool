using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WavConfigTool
{

    public class Reclist
    {
        public static List<Phoneme> Phonemes;
        public List<Recline> Reclines;
        public string VoicebankPath;
        public List<Phoneme> Vowels { get { return Phonemes.Where(n => n.IsVowel).ToList(); } }
        public List<Phoneme> Consonants { get { return Phonemes.Where(n => n.IsConsonant).ToList(); } }
        public List<string> Aliases;

        public Reclist(string[] vs, string[] cs)
        {
            Phonemes = new List<Phoneme>();
            foreach (string v in vs) Phonemes.Add(new Vowel(v));
            foreach (string c in cs) Phonemes.Add(new Consonant(c));
            Reclines = new List<Recline>();
        }
    }

    public class Recline
    {
        public string Filename;
        public string Description { get; set; }
        public List<Phoneme> Phonemes;
        public List<Phoneme> Vowels { get { return Phonemes.Where(n => n.IsVowel).ToList(); } }
        public List<Phoneme> Consonants { get { return Phonemes.Where(n => n.IsConsonant).ToList(); } }
        public Reclist Reclist;
        public string Path { get { return System.IO.Path.Combine(Reclist.VoicebankPath, Filename); } }
        public string Name
        {
            get
            {
                int ind = Reclist.Reclines.IndexOf(this) + 1;
                return $"{ind}. {Description} [{String.Join(" ", Phonemes.Select(n => n.Alias))}]";
            }
        }

        public Recline(string filename, string phonemes)
        {
            Filename = filename;
            Description = phonemes;
            Phonemes = new List<Phoneme>();
            foreach (string ph in phonemes.Split(' '))
            {
                Phoneme phoneme = Reclist.Phonemes.Find(n => n.Alias == ph).Clone();
                phoneme.Recline = this;
                Phonemes.Add(phoneme);
            }
        }

        public Recline(string filename, string phonemes, string description) : this(filename, phonemes)
        {
            Description = description;
        }
    }
}
