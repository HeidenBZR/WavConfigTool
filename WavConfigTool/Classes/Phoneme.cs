

using System.ComponentModel;

namespace WavConfigTool.Classes
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

    public abstract class Phoneme : INotifyPropertyChanged
    {
        public double Preutterance { get; set; }
        public double Overlap { get; set; }
        public double Length { get { return Zone.Out - Zone.In; } }
        public virtual double Attack { get; set; }

        public string Alias { get; set; }
        public string Letter { get; set; }
        public PhonemeType Type { get; set; }
        public Zone Zone { get; set; }
        public int GlobalIndex
        {
            get
            {
                return Recline == null ? -1 : Recline.Phonemes.IndexOf(this);
            }
        }
        public int LocalIndex
        {
            get
            {
                if (IsConsonant)
                    return Recline == null ? -1 : Recline.Consonants.IndexOf(this);
                else if (IsVowel)
                    return Recline == null ? -1 : Recline.Vowels.IndexOf(this);
                else
                    return Recline == null ? -1 : Recline.Rests.IndexOf(this);
            }
        }
        private bool hasZone;
        public bool HasZone { get => hasZone; set { hasZone = value; FireChanged(this); } }
        public Recline Recline;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsConsonant { get { return Type == PhonemeType.Consonant; } }
        public bool IsVowel { get { return Type == PhonemeType.Vowel; } }
        public bool IsRest { get { return Type == PhonemeType.Rest; } }

        public Phoneme(string l, string letter = "") { Alias = l; Letter = letter; Preutterance = 60; Overlap = 30; }


        public abstract Phoneme Clone();

        public override string ToString()
        {
            return Alias is null ? "/PH/" : Alias;
        }

        public static implicit operator string(Phoneme phoneme)
        {
            return phoneme.Alias;
        }

        public void FireChanged(object sender)
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("HasZone"));
        }
    };


    public class Consonant : Phoneme
    {
        public override double Attack { get => Project.Current.ConsonantAttack; }
        public Consonant(string l, string letter = "") : base(l, letter) { Type = PhonemeType.Consonant; }

        public override Phoneme Clone() { return new Consonant(Alias, Letter); }
    };


    public class Vowel : Phoneme
    {
        public override double Attack { get => Project.Current.VowelAttack; }
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
        public const string ALIAS = "-";
        public override double Attack { get => Project.Current.RestAttack; }

        public static Rest Create(Recline recline = null)
        {
            return new Rest(Rest.ALIAS) { Recline = recline };
        }
        private Rest(string l, string letter = "") : base(l, letter)
        {
            Type = PhonemeType.Rest;
            Attack = 0;
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
