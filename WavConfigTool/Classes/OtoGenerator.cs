using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{
    public class OtoGenerator
    {

        public Dictionary<string, string> Replacement { get; private set; }
        public Reclist Reclist { get; set; }
        public Project Project { get; set; }
        public List<string> EmptyAliases { get; set; }
        
        public bool MustGeneratePreoto { get; set; } = true;

        public static OtoGenerator Current { get; private set; }

        public OtoGenerator(Reclist reclist, Project project)
        {
            Reclist = reclist;
            string path = Settings.GetResoucesPath(@"WavConfigTool\WavSettings\" + reclist.Name + ".txt");
            Replacement = ReadReplacementFile(path);
            Project = project;
            Current = this;
        }

        public Dictionary<string, string> ReadReplacementFile(string path)
        {
            if (!System.IO.File.Exists(path))
                return null;
            var replacement = new Dictionary<string, string>();
            foreach (string line in System.IO.File.ReadAllLines(path, Encoding.UTF8))
            {
                if (line.Contains("="))
                {
                    var pair = line.Split('=');
                    pair[0] = pair[0].Replace("%V%", $"({(string.Join("|", Reclist.Vowels.Select(n => n.Alias)))})");
                    pair[0] = pair[0].Replace("%C%", $"({(string.Join("|", Reclist.Consonants.Select(n => n.Alias)))})");
                    replacement[pair[0].Trim().Replace("\\s", " ")] = pair[1].Trim().Replace("\\s", " ");
                }
            }
            return replacement;
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

        public void Generate(ProjectLine projectLine)
        {
            var recline = projectLine.Recline;
            recline.ResetOto();
            projectLine.CalculateZones();
            var phs = recline.Phonemes;
            var position = 0;
            for (int i = 0; i < recline.Phonemes.Count; i++)
            {
                for (int count = 1; count < 6 && i + count - 1 < recline.Phonemes.Count; count++)
                {
                    var phonemes = recline.Phonemes.GetRange(i, count);
                    var prev = i - 1 >= 0 ? recline.Phonemes[i - 1] : null;
                    var next = i + count + 1 < recline.Phonemes.Count ? recline.Phonemes[i + count + 1] : null;
                    Generate(projectLine, position, phonemes.ToArray(), prev, next);
                }

                if (recline.Phonemes[i].Type != PhonemeType.Consonant)
                {
                    position++;
                }
            }
        }

        public void Generate(ProjectLine projectLine, int position, Phoneme[] phonemes, Phoneme prev, Phoneme next)
        {
            var recline = projectLine.Recline;
            string alias = GetAlias(phonemes);

            Phoneme p1 = phonemes.First();
            Phoneme p2 = phonemes.Last();
            double offset = 0, consonant = 0, cutoff = 0, preutterance = 0, overlap = 0;
            bool hasZones = projectLine.ApplyZones(p1) && projectLine.ApplyZones(p2);
            var aliasType = AliasTypeResolver.GetInstance().GetAliasType(GetAliasType(phonemes));
            var masked = aliasType != AliasType.undefined && Reclist.WavMask.CanGenerateOnPosition(projectLine.Recline.Filename, aliasType, position);

            if (!masked)
                return;

            bool hasAliasType = true;
            switch (aliasType)
            {
                // Absolute values, relative ones are made in Oto on write (!)
                case AliasType.V:
                    offset = p1.Zone.In + Project.VowelDecay - p1.Attack;
                    overlap = p1.Zone.In + Project.VowelDecay;
                    preutterance = overlap;
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
                    offset = p1.Zone.In - p1.Attack;
                    overlap = p1.Zone.In;
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
                case AliasType.CC:
                case AliasType.CmC:
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
                    ProcessOto(new Oto(recline.Filename, alias, offset, consonant, cutoff, preutterance, overlap), projectLine);
                }
                else if (MustGeneratePreoto)
                {
                    recline.AddOto(new Oto(recline.Filename, alias));
                }
            }
        }

        public Oto ProcessOto(Oto oto, ProjectLine line)
        {
            oto.Smarty();
            line.Recline.AddOto(oto);
            return oto;
        }
    }
}
