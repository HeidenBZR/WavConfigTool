using System.IO;
using System.Linq;
using System.Text;

namespace WavConfigTool.Classes
{
    public class Voicebank
    {
        public bool IsLoaded { get; private set; } = false;
        public string Location { get; private set; } = "";

        public string Name { get; private set; } = "(Voicebaink is not avialable)";
        public string Subfolder { get; set; }
        public string Image { get; private set; } = "";
        public string ImagePath
        {
            get
            {
                return Image is null || Image == "" ? "" : Path.Combine(Location, Image);
            }
        }
        //public string[] Samples { get; private set; }

        public Voicebank(string location)
        {
            Location = location;
            if (!Directory.Exists(Location))
                return;

            Name = Path.GetFileName(Location);
            Read();
            //Samples = Directory.GetFiles(Location, "*.wav");

            if (!File.Exists(ImagePath))
                Image = "";

            IsLoaded = true;
        }

        public void Read()
        {
            string character = Path.Combine(Location, "character.txt");
            if (!File.Exists(character))
            {
                character = Path.Combine(Location, "..", "character.txt");
                Subfolder = Path.GetDirectoryName(Location);
            }
            if (!File.Exists(character))
                return;
            var lines = File.ReadAllLines(character, Encoding.UTF8);
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
        }

        public bool IsSampleEnabled(string sample)
        {
            if (!IsLoaded)
                return false;
            return File.Exists(Path.Combine(Location, sample + ".wav"));
        }
    }
}
