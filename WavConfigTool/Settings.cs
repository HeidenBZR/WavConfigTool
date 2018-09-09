using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool
{
    public static class Settings
    {
        public static int ItemsOnPage
        {
            get
            {
                return Properties.Settings.Default.ItemsOnPage;
            }
            set
            {
                Properties.Settings.Default.ItemsOnPage = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int CurrentPage
        {
            get
            {
                return Properties.Settings.Default.CurrentPage;
            }
            set
            {
                Properties.Settings.Default.CurrentPage = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int FadeV
        {
            get
            {
                return Properties.Settings.Default.FadeV;
            }
            set
            {
                Properties.Settings.Default.FadeV = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int FadeC
        {
            get
            {
                return Properties.Settings.Default.FadeC;
            }
            set
            {
                Properties.Settings.Default.FadeC = value;
                Properties.Settings.Default.Save();
            }
        }

        public static int FadeD
        {
            get
            {
                return Properties.Settings.Default.FadeD;
            }
            set
            {
                Properties.Settings.Default.FadeD = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Waveform Amplitude Multiplayer
        /// </summary>
        public static double WAM
        {
            get
            {
                return Properties.Settings.Default.WAM;
            }
            set
            {
                Properties.Settings.Default.WAM = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string ProjectFile
        {
            get
            {
                return Properties.Settings.Default.ProjectFile;
            }
            set
            {
                Properties.Settings.Default.ProjectFile = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string WavSettings
        {
            get
            {
                return Properties.Settings.Default.WavSettings;
            }
            set
            {
                Properties.Settings.Default.WavSettings = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
