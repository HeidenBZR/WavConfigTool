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

        public static PhonemeType Mode { get; set; } = PhonemeType.Rest;

        public static double ScaleX => 0.45;
        public static double ScaleY => 60;

        public static double UserScaleX => Project.Current != null ? Project.Current.UserScaleX : 1;
        public static double UserScaleY => Project.Current != null ? Project.Current.UserScaleY : 1;

        public static double RealToViewX(int position)
        {
            return position * ScaleX * UserScaleX;
        }

        public static double RealToViewX(double position)
        {
            return position * ScaleX * UserScaleX;
        }

        public static int ViewToRealX(double position)
        {
            return (int)Math.Round(position / ScaleX / UserScaleX);
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
