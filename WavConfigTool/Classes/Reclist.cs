﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{

    public class Reclist
    {
        public static List<Phoneme> Phonemes;
        public List<Recline> Reclines;
        public List<Phoneme> Vowels { get { return Phonemes.Where(n => n.IsVowel).ToList(); } }
        public List<Phoneme> Consonants { get { return Phonemes.Where(n => n.IsConsonant).ToList(); } }

        public string Name { get; private set; } = "(Reclist is not avialable)";
        public string Location { get; private set; }

        private Dictionary<string, Recline> _reclineByFilename;

        public bool IsLoaded { get; private set; } = false;

        public WavMask WavMask { get; private set; } = new WavMask(false);

        public Reclist(string location)
        {
            Phonemes = new List<Phoneme>();
            Reclines = new List<Recline>();
            _reclineByFilename = new Dictionary<string, Recline>();
            IsLoaded = false;

            Location = PathResolver.Reclist(location + ".reclist");
            if (!File.Exists(Location))
                Location = PathResolver.Reclist(location + ".wsettings");

            if (File.Exists(Location))
            {
                IsLoaded = Read();
                Name = Path.GetFileNameWithoutExtension(Location);
                WavMask = IO.WavMaskReader.GetInstance().Read(PathResolver.Reclist(Name + ".mask"));
            }
        }

        public bool Read()
        {
            string[] lines = File.ReadAllLines(Location, Encoding.UTF8);
            if (lines.Length < 2)
            {
                IsLoaded = false;
            }
            var vs = lines[0].Split(' ');
            var cs = lines[1].Split(' ');
            Phonemes = new List<Phoneme>();
            Phonemes.Add(new Rest("-"));
            foreach (string v in vs) Phonemes.Add(new Vowel(v));
            foreach (string c in cs) Phonemes.Add(new Consonant(c));

            Reclines = new List<Recline>();
            _reclineByFilename = new Dictionary<string, Recline>();
            for (int i = 2; i < lines.Length; i++)
            {
                string[] items = lines[i].Split('\t');
                if (items.Length < 2)
                    continue;
                string desc = items.Length >= 3 ? items[2] : $"{items[0]}";
                AddRecline(new Recline(this, Path.GetFileNameWithoutExtension(items[0]), items[1], desc));
            }

            return true;
        }

        void AddRecline(Recline recline)
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

        public Phoneme GetPhoneme(string alias)
        {
            var phoneme = Phonemes.Find(n => n.Alias == alias);
            if (phoneme is null)
            {
                Phonemes.Add(new Consonant(alias));
                phoneme = Phonemes.Find(n => n.Alias == alias);
            }
            return phoneme.Clone();
        }

        public Recline GetRecline(string filename)
        {
            if (_reclineByFilename.TryGetValue(filename, out Recline recline))
                return recline;
            else
                return AddUnknownRecline(filename);
        }

    }
}
