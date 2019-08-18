using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{
    public class Project
    {
        #region variables
        private Reclist _reclist;
        private Voicebank _voicebank;
        private List<ProjectLine> _projectLines;
        private Dictionary<string, ProjectLine> _projectLinesByFilename;
        public Dictionary<string, string> Options;

        public Reclist Reclist { get => _reclist; private set { _reclist = value; ProjectChanged(); } }
        public Voicebank Voicebank { get => _voicebank; private set { _voicebank = value; ProjectChanged(); } }
        public OtoGenerator OtoGenerator { get; private set; }

        private int _vowelDecay = 200;
        private int _vowelAttack = 60;
        private int _consonantAttack = 30;
        private int _restAttack = 40;
        private string _prefix = "";
        private string _suffix = "";
        private double _wavAmplitudeMultiplayer = 1;

        public int VowelDecay { get => _vowelDecay; set { _vowelDecay = value; ProjectChanged(); } }
        public int VowelAttack { get => _vowelAttack; set { _vowelAttack = value; ProjectChanged(); } }
        public int ConsonantAttack { get => _consonantAttack; set { _consonantAttack = value; ProjectChanged(); } }
        public int RestAttack { get => _restAttack; set { _restAttack = value; ProjectChanged(); } }
        public string Prefix { get => _prefix; set { _prefix = value; ProjectChanged(); } }
        public string Suffix { get => _suffix; set { _suffix = value; ProjectChanged(); } }

        public List<ProjectLine> ProjectLines { get => _projectLines; set { _projectLines = value; ProjectChanged(); } }
        public Dictionary<string, ProjectLine> ProjectLinesByFilename { get => _projectLinesByFilename; set { _projectLinesByFilename = value; ProjectChanged(); } }
        public bool IsLoaded { get; set; } = false;
        public double WavAmplitudeMultiplayer { get => _wavAmplitudeMultiplayer; set { _wavAmplitudeMultiplayer = value; ProjectChanged(); Settings.WAM = _wavAmplitudeMultiplayer; } }

        public static Project Current { get; private set; }

        #endregion


        public delegate void SimpleHandler();
        public event SimpleHandler ProjectChanged;
        public event SimpleHandler ProjectLinesChanged;
        public event SimpleHandler BeforeSave;
        public event SimpleHandler AfterSave;

        public Project(string voicebank = "", string reclist = "")
        {
            Current = this;
            _projectLines = new List<ProjectLine>();
            Options = new Dictionary<string, string>();
            ProjectChanged += Project_OnProjectChanged;
            ProjectLinesChanged += Project_OnProjectLineChanged;
            BeforeSave += () => { };
            AfterSave += () => { };
            Voicebank = new Voicebank(voicebank);
            Reclist = new Reclist(reclist);
            IsLoaded = Voicebank.IsLoaded && Reclist.IsLoaded;
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
            project.OtoGenerator = new OtoGenerator(project.Reclist, project);
            Settings.ProjectFile = project_path;
            return project;
        }


        public void ChangeVoicebank(string voicebankLocation)
        {
            Voicebank = new Voicebank(voicebankLocation);
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
            text.Append($"$VowelDecay={VowelDecay}\r\n");
            text.Append($"$VowelAttack={VowelAttack}\r\n");
            text.Append($"$ConsonantAttack={ConsonantAttack}\r\n");
            text.Append($"$WavAmplitudeMultiplayer={WavAmplitudeMultiplayer.ToString("F2")}\r\n");
            foreach (var key in Options.Keys)
            {
                text.Append($"${key}={Options[key]}\r\n");
            }

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

        public async void SaveAsync()
        {
            await Task.Run(() => Save());
        }

        public void Save()
        {
            if (!IsLoaded)
                return;
            BeforeSave();
            if (Settings.IsUnsaved)
                Write(Settings.TempProject);
            else
                Write(Settings.ProjectFile);

            Settings.IsUnsaved = Settings.ProjectFile == Settings.TempProject;
            AfterSave();
        }

        void ReadOption(string line)
        {
            var pair = line.Split('=');
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

                case "VowelDecay":
                    if (int.TryParse(value, out int vowelDecay))
                        VowelDecay = vowelDecay;
                    break;

                case "VowelAttack":
                    if (int.TryParse(value, out int vowelAttack))
                        VowelAttack = vowelAttack;
                    break;

                case "ConsonantAttack":
                    if (int.TryParse(value, out int consonantAttack))
                        ConsonantAttack = consonantAttack;
                    break;

                case "WavAmplitudeMultiplayer":
                    if (double.TryParse(value, out double wavAmplitudeMultiplayer))
                        WavAmplitudeMultiplayer = wavAmplitudeMultiplayer;
                    break;

                default:
                    Options[option] = value;
                    break;

            }
        }

        bool Read(string location)
        {
            string[] lines = File.ReadAllLines(location, Encoding.UTF8);
            Options = new Dictionary<string, string>();
            ProjectLines = new List<ProjectLine>();
            _projectLinesByFilename = new Dictionary<string, ProjectLine>();
            int i = 0;
            /// Совместимость со старыми сейвами без опций
            if (!lines[0].StartsWith("$"))
            {
                Voicebank = new Voicebank(lines[0]);
                i++;
            }
            /// Чтение опций
            for (; lines.Length > i && lines[i].StartsWith("$"); i++)
            {
                if (!lines[i].Contains("="))
                    continue;
                ReadOption(lines[i]);

            }
            ProjectLines = new List<ProjectLine>();
            var usedReclines = new List<Recline>();
            /// Чтение строк реклиста из проекта
            for (; i + 3 < lines.Length; i += 4)
            {
                var recline = Reclist.GetRecline(lines[i]);
                usedReclines.Add(recline);
                var projectLine = ProjectLine.Read(recline, lines[i + 1], lines[i + 2], lines[i + 3]);
                ProcessLineAfterRead(projectLine);
            }
            /// Чтение строк реклиста, которых нет в проекте 
            if (Reclist != null)
            {
                foreach (var recline in Reclist.Reclines)
                {
                    if (!usedReclines.Contains(recline))
                    {
                        ProcessLineAfterRead(ProjectLine.CreateNewFromRecline(recline));
                    }
                }
            }
            // TODO: сортировать по реклисту
            return true;
        }

        public void ProcessLineAfterRead(ProjectLine projectLine)
        {
            projectLine.ReclistAndVoicebankCheck(Reclist, Voicebank);
            projectLine.ProjectLineChanged += delegate { ProjectLinesChanged(); };
            projectLine.ProjectLinePointsChanged += delegate { Save(); };
            ProjectLines.Add(projectLine);
            ProjectLinesByFilename[projectLine.Recline.Filename] = projectLine;
        }

        public void Sort()
        {
            foreach (var line in ProjectLines)
                line.Sort();
        }

        public void GenerateOto()
        {
            Sort();
            OtoGenerator.Project = this;
            foreach (Recline recline in Reclist.Reclines)
            {
                if (!Voicebank.IsSampleEnabled(recline.Filename))
                    continue;
                var projectLine = ProjectLinesByFilename[recline.Filename];
                projectLine.Sort();
                OtoGenerator.Generate(projectLine);
            }
        }

        public string GetOtoText()
        {
            GenerateOto();
            var text = new StringBuilder();
            foreach (Recline recline in Reclist.Reclines)
            {
                text.Append(recline.WriteOto(Suffix, Prefix));
            }
            return text.ToString();
        }

        public void GenerateOto(string filename)
        {
            var oto = GetOtoText();
            File.WriteAllText(filename, oto, Encoding.UTF8);
        }
    }
}
