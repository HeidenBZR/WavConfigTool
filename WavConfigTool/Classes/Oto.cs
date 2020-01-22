using System;

namespace WavConfigTool.Classes
{
    public class Oto
    {
        public string Filename { get; set; } = "";
        public string Alias { get; set; } = "";
        public double Offset { get; set; }
        public double Consonant { get; set; }
        public double Cutoff { get; set; }
        public double Preutterance { get; set; }
        public double Overlap { get; set; }
        public int Number { get; set; }
        public string NumberView => Number == 0 ? "" : (Number + 1).ToString();

        public Oto() { } 
        public Oto(string filename, string alias = "", double offset = 0, double consonant = 0, double cutoff = 0, double preuttercance = 0, double overlap = 0)
        {
            Filename = filename;
            Alias = alias;
            Offset = offset;
            Consonant = consonant;
            Cutoff = cutoff;
            Preutterance = preuttercance;
            Overlap = overlap;
        }
        public static Oto Read(string line, double length)
        {
            try
            {
                var data = line.Split('=');
                string filename = data[0];
                string alias = "";
                double offset = 0;
                double consonant = 0;
                double cutoff = 0;
                double preuttercance = 0;
                double overlap = 0;
                data = data[1].Split(',');
                alias = data[0];
                if (double.TryParse(data[1], out double result))
                {
                    offset = result;
                }
                if (double.TryParse(data[2], out result))
                {
                    consonant = result;
                }
                if (double.TryParse(data[3], out result))
                {
                    cutoff = result;
                }
                if (double.TryParse(data[4], out result))
                {
                    preuttercance = result;
                }
                if (double.TryParse(data[5], out result))
                {
                    overlap = result;
                }
                return new Oto(
                    filename,
                    alias,
                    offset,
                    offset + consonant,
                    cutoff > 0 ? length - cutoff : offset - cutoff,
                    offset + preuttercance,
                    offset + overlap
                );
            }
            catch (Exception)
            {
                return null;
            }
        }

        public double OffsetWrite => Offset;
        public double ConsonantWrite => Consonant - Offset;
        public double CutoffWrite => Cutoff != 0 ? -(Cutoff - Offset) : 10;
        public double PreutteranceWrite => Preutterance - Offset;
        public double OverlapWrite => Overlap - Offset;

        public string Write(string prefix = "", string suffix = "", string wavPrefix = "", string wavSuffix = "")
        {
            // Relative values
            return $"{wavPrefix}{Filename}{wavSuffix}.wav={prefix}{Alias}{NumberView}{suffix},{OffsetWrite},{ConsonantWrite},{CutoffWrite},{PreutteranceWrite},{OverlapWrite}";
        }

        public void Smarty()
        {
            Cutoff = Cutoff != 0 && Cutoff <= Offset ? Offset + 30 : Cutoff;
            Consonant = Consonant < Preutterance ? Preutterance : Consonant;
            Consonant = Cutoff != 0 && Consonant > Cutoff ? Cutoff - 10 : Consonant;
            Overlap = Preutterance < Overlap ? Preutterance - 5 : Overlap;
            Offset = Overlap < Offset ? Overlap - 10 : Offset;
        }
    }
}
