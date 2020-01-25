
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

        public Zone(double p_in, double p_out)
        {
            In = p_in;
            Out = p_out;
        }

        public override string ToString()
        {
            return $"{In}:{Out}";
        }
    }

    public abstract class Phoneme
    {
        public double Length => Zone.Out - Zone.In;

        public abstract double Attack { get; }
        public abstract int LocalIndex { get; }

        public string Alias { get; set; }
        public string Letter { get; set; }
        public PhonemeType Type { get; set; }
        public Zone Zone { get; set; }
        public int GlobalIndex => Recline == null ? -1 : Recline.Phonemes.IndexOf(this);
        private bool hasZone;
        public bool HasZone { get => hasZone; set { hasZone = value; FireChanged(); } }
        public Recline Recline;

        public bool IsConsonant { get { return Type == PhonemeType.Consonant; } }
        public bool IsVowel { get { return Type == PhonemeType.Vowel; } }
        public bool IsRest { get { return Type == PhonemeType.Rest; } }

        public Phoneme(string l, string letter = "") 
        { 
            Alias = l; 
            Letter = letter; 
        }


        public abstract Phoneme Clone();

        public override string ToString()
        {
            return Alias is null ? "/PH/" : Alias;
        }

        public static implicit operator string(Phoneme phoneme)
        {
            return phoneme.Alias;
        }

        public void FireChanged()
        {
            // TODO: Make phoneme view model?
        }
    };


    public class Consonant : Phoneme
    {
        public override double Attack { get => Project.Current.ConsonantAttack; }
        public override int LocalIndex => Recline == null ? -1 : Recline.Consonants.IndexOf(this);
        public Consonant(string l, string letter = "") : base(l, letter) { Type = PhonemeType.Consonant; }
        public override Phoneme Clone() { return new Consonant(Alias, Letter); }
    };
    

    public class Vowel : Phoneme
    {
        public override double Attack => Project.Current.VowelAttack;
        public override int LocalIndex => Recline == null ? -1 : Recline.Vowels.IndexOf(this);
        public Vowel(string l, string letter = "") : base(l, letter) { Type = PhonemeType.Vowel; }
        public override Phoneme Clone() { return new Vowel(Alias, Letter); }
    }
    
    public class Rest : Phoneme
    {
        public const string ALIAS = "-";
        public override double Attack => Project.Current.RestAttack;
        public override int LocalIndex => Recline == null ? -1 : Recline.Rests.IndexOf(this);

        public static Rest Create(Recline recline = null)
        {
            return new Rest(Rest.ALIAS) { Recline = recline };
        }
        private Rest(string l, string letter = "") : base(l, letter)
        {
            Type = PhonemeType.Rest;
        }
        public Rest(string l, double zoneIn, double zoneOut, Recline recline, string letter = "") : this(l, letter)
        {
            Zone = new Zone()
            {
                In = zoneIn,
                Out = zoneOut
            };
            Recline = recline;
        }

        public override Phoneme Clone() { return new Rest(Alias, Letter); }


    };
}
