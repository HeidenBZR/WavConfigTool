﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WavConfigCore.Tools;

namespace WavConfigCore
{
    public class Replacer
    {
        public string Name = "";
        public List<string> FormatStrings;

        public Replacer()
        {
            InitFormats();
        }

        public void SetReplacement(string key, string value)
        {
            replacements[key] = value;
        }

        public void SetFormat(AliasType key, string value)
        {
            formats[key] = value;
        }

        public string Replace(string alias)
        {
            foreach (KeyValuePair<string, string> entry in replacements)
                alias = Regex.Replace(alias.ToString(), entry.Key, entry.Value);
            return alias;
        }

        public string MakeAlias(Phoneme[] phonemes, AliasType aliasType)
        {
            var alias = formats[aliasType];
            var multiconsonants = new List<string>();
            for (var i = 0; i < phonemes.Count(); i++)
            {
                var phoneme = phonemes[i];
                string mask = ExtractFirstPhonemeMask(alias);
                if (mask == null)
                    return null; // too many phonemes
                if (mask == MULTICONSONANT_MASK)
                {
                    var isCorrect = IsPhonemeTypeCorrect(CONSONANT_MASK, phoneme.Type);
                    if (IsPhonemeTypeCorrect(CONSONANT_MASK, phoneme.Type))
                    {
                        multiconsonants.Add(phoneme.Alias);
                    }
                    if (!isCorrect || i == phonemes.Count() - 1)
                    {
                        alias = new Regex(EcraneMask(mask)).Replace(alias, string.Join(formats[AliasType.CmC], multiconsonants), 1);
                        multiconsonants.Clear();
                        if (!isCorrect)
                        {
                            i--;
                        }
                    }
                }
                else
                {
                    if (multiconsonants.Count > 0)
                    {
                        throw new Exception();
                    }
                    else
                    {
                        if (!IsPhonemeTypeCorrect(mask, phoneme.Type))
                            return null; // wrong phoneme type
                        alias = new Regex(EcraneMask(mask)).Replace(alias, phoneme.Alias, 1);
                    }
                }
            }
            if (ExtractFirstPhonemeMask(alias) != null)
                return null; // not enough phonemes
            return Replace(alias);
        }

        public Dictionary<string, string> GetReplacements()
        {
            return replacements;
        }

        public Dictionary<AliasType, string> GetFormats()
        {
            return formats;
        }

        const string VOWEL_MASK = "$V";
        const string CONSONANT_MASK = "$C";
        const string MULTICONSONANT_MASK = "$C*";
        const string REST_MASK = "$R";

        private Dictionary<string, string> replacements = new Dictionary<string, string>();
        private Dictionary<AliasType, string> formats = new Dictionary<AliasType, string>();

        string ExtractFirstPhonemeMask(string format)
        {
            for (var i = 0; i + 1 < format.Length; i++)
            {
                if (i + 1 < format.Length && format.Length >= i + 3 && format.Substring(i, 3) == MULTICONSONANT_MASK)
                    return MULTICONSONANT_MASK;
                var sub = format.Substring(i, 2);
                if (sub == VOWEL_MASK || sub == CONSONANT_MASK || sub == REST_MASK)
                    return sub;
            }
            return null;
        }

        string EcraneMask(string mask)
        {
            return mask.Replace("$", "\\$").Replace("*", "\\*");
        }

        bool IsPhonemeTypeCorrect(string mask, PhonemeType phonemeType)
        {
            return mask == VOWEL_MASK && phonemeType == PhonemeType.Vowel || mask == REST_MASK && phonemeType == PhonemeType.Rest || 
                (mask == CONSONANT_MASK || mask == MULTICONSONANT_MASK) && phonemeType == PhonemeType.Consonant;
        }
        void InitFormats()
        {
            FormatStrings = new List<string>();
            foreach (AliasType aliasType in Enum.GetValues(typeof(AliasType)))
            {
                if (aliasType == AliasType.CmC)
                {
                    formats[AliasType.CmC] = " ";
                }
                else
                {
                    var aliasTypeString = AliasTypeResolver.Current.GetAliasTypeFormat(aliasType);
                    aliasTypeString = aliasTypeString.Replace("$C $V", "$C$V");
                    formats[aliasType] = aliasTypeString;
                }
                FormatStrings.Add(aliasType.ToString());
            }
        }
    }
}
