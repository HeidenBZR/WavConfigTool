using System;
using System.Collections.Generic;
using System.Linq;

namespace WavConfigTool.Classes
{
    public class Recline
    {
        public string Filename;
        public string Description { get; set; }
        public List<Phoneme> Phonemes;
        public List<Phoneme> Vowels { get { return Phonemes.Where(n => n.IsVowel).ToList(); } }
        public List<Phoneme> Consonants { get { return Phonemes.Where(n => n.IsConsonant).ToList(); } }
        public List<Phoneme> Rests { get { return Phonemes.Where(n => n.IsRest).ToList(); } }
        public Reclist Reclist;

        /// <summary>
        /// Определяет, присутствует ли фактически строка в реклисте, или это рудимент от старого, чтобы не потерять данные.
        /// Недоступный Recline не должен генерировать ото, но должен сохраняться в объекте и ВОЗМОЖНО его стоит отображать.
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        public string Name
        {
            get
            {
                int ind = Reclist.Reclines.IndexOf(this) + 1;
                return $"{ind}. {Description} [{String.Join(" ", Phonemes.Select(n => n.Alias))}]";
            }
        }

        public Recline (Reclist reclist, string filename)
        {
            Reclist = reclist;
            Filename = filename;
            Phonemes = new List<Phoneme>();
            Description = "(Unknown Recline)";
        }

        public Recline (Reclist reclist, string filename, string phonemes, string description)
        {
            Reclist = reclist;
            Filename = filename;
            Description = description;
            Phonemes = new List<Phoneme>();
            Phonemes.Add(new Rest("-") { Recline = this });
            foreach (string ph in phonemes.Split(' '))
            {
                Phoneme phoneme = Reclist.GetPhoneme(ph);
                phoneme.Recline = this;
                Phonemes.Add(phoneme);
            }
            Phonemes.Add(new Rest("-") { Recline = this });
            IsEnabled = true;
        }
        
        public List<Phoneme> PhonemesOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? Consonants :
                (type == PhonemeType.Rest ? Rests : Vowels);
        }

        public override string ToString()
        {
            if (Phonemes is null)
                return "Recline: {undefined}";
            else if (Description is null)
                return $"Recline: [{String.Join(" ", Phonemes)}]";
            else
                return $"Recline: {Description} [{String.Join(" ", Phonemes.Select(n => n.Alias))}]";
        }

    }
}
