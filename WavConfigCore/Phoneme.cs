
namespace WavConfigCore
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

        public Zone(double pIn, double pOut)
        {
            In = pIn;
            Out = pOut;
        }

#if TOSTRING
        public override string ToString()
        {
            return $"{In}:{Out}";
        }
#endif
    }

    public abstract class Phoneme
    {
        public double Length => Zone.Out - Zone.In;

        public abstract PhonemeType PhonemeType { get; }
        public abstract int LocalIndex { get; }

        public string Alias { get; set; } = "/PH/";
        public string Letter { get; set; }
        public PhonemeType Type { get; set; }
        public Zone Zone { get; set; }
        public int GlobalIndex => Recline == null ? -1 : Recline.Phonemes.IndexOf(this);
        public bool HasZone { get; set; }
        public bool IsSkipped { get; set; }
        public Recline Recline;

        public bool IsLast { get; set; }
        public bool IsFirst { get; set; }

        public bool IsConsonant { get { return Type == PhonemeType.Consonant; } }
        public bool IsVowel { get { return Type == PhonemeType.Vowel; } }
        public bool IsRest { get { return Type == PhonemeType.Rest; } }

        public Phoneme(string l, string letter = "") 
        { 
            Alias = l; 
            Letter = letter;
        }

        public abstract Phoneme Clone();

#if TOSTRING
        public override string ToString()
        {
            return Alias is null ? "/PH/" : Alias;
        }

        public static implicit operator string(Phoneme phoneme)
        {
            return phoneme.Alias;
        }
#endif
    };

    public class Consonant : Phoneme
    {
        public override PhonemeType PhonemeType => PhonemeType.Consonant;
        public override int LocalIndex => Recline == null ? -1 : Recline.Consonants.IndexOf(this);
        public Consonant(string l, string letter = "") : base(l, letter) { Type = PhonemeType.Consonant; }
        public override Phoneme Clone() { return new Consonant(Alias, Letter); }
    };

    public class Vowel : Phoneme
    {
        public override PhonemeType PhonemeType => PhonemeType.Vowel;
        public override int LocalIndex => Recline == null ? -1 : Recline.Vowels.IndexOf(this);
        public Vowel(string l, string letter = "") : base(l, letter) { Type = PhonemeType.Vowel; }
        public override Phoneme Clone() { return new Vowel(Alias, Letter); }
    }
    
    public class Rest : Phoneme
    {
        public const string ALIAS = "-";
        public override PhonemeType PhonemeType => PhonemeType.Rest;
        public override int LocalIndex => Recline == null ? -1 : Recline.Rests.IndexOf(this);

        public static Rest Create(Recline recline = null)
        {
            return new Rest(Rest.ALIAS) { Recline = recline };
        }

        private Rest(string l, string letter = "") : base(l, letter)
        {
            Type = PhonemeType.Rest;
        }

        public override Phoneme Clone() { return new Rest(Alias, Letter); }

    };
}
