﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WavConfigTool.Classes
{
    public class Recline
    {
        public string Filename;
        public string Description { get; set; }
        public List<Phoneme> PhonemesRaw;
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

        public Dictionary<string, Oto> Otos { get; set; }
        public Oto[] OtoList { get => Otos.Values.ToArray(); }
        public string Name
        {
            get
            {
                int ind = Reclist.Reclines.IndexOf(this) + 1;
                return $"{ind}. {Description} [{String.Join(" ", Phonemes.Select(n => n.Alias))}]";
            }
        }

        public Recline(Reclist reclist, string filename)
        {
            Reclist = reclist;
            Filename = filename;
            Phonemes = new List<Phoneme>();
            Description = "(Unknown Recline)";
            Otos = new Dictionary<string, Oto>();
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
        public Recline(Reclist reclist, string filename, List<Phoneme> phonemes, string description)
        {
            Reclist = reclist;
            Filename = filename;
            Description = description;
            Phonemes = new List<Phoneme>();
            PhonemesRaw = new List<Phoneme>();
            Phonemes.Add(Rest.Create(this));
            foreach (var phoneme in phonemes)
            {
                Phonemes.Add(phoneme);
                PhonemesRaw.Add(phoneme);
            }
            Phonemes.Add(Rest.Create(this));
            Otos = new Dictionary<string, Oto>();
            IsEnabled = true;
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

        public string WriteOto(string prefix = "", string suffix = "")
        {
            var text = new StringBuilder();
            foreach (var oto in OtoList)
            {
                text.AppendLine(oto.Write(suffix, prefix));
            }
            return text.ToString();
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
