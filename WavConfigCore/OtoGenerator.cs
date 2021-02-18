using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WavConfigCore.Tools;

namespace WavConfigCore
{
    public class OtoGenerator
    {
        public Project Project { get; set; }

        public bool MustGeneratePreoto { get; set; } = true;

        public OtoGenerator(Project project)
        {
            Project = project;
        }

        public void GenerateAllAndSave(string filename)
        {
            Project.ResetOto();
            var text = new StringBuilder();

            foreach (Recline recline in Project.Reclist.Reclines)
            {
                recline.ResetOto();
                var projectLine = Project.ProjectLinesByFilename[recline.Name];
                projectLine.Sort();
                GenerateFromProjectLine(projectLine);

                text.Append(recline.WriteOto(Project.Suffix, Project.Prefix, Project.WavPrefix, Project.WavSuffix));
            }
            File.WriteAllText(filename, text.ToString(), Encoding.GetEncoding(932));
        }

        public void GenerateFromProjectLine(ProjectLine projectLine)
        {
            var recline = projectLine.Recline;
            var position = 0;
            var reclinePhonemes = recline.GetPhonemesForGeneration();

            var phonemesOfType = new Dictionary<PhonemeType, List<Phoneme>>
            {
                [PhonemeType.Vowel] = new List<Phoneme>(),
                [PhonemeType.Consonant] = new List<Phoneme>(),
                [PhonemeType.Rest] = new List<Phoneme>()
            };
            foreach (var phoneme in reclinePhonemes)
                phonemesOfType[phoneme.Type].Add(phoneme);

            for (int i = 1; i < reclinePhonemes.Count - 1; i++)
            {
                for (int count = 1; count < 6 && i + count - 1 < reclinePhonemes.Count; count++)
                {
                    var phonemes = reclinePhonemes.GetRange(i, count);
                    var next = i + count < reclinePhonemes.Count ? reclinePhonemes[i + count] : null;
                    var otoRaw = GenerateSingleOto(projectLine, position, phonemes.ToArray(), phonemesOfType, next);
                    if (otoRaw != null)
                    {
                        (var alias, var oto) = Project.AddOto(otoRaw);
                        if (oto != null)
                        {
                            recline.AddOto(alias, oto);
                        }
                    }
                }

                if (reclinePhonemes[i].Type != PhonemeType.Consonant)
                {
                    position++;
                }
            }
        }

        #region private

        private Oto GenerateSingleOto(ProjectLine projectLine, int position, Phoneme[] phonemes, Dictionary<PhonemeType, List<Phoneme>> phonemesOfType, Phoneme next)
        {
            var recline = projectLine.Recline;

            Phoneme p1 = phonemes.First();
            Phoneme p2 = phonemes.Last();
            double offset = 0, consonant = 0, cutoff = 0, preutterance = 0, overlap = 0;
            bool hasZones = p1.HasZone && p2.HasZone;
            AliasType aliasType = AliasTypeResolver.Current.GetAliasType(GetAliasType(phonemes));
            bool masked = aliasType != AliasType.undefined && Project.Reclist.WavMask.CanGenerateOnPosition(projectLine.Recline.Name, aliasType, position);
            var p1Attack = Project.AttackOfType(p1.Type);
            var p2Attack = Project.AttackOfType(p2.Type);

            if (!masked)
                return null;

            string alias = Project.Replacer.MakeAlias(phonemes, aliasType);
            if (alias == null)
                return null;

            bool hasAliasType = true;
            switch (aliasType)
            {
                // Absolute values, relative ones are made in Oto on write (!)
                case AliasType.V:
                    offset = p1.Zone.In + Project.DecayV;
                    overlap = p1.Zone.In + Project.DecayV + Project.DecayV;
                    preutterance = overlap - 5;
                    consonant = overlap;
                    cutoff = p1.Zone.Out - p1Attack;
                    break;

                // Ends with vowel
                case AliasType.VV:
                case AliasType.CV:
                case AliasType.CmV:
                case AliasType.VCV:
                case AliasType.VCmV:
                    offset = p1.Zone.Out - p1Attack < p1.Zone.In ? p1.Zone.In : p1.Zone.Out - p1Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.In + Project.DecayV;
                    cutoff = p2.Zone.Out - p2Attack;
                    break;

                case AliasType.RV:
                    offset = p1.Zone.Out;
                    overlap = p1.Zone.Out + p1Attack;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.In + Project.DecayV;
                    cutoff = p2.Zone.Out - p2Attack;
                    break;

                case AliasType.RCV:
                case AliasType.RCmV:
                    offset = p1.Zone.Out;
                    overlap = p1.Zone.Out + p1Attack;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.In + Project.DecayV;
                    cutoff = p2.Zone.Out - p2Attack;
                    break;

                case AliasType.RC:
                case AliasType.RCm:
                    var nextP = next != null ? next.Zone.In : p2.Zone.Out;
                    var firstC = phonemes[1];
                    offset = firstC.Zone.In - p1Attack;
                    overlap = firstC.Zone.In;
                    preutterance = p2.Zone.In;
                    consonant = nextP + Project.DecayC;
                    cutoff = nextP + Project.DecayC + Project.AttackC;
                    break;

                // Ends with Rest

                case AliasType.VR:
                case AliasType.VCR:
                case AliasType.VCmR:
                    offset = p1.Zone.Out - p1Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out + Project.DecayR;
                    cutoff = next == null || IsNextLast(next) ? 0 : next.Zone.In - Project.AttackR;
                    break;

                case AliasType.CR:
                case AliasType.CmR:
                    offset = p1.Zone.In;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out + Project.DecayR;
                    cutoff = next == null || IsNextLast(next) ? 0 : next.Zone.In - Project.AttackR;
                    break;

                // Ends with Consonant
                case AliasType.VC:
                case AliasType.VCm:
                case AliasType.Cm:
                    offset = p1.Zone.Out - p1Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - p2Attack;
                    cutoff = p2.Zone.Out;
                    break;

                default:
                    hasAliasType = false;
                    break;
            }
            if (hasAliasType)
            {
                if (hasZones)
                {
                    var oto = new Oto(recline.Name, alias, offset, consonant, cutoff, preutterance, overlap);
                    oto.Smarty();
                    return oto;
                }
                else if (MustGeneratePreoto)
                {
                    return new Oto(recline.Name, alias, 10, 100, 150, 60, 40);
                }
            }
            return null;
        }

        private bool IsNextLast(Phoneme nextPhoneme)
        {
            return nextPhoneme != null && nextPhoneme.Type == PhonemeType.Rest && nextPhoneme.IsLast;
        }

        private string GetAliasType(params Phoneme[] phonemes)
        {
            return String.Join("", phonemes.Select(n => n.Type.ToString().Substring(0, 1)));
        }

        #endregion
    }
}
