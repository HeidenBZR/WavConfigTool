using System;
using System.IO;

namespace WavConfigCore.Tools
{
    public class PathResolver
    {
        public const string SETTINGS_EXT = ".reclist";
        public const string REPLACEMENT_EXT = ".wtr";
        public const string PROJECT_EXT = ".wcp";
        public const string MASK_EXT = ".mask";

        public const string LOG_FILE = "log.txt";
        public const string BACKUP_FILE = "~temp" + PROJECT_EXT;

        public string TempDir = Path.Combine(Path.GetTempPath(), "WavConfigTool");
        public string ResourcesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WavConfigTool");

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
            if (!Directory.Exists(TempDir))
                Directory.CreateDirectory(TempDir);
            if (!Directory.Exists(Backup(true)))
                Directory.CreateDirectory(Backup(true));
        }

        public string Reclist(string filename = "")
        {
            return Path.Combine(ResourcesDir, "Settings", filename);
        }

        public string Log()
        {
            return Path.Combine(TempDir, LOG_FILE);
        }

        public string Replacer(string reclistName, string name = "")
        {
            var filename = name == "" ? reclistName : $"{reclistName}__{name}";
            filename += REPLACEMENT_EXT;
            return Path.Combine(ResourcesDir, "Settings", filename);
        }

        public string Backup(bool onlyFolder = false)
        {
            var folder = Path.Combine(Path.GetTempPath(), "WavConfigTool", "backups");
            if (onlyFolder)
            {
                return folder;
            }
            string timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var filename = $"backup_{timeStamp}" + PROJECT_EXT;
            return Path.Combine(folder, filename);
        }

        public bool IsProjectFile(string filename)
        {
            return filename != null && filename != "" && Path.GetExtension(filename) == PROJECT_EXT;
        }
    }
}
