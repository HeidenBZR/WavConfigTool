using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Classes;
using WavConfigTool.Tools;
using WavConfigTool.UserControls;
using WavConfigTool.Windows;

namespace WavConfigTool.Tools
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

        public static int VowelAttack
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

        public static int ConsonantAttack
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

        public static int RestAttack
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

        public static bool IsMaximized
        {
            get
            {
                return Properties.Settings.Default.IsMaximized;
            }
            set
            {
                Properties.Settings.Default.IsMaximized = value;
                Properties.Settings.Default.Save();
            }
        }

        public static System.Drawing.Point WindowSize
        {
            get
            {
                return Properties.Settings.Default.WindowSize;
            }
            set
            {
                Properties.Settings.Default.WindowSize = value;
                Properties.Settings.Default.Save();
            }
        }

        public static System.Drawing.Point WindowPosition
        {
            get
            {
                return Properties.Settings.Default.WindowPosition;
            }
            set
            {
                Properties.Settings.Default.WindowPosition = value;
                Properties.Settings.Default.Save();
            }
        }

        public static bool IsUnsaved
        {
            get
            {
                return Properties.Settings.Default.IsUnsaved;
            }
            set
            {
                Properties.Settings.Default.IsUnsaved = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
