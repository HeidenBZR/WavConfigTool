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

        Dictionary<string, string> LineByAlias { get; set; }
        Dictionary<string, bool> HasOtoByAlias { get; set; }
        public List<string> Aliases { get; set; }

        public bool MustGeneratePreoto { get; set; } = true;

        public static OtoGenerator Current { get; private set; }

        public OtoGenerator(Reclist reclist)
        {
            Reclist = reclist;
            string path = Settings.GetResoucesPath(@"WavConfigTool\WavSettings\" + reclist.Name + ".txt");
            Replacement = ReadReplacementFile(path);
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

        public string Generate(Recline recline, ProjectLine projectLine)
        {
            projectLine.CalculateZones();
            var text = new StringBuilder();
            LineByAlias = new Dictionary<string, string>();
            HasOtoByAlias = new Dictionary<string, bool>();
            Aliases = new List<string>();
            var phs = recline.Phonemes;
            for (int i = 0; i < recline.Phonemes.Count; i++)
            {
                if (phs.Count > i + 1)
                    AddLine(Generate(projectLine, phs[i], phs[i + 1]));
                if (phs.Count > i + 2)
                    AddLine(Generate(projectLine, phs[i], phs[i + 1], phs[i + 2]));
                if (phs.Count > i + 3)
                    AddLine(Generate(projectLine, phs[i], phs[i + 1], phs[i + 2], phs[i + 3]));
                if (phs.Count > i + 4)
                    AddLine(Generate(projectLine, phs[i], phs[i + 1], phs[i + 2], phs[i + 3], phs[i + 4]));
            }
            foreach (var alias in Aliases)
            {
                if (MustGeneratePreoto || HasOtoByAlias[alias])
                    text.AppendLine(LineByAlias[alias]);
            }
            return text.ToString();
        }

        public void AddLine((bool hasOto, string alias, string line) oto)
        {
            /// Если раньше не приходили такие алиасы, то сохраняем строку в любом случае
            /// Если приходили, то смотрим, была прежняя и текущая строка полноценной или преото, 
            /// если прежняя полноценнная то она остается, если обе преото тоже,
            /// если первая преото и эта полноценная -- заменяется.
            /// пустой алиас -- если неадекватный алиас тип вроде VRVC, такие пропускаем сразу
            if (oto.alias == "")
                return;
            if (!Aliases.Contains(oto.alias))
                Aliases.Add(oto.alias);
            if (!HasOtoByAlias.ContainsKey(oto.alias))
            {
                HasOtoByAlias[oto.alias] = oto.hasOto;
            }
            if (!LineByAlias.ContainsKey(oto.alias))
            {
                LineByAlias[oto.alias] = oto.line;
                HasOtoByAlias[oto.alias] = oto.hasOto;
                return;
            }
            else
            {
                if (!HasOtoByAlias[oto.alias] && oto.hasOto)
                {
                    LineByAlias[oto.alias] = oto.line;
                    HasOtoByAlias[oto.alias] = oto.hasOto;
                }
            }
        }

        public (bool hasOto, string alias, string line) Generate(ProjectLine projectLine, params Phoneme[] phonemes)
        {
            string alias = GetAlias(phonemes);

            Phoneme p1 = phonemes.First();
            Phoneme p2 = phonemes.Last();
            double offset = 0, consonant = 0, cutoff = 0, preutterance = 0, overlap = 0;
            bool hasZones = projectLine.ApplyZones(p1) && projectLine.ApplyZones(p2);
            var aliasType = GetAliasType(phonemes);

            bool hasAliasType = true;
            switch (aliasType)
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
                    consonant = p2.Zone.Out - Project.VowelSustain - p2.Attack;
                    cutoff = p2.Zone.Out - p2.Attack;
                    break;

                case "RCV":
                case "RCCV":
                case "RCCCV":
                    offset = p1.Zone.In;
                    overlap = p1.Zone.Out + p1.Attack;
                    preutterance = p2.Zone.In;
                    consonant = p2.Zone.Out - Project.VowelSustain - p2.Attack;
                    cutoff = p2.Zone.Out - p2.Attack;
                    break;

                case "RC":
                case "RCC":
                case "RCCC":
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
                    hasAliasType = false;
                    break;
            }
            var otoParams = hasZones ? Oto(offset, consonant, cutoff, preutterance, overlap) : Oto(0, 0, 0, 0, 0);
            alias = hasAliasType ? alias : "";
            string oto = $"{projectLine.Recline.Filename}={Project.Prefix}{alias}{Project.Suffix},{otoParams}";
            return (hasZones && hasAliasType, alias, oto);
        }
    }
}
