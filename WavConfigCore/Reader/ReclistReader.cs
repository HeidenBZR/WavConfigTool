using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore.Reader.IO;
using WavConfigCore.Tools;
using YamlDotNet.Serialization;

namespace WavConfigCore.Reader
{
    public class ReclistReader
    {
        #region singleton base

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

        #endregion

        public Reclist Read(string name)
        {
            var filename = PathResolver.Current.Reclist(name + PathResolver.RECLIST_EXT);
            var ioReclist = ReadYaml(filename);
            return ioReclist != null ? GetReclist(ioReclist, name) : new Reclist();
        }

        public Reclist ReadTest(string name)
        {
            var filename = PathResolver.Current.Reclist(name + PathResolver.RECLIST_EXT, true);
            var ioReclist = ReadYaml(filename);
            var reclist = ioReclist != null ? GetReclist(ioReclist, name, true) : new Reclist();
            return reclist;
        }

        public void WriteWithName(Reclist reclist)
        {
            if (reclist.Name == Reclist.EMPTY_NAME)
                return; // IsLoaded went wrong
            var filename = PathResolver.Current.Reclist(reclist.Name + PathResolver.RECLIST_EXT, reclist.IsTest);
            Write(filename, reclist);
        }

        public void Write(string filename, Reclist reclist)
        {
            WriteYaml(filename, GetIOReclist(reclist));
        }

        public IOReclist ReadYaml(string filename)
        {
            IOReclist ioReclist = null;
            try
            {
                using (var fileStream = new FileStream(filename, FileMode.Open))
                {
                    var serializer = new Deserializer();
                    ioReclist = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOReclist)) as IOReclist;
                }
            }
            catch { }
            return ioReclist;
        }

        public void WriteYaml(string filename, IOReclist ioReclist)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                var serializer = new Serializer();
                serializer.Serialize(writer, ioReclist, typeof(IOReclist));
            }
        }

        private Reclist GetReclist(IOReclist ioReclist, string name, bool isTest = false)
        {
            var reclist = new Reclist();
            reclist.IsTest = isTest;
            reclist.Name = name;
            reclist.WavMask = WavMaskReader.Current.Read(PathResolver.Current.Mask(reclist.Name, reclist.IsTest));

            var phonemes = new List<Phoneme>
            {
                Rest.Create()
            };
            foreach (var phoneme in ioReclist.Phonemes.Consonants)
            {
                if (phoneme == Rest.ALIAS || phoneme == "")
                    continue;

                // HACK: can't find how to escape ~ in yaml
                if (phoneme == null)
                    phonemes.Add(new Consonant("~"));
                else
                    phonemes.Add(new Consonant(phoneme));
            }
            foreach (var phoneme in ioReclist.Phonemes.Vowels)
            {
                if (phoneme == Rest.ALIAS || phoneme == "")
                    continue;

                if (phoneme == null)
                    phonemes.Add(new Vowel("~"));
                else
                    phonemes.Add(new Vowel(phoneme));
            }
            reclist.SetPhonemes(phonemes);

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
                    Filename = recline.Name,
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
