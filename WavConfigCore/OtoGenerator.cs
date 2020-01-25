﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WavConfigCore.Tools;

namespace WavConfigCore
{
    public class OtoGenerator
    {
        public Reclist Reclist { get; set; }
        public Project Project { get; set; }
        public Replacer Replacer { get; set; }

        public bool MustGeneratePreoto { get; set; } = true;

        public OtoGenerator(Reclist reclist, Project project, Replacer replacer)
        {
            Reclist = reclist;
            Project = project;
            Replacer = replacer;
        }

        public void Generate(ProjectLine projectLine)
        {
            var recline = projectLine.Recline;
            projectLine.CalculateZones();
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
                    var otoRaw = GenerateSingleOto(projectLine, position, phonemes.ToArray(), phonemesOfType);
                    if (otoRaw != null)
                    {
                        (var alias, var oto) = Project.AddOto(otoRaw);
                        recline.AddOto(alias, oto);
                    }
                }

                if (reclinePhonemes[i].Type != PhonemeType.Consonant)
                {
                    position++;
                }
            }
        }

        #region private

        private Oto GenerateSingleOto(ProjectLine projectLine, int position, Phoneme[] phonemes, Dictionary<PhonemeType, List<Phoneme>> phonemesOfType)
        {
            var recline = projectLine.Recline;

            Phoneme p1 = phonemes.First();
            Phoneme p2 = phonemes.Last();
            double offset = 0, consonant = 0, cutoff = 0, preutterance = 0, overlap = 0;
            bool hasZones = projectLine.ApplyZones(phonemesOfType[p1.Type], p1) && projectLine.ApplyZones(phonemesOfType[p2.Type], p2);
            AliasType aliasType = AliasTypeResolver.Current.GetAliasType(GetAliasType(phonemes));
            bool masked = aliasType != AliasType.undefined && Reclist.WavMask.CanGenerateOnPosition(projectLine.Recline.Name, aliasType, position);

            if (!masked)
                return null;

            string alias = Replacer.MakeAlias(phonemes, aliasType);
            if (alias == null)
                return null;

            bool hasAliasType = true;
            switch (aliasType)
            {
                // Absolute values, relative ones are made in Oto on write (!)
                case AliasType.V:
                    offset = p1.Zone.In + Project.VowelDecay - p1.Attack;
                    overlap = p1.Zone.In + Project.VowelDecay;
                    preutterance = overlap - 5;
                    consonant = overlap;
                    cutoff = p1.Zone.Out - p1.Attack;
                    break;

                // Ends with vowel
                case AliasType.VV:
                case AliasType.CV:
                case AliasType.CmV:
                case AliasType.VCV:
                case AliasType.VCmV:
                    offset = p1.Zone.Out - p1.Attack < p1.Zone.In ? p1.Zone.In : p1.Zone.Out - p1.Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.In + Project.VowelDecay;
                    cutoff = p2.Zone.Out - p2.Attack;
                    break;

                case AliasType.RV:
                    offset = p1.Zone.Out - p1.Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.In + Project.VowelDecay;
                    cutoff = p2.Zone.Out - p2.Attack;
                    break;

                case AliasType.RCV:
                case AliasType.RCmV:
                    offset = p1.Zone.Out - p1.Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.In + Project.VowelDecay;
                    cutoff = p2.Zone.Out - p2.Attack;
                    break;

                case AliasType.RC:
                case AliasType.RCm:
                    offset = p1.Zone.In - p1.Attack;
                    overlap = p1.Zone.In;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out + 80;
                    cutoff = p2.Zone.Out + 90;
                    break;

                // Ends with Rest

                case AliasType.VR:
                case AliasType.VCR:
                case AliasType.VCmR:
                    offset = p1.Zone.Out - p1.Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out + Project.VowelDecay;
                    cutoff = 0;
                    break;

                case AliasType.CR:
                case AliasType.CmR:
                    offset = p1.Zone.In;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out + Project.VowelDecay;
                    cutoff = 0;
                    break;

                // Ends with Consonant
                case AliasType.VC:
                case AliasType.VCm:
                case AliasType.Cm:
                    offset = p1.Zone.Out - p1.Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - p2.Attack;
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

        private string GetAliasType(params Phoneme[] phonemes)
        {
            return String.Join("", phonemes.Select(n => n.Type.ToString().Substring(0, 1)));
        }

        #endregion
    }
}
