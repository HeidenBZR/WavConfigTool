using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WavConfigTool
{
    public enum PhonemeType
    {
        Vowel,
        Consonant,
        Rest
    }

    public interface IPhoneme
    {
        string GetMonophone(string filename);
        string GetDiphone(string filename, Phoneme prev);
        string GetTriphone(string filename, Phoneme prev, Phoneme preprev);
    }

    public struct Zone
    {
        public WavMarker In;
        public WavMarker Out;
    }

    public abstract class Phoneme : IPhoneme
    {
        public double Preutterance = 60;
        public double Overlap = 30;
        public double Length { get { return (Zone.Out).Position - (Zone.In).Position; } }
        public double FadeIn = 10;
        public double FadeOut = 0;

        public string Alias;
        public string Letter;
        public PhonemeType Type;
        public Zone Zone;
        public bool HasZone { get { return Zone.In != null && Zone.Out != null; } }
        public Recline Recline;

        public bool IsConsonant { get { return Type == PhonemeType.Consonant; } }
        public bool IsVowel { get { return Type == PhonemeType.Vowel; } }
        public bool IsRest { get { return Type == PhonemeType.Rest; } }

        public Phoneme(string l, string letter = "") { Alias = l; Letter = letter; }


        public abstract Phoneme Clone();

        // Override to build Alias
        public virtual string GetAlias() { return Alias; }
        public virtual string GetAlias(Phoneme prev) { return $"{prev.Alias}{Alias}"; }
        public virtual string GetAlias(Phoneme prev, Phoneme preprev) { return $"{preprev.Alias}{prev.Alias}{Alias}"; }

        // Override to check if these phonemes must have alias
        public virtual bool NeedAlias() { return true; }
        public virtual bool NeedAlias(Phoneme prev) { return true; }
        public virtual bool NeedAlias(Phoneme prev, Phoneme preprev) { return true; }

        public string Oto(double of, double con, double cut, double pre, double ov)
        {
            string f = "f0";
            return $"{of.ToString(f)},{con.ToString(f)},{cut.ToString(f)},{pre.ToString(f)},{ov.ToString(f)}";
        }

        public virtual string GetMonophone(string filename)
        {
            if (!NeedAlias() || !HasZone) return "";
            string alias = GetAlias();
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double of = Zone.In.Position + FadeIn;
            double con = FadeIn;
            double cut = -Length + FadeOut + FadeIn;
            double pre = Preutterance;
            double ov = Overlap;
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        public virtual string GetDiphone(string filename, Phoneme prev)
        {
            if (prev == null || !NeedAlias(prev) || !HasZone || !prev.HasZone) return "";
            if (IsConsonant && prev.IsRest) return ConsonantDiphone(filename, prev);
            if (IsRest && prev.IsConsonant) return ConsonantEnding(filename, prev);
            return NormalDiphone(filename, prev);

        }

        string NormalDiphone(string filename, Phoneme prev)
        {
            string alias = GetAlias(prev);
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double prevp = prev.Zone.Out.Position - prev.FadeOut;
            double dist = Zone.In.Position - prevp;

            double of = prevp - prev.Overlap;
            double con = prev.Overlap + dist + FadeIn;
            double cut = -(Zone.Out.Position - of - FadeOut);
            double pre = IsConsonant && prev.IsVowel ? prevp - of + prev.FadeOut : Zone.In.Position - of;
            double ov = prev.Overlap;
            if (IsRest) cut -= 50;
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        string ConsonantDiphone(string filename, Phoneme prev)
        {
            string alias = GetAlias(prev);
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double prevp = prev.Zone.Out.Position - prev.FadeOut;
            double dist = Zone.In.Position - prevp;

            double of = prevp - prev.Overlap;
            double con = Zone.Out.Position - of + FadeIn;
            double cut = -(Zone.Out.Position - of - FadeOut);
            double pre = Zone.Out.Position - of;
            double ov = prev.Overlap;
            cut -= 50;
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        string ConsonantEnding(string filename, Phoneme prev)
        {
            string alias = GetAlias(prev);
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double prevp = prev.Zone.Out.Position - prev.FadeOut;
            double dist = Zone.In.Position - prevp;

            double of = prevp - prev.Overlap;
            double con = Zone.Out.Position - of + FadeIn;
            double cut = -(Zone.Out.Position - of - FadeOut);
            double pre = prev.Zone.Out.Position - of + Overlap;
            double ov = prev.Overlap;
            cut -= 50;
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        public virtual string GetTriphone(string filename, Phoneme prev, Phoneme preprev)
        {
            if (prev == null || preprev == null || !NeedAlias(prev, preprev) || !HasZone || !prev.HasZone || !preprev.HasZone) return "";
            string alias = GetAlias(prev, preprev);
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double prevp = preprev.Zone.Out.Position;
            double preprevoffset = prevp;
            double prevoffset = prev.Zone.In.Position;
            double offset = Zone.In.Position;
            double dist = offset - preprevoffset;

            double of = preprevoffset - Preutterance;
            double con = Preutterance + dist + FadeIn;
            double cut = -(Zone.Out.Position - of - FadeOut);
            double pre = dist + Preutterance;
            double ov = Overlap;
            if (IsRest) pre = of + Preutterance;
            if (IsRest) cut -= 50;
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        public static implicit operator string(Phoneme phoneme)
        {
            return phoneme.Alias;
        }
    };


    public class Consonant : Phoneme
    {
        public Consonant(string l, string letter = "") : base(l, letter) { Type = PhonemeType.Consonant; }
        public override bool NeedAlias() { return false; }
        public override bool NeedAlias(Phoneme prev, Phoneme preprev) { return false; }
        public override Phoneme Clone() { return new Consonant(Alias, Letter); }
    };


    public class Vowel : Phoneme
    {
        public Vowel(string l, string letter = "") : base(l, letter)
        {
            Type = PhonemeType.Vowel;
            FadeIn = 150;
            FadeOut = 150;
            Overlap = 50;
            Preutterance = 60;
        }
        // only V
        public override bool NeedAlias() { return IsVowel; }
        // only CV & -V
        public override bool NeedAlias(Phoneme prev) { return prev.IsConsonant || prev.IsRest; }
        // only -CV
        public override bool NeedAlias(Phoneme prev, Phoneme preprev) { return prev.IsConsonant && preprev.IsRest; }
        public override Phoneme Clone() { return new Vowel(Alias, Letter); }
    }


    public class Rest : Phoneme
    {
        public Rest(string l, string letter = "") : base(l, letter)
        {
            Type = PhonemeType.Rest;
            FadeIn = 0;
            FadeOut = 0;
        }
        public override bool NeedAlias() { return false; }
        // only V- & C
        public override bool NeedAlias(Phoneme prev) { return prev.IsVowel || prev.IsConsonant; }
        // only VC-
        public override bool NeedAlias(Phoneme prev, Phoneme preprev) { return prev.IsConsonant && preprev.IsVowel; }

        public override Phoneme Clone() { return new Rest(Alias, Letter); }

        public override string GetAlias(Phoneme prev)
        {
            if (prev.IsVowel)
                return $"{prev.Alias}{Alias}"; // v-
            else
                return $"{prev.Alias}"; // c
        }
    };

    public class Reclist
    {
        public static List<Phoneme> Phonemes;
        public List<Recline> Reclines;
        public string VoicebankPath;
        public List<Phoneme> Vowels { get { return Phonemes.Where(n => n.IsVowel).ToList(); } }
        public List<Phoneme> Consonants { get { return Phonemes.Where(n => n.IsConsonant).ToList(); } }
        public List<string> Aliases;

        public Reclist(string[] vs, string[] cs)
        {
            Phonemes = new List<Phoneme>();
            foreach (string v in vs) Phonemes.Add(new Vowel(v));
            foreach (string c in cs) Phonemes.Add(new Consonant(c));
            Reclines = new List<Recline>();
        }
    }

    public class Recline
    {
        public string Filename;
        public string Description;
        public List<Phoneme> Phonemes;
        public List<Phoneme> Vowels { get { return Phonemes.Where(n => n.IsVowel).ToList(); } }
        public List<Phoneme> Consonants { get { return Phonemes.Where(n => n.IsConsonant).ToList(); } }
        public Reclist Reclist;
        public string Path { get { return System.IO.Path.Combine(Reclist.VoicebankPath, Filename); } }

        public Recline(string filename, string phonemes)
        {
            Filename = filename;
            Description = phonemes;
            Phonemes = new List<Phoneme>();
            foreach (string ph in phonemes.Split(' '))
            {
                Phoneme phoneme = Reclist.Phonemes.Find(n => n.Alias == ph).Clone();
                phoneme.Recline = this;
                Phonemes.Add(phoneme);
            }
        }

        public Recline(string filename, string phonemes, string description) : this(filename, phonemes)
        {
            Description = description;
        }
    }
}
