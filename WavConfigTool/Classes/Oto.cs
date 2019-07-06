using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Oto() { }
        public Oto(string filename, string alias="", double offset=0, double consonant=0, double cutoff=0, double preuttercance=0, double overlap=0)
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
                double result;
                if (double.TryParse(data[1], out result))
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
            catch (Exception ex)
            {
                return null;
            }
        }

        public string Write(string prefix = "", string suffix = "")
        {
            // Relative values
            var offset = Offset;
            var overlap = Overlap - offset;
            var preutterance = Preutterance - offset;
            var consonant = Consonant - offset;
            var cutoff = Cutoff != 0 ? -(Cutoff - offset) : 10;
            return $"{Filename}={prefix}{Alias}{suffix},{offset},{consonant},{cutoff},{preutterance},{overlap}";
        }

        public void Smarty()
        {
            Cutoff = Cutoff <= Offset ? Offset + 30 : Cutoff;
            Consonant = Consonant < Offset ? Offset : Consonant;
            Consonant = Consonant > Cutoff ? Cutoff - 10 : Consonant;
            Overlap = Preutterance < Overlap ? Preutterance - 5 : Overlap;
            Offset = Overlap < Offset ? Overlap - 10 : Offset;
        }
    }
}
