using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes
{

    public class WavImagesHash
    {
        public string Reclist;
        public string Voicebank;
        public double UserScaleX;
        public double UserScaleY;
        public string Recline;
        public string Suffix;
        public string Prefix;

        public WavImagesHash() { }

        public WavImagesHash(string reclist, string voicebank, double scaleX, double scaleY, string recline, string suffix, string prefix)
        {
            Reclist = reclist;
            Voicebank = voicebank;
            UserScaleX = scaleX;
            UserScaleY = scaleY;
            Recline = recline;
            Suffix = suffix;
            Prefix = prefix;
        }

        public bool Equals(WavImagesHash other)
        {
            if (other == null)
                return false;
            if (this.Reclist != other.Reclist)
                return false;
            if (this.Voicebank != other.Voicebank)
                return false;
            if (this.UserScaleX != other.UserScaleX)
                return false;
            if (this.UserScaleY != other.UserScaleY)
                return false;
            if (this.Recline != other.Recline)
                return false;
            if (this.Suffix != other.Suffix)
                return false;
            if (this.Prefix != other.Prefix)
                return false;

            return true;
        }
    }
}
