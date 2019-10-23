using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes.Reader
{
    class WavSettingsReader
    {
        private static WavSettingsReader current;
        private WavSettingsReader() { }

        public static WavSettingsReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new WavSettingsReader();
                }
                return current;
            }
        }

        public Reclist Read(string name)
        {
            var reclist = new Reclist();
            var filename = PathResolver.Reclist(name + ".wsettings");

            if (!File.Exists(filename))
                return reclist;

            reclist.Name = Path.GetFileNameWithoutExtension(filename);
            reclist.Location = filename;
            reclist.WavMask = WavMaskReader.Current.Read(PathResolver.Reclist(reclist.Name + ".mask"));

            string[] lines = File.ReadAllLines(filename, Encoding.UTF8);
            if (lines.Length < 2)
            {
                reclist.IsLoaded = false;
            }
            var vs = lines[0].Split(' ');
            var cs = lines[1].Split(' ');
            var phonemes = new List<Phoneme>
            {
                Rest.Create()
            };
            foreach (string v in vs) phonemes.Add(new Vowel(v));
            foreach (string c in cs) phonemes.Add(new Consonant(c));
            reclist.Phonemes = phonemes;

            for (int i = 2; i < lines.Length; i++)
            {
                string[] items = lines[i].Split('\t');
                if (items.Length < 2)
                    continue;
                string desc = items.Length >= 3 ? items[2] : $"{items[0]}";
                var reclinePhonemes = items[1].Split(' ').Select(n => reclist.GetPhoneme(n)).ToList();
                var recline = new Recline(reclist, Path.GetFileNameWithoutExtension(items[0]), reclinePhonemes, desc);
                reclist.AddRecline(recline);
            }
            reclist.IsLoaded = true;
            return reclist;
        }
    }
}
