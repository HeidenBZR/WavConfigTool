﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Classes.Reader.IO;
using WavConfigTool.Tools;
using YamlDotNet.Serialization;

namespace WavConfigTool.Classes.Reader
{
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
            Settings.LastReclist = reclist != null ? reclist.Name : Settings.LastReclist;
            return reclist ?? new Reclist();
        }

        public void Write(string filename, Reclist reclist)
        {
            WriteYaml(filename, reclist);
        }

        private Reclist ReadYaml(string name)
        {
            var filename = PathResolver.Current.Reclist(name + ".reclist");
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var serializer = new Deserializer();
                IOReclist ioReclist = null;
                try
                {
                    ioReclist = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOReclist)) as IOReclist;
                }
                catch { }
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
            reclist.WavMask = WavMaskReader.Current.Read(PathResolver.Current.Reclist(reclist.Name + ".mask"));

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
}
