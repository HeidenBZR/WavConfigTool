using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WavConfigTool
{
    public interface IPhoneme
    {
        string GetMonophone(string filename);
        string GetDiphone(string filename, Phoneme prev);
        string GetTriphone(string filename, Phoneme prev, Phoneme preprev);
    }
    public interface IPhonemeZone : IPhoneme { }
    public interface IPhonemeSingletone : IPhoneme { }

    public abstract class Phoneme : IPhoneme
    {
        public double Preutterance = 60;
        public double Overlap = 30;
        public double Length { get { return IsZoned ? (MarkerOut as WavMarker).Msec - (Marker as WavMarker).Msec : 90; } }
        public double FadeIn = 10;
        public double FadeOut = 0;

        public string Alias;
        public string Letter;
        public bool IsVowel = false;
        public bool IsConsonant = false;
        public bool IsFricative = false;
        public bool IsOcclusive { get { return !IsFricative && IsConsonant; } }
        public bool IsRest = false;
        public bool IsZoned = true;
        public UserControl Marker;
        public UserControl MarkerOut;

        public Phoneme(string l) { Alias = l; }

        public static Phoneme Copy(Phoneme phoneme)
        {
            if (phoneme.IsRest) return new Rest(phoneme.Alias);
            if (phoneme.IsVowel) return new Vowel(phoneme.Alias);
            if (phoneme.IsOcclusive) return new Occlusive(phoneme.Alias);
            if (phoneme.IsFricative) return new Frivative(phoneme.Alias);
            else throw new Exception("Unknown phoneme type");
        }

        // Override to build Alias
        public virtual string GetAlias() { return Alias; }
        public virtual string GetAlias(Phoneme prev) { return $"{prev.Alias}{Alias}"; }
        public virtual string GetAlias(Phoneme prev, Phoneme preprev) { return $"{preprev.Alias} {prev.Alias} {Alias}"; }

        // Override to check if these phonemes must have alias
        public virtual bool NeedAlias() { return true; }
        public virtual bool NeedAlias(Phoneme prev) { return true; }
        public virtual bool NeedAlias(Phoneme prev, Phoneme preprev) { return true; }

        public virtual string GetMonophone(string filename)
        {
            if (!NeedAlias()) return "";
            if (Marker is null) return "";
            double offset = (Marker as WavMarker).Msec;
            string oto = $"{GetAlias()},{offset - Preutterance},{Preutterance + FadeIn},{offset + Length},{Preutterance},{Overlap}";
            return $"{filename}={oto}\r\n";
        }
        public virtual string GetDiphone(string filename, Phoneme prev)
        {
            if (prev == null) return "";
            if (!NeedAlias(prev)) return "";
            if (Marker is null || prev.Marker is null) return "";
            double prevoffset = (prev.Marker as WavMarker).Msec;
            double offset = (Marker as WavMarker).Msec;
            double dist = offset - prevoffset;
            string oto = $"{GetAlias(prev)},{prevoffset - Preutterance},{Preutterance + dist + FadeIn}," +
                $"{prevoffset + Length},{dist + Preutterance},{Overlap}";
            return $"{filename}={oto}\r\n";
        }
        public virtual string GetTriphone(string filename, Phoneme prev, Phoneme preprev)
        {
            if (prev == null || preprev == null) return "";
            if (!NeedAlias(prev, preprev)) return "";
            if (Marker is null || prev.Marker is null || preprev.Marker is null) return "";
            double preprevoffset = (preprev.Marker as WavMarker).Msec;
            double prevoffset = (prev.Marker as WavMarker).Msec;
            double offset = (Marker as WavMarker).Msec;
            double dist = offset - preprevoffset;
            string oto = $"{GetAlias(prev, preprev)},{preprevoffset - Preutterance}," +
                $"{Preutterance + dist + FadeIn},{preprevoffset + Length},{dist + Preutterance},{Overlap}";
            return $"{filename}={oto}\r\n";
        }
    };
    public abstract class Consonant : Phoneme
    {
        public Consonant(string l) : base(l) { IsConsonant = true; }
        public override bool NeedAlias() { return false; }
        public override bool NeedAlias(Phoneme prev, Phoneme preprev) { return false; }
    };
    public class Frivative : Consonant, IPhonemeZone
    {
        public Frivative(string l) : base(l) { IsFricative = true; }
        public Frivative Copy() { return (Frivative)MemberwiseClone(); }
    };
    public class Occlusive : Consonant, IPhonemeSingletone
    {
        public Occlusive(string l) : base(l) { IsZoned = false; }
        public Occlusive Copy() { return (Occlusive)MemberwiseClone(); }
    };
    public class Vowel : Phoneme, IPhonemeZone
    {
        public double Length = 200;
        public double FadeIn = 60;
        public double FadeOut = 30;
        public Vowel(string l) : base(l) { IsVowel = true; }
        public Vowel Copy() { return (Vowel)MemberwiseClone(); }
        // only CV & -V
        public override bool NeedAlias(Phoneme prev) { return prev.IsConsonant || prev.IsRest; }
        // only -CV
        public override bool NeedAlias(Phoneme prev, Phoneme preprev) { return prev.IsConsonant && preprev.IsRest; }
    }
    public class Rest : Phoneme, IPhonemeSingletone
    {
        public Rest(string l) : base(l) { IsRest = true; IsZoned = false; }
        public Rest Copy() { return (Rest)MemberwiseClone(); }
        public override bool NeedAlias() { return false; }
        // only V- & C
        public override bool NeedAlias(Phoneme prev) { return prev.IsVowel || prev.IsConsonant; }
        // only VC-
        public override bool NeedAlias(Phoneme prev, Phoneme preprev) { return prev.IsConsonant && preprev.IsVowel; }

        public override string GetAlias(Phoneme prev)
        {
            if (prev.IsVowel)
                return $"{prev.Alias}{Alias}"; // v-
            else
                return $"{Alias}"; // c
        }
        public override string GetAlias(Phoneme prev, Phoneme preprev)
        {
            return $"{preprev}{prev.Alias}{Alias}"; // vc-
        }
    };

    public class Reclist
    {
        public static List<Phoneme> Phonemes;
        public List<Recline> Reclines;

        public Reclist()
        {
            Phonemes = new List<Phoneme>
            {
                new Rest("-"),
                new Vowel("a"),
                new Vowel("e"),
                new Vowel("i"),
                new Vowel("o"),
                new Vowel("u"),
                new Vowel("y"),
                new Occlusive("`"),
                new Occlusive("t"),
                new Occlusive("t'"),
                new Occlusive("d"),
                new Occlusive("d'"),
                new Occlusive("p"),
                new Occlusive("p'"),
                new Occlusive("b"),
                new Occlusive("b'"),
                new Occlusive("k"),
                new Occlusive("k'"),
                new Occlusive("g"),
                new Occlusive("g'"),
                new Frivative("s"),
                new Frivative("s'"),
                new Frivative("z"),
                new Frivative("z'"),
                new Frivative("f"),
                new Frivative("f'"),
                new Frivative("v"),
                new Frivative("v'"),
                new Frivative("w"),
                new Frivative("w'"),
                new Frivative("j"),
                new Frivative("j'"),
                new Frivative("h"),
                new Frivative("h'"),
                new Frivative("l"),
                new Frivative("l'"),
                new Frivative("m"),
                new Frivative("m'"),
                new Frivative("n"),
                new Frivative("n'"),
                new Frivative("~"),
                new Occlusive("4'"),
                new Occlusive("c")
            };
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
        public List<Phoneme> Fricatives { get { return Phonemes.Where(n => n.IsFricative).ToList(); } }
        public List<Phoneme> Occlusives { get { return Phonemes.Where(n => n.IsOcclusive).ToList(); } }

        public Recline(string filename, string phonemes)
        {
            Filename = filename;
            Description = phonemes;
            Phonemes = new List<Phoneme>();
            foreach (string ph in phonemes.Split(' '))
            {
                Phoneme phoneme = Reclist.Phonemes.Find(n => n.Alias == ph);
                Phonemes.Add(Phoneme.Copy(phoneme));
            }
        }

        public Recline(string filename, string phonemes, string description) : this(filename, phonemes)
        {
            Description = description;
        }
    }
}
