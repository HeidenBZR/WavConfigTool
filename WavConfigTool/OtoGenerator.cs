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
        public bool IsVcSeparated { get; private set; }
        public bool IsCvSeparated { get; private set; }

        public Dictionary<string, string> Replacement { get; private set; }

        public static OtoGenerator Current { get; private set; }

        private OtoGenerator(string voicebankType)
        {
            string path = Project.GetTempPath(@"WavConfigTool\WavSettings\" + voicebankType + ".txt");
            Replacement = ReadReplacementFile(path);
        }

        public static void Init(string vocebankType, bool isVcSeparated = false, bool isCvSeparated = false)
        {
            Current = new OtoGenerator(vocebankType)
            {
                IsVcSeparated = isVcSeparated,
                IsCvSeparated = isCvSeparated
            };
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
                        replacement[pair[0].Trim()] = pair[1].Trim();
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
            cut = -(cut - of);

            return $"{of.ToString(f)},{con.ToString(f)},{cut.ToString(f)},{pre.ToString(f)},{ov.ToString(f)}";
        }

        public string Replace(string alias)
        {
            if (Replacement is null)
                return alias;
            foreach (KeyValuePair<string, string> entry in Replacement)
                alias = Regex.Replace(alias.ToString(), entry.Key, entry.Value);
            return alias;
        }

        public string GetAlias(params Phoneme[] phonemes)
        {
            var alias = new StringBuilder(phonemes[0].Alias);
            for (int i = 1; i < phonemes.Length; i++)
            {
                if (IsCvSeparated && GetAliasType(phonemes[i - 1], phonemes[i]) == "CV")
                    alias.Append(" ");
                else if (IsVcSeparated)
                    alias.Append(" ");
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
            Phoneme p1 = phonemes.Length > 0 ? phonemes[0] : null;
            Phoneme p2 = phonemes.Length > 1 ? phonemes[1] : null;
            Phoneme p3 = phonemes.Length > 2 ? phonemes[2] : null;

            switch (GetAliasType(phonemes))
            {
                // Absolute values, relative ones are made in Oto()
                case "CV":
                case "VC":
                case "CC":
                case "VV":
                case "RV":
                case "RC":
                case "VR":
                case "CR":
                    offset = p1.Zone.Out - p1.Overlap;
                    overlap = p1.Zone.Out;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - p2.Release;
                    cutoff = p2.Zone.Out;
                    break;

                case "VCV":
                case "CCV":
                case "RCV":
                case "VCR":
                    offset = p1.Zone.Out - p1.Overlap;
                    overlap = p1.Zone.Out;
                    preutterance = p3.Zone.In;
                    consonant = p3.Zone.Out - p3.Release;
                    cutoff = p3.Zone.Out;
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
