using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{
    public class ProjectOptions
    {
        public int PageSize = 5;
        public int LastPage = 0;
    }

    public class Project
    {
        #region variables

        public static Project Current { get; private set; }

        public Dictionary<string, string> Options;
        public Dictionary<string, Oto> Otos { get; set; }
        public Oto[] OtoList { get => Otos.Values.ToArray(); }

        public Reclist Reclist { get => _reclist; private set { _reclist = value; ProjectChanged(); } }
        public Voicebank Voicebank { get => _voicebank; private set { _voicebank = value; ProjectChanged(); } }
        public Replacer Replacer { get => _replacer; private set { _replacer = value; ProjectChanged(); } }
        public OtoGenerator OtoGenerator { get; private set; }
        public ProjectOptions ProjectOptions { get; set; }

        public bool IsLoaded { get; set; } = false;
        public bool IsChangedAfterBackup { get; private set; }

        public int VowelDecay { get => _vowelDecay; set { _vowelDecay = value; ProjectChanged(); } }
        public int VowelAttack { get => _vowelAttack; set { _vowelAttack = value; ProjectChanged(); } }
        public int ConsonantAttack { get => _consonantAttack; set { _consonantAttack = value; ProjectChanged(); } }
        public int RestAttack { get => _restAttack; set { _restAttack = value; ProjectChanged(); } }
        public string Prefix { get => _prefix; set { _prefix = value; ProjectChanged(); } }
        public string Suffix { get => _suffix; set { _suffix = value; ProjectChanged(); } }
        public string WavPrefix
        {
            get => wavPrefix;
            set { wavPrefix = value; ProjectChanged(); }
        }
        public string WavSuffix
        {
            get => wavSuffix;
            set { wavSuffix = value; ProjectChanged(); }
        }
        public double UserScaleY
        {
            get => userScaleY;
            set { userScaleY = value; ProjectChanged(); }
        }
        public double UserScaleX
        {
            get => userScaleX;
            set { userScaleX = value; ProjectChanged(); }
        }

        public List<ProjectLine> ProjectLines { get => _projectLines; set { _projectLines = value; ProjectChanged(); } }
        public Dictionary<string, ProjectLine> ProjectLinesByFilename { get => _projectLinesByFilename; set { _projectLinesByFilename = value; ProjectChanged(); } }


        #endregion


        public event SimpleHandler ProjectChanged = delegate { };
        public event SimpleHandler ProjectLinesChanged = delegate { };
        public event SimpleHandler BeforeSave = delegate { };
        public event SimpleHandler AfterSave = delegate { };
        public event SimpleHandler SaveMe = delegate { };

        public Project()
        {
            ProjectOptions = new ProjectOptions();
            _voicebank = new Voicebank("");
            _reclist = new Reclist();
            Current = this;
            _projectLines = new List<ProjectLine>();
            _projectLinesByFilename = new Dictionary<string, ProjectLine>();
            Options = new Dictionary<string, string>();
            ProjectChanged += Project_OnProjectChanged;
            ProjectLinesChanged += Project_OnProjectLineChanged;
            IsLoaded = false;
            ResetOto();
        }

        public void SetVoicebank(Voicebank voicebank)
        {
            try
            {
                Voicebank = voicebank;
                IsLoaded = Voicebank.IsLoaded && Reclist.IsLoaded;
                CheckEnabled();
                ProjectChanged();
            }
            catch (Exception ex)
            {
                Debug.Log(ex, "Error on SetVoicebank");
            }
        }

        public void SetReclist(Reclist reclist)
        {
            Reclist = reclist;
            if (Reclist.IsLoaded)
                Reader.ReclistReader.Current.Write(PathResolver.Current.Reclist(Reclist.Name + PathResolver.SETTINGS_EXT), Reclist);
            IsLoaded = Voicebank.IsLoaded && Reclist.IsLoaded;
            CheckEnabled();
            ProjectChanged();
        }
        public void SetReplacer(Replacer replacer)
        {
            Replacer = replacer;
            Reader.ReplacerReader.Current.Write(replacer, Reclist);
            ProjectChanged();
        }

        public void CheckEnabled()
        {
            if (!IsLoaded)
                return;

            foreach (var projectLine in ProjectLines)
            {
                projectLine.IsEnabled = false;
            }
            foreach (var recline in Reclist.Reclines)
            {
                if (!ProjectLinesByFilename.ContainsKey(recline.Filename))
                {
                    AddProjectLine(recline.Filename, new ProjectLine());
                }
                if (ProjectLinesByFilename.TryGetValue(recline.Filename, out var projectLine))
                {
                    projectLine.Recline = recline;
                    projectLine.IsEnabled = Voicebank.IsSampleEnabled(projectLine.Recline.Filename);
                    ProcessLineAfterRead(projectLine);
                }
            }
        }

        public void AddProjectLine(string filename, ProjectLine projectLine)
        {
            ProjectLines.Add(projectLine);
            ProjectLinesByFilename[filename] = projectLine;
            ProcessLineAfterRead(projectLine);
        }


        public void SetOtoGenerator(OtoGenerator otoGenerator)
        {
            OtoGenerator = otoGenerator;
            ProjectChanged();
        }

        public void ResetOto()
        {
            Otos = new Dictionary<string, Oto>();
        }

        public (string, Oto) AddOto(Oto oto)
        {
            ExceptionCatcher.Current.Assert(oto != null, "oto was null");
            var newAlias = oto.Alias;
            int i = 0;
            ExceptionCatcher.Current.Assert(Otos != null, "Otos was null");
            if (Otos.ContainsKey(oto.Alias))
            {
                do
                {
                    i++;
                    newAlias = $"{oto.Alias} ({i})";
                    oto.Number = i;
                }
                while (Otos.ContainsKey(newAlias));
            }
            if (CheckForDuplicates(i))
                Otos[newAlias] = oto;
            return (newAlias, oto);
        }

        public void ProcessLineAfterRead(ProjectLine projectLine)
        {
            projectLine.ReclistAndVoicebankCheck(Reclist, Voicebank);
            projectLine.ProjectLineChanged += delegate { ProjectLinesChanged(); };
            projectLine.ProjectLinePointsChanged += delegate { FireSaveMe(); };
        }

        public void HandleBackupSaved()
        {
            IsChangedAfterBackup = false;
        }

        public void FireSaveMe()
        {
            if (!IsLoaded)
                return;
            IsChangedAfterBackup = true;
            BeforeSave();
            SaveMe();
            AfterSave();
        }
        public void Sort()
        {
            foreach (var line in ProjectLines)
                line.Sort();
        }

        public void GenerateOto()
        {
            Sort();
            ExceptionCatcher.Current.Assert(OtoGenerator != null, "Oto generator was null");
            OtoGenerator.Project = this;
            ResetOto();
            ExceptionCatcher.Current.Assert(Reclist.Reclines != null, "Reclines was null");
            foreach (Recline recline in Reclist.Reclines)
            {
                //if (!Voicebank.IsSampleEnabled(recline.Filename))
                //    continue;
                ExceptionCatcher.Current.Assert(recline != null, "recline was null");
                recline.ResetOto();
                ExceptionCatcher.Current.Assert(ProjectLinesByFilename != null, "ProjectLinesByFilename was null");
                var projectLine = ProjectLinesByFilename[recline.Filename];
                ExceptionCatcher.Current.Assert(projectLine != null, "projectLine was null");
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
            File.WriteAllText(filename, oto, Encoding.GetEncoding(932));
        }

        #region private

        private Reclist _reclist;
        private Voicebank _voicebank;
        private Replacer _replacer;
        private List<ProjectLine> _projectLines;
        private Dictionary<string, ProjectLine> _projectLinesByFilename;

        private int _vowelDecay = 200;
        private int _vowelAttack = 60;
        private int _consonantAttack = 30;
        private int _restAttack = 40;
        private string _prefix = "";
        private string _suffix = "";
        private double userScaleY = 1;
        private double userScaleX = 1;
        private string wavPrefix = "";
        private string wavSuffix = "";

        private void Project_OnProjectChanged()
        {
            FireSaveMe();
        }

        private void Project_OnProjectLineChanged()
        {
            FireSaveMe();
        }

        private bool CheckForDuplicates(int i)
        {
            return i == 0 || Reclist.WavMask.MaxDuplicates == 0 || i < Reclist.WavMask.MaxDuplicates;
        }

        #endregion
    }
}
