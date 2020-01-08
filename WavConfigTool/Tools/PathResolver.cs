using System.IO;

namespace WavConfigTool.Tools
{
    class PathResolver
    {
        public const string SETTINGS_EXT = ".reclist";
        public const string REPLACEMENT_EXT = ".txt";
        public const string PROJECT_EXT = ".wcp";
        public const string MASK_EXT = ".mask";

        public const string LOG_FILE = "log.txt";
        public const string BACKUP_FILE = "~temp" + PROJECT_EXT;

        private static PathResolver current;
        public static PathResolver Current
        {
            get
            {
                if (current == null)
                {
                    current = new PathResolver();
                }
                return current;
            }
        }

        private PathResolver()
        {
            if (!Directory.Exists(Reclist()))
                Directory.CreateDirectory(Reclist());
            if (!Directory.Exists(Settings.TempDir))
                Directory.CreateDirectory(Settings.TempDir);
        }

        public string Reclist(string filename = "")
        {
            return Settings.GetResoucesPath(Path.Combine("WavConfigTool", "Settings", filename));
        }

        public string Log()
        {
            return Path.Combine(Settings.TempDir, LOG_FILE);
        }

        public string Replacer(string reclistName, string name = "")
        {
            var filename = name == "" ? reclistName : $"{reclistName}__{name}";
            filename += REPLACEMENT_EXT;
            return Settings.GetResoucesPath(Path.Combine("WavConfigTool", "Settings", filename));
        }
    }
}
