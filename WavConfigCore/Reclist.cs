using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WavConfigCore.Tools;

namespace WavConfigCore
{

    public class Reclist
    {
        public List<Phoneme> Phonemes { get; private set; }
        public List<Phoneme> Vowels { get; private set; }
        public List<Phoneme> Consonants { get; private set; }

        public List<Recline> Reclines { get; private set; }

        public string Name { get; set; }
        public WavMask WavMask { get; set; }
        public bool IsLoaded { get; set; }

        public const string EMPTY_NAME = "(Reclist is not avialable)";

        public Reclist()
        {
            Name = EMPTY_NAME;
            WavMask = new WavMask(false);
            Phonemes = new List<Phoneme>();
            Reclines = new List<Recline>();
            reclineByFilename = new Dictionary<string, Recline>();
        }

        public void AddRecline(Recline recline)
        {
            reclineByFilename[recline.Name] = recline;
            Reclines.Add(recline);
        }

        public void SetPhonemes(List<Phoneme> phonemes)
        {
            Phonemes = phonemes;
            Vowels = Phonemes.Where(n => n.IsVowel).ToList();
            Consonants = Phonemes.Where(n => n.IsConsonant).ToList();
        }

        public Phoneme GetPhoneme(string rawAlias)
        {
            // HACK: can't find how to escape ~ in yaml
            var alias = rawAlias ?? "~";
            var phoneme = Phonemes.Find(n => n.Alias == alias);
            if (phoneme is null)
            {
                phoneme = new Consonant(alias);
                Phonemes.Add(phoneme);
                Consonants.Add(phoneme);
            }
            var clone = phoneme.Clone();
            return clone;
        }

        public Recline GetRecline(string filename)
        {
            if (reclineByFilename.TryGetValue(filename, out Recline recline))
                return recline;
            else
                return CreateUnknownRecline(filename);
        }

        public void ApplyZones(ProjectLine projectLine)
        {
            var completed = true;
            completed &= ApplyZonesAndReturnCompleted(projectLine, PhonemeType.Vowel);
            completed &= ApplyZonesAndReturnCompleted(projectLine, PhonemeType.Consonant);
            completed &= ApplyZonesAndReturnCompleted(projectLine, PhonemeType.Rest);
            projectLine.IsCompleted = completed;
        }



        #region private

        private readonly Dictionary<string, Recline> reclineByFilename;

        private bool ApplyZonesAndReturnCompleted(ProjectLine projectLine, PhonemeType type)
        {
            var points = projectLine.PointsOfType(type, false);
            var zones = projectLine.ZonesOfType(type);
            var phonemes = projectLine.Recline.PhonemesOfType(type);
            var filename = projectLine.Recline.Name;

            zones.Clear();
            var completed = true;

            int pointI = 0;
            if (type == PhonemeType.Rest)
            {
                for (int i = 0; i < phonemes.Count; i++)
                {
                    while (WavMask.MustSkipPhoneme(filename, type, i) && i <= phonemes.Count)
                    {
                        phonemes[i].IsSkipped = true;
                        i++;
                    }
                    if (i >= phonemes.Count)
                        return completed;
                    var phoneme = phonemes[i];
                    phoneme.IsSkipped = false;

                    if (pointI >= points.Count)
                    {
                        completed = false;
                        phoneme.Zone = new Zone();
                        phoneme.HasZone = false;
                    }
                    else
                    {
                        var pointIn = points[pointI];
                        var pointOut = points[pointI];
                        pointI++;
                        phoneme.Zone = new Zone(pointIn, pointOut);
                        phoneme.HasZone = true;
                        zones.Add(phoneme.Zone);
                    }
                }
            }
            else
            {
                for (int i = 0; i < phonemes.Count; i++)
                {
                    while (WavMask.MustSkipPhoneme(filename, type, i) && i < phonemes.Count)
                    {
                        phonemes[i].IsSkipped = true;
                        i++;
                    }
                    if (i >= phonemes.Count)
                        return completed;
                    var phoneme = phonemes[i];
                    phoneme.IsSkipped = false;

                    if (pointI + 1 >= points.Count)
                    {
                        completed = false;
                        phoneme.Zone = new Zone();
                        phoneme.HasZone = false;
                    }
                    else
                    {
                        var pointIn = points[pointI];
                        pointI++;
                        var pointOut = points[pointI];
                        pointI++;
                        phoneme.Zone = new Zone(pointIn, pointOut);
                        phoneme.HasZone = true;
                        zones.Add(phoneme.Zone);
                    }
                }
            }
            return completed;
        }

        private Recline CreateUnknownRecline(string filename)
        {
            var recline = new Recline(this, filename);
            AddRecline(recline);
            return recline;
        }

        #endregion
    }
}
