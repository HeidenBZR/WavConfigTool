using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool
{
    public enum PhonemeType
    {
        Vowel,
        Consonant,
        Rest
    }

    public struct Zone
    {
        public double In;
        public double Out;
    }

    public abstract class Phoneme
    {
        public double Preutterance;
        public double Overlap;
        public double Length { get { return Zone.Out - Zone.In; } }
        public double Fade;

        public string Alias;
        public string Letter;
        public PhonemeType Type;
        public Zone Zone;
        public int GlobalIndex
        {
            get
            {
                return Recline.Phonemes.IndexOf(this);
            }
        }
        public int LocalIndex
        {
            get
            {
                if (IsConsonant)
                    return Recline.Consonants.IndexOf(this);
                else if (IsVowel)
                    return Recline.Vowels.IndexOf(this);
                else
                    return Recline.Data.IndexOf(this);
            }
        }
        public bool HasZone
        {
            get
            {
                return Zone.In != 0 && Zone.Out != 0;
            }
        }
        public Recline Recline;

        public bool IsConsonant { get { return Type == PhonemeType.Consonant; } }
        public bool IsVowel { get { return Type == PhonemeType.Vowel; } }
        public bool IsRest { get { return Type == PhonemeType.Rest; } }

        public Phoneme(string l, string letter = "") { Alias = l; Letter = letter; Preutterance = 60; Overlap = 30; }


        public abstract Phoneme Clone();

        // Override to build Alias
        public virtual string GetAlias() { return $"{WavControl.Prefix}{Alias}{WavControl.Suffix}"; }
        public virtual string GetAlias(Phoneme prev) { return $"{WavControl.Prefix}{prev.Alias}{Alias}{WavControl.Suffix}"; }
        public virtual string GetAlias(Phoneme prev, Phoneme preprev) { return $"{WavControl.Prefix}{preprev.Alias}{prev.Alias}{Alias}{WavControl.Suffix}"; }

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
            string alias = GetAlias();
            if (!NeedAlias()) return "";
            if (!HasZone) return "";
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double of = Zone.In + Fade;
            double con = Fade;
            double cut = -(Zone.Out - of - Fade) / 1.5;
            double pre = Preutterance;
            double ov = Overlap;
            con = -cut / 3;
            con = pre + Fade * 5;
            cut = -(Zone.Out - of - Fade * 5);
            if (cut > 0)
                cut = of - Zone.Out + Fade;
            if (con > -cut)
                con = -cut - 80;
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        public virtual string GetDiphone(string filename, Phoneme prev)
        {
            if (prev == null || !NeedAlias(prev) || !HasZone || !prev.HasZone) return "";
            if (IsConsonant && prev.IsRest) return ConsonantBeginning(filename, prev);
            if (IsRest && prev.IsConsonant) return ConsonantEnding(filename, prev);
            return NormalDiphone(filename, prev);

        }

        string NormalDiphone(string filename, Phoneme prev)
        {
            string alias = GetAlias(prev);
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double prevp = prev.Zone.Out - prev.Fade;
            if (prev.IsRest) prevp = prev.Zone.Out;
            double dist = Zone.In - prevp;

            double of = prevp;
            if (prev.IsVowel)
                of -= prev.Overlap;
            double con = prev.Overlap + dist + Fade;
            double cut = -(Zone.Out - of);
            if (IsVowel) cut = -(Zone.Out - of - Fade);
            double pre = Zone.In - of;
            double ov = prev.Zone.Out - of;
            if (IsConsonant)
            {
                pre = prev.Zone.Out - of;
                con = Zone.In - of;
                ov = prev.Overlap;
            }
            if (IsRest)
            {
                pre = prev.Zone.Out - of;
                con = pre + prev.Fade;
                ov = prev.Overlap;
                cut = -(Zone.Out - of + Fade);
                cut = 10;
            }
            if (IsVowel)
            {
                con = pre + Fade;
                cut = -(Zone.Out - of - Fade);
            }
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        string ConsonantBeginning(string filename, Phoneme prev)
        {
            string alias = GetAlias(prev);
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double prevp = prev.Zone.Out;
            double dist = Zone.In - prevp;

            double of = prevp;
            double cut = -(Zone.Out - of + Fade);
            double pre = Zone.Out - of;
            double ov = Zone.In - of;
            cut -= 10;
            double con = -cut - 10;
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        string ConsonantEnding(string filename, Phoneme prev)
        {
            string alias = GetAlias(prev);
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double prevp = prev.Zone.In;
            double dist = Zone.In - prevp;

            double of = prev.Zone.In;
            double con = Zone.Out - of;
            double cut = -(Zone.Out - of + Fade);
            double pre = prev.Zone.Out - of;
            double ov = prev.Overlap;
            if (ov > pre) ov = pre;
            cut = 10;
            string oto = Oto(of, con, cut, pre, ov);
            return $"{filename}={alias},{oto}\r\n";
        }

        public virtual string GetTriphone(string filename, Phoneme prev, Phoneme preprev)
        {
            if (prev == null || preprev == null || !NeedAlias(prev, preprev) || !HasZone || !prev.HasZone || !preprev.HasZone) return "";
            string alias = GetAlias(prev, preprev);
            if (Recline.Reclist.Aliases.Contains(alias)) return "";
            else Recline.Reclist.Aliases.Add(alias);

            double prevp = preprev.Zone.Out;
            double preprevoffset = prevp;
            double prevoffset = prev.Zone.In;
            double offset = Zone.In;
            double dist = offset - preprevoffset;

            double of = preprev.Zone.Out - preprev.Overlap;
            double con = Preutterance + dist + Fade;
            double cut = -(Zone.Out - of - Fade);
            double pre = dist + Preutterance;
            double ov = preprev.Overlap;
            if (preprev.IsVowel) // VCV, VC-
            {
                of = preprev.Zone.Out - preprev.Fade - preprev.Overlap;
            }
            if (preprev.IsVowel && IsRest) // VC-
            {
                ov = preprev.Zone.Out - 10;
                of = ov - preprev.Overlap;
                ov -= of;
                pre = pre + 50 + of < prev.Zone.Out? pre + 50: prev.Zone.Out - of;
                pre = prev.Zone.Out - 5 - of;
                con = Zone.Out - of;
                cut = -(Zone.In - of + Fade);
                cut = 10;
            }
            else if (preprev.IsVowel) // VCV
            {
                pre = Zone.In - of;
                con = pre + Fade;
            }
            else if (preprev.IsRest) // -CV
            {
                of = prevp;
                pre = Zone.In - of;
                con = pre + Fade;
                ov = prev.Zone.In - of;
                cut = -(Zone.Out - of - Fade);
            }
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
            Overlap = 50;
            Preutterance = 60;
        }
        // only V
        public override bool NeedAlias() { return IsVowel; }
        // only -CV && VCV && CCV
        public override bool NeedAlias(Phoneme prev, Phoneme preprev) { return prev.IsConsonant; }
        public override Phoneme Clone() { return new Vowel(Alias, Letter); }
    }


    public class Rest : Phoneme
    {
        public Rest(string l, string letter = "") : base(l, letter)
        {
            Type = PhonemeType.Rest;
            Fade = Settings.FadeD;
        }
        public Rest(string l, double position, Recline recline, string letter = "") : this(l, letter)
        {
            Zone.In = position;
            Zone.Out = position;
            Recline = recline;
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
                return $"{WavControl.Prefix}{prev.Alias}{Alias}{WavControl.Suffix}"; // v-
            else // c 
            {
                string alias = $"{WavControl.Prefix}{prev.Alias}{WavControl.Suffix}";
                if (alias == "r") return "rr";
                else return alias;
            }

        }
    };
}
