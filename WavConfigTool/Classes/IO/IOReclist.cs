using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Tools;
using YamlDotNet.Serialization;

namespace WavConfigTool.Classes.IO
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
    class ReclistReader
    {
        private static ReclistReader current;
        private ReclistReader() { }

        public static ReclistReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new ReclistReader();
                }
                return current;
            }
        }

        public Reclist Read(string name)
        {
            var reclist = ReadYaml(name);
            return reclist ?? new Reclist();
        }

        public void Write(string filename, Reclist reclist)
        {
            WriteYaml(filename, reclist);
        }

        private Reclist ReadYaml(string name)
        {
            var filename = PathResolver.Reclist(name + ".reclist");
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var serializer = new Deserializer();
                var ioReclist = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOReclist)) as IOReclist;
                return ioReclist == null ? null : GetReclist(ioReclist, name);
            }
        }

        private void WriteYaml(string filename, Reclist reclist)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                var ioReclist = GetIOReclist(reclist);
                var serializer = new Serializer();
                serializer.Serialize(writer, ioReclist, typeof(IOReclist));
            }
        }

        private Reclist GetReclist(IOReclist ioReclist, string name)
        {
            var reclist = new Reclist();
            reclist.Name = name;
            reclist.WavMask = WavMaskReader.Current.Read(PathResolver.Reclist(reclist.Name + ".mask"));

            var phonemes = new List<Phoneme>
            {
                Rest.Create()
            };
            foreach (var phoneme in ioReclist.Phonemes.Consonants)
            {
                if (phoneme != Rest.ALIAS)
                    phonemes.Add(new Consonant(phoneme));
            }
            foreach (var phoneme in ioReclist.Phonemes.Vowels)
            {
                if (phoneme != Rest.ALIAS)
                    phonemes.Add(new Vowel(phoneme));
            }
            reclist.Phonemes = phonemes;

            foreach (var file in ioReclist.Files)
            {
                var reclinePhonemes = file.Phonemes.Select(n => reclist.GetPhoneme(n)).ToList();
                var recline = new Recline(reclist, file.Filename, reclinePhonemes, file.Description);
                reclist.AddRecline(recline);
            }
            reclist.IsLoaded = true;
            return reclist;
        }

        private IOReclist GetIOReclist(Reclist reclist)
        {
            if (reclist == null)
                return null;
            var iOReclist = new IOReclist();
            var phonemes = new IOPhonemes();
            phonemes.Vowels = reclist.Vowels.Select(n => n.Alias).ToArray();
            phonemes.Consonants = reclist.Consonants.Select(n => n.Alias).ToArray();
            iOReclist.Phonemes = phonemes;
            var files = new List<IOWavParams>();
            foreach (var recline in reclist.Reclines)
            {
                var wavParams = new IOWavParams()
                {
                    Filename = recline.Filename,
                    Description = recline.Description,
                    Phonemes = recline.PhonemesRaw == null ? new string[0] : recline.PhonemesRaw.Select(n => n.Alias).ToArray()
                };
                files.Add(wavParams);
            }
            iOReclist.Files = files.ToArray();
            return iOReclist;
        }
    }

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
