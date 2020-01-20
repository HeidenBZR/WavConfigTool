using System;
using System.IO;
using System.Linq;
using System.Text;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{
    public class Voicebank
    {
        public bool IsLoaded { get; private set; } = false;
        public string Location { get; private set; }
        public string Fullpath { get; private set; }
        public string CharacterPath { get; private set; }

        public string Name { get; private set; } = "(Voicebank is not available)";
        public string Subfolder { get; private set; }
        public string Image { get; private set; } = "";
        public string ImagePath { get; private set; } = "";

        public Voicebank(string location)
        {
            Location = location;
            ManageLocations(location);
            if (CharacterPath != null)
            {
                ReadCharacter();
            }

            if (!File.Exists(ImagePath))
                Image = "";

            IsLoaded = true;
        }

        public void ReadCharacter()
        {
            var lines = File.ReadAllLines(CharacterPath, Encoding.UTF8);
            foreach (var line in lines)
            {
                if (line.Contains('='))
                {
                    var pair = line.Split('=');
                    if (pair[0] == "name")
                        Name = pair[1];
                    else if (pair[0] == "image")
                        Image = pair[1];
                }
            }
            ImagePath = CharacterPath != null && Image != null && Image != "" ? Path.Combine(Path.GetDirectoryName(CharacterPath), Image) : "";
        }

        public bool IsSampleEnabled(string sample)
        {
            if (!IsLoaded)
                return false;
            return File.Exists(Path.Combine(Fullpath, Project.Current.WavPrefix + sample + Project.Current.WavSuffix + ".wav"));
        }

        public string GetFullName()
        {
            var name = Name;
            if (IsLoaded && Subfolder != null && Subfolder != "")
                name += $" ({Subfolder})";
            return name;
        }

        public void UpdateLocations()
        {
            ManageLocations(Fullpath);
            if (CharacterPath != null)
                ReadCharacter();
        }

        private void ManageLocations(string location)
        {
            if (Settings.ProjectFile == "" || location == null)
            {
                IsLoaded = false;
                return;
            }
            var projectDir = Path.GetDirectoryName(Settings.ProjectFile);
            Fullpath = Path.Combine(projectDir, location);
            Subfolder = "";
            if (!Directory.Exists(Fullpath))
            {
                IsLoaded = false;
                return;
            }

            Location = location;
            try
            {
                var uri = new Uri(Fullpath);
                var projectUri = new Uri(projectDir);
                var localUri = uri.MakeRelativeUri(projectUri);
                if (!localUri.OriginalString.Contains(".."))
                    Location = localUri.OriginalString;
            }
            catch { }

            string character = Path.Combine(Fullpath, "character.txt");
            Name = Path.GetFileName(Fullpath);
            if (!File.Exists(character))
            {
                character = Path.Combine(Fullpath, "..", "character.txt");
                Subfolder = Name;
            }
            if (File.Exists(character))
            {
                CharacterPath = character;
            }
            else
            {
                Subfolder = "";
            }
        }
    }
}
