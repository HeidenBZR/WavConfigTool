using System;
using System.IO;
using WavConfigCore;

namespace WavConfigTool.Classes
{
    public static class Settings
    {

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


        public static WavConfigCore.PhonemeType Mode { get; set; } = PhonemeType.Rest;

        public static double ScaleX => 0.45;
        public static double ScaleY => 60;
        public static double UserScaleX => ProjectManager.Current.Project != null ? ProjectManager.Current.Project.UserScaleX : 1;
        public static double UserScaleY => ProjectManager.Current.Project != null ? ProjectManager.Current.Project.UserScaleY : 1;

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
