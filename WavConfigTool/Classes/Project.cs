using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{
    public class Project
    {
        #region variables
        private Reclist _reclist;
        private Voicebank _voicebank;
        private List<ProjectLine> _projectLines;


        public Reclist Reclist { get => _reclist; private set { _reclist = value; ProjectChanged(); } }
        public Voicebank Voicebank { get => _voicebank; private set { _voicebank = value; ProjectChanged(); } }
        public OtoGenerator OtoGenerator { get; private set; }

        private int _vowelSustain = 200;
        private int _vowelAttack = 60;
        private int _consonantAttack = 30;
        private string _prefix = "";
        private string _suffix = "";

        public int VowelSustain { get => _vowelSustain; set { _vowelSustain = value; ProjectChanged(); } }
        public int VowelAttack { get => _vowelAttack; set { _vowelAttack = value; ProjectChanged(); } }
        public int ConsonantAttack { get => _consonantAttack; set { _consonantAttack = value; ProjectChanged(); } }
        public string Prefix { get => _prefix; set { _prefix = value; ProjectChanged(); } }
        public string Suffix { get => _suffix; set { _suffix = value; ProjectChanged(); } }

        public List<ProjectLine> ProjectLines { get => _projectLines; set { _projectLines = value; ProjectChanged(); } }
        public bool IsLoaded = false;

        #endregion


        public delegate void ProjectChangedEventHandler();
        public event ProjectChangedEventHandler ProjectChanged;
        public delegate void ProjectLinesChangedEventHandler();
        public event ProjectLinesChangedEventHandler ProjectLinesChanged;

        public Project()
        {
            _projectLines = new List<ProjectLine>();
            ProjectChanged += Project_OnProjectChanged;
            ProjectLinesChanged += Project_OnProjectLineChanged;
            Voicebank = new Voicebank("");
            Reclist = new Reclist("");
        }

        private void Project_OnProjectChanged()
        {
            foreach (var line in ProjectLines)
                line.IsEnabled = Voicebank.IsSampleEnabled(line.Recline.Filename);
            if (IsLoaded)
                Save();
        }

        private void Project_OnProjectLineChanged()
        {
            if (IsLoaded)
                Save();
        }

        public static Project OpenBackup()
        {
            if (!Settings.IsUnsaved || !File.Exists(Settings.TempProject))
                return null;
            var project = Open(Settings.TempProject);
            Settings.IsUnsaved = true;
            return project;
        }

        public static Project OpenLast()
        {
            if (!File.Exists(Settings.ProjectFile))
                return null;
            var project = Open(Settings.ProjectFile);
            return project;
        }

        public static Project Open(string project_path)
        {

            if (!File.Exists(project_path))
                return null;
            var project = new Project();
            project.Read(project_path);
            project.IsLoaded = project.Reclist.IsLoaded && project.Voicebank.IsLoaded;
            project.OtoGenerator = new OtoGenerator(project.Reclist);
            Settings.ProjectFile = project_path;
            return project;
        }


        public void ChangeVoicebank(string voicebankLocation)
        {
            Voicebank = new Voicebank(voicebankLocation);
            OpenLast();
            IsLoaded = Voicebank.IsLoaded && Reclist.IsLoaded;
            ProjectChanged();
        }

        public void ChangeReclist(string reclist_location)
        {
            Reclist = new Reclist(reclist_location);
            OpenLast();
            IsLoaded = Voicebank.IsLoaded && Reclist.IsLoaded;
            ProjectChanged();
        }

        void NewProject(string reclist, string voicebank)
        {
            ProjectLines = new List<ProjectLine>();
            Voicebank = new Voicebank(voicebank);
            Reclist = new Reclist(reclist);
            Save();
        }

        public void Write(string path)
        {

            StringBuilder text = new StringBuilder();
            text.Append($"$Voicebank={Voicebank.Location}\r\n");
            text.Append($"$Reclist={Reclist.Name}\r\n");
            text.Append($"$Suffix={Suffix}\r\n");
            text.Append($"$Prefix={Prefix}\r\n");
            text.Append($"$VowelSustain={VowelSustain}\r\n");
            text.Append($"$VowelAttack={VowelAttack}\r\n");
            text.Append($"$ConsonantAttack={ConsonantAttack}\r\n");

            foreach (var projectLine in ProjectLines)
            {
                if (projectLine.RestPoints.Count == 0 && projectLine.VowelPoints.Count == 0 && projectLine.ConsonantPoints.Count == 0)
                    continue;
                text.Append($"{projectLine.Recline.Filename}\r\n");
                text.Append($"{String.Join(" ", projectLine.RestPoints)}\r\n");
                text.Append($"{String.Join(" ", projectLine.VowelPoints) }\r\n");
                text.Append($"{String.Join(" ", projectLine.ConsonantPoints)}\r\n");
            }
            File.WriteAllText(path, text.ToString(), Encoding.UTF8);
        }

        public void Save()
        {
            if (!IsLoaded)
                return;
            // TODO: Сохранять в файл проекта параметры, связанные с проектом
            if (Settings.IsUnsaved)
                Write(Settings.TempProject);
            else
                Write(Settings.ProjectFile);

            Settings.IsUnsaved = Settings.ProjectFile == Settings.TempProject;
        }

        bool Read(string location)
        {
            string[] lines = File.ReadAllLines(location, Encoding.UTF8);
            ProjectLines = new List<ProjectLine>();
            int i = 0;
            if (!lines[0].StartsWith("$"))
            {
                // Совместимость со старыми сейвами
                Voicebank = new Voicebank(lines[0]);
                i++;
            }
            for (; lines[i].StartsWith("$"); i++)
            {
                if (!lines[i].Contains("="))
                    continue;
                var pair = lines[i].Split('=');
                var option = pair[0].Substring(1);
                var value = pair[1];
                switch (option)
                {
                    case "Voicebank":
                        Voicebank = new Voicebank(value);
                        break;

                    case "Reclist":
                        Reclist = new Reclist(value);
                        break;

                    case "Suffix":
                        Suffix = value;
                        break;

                    case "Prefix":
                        Prefix = value;
                        break;

                    case "VowelSustain":
                        if (int.TryParse(value, out int vowelSustain))
                            VowelSustain = vowelSustain;
                        break;

                    case "VowelAttack":
                        if (int.TryParse(value, out int vowelAttack))
                            VowelAttack = vowelAttack;
                        break;

                    case "ConsonantAttack":
                        if (int.TryParse(value, out int consonantAttack))
                            ConsonantAttack = consonantAttack;
                        break;
                }
            }
            ProjectLines = new List<ProjectLine>();
            for (; i + 3 < lines.Length; i += 4)
            {
                string filename = lines[i];
                string pds = lines[i + 1];
                string pvs = lines[i + 2];
                string pcs = lines[i + 3];
                var recline = Reclist.GetRecline(filename);
                var projectLine = new ProjectLine(recline);
                projectLine.IsEnabled = Voicebank.IsSampleEnabled(filename);
                if (pds.Length > 0)
                    projectLine.RestPoints = pds.Split(' ').Select(n => int.Parse(n)).ToList();
                if (pvs.Length > 0)
                    projectLine.VowelPoints = pvs.Split(' ').Select(n => int.Parse(n)).ToList();
                if (pcs.Length > 0)
                    projectLine.ConsonantPoints = pcs.Split(' ').Select(n => int.Parse(n)).ToList();
                projectLine.CalculateZones();
                if (projectLine.IsEnabled)
                    projectLine.WavImageHash = $"{Voicebank.Location}{recline.Filename}{Reclist.Name}{Settings.WAM}".GetHashCode();
                if (projectLine.IsEnabled)
                    projectLine.WaveForm = new WaveForm(Path.Combine(Voicebank.Location, recline.Filename));
                
                projectLine.ProjectLineChanged += delegate { ProjectLinesChanged(); };
                ProjectLines.Add(projectLine);
            }
            return true;
        }

        public string GenerateOto()
        {
            var text = new StringBuilder();
            Reclist.ResetAliases();
            foreach (ProjectLine projectLine in ProjectLines)
            {
                if (!Voicebank.IsSampleEnabled(projectLine.Recline.Filename))
                    continue;
                projectLine.Sort();
                text.Append(OtoGenerator.Generate(projectLine));
            }
            return text.ToString();
        }

    }
}
