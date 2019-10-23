using System.IO;

namespace WavConfigTool.Tools
{
    static class PathResolver
    {
        public const string SETTINGS_EXT = ".reclist";
        public const string REPLACEMENT_EXT = ".txt";
        public const string PROJECT_EXT = ".wcp";
        public const string MASK_EXT = ".mask";

        public const string LOG_FILE = "log.txt";
        public const string BACKUP_FILE = "~temp" + PROJECT_EXT;
        public static string Reclist(string filename)
        {
            return Settings.GetResoucesPath(Path.Combine("WavConfigTool", "Settings", filename));
        }

        public static string Log()
        {
            return Path.Combine(Settings.TempDir, LOG_FILE);
        }

        public static string Replacer(string reclistName, string name = "")
        {
            var filename = name == "" ? reclistName : $"{reclistName}__{name}";
            filename += REPLACEMENT_EXT;
            return Settings.GetResoucesPath(Path.Combine("WavConfigTool", "Settings", filename));
        }
    }
}
