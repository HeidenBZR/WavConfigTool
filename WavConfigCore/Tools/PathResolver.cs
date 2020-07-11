using System;
using System.IO;

namespace WavConfigCore.Tools
{
    public class PathResolver
    {
        public const string RECLIST_EXT = ".reclist";
        public const string REPLACEMENT_EXT = ".wtr";
        public const string PROJECT_EXT = ".wcp";
        public const string MASK_EXT = ".mask";

        public const string LOG_FILE = "log.txt";
        public const string BACKUP_FILE = "~temp" + PROJECT_EXT;

        public const string TEST_FOLDER = "test";

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

        public string TryGetDirectoryName(string path, string defaultVal = "")
        {
            try
            {
                return Path.GetDirectoryName(path);
            }
            catch
            {
                return defaultVal;
            }
        }

        public string Reclist(string filename = "", bool isTest = false)
        {
            var folder = Path.Combine(ResourcesDir, "Settings");
            if (isTest)
                folder = Path.Combine(folder, TEST_FOLDER);
            if (filename == "")
                return folder;
            return Path.Combine(folder, filename);
        }

        public string Mask(string filename = "")
        {
            return Path.Combine(ResourcesDir, "Settings", filename + MASK_EXT);
        }

        public string Log()
        {
            return Path.Combine(TempDir, LOG_FILE);
        }

        public string Replacer(string reclistName, string name = "", bool isTest = false)
        {
            var filename = name == "" ? reclistName : $"{reclistName}__{name}";
            filename += REPLACEMENT_EXT;
            var folder = Path.Combine(ResourcesDir, "Settings");
            if (isTest)
                folder = Path.Combine(folder, TEST_FOLDER);
            return Path.Combine(folder, filename);
        }

        public string OremoPack(string oremoPackName)
        {
            var dir = Path.Combine(ResourcesDir, "Settings", oremoPackName + "_oremo");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
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
