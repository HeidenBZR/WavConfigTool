using System.Collections.Generic;
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

        public List<string> Aliases { get; private set; }

        public string Name { get; private set; } = "(Reclist is not avialable)";
        public string Location { get; private set; }

        private Dictionary<string, Recline> _reclineByFilename;

        public bool IsLoaded { get; private set; } = false;

        public Reclist(string location)
        {
            Location = Settings.GetResoucesPath(Path.Combine("WavConfigTool", "WavSettings", location + ".reclist"));
            if (!File.Exists(Location))
                Location = Settings.GetResoucesPath(Path.Combine("WavConfigTool", "WavSettings", location + ".wsettings"));
            if (!File.Exists(Settings.GetResoucesPath(Path.Combine("WavConfigTool", "WavSettings", Location))))
                return;

            IsLoaded = Read();
            Name = Path.GetFileNameWithoutExtension(Location);
        }

        public bool Read()
        {
            string[] lines = File.ReadAllLines(Location, Encoding.GetEncoding(932));
            if (lines.Length < 2)
            {
                IsLoaded = false;
            }
            var vs = lines[0].Split(' ');
            var cs = lines[1].Split(' ');
            Phonemes = new List<Phoneme>();
            foreach (string v in vs) Phonemes.Add(new Vowel(v));
            foreach (string c in cs) Phonemes.Add(new Consonant(c));

            Reclines = new List<Recline>();
            _reclineByFilename = new Dictionary<string, Recline>();
            for (int i = 2; i < lines.Length; i++)
            {
                string[] items = lines[i].Split('\t');
                if (items.Length != 3)
                    continue;
                AddRecline(new Recline(this, items[0], items[1], items[2]));
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

        public void ResetAliases()
        {
            // TODO: Ввести параметр "максимальное количество дубликатов", чтобы можно было несколько дубликатов все же иметь
            Aliases = new List<string>();
        }
    }
}
