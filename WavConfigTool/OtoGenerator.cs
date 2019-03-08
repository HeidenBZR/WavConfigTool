using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WavConfigTool
{
    public class OtoGenerator
    {

        public Dictionary<string, string> Replacement { get; private set; }

        public static OtoGenerator Current { get; private set; }

        private OtoGenerator(string voicebankType)
        {
            string path = ProjectWindow.GetTempPath(@"WavConfigTool\WavSettings\" + voicebankType + ".txt");
            Replacement = ReadReplacementFile(path);
        }

        public static void Init(string vocebankType)
        {
            Current = new OtoGenerator(vocebankType);
        }

        public Dictionary<string, string> ReadReplacementFile(string path)
        {
            if (!System.IO.File.Exists(path))
                return null;
            try
            {
                var replacement = new Dictionary<string, string>();
                foreach (string line in System.IO.File.ReadAllLines(path, Encoding.UTF8))
                {
                    if (line.Contains("="))
                    {
                        var pair = line.Split('=');
                        pair[0] = pair[0].Replace("%V%", $"({(string.Join("|", Reclist.Current.Vowels.Select(n => n.Alias)))})");
                        pair[0] = pair[0].Replace("%C%", $"({(string.Join("|", Reclist.Current.Consonants.Select(n => n.Alias)))})");
                        replacement[pair[0].Trim().Replace("\\s", " ")] = pair[1].Trim().Replace("\\s", " ");
                    }
                }
                return replacement;
            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "can't read replacement file");
                return null;
            }
        }

        public string Oto(double of, double con, double cut, double pre, double ov)
        {
            string f = "f0";
            // Relative values
            ov -= of;
            pre -= of;
            con -= of;
            if (cut != 0)
                cut = -(cut - of);
            else
                cut = 10;

            return $"{of.ToString(f)},{con.ToString(f)},{cut.ToString(f)},{pre.ToString(f)},{ov.ToString(f)}";
        }

        public string Replace(string alias)
        {
            if (Replacement is null)
                Replacement = new Dictionary<string, string>();

            foreach (var alias_type in new[] { "VC", "CC", "RV", "RC", "VR", "CR", "VV" })
            {
                if (!Replacement.ContainsKey($"#{alias_type}#"))
                    alias = alias.Replace($"#{alias_type}#", " ");
            }
            foreach (var alias_type in new[] { "CV"})
            {
                if (!Replacement.ContainsKey($"#{alias_type}#"))
                    alias = alias.Replace($"#{alias_type}#", "");
            }


            foreach (KeyValuePair<string, string> entry in Replacement)
                alias = Regex.Replace(alias.ToString(), entry.Key, entry.Value);
            return alias;
        }

        public string GetAlias(params Phoneme[] phonemes)
        {
            var alias = new StringBuilder(phonemes[0].Alias);
            for (int i = 1; i < phonemes.Length; i++)
            {
                var alias_type = GetAliasType(phonemes[i - 1], phonemes[i]);
                var alias_type2 = i + 1 < phonemes.Length ? GetAliasType(phonemes[i - 1], phonemes[i], phonemes[i + 1]) : "";
                switch (alias_type)
                {
                    case "VC":
                    case "RC":
                    case "RV":
                    case "CR":
                    case "VR":
                    case "CC":
                    case "VV":
                    case "CV":
                        alias.Append($"#{alias_type}#");
                        break;

                }
                alias.Append(phonemes[i].Alias);
            }

            return Replace(alias.ToString());
        }

        public string GetAliasType(params Phoneme[] phonemes)
        {
            return String.Join("", phonemes.Select(n => n.Type.ToString().Substring(0, 1)));
        }

        public string Generate(string filename, params Phoneme[] phonemes)
        {
            string alias = GetAlias(phonemes);
            if (MainWindow.Current.Reclist.Aliases.Contains(alias))
                return "";

            double offset, consonant, cutoff, preutterance, overlap;
            Phoneme p1 = phonemes.First();
            Phoneme p2 = phonemes.Last();
            if (p1.Zone.In == 0 || p1.Zone.Out == 0 || p2.Zone.In == 0 || p2.Zone.Out == 0)
                return "";

            switch (GetAliasType(phonemes))
            {
                // Absolute values, relative ones are made in Oto()

                // Ends with vowel
                case "VV":
                case "RV":
                case "CV":
                case "CCV":
                case "CCCV":
                case "CCCCV":
                case "VCV":
                case "VCCV":
                case "VCCCV":
                    offset = p1.Zone.Out - p1.Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - WavControl.VowelSustain - p2.Attack;
                    cutoff = p2.Zone.Out - p2.Attack;
                    break;

                case "RCV":
                case "RCCV":
                case "RCCCV":
                    offset = p1.Zone.In;
                    overlap = p1.Zone.Out + p1.Attack;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - WavControl.VowelSustain - p2.Attack;
                    cutoff = p2.Zone.Out - p2.Attack;
                    break;

                case "RCCCC":
                    offset = p1.Zone.In;
                    overlap = p1.Zone.Out + p1.Attack;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - p2.Attack;
                    cutoff = p2.Zone.Out;
                    break;

                // Ends with Rest

                case "VR":
                case "VCR":
                case "VCCR":
                case "VCCCR":
                    offset = p1.Zone.Out - p1.Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - p2.Attack;
                    cutoff = 0;
                    break;

                case "CR":
                case "CCR":
                case "CCCR":
                case "CCCCR":
                    offset = p1.Zone.In;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out;
                    cutoff = 0;
                    break;

                // Ends with Consonant
                case "VC":
                case "VCC":
                case "VCCC":
                case "VCCCC":
                case "CC":
                case "CCC":
                case "CCCC":
                case "CCCCC":
                    offset = p1.Zone.Out - p1.Attack;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - p2.Attack;
                    cutoff = p2.Zone.Out;
                    break;

                default:
                    return "";
            }
            MainWindow.Current.Reclist.Aliases.Add(alias);
            var oto = $"{filename}={WavControl.Prefix}{alias}{WavControl.Suffix},{Oto(offset, consonant, cutoff, preutterance, overlap)}\r\n";
            return oto;
        }
    }
}
