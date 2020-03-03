using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WavConfigCore
{
    public class Recline
    {
        public string Name;
        public string Description { get; set; }
        public List<Phoneme> PhonemesRaw { get; private set; }
        public List<Phoneme> Phonemes { get; private set; }
        public List<Phoneme> Vowels { get; private set; }
        public List<Phoneme> Consonants { get; private set; }
        public List<Phoneme> Rests { get; private set; }
        public Reclist Reclist;

        public Dictionary<string, Oto> Otos { get; set; }
        public Oto[] OtoList => Otos.Values.ToArray();
        public string InfoString { get; private set; }

        /// <summary>
        /// Определяет, присутствует ли фактически строка в реклисте, или это рудимент от старого, чтобы не потерять данные.
        /// Недоступный Recline не должен генерировать ото, но должен сохраняться в объекте и ВОЗМОЖНО его стоит отображать.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        public Recline(Reclist reclist, string name)
        {
            Reclist = reclist;
            Name = name;
            Phonemes = new List<Phoneme>();
            Description = "(Unknown Recline)";
            Otos = new Dictionary<string, Oto>();
            Phonemes = new List<Phoneme>();
            PhonemesRaw = new List<Phoneme>();

            int ind = Reclist.Reclines.IndexOf(this) + 1;
            InfoString = $"{ind}. {Description} [{String.Join(" ", Phonemes.Select(n => n.Alias))}]";
        }

        public Recline(Reclist reclist, string name, List<Phoneme> phonemes, string description) : this(reclist, name)
        {
            Description = description;

            Phonemes.Add(Rest.Create(this));
            foreach (var phoneme in phonemes)
            {
                Phonemes.Add(phoneme);
                PhonemesRaw.Add(phoneme);
            }
            Phonemes.Add(Rest.Create(this));

            Vowels = Phonemes.Where(n => n.IsVowel).ToList();
            Consonants = Phonemes.Where(n => n.IsConsonant).ToList();
            Rests = Phonemes.Where(n => n.IsRest).ToList();

            IsEnabled = true;
        }

        public List<Phoneme> GetPhonemesForGeneration()
        {
            var phonemesForGeneration = new List<Phoneme>();
            foreach (var phoneme in Phonemes)
            {
                if (phoneme.Type == PhonemeType.Rest)
                {
                    var r1 = Rest.Create(this);
                    var r2 = Rest.Create(this);
                    r1.Zone = new Zone(phoneme.Zone.In, phoneme.Zone.In);
                    r2.Zone = new Zone(phoneme.Zone.Out, phoneme.Zone.Out);
                    r1.HasZone = true;
                    r2.HasZone = true;
                    phonemesForGeneration.Add(r1);
                    phonemesForGeneration.Add(r2);
                }
                else
                {
                    phonemesForGeneration.Add(phoneme);
                }
            }
            return phonemesForGeneration;
        } 

        public List<Phoneme> PhonemesOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? Consonants :
                (type == PhonemeType.Rest ? Rests : Vowels);
        }

        public void ResetOto()
        {
            Otos = new Dictionary<string, Oto>();
        }

        public void AddOto(string alias, Oto oto)
        {
            Otos[alias] = oto;
        }

        public string WriteOto(string prefix = "", string suffix = "", string wavPrefix = "", string wavSuffix = "")
        {
            var text = new StringBuilder();
            foreach (var oto in OtoList)
            {
                text.AppendLine(oto.Write(suffix, prefix, wavPrefix, wavSuffix));
            }
            return text.ToString();
        }

        public override string ToString()
        {
            if (Phonemes is null)
                return "Recline: {undefined}";
            else if (Description is null)
                return $"Recline: [{string.Join(" ", Phonemes)}]";
            else
                return $"Recline: {Description} [{string.Join(" ", Phonemes.Select(n => n.Alias))}]";
        }

    }
}
