using System;
using System.IO;
using System.Linq;
using System.Text;
using WavConfigCore.Tools;

namespace WavConfigCore
{
    public class Voicebank
    {
        public bool IsLoaded { get; private set; } = false;
        public string Location { get; private set; }
        public string Fullpath { get; private set; }
        public string CharacterPath { get; private set; }

        public string Name { get; private set; } = EMPTY_NAME;
        public string Subfolder { get; private set; }
        public string Image { get; private set; } = "";
        public string ImagePath { get; private set; } = "";

        const string EMPTY_NAME = "(Voicebank is not available)";

        public Voicebank(string projectDir, string location)
        {
            Location = location;
            ManageLocations(projectDir, location);
            if (CharacterPath != null)
            {
                ReadCharacter();
            }

            if (!File.Exists(ImagePath))
                Image = "";
        }

        public void ReadCharacter()
        {
            var lines = File.ReadAllLines(CharacterPath, Encoding.GetEncoding(932));
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
            ImagePath = CharacterPath != null && Image != null && Image != "" ? Path.Combine(PathResolver.Current.TryGetDirectoryName(CharacterPath), Image) : "";
        }

        public bool IsSampleEnabled(string sample, string wavPrefix, string wavSuffix)
        {
            if (!IsLoaded)
                return false;
            return File.Exists(GetSamplePath(sample, wavPrefix, wavSuffix));
        }

        public string GetFullName()
        {
            var name = Name;
            if (IsLoaded && Subfolder != null && Subfolder != "")
                name += $" ({Subfolder})";
            return name;
        }

        public void UpdateLocations(string projectDir)
        {
            ManageLocations(projectDir, Fullpath);
            if (CharacterPath != null)
                ReadCharacter();
        }

        public string GetSamplePath(string sample, string wavPrefix, string wavSuffix)
        {
            if (Fullpath == null)
            {
                throw new Exception("Voicebank is not loaded");
            }
            return Path.Combine(Fullpath, string.Format("{0}{1}{2}.wav", wavPrefix, sample, wavSuffix));
        }

        #region private

        private void ManageLocations(string projectDir, string location)
        {
            if (projectDir == "" || location == null)
            {
                IsLoaded = false;
                return;
            }
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

            IsLoaded = true;
        }

        #endregion
    }
}
