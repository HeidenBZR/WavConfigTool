using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{
    public class Replacer
    {
        public string Name = "";

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
            foreach (Phoneme phoneme in phonemes)
            {
                string mask = ExtractFirstPhonemeMask(alias);
                if (mask == null)
                    return null; // too many phonemes
                if (!IsPhonemeTypeCorrect(mask, phoneme.Type))
                    return null; // wrong phoneme type
                alias = new Regex($"{"\\"}{mask}").Replace(alias, phoneme.Alias, 1);
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
        const string REST_MASK = "$R";

        private Dictionary<string, string> replacements = new Dictionary<string, string>();
        private Dictionary<AliasType, string> formats = new Dictionary<AliasType, string>();

        string ExtractFirstPhonemeMask(string format)
        {
            for (var i = 0; i + 1 < format.Length; i++)
            {
                var sub = format.Substring(i, 2);
                if (sub == VOWEL_MASK || sub == CONSONANT_MASK || sub == REST_MASK)
                    return sub;
            }
            return null;
        }
        bool IsPhonemeTypeCorrect(string mask, PhonemeType phonemeType)
        {
            return mask == VOWEL_MASK && phonemeType == PhonemeType.Vowel || mask == CONSONANT_MASK && phonemeType == PhonemeType.Consonant || mask == REST_MASK && phonemeType == PhonemeType.Rest;
        }
        void InitFormats()
        {
            foreach (AliasType aliasType in Enum.GetValues(typeof(AliasType)))
            {
                var aliasTypeString = AliasTypeResolver.Current.GetAliasTypeFormat(aliasType);
                aliasTypeString = aliasTypeString.Replace("$C $V", "$C$V");
                formats[aliasType] = aliasTypeString;
            }
        }
    }
}
