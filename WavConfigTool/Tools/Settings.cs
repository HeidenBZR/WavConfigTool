using System;
using System.IO;
using WavConfigTool.Classes;

namespace WavConfigTool.Tools
{
    public static class Settings
    {
        public static string TempProject
        {
            get
            {
                var tempdir = Path.GetTempPath();
                tempdir = Path.Combine(tempdir, "WavConfigTool");
                if (!Directory.Exists(tempdir))
                    Directory.CreateDirectory(tempdir);
                tempdir = Path.Combine(tempdir, @"~temp.wcp");
                return tempdir;

            }
        }

        public static string TempDir
        {
            get
            {
                var tempdir = Path.GetTempPath();
                tempdir = Path.Combine(tempdir, "WavConfigTool");
                if (!Directory.Exists(tempdir))
                    Directory.CreateDirectory(tempdir);
                tempdir = Path.Combine(tempdir, "waveform");
                if (!Directory.Exists(tempdir))
                    Directory.CreateDirectory(tempdir);
                return tempdir;
            }
        }

        public static void CheckPath()
        {
            if (!Directory.Exists(GetResoucesPath(@"WavConfigTool")))
                Directory.CreateDirectory(GetResoucesPath(@"WavConfigTool"));
            if (!Directory.Exists(GetResoucesPath(@"WavConfigTool\WavSettings\")))
                Directory.CreateDirectory(GetResoucesPath(@"WavConfigTool\WavSettings\"));
        }

        public static string GetResoucesPath(string path)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                path);
        }

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
        public static string LastReclist
        {
            get
            {
                return Properties.Settings.Default.LastReclist;
            }
            set
            {
                Properties.Settings.Default.LastReclist = value;
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

        public static double ScaleX
        {
            get
            {
                return Properties.Settings.Default.ScaleX;
            }
            set
            {
                Properties.Settings.Default.ScaleX = value;
                Properties.Settings.Default.Save();
            }
        }

        public static double ScaleY
        {
            get
            {
                return Properties.Settings.Default.ScaleY;
            }
            set
            {
                Properties.Settings.Default.ScaleY = value;
                Properties.Settings.Default.Save();
            }
        }

        public static PhonemeType Mode { get; set; } = PhonemeType.Rest;

        public static double RealToViewX(int position)
        {
            return position * ScaleX;
        }

        public static double RealToViewX(double position)
        {
            return position * ScaleX;
        }

        public static int ViewToRealX(double position)
        {
            return (int)Math.Round(position / ScaleX);
        }

        public static double RealToViewY(int position)
        {
            return position * ScaleY;
        }

        public static double RealToViewY(double position)
        {
            return position * ScaleY;
        }

        public static int ViewToRealY(double position)
        {
            return (int)Math.Round(position / ScaleY);
        }
    }
}
