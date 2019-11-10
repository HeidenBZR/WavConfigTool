using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes.Reader
{
    class ReplacerReader
    {
        private static ReplacerReader current;
        private ReplacerReader() { }

        public static ReplacerReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new ReplacerReader();
                }
                return current;
            }
        }

        public Replacer Read(string name, Reclist reclist)
        {
            var replacer = new Replacer();
            var filename = PathResolver.Replacer(reclist.Name, name);
            if (!System.IO.File.Exists(filename))
                return replacer;
            foreach (string line in System.IO.File.ReadAllLines(filename, Encoding.UTF8))
            {
                if (line.Contains("="))
                {
                    var pair = line.Split('=');
                    var key = pair[0].Trim().Replace("\\s", " ");
                    var value = pair[1].Trim().Replace("\\s", " ");
                    if (key == "")
                        continue;
                    if (value.Contains("$"))
                    {
                        if (value != "")
                        {
                            var aliasType = AliasTypeResolver.Current.GetAliasType(key);
                            if (AliasTypeResolver.Current.IsFormatValid(aliasType, value))
                            {
                                replacer.SetFormat(aliasType, value);
                            }
                        }
                    }
                    else
                    {
                        key = key.Replace("%V%", $"({(string.Join("|", reclist.Vowels.Select(n => n.Alias)))})");
                        key = key.Replace("%C%", $"({(string.Join("|", reclist.Consonants.Select(n => n.Alias)))})");
                        replacer.SetReplacement(key, value);
                    }
                }
            }
            return replacer;
        }

        public void Write(Replacer replacer, Reclist reclist)
        {
            var filename = PathResolver.Replacer(reclist.Name, replacer.Name);
            var text = new StringBuilder();
            foreach (var format in replacer.GetFormats())
            {
                if (format.Key == AliasType.undefined)
                    continue;
                text.AppendLine($"{format.Key} = {(format.Value == " " ? "\\s" : format.Value)}");
            }

            foreach (var replacement in replacer.GetReplacements())
            {
                text.AppendLine($"{replacement.Key} = {(replacement.Value == " " ? "\\s" : replacement.Value)}");
            }
            File.WriteAllText(filename, text.ToString());
        }

    }
}
