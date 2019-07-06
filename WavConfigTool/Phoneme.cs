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
        public double Attack;

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


        public static implicit operator string(Phoneme phoneme)
        {
            return phoneme.Alias;
        }
    };


    public class Consonant : Phoneme
    {
        public Consonant(string l, string letter = "") : base(l, letter)
        {
            Type = PhonemeType.Consonant;
            Attack = Settings.ConsonantAttack;
        }

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

        public override Phoneme Clone() { return new Vowel(Alias, Letter); }
    }


    public class Rest : Phoneme
    {
        public Rest(string l, string letter = "") : base(l, letter)
        {
            Type = PhonemeType.Rest;
            Attack = Settings.RestAttack;
        }
        public Rest(string l, double zoneIn, double zoneOut, Recline recline, string letter = "") : this(l, letter)
        {
            Zone.In = zoneIn;
            Zone.Out = zoneOut;
            Recline = recline;
        }

        public override Phoneme Clone() { return new Rest(Alias, Letter); }


    };
}
